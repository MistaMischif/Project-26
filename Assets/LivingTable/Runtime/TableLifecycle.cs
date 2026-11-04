using UnityEngine;
namespace LivingTable {
 // One gate for focus, sleep and headset presence. Never reset the native audio device on wake.
 [DefaultExecutionOrder(-10000)]
 public sealed class TableLifecycle:MonoBehaviour {
  public static bool Blocked {get;private set;}
  bool paused,unfocused,absent;float readyAt;bool wasListenerPaused;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetState(){Blocked=false;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void Install(){
#if !UNITY_ANDROID || UNITY_EDITOR
   return;
#else
   var go=new GameObject("Living Table lifecycle");DontDestroyOnLoad(go);go.AddComponent<TableLifecycle>();
#endif
  }
  void OnEnable(){OVRManager.HMDUnmounted+=Removed;OVRManager.HMDMounted+=Mounted;}
  void OnDisable(){OVRManager.HMDUnmounted-=Removed;OVRManager.HMDMounted-=Mounted;if(Blocked)AudioListener.pause=wasListenerPaused;Blocked=false;}
  void Removed(){absent=true;Evaluate();}
  void Mounted(){absent=false;Evaluate();}
  void OnApplicationPause(bool value){paused=value;Evaluate();}
  void OnApplicationFocus(bool value){unfocused=!value;Evaluate();}
  void Evaluate(){if(!paused&&!unfocused&&!absent&&!Blocked)return;if(!Blocked){wasListenerPaused=AudioListener.pause;Blocked=true;AudioListener.pause=true;Debug.Log("[Living Table] XR suspended; audio and camera work held");}readyAt=Time.realtimeSinceStartup+2f;}
  void Update(){if(Blocked&&!paused&&!unfocused&&!absent&&Time.realtimeSinceStartup>=readyAt){Blocked=false;AudioListener.pause=wasListenerPaused;Debug.Log("[Living Table] XR focus stable; resume permitted");}}
 }
}
