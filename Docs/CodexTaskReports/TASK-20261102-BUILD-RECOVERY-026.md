# VoxelOn local Codex task report: TASK-20261102-BUILD-RECOVERY-026

- Status: **completed**
- Mode: check
- Priority: 98
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T21:27:30.8981351Z
- Finished: 2026-11-01T21:28:34.9864820Z

## Summary

판정: **변경 중단. 새 Windows 빌드는 생성하지 않았습니다.**

- **변경 파일:** 없음. 생성·편집·삭제·스테이징하지 않았습니다.
- **실행한 확인:** 작업 트리 파일 목록·문서·점프 소스 읽기, 보고서/러너 설정 검색, Git·Unity 명령 검색.
- **차단 요인:** 중첩 작업자에서 `git`과 `rg`가 검색되지 않습니다. 009·023·024 보고서와 큐/러너 설정은 작업 트리에 없습니다. `.git`은 외부 경로를 가리켜 접근하지 않았습니다.
- **검증 상태:** 현재 HEAD·브랜치와 승인 SHA의 일치를 확인하지 못했습니다. 외부 러너의 SHA 고정 및 Git PATH도 미확인입니다. Unity 명령은 검색되지 않았으며, 설치 여부·빌드·스모크·스크린샷은 검증하지 않았습니다. 제공된 209 PASS는 이번 실행 결과가 아닙니다.

운영자의 최단 복구 절차:

1. 외부 러너에서 실제 Git 설치 경로를 확인하고, 해당 디렉터리를 러너와 자식 작업자의 PATH에 전달한 뒤 재시작합니다.
2. **외부 러너와 중첩 작업자 양쪽에서**, 대상 작업 트리를 지정하여 아래 검사를 실행하고 출력을 보존합니다.

```powershell
$expected = 'b757bcca068acbf59cd0eed4f97910246202f2bb'
Get-Command git -ErrorAction Stop
git --version
git -C $worktree rev-parse --verify "$expected^{commit}"
git -C $worktree rev-parse HEAD
git -C $worktree branch --show-current
git -C $worktree status --porcelain
```

`$worktree`는 각 단계의 실제 대상 경로로 지정해야 합니다. 명령 실패, SHA 불일치, 지정 브랜치 불일치 또는 작업 트리 변경이 있으면 중단합니다. 큐의 기준값도 **전체 SHA**인지 확인하고, 009·023·024 보고서를 다음 작업자가 읽을 수 있도록 제공합니다. 현재 설정 형식을 볼 수 없어 구체적인 설정 키는 확정할 수 없습니다.

검증 이후에는 완료 ID를 재실행하지 않고 새 격리 작업 두 개로 진행하는 것이 적절합니다.

- **점프:** 기존 고정 2배 점프 요구를 명시적으로 대체하고, 무게 기반 일반 점프·Space 유지 상승·공중 해제 후 재입력을 구현합니다. 조립/중량 UI 확장은 제외합니다.
- **Kestrel:** 실제 머리·어깨 부피만 소폭 개선하고 장식봉은 추가하지 않습니다.

각 작업은 승인 SHA에서 별도 브랜치로 시작하고, 기존 정상 실행본을 보존한 뒤 Unity 빌드 → `VOXSKY_BUILD_PASS` 및 오류 확인 → 기존 스모크 전체 실행 → 같은 포즈·카메라 전후 스크린샷 비교 순으로 검증해야 합니다. 정확한 브랜치·커밋·로그를 기록하며, 009 완료나 콘셉트 아트 승인은 선언하지 않습니다.

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.