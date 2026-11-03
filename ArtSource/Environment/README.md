# Current Blender environment source

Use **VoxelOn_Environment_v006.blend** as the current complete eight-object kit. v001–v005 are preserved historical sources; v005 had expanded diagonal braces and is not the current model.

| Runtime FBX in Assets/Resources/Environment | Current authoring iteration |
|---|---|
| FoundryBackdrop | v006 — original diagonal topology preserved, muted service bays |
| CanyonBoundary | v004 — opaque boundary, 24–34m irregular cliff crowns |
| CanyonVista | v003 — irregular mesa shoulders |
| FoundryCargo / FoundryGenerator / FoundryBarricade | v002 — doors, vents, armor and machined edges |
| CanyonRock / OutdoorGround | v002 — geological strata and low-noise ground patches |

All eight are present in v006. Keep material slots, FBX GUIDs and source versions. Covers are normalized X/Y -0.5..0.5, Blender Z0..1. World scenery uses Blender meters/Z-up; export forward-Z, upY via the scripts. Unity placement uses an independent parent so imported axis/unit conversion is preserved. The playable ground is UnityY=-0.91 and boundaryX/Z=+/-140. Collider envelopes are analytical; cosmetic edits must preserve footprint/roof alignment. Indoor ceiling remains the legacy visual with shadow casting disabled for the interior key light; its camera bounds are unchanged.

Reproduce FoundryBackdrop v006 by opening preserved v004 in Blender4.5.15 and running Tools/Art/refine_environment_v006.py. It reuses Kit primitives from refine_environment_v002.py but preserves every retained original vertex/polygon instead of replacing oriented beams with axis-aligned boxes. Other generator scripts correspond to the historical stages. Export changes to new source versions and keep previous work.

Actual comparison/evidence: Docs/Screenshots/Overnight-20261104 and Docs/Validation/20261104-Checkpoint6.md. Current validation: latest Foundry38 checks; prior unchanged Canyon23 checks. Manual final art approval pending.
