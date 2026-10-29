using System;
using System.Collections.Generic;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // One colored mesh per item, shared by the whole orbit. No physics or character models.
    public sealed class CandyRewardAssets : IDisposable
    {
        public static readonly string[] Items={"Purple berry","Striped candy cane","Sugared gumdrop","Golden peanut","Swirl lollipop","Ice snowflake"};
        public static readonly string[] Scenery={"Berry sprout","Peppermint shoots","Gumdrop mound","Peanut brittle cluster","Lollipop grove","Crystal garden"};
        readonly Mesh[] items=new Mesh[3];readonly Mesh scenery;readonly Material material;
        public CandyRewardAssets(int theme)
        {
            material=new Material(Resources.Load<Shader>("CandyRewardItem"));material.enableInstancing=true;
            for(int i=0;i<3;i++){var b=new Builder();b.Item(theme,i,Vector3.zero,1);items[i]=b.Finish(Items[theme]);}
            var garden=new Builder();garden.Garden(theme);scenery=garden.Finish(Scenery[theme]);
        }
        public Transform Create(Transform parent,int theme,int variation,bool environment=false)
        {
            var go=new GameObject(environment?Scenery[theme]:Items[theme]);go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=environment?scenery:items[variation%3];go.AddComponent<MeshRenderer>().sharedMaterial=material;return go.transform;
        }
        public void Dispose(){foreach(var mesh in items)if(mesh!=null)UnityEngine.Object.Destroy(mesh);if(scenery!=null)UnityEngine.Object.Destroy(scenery);if(material!=null)UnityEngine.Object.Destroy(material);}
        sealed class Builder
        {
            readonly List<Vector3> vertices=new List<Vector3>();readonly List<Color> colors=new List<Color>();readonly List<int> triangles=new List<int>();
            static readonly Color Cream=new Color(1,.96f,.8f),Purple=new Color(.5f,.04f,.72f),Green=new Color(.15f,.6f,.07f),Brown=new Color(.39f,.19f,.04f),Red=new Color(.94f,.025f,.07f),Ice=new Color(.2f,.8f,1);
            static readonly Color[] Sweets={new Color(1,.12f,.35f),new Color(.3f,.94f,.1f),new Color(1,.65f,.03f)};
            int Vertex(Vector3 p,Color c){vertices.Add(p);colors.Add(c);return vertices.Count-1;}
            void Tri(Vector3 a,Vector3 b,Vector3 c,Color color){int start=Vertex(a,color);Vertex(b,color);Vertex(c,color);triangles.Add(start);triangles.Add(start+1);triangles.Add(start+2);}
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){Tri(a,b,c,color);Tri(a,c,d,color);}
            void Ball(Vector3 p,Vector3 scale,Color color,int rings=6,int sides=10)
            {
                int start=vertices.Count;
                for(int j=0;j<=rings;j++)for(int i=0;i<=sides;i++){float latitude=j*Mathf.PI/rings,angle=i*Mathf.PI*2/sides;Vertex(p+Vector3.Scale(new Vector3(Mathf.Sin(latitude)*Mathf.Cos(angle),Mathf.Cos(latitude),Mathf.Sin(latitude)*Mathf.Sin(angle)),scale),color);}
                for(int j=0;j<rings;j++)for(int i=0;i<sides;i++){int a=start+j*(sides+1)+i,b=a+sides+1;triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
            }
            void Rod(Vector3 a,Vector3 b,float radius,Color color,int sides=7)
            {
                var rotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
                for(int i=0;i<sides;i++){float u=i*2*Mathf.PI/sides,v=(i+1)*2*Mathf.PI/sides;var d=rotation*new Vector3(Mathf.Cos(u)*radius,0,Mathf.Sin(u)*radius);var e=rotation*new Vector3(Mathf.Cos(v)*radius,0,Mathf.Sin(v)*radius);Quad(a+d,b+d,b+e,a+e,color);Tri(a,a+e,a+d,color);Tri(b,b+d,b+e,color);}
            }
            void Crystal(Vector3 p,float radius,float height,Color color)
            {
                for(int i=0;i<6;i++){float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;var v=p+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);var w=p+new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius);Tri(w,v,p+Vector3.up*height,Color.Lerp(color,Cream,(i%3)*.18f));}
            }
            public void Item(int theme,int variation,Vector3 p,float s)
            {
                switch(theme)
                {
                    case 0:
                        Ball(p,new Vector3(.42f,.46f,.40f)*s,Color.Lerp(Purple,new Color(.8f,.16f,.65f),variation*.12f));Rod(p+Vector3.up*.4f*s,p+new Vector3(.08f,.65f,0)*s,.035f*s,Brown);Ball(p+new Vector3(.23f,.52f,0)*s,new Vector3(.22f,.06f,.12f)*s,Green);break;
                    case 1:
                        var points=new List<Vector3>();for(int j=0;j<=7;j++)points.Add(new Vector3(0,-.55f+j*.105f,0));for(int j=1;j<=10;j++){float a=Mathf.PI-j*Mathf.PI/10;points.Add(new Vector3(.23f+Mathf.Cos(a)*.23f,.185f+Mathf.Sin(a)*.23f,0));}points.Add(new Vector3(.46f,.05f,0));
                        for(int j=1;j<points.Count;j++)Rod(p+points[j-1]*s,p+points[j]*s,.085f*s,j%2==0?Red:Cream);break;
                    case 2:
                        // Flat-bottomed rounded sugar dome.
                        float[] heights={-.4f,-.22f,.05f,.30f,.43f},radii={.43f,.46f,.42f,.30f,0f};
                        for(int j=0;j<4;j++)for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;Quad(p+new Vector3(Mathf.Cos(a)*radii[j],heights[j],Mathf.Sin(a)*radii[j])*s,p+new Vector3(Mathf.Cos(a)*radii[j+1],heights[j+1],Mathf.Sin(a)*radii[j+1])*s,p+new Vector3(Mathf.Cos(b)*radii[j+1],heights[j+1],Mathf.Sin(b)*radii[j+1])*s,p+new Vector3(Mathf.Cos(b)*radii[j],heights[j],Mathf.Sin(b)*radii[j])*s,Sweets[variation%3]);}
                        for(int i=0;i<12;i++){float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;Tri(p+Vector3.down*.4f*s,p+new Vector3(Mathf.Cos(a)*.43f,-.4f,Mathf.Sin(a)*.43f)*s,p+new Vector3(Mathf.Cos(b)*.43f,-.4f,Mathf.Sin(b)*.43f)*s,Sweets[variation%3]);}
                        for(int i=0;i<10;i++){float a=i*2.39996f;Crystal(p+new Vector3(Mathf.Cos(a)*.36f,-.1f+(i%3)*.13f,Mathf.Sin(a)*.36f)*s,.035f*s,.07f*s,Cream);}break;
                    case 3:
                        var shell=new Color(.91f,.59f,.22f);for(int j=-1;j<=1;j+=2)Ball(p+Vector3.up*j*.21f*s,new Vector3(.28f,.35f,.25f)*s,shell);
                        for(int j=-2;j<=2;j++)Rod(p+new Vector3(-.20f,j*.13f,-.235f)*s,p+new Vector3(.20f,j*.13f,-.235f)*s,.018f*s,Brown);break;
                    case 4:
                        Rod(p+Vector3.down*.76f*s,p+Vector3.up*.17f*s,.045f*s,Cream);Ball(p+Vector3.up*.2f*s,new Vector3(.40f,.40f,.105f)*s,Sweets[variation%3]);
                        for(int j=1;j<=30;j++){float t=(j-1)/30f,u=j/30f;var a=new Vector3(Mathf.Cos(t*Mathf.PI*4)*t*.34f,.2f+Mathf.Sin(t*Mathf.PI*4)*t*.34f,-.11f);var b=new Vector3(Mathf.Cos(u*Mathf.PI*4)*u*.34f,.2f+Mathf.Sin(u*Mathf.PI*4)*u*.34f,-.11f);Rod(p+a*s,p+b*s,.026f*s,Cream,5);}break;
                    case 5:
                        for(int j=0;j<6;j++){float a=j*Mathf.PI/3;var direction=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);Rod(p,p+direction*.55f*s,.035f*s,Ice);for(int sign=-1;sign<=1;sign+=2){var branch=Quaternion.Euler(0,0,sign*55)*direction;Rod(p+direction*.34f*s,p+(direction*.34f-branch*.16f)*s,.025f*s,Cream);}}Ball(p,Vector3.one*.09f*s,Ice,4,6);break;
                }
            }
            public void Garden(int theme)
            {
                if(theme==0){Rod(Vector3.zero,Vector3.up*.9f,.055f,Brown);for(int i=0;i<4;i++){float a=i*2.4f;var p=new Vector3(Mathf.Cos(a)*.35f,.45f+i*.13f,Mathf.Sin(a)*.3f);Rod(Vector3.up*.4f,p,.025f,Brown);Ball(p,new Vector3(.32f,.14f,.22f),Green);Item(0,i%3,p+Vector3.down*.08f,.44f);}return;}
                if(theme==3){for(int i=0;i<4;i++){var p=new Vector3((i%2-.5f)*.4f,0,(i/2-.5f)*.4f);Crystal(p,.3f,.35f+(i%2)*.3f,new Color(1,.65f,.1f));Item(3,0,p+Vector3.up*.35f,.48f);}return;}
                if(theme==5){for(int i=0;i<5;i++){float a=i*2.4f;Crystal(new Vector3(Mathf.Cos(a)*.28f,0,Mathf.Sin(a)*.28f),.18f,.55f+i*.13f,Ice);}return;}
                for(int i=0;i<3;i++)Item(theme,i,new Vector3((i-1)*.35f,theme==4?.65f:theme==1?.5f:.3f,(i%2)*.2f),.60f+(i%2)*.18f);
            }
            public Mesh Finish(string name){var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
        }
    }
}
