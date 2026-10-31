window.CONTEST_PLAN = {
  "schema": 1,
  "revision": "2026-10-31",
  "title": "Living Table \u00b7 Chess contest rundown",
  "deadline": "2026-11-18T14:00:00-06:00",
  "target": "2026-11-17T17:00:00-06:00",
  "groups": [
    {
      "id": 1,
      "title": "Prove the handoff",
      "why": "The differentiator is one game continuing across people and surfaces.",
      "gate": "Zero false commits, lost state or unexplained recovery.",
      "window": "Oct 29\u2013Nov 5"
    },
    {
      "id": 2,
      "title": "Make it effortless",
      "why": "Judges must experience the benefit without developer coaching.",
      "gate": "Five uncoached testers, comfortable hands-first use, measured performance.",
      "window": "Oct 30\u2013Nov 13"
    },
    {
      "id": 3,
      "title": "Deliver the contender",
      "why": "A working feature needs an accessible build and a truthful, memorable demonstration.",
      "gate": "Verified judging package and submission receipt.",
      "window": "Oct 30\u2013Nov 18"
    }
  ],
  "tasks": [
    {
      "id": "H01",
      "group": 1,
      "title": "Pin the exact build and test environment",
      "date": "2026-10-29",
      "owner": "Codex + MASTER OMEGA",
      "depends": [],
      "steps": [
        "Select the APK to test; record version, SHA-256, source commit, Quest OS, refresh rate and permission state.",
        "Photograph the physical board orientation and note piece style, room, lighting, seat and table dimensions.",
        "Export existing saves before testing. Use a named disposable session; capture headset video plus a view of the real board."
      ],
      "passCriteria": "Every result can be traced to one APK, session and physical setup.",
      "evidence": "Artifact manifest, device/setup notes, starting session ID.",
      "baseline": "First Steps 0.1.2 is the current chess handoff test candidate. Use its exact published fingerprint and record device/environment; never inherit older headset passes.",
      "scope": "build"
    },
    {
      "id": "H02",
      "group": 1,
      "title": "Prepare diagnostic evidence and reproducible positions",
      "date": "2026-10-30",
      "owner": "Codex",
      "depends": [
        "H01"
      ],
      "steps": [
        "Expose or export position/FEN, side to move, move number, session ID, journal sequence, pending AI move, control mode and surface mode.",
        "Prepare legal positions for a normal move, capture, castling, en passant and promotion. Keep imported fixtures visibly labeled.",
        "Run chess rules, handoff and session regression suites against the selected source; retain output and fixture IDs."
      ],
      "passCriteria": "Before/after state can be compared without relying only on the board animation.",
      "evidence": "Regression logs, fixture manifest, diagnostic snapshot.",
      "baseline": "0.1.2: 33 full-board observation gate cases, 14 scene-transition checks, 20 move observation cases and chess/handoff regression suites. Camera accuracy is not established by synthetic observations.",
      "scope": "build"
    },
    {
      "id": "H03",
      "group": 1,
      "title": "Align and recognize an ordinary human move",
      "date": "2026-10-30",
      "owner": "MASTER OMEGA + Codex",
      "depends": [
        "H02"
      ],
      "steps": [
        "Start a tracked physical game in the standard position. Align playing-area corners, excluding the decorative frame.",
        "Move e2 to e4; clear hands and wait. Repeat legal moves across the center and all four corners.",
        "Inspect journal and recognition feedback after each move; test from natural head positions."
      ],
      "passCriteria": "Each legal move commits exactly once; all squares remain aligned. No manual confirmation is silently presented as camera verification.",
      "evidence": "Video, recognition latency per move, before/after state.",
      "baseline": "First Steps checks the starting physical position before watching legal moves. Local Meta observations are gated against saved state. Previous synthetic model errors remain unresolved; measure real-board accuracy and latency.",
      "scope": "build"
    },
    {
      "id": "H04",
      "group": 1,
      "title": "Transfer the opponent from human to AI",
      "date": "2026-10-31",
      "owner": "MASTER OMEGA",
      "depends": [
        "H03"
      ],
      "steps": [
        "After a human turn, open HANDOFF and choose AI TAKE OVER; compare position, history and session identity.",
        "Wait for the AI instruction; verify the named piece, origin and destination are unambiguous.",
        "Move the physical AI piece as instructed and clear hands; confirm one commit before the next turn."
      ],
      "passCriteria": "Same game continues; the AI proposal is fixed until correctly executed.",
      "evidence": "Continuous video plus pre-handoff, pending and committed snapshots.",
      "baseline": "Scene regression passed: switching to AI retains session and position. Real-board execution and clear prompts still require headset acceptance.",
      "scope": "build"
    },
    {
      "id": "H05",
      "group": 1,
      "title": "Reject wrong, extra and ambiguous physical changes",
      "date": "2026-10-31",
      "owner": "MASTER OMEGA + Codex",
      "depends": [
        "H04"
      ],
      "steps": [
        "For a pending AI move, try a wrong destination, a different piece, and two changed pieces.",
        "Leave the correct piece between squares, cover the origin/destination, then restore the expected layout.",
        "Check that recovery names the required action and that the journal never advances during ambiguity."
      ],
      "passCriteria": "Zero false commits. Correcting the board resumes the locked move once, without reset or replacement.",
      "evidence": "One clip and state diff per negative case; bug IDs for every false accept.",
      "baseline": "Incomplete, duplicate, uncertain, mismatched and stale observations remain blocked. Wrong-layout rejection must still be tested with a real camera.",
      "scope": "build"
    },
    {
      "id": "H06",
      "group": 1,
      "title": "Verify capture and compound-move reconciliation",
      "date": "2026-11-01",
      "owner": "MASTER OMEGA + Codex",
      "depends": [
        "H05"
      ],
      "steps": [
        "Run the capture fixture; deliberately leave the captured piece on the board before removing it.",
        "Castle king first and stop, then move the rook; repeat in reverse order. Try en passant without removing the captured pawn.",
        "Promote with a missing or incorrect replacement; then correct it. Inspect all required squares and state fields."
      ],
      "passCriteria": "Partial actions do not commit. Each complete legal action commits once with correct rights and counters.",
      "evidence": "Five fixture videos and state diffs: capture, both castle orders, en passant, promotion.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "H07",
      "group": 1,
      "title": "Return control to the human without replacing an AI move",
      "date": "2026-11-01",
      "owner": "MASTER OMEGA",
      "depends": [
        "H04"
      ],
      "steps": [
        "Switch HUMAN RETURNS while an AI move is pending; attempt another legal move.",
        "Complete the original instruction, then let the human opponent make the following move.",
        "Repeat after the AI move has already committed; compare history and active seat."
      ],
      "passCriteria": "A pending proposal cannot be rerolled; control changes take effect at a clear boundary.",
      "evidence": "Pending-move identity and journal comparison across both timings.",
      "baseline": "Scene regression passed: human return retains an already proposed AI move rather than rerolling it.",
      "scope": "build"
    },
    {
      "id": "H08",
      "group": 1,
      "title": "Continue the same position virtually",
      "date": "2026-11-02",
      "owner": "MASTER OMEGA",
      "depends": [
        "H06",
        "H07"
      ],
      "steps": [
        "From the physical session choose GO VIRTUAL. Compare every piece and side to move.",
        "Repeat with a physical AI proposal still pending; confirm virtual execution occurs exactly once.",
        "Play three virtual moves, save, relaunch and resume. Compare session, history and legal rights."
      ],
      "passCriteria": "No reset, duplicate AI move, lost capture or lost rule state.",
      "evidence": "Board snapshots, move list and save/reload video.",
      "baseline": "Scene regression passed: the locked AI move executes exactly once after virtual transfer and remains consumed after relaunch.",
      "scope": "build"
    },
    {
      "id": "H09",
      "group": 1,
      "title": "Reconcile virtual progress back onto the real board",
      "date": "2026-11-03",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "H08"
      ],
      "steps": [
        "Choose USE REAL BOARD after virtual moves. Compare the guide against the real pieces.",
        "Deliberately leave one piece misplaced and attempt to continue; then correct the arrangement.",
        "Verify the camera can re-establish a trustworthy baseline. If it cannot, keep MANUAL CHECK explicit, record its provenance and keep the automatic-reconciliation claim blocked.",
        "Let the full 64-square scan run. Measure total latency; try an extra piece, wrong-color piece and an interruption. Confirm every failure leaves the journal unchanged. MANUAL CHECK must enter manual mode; USE CAMERA must require a new scan."
      ],
      "passCriteria": "Automatic reconciliation passes only with measured rejection of wrong layouts. A labeled manual fallback is recorded as a limitation, not an automatic pass.",
      "evidence": "Wrong-layout and correct-layout videos; baseline invalidation/reacquisition evidence.",
      "baseline": "0.1.2 implements full 64-square model-assisted reconciliation, mismatch prompts, retry and fresh-frame/hand guards. Automated checks passed. Real-board accuracy and speed remain blocked; manual fallback is explicitly assisted.",
      "scope": "build"
    },
    {
      "id": "H10",
      "group": 1,
      "title": "Recover from tracking, permission and lifecycle interruptions",
      "date": "2026-11-04",
      "owner": "MASTER OMEGA + Codex",
      "depends": [
        "H09"
      ],
      "steps": [
        "Cover the camera, move hands out of view and change lighting during a pending move; restore each condition.",
        "Sleep/wake and force-close/reopen before and after a move commit, and during a handoff.",
        "Deny camera permission, exercise the labeled manual fallback, then restore permission and rescan."
      ],
      "passCriteria": "No unsolicited moves, lost state or dead ends. Recovery states what the user should do next.",
      "evidence": "Interruption matrix with expected/actual behavior, session snapshots and logs.",
      "baseline": "Scene tests reject recovery during pause, open handoff menus or stale revisions. Manual-to-camera switching now requires verification; hardware interruption tests remain open.",
      "scope": "build"
    },
    {
      "id": "H11",
      "group": 1,
      "title": "Import an existing game honestly and protect saved state",
      "date": "2026-11-04",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "H02",
        "H10"
      ],
      "steps": [
        "Use IMPORT GAME for a real midgame position; explicitly set turn, castling and en-passant state.",
        "Compare all 64 squares, then hand control to AI. Verify imported history remains labeled.",
        "In disposable test data, simulate a save failure or damaged journal; check refusal/retry without silent restart."
      ],
      "passCriteria": "Imported state is exact from import onward; unknown earlier history is never fabricated. Save failure blocks further commits.",
      "evidence": "Import fixture, visible provenance label, save-failure regression logs.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "H12",
      "group": 1,
      "title": "Close the full handoff acceptance gate",
      "date": "2026-11-05",
      "owner": "MASTER OMEGA",
      "depends": [
        "H03",
        "H04",
        "H05",
        "H06",
        "H07",
        "H08",
        "H09",
        "H10",
        "H11"
      ],
      "steps": [
        "Run human \u2192 AI \u2192 human \u2192 virtual \u2192 physical \u2192 save/relaunch three times without developer intervention.",
        "Repeat in a second room with a second board/piece style. Record failures and manual interventions separately.",
        "Fix every state-loss, false-commit or dead-end defect; repeat affected scenarios on the new build."
      ],
      "passCriteria": "Three clean cycles per setup, zero false commits or lost state. This is our internal release target, not a contest rule.",
      "evidence": "Six continuous runs, defect closure links, per-build results.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "U01",
      "group": 2,
      "title": "Design and time the first five minutes",
      "date": "2026-10-30",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "H01"
      ],
      "steps": [
        "Write the first-use sequence: launch, permission explanation, place/align, pick up a piece, legal move, opponent handoff.",
        "Keep one primary action per state. Teach the pinch where it is used and explain physical AI piece movement before it is needed.",
        "Time a cold launch and note where instructions require coaching."
      ],
      "passCriteria": "Proposed internal targets: first legal move within two minutes; handoff payoff within five. No controller needed.",
      "evidence": "Annotated flow and timed walkthrough; failure points become bugs.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "U02",
      "group": 2,
      "title": "Make pickup and release feel precise",
      "date": "2026-11-02",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "U01"
      ],
      "steps": [
        "With each hand, pick up every piece type near board edges and neighboring pieces.",
        "Verify the piece remains between thumb and forefinger without a scale jump; release on legal and illegal squares.",
        "Test fast pinches, hand loss and hand crossing; confirm hover/press feedback and pickup/drop audio.",
        "Pinch opposite outer rims with both hands; translate and rotate. Release one hand, then both, and re-grab. Confirm no accidental piece moves or resizing."
      ],
      "passCriteria": "No wrong-piece selection, unintended move or duplicate sound; invalid release returns clearly and safely.",
      "evidence": "Twenty pickups per hand, failures counted by type, video of edge cases.",
      "baseline": "Includes opposite-rim two-hand movement and yaw without resizing. Release both pinches before re-grabbing. Actual hand tracking remains to test.",
      "scope": "build"
    },
    {
      "id": "U03",
      "group": 2,
      "title": "Keep essential UI readable and reachable",
      "date": "2026-11-03",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "U01"
      ],
      "steps": [
        "Use the board seated at near/far comfortable distances; read turn, move and recovery prompts.",
        "Move/anchor the panel and board; inspect overlap, depth sorting, contrast and hand reach.",
        "Check a narrower comfortable field of view and audio-off operation; verify color is not the only status cue.",
        "Choose MOVE BOARD: board, pieces and board UI must disappear until placement. Place again; verify position, history and prior pause state are unchanged."
      ],
      "passCriteria": "Critical information stays in comfortable view, controls are reachable and prompts remain understandable without sound.",
      "evidence": "Screenshots from seated viewpoints, tester reach/readability notes.",
      "baseline": "MOVE BOARD hides board and pieces during placement. Full-board scan reports progress, expected square contents and SCAN BOARD retry. Headset readability remains to test.",
      "scope": "build"
    },
    {
      "id": "U04",
      "group": 2,
      "title": "Validate anchoring and scene understanding in unfamiliar rooms",
      "date": "2026-11-05",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "H03",
        "U03"
      ],
      "steps": [
        "Inspect actual MRUK/scene-surface integration; package presence alone does not establish use.",
        "Test placement, relocalization and board movement on three tables in three rooms, including one not used for development.",
        "Restart, look away/back and change seats; measure overlay drift against physical square edges.",
        "After physical-board relocation, confirm realignment is required. Test tracking loss and failed anchor persistence without losing the game."
      ],
      "passCriteria": "No unsafe or visibly wrong placement; drift stays within an agreed fraction of a square (initial target 10%). Unsupported surfaces explain recovery.",
      "evidence": "Room/table matrix, measured drift, anchor status and scene integration notes.",
      "baseline": "Session/history and prior pause state survive relocation. Physical mode requires realignment and verification. Anchor persistence and unfamiliar rooms remain unverified.",
      "scope": "build"
    },
    {
      "id": "U05",
      "group": 2,
      "title": "Measure sustained Quest performance",
      "date": "2026-11-06",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "H08",
        "U02",
        "U04"
      ],
      "steps": [
        "Capture on-device FPS, CPU/GPU frame times, dropped frames and memory with the final intended refresh rate.",
        "Run a 20-minute session spanning recognition, AI thinking, handoff, virtual play and return; include the warm device period.",
        "Identify stalls/leaks, optimize the measured bottlenecks and rerun the same route."
      ],
      "passCriteria": "At least 60 fps in measured gameplay, with headroom toward selected refresh rate; investigate every sustained dip and stall. No monotonic memory growth.",
      "evidence": "Raw performance capture, segment timestamps, frame-time summary, thermal/refresh settings.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "U06",
      "group": 2,
      "title": "Test with five people who have not been coached",
      "date": "2026-11-09",
      "owner": "MASTER OMEGA",
      "depends": [
        "H12",
        "U02",
        "U03",
        "U04",
        "U05"
      ],
      "steps": [
        "Recruit five testers with mixed chess/XR familiarity; record consent before any filming.",
        "Give only the goal: start a game, let AI take over, continue virtually and return. Do not explain the controls.",
        "Record first-move time, handoff time, assistance, mistakes, discomfort and whether they want to repeat the experience."
      ],
      "passCriteria": "Internal target: all five complete the core route unaided, first move \u22642 min and payoff \u22645 min. Misses produce specific fixes, not adjusted scores.",
      "evidence": "Five anonymized worksheets with build hash and timestamps; consent kept privately.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "U07",
      "group": 2,
      "title": "Repair and repeat failed usability scenarios",
      "date": "2026-11-12",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "U06"
      ],
      "steps": [
        "Rank failures: state corruption/false moves/crashes first, blocked tasks second, friction/readability third.",
        "Fix the causes and rerun dependent handoff and performance tests; collect fresh observations on the new APK.",
        "Use fresh uncoached testers for onboarding changes; prior coaching cannot prove discoverability."
      ],
      "passCriteria": "No open critical or blocking defects. Changed behavior has new-build evidence.",
      "evidence": "Issue list with reproduction, severity, fix commit and retest evidence.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "U08",
      "group": 2,
      "title": "Freeze the candidate and perform a repeat-use check",
      "date": "2026-11-13",
      "owner": "MASTER OMEGA + Codex",
      "depends": [
        "U07"
      ],
      "steps": [
        "Select the release candidate and pin its SHA-256; export the complete test log.",
        "Resume a saved game the next day and complete a short session entirely with hands.",
        "Freeze features; allow only fixes with targeted regression and an explicit new candidate fingerprint."
      ],
      "passCriteria": "A returning player can resume confidently; the candidate has a traceable acceptance record.",
      "evidence": "Candidate manifest, next-day resume recording, signed-off defect list.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "S01",
      "group": 3,
      "title": "Resolve membership, division and account access early",
      "date": "2026-10-30",
      "owner": "MASTER OMEGA",
      "depends": [],
      "steps": [
        "Verify Start membership and Developer Dashboard access using the submitting account; retain private receipts.",
        "Review dated project provenance against the official division definitions; record the selected division and supporting history.",
        "Resolve any funding/support eligibility uncertainty with the organizer before submission. Draft questions separately; do not send automatically."
      ],
      "passCriteria": "Eligibility questions have evidence or a written resolution; unknowns remain blockers.",
      "evidence": "Private membership/access confirmation, provenance note, organizer response if needed.",
      "baseline": "",
      "scope": "submission"
    },
    {
      "id": "S02",
      "group": 3,
      "title": "Audit the chess contest package and asset provenance",
      "date": "2026-10-31",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "S01"
      ],
      "steps": [
        "Inventory code, art, music, fonts and plugins with origins and applicable permissions.",
        "Keep Candy Land in the general product; verify the separate contest APK and promotional materials exclude it and other uncleared content.",
        "Review the final packed-asset report, app metadata and screenshots against the official content requirements."
      ],
      "passCriteria": "Every submitted asset has a documented basis for inclusion; no general-build content leaks into the contest package.",
      "evidence": "Asset ledger, packed report, screenshot/metadata review.",
      "baseline": "0.4.0 excluded Candy Land Resources; repeat audit for the new candidate.",
      "scope": "submission"
    },
    {
      "id": "S03",
      "group": 3,
      "title": "Establish the Competition distribution path",
      "date": "2026-11-02",
      "owner": "MASTER OMEGA + Codex",
      "depends": [
        "S01"
      ],
      "steps": [
        "Verify the existing Meta app ID, package identity and signing continuity; keep keys outside the repository.",
        "Prepare the Competition channel and test upload/access with the intended account.",
        "Document installation steps and any judge access requirements; do not confuse the GitHub download with the channel submission."
      ],
      "passCriteria": "A tester can obtain and launch the intended package through the established contest distribution path.",
      "evidence": "Channel link, successful processing status and independent install record.",
      "baseline": "",
      "scope": "submission"
    },
    {
      "id": "S04",
      "group": 3,
      "title": "Storyboard the handoff demonstration",
      "date": "2026-11-05",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "H04"
      ],
      "steps": [
        "0:00\u20130:15: show a real chess game and the interruption: your opponent leaves.",
        "0:15\u20131:35: show AI takeover, physical response, virtual continuation and return to the real board in the same session.",
        "1:35\u20132:40: show saved resume and a brief explanation of local operation and honest limits; reserve time for a clear ending."
      ],
      "passCriteria": "The distinctive benefit is visible immediately; the planned cut stays below three minutes.",
      "evidence": "Shot list mapped to actual working features; no simulated capability claims.",
      "baseline": "",
      "scope": "submission"
    },
    {
      "id": "S05",
      "group": 3,
      "title": "Record honest gameplay from the accepted candidate",
      "date": "2026-11-14",
      "owner": "MASTER OMEGA",
      "depends": [
        "U08",
        "S02",
        "S04"
      ],
      "steps": [
        "Record headset footage and a synchronized real-board view on the pinned candidate.",
        "Show physical corrections and manual steps where they are part of the experience; avoid cuts that imply automatic recognition.",
        "Edit a concise narrated demonstration with readable captions and balanced audio."
      ],
      "passCriteria": "The video accurately represents the accepted build and its limits; duration is under three minutes.",
      "evidence": "Raw takes, edit file, final video duration and candidate hash.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "S06",
      "group": 3,
      "title": "Prepare the description and judge quick-start",
      "date": "2026-11-14",
      "owner": "Codex + MASTER OMEGA",
      "depends": [
        "S04",
        "U08"
      ],
      "steps": [
        "Write the problem, distinctive handoff benefit, implementation, future plans and target launch date.",
        "Provide an equipment list and a short hands-only test route, including physical-board alignment and manual fallback.",
        "Review claims against acceptance evidence: no general piece scanning, cheat-proof guarantee or network multiplayer claim."
      ],
      "passCriteria": "Every product claim is demonstrated or explicitly labeled as future work; a judge can reproduce the route.",
      "evidence": "Submission draft, screenshots, quick-start and claim-to-evidence review.",
      "baseline": "",
      "scope": "submission"
    },
    {
      "id": "S07",
      "group": 3,
      "title": "Upload and verify the final judging package",
      "date": "2026-11-15",
      "owner": "MASTER OMEGA + Codex",
      "depends": [
        "U08",
        "S02",
        "S03",
        "S05",
        "S06"
      ],
      "steps": [
        "Upload the accepted chess APK to the Competition channel; verify processed version and install it from that channel.",
        "Publish the final video on the required public video platform and check playback signed out.",
        "Retest the core route from the distributed build and verify all submission links from the intended viewer context."
      ],
      "passCriteria": "Distributed APK matches the accepted candidate and every judge-facing link works.",
      "evidence": "Channel version/hash, fresh install result, video URL and access checks.",
      "baseline": "",
      "scope": "build"
    },
    {
      "id": "S08",
      "group": 3,
      "title": "Submit early and preserve the receipt",
      "date": "2026-11-17",
      "owner": "MASTER OMEGA",
      "depends": [
        "H12",
        "U08",
        "S01",
        "S02",
        "S03",
        "S05",
        "S06",
        "S07"
      ],
      "steps": [
        "Complete the Devpost fields with final channel/video links and the evidenced track/division.",
        "Review the submitted preview against the candidate; submit by the internal target of Nov 17, 5 p.m. Central.",
        "Retain confirmation, final field export and links. Keep Nov 18 before 2 p.m. Central for recovery only."
      ],
      "passCriteria": "Submission is actually accepted, with receipt and working links; a filled draft does not count.",
      "evidence": "Timestamped receipt, final submission URL and archive.",
      "baseline": "",
      "scope": "submission"
    },
    {
      "id": "S09",
      "group": 3,
      "title": "Preserve judging access and the submitted version",
      "date": "2026-11-18",
      "owner": "MASTER OMEGA",
      "depends": [
        "S08"
      ],
      "steps": [
        "Recheck receipt and access before the official cutoff; resolve only concrete submission issues.",
        "Retain the exact submitted APK, source commit, checksums, video and evidence archive.",
        "Keep judging access available; do not replace the submitted version after the deadline. Recheck the official announcement schedule."
      ],
      "passCriteria": "The submitted entry remains reproducible and accessible through judging.",
      "evidence": "Final archive and access record; ongoing access is not assumed from a past check.",
      "baseline": "",
      "scope": "submission"
    }
  ],
  "build": {
    "file": "LivingTable-0.6.3-Quest.apk",
    "sourceCommit": "4f0d527705b5cb99a473f19b543d82aec9c851c5",
    "sha256": "b6d68e33ce1d0f9a91a2d6f13911917f5b35c706281e2e40197b44cd36495317",
    "bytes": 84367357,
    "versionCode": 63,
    "version": "0.6.3"
  },
  "contestBuild": {
    "version": "0.4.0",
    "versionCode": 40,
    "package": "com.UnityTechnologies.com.unity.template.urpblank",
    "sourceCommit": "b0308fe9d6ef471456a3fcf256eb63f688b9b3e2",
    "file": "LivingTable-Chess-0.4.0-Quest.apk",
    "bytes": 72780084,
    "sha256": "dd696ee055f0d580b889fea8bdd8dc7620b6c8647bf35005349aeb839e296140",
    "buildUtc": "2026-10-28T19:33:08.2445022Z",
    "installedOnQuest3": true,
    "contestContentAudit": "No excluded Candy Land Resources assets packed",
    "physicalCameraAccuracy": "Not yet validated on a real board"
  },
  "rules": "Gaming requires hands-first operation. Judging weights innovation, experience design, technical implementation, and presentation equally; technical guidance includes 60 fps minimum, MRUK/scene understanding and unfamiliar rooms. Submit an APK through a Competition channel, a public video under three minutes, and the requested description. Confirm membership, division, provenance and content eligibility. Keep access through judging and freeze the submission after the deadline.",
  "sources": [
    {
      "title": "Official contest rules \u00b7 checked Oct 29",
      "url": "https://start-developer-competition-26.devpost.com/rules"
    },
    {
      "title": "Chess 0.4.0 validation and limitations",
      "url": "https://github.com/MistaMischif/Project-26/tree/main/releases/0.4.0-chess-contest"
    },
    {
      "title": "Current general build 0.6.3",
      "url": "https://github.com/MistaMischif/Project-26/releases/tag/v0.6.3-themed-rewards"
    },
    {
      "title": "Session integrity and physical verification limits",
      "url": "https://github.com/MistaMischif/Project-26/blob/main/Assets/LivingTable/SESSIONS.md"
    },
    {
      "title": "Published First Steps 0.1.0 preview and local vision limitations",
      "url": "https://github.com/MistaMischif/Project-26/releases/tag/firststeps-v0.1.2"
    }
  ],
  "firstSteps": {
    "version": "0.1.2",
    "versionCode": 3,
    "applicationId": "com.livingtable.firststeps",
    "sha256": "6e453d9898a8567983b3804a9ec32845ad6033afd024bb9c95ec17a387aa5f81",
    "bytes": 104665107,
    "file": "LivingTable-FirstSteps-0.1.2.apk",
    "built": "2026-10-31",
    "reviewed": "2026-10-31",
    "status": "Installed and launched on Quest 3",
    "publishedVersion": "0.1.2",
    "publishedUrl": "https://github.com/MistaMischif/Project-26/releases/tag/firststeps-v0.1.2",
    "acceptance": "Pending real-board and founder testing"
  }
};
