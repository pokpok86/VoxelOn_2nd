# TASK-037 HOUND dash range

WHY: HOUND requires a longer ground dash than WISP without changing common dash speed or legacy eight-frame balance.
DESIGN INTENT: opt-in CraftSpec dashRangeScale (default 1.0); HOUND prototype 1.45. ResetRound applies it to existing per-fighter dashDistanceFactor. No new movement input or physics rewrite.
CHANGED: work commits 14e98e07d58d316c05b1f6af1b10d13021d73dcb (Assets/Scripts/CraftCatalog.cs), 3896bc3d103a1fbd0d2b410929ff95ab022d5c49 (Assets/Scripts/VoxArena.cs).
INVARIANTS: all original frames use multiplier 1.0, no boost fuel, cooldown, dash speed, lock-on, camera or jump change. main untouched.
VALIDATION: Unity compile/smoke/manual play NOT RUN (home PC off). HOUND prototype not yet selectable. Once wired, compare ground dash displacement under same velocity/energy and verify 1.45x factor relative to baseline, no speed increase, and other frames unchanged. Repro: select HOUND, dash on level ground, record position delta; compare WISP with identical base dash range.
LIMITATIONS/NEXT: Selection and quadruped model not wired; this only prepares dash tuning. HOUND movement input must be checked against existing TryDash and booster restrictions before claiming gameplay completion. Visual BEFORE/AFTER: no mesh change, so no visual rendering generated. When quadruped geometry changes, compare full assembled silhouette at same camera/scale/pose; check joints, ground contact, weapon placement and collisions; distinguish approximate render from actual Unity capture.
