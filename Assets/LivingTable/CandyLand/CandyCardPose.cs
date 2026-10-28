using System;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // Excited illustrated pose bounces during reveal, then settles flat in the discard pile.
    public sealed class CandyCardPose:MonoBehaviour
    {
        public Func<bool> IsPaused;float age;Vector3 rest,position;Quaternion rotation;
        void Start(){rest=transform.localScale;position=transform.localPosition;rotation=transform.localRotation;}
        void Update()
        {
            if(IsPaused?.Invoke()==true)return;age+=Time.deltaTime;
            float envelope=Mathf.Clamp01(1-age/1.8f),wave=Mathf.Sin(age*15)*envelope;
            transform.localScale=Vector3.Scale(rest,new Vector3(1-wave*.055f,1+wave*.08f,1));
            transform.localPosition=position+Vector3.forward*(Mathf.Abs(wave)*.003f);
            transform.localRotation=rotation*Quaternion.Euler(0,0,wave*7);
            if(age>=1.8f){transform.localScale=rest;transform.localPosition=position;transform.localRotation=rotation;enabled=false;}
        }
    }
}
