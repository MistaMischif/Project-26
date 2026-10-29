#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using LivingTable.Chess;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace LivingTable.Editor
{
 public static class FirstStepsRelease
 {
  static void Check(bool ok,string name){if(!ok)throw new Exception("FIRST STEPS: "+name);}
  public static void Tests(){
   ChessBuilder.RulesTests();int count=0;
   foreach(var fixture in new[]{
    new[]{"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1","12","28","0"},
    new[]{"4k3/8/8/8/8/3p4/4P3/4K3 w - - 0 1","12","19","0"},
    new[]{"r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1","4","6","0"},
    new[]{"4k3/8/8/3pP3/8/8/8/4K3 w - d6 0 1","36","43","0"},
    new[]{"4k3/P7/8/8/8/8/8/4K3 w - - 0 1","48","56","5"}}){
    var p=ChessPosition.FromFen(fixture[0]);var move=p.LegalMoves().Single(m=>m.From==int.Parse(fixture[1])&&m.To==int.Parse(fixture[2])&&m.Promotion==int.Parse(fixture[3]));var after=p.After(move);
    var o=new ChessObservationGate.Observation{revision=7,cells=ChessPhysicalVision.ChangedSquares(p,move).Select(s=>new ChessObservationGate.Cell{square=ChessMove.Square(s),piece=ChessObservationGate.Piece(after.Board[s])}).ToArray()};
    Check(ChessObservationGate.Accept(p,move,7,o),"correct full observation");Check(!ChessObservationGate.Accept(p,move,8,o),"stale response");o.cells[0].piece="uncertain";Check(!ChessObservationGate.Accept(p,move,7,o),"uncertainty");o.cells=o.cells.Skip(1).ToArray();Check(!ChessObservationGate.Accept(p,move,7,o),"incomplete observation");count+=4;
   }
   File.WriteAllText("Builds/FirstSteps-tests.txt",count+" observation gate cases and chess regression suite passed");Debug.Log("FIRST_STEPS_TESTS_PASSED "+count);
  }
  public static void Build(){
   Tests();var scene=EditorSceneManager.OpenScene(ChessContestRelease.QuestScene);var experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();experience.firstStepsMode=true;experience.chessOnlyMode=true;experience.desktopPreview=false;
   const string target="Assets/LivingTable/Chess/FirstStepsQuest.unity";EditorSceneManager.SaveScene(scene,target);
   var oldHttp=PlayerSettings.insecureHttpOption;string oldName=PlayerSettings.productName;
   string oldId=PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android),oldVersion=PlayerSettings.bundleVersion;int oldCode=PlayerSettings.Android.bundleVersionCode;
   try{PlayerSettings.productName="Living Table First Steps";PlayerSettings.insecureHttpOption=InsecureHttpOption.DevelopmentOnly;PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.livingtable.firststeps");PlayerSettings.bundleVersion="0.1.0";PlayerSettings.Android.bundleVersionCode=1;
    var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{target},locationPathName="Builds/LivingTable-FirstSteps-0.1.0.apk",target=BuildTarget.Android,options=BuildOptions.Development});
    File.WriteAllText("Builds/FirstSteps-build.txt",result.summary.result+" Errors: "+result.summary.totalErrors);if(result.summary.result!=BuildResult.Succeeded)throw new Exception("First Steps build failed");
   }finally{PlayerSettings.productName=oldName;PlayerSettings.insecureHttpOption=oldHttp;PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,oldId);PlayerSettings.bundleVersion=oldVersion;PlayerSettings.Android.bundleVersionCode=oldCode;AssetDatabase.SaveAssets();}
  }
 }
}
#endif
