using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  float pieceOpacity=1;Transform opacityRail,opacityThumb;bool draggingOpacity;
  public float PieceOpacity=>pieceOpacity;
  public void SetPieceOpacity(float value){pieceOpacity=Mathf.Clamp01(value);ghostWhite?.SetFloat("_Opacity",pieceOpacity);ghostBlack?.SetFloat("_Opacity",pieceOpacity);if(opacityThumb!=null)opacityThumb.localPosition=new Vector3(Mathf.Lerp(-.125f,.125f,pieceOpacity),0,-.01f);PlayerPrefs.SetFloat("LivingTable.PieceOpacity",pieceOpacity);PlayerPrefs.Save();}
  void DrawOpacitySlider(){opacityRail=opacityThumb=null;if(!Physical)return;opacityRail=new GameObject("Virtual piece opacity slider").transform;opacityRail.SetParent(panel.transform,false);opacityRail.localPosition=new Vector3(0,-.145f,-.025f);Shape(opacityRail,"Opacity rail",PrimitiveType.Cube,Vector3.zero,new Vector3(.25f,.004f,.004f),gold);opacityThumb=Shape(opacityRail,"Opacity thumb",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.022f,blue);Label(panel.transform,"VIRTUAL PIECE OPACITY",new Vector3(0,-.17f,-.025f),.0016f);SetPieceOpacity(pieceOpacity);}
  bool OpacityPoint(Vector3 world,out float value,bool dragging=false){value=pieceOpacity;if(opacityRail==null)return false;var p=opacityRail.InverseTransformPoint(world);if(!dragging&&(Mathf.Abs(p.y)>.025f||Mathf.Abs(p.z)>.045f||Mathf.Abs(p.x)>.15f))return false;value=Mathf.InverseLerp(-.125f,.125f,p.x);return true;}
 }
}
