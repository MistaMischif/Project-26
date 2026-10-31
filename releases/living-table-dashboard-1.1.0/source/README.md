# Living Table progress dashboard

## Latest build refresh — October 31, 2026

Dashboard 1.1.0 reconciles local build logs, release manifests and the GitHub release list. First Steps 0.1.1 built successfully October 29 (SHA-256 in latest-build.json), with 23 scene/geometry checks, 3 offset checks and 20 observation-gate cases plus chess regressions. No 0.1.1 installation, GitHub APK publication or headset acceptance is established. Published First Steps remains 0.1.0; general Living Table remains 0.6.3. This task updates the tracker, not the headset installation.

New browser sessions default to the latest local First Steps fingerprint. Existing selected candidates and observations remain untouched. H01/H02/H03/H05 and U02/U03/U04 now document current implementation and remaining checks. The generator reads latest-build.json so regeneration retains the update. Synthetic model errors and latency remain explicit. Historical October 25 tracker is unchanged.

Studio alignment: directly supports Living Table testing and the Brain capability's truthful evidence reporting. No backend, access policy, contest registration or product-scope change. Prior contest-rule text/dates remain the October 29 snapshot, not a fresh rules review. No evidence implies founder acceptance. Static/model interaction checks passed; visual browser QA was not performed. Local source remains self-hostable with no paid dependency.

## Previous chess contest rundown — October 29, 2026

The homepage now contains 29 detailed checks across physical handoff (12), usability/performance (8), and submission (9). Each check includes three execution steps, evidence requirements, an explicit pass condition, proposed date, owner and dependencies. Dates are planning targets, not verified availability.

Results use the separate `living-table-chess-rundown-v1` browser-local key. Build checks are isolated by full APK SHA-256; administrative checks persist, but dependent passes reopen when prerequisites fail. JSON export/import preserves observation history. No headset test is seeded as passed. The printable Markdown checklist is available from the dashboard's download button.

`dist/legacy.html` retains the original October 25 tracker and storage. Its sandbox-specific tests are historical. The old refresh scripts below update only that historical evidence, not the chess rundown. Run `python make-contest-plan.py` from this directory or use the checked-in `dist/contest-plan.js`; the generator expects the parent project's release manifests. Run `node check-contest.mjs`, `node check-tracking.mjs`, and `node --check dist/contest-app.js` for validation.

Self-host with `python -m http.server 8080 --directory dist`. No paid API, remote database, or proprietary runtime is required. The established private Site serves the same static files. Records are per browser and origin: export/import when moving hosts.

## Historical tracker documentation

Open dist/index.html directly for the local version, or use the private deployed Site. Build facts are a snapshot. Headset observations are stored only in the current browser and are tied to the exact APK SHA-256. Export/import the JSON test log to back it up or transfer it. Imported observations for older APKs remain in history but cannot contribute to current progress.

Refresh project evidence with `python LivingTableDashboard/refresh-evidence.py` from the Unity project root, then publish the updated source through the Sites workflow. A new APK invalidates the prior signature/architecture and installation confirmations. Those fields must be reverified before updating the confirmed hash in the evidence script. Merely refreshing the browser does not update build facts.

Initial evidence: Unity build and smoke logs, local APK size/hash, signature and architecture checks, and the successful Quest 3 ADB installation recorded in this chat. Headset behavior is untested until observations are entered. This dashboard does not automatically monitor Unity, ADB, or Muse.

Original dashboard review: sign-in succeeded, but Muse's artifact viewer reported "Couldn't load this artifact" after retry. Original content has not been reviewed or reconciled. Its link is retained in the dashboard.
