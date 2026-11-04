using System.Collections.Generic;
using UnityEngine;
namespace LivingTable
{
 // Code-generated finishes and geometry; no external textures or service dependencies.
 public sealed class TableCabinetVisual:MonoBehaviour
 {
  readonly List<Material> owned=new List<Material>();readonly List<Mesh> meshes=new List<Mesh>();
  Material wood,gold,cream,purple;
  void Init(){if(wood!=null)return;wood=Finish(0,Color.white);gold=Finish(1,Color.white);cream=Finish(2,new Color(.83f,.73f,.51f));purple=Finish(2,new Color(.43f,.025f,.8f));}
  Material Finish(int finish,Color color){var m=new Material(Resources.Load<Shader>("TableCabinet"));m.SetFloat("_Finish",finish);m.color=color;owned.Add(m);return m;}
  Transform Part(string name,PrimitiveType type,Vector3 at,Vector3 scale,Material material){var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(transform,false);g.transform.localPosition=at;g.transform.localScale=scale;Destroy(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=material;return g.transform;}
  void Box(string name,Vector3 center,Vector3 size,float bevel,Material material)
  {
   var vertices=new List<Vector3>();var indices=new List<int>();float x=size.x*.5f,z=size.z*.5f,h=size.y*.5f;
   float[] heights={-h,-h+bevel,h-bevel,h};float[] inset={bevel,0,0,bevel};
   for(int r=0;r<4;r++){float a=x-inset[r],b=z-inset[r];vertices.AddRange(new[]{new Vector3(-a,heights[r],-b),new Vector3(-a,heights[r],b),new Vector3(a,heights[r],b),new Vector3(a,heights[r],-b)});if(r>0)for(int j=0;j<4;j++){int k=(j+1)%4,lo=(r-1)*4,hi=r*4;indices.AddRange(new[]{lo+j,hi+j,hi+k,lo+j,hi+k,lo+k});}}
   indices.AddRange(new[]{12,13,14,12,14,15,0,2,1,0,3,2});var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
   var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.localPosition=center;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
  }
  public void Board(float width,float depth,bool candy)
  {
   Init();Box(candy?"Cream candy cabinet":"Burl oak cabinet",new Vector3(0,-.019f,0),new Vector3(width+.055f,.036f,depth+.055f),.005f,candy?cream:wood);
   Box("Lower brushed gold molding",new Vector3(0,-.033f,0),new Vector3(width+.058f,.007f,depth+.058f),.002f,gold);
   for(int side=-1;side<=1;side+=2){
    Part("Brushed gold side inlay",PrimitiveType.Cube,new Vector3(side*(width*.5f+.006f),.001f,0),new Vector3(.005f,.006f,depth+.017f),gold);
    Part("Brushed gold end inlay",PrimitiveType.Cube,new Vector3(0,.001f,side*(depth*.5f+.006f)),new Vector3(width+.017f,.006f,.005f),gold);
    if(candy){var post=Part("Purple candy rail",PrimitiveType.Cylinder,new Vector3(side*(width*.5f+.021f),.002f,0),new Vector3(.014f,depth*.5f+.013f,.014f),purple);post.localRotation=Quaternion.Euler(90,0,0);for(int end=-1;end<=1;end+=2)Part("Candy finial",PrimitiveType.Sphere,new Vector3(side*(width*.5f+.021f),.002f,end*(depth*.5f+.013f)),Vector3.one*.019f,gold);}
   }
  }
  public Material WoodFinish(){Init();return wood;}
  public void Knob(){Init();Box("Polished burl oak knob",Vector3.zero,new Vector3(.052f,.028f,.052f),.006f,wood);Box("Beveled gold inlay",new Vector3(0,.014f,0),new Vector3(.037f,.003f,.037f),.001f,gold);}
  public void CaptureTray(float width,float depth){Init();Box("Polished burl oak tray",new Vector3(0,-.018f,0),new Vector3(width,.018f,depth),.004f,wood);foreach(int sign in new[]{-1,1}){Box("Tray gold rim",new Vector3(sign*(width*.5f-.003f),-.005f,0),new Vector3(.006f,.008f,depth),.002f,gold);Box("Tray wood lip",new Vector3(0,-.005f,sign*(depth*.5f-.004f)),new Vector3(width,.008f,.008f),.002f,wood);}}
  public void Pocket(Vector3 center,float diameter){Init();foreach(int sign in new[]{-1,1}){Box("Recess side",center+new Vector3(sign*(diameter*.5f+.002f),-.004f,0),new Vector3(.004f,.008f,diameter+.008f),.001f,wood);Box("Recess end",center+new Vector3(0,-.004f,sign*(diameter*.5f+.002f)),new Vector3(diameter,.008f,.004f),.001f,wood);}}
  public void Panel(float width,float height,bool candy)
  {
   Init();var backing=Part(candy?"Cream candy panel":"Burl oak panel",PrimitiveType.Cube,new Vector3(0,0,.020f),new Vector3(width+.038f,height+.035f,.025f),candy?purple:wood);
   Part("Inset panel",PrimitiveType.Cube,new Vector3(0,0,.009f),new Vector3(width,height,.007f),candy?cream:gold);
   for(int side=-1;side<=1;side+=2){Part("Gold side molding",PrimitiveType.Cube,new Vector3(side*(width*.5f+.006f),0,.007f),new Vector3(.009f,height+.022f,.010f),gold);Part("Gold edge molding",PrimitiveType.Cube,new Vector3(0,side*(height*.5f+.006f),.007f),new Vector3(width+.022f,.009f,.010f),gold);if(candy)Part("Purple sugar column",PrimitiveType.Capsule,new Vector3(side*(width*.5f+.019f),0,.004f),new Vector3(.015f,height*.5f+.012f,.015f),purple);}
   if(candy){var v=new List<Vector3>();var t=new List<int>();for(int side=-1;side<=1;side+=2){int offset=v.Count;for(int i=0;i<=32;i++){float x=(i/32f-.5f)*width;float y=side*(height*.5f+.005f+.007f*Mathf.Cos(i/32f*Mathf.PI*2));v.Add(new Vector3(x,y-.005f,.001f));v.Add(new Vector3(x,y+.005f,.001f));if(i>0){int a=offset+(i-1)*2;t.AddRange(new[]{a,a+1,a+2,a+1,a+3,a+2});}}}var mesh=new Mesh{name="Sculpted candy gold scroll"};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();meshes.Add(mesh);var go=new GameObject(mesh.name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=gold;}
  }
  void OnDestroy(){foreach(var m in owned)Destroy(m);foreach(var m in meshes)Destroy(m);}
 }
}
