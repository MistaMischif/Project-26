window.LT_PLAN = {
  "schema": 1,
  "reviewedOn": "2026-10-25",
  "sourceUrl": "https://muse.ai/library/artifacts/progress-dashboard",
  "sourceChecked": 7,
  "sourceTotal": 63,
  "sourceLastLog": "2026-10-05",
  "target": "2026-11-17T17:00:00-06:00",
  "deadline": "2026-11-18T14:00:00-06:00",
  "officialUrl": "https://start-developer-competition-26.devpost.com/",
  "rulesUrl": "https://start-developer-competition-26.devpost.com/rules",
  "capacity": "Proposed Living Table priority: 4 hours Monday\u2013Saturday; 2 hours Sunday. Time boxes are estimates, not a completion guarantee. Capacity choice remains unconfirmed.",
  "conflict": "The previously read Muse contest plan allocates Oct 26\u2013Nov 4 to Meta Global AI and Nov 5\u201315 to the Unity/MegaJam work. This recovery schedule cannot also fit inside those exclusive blocks. It requires a dedicated Living Table allocation or an explicit scope/priority change. These are prior planning allocations, not newly verified contest deadlines.",
  "phases": [
    {
      "id": "P0",
      "title": "Foundation & lockdown",
      "originalDates": "Oct 1\u20135",
      "status": "Incomplete / evidence gap",
      "original": "Muse marks 7 of 13 foundation tasks complete and logs setup on Oct 5. Application submitted is not approval. The design freeze and anchor-persistence exit test were still open.",
      "current": "Unity 6.0 (6000.3.9f1), Meta XR 207, a configured Git remote, and a working APK exist. No MRUK package was found in the current manifest/lockfile; the code uses OVRSpatialAnchor. Today\u2019s new Unity source folders are untracked.",
      "next": "Confirm eligibility, freeze the rules and physical-piece workflow, commit and back up the current source, and validate the on-device foundation."
    },
    {
      "id": "P1",
      "title": "Table & hands",
      "originalDates": "Oct 6\u201314",
      "status": "Partially implemented / unverified",
      "original": "Original gate: drift below 2 cm at 5 minutes; hand-release move registration; hover/preview; two seated hands-only runs.",
      "current": "Grid, fingertip pinch selection, anchor save/restore, and repositioning are coded. The current gesture is pinch-to-select then pinch-to-move virtual pieces. No physical-piece release binding, drift measurements, per-attempt accuracy log, hover preview, or fiducial path is verified.",
      "next": "Measure anchoring on two tables and implement/test the chosen physical-piece confirmation flow. Do not treat virtual movement as physical-piece tracking."
    },
    {
      "id": "P2",
      "title": "The living layer",
      "originalDates": "Oct 15\u201324",
      "status": "Visual foundation only",
      "original": "Original gate: animation, captures, score, spatial audio, beatable AI, and a complete round a newcomer can follow.",
      "current": "Jump animations, landing pulse, selection color, move counter, and breathing frame exist. A move counter is not score or round state. Rules, turns, captures, victory, spatial audio, and AI are absent from the delivered game runtime.",
      "next": "Build one complete ruleset and round first, then AI and concise audio/visual feedback."
    },
    {
      "id": "P3",
      "title": "Game feel & session",
      "originalDates": "Oct 25\u201331",
      "status": "Not reached",
      "original": "Original gate: 5\u201310 minute rounds, launch under 30 seconds, pause/removal recovery, hands-only onboarding, pass-and-play.",
      "current": "No complete game round exists to time. Reset and layout switching exist; they do not satisfy pause/resume, onboarding, or pass-and-play.",
      "next": "Reach a legal start-to-finish round before tuning pacing. Recover session state and add the handoff flow."
    },
    {
      "id": "P4",
      "title": "Polish & performance",
      "originalDates": "Nov 1\u20139",
      "status": "Not verified",
      "original": "Original gate: sustained 72 fps, comfort/accessibility controls, clean install, frozen candidate.",
      "current": "APK compilation and an update install succeeded. There is no recorded FPS profile, fresh-install play session, or comfort/accessibility acceptance. 978 build warnings remain untriaged.",
      "next": "Profile actual Quest sessions, close reliability defects, add readable/seated controls, and freeze by Nov 9."
    },
    {
      "id": "P5",
      "title": "Playtest",
      "originalDates": "Nov 10\u201314",
      "status": "Not started in available evidence",
      "original": "Original gate: at least five external testers; ranked issue fixes; two retests proving self-service.",
      "current": "No external test reports or completed current-build headset checks have been recorded in the project chat.",
      "next": "Recruit ahead of time, test five people without coaching, fix issues by severity, and retest at least two."
    },
    {
      "id": "P6",
      "title": "Submission",
      "originalDates": "Nov 15\u201317",
      "status": "Packaging foundation only",
      "original": "Original gate: tested candidate, trailer, screenshots, full entry; submit Nov 16\u201317.",
      "current": "A local APK is available and installed. No Competition channel invite, public demo video, final entry, or submission confirmation has been verified.",
      "next": "Prepare judge access through the Competition channel, finish media and entry, submit Nov 17, and retain Nov 18 as emergency time."
    }
  ],
  "days": [
    {
      "date": "2026-10-25",
      "title": "Back up today\u2019s implementation",
      "phase": "P0 / P1",
      "light": true,
      "buffer": false,
      "gate": "Baseline observations and a recoverable source backup exist.",
      "tasks": [
        {
          "id": "2026-10-25-1",
          "title": "Run the installed-build baseline",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 45,
          "output": "Record the eight existing headset checks with observations. Rank the top three failures; do not claim they passed merely because the app installed.",
          "depends": []
        },
        {
          "id": "2026-10-25-2",
          "title": "Collect the original project decisions",
          "owner": "Jay \u00b7 project records",
          "minutes": 30,
          "output": "Locate the design-lockdown document and current Start/Devpost status. Record missing decisions and confirm daily capacity.",
          "depends": []
        },
        {
          "id": "2026-10-25-3",
          "title": "Back up today\u2019s implementation",
          "owner": "Codex + Jay \u00b7 Git",
          "minutes": 45,
          "output": "Review untracked LivingTable/MRBoardMVP source, stage only intended project files, commit, and verify the private remote backup. Save the APK hash; close with done/blocked/next.",
          "depends": []
        }
      ]
    },
    {
      "date": "2026-10-26",
      "title": "Lock the physical-piece interaction contract",
      "phase": "P0",
      "light": false,
      "buffer": false,
      "gate": "Rules and interaction contract are frozen; eligibility gaps have owners.",
      "tasks": [
        {
          "id": "2026-10-26-1",
          "title": "Freeze one ruleset and the win condition",
          "owner": "Jay + Codex \u00b7 design",
          "minutes": 90,
          "output": "One agreed rules page: legal moves, turns, captures, score/result, setup, round end, and 5\u201310 minute target. Reuse the original design if it exists.",
          "depends": [
            "2026-10-25-2"
          ]
        },
        {
          "id": "2026-10-26-2",
          "title": "Verify eligibility and submission setup",
          "owner": "Jay \u00b7 Meta / Devpost",
          "minutes": 45,
          "output": "Record Start membership status, entry link, team/track/division choice, and Competition-channel access. Resolve the original disclosure-review gate before any public assets.",
          "depends": []
        },
        {
          "id": "2026-10-26-3",
          "title": "Lock the physical-piece interaction contract",
          "owner": "Jay + Codex \u00b7 Unity",
          "minutes": 105,
          "output": "Choose and document hand-release-to-cell confirmation and how physical pieces map to virtual IDs. Mark the optional fiducial as a fallback; do not claim camera recognition.",
          "depends": [
            "2026-10-26-1"
          ]
        }
      ]
    },
    {
      "date": "2026-10-27",
      "title": "Retest recovery and choose alignment fallback",
      "phase": "P1",
      "light": false,
      "buffer": false,
      "gate": "Drift and recovery are measured; failing anchoring blocks the device gate.",
      "tasks": [
        {
          "id": "2026-10-27-1",
          "title": "Fix placement / restore failures first",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Rebuild and install a candidate with the worst baseline anchoring defects fixed. Preserve calm retry and reposition paths.",
          "depends": [
            "2026-10-25-1",
            "2026-10-26-3"
          ]
        },
        {
          "id": "2026-10-27-2",
          "title": "Measure 20 placements on two tables",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 90,
          "output": "Record table, lighting, placement, drift at 5 minutes, and restore outcome. Fail the stability gate if drift exceeds 2 cm; two consecutive failures trigger fallback evaluation.",
          "depends": [
            "2026-10-27-1"
          ]
        },
        {
          "id": "2026-10-27-3",
          "title": "Retest recovery and choose alignment fallback",
          "owner": "Jay + Codex \u00b7 Quest / Unity",
          "minutes": 60,
          "output": "Verify save-failure retry and relaunch; document whether marker support must be implemented. A required fallback replaces cosmetic work, not test time.",
          "depends": [
            "2026-10-27-2"
          ]
        }
      ]
    },
    {
      "date": "2026-10-28",
      "title": "Add candidate-square preview and seated retest",
      "phase": "P1",
      "light": false,
      "buffer": false,
      "gate": "Original P1 behavior is evidenced; a virtual-only demo is not enough.",
      "tasks": [
        {
          "id": "2026-10-28-1",
          "title": "Bind hand release to the agreed move path",
          "owner": "Codex \u00b7 Unity",
          "minutes": 120,
          "output": "Map a physical-piece grab/release to piece ID and candidate cell, with explicit confirmation/correction and single-event behavior.",
          "depends": [
            "2026-10-26-3",
            "2026-10-27-3"
          ]
        },
        {
          "id": "2026-10-28-2",
          "title": "Log move accuracy and difficult conditions",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 60,
          "output": "Run ten attempts per hand, then occlusion/fast-move/low-light trials. Record intended vs registered cells. If 2+ of 10 fail, fix forgiveness and repeat.",
          "depends": [
            "2026-10-28-1"
          ]
        },
        {
          "id": "2026-10-28-3",
          "title": "Add candidate-square preview and seated retest",
          "owner": "Codex + Jay \u00b7 Unity / Quest",
          "minutes": 60,
          "output": "Implement hover/preview distinct from selection. Complete two seated hands-only ten-move drills with physical pieces, recording errors.",
          "depends": [
            "2026-10-28-2"
          ]
        }
      ]
    },
    {
      "date": "2026-10-29",
      "title": "Verify complete local rounds",
      "phase": "P2",
      "light": false,
      "buffer": false,
      "gate": "A complete rules-driven game exists, not just freely movable pieces.",
      "tasks": [
        {
          "id": "2026-10-29-1",
          "title": "Implement turns and legal moves",
          "owner": "Codex \u00b7 Unity",
          "minutes": 120,
          "output": "Rule validation and turn ownership enforce the frozen rules; illegal moves cannot advance the game.",
          "depends": [
            "2026-10-26-1"
          ]
        },
        {
          "id": "2026-10-29-2",
          "title": "Implement capture, scoring, and round end",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Captures update occupancy and visuals; result/score state has deterministic win/lose/draw handling and restart.",
          "depends": [
            "2026-10-29-1"
          ]
        },
        {
          "id": "2026-10-29-3",
          "title": "Verify complete local rounds",
          "owner": "Jay + Codex \u00b7 tests / Quest",
          "minutes": 30,
          "output": "Record at least two start-to-finish rounds and targeted edge-case tests. Fix state bugs before effects.",
          "depends": [
            "2026-10-29-2",
            "2026-10-28-3"
          ]
        }
      ]
    },
    {
      "date": "2026-10-30",
      "title": "Play and log two vs-AI rounds",
      "phase": "P2",
      "light": false,
      "buffer": false,
      "gate": "Two complete vs-AI rounds finish without a state failure.",
      "tasks": [
        {
          "id": "2026-10-30-1",
          "title": "Implement the simplest legal AI",
          "owner": "Codex \u00b7 Unity",
          "minutes": 120,
          "output": "AI selects legal actions, takes its turn, and stops at the round result without locking input.",
          "depends": [
            "2026-10-29-3"
          ]
        },
        {
          "id": "2026-10-30-2",
          "title": "Tune first-session difficulty",
          "owner": "Jay + Codex \u00b7 Quest",
          "minutes": 60,
          "output": "Document beginner wins/losses and obvious exploits; adjust for learnability rather than adding complex search.",
          "depends": [
            "2026-10-30-1"
          ]
        },
        {
          "id": "2026-10-30-3",
          "title": "Play and log two vs-AI rounds",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 60,
          "output": "Record round time, result, confusion, and any state/turn bug; both rounds finish hands-only.",
          "depends": [
            "2026-10-30-2"
          ]
        }
      ]
    },
    {
      "date": "2026-10-31",
      "title": "Test the whole living layer",
      "phase": "P2",
      "light": false,
      "buffer": false,
      "gate": "P2 is demonstrated in complete rounds, not isolated animation clips.",
      "tasks": [
        {
          "id": "2026-10-31-1",
          "title": "Add clear capture and turn feedback",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Use the existing jump/pulse; add a readable capture effect under 1.2 seconds and an unambiguous turn indicator.",
          "depends": [
            "2026-10-29-3"
          ]
        },
        {
          "id": "2026-10-31-2",
          "title": "Add spatial interaction audio",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Grabs, moves, captures, and a restrained ambient bed have usable levels, correct positioning, and recorded asset licenses.",
          "depends": [
            "2026-10-31-1"
          ]
        },
        {
          "id": "2026-10-31-3",
          "title": "Test the whole living layer",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 60,
          "output": "Play two complete rounds; remove distracting effects and ensure a newcomer can identify whose turn it is and why a capture happened.",
          "depends": [
            "2026-10-30-3",
            "2026-10-31-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-01",
      "title": "Retest and close the day",
      "phase": "P2 / P3",
      "light": true,
      "buffer": false,
      "gate": "The worst feel issue is measurably improved; no new feature scope.",
      "tasks": [
        {
          "id": "2026-11-01-1",
          "title": "Rank the three largest feel problems",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 30,
          "output": "List observed friction by frequency and severity from complete rounds.",
          "depends": [
            "2026-10-31-3"
          ]
        },
        {
          "id": "2026-11-01-2",
          "title": "Fix the highest-impact feel issue",
          "owner": "Codex \u00b7 Unity",
          "minutes": 60,
          "output": "One targeted fix; remove optional visual work if reliability is behind.",
          "depends": [
            "2026-11-01-1"
          ]
        },
        {
          "id": "2026-11-01-3",
          "title": "Retest and close the day",
          "owner": "Jay \u00b7 Quest / records",
          "minutes": 30,
          "output": "Repeat the same scenario, record before/after behavior, and check any pending Start membership status.",
          "depends": [
            "2026-11-01-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-02",
      "title": "Build hands-only onboarding",
      "phase": "P3",
      "light": false,
      "buffer": false,
      "gate": "A new user can begin a legal round with hands only.",
      "tasks": [
        {
          "id": "2026-11-02-1",
          "title": "Add concise round-state UI",
          "owner": "Codex \u00b7 Unity",
          "minutes": 60,
          "output": "Readable score/result and turn state appear above the table; retain the move counter only as a separate debug metric.",
          "depends": [
            "2026-10-29-3"
          ]
        },
        {
          "id": "2026-11-02-2",
          "title": "Polish idle life only if core gates pass",
          "owner": "Codex \u00b7 Unity",
          "minutes": 60,
          "output": "Use restrained board-edge/idle feedback. Spend this block on overdue P1/P2 defects if those gates remain open.",
          "depends": [
            "2026-10-31-3"
          ]
        },
        {
          "id": "2026-11-02-3",
          "title": "Build hands-only onboarding",
          "owner": "Codex + Jay \u00b7 Unity / Quest",
          "minutes": 120,
          "output": "A short show-and-do sequence teaches placement, selection, movement, and reset; verify without verbal coaching.",
          "depends": [
            "2026-11-02-1"
          ]
        }
      ]
    },
    {
      "date": "2026-11-03",
      "title": "Time five complete rounds",
      "phase": "P3",
      "light": false,
      "buffer": false,
      "gate": "Launch, removal/resume, and round duration meet the original session targets.",
      "tasks": [
        {
          "id": "2026-11-03-1",
          "title": "Implement pause and headset-removal recovery",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Round, turn, score, and piece state survive headset removal and resume without duplicate actions.",
          "depends": [
            "2026-10-29-3"
          ]
        },
        {
          "id": "2026-11-03-2",
          "title": "Measure and improve cold start",
          "owner": "Jay + Codex \u00b7 Quest",
          "minutes": 60,
          "output": "Time five launches to the usable hand-tracked table; record each result and target under 30 seconds.",
          "depends": [
            "2026-11-02-3"
          ]
        },
        {
          "id": "2026-11-03-3",
          "title": "Time five complete rounds",
          "owner": "Jay + Codex \u00b7 Quest",
          "minutes": 90,
          "output": "Measure complete rounds and tune toward 5\u201310 minutes without hiding hard rules or resetting state.",
          "depends": [
            "2026-10-30-3",
            "2026-11-03-1"
          ]
        }
      ]
    },
    {
      "date": "2026-11-04",
      "title": "Fix blockers and preserve a candidate",
      "phase": "P3",
      "light": false,
      "buffer": false,
      "gate": "One complete controller-free end-to-end session passes.",
      "tasks": [
        {
          "id": "2026-11-04-1",
          "title": "Build player handoff and seat recalibration",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Clear player ownership, handoff prompt, and recalibration when table-side position changes.",
          "depends": [
            "2026-10-29-3",
            "2026-11-03-1"
          ]
        },
        {
          "id": "2026-11-04-2",
          "title": "Run the full hands-only soak",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 90,
          "output": "Cold start \u2192 onboarding \u2192 AI round \u2192 pass-and-play round \u2192 pause/resume. Record each transition and failure.",
          "depends": [
            "2026-11-04-1",
            "2026-11-03-3"
          ]
        },
        {
          "id": "2026-11-04-3",
          "title": "Fix blockers and preserve a candidate",
          "owner": "Codex \u00b7 Unity / Git",
          "minutes": 60,
          "output": "Resolve critical session bugs, rebuild, install, archive the hash and source commit, and write the P3 gate evidence.",
          "depends": [
            "2026-11-04-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-05",
      "title": "Triage build warnings",
      "phase": "P4",
      "light": false,
      "buffer": false,
      "gate": "A performance trace exists and optimization is evidence-led.",
      "tasks": [
        {
          "id": "2026-11-05-1",
          "title": "Profile worst-case Quest frame time",
          "owner": "Jay + Codex \u00b7 profiler",
          "minutes": 120,
          "output": "Capture CPU/GPU behavior during idle effects, capture, audio, and AI; name the top five costs.",
          "depends": [
            "2026-11-04-3"
          ]
        },
        {
          "id": "2026-11-05-2",
          "title": "Remove the largest measured cost",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Optimize the measured bottleneck; target sustained 72 fps (about 13.9 ms frame budget).",
          "depends": [
            "2026-11-05-1"
          ]
        },
        {
          "id": "2026-11-05-3",
          "title": "Triage build warnings",
          "owner": "Codex \u00b7 build logs",
          "minutes": 30,
          "output": "Group the 978 baseline warnings; separate unused-package warnings from player-facing risks and assign unresolved issues.",
          "depends": []
        }
      ]
    },
    {
      "date": "2026-11-06",
      "title": "Validate controls on device",
      "phase": "P4",
      "light": false,
      "buffer": false,
      "gate": "Performance and accessible seated interaction are demonstrated.",
      "tasks": [
        {
          "id": "2026-11-06-1",
          "title": "Repeat worst-case profiling",
          "owner": "Jay + Codex \u00b7 Quest",
          "minutes": 120,
          "output": "Verify sustained 72 fps over a representative session; reduce effects if the target fails.",
          "depends": [
            "2026-11-05-2"
          ]
        },
        {
          "id": "2026-11-06-2",
          "title": "Implement comfort and accessibility controls",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Effect-intensity and volume controls, appropriate vignette option, non-color piece cues, key-audio labels, and seated defaults.",
          "depends": [
            "2026-11-02-3"
          ]
        },
        {
          "id": "2026-11-06-3",
          "title": "Validate controls on device",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 30,
          "output": "Record which controls work, whether state persists as intended, and any readability or reach problem.",
          "depends": [
            "2026-11-06-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-07",
      "title": "Fix clean-install and remaining high-severity issues",
      "phase": "P4 / P5",
      "light": false,
      "buffer": false,
      "gate": "Fresh-install behavior is tested and playtest sessions are booked.",
      "tasks": [
        {
          "id": "2026-11-07-1",
          "title": "Run a clean-install session",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 60,
          "output": "Back up needed data first; obtain confirmation before uninstalling. Verify onboarding, placement, a full round, and restart from a genuinely clean state. An update install is not this test.",
          "depends": [
            "2026-11-06-3"
          ]
        },
        {
          "id": "2026-11-07-2",
          "title": "Arrange five external testers",
          "owner": "Jay \u00b7 scheduling",
          "minutes": 60,
          "output": "Secure five board-game players for Nov 10\u201311 and two retests for Nov 13. Confirm availability and prepare the observation sheet.",
          "depends": []
        },
        {
          "id": "2026-11-07-3",
          "title": "Fix clean-install and remaining high-severity issues",
          "owner": "Codex \u00b7 Unity",
          "minutes": 120,
          "output": "Resolve crash, inaccessible-control, and state-corruption defects before cosmetic polish.",
          "depends": [
            "2026-11-07-1"
          ]
        }
      ]
    },
    {
      "date": "2026-11-08",
      "title": "Prepare freeze notes and test kit",
      "phase": "P4",
      "light": true,
      "buffer": false,
      "gate": "A clear freeze decision is ready; no feature wish list is added.",
      "tasks": [
        {
          "id": "2026-11-08-1",
          "title": "Run an uncoached candidate session",
          "owner": "Jay / helper \u00b7 Quest",
          "minutes": 60,
          "output": "Observe first touch through full round without explaining the controls.",
          "depends": [
            "2026-11-07-3"
          ]
        },
        {
          "id": "2026-11-08-2",
          "title": "Choose final cuts",
          "owner": "Jay + Codex \u00b7 scope",
          "minutes": 30,
          "output": "Remove rough optional effects. Keep hands-only use, legal complete rounds, recovery, and judge access as mandatory. Record any proposed original-scope reduction explicitly.",
          "depends": [
            "2026-11-08-1"
          ]
        },
        {
          "id": "2026-11-08-3",
          "title": "Prepare freeze notes and test kit",
          "owner": "Codex + Jay \u00b7 records",
          "minutes": 30,
          "output": "List remaining blockers, candidate commit/hash, table setup, observation form, and tester instructions.",
          "depends": [
            "2026-11-08-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-09",
      "title": "Confirm testers can obtain the build",
      "phase": "P4 / P6",
      "light": false,
      "buffer": false,
      "gate": "Feature freeze and usable external distribution are proven.",
      "tasks": [
        {
          "id": "2026-11-09-1",
          "title": "Freeze the feature candidate",
          "owner": "Codex + Jay \u00b7 Git / build",
          "minutes": 60,
          "output": "Tag the reviewed source and APK. Any unresolved critical defect blocks freeze; later work is bug fixes only.",
          "depends": [
            "2026-11-08-3"
          ]
        },
        {
          "id": "2026-11-09-2",
          "title": "Create the Competition release-channel path",
          "owner": "Jay \u00b7 Meta Developer Dashboard",
          "minutes": 90,
          "output": "Configure the Competition channel, upload the candidate, and generate an invite link for testing. Do not confuse this with ADB installation.",
          "depends": [
            "2026-11-09-1",
            "2026-10-26-2"
          ]
        },
        {
          "id": "2026-11-09-3",
          "title": "Confirm testers can obtain the build",
          "owner": "Jay \u00b7 Quest / testing",
          "minutes": 90,
          "output": "Verify access and launch using the distribution path; prepare a clean, repeatable test setup for five testers.",
          "depends": [
            "2026-11-09-2",
            "2026-11-07-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-10",
      "title": "Publish a tested bug-fix candidate",
      "phase": "P5",
      "light": false,
      "buffer": false,
      "gate": "Two real external sessions and traceable issue records exist.",
      "tasks": [
        {
          "id": "2026-11-10-1",
          "title": "Run two uncoached sessions",
          "owner": "Jay + testers \u00b7 Quest",
          "minutes": 120,
          "output": "Log onboarding, anchoring, incorrect moves, round completion, duration, comfort, and any help requested.",
          "depends": [
            "2026-11-09-3"
          ]
        },
        {
          "id": "2026-11-10-2",
          "title": "Fix the highest-severity observed blockers",
          "owner": "Codex \u00b7 Unity",
          "minutes": 90,
          "output": "Fix blockers only; retain reproduction steps and map fixes to issues.",
          "depends": [
            "2026-11-10-1"
          ]
        },
        {
          "id": "2026-11-10-3",
          "title": "Publish a tested bug-fix candidate",
          "owner": "Jay + Codex \u00b7 build / channel",
          "minutes": 30,
          "output": "Run regression checks, record the new hash and change list, and keep testers on the intended candidate.",
          "depends": [
            "2026-11-10-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-11",
      "title": "Assign fixes and retests",
      "phase": "P5",
      "light": false,
      "buffer": false,
      "gate": "Five testers completed sessions; the fix list is evidence-based.",
      "tasks": [
        {
          "id": "2026-11-11-1",
          "title": "Test three additional players",
          "owner": "Jay + testers \u00b7 Quest",
          "minutes": 150,
          "output": "Reach at least five unique external testers; use the same observation sheet and no coaching.",
          "depends": [
            "2026-11-10-3"
          ]
        },
        {
          "id": "2026-11-11-2",
          "title": "Rank the top ten issues",
          "owner": "Jay + Codex \u00b7 issue log",
          "minutes": 60,
          "output": "Rank by frequency \u00d7 severity; mark crashes, inaccessible controls, and state loss as blockers.",
          "depends": [
            "2026-11-11-1"
          ]
        },
        {
          "id": "2026-11-11-3",
          "title": "Assign fixes and retests",
          "owner": "Codex + Jay \u00b7 plan",
          "minutes": 30,
          "output": "Each issue has an owner, reproduction, acceptance condition, and target candidate.",
          "depends": [
            "2026-11-11-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-12",
      "title": "Run targeted regression on Quest",
      "phase": "P5",
      "light": false,
      "buffer": false,
      "gate": "Critical playtest issues are fixed and verified; no new features.",
      "tasks": [
        {
          "id": "2026-11-12-1",
          "title": "Resolve the ranked issues",
          "owner": "Codex \u00b7 Unity",
          "minutes": 180,
          "output": "Work from the top-ten list in severity order. If ten cannot be closed, list remaining items and cut optional scope; never relabel them done.",
          "depends": [
            "2026-11-11-3"
          ]
        },
        {
          "id": "2026-11-12-2",
          "title": "Run targeted regression on Quest",
          "owner": "Jay + Codex \u00b7 Quest",
          "minutes": 60,
          "output": "Repeat each changed interaction plus a complete round. Record the candidate hash and failures.",
          "depends": [
            "2026-11-12-1"
          ]
        }
      ]
    },
    {
      "date": "2026-11-13",
      "title": "Close or reopen issues with evidence",
      "phase": "P5",
      "light": false,
      "buffer": false,
      "gate": "The stranger test passes twice without critical regression.",
      "tasks": [
        {
          "id": "2026-11-13-1",
          "title": "Run two independent retests",
          "owner": "Jay + testers \u00b7 Quest",
          "minutes": 120,
          "output": "Confirm fixes with at least two players; they must self-serve from launch through a full round.",
          "depends": [
            "2026-11-12-2"
          ]
        },
        {
          "id": "2026-11-13-2",
          "title": "Recheck performance and session recovery",
          "owner": "Jay + Codex \u00b7 Quest",
          "minutes": 90,
          "output": "Repeat worst-case profiling, pause/resume, handoff, and saved-anchor restoration on the same candidate.",
          "depends": [
            "2026-11-13-1"
          ]
        },
        {
          "id": "2026-11-13-3",
          "title": "Close or reopen issues with evidence",
          "owner": "Jay + Codex \u00b7 log",
          "minutes": 30,
          "output": "Attach observations; a failed retest reopens the issue and prevents a green gate.",
          "depends": [
            "2026-11-13-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-14",
      "title": "Draft entry and asset checklist",
      "phase": "P5 / P6",
      "light": false,
      "buffer": false,
      "gate": "The candidate and all submission claims refer to the same tested build.",
      "tasks": [
        {
          "id": "2026-11-14-1",
          "title": "Run final complete regression",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 90,
          "output": "Validate the hands-only loop, two tables, restore, AI, handoff, cold start, and performance.",
          "depends": [
            "2026-11-13-3"
          ]
        },
        {
          "id": "2026-11-14-2",
          "title": "Archive source and final APK",
          "owner": "Codex \u00b7 Git / builds",
          "minutes": 60,
          "output": "Record commit, APK SHA-256, version, test results, known issues, and backups. Keep final build identity consistent across materials.",
          "depends": [
            "2026-11-14-1"
          ]
        },
        {
          "id": "2026-11-14-3",
          "title": "Draft entry and asset checklist",
          "owner": "Jay + Codex \u00b7 submission",
          "minutes": 90,
          "output": "Draft title/tagline, description, track/division evidence, credits, and the planned screenshots/video. Reconfirm eligibility and original disclosure gate.",
          "depends": [
            "2026-11-14-2",
            "2026-10-26-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-15",
      "title": "Verify capture and access setup",
      "phase": "P6",
      "light": true,
      "buffer": false,
      "gate": "Capture is ready and the final candidate still works.",
      "tasks": [
        {
          "id": "2026-11-15-1",
          "title": "Fresh-install and play the final candidate",
          "owner": "Jay \u00b7 Quest 3",
          "minutes": 45,
          "output": "Use a backed-up, approved clean-install procedure and complete a seated hands-only round.",
          "depends": [
            "2026-11-14-2"
          ]
        },
        {
          "id": "2026-11-15-2",
          "title": "Storyboard the short demo",
          "owner": "Jay + Codex \u00b7 media plan",
          "minutes": 45,
          "output": "Use the original 60\u201390 second target, which fits the official under-three-minute limit. Show actual physical-piece behavior only if implemented and tested.",
          "depends": [
            "2026-11-15-1"
          ]
        },
        {
          "id": "2026-11-15-3",
          "title": "Verify capture and access setup",
          "owner": "Jay \u00b7 Quest / channel",
          "minutes": 30,
          "output": "Check recording, audio, camera framing, channel invitation, and the media shot list.",
          "depends": [
            "2026-11-15-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-16",
      "title": "Verify judge access to the final build",
      "phase": "P6",
      "light": false,
      "buffer": false,
      "gate": "Build access, video, screenshots, and form are ready for review.",
      "tasks": [
        {
          "id": "2026-11-16-1",
          "title": "Capture and edit the demo",
          "owner": "Jay + Codex \u00b7 media",
          "minutes": 120,
          "output": "Produce the 60\u201390 second demo, verify playback/audio on a second device, and prepare its public link only after the disclosure gate is resolved.",
          "depends": [
            "2026-11-15-3"
          ]
        },
        {
          "id": "2026-11-16-2",
          "title": "Finish screenshots and entry text",
          "owner": "Jay + Codex \u00b7 Devpost draft",
          "minutes": 60,
          "output": "Create 4\u20136 screenshots from the tested build and complete accurate entry text, team/track/division fields, and credits.",
          "depends": [
            "2026-11-14-3"
          ]
        },
        {
          "id": "2026-11-16-3",
          "title": "Verify judge access to the final build",
          "owner": "Jay \u00b7 Meta / Quest",
          "minutes": 60,
          "output": "Upload the intended final APK to Competition and test the invite URL. Check the public video URL and keep access available for judging.",
          "depends": [
            "2026-11-16-1",
            "2026-11-16-2"
          ]
        }
      ]
    },
    {
      "date": "2026-11-17",
      "title": "Reserve same-day correction time",
      "phase": "P6",
      "light": false,
      "buffer": false,
      "gate": "Submission and judge access are confirmed before the hard deadline.",
      "tasks": [
        {
          "id": "2026-11-17-1",
          "title": "Run the final checklist",
          "owner": "Jay + Codex \u00b7 submission review",
          "minutes": 60,
          "output": "Confirm hands-first operation, eligibility, accurate permissions/privacy, credits, media links, and exact build identity.",
          "depends": [
            "2026-11-16-3"
          ]
        },
        {
          "id": "2026-11-17-2",
          "title": "Submit the completed entry",
          "owner": "Jay \u00b7 Devpost",
          "minutes": 90,
          "output": "Submit by the internal 5 PM Central target, with Competition invite and working demo link. Save the receipt and rendered-entry URL.",
          "depends": [
            "2026-11-17-1"
          ]
        },
        {
          "id": "2026-11-17-3",
          "title": "Verify the submitted entry and archive proof",
          "owner": "Jay \u00b7 independent browser / records",
          "minutes": 60,
          "output": "Inspect the rendered entry and all links, retain submission confirmation, and archive the exact submitted candidate.",
          "depends": [
            "2026-11-17-2"
          ]
        },
        {
          "id": "2026-11-17-4",
          "title": "Reserve same-day correction time",
          "owner": "Jay + Codex",
          "minutes": 30,
          "output": "Correct any access or form defect while the entry period is still open; do not start another feature.",
          "depends": [
            "2026-11-17-3"
          ]
        }
      ]
    },
    {
      "date": "2026-11-18",
      "title": "Check receipt and availability before the hard close",
      "phase": "P6",
      "light": false,
      "buffer": true,
      "gate": "Hard close: 2 PM America/Chicago. This is not a planned build day.",
      "tasks": [
        {
          "id": "2026-11-18-1",
          "title": "Check receipt and availability before the hard close",
          "owner": "Jay \u00b7 submission records",
          "minutes": 60,
          "output": "Resolve only emergency access/submission defects before 2 PM Central. After the deadline, preserve the submitted state and keep judge access available.",
          "depends": [
            "2026-11-17-3"
          ]
        }
      ]
    }
  ]
};
