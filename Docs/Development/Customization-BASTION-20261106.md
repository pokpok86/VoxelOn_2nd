# BASTION 디자인 및 커스터마이징 화면

## 반영 내용

- 승인된 행거 UI 목업을 `Docs/VisualReferences/Customization-Bastion-Mockup-20261106.png`로 보존했다.
- BASTION은 기존 v009 원본을 보존하고 Blender 제작 v010을 추가했다: `Assets/Art/Robots/BASTION/Source/BASTION_v010.blend`, `Assets/Art/Robots/BASTION/BASTION_v010.fbx`. 밝은 회백색 몸체, 주황 어깨, 청록색 센서, 궤도 하체를 적용했다. 기존 관절 계층은 유지했다.
- Unity 장면과 `VoxBuild.BuildWindows` 모두 BASTION v010을 사용하도록 연결했다. 이전 v009 FBX/Blend는 변경하지 않았다.
- 캐릭터 선택 카드에서 커스터마이징 화면을 열면 좌측에 선택 기체의 실제 모델과 무장이 보인다. 마우스 드래그와 45도 버튼으로 회전한다.
- 좌측 기체 선택은 WISP/BASTION/TARANTULA 모두 가능하다. TARANTULA 외형은 이번 작업에서 변경하지 않았다.
- 우측 장착 화면에 왼손, 오른손, 좌우 어깨 슬롯을 두고, 장착 가능한 무기를 실제 3D 썸네일 목록에서 고른다. 선택한 무기는 우측 큰 3D 미리보기에 보이며 옵션 편집 화면으로 바로 이동할 수 있다.
- 별도 탭에서 무기 수치, 파츠 수치, 설정 파일 이름·저장·불러오기·전투 적용을 편집한다.
- 일반 실행은 Windows 전체화면 창 모드 설정을 유지한다.

## 빌드 및 화면 증거

- Unity 6000.6.7f1 Windows 개발 빌드 성공: `Logs/Customization-BASTION-UI-Build-v006.log`, `VOXSKY_BUILD_PASS 237220694 bytes`.
- 실제 실행 화면 캡처: `Docs/Screenshots/Customization-20261106/Bastion-Customization-Actual-v002.png`.
- 실행 로그: `Logs/Customization-BASTION-UI-Capture-v005.log`, `BASTION_CUSTOMIZATION_CAPTURED` 및 종료 코드 0.
- 화면 확인 후 BASTION 프리뷰 배율과 우측 콘텐츠 위치를 조정하고 다시 빌드·캡처했다. 최종 캡처에서 기체가 프리뷰 틀 안에 들어오고 무기 썸네일 목록이 표시됨을 확인했다.
- 이 작업에서는 테스트 스위트를 돌리지 않았다. 수동 마우스 조작과 저장 후 재실행 확인은 남아 있다.

## 제한 및 다음 단계

- 무기·파츠 수치 중 UI에 `[보관]`으로 표기되는 항목은 파일에 저장되지만 아직 전투 계산에 반영되지 않는다. 표시대로 구현된 상태를 구분한다.
- 무기 썸네일은 실행 시 카탈로그의 실제 3D 무기 모델을 작은 이미지로 렌더링한다.
- BASTION v010의 수동 미술 최종 승인은 박PD 확인 대기다. TARANTULA 리디자인은 이번 범위에서 제외했다.
- 수정은 로컬 `work` 작업 폴더에만 적용했으며 커밋/푸시는 하지 않았다.

## 후속 요청 — 몸통 파츠 분리 (미적용)

박PD 후속 의견: 2·3번을 1번 몸통에 하체만 교체한 형태로 두지 않고, 몸통도 무기처럼 늘리고 교체할 수 있도록 각 기체의 상체 실루엣을 차별화한다. 이번 커밋에는 해당 모델/장착 슬롯 구현이 포함되지 않았다. 2번은 궤도 하체와 이어지는 넓고 낮은 중량형 몸통, 3번은 사족 다리 관절에 연결되는 길고 낮은 중앙 몸통으로 설계하는 후속 작업으로 기록한다. 승인된 BASTION v010과 기존 게임 동작은 보존한다.
