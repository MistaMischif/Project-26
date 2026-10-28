using System;
using System.Collections.Generic;
using UnityEngine;
namespace LivingTable.CandyLand
{
    public sealed class CandyCharacterPlay : MonoBehaviour
    {
        CandyCharacterActor actor;CandyTable table;AudioSource voice;AudioClip[] lines;
        static AudioSource currentVoice;
        public static bool AnySpeaking=>currentVoice!=null&&currentVoice.isPlaying;
        public bool HasPlayedVoice {get;private set;}
        public int VoiceClipCount=>lines==null?0:lines.Length;
        public int EffectCount=>items.Count;
        public Transform EffectRoot=>root==null?null:root.transform;
        public float EffectAge=>age;
        public string LastLine {get;private set;}="";
        readonly List<Transform> items=new List<Transform>(),poofs=new List<Transform>();
        readonly List<AudioSource> flaps=new List<AudioSource>();
        readonly List<Material> materials=new List<Material>();
        GameObject root;Mesh wing,cane;Material purple,white,red,black,glow;Vector3 origin;float size,age,lastAt=-10;int nextLine;bool wasPaused;
        static readonly string[][] Words={
            new[]{"Oops! My berries are making a break for it!","I was saving those for my second breakfast!"},
            new[]{"Fresh candy canes! No watering required!","You have got me in mint condition!"},
            new[]{"That tickles my gumdrops!","I am not wobbly. I am extra jolly!"},
            new[]{"Careful, dear. My pockets are full of peanuts!","A little nutty? Why, thank you!"},
            new[]{"One royal twirl, extra sprinkles!","My dance moves are pop approved!"},
            new[]{"Stay cool, my sweet little snowflake.","A royal wiggle? How ice of you!"},
            new[]{"By royal decree, more giggles!","My crown is straight. My jokes are crooked!"},
            new[]{"My bats! They always leave before the punch line!","Behold, my very dramatic entrance!"},
            new[]{"Oops. Was that your shoe, or my snack?","I give the stickiest hugs in Candy Land!"}
        };
        public void Initialize(CandyTable owner,CandyCharacterActor character)
        {
            table=owner;actor=character;actor.Interacted+=Trigger;
            voice=gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.spatialBlend=.65f;voice.minDistance=.3f;voice.maxDistance=3;voice.volume=.6f;
            lines=new[]{Resources.Load<AudioClip>("Audio/Voices/"+CandyCharacterModel.Ids[actor.CharacterId]+"-0"),Resources.Load<AudioClip>("Audio/Voices/"+CandyCharacterModel.Ids[actor.CharacterId]+"-1")};
            purple=Material(new Color(.45f,.035f,.65f));white=Material(new Color(1,.93f,.8f));red=Material(new Color(.92f,.025f,.06f));black=Material(new Color(.045f,.015f,.075f));glow=Material(new Color(.8f,.4f,1),true);
        }
        Material Material(Color color,bool sparkle=false){var m=new Material(Resources.Load<Shader>(sparkle?"CandyMagicGlow":"CandyPiecePlastic"));m.color=color;materials.Add(m);return m;}
        Transform Part(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Material m)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;Destroy(g.GetComponent<Collider>());g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=m;return g.transform;
        }
        Transform Node(Transform parent,string name){var g=new GameObject(name);g.transform.SetParent(parent,false);return g.transform;}
        void Trigger()
        {
            if(table==null||table.Paused||Time.unscaledTime-lastAt<1.6f)return;lastAt=Time.unscaledTime;
            table.CelebrateCharacter(actor.CharacterId,false);
            int line=nextLine++%2;LastLine=Words[actor.CharacterId][line];table.ShowCharacterLine(CandyCharacterModel.Names[actor.CharacterId]+": "+LastLine);
            if(table.CharacterVoicesEnabled&&lines[line]!=null)
            {
                if(currentVoice!=null)currentVoice.Stop();currentVoice=voice;voice.clip=lines[line];voice.Play();HasPlayedVoice=true;
            }
            ClearEffects();int id=actor.CharacterId;if(id!=0&&id!=1&&id!=7)return;
            root=new GameObject("Character playful props");root.transform.SetParent(transform.parent,false);origin=transform.localPosition;origin.y=.0005f;size=transform.localScale.y;age=0;
            int count=id==1?3:4;
            for(int i=0;i<count;i++)
            {
                var item=Node(root.transform,id==0?"Rolling purple berry":id==1?"Sprouting candy cane":"Flying bat");item.localScale=Vector3.zero;items.Add(item);
                if(id==0)
                {
                    Part(item,"Berry",PrimitiveType.Sphere,Vector3.zero,Vector3.one,purple);Part(item,"Berry stem",PrimitiveType.Capsule,new Vector3(0,.5f,0),new Vector3(.08f,.13f,.08f),black);
                    var puff=Node(root.transform,"Tiny berry poof");puff.localScale=Vector3.zero;poofs.Add(puff);
                    Part(puff,"Poof cloud",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.4f,white);
                    for(int k=0;k<6;k++){float a=k*Mathf.PI/3;Part(puff,"Poof sparkle",PrimitiveType.Quad,new Vector3(Mathf.Cos(a),.3f,Mathf.Sin(a))*.55f,Vector3.one*.25f,glow);}
                }
                else if(id==1)
                {
                    if(cane==null)cane=CaneMesh();item.gameObject.AddComponent<MeshFilter>().sharedMesh=cane;item.gameObject.AddComponent<MeshRenderer>().sharedMaterials=new[]{white,red};
                }
                else
                {
                    Part(item,"Bat body",PrimitiveType.Sphere,Vector3.zero,new Vector3(.18f,.48f,.18f),black);
                    Part(item,"Bat head",PrimitiveType.Sphere,new Vector3(0,.23f,0),new Vector3(.24f,.23f,.19f),black);
                    for(int side=-1;side<=1;side+=2)
                    {
                        Part(item,"Pointy ear",PrimitiveType.Capsule,new Vector3(side*.07f,.36f,0),new Vector3(.045f,.09f,.045f),black);
                        var w=Node(item,side<0?"Left wing":"Right wing");if(wing==null)wing=WingMesh();w.gameObject.AddComponent<MeshFilter>().sharedMesh=wing;w.gameObject.AddComponent<MeshRenderer>().sharedMaterial=black;w.localScale=new Vector3(side,1,1);
                    }
                    var audio=item.gameObject.AddComponent<AudioSource>();audio.clip=Resources.Load<AudioClip>("Audio/bat-flutter");audio.loop=true;audio.playOnAwake=false;audio.spatialBlend=1;audio.minDistance=.2f;audio.maxDistance=2;audio.volume=.12f;audio.pitch=1+i*.06f;flaps.Add(audio);
                }
            }
        }
        void Update()
        {
            if(table==null)return;
            if(!table.CharacterVoicesEnabled&&voice.isPlaying)voice.Stop();
            if(wasPaused!=table.Paused)
            {
                wasPaused=table.Paused;if(wasPaused){voice.Pause();foreach(var f in flaps)f.Pause();}else{if(table.CharacterVoicesEnabled)voice.UnPause();foreach(var f in flaps)f.UnPause();}
            }
            if(table.Paused||root==null)return;age+=Time.deltaTime;
            int id=actor.CharacterId;float duration=id==0?2.8f:id==1?3.8f:3.1f;
            if(age>duration){ClearEffects();return;}
            for(int i=0;i<items.Count;i++)
            {
                var item=items[i];float t=age-i*.12f;if(t<0)continue;float a=i*2.39996f+.4f;
                if(id==0)
                {
                    float travel=Mathf.Min(t,1.65f)*size*.55f;
                    float drop=Mathf.Max(0,size*.45f-.38f*t*t);
                    float bounce=t>.33f?size*.10f*Mathf.Abs(Mathf.Sin((t-.33f)*13))*Mathf.Exp(-(t-.33f)*5):0;
                    item.localPosition=origin+new Vector3(.02f+Mathf.Cos(a)*travel,.0035f+drop+bounce,Mathf.Sin(a)*travel);
                    item.localScale=Vector3.one*(t<1.65f?size*.10f:0);item.localRotation=Quaternion.Euler(t*500,i*65,t*120);
                    var puff=poofs[i];float u=Mathf.Clamp01((t-1.65f)/.34f);puff.localPosition=item.localPosition;puff.localScale=Vector3.one*(Mathf.Sin(u*Mathf.PI)*size*.13f);
                    foreach(Transform part in puff)if(part.name=="Poof sparkle"&&Camera.main!=null)part.rotation=Camera.main.transform.rotation;
                }
                else if(id==1)
                {
                    float appear=Mathf.SmoothStep(0,1,t/.65f),leave=Mathf.SmoothStep(0,1,(t-2.5f)/.8f);
                    item.localPosition=origin+new Vector3(Mathf.Cos(a)*size*.6f,0,Mathf.Sin(a)*size*.6f);
                    item.localScale=Vector3.one*(size*.7f*appear*(1-leave));item.localRotation=Quaternion.Euler(0,i*110,Mathf.Sin(t*5+i)*3);
                }
                else
                {
                    item.localPosition=origin+new Vector3(Mathf.Cos(a)*size*t*.7f,size*(t*.8f+t*t*.55f),Mathf.Sin(a)*size*t*.7f);
                    item.localScale=Vector3.one*(size*.3f*Mathf.SmoothStep(0,1,t/.15f)*Mathf.Clamp01((2.7f-t)/.3f));item.localRotation=Quaternion.Euler(0,i*75+Mathf.Sin(t*3)*10,0);
                    item.Find("Left wing").localRotation=Quaternion.Euler(0,Mathf.Sin(t*Mathf.PI*2/.22f*flaps[i].pitch)*60,0);item.Find("Right wing").localRotation=Quaternion.Euler(0,-Mathf.Sin(t*Mathf.PI*2/.22f*flaps[i].pitch)*60,0);
                    if(t<2.7f&&!flaps[i].isPlaying)flaps[i].Play();if(t>=2.7f)flaps[i].Stop();
                }
            }
        }
        public void MuteVoice(){if(voice!=null)voice.Stop();}
        public void StopInteraction(){ClearEffects();if(voice!=null)voice.Stop();}
        void ClearEffects(){if(root!=null){root.SetActive(false);Destroy(root);}root=null;items.Clear();poofs.Clear();foreach(var f in flaps)if(f!=null)f.Stop();flaps.Clear();}
        static Mesh WingMesh()
        {
            var m=new Mesh{name="Scalloped bat wing"};m.vertices=new[]{Vector3.zero,new Vector3(.85f,.4f,0),new Vector3(1,-.1f,0),new Vector3(.7f,-.02f,0),new Vector3(.5f,-.32f,0),new Vector3(.3f,-.15f,0),new Vector3(0,-.35f,0)};
            m.triangles=new[]{0,1,2,0,2,3,0,3,4,0,4,5,0,5,6};m.RecalculateNormals();m.RecalculateBounds();return m;
        }
        static Mesh CaneMesh()
        {
            const int rings=40,sides=10;var vertices=new List<Vector3>();var whiteFaces=new List<int>();var redFaces=new List<int>();
            for(int j=0;j<=rings;j++)
            {
                float t=(float)j/rings,a=Mathf.Max(0,(t-.65f)/.35f)*Mathf.PI;var center=t<.65f?new Vector3(0,t/.65f*.7f,0):new Vector3((1-Mathf.Cos(a))*.18f,.7f+Mathf.Sin(a)*.18f,0);var normal=new Vector3(Mathf.Cos(a),-Mathf.Sin(a),0);
                for(int k=0;k<sides;k++){float theta=k*Mathf.PI*2/sides;vertices.Add(center+(normal*Mathf.Cos(theta)+Vector3.forward*Mathf.Sin(theta))*.045f);}
                if(j==rings)continue;var tris=(j/3)%2==0?whiteFaces:redFaces;
                for(int k=0;k<sides;k++){int v=j*sides+k,n=j*sides+(k+1)%sides;tris.AddRange(new[]{v,n,v+sides,n,n+sides,v+sides});}
            }
            for(int k=1;k<sides-1;k++){whiteFaces.AddRange(new[]{0,k+1,k});redFaces.AddRange(new[]{rings*sides,rings*sides+k,rings*sides+k+1});}
            var mesh=new Mesh{name="Striped curved candy cane"};mesh.SetVertices(vertices);mesh.subMeshCount=2;mesh.SetTriangles(whiteFaces,0);mesh.SetTriangles(redFaces,1);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void OnDestroy(){if(actor!=null)actor.Interacted-=Trigger;StopInteraction();foreach(var m in materials)if(m!=null)Destroy(m);if(wing!=null)Destroy(wing);if(cane!=null)Destroy(cane);}
    }
}
