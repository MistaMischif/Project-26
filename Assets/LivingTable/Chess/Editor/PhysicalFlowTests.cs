#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LivingTable.Chess;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace LivingTable.Editor {
 public static class PhysicalFlowTests {
  static int count;static double deadline;
  static void Check(bool ok,string name){count++;if(!ok)throw new Exception("PHYSICAL FLOW: "+name);}
  static void Invoke(ChessTable t,string name,params object[] args)=>typeof(ChessTable).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(t,args);
  static void Pending(ChessTable t,ChessMove? move)=>typeof(ChessTable).GetProperty("PendingAi").SetValue(t,move);
  static void Corners(ChessTable t){t.BeginFourCorners();foreach(var p in new[]{new Vector3(-.28f,.8f,-.28f),new Vector3(.28f,.8f,-.28f),new Vector3(.28f,.8f,.28f),new Vector3(-.28f,.8f,.28f)})Check(t.SelectBoardCorner(p),"Select ordered playing corner");}
  public static void Run(){SessionState.SetString("PhysicalFlowRoot",Path.GetFullPath("Logs/physical-flow-"+Guid.NewGuid().ToString("N")));EditorSceneManager.OpenScene(ChessContestRelease.DesktopScene);SessionState.SetBool("PhysicalFlowTests",true);EditorApplication.EnterPlaymode();}
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("PhysicalFlowTests",false))return;GameSessionStore.TestRoot=SessionState.GetString("PhysicalFlowRoot","");deadline=EditorApplication.timeSinceStartup+120;EditorApplication.update+=Tick;}
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Timed out");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;var t=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(t==null||t.Vision==null)return;
   Check(ChessTable.InferSide(ChessPosition.Initial().Board)==0,"Starting layout cannot reveal move history or turn");
   var corners=new[]{new Vector3(0,0,0),new Vector3(.56f,0,0),new Vector3(.56f,0,.56f),new Vector3(0,0,.56f)};
   Check(ChessTable.FitCorners(corners,out var center,out var rot,out var size)&&Mathf.Abs(size-.56f)<.001f,"Square corner fit");
   corners[2]+=Vector3.up*.2f;Check(!ChessTable.FitCorners(corners,out _,out _,out _),"Reject inconsistent height");
   t.GetComponent<ChessMetaVision>().Required=true;t.Configure(true,true,1);Corners(t);Check(t.SizingKing&&t.VisiblePieceCount==1,"Four corners lead to single king sizing");
   var king=t.PieceWorldPosition(4)+Vector3.up*.05f;Check(t.BeginSetupGrab(88,king),"Grab king for set sizing");t.UpdateSetupGrab(88,king+Vector3.up*.02f,true);
   Check(t.Aligned&&!t.Playing,"Sizing alone never starts game");t.BeginPositionScan();Check(t.Vision.Checking,"Full position scan requested");t.OpenStandardMenu();Check(t.StandardMenuOpen&&!t.Vision.Checking,"Menu cancels scan immediately");t.OpenStandardMenu();
   Check(t.SetupReviewing,"Canceled scan has manual review route");
   // Supply a known layout via the same scan-result entry point; no real camera accuracy claim.
   t.BeginPositionScan();t.Vision.CancelScan();t.ReceiveLayout(ChessPosition.Initial().Board,"Synthetic fixture");Invoke(t,"ConfirmLayout");Check(t.AlignmentInProgress&&!t.Playing,"Turn selection precedes game");
   typeof(ChessTable).GetField("turnChosen",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(t,true);Invoke(t,"ConfirmTurn");
   var cta=t.GetComponentsInChildren<TableButtonVisual>().Single(x=>x.name=="START GAME button");var face=cta.transform.Find("Moving button cap/Gloss face").GetComponent<Renderer>().sharedMaterial;
   Check(face.GetFloat("_Circle")==1&&face.GetColor("_Tint").b>face.GetColor("_Tint").g,"Purple circular START GAME is presented");
   Check(cta.transform.localPosition.z<-t.Size*.5f,"CTA is at base, outside playing squares");
   Invoke(t,"StartObservedGame");Check(!t.Playing,"Stale scan cannot silently start verified game");
   // Manual fallback remains explicit and starts only on the same START GAME action.
   typeof(ChessTable).GetField("setupManual",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(t,true);Invoke(t,"StartObservedGame");Check(t.Playing&&t.ManualFallback&&t.Unverified,"Manual review starts labeled assisted game");
   t.ToggleManual();Check(t.Recovering,"Return to camera requires verification");Check(t.AcceptRecovery(t.Version,true),"Synthetic full-board approval unlocks camera mode");
   var human=t.Match.Position.LegalMoves().Single(m=>m.From==12&&m.To==28);var key=t.Match.Position.Key();var id=t.SessionId;int revision=t.Version;
   Check(t.PreviewHumanMove(human,revision),"Detected human move enters preview");Check(t.Match.Position.Key()==key&&t.Match.Position.Ply==0,"Preview does not commit saved position");
   Check(Vector3.Distance(t.PieceWorldPosition(28),t.SquareWorld(28))<.00001f,"Preview overlays correct physical square");
   Check(!t.CommitObservedMove(human,revision-1,false),"Stale confirmation rejected");Check(t.CommitObservedMove(human,revision,false),"Fresh accepted human move commits");Check(!t.CommitObservedMove(human,revision,false),"Duplicate human approval rejected");
   var ai=t.Match.Position.LegalMoves().Single(m=>m.From==52&&m.To==36);Pending(t,ai);t.SaveSession("synthetic pending AI fixture");Invoke(t,"DrawMen");
   Check(t.Match.Position.Ply==1&&Vector3.Distance(t.PieceWorldPosition(36),t.SquareWorld(36))<.00001f,"AI ghost previews target without advancing turn");
   t.Vision.CheckNow();Check(t.GetComponentsInChildren<TableButtonVisual>().Count(x=>x.name=="CANCEL SCAN button")==1,"Check changes the central CTA to cancel");Check(t.Vision.Checking&&t.TurnGuidance.Contains("Checking AI move"),"CHECK BOARD forces visible AI identity verification");
   var after=t.Match.Position.After(ai);var observation=new ChessObservationGate.Observation{revision=t.Version,cells=ChessPhysicalVision.ChangedSquares(t.Match.Position,ai).Select(s=>new ChessObservationGate.Cell{square=ChessMove.Square(s),piece=ChessObservationGate.Piece(after.Board[s])}).ToArray()};
   Check(ChessObservationGate.Accept(t.Match.Position,ai,t.Version,observation),"Expected AI destination passes independent of histogram delta");observation.cells[0].piece="uncertain";Check(!ChessObservationGate.Accept(t.Match.Position,ai,t.Version,observation),"Uncertain AI identity rejected");
   t.Vision.CancelScan();Check(t.GetComponentsInChildren<TableButtonVisual>().Count(x=>x.name=="CHECK BOARD button")==1,"Cancel restores the central check action");Check(!t.Vision.Checking&&t.PendingAi.HasValue&&t.Match.Position.Ply==1,"Cancel leaves pending AI and history intact");
   var saved=t.CaptureSession();t.ResumeSession(id);Corners(t);t.BeginPositionScan();Check(t.Recovering&&t.PendingAi.HasValue,"Resume preserves pending AI and demands verification");
   revision=t.Version;Check(!t.CommitObservedMove(ai,revision,false),"Partial verification cannot unlock restored session");
   Check(t.CommitObservedMove(ai,revision,true)&&t.Match.Position.Ply==2&&!t.PendingAi.HasValue,"Full target verification commits restored AI exactly once");Check(!t.CommitObservedMove(ai,revision,true),"Repeated AI approval cannot double-play");
   // A camera/viewpoint change must never change the board coordinate frame.
   var before=t.SquareWorld(0);var eye=Camera.main.transform;eye.position+=new Vector3(.3f,.1f,.2f);t.SendMessage("Update");Check(Vector3.Distance(before,t.SquareWorld(0))<.00001f,"Head movement leaves world board frame fixed");
   var delta=new Vector3(.1f,.02f,.1f);t.transform.position+=delta;Invoke(t,"DrawMen");Check(Vector3.Distance(t.PieceWorldPosition(36),t.SquareWorld(36))<.00001f,"Pieces and grid share transformed board frame");
   t.OpenStandardMenu();Check(t.StandardMenuOpen,"Menu available during physical game");Invoke(t,"SetPanelScale",1.5f);var panel=UnityEngine.Object.FindObjectsByType<TableFacingPanel>(FindObjectsSortMode.None).First(x=>x.name=="Chess status"&&x.gameObject.activeInHierarchy);Check(Mathf.Abs(panel.transform.localScale.x-1.5f)<.001f,"Panel size control applied");
   var away=Vector3.ProjectOnPlane(t.transform.position-eye.position,Vector3.up).normalized;Check(Vector3.Dot(panel.transform.position-t.transform.position,away)>t.Size*.7f,"Panel floats behind playing area");
   t.OpenStandardMenu();t.NewGame();Finish(0,"PHYSICAL_FLOW_TESTS_PASSED "+count);
  }catch(Exception e){Finish(1,e.ToString());}}
  static void Finish(int code,string message){SessionState.SetBool("PhysicalFlowTests",false);EditorApplication.update-=Tick;if(code==0){File.WriteAllText("Builds/Physical-flow-tests.txt",count+" synthetic scene checks passed. Real-board recognition and world-lock accuracy still require Quest testing.");Debug.Log(message);}else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
