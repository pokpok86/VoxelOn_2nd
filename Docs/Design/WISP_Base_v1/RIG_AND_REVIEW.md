# WISP V2 — articulation, rig & review contract

## Object hierarchy
```text
ROOT
└─ Pelvis_Frame
   ├─ pelvis and both hips -> knees -> ankles -> feet
   └─ Waist_Yaw_Z (Z-axis ONLY)
      └─ UpperBody_BreathIdle
         ├─ torso and chest
         ├─ Head_FIXED
         ├─ Shoulder_L/R_PitchX -> Shoulder_L/R_AbductY
         │  ├─ body socket -> upper/lower shoulder connector struts -> shoulder axle/cap
         │  └─ UpperArm -> Elbow_L/R_PitchX -> ForearmGun, Socket_Muzzle
         └─ Socket_Backpack
```

**Do not put hip joints under Waist_Yaw_Z**; yaw must rotate upper body only.

## Pose rules
- Idle: shoulder slight outward angle, elbows bent; cannons rest beside legs without touching. Never generate hands.
- Movement: small alternating shoulder swings; hips pitch plus side abduction; knees and ankles pitch.
- Fire: shoulders rotate forearms forward and elbows extend. Verify muzzle orientation after posing.
- Head: fixed relative to upper body. Waist: yaw only, no forward/back bend.
- Idle breathing illusion: 49 frames / 24fps (~2 seconds), upper-body rise 0–6 mm, Z-only scale 1.000→1.006→1.000, <1 degree arm change. Do not add large body bob or camera shake.

## Orthographic camera guide
| View | Camera direction | Validation |
| --- | --- | --- |
| Front | from -Y looking +Y | head tiny, symmetry, no hands, slim legs |
| Right | from +X looking -X | cannon barrel and elbow position, normal knees |
| Back | from +Y looking -Y | exposed joint housing, backpack socket |
| Left | from -X looking +X | mirror geometry and arm clearance |

Use identical **orthographic scale**, rest pose, height and lighting across screenshots.

## PD acceptance gate
1. Match the approved silhouette visually before any texture detailing.
2. Keep the bare-joint base option available; no hands or weapon grip.
3. Shoulder pivot connects visibly to the torso socket; connector links articulate with the shoulder.
4. Large forearm cannons cannot collide with thighs or floor while idle, strafing, walking or firing.
5. Waist yaw does not rotate hips/legs; neck cannot rotate.
6. Idle micro-motion visible subtly without distorting armor, creating gaps or causing camera jitter.
7. Muzzle sockets point along actual emitter direction; eventual muzzle flash is separate implementation work.
8. Developers run Blender and, later, FBX/Unity checks themselves. Nothing in this pack was approved for Unity.

**Status:** reference-only, awaiting PD's explicit implementation request.
