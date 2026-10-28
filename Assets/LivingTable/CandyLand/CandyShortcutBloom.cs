using System;
using System.Collections;
using UnityEngine;
namespace LivingTable.CandyLand {
 public sealed class CandyShortcutBloom:MonoBehaviour {
  Transform root; Material material;
  public IEnumerator Show(Vector3 from,Vector3 to,Func<bool> paused){
   root=new GameObject("Gumdrop Pass bloom").transform;root.SetParent(transform,false);
   material=new Material(Resources.Load<Shader>("CandyPiecePlastic"));material.color=new Color(.5f,1,.18f);
   var drops=new Transform[14];var stars=new Transform[28];
   var side=Vector3.Cross((to-from).normalized,Vector3.up)*.012f;
   for(int i=0;i<14;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(g.GetComponent<Collider>());g.transform.SetParent(root,false);g.transform.localPosition=Vector3.Lerp(from,to,(i/2+1)/8f)+side*(i%2==0?1:-1);g.GetComponent<Renderer>().sharedMaterial=material;g.transform.localScale=Vector3.zero;drops[i]=g.transform;}
   for(int i=0;i<28;i++){var g=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(g.GetComponent<Collider>());g.transform.SetParent(root,false);g.GetComponent<Renderer>().sharedMaterial=material;g.transform.localScale=Vector3.zero;stars[i]=g.transform;}
   float age=0;while(age<1.5f){if(!paused())age+=Time.deltaTime;
    for(int i=0;i<14;i++){float t=Mathf.Clamp01((age-i/2*.11f)/.45f);float size=t==0?0:1+Mathf.Sin(t*Mathf.PI*3)*(1-t)*.4f;drops[i].localScale=new Vector3(.012f,.016f,.012f)*size;}
    for(int i=0;i<28;i++){float t=Mathf.Repeat(age*1.7f+i*.137f,1);stars[i].localPosition=Vector3.Lerp(from,to,i/27f)+side*Mathf.Sin(i*2.4f)+Vector3.up*(.006f+t*.035f);stars[i].localRotation=Quaternion.Euler(65,age*120+i*20,45);stars[i].localScale=Vector3.one*(Mathf.Sin(t*Mathf.PI)*.004f);}
    yield return null;
   }Destroy(root.gameObject);Destroy(material);Destroy(this);
  }
  void OnDestroy(){if(root)Destroy(root.gameObject);if(material)Destroy(material);}
 }
}
