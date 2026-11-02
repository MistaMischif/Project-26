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
 public static class RecoverySceneTests
 {
  static int count;static double deadline;
  static void Check(bool ok,string name){count++;if(!ok)throw new Exception("RECOVERY SCENE: "+name);}
  public static void Run(){ReconciliationTests.Run();SessionState.SetString("RecoverySceneRoot",Path.GetFullPath("Logs/recovery-scene-"+Guid.NewGuid().ToString("N")));EditorSceneManager.OpenScene(ChessContestRelease.DesktopScene);SessionState.SetBool("RecoverySceneSmoke",true);EditorApplication.EnterPlaymode();}
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("RecoverySceneSmoke",false))return;GameSessionStore.TestRoot=SessionState.GetString("RecoverySceneRoot","");deadline=EditorApplication.timeSinceStartup+120;EditorApplication.update+=Tick;}
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Recovery scene timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
   var t=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(t==null||t.Vision==null)return;
   t.GetComponent<ChessMetaVision>().Required=true;t.Configure(false,true,1);t.StartGame();var id=t.SessionId;var key=t.Match.Position.Key();t.SwitchSurface();
   Check(t.Recovering&&!t.Aligned,"Physical return locked before alignment");
   t.BeginManualAlignment();t.TouchWorld(new Vector3(-.28f,.8f,-.28f));t.TouchWorld(new Vector3(.28f,.8f,-.28f));
   Check(t.Aligned&&t.Recovering,"Alignment alone cannot unlock play");Check(!t.AcceptRecovery(t.Version),"Appearance alone rejected in First Steps");Check(!t.AcceptRecovery(t.Version-1,true),"Stale identity revision rejected");
   t.TogglePause();Check(!t.AcceptRecovery(t.Version,true),"Paused recovery rejected");t.TogglePause();t.ToggleSessionMenu();Check(!t.AcceptRecovery(t.Version,true),"Handoff menu recovery rejected");t.ToggleSessionMenu();
   Check(t.AcceptRecovery(t.Version,true)&&!t.Recovering,"Validated recovery unlocks without a move");Check(t.SessionId==id&&t.Match.Position.Key()==key&&t.Match.Position.Ply==0,"Same session, exact position and turn");
   t.ToggleManual();Check(t.ManualFallback&&t.Unverified,"Manual mode records assisted provenance");t.ToggleManual();Check(t.Recovering&&!t.ManualFallback,"Returning to camera requires reconciliation");
   t.AttestRecovery();Check(!t.Recovering&&t.ManualFallback&&t.Unverified,"Manual check stays explicitly manual");t.ToggleManual();Check(t.Recovering,"Cannot rebaseline silently after manual check");
   Check(t.AcceptRecovery(t.Version,true)&&!t.ManualFallback&&t.Unverified,"Camera resumes but assisted history remains");
   var saved=GameSessionStore.Load(id);Check(saved.payload.Contains("\"unverified\":true"),"Assisted history durably saved");
   t.NewGame();Finish(0,"RECOVERY_SCENE_TESTS_PASSED "+count);
  }catch(Exception e){Finish(1,e.ToString());}}
  static void Finish(int code,string message){SessionState.SetBool("RecoverySceneSmoke",false);EditorApplication.update-=Tick;if(code==0){File.WriteAllText("Builds/Recovery-scene-tests.txt",count+" scene transition checks passed using synthetic identity approval; no camera accuracy claim.");Debug.Log(message);}else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
