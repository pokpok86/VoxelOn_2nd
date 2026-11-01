---
id: TASK-20261101-AUTONOMY-E2E-002
mode: check
status: ready
priority: 10
title: Verify autonomous task handoff and candidate-branch safety without changing game code
---

## Context
The producer has received the published implementation and runner reports:
- `Docs/CodexTaskReports/TASK-20261101-SMOKE-REPAIR-001.md`: game candidate `codex/implementation/2026-11-01-sd-ground`, Windows build PASS, 209 player-smoke checks PASS, 36 captures, manual gameplay unverified.
- `Docs/CodexTaskReports/TASK-20261101-AUTONOMY-RUNNER-001.md`: runner candidate `codex/implementation/task-20261101-runner-autopublish`, 11 self-tests PASS; new ready task execution not yet end-to-end verified.

This is a **new task ID** specifically to verify that a fresh GitHub queue item is picked up by the installed scheduled runner. Existing ready files from prior work must NOT be replayed. Read and respect the task ID ledger.

## Objective
Run a read-only, non-destructive acceptance check for the current automatic handoff, status reporting, and published candidate evidence. This task itself must not edit game code, enable new permissions, or update main.

## Acceptance criteria
1. Log the exact task ID, source ref/SHA, start/finish UTC time, installed runner version or script checksum (if observable), and which queue/report paths the installed runner actually uses. Distinguish installed scheduled-runner code from unmerged runner candidate code; do not assume candidate features are active.
2. Confirm the task is seen only once and status/final report are published to `codex/task-reports`; provide exact report path and any observed poll/lock/ledger data. Do not force a second execution of this ID.
3. Read both candidate reports and verify candidate branch heads exist (if git/GitHub access allows). State clearly that 209 smoke checks and 36 captures are **previous evidence**, not tests re-executed by this read-only check.
4. Identify blockers to a live `mode: change` -> isolated edits -> Unity build -> built-player smoke -> candidate branch publication E2E. Recommend a **separate small change task** as the next exercise, but do not invent or execute unauthorized game changes or alter the runner in this check.
5. If this is processed by the old runner and modern status reports are not emitted, state the limitation clearly; never claim that the new runner is active without evidence.
6. Report requested producer approvals: no merge to main, no approval of final Kestrel silhouette, no direct changes to user's working tree.

## Safety
Read-only; no edits, git push of game or runner sources, no build or screenshots, no permission changes, no main merge. Normal runner-generated status/final report to `codex/task-reports` is permitted. Failure to access external files should be recorded as a blocker, not bypassed.
