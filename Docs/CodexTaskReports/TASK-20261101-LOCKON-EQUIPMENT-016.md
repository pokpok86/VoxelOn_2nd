# VoxelOn local Codex task report: TASK-20261101-LOCKON-EQUIPMENT-016

- Status: **completed**
- Mode: check
- Priority: 5
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T14:58:08.3359742Z
- Finished: 2026-11-01T14:59:41.6920472Z

## Summary

읽기 전용 설계 검토를 마쳤습니다. **다리는 락온 장착 자격·성능에 관여하지 않으며, 머리와 무기가 락온 성능을 결정**하는 안으로 정리합니다. 아래는 제안이며 구현 완료를 의미하지 않습니다.

| 담당 | 제안 책임 |
|---|---|
| 머리 | 표적 감지, 포착 속도·거리, 추적 품질·표적 유지 보정 |
| 무기 | 락온 사용 방식, 포착·추적 기본값, 유도 능력과 사격 조건 |
| 다리 | 차체·상체 독립 회전 가능 여부와 기계적 회전 한계, 점프·착지·기동 |
| 기본 입력 | 마우스로 카메라와 일반 이족 기체 방향 정렬 |
| 코어·팔 | 기존 내구·에너지·무장 취급 역할 유지 |

이전 명세의 “특정 다리만 락온 무기 장착 가능” 조건은 폐기해야 합니다. 사족·탱크형은 독립 회전 덕분에 활용하기 편할 수 있지만, 락온 수치 보너스나 장착 허가 조건으로 연결하면 안 됩니다.

확인한 기존 코드:

- `VoxArena.cs:303–359`: 거리·가림·입력에 따른 락온과 일부 특수무기의 락온 후 발사 조건이 있습니다. 이는 **발사 조건**이며 다리별 장착 제한은 아닙니다.
- `CraftCatalog.cs:16–19`, `VoxArena.TestLab.cs:15`: 센서 거리·유지 시간이 머리 항목에 배치되어 있습니다. 무기와 공동으로 성능을 구성하는 설계는 추가 정리가 필요합니다.
- `VoxArena.cs:313–314`: 락온 해제 시 이동 방향으로 회전합니다. 요청된 기본 마우스 방향 정렬과 차이가 있습니다.
- `VoxArena.Smoke.cs:396–400`: 기존 이동 방향 회전 검증이 있습니다. 삭제·약화하지 말고, 향후 기본 입력과 별도 회전 기능의 기대 동작을 명확히 구분해야 합니다.
- 검색 범위에서 다리 기반 락온 장착 제한은 발견하지 못했습니다. 향후 일반 호환성 검사에도 이 조건을 추가하지 않아야 합니다.

최소 프로토타입·검증안:

1. 같은 머리·무기를 이족/사족/탱크형에 적용해 모두 장착·포착·발사가 가능한지 확인합니다.
2. 동일한 표적 경로에서 다리만 바꿔 포착 시간·거리·유지 시간이 동일한지 확인하고, 회전 편의성은 별도로 측정합니다.
3. 머리 또는 무기만 바꿔 해당 락온 성능만 달라지는지 확인합니다.
4. 자유 마우스 조준, 좌우·후진 중 기본 방향 정렬, 락온 진입·해제, 가림·거리 이탈·표적 소멸·재포착을 검증합니다.
5. Practice AI·조준 보조 기본값과 기존 회귀 검증을 유지합니다.

**변경 파일:** 없음.  
**실행한 검사:** 작업 트리 파일 목록, 필수 문서·관련 코드 정적 검토, 제한·호환성 키워드 검색, 도구 가용성 확인.  
**제약:** `git`·`rg`가 PATH에 없어 브랜치와 diff를 확인하지 못했습니다. 이전 작업 보고서와 `codex/task-reports`가 없어 원문 대조가 제한됐습니다. 읽기 전용 규칙에 따라 보고서를 저장·게시하지 않았으며, Unity 빌드·스모크·플레이테스트는 실행하지 않았습니다.

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.