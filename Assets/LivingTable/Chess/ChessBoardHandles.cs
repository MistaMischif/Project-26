using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  int resizeHand=int.MinValue,resizeCorner=-1;float resizeSeen,resizeStartSize;Vector3 resizeStartPoint,resizeAxis,resizeFixed,resizeNormal,resizeOffset;Quaternion resizeRotation;Material handleGlow,adjustGrid;Transform[] handleGrips=new Transform[4];Renderer[] handleRenderers=new Renderer[4];
  readonly TextMesh[,] knobIcons=new TextMesh[4,3];Vector3 knobLastPosition;Quaternion knobLastRotation;float knobLastSize;readonly float[] knobLitUntil=new float[3];
  public bool ResizingBoard=>resizeHand!=int.MinValue;
  public bool AdjustingBoard=>relocating;
  public Vector3 ResizeHandleWorld(int corner)=>transform.TransformPoint(new Vector3((corner%2==0?-1:1)*(Size*.5f+.06f),.025f,(corner<2?-1:1)*(Size*.5f+.06f)));
  void DrawBoardHandles(){
   if(quickCapture&&!quickGridConfirmed&&entryPhase==0)return;
   if(handleGlow==null){handleGlow=Mat(new Color(1,.85f,.35f),true);handleGlow.SetFloat("_Square",1);adjustGrid=Mat(new Color(1,.72f,.2f),true);adjustGrid.SetFloat("_Square",1);}
   var root=new GameObject("Board adjustment handles").transform;root.SetParent(board,false);
   for(int i=0;i<4;i++){
    var p=transform.InverseTransformPoint(ResizeHandleWorld(i));Shape(root,"Knob mount "+i,PrimitiveType.Cylinder,p-Vector3.up*.018f,new Vector3(.055f,.006f,.055f),gold);
    var grip=new GameObject("Corner knob "+i).transform;grip.SetParent(root,false);grip.localPosition=p;grip.gameObject.AddComponent<TableCabinetVisual>().Knob();handleGrips[i]=grip;for(int mode=0;mode<3;mode++){var icon=Label(grip,new[]{"\u2194 MOVE","\u21bb TURN","\u2197 SIZE"}[mode],new Vector3(0,.018f,(mode-1)*.010f),.00065f);icon.transform.localRotation=Quaternion.Euler(90,0,0);knobIcons[i,mode]=icon;}
   }
   UpdateHandleFeedback();
  }
  public bool BeginBoardResize(int hand,Vector3 point){
   if(BoardPoseLocked&&entryPhase!=3||HomeScreen||Busy||StorageBlocked||SessionMenuOpen||relocating||SelectingCorners||calibrationBusy||quickCapture&&!quickGridConfirmed&&entryPhase==0)return false;
   int corner=-1;float nearest=.055f;for(int i=0;i<4;i++){float distance=Vector3.Distance(point,ResizeHandleWorld(i));if(distance<nearest){nearest=distance;corner=i;}}
   if(corner<0)return false;BoardPoseLocked=false;if(entryPhase==3){entryPhase=1;quickGridConfirmed=false;}resizeCorner=corner;resizeHand=hand;resizeSeen=Time.unscaledTime;resizeStartSize=Size;resizeStartPoint=point;resizeRotation=transform.rotation;resizeNormal=transform.up;
   var diagonal=new Vector3(corner%2==0?-1:1,0,corner<2?-1:1);resizeFixed=transform.TransformPoint(-diagonal*Size*.5f);resizeAxis=transform.rotation*diagonal;resizeOffset=point-ResizeHandleWorld(corner);
   BeginRelocation();Message="Opposite playing corner locked. Pull this knob to fit the board; release to review.";return true;
  }
  public void ScaleHeldBoard(float size){Size=Mathf.Clamp(size,.2f,1.2f);float ratio=Size/resizeStartSize;foreach(var root in new[]{board,men,ui,marks})if(root!=null)root.localScale=Vector3.one*ratio;}
  public void BeginCornerAdjustment(){if(BoardPoseLocked&&entryPhase!=3||quickCapture&&!quickGridConfirmed&&entryPhase==0)return;BoardPoseLocked=false;if(entryPhase==3){entryPhase=1;quickGridConfirmed=false;}if(ResizingBoard){resizeHand=int.MinValue;resizeCorner=-1;DrawScene();}resizeStartSize=Size;BeginRelocation();}
  public bool UpdateBoardResize(int hand,Vector3 point,bool release){
   if(hand!=resizeHand)return false;resizeSeen=Time.unscaledTime;
   var line=Vector3.ProjectOnPlane(point-resizeOffset-resizeFixed,resizeNormal);if(line.magnitude<.08f){if(release)EndBoardResize();return true;}
   var turn=Quaternion.AngleAxis(Vector3.SignedAngle(resizeAxis,line,resizeNormal),resizeNormal);var rotation=turn*resizeRotation;
   float size=Mathf.Clamp(line.magnitude/Mathf.Sqrt(2)-.06f,.2f,1.2f);var diagonal=new Vector3(resizeCorner%2==0?-1:1,0,resizeCorner<2?-1:1);
   transform.SetPositionAndRotation(resizeFixed+rotation*diagonal*(size*.5f),rotation);ScaleHeldBoard(size);
   if(release)EndBoardResize();return true;
  }
  void EndBoardResize(){if(!ResizingBoard)return;resizeHand=int.MinValue;resizeCorner=-1;EndRelocation();}
  void UpdateHandleFeedback(){
   if(ResizingBoard&&Time.unscaledTime-resizeSeen>.3f){EndBoardResize();return;}
   float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*5);if(adjustGrid!=null)adjustGrid.SetFloat("_Strength",.5f+pulse*.9f);
   if(relocating){if(Vector3.Distance(transform.position,knobLastPosition)>.0005f)knobLitUntil[0]=Time.unscaledTime+.25f;if(Quaternion.Angle(transform.rotation,knobLastRotation)>.1f)knobLitUntil[1]=Time.unscaledTime+.25f;if(Mathf.Abs(Size-knobLastSize)>.0005f)knobLitUntil[2]=Time.unscaledTime+.25f;}knobLastPosition=transform.position;knobLastRotation=transform.rotation;knobLastSize=Size;
   for(int i=0;i<4;i++)if(handleGrips[i]!=null){handleGrips[i].localScale=Vector3.one*(relocating?1.1f:1);for(int mode=0;mode<3;mode++)if(knobIcons[i,mode]!=null)knobIcons[i,mode].color=Time.unscaledTime<knobLitUntil[mode]?new Color(.2f,1,1):entryPhase==1?Color.Lerp(new Color(.3f,.65f,.9f),Color.cyan,pulse):new Color(1,.8f,.2f);}

  }
 }
}
