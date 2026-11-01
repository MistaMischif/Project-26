# First Steps 0.1.4 - game-over and replay test

The completed-game result is four times the normal status text size and bold. The post-game panel offers a large gold PLAY AGAIN button and MENU, with no stale move instructions or pause/camera controls. One press starts a fresh game with the same board mode, opponent and color; the completed session remains saved. Repeated replay presses cannot reset an active game. Physical play still requires the real pieces to be reset and applicable alignment/recognition checks to pass.

MENU replaces HANDOFF during play. Existing handoff options remain inside it. The general Living Table source shares this UI; this downloadable package is the chess-only First Steps build.

Recognition is unchanged from 0.1.3: see ../firststeps-0.1.3/VERIFICATION.md. Real-board/all-piece accuracy and full-scan latency remain open. The local host is available in the 0.1.3 release; this update does not restart services or add dependencies.

Test: finish a game, check the result and context, press PLAY AGAIN once, confirm 32 starting pieces and unchanged choices. Check MENU, both winning colors and a draw. This development build requires founder headset acceptance.

Dashboard: https://living-table-progress-jz.mistamischif.chatgpt.site
