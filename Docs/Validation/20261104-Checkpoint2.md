# Actual Windows run evidence — 2026-11-04 checkpoint2

Captured from local Unity/player logs after Blender environment v002 coplanar-face fix. Logs are local build outputs; these are exact success/performance lines.

```text
VOXSKY_BUILD_PASS 188146670 bytes
VOXSKY_SMOKE_PASS 268 checks: ground, modular robots, camera, slot-one gun, mobility, practice AI, aim assist, audio
ENV_PERF Foundry samples=601 avgMs=8.33 p95Ms=8.39
VOXELON_ENV_PASS Foundry 36 checks
ENV_PERF Canyon samples=601 avgMs=8.33 p95Ms=8.41
VOXELON_ENV_PASS Canyon 15 checks
VOXSKY_PREFS_WRITE_PASS
VOXSKY_PREFS_RESTART_PASS
WISP_GAMEPLAY_UI_CAPTURE_FAIL blank image
```

The blank capture failure is an intentional negative test: hidden windows must not publish black screenshots. Both maps passed visible GUI capture; valid files remain in Docs/Models. Environment images and before/after comparison are in Docs/Screenshots/Overnight-20261104. Settings probes used an isolated namespace and were run before the second graphics-only iteration. Manual long play and final visual approval remain pending; main untouched.
