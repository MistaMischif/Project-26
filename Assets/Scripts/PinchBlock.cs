using UnityEngine;

// PinchBlock — the simplest possible test.
// No anchor, no restore, no grid, no pieces, no pulse.
// First pinch creates ONE cube at the hand. Nothing ever moves it afterward.
// Later pinches are ignored, so a second pinch can never create a second block.
public class PinchBlock : MonoBehaviour
{
    public float pinchThreshold = 0.7f;
    public float pinchRearm = 0.4f;
    public float blockSize = 0.3f;

    OVRHand[] hands;
    bool placed = false;
    bool pinchWasHeld = false;

    void Start()
    {
        hands = FindObjectsOfType<OVRHand>();
        Debug.Log($"[PinchBlock] Found {hands.Length} hand(s). Pinch to place one block.");
    }

    void Update()
    {
        if (hands == null || hands.Length == 0) hands = FindObjectsOfType<OVRHand>();
        if (hands == null) return;

        float best = 0f;
        Vector3 bestPos = Vector3.zero;
        foreach (var hand in hands)
        {
            if (hand == null) continue;
            float s = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            if (s > best) { best = s; bestPos = hand.transform.position; }
        }

        if (!pinchWasHeld && best >= pinchThreshold)
        {
            pinchWasHeld = true;
            OnPinch(bestPos);
        }
        else if (pinchWasHeld && best < pinchRearm)
        {
            pinchWasHeld = false;
        }
    }

    void OnPinch(Vector3 pos)
    {
        if (placed)
        {
            Debug.Log("[PinchBlock] Block already placed, ignoring pinch.");
            return;
        }
        placed = true;

        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = "PinchBlock";
        block.transform.position = pos;
        block.transform.rotation = Quaternion.identity;
        block.transform.localScale = Vector3.one * blockSize;

        var r = block.GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null)
        {
            var mat = new Material(shader);
            Color green = new Color(0.1f, 0.8f, 0.2f);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", green);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", green);
            r.material = mat;
            Debug.Log($"[PinchBlock] Block placed at {pos} with shader {shader.name}.");
        }
        else
        {
            Debug.LogWarning("[PinchBlock] URP shaders not found, using default material.");
            Debug.Log($"[PinchBlock] Block placed at {pos} with default material.");
        }
        Debug.Log($"[PinchBlock] Block active={block.activeSelf} pos={block.transform.position} scale={block.transform.lossyScale}");
    }
}
