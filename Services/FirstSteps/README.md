# Living Table: First Steps — first playable slice

## Recognition reliability update — November 4, host 0.4.3 / app 0.1.14

The current recognition path requires visibility/classification agreement for empty squares as well as occupied squares. `/ready/vision` checks the recognition model independently of companion readiness. The existing authorized login/recovery supervisor remains supported. App 0.1.14 preserves crop aspect ratio, rejects inadequate crops and requires fresh stable camera frames before layout review or move approval. See `releases/firststeps-0.1.14/README.md` in the repository for the acceptance procedure and limitations. Historical measurements below describe their named revisions, not today's accuracy.

Existing Astra-derived Quest chess runtime, with an optional isolated First Steps app profile. General Living Table and Candy Land remain intact.

## Feature contract

1. Align a known physical chess board and establish the current registered position.
2. Existing on-device appearance detection proposes exactly one legal move after hands are clear.
3. The Quest sends only 2–4 cropped affected-square images to your local computer.
4. A local vision model first checks visibility, then identifies a clear piece. Empty surfaces skip classification; obscured or inconsistent observations become uncertain. It is not told the expected move or expected identities.
5. Deterministic chess rules compare every affected square to the proposed legal result, including captures, castling, en passant and promotion.
6. A fresh camera check must still match; any intervening hand movement, session change, pause, or revision invalidates the response.
7. A mismatch, unavailable service or uncertain observation leaves the position unchanged. MANUAL remains an explicitly assisted fallback.

This does not recognize arbitrary starting layouts, infer missing historical castling rights, prove absence of cheating, or prove physical-set recognition accuracy. First Steps 0.1.2 adds a guarded full-board reconciliation path after virtual moves; it is not yet validated on a real board. Physical camera accuracy and crop framing require Quest testing. The model runs on the host PC, not the headset.

## Self-host

Requires Node 22+ and Ollama with the multimodal `qwen3.5:latest` model (about 6.6 GB). This revision uses the already-installed model and Ollama 0.40.13 on the development RTX 3080 Ti (12 GB). No download, paid API or external provider was added. Model information: https://ollama.com/library/qwen3.5 . This Qwen recognition path is not evidence of Meta/Llama hackathon qualification.

Start an isolated local Ollama runtime with `OLLAMA_HOST=127.0.0.1:11436` and your existing `OLLAMA_MODELS` directory, then run `ollama serve`. Keep it loopback-only. Set `OLLAMA_URL=http://127.0.0.1:11436`, `FIRST_STEPS_MODEL=qwen3.5:latest` and a random `LIVING_TABLE_DEVICE_TOKEN`, then run `node server.mjs`. `Start-FirstSteps.ps1` configures the authenticated host and optional USB bridge; it does not start Ollama. Existing service processes must be stopped explicitly before loading changed host source. No startup service is installed.

Default host is loopback, port 8790. GET /health reports configuration, not model readiness. POST /observe performs actual inference. Camera crops are held in memory and not logged, persisted or cached across frames. Client disconnection cancels provider work. Only one request runs at a time. The model is kept resident for 30 minutes after a request (override FIRST_STEPS_KEEP_ALIVE); first use after unloading remains slower.

Other compatible local models remain configurable. The previous Llama 3.2 Vision 11B setup used isolated Ollama 0.24.0 on port 11435 because newer mllama loading failed here. Its measured recognition failures are retained below; do not treat it as equally verified. The default model has changed, not the authoritative chess rules or observation protocol.

For USB Quest testing: `adb reverse tcp:8790 tcp:8790`. Put `first-steps-connection.json` in /sdcard/Android/data/com.livingtable.firststeps/files (Unity persistent data directory) with url http://127.0.0.1:8790 and the same deviceToken. Never commit that file. The development APK supports `adb shell run-as com.livingtable.firststeps`. HTTPS endpoints are also supported; cleartext non-loopback endpoints are rejected.

## Checks

`node --test server.test.mjs` exercises HTTP authentication, schema rejection, malformed model output and failure handling using test doubles. `node evaluate.mjs` runs real inference against ten synthetic crop fixtures; these simple 2D fixtures are NOT headset camera accuracy measurements. Unity FirstStepsRelease.Tests runs the chess rules suite plus full/sparse/stale/uncertain observation tests for five move types.

Next acceptance: physical pawn move; wrong destination; captured piece still present; incomplete castle; hand entering during inference; service offline; recovery to manual; pause/resume; human/AI handoff; switch to virtual. Verify no mismatched observation advances the journal. Record real latency and framing failures before calling this seamless.

## Measured development result, October 29

APK build succeeded with zero build errors; installed and launched on the connected Quest. Connection file and USB reverse installed. No Unity script exceptions appeared in the startup check; existing OVR extension warnings remain. This is not a visual or physical-board acceptance pass.

Actual local inference completed five requests / ten synthetic crops. Cold pair: 30.9 seconds. Warm pairs: 5.5–6.0 seconds. Five of ten labels differed from fixture expectations, including a blank square called a rook and occluded crops not returning uncertain. These fixtures are deliberately simple 2D drawings, not board photographs; they expose issues but do not measure physical-set accuracy. The next iteration must improve abstention and latency and test real camera framing. See evaluation.json for exact outputs.

## 0.1.2 reconciliation

The host protocol is unchanged. The Quest requests batches of four cells and never supplies the saved answer to the model. Each batch must match exactly and all 64 squares must pass. A mismatch or interruption leaves play locked and permits an explicit retry. Full scans remain slow (see the current measurement below); do not describe this as frictionless or real-camera-validated. First Steps manual checks stay in manual mode; switching back to camera requires reconciliation. The general app is separate.

## 0.1.3 recognition iteration — October 31

Host 0.2.0 uses two independent stages: visibility, then piece type/color only when clear. Unknown output, visibility/type disagreement and provider failures leave the move unverified. The headset uses neutral local-vision wording. The 64-square reconciliation and stale-frame/hand guards remain intact.

- Existing fixture suite: 10/10 labels matched (six unique images, with duplicate cases). Warm two-square requests: 0.35–1.06 seconds; first pair in this run: 5.1 seconds.
- Related brightness/rotation/JPEG variants: 18/18 matched; two-square requests 0.46–1.53 seconds. These are transformations of the same six sources, not independent camera photographs.
- 64-crop synthetic workload: 64/64 matched in 30.731 seconds, including a 9.765-second outlier while Unity was building. Inputs repeat empty squares and pawns; this is throughput, not full chess-set accuracy.
- Direct cold-model probe took 48.3 seconds, including 30.4 seconds of loading. Keeping the model resident reduces repeated load cost, but cold start is still a gap.
- A faster multi-image visibility experiment reintroduced covered-piece misreads and was rejected. Production retains per-image checks.

Run `node --test server.test.mjs`, `node evaluate.mjs`, `node evaluate-holdout.mjs`, and `node evaluate-64.mjs` from this directory with the model available. `make-holdout.py` requires Pillow to regenerate variants. Evaluation scripts use ephemeral authenticated hosts, not live device credentials. Historical `evaluation.json` is preserved; new output is separate.

These results cover empty surfaces, white/black pawns and covered pawns only. Knights, bishops, rooks, queens, kings, real sets, lighting, camera framing and total headset handoff latency remain unverified. The next hardware run must reject wrong identities and occlusions without advancing the journal, then measure correct moves and a complete return scan. Do not label this seamless or founder-accepted from synthetic tests.

## 0.1.5 / host 0.3.0: Meta chess companion

ASK THE TABLE opens three button-driven questions: WHAT HAPPENED, EXPLAIN MOVE and HELP CONTINUE. This release uses text responses, not speech recognition or freeform voice conversation. The local Meta `llama3.2:3b` model explains authoritative engine facts; the existing Qwen recognizer remains separate and explicitly identified.

The engine supplies current position, legal moves, a bounded-search suggestion (or the locked AI move), last move, turn, mode, check status and continuation instructions. The host rejects malformed output, unknown fact references and substituted/illegal suggested move codes. The client binds every answer to session, position and revision. Replies never execute moves or change history. Generated prose can still be imperfect: schema/citation checks do not prove semantic correctness. Verified next-step/last-move text and the engine move are shown separately.

On Windows, install Node 22+ and Ollama, then double-click Prepare-Local-AI.cmd. It starts an isolated loopback runtime if needed, downloads missing local model weights (roughly 2 GB Meta plus 6.6 GB Qwen), warms Meta, starts this authenticated host, and pairs USB when adb is available. It does not install a startup service or scheduled task. First-time downloads require internet; later inference stays on your computer. Keep the headset connected by USB for this test. Android platform-tools must be installed if the established Unity adb path is absent. The host ZIP contains no credentials or model weights; consult the respective model licenses before redistribution.

`Launch-FirstSteps-Test.ps1` without PrepareModels refuses missing models instead of downloading. Optional ModelDirectory reuses an existing cache. A host running from another folder must be closed explicitly; the launcher will not kill an unrelated process. The project now uses a PowerShell 5-compatible random token generator.

Run `node --test server.test.mjs companion.test.mjs` and `node evaluate-companion.mjs`. Three actual local Meta fixture responses were checked, followed by a live Unity client response and stale-revision rejection. This is development evidence, not tournament-strength coaching or headset acceptance. A cold model load remains slower than warm responses.

## Host 0.3.1 / app 0.1.6

Authenticated GET /ready checks the runtime and installed Meta model. The app reloads pairing on retry and reports missing pairing, lost USB transport, missing model/runtime and busy scans separately. The companion has a 90-second server budget / 100-second client budget for a cold model. This does not guarantee a fast cold response.

USB reconnection and Unity shutdown may remove adb reverse forwarding. Rerun Prepare-Local-AI.cmd afterward, then CHECK CONNECTION in game. No background reconnect job or startup service is installed. Chess opponent search is on-device and independent of Meta. Camera verification still needs the host; use the explicitly labeled manual fallback when it is unavailable.

## Host 0.4.0 / app 0.1.7: wireless HTTPS

Prepare-Local-AI now configures a local HTTPS endpoint with OpenSSL and pins its certificate into the private headset pairing file. Git for Windows supplies the default OpenSSL path. TCP 8791 must be reachable on the local network; only token-authenticated inference is allowed. The loopback 8790 path remains for compatibility. Pair once over USB, then use Wi-Fi with the host powered on. Regenerate/re-pair if changing the host identity; rerun pairing when its address changes. Never distribute private-tls or first-steps-connection.json. The release contains neither. Unplugging USB does not stop the host or remove HTTPS. Network/host failures remain real availability limits; on-device chess search remains independent.

## Host 0.4.1 recovery

The launcher remembers an explicitly selected `-ModelDirectory` in ignored `runtime-config.json`, so restarts reuse the same weights instead of searching an empty default cache. The model drive must remain connected. Paired HTTPS headsets do not need ADB reverse or a USB connection when restarting the host. New headsets still need initial pairing. No scheduled job, startup entry or service is created. Keep the PC awake and the launcher host running on the same reachable Wi-Fi.

Meta companion output now requires at least one fact ID chosen from the supplied authoritative facts in its JSON generation schema. Existing answer validation still rejects unknown facts and substituted moves. This fixes observed missing-citation rejection without inventing a fallback answer.

## Automatic availability — host 0.4.2
See releases/firststeps-host-0.4.2/README.md for founder-authorized login startup, crash recovery, active-headset sleep prevention, installation and rollback. Pairing, certificates and model directories remain private and are not included in the downloadable host package.
