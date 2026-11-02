# TASK-041 — Original synthesized sound effects, no background music

## Requirement
Develop many independent, copyright-safe game effects. Do NOT add BGM/music at this stage.

## Work branch changes
- `47e17e27630fc91b2946bbec3da85d8c421f7f65`: VoxArena.Audio.cs extends 7 original effects with Footstep, HeavyStep, Joint, Landing, HeavyLanding, Charge, ChargeRelease, CoverBreak and Rubble. All clips are generated from original waveforms/noise in memory at startup, no licensed files. Adds gait/charge scheduling per fighter.
- `38d8439fc9d8a9fe1823432bb27efb1348214496`: hook UpdateMechanicalAudio from Animate.
- `e46e0afe93f8888fea9a4e83c750f45177bb6d67`: play landing sounds on hard impact and cover-break sound on destruction.

## Important limitations
Unity compilation and audio playback not tested. Joint, ChargeRelease, and Rubble clips are generated but **not yet connected to gameplay triggers**; future independent hookup can add them. Charge sound repeats as short pulses while held rather than one seamless loop. Audio is synthesized, not recorded or sampled. Existing 10-voice mixer and 0.07s per-kind cooldown remain. Footstep timing approximates speed and does not yet use true planted-foot contacts. In particular, indoor/outdoor acoustic reverb is not implemented.

## QA
Confirm no compile issues, all 16 clips generated, mute/volume, step cadence at idle/walk/sprint, tank heavy step, 2-second TARANTULA charge when playable, hard landing, cover break, no background music, no audible clipping. Keep changes on work only; main untouched.
