using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  int resizeHand=int.MinValue,resizeCorner=-1;float resizeSeen,resizeStartSize;Vector3 resizeStartPoint,resizeAxis;Material handleGlow,adjustGrid;Transform[] handleGrips=new Transform[4];Renderer[] handleRenderers=new Renderer[4];
  public bool ResizingBoard=>resizeHand!=int.MinValue;
  public bool AdjustingBoard=>relocating;
  public Vector3 ResizeHandleWorld(int corner)=>transform.TransformPoint(new Vector3((corner%2==0?-1:1)*(Size*.5f+.06f),.025f,(corner<2?-1:1)*(Size*.5f+.06f)));
  void DrawBoardHandles(){
   if(handleGlow==null){handleGlow=Mat(new Color(1,.85f,.35f),true);handleGlow.SetFloat("_Square",1);adjustGrid=Mat(new Color(1,.72f,.2f),true);adjustGrid.SetFloat("_Square",1);}
   var root=new GameObject("Board adjustment handles").transform;root.SetParent(board,false);
   for(int i=0;i<4;i++){
    var p=transform.InverseTransformPoint(ResizeHandleWorld(i));Shape(root,"Knob mount "+i,PrimitiveType.Cylinder,p-Vector3.up*.018f,new Vector3(.055f,.006f,.055f),gold);
    var grip=Shape(root,"Corner knob "+i,PrimitiveType.Sphere,p,Vector3.one*.048f,gold);handleGrips[i]=grip;handleRenderers[i]=grip.GetComponent<Renderer>();
   }
   UpdateHandleFeedback();
  }
  public bool BeginBoardResize(int hand,Vector3 point)=>false; // Two corner pinches own board adjustment.
  public void ScaleHeldBoard(float size){Size=Mathf.Clamp(size,.2f,1.2f);float ratio=Size/resizeStartSize;foreach(var root in new[]{board,men,ui,marks})if(root!=null)root.localScale=Vector3.one*ratio;}
  public void BeginCornerAdjustment(){resizeStartSize=Size;BeginRelocation();}
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
   for(int i=0;i<4;i++)if(handleGrips[i]!=null){bool active=relocating;handleRenderers[i].sharedMaterial=active?handleGlow:gold;handleGrips[i].localScale=Vector3.one*.048f*(active?1.25f:1);}

  }
 }
}
