using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MRBoardMVP
{
    public sealed class MRBoardPlatform : MonoBehaviour
    {
        public TextAsset initialGame;
        public Camera interactionCamera;
        public BoardRuntime board;
        public bool allowDesktopInput = true;
        public GameRegistry Registry { get; private set; }
        public string Status { get; private set; } = "Starting";
        public event Action<string, Vector2Int, Vector2Int> PieceMoved;

        void Awake()
        {
            Registry = new GameRegistry();
            if (interactionCamera == null) interactionCamera = Camera.main;
            if (board == null)
            {
                var anchor = new GameObject("Board Anchor - align to physical board");
                anchor.transform.SetParent(transform, false);
                anchor.transform.localPosition = new Vector3(0, .75f, 0);
                board = anchor.AddComponent<BoardRuntime>();
            }
            board.PieceMoved += OnMoved;
            try
            {
                if (initialGame == null) initialGame = Resources.Load<TextAsset>("demo-game");
                if (initialGame == null) throw new InvalidOperationException("Assign a game JSON asset.");
                var game = Registry.Register(initialGame.text);
                LoadGame(game.id);
            }
            catch (Exception error) { Status = error.Message; Debug.LogException(error, this); }
        }

        public GameDefinition RegisterGame(string json) => Registry.Register(json);
        public void LoadGame(string id)
        {
            board.Build(Registry.Get(id));
            Status = "Select a piece, then an empty cell.";
        }

        // Call with the tracked board pose supplied by your marker/anchor provider.
        // The pose origin is the physical board center, with local Y pointing up.
        public void SetBoardPose(Vector3 worldPosition, Quaternion worldRotation)
        {
            if (board.IsAnimating) return;
            board.transform.SetPositionAndRotation(worldPosition, worldRotation);
        }

        // Bind this to an XR controller trigger or hand-pinch event.
        public void SelectFromPose(Transform pointer)
        {
            if (pointer != null) board.Interact(new Ray(pointer.position, pointer.forward));
        }
        public void SelectFromRay(Vector3 origin, Vector3 direction) => board.Interact(new Ray(origin, direction));

        // Mirror a manually confirmed physical move. No camera-based piece recognition in this MVP.
        public bool ConfirmPhysicalMove(string pieceId, int column, int row)
            => board.TryMove(pieceId, new Vector2Int(column, row));

        void OnMoved(string id, Vector2Int from, Vector2Int to)
        {
            Status = $"{id}: {from.x},{from.y} -> {to.x},{to.y}";
            PieceMoved?.Invoke(id, from, to);
        }

        void Update()
        {
            if (!allowDesktopInput || interactionCamera == null || board.Definition == null) return;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                board.Interact(interactionCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                LoadGame(board.Definition.id);
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0)) board.Interact(interactionCamera.ScreenPointToRay(Input.mousePosition));
            if (Input.GetKeyDown(KeyCode.R)) LoadGame(board.Definition.id);
#endif
        }

        void OnGUI()
        {
            if (!allowDesktopInput) return;
            GUI.Box(new Rect(12, 12, 580, 80), "MR Board MVP");
            GUI.Label(new Rect(24, 38, 550, 24), Status);
            GUI.Label(new Rect(24, 60, 550, 24), "Click piece / click destination | R: reset | XR: bind SelectFromPose");
        }

        void OnDestroy() { if (board != null) board.PieceMoved -= OnMoved; }
    }
}
