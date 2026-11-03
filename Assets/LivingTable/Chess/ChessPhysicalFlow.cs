using System;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  ChessMove? humanPreview;int humanPreviewVersion;
  public float MaxPhysicalPieceHeight=>Mathf.Clamp(PieceScale(HumanSide*6).y*.082f,.04f,.22f);
  public void RefreshScanControls(){if(ui!=null&&isActiveAndEnabled)DrawUI();}
  public bool AwaitingMoveAcceptance=>humanPreview.HasValue&&humanPreviewVersion==Version;
  ChessPosition DisplayedPosition {
   get {
    if(editing)return importPosition;
    if(SizingKing){var guide=new ChessPosition{Board=new int[64]};guide.Board[kingSquare]=HumanSide*6;return guide;}
    if(setupPosition!=null&&!Playing)return setupPosition;
    var position=Match?.Position??ChessPosition.Initial();
    if(Physical&&PendingAi.HasValue)return position.After(PendingAi.Value);
    return AwaitingMoveAcceptance?position.After(humanPreview.Value):position;
   }
  }
  public void CancelHumanReview(){bool redraw=humanPreview.HasValue;humanPreview=null;if(redraw&&men!=null&&isActiveAndEnabled){DrawMen();DrawUI();}}
  public bool PreviewHumanMove(ChessMove move,int version){
   if(version!=Version||!Playing||!Physical||Recovering||PendingAi.HasValue||Paused||Busy||SessionMenuOpen||StorageBlocked||!HandsClear||!Match.Position.LegalMoves().Contains(move)||(!TwoPlayers&&Match.Position.Side!=HumanSide))return false;
   humanPreview=move;humanPreviewVersion=version;DrawMen();DrawUI();return true;
  }
  public bool CommitObservedMove(ChessMove move,int version,bool fullBoardVerified){
   if(version!=Version||!Playing||!Physical||!Aligned||Paused||Busy||SessionMenuOpen||StorageBlocked||!HandsClear||!Match.Position.LegalMoves().Contains(move))return false;
   if(PendingAi.HasValue){if(!PendingAi.Value.Equals(move)||Recovering&&!fullBoardVerified)return false;}
   else if(Recovering||!AwaitingMoveAcceptance||!humanPreview.Value.Equals(move))return false;
   // A restored pending AI move is committed only after its entire resulting board was verified.
   bool recovery=Recovering,manual=ManualFallback;Recovering=false;if(recovery&&fullBoardVerified)ManualFallback=false;bool result=ApplyCameraMove(move,version);if(!result){Recovering=recovery;ManualFallback=manual;}return result;
  }
  Transform panelMenu;TableButtonVisual panelMenuVisual;bool panelArmed;float panelClickAt;
  void ConfigureBackPanel(Transform p){
   p.SetParent(FindFirstObjectByType<LivingTableExperience>()?.transform,true);p.localScale=Vector3.one*panelScale;
   panel.SetBoardBackground(transform,Size,Playing?.34f:.25f);panel.FaceViewer();
  }
  void DrawPanelMenu(Transform p){
   panelMenu=new GameObject("Always available menu").transform;panelMenu.SetParent(p,false);panelMenu.localPosition=new Vector3(.19f,.075f,-.025f);panelMenu.localRotation=Quaternion.Euler(-90,0,0);
   var label=Label(panelMenu,"≡",Vector3.zero,.003f);label.transform.localRotation=Quaternion.Euler(90,0,0);label.color=Color.black;
   panelMenuVisual=panelMenu.gameObject.AddComponent<TableButtonVisual>();panelMenuVisual.Build(label.transform,Purple,false,true,.035f);panelMenuVisual.UseGoldTrim();
  }
  void HoverPanel(Vector3 world){if(panelMenu==null)return;var p=panelMenu.InverseTransformPoint(world);if(Mathf.Abs(p.x)<.025f&&Mathf.Abs(p.z)<.025f&&p.y>-.02f&&p.y<.07f)panelMenuVisual.Hover();}
  bool TryPanelButton(Vector3 world){if(panelMenu==null||Time.unscaledTime<panelClickAt)return false;var p=panelMenu.InverseTransformPoint(world);if(Mathf.Abs(p.x)>.025f||Mathf.Abs(p.z)>.025f||p.y<-.02f||p.y>.05f)return false;panelClickAt=Time.unscaledTime+.4f;panelMenuVisual.Press();OpenStandardMenu();return true;}
  bool TryPanelPress(int hand,Vector3 world){HoverPanel(world);if(panelMenu==null)return false;var p=panelMenu.InverseTransformPoint(world);if(p.y>.045f)panelArmed=true;if(panelArmed&&p.y<.022f&&TryPanelButton(world)){panelArmed=false;return true;}return false;}
  public bool TryPersistentPanel(Vector3 world){if(TryPanelButton(world))return true;if(!standardMenu)return false;foreach(var b in buttons){var p=b.root.InverseTransformPoint(world);if(Mathf.Abs(p.x)<=b.radius&&Mathf.Abs(p.z)<=b.radius&&p.y>=-.02f&&p.y<=.05f){b.action();return true;}}return false;}
  void DrawPanelSlider(){
   panelSlider=new GameObject("Back panel size slider").transform;panelSlider.SetParent(panel.transform,false);panelSlider.localPosition=new Vector3(0,-.08f,-.025f);
   Shape(panelSlider,"Size rail",PrimitiveType.Cube,Vector3.zero,new Vector3(.25f,.004f,.004f),gold);
   sliderKnob=Shape(panelSlider,"Size thumb",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.022f,gold);
   Label(panel.transform,"PANEL SIZE",new Vector3(0,-.105f,-.025f),.0018f);SetPanelScale(panelScale);
  }
  bool PanelSliderPoint(Vector3 world,out float value,bool dragging=false){value=panelScale;if(panelSlider==null)return false;var p=panelSlider.InverseTransformPoint(world);if(!dragging&&(Mathf.Abs(p.y)>.04f||Mathf.Abs(p.z)>.06f||Mathf.Abs(p.x)>.15f))return false;value=Mathf.Lerp(.75f,1.8f,Mathf.InverseLerp(-.125f,.125f,p.x));return true;}
  void SetPanelScale(float value){panelScale=Mathf.Clamp(value,.75f,1.8f);if(panel!=null){panel.transform.localScale=Vector3.one*panelScale;panel.FaceViewer();}if(sliderKnob!=null)sliderKnob.localPosition=new Vector3(Mathf.Lerp(-.125f,.125f,Mathf.InverseLerp(.75f,1.8f,panelScale)),0,-.01f);}
 }
}
