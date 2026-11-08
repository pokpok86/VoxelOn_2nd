# Iteration 10 — Shared match participant creation and cleanup (2026-11-07)

Work remains local on `work`; no commit, push, merge, or `main` change.

## Changes

- Added shared participant creation/removal helpers. Creation builds a selected unlocked chassis, initializes combat state, assigns one of the two existing team IDs, and registers the fighter.
- Removal unregisters the actor and clears its projectiles, delayed attacks, beams, visual root, and contact shadow so retired fighters cannot leave stale combat actions behind.
- Reused these helpers in the multi-AI performance ladder and roster smoke check instead of maintaining separate construction and cleanup paths.
- The normal hangar still starts only the existing player-versus-rival duel. This change does not add a team-mode selector or expand team scoring rules.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237288610 bytes` in `Logs/Build-Roster-Factory-20261107.log`.
- Built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 369 checks` in `Logs/Smoke-Roster-Factory-20261107.log`.
- The multi-AI performance path completed with 2/4/8/16/32 registered participants and nearest-hostile targets in `Logs/MultiAI-Roster-Factory-20261107.log`. On this run, 8 actors averaged 16.67ms, 16 averaged 23.52ms, and 32 averaged 33.12ms. This remains a stress test, not a team-readiness claim.
- Existing outdoor screenshots were restored after the test run; no screenshot artifacts were added by this iteration.

## Next work

The next match step is a user-visible roster/mode entry point, with its participant count and scoring rules confirmed before implementation. In line with the user's new standing request, continue alternating in visible graphics improvements as well as gameplay work; keep manual art approval explicit.
