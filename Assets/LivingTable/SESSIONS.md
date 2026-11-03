# Continuous Living Table sessions

Chess and Candy Land now use the same local, append-only checkpoint store. A session keeps its identity when the opponent changes between human and AI, or when the surface changes between a real board and a virtual board. No account, subscription, paid API, or remote server is needed. Files live in Unity's persistent-data directory under `sessions/<id>/`; copying that directory preserves the journal on hardware the user controls.

## Controls

In Candy Land, press **SESSION** to open the continuity controls; **BACK** restores the normal one-row toolbar.

- **AI TAKE OVER / HUMAN RETURNS** changes control of the opponent's seat. Candy Land retains the existing player count; HUMANS RETURN makes the other seats human-controlled.
- **GO VIRTUAL / USE REAL BOARD** carries the current state forward. A physical return blocks play until the real layout is reconciled. Candy Land waits for an in-flight animation to finish before changing surfaces; an already drawn card remains saved throughout.
- **VIRTUAL ROOM / SEE ROOM** switches between a dark virtual environment and passthrough around the virtual board.
- **SAVE EXIT / NEW GAME** in chess, or **NEW GAME** in Candy Land, saves the current session and returns to setup. **RESUME SAVED** restores it. The adjacent SAVE selector cycles stored sessions.

Chess saves the starting position, every legal move, repetition history, side to move, castling and en-passant rights, move counters, draw claims, and an unexecuted AI proposal. A proposed physical AI move remains locked when controllers change: a different move cannot replace it. Physical play commits only after recognition or a recorded manual fallback. Ordinary gameplay no longer exposes the old silent undo button.

Candy Land saves player positions, current turn, pending card, remaining deck, discards, and the random-generator cursor. Physical card draws consume matching cards from the same deck inventory. A spent card cannot be drawn again until the deck is exhausted and reshuffled. Virtual continuation uses the stored remaining order; it does not claim to know the unseen order of a real deck. Returning to physical play requires using the same remaining card inventory. The app cannot inspect a face-down deck's order.

## Joining an existing chess game

Choose the surface and player color first. **IMPORT GAME** opens a board editor: choose piece type and color, tap squares, set whose turn it is, and explicitly set castling rights and any en-passant square. **USE POSITION** validates and starts a new imported session. The initial editor position is standard chess, with castling rights disabled until selected. In First Steps 0.1.10, select all four outer playing corners in order: near-left, near-right, far-right, far-left, excluding the decorative frame. Size the whole set against your king before scanning.

An imported position is permanently labeled manual/imported. The app cannot reconstruct earlier repetition history or prove castling eligibility from a photograph. The UI import begins the move counter at zero; tracked history is enforced from the import onward. First Steps 0.1.10 implements a local-model scan of all 64 squares for a new chess import; real-set accuracy is not yet accepted. Importing a previously untracked Candy Land game is not implemented.

## Physical reconciliation and integrity limits

First Steps 0.1.2 adds a full-board identity reconciliation path. After realignment, the camera takes one stable snapshot of all 64 squares, including empty squares. The local model receives batches of four crops without the expected answer. Every response must match the saved position and revision; missing, duplicate, uncertain or mismatched squares keep play locked. A fresh camera appearance check and hand-activity guard must also pass before resuming. A rejected scan names the expected square contents and exposes SCAN BOARD to retry. The saved position, pending AI proposal and hidden chess rights never come from the model.

This is development functionality, not validated physical-board accuracy. First Steps 0.1.3 uses visibility-gated local Qwen recognition. It passes the limited pawn/empty/occluded synthetic fixtures, but a 64-crop workload still took 30.7 seconds; a full scan can be slow or fail. Real-set accuracy across all piece types remains unverified. Hardware acceptance, recognition tuning and convenient judge access remain open. A successful comparison is model-assisted verification, not proof against deliberate manipulation. MANUAL CHECK explicitly enters manual mode and permanently marks the session assisted; it cannot be presented as automatic verification.

The general Living Table build retains its previous behavior: Chess can compare a returning board against its saved per-square camera appearance. Changed appearances block play; good lighting, alignment, and a similar viewpoint matter. A new physical layout after virtual moves has no matching camera reference, so the user must arrange the saved layout and explicitly choose **MANUAL CHECK**. Candy Land likewise displays the saved layout and requires a recorded manual reconciliation. Camera recognition then resumes when available; manual controls remain a fallback.

Manual checks and imported positions remain visibly marked. Appearance matching is not piece-identity authentication, and manual confirmation is not proof of correct physical placement. This release enforces legal state transitions and prevents silent reset/reroll during mode changes; it does **not** provide cheat-proof physical verification, trusted remote arbitration, or monitoring while the app is absent.

Checkpoints use sequential files, atomic publication, and a SHA-256 chain. Corrupt/incomplete histories are refused rather than silently reset. The chain detects corruption, not a determined owner rewriting local files or deleting an entire journal. Saves are synchronous at gameplay boundaries; a save error locks further play and exposes RETRY SAVE. A retry restores the presentation to the retained logical state.

## Validation

`LivingTable.Editor.SessionTests.Run` checks full chess replay, repetition persistence, pending/empty card serialization, remaining deck and RNG continuation, physical deck conservation, corrupted-journal refusal, board-appearance mismatch, human/AI handoff, physical reconciliation gates, locked AI proposals, import provenance, and both games' scene-level resume paths. Existing chess and Candy Land regression suites remain required. These tests do not replace real-board Quest camera validation.

## Shared viewing gap

First Steps has no verified dedicated spectator receiver in this release. For future shared viewing, use a read-only projection of the authoritative public board/turn state; viewing must not grant player or host controls, and connection credentials/private account UI must stay off that view. A supported Quest-to-display pair, reconnect behavior, audio/latency and an accessible fallback still need physical-device verification before casting support is advertised. Recognition work does not establish that capability.


## First Steps 0.1.10 physical flow

Setup now selects four playing-area corners, then sizes all pieces using the player's king. The registered board transform owns both the virtual grid and the piece-square mapping. The backdrop follows the viewing direction independently; it does not move the board. Room/origin recovery still pauses play and requires physical realignment. A physically displaced real board is not continuously tracked: realign it before continuing. Hardware world-lock accuracy remains an acceptance test, not an established guarantee.

A new physical game scans every square, reviews detected/unknown pieces, asks whose turn it is unless only one side is legal, and explicitly sets unknown castling/en-passant history. The final single purple circular **START GAME** action imports the reviewed position. Earlier move/repetition history remains unknown. A changed or stale camera view cannot silently start a camera-verified game. Manual edits or STANDARD SET are explicitly assisted; they do not certify camera accuracy.

**CHECK BOARD** actively requests verification. A known pending AI move is checked against its resulting position, including origin removal, captures, castling rook movement, en passant and promotion; it no longer depends on a minimum histogram change to begin checking. Unrelated visual changes cause a full-board check. After restoring a session, a pending AI move requires full-board verification of the proposed resulting layout. Its saved proposal cannot be rerolled or committed twice.

Detected human moves display a virtual preview while authoritative history remains unchanged. **ACCEPT MOVE** requires an unchanged fresh camera frame before committing. The AI's virtual target previews next; its physical move must be verified before returning control. Only legal moves are accepted. Camera identities are observations, not permission to overwrite saved history. Local vision currently requires the paired self-hosted computer; offline chess and labeled manual fallback remain available.

Scans have progress, cancel, visible errors and a 120-second overall bound (12 seconds for a final acceptance frame). This is a failure bound, not a performance target. Failed scans expose review/manual controls. The persistent backdrop menu supports return to main menu, quit and panel sizing. Gameplay controls sit at the board's base, with one central circular purple next-action button, teal human controls and orange-red AI controls. The backdrop is positioned beyond the board and can be scaled separately.

Verification: see releases/firststeps-0.1.10/VERIFICATION.md. Synthetic tests are not physical-camera or founder acceptance.
