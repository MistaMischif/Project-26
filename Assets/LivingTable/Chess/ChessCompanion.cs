using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace LivingTable.Chess {
 public sealed class ChessCompanion:MonoBehaviour {
  [Serializable] public sealed class Fact {public string id,text;public Fact(string i,string t){id=i;text=t;}}
  [Serializable] public sealed class Context {public string sessionId,positionKey,kind,suggestedMove;public int revision;public string[] legalMoves;public Fact[] facts;}
  [Serializable] public sealed class Answer {public string sessionId,positionKey,answer,move,model;public int revision;public string[] factIds;}
  public string Status {get;private set;}="Checking local Meta connection...";
  UnityWebRequest request;ChessMetaVision.Connection connection;public bool Configured=>ChessHostConnection.Valid(connection);
  void Awake()=>ReloadConnection();
  void Start(){CheckConnection(_=>{});}
  void ReloadConnection(){var loaded=ChessHostConnection.Load();if(loaded!=null)connection=loaded;}
  public void CheckConnection(Action<bool> done){Cancel();ReloadConnection();if(!Configured){Status="Meta not paired. Run Prepare-Local-AI on your computer with USB connected. Chess AI works offline.";done(false);return;}StartCoroutine(Probe(done));}
  IEnumerator Probe(Action<bool> done){request=UnityWebRequest.Get(connection.url.TrimEnd('/')+"/ready");ChessHostConnection.Configure(request,connection);request.timeout=6;yield return request.SendWebRequest();bool ok=request.result==UnityWebRequest.Result.Success&&request.downloadHandler.text.Contains("companionModel");Status=ok?"Local Meta host reached. Choose a question. Wireless Meta ready; chess AI also works offline.":request.responseCode==401?"Meta pairing expired. Rerun Prepare-Local-AI with USB connected.":request.responseCode==503?"Meta model runtime is not ready. Run Prepare-Local-AI on your computer.":"Local Meta host is offline. Open Prepare-Local-AI on your computer, then RECONNECT. Keep both devices on Wi-Fi. Chess AI works offline.";Debug.Log("FIRST_STEPS_COMPANION_CONNECTION: "+(ok?"HOST_REACHED":"UNREACHABLE"));request.Dispose();request=null;done(ok);}
  public void Cancel(){StopAllCoroutines();request?.Abort();request?.Dispose();request=null;}
  void OnDisable()=>Cancel();
  public void Ask(Context context,Action<Answer> done){Cancel();ReloadConnection();if(!Configured){Status="Meta not paired. Run Prepare-Local-AI with USB connected. Chess AI works offline.";done(null);return;}StartCoroutine(Send(context,done));}
  IEnumerator Send(Context context,Action<Answer> done){
   bool ready=false;yield return Probe(ok=>ready=ok);if(!ready){done(null);yield break;}
   for(int attempt=0;attempt<3;attempt++){
   request=new UnityWebRequest(connection.url.TrimEnd('/')+"/companion","POST");request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(context)));request.downloadHandler=new DownloadHandlerBuffer();request.SetRequestHeader("Content-Type","application/json");ChessHostConnection.Configure(request,connection);request.timeout=100;
   yield return request.SendWebRequest();if(attempt<2&&request.responseCode==429){request.Dispose();request=null;Status="Waiting for the local scan to finish...";yield return new WaitForSecondsRealtime(1.5f);continue;}Answer answer=null;if(request.result==UnityWebRequest.Result.Success){try{answer=JsonUtility.FromJson<Answer>(request.downloadHandler.text);}catch{}}Status=answer!=null?"Meta response received":request.responseCode==401?"Meta pairing expired. Rerun Prepare-Local-AI with USB connected.":request.responseCode==429?"Local AI is finishing a scan. Wait a moment and ask again.":request.responseCode>=500?"Meta model could not answer. Run Prepare-Local-AI to warm the model, then retry.":request.result==UnityWebRequest.Result.ConnectionError?"Local Meta host disconnected. Open Prepare-Local-AI on your computer, then RECONNECT. Chess AI works offline.":"Meta reply could not be read. Retry the question; your game is unchanged.";Debug.Log("FIRST_STEPS_COMPANION_REPLY: "+(answer!=null?"RECEIVED":"FAILED")+" HTTP "+request.responseCode);request.Dispose();request=null;done(answer);yield break;}
  }
 }
}
