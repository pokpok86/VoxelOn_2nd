---
id: TASK-20261101-OVERNIGHT-INTEGRATION-005
mode: check
status: ready
priority: 25
title: Plan safe integration of verified game and runner candidates
---

## Goal and acceptance
Audit candidate game branch codex/implementation/2026-11-01-sd-ground and runner branch codex/implementation/task-20261101-runner-autopublish against main and task-queue, including source SHAs and changes. Check whether the installed scheduled runner is already running the candidate script, and whether new ready tasks can build on the game candidate instead of stale main; do not assume either. Provide a precise non-destructive method to avoid losing Kestrel and ground fixes when starting next task, and a staged approval plan for eventual merge. No merge, no pushes of project code, no runner configuration or permission changes, and no final silhouette approval. Distinguish proven 209-check prior pass from untested combined-branch results. Report blockers and recommend the smallest safe next implementation task.

## Safety and reporting
Read-only check in an isolated environment. Report precise source refs, evidence, limitations, proposed next tasks and any producer approval gates to codex/task-reports. Never change main, existing producer worktrees, runner settings, or game balance. Do not redo an already completed task merely because its queue file still says ready.
