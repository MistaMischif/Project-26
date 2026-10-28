using System;
using System.Collections.Generic;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // Bounded, deterministic, cosmetic celebration; never consumes the deck random source.
    public sealed class CandyCardCelebration:MonoBehaviour
    {
        public Func<bool> IsPaused;
        public int Theme {get;private set;}
        public int LiveCount=>bits.Count;
        public float Age=>age;
        public Vector3 Origin {get;private set;}
        readonly List<Transform> bits=new List<Transform>();
        readonly List<Material> materials=new List<Material>();
        float age,width;Transform portrait;Vector3 portraitSize;
        static readonly Color[] hues={new Color(.67f,.12f,.9f),new Color(1,.1f,.2f),new Color(.96f,.2f,.65f),new Color(1,.65f,.18f),new Color(1,.3f,.7f),new Color(.35f,.85f,1),new Color(1,.7f,.05f),new Color(1,.3f,.05f),new Color(.4f,.15f,.06f)};
        Material Mat(Color c,bool glow=false){var m=new Material(Resources.Load<Shader>(glow?"CandyMagicGlow":"CandyPiecePlastic"));m.color=c;materials.Add(m);return m;}
        Transform Part(Transform parent,PrimitiveType type,Vector3 p,Vector3 scale,Material m)
        {
            var go=GameObject.CreatePrimitive(type);Destroy(go.GetComponent<Collider>());go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=m;return go.transform;
        }
        public void Play(int theme,float boardWidth,Vector3 origin)
        {
            Clear();Theme=theme;age=0;width=boardWidth;Origin=origin;
            var color=Mat(hues[theme]);var cream=Mat(new Color(1,.96f,.78f));var dark=Mat(new Color(.19f,.04f,.23f));
            var glow=Mat(hues[theme],true);var gold=Mat(new Color(1,.85f,.3f),true);
            for(int i=0;i<54;i++){var bit=Part(transform,PrimitiveType.Quad,Vector3.zero,Vector3.zero,i%3==0?gold:glow);bit.name="Firework star";bits.Add(bit);}
            for(int i=0;i<20;i++)
            {
                var root=new GameObject("Character treat burst").transform;root.SetParent(transform,false);root.localScale=Vector3.zero;bits.Add(root);
                switch(theme)
                {
                    case 0:
                        Part(root,PrimitiveType.Sphere,Vector3.zero,Vector3.one,color);Part(root,PrimitiveType.Capsule,new Vector3(0,.55f,0),new Vector3(.1f,.15f,.1f),dark);break;
                    case 1:
                        Part(root,PrimitiveType.Cylinder,Vector3.zero,new Vector3(1,.12f,1),cream);
                        for(int j=0;j<3;j++){var stripe=Part(root,PrimitiveType.Cube,new Vector3(0,.13f,0),new Vector3(.16f,.06f,.9f),color);stripe.localRotation=Quaternion.Euler(0,j*60,0);}break;
                    case 2:Part(root,PrimitiveType.Sphere,Vector3.zero,new Vector3(.9f,1.2f,.9f),i%2==0?color:cream);break;
                    case 3:
                        for(int j=-1;j<=1;j+=2)Part(root,PrimitiveType.Sphere,new Vector3(0,j*.24f,0),new Vector3(.65f,.8f,.65f),color);break;
                    case 4:
                        Part(root,PrimitiveType.Sphere,new Vector3(0,.2f,0),new Vector3(1,1,.25f),color);Part(root,PrimitiveType.Cube,new Vector3(0,-.5f,0),new Vector3(.1f,.8f,.1f),cream);break;
                    case 5:
                        for(int j=0;j<3;j++){var crystal=Part(root,PrimitiveType.Cube,Vector3.zero,new Vector3(.13f,1.2f,.13f),j==0?cream:color);crystal.localRotation=Quaternion.Euler(0,0,j*60);}break;
                    case 6:
                        Part(root,PrimitiveType.Sphere,Vector3.zero,new Vector3(1,.6f,1),cream);Part(root,PrimitiveType.Sphere,Vector3.up*.42f,Vector3.one*.4f,color);break;
                    case 8:
                        Part(root,PrimitiveType.Sphere,Vector3.zero,new Vector3(.75f,1,.75f),color);Part(root,PrimitiveType.Sphere,Vector3.up*.4f,Vector3.one*.35f,color);break;
                    default: // Wrapped trick-or-treat sweets for Lord Licorice.
                        Part(root,PrimitiveType.Sphere,Vector3.zero,new Vector3(1,.65f,.65f),i%2==0?color:dark);
                        for(int j=-1;j<=1;j+=2){var wrap=Part(root,PrimitiveType.Cube,new Vector3(j*.65f,0,0),new Vector3(.35f,.5f,.1f),i%2==0?dark:color);wrap.localRotation=Quaternion.Euler(45,0,45);}break;
                }
            }
            var texture=Resources.Load<Texture2D>("Celebrations/"+CandyCharacterModel.Ids[theme]);
            if(texture!=null)
            {
                var m=new Material(Resources.Load<Shader>("CharacterCard"));m.mainTexture=texture;materials.Add(m);
                portrait=Part(transform,PrimitiveType.Quad,Vector3.zero,Vector3.zero,m);portrait.name="Excited illustrated character";
                portraitSize=new Vector3((float)texture.width/texture.height,1,1)*width*.24f;
            }
        }
        void Update()
        {
            if(bits.Count==0||IsPaused?.Invoke()==true)return;age+=Time.deltaTime;
            if(age>=3.2f){Clear();return;}
            for(int i=0;i<bits.Count;i++)
            {
                var bit=bits[i];
                if(i<54)
                {
                    int burst=i/18;float t=age-.18f-burst*.28f;float a=(i%18)*2.39996f;
                    var center=Origin+new Vector3((burst-1)*width*.12f,width*(.26f+burst*.045f),0);
                    var direction=new Vector3(Mathf.Cos(a),Mathf.Sin(a),Mathf.Sin(a*2)*.3f);
                    bit.localPosition=center+direction*Mathf.Max(0,t)*width*.18f-Vector3.up*Mathf.Max(0,t*t)*width*.045f;
                    float fade=t<0?0:Mathf.Clamp01(1-t/1.35f);bit.localScale=Vector3.one*width*.035f*fade*(.7f+.3f*Mathf.Sin(t*30+i));
                    if(Camera.main!=null)bit.rotation=Camera.main.transform.rotation;
                }
                else
                {
                    int k=i-54;float t=Mathf.Max(0,age-.2f-k*.012f),a=k*2.39996f;
                    bit.localPosition=Origin+new Vector3(Mathf.Cos(a)*t*width*.17f,width*.08f+t*width*(.36f+k%3*.045f)-t*t*width*.24f,Mathf.Sin(a)*t*width*.12f);
                    float fade=Mathf.Clamp01((2.6f-age)/.5f);if(bit.localPosition.y<.015f)fade=0;
                    bit.localScale=Vector3.one*(width*.024f*fade*Mathf.Clamp01(t*10));bit.localRotation=Quaternion.Euler(age*150+k*17,age*210,age*100);
                }
            }
            if(portrait!=null)
            {
                float envelope=Mathf.Clamp01(age*5)*Mathf.Clamp01((2.8f-age)*3);
                portrait.localPosition=Origin+Vector3.up*width*(.42f+Mathf.Abs(Mathf.Sin(age*8))*.025f);
                portrait.localScale=portraitSize*envelope*(1+Mathf.Sin(age*12)*.04f);
                if(Camera.main!=null)portrait.rotation=Camera.main.transform.rotation*Quaternion.Euler(0,0,Mathf.Sin(age*9)*7);
            }
        }
        public void Clear(){foreach(var bit in bits)if(bit!=null){bit.gameObject.SetActive(false);Destroy(bit.gameObject);}bits.Clear();if(portrait!=null){portrait.gameObject.SetActive(false);Destroy(portrait.gameObject);portrait=null;}foreach(var m in materials)Destroy(m);materials.Clear();}
        void OnDestroy(){Clear();}
    }
}
