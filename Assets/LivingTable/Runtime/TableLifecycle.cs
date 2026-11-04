using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;
namespace LivingTable {
 // One gate for focus, sleep and headset presence. Never reset the native audio device on wake.
 [DefaultExecutionOrder(-10000)]
 public sealed class TableLifecycle:MonoBehaviour {
  public static bool Blocked {get;private set;}
  bool paused,unfocused,absent;float readyAt;bool wasListenerPaused;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetState(){Blocked=false;}
  public static void ApplyRenderBudget(UniversalRenderPipelineAsset pipeline){if(pipeline==null)return;pipeline.renderScale=Mathf.Min(pipeline.renderScale,1f);pipeline.msaaSampleCount=Mathf.Min(pipeline.msaaSampleCount,2);}
  static void Memory(string stage){Debug.Log("[Living Table] "+stage+" managed/native="+(Profiler.GetTotalAllocatedMemoryLong()/1048576)+"MB graphics="+(Profiler.GetAllocatedMemoryForGraphicsDriver()/1048576)+"MB eye="+UnityEngine.XR.XRSettings.eyeTextureWidth+"x"+UnityEngine.XR.XRSettings.eyeTextureHeight);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void Install(){
#if !UNITY_ANDROID || UNITY_EDITOR
   return;
#else
   ApplyRenderBudget(QualitySettings.renderPipeline as UniversalRenderPipelineAsset);
   var go=new GameObject("Living Table lifecycle");DontDestroyOnLoad(go);go.AddComponent<TableLifecycle>();
#endif
  }
  void OnEnable(){OVRManager.HMDUnmounted+=Removed;OVRManager.HMDMounted+=Mounted;}
  void OnDisable(){OVRManager.HMDUnmounted-=Removed;OVRManager.HMDMounted-=Mounted;if(Blocked)AudioListener.pause=wasListenerPaused;Blocked=false;}
  void Removed(){absent=true;Evaluate();}
  void Mounted(){absent=false;Evaluate();}
  void OnApplicationPause(bool value){paused=value;Evaluate();}
  void OnApplicationFocus(bool value){unfocused=!value;Evaluate();}
  void Evaluate(){if(!paused&&!unfocused&&!absent&&!Blocked)return;if(!Blocked){wasListenerPaused=AudioListener.pause;Blocked=true;AudioListener.pause=true;Debug.Log("[Living Table] XR suspended; audio and camera work held");Memory("Suspend");}readyAt=Time.realtimeSinceStartup+2f;}
  void Update(){if(Blocked&&!paused&&!unfocused&&!absent&&Time.realtimeSinceStartup>=readyAt){Blocked=false;AudioListener.pause=wasListenerPaused;Debug.Log("[Living Table] XR focus stable; resume permitted");Memory("Resume");}}
 }
}
