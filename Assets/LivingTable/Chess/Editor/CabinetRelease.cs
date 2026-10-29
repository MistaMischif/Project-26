#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LivingTable.Editor
{
 public static class CabinetRelease
 {
  public const string Desktop="Assets/LivingTable/Chess/LivingTableCabinetDesktop.unity",Quest="Assets/LivingTable/Chess/LivingTableCabinetQuest.unity";
  public static void Prepare(){Create("Assets/LivingTable/CandyLand/CandyLandDesktop.unity",Desktop,true);Create("Assets/LivingTable/CandyLand/CandyLandQuest.unity",Quest,false);AssetDatabase.SaveAssets();}
  static void Create(string source,string target,bool preview){var scene=EditorSceneManager.OpenScene(File.Exists(target)?target:source);var experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();experience.chessOnlyMode=false;experience.candyLandMode=true;experience.startWithChess=true;experience.desktopPreview=preview;EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,target);}
  public static void Build(){Prepare();PlayerSettings.bundleVersion="0.6.3";PlayerSettings.Android.bundleVersionCode=63;AssetDatabase.SaveAssets();Directory.CreateDirectory("Builds");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Quest},locationPathName="Builds/LivingTable-Cabinet-Quest.apk",target=BuildTarget.Android,options=BuildOptions.None});var s=report.summary;File.WriteAllText("Builds/Cabinet-build-report.txt",$"Result: {s.result}\nErrors: {s.totalErrors}\nWarnings: {s.totalWarnings}\nDuration: {s.totalTime}\n");if(s.result!=BuildResult.Succeeded)throw new Exception("Cabinet build failed");Debug.Log("CABINET_BUILD_SUCCEEDED");}
 }
}
#endif
