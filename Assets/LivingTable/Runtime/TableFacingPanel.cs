using UnityEngine;
namespace LivingTable
{
 public sealed class TableFacingPanel : MonoBehaviour
 {
  Transform board;float boardSize,panelHeight;public bool InstructionView{get;private set;}
  public bool FixedToBoard;
  bool compactInstructions;public float InstructionDistance=.60f;
  public void SetInstructionView(bool value,bool compact=false){InstructionView=value;compactInstructions=compact;FaceViewer();}
  public void SetBoardBackground(Transform reference,float size,float height){board=reference;boardSize=size;panelHeight=height;}
  public bool Floating { get; private set; }
  public void SetFloating(bool value){Floating=value;}
  public void Pin(Vector3 point){Floating=false;transform.position=point;}
  public void FaceViewer()
  {
   var camera=Camera.main;if(camera==null)return;
   if(FixedToBoard&&board!=null){transform.position=board.position+board.forward*(boardSize*.75f+.18f)+board.up*.28f;transform.rotation=Quaternion.LookRotation(board.forward,board.up);return;}
   if(InstructionView){transform.position=camera.transform.position+camera.transform.forward*InstructionDistance+camera.transform.up*(compactInstructions?.13f:.12f)*(InstructionDistance/.60f);}
   else if(board!=null){var away=Vector3.ProjectOnPlane(board.position-camera.transform.position,Vector3.up).normalized;if(away.sqrMagnitude<.01f)away=Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;float extent=panelHeight*transform.lossyScale.y*.5f;transform.position=board.gameObject.activeInHierarchy?board.position+away*(boardSize*.72f+.14f+extent)+Vector3.up*(.17f+extent):camera.transform.position+away*.65f;}
   else if(Floating){var target=camera.transform.position+camera.transform.forward*.9f+camera.transform.up*.18f;transform.position=Vector3.Lerp(transform.position,target,1-Mathf.Exp(-Time.unscaledDeltaTime*4));}
   var direction=transform.position-camera.transform.position;if(direction.sqrMagnitude>.0001f)transform.rotation=Quaternion.LookRotation(direction,InstructionView?camera.transform.up:Vector3.up);
  }
  void LateUpdate(){FaceViewer();}
 }
}
