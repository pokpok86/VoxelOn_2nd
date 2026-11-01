---
id: TASK-20261102-BUILD-RECOVERY-026
mode: check
status: ready
priority: 98
title: Recover latest approved game candidate build and implementation pipeline
---

## Goal
Producer explicitly wants overdue development/build requests delivered, not another cycle of generic design. Assess what can be done safely now, and identify the shortest concrete route to an updated playable Windows build.

## Approved baseline and constraints
Latest verified game candidate is codex/implementation/2026-11-01-sd-ground SHA b757bcca068acbf59cd0eed4f97910246202f2bb (Unity build PASS, player smoke 209 PASS, 36 screenshots); manual play and concept-art fidelity unverified. Old source f808e5952fee must NOT be used as a fallback. The 009 jump change previously aborted because nested git unavailable. Do not claim 009 completed. Do not re-run completed IDs.

## Work
1. Read 009, 023, 024 reports and current queue/runner configuration available; report exactly whether pinned full SHA and git PATH can be verified. No edit unless approved full SHA is positively validated in the OUTER runner and nested worker. If cannot verify, stop change safely and produce exact owner-facing repair commands/steps.
2. Once a safe runner path is available, propose a new isolated change task for **weight-based ordinary jump, Space-hold booster ascent, release and re-press midair**, and small independent Kestrel silhouette improvement (real shoulder/head mass, no decorative rods). Resolve conflict with old fixed 2x jump task; do not simultaneously implement conflicting specifications.
3. For any actual change: isolated branch from exact SHA, Unity Windows build, player smoke, screenshots same pose/camera before-after, report exact commit/branch/logs, retain last known-good executable. If cannot run Unity or source SHA check, say no build was produced.
4. Do not modify main, merge, send emails, invent tests, or finalize concept art approval. Report concrete next actions, not generic design repetitions.
