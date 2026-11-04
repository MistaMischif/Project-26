using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  string captureInstruction;float capturePromptUntil;int captureTrayColor;
  Material instructionMaterial,scanReading,scanComplete,scanUncertain,scanMismatch;Transform spinner,scanRoot;readonly Renderer[] scanSquares=new Renderer[64];
  bool CriticalInstructions=>boardNeedsMove||VirtualSafetyOpen||standardMenu||!HomeScreen&&Physical&&(!Playing||relocating||AlignmentInProgress||Recovering||AwaitingMoveAcceptance||Vision.Checking||Vision.NeedsAttention);
  public bool PointAtMenu(Ray ray,bool activate){
   if(VirtualSafetyOpen){foreach(var button in buttons){var planeButton=new Plane(button.root.up,button.root.position);if(!planeButton.Raycast(ray,out float depth)||depth<.05f||depth>4)continue;var point=button.root.InverseTransformPoint(ray.GetPoint(depth));if(Mathf.Abs(point.x)>button.radius||Mathf.Abs(point.z)>button.radius)continue;button.visual.Hover();if(activate&&Time.unscaledTime>=panelClickAt){panelClickAt=Time.unscaledTime+.4f;button.action();}return true;}}
   if(panelMenu==null||grabHand!=int.MinValue||setupHand!=int.MinValue||relocating&&gameObject.activeInHierarchy)return false;
   var plane=new Plane(panelMenu.up,panelMenu.position);if(!plane.Raycast(ray,out float distance)||distance<.05f||distance>4)return false;var hit=ray.GetPoint(distance);var local=panelMenu.InverseTransformPoint(hit);
   if(Mathf.Abs(local.x)>.035f||Mathf.Abs(local.z)>.035f)return false;panelMenuVisual.Hover();if(activate)TryPanelButton(panelMenu.position);return true;
  }
  void DrawScanSpinner(Transform parent){
   spinner=new GameObject("Scan loading spinner").transform;spinner.SetParent(parent,false);spinner.localPosition=new Vector3(0,CriticalInstructions&&!standardMenu?-.052f:-.095f,-.025f);
   for(int i=0;i<10;i++){float angle=i*Mathf.PI*2/10;Shape(spinner,"Loading dot "+i,PrimitiveType.Sphere,new Vector3(Mathf.Cos(angle)*.012f,Mathf.Sin(angle)*.012f,0),Vector3.one*(.0025f+i*.00025f),gold);}
   spinner.gameObject.SetActive(calibrationBusy||Vision.Checking);
  }
  void DrawScanSquares(){
   if(scanRoot!=null){scanRoot.gameObject.SetActive(false);Destroy(scanRoot.gameObject);}scanRoot=Node("Physical scan progress");
   if(scanReading==null){scanReading=Mat(new Color(.1f,.65f,1),true);scanComplete=Mat(new Color(.1f,1,.45f),true);scanUncertain=Mat(new Color(1,.4f,.03f),true);scanMismatch=Mat(new Color(1,.04f,.08f),true);foreach(var m in new[]{scanReading,scanComplete,scanUncertain,scanMismatch}){m.SetFloat("_Square",1);m.SetFloat("_Strength",.22f);}}
   for(int square=0;square<64;square++){var tile=Shape(scanRoot,"Scan "+ChessMove.Square(square),PrimitiveType.Quad,SquareLocal(square)+Vector3.up*.002f,new Vector3(Size/8*.88f,Size/8*.88f,1),scanReading);tile.localRotation=Quaternion.Euler(90,0,0);scanSquares[square]=tile.GetComponent<Renderer>();tile.gameObject.SetActive(false);}UpdateScanFeedback();
  }
  void UpdateScanFeedback(){
   if(spinner!=null){spinner.gameObject.SetActive(calibrationBusy||Vision.Checking);spinner.localRotation=Quaternion.Euler(0,0,-Time.unscaledTime*200);}
   if(scanRoot==null||Vision==null)return;bool show=Physical&&Aligned&&!HomeScreen&&!relocating;for(int square=0;square<64;square++){if(scanSquares[square]==null)continue;byte state=Vision.CellProgress[square];scanSquares[square].gameObject.SetActive(show&&state>0);scanSquares[square].sharedMaterial=state==1?scanReading:state==2?scanComplete:state==4?scanMismatch:scanUncertain;}
  }
  public static int[] CapturedSinceStart(ChessMatch.State state){
   var taken=new List<int>();if(state?.initial==null||state.moves==null)return taken.ToArray();var position=state.initial.Copy();foreach(var saved in state.moves){var move=saved.Move;int square=move.EnPassant?move.To-position.Side*8:move.To;int piece=position.Board[square];if(piece!=0)taken.Add(piece);position=position.After(move);}return taken.ToArray();
  }
  public static int MissingBeforeStart(ChessMatch.State state,int color)=>state?.initial==null?0:Math.Max(0,16-state.initial.Board.Count(piece=>Math.Sign(piece)==color));
  public float CaptureSlotSize=>Mathf.Max(.045f*Size/.56f,.038f*PieceScale(6).x+.012f);
  public float CaptureInnerEdge=>Size*.5f+.06f+.055f+Mathf.Max(.035f,Size*.06f);
  void DrawCapturedPieces(){
   if(!Playing||HomeScreen)return;var state=Match.Capture();var recorded=CapturedSinceStart(state);float unit=Size/.56f;float slot=CaptureSlotSize;
   foreach(int color in new[]{1,-1}){var pieces=recorded.Where(p=>Math.Sign(p)==color).ToArray();int unknown=MissingBeforeStart(state,color),total=unknown+pieces.Length;
    var tray=new GameObject(SideName(color)+" captured pieces").transform;tray.SetParent(men,false);tray.localPosition=new Vector3((color>0?-1:1)*(CaptureInnerEdge+slot),0,0);
    for(int column=0;column<=2;column++)Shape(tray,"Capture gold grid line",PrimitiveType.Cube,new Vector3((column-1)*slot,.0005f,0),new Vector3(.002f,.001f,slot*8),gold);
    for(int row=0;row<=8;row++)Shape(tray,"Capture gold grid line",PrimitiveType.Cube,new Vector3(0,.0005f,(row-4)*slot),new Vector3(slot*2,.001f,.002f),gold);
    for(int i=0;i<total;i++){var at=new Vector3((i%2==0?-.5f:.5f)*slot,.0015f,(i/2-3.5f)*slot);var name=Label(tray,i>=unknown?PieceName(pieces[i-unknown]).ToUpperInvariant():"UNKNOWN",at,.0009f*unit);name.name="Captured piece label";name.transform.localRotation=Quaternion.Euler(90,0,0);name.color=new Color(1,.85f,.45f);}
    if(Physical&&color==captureTrayColor&&Time.unscaledTime<capturePromptUntil&&total>0){int slotIndex=total-1;var at=new Vector3((slotIndex%2==0?-.5f:.5f)*slot,.001f,(slotIndex/2-3.5f)*slot);var marker=Shape(tray,"Place captured physical piece here",PrimitiveType.Quad,at,new Vector3(slot*.72f,slot*.72f,1),green);marker.localRotation=Quaternion.Euler(90,0,0);}
    string caption=SideName(-color).ToUpperInvariant()+" CAPTURED: "+total+(unknown>0?"\n"+unknown+" before scan · types unknown":"");var label=Label(tray,caption,new Vector3(0,.065f,-slot*4-.035f),.0017f*unit);label.fontStyle=FontStyle.Bold;label.gameObject.AddComponent<TableFacingPanel>();
   }
  }
 }
}
