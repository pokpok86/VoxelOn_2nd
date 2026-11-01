# TASK-20261102-MOBILITY-VISUAL-031

## Status
Implemented on remote `work`; Unity compile, screenshots and motion review pending. This is a visual prototype, not a finished tracked/quadruped model.

## WHY
Movement archetypes cannot be judged by feel alone when all robots share the same leg silhouette. Runner and tank handling need distinct, readable shapes in motion.

## DESIGN INTENT
Use broad bevelled armor volumes, not scattered decorative rods. Preserve existing animated leg sockets and mesh-based visuals; avoid introducing independent track animation or gameplay collision changes until a full tank body design is validated. Runner gets swept calf aero shell and rear heel spur. BASTION tank-handling prototype gets wide outer greaves, armor cover and four tread-relief ribs per leg. Tread detail is a silhouette cue, not real continuous tracks.

## CHANGED
- `Assets/Scripts/VoxArena.Robot.cs` under existing leg module assembly.
- Code commit `c4bb51fab56ce46b7b273839490c2ae0d098171e`.
- Earlier mobility commits `04b3e66b`, `f42f4753`, `92cf520a`, `c84d4d6b`.

## INVARIANTS
- All eight original selectable frames remain; WISP is Runner and BASTION Tank handling prototype only.
- Existing leg/arm animation, collision radius, hitboxes, gameplay movement, weapon positions, fuel and lock-on are unchanged by this visual change.
- No changes to main. No mesh is represented as a fully functioning track system.

## VALIDATION
- On local Unity: compile and run existing smoke suite; compare WISP and BASTION front/side/rear at equal camera scale in idle, forward sprint, side strafe, reverse, jump and landing.
- Confirm no floating or intersecting calf/foot pieces; legs still animate and weapon remains visible; compare normal biped frame to ensure no accidental visual changes.
- Record screenshots, source SHA, expected vs actual and any Unity errors before fixing.

## LIMITATIONS / NEXT
- No Unity build or screenshots available with home PC off. Numeric dimensions are provisional.
- Next: proper tank/quad lower-body geometry, upper/lower-body heading separation, gait and per-archetype animation, then silhouette tuning against approved concept art.
