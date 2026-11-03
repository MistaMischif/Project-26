#if UNITY_EDITOR
using System;
using System.IO;
using LivingTable.Chess;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LivingTable.Editor
{
 public static class BoardPlacementTests
 {
  static int checks;static void Check(bool condition,string message){checks++;if(!condition)throw new Exception("BOARD PLACEMENT: "+message);}
  public static void Run(){
   var go=new GameObject("Grip geometry test");var t=go.transform;t.SetPositionAndRotation(new Vector3(1,.8f,2),Quaternion.Euler(0,25,0));var scale=t.localScale;var grip=new TableBoardGrip();
   var a=t.TransformPoint(new Vector3(-.32f,0,0));var b=t.TransformPoint(new Vector3(.32f,0,0));
   Check(!grip.Begin(t,.28f,.28f,t.TransformPoint(new Vector3(-.24f,0,0)),b),"Playing squares do not start board grabs");
   Check(!grip.Begin(t,.28f,.28f,a,a+Vector3.forward*.1f),"Same side rejected");
   Check(grip.Begin(t,.28f,.28f,a,b),"Opposite rims accepted");var original=t.position;var rotation=t.rotation;var offset=new Vector3(.3f,.2f,-.1f);var yaw=Quaternion.Euler(0,90,0);
   Check(grip.Follow(t,original+offset+yaw*(a-original),original+offset+yaw*(b-original)),"Two hands follow");
   Check(Vector3.Distance(t.position,original+offset)<.0001f,"Translation follows hand midpoint");Check(Quaternion.Angle(t.rotation,yaw*rotation)<.001f,"Yaw follows both hands");Check(t.localScale==scale,"No accidental scaling");var last=t.position;Check(!grip.Follow(t,Vector3.zero,Vector3.zero)&&t.position==last,"Collapsed hands freeze last pose");grip.End();Check(!grip.Active,"Release ends grip");UnityEngine.Object.DestroyImmediate(go);
   SessionState.SetInt("BoardPlacementChecks",checks);SessionState.SetString("BoardPlacementRoot",Path.GetFullPath("Logs/board-placement-tests-"+Guid.NewGuid().ToString("N")));
   EditorSceneManager.OpenScene(ChessContestRelease.DesktopScene);SessionState.SetBool("BoardPlacementSmoke",true);EditorApplication.EnterPlaymode();
  }
  public static void OffsetFrame(){
   var root=new GameObject("Offset root");var frame=new GameObject("Calibrated board");frame.transform.SetParent(root.transform,false);frame.transform.localPosition=new Vector3(.7f,.012f,.2f);frame.transform.localRotation=Quaternion.Euler(0,30,0);
   var a=frame.transform.TransformPoint(new Vector3(-.32f,0,0));var b=frame.transform.TransformPoint(new Vector3(.32f,0,0));var center=frame.transform.position;var target=center+new Vector3(.1f,.2f,.3f);var turn=Quaternion.Euler(0,45,0);var grip=new TableBoardGrip();
   Check(grip.Begin(root.transform,.28f,.28f,a,b,frame.transform),"Calibrated rim hit frame");Check(grip.Follow(root.transform,target+turn*(a-center),target+turn*(b-center)),"Offset board follows");Check(Vector3.Distance(frame.transform.position,target)<.0001f,"Calibrated board stays between hands");UnityEngine.Object.DestroyImmediate(root);File.WriteAllText("Builds/BoardGrip-offset-tests.txt","3 calibrated-board offset checks passed");
  }
  static double deadline;
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("BoardPlacementSmoke",false))return;checks=SessionState.GetInt("BoardPlacementChecks",0);GameSessionStore.TestRoot=SessionState.GetString("BoardPlacementRoot","");deadline=EditorApplication.timeSinceStartup+120;EditorApplication.update+=Tick;}
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Scene timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
   var e=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();var table=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(table==null||table.Vision==null)return;
   var mount=table.transform.parent;table.Configure(false,true,1);table.StartGame();var id=table.SessionId;var position=table.Match.Position;var original=mount.position;
   var hide=e.RepositionBoard();Check(hide.IsCompleted,"Desktop relocation completes without XR anchor");Check(!table.gameObject.activeInHierarchy&&!mount.gameObject.activeSelf,"MOVE BOARD hides board and pieces immediately");Check(table.Paused,"Relocation pauses game");Check(e.ChessActive,"Hidden parent retains chess selection");
   e.PlaceBoard(original,Quaternion.identity).GetAwaiter().GetResult();Check(table.gameObject.activeInHierarchy&&!table.Paused,"Placement restores visibility and previous pause state");
   var a=table.ResizeHandleWorld(0);var b=table.ResizeHandleWorld(3);Check(e.UpdateBoardGrip(a,b,true),"Side grip begins in live scene");Check(e.BoardHeld&&table.Paused,"Held board suspends turns");Check(table.gameObject.activeInHierarchy,"Direct side grab keeps board visible");var delta=new Vector3(.1f,.15f,.2f);e.UpdateBoardGrip(a+delta,b+delta,true);Check(Vector3.Distance(mount.position,original+delta)<.0001f,"Live board translates");e.UpdateBoardGrip(default,default,false);Check(!e.BoardHeld&&!table.Paused,"Release restores gameplay");Check(table.SessionId==id&&table.Match.Position==position&&position.Ply==0,"Manipulation preserves journal and moves");
   Check(!e.UpdateBoardGrip(a+delta,b+delta,true),"Both hands must release before grabbing again");e.UpdateBoardGrip(default,default,false,true);table.TogglePause();Check(e.UpdateBoardGrip(a+delta,b+delta,true),"Paused game can be relocated");e.UpdateBoardGrip(default,default,false);Check(table.Paused,"Original paused state retained");
   File.WriteAllText("Builds/BoardPlacement-tests.txt",checks+" geometry and live-scene placement checks passed");Finish(0,"BOARD_PLACEMENT_TESTS_PASSED "+checks);
  }catch(Exception error){Finish(1,error.ToString());}}
  static void Finish(int code,string message){SessionState.SetBool("BoardPlacementSmoke",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(message);else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
