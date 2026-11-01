---
id: TASK-20261101-SCREENSHOT-ZIP-001
mode: change
status: ready
title: Package Unity screenshot gallery as ZIP for visual review
---

## Goal

Prepare a single ZIP archive containing the existing actual Unity capture gallery under `docs/Screenshots/` (8 robots x 4 directions and 4 arena views, if present). This archive will be used to compare real gameplay rendering with concept art.

## Acceptance criteria

- Inspect the screenshot gallery and report the actual number of PNG files and missing expected images.
- Create `docs/ScreenshotGallery-VoxelOn.zip` **inside the isolated task worktree**, containing `docs/Screenshots/README.md` and available PNGs with their relative folder structure.
- Validate that the ZIP opens and its entries match the source images.
- Report the ZIP path, size, included image count, and any blockers.
- If the files are unavailable in the configured source branch, report that rather than inventing captures.

## Constraints

- Do not modify game code or generate new captures.
- Do not access files outside the isolated worktree.
- Do not commit, push, publish, merge, or change branches.
- The task runner cannot upload ZIP files to GitHub automatically; report the archive location so a separate approved publication step can follow.
