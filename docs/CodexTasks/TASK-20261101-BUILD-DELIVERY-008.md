---
id: TASK-20261101-BUILD-DELIVERY-008
mode: check
status: ready
priority: 12
title: Specify latest playable Windows build retention after every approved change
---

## Objective
Producer requests a usable up-to-date Windows executable after every completed game-code change. Audit installed runner and candidate runner build pipeline, artifact paths, version stamps, last-known-good executable, retention, log files, and whether latest built EXE corresponds to candidate SHA. Produce an implementable safe policy: for every game-code change, run Unity Windows build and built-player smoke; on success publish candidate code and preserve timestamped or SHA-named playable build in a designated local folder, plus an explicit LATEST-VERIFIED pointer/manifest with branch, full SHA, timestamp, 209-vs-208 test count and result, and screenshot/log paths; preserve last good build and never replace it on failure. Avoid assuming builds can be stored in GitHub (size limits) or that the existing runner candidate is installed. Distinguish runner-only/documentation-only changes (self-tests sufficient unless requested). If existing behavior already meets the goal, report evidence; otherwise draft a bounded runner change task and precise acceptance criteria. No main changes, no runner configuration changes in this check.

## Delivery and safeguards
Use existing GitHub candidate evidence and record exact source branch/SHA. Do not overwrite the user's local build or dirty worktree, change main, merge, or alter runner permissions. Report findings and actionable follow-up to codex/task-reports. Avoid repeating completed game collision or Kestrel tasks.
