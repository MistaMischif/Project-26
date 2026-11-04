# First Steps 0.1.21 â€” Physical surface and alignment guides

Clean replacement for the withdrawn 0.1.20 cached build. Release requires zero build errors and a nonempty packed-asset report.

## Changes
- Four world-space pinch positions define the board center, surface normal, rotation and equal 8x8 spacing. Raw pinch coordinates are stored with registration. Camera refinement is constrained to this user-defined plane rather than replacing its height/tilt.
- Accepted calibration re-roots the board mount at the calibrated XYZ pose and requests an existing Meta spatial anchor there. Registration records the resulting anchor and local/world pose. Standing/sitting does not recompute the board from eye height.
- Physical piece bases and camera sampling use the calibrated surface rather than a 9 mm model lift plus a 5 mm sampling lift. Thin grid lines use only a submillimeter display separation.
- Each corner has two long edge axes meeting adjacent corners. Completed valid square geometry is green; invalid geometry is red; incomplete geometry is amber. Feedback updates during marker adjustment. This is geometry validation, not proof of camera recognition; SNAPPED/INFERRED/MANUAL provenance labels remain separate.
- Physical setup board fill, piece guides and markers are translucent. The existing gameplay opacity setting resumes after setup.
- Five stacked plus guides are 3x wider. Blue means the view is off axis. Green requires the eye within 5 degrees of the board normal and camera aim within 8 degrees. After a 0.35-second aligned hold with four corners and hands clear, one refinement attempt starts. Camera unavailability preserves manual confirmation; the board is not moved to follow the head.
- Align -> king sizing -> PLAY remains intact.
- Pink UNDO LAST MOVE appears directly beneath ASK META after a human move. It also retracts an already-completed AI reply so the human can correct their move. Physical undo requires matching the real board to the restored position; the saved session is marked assisted.

- Capture areas use only thin gold grid lines and captured-piece names; no wooden surfaces or duplicate 3D captured models obscure physical pieces.

## Headset acceptance
1. Pinch the actual playing surface at all four outer playing corners. Inspect long edge lines; move one marker off plane and confirm red feedback. Correct it and verify green geometry.
2. Line up the enlarged plus centers from above. Check blue to green and brief hold -> refinement. Read provenance labels; green geometry or view alignment alone does not mean camera calibration succeeded.
3. Accept alignment, size the king and PLAY. Stand, sit, lean and view from each side: virtual grid and piece bases should remain flush with the physical surface. Confirm no unwanted movement or parent offset after anchor creation.
4. Verify transparent guides reveal real edges/pieces. Test an unavailable-camera case: manual adjustment/confirmation must remain usable.
5. Save and continue, then re-register at another tabletop height. Check stored game positions and turn remain intact.

61 integrated checks passed, including tilted XYZ pose, camera-height changes, plane-constrained refinement, invalid raised corner, transparency and enlarged guides. Existing release regression results are included. Actual Quest spatial anchoring, passthrough registration accuracy, visual comfort and camera latency require MASTER OMEGA acceptance; automated geometry tests are not headset proof. The existing self-hosted vision service remains required for physical piece identity.

Rollback: firststeps-v0.1.19. Same package ID; install -r preserves saves.
