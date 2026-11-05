using System;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  static readonly string[] CornerNames={"NEAR LEFT","NEAR RIGHT","FAR RIGHT","FAR LEFT"};
  Vector3 setupRight=Vector3.right,setupForward=Vector3.forward;
  readonly bool[] cornerLocked=new bool[4];
  public int LockedCornerCount=>System.Array.FindAll(cornerLocked,x=>x).Length;
  bool quickGridConfirmed;int editingSetupCorner=-1;
  readonly Transform[] cornerMarkers=new Transform[4];
  public bool ReviewingQuickGrid=>quickCapture&&alignStage==12;
  public bool QuickGridConfirmed=>quickGridConfirmed;
  public int PlacedCornerCount=>cornerCount;
  readonly bool[] cornerSnapped=new bool[4],cornerInferred=new bool[4];
  bool cornerSnapBusy;
  const string ManualCornerNotice="MANUAL markers — camera snapping unavailable. Confirm each marker on the playing-area corner, excluding the frame.";
  void CaptureSetupHeading(){var eye=Camera.main;setupForward=Vector3.ProjectOnPlane(eye!=null?eye.transform.forward:transform.forward,Vector3.up);if(setupForward.magnitude<.1f)setupForward=Vector3.ProjectOnPlane(eye!=null?eye.transform.up:transform.forward,Vector3.up);if(setupForward.sqrMagnitude<.01f)setupForward=Vector3.forward;setupForward.Normalize();setupRight=Vector3.Cross(Vector3.up,setupForward).normalized;quickGridConfirmed=false;cornerCount=0;}
  void BeginQuickCorners(){viewAlignedSince=-1;viewAttempted=false;
   calibration?.Cancel();calibrationBusy=false;Vision.CancelScan();CancelHumanReview();Version++;CancelGrab();HomeScreen=false;standardMenu=sessionMenu=extraSettings=pieceSettings=false;Aligned=BoardPoseLocked=quickGridConfirmed=false;metricFitReady=false;cornerCount=0;Array.Clear(cornerLocked,0,4);Array.Clear(cornerSnapped,0,4);Array.Clear(cornerInferred,0,4);cornerSnapBusy=false;editingSetupCorner=-1;alignStage=1;
   Message="1 / 4: pinch NEAR LEFT. "+ManualCornerNotice;DrawScene();
  }
  public static bool OrderedFromView(Vector3[] corners,Vector3 right,Vector3 forward){return corners!=null&&corners.Length==4&&Vector3.Dot(corners[1]-corners[0],right)>.1f&&Vector3.Dot(corners[3]-corners[0],forward)>.1f;}
  public static bool PreciseCornerFit(Vector3[] points,out Vector3 center,out Quaternion rotation,out float size){if(!FitCorners(points,out center,out rotation,out size))return false;for(int i=0;i<4;i++){var expected=center+rotation*new Vector3(i==0||i==3?-size*.5f:size*.5f,0,i<2?-size*.5f:size*.5f);if(!float.IsFinite(points[i].sqrMagnitude)||Vector3.Distance(expected,points[i])>Mathf.Clamp(size*.015f,.004f,.012f))return false;}return true;}
  void LockPassingCorners(){if(cornerCount<2||CornerLayoutState(setupCorners,cornerCount)<0)return;for(int i=0;i<cornerCount;i++)if(cornerSnapped[i])cornerLocked[i]=true;}
  bool FinishQuickCorners(){
   Aligned=BoardPoseLocked=quickGridConfirmed=false;
   if(cornerCount!=4||!OrderedFromView(setupCorners,setupRight,setupForward)||!PreciseCornerFit(setupCorners,out var center,out var rotation,out var size)){
    alignStage=1;Message="Corners do not form an ordered square. Pinch a marker to adjust it, or RESTART SETUP. Order: near-left, near-right, far-right, far-left.";DrawScene();return false;
   }
   LockPassingCorners();Size=size;transform.SetPositionAndRotation(center,rotation);alignStage=12;Message="Does this grid match your chessboard?\n"+ManualCornerNotice;DrawScene();return true;
  }
  public void ConfirmQuickGrid(){
   if(cornerSnapBusy||!ReviewingQuickGrid||cornerCount!=4||!OrderedFromView(setupCorners,setupRight,setupForward)||!PreciseCornerFit(setupCorners,out _,out _,out _))return;
   calibration?.Lock();calibrationBusy=false;quickGridConfirmed=BoardPoseLocked=Aligned=true;metricFitReady=false;Version++;quickPhase=2;RecordRegistration();BeginKingSizing();AnchorConfirmedPlane();
  }
  public void AdjustQuickCorners(){if(!quickCapture)return;cornerSnapBusy=false;calibration?.Cancel();calibrationBusy=false;quickGridConfirmed=BoardPoseLocked=Aligned=false;alignStage=1;Message="Pinch and move any labeled marker, then release. "+ManualCornerNotice;DrawScene();}
  public void RestartQuickSetup(){if(!quickCapture)return;cornerSnapBusy=false;quickGridConfirmed=BoardPoseLocked=Aligned=false;cornerCount=0;editingSetupCorner=-1;setupPosition=null;unknownSquares=null;quickPhase=0;BeginAutomaticAlignment();}
  bool QuickCornerUI(){
   if(!quickCapture)return false;
   if(SelectingCorners||ReviewingQuickGrid)Add(cornerSnapBusy?"CANCEL SNAP":"RETRY SNAP",2,0,()=>{if(cornerSnapBusy){calibration?.Cancel();cornerSnapBusy=false;Message="Snap cancelled. Confirm manual alignment or retry.";DrawScene();}else RequestCornerSnapping();},Teal);
   if(ReviewingQuickGrid){Add(cornerSnapBusy?"CHECKING CORNERS":"CONFIRM",0,0,ConfirmQuickGrid,Purple,true);Add("ADJUST CORNERS",-2,0,AdjustQuickCorners,Teal);Add("RESTART SETUP",-1,0,RestartQuickSetup,Teal);return true;}
   if(alignStage!=1)return false;
   Add(cornerCount<4?"PLACE CORNER "+(cornerCount+1):"REVIEW GRID",0,0,()=>{if(cornerCount==4)FinishQuickCorners();else Message="Pinch "+CornerNames[cornerCount]+". "+ManualCornerNotice;},Purple,true);
   Add("RESTART SETUP",-2,0,RestartQuickSetup,Teal);if(cornerCount>0)Add("UNDO CORNER",-1,0,()=>{cornerLocked[--cornerCount]=false;quickGridConfirmed=false;Message="Place "+CornerNames[cornerCount]+" again. "+ManualCornerNotice;DrawScene();},Teal);return true;
  }
  readonly Renderer[] cornerBodies=new Renderer[4],cornerWedges=new Renderer[4];
  readonly Transform[,] cornerAxes=new Transform[4,2];readonly TextMesh[] cornerLabels=new TextMesh[4];
  Material cornerValid,cornerInvalid,cornerPending;
  Material TransparentGuide(Color color,float opacity){var m=new Material(Resources.Load<Shader>("ChessAlignmentGhost"));m.color=color;m.SetFloat("_Opacity",opacity);materials.Add(m);return m;}
  public static int CornerLayoutState(Vector3[] points,int count){
   if(count==4)return PreciseCornerFit(points,out _,out _,out _)?1:-1;
   for(int i=1;i<count;i++){float length=Vector3.Distance(points[i],points[i-1]);if(length<.2f||length>1.2f)return -1;}
   if(count>=3){var a=points[1]-points[0];var b=points[2]-points[1];if(Mathf.Abs(Vector3.Dot(a.normalized,b.normalized))>.12f||Mathf.Abs(a.magnitude-b.magnitude)>.02f)return -1;}
   return 0;
  }
  void DrawQuickCornerMarkers(Transform guide){
   if(cornerValid==null){cornerValid=TransparentGuide(new Color(.1f,1,.35f),.32f);cornerInvalid=TransparentGuide(new Color(1,.04f,.03f),.38f);cornerPending=TransparentGuide(new Color(1,.7f,.15f),.25f);}
   for(int i=0;i<4;i++)cornerMarkers[i]=null;
   for(int i=0;i<cornerCount;i++){
    var marker=new GameObject("Manual corner "+(i+1)+" "+CornerNames[i]).transform;marker.SetParent(guide,false);marker.position=setupCorners[i];cornerMarkers[i]=marker;
    cornerBodies[i]=Shape(marker,"Square corner marker",PrimitiveType.Cube,new Vector3(0,-.002f,0),new Vector3(.029f,.004f,.029f),cornerPending).GetComponent<Renderer>();
    var wedge=new Mesh{name="Playable edge wedge"};wedge.vertices=new[]{Vector3.zero,new Vector3(.024f,0,0),new Vector3(0,0,.024f)};wedge.triangles=new[]{0,2,1};wedge.RecalculateNormals();guideMeshes.Add(wedge);
    var edge=new GameObject("Playable corner triangular edge");edge.transform.SetParent(marker,false);edge.transform.localPosition=Vector3.up*.0005f;edge.transform.localRotation=Quaternion.Euler(0,new[]{0,-90,180,90}[i],0);edge.AddComponent<MeshFilter>().sharedMesh=wedge;cornerWedges[i]=edge.AddComponent<MeshRenderer>();
    for(int axis=0;axis<2;axis++)cornerAxes[i,axis]=Shape(marker,"Corner edge axis",PrimitiveType.Cube,Vector3.zero,Vector3.one,cornerPending);
    cornerLabels[i]=Label(marker,"",new Vector3(0,.018f,.025f),.0012f);cornerLabels[i].transform.localRotation=Quaternion.Euler(90,0,0);
   }
   UpdateCornerVisuals();
  }
  void UpdateCornerVisuals(){
   int state=CornerLayoutState(setupCorners,cornerCount);var mat=state>0?cornerValid:state<0?cornerInvalid:cornerPending;
   for(int i=0;i<cornerCount;i++)if(cornerMarkers[i]!=null){
    var marker=cornerMarkers[i];marker.position=setupCorners[i];cornerBodies[i].sharedMaterial=cornerWedges[i].sharedMaterial=cornerLocked[i]?cornerValid:mat;
    for(int axis=0;axis<2;axis++){
     int neighbor=(i+(axis==0?1:3))%4;var localCorner=new Vector3(i==0||i==3?-.5f:.5f,0,i<2?-.5f:.5f);var nextCorner=new Vector3(neighbor==0||neighbor==3?-.5f:.5f,0,neighbor<2?-.5f:.5f);
     var delta=neighbor<cornerCount?marker.InverseTransformVector(setupCorners[neighbor]-setupCorners[i]):(nextCorner-localCorner)*Size;
     var rail=cornerAxes[i,axis];rail.localPosition=delta*.25f;rail.localRotation=delta.sqrMagnitude>.000001f?Quaternion.LookRotation(delta.normalized):Quaternion.identity;rail.localScale=new Vector3(.0025f,.0005f,delta.magnitude*.5f);rail.GetComponent<Renderer>().sharedMaterial=mat;
    }
    cornerLabels[i].text=(i+1)+" "+CornerNames[i]+(state>0?"\nVALID":state<0?"\nADJUST":"\nPLACE ALL FOUR")+(cornerLocked[i]?" / LOCKED":"")+(cornerSnapped[i]?" / SNAPPED":cornerInferred[i]?" / INFERRED":" / MANUAL");
   }
  }
  bool GrabQuickCorner(int hand,Vector3 world){
   if(!quickCapture||!(SelectingCorners||ReviewingQuickGrid))return false;
   int best=-1;float distance=.045f;for(int i=0;i<cornerCount;i++){float d=Vector3.Distance(world,setupCorners[i]);if(d<distance){best=i;distance=d;}}
   if(best<0)return false;calibration?.Cancel();cornerSnapBusy=false;cornerLocked[best]=false;cornerSnapped[best]=cornerInferred[best]=false;editingSetupCorner=best;setupHand=hand;setupSeen=Time.unscaledTime;quickGridConfirmed=BoardPoseLocked=Aligned=false;alignStage=1;Message="Adjust "+CornerNames[best]+"; release to review. MANUAL — not camera verified.";DrawUI();return true;
  }
  void PreviewMarkedSquare(){
   if(!quickCapture||cornerCount<2||cornerCount>=4)return;
   var edge=setupCorners[1]-setupCorners[0];float size=edge.magnitude;if(size<.2f||size>1.2f)return;
   var normal=Vector3.ProjectOnPlane(Vector3.up,edge).normalized;if(cornerCount==3){var n=Vector3.Cross(setupCorners[2]-setupCorners[1],edge).normalized;if(n.y>.9f)normal=n;}
   var forward=Vector3.Cross(edge.normalized,normal).normalized;if(Vector3.Dot(forward,setupForward)<0)return;
   Size=size;transform.SetPositionAndRotation((setupCorners[0]+setupCorners[1])*.5f+forward*size*.5f,Quaternion.LookRotation(forward,normal));
  }
  void RequestCornerSnapping(){
   if(desktop||calibration==null||!quickCapture||cornerCount==0||!(SelectingCorners||ReviewingQuickGrid))return;
   for(int i=0;i<4;i++)if(!cornerLocked[i]){cornerSnapped[i]=cornerInferred[i]=false;}cornerSnapBusy=true;Message="Checking playable edges. Lift hands and keep the board visible. Amber markers remain manual until verified.";DrawUI();
   var seed=new ChessSurfaceSolver.Estimate();if(cornerCount==4&&FitCorners(pinchedCorners,out var c,out var r,out var size))seed=new ChessSurfaceSolver.Estimate{valid=true,center=c,rotation=r,size=size};
   calibration.SnapCorners(seed,(fit,edges,error)=>{
    cornerSnapBusy=false;if(!quickCapture||quickGridConfirmed||!(SelectingCorners||ReviewingQuickGrid))return;
    if(fit.valid){
     // Pinches own the physical surface height and normal. Camera evidence only refines within it.
     if(!seed.valid){Message="Place four surface corners before snapping.";DrawScene();return;}
     if(Mathf.Abs(Vector3.Dot(fit.center-seed.center,seed.rotation*Vector3.up))>.006f||Vector3.Angle(fit.rotation*Vector3.up,seed.rotation*Vector3.up)>2)edges=0; // A corrected plane is inferred, not camera-verified.
     fit=PreserveLockedCornerFit(ConstrainSnapToPinchPlane(fit,seed));
     for(int i=0;i<cornerCount;i++){
      var best=NearestVerifiedCorner(fit,setupCorners[i],out float distance);
      var local=Quaternion.Inverse(fit.rotation)*(best-fit.center);int required=(local.x<0?1:2)|(local.z<0?4:8);bool observed=(edges&required)==required;
      if(!cornerLocked[i]&&distance<=Mathf.Min(.03f,fit.size/8*.35f)&&(observed||cornerCount==4)){setupCorners[i]=best;cornerSnapped[i]=observed;cornerInferred[i]=!observed;}
      else if(!cornerLocked[i])cornerSnapped[i]=false;
     }
     if(cornerCount==4)FinishQuickCorners();else {PreviewMarkedSquare();DrawScene();}
     Message=ReviewingQuickGrid?"Does this grid match your chessboard? Green edges: valid square. Red edges: adjust corners. Labels show camera SNAPPED or MANUAL.":"Place the next labeled corner. Green: valid geometry; red: adjust; amber: incomplete.";
    }else Message=error;
    DrawScene();
   });
  }
  ChessSurfaceSolver.Estimate PreserveLockedCornerFit(ChessSurfaceSolver.Estimate fit){
   int first=-1,second=-1;for(int i=0;i<cornerCount;i++)if(cornerLocked[i]){if(first<0)first=i;else if(second<0)second=i;}
   if(first<0)return fit;
   Vector3 Unit(int i)=>new Vector3(i==0||i==3?-.5f:.5f,0,i<2?-.5f:.5f);
   if(second>=0){var normal=fit.rotation*Vector3.up;var delta=Vector3.ProjectOnPlane(setupCorners[second]-setupCorners[first],normal);var local=Unit(second)-Unit(first);if(delta.magnitude>.1f){fit.size=delta.magnitude/local.magnitude;fit.rotation=Quaternion.AngleAxis(Vector3.SignedAngle(fit.rotation*local,delta,normal),normal)*fit.rotation;}}
   fit.center=setupCorners[first]-fit.rotation*(Unit(first)*fit.size);return fit;
  }
  public static ChessSurfaceSolver.Estimate ConstrainSnapToPinchPlane(ChessSurfaceSolver.Estimate fit,ChessSurfaceSolver.Estimate plane){
   var normal=plane.rotation*Vector3.up;fit.center-=normal*Vector3.Dot(fit.center-plane.center,normal);
   var forward=Vector3.ProjectOnPlane(fit.rotation*Vector3.forward,normal);fit.rotation=Quaternion.LookRotation(forward.sqrMagnitude>.001f?forward:plane.rotation*Vector3.forward,normal);return fit;
  }
  public static Vector3 NearestVerifiedCorner(ChessSurfaceSolver.Estimate fit,Vector3 point,out float distance){
   Vector3 best=point;distance=float.PositiveInfinity;
   for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2){var p=fit.center+fit.rotation*new Vector3(x*.5f,0,z*.5f)*fit.size;float d=Vector3.Distance(p,point);if(d<distance){distance=d;best=p;}}return best;
  }
  void DrawMainMenuButton(){
   float unit=Size/.56f;var root=new GameObject("Menu button").transform;root.SetParent(ui,false);root.localPosition=new Vector3(0,.025f,-Size*.5f-.18f*unit);var text=Label(root,"MENU",Vector3.up*.012f,.0014f*unit);text.transform.localRotation=Quaternion.Euler(90,0,0);var visual=root.gameObject.AddComponent<TableButtonVisual>();visual.Build(text.transform,Teal,false,false,.055f*unit);visual.UseGoldTrim();text.color=Color.black;buttons.Add(new Button{root=root,visual=visual,radius=.035f*unit,action=OpenStandardMenu});
  }
 }
}
