# TASK-20261102-FOUR-MOBILITY-JUMP-033

## Status
Approved requirement. Incremental implementation on work; Unity build and gameplay tests pending. Do not mark all four complete until four playable rigs are wired. Do not modify main.

## WHY
Different silhouettes need genuinely different movement and landing behavior. All directional ratios use the SAME reference maximum, with only TARANTULA granted a small exception above that baseline.

## Required controls and movement
- All: camera-relative WASD. Remove legacy universal high-speed mode; booster is a per-frame action, not an unconditional speed boost. Maintain the same base maximum reference; direction ratios normalize to it.
- 1 WISP: forward:back:left:right = 5:2:3:3. Booster forward = faster ground run. Booster backward/side = short, low-hop distance dash. Medium jump; midair booster permitted; hard landing causes stagger.
- 2 BASTION: direction change slows first, accelerates back to its usual movement speed; all-direction booster increases travel speed with heavy acceleration. Very low jump, slow boost lift, fast fall, longer landing stagger.
- 3 TARANTULA: equal speeds in every direction, slightly higher normal max than other rigs; NO booster (ground or air). High jump.
- 4 HOUND: forward:back:left:right = 6:2:2:2; booster like WISP with longer directional low-hop dash. High jump; NO vertical booster lift (per last user sentence, like #3).

## DESIGN INTENT / unresolved input
The request uses a common maximum speed with a small exception for #3. Keep base speed as a common reference rather than increasing all directions simultaneously. Exact acceleration times, jump meters, and boost distances are tuning values, not yet approved. Preserve earlier weapon target TASK-032.

## Local verification
Compare identical input durations in cardinal directions, transition 90/180 degrees, booster on/off, jumps at each height, landing stagger and collision with walls; verify lock-on aiming doesn't corrupt camera-relative movement. Record Unity build and screenshots separately from design mockups.

## Handoff
When implementing, record source SHA, chosen tuning values and WHY, affected files, invariants (existing 8 frames not silently remapped), test evidence, and remaining gaps in this report.

## Remote implementation checkpoint — 2026-11-02

- `work` commit `23457858019824f626100d179312465ea8241b09`: new opt-in prototype movement fields in `CraftCatalog.cs`. WISP forward/reverse/left/right = 1/.4/.6/.6; BASTION turn acceleration = 4 speed units per second relative to base speed, jump impulse x.55, vertical thrust x.55, landing stagger x1.35. These are provisional tunings.
- `work` commit `da66d79bb20c7c43c2ef4a4de936e34b4fd7560f`: apply jump impulse, vertical boost permission/scale, BASTION direction-change acceleration, and landing penalty. Disable old BASTION directional multiplier when prototype movement is active to avoid double penalties.
- **Not done:** replacing universal horizontal boost with the requested per-frame booster behaviors; selecting playable TARANTULA and HOUND rigs; their exact speed/jump rules; visual gait and animations. WISP/BASTION still have legacy boost logic. Do not treat this task as complete.
- **Unity validation:** NOT RUN (home PC offline). Verify build and directional speed ratios, and specifically that BASTION direction-change acceleration is not overwritten by Glide smoothing; test all existing eight frames for regressions.
- **Design rationale:** explicit per-spec flags permit gradual integration without silently changing every catalog entry. No new input bindings yet. Preserve previous weapon request and main branch.

## Remote checkpoint 2 — input conflict and WISP low-hop dash
- `work` commit `a4e214b51c89dd8a541c81c4dbc465a3807b351f`: opt-in movement prototypes no longer trigger legacy double-tap dash; existing standard frames retain it.
- `work` commit `bb6bbcac733027012b31798d27cb401cdb955203`: WISP Shift key-down with sideways/backward camera-relative movement triggers the existing distance-based dash and a small jump impulse (0.23x common jump speed). Forward Shift remains the legacy boosted run as a provisional implementation.
- **Intent:** Make directional booster behavior testable without changing existing combat input or all frames at once. Reuse dash collision/cooldown instead of introducing a parallel collision path.
- **Limitations:** Shift dash currently inherits the existing dash fuel check/cost and distance, which need tuning; forward run is still legacy boost; airborne/held Shift and other rigs still need dedicated rules. Unity compilation and runtime NOT tested.
- **Local tests:** WISP Shift+W vs Shift+A/S/D, diagonal forward threshold, pressing Shift while airborne, collision termination, repeated Shift after cooldown, fuel exhaustion, other frame double-tap behavior. Record any unexpected extra horizontal boost immediately after dash.

## Remote checkpoint 3 — BASTION gravity and boost direction
- `898ab520a3bd9cc723c654056cc95ecc06b291af`: BASTION prototype descent gravity factor 1.35x, consistent with heavy faster-fall request.
- `ad647afd268bdfe267c093d7f3531d50c37ccf1b`: BASTION prototype boost speed no longer varies with opponent direction; existing non-prototype boost target bias unchanged.
- **Why:** The previous opponent-relative boost multiplier contradicted the all-direction tank booster requirement. These changes preserve the current boost system while removing one inconsistent speed source.
- **Still pending:** actual four-rig selection, 3/4 movement and jump, common max speed normalization, new booster implementation for all four, full animation/visuals, Unity compile and gameplay verification.

## Remote checkpoint 4 — shared normal movement speed
- `f4239f3eb59d6e76d0dd93bc22929c66c9987e1c`: explicit `prototypeBaseSpeed=13f` shared speed reference; BASTION cardinal directional factors set to 1:1:1:1. WISP remains 5:2:3:3 normalized.
- `ff4fa31ed7c867a800a2ca6bd5ebd6aaff0ebaa2`: prototype rigs use shared 13f normal movement reference, including tank turn acceleration. Legacy frames retain their original per-catalog speeds.
- **Why:** Earlier WISP speed 15 and BASTION speed 10 violated the user-approved equal maximum baseline. Tank direction change should be a transient acceleration penalty, not a permanent side/reverse penalty.
- **Known caveat:** Existing horizontal boost still uses legacy per-catalog boostSpeed and may exceed shared reference; requested booster behavior is not fully implemented. Tank turn ramp is a provisional linear acceleration; no independent chassis facing yet. Unity NOT run.
- **QA:** Measure WISP cardinal speeds 13/5.2/7.8/7.8; BASTION 13 in all cardinal directions after acceleration settles, check 180-degree turnaround delay, ensure old frames unchanged. These are theoretical expected values before collision/animation effects.

## Remote checkpoint 5 — WISP booster conflict (2026-11-02 09:55 KST)
**WHY:** WISP's new Shift low-hop dash coexisted with the original Pulse activation and continued high-speed boost on backward/side input, producing inconsistent movement.
**DESIGN INTENT:** Keep the already-approved WISP prototype as forward-only held sprint and Shift-triggered lateral/backward low-hop dash; avoid rewriting legacy eight-frame combat. No visual mesh change this checkpoint.
**CHANGED:** Read work HEAD `ff4fa31ed7c867a800a2ca6bd5ebd6aaff0ebaa2` before modification. `Assets/Scripts/VoxArena.cs`, commit `8ed7bd93141797fdd2885df0a9c42a29fa265482`: suppress legacy Pulse for opt-in movement prototypes; prevent held side/reverse Shift from applying legacy continuous horizontal boost on WISP; use the correct prototype normal speed as boost interpolation baseline.
**INVARIANTS:** Other frames' Pulse behavior remains unchanged. No edits to main, weapon controls, shield logic, or dash collision code.
**VALIDATION:** Unity build, smoke and manual gameplay NOT RUN (Windows PC off). When local Unity is available: hold Shift+W, Shift+A/S/D, reverse while holding Shift, double-tap, test dash wall collision and fuel; compare normal WISP cardinal 13/5.2/7.8/7.8. Check boost logic with target lock and camera yaw. Failure reproduction: choose WISP, Shift+A on ground, observe whether pulse/continuous boost erroneously accompanies low-hop.
**LIMITATIONS/NEXT:** Existing `f.facing` may not equal camera forward during lock-on; facing-relative sprint classification is provisional and should be changed to explicit camera-relative intent once input is plumbed through. TARANTULA/HOUND selectable rigs and visual comparison remain unimplemented. BEFORE/AFTER visual intent: same rig silhouette, altered motion only; identical camera, pose, scale capture requested once Unity is available. No image created because there is no confirmed Unity before/after capture and no visual mesh change.
