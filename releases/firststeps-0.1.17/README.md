# First Steps 0.1.17 — Comfortable piece grip and automatic physical tracking

## Changes
- Virtual piece pickup uses thumb/index tip distance: starts at up to 45 mm, stays held up to 65 mm. The piece follows the exact fingertip midpoint; fingers need not touch. Existing button, setup and board-handle pinch thresholds remain unchanged.
- Grab height now accommodates the configured piece scale.
- Physical camera watching remains active while manual fallback controls are available. A complete verified board or move restores automatic tracking; partial observations cannot bypass manual/recovery safeguards.
- Removed blind initial baseline capture. The first board must match the saved position or exactly one legal successor by identity, followed by fresh stable frames. This avoids swallowing a first physical move as the reference image.
- Human and pending AI physical moves retain automatic legal verification and virtual updates without CHECK BOARD / DONE. The next turn waits for verification; ambiguous/illegal changes do not advance saved state.

## Headset test
1. In virtual play, place thumb and forefinger around a piece with approximately 3–4 cm separation. Lift and move it; open beyond 6.5 cm to release. Check that it stays between fingertips, including resized pieces.
2. In real-board play, move a legal human piece, then move hands clear and look down at the whole board. Its virtual counterpart should update and the AI should begin without a button press.
3. Move the physical AI piece to its pulsing destination. Verification should return control automatically.
4. Repeat with manual fallback controls enabled and on the first move immediately after setup. Test illegal changes: saved game must not advance.

The existing configured vision host is still required for piece identity recognition; it reported ready during this test session. This release does not replace that backend or promise hand-occluded continuous tracking. Physical updates occur after a clear, stable image and identity verification. Camera accuracy, latency and pinch comfort remain pending MASTER OMEGA headset acceptance.

Rollback: firststeps-v0.1.16, same application ID. Install -r preserves saves.
