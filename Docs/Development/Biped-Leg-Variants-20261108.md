# Biped leg model variants — 2026-11-08

## Added models

Added four selectable procedural models for standard biped and runner leg slots:

- `LEGS_RANGER`: layered, light ranger greaves with cyan servo inlays.
- `LEGS_HEAVY`: broad thigh and shin armor with ankle shock absorber.
- `LEGS_JUMPER`: thigh charge cells, paired external pistons, piston collars, and heel thrusters.
- `LEGS_REVERSE`: high rear calf, angled shin, exposed tendon, and raised heel.

The existing standard, assault, and scout variants remain available. The customization selector wraps the seven biped options into two rows and moves the leg trait and stat controls below them. Quadruped and tank selectors continue to show their existing three choices.

## Validation performed

- Unity 6000.6.7f1 Windows build succeeded: `VOXSKY_BUILD_PASS 237459502 bytes`. Log: `Logs/Biped-Leg-Variants-Build-20261108.log`.
- Ran the built player with `--fx-capture --balance-editor-study`. The new check verified all four variants on both live WISP leg rigs, and the new save/reload check passed.
- The full study reported 144 checks and 32 image captures, with two unrelated existing failures: quadruped knee armor attachment and configured live-shot damage/velocity/magazine. The full study is not a pass. Log: `Logs/Biped-Leg-Variants-Study-20261108.log`.
- Inspected the generated customization screenshot to confirm the seven biped choices wrap into two rows without overlapping the trait controls: `Logs/Balance-Editor-Test-20261108-050130/Screenshots/02-part-options.png`.
- Inspected generated model previews: `02o-ranger-biped-leg-preview.png`, `02p-heavy-biped-leg-preview.png`, `02q-jumper-biped-leg-preview.png`, and `02r-reverse-biped-leg-preview.png` in the same screenshot folder.
- `git diff --check` reported no whitespace errors; Git emitted line-ending normalization warnings for existing CRLF files.
- Unity's build step rewrote `Assets/Scenes/Arena.unity`; it was restored from a pre-build byte snapshot. Its SHA256 and `Data/Balance/PD-Balance.json` SHA256 match their pre-build values.

## Build output

`Builds/Windows/Voxel-On.exe` and its companion `Voxel-On_Data` folder. The build harness reported 237,459,502 output bytes.
