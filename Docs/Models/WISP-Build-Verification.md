# WISP FBX Integration Verification

Current v003 rig and build results: [WISP-v003-Handoff.md](WISP-v003-Handoff.md). The results below record the earlier v001 ground correction.

Date: 2026-11-03  
Unity: 6000.6.7f1, Windows 64-bit  
Base: latest fetched `work` commit `18ad368`

## Results

- Windows build: **PASS**, `VOXSKY_BUILD_PASS 162624585 bytes`.
- Windows player smoke suite: **PASS**, `VOXSKY_SMOKE_PASS 234 checks`.
- Visual capture: **PASS**, 16 screenshots, four views for each selectable frame.
- WISP checks cover imported head/core/arms/legs roots; arm, knee and ankle connections; weapon sockets; base-color and emission texture assignment; hidden duplicate procedural torso; sole-to-floor alignment; attached shield; and sword activation during its existing saber-rush special.
- WISP's designer FBX uses a sole-level origin while the game uses a chest-level movement root. The four imported modules are offset by the measured support height, normalized for the frame's visual scale, and calibrated against renderer bounds. Grounded WISP no longer receives the idle vertical bob. The smoke check measures actual rendered bounds: sole minimum `Y=-0.910`, arena floor `Y=-0.910`.
- Frames without a lock-on sensor keep facing the mouse-camera heading while WASD movement remains camera-relative. Side input is tested as a lateral sidestep. Lock-capable frames retain movement-facing behavior whenever lock-on is inactive.

The four WISP views are `../VisualReferences/Implemented/01-wisp-front.png`, `01-wisp-right.png`, `01-wisp-rear.png`, and `01-wisp-left.png`. Other frames were recaptured as a same-build comparison. The current front capture was visually inspected after the ground correction.

The build and logs are local and are not stored in Git: `Builds/Windows/VoxSky.exe`, `Logs/Build-Wisp-FBX.log`, `Logs/Smoke-Wisp-FBX.log`, and `Logs/Capture-Wisp-FBX.log`.

The smoke suite verifies that the imported sword renderer is activated by Saber Rush. The comparison gallery does not contain a dedicated close-up of the swinging sword, and a manual in-game movement/combat review remains to be done. The left-arm shield mesh is cosmetic and does not change combat stats.
