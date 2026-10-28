using UnityEngine;
namespace LivingTable
{
 public sealed class TableFacingPanel : MonoBehaviour
 {
  public bool Floating { get; private set; }
  public void SetFloating(bool value){Floating=value;}
  public void Pin(Vector3 point){Floating=false;transform.position=point;}
  public void FaceViewer()
  {
   var camera=Camera.main;if(camera==null)return;
   if(Floating){var target=camera.transform.position+camera.transform.forward*.9f+camera.transform.up*.18f;transform.position=Vector3.Lerp(transform.position,target,1-Mathf.Exp(-Time.unscaledDeltaTime*4));}
   var direction=transform.position-camera.transform.position;if(direction.sqrMagnitude>.0001f)transform.rotation=Quaternion.LookRotation(direction,Vector3.up);
  }
  void LateUpdate(){FaceViewer();}
 }
}
