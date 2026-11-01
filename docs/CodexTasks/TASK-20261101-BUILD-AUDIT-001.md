---
id: TASK-20261101-BUILD-AUDIT-001
mode: check
status: ready
title: Audit Unity build environment and screenshot pipeline
---

## Goal

Read-only investigation of why the local Codex task runner cannot validate Unity builds, and document a reproducible safe verification workflow for the real Windows Unity project. This follows completed VISUAL-001 and VISUAL-002 reports, both of which explicitly did NOT perform Unity compilation or playtests.

## Scope

- Inspect only project-local build scripts, Unity project metadata, CI workflows, README, and existing screenshot tooling within the isolated worktree.
- Identify the expected Unity Editor version, build target, entry scene, build invocation method, output location and required configuration, using evidence rather than guesses.
- Distinguish the runner sandbox from the owner's actual Windows PC. Check whether Unity Editor, git, and build scripts are discoverable in this runner; report exactly what was checked and any limitations.
- Propose a safe minimal build-and-capture procedure for the Windows host. Do not attempt to install tools or alter environment variables.
- Explain how to compile/test the pending Kestrel changes from VISUAL-001 without assuming they were integrated. Recommend baseline build first, then isolated changes, then screenshots at matched camera settings.

## Acceptance criteria

- Report actual build environment facts with paths and project references if available.
- If Unity is available inside this isolated runner, attempt only a non-destructive validation with existing documented commands; otherwise explicitly mark build unverified.
- Report exact reasons for any build failure and suggested fixes without claiming success.
- No source edits, project configuration edits, external writes, commits, pushes, merges, or changes to main.

## Constraints

- This is read-only and must not touch credentials or personal directories.
- Do not modify or overwrite any existing task or report.
- Do not claim that running static checks equals a successful Unity build.
