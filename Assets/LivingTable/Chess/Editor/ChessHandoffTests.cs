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
  public static void RunWithCompanion(){SessionState.SetBool("ChessLiveCompanion",true);Run();}
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
     Check(table.HomeScreen&&table.VisiblePieceCount==0,"Contest starts at branded home menu");table.Configure(false,true,1);table.StartGame();id=table.SessionId;
     Check(LivingTableExperience.HandJoint(OVRSkeleton.SkeletonType.XRHandLeft,0)==OVRSkeleton.BoneId.XRHand_ThumbTip&&LivingTableExperience.HandJoint(OVRSkeleton.SkeletonType.XRHandRight,1)==OVRSkeleton.BoneId.XRHand_IndexTip,"OpenXR tips use OpenXR IDs");
     Check(LivingTableExperience.HandJoint(OVRSkeleton.SkeletonType.HandLeft,1)==OVRSkeleton.BoneId.Hand_IndexTip,"Legacy skeleton retains legacy IDs");
     var pinch=table.SquareWorld(12)+table.transform.up*.04f;Check(table.BeginGrab(997,pinch),"Piece picked at fingertips");var moved=pinch+table.transform.up*.08f+table.transform.right*.025f;Check(table.UpdateGrab(997,moved,false)&&Vector3.Distance(table.HeldGripWorld,moved)<.0001f,"Grab point follows fingertip midpoint exactly");table.ForgetHand(997);Check(table.Match.Position.Ply==0,"Tracking loss returns piece without committing");
     table.OpenCompanion();Check(table.CompanionOpen&&table.SessionMenuOpen&&!table.Play(Move(table.Match.Position,12,28)),"Companion holds gameplay");table.CloseCompanion();Check(!table.CompanionOpen&&!table.SessionMenuOpen&&table.SessionId==id&&table.Match.Position.Ply==0,"Companion close preserves same game");
     var experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();Check(LivingTableExperience.BoardOutOfReach(new Vector3(4,1.6f,0),Vector3.zero)&&!LivingTableExperience.BoardOutOfReach(new Vector3(0,1.6f,0),new Vector3(0,.75f,0)),"Room reach threshold");experience.RequestRoomRecovery();experience.SendMessage("Update");Check(!table.gameObject.activeInHierarchy&&table.Paused&&table.SessionId==id,"Room recovery hides old board and retains session");experience.PlaceBoard(new Vector3(0,.75f,0),Quaternion.identity).GetAwaiter().GetResult();Check(table.gameObject.activeInHierarchy&&!table.Paused&&table.SessionId==id&&table.Match.Position.Ply==0,"New room placement restores same game");
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
     table.NewGame();table.Configure(false,true,-1);
     foreach(var fen in new[]{"7k/6Q1/6K1/8/8/8/8/8 b - - 0 1","8/8/8/8/8/6k1/6q1/7K w - - 0 1","7k/5Q2/6K1/8/8/8/8/8 b - - 0 1"}){
      if(table.Playing)table.NewGame();Check(table.ImportPosition(ChessPosition.FromFen(fen)),"Completed fixture imports");table.SendMessage("Update");Check(table.GameOver,"Terminal result detected");Check(!table.TurnGuidance.Contains("Your move")&&!table.TurnGuidance.Contains("Pinch"),"No stale turn guidance after game");
      var result=table.GetComponentsInChildren<TextMesh>().Single(t=>t.name=="Game result");Check(Mathf.Approximately(result.characterSize,.0026f*4)&&result.fontStyle==FontStyle.Bold,"Result exactly four times larger and bold");
      var controls=table.GetComponentsInChildren<TableButtonVisual>();Check(controls.Any(b=>b.name=="PLAY AGAIN button")&&controls.Any(b=>b.name=="HOME button")&&!controls.Any(b=>b.name=="PAUSE button"||b.name=="HANDOFF button"),"Focused postgame controls");
      string endedId=table.SessionId;table.PlayAgain();Check(table.Playing&&!table.GameOver&&table.Match.Position.Ply==0&&table.VisiblePieceCount==32,"One action starts next game");Check(table.SessionId!=endedId&&GameSessionStore.Load(endedId)!=null,"Finished game preserved under old session");Check(table.TwoPlayers&&table.HumanSide==-1&&!table.Physical,"Replay keeps selected settings");
      string newId=table.SessionId;table.PlayAgain();Check(table.SessionId==newId,"Repeated replay cannot reset active game");
     }
     table.NewGame();table.ImportPosition(ChessPosition.FromFen("7k/6Q1/6K1/8/8/8/8/8 b - - 0 1"));
     File.WriteAllText("Builds/Companion-grip-tests.txt","PASS: OpenXR and legacy joint mapping; grabbed piece follows pinch point; tracking loss does not commit; companion holds game and close retains session; room reach detection, old board hidden, same session restored at new placement.\n");SaveView();File.WriteAllText("Builds/EndGame-tests.txt","PASS: white mate, black mate, stalemate; four-times bold result; contextual guidance; PLAY AGAIN/HOME controls; fresh session and 32 pieces; saved result retained; settings preserved; duplicate replay refused.\n");if(SessionState.GetBool("ChessLiveCompanion",false)){
      table.NewGame();table.Configure(false,true,1);table.StartGame();id=table.SessionId;
      var client=table.GetComponent<ChessCompanion>();var connection=JsonUtility.FromJson<ChessMetaVision.Connection>(File.ReadAllText("Services/FirstSteps/first-steps-connection.json"));typeof(ChessCompanion).GetField("connection",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(client,connection);
      table.OpenCompanion();table.SendMessage("AskCompanion","continue");stage=3;deadline=EditorApplication.timeSinceStartup+90;
     }else Finish(0,"CHESS_HANDOFF_TESTS_PASSED");
    }else if(stage==3){
     if(table.CompanionDisplay.Contains("thinking"))return;
     Check(table.CompanionDisplay.StartsWith("META LLAMA"),"Actual Meta response reaches Unity: "+table.CompanionDisplay);Check(table.Match.Position.Ply==0&&table.SessionId==id,"Explanation cannot move or reset game");SaveView();File.Copy("Logs/Chess-Handoff.png","Logs/Chess-Companion.png",true);
     table.SendMessage("AskCompanion","what");table.TogglePause();stage=4;
    }else if(stage==4){
     if(table.CompanionDisplay.Contains("thinking"))return;
     Check(table.CompanionDisplay.Contains("game changed"),"Stale companion answer rejected");Check(table.Match.Position.Ply==0&&table.SessionId==id,"Stale answer cannot mutate game");table.CloseCompanion();File.WriteAllText("Builds/Companion-live-tests.txt","PASS: actual local Meta response through Unity client; same session and board unchanged; changed revision rejects stale response.\n");Finish(0,"CHESS_HANDOFF_TESTS_PASSED");
    }
   }catch(Exception e){Finish(1,e.ToString());}
  }
  static void SaveView(){var camera=Camera.main;camera.transform.position=new Vector3(.1f,1.55f,-.78f);camera.transform.LookAt(new Vector3(0,.8f,-.05f));table.SendMessage("Update");foreach(var panel in table.GetComponentsInChildren<TableFacingPanel>())panel.FaceViewer();var rt=new RenderTexture(1600,1200,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,1200,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1200),0,0);image.Apply();File.WriteAllBytes("Logs/Chess-Handoff.png",image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
  static void Finish(int code,string text){SessionState.SetBool("ChessHandoffSmoke",false);SessionState.SetBool("ChessLiveCompanion",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(text);else Debug.LogError(text);EditorApplication.Exit(code);}
 }
}
#endif
