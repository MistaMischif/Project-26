using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LivingTable
{
    // Game-independent presentation: callers supply card artwork and pause state.
    public sealed class TableCardPresentation : MonoBehaviour
    {
        public const float Duration = 2f;
        public int SettledCount { get; private set; }
        public bool IsPresenting { get; private set; }
        public Transform CurrentCard { get; private set; }
        public float LastDuration { get; private set; }
        readonly List<GameObject> cards = new List<GameObject>();
        public IEnumerator Reveal(Transform card, float boardLength, System.Func<bool> paused, float revealSetback=0)
        {
            IsPresenting=true; CurrentCard=card; cards.Add(card.gameObject);
            card.SetParent(transform,false);
            Vector3 start=new Vector3(0,boardLength*.25f,0);
            Vector3 end=new Vector3(boardLength*.5f+.075f,.005f+SettledCount*.0007f,-boardLength*.14f);
            // Deterministic presentation variation never consumes the game's shuffle RNG.
            float skew=SettledCount%4==0?0:Mathf.Sin(SettledCount*2.39996f)*9f;
            if(SettledCount>0)end+=new Vector3(Mathf.Sin(SettledCount*1.7f)*.004f,0,Mathf.Cos(SettledCount*2.3f)*.004f);
            Quaternion resting=Quaternion.Euler(0,skew,0);
            Quaternion upright=Quaternion.Euler(-90,0,0);
            // The table can be placed from either side. Present the printed face to the wearer.
            var facing=Vector3.back;
            if(Camera.main!=null){var viewer=transform.InverseTransformPoint(Camera.main.transform.position)-start;viewer.y=0;if(viewer.sqrMagnitude>.0001f){facing=viewer.normalized;upright=Quaternion.LookRotation(-facing,Vector3.up)*upright;}}
            // A lifted illustration occupies board center; keep its departing card behind it.
            start-=facing*revealSetback;
            float elapsed=0;
            card.localPosition=start;card.localRotation=upright;card.localScale=Vector3.one*2;
            while(elapsed<Duration)
            {
                yield return null;
                if(paused())continue;
                elapsed=Mathf.Min(Duration,elapsed+Time.deltaTime);
                float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.85f)/1.15f));
                card.localPosition=Vector3.Lerp(start,end,t)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*boardLength*.08f);
                card.localRotation=Quaternion.Slerp(upright,resting,t);
                card.localScale=Vector3.one*Mathf.Lerp(2,1,t);
            }
            card.localPosition=end;card.localRotation=resting;card.localScale=Vector3.one;
            SettledCount++;LastDuration=elapsed;IsPresenting=false;
            // Keep exposed artwork on askew lower cards visible.
        }
        public void RestoreSettled(Transform card,float boardLength)
        {
            card.SetParent(transform,false);cards.Add(card.gameObject);float skew=SettledCount%4==0?0:Mathf.Sin(SettledCount*2.39996f)*9f;
            card.localPosition=new Vector3(boardLength*.5f+.075f,.005f+SettledCount*.0007f,-boardLength*.14f);card.localRotation=Quaternion.Euler(0,skew,0);card.localScale=Vector3.one;CurrentCard=card;SettledCount++;
        }
        public void Clear()
        {
            foreach(var card in cards)if(card!=null){card.SetActive(false);Destroy(card);}cards.Clear();
            SettledCount=0;IsPresenting=false;CurrentCard=null;LastDuration=0;
        }
    }
}
