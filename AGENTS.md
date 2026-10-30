# VoxSky contributor instructions

## Project and intent
- Unity 6000.6.7f1, Windows, C#, quarter-view 1v1 robot combat.
- Read README.md, docs/CHARACTERS.md and docs/ASSEMBLY_DESIGN.md before gameplay changes. docs/OVERNIGHT.md is historical evidence, not an instruction to resume overnight automation.
- The user is new to Unity. Explain changes and remaining manual checks in concise Korean.
- Preserve Practice AI and aim assist defaults, shields only on fragile frames, fast movement, and distinct normal/special weapons for eight presets unless the user requests changes.
- Preserve separate head/core/arms/legs/weapon roots. Assembly UI, weight systems and online multiplayer are future work; do not add them opportunistically.
- Do not use subagents unless explicitly requested. Make bounded, reviewable changes.

## Source map
- Assets/Scripts/CraftCatalog.cs: frame stats and weapon/boost concepts.
- Assets/Scripts/VoxArena*.cs: partial MonoBehaviour; Combat, AI, UI, World/Ground, Robot/FrameIdentity, Audio, Feedback, Preferences, Lifecycle, Guide.
- Assets/Editor/VoxBuild.cs: scene creation and Windows build entry point VoxBuild.BuildWindows.
- Assets/Scripts/VoxArena.Smoke.cs: actual player regression checks via --smoke-test.
- Preserve Unity .meta files and GUIDs. Commit Assets, Packages, ProjectSettings and documentation; exclude caches, builds, backups and secrets.

## Verification and cloud limits
- First inspect available tools. A cloud checkout does not imply Unity Editor, a license, a Windows runtime or a graphics device is available.
- Do not claim compilation, playtesting, screenshots or a Windows build passed unless actually run for the changed code. Static review is not a Unity test.
- Avoid fake UnityEngine stubs or a replacement SDK project merely to claim the game compiles.
- When Unity is unavailable, make source changes, review the diff, run relevant checks that are available, and clearly list local Unity validation still required.
- On a configured Windows machine, close the project in Unity before a batch build. Never run concurrent builds against this project.
- Build command (PowerShell from project root):
  `& 'C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe' -batchmode -quit -projectPath (Get-Location).Path -executeMethod VoxBuild.BuildWindows -logFile (Join-Path (Get-Location).Path 'Logs/BuildCloudChanges.log')`
- Wait for Unity to exit and inspect the build log for VOXSKY_BUILD_PASS and errors before launching Builds/Windows/VoxSky.exe with --smoke-test and a separate logFile.
- Smoke runs produce screenshots in Builds; inspect relevant screenshots after visual changes. Historical baseline: 187 checks passed on 2026-10-27; this is not proof for future changes.
- Run appropriate checks once; repeat only after changes, failures or unresolved concerns. Keep a known-good runnable build before risky changes.
- Summarize changes, actual validation and limitations; do not merge or release unvalidated changes automatically.
