# Overnight runner E2E probe — 2026-11-01

Task: `TASK-20261101-RUNNER-LIVE-006`.

This single documentation file is a harmless E2E runner probe, not a gameplay improvement. It exercises the runner's change, isolated worktree, Windows Unity build, built-player smoke, and conditional review-branch publication path without changing gameplay, scenes, tests, or assertions.

## Previously reported game-candidate QA

The game candidate `codex/implementation/2026-11-01-sd-ground` was previously reported to pass a Unity Windows build and 209 built-player smoke checks, with 36 images. These are historical reported results, not validation rerun by this documentation change. Manual play and concept-art match remain unverified.

## Source identity and evidence limitation

Exact runner-provided source SHA: **unavailable in the task input and accessible worktree evidence**. It must be supplied and verified by the runner before this probe satisfies source-identity acceptance; no SHA is invented here.

The task identifies the runner candidate's example source branch as `feature/visual-polish-20261031`, which is not the named game-candidate branch `codex/implementation/2026-11-01-sd-ground`. The actual checked-out source SHA and its equivalence to the game candidate could not be verified: `git` is unavailable on PATH, and the worktree's Git metadata points outside the authorized worktree. The example branch alone does not prove commit identity.

## Runner-owned follow-up

Codex has not run a Unity build or built-player smoke for this change, committed, pushed, or changed runner configuration. The runner must perform its standard Windows Unity build and built-player smoke with all existing assertions intact. Only after both pass may the runner publish the unique review branch `codex/implementation/task-20261101-runner-live-006`; main must not be pushed or merged. A failed check must remain recorded and must prevent candidate publication.

The runner must record the exact source SHA, review SHA, actual validation markers and check counts, any incidental Unity-generated files, and status-transition evidence under `codex/task-reports`. No report files are created by this documentation-only probe. No current validation markers, review SHA, or publication evidence are available at authoring time, so use of the newer installed auto-publish path is not yet demonstrated; only this local documentation change has been made.

Migration to the verified game candidate and further Kestrel silhouette work require separately scoped follow-up. Final visual silhouette and any main merge still require producer approval.
