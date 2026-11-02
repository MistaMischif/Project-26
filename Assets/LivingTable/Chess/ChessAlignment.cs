using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  float nextAlignmentPreview;
  public bool AlignmentInProgress=>alignStage>0;
  public void BeginAlignment(){Version++;CancelGrab();Vision.ClearEvidence();alignStage=1;Aligned=false;Message="Match the grid to the PLAYING squares, not the frame. Pinch near-left, then near-right. Cyan = White; amber = Black.";DrawScene();}
  void DrawAlignmentGuide(){
   if(!Physical||Playing&&!Recovering&&alignStage==0)return;
   var guide=new GameObject("Alignment grid").transform;guide.SetParent(board,false);
   for(int i=0;i<=8;i++){float at=-Size*.5f+i*Size/8;Shape(guide,"Grid file "+i,PrimitiveType.Cube,new Vector3(at,.004f,0),new Vector3(.0015f,.0015f,Size),gold);Shape(guide,"Grid rank "+i,PrimitiveType.Cube,new Vector3(0,.004f,at),new Vector3(Size,.0015f,.0015f),gold);}
   for(int i=0;i<4;i++){var p=new Vector3((i%2==0?-1:1)*Size*.5f,.008f,(i<2?-1:1)*Size*.5f);Shape(guide,"Corner marker "+i,PrimitiveType.Sphere,p,Vector3.one*.018f,i==0?green:blue);var label=Label(guide,new[]{"1 NEAR LEFT","2 NEAR RIGHT","FAR LEFT","FAR RIGHT"}[i],p+new Vector3(0,.015f,i<2?-.035f:.035f),.0016f);label.transform.localRotation=Quaternion.Euler(90,0,0);}
   var size=Label(guide,$"{Size*100:F1} cm / {Size*100/8:F1} cm squares",new Vector3(0,.006f,-Size*.5f-.03f),.0018f);size.transform.localRotation=Quaternion.Euler(90,0,0);
  }
  bool AlignmentPose(Vector3 point){var edge=point-firstCorner;edge.y=0;if(edge.magnitude<.2f||edge.magnitude>1.2f||Mathf.Abs(point.y-firstCorner.y)>.12f)return false;Size=edge.magnitude;var forward=Vector3.Cross(edge.normalized,Vector3.up);transform.SetPositionAndRotation(firstCorner+edge*.5f+forward*Size*.5f,Quaternion.LookRotation(forward));return true;}
  void PreviewAlignment(Vector3 world){if(alignStage!=2||Time.unscaledTime<nextAlignmentPreview)return;nextAlignmentPreview=Time.unscaledTime+.12f;if(AlignmentPose(world))DrawScene();}
  void AcceptAlignmentPoint(Vector3 world){if(alignStage==1){firstCorner=world;alignStage=2;Message="First corner fixed. Move to near-right PLAYING corner: grid and pieces resize live. Pinch to set.";return;}if(!AlignmentPose(world)){Message="Use the other near playing corner: 20–120 cm across, on the same table.";return;}Aligned=true;alignStage=0;Vision.ClearEvidence();Message="Check all four corner markers and ghost pieces. ALIGN BOARD retries; PLAY NOW starts. Cyan = White, amber = Black.";if(Playing){Recovering=true;SaveSession("board realigned");}DrawScene();}
 }
}
