# Quadruped leg study correction — 2026-11-10

## Finding
- Each design stores part definitions for every chassis. The study selected the first `LEGS` entry in the quadruped design, which belongs to a different chassis, so the assault model was never assigned to TARANTULA.

## Change and verification
- The fixture now selects the `LEGS` part whose `frame` matches the quadruped design's `chassisId`.
- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237510006 bytes` (`Logs/Quadruped-Leg-Study-Build-20261110.log`).
- Built player `--fx-capture --balance-editor-study`: “assault leg armor remains attached to all four articulated quadruped knee joints” passed. The wider study still exits 2 on the unrelated “unarmed attack damages nearby opponent without bullets or ammo use” check (`Logs/Quadruped-Leg-Study-Player-20261110.log`).
- Restored `Arena.unity` byte-for-byte after build; SHA-256 before/after: `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`. `PD-Balance.json` remained unchanged; SHA-256: `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.
