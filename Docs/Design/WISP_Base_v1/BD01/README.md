# WISP BD-01 compact body and shoulder module

Standalone torso/shoulder module. It does not include a head, upper arms, forearm weapons, pelvis/legs, or a complete WISP assembly.

## Review files

- `BD01.blend`: editable Blender 5.2.3 scene; metric units, Z up, front toward -Y
- `BD01-01-Front.png` through `BD01-05-45deg.png`: front, right, back, left, and three-quarter review views
- `BD01-06-Connections-Closeup.png`: clean shoulder/waist connection close-up
- `BD01-07-Connection-Axes.png`: the same connection area with the actual pivot axes and centers marked
- `BD01-Concept-Comparison.png`: supplied BD-01 sheet beside the generated 45-degree view
- `BD01-Review-Phone.png`: portrait review board for mobile viewing
- `build_bd01.py`: rebuilds the module, all Blender renders, and JSON manifest
- `make_bd01_review.ps1`: generates the annotated pivot image and comparison boards
- `manifest.json`: dimensions, joint centers, render names, and automated checks

## Shape and attachment points

The chest is a short, broad core with a single large front plate and a small center point plate. A plain rear plate has one central recess. The neck interface is fixed and compact. Each side has a cylindrical X-axis shoulder pitch joint, a nested Y-axis slight-spread pivot, a small shoulder armor cap, and only a short upper-arm connector stub. The bottom is a short Z-axis waist-yaw housing and interface.

Approximate outer bounds: **0.736 m wide × 0.271 m deep × 0.413 m high**. The actual main torso core is about 0.392 m wide. Pivot centers are stored in `manifest.json` and named Blender empties. The generator temporarily rotates each shoulder and the waist interface, checks that the connector moves while the waist rotates in place, and restores the neutral pose before rendering.

## Rebuild and validation

From the project root:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python Docs/Design/WISP_Base_v1/BD01/build_bd01.py
& Docs/Design/WISP_Base_v1/BD01/make_bd01_review.ps1
```

Blender generation and the pivot motion checks passed with Blender 5.2.3 LTS. The renders were opened and visually inspected. **Design approval remains pending PD review.** No Unity import or full-robot edits were made.
