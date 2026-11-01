---
id: TASK-20261101-WEIGHT-HOLD-BOOST-JUMP-011
mode: check
status: ready
priority: 8
title: Reconcile approved 2x jump with weight-based tap jump and hold-to-boost ascent
---

## Latest producer direction (supersedes simple fixed jump tuning where conflicting)
- Normal jump apex varies by robot total weight. Lighter builds generally jump higher; heavier builds jump lower.
- Tap Space = normal jump; **hold Space** after jump initiation = consume boost resource to ascend higher while held, with an upper bound much higher than current.
- The risk and reward come from boost spent and leg-dependent high-fall landing stagger; altitude should be a player decision, not a universally tiny hard cap.
- The producer previously approved ~2x jump height relative to old behavior. Treat this as a **target for the revised normal-jump baseline/reference build**, not a mandate to blindly double every impulse while also adding unlimited hold-boost. Resolve ambiguities through measured options and PM report rather than conflicting edits.
- The already queued LEG-LANDING-STAGGER-010 design must be integrated conceptually; impact velocity relative to support surface and leg shock absorption matter.

## Required analysis
1. Inspect actual latest game candidate `codex/implementation/2026-11-01-sd-ground` at reported `b757bcc`, verify full SHA, and check status of `TASK-20261101-JUMP-IMPLEMENT-009`. Do not start from stale `f808e5952fee`.
2. Map current input handling (Space tap/hold), jump impulse, gravity, boost energy and regeneration, robot mass and leg-part attributes, target lock, aerial steering, roof collision and landing. Identify whether weight is an actual stat or needs a new tunable field.
3. Propose deterministic mechanics: edge-triggered tap jump; continuous boost ascent only while held and energy >0; release stops upward thrust but preserves physics momentum; safe energy depletion; no repeated free impulses from key repeat; clear HUD/FX feedback.
4. Provide initial balance table for light/medium/heavy **without hard-coding unapproved values**: normal jump height, boost ascent acceleration, energy consumption, approximate max height, and impact thresholds. Explain how to tune against the producer's prior 2x request.
5. Define regressions: tap jump by mass, held Space climb and energy drain, release, exhaustion, no infinite hovering, jump from roofs, safe landing, high fall stagger by leg type, boost regeneration, target-lock and collision, automated Windows build/player smoke and capture comparisons.
6. Identify smallest safe implementation task(s) that can be queued after confirming runner can pin latest game candidate source. The runner may otherwise silently start at old source, which is a hard blocker. **No source edits in this check**.
7. Report precise file paths, baseline measurements where possible, missing producer balance choices, and dependency on current jump task. Never modify main or producer worktree.

## PM handoff
This specification is the producer's newest gameplay direction. Do not implement a conflicting fixed-height jump as the final solution. Queue implementation only after source-base and dependency reconciliation. Publish findings to codex/task-reports.
