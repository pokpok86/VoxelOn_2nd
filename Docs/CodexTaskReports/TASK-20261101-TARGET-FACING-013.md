# VoxelOn local Codex task report: TASK-20261101-TARGET-FACING-013

- Status: **completed**
- Mode: check
- Priority: 9
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T14:33:31.2679218Z
- Finished: 2026-11-01T14:36:16.8976155Z

## Summary

읽기 전용 조사를 완료했습니다. **후보 SHA와 점프·다리 작업의 충돌 여부를 확인하지 못해 확정 구현 작업서는 보류합니다.**

- **확인 결과:** `VoxArena.cs:313,360`에서 락온 중에는 이미 적을 향하지만 즉시 회전합니다. 락온이 없으면 몸은 이동 방향, 조준은 화면 중앙을 따릅니다. 따라서 제보 현상을 유효 락온 상태의 결함으로 확정할 수는 없습니다.
- **주의점:** 회전 처리가 입력 단계와 `Update`에서 중복됩니다. 이동 속도는 몸 방향에 따른 전진·횡이동·후진 배율을 사용하므로, 회전만 바꿔도 속도가 달라질 수 있습니다. 카메라 회전도 다음 프레임의 WASD 방향에 영향을 줍니다.
- **리그:** 머리·코어·팔·다리는 분리되어 있으나 공통 상체 회전축은 없습니다. 상체 분리는 가능하지만 여러 모듈과 애니메이션의 협조가 필요합니다. 최소 변경에는 전신 회전 방식이 적합합니다.

잠정 설계는 플레이어 회전을 프레임당 한 번 처리하고, 락온 회전 속도 **8 rad/s**, 각도 데드존 **1°**, 근접 방향 유지 반경 **0.25m**부터 검토하는 것입니다. 이동 벡터와 방향별 속도 계산을 시각적 회전에서 분리하고, 기존 발사각 **65°**와 조준 보정을 유지해야 합니다. AI 회전은 별도로 유지합니다. 이 수치는 검증 전 제안입니다.

필요한 후속 검증은 횡이동·후진·점프·부스트 중 사격, 적의 후방 통과, 근접 교차, 락온 해제·상실, 무락온 이동입니다. 이동 방향·속도, 회전 상한, 발사각 제한, 카메라 연속성을 함께 확인해야 합니다. 현재는 단일 `enemy` 구조이며 온라인 결정성은 입증되지 않았습니다.

**변경 파일:** 없음.  
**실제 수행:** 소스·문서·기존 Smoke assertion·Unity 버전 정적 확인, 도구 검색. 빌드·플레이·테스트 실행 없음.  
**차단 사항:** PATH에서 Git·Unity·rg를 찾지 못했습니다. 외부 Git 메타데이터는 접근하지 않았습니다. 읽기 전용 규칙에 따라 `codex/task-reports`에도 파일을 생성하지 않았습니다.

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.