using System;
using System.Collections.Generic;
using UnityEngine;

namespace LivingTable.CandyLand
{
    // One molded silhouette for every player. Dimensions are metres; front faces -Z.
    // Hand-modeled from the user's front/back original-piece photographs.
    public sealed class CandyPlayerPiece : MonoBehaviour
    {
        Mesh body;GameObject sludge;Material chocolate;
        public bool Sludged=>sludge!=null&&sludge.activeSelf;
        public void SetSludged(bool value)
        {
            if(!value){if(sludge!=null)sludge.SetActive(false);return;}
            if(sludge==null)
            {
                chocolate=new Material(Resources.Load<Shader>("CandyPiecePlastic"));chocolate.color=new Color(.30f,.105f,.038f);
                sludge=new GameObject("Gloppy chocolate sludge");sludge.transform.SetParent(transform.parent,false);
                var shell=new GameObject("Chocolate coating");shell.transform.SetParent(sludge.transform,false);shell.AddComponent<MeshFilter>().sharedMesh=body;shell.AddComponent<MeshRenderer>().sharedMaterial=chocolate;shell.transform.localScale=new Vector3(1.05f,1.025f,1.85f);
                for(int i=0;i<9;i++)
                {
                    var drop=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(drop.GetComponent<Collider>());drop.name="Sticky chocolate drip";drop.transform.SetParent(sludge.transform,false);
                    float x=new[]{-.008f,-.003f,.005f,.009f,-.018f,.018f,-.006f,.004f,0}[i],y=new[]{.041f,.039f,.040f,.039f,.028f,.028f,.022f,.019f,.014f}[i];drop.transform.localPosition=new Vector3(x,y,-.0045f);drop.transform.localScale=new Vector3(.0035f+(i%3)*.0004f,.007f+(i%4)*.003f,.0035f);drop.GetComponent<Renderer>().sharedMaterial=chocolate;
                }
                var puddle=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(puddle.GetComponent<Collider>());puddle.name="Molasses puddle";puddle.transform.SetParent(sludge.transform,false);puddle.transform.localPosition=new Vector3(0,.0015f,0);puddle.transform.localScale=new Vector3(.033f,.003f,.024f);puddle.GetComponent<Renderer>().sharedMaterial=chocolate;
            }
            sludge.transform.localPosition=transform.localPosition;sludge.transform.localRotation=transform.localRotation;sludge.transform.localScale=transform.localScale;sludge.SetActive(true);
        }
        public void Build(Material plastic, Material icing)
        {
            icing.SetFloat("_Cull",0);
            var outline=new List<Vector2>{
                new Vector2(-.21f,.08f),new Vector2(-.36f,.10f),new Vector2(-.40f,.14f),new Vector2(-.39f,.17f),
                new Vector2(-.29f,.18f),new Vector2(-.23f,.21f),new Vector2(-.21f,.27f),new Vector2(-.20f,.39f),
                new Vector2(-.23f,.44f),new Vector2(-.40f,.48f),new Vector2(-.49f,.52f),new Vector2(-.52f,.57f),
                new Vector2(-.51f,.63f),new Vector2(-.47f,.66f),new Vector2(-.41f,.67f),new Vector2(-.38f,.69f),
                new Vector2(-.33f,.68f),new Vector2(-.25f,.65f),new Vector2(-.19f,.65f),new Vector2(-.18f,.69f),
                new Vector2(-.22f,.73f),new Vector2(-.26f,.75f),new Vector2(-.28f,.79f),new Vector2(-.27f,.83f),
                new Vector2(-.24f,.86f),new Vector2(-.23f,.91f),new Vector2(-.19f,.96f),new Vector2(-.12f,.99f),
                new Vector2(0,1.01f)};
            int half=outline.Count;
            for(int i=half-2;i>=0;i--)outline.Add(new Vector2(-outline[i].x,outline[i].y));
            outline.Add(new Vector2(.07f,.085f));outline.Add(new Vector2(0,.11f));outline.Add(new Vector2(-.07f,.085f));
            body=Extrude(outline,.005f);
            var shape=new GameObject("Original molded character");shape.transform.SetParent(transform,false);
            shape.AddComponent<MeshFilter>().sharedMesh=body;shape.AddComponent<MeshRenderer>().sharedMaterial=plastic;
            var foot=GameObject.CreatePrimitive(PrimitiveType.Cylinder);foot.name="Oval plastic pedestal";foot.transform.SetParent(transform,false);
            foot.transform.localPosition=new Vector3(0,.002f,0);foot.transform.localScale=new Vector3(.027f,.002f,.017f);
            Destroy(foot.GetComponent<Collider>());foot.GetComponent<Renderer>().sharedMaterial=plastic;
            // Raised white printing follows the front only; the reverse is molded green/color relief.
            Stroke("Face rim",icing,-.0028f,.0008f, new[]{P(-.18f,.68f),P(-.18f,.73f),P(-.24f,.76f),P(-.25f,.81f),P(-.22f,.85f),P(-.21f,.90f),P(-.17f,.95f),P(-.10f,.975f),P(0,.985f),P(.10f,.975f),P(.17f,.95f),P(.21f,.90f),P(.22f,.85f),P(.25f,.81f),P(.24f,.76f),P(.18f,.73f),P(.18f,.68f)});
            Stroke("Swept hair",icing,-.0029f,.00085f,new[]{P(-.22f,.85f),P(-.16f,.87f),P(-.13f,.95f),P(-.04f,.91f),P(.07f,.89f),P(.09f,.90f),P(.08f,.93f),P(.20f,.87f)});
            Circle("Left eye",icing,-.0029f,-.06f,.845f,.018f,true);
            Circle("Right eye",icing,-.0029f,.065f,.845f,.018f,true);
            Stroke("Nose",icing,-.003f,.0008f,new[]{P(-.02f,.805f),P(0,.81f),P(.02f,.805f)});
            Stroke("Smile",icing,-.003f,.0009f,new[]{P(-.10f,.79f),P(-.06f,.77f),P(0,.755f),P(.06f,.77f),P(.10f,.79f)});
            Stroke("Jacket",icing,-.0029f,.0008f,new[]{P(-.19f,.67f),P(-.18f,.48f),P(.18f,.48f),P(.19f,.67f)});
            Stroke("Bow left",icing,-.003f,.0008f,new[]{P(-.04f,.67f),P(-.12f,.70f),P(-.115f,.63f),P(-.04f,.65f)});
            Stroke("Bow right",icing,-.003f,.0008f,new[]{P(.04f,.67f),P(.12f,.70f),P(.115f,.63f),P(.04f,.65f)});
            Circle("Bow knot",icing,-.003f,0,.665f,.036f,false);
            Circle("Top button",icing,-.003f,0,.585f,.025f,false);
            Circle("Lower button",icing,-.003f,0,.525f,.025f,false);
            Stroke("Shorts",icing,-.0029f,.0008f,new[]{P(-.20f,.46f),P(-.20f,.31f),P(-.05f,.30f),P(0,.345f),P(.05f,.30f),P(.20f,.31f),P(.20f,.46f)});
            Stroke("Jacket tails",icing,-.003f,.0008f,new[]{P(-.19f,.46f),P(-.09f,.40f),P(0,.45f),P(.09f,.40f),P(.19f,.46f)});
            for(int side=-1;side<=1;side+=2)
            {
                Stroke("Mitten",icing,-.0029f,.0008f,new[]{P(side*.21f,.65f),P(side*.31f,.68f),P(side*.38f,.67f),P(side*.40f,.645f),P(side*.46f,.635f),P(side*.49f,.60f),P(side*.48f,.55f),P(side*.43f,.52f),P(side*.35f,.49f),P(side*.21f,.46f)});
                Stroke("Cuff",icing,-.003f,.0008f,new[]{P(side*.32f,.65f),P(side*.34f,.50f)});
                Stroke("Sleeve zigzag",icing,-.003f,.0008f,new[]{P(side*.25f,.64f),P(side*.28f,.60f),P(side*.25f,.58f),P(side*.29f,.54f),P(side*.25f,.52f)});
            }
            Stroke("Back jacket relief",plastic,.0029f,.0007f,new[]{P(-.18f,.68f),P(-.18f,.48f),P(.18f,.48f),P(.18f,.68f),P(-.18f,.68f)});
            Stroke("Back shorts relief",plastic,.0029f,.0007f,new[]{P(-.18f,.46f),P(-.20f,.31f),P(-.05f,.30f),P(0,.345f),P(.05f,.30f),P(.20f,.31f),P(.18f,.46f)});
        }
        static Vector2 P(float x,float y)=>new Vector2(x*.045f,.001f+y*.049f);
        void Stroke(string name,Material material,float z,float width,Vector2[] points)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();
            line.useWorldSpace=false;line.alignment=LineAlignment.View;line.sharedMaterial=material;
            line.widthMultiplier=width;line.numCapVertices=4;line.numCornerVertices=4;line.positionCount=points.Length;
            for(int i=0;i<points.Length;i++)line.SetPosition(i,new Vector3(points[i].x,points[i].y,z));
        }
        void Circle(string name,Material material,float z,float x,float y,float radius,bool filled)
        {
            var points=new Vector2[25];for(int i=0;i<points.Length;i++){float a=i*Mathf.PI*2/24;points[i]=P(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius);}
            Stroke(name,material,z,filled?radius*.07f:.0007f,points);
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static Mesh Extrude(List<Vector2> outline,float depth)
        {
            for(int i=0;i<outline.Count;i++)outline[i]=P(outline[i].x,outline[i].y);
            float area=0;for(int i=0;i<outline.Count;i++)area+=Cross(outline[i],outline[(i+1)%outline.Count]);
            if(area<0)outline.Reverse();
            var vertices=new List<Vector3>();var triangles=new List<int>();
            int n=outline.Count;
            foreach(var p in outline)vertices.Add(new Vector3(p.x,p.y,-depth/2));
            foreach(var p in outline)vertices.Add(new Vector3(p.x,p.y,depth/2));
            var remaining=new List<int>();for(int i=0;i<n;i++)remaining.Add(i);
            int guard=n*n;
            while(remaining.Count>2&&guard-->0)
            {
                bool clipped=false;
                for(int k=0;k<remaining.Count;k++)
                {
                    int a=remaining[(k+remaining.Count-1)%remaining.Count],b=remaining[k],c=remaining[(k+1)%remaining.Count];
                    if(Cross(outline[b]-outline[a],outline[c]-outline[b])<=0)continue;
                    bool inside=false;
                    foreach(int p in remaining)
                    {
                        if(p==a||p==b||p==c)continue;
                        if(Cross(outline[b]-outline[a],outline[p]-outline[a])>=0 && Cross(outline[c]-outline[b],outline[p]-outline[b])>=0 && Cross(outline[a]-outline[c],outline[p]-outline[c])>=0){inside=true;break;}
                    }
                    if(inside)continue;
                    triangles.AddRange(new[]{a,c,b,a+n,b+n,c+n});remaining.RemoveAt(k);clipped=true;break;
                }
                if(!clipped)throw new InvalidOperationException("Player silhouette triangulation failed");
            }
            for(int i=0;i<n;i++)
            {
                int j=(i+1)%n,start=vertices.Count;
                vertices.AddRange(new[]{vertices[i],vertices[j],vertices[j+n],vertices[i+n]});
                triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
            }
            var mesh=new Mesh{name="Original Candy Land molded pawn"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void LateUpdate(){if(Sludged){sludge.transform.localPosition=transform.localPosition;sludge.transform.localRotation=transform.localRotation;}}
        void OnDestroy(){if(sludge!=null)Destroy(sludge);if(chocolate!=null)Destroy(chocolate);if(body!=null)Destroy(body);}
    }
}
