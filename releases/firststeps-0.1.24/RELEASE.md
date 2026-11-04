# Living Table 0.1.24 â€” Direct setup-to-play and Quest memory budget

Status: installed on Quest. 77 integrated and 80 physical-flow checks passed; Android build completed with zero errors. Setup flow awaits founder acceptance.

Known critical issue: sleep/wake memory leakage remains unresolved. Three automated cycles retained PID 28985, but graphics PSS grew from 862,216 KB to 1,259,886 KB, 1,615,198 KB, then 1,969,834 KB. This is a diagnostic test build, not a verified crash fix.

Setup ends at four-corner alignment, king sizing, then Play. New and continued games enter gameplay without an explicit Check Board pass; continued position, side to move and pending AI move remain unchanged. Physical gameplay uses YOUR MOVE / MOVE AI PIECE instead of CHECK BOARD. Automatic observation and legal-move validation remain; camera uncertainty is never treated as a verified move. Undo/handoff reconciliation remains separate from starting a user-confirmed layout.

Quest URP render scale is capped at 1.0 and MSAA at 2x. Previous 1.6/4x settings measured about 1.7 GB graphics PSS; the 0.1.22 second wake produced global OOM and GPU allocation failure. Lower graphics settings remain lower. Pause/focus/headset-presence guards retain saves, invalidate scan callbacks and defer anchor persistence during suspension.

0.1.23 was rejected before installation by the build verifier due to an incorrectly attached startup attribute. That declaration is corrected and covered by startup-hook checks in this revision.

Acceptance: validate grid with four corners, size king and Play; no Check Board or above-board instruction intervenes. Repeat with Continue and confirm exact saved position and turn. Remove/re-wear the headset during play, alignment and a pending AI move. Check controls, sound, camera and saved position. Automated power-key survival does not prove visor-proximity or in-game acceptance.
