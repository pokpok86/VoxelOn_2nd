# WISP v005 / centered rear camera — 2026-11-04 morning

Requested: shrink WISP head and use a centered rear view; investigate mouse camera suddenly looking down.

- Blender v005 shrinks the 21 head meshes by 15%, anchored at the neck. v001–v004 preserved. All 91 non-head meshes and 24 rig bones are identical to v004 (see WISP-v005-BodyIntegrity.json).
- Removed collision-triggered overhead view and automatic LookAt/head-avoidance rotation. WISP rejects stale lock state. Mouse yaw/pitch drives orientation; capture/focus changes discard recenter deltas.
- Center rear boom, no shoulder offset; wider view and immediate positional following avoid losing the pilot during fast travel. Cover only retracts the camera; nearby self-occlusion uses screen-door fading.
- Gameplay rules, roster, gun, jump and guard behavior unchanged.

Validation and captures are recorded in Docs/Validation/20261104-WISP-v005-Camera.md. Hardware mouse/Alt-Tab feel and final visual approval remain for the user. No main merge.
