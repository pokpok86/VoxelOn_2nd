# Iteration 14 — Editable weapon trait glyphs (2026-11-07)

Work remains local on `work`; no commit, push, merge, or `main` change.

## Changes

- Added the optional stable `traitIconId` field to weapon records. Existing `icon` text remains as a separate editable label. Old balance JSON without the new field receives a safe icon default derived from current weapon kind and stored stats; unsupported IDs are rejected.
- The weapon editor offers eight selectable code-drawn glyphs: `DAMAGE`, `LOCK`, `RADAR`, `AREA`, `RAPID`, `RELOAD`, `JUMP`, and `SHIELD`. The chosen glyph appears beside the weapon in the list and persists through JSON save/load.
- Defaults assign shield, blast-area, lock, rapid-fire, or damage symbols from existing data. No shot, damage, or equipment rule changed.
- This slice covers weapon icons only. Part glyph editing and icon colors tied to grade remain unimplemented; current glyph tint distinguishes future-family weapons from other families.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237305078 bytes` in `Logs/WeaponTraitIcons-20261107-Build2.log`.
- Built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 369 checks` in `Logs/WeaponTraitIcons-20261107-Smoke2.log`.
- Actual balance editor study exited 0: `BALANCE_EDITOR_STUDY_PASS 85 checks; 14 images` in `Logs/WeaponTraitIcons-20261107-EditorStudy2.log`. Coverage includes old-file fallback, defaults, ID rejection, custom icon preservation and JSON round trip.
- Inspected `Logs/Balance-Editor-Test-20261107-082601/Screenshots/01-weapon-options.png`. `git diff --check` passed. The build regenerated only clean `Assets/Scenes/Arena.unity` object IDs; they were restored. No existing screenshots were changed.

## Remaining

Add selectable trait glyphs to parts and decide how icon color should encode grade before calling the request complete.
