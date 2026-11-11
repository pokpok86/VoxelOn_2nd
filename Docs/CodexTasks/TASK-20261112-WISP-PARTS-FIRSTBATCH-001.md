---
task_id: TASK-20261112-WISP-PARTS-FIRSTBATCH-001
title: "WISP first batch modular parts: five forearms and twin booster"
status: draft
mode: change
source_branch: main
target_branch: work
priority: high
owner: developer
---

# PD design brief — WISP modular parts / first batch

**CURRENT STATE: WAITING FOR REFERENCE PNG ASSETS. DO NOT START MODELING WHILE THE FILES ARE MISSING.**  
This task is deliberately **draft**, not ready. PD has asked to prioritize items with illustrated sheets; the images must be available to the developer in GitHub before activation.

## Reference images (REQUIRED)

Upload these SIX PNGs from the supplied `WISP_Parts_FirstBatch_20261112.zip` into
`Docs/Design/WISP_Parts/References/` on the `work` branch, keeping exact file names:

| Code | Image | Deliverable |
| --- | --- | --- |
| W-01 LUMEN | `W-01_LUMEN.png` | integrated forearm laser cannon |
| W-02 BREAKER | `W-02_BREAKER.png` | large-bore bazooka forearm module |
| W-03 RATTLE | `W-03_RATTLE.png` | rotary multi-barrel Vulcan forearm module |
| W-04 EDGE | `W-04_EDGE.png` | integrated forearm blade (not hand-held) |
| W-05 AEGIS | `W-05_AEGIS.png` | shield replacing the forearm |
| B-01 TWIN DRIVE | `B-01_TWIN_DRIVE.png` | compact symmetric twin-booster backpack |

**No new visual inventions:** approved images control silhouette and armor segmentation. Color labels and any approximate dimensions in generated art are illustrative, not verified geometric specs. Maintain the existing approved WISP robot base. For all weapons, **NO hands/fingers/wrists/grips**: these are entire elbow-down interchangeable modules. Backpack attaches to the standardized torso-back mount.

## Implementation phases AFTER activation

1. Inspect six image files and existing WISP Blender base from the latest `work` branch. Capture current shape and mount coordinates; never overwrite the baseline `.blend`.
2. Produce **W-01 LUMEN only** as the first quality gate. Create an editable Blender file with separate structural/armor parts; compare front, side, rear and 45° render at identical orthographic scale. Produce mounted WISP preview. Do not proceed to W-02~W-05/B-01 until PD approves W-01's appearance and joint interface.
3. Once approved, implement W-02/W-03/W-04/W-05 following the same elbow connection; then B-01 with common back mount. Each item needs its own editable `.blend`, 4-view images, reference-vs-render comparison sheet, and mount close-up.
4. Validate idle arm hang clearance, aimed pose, blade/shield self intersection, large-gun clearance, and backpack collisions. Keep existing waist yaw, short slim normal legs, small fixed head.
5. Post a truthful status report distinguishing Blender automation pass, image likeness PD approval, and Unity integration separately.

## Acceptance criteria

- Shape and proportion resemblance to the **specific provided PNG**, not just a vaguely matching weapon category.
- All six part IDs and exact file names retained in output files, Blender object names and report.
- No separate handheld weapon or hand-like connectors.
- PD signs off the first example before bulk production. If design mismatches, modify only the selected module rather than restarting the full robot.
- Save/reuse base model in `work`; do not merge into `main` without PD confirmation.
- Do not change `Data/Balance/PD-Balance.json`, `Assets/Scenes/Arena.unity`, game weapons/balance, or active runner schedule.

## Notes on delivery and automation

The original concept images are generated presentation art rather than precise CAD. Orthographic views may disagree across angles; treat the PD-approved front and 45° silhouette as visual priority, explicitly annotate hidden-geometry assumptions. Do **not** claim a render is an exact match before side-by-side PD inspection.

After PNGs arrive on `work`, PD may activate this task by changing the YAML `status` to `ready` and assign implementation. This document alone is not permission for autonomous work.
