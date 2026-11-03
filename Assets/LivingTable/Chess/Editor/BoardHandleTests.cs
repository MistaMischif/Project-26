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
   Check(t.HomeScreen,"Startup opens home menu");Check(t.VisiblePieceCount==0,"Home hides gameplay pieces");
   var experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();var eye=Camera.main.transform;var eyeStart=eye.position;eye.position+=Vector3.right*2;experience.SendMessage("Update");Check(t.HomeScreen&&t.gameObject.activeInHierarchy&&!LivingTableExperience.BoardOutOfReach(eye.position,t.transform.position),"Home remains reachable after tracking/head movement");eye.position=eyeStart;experience.SendMessage("Update");
   var home=t.GetComponentsInChildren<TableButtonVisual>();Check(home.Length==3&&home.Any(x=>x.name=="QUICK PLAY button")&&home.Any(x=>x.name=="CONTINUE button")&&home.Any(x=>x.name=="NEW GAME button"),"Exactly three entry choices");
   Check(t.GetComponentsInChildren<TextMesh>().Any(x=>x.text.Contains("Patent application in preparation")),"Branding does not claim a filed patent");
   t.Configure(false,true,1);t.StartGame();var id=t.SessionId;var pos=t.Match.Position;float initial=t.Size;
   foreach(var button in t.GetComponentsInChildren<TableButtonVisual>()){var face=button.transform.Find("Moving button cap/Gloss face");float radius=face.localScale.x*.5f;var at=button.transform.localPosition;Check(Mathf.Abs(at.x)+radius<=t.Size*.3125f+.0001f&&Mathf.Abs(at.z)+radius<=t.Size*.3125f+.0001f,"Button entirely inside central 5/8: "+button.name);Check(at.y>.15f,"Controls separated from board pieces");Check(Mathf.Abs(face.localScale.x-face.localScale.y)<.0001f,"Square button");}
   Check(t.GetComponentsInChildren<Transform>().Count(x=>x.name.StartsWith("Corner knob "))==4,"Exactly four corner knobs");Check(!t.GetComponentsInChildren<Transform>().Any(x=>x.name.StartsWith("Move side ")||x.name.StartsWith("Corner marker ")),"No side or colored corner controls");
   var first=t.GetComponentsInChildren<Transform>().First(x=>x.name=="Chess pieces").GetChild(0);var scale=first.localScale;t.ResizePieceSet(1.1f);Check(Vector3.Distance(t.GetComponentsInChildren<Transform>().First(x=>x.name=="Chess pieces").GetChild(0).localScale,scale*1.1f)<.0001f,"Whole set resizes");Check(t.Match.Position==pos&&t.SessionId==id,"Sizing preserves game");
   var e=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();var a=t.ResizeHandleWorld(0);var b=t.ResizeHandleWorld(3);var center=t.transform.position;float originalYaw=t.transform.eulerAngles.y;
   Check(!e.UpdateBoardGrip(center,b,true),"Center cannot grab board");Check(e.UpdateBoardGrip(a,b,true),"Two corner knobs grab board");Check(t.Paused&&t.AdjustingBoard,"Grab holds game");Check(!t.Play(pos.LegalMoves().First()),"Moves blocked while held");
   e.UpdateBoardGrip(a,b,true);Check(Mathf.Abs(t.Size-initial)<.0001f,"No initial scale jump");Check(t.GetComponentsInChildren<Transform>().Any(x=>x.name=="Alignment grid"),"Pulsing grid visible");
   var turn=Quaternion.Euler(0,30,0);var offset=new Vector3(.1f,.05f,.1f);e.UpdateBoardGrip(center+offset+turn*(a-center)*1.2f,center+offset+turn*(b-center)*1.2f,true);
   Check(Mathf.Abs(t.Size-initial*1.2f)<.0001f,"Two hands resize");Check(Vector3.Distance(t.transform.position,center+offset)<.0001f,"Two hands move center without drift");Check(Mathf.Abs(Mathf.DeltaAngle(t.transform.eulerAngles.y,originalYaw+30))<.01f,"Two hands rotate");SaveView();e.UpdateBoardGrip(default,default,false,true);
   Check(!t.Paused&&!t.AdjustingBoard,"Release restores gameplay");Check(t.SessionId==id&&t.Match.Position==pos,"Gesture preserves session");
   t.NewGame();t.Configure(true,true,1);t.BeginCornerAdjustment();t.EndRelocation();Check(!t.Aligned&&t.AlignmentInProgress,"Physical manual fine adjustment requires acceptance");Check(t.VisiblePieceCount==32,"Physical alignment displays all guide pieces");t.StartGame();Check(!t.Playing,"Unaccepted fit cannot start play");
   typeof(ChessTable).GetMethod("FinishPieceScan",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(t,new object[]{false});t.StartGame();Check(t.Playing&&t.VisiblePieceCount==32,"Physical play retains virtual counterparts");
   var move=t.Match.Position.LegalMoves().First(m=>m.From==12&&m.To==28);Check(t.ApplyCameraMove(move,t.Version),"Verified camera move commits");Check(Vector3.Distance(t.PieceWorldPosition(28),t.SquareWorld(28))<.0001f,"Virtual twin follows real move");
   File.WriteAllText("Builds/BoardHandle-tests.txt",count+" checks passed: home menu, central square controls, corner gestures, set sizing, physical guides, session safety.");Finish(0,"BOARD_HANDLE_TESTS_PASSED "+count);
  }catch(Exception e){Finish(1,e.ToString());}}
  static void SaveView(){var camera=Camera.main;camera.transform.position=new Vector3(.25f,1.6f,-1f);camera.transform.LookAt(new Vector3(0,.8f,0));foreach(var p in UnityEngine.Object.FindObjectsByType<TableFacingPanel>(FindObjectsSortMode.None))p.FaceViewer();var rt=new RenderTexture(1400,1100,24);camera.targetTexture=rt;camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1400,1100,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1100),0,0);image.Apply();File.WriteAllBytes("Logs/Board-handles.png",image.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
  static void Finish(int code,string message){SessionState.SetBool("HandleTests",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(message);else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
