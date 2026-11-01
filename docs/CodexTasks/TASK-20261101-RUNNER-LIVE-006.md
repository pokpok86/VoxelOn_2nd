---
id: TASK-20261101-RUNNER-LIVE-006
mode: change
status: ready
priority: 90
title: Prove fresh change task build smoke and candidate publication with harmless QA documentation
---

## Why
The installed poller demonstrably consumed four new check tasks and published reports. The new change -> isolated worktree -> Unity build -> built-player smoke -> candidate GitHub push path remains untested. The configured source branch in the runner candidate's example is feature/visual-polish-20261031, not the verified Kestrel/ground candidate. Therefore do not touch robot, map, game code, tests or scene in this first E2E test.

## Authorized narrow change
Add exactly one new Markdown file `Docs/QA/OVERNIGHT-RUNNER-E2E-20261101.md` in the isolated task worktree. Its content should describe the objective QA facts: game candidate codex/implementation/2026-11-01-sd-ground was previously reported to pass Unity Windows build and 209 player smoke checks with 36 images; manual play and concept-art match are unverified. State the new file is a harmless E2E runner probe, not a gameplay improvement. Include the exact source SHA the runner provides and note whether it is or is not the game candidate. Do not claim these previous validation results were re-run unless the runner actually executes them.

## Strict acceptance
1. Create the single documentation file only, no other intentional edits.
2. Let the runner (not Codex) perform its standard Windows Unity build and built-player smoke after this change. Do not weaken or remove any assertions, and do not bypass validation.
3. Only if both checks pass may the runner commit and push the unique `codex/implementation/task-20261101-runner-live-006` review branch. Never push or merge to main.
4. Record source SHA, review SHA, actual validation markers/check counts, any incidental Unity-generated files, and status transition evidence under codex/task-reports. If build or smoke fails, keep the failed result and do not publish a candidate.
5. Report whether the installed runner is demonstrably using the newer auto-publish path; if it only leaves local changes, report that instead. No runner configuration, scheduler, permissions or source branch edits.

## Follow-up only, not authorized here
Once this E2E succeeds, PM will arrange a separately scoped source-base migration to the verified game candidate before requesting more Kestrel silhouette implementation. The producer still must approve final visual silhouette and any main merge.
