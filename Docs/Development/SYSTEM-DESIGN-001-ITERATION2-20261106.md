# 시스템 확장 30분 단위 작업 — 2차 8개 설계 슬롯 UI 연결

작성: 2026-11-06. 이번 단계에서는 저장 기반을 행거 편집 화면에 연결했다. 변경은 현재 `work` 기준 작업 트리에만 있으며 `main`은 건드리지 않았다. 아직 커밋·푸시하지 않았다.

## 구현

- 데이터 형식을 v2에서 v3으로 올리고 `activeDesignId`를 저장한다.
- 기존 v1 밸런스 파일은 기존 세 기체 프리셋을 보존하며 v3으로 옮긴다. v2의 8개 설계도 이름·기체·4개 장착 정보를 유지해 v3으로 옮기고 첫 슬롯을 기본 선택으로 정한다.
- 행거 왼쪽에 `DESIGN_01`~`DESIGN_08` 선택 버튼을 배치했다. 버튼은 현재 기체를 함께 표시하고, 선택한 슬롯의 이름·기체·손/어깨 장비를 편집한다.
- 저장 파일 불러오기 후 저장된 현재 슬롯과 기체를 편집 화면에 복원한다. 기체 종류를 바꾸면 호환되지 않는 무기만 그 기체 기본 장비로 교체한다.
- [전투에 적용]은 선택한 설계의 장비를 해당 플레이어 기체에 반영한다. 상대가 다른 기체면 기존 기체 기본 장비를 사용한다. 파츠 수치는 아직 기체별 값 한 벌이라 설계 슬롯별 독립 튜닝은 미완성이다.
- 별도 기본 기체/아이템 등록 체계는 아직 없다. 새로운 무기나 파츠 ID 등록은 여전히 기본 카탈로그/아트 연결 작업을 요구한다.

## 실제 검증

- Unity 6000.6.7f1 Windows 빌드 성공: `VOXSKY_BUILD_PASS 237239710 bytes`, `Logs/DesignSlots-Iteration2-Build-StudyRetry.log`.
- 실제 편집/버전 이전 검사: `BALANCE_EDITOR_STUDY_PASS 51 checks; 10 images`, `Logs/DesignSlots-Iteration2-EditorStudy.log`.
- 실제 전투 회귀: `VOXSKY_SMOKE_PASS 358 checks`, 최종 실행 파일 검사 `Logs/DesignSlots-Iteration2-Smoke-Final.log`.
- 손 장착 회귀: `REFERENCE_HAND_STUDY_PASS 36 checks; 27 actual images`, `Logs/DesignSlots-Iteration2-ReferenceHands.log`. 새 결과는 `Docs/Screenshots/ReferenceHands-20261106/Unity-v004-SchemaV3-Iteration2/`에 저장했다.
- 캡처 스크립트는 화면 파일 10개를 만들었지만, 이후 확인에서 `00-custom-profile-live-hud.png`, `00b-gallery-actual-equipped-slots.png`, `01-weapon-options.png`, `02-part-options.png`, `03-independent-loadout.png`, `04-save-load.png`가 동일한 검은 이미지임을 발견했다. 이 6개는 유효한 시각 증거가 아니며 `Backups/Rejected-Black-Captures-20261107/`에 보존했다. 화면 비교와 수동 UI 승인은 미완료다. 실제 유효 화면 캡처는 뒤이은 Iteration 3·4·5·7 폴더를 사용한다.
- 기존 손 테스트 사진 디렉터리와 이전 편집 화면을 덮지 않았다. Unity 빌드가 자동 변경한 `Assets/Scenes/Arena.unity`는 원래 저장소 내용으로 복구했다.

## 다음 단계와 제한

현재 8개는 고정 슬롯이며 복사/삭제/초기화 UI는 없다. 슬롯 4~8은 WISP 기본 구성으로 시작한다. 커스터마이징된 기체 이름은 슬롯에서 저장되지만, 외부 플레이어 선택/리스폰 화면과 연결되지 않는다. 파츠 수치가 설계 슬롯별로 격리되지 않고 무기/파츠 정의 자체도 아직 확장 레지스트리 방식이 아니다.

다음에는 슬롯 복사·기본값 초기화와 파츠 수치의 설계별 분리 범위를 확정해 저장/불러오기 테스트를 추가한다. 이후 카탈로그 ID를 정의 데이터와 분리하고, 새 무기 한 종류와 새 파츠 한 종류를 추가해 실제 발사/이동 변화가 런타임에 나타나는 수직 검증을 한다. 팀전·스킬·중력/날씨 확장은 그 뒤로 둔다.
