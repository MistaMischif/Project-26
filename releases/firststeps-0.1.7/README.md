# First Steps 0.1.7 - automatic checker fit, piece proportions and wireless Meta

ALIGN BOARD now scans the visible checker pattern on-device. It fits position, yaw and square pitch using the placed table plane and camera perspective, checks a second frame, then offers ACCEPT BOARD and ROTATE 90. It does not recover absolute table height from one camera image: place the table at the real board surface first. Manual corners remain available if the pattern is occluded or low contrast.

After accepting the board, keep the pieces still and move your head at least 4 cm sideways. Two camera views estimate heights and widths for the expected pieces. Consistent piece/color profiles preview as ghost pieces; APPLY SIZES saves them with the session, KEEP SIZES retains prior proportions. This is conservative silhouette estimation, not exact 3D reconstruction or proof of piece identity. Uncertain profiles are not replaced. Physical layout verification remains a separate gate.

YOUR TURN / OPPONENT TURN stays large and bold above instructions. Physical AI proposals retain OPPONENT TURN until the move is committed. Two-human mode names the active color; pause, alignment and game-over states have contextual headings.

Meta and camera inference use paired HTTPS on the local network, with an exact certificate pin and the existing device token. USB is used for initial pairing/install only. The host computer must remain powered, its model runtime available, and Quest must reach it over Wi-Fi. Unplugging USB does not remove this network path. Chess opponent search remains on the Quest even if the host is unavailable. The app rechecks connectivity while running and retries busy requests; no startup service or scheduled job is installed.

Setup: Node 22+, Ollama and OpenSSL (Git for Windows includes it). Run Prepare-Local-AI.cmd with Quest connected once. Configure-Wireless.ps1 chooses the routed local IPv4 address; use -Address explicitly when multiple networks exist. Permit inbound Node TCP 8791 on the private local network. Pairing/certificate/private key files are private and excluded from this release. If the host IP changes, rerun pairing. No paid service or internet inference dependency.

Headset acceptance: place at real board height; ALIGN BOARD; compare every corner before ACCEPT BOARD; move head sideways for piece sizing; compare ghosts before APPLY SIZES; play both seats and inspect turn headings. Unplug USB and ask Meta HELP CONTINUE. Do not infer real-board recognition accuracy from synthetic tests.
