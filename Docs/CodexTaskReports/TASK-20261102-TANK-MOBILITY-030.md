# TASK-20261102-TANK-MOBILITY-030

- Status: implemented on work; Unity build/playtest pending
- Source: TASK-20261101-MOBILITY-ARCHETYPES-017, TASK-20261101-ARCHETYPE-FIRST-018
- Branch: work (main untouched)

## Scope
Introduce a movement-only tank handling prototype without removing the existing eight frames. BASTION is temporarily assigned Tank mobility, although its mesh is still bipedal. Tank movement gets a 1.3x forward travel multiplier, 0.7x lateral and 0.55x reverse multiplier relative to existing directional movement. This is not yet a tank visual, independent turret, or completed three-archetype feature.

## Code commits
- CraftCatalog.cs: `92cf520a1c81c151afd2a71af37ec149995cfda6`
- VoxArena.cs: `c84d4d6b2b31714836f1acc7b5a93fade9a2fb48`

## Pending
- Unity compile, smoke tests, BASTION directional speed and obstacle handling regression.
- Implement distinct lower-body heading and independent upper-body/turret aim.
- Create proper tank visual prototype, quadruped mobility and geometry, and matching tests.

## Design intent / local developer handoff

**Why:** Movement archetypes must feel different before expensive mesh/animation work. BASTION is a reversible stand-in for tank handling, not a declaration that its existing two-legged mesh is a tank.

**Why these changes:** The explicit `MobilityKind` flag separates movement rules from appearance and armor. The directional multipliers are first-pass feel-tuning values, not a final balance contract. Avoid inferring archetype from frame index in future systems; prefer `f.spec.mobility`.

**Preserve:** Eight catalog frames and existing selections; boost fuel/cooldown rules; shared movement collision, step and landing behavior; target lock and projectile firing logic. Do not replace the BASTION mesh until the dedicated visual task is reviewed.

**Local verification:** Build Unity and run existing smoke tests; compare BASTION forward/lateral/reverse travel over equal input intervals, including boost and obstacle contact; verify WISP runner free ground dodge and standard frames unchanged. Test that lock-on still faces targets. If results diverge, record Unity errors, observed input, expected behavior, actual behavior, and source SHA in the same TASK report before fixing.

**Known limitation / next step:** Current tank prototype still uses common `f.facing` for body and weapon, so movement/aim are not independently controlled. Next task must separate locomotion heading and aiming heading with an explicit upper-body pivot and maintain backwards compatibility for StandardBiped/Runner. This is a design handoff, not a claim of completed turret support.
