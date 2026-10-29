using System;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // The card owns bounded board-space props; all cosmetics use a deterministic clock.
    public sealed class CandyCardMagic : MonoBehaviour
    {
        public const int ItemCount=12,EnvironmentCount=4;
        public const float OrbitStart=.55f,OrbitDuration=2.4f,FlightDuration=1.25f;
        public const float ArrivalTime=OrbitStart+OrbitDuration+FlightDuration+(ItemCount-1)*.018f;
        public const float EndTime=ArrivalTime+3.2f;
        static readonly Color[] Hues={new Color(.72f,.25f,1),new Color(1,.18f,.25f),new Color(.4f,1,.25f),new Color(1,.7f,.2f),new Color(1,.25f,.7f),new Color(.45f,.9f,1)};
        Renderer portrait;MaterialPropertyBlock properties;CandyRewardAssets assets;
        Transform board,effects,popup;Transform[] bits,garden;Func<bool> paused;
        Vector3 target,center,popStart,artScale;Quaternion orbitFrame;float age,width,radiusX,radiusY;bool lifted;
        public float Dissolve {get;private set;}
        public int ParticleCount=>bits==null?0:bits.Length;
        public Renderer Portrait=>portrait;
        public Transform Popup=>popup;
        public Vector3 Target=>target;
        public int ArrivedParticles {get;private set;}
        public float Elapsed=>age;
        public float OrbitTurns=>Mathf.Clamp01((age-OrbitStart)/OrbitDuration)*2;
        public bool TransferComplete=>age>=ArrivalTime;
        public bool ShowingIllustration=>enabled&&Dissolve<1;
        public int SproutedCount {get;private set;}
        public bool SproutingObserved {get;private set;}
        public Vector3 ItemPosition(int index)=>bits[index].localPosition;
        public Transform Item(int index)=>bits[index];
        public Transform Sprout(int index)=>garden[index];
        public Quaternion OrbitFrame=>orbitFrame;
        public Vector3 OrbitCenter=>center;
        public void Initialize(Renderer art,int theme,Func<bool> isPaused,Func<bool> isPresenting,bool restored,Transform boardFrame,Vector3 destination,float boardWidth)
        {
            portrait=art;paused=isPaused;board=boardFrame;target=destination;width=boardWidth;
            center=new Vector3(0,width*.30f,0);properties=new MaterialPropertyBlock();properties.SetColor("_SparkColor",Hues[Mathf.Clamp(theme,0,5)]);
            if(restored){SetDissolve(1);enabled=false;return;}
            effects=new GameObject("Special card board celebration").transform;effects.SetParent(board,false);assets=new CandyRewardAssets(theme);
            bits=new Transform[ItemCount];garden=new Transform[EnvironmentCount];
            for(int i=0;i<bits.Length;i++){bits[i]=assets.Create(effects,theme,i);bits[i].localScale=Vector3.zero;}
            for(int i=0;i<garden.Length;i++){garden[i]=assets.Create(effects,theme,i,true);garden[i].localScale=Vector3.zero;}
        }
        void Lift()
        {
            lifted=true;popup=new GameObject("Lifted character illustration").transform;popup.SetParent(effects,false);
            popStart=board.InverseTransformPoint(portrait.transform.position);popup.localPosition=popStart;popup.rotation=portrait.transform.rotation;
            portrait.transform.SetParent(popup,true);artScale=portrait.transform.localScale;portrait.transform.localPosition=Vector3.zero;portrait.transform.localRotation=Quaternion.identity;
            orbitFrame=popup.localRotation;radiusX=artScale.x*.625f+width*.045f;radiusY=artScale.y*.625f+width*.03f;
        }
        Vector3 OrbitPoint(int i,float turns)
        {
            float angle=i*Mathf.PI*2/ItemCount+turns*Mathf.PI*2;float ring=1+(i%3)*.035f;
            return center+orbitFrame*new Vector3(Mathf.Cos(angle)*radiusX*ring,Mathf.Sin(angle)*radiusY*ring,.014f);
        }
        void SetDissolve(float value){Dissolve=value;if(portrait!=null){properties.SetFloat("_Dissolve",value);portrait.SetPropertyBlock(properties);}}
        void Update()
        {
            if(bits==null){enabled=false;return;}if(paused?.Invoke()==true)return;age+=Time.deltaTime;
            if(!lifted&&age>=.18f)Lift();if(!lifted)return;
            float pop=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.18f)/.32f));popup.localPosition=Vector3.Lerp(popStart,center,pop);
            portrait.transform.localScale=artScale*Mathf.Lerp(1,1.25f,pop);
            if(Camera.main!=null){var away=popup.position-Camera.main.transform.position;away.y=0;if(away.sqrMagnitude>.0001f)popup.rotation=Quaternion.LookRotation(away,board.up);}
            SetDissolve(Mathf.Clamp01((age-ArrivalTime-.2f)/.9f));ArrivedParticles=0;
            for(int i=0;i<bits.Length;i++)
            {
                float flightStart=OrbitStart+OrbitDuration+i*.018f;
                float travel=Mathf.Clamp01((age-flightStart)/FlightDuration);
                var from=OrbitPoint(i,2);var control=(from+target)*.5f+Vector3.up*(width*.12f);
                if(age<OrbitStart)bits[i].localPosition=Vector3.Lerp(center+orbitFrame*Vector3.forward*.014f,OrbitPoint(i,0),Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.22f)/.33f)));
                else if(age<flightStart)bits[i].localPosition=OrbitPoint(i,OrbitTurns);
                else bits[i].localPosition=(1-travel)*(1-travel)*from+2*(1-travel)*travel*control+travel*travel*target;
                if(travel>=1)ArrivedParticles++;
                float appear=Mathf.Clamp01((age-.22f)/.16f),fade=1-Mathf.Clamp01((age-flightStart-FlightDuration)/.22f);
                bits[i].localScale=Vector3.one*(.028f+.004f*(i%3))*(width/.64f)*appear*fade;
                bits[i].localRotation=orbitFrame*Quaternion.Euler(age*65+i*23,age*95+i*41,age*70+i*31);
            }
            SproutedCount=0;
            for(int i=0;i<garden.Length;i++)
            {
                float growth=Mathf.Clamp01((age-ArrivalTime-i*.08f)/.55f);float ease=Mathf.SmoothStep(0,1,growth);
                float bounce=1+Mathf.Sin(growth*Mathf.PI*3)*(1-growth)*.18f;float retract=1-Mathf.Clamp01((age-(EndTime-.4f))/.4f);
                float a=i*Mathf.PI*.5f+.7f;var ground=new Vector3(target.x,.001f,target.z);
                garden[i].localPosition=ground+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*(width*.042f*ease);
                garden[i].localScale=Vector3.one*(width*.036f*ease*bounce*retract);garden[i].localRotation=Quaternion.Euler(0,i*81,Mathf.Sin(growth*Mathf.PI*2)*8*(1-growth));
                if(growth>0){SproutedCount++;SproutingObserved=true;}
            }
            if(age>=EndTime){SetDissolve(1);Cleanup();enabled=false;}
        }
        void Cleanup(){if(effects!=null){effects.gameObject.SetActive(false);Destroy(effects.gameObject);effects=null;}bits=null;garden=null;popup=null;portrait=null;assets?.Dispose();assets=null;}
        void OnDisable(){Cleanup();}
        void OnDestroy(){Cleanup();}
    }
}
