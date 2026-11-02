# TASK-20261102-TANK-TREAD-LOOP-046

Status: IMPLEMENTED IN SOURCE / UNITY UNVERIFIED
Base work SHA: dafbe92f29d7270c0dc4c82f79d908a45f836b8c
Source commit: d5c054849e6a13e454ffc027b282356ff1d3d877

## WHY
BASTION greaves showed four isolated horizontal tread reliefs, not a recognizable continuous tank track. This improves the visual reading without claiming functional tracks.

## DESIGN INTENT
Preserve current biped sockets and collisions. Make a closed loop around the outer side of each knee-attached greave using short block segments, with front/rear vertical runs and upper/lower horizontal runs. This is a temporary cosmetic proxy, not a drive chassis. The approved goal remains a true independent tracked lower body and upper-body aiming rig.

## CHANGED
Assets/Scripts/VoxArena.Robot.cs: replace four horizontal tread reliefs per side with 18 loop segments per side: five front + five rear + four upper + four lower. All use existing edge material and knee parent. No movement stats altered.

## PART TREE / JOINT AXES
Robot > left/right leg > knee > outer greave, greave armor, tread loop. Knee pivot and local axes unchanged. Tread is attached to knee, so it will bend with the biped lower leg; this is NOT a continuous vehicle track rig.

## BEFORE / AFTER VISUAL INTENT
BEFORE: four short bars along outer greave. AFTER: rectangular continuous loop silhouette with front/rear vertical sides and upper/lower horizontal runs. Compare front, side, 3/4 under identical orthographic camera, scale, neutral pose and lighting. Source model is at work commit d5c05484. Unity captures pending; any numerical illustration is only a simplified approximation. No image generated or uploaded by this GitHub-only task.

## INVARIANTS
Do not change movement, boost, weapon lock-on, colliders, pivot positions, main branch or earlier WISP/BASTION armor dimensions.

## VALIDATION
Source inspection and exact text replacement only. Unity compile: NOT RUN. Player smoke: NOT RUN. Manual play: NOT RUN. Check tread segment continuity, opposite-side mirroring, foot clearance, knee flex interference, silhouette from front/side/3-4, ground contact, center of mass and weapon clearance. Check BASTION turning slowdown/acceleration and independent aim when true rig exists.

## LIMITATIONS / NEXT
Still a biped; the visible tread is decorative, not physically continuous or animated. Build an isolated true tracked chassis with upper-body yaw pivot and controlled tread animation after Unity verification. If collision/visual interference occurs, reproduce using BASTION knee flex and neutral/turn poses on source SHA d5c05484, then revert this single commit if necessary. Do not call tracked locomotion complete.
