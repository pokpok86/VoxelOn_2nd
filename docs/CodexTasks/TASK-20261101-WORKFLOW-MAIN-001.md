---
id: TASK-20261101-WORKFLOW-MAIN-001
mode: check
status: ready
title: Review proposed main-first workflow without changing main
---

# TASK-20261101-WORKFLOW-MAIN-001

Priority: high
Requested by: project owner

## Goal
Simplify VoxelOn development to a main-first, build-and-test workflow. The owner has expressed a preference for working on main and using Git history to roll back regressions instead of accumulating feature branches. Treat this as a proposal requiring separate explicit authorization, not permission to change main or runner behavior now.

## Instructions
1. Inspect the current local Codex runner, task queue polling, Git working tree, and any AGENTS.md instructions. Explain whether this queued request can safely change the runner's future workflow. Do not assume the task runner is authorized to push main.
2. Before any future gameplay/visual code change, record the current main commit SHA and verify that the worktree is clean; preserve uncommitted work and report conflicts rather than discarding anything.
3. Proposed steady-state flow: start from latest main -> make one small change -> Unity Editor Windows build -> automated smoke tests -> capture fresh screenshots when relevant -> commit/push to main ONLY when all gates pass and permissions/branch protections allow AND the owner has separately approved that workflow. If any gate fails, leave main unchanged and report logs and recovery steps.
4. Reuse existing task queue and report branches for coordination until the workflow is safely migrated. Avoid new per-task branches unless needed for failure isolation. Do not force push, delete branches, reset, or merge unknown work.
5. Identify whether previously reported Kestrel visual changes and recent collision/jump build changes are present in main; do not overwrite or silently discard either. Do not claim integration without checking.
6. Make rollback straightforward: small descriptive commits, record before/after SHAs, keep last known-good build marker, and explain non-destructive revert.
7. Do not commit generated Windows build binaries, logs, secrets, or bulky screenshots by default; if screenshots are requested, use a consistent reviewed gallery path and avoid overwriting baseline evidence.

## Authorization boundary for THIS task
This is a read-only workflow review. Do NOT modify any project files or local runner settings. Do NOT modify or push main. Do NOT perform a merge or force push. Report exactly what configuration/permissions would be needed for future direct-main development and whether the owner needs to approve a local runner setting change. The producer's final approval remains separate.

## Acceptance criteria
- Report current main/runner status, risks, proposed minimal settings, and exact next steps.
- Make no unverified claims about builds, visual integration, or runner changes.
- No project files changed.
