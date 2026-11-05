using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  bool standardEntry;
  int entryPhase;bool entryCapture,entryWasPhysical,entryKeepPhysical;Material entryGlow;
  public int EntryPhase=>entryPhase;
  string EntryInstructions=>entryPhase==1?"Align the board over your physical board.\nSelect CONTINUE.":WrapStatus(Message);
  public void BeginEntry(bool capture=false){
   entryCapture=capture;entryKeepPhysical=Physical;entryWasPhysical=Physical;entryPhase=1;HomeScreen=false;BoardPoseLocked=Aligned=false;quickCapture=true;quickGridConfirmed=false;Paused=Playing;alignStage=13;calibration?.Cancel();calibration?.ResetCornerReference();calibrationBusy=false;Vision.CancelScan();CaptureSetupHeading();
   var eye=Camera.main;if(eye!=null)transform.SetPositionAndRotation(eye.transform.position+setupForward*.65f-Vector3.up*.40f,Quaternion.LookRotation(setupForward));
   Message="Align the board over your physical board, then select CONTINUE.";DrawScene();
  }
  public void ContinueEntry(){if(entryPhase!=1)return;entryPhase=2;BeginQuickCorners();Message=standardEntry&&!Playing?"Set four playable corners: near-left, near-right, far-right, far-left. A green outline prepares the standard pieces. Then select PLAY.":"Set four playable corners: near-left, near-right, far-right, far-left. Then select ACCEPT when the border turns green.";DrawScene();}
  public void AlignEntry(){if(entryPhase!=2)return;if(cornerSnapBusy){Message="Checking corners. Lift hands or cancel snapping through MENU.";return;}if(!CanAcceptCorners||!FinishQuickCorners())return;for(int i=0;i<4;i++)cornerLocked[i]=true;quickGridConfirmed=BoardPoseLocked=Aligned=true;calibration?.Lock();RecordRegistration();AnchorConfirmedPlane();if(entryCapture&&!Playing){entryPhase=0;entryKeepPhysical=false;quickPhase=1;if(setupPosition==null)BeginPositionScan();else{alignStage=8;Message="Review the captured pieces before sizing.";DrawScene();}return;}if(standardEntry&&!Playing){entryPhase=3;alignStage=0;quickPhase=2;quickCapture=false;HumanSide=1;TwoPlayers=false;Message="Standard chess is ready. You are White and move first. Select PLAY.";DrawScene();return;}entryPhase=4;quickPhase=2;BeginKingSizing();}
  public void PlayEntry(){
   if(entryPhase!=3)return;entryPhase=0;alignStage=0;quickCapture=false;Paused=false;Vision.ClearEvidence();
   if(Playing){Physical=entryWasPhysical;Recovering=false;Vision.ResetBoard();Message="Your saved game is ready. Move a piece to continue.";SaveSession("user accepted layout; gameplay resumed");DrawScene();return;}
   Physical=true;
   if(setupPosition!=null){
    // Preserve a previously captured draft; never replace unknown saved pieces with a standard board.
    quickCapture=true;quickPhase=3;alignStage=8;Message="Review your saved captured position before play.";DrawScene();return;
   }
   Match=new ChessMatch();pieceRegistry.Enroll(Match.Position);SessionId=GameSessionStore.NewId();Version++;Recovering=false;ManualFallback=false;PendingAi=null;Unverified=false;Vision.ResetBoard();SaveSession("new standard game; White to move");DrawScene();
  }
  void UpdateStandardEntry(){if(standardEntry&&!Playing&&entryPhase==2&&CanAcceptCorners)AlignEntry();}
  public bool CanAcceptCorners=>cornerCount==4&&!cornerSnapBusy&&editingSetupCorner<0&&OrderedFromView(setupCorners,setupRight,setupForward)&&CornerLayoutState(setupCorners,cornerCount)==1;
  bool DrawEntryUI(){if(entryPhase==0||standardMenu)return false;status.text=EntryInstructions;if(entryPhase==4){Add("PLAY",0,0,CompleteKingSizing,Purple,true);Add("KING +",-2,0,()=>ResizePieceSet(1.08f),Teal);Add("KING -",-1,0,()=>ResizePieceSet(1/1.08f),Teal);return true;}Add(entryPhase==1?"CONTINUE":entryPhase==2?(CanAcceptCorners?"ACCEPT":"ALIGN"):"PLAY",0,0,()=>{if(entryPhase==1)ContinueEntry();else if(entryPhase==2)AlignEntry();else PlayEntry();},entryPhase==2&&CanAcceptCorners?new Color(.12f,.85f,.3f):Purple,true);return true;}
  void DrawEntryGuide(){if(entryPhase!=3)return;var label=Label(board,"To adjust go to MENU / ALIGN or use these knobs",new Vector3(0,.045f,-Size*.5f-.075f),.0014f);label.transform.localRotation=Quaternion.Euler(90,0,0);}
  public void MenuAlign(){standardMenu=false;entryPhase=1;quickGridConfirmed=false;BoardPoseLocked=false;BeginEntry(false);}
 }
}
