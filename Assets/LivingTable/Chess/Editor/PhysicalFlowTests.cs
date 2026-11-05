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
   Check(LivingTableExperience.PiecePinchActive(.035f,false),"Piece grip begins with 35 mm fingertip gap");Check(!LivingTableExperience.PiecePinchActive(.075f,false),"Relaxed wider hand does not initiate grip");Check(LivingTableExperience.PiecePinchActive(.045f,true),"Held piece tolerates gap jitter");Check(!LivingTableExperience.PiecePinchActive(.051f,true),"Opening fingers releases piece");Check(!LivingTableExperience.PiecePinchActive(float.NaN,true),"Invalid joints cannot hold piece");
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
   Check(!(bool)typeof(ChessPhysicalVision).GetProperty("Active",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(t.Vision),"Turn-time camera scanning remains off");Check(!t.AcceptObservedBaseline(t.Version-1),"Stale baseline cannot enable tracking");Check(t.AcceptObservedBaseline(t.Version)&&!t.ManualFallback,"Verified unchanged baseline enables automatic tracking");t.ToggleManual();
   t.ToggleManual();Check(t.Recovering,"Return to camera requires verification");Check(t.AcceptRecovery(t.Version,true),"Synthetic full-board approval unlocks camera mode");
   var human=t.Match.Position.LegalMoves().Single(m=>m.From==12&&m.To==28);var key=t.Match.Position.Key();var id=t.SessionId;int revision=t.Version;
   Check(t.PreviewHumanMove(human,revision),"Detected human move enters preview");Check(t.Match.Position.Key()==key&&t.Match.Position.Ply==0,"Preview does not commit saved position");
   Check(Vector3.Distance(t.PieceWorldPosition(28),t.SquareWorld(28))<.00001f,"Preview overlays correct physical square");
   Check(!t.CommitObservedMove(human,revision-1,false),"Stale confirmation rejected");Check(t.CommitObservedMove(human,revision,false),"Fresh accepted human move commits");Check(!t.CommitObservedMove(human,revision,false),"Duplicate human approval rejected");
   var ai=t.Match.Position.LegalMoves().Single(m=>m.From==52&&m.To==36);Pending(t,ai);t.SaveSession("synthetic pending AI fixture");Invoke(t,"DrawMen");
   Check(t.Match.Position.Ply==1&&Vector3.Distance(t.PieceWorldPosition(36),t.SquareWorld(36))<.00001f,"AI ghost previews target without advancing turn");
   t.Vision.CheckNow();Check(t.GetComponentsInChildren<TableButtonVisual>().Count(x=>x.name=="MOVED IT button")==1,"Legacy scan request retains Moved It CTA");Check(!t.Vision.Checking&&t.TurnGuidance.Contains("Move for AI"),"Legacy scan request cannot start gameplay scanning");
   var after=t.Match.Position.After(ai);var observation=new ChessObservationGate.Observation{revision=t.Version,cells=ChessPhysicalVision.ChangedSquares(t.Match.Position,ai).Select(s=>new ChessObservationGate.Cell{square=ChessMove.Square(s),piece=ChessObservationGate.Piece(after.Board[s])}).ToArray()};
   Check(ChessObservationGate.Accept(t.Match.Position,ai,t.Version,observation),"Expected AI destination passes independent of histogram delta");observation.cells[0].piece="uncertain";Check(!ChessObservationGate.Accept(t.Match.Position,ai,t.Version,observation),"Uncertain AI identity rejected");
   t.Vision.CancelScan();Check(t.GetComponentsInChildren<TableButtonVisual>().Count(x=>x.name=="MOVED IT button")==1,"Cancel leaves Moved It visible");Check(!t.Vision.Checking&&t.PendingAi.HasValue&&t.Match.Position.Ply==1,"Cancel leaves pending AI and history intact");
   var saved=t.CaptureSession();t.ResumeSession(id);Corners(t);t.BeginPositionScan();Check(t.Recovering&&t.PendingAi.HasValue,"Resume preserves pending AI and demands verification");
   revision=t.Version;Check(!t.CommitObservedMove(ai,revision,false),"Partial verification cannot unlock restored session");
   Check(t.CommitObservedMove(ai,revision,true)&&t.Match.Position.Ply==2&&!t.PendingAi.HasValue,"Full target verification commits restored AI exactly once");Check(!t.CommitObservedMove(ai,revision,true),"Repeated AI approval cannot double-play");
   // A camera/viewpoint change must never change the board coordinate frame.
   var before=t.SquareWorld(0);var eye=Camera.main.transform;eye.position+=new Vector3(.3f,.1f,.2f);t.SendMessage("Update");Check(Vector3.Distance(before,t.SquareWorld(0))<.00001f,"Head movement leaves world board frame fixed");
   var delta=new Vector3(.1f,.02f,.1f);t.transform.position+=delta;Invoke(t,"DrawMen");Check(Vector3.Distance(t.PieceWorldPosition(36),t.SquareWorld(36))<.00001f,"Pieces and grid share transformed board frame");
   t.OpenStandardMenu();Check(t.StandardMenuOpen,"Menu available during physical game");Invoke(t,"SetPanelScale",1.5f);var panel=UnityEngine.Object.FindObjectsByType<TableFacingPanel>(FindObjectsSortMode.None).First(x=>x.name=="Floating setup instructions"&&x.gameObject.activeInHierarchy);Check(Mathf.Abs(panel.transform.localScale.x-1.5f)<.001f,"Panel size control applied");
   var away=Vector3.ProjectOnPlane(t.transform.position-eye.position,Vector3.up).normalized;Check(panel.InstructionView&&Vector3.Distance(panel.transform.position,eye.position)<.75f,"Menu guidance floats within reach");
   t.OpenStandardMenu();
   var automatic=t.Match.Position.LegalMoves().First();revision=t.Version;Check(!t.AwaitingMoveAcceptance&&t.CommitObservedMove(automatic,revision,true,true),"Confident legal observation commits automatically without preview or button");Check(!t.CommitObservedMove(automatic,revision,true,true),"Automatic duplicate cannot commit twice");
   var target=t.Match.Position.Copy();var bad=(int[])target.Board.Clone();bad[0]=0;bad[1]=99;bad[16]=5;t.ShowPhysicalMismatch(bad,target);Check(t.HighlightedProblemSquares.OrderBy(x=>x).SequenceEqual(new[]{0,1,16}),"All mismatched and uncertain squares highlighted");Check(t.Match.Position.Key()==target.Key(),"Mismatch highlighting cannot change saved game");Check(t.GetComponentsInChildren<Transform>().Any(x=>x.name=="Pulse square b1"),"Unreadable square has visible pulse");t.ClearPhysicalMismatch();Check(t.HighlightedProblemSquares.Length==0,"Resolved mismatch clears guidance");
   var castle=new ChessMatch(ChessPosition.FromFen("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1"));typeof(ChessTable).GetProperty("Match").SetValue(t,castle);Pending(t,castle.Position.LegalMoves().Single(m=>m.From==4&&m.To==6));Invoke(t,"DrawMen");foreach(string square in new[]{"e1","g1","h1","f1"})Check(t.GetComponentsInChildren<Transform>().Any(x=>x.name=="Pulse square "+square),"Castling guidance pulses "+square);Check(t.GetComponentsInChildren<Transform>().Any(x=>x.name=="Move from e1"),"Source piece ghost remains where real king needs lifting");Check(Mathf.Abs(ChessTable.GuidancePulse(0)-ChessTable.GuidancePulse(.25f))>.3f,"Guidance pulse animates over time");
   t.NewGame();t.Configure(true,true,1);
   for(int corner=0;corner<4;corner++){var diagonal=new Vector3(corner%2==0?-1:1,0,corner<2?-1:1);var fixedPoint=t.transform.TransformPoint(-diagonal*t.Size*.5f);var knob=t.ResizeHandleWorld(corner);Check(t.BeginBoardResize(77,knob),"Single knob can be grabbed "+corner);var moved=knob+t.transform.TransformDirection(diagonal.normalized*.08f);t.UpdateBoardResize(77,moved,false);Check(Vector3.Distance(fixedPoint,t.transform.TransformPoint(-diagonal*t.Size*.5f))<.00001f,"Opposite playing corner remains locked "+corner);t.UpdateBoardResize(77,moved,true);Check(!t.ResizingBoard&&!t.Aligned,"Release requires physical fit review "+corner);}
   var heldPose=t.transform.position;float heldSize=t.Size;t.PreviewAutoAlignment(new ChessCheckerFit.Fit{valid=false,center=Vector3.one*50,size=.6f});Check(t.transform.position==heldPose&&t.Size==heldSize,"Invalid physical fit never moves locked geometry");
   var instructions=UnityEngine.Object.FindObjectsByType<TableFacingPanel>(FindObjectsSortMode.None).First(x=>x.InstructionView&&x.gameObject.activeInHierarchy);eye.rotation=Quaternion.Euler(80,20,15);instructions.FaceViewer();var relative=eye.InverseTransformPoint(instructions.transform.position);Check(Mathf.Abs(relative.z-instructions.InstructionDistance)<.001f&&relative.y>.05f&&relative.y<.2f&&Mathf.Abs(relative.x)<.001f,"Instructions stay at their configured distance in direct view while looking down with head roll");
   t.NewGame();Finish(0,"PHYSICAL_FLOW_TESTS_PASSED "+count);
  }catch(Exception e){Finish(1,e.ToString());}}
  static void Finish(int code,string message){SessionState.SetBool("PhysicalFlowTests",false);EditorApplication.update-=Tick;if(code==0){File.WriteAllText("Builds/Physical-flow-tests.txt",count+" synthetic scene checks passed. Real-board recognition and world-lock accuracy still require Quest testing.");Debug.Log(message);}else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
