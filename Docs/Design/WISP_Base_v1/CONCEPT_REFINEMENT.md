# WISP concept refinement — 2026-11-12

The latest full-body reference is `WISP-Latest-Concept.png`. The portrait comparison `WISP-Concept-Refinement-Phone.png` places cropped concept regions beside actual Blender renders. Camera angles were lowered for a closer visual comparison; crops are review aids and do not establish assembled robot proportions.

## Changes actually made

- **BD-01:** replaced the flat rectangular chest silhouette with an open-bottom armor contour and lower angled cheek plates. Added a recessed tapered abdomen. Shoulder caps are tilted and have real fore/aft thickness; upper-arm sockets descend beneath them. Shortened the waist interface. Existing shoulder/waist pivot hierarchy remains.
- **HD-01:** replaced the horizontal optical strip with one rectangular sensor window and a shallow dark bezel to follow the latest reference. Repositioned dark side pods toward the rear and gave the antenna a flat section. Removed the separately attached cheek tabs. Lowered studio exposure and added subtle material surface grain.
- **LG-01:** reduced hip/knee drum dimensions, tapered the main armor, exposed the narrow dark links, and added shaped thigh/shin front plates. Reduced knee-guard projection, narrowed feet and separated toe bumpers and heel blocks. Corrected floating safety-inset offsets. Pivot centers remain unchanged.

## Validation

Blender 5.2.3 LTS executed the generators with Python exceptions configured to return an error exit code. BD-01 shoulder pitch/spread and waist yaw checks and LG-01 hip/knee/ankle motion checks ran during generation. Neutral pose is restored before saving. The generated review images were opened for visual inspection. No Unity import or gameplay test is part of this modeling pass.

## Remaining differences

This is a review candidate, not a declaration of design completion. The reference still has more convincing shoulder-cap transitions, torso depth changes, head armor seams, and foot/ankle integration. Surface roughness is procedural and does not yet reproduce the reference's painted-metal wear. A full assembly is still required to judge global head/body/leg proportions. The LA-01L asset was preserved in this pass.
