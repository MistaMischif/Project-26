# Quest device verification — 0.1.30

Installed and launched on the connected Quest 3, package versionCode 31. APK signature verified. Process 15085 remained unchanged throughout three controlled power sleep/wake cycles; each cycle reached Asleep then Awake. No matching crash/OOM entries appeared in the captured test-window filter.

Graphics PSS from Android dumpsys meminfo, in KiB:
- Awake baseline: 432374
- Wake 1: 435562
- Wake 2: 435730
- Wake 3: 435738

Total growth: 3364 KiB (3.29 MiB); growth from wake 1 to wake 3: 176 KiB. This bounded test does not reproduce the previous hundreds-of-MiB-per-wake leak. It does not establish that every possible headset crash is eliminated.

Runtime confirmed OpenXR 1.17.1, Mobile_RPAsset, render scale 1, MSAA 2, depth texture off, opaque texture off. Unity issue UUM-152570 is the basis for the package rollback.

Automated handoff: 50 checks passed using the actual chess AI across six plies and physical > virtual > physical; all 247 regression checks passed. Input/corner placement and lifecycle events in that suite are synthetic.

Live headset handoff telemetry: the user played physical chess, switched to virtual at ply 3, returned to physical at ply 16, and continued to ply 20. Both surface switches preserved the exact match and board registration. All 35 captured checkpoints passed hash-chain validation. App PID remained 15085. Two additional suspend/resume pairs during user testing retained reported graphics allocation at 375 MiB; a subsequent third suspension was still awaiting wake at this observation. Android Graphics PSS during gameplay was 446562 KiB. Raw saved sessions and device logs remain private.

The physical > virtual > physical handoff and continued physical play are verified by headset checkpoints. Visual/control comfort and explicit founder acceptance remain pending; no response has yet been received to the hands-on test question.


## Founder acceptance — November 4, 2026 (America/Chicago)

MASTER OMEGA confirmed "Everything worked" for the requested physical → virtual → physical gameplay handoff and both 10-second visor removal/replacement tests, including RESUME where shown. This revision is founder-accepted for that tested scope. The bounded test does not guarantee absence of every possible crash.
