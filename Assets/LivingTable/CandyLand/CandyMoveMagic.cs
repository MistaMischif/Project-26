using System;
using System.Collections.Generic;
using UnityEngine;
namespace LivingTable.CandyLand
{
    public sealed class CandyMoveMagic : MonoBehaviour
    {
        public Func<bool> IsPaused;
        public Vector3 From {get;private set;}
        public Vector3 To {get;private set;}
        public int Theme {get;private set;}=-1;
        public Color LandingColor {get;private set;}
        public Transform SquareGlow {get;private set;}
        Material glowMaterial;Vector2 squareSize;Quaternion squareRotation;float releasedAt=-1;
        public void Landed(){releasedAt=age;}
        public int LiveCount=>bits.Count;
        public float Elapsed=>age;
        readonly List<Transform> bits=new List<Transform>();
        readonly List<Material> materials=new List<Material>();
        float age=10,width;Action arrived;bool signaled;
        static readonly Color[] hues={new Color(.65f,.16f,.85f),new Color(1,.12f,.22f),new Color(.4f,1,.25f),new Color(1,.65f,.20f),new Color(1,.25f,.68f),new Color(.45f,.9f,1)};
        Material Make(Color color){var m=new Material(Resources.Load<Shader>("CandyPiecePlastic"));m.color=color;materials.Add(m);return m;}
        Transform Part(Transform parent,PrimitiveType shape,Vector3 pos,Vector3 scale,Material material)
        {
            var g=GameObject.CreatePrimitive(shape);Destroy(g.GetComponent<Collider>());g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;return g.transform;
        }
        public void Play(Vector3 from,Vector3 to,int theme,float boardWidth,Color landingColor,Vector3 tangent,Vector2 size,Action onArrival,bool emitSparkles=true)
        {
            Clear();From=from;To=to;Theme=theme;width=boardWidth;age=0;arrived=onArrival;signaled=false;releasedAt=-1;LandingColor=landingColor;squareSize=size;
            squareRotation=Quaternion.LookRotation(tangent.sqrMagnitude>.00001f?tangent:Vector3.forward,Vector3.up)*Quaternion.Euler(90,0,0);
            Color.RGBToHSV(landingColor,out float hue,out _,out _);var neon=Color.HSVToRGB(hue,1,1);
            glowMaterial=new Material(Resources.Load<Shader>("CandyMagicGlow"));glowMaterial.color=neon;glowMaterial.SetFloat("_Square",1);glowMaterial.SetFloat("_Saturated",1);glowMaterial.SetFloat("_DstBlend",10);materials.Add(glowMaterial);
            SquareGlow=Part(transform,PrimitiveType.Quad,new Vector3(to.x,.0018f,to.z),Vector3.zero,glowMaterial);SquareGlow.name="Neon destination square interior";SquareGlow.localRotation=squareRotation;
            var sparkle=new Material(Resources.Load<Shader>("CandyMagicGlow"));sparkle.color=neon;materials.Add(sparkle);
            var white=Make(new Color(1,1,.82f));var color=Make(theme>=0?hues[theme]:new Color(1,.8f,.3f));
            // 28 upward twinkles and at most 12 themed trail charms, bounded on every draw.
            for(int i=0;i<(emitSparkles?28:0);i++)
            {
                var bit=Part(transform,PrimitiveType.Quad,Vector3.zero,Vector3.zero,sparkle);bit.name="Destination rising sparkle";bits.Add(bit);
            }
            if(theme<0)return;
            for(int i=0;i<12;i++)
            {
                var root=new GameObject("Character trail charm").transform;root.SetParent(transform,false);root.localScale=Vector3.zero;bits.Add(root);
                switch(theme)
                {
                    case 0: // Plum and leaf
                        Part(root,PrimitiveType.Sphere,Vector3.zero,new Vector3(.8f,1,.8f),color);Part(root,PrimitiveType.Sphere,new Vector3(.25f,.5f,0),new Vector3(.5f,.14f,.24f),white);break;
                    case 1: // Striped peppermint sweet
                        Part(root,PrimitiveType.Cylinder,Vector3.zero,new Vector3(1,.15f,1),white);
                        for(int j=0;j<3;j++){var p=Part(root,PrimitiveType.Cube,new Vector3(0,.16f,0),new Vector3(.18f,.08f,.94f),color);p.localRotation=Quaternion.Euler(0,j*60,0);}break;
                    case 2: // Bright gumdrop
                        Part(root,PrimitiveType.Sphere,Vector3.zero,new Vector3(1,1.2f,1),i%2==0?color:white);break;
                    case 3: // Peanut halves
                        Part(root,PrimitiveType.Sphere,new Vector3(0,-.23f,0),new Vector3(.65f,.8f,.65f),color);Part(root,PrimitiveType.Sphere,new Vector3(0,.23f,0),new Vector3(.65f,.8f,.65f),color);break;
                    case 4: // Lollipop with stick
                        Part(root,PrimitiveType.Sphere,new Vector3(0,.2f,0),new Vector3(.8f,.8f,.25f),color);Part(root,PrimitiveType.Cube,new Vector3(0,-.35f,0),new Vector3(.1f,.65f,.1f),white);break;
                    case 5: // Six-point ice crystal
                        for(int j=0;j<3;j++){var p=Part(root,PrimitiveType.Cube,Vector3.zero,new Vector3(.1f,1,.1f),j==1?white:color);p.localRotation=Quaternion.Euler(0,0,j*60);}break;
                }
            }
        }
        void Update()
        {
            if((bits.Count==0&&SquareGlow==null)||IsPaused?.Invoke()==true)return;age+=Time.deltaTime;
            float fade=releasedAt<0?1:Mathf.Clamp01(1-(age-releasedAt));
            if(SquareGlow!=null){SquareGlow.localScale=new Vector3(squareSize.x,squareSize.y,1);glowMaterial.SetFloat("_Strength",fade*(1.15f+Mathf.Sin(age*6)*.12f));}
            if(!signaled&&age>=1.5f){signaled=true;arrived?.Invoke();}
            if(releasedAt>=0&&age>=2.7f&&age-releasedAt>=1){Clear();return;}
            for(int i=0;i<bits.Count;i++)
            {
                var bit=bits[i];
                if(i<28)
                {
                    float t=Mathf.Clamp01((age-i*.017f)/2.1f),a=i*2.39996f,r=(.002f+t*.010f)*width/.64f;
                    bit.localPosition=new Vector3(To.x+Mathf.Cos(a)*r,.002f+t*width*.19f,To.z+Mathf.Sin(a)*r);
                    float twinkle=.55f+.45f*Mathf.Sin(age*15+i*2.3f);
                    bit.localScale=Vector3.one*(.022f*width/.64f*Mathf.Sin(t*Mathf.PI)*twinkle);
                    if(Camera.main!=null)bit.rotation=Camera.main.transform.rotation*Quaternion.Euler(0,0,i*31+age*24);
                }
                else
                {
                    float t=Mathf.Clamp01((age-(i-28)*.045f)/1.15f);bit.localPosition=Vector3.Lerp(From,To,t)+Vector3.up*(.012f+Mathf.Sin(t*Mathf.PI)*width*.12f);
                    float visible=age<(i-28)*.045f?0:Mathf.Sin(t*Mathf.PI);bit.localScale=Vector3.one*(width*.013f*visible);bit.localRotation=Quaternion.Euler(age*70,i*47,age*45);
                }
            }
        }
        public void Clear(){if(SquareGlow!=null){SquareGlow.gameObject.SetActive(false);Destroy(SquareGlow.gameObject);SquareGlow=null;}foreach(var bit in bits)if(bit!=null){bit.gameObject.SetActive(false);Destroy(bit.gameObject);}bits.Clear();foreach(var m in materials)if(m!=null)Destroy(m);materials.Clear();arrived=null;}
        void OnDestroy(){Clear();}
    }
}
