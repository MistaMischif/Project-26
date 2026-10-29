# Living Table — Chess contest checklist

Reviewed October 29, 2026. Proposed dates and internal acceptance targets; no checks are pre-completed.

## 1. Prove the handoff

The differentiator is one game continuing across people and surfaces.

- [ ] **H01 — Pin the exact build and test environment**

  Target: 2026-10-29. Owner: Codex + MASTER OMEGA. Prerequisites: .
  1. Select the APK to test; record version, SHA-256, source commit, Quest OS, refresh rate and permission state.
  2. Photograph the physical board orientation and note piece style, room, lighting, seat and table dimensions.
  3. Export existing saves before testing. Use a named disposable session; capture headset video plus a view of the real board.

  Pass: Every result can be traced to one APK, session and physical setup.
  Evidence: Artifact manifest, device/setup notes, starting session ID.

- [ ] **H02 — Prepare diagnostic evidence and reproducible positions**

  Target: 2026-10-30. Owner: Codex. Prerequisites: H01.
  1. Expose or export position/FEN, side to move, move number, session ID, journal sequence, pending AI move, control mode and surface mode.
  2. Prepare legal positions for a normal move, capture, castling, en passant and promotion. Keep imported fixtures visibly labeled.
  3. Run chess rules, handoff and session regression suites against the selected source; retain output and fixture IDs.

  Pass: Before/after state can be compared without relying only on the board animation.
  Evidence: Regression logs, fixture manifest, diagnostic snapshot.

- [ ] **H03 — Align and recognize an ordinary human move**

  Target: 2026-10-30. Owner: MASTER OMEGA + Codex. Prerequisites: H02.
  1. Start a tracked physical game in the standard position. Align playing-area corners, excluding the decorative frame.
  2. Move e2 to e4; clear hands and wait. Repeat legal moves across the center and all four corners.
  3. Inspect journal and recognition feedback after each move; test from natural head positions.

  Pass: Each legal move commits exactly once; all squares remain aligned. No manual confirmation is silently presented as camera verification.
  Evidence: Video, recognition latency per move, before/after state.

- [ ] **H04 — Transfer the opponent from human to AI**

  Target: 2026-10-31. Owner: MASTER OMEGA. Prerequisites: H03.
  1. After a human turn, open HANDOFF and choose AI TAKE OVER; compare position, history and session identity.
  2. Wait for the AI instruction; verify the named piece, origin and destination are unambiguous.
  3. Move the physical AI piece as instructed and clear hands; confirm one commit before the next turn.

  Pass: Same game continues; the AI proposal is fixed until correctly executed.
  Evidence: Continuous video plus pre-handoff, pending and committed snapshots.

- [ ] **H05 — Reject wrong, extra and ambiguous physical changes**

  Target: 2026-10-31. Owner: MASTER OMEGA + Codex. Prerequisites: H04.
  1. For a pending AI move, try a wrong destination, a different piece, and two changed pieces.
  2. Leave the correct piece between squares, cover the origin/destination, then restore the expected layout.
  3. Check that recovery names the required action and that the journal never advances during ambiguity.

  Pass: Zero false commits. Correcting the board resumes the locked move once, without reset or replacement.
  Evidence: One clip and state diff per negative case; bug IDs for every false accept.

- [ ] **H06 — Verify capture and compound-move reconciliation**

  Target: 2026-11-01. Owner: MASTER OMEGA + Codex. Prerequisites: H05.
  1. Run the capture fixture; deliberately leave the captured piece on the board before removing it.
  2. Castle king first and stop, then move the rook; repeat in reverse order. Try en passant without removing the captured pawn.
  3. Promote with a missing or incorrect replacement; then correct it. Inspect all required squares and state fields.

  Pass: Partial actions do not commit. Each complete legal action commits once with correct rights and counters.
  Evidence: Five fixture videos and state diffs: capture, both castle orders, en passant, promotion.

- [ ] **H07 — Return control to the human without replacing an AI move**

  Target: 2026-11-01. Owner: MASTER OMEGA. Prerequisites: H04.
  1. Switch HUMAN RETURNS while an AI move is pending; attempt another legal move.
  2. Complete the original instruction, then let the human opponent make the following move.
  3. Repeat after the AI move has already committed; compare history and active seat.

  Pass: A pending proposal cannot be rerolled; control changes take effect at a clear boundary.
  Evidence: Pending-move identity and journal comparison across both timings.

- [ ] **H08 — Continue the same position virtually**

  Target: 2026-11-02. Owner: MASTER OMEGA. Prerequisites: H06, H07.
  1. From the physical session choose GO VIRTUAL. Compare every piece and side to move.
  2. Repeat with a physical AI proposal still pending; confirm virtual execution occurs exactly once.
  3. Play three virtual moves, save, relaunch and resume. Compare session, history and legal rights.

  Pass: No reset, duplicate AI move, lost capture or lost rule state.
  Evidence: Board snapshots, move list and save/reload video.

- [ ] **H09 — Reconcile virtual progress back onto the real board**

  Target: 2026-11-03. Owner: Codex + MASTER OMEGA. Prerequisites: H08.
  1. Choose USE REAL BOARD after virtual moves. Compare the guide against the real pieces.
  2. Deliberately leave one piece misplaced and attempt to continue; then correct the arrangement.
  3. Verify the camera can re-establish a trustworthy baseline. If it cannot, keep MANUAL CHECK explicit, record its provenance and keep the automatic-reconciliation claim blocked.

  Pass: Automatic reconciliation passes only with measured rejection of wrong layouts. A labeled manual fallback is recorded as a limitation, not an automatic pass.
  Evidence: Wrong-layout and correct-layout videos; baseline invalidation/reacquisition evidence.

- [ ] **H10 — Recover from tracking, permission and lifecycle interruptions**

  Target: 2026-11-04. Owner: MASTER OMEGA + Codex. Prerequisites: H09.
  1. Cover the camera, move hands out of view and change lighting during a pending move; restore each condition.
  2. Sleep/wake and force-close/reopen before and after a move commit, and during a handoff.
  3. Deny camera permission, exercise the labeled manual fallback, then restore permission and rescan.

  Pass: No unsolicited moves, lost state or dead ends. Recovery states what the user should do next.
  Evidence: Interruption matrix with expected/actual behavior, session snapshots and logs.

- [ ] **H11 — Import an existing game honestly and protect saved state**

  Target: 2026-11-04. Owner: Codex + MASTER OMEGA. Prerequisites: H02, H10.
  1. Use IMPORT GAME for a real midgame position; explicitly set turn, castling and en-passant state.
  2. Compare all 64 squares, then hand control to AI. Verify imported history remains labeled.
  3. In disposable test data, simulate a save failure or damaged journal; check refusal/retry without silent restart.

  Pass: Imported state is exact from import onward; unknown earlier history is never fabricated. Save failure blocks further commits.
  Evidence: Import fixture, visible provenance label, save-failure regression logs.

- [ ] **H12 — Close the full handoff acceptance gate**

  Target: 2026-11-05. Owner: MASTER OMEGA. Prerequisites: H03, H04, H05, H06, H07, H08, H09, H10, H11.
  1. Run human → AI → human → virtual → physical → save/relaunch three times without developer intervention.
  2. Repeat in a second room with a second board/piece style. Record failures and manual interventions separately.
  3. Fix every state-loss, false-commit or dead-end defect; repeat affected scenarios on the new build.

  Pass: Three clean cycles per setup, zero false commits or lost state. This is our internal release target, not a contest rule.
  Evidence: Six continuous runs, defect closure links, per-build results.

## 2. Make it effortless

Judges must experience the benefit without developer coaching.

- [ ] **U01 — Design and time the first five minutes**

  Target: 2026-10-30. Owner: Codex + MASTER OMEGA. Prerequisites: H01.
  1. Write the first-use sequence: launch, permission explanation, place/align, pick up a piece, legal move, opponent handoff.
  2. Keep one primary action per state. Teach the pinch where it is used and explain physical AI piece movement before it is needed.
  3. Time a cold launch and note where instructions require coaching.

  Pass: Proposed internal targets: first legal move within two minutes; handoff payoff within five. No controller needed.
  Evidence: Annotated flow and timed walkthrough; failure points become bugs.

- [ ] **U02 — Make pickup and release feel precise**

  Target: 2026-11-02. Owner: Codex + MASTER OMEGA. Prerequisites: U01.
  1. With each hand, pick up every piece type near board edges and neighboring pieces.
  2. Verify the piece remains between thumb and forefinger without a scale jump; release on legal and illegal squares.
  3. Test fast pinches, hand loss and hand crossing; confirm hover/press feedback and pickup/drop audio.

  Pass: No wrong-piece selection, unintended move or duplicate sound; invalid release returns clearly and safely.
  Evidence: Twenty pickups per hand, failures counted by type, video of edge cases.

- [ ] **U03 — Keep essential UI readable and reachable**

  Target: 2026-11-03. Owner: Codex + MASTER OMEGA. Prerequisites: U01.
  1. Use the board seated at near/far comfortable distances; read turn, move and recovery prompts.
  2. Move/anchor the panel and board; inspect overlap, depth sorting, contrast and hand reach.
  3. Check a narrower comfortable field of view and audio-off operation; verify color is not the only status cue.

  Pass: Critical information stays in comfortable view, controls are reachable and prompts remain understandable without sound.
  Evidence: Screenshots from seated viewpoints, tester reach/readability notes.

- [ ] **U04 — Validate anchoring and scene understanding in unfamiliar rooms**

  Target: 2026-11-05. Owner: Codex + MASTER OMEGA. Prerequisites: H03, U03.
  1. Inspect actual MRUK/scene-surface integration; package presence alone does not establish use.
  2. Test placement, relocalization and board movement on three tables in three rooms, including one not used for development.
  3. Restart, look away/back and change seats; measure overlay drift against physical square edges.

  Pass: No unsafe or visibly wrong placement; drift stays within an agreed fraction of a square (initial target 10%). Unsupported surfaces explain recovery.
  Evidence: Room/table matrix, measured drift, anchor status and scene integration notes.

- [ ] **U05 — Measure sustained Quest performance**

  Target: 2026-11-06. Owner: Codex + MASTER OMEGA. Prerequisites: H08, U02, U04.
  1. Capture on-device FPS, CPU/GPU frame times, dropped frames and memory with the final intended refresh rate.
  2. Run a 20-minute session spanning recognition, AI thinking, handoff, virtual play and return; include the warm device period.
  3. Identify stalls/leaks, optimize the measured bottlenecks and rerun the same route.

  Pass: At least 60 fps in measured gameplay, with headroom toward selected refresh rate; investigate every sustained dip and stall. No monotonic memory growth.
  Evidence: Raw performance capture, segment timestamps, frame-time summary, thermal/refresh settings.

- [ ] **U06 — Test with five people who have not been coached**

  Target: 2026-11-09. Owner: MASTER OMEGA. Prerequisites: H12, U02, U03, U04, U05.
  1. Recruit five testers with mixed chess/XR familiarity; record consent before any filming.
  2. Give only the goal: start a game, let AI take over, continue virtually and return. Do not explain the controls.
  3. Record first-move time, handoff time, assistance, mistakes, discomfort and whether they want to repeat the experience.

  Pass: Internal target: all five complete the core route unaided, first move ≤2 min and payoff ≤5 min. Misses produce specific fixes, not adjusted scores.
  Evidence: Five anonymized worksheets with build hash and timestamps; consent kept privately.

- [ ] **U07 — Repair and repeat failed usability scenarios**

  Target: 2026-11-12. Owner: Codex + MASTER OMEGA. Prerequisites: U06.
  1. Rank failures: state corruption/false moves/crashes first, blocked tasks second, friction/readability third.
  2. Fix the causes and rerun dependent handoff and performance tests; collect fresh observations on the new APK.
  3. Use fresh uncoached testers for onboarding changes; prior coaching cannot prove discoverability.

  Pass: No open critical or blocking defects. Changed behavior has new-build evidence.
  Evidence: Issue list with reproduction, severity, fix commit and retest evidence.

- [ ] **U08 — Freeze the candidate and perform a repeat-use check**

  Target: 2026-11-13. Owner: MASTER OMEGA + Codex. Prerequisites: U07.
  1. Select the release candidate and pin its SHA-256; export the complete test log.
  2. Resume a saved game the next day and complete a short session entirely with hands.
  3. Freeze features; allow only fixes with targeted regression and an explicit new candidate fingerprint.

  Pass: A returning player can resume confidently; the candidate has a traceable acceptance record.
  Evidence: Candidate manifest, next-day resume recording, signed-off defect list.

## 3. Deliver the contender

A working feature needs an accessible build and a truthful, memorable demonstration.

- [ ] **S01 — Resolve membership, division and account access early**

  Target: 2026-10-30. Owner: MASTER OMEGA. Prerequisites: .
  1. Verify Start membership and Developer Dashboard access using the submitting account; retain private receipts.
  2. Review dated project provenance against the official division definitions; record the selected division and supporting history.
  3. Resolve any funding/support eligibility uncertainty with the organizer before submission. Draft questions separately; do not send automatically.

  Pass: Eligibility questions have evidence or a written resolution; unknowns remain blockers.
  Evidence: Private membership/access confirmation, provenance note, organizer response if needed.

- [ ] **S02 — Audit the chess contest package and asset provenance**

  Target: 2026-10-31. Owner: Codex + MASTER OMEGA. Prerequisites: S01.
  1. Inventory code, art, music, fonts and plugins with origins and applicable permissions.
  2. Keep Candy Land in the general product; verify the separate contest APK and promotional materials exclude it and other uncleared content.
  3. Review the final packed-asset report, app metadata and screenshots against the official content requirements.

  Pass: Every submitted asset has a documented basis for inclusion; no general-build content leaks into the contest package.
  Evidence: Asset ledger, packed report, screenshot/metadata review.

- [ ] **S03 — Establish the Competition distribution path**

  Target: 2026-11-02. Owner: MASTER OMEGA + Codex. Prerequisites: S01.
  1. Verify the existing Meta app ID, package identity and signing continuity; keep keys outside the repository.
  2. Prepare the Competition channel and test upload/access with the intended account.
  3. Document installation steps and any judge access requirements; do not confuse the GitHub download with the channel submission.

  Pass: A tester can obtain and launch the intended package through the established contest distribution path.
  Evidence: Channel link, successful processing status and independent install record.

- [ ] **S04 — Storyboard the handoff demonstration**

  Target: 2026-11-05. Owner: Codex + MASTER OMEGA. Prerequisites: H04.
  1. 0:00–0:15: show a real chess game and the interruption: your opponent leaves.
  2. 0:15–1:35: show AI takeover, physical response, virtual continuation and return to the real board in the same session.
  3. 1:35–2:40: show saved resume and a brief explanation of local operation and honest limits; reserve time for a clear ending.

  Pass: The distinctive benefit is visible immediately; the planned cut stays below three minutes.
  Evidence: Shot list mapped to actual working features; no simulated capability claims.

- [ ] **S05 — Record honest gameplay from the accepted candidate**

  Target: 2026-11-14. Owner: MASTER OMEGA. Prerequisites: U08, S02, S04.
  1. Record headset footage and a synchronized real-board view on the pinned candidate.
  2. Show physical corrections and manual steps where they are part of the experience; avoid cuts that imply automatic recognition.
  3. Edit a concise narrated demonstration with readable captions and balanced audio.

  Pass: The video accurately represents the accepted build and its limits; duration is under three minutes.
  Evidence: Raw takes, edit file, final video duration and candidate hash.

- [ ] **S06 — Prepare the description and judge quick-start**

  Target: 2026-11-14. Owner: Codex + MASTER OMEGA. Prerequisites: S04, U08.
  1. Write the problem, distinctive handoff benefit, implementation, future plans and target launch date.
  2. Provide an equipment list and a short hands-only test route, including physical-board alignment and manual fallback.
  3. Review claims against acceptance evidence: no general piece scanning, cheat-proof guarantee or network multiplayer claim.

  Pass: Every product claim is demonstrated or explicitly labeled as future work; a judge can reproduce the route.
  Evidence: Submission draft, screenshots, quick-start and claim-to-evidence review.

- [ ] **S07 — Upload and verify the final judging package**

  Target: 2026-11-15. Owner: MASTER OMEGA + Codex. Prerequisites: U08, S02, S03, S05, S06.
  1. Upload the accepted chess APK to the Competition channel; verify processed version and install it from that channel.
  2. Publish the final video on the required public video platform and check playback signed out.
  3. Retest the core route from the distributed build and verify all submission links from the intended viewer context.

  Pass: Distributed APK matches the accepted candidate and every judge-facing link works.
  Evidence: Channel version/hash, fresh install result, video URL and access checks.

- [ ] **S08 — Submit early and preserve the receipt**

  Target: 2026-11-17. Owner: MASTER OMEGA. Prerequisites: H12, U08, S01, S02, S03, S05, S06, S07.
  1. Complete the Devpost fields with final channel/video links and the evidenced track/division.
  2. Review the submitted preview against the candidate; submit by the internal target of Nov 17, 5 p.m. Central.
  3. Retain confirmation, final field export and links. Keep Nov 18 before 2 p.m. Central for recovery only.

  Pass: Submission is actually accepted, with receipt and working links; a filled draft does not count.
  Evidence: Timestamped receipt, final submission URL and archive.

- [ ] **S09 — Preserve judging access and the submitted version**

  Target: 2026-11-18. Owner: MASTER OMEGA. Prerequisites: S08.
  1. Recheck receipt and access before the official cutoff; resolve only concrete submission issues.
  2. Retain the exact submitted APK, source commit, checksums, video and evidence archive.
  3. Keep judging access available; do not replace the submitted version after the deadline. Recheck the official announcement schedule.

  Pass: The submitted entry remains reproducible and accessible through judging.
  Evidence: Final archive and access record; ongoing access is not assumed from a past check.

Sources:
- [Official contest rules · checked Oct 29](https://start-developer-competition-26.devpost.com/rules)
- [Chess 0.4.0 validation and limitations](https://github.com/MistaMischif/Project-26/tree/main/releases/0.4.0-chess-contest)
- [Current general build 0.6.3](https://github.com/MistaMischif/Project-26/releases/tag/v0.6.3-themed-rewards)
- [Session integrity and physical verification limits](https://github.com/MistaMischif/Project-26/blob/main/Assets/LivingTable/SESSIONS.md)
