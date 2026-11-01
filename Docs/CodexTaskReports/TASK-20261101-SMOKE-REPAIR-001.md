# Smoke repair report: TASK-20261101-SMOKE-REPAIR-001

- Status: completed; candidate branch published for review
- Candidate branch: `codex/implementation/2026-11-01-sd-ground`
- Candidate commit: `b757bcc` (`b757bcc fix: align ground collision and refine Kestrel SD silhouette`)
- Unity Windows build: PASS; `VOXSKY_BUILD_PASS`, executable size 158,016,042 bytes
- Built-player smoke test: PASS; `VOXSKY_SMOKE_PASS 209 checks`
- Screenshot gallery: PASS; `VOXSKY_GALLERY_PASS 36 images`
- Main: unchanged; no merge performed
- Manual gameplay review: not performed

## Failure cause and correction

The first smoke run did not land on the obstacle roof because the test route kept horizontal movement after reaching the landing height and crossed the roof edge (diagnostic position z=109.94). The expected roof landing height was correct. The test route now stops within the roof's safe center margin (z < 107.5) and checks the landing against `roof.max.y + FighterSupportOffset`. No existing assertion was removed or weakened.

## Changes

- Corrected the smoke test's horizontal jump path so it remains over the roof through landing.
- Refined the Kestrel SD head, chest, shoulder, pelvis, and foot proportions as a base silhouette only; decorative detail was intentionally deferred.
- Aligned obstacle collider bottoms, roof tops, step-up/support offsets, shadows, and ground cover with the shared arena floor convention.
- Added/updated deterministic checks for the Kestrel dimensions, obstacle collision, and rooftop landing.

## Evidence and files

- Build log: local runner artifact `Logs/Build-SD-Ground-Final.log`; marker `VOXSKY_BUILD_PASS 158016042 bytes`.
- Smoke log: local runner artifact `Logs/Smoke-SD-Ground-Final.log`; marker `VOXSKY_SMOKE_PASS 209 checks`.
- Gallery log: local runner artifact `Logs/Gallery-SD-Ground-Final.log`; marker `VOXSKY_GALLERY_PASS 36 images`.
- Screenshots and before/after comparison: `Docs/Screenshots/Work-2026-11-01-Kestrel-SD-Ground/` on the candidate branch. See its README for the comparison and image index.
- Candidate files and commit: https://github.com/pokpok86/VoxelOn_2nd/tree/codex/implementation/2026-11-01-sd-ground

> The local build and logs are ignored/generated artifacts and are not claimed as committed source files. The automated Unity build and player smoke passed; a human visual/gameplay playtest remains outstanding.
