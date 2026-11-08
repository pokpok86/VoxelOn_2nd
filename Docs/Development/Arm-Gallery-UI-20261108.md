# Image-first part picker — 2026-11-08

- Replaced visible names in part-model options with selectable silhouettes for head, core, left/right arms, legs, and boosters. Names remain available as hover tooltips.
- Enlarged the silhouettes, added a cyan frame around the selected option, and moved trait/stat controls below the two-row arm and biped-leg galleries.
- Added Bruiser, Vanguard, and Tactical arm models. The editor study checks all seven non-standard arm models on either arm while preserving the opposite-arm selection, and checks Bruiser/Tactical save and reload independently.

## Validation

- Unity 6000.6.7f1 build passed: `VOXSKY_BUILD_PASS 237465522 bytes`. Log: `Logs/Arm-Visual-UI-Final-Build-20261108.log`.
- Built player `--fx-capture --balance-editor-study`: arm assembly and independent save/reload checks passed. The full study reported 144 checks with two unrelated existing failures: quadruped knee armor attachment and configured damage/velocity/magazine shot. Log: `Logs/Arm-Visual-UI-Final-Study-20261108.log`.
- Inspected the generated arm gallery screenshot: `Logs/Balance-Editor-Test-20261108-071539/Screenshots/02e-arm-selector.png`.
- `git diff --check` reported no whitespace errors; Git warned that the existing CRLF script files may normalize to LF.
- `Arena.unity` and the user balance JSON were restored to their exact pre-build bytes and their SHA256 hashes were verified unchanged.
