---
id: TASK-20261102-RUNNER-SOURCE-CONFIG-023
mode: check
status: ready
priority: 90
title: Runner source-branch drift: evidence and safe maintenance handoff
---

## Confirmed PM GitHub evidence (do not re-audit inaccessible files)
At commit dc21297e7e57a5fe03fe731d8b2e86f9c70f6350 on codex/implementation/task-20261101-runner-autopublish, Scripts/CodexTaskRunner.settings.example.json has SourceBranch = feature/visual-polish-20261031. Scripts/CodexTaskRunner.ps1 Process-OneTask (around lines 390-460) calls Fetch-Branch $Settings.SourceBranch and uses that same sourceRef for every worktree, including mode: change. Task metadata parser supports id/mode/status/priority but does not use an expected source SHA. Prior reports all show source f808e5952fee; latest approved game candidate is full SHA b757bcca068acbf59cd0eed4f97910246202f2bb on codex/implementation/2026-11-01-sd-ground. These are not interchangeable.

## Task (read-only; maintenance handoff)
1. Assess precise safe runner patch and config migration to support full expected_source_sha per change task, or a mandatory pinned game candidate SHA set in trusted runner config; prefer both with exact equality checks. Require no fallback, clean worktree verification in outer runner, and durable evidence injected into nested Codex.
2. Explicitly acknowledge worker policy prohibits edits to runner files and cannot inspect settings outside worktree. Do NOT try to bypass or edit installed runner. Give exact maintenance steps for separately authorized local runner owner, including existing Windows scheduled task configuration and rollback.
3. Define unit and integration tests for stale SourceBranch, missing/mismatched SHA, dirty worktree, missing git PATH in nested worker, and legitimate pinned build candidate. Do not weaken current 11 self-tests.
4. Document whether changing SourceBranch to codex/implementation/2026-11-01-sd-ground is only an interim fix (it is: branch can move and isn't full SHA pin). Note existing processed task IDs cannot simply be replayed; a new task ID is required.
5. Include minimal separate visual enhancement suggestion (Kestrel head/shoulder proportions with same-angle before/after captures) to execute only AFTER safe pinning. No actual change, build, email, or main edits.
