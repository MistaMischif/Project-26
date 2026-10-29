#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Android;
namespace LivingTable.Editor
{
 public sealed class FirstStepsNetworkPolicy:IPostGenerateGradleAndroidProject
 {
  public int callbackOrder=>int.MaxValue;
  public void OnPostGenerateGradleAndroidProject(string path){
   if(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)!="com.livingtable.firststeps")return;
   string folder=Path.Combine(path,"src/main/res/xml");Directory.CreateDirectory(folder);
   File.WriteAllText(Path.Combine(folder,"network_sec_config.xml"),"<?xml version=\"1.0\" encoding=\"utf-8\"?><network-security-config><base-config cleartextTrafficPermitted=\"false\"/><domain-config cleartextTrafficPermitted=\"true\"><domain>127.0.0.1</domain><domain>localhost</domain></domain-config></network-security-config>");
  }
 }
}
#endif

