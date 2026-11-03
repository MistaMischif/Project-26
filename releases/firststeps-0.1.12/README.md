# Living Table First Steps 0.1.12 — physical surface calibration

Quest 3, version code 13. Calibration-only replacement approved by MASTER OMEGA; 0.1.11 remains the rollback download. No new host dependency, paid API, SDK upgrade or native plugin.

## Headset test
1. Choose the physical board and ALIGN BOARD. Look down so the whole 8×8 playing area is visible; clear hands from its corners.
2. The app detects inner grid corners and estimates real board dimensions from depth. If depth is unclear, follow the floating prompt to move your head sideways about a hand's width while the board stays still. No square-size entry is required.
3. The grid is updated only after consistent metric observations. Review every edge and the White home-row direction, then press ACCEPT BOARD. ROTATE 90 is retained for board orientation.
4. Single-knob adjustment keeps the opposite playing corner fixed and triggers refinement on release. Four-corner setup also supplies a refinement seed. A failed scan offers retry/cancel and clearly labeled assisted fit; it cannot silently accept guessed scale.
5. Continue through king sizing and physical-position review. Quick Play retains the same capture/Continue/virtual-AI workflow, with explicit board acceptance before its initial piece scan.

The existing game journal, pending AI verification, legal-move rejection, safe virtual transition and floating menu are retained. Grid refinement preserves existing piece dimensions. Calibration uses the Quest camera/depth/tracking; piece identity recognition uses the existing local vision host.

## Limits
Tabletop boards only, 20–120 cm playing area. A stationary physical board and sufficient visible intersections are required. Pure head rotation cannot establish scale when depth is unavailable. The grid stays at its last accepted world pose on failure/occlusion; moving the physical board requires recalibration. Symmetric checkerboards require the user's orientation confirmation.

Synthetic solver and scene tests are recorded separately. Actual positional error, jitter, drift, camera framing, reacquisition and performance require a real-headset measurement; no millimetre accuracy or founder acceptance is claimed.

Implementation, coordinate conventions, API constraints and measurement procedure: Assets/LivingTable/Chess/SURFACE_CALIBRATION.md.
