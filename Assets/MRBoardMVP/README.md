# MR Board MVP

Code-first Unity tabletop prototype. No paid assets or third-party runtime packages.

## Quick start
1. Use a Unity 2022.3 LTS or Unity 6 3D project (Built-in or URP).
2. Copy the archive's Assets/MRBoardMVP folder into your project.
3. Wait for compilation, then choose MR Board MVP > Create Demo Scene.
4. Press Play. Click a piece, then an empty tile. R resets the game.

The scene builder creates and saves Demo.unity; the board, eight pieces, selection highlights, jump animations, and landing pulses are generated at runtime. One Unity unit is one meter. The sample board measures 36 by 36 cm. No imported art is required.

## Register a game
Duplicate Resources/demo-game.json, change its ID and pieces, then assign it to the platform's Initial Game field. Runtime registration:

```csharp
var definition = platform.RegisterGame(json);
platform.LoadGame(definition.id);
```

Piece IDs are unique within a game. Coordinates start at zero. Supported shapes: Cylinder, Cube, Sphere. Colors use HTML hex notation. Cell size is in meters. Registration validates board bounds, duplicate IDs and occupied starting cells. Definitions should be treated as immutable after registration. Registry is in-memory; JSON assets are the persistent game definitions. This is not an account or backend registration service.

## Headset integration
This scaffold does NOT install or configure a headset SDK, passthrough, spatial anchors, or controller bindings. The desktop demo works independently of them.

1. Configure the target headset's supported XR provider, OpenXR/XR Interaction Toolkit as appropriate, and vendor passthrough support for MR. Set up an XR Origin and tracked camera, replacing the demo camera and audio listener.
2. Assign the tracked camera to the platform and disable Allow Desktop Input. The desktop OnGUI panel is not a headset UI.
3. Bind controller select or hand pinch to platform.SelectFromPose(pointerTransform). Its local +Z direction must follow the selection ray. Alternatively call SelectFromRay(origin, direction). The adapter is intentionally SDK-independent; you supply the input binding and optional visual ray.
4. Align the board to the physical board center using SetBoardPose(position, rotation). Local X/Z span the board and local Y is up. Manual calibration can supply this pose; a marker or spatial-anchor integration can supply it later. Pose updates are ignored during move animations; retry on the next update.
5. Match the physical grid dimensions and cellSize in the game JSON. Keep the platform and board hierarchy at unit scale. The board plane is local Y=0.

For VR, the generated board and pieces are visible in the virtual scene. For MR, vendor passthrough is required to see the real board; virtual tiles are opaque in this prototype. Transparent overlays/occlusion are not implemented. This demo does not detect a real board or track real pieces automatically. Players must confirm moves through input or ConfirmPhysicalMove(pieceId, column, row); this moves the digital counterpart only. A real piece may hide the virtual selection target, so a production MR UI needs an explicit piece picker or calibrated tracking adapter.

## Extension points
- board.MoveValidator = (id, from, to) => ...; install game-specific move rules.
- platform.PieceMoved += (id, from, to) => ...; subscribe to completed moves for sound, scoring, or narrative effects.
- platform.ConfirmPhysicalMove("azure-1", 2, 3); confirms a physical move through the same validation path.
- Register additional JSON definitions and LoadGame(id) to switch games.

This is a local hot-seat sandbox: it permits movement to any empty in-bounds cell, with one move animation at a time. It has no turns, capture rules, wins, undo, multiplayer, shared anchors, save-state synchronization, automatic recognition, or production backend. Loading/resetting rebuilds the board from its original definition. Input raycasts use default physics layers; isolate layers in a larger project.

## Validation checklist
- Load the generated scene and verify eight pieces.
- Select azure-1 and move it to an empty cell; confirm jump and landing pulse.
- Try an occupied tile or invalid ConfirmPhysicalMove coordinate; move is rejected.
- Reset during a move; the original layout is restored.
- Add a duplicate piece ID to JSON; startup reports a validation error.
- Build for the target headset after installing/configuring its XR provider; verify tracking, controller binding, scale, board alignment, and passthrough on device.

Generated source has not been compiled or hardware-tested in Unity in this environment. No Unity package manifest is overwritten, and no XR compatibility certification is implied.
