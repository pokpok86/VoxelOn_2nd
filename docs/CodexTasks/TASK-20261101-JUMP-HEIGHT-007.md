---
id: TASK-20261101-JUMP-HEIGHT-007
mode: check
status: ready
priority: 30
title: Investigate low jump height on the latest game candidate
---

## Objective
Producer manual playtest: ground/obstacle placement and rooftop support look improved, but jump still feels too low. Inspect jump impulse, gravity, air control, boost/dash, camera framing, and obstacle heights in codex/implementation/2026-11-01-sd-ground at commit b757bcc (verify full SHA). Measure jump apex above ground, flight time, max obstacle climb and any per-frame differences using source and existing tests; compare with baseline f808e5952fee. Determine whether producer's approximately 22:00 executable was built from this candidate or an older commit if build metadata is available; do not guess. Propose 2-3 safe, adjustable jump-height options and regression criteria. Do not change jump values or combat feel until producer chooses the target. Report exact code paths and measured numbers; mark unmeasured as unverified.

## Delivery and safeguards
Use existing GitHub candidate evidence and record exact source branch/SHA. Do not overwrite the user's local build or dirty worktree, change main, merge, or alter runner permissions. Report findings and actionable follow-up to codex/task-reports. Avoid repeating completed game collision or Kestrel tasks.
