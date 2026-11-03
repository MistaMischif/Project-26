using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  int resizeHand=int.MinValue,resizeCorner=-1;float resizeSeen,resizeStartSize;Vector3 resizeStartPoint,resizeAxis;Material handleGlow,adjustGrid;Transform[] handleGrips=new Transform[6];Renderer[] handleRenderers=new Renderer[6];
  public bool ResizingBoard=>resizeHand!=int.MinValue;
  public bool AdjustingBoard=>relocating;
  public Vector3 ResizeHandleWorld(int corner)=>transform.TransformPoint(new Vector3((corner%2==0?-1:1)*(Size*.5f+.06f),.025f,(corner<2?-1:1)*(Size*.5f+.06f)));
  void DrawBoardHandles(){
   if(handleGlow==null){handleGlow=Mat(new Color(.15f,1,.85f),true);handleGlow.SetFloat("_Square",1);adjustGrid=Mat(new Color(.05f,1,.8f),true);adjustGrid.SetFloat("_Square",1);}
   var root=new GameObject("Board adjustment handles").transform;root.SetParent(board,false);
   for(int i=0;i<6;i++){
    bool corner=i>=2;var p=corner?transform.InverseTransformPoint(ResizeHandleWorld(i-2)):new Vector3((i==0?-1:1)*(Size*.5f+.06f),.025f,0);
    var mount=Shape(root,"Handle mount "+i,PrimitiveType.Cube,p-Vector3.up*.018f,corner?new Vector3(.065f,.012f,.065f):new Vector3(.08f,.012f,.16f),gold);
    var grip=Shape(root,(corner?"Resize corner ":"Move side ")+i,corner?PrimitiveType.Sphere:PrimitiveType.Cube,p,corner?Vector3.one*.048f:new Vector3(.035f,.035f,.13f),gold);
    handleGrips[i]=grip;handleRenderers[i]=grip.GetComponent<Renderer>();
    var label=Label(root,corner?"RESIZE":"GRAB BOTH\nMOVE / TURN",p+new Vector3(0,.03f,corner?0:.105f),corner?.0013f:.0015f);label.transform.localRotation=Quaternion.Euler(90,0,0);
   }
   UpdateHandleFeedback();
  }
  public bool BeginBoardResize(int hand,Vector3 point){
   if(ResizingBoard||relocating||Busy||StorageBlocked)return false;
   int corner=-1;float distance=.055f;for(int i=0;i<4;i++){float d=Vector3.Distance(point,ResizeHandleWorld(i));if(d<distance){distance=d;corner=i;}}
   if(corner<0)return false;
   resizeHand=hand;resizeCorner=corner;resizeStartSize=Size;resizeStartPoint=point;resizeAxis=transform.TransformDirection(new Vector3(corner%2==0?-1:1,0,corner<2?-1:1).normalized);resizeSeen=Time.unscaledTime;
   BeginRelocation();Message="Resizing board. Pull the corner outward or inward; release to review.";UpdateHandleFeedback();return true;
  }
  public bool UpdateBoardResize(int hand,Vector3 point,bool release){
   if(hand!=resizeHand)return false;resizeSeen=Time.unscaledTime;
   Size=Mathf.Clamp(resizeStartSize+Vector3.Dot(point-resizeStartPoint,resizeAxis)*Mathf.Sqrt(2),.2f,1.2f);
   if(release){EndBoardResize();return true;}
   float scale=Size/resizeStartSize;foreach(var root in new[]{board,men,ui,marks})if(root!=null)root.localScale=Vector3.one*scale;return true;
  }
  void EndBoardResize(){if(!ResizingBoard)return;resizeHand=int.MinValue;resizeCorner=-1;EndRelocation();}
  void UpdateHandleFeedback(){
   if(ResizingBoard&&Time.unscaledTime-resizeSeen>.3f){EndBoardResize();return;}
   float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*5);if(adjustGrid!=null)adjustGrid.SetFloat("_Strength",.5f+pulse*.9f);
   for(int i=0;i<6;i++)if(handleGrips[i]!=null){bool active=relocating&&(ResizingBoard?i==resizeCorner+2:i<2);handleRenderers[i].sharedMaterial=active?handleGlow:gold;var basis=i<2?new Vector3(.035f,.035f,.13f):Vector3.one*.048f;handleGrips[i].localScale=basis*(active?1.25f:1);}
  }
 }
}
