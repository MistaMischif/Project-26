# Living Table 0.1.22 — sleep/resume test build

Status: clean build passed (zero errors); 69 integrated and 80 physical-flow checks passed. APK signature and installation on Quest 3 verified, version 0.1.22/code 23. Foreground launch, repeated device sleep/wake cycles and founder visor acceptance remain pending.

## Evidence
On November 4, the Quest crash buffer recorded HAS::createTrack timing out in audioserver, followed by repeated null dereferences in libhas.so. Android recorded Living Table 0.1.21 killed for an undeliverable broadcast at 15:24:16. No Living Table native tombstone was present in that captured buffer. These observations do not establish that app changes alone repair the system audio service.

## Changes
- Shared focus/pause/headset-presence gate holds audio and interaction, with a two-second stable-focus recovery period. No AudioSettings.Reset retry loop.
- Recognition generation invalidation rejects old readbacks and clears pending requests; calibration cancels on every suspension, including corner snapping.
- Board-grip anchor persistence is deferred while suspended rather than starting XR anchor work on headset removal.
- Chess saves its position on suspension and defers rebuilding its UI until focus has settled. It remains paused for an intentional resume.

## Acceptance
1. Start virtual chess, move a piece, remove the headset for 10 seconds, put it back on. Repeat three times. The app must remain running, preserve the position and resume controls and sounds.
2. Sleep for at least one minute during physical play; confirm the same board pose, move history and pending opponent move, then perform a fresh scan.
3. Remove the headset during corner snapping and while holding a board knob. After wake, retry alignment without stale camera results or an unreachable board.
4. Open and close the system menu. Gameplay must not advance while focus is lost.
5. If Android terminates the app, Continue must retain the saved position; process recreation is distinct from successful in-process resume.

Remote power-key checks do not prove visor-proximity sensor behavior or physical-camera accuracy. Founder headset acceptance is required.
