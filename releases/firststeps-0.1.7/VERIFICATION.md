# Verification - November 2, 2026

Implemented: on-device known-plane checker-pattern fitting with perspective projection and edge refinement; second-frame agreement; explicit board acceptance; two-view, confidence-gated piece proportion estimates; session geometry persistence; large contextual turn heading; TLS local-network pairing with certificate pinning and app-lifetime reconnection checks.

Synthetic tests passed for perspective fit, metric size/pose on the supplied plane, blank table rejection, inconsistent fits, correct/wrong certificate pins, and refusal of unencrypted LAN endpoints. Scene tests passed for acceptance before play, piece-profile persistence, manual fallback, physical AI proposal gating, virtual AI play and large turn headings. These do not establish real-camera fitting or silhouette-measurement accuracy.

Wireless host: authenticated HTTPS readiness returned 200 using the generated certificate as a trusted root. Existing Node private-network firewall permission was reused; adding a narrower rule was denied by Windows administrator permissions, so no firewall policy was changed. Secrets remain ignored. No paid endpoint or recurring OS job was added.

Founder confirmed Meta answered in the 0.1.6 headset build. That is historical USB-path acceptance, not acceptance of this new wireless or calibration build.

Final calibration regression passed, including rejection of stale piece-size proposals after a failed rescan. Unity live HTTPS Meta response, readiness preflight and busy retry checks passed; eight Node host contract groups passed.
