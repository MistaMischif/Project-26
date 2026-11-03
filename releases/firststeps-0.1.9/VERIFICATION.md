# Verification — 2026-11-03

APK source: 298a0fd5f1d43a8c8778d262b69677aef3dca525. Host packaging includes a subsequent Windows PowerShell no-USB recovery correction.

## Passed before packaging
- 38 editor checks: branded three-choice home and tracking-settlement recovery, safe square-button bounds, four knobs/no side controls, set sizing, two-hand translation/rotation/scaling, no initial scale jump, paused move rejection, session preservation, physical acceptance, visible counterparts and verified physical-move callback updating its virtual twin.
- Synthetic perspective checker fit, blank/unstable rejection and TLS pin checks; live alignment/guide sizing and physical/virtual offline AI scene tests.
- Chess rules, handoff/session regression, pending AI action retained and consumed once, fingertip grip, new-room hiding/replacement, postgame and replay checks.
- Eight Node host tests: strict request/response contracts, Meta fact binding, invalid answer rejection, visibility gating, uncertainty and cancellation.
- Actual host 0.4.1 Meta Llama reply to a continuation fixture, with valid fact ID and unchanged game context.
- Actual Qwen observation: two synthetic empty-square crops correct, cold 20.51 s, warm 1.16 s. Not a real-board benchmark; Android compilation was also active.
- Windows PowerShell launcher path with simulated missing ADB device: already-running host remains usable, no exception, no USB-forwarding requirement for HTTPS.
- Editor screenshots reviewed for gold knobs, pulsing grid, compact square control layout, guide pieces and readable back-panel text.

## Still needs MASTER OMEGA on the exact APK
Actual automatic tabletop depth and checker fitting; every real piece type and color; whole-board recovery latency; false-positive/occlusion testing; set sizing and touch comfort; two-hand tracking loss; unfamiliar-room recovery; FPS; final gameplay acceptance. No automatic checklist passes are seeded.

## Graphics and IP
See GRAPHICS-AND-IP.md. Procedural project visuals; licensed Unity/Meta runtime/font dependencies remain third-party. No filed patent receipt is available. Home wording is application in preparation.

## Unpublished candidate package and headset transport (superseded)
- Android package com.livingtable.firststeps, 0.1.9 / code 10; 88,234,023 bytes. APK signature verifies. Build completed with zero errors; packed-assets audit excludes Candy Land resources.
- APK SHA-256: 89a76b133c04a703de6004b01442d94e1ca6690a38d40d2b1c09f6402e809117.
- Installed successfully on Quest 3, launched PID 25401. Unity:E and AndroidRuntime:E startup logs were empty.
- adb reverse list was empty. Headset logged FIRST_STEPS_WIRELESS_READY: CONNECTED HTTP 200 twice (10:53:27 and 10:53:47). This verifies headset-to-host HTTPS without USB forwarding, not all gameplay/camera behavior.

## Superseding release candidate — reachable home correction

The preceding package/hash/PID and transport checks describe the first, unpublished 0.1.9 candidate from 298a0fd. They are retained as historical evidence, not the final release fingerprint. Headset inspection revealed a first-launch menu reachability issue. Source 9349048142d6b2cbe6612d79017d8f19b5ee66c2 corrects it and includes the Windows PowerShell no-USB recovery fix. All 38 interaction checks passed on this source. The final package fingerprint and headset check will be recorded below after rebuilding.

## Final released package (authoritative)

Source: 9349048142d6b2cbe6612d79017d8f19b5ee66c2

SHA-256: d8da5ba8c0f9e07764f4b369797152e7a166f12c98d8cba3679306140b2dbc07

Size: 119200106 bytes. Version 0.1.9 / code 10. Signature verified; zero build errors; Candy Land resources absent from packed-assets report. Installed and launched successfully on Quest 3 (PID 28001). At 11:11:16, the headset logged both FIRST_STEPS_WIRELESS_READY: CONNECTED HTTP 200 and FIRST_STEPS_COMPANION_CONNECTION: HOST_REACHED. ADB reverse list was empty. Unity:E and AndroidRuntime:E startup logs were empty. This is transport/startup evidence; real-board accuracy and founder gameplay acceptance remain pending.

Dashboard source: 081278a40d708d45545606ef18e39b581849c3ee. All three dashboard checks pass; prior observations preserved; no founder passes seeded.

