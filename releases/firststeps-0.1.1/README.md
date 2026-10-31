# Living Table First Steps 0.1.1 — board handling

Pinch the outside left and right rims with both hands. Move both hands together to lift or translate the board; turn them to rotate it horizontally. The board stays level and keeps its size. Releasing either hand, or losing reliable tracking, ends the grab at the last valid pose. Release both pinches before starting another grab.

MOVE BOARD now hides the entire board, pieces and board UI immediately while the new-placement prompt is shown. Pinch the new table center to place it again. Direct two-hand manipulation keeps the board visible while held.

Turns and camera acceptance pause during movement. The current game, move history and original paused state are preserved. Physical mode requires realignment afterward. New spatial anchors are saved on release where the headset permits it; if anchor persistence fails, the current placement remains session-only.

Validation: 23 geometry/live-scene checks, 3 calibrated-board offset checks, 20 observation-gate cases and the chess rules regression suite. The live-scene checks cover hidden placement, movement, release, grip rearming, unchanged move history and preserved pause state. Actual hand tracking and spatial-anchor persistence require a headset acceptance pass.

Installation updates com.livingtable.firststeps in place. Keep the existing local host and USB bridge for Meta vision; the connection settings and game journals remain in that app's data directory.

Dashboard: https://contest-command-jz.mistamischif.chatgpt.site/#hack

Superseded before publication by First Steps 0.1.2, which includes this board-handling work. No 0.1.1 headset acceptance is claimed.
