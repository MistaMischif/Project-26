# First Steps 0.1.6 - physical alignment and Meta connection recovery

Physical setup shows a transparent 32-piece starting overlay, an 8x8 grid, four labeled playing-area corners, and board/square measurements. Select near-left then near-right corners of the PLAYING squares, excluding the frame. The second corner previews scale live. During saved-game recovery, ghosts show the saved position. Guides disappear after reconciliation; alignment alone never approves a physical position.

Meta connection checking authenticates pairing and checks the local runtime has the Meta model. CHECK CONNECTION distinguishes missing pairing, missing runtime/model and lost transport. Settings reload on retry. The cold-model answer timeout increased. The companion remains text-based, local and read-only.

The chess opponent runs on the Quest independently of Meta. Tests cover a legal physical AI proposal (not committed until moved) and a virtual AI response without companion requests. Camera verification still requires the separate host or clearly labeled manual fallback.

For Meta and camera use, keep USB connected and run Prepare-Local-AI.cmd from the host ZIP. Reconnecting USB or closing Unity can remove adb forwarding: rerun this launcher after either event, then CHECK CONNECTION in game. No always-on reconnect service was installed. This release does not claim standalone on-headset Meta inference or wireless host pairing.

Test: REAL BOARD, ALIGN BOARD, match grid and ghosts, PLAY NOW. Check both player colors and different board sizes. Test ASK THE TABLE, then disconnect USB and confirm connection guidance; virtual chess AI must still play. In physical manual mode, move the AI's real piece before MOVED IT.
