# WISP HD-01 sensor head

Standalone small sensor-head module only. It contains no torso, shoulder parts, arms, forearm cannon, or legs.

## Deliverables

- `HD01.blend`: editable Blender model
- `HD01-01-Front.png` through `HD01-05-45deg.png`: front, right, back, left, and three-quarter views
- `HD01-06-Top.png`: top view for shell depth and antenna placement
- `HD01-Concept-Comparison.png`: supplied head guide beside the generated head
- `HD01-Review-Phone.png`: portrait review board
- `HD01-Design-Guide.jpg`, `WISP-Reference-Concept.png`: source references
- `build_hd01.py`: repeatable Blender generator and render setup
- `make_hd01_review.ps1`: creates review boards
- `manifest.json`: dimensions and generation scope

## Design

The latest full-body concept is the reference for this revision. A single rectangular cyan sensor sits within a shallow dark bezel. The shell has clipped brow and cheek planes, dark rear-side sensor pods and a short flattened antenna. The rectangular sensor replaces the previous horizontal strip to match the newly supplied reference. A small fixed mount is included at the bottom.

Coordinates use meters, Z-up, facing -Y. Current measured dimensions are recorded in `manifest.json`; the bounds include the antenna and fixed connector. The render lighting was lowered to preserve material color, and a subtle procedural surface normal was added.

## Rebuild

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python Docs/Design/WISP_Base_v1/HD01/build_hd01.py
& Docs/Design/WISP_Base_v1/HD01/make_hd01_review.ps1
```

Blender generation completed with Blender 5.2.3 LTS and all generated views were visually inspected. **Design approval remains pending PD review.** No Unity import or whole-robot edit was made.
