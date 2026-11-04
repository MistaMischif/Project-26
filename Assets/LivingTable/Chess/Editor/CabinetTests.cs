#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using LivingTable.Chess;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LivingTable.Editor
{
 public static class CabinetTests
 {
  static void Check(bool ok,string why){if(!ok)throw new Exception("CABINET TEST: "+why);}
  public static void Run(){CabinetRelease.Prepare();EditorSceneManager.OpenScene(CabinetRelease.Desktop);SessionState.SetBool("CabinetSmoke",true);EditorApplication.EnterPlaymode();}
  static int stage;static double deadline;static ChessTable table;static ChessPieceAudio audio;static LivingTableExperience experience;
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("CabinetSmoke",false))return;stage=0;deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;}
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Cabinet scene timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
   if(stage==0){experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();table=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(table==null||table.Vision==null)return;Check(experience.ChessActive&&!experience.chessOnlyMode,"Chess opens with both games available");Check(table.GetComponentsInChildren<TableButtonVisual>().Any(b=>b.name=="CANDY LAND button"),"Candy Land option present");Check(table.GetComponentsInChildren<TableCabinetVisual>().Length>=2,"Chess board and floating panel styled");table.Configure(false,true,1);table.StartGame();audio=table.GetComponentInChildren<ChessPieceAudio>();var initial=table.SquareWorld(12)+Vector3.up*.03f;Check(table.BeginGrab(41,initial),"Begin pawn grip");var high=initial+new Vector3(.08f,.40f,.03f);table.UpdateGrab(41,high,false);Check(Vector3.Distance(table.HeldGripWorld,high)<.0001f,"Pinch contact stays exact above old height clamp");Check(audio.PlayCount>0&&audio.LastCue==ChessPieceAudio.Cue.Lift,"Pickup sound triggered");var button=table.GetComponentsInChildren<TableButtonVisual>().First();int soundsBefore=audio.PlayCount;table.DirectPress(41,button.transform.position+Vector3.up*.08f);table.DirectPress(41,button.transform.position);Check(audio.PlayCount==soundsBefore,"Held pieces cannot accidentally press toolbar buttons");table.ForgetHand(41);Check(table.Match.Position.Ply==0&&Vector3.Distance(table.PieceWorldPosition(12),table.SquareWorld(12))<.0001f,"Tracking loss resets without move");table.BeginGrab(42,initial);table.UpdateGrab(42,table.SquareWorld(28)+Vector3.up*.35f,true);Check(table.Match.Position.Ply==0,"Release far above board cancels");table.BeginGrab(43,initial);table.UpdateGrab(43,table.SquareWorld(28)+Vector3.up*.04f,true);Check(table.AwaitingDone&&table.ConfirmPlayerMove()&&table.Match.Position.Ply==1,"Legal drop commits with Done");stage=1;
   }else if(stage==1){if(table.Busy)return;Check(audio.LastCue==ChessPieceAudio.Cue.Place,"Placement sound follows landing");Check(Vector3.Distance(table.PieceWorldPosition(28),table.SquareWorld(28))<.0001f,"Piece settles at exact destination");Capture("Logs/Chess-Cabinet.png",table);table.NewGame();experience.SelectCandy();stage=2;
   }else if(stage==2){var candy=UnityEngine.Object.FindFirstObjectByType<LivingTable.CandyLand.CandyTable>();Check(candy!=null&&candy.enabled&&!experience.ChessActive,"Candy remains playable from chess");Check(candy.GetComponentsInChildren<TableBoardCabinet>().Length==1,"Candy board themed");Check(candy.StatusPanel.GetComponentInChildren<TableCabinetVisual>()!=null,"Candy floating panel themed");Capture("Logs/Candy-Cabinet.png",candy);experience.SelectChess();Check(experience.ChessActive,"Return to chess");Finish(0,"CABINET_TESTS_PASSED");}
  }catch(Exception e){Finish(1,e.ToString());}}
  static void Capture(string path,MonoBehaviour owner){var camera=Camera.main;camera.transform.position=new Vector3(.10f,1.65f,-1.05f);camera.transform.LookAt(new Vector3(0,.78f,-.10f));owner.SendMessage("Update");foreach(var p in owner.GetComponentsInChildren<TableFacingPanel>())p.FaceViewer();var rt=new RenderTexture(1600,1400,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,1400,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1400),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(rt);}
  static void Finish(int code,string message){SessionState.SetBool("CabinetSmoke",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(message);else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
