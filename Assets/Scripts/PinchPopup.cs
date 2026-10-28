using UnityEngine;

public class PinchPopup : MonoBehaviour
{
    public OVRHand leftHand;
    public OVRHand rightHand;
    public GameObject popup;

    public float showThreshold = 0.7f;
    public float hideThreshold = 0.4f;

    void Start()
    {
        if (popup != null) popup.SetActive(false);
    }

    void Update()
    {
        if (popup == null) return;

        float left = leftHand != null ? leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) : 0f;
        float right = rightHand != null ? rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) : 0f;
        float pinch = Mathf.Max(left, right);

        if (!popup.activeSelf && pinch >= showThreshold) popup.SetActive(true);
        else if (popup.activeSelf && pinch < hideThreshold) popup.SetActive(false);
    }
}
