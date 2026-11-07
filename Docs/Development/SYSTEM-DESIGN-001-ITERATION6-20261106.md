# Extensible custom part registry — iteration 6

Work-tree changes only. This iteration did not commit/push or touch `main`.

## Implemented

- Balance documents accept up to 32 additional parts with `CUSTOM_` IDs while preserving all 18 built-in part definitions. A custom part must match an existing chassis and one of its existing part categories; its stat keys/ranges inherit that matching built-in template. Unknown chassis/categories/stat keys, duplicate IDs, removed built-in IDs and invalid ranges are rejected.
- Each of the eight designs keeps its own custom-part values. When an older v4 file contains a custom part definition but omits that new part from its design arrays, normalization fills in the definition defaults. Copy/paste carries the custom per-design values; reset retains the custom definition and returns its selected slot to default values.
- Custom parts made from existing `CraftSpec` fields flow through the existing per-design apply path. The runtime test adds a WISP core-derived `CUSTOM_WISP_REACTOR`, saves/loads its values, then confirms the selected design's value reaches the live chassis stat.
- The part editor lists and edits the custom entry. A screenshot captures this UI state. Creating new geometry, custom numeric schemas/categories, per-part icons/art resources and runtime behavior for fields not already implemented are still future work; definitions currently need to be added to the balance JSON.

## Verification

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237269566 bytes`, `Logs/DesignSlots-Iteration6-Build.log`.
- Actual Windows editor study exited 0: `BALANCE_EDITOR_STUDY_PASS 75 checks; 12 images`, `Logs/DesignSlots-Iteration6-EditorStudy.log`. Checks cover custom weapon/part IDs, per-design separation, optional default fill for older slots, reset and copy, save/load, invalid definition rejection, actual stat application and firing, and existing editor/weapon regression coverage. Earlier fixture errors were corrected and the full suite passed; no check was removed or loosened.
- Actual Windows smoke exited 0 after the data/runtime changes: `VOXSKY_SMOKE_PASS 358 checks`, `Logs/DesignSlots-Iteration6-Smoke.log`.
- Inspected [`02b-custom-part-options.png`](../Screenshots/Balance-Editor-20261106/SchemaV4-Iteration5/02b-custom-part-options.png): the custom WISP part ID and inherited editable stats appear in the part-values tab. This is a test editor capture, not producer manual approval.
- Unity's regenerated scene object IDs were restored after building; `Assets/Scenes/Arena.unity` has no remaining diff.

## Next

Iteration 7 adds in-editor creation for a custom firearm and a core-derived part. Remaining: allow deliberate built-in schema selection and explicit safe art resource keys; new categories and geometry are not yet supported. Manual play and producer review remain pending. See [iteration 7](SYSTEM-DESIGN-001-ITERATION7-20261107.md).
