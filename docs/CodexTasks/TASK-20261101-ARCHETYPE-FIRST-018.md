---
id: TASK-20261101-ARCHETYPE-FIRST-018
mode: check
status: ready
priority: 3
title: Refocus prototype selection on movement archetypes, not eight fixed characters
---

## Producer's authoritative scope change
Existing eight visually distinct robots have little meaningful gameplay difference. **Stop treating them as eight fixed character classes that each need separate balance, unique abilities, or final visual polish.** For current gameplay/visual validation, selecting **movement archetype only** is sufficient. Later, individual builds/identities emerge organically from core, legs, head, weapons and other part combinations. Preserve existing eight assets/data where useful; do not delete them or assume they are final character roster.

## Prototype targets
- Humanlike agile runner: mouse/camera/mech facing aligned, fast ground leap/dash without boost energy consumption, Space boost ascent/reboost uses energy.
- Tank/tread: independent upper aim, chassis-forward-only high-speed traversal.
- Quadruped: independent upper aim, no high-speed traversal.
- Other biped forms may support independent upper/lower rotation; not all bipeds are humanlike.
- Head and weapon determine lock-on capabilities; no leg-based weapon equip gates.
- Avoid premature core/leg weight restrictions; document candidates for later balancing only.

## Deliverables
1. Inspect current candidate codex/implementation/2026-11-01-sd-ground (verify full SHA) and task MOBILITY-ARCHETYPES-017. Identify the smallest reversible prototype selection UI/data change to expose only the 3 movement archetypes while retaining the eight original robot definitions/assets internally. If full three-way prototype is not feasible yet, plan a 1-2 robot A/B test first.
2. Define minimal vertical slice to validate handling and visuals per movement archetype; specify what should be common (camera, input, attack, jump/boost, target lock) vs unique (ground dash/drive, torso yaw).
3. Propose acceptance criteria for real Windows executable and screenshot/video comparisons; do not claim final art or balance approval without producer playtest. Preserve existing verified Kestrel/ground work and all smoke assertions.
4. Create a concise implementation sequence avoiding conflict with pending jump, target-facing, and leg movement tasks. No eight-character-specific ability work, no premature garage/customization overhaul, no broad destructive refactor.
5. Check/design only; do not edit game source, main, or merge. Report findings and safe implementation-ready task breakdown to codex/task-reports.

## Decision hierarchy
This producer instruction supersedes any older plan that assumes the eight initial robots must each become a distinct finished character.
