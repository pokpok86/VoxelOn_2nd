# WISP LG-01 Biped Leg Pair

LG-01 is a standalone, mirrored pair of short humanoid legs. It contains no pelvis, torso, arms, or head. The geometry follows the supplied leg guide and the WISP base concept's compact lower-body proportions.

## Deliverables

- `LG01.blend`: editable paired leg modules with named Blender pivot empties
- `LG01-01-Front.png` through `LG01-05-45deg.png`: front, right, back, left, and 45-degree views
- `LG01-06-Joint-Centers-Axes.png`: pivot centers and rotation-axis guide
- `LG01-Review-Phone.png`: phone-sized view sheet
- `LG01-Design-Guide.jpg` and `WISP-Reference-Concept.png`: supplied references
- `build_lg01.py`: repeatable model and render generator
- `make_lg01_review.ps1`: adds the pivot legend and builds the review sheet
- `manifest.json`: measured pair dimensions, pivot centers, and rig motion checks

## Scale and rig

WISP height is normalized to 1 m. The pair measures 0.294 m across, 0.161 m front-to-back, and 0.494 m high. Hip centers are at ±0.092 m X, Z=0.455 m; knees at Z=0.254 m; ankles at Z=0.066 m.

Each hip uses nested Y-axis abduction and X-axis pitch pivots. Knee and ankle pivots rotate around X. Their Blender empty origins are placed at the actual joint centers. The generator temporarily rotates each pivot and checks that the foot moves, the hips spread outward, and the knees bend forward, then restores the neutral pose before saving.

## Rebuild

Run Blender 5.2.3 with `build_lg01.py`, then run `make_lg01_review.ps1` in PowerShell. Generation and the pivot-motion checks completed; design approval remains pending PD review. No Unity import or full WISP edits were made.
