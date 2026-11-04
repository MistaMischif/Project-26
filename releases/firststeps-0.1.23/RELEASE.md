# Living Table 0.1.23 — Quest graphics memory and sleep recovery

Status: rejected by final build verification (one invalid RuntimeInitializeOnLoad method signature). Not installed or published. Corrected in combined 0.1.24.

The 0.1.22 power-key test passed one sleep/wake cycle but lost the app on the second. Kernel logs recorded global out-of-memory kills and a failed 237512 KB GPU allocation before Android reported an undeliverable broadcast. A fresh 0.1.22 launch measured 1,745,962 KB graphics PSS and 2,153,467 KB total PSS. Earlier system audio errors were not sufficient to identify the original cause.

This revision caps Quest URP rendering at 1.0 scale and at most 2x MSAA (previous mobile settings: 1.6 scale / 4x MSAA). Lower configured settings remain lower. The runtime-only cap leaves desktop settings unchanged. Lifecycle logs include graphics/native allocation estimates and eye dimensions. The previous pause/focus/camera/anchor guards remain.

Acceptance requires repeated sleep/wake with the same app process, stable memory, no new OOM/crash/ANR evidence, retained game state, audible effects and functioning hands/camera. Automated power-key tests do not establish visor-proximity behavior, game-state continuity or physical recognition without an actual in-game headset check.
