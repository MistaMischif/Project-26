# Living Table First Steps 0.1.3 — recognition test build

Chess-only Quest test package with visibility-gated local recognition. A separate computer runs the optional open local model; virtual and explicit manual play remain available without it. This is a development prerelease, not a contest submission or founder acceptance.

## Install and test

Install the APK over the existing First Steps app (same package; preserve saves). Start the local Ollama runtime and host using Services/FirstSteps/README.md or the included host ZIP. Keep USB connected with `adb reverse tcp:8790 tcp:8790`. The private device connection file is created locally and is never included in this release. Model weights are not included.

1. Use a disposable tracked chess session and the full physical set. Align the playing area.
2. Test each piece type/color, including edge squares; clear hands before scanning.
3. Try a wrong identity, partial occlusion, wrong destination and an incomplete capture. None may advance the journal.
4. Run human → AI → human, continue virtually, then arrange the saved physical position and scan. Record cold/warm move latency and full-return time.
5. Export failures with the APK fingerprint, board/set, lighting and reproduction. Do not treat manual confirmation as camera verification.

The recognizer now matches the limited synthetic pawn/empty/covered fixtures. Real-board/all-piece accuracy is still unverified, and the 64-crop synthetic workload took 30.7 seconds. See VERIFICATION.md and raw evaluation files. The old faster batching experiment failed occlusion checks and was not shipped.

Qwen3.5 is the current local recognizer. Llama remains configurable; this path does not establish Meta/Llama hackathon qualification. No paid API is required. Candy Land stays in the separate general Living Table app.

Dashboard: https://living-table-progress-jz.mistamischif.chatgpt.site
Release: https://github.com/MistaMischif/Project-26/releases/tag/firststeps-v0.1.3
