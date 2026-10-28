#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
namespace LivingTable.Editor
{
 public static class SessionRelease
 {
  public static void Build()
  {
   PlayerSettings.bundleVersion="0.3.0";PlayerSettings.Android.bundleVersionCode=30;
   AssetDatabase.SaveAssets();Directory.CreateDirectory("Logs");
   File.WriteAllLines("Logs/Session-source-dependencies.txt",AssetDatabase.GetDependencies("Assets/LivingTable/CandyLand/CandyLandQuest.unity",true).Where(p=>p.StartsWith("Assets/")));
   CandyBuilder.BuildQuest();
  }
 }
}
#endif
