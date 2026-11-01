# VoxelOn local Codex task report: TASK-20261101-BUILD-AUDIT-001

- Status: **completed**
- Mode: check
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Started: 2026-11-01T04:36:01.0097118Z
- Finished: 2026-11-01T04:37:01.7797313Z

## Summary

조사 결과, **이 러너에서 Unity 빌드·플레이 테스트는 미검증**입니다. 확인된 문제는 게임 컴파일 오류가 아니라 도구 검색 실패와 읽기 전용 실행 제약입니다.

- `git --version`, `git status --short --branch`, `rg` 실행은 명령을 찾지 못해 실패했습니다.
- `Get-Command Unity,Unity.exe,git` 검색 결과가 없었고, 작업 폴더 내부에서도 해당 실행 파일을 발견하지 못했습니다.
- 작업 폴더 밖의 Unity 설치 경로는 접근 제한에 따라 확인하지 않았습니다. 따라서 소유자의 Windows PC에도 Unity가 없다고 판단할 수 없습니다.
- `.github`, `Builds`, `Logs` 폴더는 없습니다. 현재 브랜치와 변경 상태도 Git으로 확인하지 못했습니다.

확인한 프로젝트 설정은 다음과 같습니다.

| 항목 | 근거와 값 |
|---|---|
| Editor | `ProjectSettings/ProjectVersion.txt`: **6000.6.7f1**, revision `b22767f04f04` |
| 진입 씬 | `ProjectSettings/EditorBuildSettings.asset`: `Assets/Scenes/Arena.unity`, 활성화됨 |
| 빌드 진입점 | `Assets/Editor/VoxBuild.cs`: `VoxBuild.BuildWindows` |
| 대상·옵션 | `StandaloneWindows64`, Development |
| 출력 | `Builds/Windows/VoxSky.exe` |
| 주의점 | `BuildWindows()`가 `CreateScene()`을 호출해 씬과 PlayerSettings, 빌드 씬 목록을 저장함 |

**기존 빌드 명령 자체가 파일을 변경하므로 이번 읽기 전용 작업에서는 실행하지 않았습니다.**

Windows 호스트에서는 다음 절차를 권장합니다. 아래 명령은 실행하지 않은 재현용 절차이며, 쓰기가 허용된 별도 검증 복사본에서 사용해야 합니다.

1. 정상 실행되던 Windows 빌드 폴더 전체를 보존합니다. Unity 6000.6.7f1, 유효한 라이선스, Windows 빌드 지원 모듈을 확인하고 해당 프로젝트의 Editor를 닫습니다. 동시에 빌드하지 않습니다.
2. 변경 전 기준 소스를 별도 검증 복사본에 준비하고, 프로젝트 루트에 `Logs` 폴더를 준비한 뒤 실행합니다.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe' `
  -batchmode -quit -projectPath (Get-Location).Path `
  -executeMethod VoxBuild.BuildWindows `
  -logFile (Join-Path (Get-Location).Path 'Logs/BuildCloudChanges.log')
```

3. Unity 종료까지 기다린 후 종료 코드, 컴파일·빌드 오류, `VOXSKY_BUILD_PASS`를 확인합니다. 실패하면 해당 로그의 실제 오류부터 해결합니다. 현재는 실패 로그가 없어 라이선스나 모듈 문제를 원인으로 단정할 수 없습니다.
4. 빌드 성공 후 그래픽 사용이 가능한 Windows 세션에서 실행합니다.

```powershell
Start-Process -FilePath .\Builds\Windows\VoxSky.exe `
  -ArgumentList @('--smoke-test','-logFile','Logs/Smoke.log') `
  -WorkingDirectory (Get-Location).Path -Wait -PassThru
```

종료 코드 0과 `VOXSKY_SMOKE_PASS`, 개별 `CHECK` 실패·예외 유무를 확인합니다. 캡처는 `Builds` 아래에 생성됩니다. 과거 187개 통과 기록은 이번 코드의 증거가 아닙니다.

5. 갤러리는 별도로 실행합니다.

```powershell
Start-Process -FilePath .\Builds\Windows\VoxSky.exe `
  -ArgumentList @('--capture-docs-gallery','-logFile','Logs/Gallery.log') `
  -WorkingDirectory (Get-Location).Path -Wait -PassThru
```

`VOXSKY_GALLERY_PASS 36 images`와 실제 PNG를 확인합니다. 이 명령은 현재 작업 디렉터리의 `docs/Screenshots` 이미지를 덮어쓰므로 기준·변경 버전을 서로 다른 검증 복사본에서 촬영해야 합니다.

VISUAL-001의 Kestrel 변경은 **통합 여부를 확인하지 못했습니다**. 작업 폴더에서 VISUAL-001/002 이름의 보고서도 발견하지 못했습니다. 먼저 기준 빌드·스모크·캡처를 완료한 뒤, 확인된 Kestrel 변경만 별도 검증 복사본에 적용하여 같은 절차를 반복해야 합니다. 기존 갤러리 도구는 로봇 촬영을 768×432, FOV 40°, 거리 8.7, 높이 오프셋 3, 정면·후면·좌우로 고정하므로 이 설정을 유지해 비교할 수 있습니다.

**변경 파일:** 없음.  
**실제 수행:** 작업 폴더 내부 파일·설정·캡처 코드 열람, 명령 검색 및 실행 시도.  
**남은 차단 요인:** 러너에서 Unity/Git 검색 불가, 읽기 전용 제약, 호스트 설치·라이선스·그래픽 환경 미확인, Kestrel 변경 통합 상태 미확인.

## Changed files

- (none)

## Error

None.

> Project changes stay in a local isolated worktree. This runner does not commit or push project code, merge branches, or write to main. It pushes this report only to the dedicated report branch.