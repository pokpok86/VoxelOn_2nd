---
id: TASK-20261102-RUNNER-PREFLIGHT-021
mode: check
status: ready
priority: 2
title: Audit runner-side immutable source SHA preflight for gameplay changes
---

## Why
JUMP-IMPLEMENT-009 stopped without edits: nested Codex environment cannot run git; many checks used stale f808e5952fee, while verified game candidate is b757bcca068acbf59cd0eed4f97910246202f2bb. E2E-006 did publish from stale source. This task is distinct from general runner E2E: identify a safe **runner-side** source-ref guard that does not depend on nested Codex git access.

## Check only; no runner modifications
- Inspect runner implementation/config on codex/implementation/task-20261101-runner-autopublish and local task pickup; distinguish runner's Git capability from nested Codex tool restrictions.
- Determine exact SHA pinning protocol for next mode:change task: expected_source_sha = b757bcca068acbf59cd0eed4f97910246202f2bb, checkout and verify full HEAD, clean working tree, and preflight failure if mismatch; pass immutable verified SHA evidence to nested Codex in accessible file/env. Check whether queue frontmatter supports source_sha or if runner must be patched first.
- Identify source of f808e5952fee default and whether all change tasks share it; distinguish cached task source vs branch checkout. Do not assume Git PATH inside Codex proves runner cannot validate.
- Produce minimal implementation plan + exact acceptance checks, and note if local runner/Windows access unavailable.
- NO code changes, no source branch switch, no main changes, no claim of Unity tests.
- Visual sidecar: identify one small independent, non-decorative SD silhouette improvement from existing 019 report (e.g. Bastion helmet width) that can follow source pinning, with same-angle before/after capture acceptance. Do not edit visuals here.
