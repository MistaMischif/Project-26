#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LivingTable.Chess;
namespace LivingTable.Editor {
 public static class BoardHandleTests {
  static double deadline;static int count;static void Check(bool value,string why){count++;if(!value)throw new Exception(why);}
  public static void Run(){ChessContestRelease.Prepare();SessionState.SetString("HandleRoot",Path.GetFullPath("Logs/handles-"+Guid.NewGuid().ToString("N")));EditorSceneManager.OpenScene(ChessContestRelease.DesktopScene);SessionState.SetBool("HandleTests",true);EditorApplication.EnterPlaymode();}
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("HandleTests",false))return;GameSessionStore.TestRoot=SessionState.GetString("HandleRoot","");deadline=EditorApplication.timeSinceStartup+100;EditorApplication.update+=Tick;}
  static void Tick(){try{if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Handle test timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;var t=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(t==null||t.Vision==null)return;
   t.Configure(false,true,1);t.StartGame();var id=t.SessionId;var pos=t.Match.Position;float initial=t.Size;var point=t.ResizeHandleWorld(3);
   Check(!t.BeginBoardResize(10,t.transform.position),"Board center must not grab resize");Check(t.BeginBoardResize(10,point),"Corner can be grabbed");Check(t.Paused&&t.AdjustingBoard,"Held handle pauses play");Check(!t.Play(pos.LegalMoves().First()),"Cannot move while resizing");t.UpdateBoardResize(10,point,false);Check(Mathf.Abs(t.Size-initial)<.0001f,"No scale jump on first update");
   Check(t.GetComponentsInChildren<Transform>().Any(x=>x.name=="Alignment grid"),"Active grid visible on virtual board");Check(t.GetComponentsInChildren<Transform>().Where(x=>x.name.StartsWith("Grid file ")).All(x=>x.localPosition.y>.006f),"Glow grid sits above solid board squares");Check(t.GetComponentsInChildren<Renderer>().Any(x=>x.name=="Resize corner 5"&&x.sharedMaterial.shader.name=="LivingTable/CandyMagicGlow"),"Held corner visibly active");
   var outward=t.transform.TransformDirection(new Vector3(1,0,1).normalized);t.UpdateBoardResize(10,point+outward*.12f,false);Check(t.Size>initial+.1f,"Corner drag increases size");Check(!t.UpdateBoardResize(11,point,true)&&t.ResizingBoard,"Another hand cannot release capture");SaveView();t.UpdateBoardResize(10,point+outward*.12f,true);Check(!t.Paused&&!t.AdjustingBoard&&!t.ResizingBoard,"Release restores play");Check(t.SessionId==id&&t.Match.Position==pos&&pos.Ply==0,"Resize preserves match history");Check(!t.GetComponentsInChildren<Transform>().Any(x=>x.name=="Alignment grid"),"Pulse grid removed after virtual release");
   t.NewGame();t.Configure(true,true,1);point=t.ResizeHandleWorld(0);Check(t.BeginBoardResize(20,point),"Physical board handle available before alignment");t.UpdateBoardResize(20,point,true);Check(!t.Aligned&&t.AlignmentInProgress,"Physical fit needs acceptance after release");Check(t.GetComponentsInChildren<TableButtonVisual>().Any(x=>x.name=="ACCEPT BOARD button"),"Physical acceptance offered");t.StartGame();Check(!t.Playing,"Unaccepted adjustment cannot start a game");
   point=t.ResizeHandleWorld(1);t.BeginBoardResize(21,point);t.ForgetHand(21);Check(!t.ResizingBoard&&!t.AdjustingBoard,"Tracking loss releases handle");
   File.WriteAllText("Builds/BoardHandle-tests.txt",count+" checks passed: capture, no initial scale jump, active visuals, blocked moves, release, session preservation, physical acceptance and tracking loss.");Finish(0,"BOARD_HANDLE_TESTS_PASSED "+count);
  }catch(Exception e){Finish(1,e.ToString());}}
  static void SaveView(){var camera=Camera.main;camera.transform.position=new Vector3(.25f,1.6f,-1f);camera.transform.LookAt(new Vector3(0,.8f,0));foreach(var p in UnityEngine.Object.FindObjectsByType<TableFacingPanel>(FindObjectsSortMode.None))p.FaceViewer();var rt=new RenderTexture(1400,1100,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1400,1100,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1100),0,0);image.Apply();File.WriteAllBytes("Logs/Board-handles.png",image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
  static void Finish(int code,string message){SessionState.SetBool("HandleTests",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(message);else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
