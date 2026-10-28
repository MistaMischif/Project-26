using System;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // Decorative reactions never move a pawn or change the rules state.
    public sealed class CandyCharacterActor : MonoBehaviour
    {
        public int CharacterId;
        public event Action Interacted;
        public Func<bool> IsPaused;
        public int ReactionCount {get;private set;}
        public bool Reacting=>reaction<duration;
        public bool Stretching {get;private set;}
        public Vector3 StretchRatio=>stretch;
        Vector3 stretchVelocity;
        Vector3 handLast,handVelocity;float handAt;
        public float ReactionIntensity {get;private set;}=1;
        Vector3 grabStart,stretch=Vector3.one,stretchTarget=Vector3.one;
        public bool BeginStretch(Vector3 worldPoint)
        {
            Initialize();if(Stretching||IsPaused?.Invoke()==true)return false;
            grabStart=transform.parent.InverseTransformPoint(worldPoint);handLast=grabStart;handAt=Time.unscaledTime;handVelocity=Vector3.zero;Stretching=true;reaction=duration;stretchVelocity=Vector3.zero;Interacted?.Invoke();return true;
        }
        public void DragStretch(Vector3 worldPoint)
        {
            if(!Stretching||IsPaused?.Invoke()==true)return;
            Vector3 delta=Quaternion.Inverse(facing)*(transform.parent.InverseTransformPoint(worldPoint)-grabStart);
            var point=transform.parent.InverseTransformPoint(worldPoint);float sample=Time.unscaledTime-handAt;
            if(sample>.0001f)handVelocity=Vector3.Lerp(handVelocity,Vector3.ClampMagnitude((point-handLast)/sample,3),.7f);
            handLast=point;handAt=Time.unscaledTime;
            stretchTarget=new Vector3(1,Mathf.Clamp(1+Mathf.Max(0,delta.y)/Mathf.Max(.01f,rest.y),1,3),1);
        }
        public void EndStretch(bool immediately=false)
        {
            Stretching=false;stretchTarget=Vector3.one;
            if(!immediately&&Time.unscaledTime-handAt<.15f&&handVelocity.magnitude>.25f)Flick(handVelocity);
            if(immediately){stretch=Vector3.one;stretchVelocity=Vector3.zero;if(initialized){transform.localScale=rest;transform.localPosition=origin;transform.localRotation=facing;}}
        }
        float age,reaction=10,duration=1.4f,cooldown;
        Vector3 rest,origin;Quaternion facing;bool initialized;Vector3 nudge;
        void Initialize(){if(initialized)return;initialized=true;rest=transform.localScale;origin=transform.localPosition;facing=transform.localRotation;}
        public void Pop(){Celebrate();}
        public void Celebrate(){Initialize();if(Stretching)return;ReactionIntensity=1;reaction=0;duration=2;cooldown=.6f;nudge=Vector3.zero;ReactionCount++;}
        public void Flick(Vector3 localVelocity)
        {
            Initialize();if(IsPaused?.Invoke()==true)return;ReactionIntensity=1+Mathf.Clamp(localVelocity.magnitude,0,3)*.7f;
            reaction=0;duration=Mathf.Lerp(1.4f,.55f,Mathf.Clamp01(localVelocity.magnitude/3));cooldown=.45f;
            nudge=Vector3.ProjectOnPlane(localVelocity,Vector3.up).normalized;ReactionCount++;
        }
        public bool Nudge(Vector3 worldPoint)
        {
            Initialize();if(Stretching||IsPaused?.Invoke()==true||cooldown>0||Reacting)return false;
            var p=transform.parent.InverseTransformPoint(worldPoint);nudge=origin-p;nudge.y=0;nudge=nudge.sqrMagnitude>.000001f?nudge.normalized:Vector3.right;
            ReactionIntensity=1;reaction=0;duration=1.4f;cooldown=1.6f;ReactionCount++;Interacted?.Invoke();return true;
        }
        public bool Contains(Vector3 point,float padding=0)
        {
            Initialize();var p=transform.parent.InverseTransformPoint(point)-origin;
            float size=Mathf.Max(rest.x,rest.y,rest.z);
            return p.y>=size*.20f-padding&&p.y<=size*1.05f+padding&&new Vector2(p.x,p.z).magnitude<size*.36f+padding;
        }
        void Update()
        {
            Initialize();if(IsPaused?.Invoke()==true)return;
            float dt=Mathf.Min(Time.deltaTime,.033f);
            if(Stretching)stretch=Vector3.Lerp(stretch,stretchTarget,1-Mathf.Exp(-dt*18));
            else{stretchVelocity+=(Vector3.one-stretch)*180*dt;stretchVelocity*=Mathf.Exp(-14*dt);stretch+=stretchVelocity*dt;}
            stretch.x=stretch.z=1;stretchVelocity.x=stretchVelocity.z=0;age+=dt;reaction+=dt;cooldown=Mathf.Max(0,cooldown-dt);
            float t=Mathf.Clamp01(reaction/duration),e=Mathf.Sin(t*Mathf.PI),w=Mathf.Sin(t*Mathf.PI*4)*e;
            float yaw=0,roll=0,pitch=0,hop=0,squash=0;
            switch(CharacterId)
            {
                case 0: hop=Mathf.Abs(w)*.11f;squash=w*.09f;roll=w*7;break;
                case 1: hop=Mathf.Abs(w)*.10f;roll=w*15;yaw=e*18;break;
                case 2: squash=w*.17f;hop=Mathf.Abs(w)*.08f;break;
                case 3: pitch=e*17;roll=w*9;break;
                case 4: yaw=360*Mathf.SmoothStep(0,1,t);hop=e*.08f;break;
                case 5: yaw=w*24;hop=e*.14f;roll=w*4;break;
                case 6: pitch=e*20;yaw=w*14;break;
                case 7: yaw=e*65;pitch=e*18;roll=w*8;break;
                default:squash=w*.19f;roll=w*12;break;
            }
            float entrance=Mathf.SmoothStep(0,1,age/.65f);
            hop*=ReactionIntensity;roll*=ReactionIntensity;pitch*=ReactionIntensity;if(CharacterId!=4)yaw*=ReactionIntensity;squash=Mathf.Clamp(squash*ReactionIntensity,-.45f,.45f);
            transform.localPosition=origin+Vector3.up*(hop*rest.y)+nudge*(e*.075f*rest.y*ReactionIntensity);
            transform.localScale=Vector3.Scale(rest,Vector3.Scale(new Vector3(1+squash,1-squash,1+squash),stretch))*entrance;
            transform.localRotation=facing*Quaternion.Euler(pitch,Mathf.Sin(age*1.3f)*4+yaw,roll);
        }
    }
}
