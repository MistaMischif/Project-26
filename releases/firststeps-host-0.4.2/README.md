# Living Table AI host 0.4.2

November 4, 2026: the host on 8790/8791 and dedicated runtime on 11436 were stopped; the PC and Quest remained on the same Wi-Fi. The cause of those processes stopping is not established. The existing app checks readiness but cannot start a stopped PC process. Restored the established TLS/Wi-Fi host without changing the APK or game saves.

Installed, with explicit founder approval, the current-user scheduled task COLTRAN Living Table AI. It starts the hidden supervisor at Windows login, retries failed supervisor execution, and polls host/runtime readiness every 15 seconds. It restores missing host/runtime processes through the existing local launcher. Only the dedicated Living Table runtime on 11436 is managed; the unrelated 11434 runtime is preserved. No credentials are embedded in the task or this release. A user must log in, and the PC/model drive/network must be available. Power-off, manual sleep, Wi-Fi loss and changed pairing/IP addresses remain external limits.

While authenticated headset requests are recent (90 seconds), the supervisor requests system-awake status; this releases when activity expires or the supervisor exits. Monitor-only probes do not count as headset activity. No Windows power-plan settings were changed.

Fixed a false failure: a grounded Meta answer, "Resume the game to continue.", was rejected solely because it was shorter than 30 characters. Short prose now passes while fact references, legal moves, context binding and output limits remain enforced. The actual inference route returned HTTP 200 with Meta llama3.2:3b after the fix.

Verification: 10 HTTP/contract tests passed. An identified idle host was deliberately stopped; a different process returned automatically in 15.4 seconds. Readiness and actual inference were verified. Runtime startup was verified; runtime-crash and Windows reboot/login have not been physically tested. Windows powercfg enumeration required administrator privileges, so native sleep-hold return status is recorded separately. Founder headset acceptance is pending.

Install on an already configured self-hosted machine: run Install-AI-Availability.ps1. For initial model/TLS setup, run Launch-FirstSteps-Test.ps1 with the existing model directory (or use -PrepareModels for a new installation). Models, private pairing and certificates are excluded from this archive. No paid services.

Disable: Stop-ScheduledTask -TaskName 'COLTRAN Living Table AI'; Disable-ScheduledTask -TaskName 'COLTRAN Living Table AI'. Disabling supervision does not forcibly stop an in-flight game host. Existing on-device chess AI is independent of Meta conversation and camera recognition.

Native sleep-hold API returned success: SLEEP_HOLD True recorded at 07:07:54 Central. The scheduled supervisor is running. Actual Meta inference returned HTTP 200. Source: 51de2c9.

Sleep hold released automatically at 07:09:26 Central after activity expired. Published archive digest verified against GitHub. Dashboard source: 498e81b7906131f3fe144eff76870dfd879d5f7d.
