# TASK-20261102-TANK-CHASSIS-STEER-047
Status: IMPLEMENTED IN WORK / UNITY UNVERIFIED
Base work SHA: 1f3c8a0467289f9bc3654796842d2302ef5b7414
Code commit: 9592c046141f8448450a88eafe123f76a4e4bcc1

## WHY
BASTION requires 0 degree immediate forward, 180 degree immediate reverse, and finite shortest-path rotation for other headings; prior MoveTowards velocity smoothing did not steer chassis.
## DESIGN INTENT
Opt-in Tank only: independent chassisForward state, reverse when dot < 0, RotateTowards at provisional 120 degrees/s, turn speed factor clamp(1-angle/90,0,1). A 135 degree request uses 45 degree reverse turn. No change to aiming-facing or legacy rigs. No tread differential animation yet.
## CHANGED
Assets/Scripts/VoxArena.cs at 9592c046. Chassis state and movement-only steering inserted before existing turn acceleration.
## INVARIANTS
main untouched; no new target-lock behavior; legacy frames and dash path unchanged; visual turret still follows legacy facing.
## PART TREE / JOINT AXES
Future: chassis root Y yaw -> left/right track X roll; separate upper turret Y yaw; currently not built. Existing biped hierarchy remains.
## BEFORE / AFTER VISUAL INTENT
No geometry modification: BEFORE and AFTER silhouette identical; movement heading changes only. For future images compare same front/side/3-4 orthographic camera, scale and pose. Unity capture pending; no image generated in this run.
## VALIDATION
Unity compile, smoke, and gameplay NOT RUN (home PC offline). Test 0/180/45/135/270 degrees, rapid reversals, collision, boost, lock-on, grounded movement. Expected 180 instant reverse; 135 reverse with 45 yaw; upper aim unaffected logically, but visual separation absent.
## LIMITATIONS / NEXT
Movement prototype only; current root still faces aim, track mesh/animations and turret hierarchy missing. Validate steering speed and zero pivot before adjusting. Reproduce issues on work 9592c046 using BASTION prototype rig and camera-relative movement; report requested vector, chassisForward, velocity, facing and collision state.
