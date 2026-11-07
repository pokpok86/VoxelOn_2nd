# In-editor creation actions for custom equipment — iteration 7

Work-tree only; no commit/push, `main` change, or merge.

## Implemented

- Added a **+ 새 사용자 화기** action on the weapon options page. It creates a stable unique `CUSTOM_FIREARM_XX` ID, clones the current Military_AR stat schema, selects the new entry for editing and leaves saving explicit.
- Added a **+ 새 사용자 코어 파츠** action on the part-values page. It derives a `CUSTOM_PART_<CHASSIS>_XX` entry from the selected chassis's built-in core schema and inserts a per-design value copy into all eight design slots. The new part is selected immediately. Existing save/load validation, copy, reset and runtime field application handle it.
- IDs cannot overlap between weapon and part registries. Creation rejects blank names, duplicate IDs, missing chassis/core templates and over-limit registries before mutating the design data.
- New catalog-specific art is still not created. Custom firearms use the Military_AR preview/model fallback; the loadout list shows a generic weapon thumbnail when the custom item has no catalog icon.

## Verification

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237277737 bytes`, `Logs/DesignSlots-Iteration7-Build.log`.
- Actual Windows player editor study exited 0: `BALANCE_EDITOR_STUDY_PASS 78 checks; 13 images`, `Logs/DesignSlots-Iteration7-EditorStudy.log`. It calls the same creation helpers as the buttons, validates the resulting firearm and all-slot part registration, then captures the editor with each new definition selected.
- Actual Windows smoke exited 0: `VOXSKY_SMOKE_PASS 358 checks`, `Logs/DesignSlots-Iteration7-Smoke.log`.
- Inspected [`01-weapon-options.png`](../Screenshots/Balance-Editor-20261106/SchemaV4-Iteration7/01-weapon-options.png) and [`02b-new-part-created.png`](../Screenshots/Balance-Editor-20261106/SchemaV4-Iteration7/02b-new-part-created.png). The add buttons, generated IDs, selection and editable values are visible. Manual producer operation/usability review remains pending.
- Unity regenerated only scene object IDs during build; those IDs were restored and `Assets/Scenes/Arena.unity` has no remaining diff.

## Limits and next

The part action currently creates only a core entry from the selected chassis's existing schema. New stat keys/categories, part geometry, catalog image/resource assignment, and arbitrary safe schema editing are not exposed in the UI. The weapon factory creates firearms only. Add deliberate template selection (existing core/head/arm/leg/booster schemas) and preview/art-resource keys with an allow-listed catalog before adding any free-form numeric schema editor. Producer gameplay approval remains pending.
