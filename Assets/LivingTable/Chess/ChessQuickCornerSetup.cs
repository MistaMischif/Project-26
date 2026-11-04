using System;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  static readonly string[] CornerNames={"NEAR LEFT","NEAR RIGHT","FAR RIGHT","FAR LEFT"};
  Vector3 setupRight=Vector3.right,setupForward=Vector3.forward;
  bool quickGridConfirmed;int editingSetupCorner=-1;
  readonly Transform[] cornerMarkers=new Transform[4];
  public bool ReviewingQuickGrid=>quickCapture&&alignStage==12;
  public bool QuickGridConfirmed=>quickGridConfirmed;
  public int PlacedCornerCount=>cornerCount;
  const string ManualCornerNotice="MANUAL markers — camera snapping unavailable. Confirm each marker on the playing-area corner, excluding the frame.";
  void CaptureSetupHeading(){var eye=Camera.main;setupForward=Vector3.ProjectOnPlane(eye!=null?eye.transform.forward:transform.forward,Vector3.up);if(setupForward.magnitude<.1f)setupForward=Vector3.ProjectOnPlane(eye!=null?eye.transform.up:transform.forward,Vector3.up);if(setupForward.sqrMagnitude<.01f)setupForward=Vector3.forward;setupForward.Normalize();setupRight=Vector3.Cross(Vector3.up,setupForward).normalized;quickGridConfirmed=false;cornerCount=0;}
  void BeginQuickCorners(){
   calibration?.Cancel();calibrationBusy=false;Vision.CancelScan();CancelHumanReview();Version++;CancelGrab();HomeScreen=false;standardMenu=sessionMenu=extraSettings=pieceSettings=false;Aligned=BoardPoseLocked=quickGridConfirmed=false;metricFitReady=false;cornerCount=0;editingSetupCorner=-1;alignStage=1;
   Message="1 / 4: pinch NEAR LEFT. "+ManualCornerNotice;DrawScene();
  }
  public static bool OrderedFromView(Vector3[] corners,Vector3 right,Vector3 forward){return corners!=null&&corners.Length==4&&Vector3.Dot(corners[1]-corners[0],right)>.1f&&Vector3.Dot(corners[3]-corners[0],forward)>.1f;}
  public static bool PreciseCornerFit(Vector3[] points,out Vector3 center,out Quaternion rotation,out float size){if(!FitCorners(points,out center,out rotation,out size))return false;for(int i=0;i<4;i++){var expected=center+rotation*new Vector3(i==0||i==3?-size*.5f:size*.5f,0,i<2?-size*.5f:size*.5f);if(!float.IsFinite(points[i].sqrMagnitude)||Vector3.Distance(expected,points[i])>Mathf.Clamp(size*.015f,.004f,.012f))return false;}return true;}
  bool FinishQuickCorners(){
   calibration?.Cancel();calibrationBusy=false;Aligned=BoardPoseLocked=quickGridConfirmed=false;
   if(cornerCount!=4||!OrderedFromView(setupCorners,setupRight,setupForward)||!PreciseCornerFit(setupCorners,out var center,out var rotation,out var size)){
    alignStage=1;Message="Corners do not form an ordered square. Pinch a marker to adjust it, or RESTART SETUP. Order: near-left, near-right, far-right, far-left.";DrawScene();return false;
   }
   Size=size;transform.SetPositionAndRotation(center,rotation);alignStage=12;Message="Does this grid match your chessboard?\n"+ManualCornerNotice;DrawScene();return true;
  }
  public void ConfirmQuickGrid(){
   if(!ReviewingQuickGrid||cornerCount!=4||!OrderedFromView(setupCorners,setupRight,setupForward)||!PreciseCornerFit(setupCorners,out _,out _,out _))return;
   calibration?.Lock();calibrationBusy=false;quickGridConfirmed=BoardPoseLocked=Aligned=true;metricFitReady=false;Version++;quickPhase=2;RecordRegistration();BeginKingSizing();
  }
  public void AdjustQuickCorners(){if(!quickCapture)return;calibration?.Cancel();calibrationBusy=false;quickGridConfirmed=BoardPoseLocked=Aligned=false;alignStage=1;Message="Pinch and move any labeled marker, then release. "+ManualCornerNotice;DrawScene();}
  public void RestartQuickSetup(){if(!quickCapture)return;quickGridConfirmed=BoardPoseLocked=Aligned=false;cornerCount=0;editingSetupCorner=-1;setupPosition=null;unknownSquares=null;quickPhase=0;BeginAutomaticAlignment();}
  bool QuickCornerUI(){
   if(!quickCapture)return false;
   if(ReviewingQuickGrid){Add("CONFIRM",0,0,ConfirmQuickGrid,Purple,true);Add("ADJUST CORNERS",-2,0,AdjustQuickCorners,Teal);Add("RESTART SETUP",-1,0,RestartQuickSetup,Teal);return true;}
   if(alignStage!=1)return false;
   Add(cornerCount<4?"PLACE CORNER "+(cornerCount+1):"REVIEW GRID",0,0,()=>{if(cornerCount==4)FinishQuickCorners();else Message="Pinch "+CornerNames[cornerCount]+". "+ManualCornerNotice;},Purple,true);
   Add("RESTART SETUP",-2,0,RestartQuickSetup,Teal);if(cornerCount>0)Add("UNDO CORNER",-1,0,()=>{cornerCount--;quickGridConfirmed=false;Message="Place "+CornerNames[cornerCount]+" again. "+ManualCornerNotice;DrawScene();},Teal);return true;
  }
  void DrawQuickCornerMarkers(Transform guide){
   for(int i=0;i<4;i++)cornerMarkers[i]=null;
   for(int i=0;i<cornerCount;i++){var marker=new GameObject("Manual corner "+(i+1)+" "+CornerNames[i]).transform;marker.SetParent(guide,false);marker.position=setupCorners[i];cornerMarkers[i]=marker;Shape(marker,"Manual amber marker",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.022f,gold);var text=Label(marker,(i+1)+" "+CornerNames[i]+"\nMANUAL",new Vector3(0,.018f,.025f),.0012f);text.transform.localRotation=Quaternion.Euler(90,0,0);}
  }
  bool GrabQuickCorner(int hand,Vector3 world){
   if(!quickCapture||!(SelectingCorners||ReviewingQuickGrid))return false;
   int best=-1;float distance=.045f;for(int i=0;i<cornerCount;i++){float d=Vector3.Distance(world,setupCorners[i]);if(d<distance){best=i;distance=d;}}
   if(best<0)return false;editingSetupCorner=best;setupHand=hand;setupSeen=Time.unscaledTime;quickGridConfirmed=BoardPoseLocked=Aligned=false;alignStage=1;Message="Adjust "+CornerNames[best]+"; release to review. MANUAL — not camera verified.";return true;
  }
  void DrawMainMenuButton(){
   float unit=Size/.56f;var root=new GameObject("Menu button").transform;root.SetParent(ui,false);root.localPosition=new Vector3(0,.025f,-Size*.5f-.16f*unit);var text=Label(root,"MENU",Vector3.up*.012f,.0014f*unit);text.transform.localRotation=Quaternion.Euler(90,0,0);var visual=root.gameObject.AddComponent<TableButtonVisual>();visual.Build(text.transform,Teal,false,false,.055f*unit);visual.UseGoldTrim();text.color=Color.black;buttons.Add(new Button{root=root,visual=visual,radius=.035f*unit,action=OpenStandardMenu});
  }
 }
}
