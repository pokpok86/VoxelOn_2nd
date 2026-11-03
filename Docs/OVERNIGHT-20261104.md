# Voxel-On overnight — 2026-11-04 (stop 06:00 KST)

## Authorization / boundaries
User requested continued development and quality comparisons through 2026-11-04 06:00 KST, including after initial tasks pass. Verified changes may be committed/pushed to work only. NEVER merge main before separate morning approval. No subagents or legacy Windows runner restart. Preserve designer v001/v002 and rigged v003. New graphics must be authored in Blender and exported FBX. Keep WISP-only roster, manual aim, left gun/right guard pose and current camera.

## Automation / handoff
App heartbeat voxel-on-11-4-06 is registered ACTIVE, hourly through 06:00. First timed execution has NOT yet been observed as of 00:50 KST. A registration success is not proof of execution. At 06:00 stop feature edits, complete active validation, publish verified work/report and pause heartbeat. Actual project is D:\Codex\VoxSky\Builds\WorkLatest; chat cwd on OneDrive is not the Unity project. Read this file and logs before continuing. Do not run concurrent Unity builds.

## Implemented checkpoint (00:50 KST)
- Blender WISP v004: muted slate/charcoal armor, ochre trim, teal sensor, slightly shorter antennas. Original rig, natural bent rest stance, 24 bones/112 skins, dimensions and joints preserved. v001/v002/v003 remain. Unity uses v004 FBX + base/emission atlases. Internal rig/object v003 names intentionally retained for socket compatibility.
- Real movement normal speeds 15/5/15/15 = forward/back/left/right 3:1:3:3. Corrected hidden WISP part-budget directional multipliers. Shift forward sustained run 24.75; side/back 32 continuous travel, no low-hop dash. W+A/D slight 17-degree turn. New articulated side-step.
- Higher/faster tap jump: observed peak3.76, airtime0.81s. WISP exhaust only airborne while Space held. Ground running/sliding has no aerial flames.
- Gun interval0.18s (previous0.105), 30round magazine, partial automatic reload after2.2s idle; reload1.5s. Manual/empty reload kept.
- Product/title/output Voxel-On. WISP-only roster preserved. Map selector switches Foundry/Canyon and persists independently.
- Seven Blender environment FBXs: cargo/generator/barricade, canyon rock/floor/vista, foundry backdrop. Editable source ArtSource/Environment/VoxelOn_Environment_v001.blend, reproducible script Tools/Art/overnight_blender_20261104.py. New runtime code only loads/places FBXs; no new procedural mesh art.
- Cover floor/roof bounds and factory roof collisions aligned. FBX placement parent preserves imported axis/unit transforms; initial orientation bug was found by tests/screenshots and fixed.

## Actual validation for this checkpoint
Unity6000.6.7f1 Windows build PASS: Logs/Build-Overnight-20261104.log. Runnable Builds/Windows/Voxel-On.exe (copy whole Windows folder to relocate). Latest DLL built after FBX-axis fix.
- Full executable smoke: 268/268 PASS, Logs/smoke-test-20261104.log.
- Rendered environment: Foundry36/36, Canyon15/15 PASS; both WISP spawns free, all seven assets load without fallback, cover bounds agree, canyon floor horizontal at real ground, tall rock dimensions exact.
- Render sampling over6sec each (first1sec excluded): 601frames/map, mean8.33ms, p95 Foundry8.42ms/Canyon8.38ms. Capped120Hz observation, not a worst-case GPU benchmark.
- Separate-process preferences write/read PASS including Map; validation namespace cleaned, player preferences untouched.
- Actual GUI captures inspected: Docs/Models/{Foundry,Canyon}-WISP-{only-hangar,crosshair-camera,guard-gameplay}.png. Visible window required: hidden-window captures were black and were replaced by visible captures.
- Environment screenshots: Docs/Screenshots/Overnight-20261104/{Foundry,Canyon}-{overview,cover}.png, inspected after final orientation fix.
- Blender v004 front/rear/three-quarter renders: Docs/Models/WISP-v004-*.png.

## Remaining quality work through 06:00
First batch is functional, not final visual quality: distant large factory faces and canyon skyline remain too simple; floor pattern is repetitive and dense detail can shimmer at distance. Compare gameplay-scale screenshots, improve forms/readability via Blender FBX, preserve collision footprint and safe routes. New graphics should not reintroduce collision misalignment. Review runtime WISP brightness vs Blender render, and genuine lateral motion/landing/Space effects visually. Do not change aim AI or add combat rules opportunistically. Guard is an animation only: no new damage reduction implemented.
After source/asset changes build sequentially and run relevant checks; repeat full smoke if behavior changes. Save before/after screenshots and update evidence here. Avoid repeating tests with no new changes. Keep working meaningfully until deadline.

## Repository / backups
Base remote voxelon/work bda654a4cc18eee72567071d55a92de2abd50197 unchanged when fetched at00:49. Detached local checkout; use explicit git-dir D:/Codex/VoxSky/.git/worktrees/WorkLatest and work-tree path. Publish HEAD:work without force; never origin (old repository) or main. No remote push yet for this checkpoint.
Backups/Before-20261104-Title/Windows preserves prior executable. Old active VoxSky.exe moved to Backups/BlenderAutosaves-20261104/VoxSky-old-launcher.exe to prevent accidentally starting old version. Generated .blend1 autosaves preserved there; not staged. Logs/Builds/Library/Backups excluded from Git.
