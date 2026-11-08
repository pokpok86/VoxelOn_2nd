# Iteration 29 — Sustained vertical booster climb (2026-11-07)

## Scope

Removed the fixed height limit from powered vertical boost. Unpowered jumps retain their frame-specific jump-height ceiling. While Space is held in the air and booster energy remains, vertical speed eases toward a slow climb speed configured by `boostClimbSpeed`; frame `verticalBoostScale` and `jumpWeightFactor` affect the climb rate. Weight also increases energy drain, while `boostCapacity` determines how long the climb can continue. Vertical lift continues to use the shared boost gauge.

This change does not give frames without vertical boost the ability to fly. Releasing Space or exhausting energy returns the frame to normal gravity.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237345166 bytes` in `Logs/Iteration29-Build-Final-20261107.log`.
- Built-player smoke test passed 375 checks in `Logs/Iteration29-Smoke-Final-20261107.log`, including checks that powered climb exceeds the unpowered ceiling, heavier frames climb more slowly and spend more energy, and doubled booster capacity sustains the climb longer.
- `git diff --check` passed. Dirty `Assets/Scenes/Arena.unity` was preserved byte-for-byte around the scene-generation build. Pre/post SHA-256: `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

## Queued user requests

Next prioritize model appearance improvements and additional parts. Other outstanding requests: improve quadruped gait; vary weapon statistics and evaluate them with simulated duels; add explosive or homing behavior for suitable non-lock-on weapons; scale map size to participant count; and make the selected weapon obvious in the HUD or reticle. UI polish remains lower priority than completing functionality.

All work remains local on `work`; no commit, push, merge, or `main` change.
