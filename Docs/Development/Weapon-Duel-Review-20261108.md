# Weapon duel baseline review — 2026-11-08

## Run

Ran the current Windows player with `--weapon-duel-study`. It completed 100 simulated 1v1 matches over five default weapon profiles, with 40 pairings recorded per weapon. The run passed its finite-state and match-count checks. Logs and raw output:

- `Logs/Weapon-Duel-Heartbeat-20261108.log`
- `Docs/Validation/Weapon-Duel-Study-20261108-141455.csv`
- `Docs/Validation/Weapon-Duel-Summary-20261108-141455.csv`

## Results

| Default weapon | Win points / 40 | Win points |
| --- | ---: | ---: |
| Military AR | 39 | 97.5% |
| Military LMG | 30 | 75% |
| Military GL | 21 | 52.5% |
| Military RL | 10 | 25% |
| Military SR | 0 | 0% |

## Interpretation and limits

This is a directional baseline, not a final balance verdict. The study equips both WISP fighters with the same tested weapon in all four mounts, uses one chassis and AI setup, and stops at 45 seconds. The average match duration is almost the full 45-second cap, so most encounters are time-limited. These assumptions can favor fast-firing weapons and do not model human aim, chassis matchups, weapon-slot tradeoffs, or the PD-authored runtime file.

The data source is `BalanceData.Defaults()`; the user's `Data/Balance/PD-Balance.json` was not loaded or edited. The large AR/SR gap is a candidate for follow-up testing with more chassis and weapon mounting scenarios. No balance values were changed from this run.
