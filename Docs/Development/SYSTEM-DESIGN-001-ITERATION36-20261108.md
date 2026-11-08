# Iteration 36 — Data-driven leg model variants

## Scope

Added three saved visual choices for the existing `LEGS` part: `LEGS_STANDARD`, `LEGS_ASSAULT`, and `LEGS_SCOUT`. The hangar picker and its part silhouette now show the leg choice. Assault armor attaches to biped shin or quadruped knee joints; scout rails and locator details attach to the existing joints or tank track assembly. These are cosmetic additions and do not change animation, collision, movement, or combat values.

Older balance files that store the generic `STANDARD` leg visual normalize to `LEGS_STANDARD`. Unknown IDs are rejected. The balance editor study now includes save/load, legacy-ID, invalid-ID, imported WISP assembly, and all-four-quadruped-knee checks, plus assault/scout preview captures.

## Validation state

- `git diff --check` passed.
- Attempted a Windows build with Unity 6000.6.7f1. The Editor returned exit code 198 before project compilation: `No valid Unity Editor license found. Please activate your license.` The retry without `-nographics` returned the same exit code. Logs: `Logs/Leg-Variants-Build-20261108.log` and `Logs/Leg-Variants-Build-Retry-20261108.log`.
- The `Arena.unity` SHA-256 remained `CEEDF122D2B457A66C8CCAB65FEC3A88D992AC6B77506776254324A8AE52459A` across both attempts.
- The built-player editor study, smoke test, and screenshots have not been run for this change. Do not treat these variants as build-verified until the Unity license is available and those checks pass.
- Unity Hub UI was not targetable for inspection after one refresh retry; no login or license dialog was automated. No Unity Editor or game player is running.

## Next step

After Unity licensing is restored, build 6000.6.7f1, run `--fx-capture --balance-editor-study` and `--smoke-test`, inspect both leg preview images, and record the actual results before marking this part-model request complete.

Changes remain local on `work`; no commit, push, merge, or `main` change.
