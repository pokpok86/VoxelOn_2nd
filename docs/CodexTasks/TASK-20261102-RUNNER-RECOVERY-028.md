---
id: TASK-20261102-RUNNER-RECOVERY-028
mode: check
status: ready
title: Verify restarted Windows runner and work branch handoff
---

## Goal
Recover from failed TASK-20261102-RUNNER-PIPELINE-027 (invalid metadata) and verify the actual running Windows runner, not merely a design. The owner says the runner has been restarted. This is a diagnostic check; do not claim code changes or Unity builds that did not occur.

## Required checks
1. Report the actual runner scheduler status, effective QueueBranch and SourceBranch, installed script version, and current work HEAD. Expected GitHub game development branch: work; previous verified baseline: b757bcca068acbf59cd0eed4f97910246202f2bb. Work may contain a newer documentation commit; do not hard-reset it.
2. Verify git is available in both runner and spawned Codex worker PATH. If unavailable, give the exact minimum Windows configuration/approval needed; do not claim a Git/Unity validation pass.
3. Verify that the runner discovers ready tasks under docs/CodexTasks and Docs/CodexTasks without confusing case, and that its durable state prevents duplicate execution. Confirm whether the newly registered 027 failed and whether this 028 was collected.
4. Check whether the installed pipeline actually performs developer -> CM -> Unity build -> report. Identify missing capabilities and the next bounded implementation step. Every game code change must trigger a real Unity build attempt, or be explicitly reported blocked.
5. Report what work is currently active, with task ID, start time, branch, source SHA, build status and next action, if verifiable. If active process cannot be observed from worker sandbox, state that limitation rather than guessing.

## Constraints
- Read-only check. Do not modify game code, change credentials, alter scheduler, restart/stop runner, or touch main.
- Do not expose tokens or sensitive machine paths.
- Do not mark completed implementation merely because a Markdown report exists.
- Publish an honest report to the existing task-reports mechanism.