# GitHub → 집 PC Codex 작업 큐

`codex/task-queue` 브랜치의 `docs/CodexTasks/` 아래에 Markdown 작업을 올리면 집 PC가 10분마다 확인합니다. 기존 문서 폴더가 소문자 `docs/`이므로 그 경로 표기를 사용합니다. Windows에서는 대소문자를 구분하지 않습니다.

## 작업 등록

1. `TEMPLATE.md`를 복사합니다.
2. 파일명과 `id`에 새 ID를 지정합니다. 예: `TASK-20261101-MECH-002.md`.
3. `mode`를 `check` 또는 `change`로 정하고 `status: ready`를 설정합니다.
4. `codex/task-queue` 브랜치에 커밋합니다. 같은 ID는 성공·실패와 관계없이 PC에서 다시 실행하지 않습니다.

`check`는 읽기 전용입니다. `change`는 설정된 소스 브랜치에서 격리된 로컬 worktree를 만들고 그 안에서만 파일을 수정합니다. 어느 모드도 프로젝트 코드를 커밋/푸시하거나 병합하지 않고, `main`도 수정하지 않습니다.

결과와 작업 오류는 `codex/task-reports` 브랜치의 `docs/CodexTaskReports/<ID>.md`에 보고합니다. `change` 결과의 코드 수정은 PC의 별도 로컬 브랜치에 남겨 직접 검토할 수 있습니다. 보고서 브랜치 외에는 자동 푸시하지 않습니다.

## 테스트 및 관리

`TASK-20261101-TEST01.md`는 코드 변경 없는 읽기 전용 연결 시험입니다. 자동 스케줄을 설치하기 전에 수동으로 실행해 전달→비대화형 Codex CLI→GitHub Markdown 보고 전체 흐름을 검증합니다.

- 한 번 확인: `pwsh -NoProfile -File Scripts/CodexTaskRunner.ps1 -Once`
- 10분 스케줄 설치: `pwsh -NoProfile -File Scripts/Install-CodexTaskScheduler.ps1`
- 스케줄 제거: `pwsh -NoProfile -File Scripts/Install-CodexTaskScheduler.ps1 -Remove`
- 예약 실행 파일과 로컬 상태·잠금·로그: `UserSettings\CodexTaskRunner\` (Git 제외)

Codex CLI는 로그인된 Windows 사용자 세션에서만 실행하고, `codex exec --ephemeral`로 기존 Codex 대화 기록을 건드리지 않습니다. PC가 꺼져 있거나 로그아웃 상태면 다음 사용 가능 시점에 확인합니다.
