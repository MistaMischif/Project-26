#if UNITY_EDITOR
using UnityEngine;
namespace LivingTable.Editor
{
 // Feed synthetic tracking in the player loop, not the independently throttled editor loop.
 [DefaultExecutionOrder(-1000)]
 public sealed class CandyTestHandFeed:MonoBehaviour
 {
  public CandyLand.CandyTable table;public int hand;public Vector3 point;public float elapsed;
  void Update(){if(table==null)return;table.UpdateCharacterStretch(hand,point,false);elapsed+=Mathf.Min(Time.deltaTime,.033f);}
 }
}
#endif
