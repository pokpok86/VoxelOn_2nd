# Held lock-on reacquisition

Manual lock-on now attempts acquisition on every update while the lock button remains held and the current weapon still supports lock. If range, occlusion, or screen visibility breaks the lock, the same held input can reacquire once the target becomes valid again. Releasing the button still clears lock immediately. Shield-equipped loadouts continue to use right-click guard and suppress manual lock as previously requested.

## Shoulder equipment constraints

- Weapon data now stores `blocksShoulderWeapons` and `allowsShoulderDuplicate`; the weapon editor exposes both switches.
- Military_RL (the bazooka profile) blocks shoulder firing while it is equipped in either hand. Shoulder selections remain in the saved design, but are skipped by weapon selection, HUD, AI selection and firing until the blocking weapon is removed.
- A weapon with shoulder duplication disabled cannot occupy both shoulder slots in one design. The right slot is cleared by the normal live editor repair when that option is switched off.
- Shoulder weapons with duplication disabled are shown diagonally across the upper back while inactive, then rotate into aim when selected. An inactive hand-held shoulder-blocking weapon uses a matching across-shoulder carry pose.
- Version 4 balance files migrate to version 5 with existing equipment preserved; the bazooka receives the requested shoulder-blocking defaults. Later edits remain per-weapon PD data.

## Verification

- Unity 6000.6.7f1 Windows build: `Logs/Shoulder-Capacity-Build-20261108.log`, `VOXSKY_BUILD_PASS 237446149 bytes`, exit 0.
- Built player `--automatic-balance-study`: `Logs/Shoulder-Capacity-Study-20261108.log`, 70 checks passed, exit 0. Includes runtime shoulder blocking, version 4 migration, the across-shoulder bazooka pose, non-duplicable shoulder storage/aim pose, and duplicate-loadout rejection.
- Built player `--fx-capture --balance-editor-study`: 138/140 checks passed; the quadruped knee armor visual and configured live-shot checks still fail. Log: `Logs/Shoulder-Capacity-EditorStudy-20261108.log`.
- Built player `--smoke-test`: `Logs/Shoulder-Capacity-Smoke-20261108.log`, exit 2 with 11 failed legacy assertions (weapon lock tied to chassis, cover distribution, shield recharge, universal rifle/speed assumptions, and held-Shift direction checks). The suite did not pass and is not counted as verification success for this change.
- `Assets/Scenes/Arena.unity` and the PD-authored `Data/Balance/PD-Balance.json` had matching SHA-256 hashes before and after the build. The automatic balance study used an isolated temporary data directory.
- `git diff --check` reported no whitespace errors; it emitted existing CRLF/LF conversion warnings.
