# VoxelOn — Engine Architecture Draft v0.1
Date: 2026-10-30. Status: design proposal, not implemented. Goal: evolve existing playable prototype without breaking it.

## Product contract
- Offline first: human vs AI, AI vs AI balance runs. Later online 1v1, 2v2, 3v3; no same-PC split-screen requirement.
- 4 hardpoints: left/right hand and left/right shoulder. Two-handed items reserve both hands. Arm types may have no hand hardpoints. Empty slots save mass.
- Main equipped weapons do not automatically drop, explode off, or get thrown. Environmental objects are temporary carried items; future unarmed builds.
- Space held: thrust upward while main boost permits; release: immediate downward motion (arcade rule). Actual world-space elevation, collision, hurtboxes, targeting must match visuals.
- Double-tap movement is named High-Speed Movement; leg part determines distance, speed, steering, eligibility, leg-specific boost capacity/cost/recharge. Main vertical booster is a separate resource.
- Legs include biped, hover, tracked, multi-leg/spider. Spider climbs eligible low obstacles but jumps very little. Weight affects falling, thrust, movement, and shield carrying. Mass changes immediately on pickup/drop/throw.
- Hand-held physical shields have position/orientation-based interception, durability and breakage; beam barriers are time-limited and vulnerable to appropriate EMP effects. Shield-mounted weapons can fire while guarding where item capabilities allow. Heavy shields reduce mobility by mass, not blanket prohibitions.
- Tactical equipment: jammers degrade homing, EMP temporarily suppresses specified electronic defenses, decoys alter tracking. Avoid universal hard counters; counterplay and cooldowns.
- Environmental items: flat debris as temporary shield, long objects as melee, small items throwable; crouch with free usable hand near a compatible object attempts pickup. World items have mass, shape, durability, pickup requirements.
- Matchups emerge from part traits, terrain and equipment, not hard-coded win/loss tables.

## Current repository audit (partial)
- VoxArena.cs (partial MonoBehaviour): Fighter state, player/enemy fields, input, movement, jump, dash, damage, camera and match lifecycle.
- VoxArena.AI.cs: EnemyAI directly references player and enemy, including spectator mode's swap workaround.
- VoxArena.Combat.cs: Attack/Special, shots, beams, effects and collisions. Several paths use Opponent(owner) so only one enemy is considered. Shot simulation and GameObject visuals are coupled.
- VoxArena.World.cs: collision and obstacles; MoveWithObstacles currently steps X/Z; needs 3D movement and support/terrain queries.
- VoxArena.UI.cs: HUD/lock reticle and menus; reticle currently combines root position with jumpHeight.
- CraftCatalog.cs: fixed CraftSpec presets; needs part-level composition rather than one aggregate spec.
- TeamId and IsHostile were added as an initial foundation, not multi-combatant support.
- Windows build scene setup currently requests 1280x720 Windowed.
- This audit is source review only: no Unity compilation or playtesting performed.

## Target boundaries
1. **Definitions**: immutable part/item/ability data with stable IDs, mount requirements, mass, stats, tags, cooldowns, resistances. Unity ScriptableObjects or serializable definitions at the authoring boundary; no rules keyed to item names.
2. **Loadout validation**: hardpoints and multi-slot reservations, limb compatibility, total mass, capabilities and hand occupancy. Distinguish permanent equipped loadout from transient world-item grip.
3. **Simulation state**: fighter ID, team ID, transform, velocity, grounded/support state, health, boost pools, status effects, mounts, held item, cooldowns. No Transform/GameObject as authoritative state.
4. **Commands**: one per-fighter input contract (move, aim, jump/thrust, high-speed movement, guard, fire by action/slot, interact/crouch). Player, AI and eventual network sources emit commands; do not special-case f==player for physics.
5. **Systems**: movement/terrain, energy, collision/hit resolution, weapon/ability execution, statuses, world interactions, team/match rules. Use data-driven capabilities (e.g., Climb, Guard, ProjectileHoming, Jammer, EMP, Throw), with explicit constraints.
6. **Presentation**: Unity views and audio/VFX/UI observe simulation; render-only randomness cannot alter gameplay. Separate simulation clock from render delta.
7. **Modes/network**: one authoritative simulation for offline/AI and future server. Clients send commands; server validates movement, hits, pickups and destruction. Prediction/interpolation later; fixed tick does not by itself guarantee deterministic cross-platform replay.
8. **Testing**: headless batch matches with seeded gameplay RNG, fixed tick, reproducible loadouts, side swaps, CSV/JSON metrics, timeout, win rates and performance budgets.

## Suggested interfaces (conceptual, not code API)
- PartDefinition { id, slotTypes, mass, modifiers, capabilities }
- Loadout { head, core, arms, legs, leftHand, rightHand, leftShoulder, rightShoulder }
- AbilityDefinition { triggers, costs, conditions, effects, cooldown }
- FighterState { id, teamId, pose, velocity, hp, mainBoost, legBoost, effects, equipment, heldWorldItem }
- FighterCommand { movement, aim, thrustHeld, highSpeedMove, guardHeld, fireActions, interact }
- WorldItemState { id, shapeClass, mass, durability, gripRequirements, capabilities }
- HitQuery { origin, trajectory, damageKind, sourceId }; resolve interceptors before body.
- BattleStep(state, commands, fixedDelta) -> state + events; render events separately.

## Safe incremental migration plan
0. **Baseline**: capture build/controls behavior; Unity compile and smoke tests; keep current playable prototype.
1. **Simulation identifiers and fighter registry**: remove hardcoded opponent assumptions behind a registry/query API; preserve 1v1 gameplay.
2. **Command abstraction**: player and AI generate the same command; combat consumes command; no gameplay logic tied to player identity.
3. **Real 3D movement**: root position and hit geometry rise together, proper grounded state, 3D projectiles/cover; Space-release immediately initiates descent; mass-based fall rate; preserve lock-on.
4. **Independent boost resources and leg locomotion strategy**: configurable per-leg high-speed movement, main vertical thrust, mass effects, compatibility with hover/tracks/climb later.
5. **Loadout/hardpoints and capability execution**: four mounts, two-handed reservations, shield and compound item action channels, no implicit equipped weapon drop.
6. **Defenses and statuses**: shield intercept geometry/durability, barriers, jammer, EMP, homing behavior.
7. **World interactions**: predefined pickup props, carried mass, crouch/interact, guard/swing/throw; then destructible props.
8. **Balance runner**: 100+ AI-vs-AI trials, reports and regression checks.
9. **Network integration**: server-authoritative multiplayer, replication, prediction and validation; start with 1v1 before team sizes.

## Migration acceptance criteria
- Existing 1v1 play remains functional after every step.
- No special player-only movement rule; AI and player use same simulation rules.
- Actual fighter hit position matches visible robot at all altitudes.
- A change in carried mass updates movement without a special-case weapon check.
- A shield can simultaneously guard and fire an embedded weapon if capability data permits.
- New leg type or tactical item can be authored by composing existing abilities, with new code needed only for genuinely new mechanics.
- Simulation can run without rendering; same combat core serves offline, AI and online.
- Keep small, coherent commits and document unverified Unity build status.

## Open design questions
- Exact button mapping, crouch/pickup priority and how to expose hand availability.
- Shield guard arcs and body-part hit geometry fidelity.
- Space release descent speed and re-thrust behavior; mass curve.
- Multiplayer authority and networking library selection after prototype refactor.
- World item physics and destructible prop budgets.
