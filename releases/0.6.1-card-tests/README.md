# Living Table 0.6.1 — Special-card test and board alignment

In Candy Land setup, press TEST SPECIAL CARDS. Plumpy draws immediately. After landing, press REPLAY SPECIAL or NEXT SPECIAL to cycle all six characters. NEW GAME exits to normal setup. This is an unsaved virtual sandbox: it does not change a saved game or its deck, and AI autoplay is disabled.

Card artwork now faces the wearer during reveal, uses an explicit texture binding and a mobile-safe dissolve shader, and sits farther above the card face. The floating status panel hides during reveal/dissolve so it cannot cover the portrait.

Corrected Mr. Mint's picture/orange order and the following early path coordinates, plus the final bend, against the supplied board image. Old saved positions at the swapped pair migrate by space identity. Card rules, vision destinations, landing glow and virtual movement share the corrected path. Committed pieces stay on their square; shared-space groups use bounded offsets and smaller pieces instead of collision avoidance pushing them onto other squares.

Landing glow uses fully saturated square colors and alpha blending instead of washing out the printed color with additive white. Landing and card sparkles are twice their previous size.

Validation details are in VALIDATION.txt. Real-board calibration and headset visual comfort require device testing; desktop rendering does not prove on-device appearance.
