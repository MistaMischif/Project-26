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

Founder headset test requested: one human/AI turn per mode, return to physical without setup, then two manual visor removal/replacement cycles. Result pending. Do not label physical headset handoff or manual visor acceptance as complete until founder response is recorded.
