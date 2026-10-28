# Living Table Chess 0.4.0 — contest test build

The contest build opens directly into chess. It uses the existing Astra placement, hand tracking and spatial-anchor foundation. Candy Land remains available in the general project and previous release; its Resources content is excluded from this APK and checked against Unity's packed-asset report.

## The demonstration

1. Choose a virtual board and human opponent. Start a game and make a move.
2. Open **HANDOFF**, choose **AI TAKE OVER**, then **BACK TO GAME**. The AI occupies the opponent's seat with the same position and history.
3. Open **HANDOFF**, choose **HUMAN RETURNS**, then return to play. A previously proposed AI move must finish first; changing the controller cannot replace it.
4. Choose **USE REAL BOARD** in Handoff. Return to the game, align the near-left and near-right outer board corners, and arrange the physical pieces to match the displayed guide. Play remains held until reconciliation.
5. Choose **GO VIRTUAL** to continue with virtual pieces. A pending physical AI move executes once after leaving Handoff and resuming. The session, move history and side to move remain intact.
6. **SAVE AND EXIT** returns to setup. **RESUME GAME** reloads the selected saved game. **SAVE n / total** selects another journal. Paused games resume paused; use **RESUME**.

Each physical AI instruction names the piece and both squares. Captures, castling, en passant and promotion include the additional required action. Both board files and ranks are labeled. Normal play has fewer controls; optional settings are grouped under Handoff, which holds play while open.

## Honest limits

The on-device camera detects changes in registered square appearances with legal-move constraints. It is not a general chess-piece identity recognizer. A stable appearance match is not proof against cheating. Returning to a real board after virtual moves invalidates the old camera reference and requires an explicit manual check. Manual and imported history stays labeled. A game begun outside Living Table must still be imported manually, including side to move, castling rights and en-passant state; earlier history cannot be reconstructed.

AI and journals run locally, without a required paid API or service. This is shared-table play, not network multiplayer. The virtual-room option hides passthrough; it is not a separate furnished environment.

This is a test release, not a claim of contest eligibility, submission, measured Quest frame rate or validated camera accuracy. The Meta Competition channel, submission video, membership and final rules review remain separate submission work.

## Headset acceptance pass

Use an actual chess set in ordinary room lighting. Start with a human move, transfer to AI, move its real piece, return control to the human, switch to virtual, make another move, and return to the real board. Also try a wrong AI destination, a half-completed castle, temporarily covering the board, putting the headset to sleep and reopening the app. No incorrect or partial action should advance the journal; each recovery should explain the next step. Test this with someone who has not been coached, and record failures and frame-rate evidence before calling the experience seamless.

The APK is published as a GitHub Release asset and kept locally beside these notes, rather than committed into Git. No credentials or signing keys are included.
