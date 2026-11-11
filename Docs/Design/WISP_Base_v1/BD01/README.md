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

The chest has an open lower outline: its lower cheek plates flank a recessed, tapered dark abdomen. A small center chest plate and restrained orange inset follow the supplied concept. Angled shoulder caps wrap around the cylindrical joint, and the short upper-arm connectors now point downward. The bottom waist interface was shortened. A subtle procedural material normal adds surface grain without extra geometric decoration.

The central chest plate now has a shallow 10 mm forward ridge, forming two broad angled faces. Its outside width remains unchanged.

Current bounds and pivot centers are stored in `manifest.json` and named Blender empties. The main torso core is about 0.350 m wide. The generator temporarily rotates each shoulder and the waist interface, checks that the connector moves while the waist rotates in place, and restores the neutral pose before rendering. Visual resemblance remains a review task; generation success does not approve the design.

## Rebuild and validation

From the project root:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python Docs/Design/WISP_Base_v1/BD01/build_bd01.py
& Docs/Design/WISP_Base_v1/BD01/make_bd01_review.ps1
```

Blender generation and the pivot motion checks passed with Blender 5.2.3 LTS. The renders were opened and visually inspected. **Design approval remains pending PD review.** No Unity import or full-robot edits were made.
