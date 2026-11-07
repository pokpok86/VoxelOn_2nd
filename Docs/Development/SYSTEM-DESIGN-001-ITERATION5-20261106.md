# Extensible custom firearm slice and weapon icon list — iteration 5

Work-tree changes only; no commit, push, main change, or merge was made.

## Implemented

- Balance files may add up to 32 extra weapon definitions while retaining all 34 built-in entries. New IDs must use the uppercase `CUSTOM_` namespace and currently must be `firearm` kind. Duplicate IDs, unsupported namespaces/kinds, missing built-ins, invalid numeric keys/ranges, and invalid loadout references remain rejected.
- A custom firearm inherits the existing `Military_AR` numeric field schema, so existing range, integer and non-finite-value validation still applies. Definitions and equipped IDs persist through the existing JSON save/load path and all eight independent design slots.
- Runtime equipment and preview resolve a custom weapon to the existing Military_AR model as an explicit art fallback. This proves the extension point reaches actual loadout and firing values; per-item Blender art resource assignment, new weapon behaviors and custom parts remain unimplemented.
- The weapon editor list now shows a cached image per catalog weapon; custom entries use the documented Military_AR fallback icon. Existing text names remain alongside icons.

## Verification

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237259482 bytes` in `Logs/DesignSlots-Iteration5-Build.log`.
- Actual Windows player editor study exited 0: `BALANCE_EDITOR_STUDY_PASS 66 checks; 11 images` in `Logs/DesignSlots-Iteration5-EditorStudy.log`. It covers custom registration, save/reload, duplicate/namespace/kind rejection, fallback art and a real configured shot, in addition to the existing editor checks.
- Actual Windows player smoke exited 0: `VOXSKY_SMOKE_PASS 358 checks` in `Logs/DesignSlots-Iteration5-Smoke.log`.
- Screenshot set: [`01-weapon-options.png`](../Screenshots/Balance-Editor-20261106/SchemaV4-Iteration5/01-weapon-options.png), plus 10 other images in that folder. Inspected the weapon-options capture: icon and label rows render, and the selected custom definition and inherited numeric fields are visible. A producer manual usability/balance review remains pending.
- Unity regenerated only Arena scene object file IDs during build; the source scene IDs were restored and its diff is empty. The Windows output's managed assembly is updated; Unity did not rewrite the unchanged launcher executable byte-for-byte.
- Initial study attempts exposed fixture errors: one saved only the built-in registry, and a later interval check observed the custom shot's cooldown. The fixture now saves the extended registry and performs a fresh configured shot before checking its interval. Final full study passed without removing or relaxing checks.

## Next

Iteration 6 added custom part IDs, per-design values, chassis/category checks and default filling; details: [iteration 6](SYSTEM-DESIGN-001-ITERATION6-20261106.md). Remaining: add custom part creation to the editor and explicit, validated art resource binding instead of the interim Military_AR placeholder. Manual gameplay and visual approval are still pending.
