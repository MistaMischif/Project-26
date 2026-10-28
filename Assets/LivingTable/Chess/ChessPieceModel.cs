using System.Collections.Generic;
using UnityEngine;
namespace LivingTable.Chess
{
 public static class ChessPieceModel
 {
  public static Transform Build(Transform parent,int piece,Material material,Material detail,List<Mesh> owned)
  {
   int kind=Mathf.Abs(piece);var root=new GameObject(new[]{"","Pawn","Knight","Bishop","Rook","Queen","King"}[kind]).transform;root.SetParent(parent,false);
   var profile=new List<Vector2>{new Vector2(.018f,0),new Vector2(.019f,.004f),new Vector2(.017f,.008f),new Vector2(.013f,.010f),new Vector2(.009f,.016f),new Vector2(.006f,.028f),new Vector2(.009f,.032f)};
   if(kind==1)profile.Add(new Vector2(.007f,.035f));else{profile.Add(new Vector2(.007f,.037f));profile.Add(new Vector2(.011f,.041f));}
   const int segments=24;var vertices=new Vector3[profile.Count*segments+2];int bottom=vertices.Length-2,top=vertices.Length-1;vertices[top]=Vector3.up*profile[profile.Count-1].y;var indices=new List<int>();
   for(int y=0;y<profile.Count;y++)for(int i=0;i<segments;i++){float angle=i*Mathf.PI*2/segments;vertices[y*segments+i]=new Vector3(Mathf.Cos(angle)*profile[y].x,profile[y].y,Mathf.Sin(angle)*profile[y].x);int next=(i+1)%segments;if(y<profile.Count-1){int a=y*segments+i,b=y*segments+next,c=a+segments,d=b+segments;indices.AddRange(new[]{a,c,b,b,c,d});}if(y==0)indices.AddRange(new[]{bottom,i,next});if(y==profile.Count-1)indices.AddRange(new[]{top,y*segments+next,y*segments+i});}
   var mesh=new Mesh{name="Lathed chess body"};mesh.vertices=vertices;mesh.triangles=indices.ToArray();mesh.RecalculateNormals();owned.Add(mesh);root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
   void Part(PrimitiveType shape,Vector3 p,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(shape);Object.Destroy(g.GetComponent<Collider>());g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;}
   if(kind==1)Part(PrimitiveType.Sphere,Vector3.up*.041f,Vector3.one*.018f,material);
   if(kind==2){Part(PrimitiveType.Cube,new Vector3(0,.046f,0),new Vector3(.014f,.024f,.014f),material);Part(PrimitiveType.Sphere,new Vector3(0,.057f,.006f),new Vector3(.017f,.020f,.021f),material);Part(PrimitiveType.Cube,new Vector3(0,.055f,.014f),new Vector3(.014f,.010f,.016f),material);for(int s=-1;s<=1;s+=2){Part(PrimitiveType.Cube,new Vector3(s*.005f,.069f,-.001f),new Vector3(.004f,.012f,.006f),material);Part(PrimitiveType.Sphere,new Vector3(s*.008f,.060f,.010f),Vector3.one*.003f,detail);}}
   if(kind==3){Part(PrimitiveType.Sphere,Vector3.up*.052f,new Vector3(.018f,.027f,.018f),material);Part(PrimitiveType.Cube,new Vector3(0,.057f,.0085f),new Vector3(.002f,.014f,.0015f),detail);Part(PrimitiveType.Sphere,Vector3.up*.068f,Vector3.one*.005f,material);}
   if(kind==4){Part(PrimitiveType.Cylinder,Vector3.up*.045f,new Vector3(.025f,.006f,.025f),material);for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Part(PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.009f,.054f,Mathf.Sin(a)*.009f),new Vector3(.007f,.010f,.007f),material);}}
   if(kind==5){Part(PrimitiveType.Sphere,Vector3.up*.051f,new Vector3(.021f,.014f,.021f),material);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Part(PrimitiveType.Sphere,new Vector3(Mathf.Cos(a)*.009f,.060f,Mathf.Sin(a)*.009f),Vector3.one*.006f,material);}Part(PrimitiveType.Sphere,Vector3.up*.065f,Vector3.one*.007f,material);}
   if(kind==6){Part(PrimitiveType.Sphere,Vector3.up*.052f,new Vector3(.021f,.017f,.021f),material);Part(PrimitiveType.Cube,Vector3.up*.071f,new Vector3(.006f,.022f,.006f),material);Part(PrimitiveType.Cube,Vector3.up*.075f,new Vector3(.018f,.006f,.006f),material);}
   root.localRotation=Quaternion.Euler(0,piece<0?180:0,0);return root;
  }
 }
}
