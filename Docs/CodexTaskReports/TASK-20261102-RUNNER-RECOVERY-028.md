# TASK-20261102-RUNNER-RECOVERY-028 — 확인 결과

## 큐 및 원장

- **큐 브랜치 HEAD:** `f818a3e7bbdc4ee2c35dc3fae969d5c9f252fbe2` 확인.
- **파일:** `docs/CodexTasks/TASK-20261102-RUNNER-RECOVERY-028.md`가 해당 커밋에 존재하며 `mode: check`, `status: ready`입니다.
- **실제 fetch:** 처음에는 격리된 세션의 .git 쓰기 및 네트워크 제한으로 실패했습니다. 세션 범위를 승인받은 뒤 Git fetch를 다시 실행하여 remote-tracking ref를 f818 커밋으로 갱신했고, 파일도 로컬 Git 트리에서 확인했습니다. Windows ACL이나 저장소 권한 설정은 바꾸지 않았습니다.
- **러너 설정:** `QueueBranch=codex/task-queue`, `QueueDirectory=docs/CodexTasks`, `SourceBranch=work`, `ReportBranch=codex/task-reports`.
- **검색 로직:** 설치된 스크립트는 원격 ref의 `git ls-tree -r --name-only` 결과를 `(?i)^docs/CodexTasks/...` 정규식으로 검사합니다. 두 폴더 표기의 대소문자는 모두 허용됩니다.
- **처리 원장:** 028 항목은 아직 없습니다. 027은 `failed / invalid-metadata`입니다. 028을 Codex로 재실행하지 않았고, 원장을 초기화하지 않았습니다.

## 실행 환경

- 현재 셸 계정은 `DESKTOP-R5DBQMC\User`; 저장소는 `D:\Codex\VoxSky`입니다.
- 설정된 Git 실행 파일은 존재하며, 이 세션에서 실제 fetch 성공으로 Git 실행과 GitHub 접근을 확인했습니다.
- fetch 확인 당시 별도 Git/Codex/Unity 프로세스와 `.git` 잠금 파일은 발견되지 않았습니다.
- 이 Codex 실행 세션은 Windows Task Scheduler/CIM 접근을 제공하지 않아, 실제 예약 작업의 실행 계정·현재 활성 여부·최근 성공 시각은 검증할 수 없습니다. 따라서 028의 자동 수신 여부는 아직 확인되지 않았습니다. 기존 러너 로그의 마지막 poll 시각은 028 커밋 생성 시각보다 앞섭니다.
- 설치된 파이프라인 코드는 변경 모드에서 Codex 실행 후 Unity Windows 빌드와 플레이어 스모크 검사를 수행하고, 통과하면 기존 `work`에 게시하도록 되어 있습니다. 별도 독립 CM 프로세스와 실제 이번 실행의 빌드/스모크 검증은 확인되지 않았습니다. 이 check 작업에서는 빌드를 실행하지 않았습니다.

## 결론 / 다음 조치

현재 로컬 큐 ref에는 028이 있고 원장에는 없어 다음 정상 runner poll에서 새 작업으로 수신될 수 있습니다. 실제 예약 작업이 그 poll을 수행하는지는 이 세션에서 확인 불가입니다. 집 PC의 작업 스케줄러 조회가 가능한 실행 환경에서 작업 상태를 확인해야 합니다. 게임 코드, `main`, 처리 원장은 변경하지 않았습니다.
