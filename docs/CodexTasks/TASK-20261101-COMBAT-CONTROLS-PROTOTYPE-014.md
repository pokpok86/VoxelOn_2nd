---
id: TASK-20261101-COMBAT-CONTROLS-PROTOTYPE-014
mode: check
status: ready
priority: 11
title: Consolidate simple lock-on and mouse-aim movement combat prototype
---

## Producer design problem
The most important unsettled VoxelOn game-feel issue is coherent movement/facing/aiming/firing. Producer references Virtual-On: lateral dash can rapidly face opponent and fire a movement-specific version of the same weapon (example: one stronger shot at rest vs three weaker shots while dashing). But Virtual-On does not rely on mouse, whereas VoxelOn may need mouse aim. Producer wants a simple control scheme with minimal special-case complexity, anticipating potentially significant iterative changes. This is an exploratory design request, not approval for broad combat-rule rewrites.

## Preferred prototype hypotheses (NOT final approval)
1. Separate movement vector from facing/aiming. With valid lock-on, automatically face and aim at target with smooth turn limits; strafe/backpedal/dash without unwanted movement redirection. Without lock-on, mouse aim defines facing and firing direction, while WASD defines movement. Explain exact camera and cursor behavior and whether current game camera supports it.
2. Single primary fire input; one test weapon has a basic single shot while idle/normal movement and a short weaker three-shot burst during lateral dash. No additional weapon-mode key; keep overall DPS/energy balanced. Airborne behavior initially unchanged. This is an optional prototype to compare, not mandate to implement across weapons.
3. Compare two simple lock-on UX variants (automatic nearest target vs explicit toggle), including how mouse aiming and lock-on transitions work, target loss and reacquisition. Do not force final decision without playtest.
4. Cover dash direction vs facing, rapid target crossovers, jump/reboost, falling/landing stagger, projectile muzzle orientation, camera shake, animation rig feasibility, target-less firing, network replication, practice AI, and control feedback.
5. Provide minimal vertical-slice implementation plan with A/B comparison, measurable tests and 3-5 gameplay feel test cases. Reuse TARGET-FACING-013 findings; do not duplicate code changes. Identify dependencies and exact latest candidate SHA, and avoid conflict with pending jump tasks.
6. Do not edit gameplay source or main. Publish a concise prototype design and sequenced, bounded tasks to codex/task-reports; all broad combat balance changes require separate producer approval.
