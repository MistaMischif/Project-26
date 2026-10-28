using System;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // Small, fully volumetric toy sculptures. Coordinates are normalized; front is -Z.
    public static class CandyCharacterModel
    {
        public static readonly string[] Ids={"plumpy","mr-mint","jolly","gramma-nutt","princess-lolly","queen-frostine","king-kandy","lord-licorice","gloppy"};
        public static readonly string[] Names={"Plumpy","Mr. Mint","Jolly","Gramma Nutt","Princess Lolly","Queen Frostine","King Kandy","Lord Licorice","Gloppy"};
        public static void Build(Transform parent,int id,Func<Color,Material> material)
        {
            var b=new Sculptor(parent,material);b.Build(id);
        }
        sealed class Sculptor
        {
            Transform root;Func<Color,Material> mat;
            readonly Color cream=new Color(1,.96f,.83f),skin=new Color(1,.68f,.42f),pink=new Color(1,.22f,.52f),purple=new Color(.48f,.08f,.60f),blue=new Color(.06f,.45f,.85f),gold=new Color(1,.68f,.035f),brown=new Color(.35f,.12f,.035f),green=new Color(.27f,.65f,.08f);
            public Sculptor(Transform r,Func<Color,Material> m){root=r;mat=m;}
            GameObject Part(string n,Vector3 p,Vector3 s,Color c,PrimitiveType t=PrimitiveType.Sphere)
            {
                var g=GameObject.CreatePrimitive(t);g.name=n;g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=s;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=mat(c);return g;
            }
            void Ball(string n,float x,float y,float z,float sx,float sy,float sz,Color c)=>Part(n,new Vector3(x,y,z),new Vector3(sx,sy,sz),c);
            void Rod(string n,Vector3 a,Vector3 b,float radius,Color c)
            {
                var g=Part(n,(a+b)*.5f,new Vector3(radius*2,(b-a).magnitude*.5f,radius*2),c,PrimitiveType.Cylinder);g.transform.up=b-a;
            }
            void Cone(string n,Vector3 p,float bottom,float top,float height,Color c)
            {
                var g=new GameObject(n);g.transform.SetParent(root,false);g.transform.localPosition=p;
                int count=24;var v=new Vector3[(count+1)*2+2];var tri=new System.Collections.Generic.List<int>();
                for(int i=0;i<=count;i++){float a=i*2*Mathf.PI/count;v[i*2]=new Vector3(Mathf.Cos(a)*bottom,0,Mathf.Sin(a)*bottom);v[i*2+1]=new Vector3(Mathf.Cos(a)*top,height,Mathf.Sin(a)*top);if(i<count){int k=i*2;tri.AddRange(new[]{k,k+1,k+2,k+2,k+1,k+3});}}
                int bottomCenter=v.Length-2,topCenter=v.Length-1;v[topCenter]=Vector3.up*height;
                for(int i=0;i<count;i++)tri.AddRange(new[]{bottomCenter,i*2,(i+1)*2,topCenter,(i+1)*2+1,i*2+1});
                var mesh=new Mesh{name=n};mesh.vertices=v;mesh.triangles=tri.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=mat(c);
            }
            void Face(float y,float width,Color face,bool mustache=false)
            {
                Ball("Face",0,y,-.02f,width,.29f,.25f,face);
                foreach(int s in new[]{-1,1}){Ball("Eye white",s*.061f,y+.035f,-.137f,.072f,.085f,.024f,cream);Ball("Pupil",s*.059f,y+.034f,-.151f,.031f,.049f,.012f,Color.black);Ball("Eye glint",s*.059f-.007f,y+.045f,-.158f,.009f,.014f,.006f,Color.white);Ball("Cheek",s*.10f,y-.027f,-.129f,.052f,.026f,.014f,pink);}
                Ball("Nose",0,y-.003f,-.16f,.055f,.044f,.048f,face);
                Ball("Smile",0,y-.062f,-.143f,.09f,.035f,.017f,new Color(.4f,.035f,.07f));
                Ball("Smile teeth",0,y-.052f,-.151f,.064f,.012f,.008f,cream);
                if(mustache)foreach(int s in new[]{-1,1})Ball("Mustache",s*.044f,y-.033f,-.165f,.10f,.035f,.025f,purple);
            }
            void Shoes(Color c,float x=.10f){foreach(int s in new[]{-1,1})Ball("Shoe",s*x,.045f,-.04f,.14f,.09f,.22f,c);}
            void Curls(float y,Color c,float radius=.16f,int count=12)
            {for(int i=0;i<count;i++){float a=i*2*Mathf.PI/count;Ball("Hair curl",Mathf.Cos(a)*radius,y+Mathf.Sin(a)*.13f,.025f,.11f,.11f,.12f,c);}}
            void Crown(float y,Color c)
            {
                Cone("Crown band",new Vector3(0,y,0),.14f,.16f,.06f,c);
                for(int i=0;i<7;i++){float a=i*2*Mathf.PI/7;Cone("Crown point",new Vector3(Mathf.Cos(a)*.13f,y+.04f,Mathf.Sin(a)*.13f),.035f,0,.13f,c);Ball("Crown jewel",Mathf.Cos(a)*.148f,y+.035f,Mathf.Sin(a)*.148f,.04f,.04f,.04f,i%2==0?pink:blue);}
            }
            void Arms(float y,Color sleeve,Color hand)
            {
                foreach(int s in new[]{-1,1}){Rod("Sleeve",new Vector3(s*.13f,y,.015f),new Vector3(s*.25f,y-.08f,-.02f),.055f,sleeve);Ball("Hand",s*.27f,y-.09f,-.025f,.09f,.10f,.085f,hand);}
            }
            void Dress(Color c,float height=.44f)
            {Cone("Bell skirt",new Vector3(0,.09f,0),.26f,.11f,height,c);Ball("Bodice",0,.52f,0,.25f,.25f,.21f,c);for(int i=0;i<12;i++){float a=i*2*Mathf.PI/12;Ball("Scalloped hem",Mathf.Cos(a)*.23f,.12f,Mathf.Sin(a)*.23f,.10f,.08f,.10f,cream);}}
            void Basket(float x,float y,Color fruit)
            {
                Ball("Basket",x,y,-.075f,.23f,.17f,.20f,brown);
                for(int i=0;i<7;i++){float a=i*Mathf.PI/6;Ball("Basket handle",x+Mathf.Cos(a)*.10f,y+.08f+Mathf.Sin(a)*.14f,-.075f,.035f,.04f,.035f,gold);}
                for(int i=0;i<6;i++)Ball("Basket contents",x+(i%3-1)*.055f,y+.07f,-.10f+(i/3)*.07f,.065f,.065f,.06f,fruit);
            }
            public void Build(int id)
            {
                if(id==0){Shoes(green,.13f);Ball("Shaggy plum body",0,.32f,0,.48f,.48f,.38f,green);Face(.63f,.33f,green);Curls(.65f,green,.16f);foreach(int s in new[]{-1,1}){var ear=Part("Pointed ear",new Vector3(s*.21f,.67f,0),new Vector3(.16f,.10f,.06f),green);ear.transform.localRotation=Quaternion.Euler(0,0,s*30);}
                    Ball("Plumpy nose",0,.63f,-.19f,.11f,.08f,.10f,green);Arms(.45f,green,green);Basket(.28f,.22f,purple);
                    for(int i=0;i<12;i++)Cone("Fur tuft",new Vector3(Mathf.Sin(i*2.4f)*.19f,.22f+(i%3)*.12f,Mathf.Cos(i*2.4f)*.16f),.035f,0,.09f,green);}
                else if(id==1){Shoes(Color.red);foreach(int s in new[]{-1,1})for(int j=0;j<8;j++)Part("Striped stocking",new Vector3(s*.095f,.10f+j*.032f,0),new Vector3(.067f,.016f,.067f),j%2==0?cream:Color.red,PrimitiveType.Cylinder);
                    Ball("Puffed shorts",0,.37f,0,.32f,.18f,.24f,pink);Ball("Pink jacket",0,.51f,0,.28f,.25f,.22f,pink);Arms(.57f,pink,pink);Face(.73f,.27f,new Color(1,.55f,.64f));Ball("Red nose",0,.73f,-.18f,.09f,.08f,.08f,Color.red);
                    Cone("Hat",new Vector3(0,.84f,0),.19f,.055f,.23f,cream);for(int j=0;j<4;j++)Cone("Hat red stripe",new Vector3(0,.86f+j*.047f,0),.18f-j*.025f,.16f-j*.025f,.025f,Color.red);
                    Rod("Peppermint axe shaft",new Vector3(-.30f,.12f,0),new Vector3(-.30f,.95f,0),.018f,cream);for(int j=0;j<8;j++)Ball("Axe stripe",-.30f,.16f+j*.09f,0,.04f,.035f,.04f,Color.red);Ball("Axe head",-.37f,.91f,0,.23f,.17f,.055f,pink);}
                else if(id==2){Shoes(purple,.17f);Ball("Gumdrop body",0,.38f,0,.65f,.62f,.48f,pink);Face(.68f,.36f,pink);Arms(.55f,pink,pink);
                    for(int i=0;i<16;i++){float a=i*2.39996f;float y=.20f+(i%4)*.09f;float r=Mathf.Sqrt(1-Mathf.Pow((y-.38f)/.31f,2));Ball("Purple gumdrop spot",Mathf.Sin(a)*.325f*r,y,Mathf.Cos(a)*.242f*r,.085f,.085f,.045f,purple);}
                    for(int i=0;i<14;i++){float a=i*2*Mathf.PI/14;Ball("Icing ruff",Mathf.Cos(a)*.23f,.59f,Mathf.Sin(a)*.20f,.11f,.12f,.09f,cream);}}
                else if(id==3){Shoes(brown);Dress(blue);Ball("White apron",0,.31f,-.17f,.32f,.31f,.08f,cream);Arms(.57f,blue,skin);Curls(.73f,new Color(.9f,.28f,.03f));Ball("Blue bonnet",0,.82f,.045f,.43f,.34f,.15f,blue);for(int i=0;i<11;i++){float a=i*Mathf.PI/10;Ball("Bonnet frill",Mathf.Cos(a)*.21f,.74f+Mathf.Sin(a)*.23f,-.025f,.065f,.065f,.06f,cream);}Face(.73f,.27f,skin);Basket(.30f,.27f,gold);}
                else if(id==4){Shoes(gold);Dress(gold);Arms(.57f,gold,skin);Curls(.77f,purple);Face(.74f,.27f,skin);Crown(.91f,gold);for(int i=0;i<12;i++){float a=i*2.39996f;float y=.18f+(i%3)*.10f;float r=.265f-(y-.09f)/.44f*.15f;Ball("Dress candy",Mathf.Sin(a)*r,y,Mathf.Cos(a)*r,.065f,.065f,.065f,i%3==0?pink:i%3==1?blue:green);}Rod("Lollipop stick",new Vector3(-.30f,.35f,0),new Vector3(-.30f,.79f,0),.012f,cream);Ball("Lollipop",-.30f,.84f,0,.18f,.18f,.045f,pink);Ball("Lollipop center",-.30f,.84f,-.025f,.10f,.10f,.012f,gold);}
                else if(id==5){Shoes(blue);Dress(blue);Ball("Snow overskirt",0,.38f,-.06f,.44f,.28f,.32f,cream);Ball("Long ice hair",0,.62f,.10f,.38f,.65f,.18f,new Color(.62f,.88f,1));Arms(.58f,blue,skin);Face(.80f,.27f,skin);Crown(.94f,blue);Rod("Ice wand",new Vector3(-.30f,.44f,0),new Vector3(-.30f,1.00f,0),.012f,blue);for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Rod("Snowflake ray",new Vector3(-.30f,1,0),new Vector3(-.30f+Mathf.Cos(a)*.11f,1+Mathf.Sin(a)*.11f,0),.008f,cream);}}
                else if(id==6){Shoes(Color.red,.14f);Ball("Royal robe",0,.37f,0,.50f,.60f,.38f,Color.red);Cone("Royal hem",new Vector3(0,.10f,0),.25f,.21f,.12f,cream);Arms(.60f,Color.red,skin);Ball("Purple beard",0,.64f,-.045f,.32f,.30f,.27f,purple);Face(.77f,.31f,skin,true);Crown(.92f,gold);for(int i=0;i<8;i++)Ball("Royal candy button",(i%2-.5f)*.17f,.25f+(i/2)*.085f,-.19f,.065f,.065f,.035f,gold);Rod("Candy scepter",new Vector3(.31f,.12f,0),new Vector3(.31f,.98f,0),.018f,cream);Ball("Scepter candy",.31f,1,0,.15f,.15f,.10f,pink);}
                else if(id==7){Shoes(Color.black);Rod("Leg",new Vector3(-.07f,.08f,0),new Vector3(-.06f,.41f,0),.035f,Color.black);Rod("Leg",new Vector3(.07f,.08f,0),new Vector3(.06f,.41f,0),.035f,Color.black);Cone("Red lined cape",new Vector3(0,.16f,.07f),.23f,.10f,.48f,new Color(.38f,.015f,.045f));Ball("Licorice suit",0,.53f,0,.20f,.32f,.15f,Color.black);Arms(.61f,Color.black,skin);Face(.80f,.24f,skin);Ball("Black swept hair",-.045f,.94f,.015f,.31f,.17f,.22f,Color.black);foreach(int side in new[]{-1,1})Ball("Black mustache",side*.04f,.76f,-.16f,.09f,.025f,.025f,Color.black);Rod("Cane",new Vector3(.29f,.04f,-.03f),new Vector3(.29f,.58f,-.03f),.014f,brown);Ball("Cane handle",.29f,.60f,-.03f,.075f,.06f,.045f,gold);}
                else {Ball("Molasses puddle",0,.04f,0,.76f,.09f,.58f,brown);Ball("Chocolate mound",0,.27f,0,.55f,.50f,.43f,brown);Face(.53f,.35f,brown);for(int i=0;i<9;i++){float a=i*2.39996f;Ball("Chocolate fold",Mathf.Sin(a)*.24f,.10f+(i%3)*.09f,Mathf.Cos(a)*.20f,.18f,.20f,.16f,brown);}Rod("Waving arm",new Vector3(-.20f,.36f,0),new Vector3(-.34f,.67f,0),.06f,brown);Ball("Waving hand",-.35f,.70f,0,.15f,.15f,.10f,brown);}
            }
        }
    }
}
