---
id: TASK-20261102-VISUAL-BASELINE-MATRIX-022
mode: check
status: ready
priority: 19
title: Audit comparable original PNG captures for SD silhouette iteration
---

## Scope
Read-only visual evidence preparation. Current 36 PNGs are reported at 768x432; they do NOT establish original concept art match nor before/after improvement. Existing visual QA 004 inspected older images, and visual 019 gave possible improvements. Do not duplicate their generic audit.

## Deliverable
- Inventory exact screenshot paths and source provenance, screenshot naming conventions and camera settings on available source; identify which images can be paired for future before/after at identical pose, zoom, lighting and map.
- Provide concrete **capture manifest schema**: source SHA, branch, Unity build hash, scene, robot, camera pose, timestamp, original PNG SHA256, visual change reason, baseline/candidate paths, pass/fail, and producer-approval pending.
- Pick **one** safe future SD silhouette delta (Bastion head width OR Tempest pelvis mass, not both) based on current evidence, with measurable shape adjustment and no rod/greeble additions. Avoid redoing already completed Kestrel shoulders/ground.
- Specify how PM can review original uncompressed PNGs from a GitHub artifact/committed QA path without assuming access to producer concept art; avoid mass screenshot duplication or private data exposure.
- Confirm no email is sent; producer explicitly deferred notification by email until later.
- No source changes or builds in this check. If tools cannot access verified candidate, say so; do not invent screenshots.
