using System;
using UnityEngine;

namespace LivingTable.Chess
{
 public sealed partial class ChessTable
 {
  bool sessionMenu;
  public bool SessionMenuOpen=>virtualSafety!=null||sessionMenu||extraSettings||pieceSettings||standardMenu;
  static readonly Color UndoPink=new Color(1,.28f,.62f);
  static readonly Color Purple=new Color(.55f,.1f,.8f), Teal=new Color(.1f,.6f,.7f), Orange=new Color(.95f,.23f,.07f);
  public static string SideName(int side)=>side>0?"White":"Black";
  public static string PieceName(int piece)=>new[]{"empty","pawn","knight","bishop","rook","queen","king"}[Math.Abs(piece)];
  public static string MoveInstruction(ChessPosition position,ChessMove move)
  {
   int piece=position.Board[move.From];
   string text=SideName(piece)+" "+PieceName(piece)+": "+ChessMove.Square(move.From)+" to "+ChessMove.Square(move.To)+".";
   if(Math.Abs(piece)==6&&Math.Abs(move.To-move.From)==2){int rookFrom=move.To>move.From?move.From+3:move.From-4;int rookTo=move.To>move.From?move.From+1:move.From-1;text+=" Rook: "+ChessMove.Square(rookFrom)+" to "+ChessMove.Square(rookTo)+".";}
   else if(move.EnPassant)text+=" Remove pawn on "+ChessMove.Square(move.To-position.Side*8)+".";
   else if(position.Board[move.To]!=0)text+=" Place the captured "+SideName(position.Board[move.To])+" "+PieceName(position.Board[move.To])+" in the "+(position.Board[move.To]>0?"left":"right")+" gold capture-grid slot.";
   if(move.Promotion!=0)text+=" Replace pawn with "+PieceName(move.Promotion)+".";
   return text;
  }
  public string TurnGuidance
  {
   get {
    if(StorageBlocked||boardNeedsMove)return Message;
    if(relocating)return "Board held. Move or turn, then release to place.";
    if(!Playing)return Message;
    if(GameOver)return sessionMenu?"Game complete. PLAY AGAIN starts a fresh game with the same settings.":"Game complete. Press PLAY AGAIN for a new game.";
    if(Paused)return "Game paused. Your position is saved.";
    if(Recovering)return !Aligned?"1 / 2 - Align the real board. Game held.":"2 / 2 - Match the guide pieces. "+Vision.Status;
    if(SessionMenuOpen)return "Game held while you choose. Same position and history.";
    if(Physical&&Time.unscaledTime<capturePromptUntil)return captureInstruction;
    if(PendingAi.HasValue)return "Move for AI: "+MoveInstruction(Match.Position,PendingAi.Value)+(Physical?" Press MOVED IT when placed.":"");
    if(AwaitingDone)return (Physical?"Move your physical piece to the virtual destination, then press MOVED IT.":"Happy with your move? Press DONE, or pick up the piece to adjust it.");
    if(VisualMovePreview&&automaticPreview)return "Following your physical move. Verifying identity before the next turn. "+Vision.Status;
    if(AwaitingMoveAcceptance)return "Check the preview. ACCEPT MOVE saves it and starts the opponent turn. "+Vision.Status;
    if(Busy)return "Moving...";
    if(ai!=null)return "AI is thinking...";
    if(Physical)return ManualFallback?"Manual fallback: move real pieces, then tap source and destination.":"Move your piece on the virtual board to match the physical move, then press MOVED IT.";
    return (TwoPlayers?SideName(Match.Position.Side)+" plays.":"Your move.")+" Pinch a piece and place it on a glowing square.";
   }
  }
  public void ToggleSessionMenu()
  {
   if(!Playing||Busy||StorageBlocked)return;
   sessionMenu=!sessionMenu;Version++;CancelGrab();Vision.ClearEvidence();
   SaveSession(sessionMenu?"handoff controls opened":"handoff controls closed");DrawUI();
  }
  void AlignBoard(){BeginAlignment();}
  void DrawHandoffUI()
  {
   if(panel!=null){panel.gameObject.SetActive(false);Destroy(panel.gameObject);panel=null;}panelMenu=null;panelSlider=null;sliderKnob=null;
   if(ui!=null){ui.gameObject.SetActive(false);Destroy(ui.gameObject);}ui=Node("Chess controls");buttons.Clear();leftButtons=rightButtons=0;
   bool guidance=entryPhase==0&&CriticalInstructions;var p=new GameObject(guidance?"Floating setup instructions":"Chess status");p.transform.SetParent(ui,false);p.transform.localPosition=new Vector3(0,.23f,Size*.5f+.02f);panel=p.AddComponent<TableFacingPanel>();status=Label(p.transform,"",guidance?new Vector3(0,.005f,-.015f):GameOver?new Vector3(0,-.115f,-.015f):Playing?new Vector3(0,-.05f,-.015f):Vector3.zero,guidance&&!standardMenu?.0022f:.0026f);
   if(guidance){if(instructionMaterial==null)instructionMaterial=Mat(new Color(.025f,.035f,.06f));Shape(p.transform,"Floating instruction card",PrimitiveType.Cube,new Vector3(0,0,.005f),new Vector3(.52f,VirtualSafetyOpen?.30f:standardMenu?.40f:AlignmentMessageView?.30f:.15f,.009f),instructionMaterial);status.color=Color.white;}
   else{p.AddComponent<TableCabinetVisual>().Panel(GameOver?.72f:companionOpen?.65f:Playing?.72f:.50f,GameOver?.34f:companionOpen?.36f:Playing?.34f:.21f,false);status.color=new Color(.10f,.04f,.008f);}
   if(GameOver&&entryPhase==0){var result=Label(p.transform,Match.Result.Replace(" wins by "," wins\nby ").Replace("Draw \u2014 ","Draw\n"),new Vector3(0,.06f,-.015f),.0026f*4);result.name="Game result";result.fontStyle=FontStyle.Bold;result.color=status.color;}
   turnHeading=null;if(entryPhase==0&&Playing&&!GameOver&&!companionOpen&&!guidance){turnHeading=Label(p.transform,TurnTitle,new Vector3(0,.10f,-.015f),.008f);turnHeading.fontStyle=FontStyle.Bold;turnHeading.color=status.color;turnHeading.name="Persistent turn heading";}
   ConfigureBackPanel(p.transform);if(entryPhase==0||standardMenu){DrawPanelMenu(p.transform);DrawScanSpinner(p.transform);DrawAlignmentMessages(p.transform);}
   DrawMainMenuButton();
   if(DrawEntryUI())return;
   if(SetupUI())return;
   if(HomeScreen){
    status.text="LIVING TABLE: FIRST STEPS\n\nby COLTRAN STUDIOS";status.fontStyle=FontStyle.Bold;status.characterSize=.0035f;
    Add("START",0,0,ChooseNewGame,Purple,true);return;
   }
   if(CalibrationUI())return;
   if(editing){SessionUI();return;}
   if(StorageBlocked){Add("RETRY SAVE",0,0,()=>{if(SaveSession("save retry")){Message="Saved. Resume to continue.";DrawUI();}},Teal,true);return;}
   if(pieceSettings){Add("BACK",0,0,()=>{pieceSettings=false;DrawUI();},Teal,true);Add("PIECES +",-2,0,()=>ResizePieceSet(1.1f),Teal);Add("PIECES -",-1,0,()=>ResizePieceSet(1/1.1f),Teal);if(Playing)Add("MORE",-2,1,()=>{pieceSettings=false;sessionMenu=false;extraSettings=true;DrawUI();},Teal);Add("HOME",-1,1,ShowHome,Teal);return;}
   if(!Playing){
    Add(Physical&&!Aligned?(BoardPoseLocked?"MOVE BOARD":"ALIGN BOARD"):"START GAME",0,0,()=>{if(Physical&&!Aligned)BeginAlignment();else StartGame();},Teal,true);
    Add(Physical?"GO VIRTUAL":"REAL BOARD",-2,0,()=>{Configure(!Physical,TwoPlayers,HumanSide);if(Physical)BeginAlignment();},Teal);
    Add("OPTIONS",-1,0,()=>{pieceSettings=true;DrawUI();},Teal);
    Add("HOME",-2,1,ShowHome,Teal);
    Add(TwoPlayers?"USE AI":"USE HUMAN",2,0,()=>Configure(Physical,!TwoPlayers,HumanSide),TwoPlayers?Orange:Teal);
    if(FindFirstObjectByType<LivingTableExperience>()?.chessOnlyMode!=true)Add("CANDY LAND",2,0,()=>FindFirstObjectByType<LivingTableExperience>()?.SelectCandy(),Orange);
    return;
   }
   if(companionOpen){
    Add("BACK TO GAME",0,0,CloseCompanion,Teal,true);Add("WHAT HAPPENED",2,0,()=>AskCompanion("what"),Orange);Add("EXPLAIN MOVE",1,0,()=>AskCompanion("explain"),Orange);Add("HELP CONTINUE",2,1,()=>AskCompanion("continue"),Orange);Add("RECONNECT",1,1,CheckCompanionConnection,Orange);return;
   }
   if(GameOver){Add("PLAY AGAIN",0,0,PlayAgain,Teal,true);Add("HOME",-2,0,ShowHome,Teal);Add("NEW GAME",-1,0,ChooseNewGame,Teal);Add("ASK META",2,0,OpenCompanion,Orange);if(CanUndoLastMove)Add("UNDO LAST MOVE",2,1,Undo,UndoPink);return;}
   if(sessionMenu){
    Add("BACK TO GAME",0,0,ToggleSessionMenu,Teal,true);
    Add(TwoPlayers?"AI TAKE OVER":"HUMAN RETURNS",2,0,SwitchOpponent,TwoPlayers?Orange:Teal);
    Add(Physical?"GO VIRTUAL":"REAL BOARD",-2,0,SwitchSurface,Teal);
    Add("MOVE BOARD",-2,1,()=>FindFirstObjectByType<LivingTableExperience>()?.MoveCandyTable(),Teal);
    if(CapturedOriginId!=null)Add("RESTART CAPTURE",1,1,RestartFromCapture,Orange);Add("OPTIONS",-1,1,()=>{pieceSettings=true;DrawUI();},Teal);Add("ASK META",2,1,OpenCompanion,Orange);return;
   }
   if(extraSettings){
    Add("BACK TO GAME",0,0,()=>{extraSettings=false;DrawUI();},Teal,true);
    Add("PROMOTE: "+PieceName(Promotion).ToUpperInvariant(),-2,0,()=>{Promotion=Promotion==5?4:Promotion==4?3:Promotion==3?2:5;SaveSession("promotion changed");DrawUI();},Teal);
    if(Physical){Add(BoardPoseLocked?"MOVE BOARD":"ALIGN BOARD",-1,0,()=>{extraSettings=false;BeginAlignment();},Teal);if(!Recovering)Add(ManualFallback?"USE CAMERA":"MANUAL MODE",-2,1,ToggleManual,Teal);}
    else Add(VirtualRoom?"SEE ROOM":"VIRTUAL ROOM",-1,0,ToggleRoom,Teal);
    if(Match.CanClaim)Add("CLAIM DRAW",-2,1,()=>{Match.Claim();SaveSession("draw claimed");DrawUI();},Teal);return;
   }
   Add("GAME OPTIONS",-2,0,ToggleSessionMenu,Teal);Add("ASK META",2,0,OpenCompanion,Orange);if(CanUndoLastMove)Add("UNDO LAST MOVE",2,1,Undo,UndoPink);
   if(Paused){Add("RESUME",0,0,TogglePause,Teal,true);return;}
   Add("PAUSE",-1,0,TogglePause,Teal);
   if(Recovering){Add(!Aligned?(BoardPoseLocked?"MOVE BOARD":"ALIGN BOARD"):Vision.Checking?"CANCEL SYNC":"SYNC PIECES",0,0,()=>{if(!Aligned)AlignBoard();else if(Vision.Checking)Vision.CancelScan();else Vision.CheckNow();},Purple,true);if(Aligned)Add("MANUAL CHECK",-2,1,AttestRecovery,Teal);}
   else if(AwaitingDone)Add(Physical?"MOVED IT":"DONE",0,0,()=>ConfirmPlayerMove(),Physical?new Color(.12f,.85f,.3f):Purple,true);
   else if(Physical&&PendingAi.HasValue)Add("MOVED IT",0,0,()=>ConfirmPhysicalAiMove(),new Color(.12f,.85f,.3f),true);
   else if(Physical&&AwaitingMoveAcceptance)Add("ACCEPT MOVE",0,0,()=>Vision.ConfirmHuman(),Purple,true);
   else if(Physical)Add(PendingAi.HasValue?"MOVE AI PIECE":"YOUR MOVE",0,0,()=>{if(PendingAi.HasValue){Message=MoveInstruction(Match.Position,PendingAi.Value);Highlights();}else ShowAvailableMoves();},Purple,true);
   else Add("SHOW MOVES",0,0,ShowAvailableMoves,Teal,true);
  }
  bool extraSettings,pieceSettings;
  void ShowAvailableMoves(){if(!Playing||Paused||Busy||Recovering||GameOver)return;var moves=Match.Position.LegalMoves();if(!TwoPlayers&&Match.Position.Side!=HumanSide){Message="Your opponent is thinking.";return;}if(moves.Count==0)return;selected=moves[0].From;Highlights();Message="Pinch a piece; its legal destinations glow.";}
 }
}
