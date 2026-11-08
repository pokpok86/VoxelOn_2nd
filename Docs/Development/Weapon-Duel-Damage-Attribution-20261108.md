# Weapon duel damage attribution — 2026-11-08

The duel study now records each fighter's actual damage credited through the normal `Hit` path. Detailed CSV rows include damage dealt by both weapon profiles, and the summary reports mean damage dealt per match alongside win points, remaining HP, and duration. This gives follow-up review a way to distinguish low damage output from a weapon that deals damage but loses on the match timer.

Damage includes health and shield depletion after armor and guard reduction. Hits fully absorbed by the salvage-shield early return are not credited; the default WISP duel setup does not equip that shield. Damage is attributed to the fighter that owns the hit, so the duel's equipped profile receives credit. Existing duel defaults and `PD-Balance.json` are unchanged.

## Validation

- `git diff --check`: passed. Reviewed the changed fields, `Hit` accounting, CSV header/row alignment, and summary aggregation.
- Unity build and duel rerun: not performed in this iteration. Iteration 36 records that the Unity 6000.6.7f1 build failed before compilation because no valid editor license was available. Only Unity Hub is currently running; no Editor or game player process is running.
