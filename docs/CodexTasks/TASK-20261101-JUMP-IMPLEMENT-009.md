---
id: TASK-20261101-JUMP-IMPLEMENT-009
mode: change
status: ready
priority: 5
title: Double Kestrel jump apex on verified latest game candidate and preserve playable build
---

## Producer approval
Producer explicitly approves increasing **jump apex approximately 2x** relative to current baseline, with movement feel and airtime validated. This approval is limited to jump/mobility tuning and directly related regression tests. It does not authorize changing combat rules, final Kestrel silhouette, main, or runner permissions.

## Mandatory source baseline and preflight
- **MUST** start from game candidate `codex/implementation/2026-11-01-sd-ground` at reported commit `b757bcc` (resolve and record full SHA). This contains Kestrel silhouette and ground/obstacle fixes validated by `VOXSKY_BUILD_PASS`, `VOXSKY_SMOKE_PASS 209 checks`, and 36 images.
- Verify HEAD equals candidate SHA, the expected ground/roof and Kestrel fixes are present, and worktree is clean/isolated. Do not silently fall back to `main`, `feature/visual-polish-20261031`, or `f808e5952fee`.
- If installed runner does not support selecting this source ref/SHA, **STOP before editing**, report exact blocker, and prepare a narrowly scoped runner base-ref fix for separate authorization. Do not overwrite producer's worktree.
- Existing queue task JUMP-HEIGHT-007 is a diagnostic; use its findings if available, but this task has explicit producer approval to implement without waiting for another reply.

## Implementation
1. Record baseline jump apex (world-space feet height relative to support surface), flight time, horizontal travel, landing velocity, roof/step collision, and differences across the 8 frames if jump mechanics vary. Derive target **2x baseline apex** within practical tolerance (e.g. 1.9x–2.1x) rather than blindly doubling impulse; preserve stable gravity/controls as far as possible.
2. Make minimal, reversible change to jump impulse/trajectory with tuning constants. Do not make jump feel floaty; if air time changes materially, measure it and explain trade-offs. Preserve dash/boost, target lock, projectile origins, collision and roof landing.
3. Add regression assertions for measured apex ratio, safe landing on ground and obstacle roofs, no feet penetration, jump input behavior and existing smoke tests. Never weaken, delete or bypass existing assertions.
4. Run Windows Unity build and **built-player smoke test** on the new candidate. Preserve exact marker/count, logs, source SHA, build timestamp, and output location. If possible preserve a separate timestamped playable EXE and a latest-verified manifest without replacing last known good; do not invent artifact paths or upload huge binaries to GitHub.
5. Publish only to unique candidate `codex/implementation/task-20261101-jump-implement-009` after PASS, with code commit and report in `codex/task-reports`. Include measured baseline/after apex, airtime, horizontal distance, 8-frame coverage, exact source and result SHAs, file changes, test markers/counts, and remaining manual playtest needs. Never merge or edit `main`.

## Failure policy
If the source SHA cannot be pinned, build environment is unavailable, or validation fails, stop safely, keep existing verified build intact, and report concrete cause and next steps. Do not report implementation complete without build and player smoke evidence.
