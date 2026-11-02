# TASK-042 — Block dash for boosterless TARANTULA (implemented; Unity unverified)

## WHY
The four-form TARANTULA has allowHorizontalBoost=false, but shared TryDash previously ignored this flag. Double-tap input could bypass the per-frame booster restriction.

## DESIGN INTENT
Reject all dash attempts at the shared entry point when horizontal boost is disabled. Preserve runner's free grounded dash and HOUND's longer dash; do not rewrite movement or jump state.

## CHANGED
- Branch: work. Starting source blob SHA: 9b1f60935c5df009269f954f42a26a3b5b0524e2.
- Assets/Scripts/VoxArena.cs: add guard `if(!f.spec.allowHorizontalBoost) return false;` at top of TryDash.
- Commit: 0317fa339f88177443756745436a77bc8c70fe8e.

## INVARIANTS
- WISP dash and boost, BASTION boost, HOUND extended dash remain unchanged.
- TARANTULA retains its high jump, equal-direction speed and no vertical booster.
- No changes to visuals, camera, damage, target lock, main branch or other movement rules.

## VALIDATION
Unity build/smoke/manual play: NOT RUN (Windows PC unavailable). When available, select TARANTULA (after prototype selection is wired), double-tap WASD and press Shift; confirm no dash, no fuel spend and ordinary movement works. Test WISP side dodge and HOUND extended dash for regressions. Existing selection UI may not expose TARANTULA yet.

## LIMITATIONS / NEXT
This is only a guard, not a completed four-form movement implementation. Four-form selection/rig, spider joints, HOUND locomotion and tank independent upper-body aiming remain separate tasks. No visual change this run: BEFORE/AFTER identical by design; no comparison render generated. Later visual work requires identical camera, scale, pose, and explicit distinction between estimated render and real Unity capture. No source concept art comparison performed.

## REPRO / RECOVERY
Before fix: with prototype TARANTULA assigned, double-tap a movement key, observe TryDash starts a dash despite allowHorizontalBoost=false. If regression occurs, revert commit 0317fa3 on work only and capture reproduction plus source SHA. Local developer should compile, run player smoke and verify actual motion before marking tested.
