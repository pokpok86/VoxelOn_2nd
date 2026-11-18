# 파츠 선택 갤러리 — 2026-11-18

## 변경 내용

신형 외형 탭의 작은 좌우 화살표 선택기를 분류별 갤러리로 교체했다. 머리, 몸통, 하체, 백팩, 왼손 무기, 오른손 무기를 나눠 실제 기체에 장착해 Unity에서 렌더한 후보 이미지를 카드로 표시한다. 카드를 누르면 왼쪽 조립 미리보기에 적용되며 기존 외형 설정 파일에 자동 저장된다. 성능·밸런스 값은 변경하지 않는다.

카드 이미지는 총 31종이며, 테스트는 양손 무기 목록을 각각 확인해 카드 텍스처 36개 참조를 검증한다. 후보 이미지는 선택 카드에 맞춰 최대 512px로 가져와 빌드 용량을 줄였다. 선택 화면을 게임 UI와 같은 해상도에서 실제로 열어 확인하는 수동 검토는 아직 하지 않았다. 화면 제어를 피하기 위해 숨김 실행으로 검사했으며, 숨김 창의 화면 캡처는 검게 나와 증거로 사용하지 않았다.

## 검증

- Unity 6000.6.7f1 Windows 빌드: `VOXSKY_BUILD_PASS 278847704 bytes` — `Logs/Part-Selection-Cards-Build-20261118-final.log`.
- 실제 플레이어 파츠·프레임·장착 검토: `WISP_FAMILY_REVIEW_PASS 330 checks` — `Logs/Part-Selection-Cards-Review-20261118-final.log`.
- 기본 플레이어 회귀 스모크: `VOXSKY_SMOKE_PASS 377 checks`, 종료 코드 0 — `Logs/Part-Selection-Cards-Smoke-20261118.log`.
- 신규 파츠 ID/GUID 검사: `Tools/verify_wisp_part_ids.py` 통과.
- `git diff --check` 통과.
- 빌드 전후 `Assets/Scenes`와 `ProjectSettings`를 같은 파일 스냅샷으로 복원했다. PD 밸런스 파일은 이번 UI 갤러리 변경에서 수정하지 않았다.
- 브랜치: `codex/pd-review-fixes-20261114`. 커밋/푸시는 하지 않았다.
