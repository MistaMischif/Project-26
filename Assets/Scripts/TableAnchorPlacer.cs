using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Living Table - pinch over the table to place the board anchor. Restores on launch.
public class TableAnchorPlacer : MonoBehaviour
{
    const string UUID_KEY = "LivingTable_BoardAnchorUuid";

    [Header("Board")]
    [Tooltip("Drag your board / table asset here. It gets parented to the anchor.")]
    public Transform boardMount;

    [Header("Pinch")]
    [Tooltip("Pinch strength (0-1) that counts as a placement pinch.")]
    [Range(0.1f, 1f)]
    public float pinchThreshold = 0.7f;

    OVRHand[] hands;
    bool busy = false;
    bool anchorReady = false;
    bool pinchWasHeld = false;

    async void Start()
    {
        hands = FindObjectsOfType<OVRHand>();
        Debug.Log($"[TableAnchor] Found {hands.Length} hand(s).");

        string saved = PlayerPrefs.GetString(UUID_KEY, "");
        if (!string.IsNullOrEmpty(saved) && Guid.TryParse(saved, out Guid uuid))
            await RestoreAnchor(uuid);
        else
            Debug.Log("[TableAnchor] No saved anchor. PINCH over the table to place the board anchor.");
    }

    void Update()
    {
        if (busy || anchorReady || hands == null) return;

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
            _ = PlaceAnchor(bestPos);
        }
        else if (pinchWasHeld && best < 0.4f)
        {
            pinchWasHeld = false;
        }
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


    async Task PlaceAnchor(Vector3 position)
    {
        busy = true;
        try
        {
            Debug.Log($"[TableAnchor] Pinch detected. Placing anchor at {position}...");
            var go = new GameObject("BoardAnchor");
            go.transform.position = position;

            var anchor = go.AddComponent<OVRSpatialAnchor>();
            while (!anchor.Created) await Task.Yield();

            var saveResult = await anchor.SaveAnchorAsync();
            if (saveResult.Success)
            {
                PlayerPrefs.SetString(UUID_KEY, anchor.Uuid.ToString());
                PlayerPrefs.Save();
                MountBoard(go.transform);
                anchorReady = true;
                Debug.Log($"[TableAnchor] ANCHOR PLACED + SAVED at {position}");
            }
            else
            {
                Debug.LogError("[TableAnchor] Save failed. Look around the room more and pinch again.");
                Destroy(go);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TableAnchor] Placement error: {e.Message}");
        }
        finally
        {
            if (!anchorReady) busy = false;
        }
    }

    async Task RestoreAnchor(Guid uuid)
    {
        busy = true;
        var uuids = new List<Guid> { uuid };
        var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
        await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);
        Debug.Log($"[TableAnchor] Loaded {unboundAnchors.Count} anchor(s).");

        foreach (var unboundAnchor in unboundAnchors)
        {
            if (await unboundAnchor.LocalizeAsync() && unboundAnchor.TryGetPose(out var pose))
            {
                var go = new GameObject("BoardAnchor");
                go.transform.position = pose.position;
                go.transform.rotation = pose.rotation;
                var anchor = go.AddComponent<OVRSpatialAnchor>();
                unboundAnchor.BindTo(anchor);
                MountBoard(go.transform);
                anchorReady = true;
                Debug.Log($"[TableAnchor] BOARD RESTORED at {pose.position}");
            }
            else
            {
                Debug.LogWarning("[TableAnchor] Restore failed - look around the room and relaunch.");
            }
        }
        busy = false;
    }

    void MountBoard(Transform anchorTransform)
    {
        if (boardMount == null)
        {
            Debug.LogWarning("[TableAnchor] Anchor ready, but Board Mount is unassigned - drag your board asset into the Board Mount field.");
            return;
        }
        boardMount.gameObject.SetActive(true);
        boardMount.SetParent(anchorTransform, false);
        boardMount.localPosition = Vector3.zero;
        boardMount.localRotation = Quaternion.identity;
        Debug.Log($"[TableAnchor] Board active={boardMount.gameObject.activeSelf} pos={boardMount.position} scale={boardMount.lossyScale}");
        Debug.Log("[TableAnchor] Board mounted to anchor.");
    }
}
