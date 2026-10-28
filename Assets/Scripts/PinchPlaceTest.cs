using UnityEngine;

// MINIMAL placement test. No anchors, no saving. Pinch -> board at fingertip.
// Board shows up = the anchor component was the problem.
// Still nothing = the prefab is the problem, anchor code was innocent.
public class PinchPlaceTest : MonoBehaviour
{
    [Tooltip("Drag your board from the HIERARCHY window here.")]
    public Transform boardMount;

    [Range(0.1f, 1f)]
    public float pinchThreshold = 0.7f;

    OVRHand[] hands;
    bool pinchWasHeld = false;

    void Start()
    {
        hands = FindObjectsOfType<OVRHand>();
        Debug.Log($"[PinchTest] Found {hands.Length} hand(s). PINCH to place the board at your fingertip.");
        if (boardMount == null)
            Debug.LogWarning("[PinchTest] Board Mount is empty - drag the board in from the Hierarchy window.");
    }

    void Update()
    {
        if (hands == null) return;

        float best = 0f;
        Vector3 bestPos = Vector3.zero;
        foreach (var hand in hands)
        {
            if (hand == null) continue;
            float s = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            if (s > best) { best = s; bestPos = GetPinchPoint(hand); }
        }

        if (!pinchWasHeld && best >= pinchThreshold)
        {
            pinchWasHeld = true;
            PlaceAt(bestPos);
        }
        else if (pinchWasHeld && best < 0.4f)
        {
            pinchWasHeld = false;
        }
    }

    void PlaceAt(Vector3 pos)
    {
        Debug.Log($"[PinchTest] Pinch at {pos}");
        if (boardMount == null)
        {
            Debug.LogWarning("[PinchTest] No board assigned, nothing to place.");
            return;
        }
        boardMount.gameObject.SetActive(true);
        boardMount.SetParent(transform, false);
        boardMount.localPosition = Vector3.zero;
        boardMount.localRotation = Quaternion.identity;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        Debug.Log($"[PinchTest] Board active={boardMount.gameObject.activeSelf} pos={boardMount.position} scale={boardMount.lossyScale}");
    }

    Vector3 GetPinchPoint(OVRHand hand)
    {
        var skeleton = hand.GetComponent<OVRSkeleton>();
        if (skeleton != null && skeleton.IsInitialized && skeleton.Bones != null)
        {
            Vector3 indexTip = Vector3.zero, thumbTip = Vector3.zero;
            bool hasIndex = false, hasThumb = false;
            foreach (var bone in skeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip) { indexTip = bone.Transform.position; hasIndex = true; }
                else if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip) { thumbTip = bone.Transform.position; hasThumb = true; }
            }
            if (hasIndex && hasThumb) return (indexTip + thumbTip) * 0.5f;
            if (hasIndex) return indexTip;
        }
        return hand.transform.position;
    }
}
