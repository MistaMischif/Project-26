# Physical checkerboard calibration (0.1.12)

Status: implemented, verification in progress. 0.1.11 remains the rollback release. No hardware accuracy claim follows from synthetic checks.

## Scope and feasibility
Unity 6000.3.9f1, MRUK 207.0.1, Meta Interaction 207.0.0, OpenXR 1.18.0; standalone Quest 3. Existing Android manifest requests horizonos.permission.HEADSET_CAMERA; that permission was granted on the test headset. Runtime still checks camera support and permission. Passthrough rendering alone is not evidence of frame access. No paid API, native plugin, model, service or SDK upgrade is introduced by this calibration replacement.

Stationary 8x8 tabletop boards, 7x7 internal intersections, 20–120 cm playing-area width. Square aspect ratio is 1:1; physical square length is never a global constant or a user-entered requirement. Depth or tracked-view triangulation supplies metric scale. Walls, moving boards and arbitrary surfaces are outside this prototype's supported scope.

## Components and coordinate chain
- ChessCameraCalibration owns frame access, depth sampling, capture cancellation, states and the three-observation acceptance sequence.
- ChessCheckerFit supplies only a coarse lattice hypothesis. Its appearance score is not sufficient to accept calibration.
- ChessSurfaceSolver refines image corners by gradient intersection, checks alternating quadrants and coverage, triangulates or fits depth correspondences, and rejects non-square/inconsistent poses.
- ChessAlignment manages preview, explicit ACCEPT BOARD, assisted fallback and floating progress. Existing ChessTable remains the common grid/piece/guide root.

Board X follows files, Z follows ranks, Y is the upward surface normal. Unity world coordinates and camera poses are metres, left-handed. MRUK viewport coordinates have a bottom-left origin. The board point is `(fileOffset/8, 0, rankOffset/8) * boardWidth`. Its world point is `worldCenter + worldRotation * boardPoint`; projection uses the inverse captured camera rotation and the SDK-equivalent viewport intrinsics. MRUK GetCameraPose supplies the camera pose at its image timestamp and includes LensOffset; do not apply camera extrinsics a second time.

Capture uses direct AsyncGPUReadback rather than Graphics.Blit, which the installed SDK warns may copy the previous texture frame. A saved image timestamp, captured camera pose, ray-derived cropped intrinsics and hand timestamp accompany each image. The full image is CPU-subsampled with the same aspect ratio and pixel endpoints; no additional crop or flip is introduced. The public CameraIntrinsics struct exposes no distortion coefficients. This implementation follows the SDK pinhole/ray projection; it does not invent lens coefficients or claim calibrated edge accuracy. Edge reprojection and actual camera orientation require device measurement.

Depth correspondences use world rays through detected corners and reject non-table-facing hits. The alternative intersects matched corner rays from tracked viewpoints, requiring at least 4.5 cm camera translation and adequate ray angle. Pure head rotation is insufficient. Fit requires at least 16 corners across both axes and all quadrants, square/orthogonal axes, <=6 mm 3D fit RMS and <=2.5 px image RMS. These are rejection thresholds, NOT measured product accuracy. Three fits must agree within 8 mm center/size and 1.2 degrees before review. Their center, orientation and size are averaged, then the averaged pose must also pass image reprojection. Outliers, hands, cancellation and failed detections do not commit a pose.

A symmetric board cannot determine a1 semantically. The solver uses the existing selected orientation as its initial correspondence ordering; ROTATE 90 and chosen player color remain the explicit disambiguation. No automatic mirrored or normal-inverted solution is accepted. An external asymmetric marker is not required or claimed.

## Workflow, controls and persistence
ALIGN BOARD and completed physical knob adjustment start camera refinement. Four-corner input remains as an approximate seed and assisted fallback. SEARCHING / CALIBRATING / TRACKING LOST messages remain in the gaze-following panel with spinner and corner/fit confidence. No valid fit: last pose remains, retry/cancel/assisted controls remain accessible. ASSISTED FIT is explicitly labeled and only available after a user-specified fit.

ACCEPT BOARD is available for a verified metric preview; it locks the existing board root and records the geometry source, corner count and reprojection residual. A Quick Play capture proceeds to piece scanning only after this acceptance. Grid refinement preserves the pieces' current dimensions; the existing separate king-sizing stage handles intentional piece scaling. Mesh base pivots remain on the board surface. Existing spatial anchoring and session persistence are reused, not replaced. Stored geometry is not proof of a relocated board: room changes and physical return still require registration and board-state verification.

After lock the camera does not chase a moving board or the headset. Occlusion preserves the last accepted frame. Deliberately moving the real board requires recalibration. Reacquisition does not rewrite the game journal. Scene/saved games, legal-move gates, AI, menus and both-hand controls are retained.

## Device build and acceptance
Use FirstStepsRelease.Build (0.1.12/code13), existing Quest signing and camera permissions. No new Inspector dependency is required. Keep 0.1.11 APK available for rollback. Camera identity reading still uses the existing local host; geometric calibration runs on the Quest.

Measure independently, do not prefill passes:
1. Use several board sizes and finishes. Record actual dimensions with a ruler only as test ground truth, not as app input.
2. Compare all four playing-area corners and interior intersections; record maximum and RMS position error in millimetres and rotation error in degrees.
3. Record stationary jitter for 30 seconds and drift at 1, 5 and 10 minutes while looking from different angles.
4. Test automatic depth and insufficient-depth/head-movement routes; test pure rotation, partial occlusion, hands, blank surfaces, glare, poor light and interrupted captures. No uncertain fit may unlock ACCEPT BOARD.
5. Hide the board, move the headset and reveal it: locked geometry must stay world-fixed. Move the physical board deliberately: recalibrate and verify no game/history changes.
6. Test four corners, single-knob fixed opposite corner, two-knob manipulation, cancel/menu, king sizing, Quick Play capture, Continue, pending AI verification and physical/virtual handoff.

Limitations: coarse localization still needs the board broadly within the initial placement/search area; visible corner coverage may be insufficient on a heavily occupied board. Specular wood/plastic, shadows, low contrast, steep views, depth noise, camera calibration and symmetric-pattern ambiguity can force assisted alignment. False convergence and millimetre accuracy must be assessed with real headset footage and independent measurements.
