# Iteration 35 — Weapon duel study and first LMG tuning pass (2026-11-08)

## Scope

Added a separate `--weapon-duel-study` player run. It takes the five existing weapon definitions from `BalanceData.Defaults()`, equips each weapon in all four slots for each WISP, then runs every unique pairing through the normal difficulty-zero AI, movement, firing, projectile, reload, hit, obstacle, and visual-effect loops. Each of ten pairings runs five deterministic seeds with both start positions swapped: 100 fights total, each up to 45 simulated seconds. Detail and summary CSVs use timestamped names and do not touch the existing PD14 study file.

The first 100-fight baseline showed `Military_LMG` at 40 wins and 0 losses across its 40 opponent fights. Its per-shot damage was 5 with a 0.15-second interval and 60-round magazine. Reduced only `Military_LMG`'s default damage to 4; the related Gatling defaults are unchanged. A second 100-fight pass moved its record to 29 wins and 11 losses. AR improved from 25 to 31 wins, and RL from 22 to 26. This is a preliminary AI matchup proxy, not a human balance approval.

Both runs also ranked `Military_SR` at 0 wins and 40 losses. Its 46 damage, 1.69-second interval, 4-round capacity, 180 projectile speed, and 0.20 radius were unchanged. The study does not record shot accuracy yet, and the AI may not represent human use of a precision weapon. Treat that outcome as a strong reason for another instrumented pass, not as grounds for an unverified stat change.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237385206 bytes` in `Logs/WeaponDuel-Tuned-Build-20261108.log`.
- The player exited with code 0 and recorded all 100 initial duels in `Docs/Validation/Weapon-Duel-Study-20261108-014245.csv`; aggregate results are in `Weapon-Duel-Summary-20261108-014245.csv`.
- After the LMG adjustment, the player exited with code 0 and recorded all 100 duels in `Docs/Validation/Weapon-Duel-Study-20261108-014536.csv`; aggregate results are in `Weapon-Duel-Summary-20261108-014536.csv`.
- Built player smoke passed 377 checks: `VOXSKY_SMOKE_PASS 377 checks` in `Logs/Weapon-Duel-Smoke-20261108.log`.
- `git diff --check` passed. No Unity Editor or game player remains running. Around the tuned build, `Arena.unity` SHA-256 was `CEEDF122D2B457A66C8CCAB65FEC3A88D992AC6B77506776254324A8AE52459A` both before and after the build.

## Remaining work

Add hit/damage attribution to the study before further tuning, especially for the sniper outlier. Expand balance review to mixed loadouts and more seeds before treating values as final. Other queued work remains: non-lock-on weapon explosion/homing, player-count map sizing, and clearer active-weapon HUD/reticle feedback.

All changes remain local on `work`; no commit, push, merge, or `main` change.
