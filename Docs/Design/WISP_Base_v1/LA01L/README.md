# WISP LA-01L Lower-Arm Laser Cannon

Standalone forearm weapon module only. It does not contain or modify the WISP head, torso, shoulders, hips, or legs.

## Outputs

- `LA01L.blend`: editable Blender scene and model
- `LA01L-01-Front.png`, `LA01L-02-Right.png`, `LA01L-03-Back.png`, `LA01L-04-Top.png`, `LA01L-05-45deg.png`: generated review views
- `LA01L-Concept-Comparison.png`: the supplied concept's 3/4 view beside the generated 45-degree view
- `LA01L-Design-Reference.jpg`: supplied source sheet
- `build_la01l.py`: repeatable Blender generator
- `manifest.json`: measured bounds and generation status

## Generation

Generated with Blender 5.2.3 LTS using `build_la01l.py`. The module root is `LA01L_ROOT`, located at the elbow pivot. Coordinates are meters, with the muzzle toward negative Y and the width along X.

Measured model bounds: 0.31 m wide × 0.735 m long × 0.267 m high. This is close to the reference's approximate 0.32 × 0.72 × 0.28 m envelope. Blender generation and the five renders completed; visual/design acceptance remains pending PD review. Unity import was not performed.
