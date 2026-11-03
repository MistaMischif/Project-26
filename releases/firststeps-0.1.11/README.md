# Living Table First Steps 0.1.11

Quest 3 chess test build, code 12. No online multiplayer, new backend dependency or service restart. Existing paired host 0.4.1 remains compatible.

## Quick Play: capture, continue virtually, return physically
1. Choose **QUICK PLAY**. Stand or sit over the real board, look down with the full playing area visible, and press **SCAN BOARD**. If automatic fitting fails, choose **MANUAL CORNERS**.
2. Review the detected pieces and correct unknown/wrong squares. **ACCEPT POSITION** proceeds to four outer playing-area corners (near-left, near-right, far-right, far-left), then king-based set sizing and color selection. Manual-corner fallback performs the position scan after geometry instead.
3. Choose whose turn it is and known castling/en-passant rights. The large purple **START GAME** begins virtual play from this exact accepted position, against AI in the other seat.
4. Every committed move saves to the same session. **MAIN MENU → CONTINUE** reopens it. Leaving during capture preserves a reviewable draft; it does not silently start a game or discard the captured layout.
5. To return physically, choose **MENU → REAL BOARD**. Register the board, then move the real pieces to match the latest virtual guides. **CHECK BOARD** must verify the saved position before play resumes. A different legal position is still a mismatch.

## Interaction feedback
- Aim at the menu icon and pinch; reaching across the board is unnecessary. Direct touch still works.
- Setup and recovery instructions float in the upper center of view, independently of the registered board.
- A spinner indicates active scanning. Squares show blue while read, green when observed, amber for uncertainty/mismatch. These are scan states, not claims of perfect model accuracy.
- Captured pieces appear in two trays outside the board with clear totals. Known captures show actual models, including en passant. Missing pieces predating a scanned midgame use question-mark markers and an explicit unknown-type count.
- No raw camera photos are stored with the save. Geometry, color, scale, accepted game state and subsequent history are stored locally. Imported prior history remains unknown.

90 synthetic scene checks and the build-time rules/observation suite are recorded separately. Real-set accuracy, latency, hand-ray comfort, world-lock drift and founder acceptance remain hardware test requirements. The local vision host is still required for camera identity checks; AI chess and explicit manual fallback work without it.

Patent wording remains “Patent application in preparation.” This is not a filing or submission receipt.
