# Living Table â€” Candy Land prototype

This is an opt-in extension of Astra's existing `LivingTableExperience`, table placement, Meta hand input, passthrough and spatial anchors. Original scenes and the original APK are retained. Historical project archives are reference material, not the implementation baseline.

## Play

Open `CandyLandDesktop.unity` for mouse preview, or `CandyLandQuest.unity` for XR. Use the existing pinch-to-place workflow, then the controls just outside the board.

The player-piece update shows two separated original-style molded figures before Play in the default virtual setup. `PLAY NOW` starts the selected setup preset. The setup presets are `VIRTUAL + AI`, `PHYSICAL + AI`, `MANUAL PHYSICAL`, and `TWO HUMANS`; the status line states which handling is selected. Physical manual mode intentionally leaves real pieces unduplicated.

All player figures use the same modeled silhouette, oval pedestal and raised white facial/clothing detailing, in red, blue, green and yellow. Front/back photographs are preserved in `archives/living-table/references/candy-land/player-pieces/`. This is a modeled approximation of those photos, not a scanned production mesh. The separately named board hosts remain placeholders.

- **Solo + AI:** choose 1â€“3 opponents. Candy Land has no strategic choices, so opponents draw and follow the same deterministic movement rules.
- **Virtual board:** displays the supplied 1984 board image with animated pieces.
- **Physical board:** PHYSICAL + AI defaults to local automatic card/piece recognition and board alignment. Grant camera access, select your real pawn color, and look at the whole board with hands clear before PLAY NOW. Lay one drawn card face-up beside the board each turn, including AI turns. Move your real pawn and lift your hand away; the camera confirms a stable legal destination. AI pawns move virtually. MANUAL exposes card selection and DONE. CAMERA OFF retains the older virtual-card/physical-pawn mode. ALIGN BOARD remains a manual corner-placement option.
- **Manual:** draw and move every real card/piece, including AI turns. Choose the card directly or use READ CARD with the configured Meta connection. Confirm the card, move the piece to the outlined space, then select DONE or pinch the destination.
- **Two humans:** physical board and manual handling, with player one wearing XR and player two using the same real board. No headset passing.
- **Confirmation:** adjustable 3â€“30 seconds in setup. Timeout clears the unconfirmed proposal and requests exactly one rescan. It never commits a card or moves a piece. After that, it waits for the player.
- **Voice:** opt-in VOICE ON. Narration and character responses require the private Meta host described below. Captions show connection failures instead of pretending the service is connected.

## Rule source and assets

The governing source is the supplied `candy-land-1984-reverse-side.pdf`, archived under `archives/living-table/references/candy-land/candyland-original/`. Its color-release penalties and final-purple finish are used, rather than later editions' rules or the BGG house variant.

The 134-space path is manually traced from `m81702829238_1.avif`. All six picture destinations, both shortcuts and three penalty spaces are included. Single-card counts are red/purple/yellow/blue 8 each and orange/green 7 each; doubles 2 per color; six picture cards, total 64. The inventory was cross-checked against https://www.donsgamecloset.com/candyland1984.html.

Reference artwork retains its original aspect ratio. Nine stylized volumetric character prefabs replace the geometric host placeholders. Six original picture-card types display corresponding character artwork; their rules remain unchanged. Eight new illustrations were generated with the built-in image tool. Queen Frostine generation was rejected twice, so its original supplied card image is used unchanged pending replacement.

## Meta connection

`Services/MetaHost/server.mjs` is a private Node 22+ relay using built-in libraries. It supports Meta Model API conversation, Muse Voice Transcribe, Meta vision card proposals, and Wit.ai speech output. Provider keys stay on the server.

Set server environment variables `MODEL_API_KEY`, `WIT_SERVER_TOKEN`, and a separate strong random `LIVING_TABLE_DEVICE_TOKEN`. Optional: `META_MODEL` (defaults to `muse-spark-1.3`), `WIT_VOICE` (defaults to `Charlie`), `PORT` (8787), `HOST` (127.0.0.1). Run `node Services/MetaHost/server.mjs`. The server is not started automatically. No provider calls were made during development tests.

Use HTTPS when connecting a headset. Place a `candy-meta-connection.json` file, based on the example in Services/MetaHost, in the app's `Application.persistentDataPath`. It contains only the relay URL and revocable device token. For Editor testing, set `LIVING_TABLE_HOST_URL` and `LIVING_TABLE_DEVICE_TOKEN` before launching Unity; loopback HTTP is allowed by the connection validator for local development. Never put provider keys into Assets, a scene, or an APK. The relay is a development service; production account/session management is not implemented.

Voice captures short microphone utterances and sends them to Meta for transcription, then sends text and bounded recent conversation to Meta for a reply. Wit.ai speaks the reply. The local relay does not log or save audio, images or conversation; provider data handling is governed by the configured Meta account. The legacy optional READ CARD action sends a central crop when explicitly requested; automatic physical mode never uses that relay and keeps frames on-device. A model confidence score is an estimate, not a calibrated accuracy guarantee.

Official interfaces checked: https://dev.meta.ai/docs/overview, https://dev.meta.ai/docs/speech-to-text, Meta's `wit-ai/wit-unity` source, and the installed Meta MR Utility Kit 207.0.1 `PassthroughCameraAccess` API.

## Validation and remaining work

Run `LivingTable.Editor.CandyBuilder.PrepareAndTest` in Unity batch mode for rules and real Play Mode tests. It creates Candy Land copies only if absent and leaves Astra's originals alone. Run `LivingTable.Editor.CandyBuilder.BuildQuest` to produce `Builds/LivingTable-CandyLand-Quest.apk`. Run `node --test Services/MetaHost/server.test.mjs` for the relay tests; these simulate upstream responses.

The rules tests exercise 250 complete seeded games, all traps and shortcuts, picture travel, deck reshuffling, finish conditions, invalid recognition and wrong-position refusal. Scene tests exercise manual confirmation, one rescan with no turn advance, four players and animated virtual movement. Reports and a preview are in Logs.

This is a prototype, not a release-ready fulfillment of the whole platform. Live Meta conversation requires account setup and hardware validation. The new automatic recognition path needs no account or user training, but its real-board accuracy and headset processing speed remain subject to physical on-device validation. Manual confirmation remains the fallback. Board alignment must be checked each session. Hands-free voice uses basic voice activity detection; acoustic echo cancellation and reliably distinguishing the XR wearer from nearby people are not verified. Fine character handoff and interruption timing need headset testing. Production character animation/rigging, custom/avatar generation, additive themed environments, persistent games, NFT purchase/export and Epic/UEFN integration remain future work. Nothing here promises universal asset portability.

## Shared interaction presentation

`Runtime/TableCardPresentation.cs` is the platform card reveal/discard presenter. Candy Land supplies a card face for every committed human or AI draw. The card is upright at one-quarter of the board's long dimension above the board, at twice its resting size. It holds for 0.85 seconds and flies/tilts to the right for 1.15 seconds, totaling two seconds of unpaused presentation time. Each card increases the visible stack by 0.7 mm; new games clear it. Recognized-but-unconfirmed cards do not enter the stack. Future game adapters should supply their own artwork to this same presenter; they are not automatically integrated yet.

`Runtime/TableButtonVisual.cs` provides chrome/gloss button faces, selected green states, hand-proximity or mouse hover brightening, and a 160 ms darkened 4 mm depression before dispatching the action. Hand hover uses the same tracked pinch midpoint and tabletop bounds as activation; losing hand tracking lets highlights expire. These controls preserve Astra's placement flow.


## Characters and accessible draw control

Character images, baked 3D meshes, materials and reusable prefabs live under `Resources/Characters/`. `CandyCharacterModel` defines the toy-style sculptures; `Editor/CandyCharacterAssets` bakes their surfaces by material into reusable prefabs. Models pop in on board placement and the matching figure reacts to a picture draw. These are volumetric meshes, not image billboards; full skeletal talking/gesture rigs are not implemented. Art prompts and method are archived in `archives/living-table/references/candy-land/character-art-prompts.json`.

The human virtual turn uses a centered 15 cm green circular DRAW CARD button with flowing shader sparkles and an 18 cm circular hit area. Pinch still works. Direct index-finger press arms above 4.5 cm and fires within 1.8 cm of the board plane; a lift is required to re-arm. Tracking loss resets that hand's arming state. Names are Silly Sally, Wobbly Walter and Giggles Greta.

Discards use bounded deterministic rotations (up to nine degrees) and offsets (up to 4 mm per axis). Some cards remain square. Presentation variation never consumes the game shuffle RNG. Lower-card artwork remains visible where the stack exposes it. The two-second reveal-to-stack duration is unchanged.

## Music, turns, viewer-facing panel and Plumpy sculpture

The original 48-second `Audio/candy-garden-waltz.wav` is composed from synthesized music-box and plucked tones by `Tools/compose_candy_waltz.py`, with circular echo tails for looping. It plays quietly, pauses with the game, ducks while the connected narrator speaks, and can be switched with MUSIC ON/OFF.

Each new turn displays the next player's name above the board at the card-reveal position for up to 1.8 seconds. The announcement clears when card presentation begins. The status/positions/host information is on a viewer-facing panel. FLOAT PANEL follows the headset smoothly; PIN PANEL holds its current location; MOVE PANEL waits for a pinch at a new location. Pinning is session-local and does not create a separate persistent spatial anchor.

Plumpy now uses a continuous sculpted body, ears, layered fur, eyes, fingers, toes, a woven basket and individual plums. The editable Blender source and portable GLB are in `Art/Plumpy`; the Unity FBX is in `Models/Plumpy`, replacing the old Plumpy prefab's geometry. It uses the existing pop-up animation, not a skeletal speech/gesture rig. Regenerate with Blender `--background --factory-startup --python Tools/build_plumpy.py` to avoid unrelated user add-ons.

## Control and depth fixes

Setup controls are purple, with a single oversized circular PLAY NOW action that starts the selected mode. Gameplay controls form one horizontal row with distinct colors, reserving the central 15 cm DRAW CARD location. Smaller controls are 7.5 cm with 8.1 cm hit areas. All bottom controls accept direct finger presses as well as pinches. Manual card choices use horizontal pages (single colors, double colors, picture cards) to avoid overlapping hit areas.

Buttons now have solid housings and a mesh silver bevel with a moving cap. The virtual board has a 12 mm body, with its artwork above the placement surface. Physical overlay hides the virtual body. Text uses a depth-tested font shader, so card surfaces occlude lower-card labels.

Music uses explicit Stop/Play with a saved sample position, rather than relying on UnPause after a lost playback state. Normal source volume is 0.11 (half the previous 0.22); narration duck volume is 0.0275. MOVE BOARD is available during play and pauses/hides the current content without disabling the gameplay object or stopping its coroutines. Re-placement restores the previous pause state and turn.

### Character play and movement magic (October 27)
Mr. Mint and Lord Licorice now use complete modeled FBX sculptures, authored by `Tools/build_mint_licorice.py`; editable Blender and portable GLB copies are in `Art/MrMint` and `Art/LordLicorice`. Their imported prefab geometry replaces the prior primitive figures.

All nine board characters accept a fingertip nudge or pinch. Contact is latched per hand until that hand leaves; cooldowns prevent repeated reactions. Each character has a separate playful motion (bounce, jig, wobble, bow, pirouette, curtsy, royal bow, cape flourish, or gooey wobble). Reactions return to their original position, pause during board relocation, and never change game state.

Confirmed draws create 28 rising destination sparkles. The six original picture cards also send 12 themed charms from the drawing player's piece to the legal destination: plums, peppermint sweets, gumdrops, peanuts, lollipops, or ice crystals. Arrival cues the character's celebration. Blocked moves create no misleading trail. Effects pause with gameplay and clear on reset. Lord Licorice remains an interactive board character, not an invented card in the original deck.

### Stretch, neon landings, and molasses
Pinch a board character and pull to stretch it while its feet stay anchored. Release eases it back to its original shape. Each character permits one grab at a time; tracking loss, pause, board relocation, recalibration, and reset release grabs. Stretch is capped to preserve a comfortable, readable tabletop character.

Destination twinkles use a procedural four-point additive glow shader and the actual square color, independently of the character trail theme. An inset, path-aligned neon rectangle illuminates the destination interior until the move is confirmed, then fades. No image or post-processing bloom dependency is required.

Landing on the red Molasses Swamp square coats only the affected piece in glossy brown sludge with drips and a puddle. It remains through blocked turns and clears on legal departure. In manual/physical mode the coating is independently visible at the confirmed piece location; this does not add continuous physical-piece tracking.

Lord Licorice now has a swept hat with a red rim and feather, a long pointed nose, a narrow face, a curling moustache, striped clothing, a candy belt buckle, and a lined cape. Regenerate only him with Blender's background script `Tools/build_mint_licorice.py -- --licorice-only`; omit the option to regenerate both Mr. Mint and Lord Licorice.

### Playful character interaction update
Gameplay switches from the menu waltz to an original 60-second soft shuffle with rounded bass/chords and a deliberately rolled-off high end. `Tools/compose_candy_soft.py` also creates the original soft bat-wing flutter sound.

Each of the nine characters has two bundled spoken jokes, built from installed Windows English speech voices with character-specific voice, pitch, and rate settings. `Tools/bake_character_voices.ps1` and `Tools/candy-character-voice-profiles.json` preserve the source. These are offline prerecorded reactions, not live Meta conversation. The existing Meta relay remains the live-conversation connection. The VOICE button now controls the bundled reactions too; background music ducks under speech.

A pinch can lift any visible virtual piece. Setup allows free placement on the board. During a game, release, tracking loss, pause, relocation, or the next card returns decorative pickups to the authoritative rules position. Grabbing never changes turns or card results. Real physical pieces remain manually moved.

Character pulls stretch only across the board plane, with a damped spring return. Plumpy drops four purple berries that fall, roll, and disappear in tiny sparkle poofs; Lord Licorice releases four scalloped-wing bats with individual flap loops; Mr. Mint sprouts three modeled striped candy canes. Effects are short, bounded, pause with the table, and clean up on reset. Character voice lines do not overlap each other.

Plumpy now follows the in-game card's golden face, rosy cheeks, rounded belly, and softer green fur. Mr. Mint now has a drooping curved peppermint hat, swept red forelock, laughing face, jacket folds, and striped candy belt. Both editable sculptures remain in Art/ and are regenerated by their Tools/ Blender scripts.

October 27 celebration update: normal moves visit every intervening square with a visible arc. Short moves share a 1.2-second movement budget; long moves retain a minimum 45 ms per square. Bridge destinations remain a separate final hop. Player visuals use conservative footprint spacing against neighboring and shared squares, including while grabbed; rules positions are unchanged. Starts use a compact two-by-two arrangement. Character pulling now stretches only height (up to 3x), with anchored feet and damped spring return.

Picture-card reveal launches 54 firework stars, 20 character-specific treats, an excited illustrated cutout, and a bouncing version of that pose on the card. The brief celebration temporarily hides the status/turn panels, preserves the turn announcement, pauses with the game, and clears on reset. The original six special cards remain unchanged. Lord Licorice's interaction uses wrapped trick-or-treat candy alongside his existing bats. New artwork is in Resources/Celebrations; generation details are in Art/celebration-generation.json.

Shared treats and cast refinement: celebrations now anchor to the selected board character's local position. Setup offers a YOUR COLOR button, rotating red/blue/green/yellow and assigning the remaining colors uniquely to opponents. The right-side open bucket receives five treats for each distinct special-card character, from any player's draw. Inventory counts include held and thrown items, so repeated draws never exceed five per character. Pinch to lift, release with hand motion to fling; toys bounce, then return after eight seconds or falling away. Tracking loss cancels pickup safely; pause freezes toys; new game empties the bucket. None of these toys alter game positions or the deck.

All nine cast prefabs now use detailed exported Blender geometry. The six formerly primitive-only characters have new full sculptures; Plumpy, Mr. Mint and Lord Licorice receive further shape and trim refinements. These are interpretation-based 3D models matched against the supplied 2D art, not certified exact 1:1 reconstructions. The editable sources and GLBs remain under Art/<Character>.

## Bowl, flick and shared celebrations — 2026-10-27
- Every character interaction presents its character-origin celebration, portrait and themed burst. Added King Kandy and Gloppy celebration portraits.
- Gumdrop Pass blooms before virtual crossing; manual play confirms the entrance, watches the bloom, then confirms the exit.
- Pinch-release velocity makes character reactions stronger and faster while returning to their board anchors.
- Bowl height halved; treats enlarged from 13 mm to 39 mm with larger grab targets. Each special card adds one treat, capped at five per character.
- Pinch the bowl rim/handles to move it; cyan rim shows selection. Stowed treats follow, held/thrown treats remain independent. Release sets it onto the board plane. Treats remain held while hand tracking updates.

## On-device physical recognition — 2026-10-27

PHYSICAL + AI now defaults to CAMERA ON. After the existing table placement, grant Quest headset-camera permission and look at the entire 1984 board with hands clear briefly. Built-in artwork landmarks align the overlay and automatically capture the board appearance for foreground tracking. No card or piece training is required from users. Select the color matching your physical pawn, then PLAY NOW.

Use one real deck for everyone: lay one face-up card beside the board for each player's turn. Keep other cards out of the camera view and remove the previous card before showing the next (including identical consecutive cards). Picture cards use shipped reference descriptors; color cards require solid color blocks, pale borders, and a plausible card outline. Recognition is local and raw camera frames are neither sent to the Meta service nor persisted. Frames are processed asynchronously, with at most one readback/worker task in flight.

Move your physical pawn to the highlighted space, then lift your hand away. Repeated stable observations of the matching colored pawn on the legal square confirm the move. AI opponents use the same physical deck and move their virtual pawns themselves. Gumdrop Pass confirms the entrance before its bloom and the exit after crossing. MANUAL reveals existing card selection and DONE controls; AUTO resumes camera observation. CAMERA OFF and MANUAL PHYSICAL remain setup fallbacks.

Validation: all 12 color cards; six picture references at altered size/brightness and perspective; board matching and rejection as cards; blank/ambiguous/wrong-color/unchanged-board piece rejection; temporal continuity; stale game/turn and duplicate guards; hand occlusion; manual fallback; existing rules and scene checks. The image tests use supplied reference photos and synthetic geometric variations, not a representative held-out Quest camera dataset. Real deck/board accuracy, lighting tolerance, and headset processing time still require physical on-device validation. Uncertain/occluded/partially visible inputs wait rather than advance. Physical relocation preserves the captured board appearance; start a new setup if the printed board itself changes.

Implementation: CandyPhysicalVision (camera/lifecycle/turn integration), CandyVisionFeatures (local binary descriptors and homography), CandyVisionDetector (edition card/foreground detection), and CandyVisionTests (reference bake and regression checks). Rebuild the shared reference file with CandyVisionTests.Bake when edition assets change. No cloud credential or neural-model download is required.

## Chess game option

CHESS on the setup panel opens the new chess MVP in the same app. It offers a virtual board against local AI or a second player, plus a physical full-set mode with local move-change detection and manual fallback. See ../Chess/README.md for setup, validation, and physical-camera limits.

## Persistent sessions (0.3.0 test build)

See ../SESSIONS.md for saved games, human/AI handoff, physical/virtual continuation, and the explicit limits of physical verification. These controls supersede the earlier restart-on-relocation and no-saved-matches behavior.
