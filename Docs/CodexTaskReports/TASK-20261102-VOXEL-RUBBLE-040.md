# TASK-040 — Original-colour voxel explosion and persistent nonblocking rubble

User request: when destructible cover breaks, scatter the voxel components outward using the original cover colours. Leave large and small pieces behind as visual rubble that never affects fighter movement or projectile collision.

## Implemented on work
- `c8109972dc8c09cb2b26cf4b66d76bae36157746` — VoxArena.World.cs: sample child renderer materials from the actual destroyed cover, create 16–34 small/large coloured cubes with randomized outward/upward velocities, apply gravity until ground contact, retain settled cubes as non-colliding visual remnants. Fragments have no registration in `obstacles`, and any primitive Collider is explicitly removed. Cap total live + settled fragments at 320 and evict oldest for performance. Existing destruction still removes original cover and obstacle Bounds immediately.

## Verification required (NOT RUN)
- Unity compile and playtest. Confirm Box() returns Transform and creates visible colour; verify material shader supports _Color (fallback if not), fragments land on flat ground and do not block fighter, bullets, AI, or navigation.
- Compare original cover colours to fragments. Test repeated destruction and 320-fragment cap; watch GameObject count, frame time, garbage collection, and shadow performance. Test midair explosion and landing break. Check that fragments are visible from third-person camera and don't hover above ground.
- This is a **first-pass visual effect**, not exact voxel mesh decomposition. It samples source renderer colours and creates approximate cubes, rather than extracting individual mesh voxels. Fragments are currently not pooled; bounded cap mitigates growth. No cracking stages or dust effect yet. No main branch changes.

## Next
Use deterministic fragment palette/positions from actual voxel geometry if available; add progressive damage cracks and optional low-cost dust; consider pooling. Keep terrain and perimeter structural/indestructible. Validate with same camera before/after screenshots.
