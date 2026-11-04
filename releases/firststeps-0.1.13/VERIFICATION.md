# Verification — First Steps 0.1.13

Source: c65501bceb505498c94116ec558d7441ccb0a30f.

143 synthetic scene checks passed (72 physical-flow, 51 capture/Continue, 20 metric/UI/lock checks). 38 synthetic calibration checks include three board sizes and deliberate edge-offset rejection. Chess regression and 20 observation gate cases passed. Rendered instruction panel inspected locally; this is desktop synthetic QA, not headset proof. Final build log: Logs/build013-shipping.log.

Actual physical edge accuracy, drift, scan latency and founder acceptance remain pending. README documents the stricter fit gate, stationary grid and explicit Move Board behavior. Prior 0.1.12 remains available for rollback. No host service was restarted.

APK signature verified; ARM64 package com.livingtable.firststeps, 0.1.13/code14. Installed with adb install -r and launched as PID 31209; package manager confirmed the version. Startup reported wireless HTTP 200 and HOST_REACHED. No startup exception matched the inspected Unity log. SHA256: 61d33f96fe9438f92794239a7237e49c4b2c0f4fc2c58e09dab0cd29097a3647 (119375117 bytes). No app data cleared.

Dashboard source 2e715c9365e5f571ec2514bba4a91630617e015c passed its three check scripts and deployed successfully as appgdep_6aea8b6485cc8191891ae123623b63b6. URL: https://living-table-progress-jz.mistamischif.chatgpt.site . Private owner audience retained. GitHub Release APK digest matched the local installed artifact.
