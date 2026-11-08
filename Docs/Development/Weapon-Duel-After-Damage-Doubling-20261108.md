# Weapon duel after damage doubling — 2026-11-08

## Run

Ran the current Windows player with `--weapon-duel-study` after the default firearm damage values were doubled. The study completed 100 AI-vs-AI matches across five weapons and passed its match-count and finite-state checks (player exit 0).

| Weapon | Win points / 40 | Points | Mean match time |
| --- | ---: | ---: | ---: |
| Military AR | 37 | 92.5% | 27.42 s |
| Military LMG | 31 | 77.5% | 30.18 s |
| Military GL | 22 | 55% | 28.60 s |
| Military RL | 10 | 25% | 33.68 s |
| Military SR | 0 | 0% | 33.37 s |

The 45-second time cap did not determine the average result in this run. The ranking remains strongly skewed toward the AR; doubling damage alone did not resolve the gap.

## Limits and data ownership

This study uses `BalanceData.Defaults()`, one WISP chassis, AI opponents, and the same weapon in all four slots. It does not load or change `Data/Balance/PD-Balance.json`; these results do not certify the PD-edited numbers or human-match balance. No additional weapon numbers were changed.

Raw results: `Docs/Validation/Weapon-Duel-Study-20261108-175320.csv` and `Docs/Validation/Weapon-Duel-Summary-20261108-175320.csv`.
Player log: `Logs/Weapon-Duel-After-Doubling-20261108.log`.
