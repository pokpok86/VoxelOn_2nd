# VoxelOn local Codex task report: TASK-20261101-WORKFLOW-MAIN-001

- Status: **completed**
- Mode: check
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Started: 2026-11-01T11:00:23.7026466Z
- Finished: 2026-11-01T11:01:35.3077598Z

## Summary

읽기 전용 검토를 마쳤습니다. **이 큐 요청만으로 runner의 향후 동작이나 main 직접 작업 권한을 변경할 수 없습니다.** 소유자의 별도 승인이 필요하며, 최종 producer 승인도 별개입니다.

- **변경 파일:** 없음. 생성·수정·스테이징·커밋·네트워크 접근을 하지 않았습니다.
- **실제 확인:** 작업 트리의 `AGENTS.md`, README, 관련 문서, 빌드·로봇·점프·충돌·스모크 소스와 `.gitignore`를 읽었습니다.
- **Git 상태:** `git` 명령을 사용할 수 없어 현재 브랜치, main SHA, clean 상태를 확인하지 못했습니다. 연결된 Git 메타데이터를 찾으려고 작업 트리 밖으로 접근하지 않았습니다.
- **Runner 상태:** 작업 트리에서 runner·큐 polling 구현이나 설정을 찾지 못했습니다. 실제 polling 주기, 대상 브랜치, 쓰기 권한은 미확인입니다.
- **통합 상태:** 점프·충돌 코드와 관련 스모크 검사 항목은 현재 파일에 존재합니다. Kestrel 관련 문서도 있지만, 이전 시각 변경의 정확한 내용과 main 반영 여부는 확인하지 못했습니다.
- **검증 한계:** Unity 빌드·플레이·스모크 테스트·스크린샷 검사를 실행하지 않았습니다.

향후 전환에 필요한 최소 조건과 순서는 다음과 같습니다.

1. 소유자가 **로컬 runner 설정 변경과 main 직접 개발·push 범위**를 별도로 승인합니다. 그전까지 기존 큐·보고 브랜치 방식을 유지합니다.
2. 승인된 담당자가 runner 설정과 브랜치 보호 규칙을 확인합니다. 저장소 쓰기 권한, 허용된 push 권한, Windows Unity 환경, 단일 작업·빌드 잠금이 필요합니다. 실제 설정 키는 runner 확인 후 정해야 합니다.
3. 변경 전 최신 main의 SHA와 clean 상태를 기록하고, Kestrel 및 충돌·점프 변경을 커밋 단위로 대조합니다. 미커밋 작업이나 충돌이 있으면 보존하고 중단합니다.
4. 작은 변경마다 Windows 빌드 종료·오류·`VOXSKY_BUILD_PASS`, 스모크 결과, 필요한 새 스크린샷을 확인합니다. 실패하면 main을 갱신하지 않고 로그와 복구 절차를 보고합니다.
5. 승인된 갱신에는 전후 SHA와 마지막 정상 빌드의 SHA·보관 위치를 기록합니다. 회귀는 이력을 지우는 reset 대신 `git revert <문제 커밋>`으로 되돌리고 다시 검증합니다.

빌드·로그는 기존 제외 규칙을 유지하고, 요청된 스크린샷만 검토된 `Docs/Screenshots/<작업ID>/` 같은 경로에 보관해 기준 자료를 덮어쓰지 않는 방식이 적절합니다.

## Changed files

- (none)

## Error

None.

> Project changes stay in a local isolated worktree. This runner does not commit or push project code, merge branches, or write to main. It pushes this report only to the dedicated report branch.