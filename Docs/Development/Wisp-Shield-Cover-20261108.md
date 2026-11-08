# WISP movement, shields, and Foundry cover

## WISP

- Forward ground Shift sprint target increased from 1.65x to 1.9x the WISP prototype base speed.
- WISP is excluded from the high-speed landing stagger path, preventing the brief movement lock after a fast descent. Other frames retain the existing toggle and thresholds.
- WISP dash recovery is 0.18s, shorter than the dash travel itself, so its cooldown does not create a pause after the burst.
- Side/back ground Shift remains a fixed-distance, fuel-consuming dash; airborne Shift remains disabled.

## Shields

- Carrying a physical shield no longer applies the old fixed 50% speed multiplier. Its weight contributes to a carry penalty; raising guard adds a separate, stronger penalty. Beam shields retain a much smaller penalty in either pose.
- Holo/beam shields are classified by their catalog ID, keep a lighter movement penalty, and apply half of their configured guard-reduction value. Existing PD-authored values are read as-is; the runtime does not rewrite the balance file.
- The translucent cyan beam panel is hidden while carried and appears only while guard is held. The ordinary shield art remains unchanged.

## Foundry

- Added ten destructible cover props around the central combat lanes, with spawn approaches kept clear. Their bounds feed the existing movement and projectile obstruction systems.

## Verification

- Unity 6000.6.7f1 Windows build passed (`VOXSKY_BUILD_PASS`, 237,433,902-byte output directory).
- Built player `--automatic-balance-study` passed 59 checks, including WISP sprint/cooldown and landing behavior, physical vs beam shield speed/guard/visibility, and Foundry cover count.
- `git diff --check` passed. `Arena.unity` and `PD-Balance.json` SHA-256 hashes matched before and after the build.
- Interactive keyboard play was not performed in this pass.
