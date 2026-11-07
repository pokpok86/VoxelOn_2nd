# Iteration 8 — Weapon library BACK navigation fix (2026-11-07)

Work remains on the existing detached `work` snapshot. No commit/push, `main` change, or merge.

## Reported issue and fix

The weapon gallery's `BACK / ESC` action set `weaponGalleryOpen` to false, but the hangar's mode-button line contained a stray unconditional block that set it true again on every hangar repaint. This made the gallery reappear as soon as the user returned. Removed that unconditional reopen. The back button and Escape now share `CloseWeaponGallery()` so they follow the same close transition.

## Validation evidence

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237285034 bytes` in `Logs/WeaponGalleryBack-20261107-Build.log`.
- Actual built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 366 checks` in `Logs/WeaponGalleryBack-20261107-Smoke.log`.
- Added a regression check for the gallery close transition (`weapon library BACK closes to the hangar`).
- Launched the rebuilt `Builds/Windows/Voxel-On.exe`; the `Voxel-On` window was present. Manual mouse-click verification of the gallery button is still pending; the automated smoke covers the close-state transition, not a GUI click.
- The previous headless screenshot assertion remains excluded because it produced a black frame; no screenshot was used as visual evidence here.

## Next steps

The timed individual frag mode and eight-design respawn selection are the first vertical slice only. Continue with a general multi-participant roster/match lifecycle while keeping the existing duel playable; then add team scoring and verify team rules. Do not claim multiplayer/networking or team mode complete until implemented and tested.
