using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  public const string AlignmentInstructions="ALIGN YOUR BOARD\nLook down at the whole playing area.\nKeep the board still and hands clear.\nIf asked, move your head sideways.\nCheck grid edges, then ACCEPT BOARD.";
  string alignmentError="",alignmentProgress="Finding checker-square edges...";float lastAlignmentIssue;
  TextMesh alignmentErrorLabel,alignmentProgressLabel;
  bool AlignmentMessageView=>!HomeScreen&&!standardMenu&&!VirtualSafetyOpen&&Physical&&(alignStage==3||alignStage==5);
  void ResetAlignmentMessages(){alignmentError="";alignmentProgress="Finding checker-square edges...";}
  public void CalibrationIssue(string text){
   // Keep an issue readable rather than flashing between successive camera samples.
   if(alignmentError==text){lastAlignmentIssue=Time.unscaledTime;return;}
   if(alignmentError.Length==0||Time.unscaledTime-lastAlignmentIssue>=3){alignmentError=text;lastAlignmentIssue=Time.unscaledTime;}
  }
  void DrawAlignmentMessages(Transform parent){
   alignmentErrorLabel=alignmentProgressLabel=null;if(!AlignmentMessageView)return;
   status.transform.localPosition=new Vector3(0,.065f,-.015f);
   alignmentProgressLabel=Label(parent,"",new Vector3(0,-.025f,-.018f),.0017f);alignmentProgressLabel.color=new Color(.6f,.85f,1);
   Shape(parent,"Error divider",PrimitiveType.Cube,new Vector3(0,-.055f,-.012f),new Vector3(.46f,.001f,.001f),gold);
   alignmentErrorLabel=Label(parent,"",new Vector3(0,-.095f,-.018f),.0019f);alignmentErrorLabel.color=new Color(1,.22f,.24f);alignmentErrorLabel.fontStyle=FontStyle.Bold;
   if(spinner!=null)spinner.localPosition=new Vector3(-.225f,-.025f,-.025f);
   UpdateAlignmentMessages();
  }
  void UpdateAlignmentMessages(){if(!AlignmentMessageView)return;if(status!=null)status.text=AlignmentInstructions;if(alignmentProgressLabel!=null)alignmentProgressLabel.text=WrapStatus(alignmentProgress);if(alignmentErrorLabel!=null)alignmentErrorLabel.text=alignmentError.Length==0?"":WrapStatus("ACTION NEEDED: "+alignmentError);}
 }
}
