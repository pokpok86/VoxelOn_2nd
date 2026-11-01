---
id: TASK-20261102-RUNNER-PIPELINE-027
status: ready
priority: critical
type: implementation
---
# VoxelOn runner pipeline recovery and unattended development

## Objective
Implement and VERIFY a reliable developer -> CM -> Unity build -> report pipeline, starting with the actual Windows runner environment. Do not close as completed based only on a design document. Preserve all existing game sources, the known-good build and main branch.

## Phase 1: preflight (mandatory)
1. Inspect the *installed* Windows scheduled task, runner script/version, effective working directory, queue branch and queue folder (including Docs/docs case), logs, actual Windows user identity, Git executable/PATH in the spawned Codex environment, GitHub authentication, Unity executable/license/batchmode, and Codex approval/sandbox settings. Do not assume repository examples match installed settings.
2. Confirm the latest approved game commit (previously reported b757bcca068acbf59cd0eed4f97910246202f2bb) is reachable, compare it to the actual source branch, and record exact SHA. Never silently build from old f808e5952fee.
3. Perform minimal read-only checks first; test pull, isolated trivial edit, push to an authorized non-main branch, and Unity batch build where safely possible. Do not change credentials, disable security, bypass approvals or use unrestricted access.
4. If permission, interactive login or administrator action is required, STOP that step and report exact UI action, machine, command, error, and required minimum scope. The owner can approve via remote desktop later. Continue independent safe checks. Never include secrets in reports.

## Phase 2: simplify, only after verified preflight
- Target only main (stable) and one persistent work branch for game development. Preserve legacy branches and histories; no destructive branch deletion or force push. Keep task-queue and task-reports infrastructure operational until a tested migration, rather than breaking delivery.
- One developer execution at a time. Same workflow runs Developer followed by CM, sequentially, not competing writes.
- 1-minute queue polling if the installed scheduler supports it; enforce a durable single-instance lock, unique task IDs, state transitions, and recovery after crashes. Polling interval does not determine the hourly PM request schedule.
- Developer completes changes; CM immediately checks changes, commits and pushes to work, attempts Unity build for EVERY game source/config change (no discretionary skip), runs smoke tests, and reports exact commit/build status.
- If build fails, hand error log to developer for bounded fix/rebuild attempts; distinguish code errors, environment errors and approvals. Never mark blocked work as successful.
- At 30 minutes from developer start, run mandatory status inspection/report. Do not build a moving/incomplete workspace. If developer is still active, report in-progress and arrange CM build upon completion. Do not launch simultaneous conflicting work.
- Only promote verified commits to main after appropriate owner approval; do not auto cherry-pick into main without that approval.
- Avoid overwriting a single task file in a way that loses pending requests: use durable IDs, append-only history or an equivalent safe queue.
- On completion, publish a Markdown report containing start/end time, source SHA, changed files, push URL, build command and result, smoke tests, artifacts, errors, and pending approvals. Distinguish attempted vs verified.

## Phase 3: notifications
- Design optional Telegram notification on build success/failure, blocked approval, retry exhaustion and verified before/after PNGs. Bot token/chat ID are NOT available yet. Do not enable sending until owner supplies credentials through secure local configuration. Never commit tokens or IDs to GitHub.

## Acceptance criteria
- Actual installed runner evidence, Git executable verification, correct starting SHA, safe work branch push, a real Unity build attempt and truthful report. Show logs and errors even if blocked.
- If current worker cannot access Windows runner or Unity, do NOT claim implementation complete; return a precise handoff/checklist to the local runner maintainer and mark blocked.
- No modifications to main, no destructive Git operations, no security approval bypass, no Telegram messages until configured.
