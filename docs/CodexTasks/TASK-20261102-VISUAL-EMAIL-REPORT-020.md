---
id: TASK-20261102-VISUAL-EMAIL-REPORT-020
mode: check
status: ready
priority: 28
title: Design opt-in visual improvement email report to producer
---

## Producer request and destination
Producer wants email reports **only when a clearly verifiable visual improvement exists**, with before/after screenshots in original quality, not recompressed. Destination provided explicitly: **pok86@naver.com**. This is an approved reporting destination, **not permission to send a test or production email yet**; design first, verify mail sending permissions and implementation separately.

## Requirements
1. Inspect current screenshot/gallery artifacts, compare baseline vs candidate at identical camera/pose/lighting/scale and record actual improvement rationale, exact branch and SHA, build/smoke result and timestamp.
2. Define report format: concise Korean summary, visual differences, exact source/review SHA, build status, known limitations, before/after original PNGs or authenticated direct links to unchanged originals. No re-encoding/downsampling of original PNG; mail preview thumbnails may be separate.
3. Propose simple Windows runner integration for *verified visual improvements only*, with idempotency per SHA/comparison pair, opt-in enable switch, safe recipient configuration, send audit log and failure handling. Never block build/QA if mail fails.
4. Determine feasible email sending provider/SMTP/API, credential storage via Windows secure credential mechanism or environment secrets; **no hardcoded passwords, tokens or credentials in GitHub**. Account for attachments size limits and privacy of private repository screenshot links.
5. Do not claim ChatGPT/Gmail is connected, do not send emails, do not modify runner/source/main, and do not configure an SMTP account without explicit authorization. Report implementable next steps, missing sender account credentials and approval to codex/task-reports.

## Priority
Do not distract from critical jump/movement or Kestrel visual fixes. This is an optional notification enhancement.
