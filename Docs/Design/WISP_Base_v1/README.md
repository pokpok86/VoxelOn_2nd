# WISP Base v2 — design-only handoff (2026-11-10)

**No developer execution request.** Deliberately located outside Docs/CodexTasks.

## Approval reference
PD picked the most recent WISP solo concept: tiny cyan-eyed head; heavy shoulder hinges without optional armor; compact torso; oversized handless integrated laser-gun forearms; short slim normal humanoid legs; gray/dark frame, orange trim, cyan muzzle.

**Important:** The four-view image approved in the chat is a visual reference, **not accurate CAD**, and is not embedded in this repository. Compare it to Blender orthographic captures before any Unity work.

## Files
- `wisp_blender_v2.py`: procedural Blender scene blockout, joint empties, idle micro-motion and fire pose preview.
- `wisp_geometry.json`: draft dimensions, joint coordinates and idle parameters.
- `RIG_AND_REVIEW.md`: parent hierarchy, four-view directions, acceptance checklist and pose validation.

## Blender instructions
1. Open a **new Blender scene** or back up your work; script replaces only the `WISP_V2` collection.
2. Scripting > open `wisp_blender_v2.py` > Run Script.
3. Inspect named empties in Outliner and idle animation frames 1–49.
4. Change `POSE="FIRE"` and rerun for fire pose reference.
5. Capture orthographic Front / Right / Back / Left plus 3/4 view for PD signoff.

## Limitations
This is a **blockout generator** to communicate geometry; not a polished asset, not an armature/FBX, and not tested in actual Blender runtime. Meshes use rigid parenting to Empty pivots, so developer must validate and convert/export appropriately. Hidden geometry and exact proportions are **author choices pending PD review**.

Do not modify `Assets/Scenes/Arena.unity`, `Data/Balance/PD-Balance.json`, implementation branches or runner tasks on the basis of this delivery.
