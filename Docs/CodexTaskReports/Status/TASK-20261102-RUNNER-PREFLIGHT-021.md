# Task status: TASK-20261102-RUNNER-PREFLIGHT-021

| Time (UTC) | Status | Detail |
| --- | --- | --- |
| 2026-11-01T16:08:05.4463833Z | started | Priority 2; isolated work branch detached/read-only; source f808e5952fee. |
| 2026-11-01T16:08:18.2353488Z | in-progress | Codex is executing the approved task in an isolated worktree. |
| 2026-11-01T16:09:26.1432774Z | completed | 확인 결과, **러너가 중첩 Codex 실행 전에 전체 SHA와 작업 트리 상태를 검증하는 방식**이 적절합니다. 다만 현재 허용된 작업 트리에는 러너 구현·큐 설정·019 보고서가 없어, 실제 지원 여부와 오래된 SHA의 출처는 확정하지 못했습니다.

- 중첩 환경에서 `git`, `rg`를 PATH로 찾지 못했습니다. **바깥 러너의 Git 사용 가능 여부는 별도 확인이 필요합니다.**
- `.git`은 작업 트리 밖의 Git 메타데이터를 가리킵니다. 접근 제한을 지켜 해당 경로는 읽지 않았습니다. 현재 브랜치·HEAD·clean 상태도 검증하지 못했습니다.
- 작업 트리 텍스트 검색에서 `source_sha`, `expected_source_sha`, 두 SHA 및 러너 설정을 찾지 못했습니다. 큐 frontmatter 지원 여부, `f808e5952fee` 기본값의 출처, 모든 change 작업의 공유 여부는 미확인입니다. 캐시된 작업 source와 실제 checkout은 따로 조사해야 합니다.

최소 구현 계획은 다음과 같습니다.

1. 러너의 큐 파서 → 작업 pickup → 기본 source 결정 → worktree 생성 → Codex 실행 경로를 확인합니다. `source_sha`가 없거나 무시된다면 러너를 먼저 수정합니다.
2. 다음 change 작업에 `expected_source_sha = b757bcca068acbf59cd0eed4f97910246202f2bb`를 필수로 지정합니다. 누락·잘못된 값은 기본 branch로 대체하지 않고 실패시킵니다.
3. 러너가 허가된 격리 작업 브랜치를 해당 커밋에서 만들고, `git rev-parse --verify HEAD`의 전체 값과 `git status --porcelain=v1 --untracked-files=all`의 빈 출력을 검증합니다. 실패하면 Codex를 시작하지 않습니다.
4. 검증 결과를 작업 ID·경로·기대 SHA·실제 SHA·clean 여부와 함께 중첩 환경이 읽을 수 있는 파일/env로 전달합니다. 러너가 원본 증거를 보관하고, Codex가 수정할 수 없도록 해야 합니다.

정확한 수용 검사는 다음과 같습니다.

- 정확한 SHA + clean 작업 트리만 실행 허용.
- SHA 누락·축약·불일치·존재하지 않는 커밋·Git 실패 시 실행 차단.
- tracked 수정·staged 변경·untracked 파일이 각각 있을 때 실행 차단.
- 중첩 Codex의 Git이 없어도 러너 검증과 증거 읽기 성공.
- 큐 캐시가 오래되거나 branch가 이동해도 지정한 전체 SHA 유지.
- 기존 테스트·assertion을 유지하고 실패 원인을 고친 뒤 재실행.

시각 후속 후보는 **Bastion 전용 헬멧 폭 확대**입니다. 현재 `Assets/Scripts/VoxArena.Robot.cs:88`의 heavy 헬멧 폭은 Anvil과 공유하므로 Bastion에만 적용해야 합니다. 019 보고서와의 일치는 미확인입니다. 수용 조건은 동일 카메라·거리·해상도·포즈·조명의 정면/쿼터뷰 전후 캡처에서 머리 실루엣 개선과 어깨 간섭 없음 확인입니다.

**변경 파일:** 없음.  
**실행한 검사:** 파일 목록·텍스트 검색, 프로젝트 문서와 관련 모델 코드 읽기, 명령 가용성 확인.  
**제약:** Windows PowerShell은 사용 가능하지만 러너 구현·pickup 기록은 허용 범위에 없습니다. Unity 빌드·플레이 테스트·스크린샷 검증은 실행하지 않았습니다. |
