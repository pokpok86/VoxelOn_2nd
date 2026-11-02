# TASK-20261102-TANK-GREAVE-DEPTH-045

Status: CODE IMPLEMENTED ON work; UNITY UNVERIFIED

WHY: BASTION's tread-inspired lower-leg armor needs a larger coherent mass in profile without adding meaningless decorative rods.
DESIGN INTENT: Visual-only incremental increase of outer greave armor depth from 0.67 to 0.77 Unity local units; same knee parent/pivot, x width 0.12, y height 0.77, center (side*0.48,-0.320,0.06), bevel 0.17, taper 0.78. No functional track system claimed.
PART TREE: robot > legs/pelvis > left/right leg socket > knee pivot > Tank greave armor. Rotation axis of knee is local X; no pivot edits.
CHANGED: Assets/Scripts/VoxArena.Robot.cs; source commit eaf8df692da7f5e081b5ad75c3a6d92006b256d8. Starting work commit 0201d1205ed3bf81693830108d53f334e9b0f16a.
INVARIANTS: no collider, CraftSpec, movement, weapons, camera, jump or pivot modifications. main untouched.
BEFORE/AFTER: width 0.12 -> 0.12; height 0.77 -> 0.77; depth 0.67 -> 0.77 (+0.10, +14.9%). Center z=0.06, frontmost local z approximately 0.395 -> 0.445 (ignoring taper). Compare neutral pose at identical camera, scale, lighting and FOV, front/side/three-quarter views. Target: broader continuous armor mass; source concept art match NOT verified.
VALIDATION: Unity build, smoke, manual gameplay, screenshots NOT RUN (home PC unavailable). Check knee bend at 0/30/60 degrees, foot contact, shin/armor clipping, ground clearance, center of mass impression, shoulder weapon clearance; compare before/after same pose. Visual approximation image not generated in this execution. No claim of actual Unity render.
LIMITATIONS/NEXT: existing biped is NOT a real tracked rig; build dedicated lower drive and independent upper aim in separate tasks. If clipping occurs, reproduce with BASTION knee 60 degrees and revert only the 0.77 depth to 0.67 at source commit eaf8df69. No changes to main.
