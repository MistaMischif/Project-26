using System;
using UnityEngine;

namespace LivingTable.Chess
{
 public sealed partial class ChessTable
 {
  [Serializable] public sealed class SessionState
  {
   public string capturedOriginId;public ChessPieceRegistry pieceRegistry;public bool draft,captureManual,turnChosen;public int capturePhase;public ChessPosition capturedPosition;public bool[] unknownSquares;public BoardRegistration registration;
   public ChessMatch.State match;public bool physical,twoPlayers,manual,unverified,room,imported;
   public bool paused;public bool hasPending;public int humanSide,promotion;public ChessMatch.SavedMove pending;public float[] appearance;public float setScale;public float boardSize;public float[] pieceSizes;
  }
  public string SessionId {get;private set;}
  public string CapturedOriginId {get;private set;}
  public bool Unverified {get;private set;}
  public bool StorageBlocked {get;private set;}
  public bool VirtualRoom {get;private set;}
  bool imported,editing;ChessPosition importPosition;int brush=1;int savedIndex;
  GameSessionStore.Entry[] saved=Array.Empty<GameSessionStore.Entry>();
  public bool Editing=>editing;
  ChessPieceRegistry pieceRegistry=new ChessPieceRegistry();
  public SessionState CaptureSession()=>new SessionState{capturedOriginId=CapturedOriginId,pieceRegistry=pieceRegistry,draft=Match==null,capturePhase=quickPhase,capturedPosition=setupPosition,unknownSquares=unknownSquares,captureManual=setupManual,turnChosen=turnChosen,registration=registration,match=Match?.Capture(),paused=Paused,physical=Physical,twoPlayers=TwoPlayers,manual=ManualFallback,unverified=Unverified,room=VirtualRoom,imported=imported,humanSide=HumanSide,promotion=Promotion,hasPending=PendingAi.HasValue,pending=PendingAi.HasValue?new ChessMatch.SavedMove(PendingAi.Value):null,appearance=Vision.CaptureAppearance(),setScale=setScale,boardSize=Size,pieceSizes=pieceSizes};
  public bool SaveSession(string reason)
  {
   if(SessionId==null||Match==null&&!quickCapture)return true;
   try{GameSessionStore.Save(SessionId,"chess",reason,JsonUtility.ToJson(CaptureSession()));StorageBlocked=false;return true;}
   catch(Exception e){StorageBlocked=true;Paused=true;Message="Session save failed. Play is locked. "+e.Message;Debug.LogError("[Session] "+e.Message);if(ui!=null)DrawUI();return false;}
  }
  bool SaveCaptureOrigin(){
   if(CapturedOriginId!=null)return true;
   try{CapturedOriginId=GameSessionStore.NewId();GameSessionStore.Save(CapturedOriginId,"chess","original physical capture",JsonUtility.ToJson(CaptureSession()));return true;}
   catch(Exception e){CapturedOriginId=null;StorageBlocked=Paused=true;Message="Cannot preserve captured position: "+e.Message;DrawUI();return false;}
  }
  public void RestartFromCapture(){
   if(CapturedOriginId==null||Busy||StorageBlocked||!SaveSession("experiment saved before restart"))return;
   string origin=CapturedOriginId;ResumeSession(origin);if(SessionId!=origin)return;
   SessionId=GameSessionStore.NewId();CapturedOriginId=origin;Paused=false;SaveSession("new experiment from original capture");DrawScene();
  }
  void RefreshSaved(){saved=GameSessionStore.List("chess");savedIndex=0;}
  public void ResumeSession(string id)
  {
   try {
    var record=GameSessionStore.Load(id);if(record.game!="chess")throw new ArgumentException("Wrong game");var state=JsonUtility.FromJson<SessionState>(record.payload);
    CapturedOriginId=state.capturedOriginId;BoardPoseLocked=false;if(state.draft){ResumeCapture(state,id);return;}registration=state.registration;quickCapture=false;quickPhase=0;
    var restored=ChessMatch.Restore(state.match);if(state.humanSide!=1&&state.humanSide!=-1||state.promotion<2||state.promotion>5)throw new ArgumentException("Invalid saved settings");
    if(state.hasPending&&(state.pending==null||!restored.Position.LegalMoves().Contains(state.pending.Move)))throw new ArgumentException("Invalid pending AI move");
    HomeScreen=false;setScale=state.setScale>0?Mathf.Clamp(state.setScale,.3f,3):1;StopAllCoroutines();CancelGrab();Version++;Match=restored;pieceRegistry=state.pieceRegistry??new ChessPieceRegistry();pieceRegistry.Enroll(restored.Position);SessionId=id;Physical=state.physical;TwoPlayers=state.twoPlayers;HumanSide=state.humanSide;Promotion=state.promotion;ManualFallback=state.manual;Unverified=state.unverified;VirtualRoom=state.room;imported=state.imported;PendingAi=state.hasPending?(ChessMove?)state.pending.Move:null;
    if(state.boardSize>=.2f&&state.boardSize<=1.2f)Size=state.boardSize;pieceSizes=state.pieceSizes!=null&&state.pieceSizes.Length==24?state.pieceSizes:new float[24];alignStage=0;calibrationBusy=false;Busy=buttonBusy=false;sessionMenu=false;Paused=state.paused;StorageBlocked=false;editing=false;selected=-1;Aligned=!Physical;Recovering=Physical;Vision.RestoreAppearance(state.appearance);
    Message=Physical?"Align the real board, then reconcile the saved position.":"Session restored. Same position and turn history.";ApplyRoom();DrawScene();nextAi=Time.unscaledTime+1;
   }catch(Exception e){Message="Cannot resume: "+e.Message;}
  }
  public void SwitchOpponent()
  {
   if(!Playing||Busy||StorageBlocked)return;CancelGrab();Version++;TwoPlayers=!TwoPlayers;
   Message=PendingAi.HasValue?"Finish the saved AI move before the new controller takes over.":TwoPlayers?"Human opponent returns. Position and history retained.":"AI takes over the opponent's seat.";
   SaveSession("opponent changed; pending move retained");DrawUI();
  }
  public void SwitchSurface(){if(!Playing||Busy||StorageBlocked)return;if(Physical){RequestVirtualSafety(CompleteSurfaceSwitch);return;}CompleteSurfaceSwitch();}
  void CompleteSurfaceSwitch()
  {
   if(!Playing||Busy||StorageBlocked)return;CancelGrab();Version++;Physical=!Physical;selected=-1;
   if(Physical){VirtualRoom=false;Aligned=true;BoardPoseLocked=true;Recovering=false;Paused=false;alignStage=0;entryPhase=0;quickCapture=false;calibration?.Cancel();calibrationBusy=false;Vision.CancelScan();Unverified=true;Message="Real board ready. Match your physical pieces to the shown position and continue.";}
   else{Recovering=false;Vision.ClearEvidence();Message="Continuing the same game on the virtual board.";}
   sessionMenu=standardMenu=false;SaveSession("surface changed; position retained; physical placement user-managed");ApplyRoom();DrawScene();
   if(!Physical&&PendingAi.HasValue&&!Paused&&!SessionMenuOpen)Play(PendingAi.Value);
  }
  public bool AcceptRecovery(int version,bool identityVerified=false)
  {
   if(version!=Version||!Recovering||!Aligned||!HandsClear||StorageBlocked||Paused||SessionMenuOpen||!Physical||Vision.RequiresIdentity&&!identityVerified)return false;
   Recovering=false;if(identityVerified)ManualFallback=false;Version++;Message=identityVerified?"Saved position verified. Continue the same game.":"Board appearance matches the saved checkpoint.";bool savedOk=SaveSession(identityVerified?"full board identity reconciliation":"camera appearance reconciliation");DrawScene();return savedOk;
  }
  public void AttestRecovery()
  {
   if(!Recovering||!Aligned||StorageBlocked)return;
   Unverified=true;ManualFallback=true;Recovering=false;Version++;Vision.ResetBoard();Message="Manual board check recorded. This is not camera verification.";
   SaveSession("manual board reconciliation; unverified");DrawScene();
  }
  void ToggleRoom(){if(Physical||StorageBlocked)return;if(!VirtualRoom){RequestVirtualSafety(()=>{VirtualRoom=true;ApplyRoom();SaveSession("view changed");DrawUI();});return;}VirtualRoom=false;ApplyRoom();SaveSession("view changed");DrawUI();}
  void ApplyRoom()=>FindFirstObjectByType<LivingTableExperience>()?.SetVirtualRoom(VirtualRoom&&!Physical);
  void BeginImport(){if(Playing)return;editing=true;importPosition=ChessPosition.Initial();importPosition.Rights=0;brush=1;Message="Match the real position. Set turn and castling rights. Earlier history is unknown.";DrawScene();}
  public bool ImportPosition(ChessPosition position)
  {
   if(Playing)return false;
   try{ChessMatch.ValidatePosition(position);if(Physical&&!Aligned){Message="Align the real board first.";return false;}
    Match=new ChessMatch(position.Copy());pieceRegistry.Enroll(Match.Position);if(!quickCapture||SessionId==null)SessionId=GameSessionStore.NewId();Unverified=imported=true;StorageBlocked=false;editing=false;Version++;PendingAi=null;Busy=false;Paused=false;Recovering=Physical;Vision.ResetBoard();SaveSession("imported position; earlier history not verified");DrawScene();Message="Imported position saved. Earlier history is not verified.";return true;
   }catch(Exception e){Message=e.Message;return false;}
  }
  void SessionUI()
  {
   var purple=new Color(.55f,.1f,.8f);
   if(editing){Add("USE POSITION",0,0,()=>ImportPosition(importPosition),purple,true);Add("PIECE "+new[]{"ERASE","PAWN","KNIGHT","BISHOP","ROOK","QUEEN","KING"}[Math.Abs(brush)],-2,0,()=>{brush=(Math.Abs(brush)+1)%7*(brush<0?-1:1);DrawUI();},purple);Add(brush<0?"BLACK PIECE":"WHITE PIECE",2,0,()=>{brush=brush==0?-1:-brush;DrawUI();},purple);Add(importPosition.Side==1?"WHITE TURN":"BLACK TURN",-2,1,()=>{importPosition.Side=-importPosition.Side;DrawUI();},purple);Add("CLEAR",-1,1,()=>{Array.Clear(importPosition.Board,0,64);importPosition.Rights=0;DrawScene();},purple);Add("CANCEL",2,1,()=>{editing=false;DrawScene();},purple);
    string[] rights={"WHITE O-O","WHITE O-O-O","BLACK O-O","BLACK O-O-O"};for(int i=0;i<4;i++){int bit=i;Add(rights[i]+(((importPosition.Rights>>i)&1)!=0?" YES":" NO"),i-2,2,()=>{importPosition.Rights^=1<<bit;DrawUI();},purple);}Add("EP "+(importPosition.Ep<0?"NONE":ChessMove.Square(importPosition.Ep)),2,2,()=>{int rank=importPosition.Side==1?5:2;int file=importPosition.Ep<0?0:importPosition.Ep%8+1;importPosition.Ep=file>7?-1:rank*8+file;DrawUI();},purple);if(Physical)Add("ALIGN BOARD",0,3,AlignBoard,purple);return;}
   if(!Playing){Add("IMPORT GAME",-1,1,BeginImport,purple);if(saved.Length>0){Add("SAVE "+(savedIndex+1)+" / "+saved.Length,1,1,()=>{savedIndex=(savedIndex+1)%saved.Length;Message="Saved "+saved[savedIndex].utc;DrawUI();},purple);}return;}
   Add(TwoPlayers?"AI TAKE OVER":"HUMAN RETURNS",-2,2,SwitchOpponent,purple);Add(Physical?"GO VIRTUAL":"USE REAL BOARD",0,2,SwitchSurface,purple);
   if(!Physical)Add(VirtualRoom?"SEE ROOM":"VIRTUAL ROOM",2,2,ToggleRoom,purple);
   if(Physical&&(!Aligned||Recovering)){Add("ALIGN BOARD",-2,3,AlignBoard,purple);if(Aligned)Add("MANUAL CHECK",0,3,AttestRecovery,new Color(.85f,.4f,.05f));}
   if(StorageBlocked)Add("RETRY SAVE",0,4,()=>{if(SaveSession("save retry")){DrawScene();Message="Saved. Resume to continue.";}},purple);
  }
 }
}
