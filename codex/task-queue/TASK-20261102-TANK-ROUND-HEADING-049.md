# TASK-20261102-TANK-ROUND-HEADING-049
Status: implemented in work; Unity unverified.

## WHY
TASK-048 initializes tank chassis heading during ApplyCharacter, but ResetRound subsequently calls FaceOpponent, changing aim heading. This can create a first-input turn inconsistent with spawn orientation.
## DESIGN INTENT
On each round reset only, initialize chassis heading from final spawn-facing direction; subsequent aim and target lock never override chassis heading. This is a minimal independent change; no tank rig or tread animation claim.
## CHANGED
Assets/Scripts/VoxArena.cs; code commit c29c7fd13c53ec8d5a0cf4e7fcc356c1e0164114 on work.
## INVARIANTS
No main changes; 120 deg/s steering, reverse selection, weapons, jump, other rigs unchanged. No chassis heading update per frame from aim.
## VALIDATION
Unity build, smoke and manual play NOT RUN (home PC unavailable). Repro on prior SHA: select tank, begin match, compare chassisForward with facing immediately after ResetRound FaceOpponent; repeat after rematch. Expected dot ~1 at spawn, then chassis independent when aiming moves. Check 0/45/135/180 degree movement and tank body/weapon pivot independently.
## VISUAL CHECK
No geometry dimensions changed. BEFORE/AFTER geometry identical; numerical heading alignment is the difference. No approximate render produced, because no shape change; actual Unity capture pending. Inspect contact, collision, turret clearance and tread animation after rig implementation.
## LIMITATIONS / NEXT
Actual tank tread drive, articulated upper-body aim pivot, geometry silhouette and runtime smoke are not implemented or validated by this task. Source SHA c29c7fd13c53ec8d5a0cf4e7fcc356c1e0164114.
