# Living Table First Steps 0.1.29

The main screen now presents LIVING TABLE: FIRST STEPS by COLTRAN STUDIOS, using the established wood, gold and purple styling. START opens positioning and mandatory four-corner placement. Once ordered corner geometry is valid and corner refinement has finished, the standard 32 pieces appear automatically. Only Menu and PLAY remain. PLAY starts a fresh game as White against AI, with White moving first. Continue and Quick Play launch choices are removed; existing saved files are retained. No official logo bitmap was found; the splash uses the studio name, not a claimed official logo reproduction.

REAL BOARD immediately returns to gameplay at the existing world pose, hides the virtual board surface, and retains all pieces, turn and session history. It does not require alignment or a reconciliation scan. Physical placement is user-managed and recorded as unverified. GO VIRTUAL retains the physical-obstacle safety acknowledgement. MOVED IT still ends a legal physical-mode turn without scanning.

Validation: 116 integrated checks and 80 physical-flow checks passed, including automatic ready state, exactly Menu/PLAY, White first, surface toggle state preservation and hidden virtual square surface. Android build, installation and founder acceptance are recorded separately in artifact.json.

Headset acceptance:
1. Launch: verify the title and START; no Continue Game or Quick Play.
2. Position board, CONTINUE, mark four playable corners. Green geometry should reveal 32 pieces with Menu and PLAY only, without scanning or sizing screens.
3. PLAY: White moves first. Move a piece and select MOVED IT; AI takes its turn without a scan.
4. GO VIRTUAL, acknowledge notice, then REAL BOARD: virtual board surface disappears and gameplay resumes immediately, at the same pose and turn, with no setup prompts.

Known limitations: turn-time camera recognition is disabled by founder direction. A physical board moved independently must be manually realigned via Menu; this switch does not detect movement. Previously observed sleep/wake graphics memory growth remains unresolved. Real headset acceptance of this revision is pending.
