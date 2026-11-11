# WISP base-part modeling review

This index groups the three WISP base modules authored independently so the design can be checked part by part before any full assembly or Unity import.

| Module | Editable source | Review board | Automated checks | Design approval |
|---|---|---|---|---|
| BD-01 torso/shoulders | `BD01/BD01.blend` | `BD01/BD01-Review-Phone.png` | shoulder pitch/spread and waist yaw pivot motion; mirrored shoulder mount centers | Pending PD |
| HD-01 sensor head | `HD01/HD01.blend` | `HD01/HD01-Review-Phone.png` | isolated head scope, measured bounds, body mating height documented | Pending PD |
| LG-01 biped leg pair | `LG01/LG01.blend` | `LG01/LG01-Review-Phone.png`, `LG01/LG01-Concept-Comparison.png` | mirrored legs, hip splay/pitch, knee bend, ankle pitch | Pending PD |
| LA-01L forearm laser cannon | `LA01L/LA01L.blend` | `LA01L/LA01L-Concept-Comparison.png` | isolated weapon scope and measured envelope | Pending PD |

Each item remains a standalone, editable Blender module. Their generated renders are for design review and do not mean the final designs are approved or integrated. The parts intentionally keep different levels of detail appropriate to their shapes: the body's armor remains broad and simple, while the forearm cannon has more weapon-specific surface treatment. No Unity import or complete WISP assembly is included in this review.

## Verification record

- BD-01 generator and its temporary joint-rotation checks ran in Blender 5.2.3 LTS; neutral pose restored before saving and rendering.
- HD-01 generator and six orthographic/three-quarter renders ran in Blender 5.2.3 LTS.
- LG-01's existing generator and pivot checks are recorded in `LG01/README.md` and `LG01/manifest.json`.
- Images are visualized in the per-module review boards. PD still decides whether the design matches the references closely enough.
