# Verification — First Steps 0.1.12

Source: 6887050142b6530148fc1b7850565ec65267573e.

136 synthetic scene checks passed: 72 physical-flow, 50 capture/Continue with metric acceptance, 14 metric preview/fallback/dimension/world-frame checks. 32 synthetic calibration checks passed across three board sizes, broad localization, refined image corners, depth/stereo metric geometry, tilt and rejection cases. Build-time chess rules and 20 observation gates passed. Failed intermediate compile/test logs remain in Logs; the asynchronous camera-failure test was corrected to await its callback.

No measured physical camera accuracy, drift, jitter, latency or founder acceptance is claimed. Calibration constraints and independent measurement procedure are in Assets/LivingTable/Chess/SURFACE_CALIBRATION.md. Existing 0.1.11 remains the rollback release. No paid API, new model dependency, SDK upgrade or native plugin was introduced.

Unity build succeeded with zero build errors. ARM64 package com.livingtable.firststeps, version 0.1.12/code13; APK signature verified. SHA256: 99ff7d29fe8989e2083adcd28b22a4c2a9f45480139d57fecbf8b7a5e3db9d7d (88,323,447 bytes). Installed and launched on Quest 3, PID 16689. Startup reported FIRST_STEPS_WIRELESS_READY: CONNECTED HTTP 200 and FIRST_STEPS_COMPANION_CONNECTION: HOST_REACHED. No ADB reverse. Existing optional OpenXR shared-space/camera capability warnings remain; startup does not establish real camera-frame alignment accuracy. No service restart was needed for this release.

Dashboard checks passed (contest plan, latest build, tracking). Dashboard source b1d2d669f4d3097b6d740a9e401a8c74baaaadff deployed successfully as appgdep_6aea5ac2f65481918989271a1e3a6da3: https://living-table-progress-jz.mistamischif.chatgpt.site . Owner-private audience preserved. GitHub Release APK digest independently matched the local SHA256 above.
