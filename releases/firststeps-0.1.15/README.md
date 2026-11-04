# First Steps 0.1.15 — mandatory Quick Play board setup

## Implemented interaction

Quick Play creates a draft and displays “Look at your physical chessboard to begin setup.” Existing camera/depth fitting can supply a provisional grid only. Every new Quick Play requires four explicit pinch placements: near-left, near-right, far-right, far-left, relative to the initial setup heading. These are the playable-area corners, excluding the frame.

All placed markers are amber and labeled MANUAL. Pinch a marker to reposition it. Invalid corner order, non-finite input or geometry that cannot fit a square within the supported tolerance cannot be confirmed. The final square grid is fitted from all four points; it is not a perspective-distorted quadrilateral.

After four valid points, the panel asks “Does this grid match your chessboard?” with CONFIRM, ADJUST CORNERS and RESTART SETUP. Confirmation records assisted geometry, locks the board and proceeds to existing king sizing and piece-state review. A game still requires the final START GAME action. Camera callbacks cannot replace the chosen corners or the confirmed grid. MOVE BOARD remains the deliberate unlock action.

A labeled MENU button is on the main board toolbar during setup and gameplay, separate from the floating-panel menu icon. Its position is centered below the principal call-to-action, outside the playing squares and away from corner markers.

## Camera-assisted corner snapping: explicit blocker

Snapping is **not implemented or claimed** in this release. The project has actual `Meta.XR.PassthroughCameraAccess` image capture, camera pose/ray projection and direct AsyncGPUReadback; this is not inferred from passthrough display. MRUK is 207.0.1, with the existing OVR interaction package 207.0.0. `ChessCameraCalibration` uses `EnvironmentRaycastManager` depth where available and tracked-camera views otherwise.

The Android manifest includes `horizonos.permission.HEADSET_CAMERA`, `com.oculus.permission.USE_SCENE`, hand-tracking and anchor permissions. The capture path checks and requests HEADSET_CAMERA at runtime and checks camera/depth support. Permission and hardware availability remain runtime conditions, not guaranteed merely by manifest declarations.

The existing detector supplies internal 7×7 checker intersections; `EdgesAgree` checks internal square edges and boundary-adjacent intersection coverage. It extrapolates outer board corners from a fitted lattice. It does not independently identify the requested physical outer corner, establish an outer-corner confidence score or disambiguate a nearby decorative border. Automatically moving a marker on that basis would overstate the evidence. The required manual fallback therefore remains explicit. A future snap implementation needs a local outer-corner/playing-boundary verifier with ambiguity rejection and real-board validation; passthrough availability alone does not supply that capability.

## Specific code

- `ChessQuickCornerSetup.cs`: mandatory state transitions, heading-relative order, stricter square fit, labeled movable markers, explicit confirmation and main-panel Menu.
- `ChessSetupFlow.cs`: routes Quick Play placements/adjustments through review; guards scanning and game start until grid confirmation.
- `ChessAlignment.cs`: automatic result is provisional for Quick Play; late results cannot overwrite corners; confirmed pose remains locked.
- `ChessAlignmentMessages.cs`: stable exact Quick Play instructions alongside existing progress/error areas.
- `ChessCaptureContinue.cs`: starts and restores the capture heading; records four manual corners as assisted registration.
- `ChessBoardHandles.cs`: disables general board resizing during unconfirmed Quick Play corner setup so controls cannot interfere.
- `ChessHandoff.cs`: adds Menu before setup/gameplay branches.
- `QuickCornerFlowTests.cs` and updated `CaptureContinueTests.cs`: interaction and continuity regression coverage.

## Acceptance checklist

- [ ] MENU is readable/reachable on the main panel in setup and gameplay.
- [ ] Quick Play shows the exact initial prompt and does not start chess.
- [ ] A successful automatic fit still requires all four manual placements.
- [ ] Markers follow initial-view order, exclude the border and say MANUAL; no unsupported snap is shown.
- [ ] Camera denied/unavailable permits explicit MANUAL CORNERS without a dead end.
- [ ] Pinch/release adjusts any marker; distorted or reversed geometry cannot confirm.
- [ ] The full grid overlays the intended playing area; CONFIRM, ADJUST CORNERS and RESTART SETUP work.
- [ ] Confirmation locks the pose; late camera results/head motion do not move it.
- [ ] Existing sizing, piece review, Start Game, save/Continue and Menu still work.

Automated checks do not establish physical placement accuracy, headset reachability or real-board recognition quality. Founder headset acceptance remains pending. App 0.1.14 is the rollback reference; existing host 0.4.3 is unchanged. Independent hosting is outside this change.
