# VoxelOn official visual targets

These reference images define the visual direction; none is a screenshot of the current build:

- `VoxelOn-four-robot-target-v2.png` — newly generated, higher-finish concept board derived from the producer's references. It is a visual target, not an in-game model or screenshot.

- `four-robot-visual-target.png` — target silhouettes and material direction for WISP, BASTION, TARANTULA, and HOUND. BASTION must use a real tracked lower chassis; TARANTULA must have four articulated spider legs; HOUND must read as a canine quadruped; WISP has an oversized head and short limbs.
- `voxel-arena-gameplay-target.png` — target third-person voxel battlefield mood, combat readability, and HUD direction.
- `four-frame-mobility-and-weapon-concept.png` — current shared movement/jump ratios and weapon/control design reference. This image is future design guidance; this visual pass does not change weapon inputs or the current weapon rules.

## Current prototype status

Unity models are procedural block meshes, so they cannot reproduce the illustration's exact surface finish. The 2026-11-02 modeling pass reshaped the playable silhouettes and kept combat, hit volumes, and controls shared:

- WISP: larger head, compact torso, shorter legs, cyan paired lenses, cheek plates and swept antennas.
- BASTION: two wider moving track loops with four rollers per side, low armored hull, twin amber sensor pods, and paired shoulder missile trays.
- TARANTULA: lower split carapace, red multi-eye sensor cluster, and four independently animated outward-splayed articulated legs.
- HOUND: longer horizontal ribcage, larger forward muzzle and nose, upright ears, visible shoulder/hip bearings, four paws, and a segmented tail.

The comparison set is `Implemented/`: 16 actual Windows-build screenshots, four directions per selectable robot, captured after this pass. Runtime smoke checks cover the modeled features and the existing movement/weapon loop. The latest Windows build completed successfully and the game-player smoke suite passed 217 checks. The shapes remain deliberately voxel-procedural and the official visual target is still not matched in material finish or fine detail; these are intermediate models, not final art approval.

- WISP: `Implemented/01-wisp-front.png` and the matching `right`, `rear`, and `left` files.
- BASTION: `Implemented/02-bastion-front.png` and the matching four views.
- TARANTULA: `Implemented/03-tarantula-front.png` and the matching four views.
- HOUND: `Implemented/04-hound-front.png` and the matching four views.

## Model validation checklist

1. Compare the Unity capture against `four-robot-visual-target.png` at matching scale and camera angle.
2. BASTION: confirm two continuous track loops touch the floor and rotate with forward/reverse travel; do not count shin tread decoration as tracks.
3. TARANTULA and HOUND: confirm four independently animated legs, knee/ankle joints, and distinct spider/canine head profiles.
4. WISP: confirm the head is visibly larger and limbs shorter than the other frames.
5. Check that weapon firing, target lock, collision, and ground movement still work with the new visual hierarchy.

Do not add final detailing or declare a silhouette approved without producer review.
