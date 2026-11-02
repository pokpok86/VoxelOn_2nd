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
