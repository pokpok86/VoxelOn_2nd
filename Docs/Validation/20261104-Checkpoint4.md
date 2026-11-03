# Actual Windows run evidence — 2026-11-04 checkpoint4

Canyon boundary Blender FBX integration. Existing gameplay clamp and full smoke checks preserved.

```text
VOXSKY_BUILD_PASS 203296421 bytes
VOXSKY_SMOKE_PASS 268 checks: ground, modular robots, camera, slot-one gun, mobility, practice AI, aim assist, audio
ENV_PERF Foundry samples=601 avgMs=8.34 p95Ms=8.42
VOXELON_ENV_PASS Foundry 37 checks
ENV_CHECK Blender boundary aligns to arena footprint and safe camera height PASS
ENV_CHECK authored cliff replaces the old plain wall without duplicate surfaces PASS
ENV_CHECK camera remains inside authored canyon X boundary -1 PASS
ENV_CHECK camera remains inside authored canyon Z boundary -1 PASS
ENV_CHECK camera remains inside authored canyon X boundary 1 PASS
ENV_CHECK camera remains inside authored canyon Z boundary 1 PASS
ENV_PERF Canyon samples=601 avgMs=8.34 p95Ms=8.41
VOXELON_ENV_PASS Canyon 22 checks
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Canyon-WISP-only-hangar.png
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Canyon-WISP-crosshair-camera.png
WISP_GAMEPLAY_UI_CAPTURE_PASS D:\Codex\VoxSky\Builds\WorkLatest\Docs\Models\Canyon-WISP-guard-gameplay.png
```

Images are in Docs/Screenshots/Overnight-20261104; Before-v004 preserves the old cliff boundary. GUI images are in Docs/Models/Canyon-WISP-*.png. The 59 environment checks include the new eighth resource, boundary footprint and four-direction camera blockage. Final manual/visual approval remains pending.
