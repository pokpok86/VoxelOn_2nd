# TASK-20261102-TANK-TREAD-HEADING-050
Status: source implemented on work; Unity unverified.

## WHY
BASTION track translation was derived from root.InverseTransformDirection(velocity), even though tank chassisForward is independent of the aiming/root heading. Aiming while moving could reverse or stop cosmetic tread movement incorrectly.

## DESIGN INTENT
Use signed velocity projected onto normalized planar chassisForward. Preserve the existing single shared phase for both tracks for now; differential turning is NOT implemented. This is a minimal correctness fix, not a complete tracked drivetrain.

## CHANGED
Base work SHA: defcc841bcf12e6163ee96ab68363c3d9b6ad28e
Source commit: a4669c761b1da23c2cdb966759141e8b93a06f0d
Assets/Scripts/VoxArena.Robot.cs / AnimateTreads: replace root-forward speed projection with chassis-forward velocity projection and a zero-vector fallback.

## INVARIANTS
No main changes. Do not alter 120 deg/s chassis steering, aiming, gameplay velocity, hitboxes, weapons, jump, or existing track mesh dimensions. Tank tread loop: 28 shoes per side, radius 0.34, half-straight 0.38; both sides still share phase.

## VALIDATION
Source review: projected speed remains signed. Unity build, smoke, manual play: NOT RUN (home PC unavailable). On next Unity run: move forward while aiming 90 deg sideways; tread should still travel forward. Move backward while aiming sideways; tread should reverse. Turn in place: differential animation is still absent and must be a separate task. Compare front/side/3-quarter views at identical camera, FOV, scale, pose, and lighting. Check shoe overlap, chassis ground contact, center of mass and weapon placement.

## VISUAL BEFORE/AFTER
Geometry and numeric sizes unchanged; same neutral-pose silhouette before and after. No new image generated because the modification affects only tread motion and the runtime Unity capture is unavailable. Numeric model baseline from current work: track x +/-0.76, y -0.89, 28 shoes per side, shoe 0.50 x 0.105 x 0.15, radius 0.34. Proposed geometric delta = 0. Compare motion frames at fixed camera when Unity is available. Existing source is a prototype, not a validated concept match.

## LIMITATIONS / NEXT
Both tracks share treadPhase; yaw-based left/right differential speeds and turret pivot hierarchy still missing. Quad rigs and full gameplay require Unity tests. If a regression occurs, reproduce using the sideways aim + forward/reverse sequence at source SHA a4669c761b1da23c2cdb966759141e8b93a06f0d; restore previous root projection only as temporary rollback while preserving chassis steering. No claim of Unity verification.
