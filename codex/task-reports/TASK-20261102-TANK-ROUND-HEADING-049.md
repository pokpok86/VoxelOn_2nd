# TASK-20261102-TANK-ROUND-HEADING-049 result
Implementation status: source changed on work; Unity unverified.

WHY: Spawn facing is assigned after character selection, leaving chassis direction stale across round reset.
DESIGN INTENT: synchronize chassis once at round reset, then preserve independent steering.
CHANGED: Assets/Scripts/VoxArena.cs, commit c29c7fd13c53ec8d5a0cf4e7fcc356c1e0164114. Inserted per-fighter planar heading initialization after both FaceOpponent calls in ResetRound.
INVARIANTS: No movement speed, booster, combat, 120 deg/s turn or rig geometry changes; main untouched.
VALIDATION: Static source inspection only; Unity build, smoke and manual play NOT RUN. Expected initial chassis/facing dot=1 after round reset. Reproduce using source prior to c29c7fd13c53ec8d5a0cf4e7fcc356c1e0164114: select tank, restart round with opponent diagonal, compare chassisForward and facing. Check reverse and 135-degree turn after local build.
LIMITATIONS/NEXT: No separate upper turret transform or moving treads; no actual Unity capture. BEFORE/AFTER geometry unchanged, so no numeric render generated. Verify contact/COM/weapon mount and original concept against same-view Unity screenshots after rig exists.
