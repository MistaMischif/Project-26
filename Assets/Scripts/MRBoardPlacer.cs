using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// Living Table MVP — fresh start, zero wiring.
//
// One GameObject + this script = the whole MVP. No prefabs, no slots,
// no fields to drag. The board is created in code (a real mesh, guaranteed),
// the hands are found in code, the anchor is created in code. Nothing in
// the scene needs to exist beforehand except the Camera Rig, Passthrough,
// and Hand Tracking blocks.
//
// What it does:
//   1. Launch: tries to restore a saved board anchor (same room only).
//   2. Pinch over a table: creates a board at your hand, anchored + saved.
//   3. Pinch over the board: spawns a piece snapped to the 8x8 grid.
//   4. The board pulses gently — the living layer, minimal version.
//   5. Quit + relaunch (same room): the board comes back. Pieces do not (MVP limit).
//
// Built from two things already proven on your headset: the Oct 7 anchor
// code (anchor created + saved + restored on device) and the Oct 24 popup
// pinch pattern (GetFingerPinchStrength, 0.7 / 0.4, both hands).
public class MRBoardPlacer : MonoBehaviour
{
    // Fresh key on purpose: no Oct 7 saved anchor can interfere.
    const string UUID_KEY = "LivingTableMVP_BoardAnchorUuid";

    [Header("Board")]
    public float boardSize = 0.6f;       // 60 cm square
    public float boardThickness = 0.02f; // 2 cm thick
    public int gridCells = 8;

    [Header("Pinch")]
    [Range(0.1f, 1f)]
    public float pinchThreshold = 0.7f;
    public float pinchRearm = 0.4f;

    OVRHand[] hands;
    Transform anchorTransform;
    GameObject board;
    Renderer boardRenderer;
    Color boardBaseColor = new Color(0.15f, 0.35f, 0.85f);
    bool busy = false;
    bool boardPlaced = false;
    bool pinchWasHeld = false;
    int pieceCount = 0;

    async void Start()
    {
        RefreshHands();
        Debug.Log($"[MRBoard] Found {hands.Length} hand(s).");

        string saved = PlayerPrefs.GetString(UUID_KEY, "");
        if (!string.IsNullOrEmpty(saved) && Guid.TryParse(saved, out Guid uuid))
            await RestoreBoard(uuid);
        else
            Debug.Log("[MRBoard] No saved board. PINCH over a table to place it.");
    }

    void RefreshHands()
    {
        hands = FindObjectsOfType<OVRHand>();
    }

    void Update()
    {
        if (hands == null || hands.Length == 0) RefreshHands();
        if (busy || hands == null) return;

        // Strongest pinch across both hands, and that hand's position.
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

        // Living layer: gentle pulse once the board exists.
        if (boardPlaced && boardRenderer != null)
        {
            float glow = 0.5f + 0.5f * Mathf.Sin(Time.time * 3f);
            boardRenderer.material.color = Color.Lerp(boardBaseColor, Color.white, 0.18f * glow);
        }
    }

    void OnPinch(Vector3 pinchPos)
    {
        if (!boardPlaced)
        {
            _ = PlaceBoard(pinchPos);
            return;
        }
        TrySpawnPiece(pinchPos);
    }

    async Task PlaceBoard(Vector3 position)
    {
        busy = true;
        try
        {
            Debug.Log($"[MRBoard] Pinch detected. Placing board at {position}...");
            var anchorGo = new GameObject("BoardAnchor");
            anchorGo.transform.position = position;

            var anchor = anchorGo.AddComponent<OVRSpatialAnchor>();
            int waited = 0;
            while (!anchor.Created && waited < 300) { await Task.Yield(); waited++; }
            if (!anchor.Created)
            {
                Debug.LogError("[MRBoard] Anchor never became ready.");
                Destroy(anchorGo);
                return;
            }

            var saveResult = await anchor.SaveAnchorAsync();
            if (saveResult.Success)
            {
                PlayerPrefs.SetString(UUID_KEY, anchor.Uuid.ToString());
                PlayerPrefs.Save();
                BuildBoard(anchorGo.transform);
                boardPlaced = true;
                Debug.Log($"[MRBoard] BOARD PLACED + SAVED at {position}");
            }
            else
            {
                Debug.LogError("[MRBoard] Anchor save failed. Look around the room more and pinch again.");
                Destroy(anchorGo);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[MRBoard] Placement error: {e.Message}");
        }
        finally
        {
            // Deliberate difference from the Oct 7 script: Update must keep
            // running after placement, for piece spawning and the pulse.
            busy = false;
        }
    }

    async Task RestoreBoard(Guid uuid)
    {
        busy = true;
        try
        {
            var uuids = new List<Guid> { uuid };
            var unboundAnchors = new List<OVRSpatialAnchor.UnboundAnchor>();
            await OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, unboundAnchors);
            Debug.Log($"[MRBoard] Loaded {unboundAnchors.Count} anchor(s).");

            foreach (var unboundAnchor in unboundAnchors)
            {
                if (await unboundAnchor.LocalizeAsync() && unboundAnchor.TryGetPose(out var pose))
                {
                    var anchorGo = new GameObject("BoardAnchor");
                    anchorGo.transform.position = pose.position;
                    anchorGo.transform.rotation = pose.rotation;
                    var anchor = anchorGo.AddComponent<OVRSpatialAnchor>();
                    unboundAnchor.BindTo(anchor);
                    BuildBoard(anchorGo.transform);
                    boardPlaced = true;
                    Debug.Log($"[MRBoard] BOARD RESTORED at {pose.position}");
                }
                else
                {
                    Debug.LogWarning("[MRBoard] Restore failed - pinch to place a fresh board.");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[MRBoard] Restore error: {e.Message}");
        }
        finally
        {
            busy = false;
        }
    }

    // Builds the board AND its grid lines as children of the anchor.
    // Anchor scale is 1, so local sizes here are real-world sizes.
    // CreatePrimitive guarantees MeshFilter + MeshRenderer: no empty objects possible.
    void BuildBoard(Transform anchor)
    {
        anchorTransform = anchor;

        board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "MRBoard";
        board.transform.SetParent(anchor, false);
        board.transform.localPosition = Vector3.zero;
        board.transform.localRotation = Quaternion.identity;
        board.transform.localScale = new Vector3(boardSize, boardThickness, boardSize);
        boardRenderer = board.GetComponent<Renderer>();
        boardRenderer.material.color = boardBaseColor;
        board.SetActive(true);

        float cell = boardSize / gridCells;
        float half = boardSize / 2f;
        float lineY = boardThickness / 2f + 0.0015f;
        for (int i = 0; i <= gridCells; i++)
        {
            float p = -half + i * cell;
            CreateLine(anchor, new Vector3(boardSize, 0.002f, 0.004f), new Vector3(0f, lineY, p));
            CreateLine(anchor, new Vector3(0.004f, 0.002f, boardSize), new Vector3(p, lineY, 0f));
        }

        Debug.Log($"[MRBoard] Board built: active={board.activeSelf} pos={board.transform.position} scale={board.transform.lossyScale}");
    }

    void CreateLine(Transform parent, Vector3 scale, Vector3 pos)
    {
        var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = "GridLine";
        line.transform.SetParent(parent, false);
        line.transform.localPosition = pos;
        line.transform.localRotation = Quaternion.identity;
        line.transform.localScale = scale;
        line.GetComponent<Renderer>().material.color = new Color(0.05f, 0.05f, 0.08f);
    }

    void TrySpawnPiece(Vector3 pinchPos)
    {
        if (anchorTransform == null) return;
        Vector3 local = anchorTransform.InverseTransformPoint(pinchPos);
        float half = boardSize / 2f;
        if (Mathf.Abs(local.x) > half || Mathf.Abs(local.z) > half) return; // pinch not over the board: ignore

        float cell = boardSize / gridCells;
        int cx = Mathf.Clamp(Mathf.FloorToInt((local.x + half) / cell), 0, gridCells - 1);
        int cz = Mathf.Clamp(Mathf.FloorToInt((local.z + half) / cell), 0, gridCells - 1);

        var piece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        piece.name = "MRPiece";
        piece.transform.SetParent(anchorTransform, false);
        piece.transform.localPosition = new Vector3(
            -half + (cx + 0.5f) * cell,
            boardThickness / 2f + 0.01f,
            -half + (cz + 0.5f) * cell);
        piece.transform.localRotation = Quaternion.identity;
        piece.transform.localScale = new Vector3(0.06f, 0.01f, 0.06f);
        piece.GetComponent<Renderer>().material.color =
            (pieceCount % 2 == 0) ? new Color(1f, 0.55f, 0.1f) : new Color(0.1f, 0.8f, 0.9f);
        pieceCount++;
        Debug.Log($"[MRBoard] Piece {pieceCount} at cell ({cx},{cz}).");
    }
}
