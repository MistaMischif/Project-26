# Living Table 0.1.28 — Piece size is the final setup step

Accepting chess-piece size now starts gameplay immediately. New Game and Continue retain their direct flow. Quick Play performs its initial physical-position capture, correction, turn review and virtual-space notice before sizing. PLAY on the size screen imports the reviewed position directly, without another scan, alignment pass or setup screen. Captured origin saves and experiments remain intact.

Green MOVED IT now commits the legal move immediately with user-confirmed provenance; turn-time occupancy/board scanning is disabled. AI physical placement also ends with MOVED IT and returns the turn immediately. Initial capture and explicit physical-return reconciliation remain separate from gameplay. No camera accuracy or sleep/wake fix is claimed.

Verification: 108 integrated checks and 80 physical-flow checks passed. Android build and headset installation pending.

Headset test: accept four corners, complete any initial Quick Play capture/review, size the king and press PLAY. The game must open immediately with the selected size and saved/reviewed position. Repeat with New Game and Continue. There must be no scan or alignment screen after sizing.

Known issue: sleep/wake graphics-memory growth remains unresolved.
