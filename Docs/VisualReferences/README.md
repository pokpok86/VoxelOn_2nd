# VoxelOn visual prototype — Unity verification checklist

Branch: `feature/visual-polish-20261031` (draft PR #6)

## What is actually implemented
- Procedural beveled/tapered armor on all eight frames (chest, shoulder, helmet, forearm, thigh, shin, toe, hip).
- Extra layered armor tiles **only on frame 01**.
- Warm directional key, cool directional rim and tuned ambient light.
- Numeric industrial skyline: eight distant towers with floor bands and light strips.
- Numeric perimeter pylons and flat arena lane markings.
- Existing weapon, animation, gameplay, target-lock and collision calculations are intended to remain unchanged.

## Important visual limits
- This is **not** the photorealistic concept illustration, nor a high-resolution voxel asset pipeline.
- Current models use procedural primitive shapes and beveled meshes; detail is intentionally an intermediate prototype.
- Environment towers are decorative and intentionally have **no colliders**.
- No local Unity editor build or screenshot was available when this branch was authored.

## First Unity test
1. Open the project using the existing Unity version. Wait for script compilation.
2. If compilation fails, record exact Console messages and file/line before changing anything.
3. Select frame 01 and compare front, side and rear against the concept image.
4. Inspect armor normals (dark/invisible faces), shoulder/hip clipping and detached module movement.
5. Confirm the far skyline appears, but does not block movement or projectile collision.
6. Test F2 lab, target lock, boost, jump, firing and frame switching.
7. Record screenshots with the same camera angle for future comparison.
8. Do not merge the draft PR until Unity compile and play-mode tests pass.

## Follow-up priorities
- Fix mesh face winding and overdraw based on Unity screenshots.
- Create proper voxelized beveled armor geometry and shared mesh/material caches.
- Add articulated mechanical joints and higher-quality materials.
- Tune lighting and performance on the target hardware.
