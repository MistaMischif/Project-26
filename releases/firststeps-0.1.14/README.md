# First Steps 0.1.14 — recognition reliability

Scope: recognition first. No independent hosting migration, model replacement, alignment overhaul or new game features.

## Implemented

- Preserve projected piece proportions with letterboxed crops instead of stretching every crop to a square. Reject clipped, behind-camera or under-24-pixel crops rather than enlarging inadequate input and guessing.
- Full position capture now waits for fresh matching camera frames before presenting the review. Move/reconciliation approval requires at least three continuous fresh frames over 0.65 seconds. Hands, changed appearance, stale revisions and interrupted sessions invalidate the observation.
- Host 0.4.3 requires occupancy agreement between visibility and piece classification. A visibility-stage false empty no longer bypasses classification. This is two prompts on the same image, not independent statistical confidence or temporal piece consensus.
- Recognition readiness checks its own model/runtime, independently of the Meta conversational model. Readiness checks model availability, not a successful inference; the model smoke evaluation supplies separate inference evidence.

## Actual pipeline and limits

Checker/surface fitting establishes alignment on Quest. Color histograms detect candidate changes on Quest. Qwen on the existing host identifies cropped pieces. Deterministic legal moves and session revision checks decide whether an observation is compatible with the authoritative saved game; fresh appearance verification precedes commit. Meta text assistance does not recognize these pieces or authorize moves.

Alignment, piece identity, appearance-change detection and state reconciliation are separate capabilities. Letterboxing does not remove neighboring-piece overlap or viewpoint occlusion. Lighting/head movement can trigger a conservative retry. No numerical model confidence is claimed. Recognition still needs the existing computer host; removing that dependency is priority 2, not completed here.

## Smallest honest demo scope

One tested board and piece set, even lighting, board and piece tops in view, no moving hands during a scan. Review/correct every initially captured piece and explicitly choose unresolved turn/history. Manual corrections are assisted, not automatic recognition. Use the same authoritative session across physical and virtual modes. Never commit uncertain or illegal observations.

## Headset acceptance — pending founder test

1. Scan a real board containing both colors and all six piece types. Check every proposed square. Record unknowns, false identities and total elapsed time.
2. Move a hand over the board during scanning; confirm the saved position does not change and retry/review is available.
3. Move a piece or change the view while inference is running; confirm old evidence does not silently commit.
4. Play a legal human move; verify virtual result. Move the AI piece physically and check that verification releases the next turn exactly once.
5. Deliberately place a piece on a wrong square. Confirm the mismatch is highlighted and the session does not advance.
6. Play virtually, save/reopen, then restore the physical board and verify reconciliation.
7. Repeat the full loop three times. Pass: zero false state commits, correct accepted layouts, recoverable errors, recorded latency. These are team acceptance gates, not contest rules.

Rollback: app 0.1.13 and host 0.4.2 release remain available. Keep saves and connection files; do not uninstall or clear application data. An app downgrade may require platform-specific installation handling.
