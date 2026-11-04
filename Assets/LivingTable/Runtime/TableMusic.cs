using UnityEngine;
namespace LivingTable
{
 public sealed class TableMusic : MonoBehaviour
 {
  AudioSource music;bool enabledMusic=true,gamePaused;int resumeSample;
  public bool Enabled=>enabledMusic;
  public AudioSource Source=>music;
  public void Initialize(){music=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/candy-garden-waltz");music.loop=true;music.playOnAwake=false;music.spatialBlend=0;music.volume=.11f;music.priority=200;Resume();}
  public void SetGameplay(bool playing)
  {
   var clip=Resources.Load<AudioClip>(playing?"Audio/candy-soft-shuffle":"Audio/candy-garden-waltz");
   if(music==null||clip==null||music.clip==clip)return;Suspend();music.clip=clip;resumeSample=0;music.volume=0;Resume();
  }
  void Resume(){if(music==null||music.clip==null||!enabledMusic||gamePaused||TableLifecycle.Blocked||!isActiveAndEnabled)return;if(!music.isPlaying){music.Play();music.timeSamples=Mathf.Clamp(resumeSample,0,music.clip.samples-1);}}
  void Suspend(){if(music!=null&&music.isPlaying){resumeSample=music.timeSamples;music.Stop();}}
  void OnEnable(){Resume();}
  void OnDisable(){Suspend();}
  public void Toggle(){enabledMusic=!enabledMusic;if(enabledMusic)Resume();else Suspend();}
  public void SetPaused(bool pause,bool speaking){if(music==null)return;music.volume=Mathf.MoveTowards(music.volume,speaking?.0275f:.11f,Time.unscaledDeltaTime*.3f);gamePaused=pause;if(pause||!enabledMusic)Suspend();else Resume();}
 }
}
