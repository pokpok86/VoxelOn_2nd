---
id: TASK-20261101-LEG-LANDING-STAGGER-010
mode: check
status: ready
priority: 35
title: Design leg-part high-fall landing stagger and boost-risk mechanic
---

## Producer design direction
Producer requests a gameplay mechanic tied to **leg-part attributes**: when a robot falls from a sufficiently high place, landing briefly staggers it. Players should manage jump height, altitude, and boost consumption; overcommitting boost or climbing too high carries a landing penalty. This is approved for design/specification and safe task decomposition only, **not yet blanket approval to change live combat/gameplay code**.

## Dependencies
- Read latest candidate `codex/implementation/2026-11-01-sd-ground` and the jump-apex implementation task `TASK-20261101-JUMP-IMPLEMENT-009` and its report when available. Do not design against stale `f808e5952fee` as if current.
- Do not conflict with 2x jump-height work. No changes to main, no final producer balancing decisions inferred.

## Design requirements
1. Identify current leg part configuration, movement/jump/boost state machine, support-height and grounded/landing detection. Propose one leg attribute e.g. landing shock absorption/resistance that can differ by leg archetype; preserve existing 8 frames.
2. Distinguish **actual fall distance / impact velocity relative to landing surface** from absolute world height. Ground-to-roof step-down and controlled boost descent should behave consistently. Regular jump on flat ground and low obstacles must have no stagger.
3. Propose a tunable threshold and short capped stagger duration, with 2-3 example archetypes (speed/balanced/heavy) and explicit values clearly labeled **initial suggestions requiring playtest**, not final approval. Prefer brief landing recovery to long loss of control; specify whether walking, boost, aim, firing, invulnerability and animation are affected, with fair telegraph and recovery.
4. Describe how boost exhaustion interacts with landing risk without forcing stagger on all ordinary jumps. Avoid double penalties or permanent stun/stun-lock. Consider repeated bouncing, stepping down, rooftop edges, network synchronization, bots, and animation readability.
5. Produce implementable small follow-up tasks and automated regression checks: normal 2x jump, safe landing, high fall stagger, leg archetype differences, boost usage, repeated landing, ground/roof collision and no false positives. Provide exact candidate branch SHA and file paths where verified.
6. Do not edit gameplay source, alter jump tuning, bypass tests, change runner settings, or merge main. Publish a concise PM design report with assumptions and unanswered balancing choices.

## Reporting
Use codex/task-reports. Cite evidence and distinguish proposed values from measured behavior. If jump task is pending, document dependencies and defer implementation decisions rather than guessing.
