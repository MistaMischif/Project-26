# Contest Command — full execution plan

Proposed schedule, October 29, 2026. Progress notes are optional.


## Frenemies

A viewer joins in one command, changes a live battle, and returns as a persistent character in the streamer’s world.


Track: Codex × TwitchCon · interactive Twitch experience


Build: Ship the dependable free battle loop, compact OBS presentation and clear audience consequences first. Then grow the same engine into streamer-owned packs, authored missions and earned rewards.


Demo: Show a viewer join, one chat decision changing the fight, the streamer reacting, the result, and one-click next battle. Include a short Codex-building segment.


Cut: Before Nov 2: cut real-money rewards, marketplace breadth, new board games and unapproved Bits claims. Keep the creator-platform roadmap, but do not let it consume the contest finish.


### 2026-10-29 — F01 Close the local/hosted deployment gap

1. Compare local commit 2a166dd with Render deployed commit and /health.

2. Deploy the tested 1.6.0 source through existing hosting; preserve configured secrets/database.

3. Verify !join, compact overlay and recap on the public build; archive exact commit and response.

Done when: Hosted behavior matches the pinned source, not only a successful health response.


### 2026-10-29 — F02 Lock TwitchCon eligibility and honest contest scope

1. Review official rules and Devpost draft with the submitting account.

2. Pin the contest slice: free Twitch battles, creator controls, persistent identity, clear recap; list UGC/economy as later work unless actually delivered.

3. Verify source access, asset rights and Codex session ID; correct stale simulated/live claims.

Done when: Submission scope matches implemented behavior; account questions resolved.


### 2026-10-30 — F03 Prove current live chat and EventSub

1. Connect the broadcaster and two consenting test accounts; verify !join and all commands.

2. Observe one genuine eligible event; do not purchase monetized events just to manufacture evidence.

3. Repeat duplicate votes, pause, team-switch rejection and reconnect; keep simulated cases labeled.

Done when: Live command path works and unverified event types remain explicitly listed.


### 2026-10-30 — F04 Check OBS and host recovery

1. Load both overlay layouts in OBS at 1080p and 720p alongside normal content.

2. Run start/pause/resume/kill and a reconnect; verify kill makes overlay transparent.

3. Measure first battle setup time and inspect text/animation readability on a streaming PC.

Done when: Host can recover without disrupting the show; overlay is legible.


### 2026-10-30 — F05 Audit persistence and safe feature gates

1. Back up test data; restart the service and reconnect Twitch.

2. Verify season history and follow receipts survive; explicitly show which session state resets.

3. Keep real Bits unavailable unless Extension eligibility and a genuine transaction are established.

Done when: No false paid-feature claim or duplicate durable award.


### 2026-10-31 — F06 Run an uncoached creator rehearsal

1. Have a willing creator set up a test overlay using only instructions.

2. Observe one complete battle and one failure recovery without coaching.

3. Record confusing steps and fix only blockers/readability; retest the selected build.

Done when: Creator can start, explain and stop a battle unaided; unresolved friction documented.


### 2026-10-31 — F07 Capture the Twitch and Codex story

1. Record the accepted live battle and a clear viewer action changing its outcome.

2. Include a short authentic Codex development segment and a result/rematch.

3. Edit below three minutes; label simulation and avoid unproved retention/paid claims.

Done when: Video depicts the delivered Twitch experience and Codex work.


### 2026-11-01 — F08 Prepare and rehearse judge access

1. Test clean repository setup with README and sample environment values.

2. Check public game, source permissions and public YouTube playback as a new viewer.

3. Fill Devpost draft with session ID and accurate feature description; archive a candidate ZIP.

Done when: Reviewer can run/watch without private developer help.


### 2026-11-02 — F09 Submit Frenemies before the buffer expires

1. Review final links and build identity at noon Central.

2. Submit and verify Submitted status, not just a saved draft.

3. Save receipt and preserve judged version/access through the official judging window.

Done when: Actual receipt before Nov 2, 4 p.m. Central.


### 2026-11-05 — F10 Check judging access without changing the entry

1. Open the source/video/game from the intended reviewer context.

2. Confirm service wake/reconnect and no expired permission.

3. Record any operational issue; do not silently replace submitted feature behavior.

Done when: Judged entry remains reachable.


### 2026-11-15 — F11 Archive contest outcome

1. Check official results and actual notifications.

2. Store outcome and feedback without assuming finalist status.

3. Keep follow-up product changes separate from the submitted snapshot.

Done when: Contest record closed with real outcome or a pending decision note.


### 2026-11-19 — F12 Implement streamer-owned content packs

1. Define versioned pack IDs, branding, factions, characters, art slots and file limits.

2. Separate C2G into an example pack; render a second distinctly styled test pack.

3. Validate uploads, permissions and safe asset URLs; keep running sessions on their selected pack version.

Done when: A creator changes the look without changing engine rules or losing inventory IDs.


### 2026-11-20 — F13 Build bounded mission and story authoring

1. Define supported objectives, trigger types, choices and persistent consequences as validated data.

2. Add draft/preview/publish and one branching mission with two outcomes.

3. Reject executable payloads, unreachable objectives and unbounded rewards; version published content.

Done when: A creator authors a mission/story without arbitrary code execution.


### 2026-11-21 — F14 Add viewer submissions and creator approval

1. Allow compatible art/content drafts from an identified contributor.

2. Require creator/moderator approval, ownership attestation and rejection reasons.

3. Test abuse limits, rollback and changing an asset without erasing ownership.

Done when: Unapproved UGC never appears live; approved versions remain traceable.


### 2026-11-22 — F15 Deliver earned currency and a durable shop

1. Name/style currency per channel; define bounded awards for meaningful completed participation.

2. Use transaction IDs and atomic ledger entries for grants, purchases and equips; reject retries/double spends.

3. Offer previewable permanent cosmetics with free earning; test first-session unlock pacing and no cash-out promise.

Done when: Balances survive restart and cannot be inflated by duplicate events or trivial custom missions.


### 2026-11-23 — F16 Resolve optional monetization separately

1. Review current Twitch Extension policies and eligibility for the proposed fixed offerings.

2. Keep earned currency and any Bits entitlement separate; do not activate real transactions from simulations.

3. Record approval path, refund/failure behavior and a no-paid-feature fallback. Treat real-money payouts as a separate researched decision.

Done when: Free game remains complete; paid path is either verified or explicitly deferred.


### 2026-11-24 — F17 Add event and repeat-play instrumentation

1. Record accepted events, live minutes, participant IDs scoped to channel and encounter IDs.

2. Measure event frequency including zero-event streams, next-battle and next-stream participation; separate revenue.

3. Provide export/retention controls and explain that chat participants do not represent every viewer.

Done when: Metrics distinguish arrivals, participation, retention and revenue without causal overclaim.


### 2026-11-25 — F18 Run scale and recovery tests

1. Generate 100, 1000 then 10000 simulated participants and bounded event bursts.

2. Measure accepted-vote latency, CPU/memory, overlay frame time and event deduplication.

3. Publish measured supported limits; lower scope when the free/self-hosted deployment cannot support a target.

Done when: No unsupported large-stream guarantee; tested capacity and failure behavior are explicit.


### 2026-11-26 — F19 Launch the approved creator pilot

1. Confirm three to five willing creators and recording/data consent; outreach requires separate authorization.

2. Schedule matched sessions or randomized encounter activation where practical.

3. Give each creator a custom pack, mission and shop; agree success metrics before observing outcomes.

Done when: Pilot has defined participants, comparisons and a four-week measurement window.


### 2026-12-03 — F20 Pilot week 1: review repeat participation

1. Export the prior week’s events and live-minute denominators.

2. Review repeat participants, host interventions and item use with each creator.

3. Record fixes separately from experimental changes so comparisons remain interpretable.

Done when: A week of observations or a documented missing-data blocker; no fabricated growth.


### 2026-12-10 — F21 Pilot week 2: review repeat participation

1. Export the prior week’s events and live-minute denominators.

2. Review repeat participants, host interventions and item use with each creator.

3. Record fixes separately from experimental changes so comparisons remain interpretable.

Done when: A week of observations or a documented missing-data blocker; no fabricated growth.


### 2026-12-17 — F22 Pilot week 3: review repeat participation

1. Export the prior week’s events and live-minute denominators.

2. Review repeat participants, host interventions and item use with each creator.

3. Record fixes separately from experimental changes so comparisons remain interpretable.

Done when: A week of observations or a documented missing-data blocker; no fabricated growth.


### 2026-12-24 — F23 Close pilot and set the product release gate

1. Analyze four weeks of comparable sessions and zero-event periods.

2. Decide what to keep based on creator second-use, host burden and player item use; identify confounders.

3. Publish a bounded MVP release, support guide and next backlog, or record a concrete failed gate.

Done when: Pilot-backed release decision; no claim that a full creator marketplace or cash economy is complete.


## MAYHEM PARTY!

Friends on different phones enter instantly and play a varied, readable party pack without installing anything.


Track: Standalone product lane · no contest assignment assumed


Build: Make the existing 16 games consistently understandable, joining frictionless and reconnects predictable. Let CAP customization create identity without obstructing guest play.


Demo: Scan a QR, join with real phones, play contrasting skill/social rounds, reveal a shared winner and rematch.


Cut: Do not add more mini-games yet. Cut mandatory sign-in, cash mechanics and new infrastructure until the current party works reliably.


### 2026-10-29 — M01 Pin deployed MAYHEM source

1. Compare Render deployment commit with local 3db06a7 and release 1.0.10.

2. Fix/report build metadata so /health is not mistaken for release identity.

3. Verify glossy controls, Custom avatar slot and representative game in the public build.

Done when: Exact deployed source and intended test build are recorded.


### 2026-10-30 — M02 Rerun the complete multiplayer regression

1. Run npm test, node verify.mjs and node smoke-party.mjs in a clean checkout.

2. Run the 16-game rotation with eight clients, final standings and rematch.

3. Retain failing game IDs and seed/round data; fix before public acceptance.

Done when: All game and lifecycle checks pass on the pinned source.


### 2026-10-31 — M03 Play on real phones across networks

1. Use a real iPhone/Safari and Android/Chrome plus a desktop; join using QR and invitation.

2. Play touch, drawing, text and timing games over different networks.

3. Test orientation, keyboard, safe areas, audio and native share without treating viewport emulation as device evidence.

Done when: No blocked controls or hidden instructions; real-device evidence exists.


### 2026-11-01 — M04 Verify host loss, late join and reconnect

1. Disconnect the host and a player mid-round; reconnect, explicitly leave and attempt stale-session reuse.

2. Join late, fill eight seats, and reduce a three-player game to two connected participants.

3. Restart a test server; show the expected room-loss message and recovery rather than promise durable rooms.

Done when: Every case resolves without a stuck lobby or unfair duplicate score.


### 2026-11-02 — M05 Accept accounts and saved rewards or hide unavailable paths

1. Check configured Google OAuth and PostgreSQL without exposing secrets.

2. If enabled, sign in, win, claim once, buy/equip and verify after service restart with two accounts.

3. If unavailable, retain complete guest play and label/disable saving; do not count local SQL fixtures as production OAuth proof.

Done when: Public account claims match observed behavior; no duplicate grants or cross-account access.


### 2026-11-03 — M06 Run an uncoached party

1. Invite a consenting mixed group of 2–8 people; give only the URL and goal.

2. Play a complete mixed pack and rematch without explaining each game.

3. Record join time, misunderstood instructions, abandoned rounds and willingness to replay.

Done when: Group completes unaided; every blocker gets a reproduction.


### 2026-11-04 — M07 Fix acceptance blockers and freeze the MVP

1. Resolve reproducible mobile/input/lifecycle defects before extra cosmetic work.

2. Rerun affected games and the full rotation on the new commit.

3. Document current room limits, cold-start behavior and account availability; select final source tag.

Done when: No critical or blocked core-party route; limits are visible and tested.


### 2026-11-05 — M08 Publish the accepted release and recovery guide

1. Deploy to the established service and verify the exact version and public route.

2. Publish downloadable source/test release and guest/account setup instructions.

3. Export database backup where configured and test recovery in an isolated instance.

Done when: Reproducible self-hosted path and tested public release.


### 2026-11-07 — M09 Measure capacity before inviting a larger audience

1. Load-test increasing simultaneous rooms with normal server timing.

2. Measure memory, latency and cold start; retain single-process limits and no horizontal-scaling assumption.

3. Set a supported pilot size and a maintenance message; do not add paid infrastructure without direction.

Done when: Capacity claim is measured and bounded.


### 2026-11-10 — M10 Audit content and player safety

1. Review text submissions, names, drawings and moderation/host-removal tools.

2. Check asset/music rights, credits, account deletion/privacy paths and absence of test-only endpoints.

3. Close exploitable or misleading behavior and publish clear instructions.

Done when: Public pilot has working moderation and accurate data disclosures.


### 2026-11-17 — M11 Review a week of public-party feedback

1. Collect consenting party-session feedback without storing unnecessary personal data.

2. Compare repeat sessions, drop-off by game and support incidents.

3. Choose release completion or a small focused remediation list; keep any Meta entry claims in its separate lane.

Done when: Accepted product MVP with evidence and a bounded next backlog.


## Living Table: First Steps

Living Table: First Steps lets a player make a real board-game move, have Meta AI interpret a bounded observation, and see useful guidance grounded in the actual game state.


Track: Recommended: Indie developer · theme: Build for Your World


Build: Keep the Astra foundation. Use one known chessboard/setup and a narrow Meta vision observation pipeline, with deterministic chess rules validating suggestions. Deliver one clear physical action → AI understanding → spatial response loop.


Demo: Show the real board first. Move one piece, show the AI’s grounded interpretation, then an AI-assisted reply and the same session continuing. Explain uncertainty through visible correction, not invisible edits.


Cut: Cut universal board recognition, a second game, NFT/IAP systems, new 3D characters and full conversational-host scope from this early entry. Do not rebuild the platform.


### 2026-10-29 — A01 Verify First Steps acceptance and track

1. Open the accepted application/participant portal and record actual enrollment; do not create a duplicate application.

2. Compare Indie developer (recommended for this individual-builder prototype) with any recorded Startup selection; seek organizer clarification if the accepted entry differs.

3. Read participant terms for existing Astra code, new work and cross-contest reuse; record unresolved issues before claiming eligibility.

Done when: Acceptance, actual track and eligible new work are evidenced; recommendation is distinguished from enrollment.


### 2026-10-30 — A02 Lock the First Steps model-assisted board interaction

1. Use the existing Living Table build and choose one bounded physical-board interaction for the early demo.

2. Proposed slice: camera observation → structured suggested board change → deterministic rule validation → clear visual feedback. Define ten representative cases and an explicit uncertain result.

3. Select an eligible Meta model that can perform the chosen job on supported hardware; do not claim automatic arbitrary-board recognition or replace the current build with a new platform.

Done when: One measurable feature fits the chosen entry and accepted rules.


### 2026-10-30 — A03 Prove a real model run on supported infrastructure

1. Select an eligible Meta model and verify its license/hardware requirements.

2. Prefer a self-hosted open-weight path; use existing authorized contest access only if needed and allowed.

3. Run fixtures with actual model output; record model/version, latency and failures without secrets.

Done when: Real inference works on the declared route; mocked output is never presented as inference.


### 2026-10-31 — A04 Integrate the narrow model workflow

1. Wire the real response through schema validation, timeouts and input limits.

2. Validate model suggestions against the chess state and require a visible uncertainty/manual fallback path; generated observations cannot bypass legal-move checks.

3. Show provider failure clearly and retain usable deterministic core behavior.

Done when: User can finish the selected workflow with real inference and safe failure handling.


### 2026-11-01 — A05 Evaluate usefulness and reliability

1. Run the ten fixtures plus malformed/hostile/empty inputs and a baseline without the model.

2. Measure completion, latency, human correction and factual/format failures.

3. Improve the largest failure source; keep raw outcomes and do not manufacture a quality percentage.

Done when: Claimed model benefit is backed by comparisons and honest limitations.


### 2026-11-02 — A06 Make the chosen entry reproducible

1. Package source, model setup and hardware/license requirements with a small example dataset.

2. Test in a clean environment; keep mandatory paid APIs out of the default path.

3. Verify how judges can access the real model according to participant instructions.

Done when: A reviewer can reproduce the declared workflow without undisclosed services.


### 2026-11-03 — A07 Prepare the actual portal submission assets

1. Read exact participant portal fields and video limits; do not assume another contest’s format.

2. Capture real input → model → useful result footage with failures/limits accurately described.

3. Complete description, technical architecture, source and deployment links against the selected entry.

Done when: All required assets match the working implementation and portal format.


### 2026-11-04 — A08 Submit with time for recovery

1. Check links and model access early; target 2 p.m. Central.

2. Submit before 8 p.m. Central and retain acceptance/receipt.

3. Verify the final portal status and preserve the judged build.

Done when: Actual submission receipt on time.


### 2026-11-05 — A09 Maintain judging access

1. Check model endpoint/self-hosted runtime and judge instructions.

2. Respond only to actual organizer requests; record any permitted change.

3. Keep the submitted feature behavior stable.

Done when: Judges can reach the declared experience.


### 2026-11-09 — A10 Close the Meta competition record

1. Check official results and any finalist instructions.

2. Archive actual outcome and feedback.

3. Record next product decision separately from prize hopes.

Done when: Outcome recorded or explicitly pending.


## Unity Jam

A compact, self-teaching WebGL game whose central choice embodies the announced theme.


Track: Official Unity Game Jam · Unity Choice route


Build: Wait for the theme, then choose one mechanic, one short progression and one satisfying ending. Put control feel and readable feedback ahead of content quantity.


Demo: The player understands the verb immediately, makes a theme-driven choice, sees a surprising consequence and can replay.


Cut: No prebuilt Living Table/MAYHEM port, multiplayer backend, sprawling story or extra systems. Theme is unknown: choose the actual game after kickoff.


### 2026-10-29 — U01 Resolve Unity timing and eligibility

1. Check official rules link and the conflicting kickoff times on the jam page.

2. Record a conservative start: no gameplay until the announced kickoff is confirmed.

3. Confirm account/team eligibility and note WebGL prize requirement and AI disclosure.

Done when: Rules gaps are resolved or explicitly blocking gameplay start.


### 2026-10-30 — U02 Verify an empty WebGL export

1. Use a disposable tooling project with no entry gameplay.

2. Install/verify the WebGL module and export an empty scene.

3. Serve locally and test load/input; record editor version and packaging settings.

Done when: Toolchain works without starting the jam game.


### 2026-11-01 — U03 Prepare repository and asset provenance process

1. Prepare an empty repository, backup location and asset-license worksheet.

2. Inventory only reusable tools/assets permitted by rules; do not build a menu/gameplay shell as entry work.

3. Document AI disclosure capture and itch account upload access.

Done when: Ready to create a new eligible game after opening.


### 2026-11-05 — U04 Choose the theme-driven one-loop concept

1. After confirmed kickoff, record the exact theme and start time.

2. Sketch three tiny mechanics; choose the one where removing the theme breaks the game.

3. Lock one level/loop, win/loss, controls and a strict cut list; create the new game repository.

Done when: A finishable original concept tied to theme, not a reskin.


### 2026-11-06 — U05 Build a complete greybox loop

1. Implement one player action, obstacle/system response and feedback.

2. Add start, win/loss and restart with placeholder assets.

3. Export WebGL immediately and play the full loop outside the editor.

Done when: A stranger can start, finish and restart a playable build.


### 2026-11-07 — U06 Test whether the mechanic is fun

1. Run three short uncoached sessions and note confusion/dead time.

2. Tune feedback, pacing and difficulty without adding a second core system.

3. Cut weak features and retest the same loop.

Done when: Clear objective and satisfying outcome without verbal explanation.


### 2026-11-08 — U07 Unify art and audio

1. Choose a coherent palette, scale and sound language around the mechanic.

2. Replace only essential placeholders, add readable UI and audio controls.

3. Record asset sources and AI use as work happens.

Done when: Presentation clarifies gameplay and has complete credits.


### 2026-11-09 — U08 Finish progression and onboarding

1. Add only the minimum progression needed to deliver the theme payoff.

2. Teach controls in play; add pause, mute, restart and accessible contrast where relevant.

3. Verify no unwinnable state and sensible ending.

Done when: Feature-complete short game with all lifecycle controls.


### 2026-11-10 — U09 Test the actual itch WebGL build

1. Upload a candidate to the intended itch page and use its embedded player.

2. Test fresh load, browser focus, audio unlock, keyboard/touch if supported and replay.

3. Measure download/load behavior and fix browser-only errors.

Done when: Playable through itch without extra downloads or developer tools.


### 2026-11-11 — U10 Freeze features and fix blockers

1. Run an uncoached full playthrough on a second machine.

2. Fix only crashes, broken objectives and unreadable controls.

3. Pin the candidate and maintain a list of nonblocking limitations.

Done when: No blocked start-to-finish route; no new scope.


### 2026-11-12 — U11 Prepare the itch submission

1. Write short description, controls, team, credits and generative-AI disclosure.

2. Capture representative screenshots and verify all game files are packaged on itch.

3. Review current prize eligibility and submission fields.

Done when: Complete, truthful page with a playable WebGL candidate.


### 2026-11-13 — U12 Submit Unity early

1. Submit by noon Central and check jam membership/submission status.

2. Test the submitted itch player as a new visitor.

3. Save the receipt and immutable final artifact; retain Nov 14 before 6 a.m. only for emergency recovery.

Done when: Confirmed submitted entry before the hard close.


### 2026-11-14 — U13 Freeze game files for judging

1. Verify the final receipt and access before cutoff.

2. Prevent automatic deployments or server changes that alter the judged game.

3. Record the freeze and only make changes permitted by official instructions.

Done when: Game behavior remains the submitted version.


### 2026-11-20 — U14 Participate and collect feedback honestly

1. Use the official rating queue and play assigned games.

2. Record feedback on your entry without vote manipulation.

3. Keep proposed fixes in a post-judging branch, not the live judged files.

Done when: Feedback captured without altering the entry.


### 2026-12-07 — U15 Verify judging close before releasing fixes

1. Recheck official judging dates and unresolved timing statements.

2. Confirm access remains available through the actual close.

3. Prepare post-jam changes separately until allowed.

Done when: No premature change to judged files.


### 2026-12-10 — U16 Archive Unity results and decide next release

1. Read actual results and reviewer feedback.

2. Archive source, build, credits and receipt.

3. Choose a small post-jam release or conclude the project with a reproducible final build.

Done when: Outcome and next/no-next decision recorded.


## Epic AI MegaJam

A short Unreal experience with one memorable spatial mechanic, a coherent visual identity and an ending.


Track: Unreal Engine · Best Overall; Solo Driver if eligible


Build: After theme reveal and the allowed opening, build one encounter/space with a distinctive interaction. Package early and preserve a working fallback.


Demo: Start the 30–60-second clip with the mechanic in action; show escalation and payoff from the packaged game.


Cut: No open world, multiplayer, dual Unreal/UEFN entries or last-minute modifier chase. Choose the final mechanic only when the theme is known.


### 2026-10-30 — E01 Obtain full MegaJam rules and confirm engine

1. Open the official Box rules with the entrant account and record unresolved terms.

2. Confirm Unreal Engine as planned or explicitly choose UEFN; never plan both entries.

3. Record opening Nov 11, 11 p.m. Central and close Nov 15, 7:59 p.m. Central.

Done when: Full terms and engine choice are known before entry development.


### 2026-11-02 — E02 Prove an empty packaged build

1. Verify installed engine/compiler and target platform SDK in a disposable project.

2. Package an empty project and run it outside the editor.

3. Record prerequisites and repeatable packaging command/settings; no entry gameplay.

Done when: Toolchain packaging succeeds on the chosen platform.


### 2026-11-04 — E03 Prepare source, tooling and premade declarations

1. Prepare empty repo, backup path and devlog capture format.

2. Test permitted editor/agent tools without implementing the game.

3. Inventory pre-existing scripts/prompts/assets and their licenses for disclosure.

Done when: Tooling ready; provenance captured.


### 2026-11-11 — E04 Record theme and choose a tiny scope

1. Watch/read theme reveal at 1 p.m. Central.

2. Write a one-loop concept and cut list; do not implement before the opening.

3. At or after 11 p.m. Central, record start evidence and create the entry project.

Done when: Theme and legal start recorded; one finishable scope.


### 2026-11-12 — E05 Build and package the core loop

1. Implement one mechanic, one encounter/space and win/loss/restart.

2. Use only necessary placeholders and explicit input instructions.

3. Package on day one and test from the executable; preserve a working fallback.

Done when: One complete playable loop outside the editor.


### 2026-11-13 — E06 Validate fun and progression

1. Get two uncoached playthroughs of the package.

2. Fix control feel, unclear goals and difficulty before adding content.

3. Add only enough progression to deliver a memorable ending.

Done when: Playable, understandable short experience with theme-driven choices.


### 2026-11-14 — E07 Polish the proven loop and freeze

1. Unify art, lighting, UI and sound around the core interaction.

2. Check performance and packaged-start behavior on a second machine.

3. Fix blockers and freeze; do not chase optional prize modifiers at the cost of completion.

Done when: Stable candidate with coherent presentation and credits.


### 2026-11-14 — E08 Capture gameplay and compile submission fields

1. Record 30–60 seconds of real packaged gameplay, showing play immediately.

2. Prepare team/game details and declared prior material; keep private account emails out of the public dashboard.

3. If Glass Box was explicitly chosen, provide the actual development log; otherwise omit the optional lane.

Done when: Video and fields match the package and selected route.


### 2026-11-15 — E09 Upload and submit with a recovery buffer

1. Upload the packaged Unreal build and test its downloaded copy; if UEFN was selected, use its required fixed version code instead.

2. Submit by noon Central; verify membership and Submitted state.

3. Keep time until 7:59 p.m. for failed uploads only; archive receipt and exact artifact.

Done when: Actual on-time submission with working access.


### 2026-11-16 — E10 Preserve the judged snapshot

1. Recheck the submission links and downloadable package.

2. Retain source, declarations, video and build as one immutable record.

3. Keep any future development separate and follow organizer instructions for access.

Done when: Judged entry is reproducible.


### 2026-12-01 — E11 Check organizer requests and access

1. Review actual communications and project access.

2. Respond to requested evidence without assuming a grant or finalist place.

3. Record pending decisions rather than close them early.

Done when: No missed required follow-up.


### 2027-01-29 — E12 Archive MegaJam outcome

1. Check official results by the announced date and any changes.

2. Record award/no-award/pending status with evidence.

3. Decide whether continuation is independently worthwhile; never budget unawarded grant money.

Done when: Competition record closed or explicitly awaiting organizer decision.


## Living Table

Continue the same chess game across human and AI opponents and between physical and virtual boards, using your hands.


Track: VR Start · Gaming; division selected from project provenance


Build: Polish the continuity already implemented: correct physical moves, fixed AI proposals, saved sessions, easy alignment and a clear route back to the physical board.


Demo: Human opponent leaves → AI takes the seat → player moves the AI piece → continue virtually → return to the same real game.


Cut: Pause new Candy Land effects, extra games, commerce and broader platform promises until this handoff is excellent. Keep Candy Land in the general product, outside the audited chess contest package.


### 2026-10-29 — LT-H01 Pin the exact build and test environment

1. Select the APK to test; record version, SHA-256, source commit, Quest OS, refresh rate and permission state.

2. Photograph the physical board orientation and note piece style, room, lighting, seat and table dimensions.

3. Export existing saves before testing. Use a named disposable session; capture headset video plus a view of the real board.

Done when: Every result can be traced to one APK, session and physical setup.


### 2026-10-30 — LT-H02 Prepare diagnostic evidence and reproducible positions

1. Expose or export position/FEN, side to move, move number, session ID, journal sequence, pending AI move, control mode and surface mode.

2. Prepare legal positions for a normal move, capture, castling, en passant and promotion. Keep imported fixtures visibly labeled.

3. Run chess rules, handoff and session regression suites against the selected source; retain output and fixture IDs.

Done when: Before/after state can be compared without relying only on the board animation.


### 2026-10-30 — LT-H03 Align and recognize an ordinary human move

1. Start a tracked physical game in the standard position. Align playing-area corners, excluding the decorative frame.

2. Move e2 to e4; clear hands and wait. Repeat legal moves across the center and all four corners.

3. Inspect journal and recognition feedback after each move; test from natural head positions.

Done when: Each legal move commits exactly once; all squares remain aligned. No manual confirmation is silently presented as camera verification.


### 2026-10-31 — LT-H04 Transfer the opponent from human to AI

1. After a human turn, open HANDOFF and choose AI TAKE OVER; compare position, history and session identity.

2. Wait for the AI instruction; verify the named piece, origin and destination are unambiguous.

3. Move the physical AI piece as instructed and clear hands; confirm one commit before the next turn.

Done when: Same game continues; the AI proposal is fixed until correctly executed.


### 2026-10-31 — LT-H05 Reject wrong, extra and ambiguous physical changes

1. For a pending AI move, try a wrong destination, a different piece, and two changed pieces.

2. Leave the correct piece between squares, cover the origin/destination, then restore the expected layout.

3. Check that recovery names the required action and that the journal never advances during ambiguity.

Done when: Zero false commits. Correcting the board resumes the locked move once, without reset or replacement.


### 2026-11-01 — LT-H06 Verify capture and compound-move reconciliation

1. Run the capture fixture; deliberately leave the captured piece on the board before removing it.

2. Castle king first and stop, then move the rook; repeat in reverse order. Try en passant without removing the captured pawn.

3. Promote with a missing or incorrect replacement; then correct it. Inspect all required squares and state fields.

Done when: Partial actions do not commit. Each complete legal action commits once with correct rights and counters.


### 2026-11-01 — LT-H07 Return control to the human without replacing an AI move

1. Switch HUMAN RETURNS while an AI move is pending; attempt another legal move.

2. Complete the original instruction, then let the human opponent make the following move.

3. Repeat after the AI move has already committed; compare history and active seat.

Done when: A pending proposal cannot be rerolled; control changes take effect at a clear boundary.


### 2026-11-02 — LT-H08 Continue the same position virtually

1. From the physical session choose GO VIRTUAL. Compare every piece and side to move.

2. Repeat with a physical AI proposal still pending; confirm virtual execution occurs exactly once.

3. Play three virtual moves, save, relaunch and resume. Compare session, history and legal rights.

Done when: No reset, duplicate AI move, lost capture or lost rule state.


### 2026-11-03 — LT-H09 Reconcile virtual progress back onto the real board

1. Choose USE REAL BOARD after virtual moves. Compare the guide against the real pieces.

2. Deliberately leave one piece misplaced and attempt to continue; then correct the arrangement.

3. Verify the camera can re-establish a trustworthy baseline. If it cannot, keep MANUAL CHECK explicit, record its provenance and keep the automatic-reconciliation claim blocked.

Done when: Automatic reconciliation passes only with measured rejection of wrong layouts. A labeled manual fallback is recorded as a limitation, not an automatic pass.


### 2026-11-04 — LT-H10 Recover from tracking, permission and lifecycle interruptions

1. Cover the camera, move hands out of view and change lighting during a pending move; restore each condition.

2. Sleep/wake and force-close/reopen before and after a move commit, and during a handoff.

3. Deny camera permission, exercise the labeled manual fallback, then restore permission and rescan.

Done when: No unsolicited moves, lost state or dead ends. Recovery states what the user should do next.


### 2026-11-04 — LT-H11 Import an existing game honestly and protect saved state

1. Use IMPORT GAME for a real midgame position; explicitly set turn, castling and en-passant state.

2. Compare all 64 squares, then hand control to AI. Verify imported history remains labeled.

3. In disposable test data, simulate a save failure or damaged journal; check refusal/retry without silent restart.

Done when: Imported state is exact from import onward; unknown earlier history is never fabricated. Save failure blocks further commits.


### 2026-11-05 — LT-H12 Close the full handoff acceptance gate

1. Run human → AI → human → virtual → physical → save/relaunch three times without developer intervention.

2. Repeat in a second room with a second board/piece style. Record failures and manual interventions separately.

3. Fix every state-loss, false-commit or dead-end defect; repeat affected scenarios on the new build.

Done when: Three clean cycles per setup, zero false commits or lost state. This is our internal release target, not a contest rule.


### 2026-10-30 — LT-U01 Design and time the first five minutes

1. Write the first-use sequence: launch, permission explanation, place/align, pick up a piece, legal move, opponent handoff.

2. Keep one primary action per state. Teach the pinch where it is used and explain physical AI piece movement before it is needed.

3. Time a cold launch and note where instructions require coaching.

Done when: Proposed internal targets: first legal move within two minutes; handoff payoff within five. No controller needed.


### 2026-11-02 — LT-U02 Make pickup and release feel precise

1. With each hand, pick up every piece type near board edges and neighboring pieces.

2. Verify the piece remains between thumb and forefinger without a scale jump; release on legal and illegal squares.

3. Test fast pinches, hand loss and hand crossing; confirm hover/press feedback and pickup/drop audio.

Done when: No wrong-piece selection, unintended move or duplicate sound; invalid release returns clearly and safely.


### 2026-11-03 — LT-U03 Keep essential UI readable and reachable

1. Use the board seated at near/far comfortable distances; read turn, move and recovery prompts.

2. Move/anchor the panel and board; inspect overlap, depth sorting, contrast and hand reach.

3. Check a narrower comfortable field of view and audio-off operation; verify color is not the only status cue.

Done when: Critical information stays in comfortable view, controls are reachable and prompts remain understandable without sound.


### 2026-11-05 — LT-U04 Validate anchoring and scene understanding in unfamiliar rooms

1. Inspect actual MRUK/scene-surface integration; package presence alone does not establish use.

2. Test placement, relocalization and board movement on three tables in three rooms, including one not used for development.

3. Restart, look away/back and change seats; measure overlay drift against physical square edges.

Done when: No unsafe or visibly wrong placement; drift stays within an agreed fraction of a square (initial target 10%). Unsupported surfaces explain recovery.


### 2026-11-06 — LT-U05 Measure sustained Quest performance

1. Capture on-device FPS, CPU/GPU frame times, dropped frames and memory with the final intended refresh rate.

2. Run a 20-minute session spanning recognition, AI thinking, handoff, virtual play and return; include the warm device period.

3. Identify stalls/leaks, optimize the measured bottlenecks and rerun the same route.

Done when: At least 60 fps in measured gameplay, with headroom toward selected refresh rate; investigate every sustained dip and stall. No monotonic memory growth.


### 2026-11-09 — LT-U06 Test with five people who have not been coached

1. Recruit five testers with mixed chess/XR familiarity; record consent before any filming.

2. Give only the goal: start a game, let AI take over, continue virtually and return. Do not explain the controls.

3. Record first-move time, handoff time, assistance, mistakes, discomfort and whether they want to repeat the experience.

Done when: Internal target: all five complete the core route unaided, first move ≤2 min and payoff ≤5 min. Misses produce specific fixes, not adjusted scores.


### 2026-11-12 — LT-U07 Repair and repeat failed usability scenarios

1. Rank failures: state corruption/false moves/crashes first, blocked tasks second, friction/readability third.

2. Fix the causes and rerun dependent handoff and performance tests; collect fresh observations on the new APK.

3. Use fresh uncoached testers for onboarding changes; prior coaching cannot prove discoverability.

Done when: No open critical or blocking defects. Changed behavior has new-build evidence.


### 2026-11-13 — LT-U08 Freeze the candidate and perform a repeat-use check

1. Select the release candidate and pin its SHA-256; export the complete test log.

2. Resume a saved game the next day and complete a short session entirely with hands.

3. Freeze features; allow only fixes with targeted regression and an explicit new candidate fingerprint.

Done when: A returning player can resume confidently; the candidate has a traceable acceptance record.


### 2026-10-30 — LT-S01 Resolve membership, division and account access early

1. Verify Start membership and Developer Dashboard access using the submitting account; retain private receipts.

2. Review dated project provenance against the official division definitions; record the selected division and supporting history.

3. Resolve any funding/support eligibility uncertainty with the organizer before submission. Draft questions separately; do not send automatically.

Done when: Eligibility questions have evidence or a written resolution; unknowns remain blockers.


### 2026-10-31 — LT-S02 Audit the chess contest package and asset provenance

1. Inventory code, art, music, fonts and plugins with origins and applicable permissions.

2. Keep Candy Land in the general product; verify the separate contest APK and promotional materials exclude it and other uncleared content.

3. Review the final packed-asset report, app metadata and screenshots against the official content requirements.

Done when: Every submitted asset has a documented basis for inclusion; no general-build content leaks into the contest package.


### 2026-11-02 — LT-S03 Establish the Competition distribution path

1. Verify the existing Meta app ID, package identity and signing continuity; keep keys outside the repository.

2. Prepare the Competition channel and test upload/access with the intended account.

3. Document installation steps and any judge access requirements; do not confuse the GitHub download with the channel submission.

Done when: A tester can obtain and launch the intended package through the established contest distribution path.


### 2026-11-05 — LT-S04 Storyboard the handoff demonstration

1. 0:00–0:15: show a real chess game and the interruption: your opponent leaves.

2. 0:15–1:35: show AI takeover, physical response, virtual continuation and return to the real board in the same session.

3. 1:35–2:40: show saved resume and a brief explanation of local operation and honest limits; reserve time for a clear ending.

Done when: The distinctive benefit is visible immediately; the planned cut stays below three minutes.


### 2026-11-14 — LT-S05 Record honest gameplay from the accepted candidate

1. Record headset footage and a synchronized real-board view on the pinned candidate.

2. Show physical corrections and manual steps where they are part of the experience; avoid cuts that imply automatic recognition.

3. Edit a concise narrated demonstration with readable captions and balanced audio.

Done when: The video accurately represents the accepted build and its limits; duration is under three minutes.


### 2026-11-14 — LT-S06 Prepare the description and judge quick-start

1. Write the problem, distinctive handoff benefit, implementation, future plans and target launch date.

2. Provide an equipment list and a short hands-only test route, including physical-board alignment and manual fallback.

3. Review claims against acceptance evidence: no general piece scanning, cheat-proof guarantee or network multiplayer claim.

Done when: Every product claim is demonstrated or explicitly labeled as future work; a judge can reproduce the route.


### 2026-11-15 — LT-S07 Upload and verify the final judging package

1. Upload the accepted chess APK to the Competition channel; verify processed version and install it from that channel.

2. Publish the final video on the required public video platform and check playback signed out.

3. Retest the core route from the distributed build and verify all submission links from the intended viewer context.

Done when: Distributed APK matches the accepted candidate and every judge-facing link works.


### 2026-11-17 — LT-S08 Submit early and preserve the receipt

1. Complete the Devpost fields with final channel/video links and the evidenced track/division.

2. Review the submitted preview against the candidate; submit by the internal target of Nov 17, 5 p.m. Central.

3. Retain confirmation, final field export and links. Keep Nov 18 before 2 p.m. Central for recovery only.

Done when: Submission is actually accepted, with receipt and working links; a filled draft does not count.


### 2026-11-18 — LT-S09 Preserve judging access and the submitted version

1. Recheck receipt and access before the official cutoff; resolve only concrete submission issues.

2. Retain the exact submitted APK, source commit, checksums, video and evidence archive.

3. Keep judging access available; do not replace the submitted version after the deadline. Recheck the official announcement schedule.

Done when: The submitted entry remains reproducible and accessible through judging.


### 2026-11-25 — LT-J01 Check Living Table judging access

1. Verify Competition-channel access and public video playback.

2. Keep the submitted APK and game state instructions unchanged.

3. Record actual judge requests and any permitted corrective action.

Done when: Entry remains accessible without unapproved behavior changes.


### 2026-12-11 — LT-J02 Archive VR Start outcome

1. Check official results and any schedule update.

2. Store actual feedback and award/no-award/pending state.

3. Choose a post-contest chess release plan using observed tester needs.

Done when: Contest outcome recorded; no prize assumptions.