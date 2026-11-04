using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MRBoardMVP;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LivingTable
{
    // XR infrastructure is supplied by the existing Meta rig; all table content is generated.
    public sealed class LivingTableExperience : MonoBehaviour
    {
        const string AnchorKey = "LivingTableMVP_BoardAnchorUuid";
        public bool desktopPreview;
        public bool candyLandMode;
        public bool chessOnlyMode;
        public bool firstStepsMode;
        public bool startWithChess;
        CandyLand.CandyTable candy;
        Chess.ChessTable chess;
        public bool ChessActive=>chess!=null&&chess.gameObject.activeSelf;
        public void SelectChess(){if(mount==null||busy)return;candy?.SetGameVisible(false);if(chess==null){var go=new GameObject("Living Table Chess");go.transform.SetParent(mount,false);go.transform.localPosition=Vector3.up*.012f;chess=go.AddComponent<Chess.ChessTable>();chess.Initialize(desktopPreview);}else chess.gameObject.SetActive(true);}
        public void SelectCandy(){if(chessOnlyMode)return;if(chess!=null)chess.gameObject.SetActive(false);candy?.SetGameVisible(true);}
        public void SetVirtualRoom(bool enabled){foreach(var layer in FindObjectsByType<OVRPassthroughLayer>(FindObjectsSortMode.None))layer.enabled=!enabled;if(Camera.main!=null){Camera.main.clearFlags=CameraClearFlags.SolidColor;Camera.main.backgroundColor=enabled?new Color(.025f,.035f,.055f,1):Color.clear;}}
        public Material surfaceMaterial;
        public Material boardMaterial;
        public string Status { get; private set; } = "Pinch at the center of your table to place the board.";
        MRBoardPlatform platform;
        Transform mount;
        OVRSpatialAnchor spatialAnchor;
        OVRHand[] hands = Array.Empty<OVRHand>();
        readonly Dictionary<OVRHand, bool> held = new Dictionary<OVRHand, bool>();
        readonly List<Material> ownedMaterials = new List<Material>();
        TextMesh caption;
        TextMesh placementPrompt;
        Renderer rim;
        bool busy, placed, destroyed, anchorSaved;
        readonly TableBoardGrip boardGrip=new TableBoardGrip();
        bool gripArmed=true;float gripStartSize;
        public bool BoardHeld=>boardGrip.Active;
        float nextHandScan,roomGrace,farSince=-1;bool roomRecoveryRequested;OVRDisplay watchedDisplay;
        int gameIndex;
        readonly string[] games = { "tabletop-sandbox", "constellation" };
        readonly Rect panel = new Rect(16, 16, 410, 218);

        public static bool BoardOutOfReach(Vector3 eye,Vector3 board)=>Vector3.ProjectOnPlane(eye-board,Vector3.up).magnitude>1.8f||Mathf.Abs(eye.y-board.y)>1.8f;
        public string RegisteredAnchorId=>spatialAnchor!=null&&spatialAnchor.Created?spatialAnchor.Uuid.ToString():null;
        public void RequestRoomRecovery(){if(placed)roomRecoveryRequested=true;}
        void OriginChanging(OVRManager.TrackingOrigin origin,OVRPose? previous)=>RequestRoomRecovery();
        void CheckRoom(){
            if(!desktopPreview&&watchedDisplay==null&&OVRManager.display!=null){watchedDisplay=OVRManager.display;watchedDisplay.RecenteredPose+=RequestRoomRecovery;}
            if(!desktopPreview&&placed&&Time.unscaledTime>roomGrace&&Camera.main!=null){
                if(BoardOutOfReach(Camera.main.transform.position,ChessActive?chess.transform.position:mount.position)){if(farSince<0)farSince=Time.unscaledTime;if(Time.unscaledTime-farSince>.8f)roomRecoveryRequested=true;}else farSince=-1;
            }
            if(roomRecoveryRequested&&placed&&!busy){roomRecoveryRequested=false;if(ChessActive&&chess.BoardPoseLocked){roomGrace=Time.unscaledTime+30;farSince=-1;chess.ReportBoardOutOfReach();}else RecoverRoom();}
        }
        void RecoverRoom(){
            boardGrip.End();gripArmed=false;farSince=-1;
            if(ChessActive){chess.PrepareRoomRecovery();chess.BeginRelocation();}else candy?.BeginRelocation();
            placed=false;mount.gameObject.SetActive(false);mount.SetParent(transform,true);
            if(ChessActive)chess.transform.SetLocalPositionAndRotation(new Vector3(0,.012f,0),Quaternion.identity);
            var old=spatialAnchor;bool saved=anchorSaved;spatialAnchor=null;anchorSaved=false;PlayerPrefs.DeleteKey(AnchorKey);PlayerPrefs.Save();
            foreach(var hand in hands)if(hand!=null)held[hand]=true;
            Status="Room changed or board out of reach. Game held.\nPinch at your new table center to place the board.";
            if(old!=null)ForgetRoomAnchor(old,saved);
        }
        async void ForgetRoomAnchor(OVRSpatialAnchor old,bool saved){try{if(saved)await old.EraseAnchorAsync();}catch(Exception e){Debug.LogWarning("Old room anchor cleanup: "+e.Message);}finally{if(old!=null)Destroy(old.gameObject);}}
        async void Start()
        {
            try
            {
                OVRManager.TrackingOriginChangePending+=OriginChanging;if(firstStepsMode&&GetComponent<Chess.ChessHostMonitor>()==null)gameObject.AddComponent<Chess.ChessHostMonitor>();
                var root = new GameObject("Living Table Platform");
                root.transform.SetParent(transform, false);
                platform = root.AddComponent<MRBoardPlatform>();
                platform.allowDesktopInput = false;
                platform.board.materialTemplate = boardMaterial;
                platform.LoadGame(games[0]);
                platform.RegisterGame(Resources.Load<TextAsset>("living-constellation").text);
                mount = platform.board.transform;
                if (candyLandMode || chessOnlyMode)
                {
                    foreach (Transform child in mount) child.gameObject.SetActive(false);
                    if(chessOnlyMode)SelectChess();
                    else {candy = mount.gameObject.AddComponent<CandyLand.CandyTable>();
                    candy.Initialize(desktopPreview, surfaceMaterial);if(startWithChess)SelectChess();}
                }
                mount.gameObject.SetActive(false);
                if (!desktopPreview)
                {
                    var promptObject = new GameObject("Placement instructions");
                    promptObject.transform.SetParent(transform, false);
                    placementPrompt = promptObject.AddComponent<TextMesh>();
                    placementPrompt.fontSize = 48;
                    placementPrompt.characterSize = .003f;
                    placementPrompt.anchor = TextAnchor.MiddleCenter;
                    placementPrompt.alignment = TextAlignment.Center;
                }
                if (desktopPreview)
                {
                    mount.SetPositionAndRotation(new Vector3(0, .75f, 0), Quaternion.identity);
                    ShowTable();
                    Status = "Desktop preview ready.";
                }
                else if(ChessActive&&chess.HomeScreen){var eye=Camera.main.transform;var forward=Vector3.ProjectOnPlane(eye.forward,Vector3.up).normalized;mount.SetPositionAndRotation(eye.position+forward*.65f-Vector3.up*.35f,Quaternion.LookRotation(forward));ShowTable();}
                else if (Guid.TryParse(PlayerPrefs.GetString(AnchorKey, ""), out var uuid))
                    await Restore(uuid);
            }
            catch (Exception error) { Status = "Table startup failed: " + error.Message; Debug.LogException(error, this); }
        }

        void Update()
        {
            if (platform == null) return;
            if(TableLifecycle.Blocked){if(boardGrip.Active){boardGrip.End();gripArmed=false;anchorDeferred=true;}return;}
            if(anchorDeferred&&!busy){anchorDeferred=false;_ = FinishBoardGrip();return;}
            if(ChessActive&&chess.HomeScreen&&Camera.main!=null){
                // Home is a reachable entry point even if tracking settles after launch or the headset moves.
                var eye=Camera.main.transform;var forward=Vector3.ProjectOnPlane(eye.forward,Vector3.up);
                if(forward.sqrMagnitude<.01f)forward=Vector3.forward;forward.Normalize();
                mount.SetParent(transform,true);mount.SetPositionAndRotation(eye.position+forward*.65f-Vector3.up*.35f,Quaternion.LookRotation(forward));
                placed=true;mount.gameObject.SetActive(true);roomRecoveryRequested=false;farSince=-1;
            }else CheckRoom();
            if (placementPrompt != null)
            {
                placementPrompt.gameObject.SetActive(!placed);
                placementPrompt.text = "LIVING TABLE\n" + Status;
                var eye = Camera.main;
                if (eye != null)
                {
                    placementPrompt.transform.position = eye.transform.position + eye.transform.forward * .65f - Vector3.up * .08f;
                    placementPrompt.transform.rotation = eye.transform.rotation;
                }
            }
            if (rim != null)
                rim.sharedMaterial.color = Color.Lerp(new Color(.03f, .24f, .26f), new Color(.08f, .65f, .58f), .5f + .5f * Mathf.Sin(Time.time * 1.5f));
            if (caption != null)
                caption.text = "LIVING TABLE  /  " + platform.board.Definition.title + "\n" +
                    Status + "\n" + platform.board.Feedback + "   Moves: " + platform.board.MoveCount;
            if (desktopPreview) { if (!candyLandMode) DesktopInput(); return; }
            if (Time.unscaledTime >= nextHandScan)
            {
                hands = FindObjectsByType<OVRHand>(FindObjectsSortMode.None);
                nextHandScan = Time.unscaledTime + 2f;
            }
            if(PollBoardGrip())return;
            foreach (var hand in hands)
            {
                if (hand == null) continue;
                if (!hand.IsTracked || hand.HandConfidence != OVRHand.TrackingConfidence.High || hand.IsSystemGestureInProgress)
                { held[hand] = true; if(ChessActive)chess.ForgetHand(hand.GetInstanceID());else if(candyLandMode&&candy!=null)candy.ForgetDirectHand(hand.GetInstanceID()); continue; } // Require release after tracking returns.
                float strength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
                held.TryGetValue(hand, out bool wasHeld);
                if(ChessActive&&hand.IsPointerPoseValid){bool activate=strength>=.7f&&!wasHeld;if(chess.PointAtMenu(new Ray(hand.PointerPose.position,hand.PointerPose.forward),activate)&&activate){held[hand]=true;continue;}}
                if(ChessActive&&placed)
                {
                    var skeleton=hand.GetComponent<OVRSkeleton>();if(skeleton!=null&&skeleton.IsInitialized&&skeleton.Bones!=null)foreach(var bone in skeleton.Bones){if(IsObservedJoint(skeleton,bone.Id))chess.ObserveHand(bone.Transform.position);if(!busy&&bone.Id==HandJoint(skeleton.GetSkeletonType(),1))chess.DirectPress(hand.GetInstanceID(),bone.Transform.position);}
                    bool valid=TryPinchPoint(hand,out var cp,out float fingerGap);
                    if(!valid||busy){chess.ForgetHand(hand.GetInstanceID());held[hand]=strength>=.4f;continue;}
                    chess.HoverPiece(cp);
                    if(chess.UpdateSetupGrab(hand.GetInstanceID(),cp,strength<.4f)||chess.UpdateBoardResize(hand.GetInstanceID(),cp,strength<.4f)){held[hand]=strength>=.4f;continue;}
                    if(chess.UpdateGrab(hand.GetInstanceID(),cp,!PiecePinchActive(fingerGap,true))){held[hand]=PiecePinchActive(fingerGap,true);continue;}
                    if(!wasHeld&&PiecePinchActive(fingerGap,false)&&chess.BeginGrab(hand.GetInstanceID(),cp)){held[hand]=true;continue;}
                    if(strength<.4f)held[hand]=false;else if(strength>=.7f&&!wasHeld){held[hand]=true;if(valid&&!busy&&!chess.BeginSetupGrab(hand.GetInstanceID(),cp)&&!chess.BeginBoardResize(hand.GetInstanceID(),cp)&&!chess.BeginGrab(hand.GetInstanceID(),cp))chess.TouchWorld(cp);}continue;
                }
                if(candyLandMode&&placed&&candy!=null)
                {
                    var visionSkeleton=hand.GetComponent<OVRSkeleton>();
                    if(visionSkeleton!=null&&visionSkeleton.IsInitialized&&visionSkeleton.Bones!=null)foreach(var bone in visionSkeleton.Bones)if(IsObservedJoint(visionSkeleton,bone.Id))candy.ObservePhysicalHand(hand.GetInstanceID()*32+(int)bone.Id,bone.Transform.position);
                    bool valid=TryPinchPoint(hand,out var stretchPoint);
                    if((candy.TreatBucket!=null&&candy.TreatBucket.UpdateGrab(hand.GetInstanceID(),stretchPoint,strength<.4f||!valid||busy))||candy.UpdatePieceGrab(hand.GetInstanceID(),stretchPoint,strength<.4f||!valid||busy)||candy.UpdateCharacterStretch(hand.GetInstanceID(),stretchPoint,strength<.4f||!valid||busy)){held[hand]=strength>=.4f;continue;}
                    if(!busy&&valid&&strength>=.7f&&!wasHeld&&((candy.TreatBucket!=null&&candy.TreatBucket.BeginGrab(hand.GetInstanceID(),stretchPoint))||candy.BeginPieceGrab(hand.GetInstanceID(),stretchPoint)||candy.BeginCharacterStretch(hand.GetInstanceID(),stretchPoint))){held[hand]=true;continue;}
                }
                if(candyLandMode && placed && !busy)
                {
                    if(TryPinchPoint(hand,out var hoverPoint))candy.HoverWorld(hoverPoint);
                    var skeleton=hand.GetComponent<OVRSkeleton>();
                    if(skeleton!=null&&skeleton.IsInitialized&&skeleton.Bones!=null)
                        foreach(var bone in skeleton.Bones)if(bone.Id==HandJoint(skeleton.GetSkeletonType(),1)){candy.HoverWorld(bone.Transform.position);candy.DirectPressWorld(hand.GetInstanceID(),bone.Transform.position);break;}
                }
                if (strength < .4f) held[hand] = false;
                else if (strength >= .7f && !wasHeld)
                {
                    held[hand] = true;
                    if (!busy && TryPinchPoint(hand, out var point))
                    {
                        if (!placed) { if(!ChessActive||!chess.TryPersistentPanel(point)) _ = Place(point); }
                        else TouchTable(point);
                    }
                }
            }
        }

        bool PollBoardGrip()
        {
            if(!ChessActive||!placed||busy)return false;
            OVRHand a=null,b=null;Vector3 pa=default,pb=default;float sa=0,sb=0;
            foreach(var hand in hands){
                if(hand==null||!hand.IsTracked||hand.HandConfidence!=OVRHand.TrackingConfidence.High||hand.IsSystemGestureInProgress||!TryPinchPoint(hand,out var point))continue;
                if(a==null){a=hand;pa=point;sa=hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);}
                else if(b==null){b=hand;pb=point;sb=hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);}
            }
            if(a!=null&&b!=null&&a.GetInstanceID()>b.GetInstanceID()){var h=a;a=b;b=h;var p=pa;pa=pb;pb=p;float v=sa;sa=sb;sb=v;}
            bool valid=a!=null&&b!=null;
            bool consumed=UpdateBoardGrip(pa,pb,valid&&sa>=(boardGrip.Active ? .4f : .7f)&&sb>=(boardGrip.Active ? .4f : .7f),valid&&sa<.4f&&sb<.4f);
            if(consumed)foreach(var hand in hands)if(hand!=null){held[hand]=true;chess.ForgetHand(hand.GetInstanceID());}
            return consumed;
        }

        public bool UpdateBoardGrip(Vector3 a,Vector3 b,bool pinching,bool released=false)
        {
            if(!pinching){if(released)gripArmed=true;if(!boardGrip.Active)return false;boardGrip.End();gripArmed=false;_ = FinishBoardGrip();return true;}
            if(!placed||busy||!ChessActive)return false;
            if(boardGrip.Active){boardGrip.Follow(mount,a,b);chess.ScaleHeldBoard(gripStartSize*boardGrip.Scale);return true;}
            if(chess.BoardPoseLocked&&chess.EntryPhase!=3||!gripArmed||chess.Busy||chess.StorageBlocked||platform.board.IsAnimating||chess.HomeScreen||!boardGrip.BeginCorners(mount,chess.Size,a,b,chess.transform))return false;
            gripStartSize=chess.Size;chess.BeginCornerAdjustment();mount.SetParent(transform,true);Status="Hold two corner knobs to move, resize or turn. Release to place.";return true;
        }

        bool anchorDeferred;
        async Task FinishBoardGrip()
        {
            if(TableLifecycle.Blocked){anchorDeferred=true;return;}
            busy=true;var position=mount.position;var rotation=mount.rotation;
            try{
                var previous=spatialAnchor;bool saved=anchorSaved;spatialAnchor=null;anchorSaved=false;
                PlayerPrefs.DeleteKey(AnchorKey);PlayerPrefs.Save();
                if(previous!=null){try{if(saved)await previous.EraseAnchorAsync();}catch(Exception error){Debug.LogWarning("[Living Table] Old anchor cleanup: "+error.Message);}finally{if(previous!=null)Destroy(previous.gameObject);}}
                if(!destroyed){if(TableLifecycle.Blocked)anchorDeferred=true;else await Place(position,rotation);}
            }finally{if(!destroyed){chess?.EndRelocation();busy=false;}}
        }

        void OnApplicationPause(bool paused){if(!paused){roomGrace=Time.unscaledTime+1f;farSince=-1;}if(paused&&boardGrip.Active){boardGrip.End();gripArmed=false;anchorDeferred=true;}}
        public async Task AnchorChessCalibration(){
            if(busy||!ChessActive||!chess.BoardPoseLocked)return;
            // Re-root at the calibrated surface, preserving its full XYZ pose instead of the old table anchor offset.
            var position=chess.transform.position;var rotation=chess.transform.rotation;
            mount.SetParent(transform,true);mount.SetPositionAndRotation(position,rotation);
            chess.transform.SetLocalPositionAndRotation(Vector3.zero,Quaternion.identity);
            await FinishBoardGrip();if(!destroyed)chess.SaveAnchoredRegistration();
        }
        public Task RepositionBoard()=>Reposition();
        public Task PlaceBoard(Vector3 position,Quaternion rotation)=>Place(position,rotation);

        public static OVRSkeleton.BoneId HandJoint(OVRSkeleton.SkeletonType type,int joint)
        {
            bool xr=type==OVRSkeleton.SkeletonType.XRHandLeft||type==OVRSkeleton.SkeletonType.XRHandRight;
            if(joint==0)return xr?OVRSkeleton.BoneId.XRHand_ThumbTip:OVRSkeleton.BoneId.Hand_ThumbTip;
            if(joint==1)return xr?OVRSkeleton.BoneId.XRHand_IndexTip:OVRSkeleton.BoneId.Hand_IndexTip;
            if(joint==2)return xr?OVRSkeleton.BoneId.XRHand_Wrist:OVRSkeleton.BoneId.Hand_WristRoot;
            return xr?OVRSkeleton.BoneId.XRHand_LittleTip:OVRSkeleton.BoneId.Hand_PinkyTip;
        }
        static bool IsObservedJoint(OVRSkeleton skeleton,OVRSkeleton.BoneId id)=>id==HandJoint(skeleton.GetSkeletonType(),1)||id==HandJoint(skeleton.GetSkeletonType(),2)||id==HandJoint(skeleton.GetSkeletonType(),3);
        public static bool PiecePinchActive(float gap,bool holding)=>float.IsFinite(gap)&&gap>=0&&gap<=(holding?.05f:.035f);
        static bool TryPinchPoint(OVRHand hand,out Vector3 point)=>TryPinchPoint(hand,out point,out _);
        static bool TryPinchPoint(OVRHand hand, out Vector3 point,out float gap)
        {
            point = default;gap=float.PositiveInfinity;
            var skeleton = hand.GetComponent<OVRSkeleton>();
            if (skeleton == null || !skeleton.IsInitialized || !skeleton.IsDataValid || !skeleton.IsDataHighConfidence || skeleton.Bones == null) return false;
            Transform index = null, thumb = null;
            foreach (var bone in skeleton.Bones)
            {
                if (bone.Id == HandJoint(skeleton.GetSkeletonType(),1)) index = bone.Transform;
                if (bone.Id == HandJoint(skeleton.GetSkeletonType(),0)) thumb = bone.Transform;
            }
            if (index == null || thumb == null || Vector3.Distance(index.position,thumb.position)>.12f) return false;
            gap=Vector3.Distance(index.position,thumb.position);
            point = (index.position + thumb.position) * .5f;
            return true;
        }

        void DesktopInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame || Camera.main == null) return;
            Vector2 pos = Mouse.current.position.ReadValue();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (!Input.GetMouseButtonDown(0) || Camera.main == null) return;
            Vector2 pos = Input.mousePosition;
#else
            return;
#endif
#if ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (panel.Contains(new Vector2(pos.x, Screen.height - pos.y))) return;
            platform.board.Interact(Camera.main.ScreenPointToRay(pos));
#endif
        }

        void TouchTable(Vector3 point)
        {
            if (ChessActive) { chess.TouchWorld(point); return; }
            if (candyLandMode) { candy.TouchWorld(point); return; }
            var p = mount.InverseTransformPoint(point);
            // Reject pinches well above/below the calibrated tabletop.
            if (p.y < -.04f || p.y > .16f) return;
            var d = platform.board.Definition;
            float half = d.columns * d.cellSize * .5f;
            float depth = d.rows * d.cellSize * .5f;
            if (p.z < -depth - .025f && p.z > -depth - .085f && Mathf.Abs(p.x) < half)
            {
                int action = Mathf.Clamp(Mathf.FloorToInt((p.x + half) / (half * 2f / 3f)), 0, 2);
                if (action == 0) SwitchGame();
                else if (action == 1) ResetGame();
                else _ = Reposition();
                return;
            }
            if (p.x < -half || p.x >= half || p.z < -depth || p.z >= depth) return;
            platform.board.InteractCell(new Vector2Int(Mathf.FloorToInt((p.x + half) / d.cellSize), Mathf.FloorToInt((p.z + depth) / d.cellSize)));
        }

        async Task Place(Vector3 position,Quaternion? orientation=null)
        {
            busy = true;
            GameObject anchorObject = null;
            Status = "Saving table position...";
            if(desktopPreview){mount.SetPositionAndRotation(position,orientation??Quaternion.identity);ShowTable();busy=false;return;}
            try
            {
                anchorObject = new GameObject("Living Table Spatial Anchor");
                var camera = Camera.main;
                float yaw = camera != null ? camera.transform.eulerAngles.y : 0;
                anchorObject.transform.SetPositionAndRotation(position, orientation??Quaternion.Euler(0, yaw, 0));
                var anchor = anchorObject.AddComponent<OVRSpatialAnchor>();
                float deadline = Time.realtimeSinceStartup + 12f;
                while (!anchor.Created && Time.realtimeSinceStartup < deadline && !destroyed) await Task.Yield();
                if (destroyed) { Destroy(anchorObject); return; }
                if (!anchor.Created) throw new InvalidOperationException("Anchor creation timed out. Look around the room and retry.");
                var result = await anchor.SaveAnchorAsync();
                if (destroyed) { Destroy(anchorObject); return; }
                spatialAnchor = anchor;
                anchorSaved = result.Success;
                mount.SetParent(anchor.transform, false);
                mount.localPosition = Vector3.zero;
                mount.localRotation = Quaternion.identity;
                ShowTable();
                if (result.Success)
                {
                    PlayerPrefs.SetString(AnchorKey, anchor.Uuid.ToString());
                    PlayerPrefs.Save();
                    Status = "Position saved. Pinch a piece, then an empty cell.";
                }
                else Status = "Session only: position could not be saved. Use Move table to retry.";
            }
            catch (Exception error)
            {
                if (anchorObject != null && (spatialAnchor==null||spatialAnchor.gameObject!=anchorObject)) Destroy(anchorObject);
                Status = error.Message;
                Debug.LogWarning("[Living Table] " + error.Message);
            }
            finally { busy = false; }
        }

        async Task Restore(Guid uuid)
        {
            busy = true;
            Status = "Looking for your saved table. Look around the room...";
            GameObject restored = null;
            try
            {
                var found = new List<OVRSpatialAnchor.UnboundAnchor>();
                await OVRSpatialAnchor.LoadUnboundAnchorsAsync(new[] { uuid }, found);
                if (destroyed) return;
                foreach (var candidate in found)
                {
                    bool localized = await candidate.LocalizeAsync();
                    if (destroyed) return;
                    if (!localized || !candidate.TryGetPose(out var pose)) continue;
                    restored = new GameObject("Living Table Restored Anchor");
                    restored.transform.SetPositionAndRotation(pose.position, pose.rotation);
                    spatialAnchor = restored.AddComponent<OVRSpatialAnchor>();
                    candidate.BindTo(spatialAnchor);
                    anchorSaved = true;
                    mount.SetParent(restored.transform, false);
                    mount.localPosition = Vector3.zero;
                    mount.localRotation = Quaternion.identity;
                    ShowTable();
                    Status = "Table restored. Pinch a piece, then an empty cell.";
                    return;
                }
                Status = "Saved table not found. Pinch to place a new table.";
            }
            catch (Exception error)
            {
                if (restored != null && !placed) Destroy(restored);
                Status = "Restore unavailable. Pinch to place a new table.";
                Debug.LogWarning("[Living Table] " + error.Message);
            }
            finally { busy = false; }
        }

        void ShowTable()
        {
            placed = true;roomGrace=Time.unscaledTime+1.5f;farSince=-1;
            mount.gameObject.SetActive(true);
            if(ChessActive)chess.EndRelocation();else if(candyLandMode&&candy!=null)candy.EndRelocation();
            if (!candyLandMode) BuildTrim();
        }

        public void MoveCandyTable() { if (candyLandMode||ChessActive) _ = Reposition(); }

        public void SwitchGame()
        {
            if (!placed || busy || platform.board.IsAnimating) return;
            gameIndex = (gameIndex + 1) % games.Length;
            platform.LoadGame(games[gameIndex]);
            Status = "Layout changed. Select a piece to begin.";
        }

        public void ResetGame()
        {
            if (!placed || busy) return;
            platform.LoadGame(games[gameIndex]);
            Status = "Pieces reset to their starting positions.";
        }

        async Task Reposition()
        {
            if (!placed || busy || boardGrip.Active || platform.board.IsAnimating || (ChessActive&&chess.Busy)) return;
            busy = true;
            if(ChessActive){chess.UnlockBoardForMove();chess.BeginRelocation();}else if(candyLandMode&&candy!=null)candy.BeginRelocation();
            placed=false;mount.gameObject.SetActive(false);Status="Preparing a new placement...";
            try
            {
                if (spatialAnchor != null)
                {
                    if (anchorSaved)
                    {
                        var result = await spatialAnchor.EraseAnchorAsync();
                        if (destroyed) return;
                        if (!result.Success) { ShowTable();Status = "Could not forget saved position. Try Move table again."; return; }
                    }
                    mount.SetParent(transform, true);
                    Destroy(spatialAnchor.gameObject);
                    spatialAnchor = null;
                    anchorSaved = false;
                }
                PlayerPrefs.DeleteKey(AnchorKey);
                PlayerPrefs.Save();
                placed = false;
                mount.gameObject.SetActive(false);
                if(ChessActive)chess.transform.SetLocalPositionAndRotation(new Vector3(0,.012f,0),Quaternion.identity);
                Status = "Pinch at the new table center to place it.";
            }
            catch (Exception error) { ShowTable();Status = "Could not move table: " + error.Message; }
            finally { busy = false; }
        }

        Material MakeMaterial(Color color)
        {
            var material = surfaceMaterial != null ? new Material(surfaceMaterial) : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = color;
            ownedMaterials.Add(material);
            return material;
        }

        void BuildTrim()
        {
            if (rim != null) return;
            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "Breathing table frame";
            slab.transform.SetParent(mount, false);
            slab.transform.localPosition = new Vector3(0, -.017f, 0);
            slab.transform.localScale = new Vector3(.40f, .018f, .40f);
            Destroy(slab.GetComponent<Collider>());
            rim = slab.GetComponent<Renderer>();
            rim.sharedMaterial = MakeMaterial(new Color(.04f, .4f, .35f));
            caption = Label("Table instructions", new Vector3(-.18f, .002f, .26f), "", .0035f);
            string[] labels = { "NEXT GAME", "RESET", "MOVE TABLE" };
            for (int i = 0; i < 3; i++)
            {
                var button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                button.name = labels[i];
                button.transform.SetParent(mount, false);
                button.transform.localPosition = new Vector3((i - 1) * .12f, -.002f, -.235f);
                button.transform.localScale = new Vector3(.112f, .006f, .052f);
                Destroy(button.GetComponent<Collider>());
                button.GetComponent<Renderer>().sharedMaterial = MakeMaterial(new Color(.035f, .08f, .12f));
                Label(labels[i], new Vector3((i - 1) * .12f - .048f, .003f, -.225f), labels[i], .0025f);
            }
        }

        TextMesh Label(string title, Vector3 position, string text, float size)
        {
            var go = new GameObject(title);
            go.transform.SetParent(mount, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var label = go.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 48;
            label.characterSize = size;
            label.anchor = TextAnchor.UpperLeft;
            label.color = new Color(.72f, .94f, .90f);
            return label;
        }

        void OnGUI()
        {
            if (candyLandMode || !desktopPreview || platform == null) return;
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label("LIVING TABLE  /  desktop preview");
            GUILayout.Label(platform.board.Definition.title + "  •  Moves: " + platform.board.MoveCount);
            GUILayout.Label("Click a piece, then an empty cell.");
            GUILayout.Label(platform.board.Feedback);
            GUILayout.Label(Status);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Next game", GUILayout.Height(32))) SwitchGame();
            if (GUILayout.Button("Reset pieces", GUILayout.Height(32))) ResetGame();
            GUILayout.EndHorizontal();
            GUILayout.Label("Local tabletop sandbox • no turns or capture rules");
            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            destroyed = true;OVRManager.TrackingOriginChangePending-=OriginChanging;if(watchedDisplay!=null)watchedDisplay.RecenteredPose-=RequestRoomRecovery;
            if (spatialAnchor != null) Destroy(spatialAnchor.gameObject);
            foreach (var material in ownedMaterials) if (material != null) Destroy(material);
        }
    }
}
