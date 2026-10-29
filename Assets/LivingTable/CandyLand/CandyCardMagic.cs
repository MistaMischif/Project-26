using System;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // Owned by the discard card, but the lifted illustration and particles live in board space.
    public sealed class CandyCardMagic : MonoBehaviour
    {
        static readonly Color[] Hues={new Color(.72f,.25f,1),new Color(1,.18f,.25f),new Color(.4f,1,.25f),new Color(1,.7f,.2f),new Color(1,.25f,.7f),new Color(.45f,.9f,1)};
        Renderer portrait;MaterialPropertyBlock properties;Material sparkle;
        Transform board,effects,popup;Transform[] bits;Func<bool> paused;
        Vector3 target,center,popStart,artScale;float age,width;bool lifted;
        public float Dissolve {get;private set;}
        public int ParticleCount=>bits==null?0:bits.Length;
        public Renderer Portrait=>portrait;
        public Transform Popup=>popup;
        public Vector3 Target=>target;
        public int ArrivedParticles {get;private set;}
        public float Elapsed=>age;
        public void Initialize(Renderer art,int theme,Func<bool> isPaused,Func<bool> isPresenting,bool restored,Transform boardFrame,Vector3 destination,float boardWidth)
        {
            portrait=art;paused=isPaused;board=boardFrame;target=destination;width=boardWidth;
            center=new Vector3(0,width*.30f,0);properties=new MaterialPropertyBlock();properties.SetColor("_SparkColor",Hues[Mathf.Clamp(theme,0,5)]);
            if(restored){SetDissolve(1);enabled=false;return;}
            effects=new GameObject("Special card board celebration").transform;effects.SetParent(board,false);
            sparkle=new Material(Resources.Load<Shader>("CandyMagicGlow"));sparkle.color=Hues[Mathf.Clamp(theme,0,5)];
            bits=new Transform[40];
            for(int i=0;i<bits.Length;i++)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="Character sparkle to destination";Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(effects,false);go.transform.localScale=Vector3.zero;go.GetComponent<Renderer>().sharedMaterial=sparkle;bits[i]=go.transform;
            }
        }
        void Lift()
        {
            lifted=true;popup=new GameObject("Lifted character illustration").transform;popup.SetParent(effects,false);
            popStart=board.InverseTransformPoint(portrait.transform.position);popup.localPosition=popStart;popup.rotation=portrait.transform.rotation;
            portrait.transform.SetParent(popup,true);artScale=portrait.transform.localScale;portrait.transform.localPosition=Vector3.zero;portrait.transform.localRotation=Quaternion.identity;
        }
        void SetDissolve(float value){Dissolve=value;if(portrait!=null){properties.SetFloat("_Dissolve",value);portrait.SetPropertyBlock(properties);}}
        void Update()
        {
            if(bits==null){enabled=false;return;}if(paused?.Invoke()==true)return;age+=Time.deltaTime;
            if(!lifted&&age>=.18f)Lift();if(!lifted)return;
            float pop=Mathf.SmoothStep(0,1,Mathf.Clamp01((age-.18f)/.32f));popup.localPosition=Vector3.Lerp(popStart,center,pop);
            portrait.transform.localScale=artScale*Mathf.Lerp(1,1.25f,pop);
            if(Camera.main!=null){var away=popup.position-Camera.main.transform.position;away.y=0;if(away.sqrMagnitude>.0001f)popup.rotation=Quaternion.LookRotation(away,board.up);}
            SetDissolve(Mathf.Clamp01((age-2.25f)/.9f));
            var rotation=popup.localRotation;var origin=center+rotation*Vector3.forward*.014f;ArrivedParticles=0;
            for(int i=0;i<bits.Length;i++)
            {
                float elapsed=age-.28f-i*.004f,angle=i*2.399963f;
                var burst=origin+rotation*new Vector3(Mathf.Cos(angle),Mathf.Sin(angle),0)*((.025f+(i%5)*.009f)*width/.64f);
                float travel=Mathf.Clamp01((elapsed-.35f)/1.35f);
                var control=(burst+target)*.5f+Vector3.up*(width*.16f);
                bits[i].localPosition=elapsed<.35f?Vector3.Lerp(origin,burst,Mathf.Clamp01(elapsed/.35f)):(1-travel)*(1-travel)*burst+2*(1-travel)*travel*control+travel*travel*target;
                if(travel>=1)ArrivedParticles++;
                float fade=elapsed<0?0:1-Mathf.Clamp01((elapsed-1.70f)/.25f);
                bits[i].localScale=Vector3.one*(.012f+.008f*(i%3))*(width/.64f)*fade*(.8f+.2f*Mathf.Sin(age*24+i));
                bits[i].localRotation=rotation*Quaternion.Euler(0,0,age*80+i*31);
            }
            if(age>=3.2f){SetDissolve(1);Cleanup();enabled=false;}
        }
        void Cleanup(){if(effects!=null){effects.gameObject.SetActive(false);Destroy(effects.gameObject);effects=null;}bits=null;popup=null;portrait=null;if(sparkle!=null){Destroy(sparkle);sparkle=null;}}
        void OnDisable(){Cleanup();}
        void OnDestroy(){Cleanup();}
    }
}
