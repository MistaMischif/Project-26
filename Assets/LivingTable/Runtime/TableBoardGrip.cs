using UnityEngine;
namespace LivingTable
{
 // Two opposite rim contacts move a rigid tabletop without resizing or tipping it.
 public sealed class TableBoardGrip
 {
  public float Scale {get;private set;}=1;bool corners;Vector3 frameOrigin;float minimumScale,maximumScale;
  public bool Active {get;private set;}Vector3 center,axis,origin;Quaternion rotation;
  public static bool OnSide(Vector3 local,float halfWidth,float halfDepth)=>Mathf.Abs(local.x)>=halfWidth+.012f&&Mathf.Abs(local.x)<=halfWidth+.11f&&Mathf.Abs(local.z)<=halfDepth+.025f&&local.y>=-.065f&&local.y<=.085f;
  public bool Begin(Transform board,float halfWidth,float halfDepth,Vector3 a,Vector3 b,Transform hitFrame=null){
   var frame=hitFrame==null?board:hitFrame;var la=frame.InverseTransformPoint(a);var lb=frame.InverseTransformPoint(b);var line=Vector3.ProjectOnPlane(b-a,Vector3.up);
   if(Active||!OnSide(la,halfWidth,halfDepth)||!OnSide(lb,halfWidth,halfDepth)||la.x*lb.x>=0||line.magnitude<.2f)return false;
   center=(a+b)*.5f;axis=line;origin=board.position;rotation=board.rotation;Active=true;return true;
  }
  public bool BeginCorners(Transform board,float size,Vector3 a,Vector3 b,Transform frame){
   var la=frame.InverseTransformPoint(a);var lb=frame.InverseTransformPoint(b);float c=size*.5f+.06f;
   bool Hit(Vector3 p)=>Mathf.Abs(Mathf.Abs(p.x)-c)<.045f&&Mathf.Abs(Mathf.Abs(p.z)-c)<.045f&&Mathf.Abs(p.y-.025f)<.045f;
   var line=Vector3.ProjectOnPlane(b-a,Vector3.up);if(Active||!Hit(la)||!Hit(lb)||line.magnitude<size*.6f)return false;
   center=(a+b)*.5f;axis=line;origin=board.position;frameOrigin=frame.position;minimumScale=.2f/size;maximumScale=1.2f/size;rotation=board.rotation;Scale=1;corners=true;Active=true;return true;
  }
  public bool Follow(Transform board,Vector3 a,Vector3 b){if(!Active)return false;var line=Vector3.ProjectOnPlane(b-a,Vector3.up);if(line.magnitude<.15f)return false;var turn=Quaternion.AngleAxis(Vector3.SignedAngle(axis,line,Vector3.up),Vector3.up);Scale=corners?Mathf.Clamp(line.magnitude/axis.magnitude,minimumScale,maximumScale):1;board.SetPositionAndRotation(corners?(a+b)*.5f+turn*(frameOrigin-center)*Scale-turn*(frameOrigin-origin):(a+b)*.5f+turn*(origin-center),turn*rotation);return true;}
  public void End(){Active=false;corners=false;Scale=1;}
 }
}
