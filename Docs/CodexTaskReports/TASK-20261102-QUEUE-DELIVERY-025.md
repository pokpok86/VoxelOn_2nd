# VoxelOn local Codex task report: TASK-20261102-QUEUE-DELIVERY-025

- Status: **completed**
- Mode: check
- Priority: 99
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T21:26:02.9377901Z
- Finished: 2026-11-01T21:27:10.7218574Z

## Summary

진단 결과는 **증거 부족으로 원인 확정 불가**입니다. 10분마다 콘솔이 뜨는 현상은 러너 성공이나 작업 수집의 증거가 아닙니다.

- 워크트리의 숨김 파일 포함 132개를 확인했으나 러너 소스·설정·로그·원장·`QUEUE-STATUS`·`CodexTasks` 파일은 없었습니다. 관련 텍스트 검색도 0건입니다.
- 실제 `QueueBranch`, 열거 로직, 마지막 처리 ID, 새 ID가 제외된 정확한 이유는 확인할 수 없습니다. 설치 영역은 허용 범위 밖이므로 접근하지 않았습니다.
- `(?i)`가 대소문자를 무시한다는 예제 검사에서는 두 경로 모두 일치했습니다. **실제 러너 정규식 검증은 아닙니다.** 경로 대소문자를 원인으로 단정하면 안 됩니다.
- `ready` 파일도 이미 처리되었을 수 있습니다. 큐 ID를 원장·완료 보고서와 대조하기 전에는 재실행하지 마세요.

소유자가 로컬에서 실행할 읽기 전용 진단 명령입니다. 자리표시자는 실제 값으로 바꾸고, 설정 전체나 비밀정보는 공유하지 마세요.

```powershell
Get-ScheduledTask -TaskName '<작업 이름>' |
  Select-Object -ExpandProperty Actions |
  Select-Object Execute,Arguments,WorkingDirectory

Get-ScheduledTaskInfo -TaskName '<작업 이름>' |
  Select-Object LastRunTime,LastTaskResult,NextRunTime

Select-String -LiteralPath '<러너 소스 경로>' `
  -Pattern 'QueueBranch|CodexTasks|\(\?i\)|processed|ledger' -Context 2,2

Select-String -LiteralPath '<설치 설정 경로>' -Pattern '"QueueBranch"\s*:'

Get-Content -LiteralPath '<최근 실행 로그 경로>' -Tail 100
Get-Content -LiteralPath '<QUEUE-STATUS 경로>' -Tail 80
```

필요한 증거는 작업 스케줄러의 **동작·트리거·최근 실행 결과·해당 시각 기록**, 오류 메시지, 처리 원장의 마지막 ID·시각입니다. 사용자명·개인 경로·토큰은 가려주세요.

최소 수정 인계안은 기존 실행 인자와 작업 디렉터리를 유지하면서 **실행별 로그**를 추가하는 것입니다. 시작·종료 시각, QueueBranch/조회 커밋, 발견·제외·선택 ID와 제외 사유, 오류 위치, 외부 명령 종료 코드를 기록하고 실패 코드를 스케줄러에 전달하세요. 시작 단계 오류도 남기되 `pause`는 추가하지 않습니다. 토큰·환경변수 전체·작업 본문은 기록하지 않습니다. 이후 한 회차에서 새 미처리 ID의 수집과 종료 결과를 확인해야 합니다.

파이프라인 복구 뒤 안전한 SD 시각 후속 작업은 **1번 프레임의 어깨 장갑 한 부위만 비율 조정**하고 동일 카메라 전후 화면을 비교하는 것입니다.

변경 파일: **없음**. 네트워크·예약 작업 변경·커밋·외부 게시 없음. 실제 검사는 파일 목록·텍스트 검색·정규식 예제뿐이며, `git`/`rg`는 PATH에 없어 실패했습니다. Unity 빌드·플레이 테스트는 실행하지 않았습니다.

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.