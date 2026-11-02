# TASK-039 — Expanded arenas and destructible cover

User request: fast mechs need spacious maps. Most ordinary obstacles should have durability and break after sustained gunfire or a forceful landing. Keep structural walls and canyon terrain intact.

Work branch commits:
- 5941b6f9d0fe9f1637b62bfea6f6ea03baaa7f77: give registered debris HP (120 + footprint*1.5); on destruction remove visuals and replace corresponding collision Bounds with a noninteractive sentinel to keep list indexing stable; apply heavy landing damage when descending faster than 9 units/s onto raised cover.
- d09f4a902c910f5da731b8f8f4a362c0f2a94b71: bullets hitting cover reduce durability, explosive shots receive 1.6x multiplier. Structural terrain and boundary unaffected.
- 781d48a81396f4cd8ee48d75217fae2a7de5e59e: arena bounds enlarged from +/-95 to +/-140 (280m square); hangar ceiling and wall spans updated, canyon cliff strata expanded.
- db192a093ed1c016174d79d6bc7a2c82967d150c: canyon stone floor enlarged to match.

Unity build/play NOT run. Known limitations: destructible geometry disappears in one piece (no fragments or damage states); only debris cover is destructible, not factory structural walls or canyon ridges. High-speed landing detection may miss some cases because step-up and floor resolution depend on approach; test landing on top of crates. Check projectile occlusion, collision disappearance, cover destruction, and AI routes. Larger map still has some existing visual environment/ground markings concentrated near center. The 280m boundary is implemented but camera/performance needs playtesting. No changes to main.

Next: staged crack/damage visuals and small fragments with pooled effects; map-specific spawn distribution; more varied canyon relief; safe traversal and line-of-sight smoke tests.
