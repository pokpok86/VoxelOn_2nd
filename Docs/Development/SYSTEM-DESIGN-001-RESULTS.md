# PD system design request — status and first validation

Baseline: `work` commit `0cd6f58c9fad06bdf5dc128da198cf3fc7ec2b29` (`pokpok86/VoxelOn_2nd`). `main` was read for the request and references only; no main files were checked out or changed. The former runner resume file was read as policy/history only; no runner was enabled.

## Existing implementation compared with the request

| Area | Present now | Missing / limit |
|---|---|---|
| Hangar/data editor | Full-screen hangar; WISP, BASTION, TARANTULA previews; four hand/shoulder mount choices; 34 catalog options (32 catalog models plus shield and unarmed); 18 part rows (6 categories × 3 frames); JSON save/load and explicit combat apply | JSON v1 requires exact default array lengths and known IDs. It cannot accept newly registered IDs or migrate versions. Only 3 example frames are saved; the requested 8 independent design slots and head/core/arm/leg/backpack assembly are not present. The actual BASTION screenshot confirms the UI and 3D preview, not all data taking effect in combat. |
| Runtime weapon/part traits | Damage, ammo, interval, projectile speed/radius/range, reload and spread are connected to the current weapon runtime. Selected part values are applied through the existing `CraftSpec` fields | Weight, ammo weight, recoil, lock penalty/strength, heat/cooling and energy are explicitly storage-only (`implemented=false`); part weight is also storage-only. The fixed editor rows are not a plug-in registry. |
| Combat/runtime architecture | Shared projectile/weapon entry points; `fighterRoster`, team IDs, `IsHostile`, nearest-hostile lookup and an AI overload already exist | The normal match loop, camera, finish/scoring and several hit/beam paths still use the player/enemy pair. The game currently starts one player versus one AI; no general match participant lifecycle or team mode is active. |
| Maps/physics | Indoor Foundry and bounded Outdoor Canyon; custom movement, jumping and AABB obstacle resolution; per-frame `CraftSpec` fall-gravity factor | Gravity is global/hard-coded for the map. No map gravity profile, volume zones or gravity devices/transition HUD. No shared mass/friction/buoyancy/drag layer. Current collision/movement is custom; no Rigidbody simulation is used in the benchmark. |
| Skills | Existing per-frame boost, shield, special-weapon and recovery behaviors | No equipped-part skill registry, Q assignment/gauge, temporary modifier layer, or verified sample skill. Existing special weapon attacks are not the requested part-skill system. |
| Timed individual frag mode | Existing 1v1 duel, win/loss and match timer | No timeout-by-kills mode with 8 saved design respawn choices, five-second selection timeout, or design-based restoration after discarded weapons. |
| Many-AI validation | Before this task, `--performance-test` rendered three one-versus-one samples; it did not run an N-AI ladder | Added a test-only `--multi-ai-performance` path for 2/4/8/16/32 real fighter objects. It is an AI stress measurement, not a multi-team match implementation. |

Reference review: `AGENTS.md`, `README.md`, `PLAN.md`, `docs/CHARACTERS.md`, `docs/ASSEMBLY_DESIGN.md`, `Docs/OVERNIGHT-20261106.md`, the hand/customization reports, validation handoff and BASTION actual customization screenshot, `CraftCatalog.cs`, `VoxArena*.cs`, `VoxArena.Smoke.cs`, `VoxBuild.cs`, and `TASK-20261102-WORK-RUNNER-RESUME.md` were checked. Both `Docs/` and `docs/` paths are retained exactly. The overnight document is historical, not a runner instruction. The work tree was clean before checkout; latest remote `work` differed from the previous local snapshot only by request-history documentation.

## Staged development plan

Effort estimates are relative: S = a few focused sessions, M = several sessions, L = multi-session subsystem work. They are planning estimates, not promises.

1. **Baseline and measurement (S) — completed.** Build latest work, run existing player regression suite, inspect current data and runtime paths, then measure AI population sizes in both maps.
2. **Extensible data vertical slice (M/L) — next.** Replace length-equality validation with stable ID registries, format-version migration and compatibility validation; add eight saved design slots without changing combat rules. First prove one new weapon ID and one part ID can be authored, loaded after restart, previewed, and rejected with a clear compatibility reason when invalid.
3. **Shared runtime command and trait binding (L).** Route fire/reload/special behavior through shared data and runtime modifiers. Bind only a small representative set of currently stored stats to real behavior and add tests that compare configured values with actual shots/movement. Keep visual IDs separate from gameplay values.
4. **Physics profile and one gravity device (L).** Introduce a map gravity profile used by jump/fall and projectile motion, then a single indoor zone/device with deterministic transitions and HUD warning. Defer buoyancy, fluid and full weather.
5. **Representative part skills (M).** Add a registry, Q selection/gauge and temporary non-persistent modifier layer; ship only a few verified examples, not every concept.
6. **Timed individual frag/respawn (M/L).** Add timeout score resolution and the eight-design respawn choice/timeout/restore flow. Keep teams, modes and team UI inactive.
7. **AI scale optimization and readiness (M/L).** Profile 8/16/32 on target hardware, isolate render/animation/AI/collision, then optimize measured bottlenecks and rerun the same ladder. Do not call it team-ready until normal match lifecycle and collision/targeting semantics are generalized and a target PC specification is agreed.

Risks: schema migration can damage user balance JSON if changed in-place; preserve and test backups. Runtime data expansion can alter balance or conflict with current slot IDs; use stable IDs and keep combat behavior unchanged until tested. Dynamic gravity touches movement, weapons and obstacle support; introduce it via a single bounded environment before adding more physics. N-AI load is sensitive to the number of visible meshes, shadows, resolution, VSync, map cover and GPU; retain the environment and map in every report.

## Changes made for the first validation step

- Added the test-only `--multi-ai-performance` route. It creates 2, 4, 8, 16 and 32 active actors using the three current selectable frames, assigns two teams, gives each AI a nearest hostile target and updates the existing AI, movement/jump/boost, attack/special, obstacle and combat paths. Existing `UpdateObstacles` now resolves every registered fighter; at normal roster size two, the behavior remains the same.
- Logs per-stage average/p95/p99/worst frame time; AI, obstacle-collision and combat CPU slices; managed/total memory; active actors, obstacles, peak projectiles and effects. GPU frame time and draw calls are reported unavailable, rather than guessed. The test notes that current part skills are not implemented and Rigidbody physics is not used.
- Saves live 8/32 actor images for each map. They show the stress scene; they are not an art approval screenshot.

## Actual verification

Windows Unity: `6000.6.7f1`; player `Builds/Windows/Voxel-On.exe`; `VoxBuild.BuildWindows` succeeded with `VOXSKY_BUILD_PASS 237229890 bytes` (`Logs/SystemDesign-MultiAI-Final-Build-Retry.log`). An intermediate build attempt failed once on an invalid Quaternion unary-minus expression; corrected it and the final build passed. No Unity build ran concurrently.

The final build's actual `--smoke-test` player run exited 0 with `VOXSKY_SMOKE_PASS 358 checks` (`Logs/SystemDesign-Final-Smoke.log`). The earlier baseline player run also passed 358; the post-change run is the applicable evidence.

### AI ladder — Outdoor Canyon

Hardware: AMD Ryzen 5 7500F (12 logical processors), NVIDIA GeForce RTX 3070 (8018 MB reported graphics memory), 15993 MB system memory; 1600×900 window; target 120 FPS, VSync 1 (observed cadence capped around 144 FPS); six seconds per population, one-second warmup. Actual run: `Logs/SystemDesign-MultiAI-Outdoor-Final.log`.

| Active AI | Average FPS | Average frame ms | p95 frame ms | AI avg / p95 ms | Collision avg ms | Combat avg ms |
|---:|---:|---:|---:|---:|---:|---:|
| 2 | 141.0 | 7.09 | 7.12 | 0.04 / 0.05 | <0.01 | 0.02 |
| 4 | 144.0 | 6.94 | 7.12 | 0.06 / 0.09 | <0.01 | 0.04 |
| 8 | 116.7 | 8.57 | 14.16 | 0.15 / 0.29 | <0.01 | 0.10 |
| 16 | 61.8 | 16.18 | 22.34 | 0.34 / 0.87 | <0.01 | 0.20 |
| 32 | 25.9 | 38.56 | 55.05 | 1.12 / 3.66 | <0.01 | 0.46 |

### AI ladder — Indoor Foundry

Same hardware/settings and six-second sample; 46 obstacle bounds. Actual run: `Logs/SystemDesign-MultiAI-Indoor-Final.log`.

| Active AI | Average FPS | Average frame ms | p95 frame ms | AI avg / p95 ms | Collision avg / p95 ms | Combat avg ms |
|---:|---:|---:|---:|---:|---:|---:|
| 2 | 144.0 | 6.94 | 7.07 | 0.11 / 0.14 | 0.04 / 0.05 | 0.02 |
| 4 | 144.0 | 6.94 | 7.09 | 0.21 / 0.37 | 0.08 / 0.10 | 0.04 |
| 8 | 125.9 | 7.94 | 14.07 | 0.46 / 0.86 | 0.17 / 0.22 | 0.09 |
| 16 | 69.2 | 14.45 | 21.62 | 1.05 / 2.19 | 0.32 / 0.44 | 0.25 |
| 32 | 32.5 | 30.80 | 41.68 | 2.68 / 5.29 | 0.59 / 0.71 | 0.42 |

The proposed 8-AI/60-FPS level was exceeded in these two short samples. At 32, average frame time was 30.80–38.56 ms (32.5–25.9 FPS), so 32-AI performance is not acceptable as a team-readiness claim. Even 16-AI p95 exceeded 16.67 ms in both maps. The measured AI/collision/combat slices explain only a small portion of total frame time at 32; the remaining time includes unmeasured animation, render-thread/GPU/presentation and other player-loop work. No separate GPU timing/draw-call counters were available in this player build. This identifies a profiling task, not a confirmed GPU bottleneck.

Screenshots (actual final-player captures):
- Indoor: [`AI-08.png`](../Screenshots/SystemDesign-20261106/MultiAI/IndoorFactory/AI-08.png), [`AI-32.png`](../Screenshots/SystemDesign-20261106/MultiAI/IndoorFactory/AI-32.png)
- Outdoor: [`AI-08.png`](../Screenshots/SystemDesign-20261106/MultiAI/OutdoorCanyon/AI-08.png), [`AI-32.png`](../Screenshots/SystemDesign-20261106/MultiAI/OutdoorCanyon/AI-32.png)

## Current decision / next action

The core direction is feasible, but the present game is still a 1v1 prototype and is not yet a data-extensible team-combat engine. The immediate next engineering slice should be schema/ID extensibility and the eight saved design slots, followed by one weapon and one part whose values demonstrably affect live combat. After that, generalize normal match actor lifecycle before implementing a team mode. A dedicated Unity Profiler capture is required before attributing the 32-AI frame-time regression to GPU, draw calls, or any one system.

No `main` changes or merge. No balance file, original source art or prior executable was deleted. Manual gameplay and art approval remain pending. This first pass has not implemented weapon/part expansions, skills, gravity devices, respawns or team mode.
