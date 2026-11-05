#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LivingTable.Chess;
namespace LivingTable.Editor {
 public static class HandoffDemoTests {
  static int stage,count;static double deadline;static string id;static Vector3 pose;static Quaternion rotation;
  static void Check(bool ok,string why){count++;if(!ok)throw new Exception(why);}
  public static void Run(){SessionState.SetString("HandoffDemoRoot",Path.GetFullPath("Logs/handoff-demo-"+Guid.NewGuid().ToString("N")));EditorSceneManager.OpenScene(ChessContestRelease.DesktopScene);SessionState.SetBool("HandoffDemo",true);EditorApplication.EnterPlaymode();}
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("HandoffDemo",false))return;GameSessionStore.TestRoot=SessionState.GetString("HandoffDemoRoot","");deadline=EditorApplication.timeSinceStartup+180;EditorApplication.update+=Tick;}
  static void HumanMove(ChessTable t){var m=t.Match.Position.LegalMoves().First();int ply=t.Match.Position.Ply;Check(t.BeginGrab(47,t.SquareWorld(m.From)+t.transform.up*.025f),"Pinch grabs human piece");Check(t.UpdateGrab(47,t.SquareWorld(m.To)+t.transform.up*.025f,true),"Pinch release places preview");Check(t.AwaitingDone&&t.Match.Position.Ply==ply,"Drop waits for explicit Done/Moved It");Check(t.ConfirmPlayerMove(),"Confirmation accepts legal move");Check(t.Match.Position.Ply==ply+1,"Exactly one ply committed");}
  static void Invariant(ChessTable t){Check(t.SessionId==id,"Session identity retained");Check(Vector3.Distance(t.transform.position,pose)<.00001f&&Quaternion.Angle(t.transform.rotation,rotation)<.001f,"World pose retained");Check(!t.Vision.Checking&&!t.Recovering&&!t.AlignmentInProgress,"No scan or alignment interrupts handoff");var saved=JsonUtility.FromJson<ChessTable.SessionState>(GameSessionStore.Load(id).payload);Check(ChessMatch.Restore(saved.match).Position.Key()==t.Match.Position.Key(),"Persisted position equals live position");Check(saved.match.moves.Length==t.Match.Position.Ply,"Persisted history contains every ply");}
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Handoff demo timed out at stage "+stage);if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;var t=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(t?.Vision==null)return;
   if(stage==0){Camera.main.transform.SetPositionAndRotation(new Vector3(0,1.4f,-.4f),Quaternion.Euler(55,0,0));t.ChooseNewGame();t.ContinueEntry();foreach(var p in new[]{new Vector3(-.28f,0,-.28f),new Vector3(.28f,0,-.28f),new Vector3(.28f,0,.28f),new Vector3(-.28f,0,.28f)})Check(t.SelectBoardCorner(new Vector3(0,.7f,0)+p),"Four-corner placement");t.SendMessage("Update");t.PlayEntry();Check(t.Playing&&t.Physical&&t.HumanSide==1,"Physical standard game starts White");id=t.SessionId;pose=t.transform.position;rotation=t.transform.rotation;HumanMove(t);stage=1;return;}
   if(stage==1){if(!t.PendingAi.HasValue)return;Check(t.Match.Position.Ply==1,"Real AI proposes without committing physical move");Check(t.ConfirmPhysicalAiMove(),"Moved It commits physical AI move");Invariant(t);Check(t.Match.Position.Ply==2,"Physical human/AI turn complete");string key=t.Match.Position.Key();t.SwitchSurface();Check(t.VirtualSafetyOpen&&t.Physical,"Physical obstacles warning blocks virtual switch");t.ConfirmVirtualSafety();Check(!t.Physical&&t.Match.Position.Key()==key,"Virtual handoff retains exact position");Invariant(t);HumanMove(t);stage=2;return;}
   if(stage==2){if(t.Busy||t.Match.Position.Ply<4)return;Check(t.Match.Position.Ply==4&&t.Match.Position.Side==1,"Actual AI completes virtual reply");string key=t.Match.Position.Key();t.ToggleSessionMenu();t.SwitchSurface();Check(t.Physical&&!t.SessionMenuOpen&&!t.Paused&&t.Match.Position.Key()==key,"Real Board resumes directly from menu");Invariant(t);HumanMove(t);stage=3;return;}
   if(stage==3){if(!t.PendingAi.HasValue)return;var pending=t.PendingAi.Value;string key=t.Match.Position.Key();t.SendMessage("OnApplicationPause",true);t.SendMessage("OnApplicationPause",false);Check(t.Paused&&t.PendingAi.HasValue&&t.PendingAi.Value.Equals(pending)&&t.Match.Position.Key()==key,"Sleep preserves pending AI and committed board");t.TogglePause();Check(t.ConfirmPhysicalAiMove(),"Resume permits pending physical move confirmation");Check(t.Match.Position.Ply==6&&t.Match.Position.Side==1,"Second physical human/AI turn complete");Invariant(t);t.NewGame();Finish(0,"HANDOFF_DEMO_TESTS_PASSED "+count);}
  }catch(Exception e){Finish(1,e.ToString());}}
  static void Finish(int code,string message){SessionState.SetBool("HandoffDemo",false);EditorApplication.update-=Tick;if(code==0){File.WriteAllText("Builds/Handoff-demo-tests.txt",message+"\nSix plies with actual on-device AI algorithm across physical > virtual > physical. Synthetic pinch/corners and lifecycle events; not headset sensor proof.");Debug.Log(message);}else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
