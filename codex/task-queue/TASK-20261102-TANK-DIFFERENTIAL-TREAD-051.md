# TASK-20261102-TANK-DIFFERENTIAL-TREAD-051

Status: IMPLEMENTED IN WORK / UNITY UNVERIFIED

## WHY
BASTION tread shoes previously shared one phase and could not counter-rotate while the chassis pivoted. The latest approved tank steering requires independent left/right tread motion.

## DESIGN INTENT
Keep the existing track mesh, pivots, 28 shoes per side, radius 0.39, straight half-length 0.43, and track centers X=±0.83. Integrate signed chassis yaw into independent tread phases without changing gameplay velocity, aiming, or tank turning rules. SignedAngle is in radians; shoe travel = forward velocity*dt + yawRadians*sideOffset. Temporary approximation, not physical track simulation.

## CHANGED
Assets/Scripts/VoxArena.cs (per-side phases and previous chassis heading); Assets/Scripts/VoxArena.Robot.cs (differential shoe animation and rig initialization). Code commits 6a849d3c843570d2443ac374787c1325ae464811 and 7450b7123e2004b9035d098c2b99195829a0767c.

## INVARIANTS
work only; main untouched. No changes to weapon, jump, lock-on, steering speed, collisions, track dimensions or colors. Existing turret aim remains independent at data level.

## PART TREE / AXES
Fighter root -> body -> legsPart -> Continuous tank track 0/1 -> 28 tread shoes. Y-axis chassis yaw, each shoe local track tangent, track center offsets ±0.83 along local X. Upper body/turret hierarchy not yet verified as independent.

## VALIDATION
Unity build, smoke, manual play: NOT RUN. Test straight forward (both shoe phases same direction), reverse (both reverse), pure clockwise/counterclockwise turn (opposite phases), 135° request (45° yaw then reverse). Confirm no phase jump on respawn, no feet/track intersection, track ground contact and center of mass. Inspect C# compilation and runtime behavior on home PC.

## BEFORE/AFTER VISUAL INTENT
Same geometry and silhouette; before both track shoes used one phase, after yaw produces differential travel. Camera/scale/pose identical for visual comparison. No rendered before/after images created in this execution: only animation phase changes; Unity capture unavailable. Static model shape does not change.

## LIMITATIONS / NEXT
No Unity verification; yaw-driven track animation is kinematic, not drivetrain physics. Existing root/body rotation and upper turret hierarchy need real Unity review. Next: inspect independent upper-body pivot and rig collisions. Reproduce any issue by selecting BASTION and rotating chassis while nearly stationary, source SHA 7450b7123e2004b9035d098c2b99195829a0767c.
