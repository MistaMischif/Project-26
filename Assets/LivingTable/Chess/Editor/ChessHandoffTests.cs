#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using LivingTable.Chess;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace LivingTable.Editor
{
 public static class ChessHandoffTests
 {
  static void Check(bool value,string message){if(!value)throw new Exception("HANDOFF TEST: "+message);}
  static ChessMove Move(ChessPosition p,int from,int to)=>p.LegalMoves().Single(m=>m.From==from&&m.To==to);
  public static void Run()
  {
   ChessBuilder.RulesTests();ChessContestRelease.Prepare();
   var castle=ChessPosition.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1");
   Check(ChessTable.MoveInstruction(castle,Move(castle,4,6)).Contains("Rook: h1 to f1"),"Castling explains second piece");
   var ep=ChessPosition.FromFen("4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1");
   Check(ChessTable.MoveInstruction(ep,ep.LegalMoves().Single(m=>m.EnPassant)).Contains("Remove pawn on d5"),"En passant explains removal");
   var promotion=ChessPosition.FromFen("4k3/P7/8/8/8/8/8/4K3 w - - 0 1");
   Check(ChessTable.MoveInstruction(promotion,promotion.LegalMoves().Single(m=>m.Promotion==5)).Contains("Replace pawn with queen"),"Promotion explains replacement");
   var capture=ChessPosition.FromFen("4k3/8/8/8/8/3p4/4P3/4K3 w - - 0 1");
   Check(ChessTable.MoveInstruction(capture,Move(capture,12,19)).Contains("Remove the captured pawn"),"Capture explains removal");
   SessionState.SetString("ChessHandoffRoot",Path.GetFullPath("Logs/handoff-tests-"+Guid.NewGuid().ToString("N")));
   EditorSceneManager.OpenScene(ChessContestRelease.DesktopScene);SessionState.SetBool("ChessHandoffSmoke",true);EditorApplication.EnterPlaymode();
  }
  static int stage;static double deadline;static ChessTable table;static string id;
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("ChessHandoffSmoke",false))return;GameSessionStore.TestRoot=SessionState.GetString("ChessHandoffRoot","");stage=0;deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;}
  static void Tick()
  {
   try {
    if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Handoff scene timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
    if(stage==0){
     table=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(table==null||table.Vision==null)return;
     Check(UnityEngine.Object.FindFirstObjectByType<LivingTable.CandyLand.CandyTable>()==null,"Contest boots chess directly without Candy objects");
     Check(!table.GetComponentsInChildren<TableButtonVisual>().Any(b=>b.name=="CANDY LAND button"),"Contest has no Candy navigation");
     Check(table.VisiblePieceCount==32,"Contest starts with 32 chess pieces");table.Configure(false,true,1);table.StartGame();id=table.SessionId;
     table.ToggleSessionMenu();Check(!table.Play(Move(table.Match.Position,12,28)),"Handoff menu holds play");table.SwitchOpponent();Check(!table.TwoPlayers&&table.SessionId==id&&table.Match.Position.Ply==0,"AI takeover retains session");table.SwitchOpponent();table.ToggleSessionMenu();
     table.Vision.RestoreAppearance(new float[768]);Check(table.Play(Move(table.Match.Position,12,28)),"Virtual move accepted");Check(!table.Vision.Ready,"Virtual move invalidates old physical evidence");stage=1;
    } else if(stage==1){
     if(table.Busy)return;table.ToggleSessionMenu();table.SwitchSurface();Check(table.Physical&&table.Recovering&&!table.Aligned,"Physical transition requires reconciliation");Check(!table.Play(Move(table.Match.Position,52,36)),"Cannot move during reconciliation");table.SwitchSurface();table.ToggleSessionMenu();
     table.TogglePause();table.SaveSession("test paused checkpoint");table.NewGame();table.ResumeSession(id);Check(table.Paused&&table.Match.Position.Ply==1,"Paused checkpoint survives restart");table.NewGame();
     var m=new ChessMatch();m.Play(Move(m.Position,12,28));var state=new ChessTable.SessionState{match=m.Capture(),physical=true,twoPlayers=false,humanSide=1,promotion=5,hasPending=true,pending=new ChessMatch.SavedMove(Move(m.Position,52,36)),appearance=new float[768]};
     id=GameSessionStore.NewId();GameSessionStore.Save(id,"chess","pending test",JsonUtility.ToJson(state));table.ResumeSession(id);Check(table.PendingAi.HasValue,"Pending move survives relaunch");table.SwitchOpponent();Check(table.PendingAi.HasValue&&table.TwoPlayers,"Human return retains locked AI move");table.TogglePause();table.SwitchSurface();Check(table.PendingAi.HasValue&&table.Match.Position.Ply==1,"Paused virtual handoff does not advance");table.TogglePause();Check(!table.Play(Move(table.Match.Position,51,35)),"Wrong replacement move rejected");stage=2;
    } else if(stage==2){
     if(table.Busy||table.Match.Position.Ply<2)return;
     Check(table.Match.Position.Board[36]==-1&&!table.PendingAi.HasValue&&table.TwoPlayers,"Exact pending AI move plays once then human takes over");
     Check(table.Match.Position.Ply==2,"No duplicate move");table.NewGame();table.ResumeSession(id);Check(table.Match.Position.Ply==2&&!table.PendingAi.HasValue,"Executed proposal is durably consumed");
     table.ToggleSessionMenu();SaveView();Finish(0,"CHESS_HANDOFF_TESTS_PASSED");
    }
   }catch(Exception e){Finish(1,e.ToString());}
  }
  static void SaveView(){var camera=Camera.main;camera.transform.position=new Vector3(.1f,1.55f,-.78f);camera.transform.LookAt(new Vector3(0,.8f,-.05f));table.SendMessage("Update");foreach(var panel in table.GetComponentsInChildren<TableFacingPanel>())panel.FaceViewer();var rt=new RenderTexture(1600,1200,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,1200,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1200),0,0);image.Apply();File.WriteAllBytes("Logs/Chess-Handoff.png",image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
  static void Finish(int code,string text){SessionState.SetBool("ChessHandoffSmoke",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(text);else Debug.LogError(text);EditorApplication.Exit(code);}
 }
}
#endif
