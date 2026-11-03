# Living Table First Steps 0.1.9

Quest 3 test build: simplified chess setup, physical/virtual counterparts and corner adjustment.

- First screen: LIVING TABLE, by COLTRAN STUDIOS; New Game, Continue, Quick Play. Quick Play starts virtual chess against the on-device engine. The home menu follows your view until selection, including when tracking settles after launch.
- Four neutral gold corner knobs. Pinch two separate knobs together, then move, spread or rotate your hands. The knobs light up and the grid pulses. Release to place. Physical geometry requires acceptance before continuing.
- Square teal human/real controls on the left, orange-red AI controls on the right, two rows, one larger central action. Every cap and hit area stays within the central 5/8 board footprint. Controls sit 18 cm above the board, separated from piece contacts.
- MENU > OPTIONS > PIECES +/- adjusts the entire set. Setup has OPTIONS too. Size is saved per session; measured physical profiles use corrected paired indices.
- Physical mode retains translucent virtual counterparts. A verified physical move updates the corresponding virtual piece. Uncertain observations cannot advance the journal.
- ALIGN BOARD attempts Quest environment-depth tabletop detection, then fits the checker pattern across two camera views. Accept the fit before piece sizing. Depth availability, fit accuracy and actual piece recognition still require real-board acceptance.
- Local host 0.4.1 remembers the selected model directory, starts without a USB device for already-paired Wi-Fi headsets, and constrains Meta replies to valid supporting fact IDs. No OS startup task or background service was installed.

## Headset test order
1. Launch: verify the three-entry home screen and branding. Quick Play should start one new game.
2. Open MENU > OPTIONS; enlarge and shrink the whole piece set. Resume the saved game and verify the size persists.
3. Pinch two corner knobs, translate, rotate and resize. Release one hand; verify the board stops and the saved position does not change. Re-grab only after both pinches release.
4. Start a real-board game. Look down at the whole board and press ALIGN BOARD. Review the overlay against all 64 squares; accept only a correct fit. Test both viewing angles for piece sizing.
5. Complete the initial position check, move e2 to e4 and clear hands. Verify the translucent pawn follows exactly once. Let AI propose its move, execute the real move, then verify one commit.
6. Unplug USB while staying on the same Wi-Fi; ASK META > HELP CONTINUE must still answer. The PC and local host must remain powered/running.
7. Check accidental-input resistance near every knob and while moving tall pieces; test after changing rooms too.

Do not mark these hardware tests passed based on editor tests. Camera accuracy, depth alignment, touch comfort, performance and Wi-Fi recovery on this exact APK remain founder acceptance items.
