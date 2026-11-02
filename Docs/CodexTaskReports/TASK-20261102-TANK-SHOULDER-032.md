# TASK-20261102-TANK-SHOULDER-032

Status: Code committed to work; Unity build, smoke, gameplay and capture NOT RUN.

## WHY
The tank handling prototype needs a clearly heavy upper silhouette, not just thickened greaves. This is a reversible visual-only incremental change.

## DESIGN INTENT / BEFORE AND AFTER
Before (inferred from code, not a Unity capture): existing shoulder cap and conventional biped outline. After (inferred, not verified): broad bevelled shoulder bulwark with a dark front inset, while retaining articulated arm socket. Both comparisons must use identical camera pose, FOV, model scale, lighting, idle pose and colors. No before/after Unity screenshots or AI render were generated in this automation run. Ask for a side-by-side AI preview in the interactive chat if desired; do not label AI art as Unity output.

## CHANGED
- `Assets/Scripts/VoxArena.Robot.cs`: conditional Tank-only integrated shoulder armor and front inset attached to existing arm socket.
- Source SHA: `8843da9ca54f666ce7e01c7e0d2b1038e0f70481` on `work` (starting work SHA `c4bb51fab56ce46b7b273839490c2ae0d098171e`).

## INVARIANTS
No changes to 8 selectable frames, locomotion, physics, fuel, lock-on, damage, hitboxes, weapons or other mobility types. Tank remains biped prototype. No main changes.

## VALIDATION
On local Unity compile, check Tank shoulders in idle, aiming, running, jump and arm swing, front/side/rear views; confirm new bulwark does not occlude muzzle, visor, shoulder weapon or forearm. Compare normal biped and runner unaffected. Save identical-condition screenshots and smoke results. On failure, log input/pose, expected vs actual, console errors, branch SHA, and revert this single commit if severe clipping.

## LIMITATIONS / NEXT
Not built or rendered in Unity. Geometry may intersect existing cap, chest or arm under animation. Later: dedicated tank locomotion mesh and separate upper-body aiming pivot, gated by runtime verification.
