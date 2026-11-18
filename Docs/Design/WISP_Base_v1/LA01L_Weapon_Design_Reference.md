# WISP LA-01L — Forearm Laser Cannon (design only)

**Status:** DESIGN REFERENCE, not an active implementation request. PD will explicitly request the Blender work later.

## Approved visual reference
Image: `wisp_la_01l_암캐논_설계_시트.png` (1122 x 1402 px), provided in PD and Max's conversation on 2026-11-11. The original image is **not embedded in this Git commit** and must be handed to the developer before modeling. Do not claim visual fidelity unless the developer has the image.

## Shape requirements
- A **complete lower-arm weapon** that replaces hand and wrist. No fingers, handle, trigger, rifle stock, or hand.
- Broad and tapered light gray upper and side armor, with stepped layered plates and distinct geometric facets.
- Exposed dark barrel/core below plates, darker gunmetal polygonal front rim and deep **cyan emitter with three narrow horizontal bars**.
- Cyan cooling/light strip inset along the lower side; sparse muted orange accents on angular shell.
- Rear cylindrical elbow hinge / coupler, perfectly aligned to upper-arm hinge. No floating subassemblies.
- Large weapon silhouette and mechanical connection geometry matter more than fine scratches, bolts, labels or effects.
- This is **one module**. Do not redesign WISP's torso, head, legs, or other weapon variants.

## Approximate ratios as written on concept sheet
Overall front-to-rear length ~0.72 L; max height ~0.28 L; max width ~0.32 L, where L is the drawing's common WISP size unit. These are **illustrative**, not measured CAD values, and must be checked against the image.

## Blender part breakdown
```text
LA01L_ROOT
├─ RearElbowHinge (joint)
├─ CannonCore (single continuous gun body)
├─ ArmorTop (stepped facets)
├─ ArmorSide_L / ArmorSide_R (tapered)
├─ ArmorBottom (simple dark shell)
├─ MuzzleOuterRim (octagonal, recessed)
├─ Emitter_CyanBars (3 bars)
├─ CoolingSideStrip (cyan inset)
├─ Socket_Elbow
└─ Socket_Muzzle
```

## Review and acceptance
1. Match original **front, right side, rear, top, 3/4** views. Use equal orthographic scales.
2. Get PD approval of the silhouette and elbow connector **before detailing**.
3. Never introduce hands or a detachable handheld gun.
4. Save editable `LA01L.blend`, export `LA01L.glb` if possible, and render the five views.
5. No changes to Unity gameplay scenes, balance, or robot base mesh in this design delivery.

**NOTE:** This document is only a developer handoff reference; it contains neither the image binary nor a Blender model. Waiting for PD's separate request before implementation.
