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

The low-poly shell uses clipped armor planes with a short front-to-back profile and tapered lower cheeks. One horizontal cyan glass slit spans the front; it is one continuous optic, not two eyes. Low-profile side sensor pods and a short rear-set antenna are kept within the small head silhouette. A small fixed mount is included at the bottom. The head has no independent articulation.

Coordinates use meters, Z-up, facing -Y. Blender measured bounds include the antenna and fixed connector: **0.157 m wide × 0.111 m deep × 0.150 m high**.

## Rebuild

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python Docs/Design/WISP_Base_v1/HD01/build_hd01.py
& Docs/Design/WISP_Base_v1/HD01/make_hd01_review.ps1
```

Blender generation completed with Blender 5.2.3 LTS and all generated views were visually inspected. **Design approval remains pending PD review.** No Unity import or whole-robot edit was made.
