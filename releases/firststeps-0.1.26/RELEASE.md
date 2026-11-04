# Living Table 0.1.26 — Done, presence checks and captured-game exploration

Virtual piece drops remain tentative until DONE. Pick up the tentative piece to adjust it. In virtual mode, DONE advances the turn directly. In real-board mode, DONE requests only the destination square; occupied advances, empty prompts placement, uncertain retries without advancing. AI destination checks use the same narrow path. This verifies occupancy, not identity, capture removal, castling-rook placement or complete-board correctness; user-confirmed physical moves retain unverified provenance.

Grab at up to 35 mm fingertip spacing; release beyond 50 mm. Capture grids now start outside the knob interaction radius plus a board-scaled clearance margin. Slot size follows the resized piece set.

New Game retains standard-position setup. Quick Play captures an existing board after alignment and sizing, allows correction and side-to-move review, and opens that position in virtual AI play. The original capture is saved separately; GAME OPTIONS / RESTART CAPTURE creates a fresh experiment without overwriting it. Returning to physical play retains full saved-board reconciliation.

Verification: 103 integrated checks and 80 physical-flow checks passed. Android build and device installation pending. Five destination parser checks also run during the release build. Physical recognition accuracy and comfort require founder headset testing.

Test: green ACCEPT, size, Play in New Game; pinch, adjust, DONE. In real mode leave the destination empty and verify no advance, then place a piece there. In virtual mode verify DONE advances without camera. Check capture-grid clearance with hands on knobs. Quick Play should review the actual scanned position and preserve its original save while you explore against AI.

Known critical issue: the earlier sleep/wake graphics-memory leak remains unresolved. No sleep-crash fix is claimed.
