# Actual Windows run evidence — 2026-11-04 checkpoint6

Blender diagonal-topology fix plus interior ceiling shadow setup. No combat/collision change. Outdoor unchanged, not rerun. Full268 suite remains last passed checkpoint4 and is scheduled for final integration.

```text
VOXSKY_BUILD_PASS 202307058 bytes
WISP_ENVELOPE Foundry vertices=100760 coreVertices=29280 height=4.038 visualHalfX=1.447 coreHalfX=0.817 coreHalfZ=0.960 collisionRadius=0.978 bakeVsBindMaxError=0.000001
FOUNDRY_LIGHTING_STUDY_PASS 3 real renders; shadow settings restored
ENV_PERF Foundry samples=601 avgMs=8.34 p95Ms=8.42
VOXELON_ENV_PASS Foundry 38 checks
WISP_ENVELOPE Foundry vertices=100760 coreVertices=29280 height=4.038 visualHalfX=1.447 coreHalfX=0.817 coreHalfZ=0.960 collisionRadius=0.978 bakeVsBindMaxError=0.000001
ENV_PERF Foundry samples=601 avgMs=8.33 p95Ms=8.39
VOXELON_ENV_PASS Foundry 38 checks
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Foundry-WISP-only-hangar.png
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Foundry-WISP-crosshair-camera.png
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Foundry-WISP-guard-gameplay.png
```

Diagnostic three-render comparison is v005 before geometry fix. Current overview/cover/motion/GUI images are v006 after the fix. Retained actual brace topology, rather than hiding all shadows or relaxing tests. Final art and manual play approval pending.
