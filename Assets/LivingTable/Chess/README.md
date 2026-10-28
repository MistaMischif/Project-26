# Living Table Chess MVP

Chess is a second game inside the existing Astra-based Living Table Quest app. Select CHESS from Candy Land setup; NEW GAME returns to chess setup, where CANDY LAND switches back. Existing scenes and the Android package are reused.

## Virtual play

Choose VIRTUAL, VS AI or 2 PLAYERS, and your color, then PLAY NOW. Pinch and lift a piece, then release on a glowing legal destination. Tapping the source and destination also works. Invalid drops reset. AI runs locally with a bounded depth-three search; no service or account is required. Two-player mode is local shared-table play, not network multiplayer.

The raised board, lightweight Staunton-style models, animated movement, viewer-facing status, purple setup buttons, circular beveled play buttons, pause, undo, and board relocation reuse the platform interaction formula. Promotion defaults to queen; cycle PROMOTE before making a promotion move. HELP explains the controls.

## Physical play

Use a full real chess set in the standard starting position. Select PHYSICAL, choose your color, and ALIGN BOARD. Pinch the near-left and near-right outer corners of the 8-by-8 playing area (exclude decorative borders), with your chosen color nearest you. PLAY NOW requests camera permission and captures a stable board appearance automatically; no user-trained piece model is required.

Move a piece and lift your hand away while looking at the whole board. Local per-square appearance changes are matched against legal moves and must remain stable before a turn advances. For AI turns, move the indicated real AI piece yourself. Captures require removing the captured piece; castling requires moving both pieces. This MVP uses all real pieces in physical mode, not virtual AI pieces mixed into a real set.

MANUAL lets you tap source and destination after moving real pieces; DONE confirms an indicated AI move. AUTO resumes observation. After UNDO, restore the real pieces to the displayed virtual guide and press BOARD READY. Moving the board during physical play restarts setup and requires alignment again.

## Rules and limits

Legal movement, king safety, captures, castling, en passant, all four promotions, checkmate, stalemate, common insufficient-material cases, claimable threefold/50-move draws, and automatic fivefold/75-move draws are implemented. This is a casual MVP, not a tournament arbiter: no clock, resignation/draw negotiation, intended-move draw claims, exhaustive dead-position solver, saved matches, remote multiplayer, voice, or chess-specific music. The AI has no claimed rating.

Camera recognition is an experimental registered-board change detector. It does not identify arbitrary pieces or reconstruct an arbitrary starting position. Board alignment, viewpoint, lighting, shadows, and piece contrast affect reliability; no held-out real Quest chess dataset has been validated. Manual controls remain available. Frames stay on-device and are not saved or uploaded.

## Validation and build

Run LivingTable.Editor.ChessBuilder.PrepareAndTest in Unity batch mode. It checks initial-position perft (20/400/8902), Kiwipete (48/2039/97862), special moves, terminal positions, draw handling, AI legality/mate, physical change inference and rejection, virtual hand moves, camera stale/duplicate guards, manual fallback, and switching back to Candy Land. Scene screenshot: Logs/Chess-MVP.png. Run LivingTable.Editor.CandyBuilder.PrepareAndTest for shared-platform regression coverage.

Build the shared app with LivingTable.Editor.CandyBuilder.BuildQuest, BEE_BUILD_THREADS=4 and -job-worker-count 4. Output remains Builds/LivingTable-CandyLand-Quest.apk and includes both games. Device startup does not establish physical recognition accuracy.

## Persistent sessions (0.3.0 test build)

See ../SESSIONS.md for saved games, human/AI handoff, physical/virtual continuation, and the explicit limits of physical verification. These controls supersede the earlier restart-on-relocation and no-saved-matches behavior.
