# Living Table: First Steps — first playable slice

Existing Astra-derived Quest chess runtime, with an optional isolated First Steps app profile. General Living Table and Candy Land remain intact.

## Feature contract

1. Align a known physical chess board and establish the current registered position.
2. Existing on-device appearance detection proposes exactly one legal move after hands are clear.
3. The Quest sends only 2–4 cropped affected-square images to your local computer.
4. Meta Llama 3.2 Vision identifies each piece (or empty/uncertain). It is not told the expected move or expected identities.
5. Deterministic chess rules compare every affected square to the proposed legal result, including captures, castling, en passant and promotion.
6. A fresh camera check must still match; any intervening hand movement, session change, pause, or revision invalidates the response.
7. A mismatch, unavailable service or uncertain observation leaves the position unchanged. MANUAL remains an explicitly assisted fallback.

This does not recognize arbitrary starting layouts, infer missing historical castling rights, prove absence of cheating, or automatically reconcile a board after virtual moves. Physical camera accuracy and crop framing require Quest testing. The model runs on the host PC, not the headset.

## Self-host

Requires Node 20+ and a compatible Ollama runtime with Meta Llama 3.2 Vision 11B. Model download is approximately 7.8 GB; development host has an RTX 3080 Ti with 12 GB VRAM. License: Meta Llama 3.2 Community License and Acceptable Use Policy, https://www.llama.com/llama3_2/license/ . No provider API key or paid service is required. Contest acceptance of this particular model remains an entrant rules check.

Ollama 0.40.11 fails to load mllama here. Upstream recommends 0.24.0: https://github.com/ollama/ollama/issues/16490 . Use a separate runtime bound to 127.0.0.1:11435; do not replace the machine's existing service. Download from https://github.com/ollama/ollama/releases/tag/v0.24.0 and reuse the installed model cache.

Set OLLAMA_URL=http://127.0.0.1:11435 and a random LIVING_TABLE_DEVICE_TOKEN, then run `node server.mjs`. Default host is loopback, port 8790. GET /health reports configuration, not model readiness. POST /observe performs actual inference. Camera crops are held in memory and not logged or persisted by the host. Do not expose the older runtime on a public interface.

For USB Quest testing: `adb reverse tcp:8790 tcp:8790`. Put `first-steps-connection.json` in /sdcard/Android/data/com.livingtable.firststeps/files (Unity persistent data directory) with url http://127.0.0.1:8790 and the same deviceToken. Never commit that file. The development APK supports `adb shell run-as com.livingtable.firststeps`. HTTPS endpoints are also supported; cleartext non-loopback endpoints are rejected.

## Checks

`node --test server.test.mjs` exercises HTTP authentication, schema rejection, malformed model output and failure handling using test doubles. `node evaluate.mjs` runs real inference against ten synthetic crop fixtures; these simple 2D fixtures are NOT headset camera accuracy measurements. Unity FirstStepsRelease.Tests runs the chess rules suite plus full/sparse/stale/uncertain observation tests for five move types.

Next acceptance: physical pawn move; wrong destination; captured piece still present; incomplete castle; hand entering during inference; service offline; recovery to manual; pause/resume; human/AI handoff; switch to virtual. Verify no mismatched observation advances the journal. Record real latency and framing failures before calling this seamless.

## Measured development result, October 29

APK build succeeded with zero build errors; installed and launched on the connected Quest. Connection file and USB reverse installed. No Unity script exceptions appeared in the startup check; existing OVR extension warnings remain. This is not a visual or physical-board acceptance pass.

Actual local inference completed five requests / ten synthetic crops. Cold pair: 30.9 seconds. Warm pairs: 5.5–6.0 seconds. Five of ten labels differed from fixture expectations, including a blank square called a rook and occluded crops not returning uncertain. These fixtures are deliberately simple 2D drawings, not board photographs; they expose issues but do not measure physical-set accuracy. The next iteration must improve abstention and latency and test real camera framing. See evaluation.json for exact outputs.
