## Dashboard 1.6.0 - November 2, 2026

Physical alignment guides, authenticated Meta readiness and retry. USB tunnel loss diagnosed; onboard chess AI tested independently. Existing observations retained; founder acceptance remains open.

## Current dashboard 1.5.0 — November 2, 2026

First Steps 0.1.5 adds room recovery, corrected OpenXR pinch joints and a local Meta text companion. Actual local model and Unity tests are recorded; headset acceptance remains open. The immediate sprint is Cerebral Valley Nov 4. The preserved 29-check VR Start plan is explicitly separate and retains its existing observation storage. No user passes are seeded.

# Living Table progress dashboard

## Game-over and replay iteration - November 1, 2026

Dashboard 1.4.0 tracks First Steps 0.1.4. Checkmate/draw results use bold 4x text, with a prominent PLAY AGAIN control and contextual game-over guidance. MENU replaces HANDOFF. Replay preserves board mode/opponent/color, starts a fresh session and retains the completed record; repeated presses cannot reset active play. U03 includes the headset acceptance steps. Recognition evidence remains the 0.1.3 record; no new accuracy claims or founder passes are added.

## Recognition iteration â€” October 31, 2026

Dashboard 1.3.0 tracks First Steps 0.1.3 and host 0.2.0. Visibility-before-identity with installed local Qwen improves synthetic results to 10/10 cases (six unique images) plus 18/18 related variants. Warm pairs took about 0.35â€“1.53 seconds; 64 repeated empty/pawn crops took 30.7 seconds. Real-set accuracy across all piece types, camera framing, cold startup and total headset handoff time remain open. H03/H05/H09 now prioritize those checks. No headset passes are seeded, and existing stored observations remain untouched. Qwen is not presented as Meta/Llama hackathon evidence.

## Chess handoff iteration â€” October 31, 2026

Dashboard 1.2.0 follows First Steps 0.1.2: full saved-position reconciliation is implemented and tested with synthetic observations; physical-camera acceptance remains open. H04/H07/H08 distinguish passing scene regressions from real-headset handoff evidence. H09/H10 cover full-board scans, wrong layouts, interruptions and explicit manual fallback. Build fingerprint and installation status are finalized from artifact evidence. Existing browser observations remain untouched. The contest package uses chess-only navigation and audited Candy Land resource exclusion; the general app retains Candy Land.

Next priority is real-board recognition accuracy and latency, followed by convenient judge access and unfamiliar-room validation. Do not mark these complete from code or fixture tests. No paid provider or new studio infrastructure was introduced.

## Latest build refresh â€” October 31, 2026

Dashboard 1.1.0 reconciles local build logs, release manifests and the GitHub release list. First Steps 0.1.1 built successfully October 29 (SHA-256 in latest-build.json), with 23 scene/geometry checks, 3 offset checks and 20 observation-gate cases plus chess regressions. No 0.1.1 installation, GitHub APK publication or headset acceptance is established. Published First Steps remains 0.1.0; general Living Table remains 0.6.3. This task updates the tracker, not the headset installation.

New browser sessions default to the latest local First Steps fingerprint. Existing selected candidates and observations remain untouched. H01/H02/H03/H05 and U02/U03/U04 now document current implementation and remaining checks. The generator reads latest-build.json so regeneration retains the update. Synthetic model errors and latency remain explicit. Historical October 25 tracker is unchanged.

Studio alignment: directly supports Living Table testing and the Brain capability's truthful evidence reporting. No backend, access policy, contest registration or product-scope change. Prior contest-rule text/dates remain the October 29 snapshot, not a fresh rules review. No evidence implies founder acceptance. Static/model interaction checks passed; visual browser QA was not performed. Local source remains self-hostable with no paid dependency.

## Previous chess contest rundown â€” October 29, 2026

The homepage now contains 29 detailed checks across physical handoff (12), usability/performance (8), and submission (9). Each check includes three execution steps, evidence requirements, an explicit pass condition, proposed date, owner and dependencies. Dates are planning targets, not verified availability.

Results use the separate `living-table-chess-rundown-v1` browser-local key. Build checks are isolated by full APK SHA-256; administrative checks persist, but dependent passes reopen when prerequisites fail. JSON export/import preserves observation history. No headset test is seeded as passed. The printable Markdown checklist is available from the dashboard's download button.

`dist/legacy.html` retains the original October 25 tracker and storage. Its sandbox-specific tests are historical. The old refresh scripts below update only that historical evidence, not the chess rundown. Run `python make-contest-plan.py` from this directory or use the checked-in `dist/contest-plan.js`; the generator expects the parent project's release manifests. Run `node check-contest.mjs`, `node check-tracking.mjs`, and `node --check dist/contest-app.js` for validation.

Self-host with `python -m http.server 8080 --directory dist`. No paid API, remote database, or proprietary runtime is required. The established private Site serves the same static files. Records are per browser and origin: export/import when moving hosts.

## Historical tracker documentation

Open dist/index.html directly for the local version, or use the private deployed Site. Build facts are a snapshot. Headset observations are stored only in the current browser and are tied to the exact APK SHA-256. Export/import the JSON test log to back it up or transfer it. Imported observations for older APKs remain in history but cannot contribute to current progress.

Refresh project evidence with `python LivingTableDashboard/refresh-evidence.py` from the Unity project root, then publish the updated source through the Sites workflow. A new APK invalidates the prior signature/architecture and installation confirmations. Those fields must be reverified before updating the confirmed hash in the evidence script. Merely refreshing the browser does not update build facts.

Initial evidence: Unity build and smoke logs, local APK size/hash, signature and architecture checks, and the successful Quest 3 ADB installation recorded in this chat. Headset behavior is untested until observations are entered. This dashboard does not automatically monitor Unity, ADB, or Muse.

Original dashboard review: sign-in succeeded, but Muse's artifact viewer reported "Couldn't load this artifact" after retry. Original content has not been reviewed or reconciled. Its link is retained in the dashboard.
