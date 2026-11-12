# Compact active-weapon HUD — 2026-11-10

## Change
- Replaced the tall left-side four-slot panel with four compact cards in the existing lower-left footer. The cards run 1–4 left to right, so the active slot is easy to find while the center of the screen stays clear.
- The selected slot now has a bright cyan outline and number. Each card shows an ammo/reload gauge and a short ammo, reload, melee, guard, or unavailable state. Reloading fills the bar in amber and labels the slot `RLD` / `RELOAD`.
- Removed duplicate mount and ammo text from the footer; kept the pilot/frame label. No weapon data or balance values changed.

## Verification
- Unity project editor/build was checked before starting; no project editor or game process was active.
- `dotnet build Assembly-CSharp.csproj -v:minimal`: 0 errors; existing warnings remain.
- Unity 6000.6.7f1 Windows build: `VOXSKY_BUILD_PASS 237507701 bytes`; batch exit 0. Log: `Logs/Weapon-HUD-Build-20261110.log`.
- Built Windows player `--smoke-test`: `VOXSKY_SMOKE_PASS 383 checks`, exit 0. Log: `Logs/Weapon-HUD-Smoke-20261110.log`.
- Captured and visually inspected [gameplay HUD](../Validation/Weapon-HUD-Gameplay-20261110.png); the four-slot bar is in the footer and does not cover the robot. Capture log: `Logs/Weapon-HUD-Capture-20261110.log`.
- `git diff --check`: passed.
- Restored `Assets/Scenes/Arena.unity` from its exact pre-build copy. Before/after SHA-256: `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`. `Data/Balance/PD-Balance.json` was unchanged; SHA-256: `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.
