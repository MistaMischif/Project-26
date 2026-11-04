# First Steps 0.1.19 — Align, size, play

- Accepting the four-corner alignment opens king sizing immediately. No position scan is requested between these steps.
- The sizing screen offers KING + / KING - and the purple PLAY button. Pinch/pull sizing remains available. PLAY saves scale and enters the game directly.
- New Game and fresh Quick Play use the standard chess starting position, White to move. They do not capture an unknown midgame layout during setup. Existing captured drafts retain their explicit review flow rather than silently replacing their pieces.
- Continue retains saved pieces, side-to-move and history. Physical reconciliation still runs during continued physical play; camera verification remains necessary before committing physical moves. This change removes the setup scan screen, not verification of legal moves.

## Headset check
1. New Game: place board, Continue, four corners, Align. Confirm exactly one king guide appears immediately without a scan screen.
2. Adjust size using the king or KING + / KING -. Press the purple PLAY button. Standard pieces and White's turn should appear immediately.
3. Repeat with Quick Play on a standard physical setup.
4. Continue a saved Black-to-move game: align, size, Play. Verify position, history and turn are unchanged. Follow physical reconciliation prompts if the real board differs.

Automated checks and build evidence are included. Real-headset flow acceptance remains pending MASTER OMEGA testing. Vision still uses the existing self-hosted service; no new infrastructure is introduced.

Rollback: firststeps-v0.1.18. Same package ID; install -r preserves saves.
