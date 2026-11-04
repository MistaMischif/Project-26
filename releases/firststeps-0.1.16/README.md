# First Steps 0.1.16 — Camera-assisted corners and occlusion-aware grid

Mandatory four-corner Quick Play remains. Camera fitting never starts gameplay or skips manual placement/confirmation.

## Implemented
- Camera-assisted snapping uses the existing on-device MRUK PassthroughCameraAccess image path and prior depth/tracked-view metric fit. Three fresh, hands-clear frames must agree on internal checker lines and playable perimeter evidence in both axes.
- Perimeter checks require the checker pattern to terminate at the predicted playing edges. Continuing checker patterns, distant frame boundaries and uncertain locations cannot be camera-snapped. Snap distance is at most 30 mm and 35% of one square, whichever is smaller.
- GREEN / SNAPPED identifies corners with two supported adjacent edges. BLUE / INFERRED identifies corners completed from the square model with all four user markers; it does not claim direct camera observation. AMBER / MANUAL remains adjustable. States are recorded with the session registration.
- Missing corners due to pieces may be inferred once all four approximate markers are provided and visible perpendicular edges support the model. If no camera metric fit exists, a consistent manually defined square can be retained as inferred geometry; it is never relabeled camera-snapped. Uncertain frames do not displace markers.
- Near-left and near-right placement generates a live square preview; the third marker supplies a surface-normal estimate. All four markers and final valid square geometry remain required. Confirm locks alignment until Move Board.
- Five cyan + guides stand along the board surface normal at 35 mm intervals. The base + is exactly at board-local (0,0,0). Looking down until the signs overlap guides viewing angle; these virtual graphics are not camera image features.
- Retry Snap / Cancel Snap keeps setup escapable. Main-panel Menu and Confirm / Adjust Corners / Restart Setup remain.

## Code
- ChessSurfaceSolver.cs: playable edge evidence mask and termination checks.
- ChessCornerSnapping.cs / ChessCameraCalibration.cs: fresh-frame validation, cancellation and existing metric-fit reuse.
- ChessQuickCornerSetup.cs / ChessSetupFlow.cs: bounded marker snapping, inferred/manual provenance, progressive preview and actions.
- ChessAlignment.cs: center + column.
- ChessCaptureContinue.cs: per-corner observed/inferred registration fields.

## Verification
37 Unity scene interaction checks; 59 synthetic surface/image geometry checks; 20 recognition checks; 33 reconciliation checks; chess rules and 20 observation gate checks passed. Synthetic cases include three board sizes, frame/offset rejection, occluded corner preservation, and an extended checker pattern. These tests do not establish real-camera success rate.

## Headset acceptance
1. Quick Play still requires all four markers in order, excluding the frame.
2. With the full board visible, place close to each playable corner and lift hands. Green SNAPPED markers should land on playing boundaries, not the decorative frame.
3. Place a piece over a corner: visible evidence and all four markers should preserve the square model; inferred corners must be BLUE, not green.
4. Check the preview after two and three markers. Look down until the vertical + signs overlap over the center of the physical playing area.
5. Test Retry/Cancel, manual adjustment, Confirm and Menu. Confirmed alignment must remain fixed when your head moves.

Physical accuracy, lighting/occlusion tolerance and marker reachability require MASTER OMEGA headset acceptance. Conservative refusal is expected with glare, weak contrast, unavailable camera/depth fit, or insufficient visible perpendicular edges. Rollback: firststeps-v0.1.15 (same application ID; install -r preserves saves).
