# Contest Command Dashboard

October 29 update: the Living Table lane and top-level banner now link to the canonical 29-check chess contest rundown on the existing Living Table Site. Its baseline reflects the chess 0.4.0 profile and installed general build 0.6.3. The October 25 portfolio schedule, estimates, other contest lanes and user records are preserved as historical context, not current Living Table acceptance evidence. Re-running the original build-data.py would restore the old snapshot; apply the current Living Table update afterward or retain the deployed data.json.

Self-host with `python -m http.server 8080 --directory dist`. Browser-local records can be exported/imported between hosts. No paid service is required.

Reviewed October 25, 2026. Private portfolio dashboard derived from the user-supplied `reference-original.html`, original Muse Living Table plan, Project 26 build/source evidence and official contest pages linked in the app.

## Scope

Four contest entries, an evidence comparison, 115 unique execution tasks including judging milestones, and an adjustable capacity view. The full-scope proposal estimates 178.5 hours through Nov 18. This is demand, not confirmed availability or guaranteed completion. All new recovery records begin empty. Historical source reports are labeled, not migrated into completed tasks.

## Tracking

Manual browser-local state under `contest-command-evidence-v1`. No external account or submission actions, background updates or cross-device synchronization. JSON export/import transfers records. Done requires a note and completed dependencies. Reopening a dependency makes downstream completion ineffective. Deferred and blocked remain incomplete. Counts are unique task counts, never project-readiness percentages.

The Living Table headset tracker is separate and linked; its state is not automatically imported. Its earlier claim that Muse was unavailable is superseded by this review.

## Validation

Run `node check.mjs` and `node --check dist/app.js`. Preview the `dist` directory using a static server. `build-data.py` rebuilds data from the neighboring LivingTableDashboard recovery draft and the original uploaded reference path; keep data.json as the deployable source if those local inputs move.

## Sources and limits

Official pages checked Oct 25. Meta waiver unreadable; Unity full-rules link pending and start/voting text inconsistent; MegaJam Box rules unavailable. Private application receipts, remote Muse prototypes, disclosure/filing clearance, written organizer rulings and future family schedule were not verified. No claim of legal eligibility or prize qualification is made.
