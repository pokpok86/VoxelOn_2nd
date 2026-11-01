---
id: TASK-20261102-QUEUE-DELIVERY-025
mode: check
status: ready
priority: 99
title: Urgent runner queue discovery and red error diagnosis
---

## Goal
Producer reports Windows runner console flashes every 10 minutes, sometimes with red error text, and no new jobs picked up. Diagnose from available runner reports and repository evidence. **Do not assume console means runner success.**

## Work
1. Verify actual QueueBranch and task enumeration behavior in runner source available in worktree. GitHub queue branch codex/task-queue contains many files in lowercase docs/CodexTasks and legacy uppercase Docs/CodexTasks. Runner source regex uses (?i) and should recognize either spelling. Distinguish code from actual installed runner settings, which may be inaccessible.
2. Inspect accessible logs/state/QUEUE-STATUS and report the last processed ID and exact reason new queued IDs were not picked up. If runtime log/installed config inaccessible, say so, give exact safe local read-only diagnostic commands and needed screenshots; do not guess.
3. Flag misleading status: ready task files can be already processed, so compare IDs against ledger/reports. Do not re-run completed IDs.
4. Give a concrete, minimally invasive repair handoff for owner to enable reliable per-run error logging without adding pause to scheduled execution. Avoid secrets and private data in logs.
5. Do not modify installed scheduled task, runner, main, or gameplay. Publish read-only report with evidence and unblock steps. Identify one safe incremental SD visual follow-up once pipeline works.
