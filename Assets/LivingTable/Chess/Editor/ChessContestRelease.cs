#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LivingTable.Editor
{
 public static class ChessContestRelease
 {
  public const string QuestScene="Assets/LivingTable/Chess/ChessContestQuest.unity";
  public const string DesktopScene="Assets/LivingTable/Chess/ChessContestDesktop.unity";
  const string CandyResources="Assets/LivingTable/CandyLand/Resources";
  const string Excluded="Assets/LivingTable/CandyLand/ContestExcluded";
  static void Move(string from,string to){string error=AssetDatabase.MoveAsset(from,to);if(!string.IsNullOrEmpty(error))throw new Exception(error);}
  public static void Prepare()
  {
   // Recover safely if an earlier editor process was interrupted during a build.
   if(AssetDatabase.IsValidFolder(Excluded)&&!AssetDatabase.IsValidFolder(CandyResources))Move(Excluded,CandyResources);
   if(!AssetDatabase.IsValidFolder("Assets/LivingTable/Runtime/Resources"))AssetDatabase.CreateFolder("Assets/LivingTable/Runtime","Resources");
   foreach(string file in new[]{"CandyPiecePlastic.shader","CandyMagicGlow.shader","GlossButton.shader"})
    if(File.Exists(CandyResources+"/"+file))Move(CandyResources+"/"+file,"Assets/LivingTable/Runtime/Resources/"+file);
   MakeScene("Assets/LivingTable/CandyLand/CandyLandQuest.unity",QuestScene,false);
   MakeScene("Assets/LivingTable/CandyLand/CandyLandDesktop.unity",DesktopScene,true);
   AssetDatabase.SaveAssets();
  }
  static void MakeScene(string source,string target,bool desktop)
  {
   var scene=EditorSceneManager.OpenScene(File.Exists(target)?target:source);
   var experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();
   if(experience==null)throw new Exception("Existing Astra experience missing.");
   experience.chessOnlyMode=true;experience.candyLandMode=true;experience.desktopPreview=desktop;
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,target);
  }
  public static void Build()
  {
   Prepare();PlayerSettings.bundleVersion="0.4.0";PlayerSettings.Android.bundleVersionCode=40;
   Directory.CreateDirectory("Builds");AssetDatabase.SaveAssets();
   Move(CandyResources,Excluded);
   try {
    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{QuestScene},locationPathName="Builds/LivingTable-Chess-Contest-Quest.apk",target=BuildTarget.Android,options=BuildOptions.DetailedBuildReport});
    var s=report.summary;File.WriteAllText("Builds/ChessContest-build-report.txt",$"Result: {s.result}\nErrors: {s.totalErrors}\nWarnings: {s.totalWarnings}\nDuration: {s.totalTime}\n");
    var assets=report.packedAssets.SelectMany(p=>p.contents).Select(a=>a.sourceAssetPath).Distinct().OrderBy(p=>p).ToArray();
    File.WriteAllLines("Builds/ChessContest-packed-assets.txt",assets);
    if(assets.Any(p=>p.StartsWith(Excluded+"/",StringComparison.OrdinalIgnoreCase)))throw new Exception("Contest build unexpectedly includes excluded Candy Land content.");
    if(s.result!=BuildResult.Succeeded)throw new Exception("Chess contest build failed.");
    Debug.Log("CHESS_CONTEST_BUILD_SUCCEEDED");
   } finally {Move(Excluded,CandyResources);AssetDatabase.SaveAssets();}
  }
 }
}
#endif
