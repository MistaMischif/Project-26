window.LT_EVIDENCE = {
  "schema": 1,
  "asOf": "2026-10-25T22:01:08.240734+00:00",
  "evidenceDate": "2026-10-25",
  "unity": "6000.3.9f1",
  "build": {
    "result": "Succeeded",
    "errors": 0,
    "warnings": 978,
    "duration": "00:15:50.5997792",
    "apkBytes": 66037926,
    "sha256": "b6995a6422b9e26a745dcde9bd6991a34d70f19ea64e21eb3bacdcea0aa3368d",
    "file": "LivingTable-Quest.apk",
    "applicationId": "com.UnityTechnologies.com.unity.template.urpblank",
    "version": "0.1.0",
    "architecture": "arm64-v8a",
    "signature": "APK Signature Scheme v2 verified",
    "newerThanRuntime": true
  },
  "smoke": {
    "passed": true,
    "newerThanRuntime": true
  },
  "install": {
    "status": "Confirmed",
    "device": "Quest 3",
    "date": "2026-10-25",
    "evidence": "ADB install -r returned Success in this project chat. No gameplay result was reported."
  },
  "sources": [
    {
      "name": "Unity build report",
      "path": "Builds/LivingTable-build-report.txt"
    },
    {
      "name": "Play Mode smoke test",
      "path": "Logs/LivingTable-smoke.log"
    },
    {
      "name": "APK signature and architecture",
      "path": "aapt + apksigner verification in project chat"
    },
    {
      "name": "Device installation",
      "path": "ADB Success in project chat, 2026-10-25"
    },
    {
      "name": "MVP scope and controls",
      "path": "Assets/LivingTable/README.md"
    }
  ],
  "verificationCurrent": true,
  "installCurrent": true
};
