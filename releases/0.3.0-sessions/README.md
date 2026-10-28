# Living Table 0.3.0 — persistent sessions (test release)

Quest 3 test build, October 28, 2026. This release extends the existing Astra-based app with saved chess and Candy Land sessions.

## Try it

- Chess: start a two-player game, make moves, choose AI TAKE OVER. HUMAN RETURNS gives the opponent seat back without resetting the position. A physical AI proposal must be carried out before a different move can proceed.
- Candy Land: SESSION opens the handoff and surface controls. The existing one-row gameplay toolbar remains intact.
- GO VIRTUAL / USE REAL BOARD preserves the session. Restore the saved physical layout before resuming real-board play. VIRTUAL ROOM turns off passthrough around a virtual board; SEE ROOM restores it.
- NEW GAME / SAVE EXIT saves the current game to the setup menu. RESUME SAVED reloads it; SAVE cycles the stored sessions.
- Chess IMPORT GAME provides a manual board editor for games begun before Living Table was watching. Set the turn, castling rights, and en-passant square explicitly. Imported history starts at that position and remains labeled unverified.

## Scope and limits

Local AI and session persistence need no paid API or subscription. Source and local JSON journals support operation on user-controlled hardware. This is a local shared-table MVP, not network multiplayer.

Legal state, pending actions, and history survive human/AI and physical/virtual transitions. Saved Candy Land cards and remaining inventory cannot be rerolled by switching modes. Physical card order remains unknowable until cards are shown; virtual continuation uses the saved shuffled remainder.

Physical appearance matching remains experimental and is not piece-identity authentication. Manual reconciliation and imported positions remain visibly recorded as unverified. The release does not promise cheat-proof camera enforcement, trusted remote arbitration, or knowledge of play while the app was absent. Full physical-board camera accuracy still needs headset validation. See Assets/LivingTable/SESSIONS.md for details.

The APK is stored beside these notes locally and published as a GitHub Release asset, rather than committed to Git. Validation and SHA-256 metadata accompany the artifact. Credentials and signing keys are excluded.
