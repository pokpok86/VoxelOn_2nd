# TASK-036 — TARANTULA variable-height jump

## User rule
Jump height for robot 3 (TARANTULA) depends on how long Space is held. Short tap = low hop; sustained hold = taller jump. No air booster.

## work commits
- 24a3d29d9dc83695b25f26c7e2e7d1ce850c9292: CraftSpec opt-in variableJumpHeight, jumpHoldWindow=.38s and heldGravityFactor=.24 for TARANTULA CreateFourFormPrototypes preset; jumpImpulseScale=1.0. Other frames unaffected.
- a2aaf94c9f566fb280b9de548a2cea5e1e31249b: track per-fighter jumpHoldElapsed, reset at jump start/round, use reduced upward gravity only while Space remains held and timer window remains. Releasing consumes remaining hold window. Existing jumpMaxHeight cap applies.

## Known limitations / verification
- Four-form TARANTULA preset is still data-only; not selectable in the live eight-frame roster yet. This mechanic is coded but not directly accessible via current selection screen.
- Unity compile/playtest not run. Verify short 0.05s, medium 0.2s, long >=0.38s presses yield increasing apex; releasing early cannot re-enable reduced gravity; no fuel consumption; no midair booster; height cap honored; other 8 frames unchanged.
- Check first physics frame, variable jump on landing/reset, and AI jumpHeld input. No main changes.
