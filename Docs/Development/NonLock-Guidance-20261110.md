# Non-lock weapon guidance — 2026-11-10

## Change
- `VoxArena.WeaponsV2.FireEquippedWeapon` now treats numeric `lockStrength` as independent guidance configuration. A weapon without the `LOCK` trait can home automatically when `lockStrength > 0`; it does not require manual target lock. `LOCK` continues to gate manual lock eligibility, and player-fired `LOCK` weapons only guide while target lock is held. AI lock guidance remains unchanged.
- Impact blast was already independent of `LOCK`; no blast behavior or user weapon values were changed. The `Future_Launcher` profile remains a non-lock weapon with its existing area blast settings.
- Added isolated automatic-balance checks for the existing non-lock impact explosion and the new non-lock guidance behavior. The check temporarily changes only the test copy of the profile and restores it afterwards.

## Verification
- `dotnet build Assembly-CSharp.csproj -v:minimal`: succeeded, 0 errors (Unity 6000.6.7f1 generated project references).
- Unity 6000.6.7f1 Windows player build: `VOXSKY_BUILD_PASS 237507574 bytes`; batch mode exited successfully. Log: `Logs/NonLock-Guidance-Build-20261110.log`.
- Built player `--automatic-balance-study`: `AUTOMATIC_BALANCE_PASS 73 checks`; both non-lock launcher checks passed. Log: `Logs/NonLock-Guidance-Study-20261110.log`.
- `git diff --check`: passed.
- Restored `Assets/Scenes/Arena.unity` from the exact pre-build copy. SHA-256 before and after: `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`. `Data/Balance/PD-Balance.json` was not modified; SHA-256: `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.
