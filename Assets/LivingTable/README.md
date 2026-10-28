# Living Table MVP

> Current playable app: Candy Land and Chess share `CandyLand/CandyLandQuest.unity` and the existing Android package. See `CandyLand/README.md` and `Chess/README.md`. The sandbox instructions below describe the earlier platform sample, not the current game launcher.

Unity 6000.3.9f1; extends MRBoardMVP and the project's existing Meta XR rig.

## Try it

Open `Assets/LivingTable/LivingTableDesktop.unity` and press Play. Click a piece and then an empty square. Next game cycles between Tabletop Sandbox (eight pieces) and Constellation Garden (six pieces); Reset pieces restores the selected layout. Completed moves animate with a jump and landing pulse. A softly pulsing frame supplies the ambient living effect.

Open `Assets/LivingTable/LivingTableQuest.unity` for the headset version. It copies the passthrough and hand-tracking rig from LiveTable001, removes experimental pinch handlers, and adds LivingTableExperience. The source scene is preserved. Build this scene for Android using the project's existing Quest settings. The original build-scene selection is not changed; explicitly include LivingTableQuest and exclude LiveTable001 when building this MVP.

## On the headset

1. Look around so room tracking is available. Pinch with thumb and index finger at the physical table's center and surface height. The board is horizontal, aligned to your current viewing direction, and measures 36 cm square (40 cm including its frame).
2. Release your pinch. Pinch above a piece to select it, then above an empty cell to move it. Input accepts fingertips from 4 cm below to 16 cm above the calibrated surface. Both hands work; tracked, high-confidence fingertip skeleton data is required.
3. Pinch over the labeled NEXT GAME, RESET, or MOVE TABLE pads along the near edge. Move table erases the saved anchor and asks for a new placement. Reset also safely cancels a move in progress.
4. Relaunch in the same room to restore the saved board position. If saving fails, the table remains usable for that session and reports the problem. If restore fails, pinch to place again.

World-space instructions appear before placement and above the board afterward. No screen-space UI is required on the headset.

## Scope

This is a local, single-board interaction sandbox. Layout selection and piece positions reset on launch. Only the spatial anchor UUID is persisted. There are no turns, captures, scoring, multiplayer, physical-piece recognition, automatic surface detection, or shared anchors. Physical moves may also be mirrored through MRBoardPlatform.ConfirmPhysicalMove. An anchor supplies location, not detection of the physical table.

## Validation

Living Table > Create MVP Scenes regenerates both scenes from the existing Meta rig. Save any scene customizations elsewhere before regenerating. LivingTable.Editor.LivingTableBuilder.SmokeTest runs a real Unity Play Mode check covering initial pieces, rejected occupied/out-of-bounds moves, concurrent-move prevention, completed animation, reset during animation, layout cycling, and invalid game definitions. It exits Unity when complete and is intended for batch runs.

Headset acceptance still requires a device: verify passthrough, permission prompts, fingertip input, readable text, saved-anchor relocalization across launches, and repositioning in the intended room. Desktop tests do not verify these device behaviors.

## Persistent sessions (0.3.0 test build)

See SESSIONS.md for saved games, human/AI handoff, physical/virtual continuation, and the explicit limits of physical verification. These controls supersede the earlier restart-on-relocation and no-saved-matches behavior.
