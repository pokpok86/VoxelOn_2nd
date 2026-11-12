# Exoskeleton arm variant — 2026-11-10

## Change
- Added selectable `ARM_EXOSKELETON` for both left and right arm slots. Its forearm has two separated armor segments, a flex gap, exposed side actuators, hinge caps and a dorsal signal strip.
- Added the matching miniature picker icon and Korean label `외골격 피스톤`; selection remains independent per side. This is a catalog option only; no PD-authored balance or loadout JSON was changed.
- Extended the balance editor study to check the model marker, both independent slots, saving/reloading the new choice, gallery availability and a rendered preview.
- Made the non-lock launcher combat fixture explicitly use a hand slot only in the isolated test copy. Its actual PD classification is shoulder-only and remains unchanged.

## Verification
- `dotnet build Assembly-CSharp.csproj -v:q`: 0 errors, 25 existing warnings.
- Unity 6000.6.7f1 Windows build: `VOXSKY_BUILD_PASS 237509498 bytes`, batch exit 0. Log: `Logs/Exoskeleton-Arm-Build-20261110.log`.
- `--automatic-balance-study`: `AUTOMATIC_BALANCE_PASS 73 checks`, exit 0. Log: `Logs/Exoskeleton-Arm-AutomaticStudy-20261110.log`.
- `--smoke-test`: `VOXSKY_SMOKE_PASS 383 checks`, exit 0. Log: `Logs/Exoskeleton-Arm-Smoke-20261110.log`.
- `--fx-capture --balance-editor-study`: the three new arm checks passed (assembly, left/right save roundtrip, and gallery availability); the wider editor study reported 142/145 checks, with failures in quadruped knee armor, configured-shot stats, and unarmed-punch damage. Overall editor study exit: 2. Log: `Logs/Exoskeleton-Arm-Gallery-Study-20261110.log`. Preview: [exoskeleton arm preview](../Validation/Arm-Exoskeleton-20261110.png).
- `git diff --check`: passed.
- Restored `Assets/Scenes/Arena.unity` from the exact pre-build copy. SHA-256: `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`. `Data/Balance/PD-Balance.json` unchanged; SHA-256: `8DDD711C95A9C42A8D35C7C33C3225D62387EB7AED5F723AF03E7550AF0F5A6C`.
