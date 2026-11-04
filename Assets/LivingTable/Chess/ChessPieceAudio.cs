using UnityEngine;
namespace LivingTable.Chess
{
 public sealed class ChessPieceAudio:MonoBehaviour
 {
  public enum Cue{Lift,Place,Return,Button}
  AudioSource source;AudioClip lift,place,button;public int PlayCount {get;private set;}public Cue LastCue {get;private set;}
  void Awake(){source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=.65f;source.minDistance=.25f;source.maxDistance=2.5f;source.rolloffMode=AudioRolloffMode.Linear;lift=Make("Soft felt lift",.13f,720,1250,.45f);place=Make("Wooden chess placement",.20f,230,670,1);button=Make("Cabinet button click",.07f,510,1020,.3f);}
  static AudioClip Make(string name,float duration,float a,float b,float body){const int rate=24000;int count=Mathf.CeilToInt(duration*rate);var data=new float[count];uint random=78123;for(int i=0;i<count;i++){float t=i/(float)rate;random=1664525*random+1013904223;float noise=(random/(float)uint.MaxValue*2-1);float attack=1-Mathf.Exp(-t*1800);data[i]=attack*(Mathf.Sin(2*Mathf.PI*a*t)*Mathf.Exp(-t*42)*.32f*body+Mathf.Sin(2*Mathf.PI*b*t)*Mathf.Exp(-t*75)*.13f+noise*Mathf.Exp(-t*110)*.18f);}var clip=AudioClip.Create(name,count,1,rate,false);clip.SetData(data,0);return clip;}
  public void Play(Cue cue,Vector3 point){if(!isActiveAndEnabled||TableLifecycle.Blocked)return;transform.position=point;LastCue=cue;PlayCount++;source.PlayOneShot(cue==Cue.Lift?lift:cue==Cue.Button?button:place,cue==Cue.Return?.18f:cue==Cue.Button?.25f:.42f);}
  void OnDestroy(){Destroy(lift);Destroy(place);Destroy(button);}
 }
}
