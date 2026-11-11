# WISP Base v2 — design-only handoff (2026-11-10)

PD authorized a local Blender blockout experiment on 2026-11-11. This is not approval to import the model into Unity or to change the runtime character.

## Approval reference
PD picked the most recent WISP solo concept: tiny cyan-eyed head; heavy shoulder hinges without optional armor; compact torso; oversized handless integrated laser-gun forearms; short slim normal humanoid legs; gray/dark frame, orange trim, cyan muzzle.

**Important:** The four-view image approved in the chat is a visual reference, **not accurate CAD**, and is not embedded in this repository. Compare it to Blender orthographic captures before any Unity work.

## Files
- `wisp_blender_v2.py`: procedural Blender scene blockout, joint empties, idle micro-motion and fire pose preview.
- `wisp_geometry.json`: draft dimensions, joint coordinates and idle parameters.
- `RIG_AND_REVIEW.md`: parent hierarchy, four-view directions, acceptance checklist and pose validation.

## One-command review run
From the repository root, run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Docs\Design\WISP_Base_v1\run_wisp.ps1
```

The runner uses the installed Blender 5.2 executable, generates three shape candidates, saves an editable `.blend` for each, renders Front / Right / Back / 45-degree PNGs, writes run logs and a 3-by-4 comparison image under `runs/`. The current pass compares a joined short neck, shaped sensor face and forward-facing integrated cannon muzzle; the body-derived shoulder joint is retained. Bolt-on stripes, vents, rails and service boxes are omitted. Set `-BlenderPath` if Blender is installed elsewhere. The core `wisp_blender_v2.py` can still be run directly for a single blockout.

The manifest separates `automation: PASS` from `design_review: PENDING_PD_VISUAL_REVIEW`. A successful render is not design approval.

## Limitations
This is a **blockout generator** to communicate geometry; not a polished asset, not an armature/FBX. Meshes use rigid parenting to Empty pivots, so a developer must validate and convert/export appropriately. Hidden geometry and exact proportions are **author choices pending PD review**. The local automation has been run in Blender 5.2.3; it has not been approved for Unity.

Do not modify `Assets/Scenes/Arena.unity`, `Data/Balance/PD-Balance.json`, implementation branches or runner tasks on the basis of this delivery.
