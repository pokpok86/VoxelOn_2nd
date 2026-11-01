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
