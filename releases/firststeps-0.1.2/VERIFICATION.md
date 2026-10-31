# Verification — October 31, 2026

- ChessHandoffTests: passed. Scene checks include chess-only navigation, unchanged session/history, physical reconciliation lock, persisted pause, locked AI proposal after human return, virtual execution exactly once and durable consumption.
- ReconciliationTests: 33 checks passed across starting, promoted and compound-rights positions. Missing/extra/duplicate/uncertain/wrong/stale observations rejected; all 64 cells required. Hidden rights and position remain unchanged.
- RecoverySceneTests: 14 checks passed. Alignment alone, stale revision, pause and open handoff menu cannot unlock First Steps. Manual checks enter manual mode; returning to camera requires verification. Assisted provenance persists. Identity approvals were synthetic inputs, not real camera observations.
- Earlier 0.1.1 board work: 23 geometry/live-scene checks and 3 calibrated-offset checks passed. Included in this release; actual hand tracking and anchor persistence remain pending.

Logs: Logs/FirstSteps-0.1.2-handoff.log and Logs/FirstSteps-0.1.2-recovery.log. Native build, installation and artifact verification are recorded separately after completion.

Known limits: no real-board full-scan accuracy or latency measurement. Prior model fixtures had 5/10 mismatches, 5.5–6 second warm pairs and 30.9 second cold pair. Full scans use 16 sequential batches of four crops from one frame and can be slow. A new frame/hand guard checks stability at completion; appearance comparison is not tamper-proof identity authentication. No contest-ready or founder-accepted claim.

Final recovery scene rerun passed after request-cancellation changes. Existing Meta MRUK teardown diagnostics appear during editor shutdown; no claim of a warning-free editor run. Node host tests: 2 groups passed with test doubles. Local vision host and model runtime were initially stopped. Founder explicitly authorized starting them for this headset test; the existing isolated local runtime and authenticated host were started without changing the machine-wide Ollama service.

Final APK: Unity succeeded with 0 build errors. Package com.livingtable.firststeps, version 0.1.2/code 3, ARM64; APK v2 signature verified. Network policy blocks cleartext except localhost/127.0.0.1. Packed-asset audit covers 4,560 source paths: no Candy Land Resources/ContestExcluded assets, and Candy Land paths in the report are scripts only. The general resource folder was restored after packaging.

ADB install -r returned Success on Quest 3. Activity launched; package version checked; PID 14136 running. Startup Unity error-channel check returned no entries. USB forwarding and existing private connection settings restored without exposing credentials. This is installation/startup verification, not visual gameplay acceptance. The founder subsequently authorized the vision/model services for this test; they are running locally, with the USB bridge restored. Actual inference readiness is recorded in vision-readiness.json.

Authorized service readiness: Ollama 0.24.0 and the authenticated host responded. A real two-crop synthetic request returned HTTP 200 in 24.374 seconds; one of the two empty fixtures was incorrectly labeled black_rook. This confirms service execution, not recognition accuracy. See vision-readiness.json. Services remain running for the current founder-authorized headset test.

Publication: GitHub prerelease firststeps-v0.1.2 targets source commit 552db3b0a4b5d995ad1267741bb462c4bc3916aa. Uploaded APK digest matches local SHA-256 6e453d9898a8567983b3804a9ec32845ad6033afd024bb9c95ec17a387aa5f81. Dashboard source 4244e593bcd7bbe0d008d547d1bdd3090edffb5a deployed successfully as appgdep_6ae5e6a8fb188191a694be7bc76ba78a at https://living-table-progress-jz.mistamischif.chatgpt.site. Existing owner-only access and stored observations preserved. Dashboard model/interaction checks passed; visual browser QA not performed. USB reverse 8790 was rechecked.
