# First Steps 0.1.18 — Setup, comfortable pickup and physical feedback

## Implemented
- New and continued games open with a board in front of the player, fixed back panel, and Continue/Menu controls. Continue requires four playable corners; Align locks the grid; Start Game enters play/capture as appropriate. Camera-refined and manual corners remain visually distinguished.
- Square markers with green triangular corner indicators; burl-oak adjustment knobs with gold move/turn/size indicators. Physical piece opacity is adjustable in Menu.
- Finger gap supports pickup up to 65 mm (over one inch) and release beyond 90 mm. Grip remains at the fingertip midpoint. The square under a hovered piece glows softly; legal virtual pieces can be picked up throughout that interaction volume.
- Confirmed physical moves receive a subtle purple destination glow fading over 2.4 seconds. Uncertain observations never receive this success indication. Local learned appearance can preview a legal move while backend identity verification protects saved history.
- Per-piece persistent IDs and view-dependent appearance descriptors are saved after confirmed layout/verification. Nearest color is inferred from known layouts; a standard new game starts White. Saved turns remain intact; arbitrary captured positions may still require turn selection.
- Polished wood capture trays have individual recesses and placement prompts. Physical mode omits opaque duplicate captured models; virtual mode displays captured models at configured scale.

## Acceptance checklist on Quest
1. New Game and Continue: see only Continue/Menu initially. Move board using knobs, Continue, place all four playable corners, Align, Start Game. Check snapped versus manual indicator accuracy and stable overlay.
2. Hover each side's pieces: a soft square glow appears. On your virtual turn, grab with at least a one-inch gap; move while maintaining grip; open past 90 mm to release. Opponent pieces must not be movable on your turn.
3. Move a physical piece legally, clear hands and look at the board. Check counterpart movement, then a subtle purple destination glow after verification. An illegal/uncertain placement must not advance the turn or receive purple success feedback.
4. Move the physical AI piece as prompted; verify automatic return to your turn. Save/continue and verify all positions and side-to-move persist.
5. Change opacity in Menu. Capture a piece and place it in the marked wooden recess: check real-piece visibility, recess sizing and counts. Test both colors and enlarged pieces.

## Verification limits
Automated checks are recorded alongside this release. Real headset comfort, capture visibility, inferred player orientation, camera accuracy and latency require MASTER OMEGA acceptance. Appearance templates are view-dependent image descriptors, not full 3D scans; indistinguishable pieces retain identity through validated move history. Hand occlusion can delay verification. The existing configured self-hosted vision service remains necessary for authoritative recognition; this is not a standalone offline recognition release. Native local chess AI remains available independently.

Rollback: firststeps-v0.1.17; same package ID, install -r preserves saves.
