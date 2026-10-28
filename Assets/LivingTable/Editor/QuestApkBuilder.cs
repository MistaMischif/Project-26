#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LivingTable.Editor
{
    public static class QuestApkBuilder
    {
        [MenuItem("Living Table/Build Quest APK")]
        public static void Build()
        {
            const string scene = "Assets/LivingTable/LivingTableQuest.unity";
            if (!File.Exists(scene)) throw new FileNotFoundException("Create the Living Table Quest scene first.", scene);
            Directory.CreateDirectory("Builds");
            string previousName = PlayerSettings.productName;
            bool previousBundle = EditorUserBuildSettings.buildAppBundle;
            bool previousExport = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            try
            {
                PlayerSettings.productName = "Living Table";
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = "Builds/LivingTable-Quest.apk",
                    target = BuildTarget.Android,
                    options = BuildOptions.None
                });
                var summary = report.summary;
                File.WriteAllText("Builds/LivingTable-build-report.txt",
                    $"Result: {summary.result}\nScene: {scene}\nSize: {summary.totalSize} bytes\nErrors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}\nDuration: {summary.totalTime}\n");
                if (summary.result != BuildResult.Succeeded)
                    throw new Exception("Living Table APK build failed; see Logs/LivingTable-apk.log.");
                Debug.Log("LIVING_TABLE_APK_BUILD_SUCCEEDED: " + Path.GetFullPath(summary.outputPath));
            }
            finally
            {
                PlayerSettings.productName = previousName;
                EditorUserBuildSettings.buildAppBundle = previousBundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = previousExport;
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
