using System;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // All geometry is in card space: no world-facing portraits or board characters.
    public sealed class CandyCardMagic : MonoBehaviour
    {
        static readonly Color[] Hues={new Color(.72f,.25f,1),new Color(1,.18f,.25f),new Color(.4f,1,.25f),new Color(1,.7f,.2f),new Color(1,.25f,.7f),new Color(.45f,.9f,1)};
        Renderer portrait; MaterialPropertyBlock properties; Material sparkle;
        Transform[] bits; Func<bool> paused,presenting; float age,settledAge;
        public float Dissolve {get;private set;}
        public int ParticleCount=>bits==null?0:bits.Length;
        public void Initialize(Renderer art,int theme,Func<bool> isPaused,Func<bool> isPresenting,bool restored)
        {
            portrait=art;paused=isPaused;presenting=isPresenting;properties=new MaterialPropertyBlock();
            properties.SetColor("_SparkColor",Hues[Mathf.Clamp(theme,0,5)]);
            if(restored){SetDissolve(1);enabled=false;return;}
            sparkle=new Material(Resources.Load<Shader>("CandyMagicGlow"));sparkle.color=Hues[Mathf.Clamp(theme,0,5)];
            bits=new Transform[40];
            for(int i=0;i<bits.Length;i++)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="Card sparkle";Destroy(go.GetComponent<Collider>());
                go.transform.SetParent(transform,false);go.transform.localRotation=Quaternion.Euler(90,0,0);
                go.transform.localPosition=new Vector3(0,.0015f,0);go.transform.localScale=Vector3.zero;
                go.GetComponent<Renderer>().sharedMaterial=sparkle;bits[i]=go.transform;
            }
        }
        void SetDissolve(float value){Dissolve=value;properties.SetFloat("_Dissolve",value);portrait.SetPropertyBlock(properties);}
        void Update()
        {
            if(paused?.Invoke()==true)return;
            age+=Time.deltaTime;
            if(age>.1f&&presenting?.Invoke()!=true)settledAge+=Time.deltaTime;
            SetDissolve(Mathf.Clamp01((settledAge-.1f)/1.1f));
            for(int i=0;i<bits.Length;i++)
            {
                bool edge=i>=24;float t=edge?settledAge-.1f-(i-24)*.025f:age-i*.009f;
                float life=edge?.85f:1.5f;float u=Mathf.Clamp01(t/life);
                float angle=i*2.399963f;float radius=(edge?.025f:0)+u*(edge?.035f:.075f);
                bits[i].localPosition=new Vector3(Mathf.Cos(angle)*radius,.0015f,Mathf.Sin(angle)*radius);
                float size=t>0&&t<life?(1-u)*(.004f+.004f*(i%3))*(.7f+.3f*Mathf.Sin(age*24+i)):0;
                bits[i].localScale=Vector3.one*size;
            }
            if(settledAge>=1.4f){foreach(var bit in bits)Destroy(bit.gameObject);bits=null;Destroy(sparkle);sparkle=null;enabled=false;}
        }
        void OnDestroy(){if(sparkle!=null)Destroy(sparkle);}
    }
}
