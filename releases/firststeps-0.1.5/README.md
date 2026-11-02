# First Steps 0.1.5 - Meta companion, fingertip grabs and room recovery

- ASK THE TABLE: three text questions answered by a local Meta Llama model, grounded in the current chess engine state. Answers cannot move pieces or rewrite the game.
- Correct OpenXR thumb/index joints replace the legacy joint IDs that caused palm-offset grabs. Invalid tracking cancels the grab safely.
- Recenter/tracking-origin changes or an out-of-reach board trigger new placement. The old board hides, the game is held, and the wearer gets a nearby placement prompt. Failed old-anchor cleanup cannot return the board to its old room. Physical play must realign/reconcile afterward.

Install over the existing First Steps app to preserve saves. Extract the host ZIP and run Prepare-Local-AI.cmd (Node 22+, Ollama and adb required; missing model weights download on first preparation). Keep USB connected. No paid API is required. No credentials are included.

Headset test order:
1. Grab pieces with both hands: confirm the held point stays at the thumb/index pinch and tracking loss returns pieces safely.
2. Start a game, select ASK THE TABLE and try all three questions. Close it and continue; no answer may execute a move.
3. Midgame, move to another room and recreate the room-scale boundary. Confirm the old board hides and the placement prompt follows you. Place again, confirm the same session/position, and verify physical mode requires realignment.
4. Finish the game and use PLAY AGAIN; existing game-over behavior must remain intact.

This is button-driven text assistance, not voice chat. The Meta explanation is distinct from Qwen camera recognition. Prose may be imperfect; the engine remains authoritative. Actual room/boundary and hand-tracking behavior needs founder testing. Recognition accuracy/latency limitations remain in the 0.1.3 record.

Dashboard: https://living-table-progress-jz.mistamischif.chatgpt.site
