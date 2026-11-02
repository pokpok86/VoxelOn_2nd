# TASK-038 — Two distinct bounded maps

## User design
Indoor: existing obstacle-heavy industrial arena, solid walls, extremely high ceiling. Outdoor: sparse obstacles, much more vertical terrain, rocky canyon-floor appearance, stratified stone perimeter. Avoid deep holes that trap fighters.

## Work branch implementation
- e59fe9d5c3d9a58c34fa6fd49a29ea23e6dd90b3: VoxArena.World.cs adds public Inspector enum ArenaMapKind (IndoorFactory default / OutdoorCanyon); map-specific BuildWorld obstacle population; four boundary walls with canyon rock strata; seven climbable stepped ridges and five shelves; X/Z position clamps at +/-95 minus fighter radius. Existing factory cover/elevation retained only for indoor.
- 94aad5f76ebeef4504a96562e74aee558e37fb36: VoxArena.Ground.cs outdoor rocky floor replaces indoor grid and runway.
- d3419e458c6f108b34886555f41d9875b11d3edd: add high hangar roof at approx 69m above ground for indoor only.

## Validation and risks
Unity compile/playtest NOT run. Map is selected in VoxArena Inspector before Play; no runtime menu selector yet. Check indoor ceiling camera clipping and indoor visibility; verify canyon slopes can be walked from both directions (step-height depends on craft), no stuck cases at ridge edges, shelf edges, wall corners, and respawn positions. Terrain currently uses flat rectangular terraces, not a continuous heightfield; canyon rock walls are blocky approximations. Some existing visual environment objects may still be themed for factory in outdoor mode and should be switched. Outdoor has no enclosed pits and no large obstacle field. Ensure AI navigation and shot occlusion match geometry. Existing endless base plane still extends behind perimeter but fighter motion is clamped. Main untouched.

## Next
Expose map choice in menu; add terrain interpolation / more natural rock silhouette, explicit safe slopes and walkable routes, map-specific environment and camera; compare same-angle overview and playtest collisions. Ensure boundary blocks both characters and projectiles appropriately.
