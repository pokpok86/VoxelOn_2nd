# TASK-20261102-TANK-DIFFERENTIAL-TREAD-051 — result

Implementation: source changes committed to work. Unity validation: NOT RUN. Main: NOT MODIFIED.

## WHY / DESIGN INTENT
Separate the left and right visual tread travel under chassis yaw while preserving approved tank movement and current geometry. Differential formula: delta distance = dot(velocity,chassisForward)*dt + signedYawRadians*(side==0?-0.83:+0.83).

## CHANGED
Assets/Scripts/VoxArena.cs commit 6a849d3c843570d2443ac374787c1325ae464811; Assets/Scripts/VoxArena.Robot.cs commit 7450b7123e2004b9035d098c2b99195829a0767c. Added per-side phase storage, previous heading, initialization and yaw-based animation.

## NUMERIC COMPARISON
At 45° chassis yaw, no translation: BEFORE left/right shoe travel 0/0; AFTER left/right travel approximately -0.652/+0.652 world units (0.785398*0.83). At 0° yaw, 1 m forward travel: BEFORE 1/1; AFTER 1/1. This is a kinematic prediction, NOT a Unity capture or 3D render.

## PART TREE / JOINT AXES
Root -> body -> legsPart -> track L/R -> 28 tread links each. Chassis yaw around Y; track centers X ±0.83; shoe placement follows existing closed loop radius .39 and halfStraight .43. No geometry changes, so same-camera BEFORE/AFTER still silhouettes coincide.

## INVARIANTS
No changes to gameplay speed, 120°/s steering, boost, weapons, collisions, materials, root aim, or main branch.

## VALIDATION
Source edit committed; Unity compilation, smoke, manual movement and visual capture NOT RUN. Verify forward, reverse, pivot turn, 135° reverse choice, round reset, ground clearance, body/track intersection, and center of mass on actual Unity build. No image created because geometry is unchanged and animation capture unavailable.

## LIMITATIONS / NEXT
Kinematic tread animation only; actual tank drivetrain, turret pivot and manual play remain unverified. If phase jumps, reproduce by selecting BASTION, rotating in place and restarting round; inspect source SHA 7450b7123e2004b9035d098c2b99195829a0767c. Next task should focus on independent upper pivot and visual contact testing.
