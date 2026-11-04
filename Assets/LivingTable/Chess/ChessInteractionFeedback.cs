using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  Transform hoverSquareGlow,placementGlow;Material hoverGlowMaterial,placementGlowMaterial;
  int hoveredSquare=-1,confirmedPlacement=-1;float hoverUntil,placementUntil;
  public int HoveredPieceSquare=>Time.unscaledTime<hoverUntil?hoveredSquare:-1;
  public int ConfirmedPlacementSquare=>Time.unscaledTime<placementUntil?confirmedPlacement:-1;
  int PieceInteractionSquare(Vector3 world){
   if(!Playing||HomeScreen||entryPhase>0||relocating||Paused||SessionMenuOpen||GameOver)return -1;
   var p=transform.InverseTransformPoint(world);int square=SquareAt(p);
   return square>=0&&p.y>=-.005f&&p.y<=MaxPhysicalPieceHeight+.025f&&DisplayedPosition.Board[square]!=0?square:-1;
  }
  public void HoverPiece(Vector3 world){int square=PieceInteractionSquare(world);if(square<0||grabHand!=int.MinValue)return;hoveredSquare=square;hoverUntil=Time.unscaledTime+.12f;}
  void ConfirmPlacement(int square){confirmedPlacement=square;placementUntil=Time.unscaledTime+2.4f;}
  void ResetInteractionFeedback(){hoveredSquare=confirmedPlacement=-1;hoverUntil=placementUntil=0;}
  void UpdateInteractionFeedback(){
   bool visible=Playing&&!HomeScreen&&!Paused&&!SessionMenuOpen&&!relocating&&entryPhase==0;
   UpdateSquareGlow(ref hoverSquareGlow,ref hoverGlowMaterial,"Piece hover glow",hoveredSquare,visible&&grabHand==int.MinValue&&Time.unscaledTime<hoverUntil,new Color(.22f,.65f,1),.24f, .9f);
   float remaining=Mathf.Clamp01((placementUntil-Time.unscaledTime)/2.4f);
   UpdateSquareGlow(ref placementGlow,ref placementGlowMaterial,"Verified purple placement",confirmedPlacement,visible&&remaining>0,new Color(.65f,.12f,1),.38f*Mathf.Min(1,remaining*3),.88f+.08f*(1-remaining));
  }
  void UpdateSquareGlow(ref Transform tile,ref Material material,string name,int square,bool show,Color color,float strength,float width){
   if(tile==null){material=Mat(color,true);material.SetFloat("_Square",1);material.SetFloat("_Saturated",1);tile=Shape(transform,name,PrimitiveType.Quad,Vector3.zero,Vector3.one,material);tile.localRotation=Quaternion.Euler(90,0,0);}
   tile.gameObject.SetActive(show&&square>=0);if(!show||square<0)return;
   tile.localPosition=SquareLocal(square)+Vector3.up*.003f;tile.localScale=new Vector3(Size/8*width,Size/8*width,1);material.SetFloat("_Strength",strength);
  }
 }
}
