# Chassis-aware weapon duel study (2026-11-08)

## Change

Expanded the existing PD-data-based weapon study to compare the three selectable chassis pairings: WISP vs BASTION, WISP vs TARANTULA, and BASTION vs TARANTULA. The study records the chassis on each detailed row and reports per-chassis/per-weapon totals alongside overall weapon totals. It retains the same five PD weapon profiles, seed count, side swap, AI combat loop, and does not write or tune `PD-Balance.json`.

The expected workload is 300 matches (10 weapon pairs × 5 seeds × 3 chassis pairings × 2 side placements).

The study also checks coverage before reporting success: every weapon appears in 120 matches overall, and each chassis/weapon group is checked against its expected count based on weapon ordering and chassis pairing. Group counts differ by design because the five weapons are paired once in index order. For each matchup, the tested weapon profile is duplicated across all four slots; this is a controlled weapon comparison, not a mixed custom loadout or a human duel.

## Validation status

- `git diff --check` passed.
- Unity-generated `Assembly-CSharp.csproj` compiled with the installed Unity 6000.6.7f1 reference assemblies via `dotnet build Assembly-CSharp.csproj -v:minimal`: succeeded, 0 errors and 25 warnings. Output: `Temp/bin/Debug/Assembly-CSharp.dll`.
- The first 300-match run completed but returned a false failure because the initial coverage assertion incorrectly expected 80 samples for every chassis/weapon group. The expected-count calculation now accounts for each chassis's role and weapon index.
- Unity 6000.6.7f1 Windows player build passed: `VOXSKY_BUILD_PASS 237506829 bytes` (`Logs/Chassis-Weapon-Duel-Final-Build-20261110.log`).
- The built player passed `--weapon-duel-study`: **300/300 matches**, 100 for each chassis pairing, with coverage checks passing (`Logs/Chassis-Weapon-Duel-Run-Final-20261110.log`). Results: [detail CSV](../Validation/Weapon-Duel-Study-20261110-164307.csv) and [summary CSV](../Validation/Weapon-Duel-Summary-20261110-164307.csv).
- Aggregate wins were AR 108/120, LMG 100/120, GL 62/120, RL 30/120, and SR 0/120. This AI study is a comparative signal, not a human balance verdict; no PD weapon values were changed.
- `Arena.unity` was restored from the pre-build byte copy and its SHA-256 is `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`. `PD-Balance.json` remains unchanged at SHA-256 `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.

## Next step

Next, review these per-chassis results with PD, then extend the study to more varied loadouts only when the weapon-slot test rules are clear. Do not auto-edit PD numbers. Participant-based map sizing and non-lock-on explosion/homing behavior remain separate queued work.
