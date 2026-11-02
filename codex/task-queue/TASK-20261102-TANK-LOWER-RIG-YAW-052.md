# TASK-20261102-TANK-LOWER-RIG-YAW-052 — request

## WHY
BASTION chassis steering and differential tread phases existed, but the lower-body visual transform still inherited turret-facing root yaw. The tank silhouette could point at the target instead of the direction of travel.

## DESIGN INTENT
Rotate only the existing legsPart module into world-space chassisForward while keeping the root/core/arm/weapon aim unchanged. Compensate for root yaw and body lean using inverse body world rotation. Reset non-tank lower rig rotation to identity. No new gameplay parameters or geometry.

## PART TREE / JOINT AXES
root (aim yaw Y) -> body (lean) -> corePart/armsPart (aim) and legsPart (independent chassis yaw Y) -> left/right continuous tracks (28 shoes per side). Visual lower yaw is driven by chassisForward, not facing.

## INVARIANTS
Keep existing 120 deg/s steering, shortest forward/reverse, independent track phase differential, movement speed, jump, weapons, collision, main untouched. No unapproved geometry changes.

## VALIDATION
Unity build, smoke, 0/45/135/180 degree steering, side aim while moving, terrain grounding, lower/upper intersection, camera-matched before/after screenshots. Expected: 90-degree aim offset yields 90-degree lower chassis offset; upper weapons remain aimed. Not yet run.

## BEFORE/AFTER VISUAL
BEFORE lower track assembly inherits upper/root yaw; AFTER lower assembly yaw matches chassisForward. Static neutral pose is identical. Use identical camera/FOV/scale/light/pose with turret/chassis 90-degree disagreement to capture actual difference. No image generated this run; Unity capture pending. Numeric directional prediction only, not a 3D render.

## LIMITATIONS/NEXT
Only visual pivot; turret sockets and collision remain inherited. Review arm/track intersection and lower-deck clearance. Reproduce by choosing BASTION, aiming perpendicular to chassis while moving; compare world forward axes. Source commit cc2d57fc7a773d4a0307fa1d65dad537010492c8. Unity NOT RUN.
