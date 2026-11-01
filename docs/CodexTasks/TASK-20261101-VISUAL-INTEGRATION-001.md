---
id: TASK-20261101-VISUAL-INTEGRATION-001
mode: check
status: ready
title: Verify Kestrel visual patch integration and prepare safe Unity validation
---

## Goal

Before making more visual changes, determine whether the Kestrel silhouette edits from TASK-20261101-VISUAL-001 are actually present in the current game source or remain only in its isolated worktree. Establish a safe, actionable path for visual verification using the existing Unity 6000.6.7f1 build entry point and screenshot gallery.

## Scope

- Read TASK-20261101-VISUAL-001 and BUILD-AUDIT-001 reports if available, and inspect the local project/worktree paths accessible to the runner.
- Compare the Kestrel geometry in the current baseline source against the claimed changes; document exact evidence. Do not assume the patch is integrated.
- Check whether the configured Unity Editor executable and Git are available within authorized workspace paths. Do not search outside permitted paths or circumvent sandbox constraints.
- Identify the smallest safe way for the owner to apply the existing visual patch to a separate validation copy and run baseline-versus-patch capture at identical camera settings.
- If Unity is available and execution is authorized in this isolated workspace, perform non-destructive verification only; otherwise report explicit blockers. Never falsely claim a build or fresh screenshot.
- Do not create additional cosmetic geometry until patch integration and build status are clear.

## Acceptance criteria

- Report whether Kestrel patch is integrated, isolated-only, or unknown, with file/path evidence.
- Report Unity build and gallery availability honestly, including exact failure category.
- Provide concise next action to enable a real build and side-by-side screenshots.

## Constraints

- Read-only check. Do not edit, commit, push, merge, overwrite screenshots, or change main.
- Do not access personal files, credentials, or unauthorized directories.
