using System;
using UnityEngine;

namespace LivingTable.Chess
{
 public sealed partial class ChessTable
 {
  bool sessionMenu;
  public bool SessionMenuOpen=>sessionMenu;
  static readonly Color Purple=new Color(.55f,.1f,.8f), Teal=new Color(.1f,.6f,.7f), Orange=new Color(.9f,.42f,.05f);
  public static string SideName(int side)=>side>0?"White":"Black";
  public static string PieceName(int piece)=>new[]{"empty","pawn","knight","bishop","rook","queen","king"}[Math.Abs(piece)];
  public static string MoveInstruction(ChessPosition position,ChessMove move)
  {
   int piece=position.Board[move.From];
   string text=SideName(piece)+" "+PieceName(piece)+": "+ChessMove.Square(move.From)+" to "+ChessMove.Square(move.To)+".";
   if(Math.Abs(piece)==6&&Math.Abs(move.To-move.From)==2){int rookFrom=move.To>move.From?move.From+3:move.From-4;int rookTo=move.To>move.From?move.From+1:move.From-1;text+=" Rook: "+ChessMove.Square(rookFrom)+" to "+ChessMove.Square(rookTo)+".";}
   else if(move.EnPassant)text+=" Remove pawn on "+ChessMove.Square(move.To-position.Side*8)+".";
   else if(position.Board[move.To]!=0)text+=" Remove the captured "+PieceName(position.Board[move.To])+".";
   if(move.Promotion!=0)text+=" Replace pawn with "+PieceName(move.Promotion)+".";
   return text;
  }
  public string TurnGuidance
  {
   get {
    if(StorageBlocked)return Message;
    if(relocating)return "Board held. Move or turn, then release to place.";
    if(!Playing)return Message;
    if(GameOver)return sessionMenu?"Game complete. PLAY AGAIN starts a fresh game with the same settings.":"Game complete. Press PLAY AGAIN for a new game.";
    if(Paused)return "Game paused. Your position is saved.";
    if(Recovering)return !Aligned?"1 / 2 - Align the real board. Game held.":"2 / 2 - Match the guide pieces. "+Vision.Status;
    if(sessionMenu)return "Game held while you choose. Same position and history.";
    if(PendingAi.HasValue)return "Move for AI: "+MoveInstruction(Match.Position,PendingAi.Value)+(Physical?" Lift your hands when finished.":"");
    if(Busy)return "Moving...";
    if(ai!=null)return "AI is thinking...";
    if(Physical)return ManualFallback?"Manual fallback: move real pieces, then tap source and destination.":Vision.Status;
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
   if(ui!=null){ui.gameObject.SetActive(false);Destroy(ui.gameObject);}ui=Node("Chess controls");buttons.Clear();
   var p=new GameObject("Chess status");p.transform.SetParent(ui,false);p.transform.localPosition=new Vector3(0,.23f,Size*.5f+.02f);panel=p.AddComponent<TableFacingPanel>();status=Label(p.transform,"",GameOver?new Vector3(0,-.115f,0):Vector3.zero,.0026f);p.AddComponent<TableCabinetVisual>().Panel(GameOver?.72f:companionOpen?.65f:.50f,GameOver?.34f:companionOpen?.36f:.21f,false);status.color=new Color(.10f,.04f,.008f);
   if(GameOver){var result=Label(p.transform,Match.Result.Replace(" wins by "," wins\nby ").Replace("Draw \u2014 ","Draw\n"),new Vector3(0,.06f,-.003f),.0026f*4);result.name="Game result";result.fontStyle=FontStyle.Bold;result.color=status.color;}
   if(editing){SessionUI();return;}
   if(!Playing){
    if(saved.Length>0){Add("RESUME GAME",0,0,()=>ResumeSession(saved[savedIndex].id),Purple,true);Add("NEW GAME",2,1,StartGame,Purple);}else Add("PLAY NOW",0,0,StartGame,Purple,true);Add(Physical?"REAL BOARD":"VIRTUAL BOARD",-2,0,()=>Configure(!Physical,TwoPlayers,HumanSide),Purple);Add(TwoPlayers?"HUMAN OPPONENT":"AI OPPONENT",2,0,()=>Configure(Physical,!TwoPlayers,HumanSide),Purple);
    Add("YOU: "+SideName(HumanSide).ToUpperInvariant(),-2,1,()=>Configure(Physical,TwoPlayers,-HumanSide),Purple);
    if(Physical)Add("ALIGN BOARD",0,1,AlignBoard,Teal);
    if(FindFirstObjectByType<LivingTableExperience>()?.chessOnlyMode!=true)Add("CANDY LAND",2,2,()=>FindFirstObjectByType<LivingTableExperience>()?.SelectCandy(),Purple);
    SessionUI();return;
   }
   if(StorageBlocked){Add("RETRY SAVE",0,0,()=>{if(SaveSession("save retry")){Message="Saved. Resume to continue.";DrawUI();}},Orange,true);return;}
   if(companionOpen){Add("WHAT HAPPENED",-2,0,()=>AskCompanion("what"),Teal);Add("EXPLAIN MOVE",0,0,()=>AskCompanion("explain"),Purple,true);Add("HELP CONTINUE",2,0,()=>AskCompanion("continue"),Teal);Add("BACK TO GAME",0,1,CloseCompanion,Orange);Add("CHECK CONNECTION",2,1,CheckCompanionConnection,Teal);return;}
   if(GameOver){
    Add("PLAY AGAIN",0,0,PlayAgain,new Color(.05f,.8f,.3f),true);
    Add(sessionMenu?"BACK":"MENU",-2,0,ToggleSessionMenu,Purple);
    if(sessionMenu){Add("GAME SETTINGS",2,0,NewGame,Purple);Add("MOVE BOARD",0,1,()=>FindFirstObjectByType<LivingTableExperience>()?.MoveCandyTable(),Teal);}
    return;
   }
   if(sessionMenu){
    Add("BACK TO GAME",0,0,ToggleSessionMenu,Teal,true);
    Add(TwoPlayers?"AI TAKE OVER":"HUMAN RETURNS",-2,0,SwitchOpponent,Purple);Add(Physical?"GO VIRTUAL":"USE REAL BOARD",2,0,SwitchSurface,Purple);
    Add("SAVE AND EXIT",-2,1,NewGame,Orange);Add("MOVE BOARD",-1,1,()=>FindFirstObjectByType<LivingTableExperience>()?.MoveCandyTable(),Teal);
    if(!Physical)Add(VirtualRoom?"SEE ROOM":"VIRTUAL ROOM",1,1,ToggleRoom,Purple);
    else if(!Recovering)Add(ManualFallback?"USE CAMERA":"MANUAL MODE",1,1,ToggleManual,Purple);
    Add("PROMOTE: "+PieceName(Promotion).ToUpperInvariant(),2,1,()=>{Promotion=Promotion==5?4:Promotion==4?3:Promotion==3?2:5;Version++;SaveSession("promotion preference changed");DrawUI();},Purple);
    return;
   }
   Add("ASK THE TABLE",0,2,OpenCompanion,Teal);
   Add("MENU",-2,0,ToggleSessionMenu,Purple);Add(Paused?"RESUME":"PAUSE",2,0,TogglePause,Orange);
   if(Recovering){Add(Aligned?"SCAN BOARD":"ALIGN BOARD",0,0,()=>{if(Aligned)Vision.RetryRecovery();else AlignBoard();},Teal,true);if(Aligned){Add("MANUAL CHECK",0,1,AttestRecovery,Orange);Add("ALIGN AGAIN",-2,1,AlignBoard,Teal);}}
   else if(Physical&&ManualFallback&&PendingAi.HasValue)Add("MOVED IT",0,0,()=>Play(PendingAi.Value),new Color(.05f,.8f,.3f),true);
   else if(Physical&&!ManualFallback)Add("CAMERA HELP",0,0,()=>{Message="Use MENU then MANUAL MODE if camera cannot see your move.";ToggleSessionMenu();},Teal);
   if(Match.CanClaim)Add("CLAIM DRAW",0,1,()=>{if(!Paused&&!Busy&&Match.CanClaim){Match.Claim();Version++;SaveSession("draw claimed");DrawUI();}},Teal);
  }
 }
}
