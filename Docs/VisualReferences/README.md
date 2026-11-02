# VoxelOn official visual targets

These reference images define the visual direction; none is a screenshot of the current build:

- `VoxelOn-four-robot-target-v2.png` — newly generated, higher-finish concept board derived from the producer's references. It is a visual target, not an in-game model or screenshot.

- `four-robot-visual-target.png` — target silhouettes and material direction for WISP, BASTION, TARANTULA, and HOUND. BASTION must use a real tracked lower chassis; TARANTULA must have four articulated spider legs; HOUND must read as a canine quadruped; WISP has an oversized head and short limbs.
- `voxel-arena-gameplay-target.png` — target third-person voxel battlefield mood, combat readability, and HUD direction.
- `four-frame-mobility-and-weapon-concept.png` — current shared movement/jump ratios and weapon/control design reference. This image is future design guidance; this visual pass does not change weapon inputs or the current weapon rules.

## Current prototype status

Unity models are procedural block meshes, so they cannot reproduce the illustration's exact surface finish. This pass focuses on silhouette and structure before surface detailing: WISP proportions, a moving linked-track BASTION base, four-joint leg rigs for TARANTULA and HOUND, and each frame's jump ceiling. Existing combat, hit volumes, and controls remain shared.

The comparison set is `Implemented/`: 16 actual Unity-build screenshots, four directions per selectable robot. Review them beside the concept before evaluating proportions. Runtime smoke checks confirm the BASTION links scroll under forward movement and TARANTULA reaches a higher jump arc. These are prototype checkpoints, not final art approval.

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
