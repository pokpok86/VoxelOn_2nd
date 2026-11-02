# TASK-20261102-ARTICULATED-LEG-PIVOTS-034

## Intent
Build movable robot assemblies, not decorative details that float when the robot bends. Compare the entire assembled silhouette at the same camera, scale and pose after each part edit. Explicit knee and ankle pivots are the first step; actual gait animation remains pending.

## Changes on work
- `8d49559484cc8de5603e07f97313b30d1ddd27b8`: create left/right knee pivot under each existing leg socket at local Y=-0.59; ankle pivot under knee at Y=-0.75. Reparent knee, shin, calf, foot and runner/tank details with compensating local offsets to preserve original world-space assembly.
- `4e3a4fc82bd93df961a7fabe41644906f383ac37`: fix conditional toe plate, grille, nozzle and tread-relief children to follow intended pivot, not the original leg root.

## Invariants and WHY
Existing Fighter.legsPart, leg sockets, head, arms, hitbox and weapon attachment paths remain unchanged. Both pivots have identity rotation so the static neutral pose should look identical; reparenting with exact translation compensation permits later bending without detached armor. All 8 legacy catalog frames continue using the same robot builder. No procedural animation was added.

## Validation required
- Unity compile/play NOT run. Inspect WISP, BASTION and original 8 frame models in neutral pose from front/side/back with same camera/scale. Confirm no visual seams or shifted details. Rotate each knee and ankle pivot in editor to check toe armor, foot, calf nozzle, runner heel spur, tank relief follow correctly. Confirm no clipping at +/-20 degree knee/ankle rotations. Verify combat hitboxes unchanged.
- The original legacy leg mesh remains a child of the leg socket; only the shin-and-below moved to knee. Thigh remains at hip. Need knee animation and collision/foot planting next.
- Four-frame TARANTULA/HOUND presets `36e0b27c` are data-only and not yet wired to four-leg geometry or selection.

## Next
Introduce a small named joint registry for gait without GameObject.Find; implement leg swing and knee/ankle bend, then build tracked lower chassis and four-leg skeletons. Render current geometry numerically AND validate against actual Unity screenshots; never call a synthetic preview an engine capture. main branch remains untouched.
