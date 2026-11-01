---
id: TASK-20261101-OVERNIGHT-VISUAL-QA-004
mode: check
status: ready
priority: 20
title: Validate Kestrel camera framing and 36 capture evidence
---

## Goal and acceptance
Review published 36-image gallery and candidate branch documentation; verify actual file counts, front/side/back framing, whether the current capture camera differs from actual play (FOV 60 pitch 12 distance 8.5 in prior analysis), and whether side-by-side framing is equivalent. Define exact deterministic capture cases for Kestrel idle/walk/boost/jump, in-game target-lock camera, ground and rooftop support, and close-up of feet/shoulder/weapon socket. Prefer using existing artifacts, and list any captures that are missing or visually ambiguous. Identify obvious clipping or oversized helmet issues only if directly supported by inspected images. Do not assert visual quality from text alone. Do not alter source or main. Produce an actionable capture/QA task specification that can run after safe candidate integration.

## Safety and reporting
Read-only check in an isolated environment. Report precise source refs, evidence, limitations, proposed next tasks and any producer approval gates to codex/task-reports. Never change main, existing producer worktrees, runner settings, or game balance. Do not redo an already completed task merely because its queue file still says ready.
