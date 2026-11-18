# 파츠 선택 화살표 화면 — 2026-11-18

## PD 요청 및 적용

왼쪽은 조립된 로봇, 오른쪽은 현재 선택한 파츠 하나를 크게 표시한다. 분류 버튼으로 머리·몸통·하체·백팩·왼손 무기·오른손 무기를 선택하고 좌우 화살표로 순환한다. 선택 시 즉시 적용 및 기존 외형 파일 자동 저장 방식이다. 이미지 카드를 대신해 실제 Blender에서 추출한 게임 모델을 전용 카메라로 렌더한다. 커스터마이징 진입 시 이 화면을 기본으로 연다.

편집 중인 설계 ID를 외형 설정 키에 사용하여 다른 설계와 혼동할 가능성도 수정했다. 성능 수치는 편집하지 않았다. 어깨 무장은 기존 구현의 백팩 연결 준비 상태를 유지한다.

## 실제 검증

- Unity 6000.6.7f1: Logs/ArrowSelector-Build.log, VOXSKY_BUILD_PASS 278849702 bytes.
- 실제 플레이어 검토: Logs/ArrowSelector-Review.log, WISP_FAMILY_REVIEW_PASS 346 checks. 새 검사는 머리·몸통·하체·백팩의 다음/이전 선택, 실제 로봇 메시 ID 일치, 전용 파츠 RenderTexture 생성을 확인한다.
- 기본 회귀: Logs/ArrowSelector-Smoke.log, VOXSKY_SMOKE_PASS 377 checks.
- 숨김 창 캡처가 검게 나와 Direct3D11 배치 렌더로 추가 시도했지만 동일했다. ArrowSelector.png는 화면 검증 증거로 사용하지 않는다. 전체 화면의 배치·글자·화살표 직접 클릭 및 미리보기의 실제 가시성은 수동 확인 대기다. 검사 통과를 화면 가시성 승인으로 취급하지 않는다.
- 빌드 전 씬·ProjectSettings 파일을 Logs/ArrowSelector-BackupPath.txt가 가리키는 백업에서 복구했다. PD-Balance.json 빌드 전후 해시 일치 확인.
- 실행 파일: Builds/Windows/Voxel-On.exe. 과거 ZIP 패키지는 이 변경을 포함하지 않는다.
- 커밋/푸시하지 않음.
