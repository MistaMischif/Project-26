# Contest Command 2.0.0

Strategy-first execution dashboard, updated October 29, 2026. Six lanes: Living Table First Steps (Meta AI), Living Table XR chess (VR Start), Frenemies (Codex x TwitchCon plus product follow-through), MAYHEM PARTY!, Unity Game Jam, and Epic AI MegaJam.

103 tasks include daily steps, suggested dependencies, completion criteria, estimated hours, scope cuts and judge-facing demonstrations. Meta recommendation: Indie developer / Build for Your World. This is a proposed track alignment, not a change to registration. Contest sources are linked within each project.

Progress is manual, stored locally under contest-command-rundowns-v2. Notes are optional; dependencies guide rather than block completion. Export/import JSON backups to move progress. Prior dashboard remains at legacy.html with its existing storage untouched. Living Table tracker imports are explicit.

Self-host: python -m http.server 8080 --directory dist

Checks: node check-rundowns.mjs; node --check dist/rundowns-app.js; node check.mjs (legacy).

RUNBOOK.md is the complete offline plan. No paid service is needed to host this static dashboard. Dates and hours are proposed scheduling, not confirmed personal availability. Unknown private entry details remain clearly separated from recommendations.

October 29 implementation update (2.1.0): published task progress now merges with newer local records. First Steps scope is locked, local real inference has run, and headset/recognition acceptance remains open. The exact synthetic evaluation failures and latencies are displayed in the First Steps lane; no inferred headset completion is marked done.
