# Living Table 0.1.25 — Corner acceptance and physical interaction

Four valid, ordered corners enable a green ACCEPT button. Adjusting a marker withdraws acceptance until geometry is valid again. King sizing and Play remain direct, without a setup scan gate.

Real-board mode now allows pinching and lifting your virtual piece. A legal drop is a preview only: camera identity verification must succeed before the saved move or turn advances. Invalid drops return to the saved square. Camera processing pauses while a piece is held.

Returning from the standard Menu resumes automatic camera tracking instead of leaving it silently suspended. Previewed destinations are provided to recognition as hypotheses, never as proof.

Verification: 86 integrated and 80 physical-flow checks passed. Android build succeeded with zero errors; APK signature verified. Version 0.1.25/code26 installed and launched on Quest (PID 1216). The existing host readiness endpoint returned HTTP 200; Quest camera permission is granted. This is not proof of real-board recognition accuracy.

Headset acceptance: place four corners and verify green ACCEPT; size then Play; pinch a pawn and drop on a legal destination; move the physical pawn there and clear hands; verify the virtual board and next turn; open Menu then Back and repeat. Also move a physical piece without pinching its virtual counterpart. An incorrect physical placement must not advance the saved turn.

Known critical issue: repeated sleep/wake graphics-memory growth remains unresolved from 0.1.24. This release does not claim a sleep-crash fix.
