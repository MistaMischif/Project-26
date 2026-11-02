# Verification - November 2, 2026

Diagnosis: Quest reconnected with no adb reverse mapping. Local host was healthy; device token and URL matched the host (compared privately, not recorded). Restored pairing. Unity shutdown also clears forwarding, so final device verification follows all Unity processes.

Eight Node test groups passed, including authenticated model-readiness positive/401/missing-model cases. Unity handoff suite passed with a real Meta response and stale-revision rejection. Alignment/AI scene tests passed: 32 ghost pieces before physical play, live size preview without acceptance, correct 8-square pitch, transparent material, guide removal after explicit manual reconciliation, physical AI pending instruction and virtual AI response with no Meta calls. This verifies chess logic, not camera accuracy.

Real physical-board alignment readability and camera recognition require founder acceptance. The overlay is a guide, not a claim of recognizing the physical pieces. USB host availability remains necessary for Meta and Qwen; onboard chess AI does not require either.

Device transport: an authenticated request originated on Quest through its loopback USB tunnel and received HTTP 200 with a real llama3.2:3b answer. Credential and temporary request files were not retained. One initial inference probe returned 503; the exact request subsequently succeeded locally and from Quest. This is connection/inference evidence, not a claim that every model request succeeds. The app reports failures and supports retry. Final app readiness will be checked after installation.
