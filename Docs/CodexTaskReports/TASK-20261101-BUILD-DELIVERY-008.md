# VoxelOn local Codex task report: TASK-20261101-BUILD-DELIVERY-008

- Status: **completed**
- Mode: check
- Priority: 12
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T14:30:25.7586366Z
- Finished: 2026-11-01T14:33:16.4301763Z

## Summary

감사 결과: **최신 후보 SHA와 실행 파일의 일치, 정상 빌드 보존, 자동 전달을 입증할 수 없습니다.** 현재 소스에서 확인한 빌드 함수만으로는 목표를 충족하지 않습니다.

- `Assets/Editor/VoxBuild.cs:32`: `Builds/Windows/VoxSky.exe`에 고정 출력하며, 성공 시 `VOXSKY_BUILD_PASS`를 기록합니다. SHA 기록·보존·스모크 실행·LATEST 갱신은 이 함수에 없습니다. `CreateScene()`이 프로젝트 파일도 저장합니다.
- `Assets/Scripts/VoxArena.Smoke.cs:628`: 실제 검사 수를 PASS 로그에 기록하고 성공/실패에 따라 종료 코드 `0/2`를 반환합니다. 스크린샷도 생성합니다.
- 버전은 `bundleVersion: 1.0`이며 후보 SHA 식별 수단으로 충분하지 않습니다.
- 과거 문서에는 187개 검사, 파일 해시 매니페스트와 백업 기록이 있습니다. 현재 208/209개 검사 또는 최신 후보의 성공 증거는 아닙니다.
- 작업 트리에 러너 구현·설치 증거·EXE·빌드 로그·전달 매니페스트가 없습니다. 설치된 러너와 후보 러너의 동일성, 마지막 정상 실행 파일, 보존 정책은 확인 불가입니다.

후속 러너 변경 작업 초안과 수용 기준:

1. **대상 식별:** 설치 러너와 후보 러너의 버전·해시를 각각 기록합니다. 빌드 대상 브랜치와 전체 SHA를 확정하고, 빌드 전후 소스 일치를 검사합니다.
2. **격리 빌드:** 사용자의 로컬 프로젝트와 기존 빌드를 건드리지 않는 전용 빌드 복사본에서 Unity `6000.6.7f1`로 순차 실행합니다. 게임 코드·에셋·패키지·설정 변경은 Windows 빌드와 해당 플레이어 스모크를 필수로 합니다. 러너/문서만 변경하면 관련 자체 검사로 충분합니다.
3. **성공 판정:** Unity 종료 코드와 `VOXSKY_BUILD_PASS`, 플레이어 종료 코드와 `VOXSKY_SMOKE_PASS N checks`를 모두 확인합니다. 승인된 후보의 기대 검사 수와 실제 수를 별도 기록하고, **209개를 기대하는 후보가 208개이면 실패**로 처리합니다. 검사 축소로 통과시키지 않습니다.
4. **로컬 전달:** 지정된 로컬 보관 루트의 `<UTC시각>-<전체SHA>` 폴더에 EXE와 모든 Unity 실행 의존 파일, 로그, 스크린샷, 파일 해시를 보존합니다. GitHub 빌드 보관을 전제하지 않습니다.
5. **검증 포인터:** 모든 검증과 보관 무결성 확인 후에만 `LATEST-VERIFIED.json`을 원자적으로 갱신합니다. 브랜치·전체 SHA·UTC 시각·Unity/러너 버전·기대/실제 검사 수·결과·실행 파일/로그/스크린샷 경로를 포함합니다.
6. **실패와 보존:** 빌드·스모크·복사·포인터 갱신 실패 시 기존 정상 빌드와 포인터를 유지합니다. 최신 정상본과 직전 정상본은 정리 대상에서 제외합니다. 실패·중단·동시 실행·검사 수 불일치 시나리오를 러너 자체 검사에 포함합니다. 후보 코드 게시도 검증 성공 후 별도 승인된 절차에서만 수행합니다.

**변경 파일:** 없음.

**실제 검사:** 작업 트리 파일 목록 및 관련 소스·문서 정적 검토, 도구 검색. `git`, `rg`, Unity는 명령 검색에서 발견되지 않았습니다. Unity 빌드·플레이·러너 자체 검사는 실행하지 않았습니다.

**차단 사항:** Git을 사용할 수 없어 정확한 브랜치/SHA와 dirty 상태를 확인하지 못했습니다. 외부 접근 금지로 GitHub 후보 증거도 확인하지 않았습니다. 읽기 전용·메시지 전송 금지에 따라 `codex/task-reports`에 저장하거나 게시하지 않았으며, 위 내용을 보고서 초안으로 제공합니다.

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.