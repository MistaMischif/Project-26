"""Maintain the contest rundown as data; no completion is inferred from source tests."""
import json
from pathlib import Path
root=Path(__file__).resolve().parent
tasks=[]
def task(id,group,title,date,owner,depends,steps,passed,evidence,baseline='',scope='build'):
    tasks.append(dict(id=id,group=group,title=title,date=date,owner=owner,depends=depends.split(),steps=steps,passCriteria=passed,evidence=evidence,baseline=baseline,scope=scope))
task('H01',1,'Pin the exact build and test environment','2026-10-29','Codex + MASTER OMEGA','',[
'Select the APK to test; record version, SHA-256, source commit, Quest OS, refresh rate and permission state.',
'Photograph the physical board orientation and note piece style, room, lighting, seat and table dimensions.',
'Export existing saves before testing. Use a named disposable session; capture headset video plus a view of the real board.'
],'Every result can be traced to one APK, session and physical setup.','Artifact manifest, device/setup notes, starting session ID.','0.6.3 installed and launched; no real-board handoff acceptance recorded.')
task('H02',1,'Prepare diagnostic evidence and reproducible positions','2026-10-30','Codex','H01',[
'Expose or export position/FEN, side to move, move number, session ID, journal sequence, pending AI move, control mode and surface mode.',
'Prepare legal positions for a normal move, capture, castling, en passant and promotion. Keep imported fixtures visibly labeled.',
'Run chess rules, handoff and session regression suites against the selected source; retain output and fixture IDs.'
],'Before/after state can be compared without relying only on the board animation.','Regression logs, fixture manifest, diagnostic snapshot.','0.4.0 tests cover legality and persistence; repeat on the chosen candidate.')
task('H03',1,'Align and recognize an ordinary human move','2026-10-30','MASTER OMEGA + Codex','H02',[
'Start a tracked physical game in the standard position. Align playing-area corners, excluding the decorative frame.',
'Move e2 to e4; clear hands and wait. Repeat legal moves across the center and all four corners.',
'Inspect journal and recognition feedback after each move; test from natural head positions.'
],'Each legal move commits exactly once; all squares remain aligned. No manual confirmation is silently presented as camera verification.','Video, recognition latency per move, before/after state.','Recognition uses square appearance changes, not general piece identity.')
task('H04',1,'Transfer the opponent from human to AI','2026-10-31','MASTER OMEGA','H03',[
'After a human turn, open HANDOFF and choose AI TAKE OVER; compare position, history and session identity.',
'Wait for the AI instruction; verify the named piece, origin and destination are unambiguous.',
'Move the physical AI piece as instructed and clear hands; confirm one commit before the next turn.'
],'Same game continues; the AI proposal is fixed until correctly executed.','Continuous video plus pre-handoff, pending and committed snapshots.')
task('H05',1,'Reject wrong, extra and ambiguous physical changes','2026-10-31','MASTER OMEGA + Codex','H04',[
'For a pending AI move, try a wrong destination, a different piece, and two changed pieces.',
'Leave the correct piece between squares, cover the origin/destination, then restore the expected layout.',
'Check that recovery names the required action and that the journal never advances during ambiguity.'
],'Zero false commits. Correcting the board resumes the locked move once, without reset or replacement.','One clip and state diff per negative case; bug IDs for every false accept.','Appearance matching can miss identity substitutions; document this limit rather than claim cheat-proof verification.')
task('H06',1,'Verify capture and compound-move reconciliation','2026-11-01','MASTER OMEGA + Codex','H05',[
'Run the capture fixture; deliberately leave the captured piece on the board before removing it.',
'Castle king first and stop, then move the rook; repeat in reverse order. Try en passant without removing the captured pawn.',
'Promote with a missing or incorrect replacement; then correct it. Inspect all required squares and state fields.'
],'Partial actions do not commit. Each complete legal action commits once with correct rights and counters.','Five fixture videos and state diffs: capture, both castle orders, en passant, promotion.')
task('H07',1,'Return control to the human without replacing an AI move','2026-11-01','MASTER OMEGA','H04',[
'Switch HUMAN RETURNS while an AI move is pending; attempt another legal move.',
'Complete the original instruction, then let the human opponent make the following move.',
'Repeat after the AI move has already committed; compare history and active seat.'
],'A pending proposal cannot be rerolled; control changes take effect at a clear boundary.','Pending-move identity and journal comparison across both timings.')
task('H08',1,'Continue the same position virtually','2026-11-02','MASTER OMEGA','H06 H07',[
'From the physical session choose GO VIRTUAL. Compare every piece and side to move.',
'Repeat with a physical AI proposal still pending; confirm virtual execution occurs exactly once.',
'Play three virtual moves, save, relaunch and resume. Compare session, history and legal rights.'
],'No reset, duplicate AI move, lost capture or lost rule state.','Board snapshots, move list and save/reload video.')
task('H09',1,'Reconcile virtual progress back onto the real board','2026-11-03','Codex + MASTER OMEGA','H08',[
'Choose USE REAL BOARD after virtual moves. Compare the guide against the real pieces.',
'Deliberately leave one piece misplaced and attempt to continue; then correct the arrangement.',
'Verify the camera can re-establish a trustworthy baseline. If it cannot, keep MANUAL CHECK explicit, record its provenance and keep the automatic-reconciliation claim blocked.'
],'Automatic reconciliation passes only with measured rejection of wrong layouts. A labeled manual fallback is recorded as a limitation, not an automatic pass.','Wrong-layout and correct-layout videos; baseline invalidation/reacquisition evidence.','Known gap: current return after virtual moves requires MANUAL CHECK.')
task('H10',1,'Recover from tracking, permission and lifecycle interruptions','2026-11-04','MASTER OMEGA + Codex','H09',[
'Cover the camera, move hands out of view and change lighting during a pending move; restore each condition.',
'Sleep/wake and force-close/reopen before and after a move commit, and during a handoff.',
'Deny camera permission, exercise the labeled manual fallback, then restore permission and rescan.'
],'No unsolicited moves, lost state or dead ends. Recovery states what the user should do next.','Interruption matrix with expected/actual behavior, session snapshots and logs.')
task('H11',1,'Import an existing game honestly and protect saved state','2026-11-04','Codex + MASTER OMEGA','H02 H10',[
'Use IMPORT GAME for a real midgame position; explicitly set turn, castling and en-passant state.',
'Compare all 64 squares, then hand control to AI. Verify imported history remains labeled.',
'In disposable test data, simulate a save failure or damaged journal; check refusal/retry without silent restart.'
],'Imported state is exact from import onward; unknown earlier history is never fabricated. Save failure blocks further commits.','Import fixture, visible provenance label, save-failure regression logs.')
task('H12',1,'Close the full handoff acceptance gate','2026-11-05','MASTER OMEGA','H03 H04 H05 H06 H07 H08 H09 H10 H11',[
'Run human → AI → human → virtual → physical → save/relaunch three times without developer intervention.',
'Repeat in a second room with a second board/piece style. Record failures and manual interventions separately.',
'Fix every state-loss, false-commit or dead-end defect; repeat affected scenarios on the new build.'
],'Three clean cycles per setup, zero false commits or lost state. This is our internal release target, not a contest rule.','Six continuous runs, defect closure links, per-build results.')
task('U01',2,'Design and time the first five minutes','2026-10-30','Codex + MASTER OMEGA','H01',[
'Write the first-use sequence: launch, permission explanation, place/align, pick up a piece, legal move, opponent handoff.',
'Keep one primary action per state. Teach the pinch where it is used and explain physical AI piece movement before it is needed.',
'Time a cold launch and note where instructions require coaching.'
],'Proposed internal targets: first legal move within two minutes; handoff payoff within five. No controller needed.','Annotated flow and timed walkthrough; failure points become bugs.')
task('U02',2,'Make pickup and release feel precise','2026-11-02','Codex + MASTER OMEGA','U01',[
'With each hand, pick up every piece type near board edges and neighboring pieces.',
'Verify the piece remains between thumb and forefinger without a scale jump; release on legal and illegal squares.',
'Test fast pinches, hand loss and hand crossing; confirm hover/press feedback and pickup/drop audio.'
],'No wrong-piece selection, unintended move or duplicate sound; invalid release returns clearly and safely.','Twenty pickups per hand, failures counted by type, video of edge cases.')
task('U03',2,'Keep essential UI readable and reachable','2026-11-03','Codex + MASTER OMEGA','U01',[
'Use the board seated at near/far comfortable distances; read turn, move and recovery prompts.',
'Move/anchor the panel and board; inspect overlap, depth sorting, contrast and hand reach.',
'Check a narrower comfortable field of view and audio-off operation; verify color is not the only status cue.'
],'Critical information stays in comfortable view, controls are reachable and prompts remain understandable without sound.','Screenshots from seated viewpoints, tester reach/readability notes.')
task('U04',2,'Validate anchoring and scene understanding in unfamiliar rooms','2026-11-05','Codex + MASTER OMEGA','H03 U03',[
'Inspect actual MRUK/scene-surface integration; package presence alone does not establish use.',
'Test placement, relocalization and board movement on three tables in three rooms, including one not used for development.',
'Restart, look away/back and change seats; measure overlay drift against physical square edges.'
],'No unsafe or visibly wrong placement; drift stays within an agreed fraction of a square (initial target 10%). Unsupported surfaces explain recovery.','Room/table matrix, measured drift, anchor status and scene integration notes.','MRUK 207.0.1 is installed; robustness still requires device evidence.')
task('U05',2,'Measure sustained Quest performance','2026-11-06','Codex + MASTER OMEGA','H08 U02 U04',[
'Capture on-device FPS, CPU/GPU frame times, dropped frames and memory with the final intended refresh rate.',
'Run a 20-minute session spanning recognition, AI thinking, handoff, virtual play and return; include the warm device period.',
'Identify stalls/leaks, optimize the measured bottlenecks and rerun the same route.'
],'At least 60 fps in measured gameplay, with headroom toward selected refresh rate; investigate every sustained dip and stall. No monotonic memory growth.','Raw performance capture, segment timestamps, frame-time summary, thermal/refresh settings.')
task('U06',2,'Test with five people who have not been coached','2026-11-09','MASTER OMEGA','H12 U02 U03 U04 U05',[
'Recruit five testers with mixed chess/XR familiarity; record consent before any filming.',
'Give only the goal: start a game, let AI take over, continue virtually and return. Do not explain the controls.',
'Record first-move time, handoff time, assistance, mistakes, discomfort and whether they want to repeat the experience.'
],'Internal target: all five complete the core route unaided, first move ≤2 min and payoff ≤5 min. Misses produce specific fixes, not adjusted scores.','Five anonymized worksheets with build hash and timestamps; consent kept privately.')
task('U07',2,'Repair and repeat failed usability scenarios','2026-11-12','Codex + MASTER OMEGA','U06',[
'Rank failures: state corruption/false moves/crashes first, blocked tasks second, friction/readability third.',
'Fix the causes and rerun dependent handoff and performance tests; collect fresh observations on the new APK.',
'Use fresh uncoached testers for onboarding changes; prior coaching cannot prove discoverability.'
],'No open critical or blocking defects. Changed behavior has new-build evidence.','Issue list with reproduction, severity, fix commit and retest evidence.')
task('U08',2,'Freeze the candidate and perform a repeat-use check','2026-11-13','MASTER OMEGA + Codex','U07',[
'Select the release candidate and pin its SHA-256; export the complete test log.',
'Resume a saved game the next day and complete a short session entirely with hands.',
'Freeze features; allow only fixes with targeted regression and an explicit new candidate fingerprint.'
],'A returning player can resume confidently; the candidate has a traceable acceptance record.','Candidate manifest, next-day resume recording, signed-off defect list.')
task('S01',3,'Resolve membership, division and account access early','2026-10-30','MASTER OMEGA','',[
'Verify Start membership and Developer Dashboard access using the submitting account; retain private receipts.',
'Review dated project provenance against the official division definitions; record the selected division and supporting history.',
'Resolve any funding/support eligibility uncertainty with the organizer before submission. Draft questions separately; do not send automatically.'
],'Eligibility questions have evidence or a written resolution; unknowns remain blockers.','Private membership/access confirmation, provenance note, organizer response if needed.',scope='submission')
task('S02',3,'Audit the chess contest package and asset provenance','2026-10-31','Codex + MASTER OMEGA','S01',[
'Inventory code, art, music, fonts and plugins with origins and applicable permissions.',
'Keep Candy Land in the general product; verify the separate contest APK and promotional materials exclude it and other uncleared content.',
'Review the final packed-asset report, app metadata and screenshots against the official content requirements.'
],'Every submitted asset has a documented basis for inclusion; no general-build content leaks into the contest package.','Asset ledger, packed report, screenshot/metadata review.','0.4.0 excluded Candy Land Resources; repeat audit for the new candidate.',scope='submission')
task('S03',3,'Establish the Competition distribution path','2026-11-02','MASTER OMEGA + Codex','S01',[
'Verify the existing Meta app ID, package identity and signing continuity; keep keys outside the repository.',
'Prepare the Competition channel and test upload/access with the intended account.',
'Document installation steps and any judge access requirements; do not confuse the GitHub download with the channel submission.'
],'A tester can obtain and launch the intended package through the established contest distribution path.','Channel link, successful processing status and independent install record.',scope='submission')
task('S04',3,'Storyboard the handoff demonstration','2026-11-05','Codex + MASTER OMEGA','H04',[
'0:00–0:15: show a real chess game and the interruption: your opponent leaves.',
'0:15–1:35: show AI takeover, physical response, virtual continuation and return to the real board in the same session.',
'1:35–2:40: show saved resume and a brief explanation of local operation and honest limits; reserve time for a clear ending.'
],'The distinctive benefit is visible immediately; the planned cut stays below three minutes.','Shot list mapped to actual working features; no simulated capability claims.',scope='submission')
task('S05',3,'Record honest gameplay from the accepted candidate','2026-11-14','MASTER OMEGA','U08 S02 S04',[
'Record headset footage and a synchronized real-board view on the pinned candidate.',
'Show physical corrections and manual steps where they are part of the experience; avoid cuts that imply automatic recognition.',
'Edit a concise narrated demonstration with readable captions and balanced audio.'
],'The video accurately represents the accepted build and its limits; duration is under three minutes.','Raw takes, edit file, final video duration and candidate hash.')
task('S06',3,'Prepare the description and judge quick-start','2026-11-14','Codex + MASTER OMEGA','S04 U08',[
'Write the problem, distinctive handoff benefit, implementation, future plans and target launch date.',
'Provide an equipment list and a short hands-only test route, including physical-board alignment and manual fallback.',
'Review claims against acceptance evidence: no general piece scanning, cheat-proof guarantee or network multiplayer claim.'
],'Every product claim is demonstrated or explicitly labeled as future work; a judge can reproduce the route.','Submission draft, screenshots, quick-start and claim-to-evidence review.',scope='submission')
task('S07',3,'Upload and verify the final judging package','2026-11-15','MASTER OMEGA + Codex','U08 S02 S03 S05 S06',[
'Upload the accepted chess APK to the Competition channel; verify processed version and install it from that channel.',
'Publish the final video on the required public video platform and check playback signed out.',
'Retest the core route from the distributed build and verify all submission links from the intended viewer context.'
],'Distributed APK matches the accepted candidate and every judge-facing link works.','Channel version/hash, fresh install result, video URL and access checks.')
task('S08',3,'Submit early and preserve the receipt','2026-11-17','MASTER OMEGA','H12 U08 S01 S02 S03 S05 S06 S07',[
'Complete the Devpost fields with final channel/video links and the evidenced track/division.',
'Review the submitted preview against the candidate; submit by the internal target of Nov 17, 5 p.m. Central.',
'Retain confirmation, final field export and links. Keep Nov 18 before 2 p.m. Central for recovery only.'
],'Submission is actually accepted, with receipt and working links; a filled draft does not count.','Timestamped receipt, final submission URL and archive.',scope='submission')
task('S09',3,'Preserve judging access and the submitted version','2026-11-18','MASTER OMEGA','S08',[
'Recheck receipt and access before the official cutoff; resolve only concrete submission issues.',
'Retain the exact submitted APK, source commit, checksums, video and evidence archive.',
'Keep judging access available; do not replace the submitted version after the deadline. Recheck the official announcement schedule.'
],'The submitted entry remains reproducible and accessible through judging.','Final archive and access record; ongoing access is not assumed from a past check.',scope='submission')
plan=dict(schema=1,revision='2026-10-29',title='Living Table · Chess contest rundown',deadline='2026-11-18T14:00:00-06:00',target='2026-11-17T17:00:00-06:00',
    groups=[dict(id=1,title='Prove the handoff',why='The differentiator is one game continuing across people and surfaces.',gate='Zero false commits, lost state or unexplained recovery.',window='Oct 29–Nov 5'),dict(id=2,title='Make it effortless',why='Judges must experience the benefit without developer coaching.',gate='Five uncoached testers, comfortable hands-first use, measured performance.',window='Oct 30–Nov 13'),dict(id=3,title='Deliver the contender',why='A working feature needs an accessible build and a truthful, memorable demonstration.',gate='Verified judging package and submission receipt.',window='Oct 30–Nov 18')],tasks=tasks,
    build=json.loads((root.parent/'releases/0.6.3-themed-rewards/artifact.json').read_text()),
    contestBuild=json.loads((root.parent/'releases/0.4.0-chess-contest/artifact.json').read_text()),
    rules='Gaming requires hands-first operation. Judging weights innovation, experience design, technical implementation, and presentation equally; technical guidance includes 60 fps minimum, MRUK/scene understanding and unfamiliar rooms. Submit an APK through a Competition channel, a public video under three minutes, and the requested description. Confirm membership, division, provenance and content eligibility. Keep access through judging and freeze the submission after the deadline.',
    sources=[dict(title='Official contest rules · checked Oct 29',url='https://start-developer-competition-26.devpost.com/rules'),dict(title='Chess 0.4.0 validation and limitations',url='https://github.com/MistaMischif/Project-26/tree/main/releases/0.4.0-chess-contest'),dict(title='Current general build 0.6.3',url='https://github.com/MistaMischif/Project-26/releases/tag/v0.6.3-themed-rewards'),dict(title='Session integrity and physical verification limits',url='https://github.com/MistaMischif/Project-26/blob/main/Assets/LivingTable/SESSIONS.md')])
# Latest verified build snapshot; do not infer headset passes from automated checks.
plan['revision']='2026-10-31'
plan['firstSteps']=json.loads((root/'latest-build.json').read_text())
updates={
'H01':'First Steps 0.1.4 is the current chess handoff test candidate. Use its exact published fingerprint and record device/environment; never inherit older headset passes.',
'H02':'0.1.3 reruns 33 full-board observation gate cases, 20 move observation cases and chess rules; prior 0.1.2 scene/handoff checks remain historical. Four new host test groups pass. Camera accuracy is not established by synthetic observations.',
'H03':'First Steps checks the starting physical position before watching legal moves. Local visibility-gated Qwen observations are checked against saved state. 10 fixture cases and 18 related variants pass; all real piece types and camera framing still need testing.',
'H04':'Scene regression passed: switching to AI retains session and position. Real-board execution and clear prompts still require headset acceptance.',
'H05':'Incomplete, duplicate, uncertain, mismatched and stale observations remain blocked. Covered pawn fixtures now abstain correctly. Wrong-layout and wrong-identity rejection must still be tested with a real camera.',
'H07':'Scene regression passed: human return retains an already proposed AI move rather than rerolling it.',
'H08':'Scene regression passed: the locked AI move executes exactly once after virtual transfer and remains consumed after relaunch.',
'H09':'0.1.3 retains full 64-square reconciliation and fresh-frame/hand guards. New local recognizer matched 64 repeated synthetic empty/pawn crops in 30.7 seconds. This is still too slow to call seamless; real-board accuracy and total headset latency remain open. Manual fallback is explicitly assisted.',
'H10':'Scene tests reject recovery during pause, open handoff menus or stale revisions. Manual-to-camera switching now requires verification; hardware interruption tests remain open.',
'U02':'Includes opposite-rim two-hand movement and yaw without resizing. Release both pinches before re-grabbing. Actual hand tracking remains to test.',
'U03':'MOVE BOARD hides board and pieces during placement. Full-board scan reports progress, expected square contents and SCAN BOARD retry. Headset readability remains to test.',
'U04':'Session/history and prior pause state survive relocation. Physical mode requires realignment and verification. Anchor persistence and unfamiliar rooms remain unverified.'}
for t in plan['tasks']:
    if t['id'] in updates: t['baseline']=updates[t['id']]
    if t['id']=='H03': t['steps'].append('Test every piece type in both colors, center and edge squares, two lighting conditions and partial occlusion. Record cold/warm latency and every false accept. Synthetic pawn-only results do not complete this check.')
    if t['id']=='H09': t['steps'].append('Let the full 64-square scan run. Measure total latency; try an extra piece, wrong-color piece and an interruption. Confirm every failure leaves the journal unchanged. MANUAL CHECK must enter manual mode; USE CAMERA must require a new scan.')
    if t['id']=='U02': t['steps'].append('Pinch opposite outer rims with both hands; translate and rotate. Release one hand, then both, and re-grab. Confirm no accidental piece moves or resizing.')
    if t['id']=='U03': t['steps'].append('After checkmate or draw, confirm the bold 4x result, no move prompts, MENU and one-press PLAY AGAIN. Confirm a fresh session with the same board mode/opponent/color and that a second press cannot reset it.')
    if t['id']=='U03': t['steps'].append('Choose MOVE BOARD: board, pieces and board UI must disappear until placement. Place again; verify position, history and prior pause state are unchanged.')
    if t['id']=='U04': t['steps'].append('After physical-board relocation, confirm realignment is required. Test tracking loss and failed anchor persistence without losing the game.')
plan['sources'].append(dict(title='Latest First Steps test build and local vision limitations',url=plan['firstSteps']['publishedUrl']))
(root/'dist/contest-plan.js').write_text('window.CONTEST_PLAN = '+json.dumps(plan,indent=2)+';\n',encoding='utf-8')
print(f'{len(tasks)} detailed checks generated')
