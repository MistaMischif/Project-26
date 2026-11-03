# Verification — First Steps 0.1.10

- App source: `208972a688553cb23ea7d185c1ae5355cd6bce3f`.
- Unity 6000.3.9f1 Android ARM64 development build succeeded with zero build errors.
- Package `com.livingtable.firststeps`, version 0.1.10, code 11; APK signature verified with Android apksigner.
- SHA-256 `e4b9f5d49462090bb20f7270a9b8ad1fcd6d04702b906f4f717a3ec0bc0d6da7`, 88,257,647 bytes.
- 46 synthetic scene checks passed: four-corner fit/rejection, king sizing, scan cancellation, final purple START GAME, stale setup rejection, manual provenance, human preview, stale/duplicate approvals, forced AI verification, persisted pending-move recovery, shared board coordinates and floating panel sizing.
- Build-time chess regression suite and 20 observation-gate cases passed. No Candy Land Resources were packed.
- Existing local host 0.4.1 returned configured=true, busy=false; no service restart performed for this release.
- Real-board recognition accuracy, scan latency, physical registration drift and founder UX acceptance remain unverified.

The later request for Quick Play capture-first, draft registration saves and shared online games is not included in 0.1.10; it is the next implementation increment. Quick Play in this release still opens a virtual game. Use NEW GAME → REAL BOARD for the test route above.

Headset install succeeded. Installed version 0.1.10/code 11 launched (PID 379). Startup reported `FIRST_STEPS_WIRELESS_READY: CONNECTED HTTP 200` and `FIRST_STEPS_COMPANION_CONNECTION: HOST_REACHED`; ADB reverse list was empty. Optional OpenXR shared-space/camera-extension warnings were present; no assertion of camera accuracy follows from startup.
