# Iteration 9 — Roster-based match lifecycle foundation (2026-11-07)

Work remains local on `work`; no commit, push, merge, or `main` change.

## Changes

- Normal match completion now checks which registered teams still have living fighters. A single knockout no longer ends a match if a hostile teammate remains. The existing duel still ends when either side is eliminated.
- Round reset retains registered participants, resets their combat state, and places additional fighters around their team's spawn anchor rather than dropping them from the roster.
- AI targeting, facing updates, and animation updates now iterate the active roster. The current duel still registers only the pilot and one rival.
- Result text and finish cleanup use the surviving roster team and registered fighters.
- Added smoke checks for retaining and separating a third registered participant through round reset, and for ending a match only after its team is eliminated.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237287222 bytes` in `Logs/Build-Roster-Reset-20261107.log`.
- Built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 369 checks` in `Logs/Smoke-Roster-Reset-20261107.log`.
- The roster regression checks for teammate survival, winner-team resolution, and round-reset retention passed.
- The outdoor Canyon climb/collision checks passed in `Logs/Environment-Canyon-Elevation-Final.log`; that full environment run still fails the separate WISP skin comparison check, which measures a 0.301304 bind-pose error against the existing 0.005 threshold.

## Remaining scope

The hangar still starts a two-fighter duel; it does not yet let the player create or select additional match participants. Several presentation and interaction paths, including camera framing, manual target selection, and salvage, still assume the player/enemy pair. This is a roster lifecycle foundation, not an implemented team mode or network multiplayer.
