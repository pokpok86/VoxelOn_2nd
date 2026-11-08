# Selectable arm model variants — 2026-11-08

## Scope

Added four procedural arm visuals that can be selected independently in the existing left and right arm slots:

- `ARM_ASSAULT`: wide knuckle gauntlet with three raised knuckles and a wrist hazard band.
- `ARM_BULWARK`: broad armored forearm with inset plate, vents, and elbow cuff.
- `ARM_SCOUT`: slim forearm with a luminous servo rail, dorsal fin, and wrist optic.
- `ARM_CLAW`: wrist socket with three forward talons.

The customization picker shows the arm variants in a wrapped two-row grid. The other part categories retain their existing layout. Existing arm model IDs remain available.

## Validation performed

- Unity 6000.6.7f1 Windows player build completed successfully. Log: `Logs/Arm-Variants-Build-20261108.log`.
- Ran the built player's balance editor study. Both new checks passed: all four arm visuals assemble without replacing the opposite arm, and left/right selections survive JSON save/reload independently.
- The complete study reported 142 checks and 28 image captures, with exit code 2 because two other assertions failed: the existing quadruped knee armor visual assertion and a configured live-shot assertion. This run is not a full-suite pass.
- `git diff --check` reported no whitespace errors; Git printed line-ending conversion warnings for existing CRLF working files.
- `Assets/Scenes/Arena.unity` and `Data/Balance/PD-Balance.json` SHA256 values were checked before and after the build and remained unchanged.

## Result

Build output: `Builds/Windows/Voxel-On.exe` and its companion `Voxel-On_Data` folder. The build harness reported 237,452,270 output bytes.
