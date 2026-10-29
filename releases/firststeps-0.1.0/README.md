# Living Table First Steps 0.1.0 — development preview

This extends the existing Astra-derived Living Table chess build. The separate Android package is com.livingtable.firststeps; it does not replace the general Living Table app or its Candy Land option.

The physical move path is registered camera change -> one legal candidate -> Meta piece observations on affected squares -> exact expected-board comparison -> fresh camera check -> journaled move. Unknown, incomplete, stale and mismatched observations do not commit moves. Manual play remains explicitly assisted. Virtual chess and existing session handoffs remain available.

Local vision: Meta Llama 3.2 Vision 11B, using a self-hosted Ollama 0.24.0 instance and Node host. The installed Ollama 0.40.11 cannot load this model; do not overwrite it. Setup, limitations and fixture commands are in Services/FirstSteps/README.md. No paid API is required. Keep USB attached for the default loopback bridge.

Validation so far: 20 observation-gate cases plus the existing chess rules regression suite; HTTP authentication, input/output validation and unavailable-provider handling. Synthetic 2D fixture tests are separate from real-camera acceptance. Physical accuracy, latency, framing and usability require MASTER OMEGA's headset test.

## First headset run

1. Start the local runtime and First Steps host; keep USB forwarding active.
2. Open Living Table First Steps, select the physical board option, and align the real chess board in its standard starting position.
3. Let the baseline settle. Move e2 to e4 and lift your hand away. Watch the Meta checking message; the journal must advance only once after agreement.
4. Follow the AI's move instruction exactly. An incorrect destination or an incomplete capture must not advance the game.
5. Move your hand through the board during a check: stale model output must not commit. Stop the host and confirm MANUAL remains available and clearly labeled.

Dashboard: https://contest-command-jz.mistamischif.chatgpt.site/#hack

This preview is not a contest submission or a claim of arbitrary-board recognition. Return-to-physical reconciliation and importing a preexisting game's hidden state retain the current manual limitations.

Development measurements: five real local requests completed across ten synthetic crops. Warm two-square requests were 5.5–6.0 seconds; the cold request was 30.9 seconds. Five crop labels mismatched the expected fixtures, including a blank square hallucination and missed abstentions on occlusion. Recognition tuning remains open; this is a development test build, not a validated automatic-board product.

Final packaging: Unity build succeeded, then the generated Gradle project was repackaged after correcting the Meta SDK network-policy override. The source postprocessor now runs after the Meta SDK. aapt2 inspection of the final APK confirms cleartext blocked by default and allowed only for 127.0.0.1 / localhost. Gradle assembleDebug succeeded. Development APK is installed separately from the general Living Table app.
