using UnityEngine;
namespace LivingTable
{
 public sealed class TableButtonVisual : MonoBehaviour
 {
  Material material,metal;Mesh bevelMesh;Transform cap;float hoverUntil,pressUntil;bool active,circle;Color tint;
  public bool Hovered=>Time.unscaledTime<hoverUntil;
  public bool Pressed=>Time.unscaledTime<pressUntil;
  public void Build(Transform label,Color color,bool selected,bool circular=false,float diameter=.15f,bool sparkle=false,bool square=false)
  {
   active=selected;tint=color;circle=circular;
   metal=new Material(Resources.Load<Shader>("CandyPiecePlastic"));metal.color=new Color(.70f,.74f,.79f);
   cap=new GameObject("Moving button cap").transform;cap.SetParent(transform,false);
   var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name="Gloss face";go.transform.SetParent(cap,false);Destroy(go.GetComponent<Collider>());go.transform.localPosition=Vector3.up*.004f;go.transform.localRotation=Quaternion.Euler(90,0,0);go.transform.localScale=(circular||square)?new Vector3(diameter,diameter,1):new Vector3(.145f,.038f,1);
   material=new Material(Resources.Load<Shader>("GlossButton"));go.GetComponent<Renderer>().sharedMaterial=material;material.SetFloat("_Circle",circular?1:0);material.SetFloat("_SquareButton",square?1:0);material.SetFloat("_Sparkle",sparkle?1:0);
   {
    const int count=64;float r=diameter*.5f;float[] radii={r*.74f,r*.84f,r*.96f,r,r};float[] heights={.004f,.006f,.004f,-.001f,-.009f};var vertices=new Vector3[count*radii.Length];var tris=new int[count*(radii.Length-1)*6];int t=0;
    for(int ring=0;ring<radii.Length;ring++)for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;float factor=radii[ring]/r;float ca=Mathf.Cos(a),sa=Mathf.Sin(a);float x=circular?ca*radii[ring]:(Mathf.Sign(ca)*(square?r-.009f:.0635f)+ca*.009f)*factor;float z=circular?sa*radii[ring]:(Mathf.Sign(sa)*(square?r-.009f:.010f)+sa*.009f)*factor;vertices[ring*count+i]=new Vector3(x,heights[ring],z);if(ring<radii.Length-1){int a0=ring*count+i,b0=ring*count+(i+1)%count,c0=(ring+1)*count+i,d0=(ring+1)*count+(i+1)%count;tris[t++]=a0;tris[t++]=b0;tris[t++]=c0;tris[t++]=b0;tris[t++]=d0;tris[t++]=c0;}}
    bevelMesh=new Mesh{name="Machined silver bevel"};bevelMesh.vertices=vertices;bevelMesh.triangles=tris;bevelMesh.RecalculateNormals();bevelMesh.RecalculateBounds();var rim=new GameObject("Physical silver bevel");rim.transform.SetParent(transform,false);rim.AddComponent<MeshFilter>().sharedMesh=bevelMesh;rim.AddComponent<MeshRenderer>().sharedMaterial=metal;
    var side=GameObject.CreatePrimitive(circular?PrimitiveType.Cylinder:PrimitiveType.Cube);side.name="Solid button body";side.transform.SetParent(transform,false);side.transform.localPosition=Vector3.down*.007f;side.transform.localScale=circular?new Vector3(diameter,.004f,diameter):new Vector3(square?diameter:.145f,.008f,square?diameter:.038f);Destroy(side.GetComponent<Collider>());side.GetComponent<Renderer>().sharedMaterial=metal;
   }
   label.SetParent(cap,true);label.localPosition=new Vector3(0,.006f,0);Refresh();
  }
  public void Hover(){hoverUntil=Time.unscaledTime+.10f;}
  public void UseGoldTrim(bool goldFace=false){material.SetFloat("_GoldTrim",1);material.SetFloat("_GoldFace",goldFace?1:0);metal.shader=Resources.Load<Shader>("TableCabinet");metal.SetFloat("_Finish",1);metal.color=Color.white;}
  public void Press(){pressUntil=Time.unscaledTime+.16f;Refresh();}
  void Update(){Refresh();}
  void Refresh(){if(material==null)return;material.SetColor("_Tint",tint*(active?1.15f:1));material.SetFloat("_Hover",Hovered?1:0);material.SetFloat("_Pressed",Pressed?1:0);cap.localPosition=Vector3.up*(Pressed?-.004f:0);}
  void OnDestroy(){if(material!=null)Destroy(material);if(metal!=null)Destroy(metal);if(bevelMesh!=null)Destroy(bevelMesh);}
 }
}
