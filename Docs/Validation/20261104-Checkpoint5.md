# Actual Windows run evidence — 2026-11-04 checkpoint5

Foundry Blender service bays and true skin-envelope measurement. No gameplay behavior changes; last full268 smoke passed in checkpoint4 and is not claimed rerun here. This checkpoint ran the targeted environment/visual smoke61 checks.

```text
VOXSKY_BUILD_PASS 202305190 bytes
WISP_ENVELOPE Foundry vertices=100760 coreVertices=29280 height=4.038 visualHalfX=1.447 coreHalfX=0.817 coreHalfZ=0.960 collisionRadius=0.978 bakeVsBindMaxError=0.000001
ENV_PERF Foundry samples=601 avgMs=8.33 p95Ms=8.39
VOXELON_ENV_PASS Foundry 38 checks
WISP_ENVELOPE Canyon vertices=100760 coreVertices=29280 height=4.038 visualHalfX=1.447 coreHalfX=0.817 coreHalfZ=0.960 collisionRadius=0.978 bakeVsBindMaxError=0.000001
ENV_PERF Canyon samples=601 avgMs=8.33 p95Ms=8.38
VOXELON_ENV_PASS Canyon 23 checks
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Foundry-WISP-only-hangar.png
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Foundry-WISP-crosshair-camera.png
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Foundry-WISP-guard-gameplay.png
```

The first new measurement failed because renderer scale was applied twice. Corrected using BakeMesh(mesh,true), retained original standing-height criterion, and strengthened it with independent bone/bind vertex agreement <0.005m. Actual final error0.000001m. The model was not resized. Torso fits existing radius; wider decorative limbs may still clip walls and remain a manual visual limitation. No main or combat-rule/hitbox changes.
