# Weapon duel study now uses PD tuning (2026-11-08)

## Change

The weapon duel study previously copied its five comparison profiles from built-in defaults, even when the user had edited `Data/Balance/PD-Balance.json`. It now reads those profiles from the active balance file for each run, falling back to built-in defaults only when that file is absent. The study does not write to the balance file. The participant setup, weapon profiles, and 100-match pairing method are unchanged.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237502990 bytes` (`Logs/PD-Weapon-Duel-Build-20261108.log`).
- Built-player `--weapon-duel-study` passed all 100 matches (`Logs/PD-Weapon-Duel-Run-20261108.log`).
- The run loaded all five requested profiles from the current PD balance document. Summary: LMG 40/40 wins, AR 30/40, GL 15/40, RL 15/40, SR 0/40. This is a comparative AI study, not a human balance verdict; no weapon values were adjusted.
- `git diff --check` passed. `Arena.unity` SHA-256 before/after build: `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`. `PD-Balance.json` SHA-256 remained `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.

## Follow-up

Expand duel presets to compare distinct frame/loadout combinations, and continue with participant-based map sizing and non-lock-on explosive/homing weapon review. Do not auto-adjust PD values from simulation results.
