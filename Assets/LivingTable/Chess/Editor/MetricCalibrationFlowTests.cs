#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LivingTable.Chess;
namespace LivingTable.Editor {
 public static class MetricCalibrationFlowTests {
  static double deadline;static int count,stage;static void Check(bool yes,string label){count++;if(!yes)throw new Exception("METRIC FLOW: "+label);}
  public static void Run(){SessionState.SetString("MetricRoot",Path.GetFullPath("Logs/metric-flow-"+Guid.NewGuid().ToString("N")));EditorSceneManager.OpenScene(ChessContestRelease.DesktopScene);SessionState.SetBool("MetricFlow",true);EditorApplication.EnterPlaymode();}
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("MetricFlow",false))return;GameSessionStore.TestRoot=SessionState.GetString("MetricRoot","");deadline=EditorApplication.timeSinceStartup+120;EditorApplication.update+=Tick;}
  static void Tick(){try{if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Metric flow timed out");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;var t=UnityEngine.Object.FindFirstObjectByType<ChessTable>();if(t?.Vision==null)return;
   if(stage==1){if(t.CalibrationBusy)return;Check(!t.Aligned&&!t.CalibrationBusy,"Unavailable camera after corners cannot claim automatic alignment");Check(t.GetComponentsInChildren<TableButtonVisual>().Any(b=>b.name=="ASSISTED FIT button"),"Failed refinement exposes explicit assisted fallback");t.AcceptAssistedFit();Check(t.Aligned&&t.SizingKing&&t.CaptureSession().registration.alignmentMethod=="assisted","Explicit assisted fit continues without invented camera evidence");t.NewGame();Finish(0,"METRIC_FLOW_TESTS_PASSED "+count);return;}
   t.QuickPlay();string id=t.SessionId;t.BeginAutomaticAlignment();Check(!t.GetComponentsInChildren<TableButtonVisual>().Any(b=>b.name=="ACCEPT BOARD button"),"No acceptance before physical evidence");
   var old=t.transform.position;t.PreviewMetricAlignment(default);Check(t.transform.position==old&&!t.Aligned,"Invalid fit preserves pose");
   var expected=new ChessSurfaceSolver.Estimate{valid=true,center=new Vector3(.06f,.8f,.04f),rotation=Quaternion.Euler(3,12,0),size=.72f,corners=35,rmsPixels=.8f,rmsMeters=.002f,source="synthetic fixture"};
   t.PreviewMetricAlignment(expected);Check(!t.Vision.Checking&&!t.Aligned&&!t.Playing,"Metric preview waits for explicit acceptance before scanning");Check(t.VisiblePieceCount==0,"Capture grid preview does not invent initial pieces");Check(t.GetComponentsInChildren<TableButtonVisual>().Count(b=>b.name=="ACCEPT BOARD button")==1,"One ACCEPT BOARD action after verified preview");Check(Vector3.Distance(t.transform.position,expected.center)<.00001f&&Quaternion.Angle(t.transform.rotation,expected.rotation)<.01f,"Full physical surface pose applied");
   t.AcceptAutoAlignment();Check(t.Aligned&&t.Vision.Checking&&!t.Playing,"Accept starts capture scan without starting game");Check(t.SessionId==id&&t.CaptureSession().registration.alignmentMethod=="synthetic fixture","Same draft records geometry provenance");t.Vision.CancelScan();
   t.NewGame();t.Configure(true,true,1);var figures=(Transform[])typeof(ChessTable).GetField("figures",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(t);var scale=figures[4].localScale;t.PreviewMetricAlignment(expected);figures=(Transform[])typeof(ChessTable).GetField("figures",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(t);Check(Vector3.Distance(scale,figures[4].localScale)<.00001f,"Grid refinement preserves existing piece dimensions");
   var root=t.transform.position;Camera.main.transform.position+=Vector3.one*.3f;t.SendMessage("Update");Check(Vector3.Distance(root,t.transform.position)<.00001f,"Camera movement cannot move calibrated root");t.BeginFourCorners();t.AcceptAutoAlignment();Check(!t.Aligned,"Manual restart invalidates previous automatic acceptance");t.NewGame();t.Configure(true,true,1);typeof(ChessTable).GetField("desktop",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(t,false);t.BeginFourCorners();foreach(var point in new[]{new Vector3(-.28f,.8f,-.28f),new Vector3(.28f,.8f,-.28f),new Vector3(.28f,.8f,.28f),new Vector3(-.28f,.8f,.28f)})t.SelectBoardCorner(point);stage=1;return;
  }catch(Exception ex){Finish(1,ex.ToString());}}
  static void Finish(int code,string message){SessionState.SetBool("MetricFlow",false);EditorApplication.update-=Tick;if(code==0){File.WriteAllText("Builds/Metric-flow-tests.txt",count+" synthetic metric-preview/acceptance/provenance/dimension/world-frame checks passed. Hardware accuracy is unmeasured.\n");Debug.Log(message);}else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
