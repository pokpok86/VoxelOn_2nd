# Projectile speed study correction — 2026-11-10

## Finding
- The balance editor converts the configured weapon `speed` into the runtime `WeaponProfile.speed` (including its existing factor of 2). `Fire()` then applies `bulletSpeedScale` to that runtime speed.
- `normalTravelScale` only scales fighter movement in `Move()`; it is not part of projectile velocity. The study compared a shot against a hard-coded pre-conversion value, so its assertion was stale.

## Change and verification
- Updated the real-shot check to compare shot velocity with the equipped runtime profile speed multiplied by `bulletSpeedScale`.
- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237510010 bytes` (`Logs/Projectile-Speed-Study-Build-20261110.log`).
- Built player `--fx-capture --balance-editor-study`: the corrected shot check passed. The wider editor study still exits 2 with two unrelated existing checks failing: quadruped knee armor attachment and unarmed punch damage (`Logs/Projectile-Speed-Study-Player-20261110.log`).
- `Arena.unity` was restored byte-for-byte after each build; SHA-256 before/after: `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`. `PD-Balance.json` stayed unchanged; SHA-256: `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.
