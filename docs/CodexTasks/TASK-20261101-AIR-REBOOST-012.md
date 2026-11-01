---
id: TASK-20261101-AIR-REBOOST-012
mode: check
status: ready
priority: 7
title: Specify producer-approved repeated midair Space boost activation
---

## Producer decision
Producer explicitly confirms: **while airborne, releasing Space and pressing Space again must restart upward boost as long as boost energy remains**. This is a core input behavior for the weight-dependent tap jump + hold-to-boost ascent design in TASK-20261101-WEIGHT-HOLD-BOOST-JUMP-011. The producer wants meaningful leg-part choices and high-fall landing stagger (TASK-20261101-LEG-LANDING-STAGGER-010).

## Intended behavior
- A grounded Space press triggers a single weight-dependent normal jump.
- Holding Space after the jump enables boost ascent while energy is available; boost consumes energy.
- Releasing Space stops boost thrust; normal momentum/gravity continue.
- **Re-pressing Space while still airborne re-enables boost ascent**, including during descent, using remaining energy. No free new ground-jump impulse in the air.
- On zero energy, no upward boost; avoid infinite flight/hover exploits, key-repeat retriggering, and unbounded energy regeneration in air. Do not arbitrarily prohibit reactivation.
- Boost consumption and altitude should affect fall risk; leg-part shock absorption controls high-fall landing stagger, and ordinary ground jumps must not trigger it.
- Preserve target lock, dash/boost interaction, collisions, roof landing, animation and multiplayer-safe deterministic input handling.

## Dependencies and validation
Read current latest verified game candidate codex/implementation/2026-11-01-sd-ground and report from TASK-20261101-JUMP-IMPLEMENT-009 if available. Resolve the producer's previous 2x apex approval as the normal-jump baseline target, without implementing conflicting fixed-height tuning in parallel. **Do not edit gameplay code in this check**. Produce a precise state machine/input transition plan and implementation task with acceptance tests: tap, hold, release, re-press while ascending, re-press while falling, energy depletion, repeated taps, no extra jump impulse, heavy/light frame differences, safe/unsafe landing, and Windows build/smoke. Record source SHA; do not use stale main silently.

## Safeguards
No changes to main, runner configuration, or producer worktree. Implementation requires a pinned latest candidate source and isolated change branch; publish findings to codex/task-reports.
