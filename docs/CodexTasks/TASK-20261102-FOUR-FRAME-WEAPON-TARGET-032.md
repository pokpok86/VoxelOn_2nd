# TASK-20261102-FOUR-FRAME-WEAPON-TARGET-032

## Status
User-approved design target; NOT implemented or Unity validated. Branches: task queue codex/task-queue, reports codex/task-reports, implementation work only. Do not modify main.

## WHY
Make the latest four-frame voxel concept illustration a concrete, testable target. Combat differences must be visible and meaningful in-game, not only in concept art.

## TARGET / CONTROLS
1. WISP runner: no lock-on; left mouse fires standard gun, right mouse activates held shield/guard.
2. BASTION tank: lock-on gun; Q fires homing missile only with valid lock; E activates energy shield. Shield prevents incoming damage for 5 seconds; cooldown starts only when shield ENDS and lasts 5 seconds. Define interactions with stun, death, and reactivation explicitly during implementation.
3. TARANTULA spider-legged quadruped with humanoid upper body: same lock-on gun and E shield as #2; Q launches radial missiles (not homing). Specify missile count, angles and friendly-fire behavior before balance finalization.
4. HOUND animal-shaped quadruped: no lock-on; fast low-damage vulcan on left mouse; right mouse activates guard with face-mounted beam blade. Guard-to-charge/headbutt is a follow-on mechanic; shoulder-only equipment restrictions are part of the target.

## VISUAL TARGET
Keep the user-approved white/blue/red WISP, gray/orange BASTION, white/orange TARANTULA and white/green HOUND voxel silhouettes and the shared gameplay HUD language. This is AI-generated target art, NOT a Unity capture or verified mesh. Before/after comparisons should use equal pose, camera and scale; distinguish mockups from actual screenshots.

## DESIGN INTENT / INVARIANTS
Preserve existing selectable frames and gameplay until the new four-form test selection is integrated safely. Separate input mapping, lock requirements, cooldown timing, damage immunity, weapon sockets, and visual effects so the local developer can debug each independently. No silent balance changes to unrelated frames. Do not represent design completion as code completion.

## IMPLEMENTATION ORDER
1. Create 4-form prototype selection and weapon config without breaking existing catalog.
2. Lock-on and non-lock-on primary firing plus Q/E/right mouse mappings.
3. Shield state machine (active 5s then cooldown 5s), missile types and vulcan.
4. HOUND face blade, guard/charge, shoulder-only equipment restriction.
5. Actual voxel silhouettes and matching animation; compare AI mockup vs Unity screenshots.
6. Unity compile, smoke, controller/input conflict checks, visual and balance QA.

## LOCAL VALIDATION
Record source SHA and exact reproduction for each bug. Check shield immunity at 0s/4.9s/5s/9.9s/10s, activation and cooldown under hit/knockdown, lock-on required for BASTION homing, radial TARANTULA launch, WISP/HOUND mouse controls, and visual attachment positions during motion.
