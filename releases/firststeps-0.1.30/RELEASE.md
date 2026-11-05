# Living Table First Steps 0.1.30 — Sleep/wake and handoff

OpenXR is pinned from 1.18.0 to 1.17.1. Unity issue UUM-152570 describes unreleased XR swapchain textures after sleep/wake on 1.18+, and reports no reproduction with 1.17.1. This matches the previous build's observed repeated graphics-memory growth. Unity issue: https://issuetracker.unity.com/issues/24846/xr-swapchain-render-textures-are-not-released-when-the-headset-is-awaken-from-being-put-to-sleep

The Android startup render budget now covers both the quality pipeline and fallback pipeline, caps supersampling/MSAA, and disables unused depth/opaque camera copies. Startup logs record the effective pipeline settings. Existing focus/pause gating, saved game state, audio handling and delayed camera restart remain in place. The release build checks the resolved OpenXR version before building.

Automated verification: 117 integrated + 80 physical-flow + 50 end-to-end handoff checks passed. The handoff suite starts standard physical chess, uses pinch/drop and MOVED IT, waits for the actual chess AI, switches through the virtual safety notice, plays a virtual human/AI pair, returns immediately to physical mode, and completes another human/AI pair after simulated suspension with a pending AI move. It checks the persistent session, exact position/history, world pose and absence of scanning interruptions. These are scene/API tests, not physical sensor proof.

Device sleep/wake measurements and founder handoff acceptance are recorded in DEVICE-TEST.md. Do not infer complete crash elimination from a bounded test. Turn-time recognition remains disabled: MOVED IT is user confirmation, not camera verification. Optional Meta conversation still needs a reachable configured host; the chess AI runs on-device.
