# Non-lock explosive weapon verification — 2026-11-08

## Change

Added an automatic balance study regression check for the existing `Future_Launcher` data path. The study confirms the launcher has an AREA blast radius without a LOCK trait, equips it through the runtime loadout pipeline, and fires it straight with zero tracking. It then invokes the explosion handler at the target and verifies damage and the blast ring. This checks the weapon profile-to-shot path and explosion handler; it does not exercise automatic projectile collision timing. The check uses the study's isolated temporary balance folder.

## Validation

- Unity 6000.6.7f1 Windows build succeeded: `VOXSKY_BUILD_PASS 237460902 bytes`. Log: `Logs/NonLockBlast-Build-20261108.log`.
- Ran the built player with `--automatic-balance-study`: `AUTOMATIC_BALANCE_PASS 72 checks`. The two new non-lock blast checks passed. Log: `Logs/NonLockBlast-AutomaticStudy-20261108.log`.
- Also ran the separate balance editor study: it reported 144 checks with two unrelated pre-existing failures (quadruped knee armor attachment and configured damage/velocity/magazine shot). Log: `Logs/NonLockBlast-Study-20261108.log`.
- `Assets/Scenes/Arena.unity` was restored from a pre-build byte snapshot after Unity's build save step. Its SHA256 and the user's `Data/Balance/PD-Balance.json` SHA256 match their pre-build values.
- No user-authored weapon values were changed.
