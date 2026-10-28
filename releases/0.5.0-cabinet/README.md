# Living Table 0.5.0 — chess grip, sound and cabinet styling

This general-app build opens into chess and includes Candy Land again. Choose **CANDY LAND** on chess setup; from an active chess game, use **HANDOFF → SAVE AND EXIT** first. Candy Land's setup has **CHESS** to return. The separate chess-only contest build/profile remains available.

## Changes

- A held chess piece follows a contact point between forefinger and thumb, rather than putting its base at the pinch point. The former height clamp is removed. Tracking loss cancels the grab without committing a move. Releasing outside the board or far above it returns the piece; a legal drop settles smoothly into its square.
- Local pickup, wooden placement, return and button sounds. Virtual AI and tap moves also receive lift/landing feedback. No external sound service or paid API is required.
- Chess: raised dark burl-oak-style cabinet, brushed-gold inlay and molding, glossy gold buttons, and a matching wood-and-gold viewer-facing panel.
- Candy Land: cream cabinet, glossy purple side rails, gold trim and finials, cream/purple/gold floating panels, and gold-edged controls. The supplied classic board artwork and gameplay remain intact. The large green Draw button retains its role and color.

The materials are procedural interpretations of the supplied art direction, with actual geometry for board depth, borders and buttons. Rendered previews are included as `Chess-preview.png` and `CandyLand-preview.png`.

## Check on Quest

Pinch a pawn near its stem, lift it well above the board, move sideways and lower it to a legal glowing square. The grip point should remain between your fingers; listen for a lift and a soft placement. Try an invalid drop and briefly hide your pinched hand: neither should advance the game. Check both games' panels and controls in your room lighting.

Saved sessions and the existing handoff safeguards remain in place. Real-board camera recognition remains experimental; these visual and interaction changes do not establish camera accuracy or cheat-proof enforcement. Headset grip feel and perceived audio loudness require a user play check.

The downloadable APK is a GitHub Release asset and a local versioned artifact. Source, checksums and validation notes are versioned; credentials and signing keys are not included.
