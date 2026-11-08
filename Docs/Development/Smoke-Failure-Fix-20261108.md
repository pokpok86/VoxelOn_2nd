# Smoke test failure review — 2026-11-08

## Findings and fixes

The previous Windows smoke run had 12 failures. Most checks still encoded behavior that had since been changed by the PD requests:

- Four lock-on checks assumed lock capability came from the chassis. Lock-on is now supplied by a `LOCK` weapon trait and suppressed when a shield is equipped. The smoke fixture now checks the trait rule across all four chassis.
- The Foundry cover check required debris farther than 180 units from center even though the arena boundary is 140 units. It now checks that cover reaches the outer playable area and is not concentrated in the inner half.
- The Homing Bloom fixture fired through a destructible cover prop. It now fires in a clear test lane. The seeker now creates its warning ring on the same update that tracking ends and the explosion fuse starts.
- The shield check expected a full recharge after ten seconds despite the current 30-second recovery. It now verifies gradual recharge without hull healing.
- Two projectile-speed checks expected 132 although the requested global speed increase sets the trial rifle to 264.
- Three WISP Shift checks expected a continuous boost in every direction. They now cover sustained forward sprint and finite fuel-consuming reverse/side dashes. The dash test stops applying movement input after initiating the dash.

## Validation

- Unity 6000.6.7f1 Windows build: passed, `VOXSKY_BUILD_PASS 237502970 bytes` (`Logs/Smoke-Final-Build-20261108.log`).
- Windows player smoke test: passed, 383 checks, 0 failures (`Logs/Smoke-Final-Player-20261108.log`).
- `git diff --check`: passed.
- `Assets/Scenes/Arena.unity` SHA-256 before and after build: `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`.
- `Data/Balance/PD-Balance.json` SHA-256 before and after build: `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.

The work remains on `work`. No merge to `main` was performed. Balance backup files are excluded from the commit.
