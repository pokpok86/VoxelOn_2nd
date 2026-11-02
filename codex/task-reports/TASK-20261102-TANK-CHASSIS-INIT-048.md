# TASK-20261102-TANK-CHASSIS-INIT-048
Status: SOURCE IMPLEMENTED ON work / UNITY UNVERIFIED
Base work SHA: af3316c00f06042ec97e8ce9214f959bc785f4c1
Code commit: f72b444dab41c5ece9850bc2e0672eb81756ea0f

## WHY
The chassisForward field previously initialized to world +Z, even when an existing fighter was facing another direction during craft selection. This could cause an unexpected first steering turn and a discontinuity when selecting BASTION.

## DESIGN INTENT
At ApplyCharacter, initialize chassisForward from the horizontal normalized facing vector, falling back to world +Z only for degenerate facing. Keep tank steering 120 degrees/second and shortest forward/reverse logic unchanged. This is a narrow state initialization fix, not a new tracked rig.

## CHANGED
Assets/Scripts/VoxArena.cs, ApplyCharacter; commit f72b444dab41c5ece9850bc2e0672eb81756ea0f.

## INVARIANTS
No main changes. No changes to aim facing, acceleration, jump, boost, lock-on, collision, weapon handling or tank turn rate. No new geometry. Existing craft selection continues to build robot and apply scale.

## PART TREE / JOINT AXES
Planned only: chassis yaw about local Y, separate turret yaw about Y, left/right track travel about longitudinal track loop. Actual model remains biped with cosmetic tread plates; not implemented in this task.

## BEFORE / AFTER VISUAL INTENT
Geometry and neutral silhouette are identical. BEFORE state heading could be +Z while the existing aim facing differed; AFTER state heading is initialized to horizontal facing. Example facing +X: before chassis +Z (90 degree mismatch); after +X (0 degree mismatch). This is a state comparison, not a Unity image or mesh render. Same camera, FOV, scale, neutral pose expected to show no geometric difference. No new render generated because geometry did not change; Unity capture pending.

## VALIDATION
Unity compile, smoke, and gameplay NOT RUN (home PC unavailable). Re-select BASTION while facing +X, -Z, +Z; verify chassisForward matches horizontal facing at selection. Then test requested 0, 180, 45, 135 degree movement and weapon aim independently. Verify no selection regressions for other rigs. Test first frame after ResetRound separately.

## LIMITATIONS / NEXT
Does not provide true track chassis, track differential animation, or upper turret pivot. Inspect ResetRound and fighter spawn orientation for further initialization paths. If unexpected first turn persists, reproduce on work commit f72b444dab41c5ece9850bc2e0672eb81756ea0f, record selected frame, facing, chassisForward, desired vector, and first-frame velocity. Do not claim Unity validation.
