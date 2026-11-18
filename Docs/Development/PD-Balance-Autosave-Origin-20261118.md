# PD Balance v7 변경 원인 조사 — 2026-11-18

## 확인된 사실

- 현재 `Data/Balance/PD-Balance.json`: SHA-256 `E7FBAC20257710F4D3750D0772118CB7C9DA2CA507CC0B3FD6BE3CC39D8EC32E`, 수정 시각 21:13:30 KST, format v7, `activeDesignId=DESIGN_02`.
- 인접 `.bak`: SHA-256 `D3BCF36F6EC3A1D3A7405C8A092164C78B31E6966FEAFE26ED74C899CDCF4346`, 수정 시각 21:12:33 KST, format v7, `activeDesignId=DESIGN_03`.
- 두 파일의 내용 차이는 활성 설계 ID다. 대화 내 앞선 검사에서 UI 확인 전 해시는 `0F470499D4E21FB112A7E502EA5175C98777B110A7F8F87D0461A21A5A1201E9`로 기록되어 있다.
- `%USERPROFILE%\AppData\LocalLow\VoxSky\Voxel-On\Player.log`에는 커스터마이징 화면의 `DrawFamilyArtOptions → SaveFamilyConfig → RefreshBalanceDraftPreview` 호출 흔적과 `FlushAutomaticBalance` 호출 흔적이 있다. 코드에서 `OpenBalanceEditor → SelectCustomizationDesign`는 기본적으로 `balanceDirty`를 설정하고, `LateUpdate`의 `FlushAutomaticBalance`는 `BalanceFiles.Save(BalancePath(), valid)`를 호출한다. 따라서 화면에서 기체/설계를 오가며 열어 본 과정의 자동 저장이 파일 변경 원인으로 판단된다.
- 현재 파일과 `.bak`은 `%TEMP%\voxelon-pd-balance-recovery-candidates`에 별도 보존했다. 기록된 사전 해시와 일치하는 원본 사본은 현 작업 폴더, Temp 복구 후보 및 LocalLow에서 찾지 못했다.

## 조치

이번 파츠 선택 UI 작업에서는 PD 밸런스 파일을 수정하지 않았다. 기존 v7 파일과 백업을 그대로 두었고, 원본 해시와 일치하는 사본을 확보하지 못했으므로 추정값으로 복구하거나 덮어쓰지 않았다. 정확한 사전 파일 복구는 원본 사본을 찾을 때까지 미완료다.
