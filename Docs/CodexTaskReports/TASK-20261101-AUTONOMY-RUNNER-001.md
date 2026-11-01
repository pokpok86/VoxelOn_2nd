# Autonomous runner implementation report: TASK-20261101-AUTONOMY-RUNNER-001

- Status: implementation published for PM review
- Candidate branch: `codex/implementation/task-20261101-runner-autopublish`
- Commit: `dc21297e7e57a5fe03fe731d8b2e86f9c70f6350`
- Project main: unchanged; no merge performed
- Runner self-tests: PASS, 11 checks
- PowerShell syntax parse: PASS for runner and scheduler installer
- `git diff --check`: PASS
- Scheduled poller: ENABLED, interactive sign-in, repeats every 10 minutes, two-hour task limit
- Latest scheduled poll: PASS, Last Result 0; no unseen ready task found; queue is idle
- New live change task through build + smoke + candidate push: not exercised because the queue contains no unseen ready change task

## Changes

- Each 10-minute poll now fetches again and drains approved unseen `status: ready` work in priority order (1–100, default 50) until the queue is empty or the 110-minute run budget is reached. The Windows task's existing two-hour execution limit leaves a short safety margin.
- A lock file prevents concurrent runner instances. Task IDs remain consumed after completion or failure to prevent duplicate execution.
- The prompt explicitly forbids deleting, weakening, bypassing, or reducing existing test assertions to obtain a pass.
- Change tasks run in a separate worktree. The runner performs a Windows Unity build and the built player's smoke test; only after both pass does it commit and push to the unique `codex/implementation/<task-id>` candidate branch.
- Start, progress, validation, candidate publishing, and final status are appended under `Docs/CodexTaskReports/Status/<task-id>.md`; final reports include changed files, commit, validation markers/counts, screenshot paths, and failure detail.
- Pending status and final reports persist locally and retry on subsequent polls.
- Corrected report path casing to match GitHub's case-sensitive API. The queue scanner matches both `Docs` and `docs` while preserving the exact GitHub path for file reads.
- Updated the scheduler installer to preserve existing local settings while filling runner/build defaults and refreshing the installed runner copy.

## Files changed

- `Scripts/CodexTaskRunner.ps1`
- `Scripts/CodexTaskRunner.settings.example.json`
- `Scripts/Install-CodexTaskScheduler.ps1`
- `docs/CodexTasks/README.md`
- `docs/CodexTasks/TEMPLATE.md`

## Verification evidence

- Runner self-test: `VOXSKY_RUNNER_SELFTEST_PASS 11 checks`.
- PowerShell parser accepted both scripts; `git diff --check` passed.
- Actual poller was launched through Task Scheduler at 2026-11-01 22:21 KST. `schtasks /Query` confirmed it is enabled, interactive-only, repeats every 10 minutes, and returned Last Result 0. The runner log shows a successful queue fetch and idle result.
- An earlier poll attempt without the required repository write access failed because Git could not write `.git/FETCH_HEAD` (`Permission denied`). Retrying with the approved repository access succeeded. No broad or permanent permissions were added.
- The scheduled task requires the user to be signed in; the computer and interactive session must remain available for automatic runs.
- No Unity build was run for this runner-only change. The separate game candidate has its own verified build and 209-check player smoke report: [TASK-20261101-SMOKE-REPAIR-001](https://github.com/pokpok86/VoxelOn_2nd/blob/codex/task-reports/Docs/CodexTaskReports/TASK-20261101-SMOKE-REPAIR-001.md).

## Candidate review

https://github.com/pokpok86/VoxelOn_2nd/tree/codex/implementation/task-20261101-runner-autopublish

> This is a candidate branch only. No code was written to `main`, no merge was performed, and no force push was used. A new ready queue item is still required to exercise the complete fresh change-task-to-Unity-build-to-candidate-push path end to end.
