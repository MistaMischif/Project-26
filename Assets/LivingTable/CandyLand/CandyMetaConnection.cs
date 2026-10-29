using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Meta.XR;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Android;

namespace LivingTable.CandyLand
{
    // Provider credentials belong on Services/MetaHost, never in a scene or APK.
    public sealed class CandyMetaConnection : MonoBehaviour
    {
        [Serializable] public sealed class Connection { public string url, deviceToken; }
        [Serializable] sealed class Reply { public string text; }
        [Serializable] sealed class Speech { public string text; }
        [Serializable] sealed class History { public string role, content; }
        [Serializable] sealed class Chat { public string text, character, state; public History[] history; }
        [Serializable] sealed class Photo { public string image; }
        [Serializable] sealed class Scan { public int color, count; public string picture; public float confidence; }
        public string Status { get; private set; } = "Meta connection not configured";
        public string CharacterLine { get; private set; } = "";
        public bool Listening { get; private set; }
        public bool Configured => connection != null && !string.IsNullOrWhiteSpace(connection.deviceToken) && ValidUrl(connection.url);
        Connection connection;
        CandyTable table;
        AudioSource speaker;
        public bool IsSpeaking=>speaker!=null&&speaker.isPlaying;
        AudioClip microphone;
        PassthroughCameraAccess cameraAccess;
        readonly List<float> utterance = new List<float>();
        readonly List<History> history = new List<History>();
        readonly Queue<string> announcements = new Queue<string>();
        readonly float[] frame = new float[480];
        int readPosition, replyVersion, voiceVersion;
        float silence, speakingTime;
        bool inSpeech, busy, announcing;
        Coroutine speechRoutine;
        const int Rate=24000;
        public static bool ValidUrl(string url) => Uri.TryCreate(url,UriKind.Absolute,out var uri) && (uri.Scheme=="https" || (uri.Scheme=="http" && uri.IsLoopback));
        void Awake()
        {
            table=GetComponent<CandyTable>();speaker=gameObject.AddComponent<AudioSource>();speaker.playOnAwake=false;speaker.spatialBlend=0;
            string path=Path.Combine(Application.persistentDataPath,"candy-meta-connection.json");
            try {if(File.Exists(path))connection=JsonUtility.FromJson<Connection>(File.ReadAllText(path));}catch {Status="Check Meta connection settings";}
#if UNITY_EDITOR
            string url=Environment.GetEnvironmentVariable("LIVING_TABLE_HOST_URL");
            if(!string.IsNullOrEmpty(url))connection=new Connection{url=url,deviceToken=Environment.GetEnvironmentVariable("LIVING_TABLE_DEVICE_TOKEN")};
#endif
            if(Configured)Status="Meta ready · turn voice on to talk";
            table.HostSpoke+=Announce;table.ScanRequested+=ScanCard;
        }
        public void ToggleVoice()
        {
            if(Listening){StopVoice();return;}
            if(!Configured){Status="Connect Meta before turning voice on";return;}
            StartCoroutine(StartVoice());
        }
        IEnumerator StartVoice()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
                float end=Time.realtimeSinceStartup+20;
                while(!Permission.HasUserAuthorizedPermission(Permission.Microphone)&&Time.realtimeSinceStartup<end)yield return null;
                if(!Permission.HasUserAuthorizedPermission(Permission.Microphone)){Status="Microphone permission is needed for voice";yield break;}
            }
#endif
            if(Microphone.devices.Length==0){Status="No microphone available";yield break;}
            microphone=Microphone.Start(null,true,5,Rate);readPosition=0;Listening=true;Status="Listening through your headset";
            Announce(table.HostLine);yield break;
        }
        public void StopVoice()
        {
            Listening=false;replyVersion++;voiceVersion++;announcements.Clear();busy=false;announcing=false;
            if(Microphone.IsRecording(null))Microphone.End(null);
            if(microphone!=null)Destroy(microphone);microphone=null;utterance.Clear();inSpeech=false;
            InterruptAudio();Status=Configured?"Voice off":"Meta connection not configured";
        }
        void InterruptAudio()
        {
            voiceVersion++;
            if(speechRoutine!=null)StopCoroutine(speechRoutine);speechRoutine=null;
            speaker.Stop();if(speaker.clip!=null){Destroy(speaker.clip);speaker.clip=null;}
        }
        void Handoff(string character) { history.Clear();replyVersion++;busy=false;CharacterLine="";if(!announcing)InterruptAudio(); }
        void Announce(string line)
        {
            if(!Listening||!Configured)return;
            // Keep the latest short announcement if several game events arrive together.
            announcements.Clear();announcements.Enqueue(line);
            if(!announcing){InterruptAudio();speechRoutine=StartCoroutine(HostQueue());}
        }
        IEnumerator HostQueue()
        {
            announcing=true;
            while(announcements.Count>0&&Listening)yield return Speak(announcements.Dequeue(),voiceVersion);
            announcing=false;speechRoutine=null;
        }
        void Update()
        {
            if(!Listening||microphone==null||table.Paused)return;
            int end=Microphone.GetPosition(null);if(end<0)return;
            int available=(end-readPosition+microphone.samples)%microphone.samples;
            while(available>=frame.Length)
            {
                microphone.GetData(frame,readPosition);readPosition=(readPosition+frame.Length)%microphone.samples;available-=frame.Length;
                float energy=0;foreach(float sample in frame)energy+=sample*sample;
                // A headset acoustic echo canceller is still required for robust hands-free barge-in.
                bool voiced=Mathf.Sqrt(energy/frame.Length)>(speaker.isPlaying?.055f:.018f);
                if(voiced&&!inSpeech)
                {
                    inSpeech=true;silence=0;speakingTime=0;utterance.Clear();
                    if(!announcing)InterruptAudio();
                    replyVersion++;busy=false;Status="Listening…";
                }
                if(!inSpeech)continue;
                utterance.AddRange(frame);speakingTime+=.02f;silence=voiced?0:silence+.02f;
                if(silence>=.8f||speakingTime>=12)
                {
                    inSpeech=false;
                    if(speakingTime-silence>=.2f&&!busy)StartCoroutine(Answer(EncodeWav(utterance),replyVersion));
                    utterance.Clear();
                }
            }
        }
        UnityWebRequest Post(string route,byte[] bytes,string type="application/json",bool audio=false)
        {
            var url=connection.url.TrimEnd('/')+route;
            var request=new UnityWebRequest(url,"POST");request.timeout=30;
            request.uploadHandler=new UploadHandlerRaw(bytes);
            request.downloadHandler=audio?(DownloadHandler)new DownloadHandlerAudioClip(url,AudioType.WAV):new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type",type);request.SetRequestHeader("Authorization","Bearer "+connection.deviceToken);return request;
        }
        IEnumerator Answer(byte[] wav,int version)
        {
            busy=true;Status="Listening to your words…";string text;
            using(var request=Post("/transcribe",wav,"audio/wav"))
            {
                yield return request.SendWebRequest();if(version!=replyVersion)yield break;
                if(request.result!=UnityWebRequest.Result.Success){Status="Couldn't hear that. Please try again.";busy=false;yield break;}
                text=JsonUtility.FromJson<Reply>(request.downloadHandler.text)?.text;
            }
            if(string.IsNullOrWhiteSpace(text)){busy=false;Status="Listening…";yield break;}
            string state=table.Game==null?"Setting up":$"Turn {table.Game.Turn}; player {table.Game.CurrentPlayer+1}; phase {table.Phase}; card {table.CardText}; host says {table.HostLine}";
            var payload=new Chat{text=text,character="Game host",state=state,history=history.ToArray()};
            using(var request=Post("/chat",Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload))))
            {
                yield return request.SendWebRequest();if(version!=replyVersion)yield break;
                if(request.result!=UnityWebRequest.Result.Success){Status="Meta couldn't reply. Please try again.";busy=false;yield break;}
                string reply=JsonUtility.FromJson<Reply>(request.downloadHandler.text)?.text;
                if(string.IsNullOrWhiteSpace(reply)){busy=false;yield break;}
                history.Add(new History{role="user",content=text});history.Add(new History{role="assistant",content=reply});while(history.Count>8)history.RemoveAt(0);
                while(announcing||table.Paused){if(version!=replyVersion)yield break;yield return null;}
                CharacterLine="Game host: "+reply;Status="Listening…";busy=false;
                speechRoutine=StartCoroutine(Speak(reply,voiceVersion));
            }
        }
        IEnumerator Speak(string text,int version)
        {
            if(text.Length>280)text=text.Substring(0,280);
            using(var request=Post("/speak",Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Speech{text=text})),audio:true))
            {
                yield return request.SendWebRequest();if(version!=voiceVersion||!Listening)yield break;
                if(request.result!=UnityWebRequest.Result.Success){Status="Speech unavailable · captions remain on";yield break;}
                var clip=DownloadHandlerAudioClip.GetContent(request);speaker.clip=clip;speaker.Play();
                while(speaker.isPlaying&&version==voiceVersion)yield return null;
                if(speaker.clip==clip)speaker.clip=null;Destroy(clip);
            }
        }
        void ScanCard()
        {
            if(table.AutomaticPhysical){Status="Automatic recognition runs on the headset";return;}
            if(!Configured){Status="Connect Meta to read physical cards";return;}
            StartCoroutine(ReadCard(table.CardRevision,table.Game));
        }
        IEnumerator ReadCard(int requestId,CandyGame game)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            const string permission="horizonos.permission.HEADSET_CAMERA";
            if(!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
                float permissionEnd=Time.realtimeSinceStartup+20;
                while(!Permission.HasUserAuthorizedPermission(permission)&&Time.realtimeSinceStartup<permissionEnd)yield return null;
                if(!Permission.HasUserAuthorizedPermission(permission)){Status="Camera permission needed · select the card manually";yield break;}
            }
            if(cameraAccess==null)
            {
                var cameraObject=new GameObject("On-demand card camera");cameraObject.transform.SetParent(transform,false);
                cameraAccess=cameraObject.AddComponent<PassthroughCameraAccess>();cameraAccess.RequestedResolution=new Vector2Int(1280,960);
            }
            cameraAccess.enabled=true;Status="Hold the card in front of you";
            float end=Time.realtimeSinceStartup+15;
            while(!cameraAccess.IsPlaying&&Time.realtimeSinceStartup<end)yield return null;
            if(!cameraAccess.IsPlaying){cameraAccess.enabled=false;Status="Camera unavailable · choose the card manually";yield break;}
            yield return new WaitForEndOfFrame();
            var source=cameraAccess.GetTexture();
            if(source==null){cameraAccess.enabled=false;Status="No camera frame · choose the card manually";yield break;}
            // Send the central crop only; no image is persisted on device or our relay.
            var target=RenderTexture.GetTemporary(640,640,0);var previous=RenderTexture.active;
            Graphics.Blit(source,target,new Vector2(.6f,.8f),new Vector2(.2f,.1f));RenderTexture.active=target;
            var image=new Texture2D(640,640,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,640,640),0,0);image.Apply();
            var bytes=image.EncodeToJPG(80);Destroy(image);RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);cameraAccess.enabled=false;
            using(var request=Post("/recognize",Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Photo{image=Convert.ToBase64String(bytes)}))))
            {
                yield return request.SendWebRequest();
                if(!isActiveAndEnabled||table.Game!=game||table.CardRevision!=requestId||table.Phase!=CandyPhase.CheckingCard)yield break;
                if(request.result!=UnityWebRequest.Result.Success){Status="Couldn't read that · select the card below";yield break;}
                var scan=JsonUtility.FromJson<Scan>(request.downloadHandler.text);
                if(scan==null||scan.color<0||scan.color>6||scan.confidence<.9f){Status="Card unclear · please select it or rescan";yield break;}
                var card=new CandyCard((CandyColor)scan.color,scan.count,scan.picture);
                try{game.Resolve(-1,card);}catch{Status="Unknown card · select it below";yield break;}
                table.OfferScan(card,scan.confidence);Status="Please confirm the card";
            }
#else
            Status="Physical-card camera requires a Quest 3 or 3S";yield break;
#endif
        }
        public static byte[] EncodeWav(List<float> samples)
        {
            using(var stream=new MemoryStream())using(var writer=new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+samples.Count*2);writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(Rate);writer.Write(Rate*2);writer.Write((short)2);writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));writer.Write(samples.Count*2);foreach(var sample in samples)writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));return stream.ToArray();
            }
        }
        void OnApplicationPause(bool paused){if(paused){StopVoice();if(cameraAccess!=null)cameraAccess.enabled=false;}}
        void OnDisable(){if(speaker!=null)StopVoice();if(cameraAccess!=null)cameraAccess.enabled=false;}
        void OnDestroy(){if(table!=null){table.HostSpoke-=Announce;table.ScanRequested-=ScanCard;}}
    }
}
