# WISP 신규 헤드·코어 선택 옵션 — 2026-11-18

## 이번 반복

이전 반복에서 연결한 백팩·하체 5종에 이어, 이미 제작되어 검토 폴더에 있던 헤드 6종과 코어 4종을 외형 선택 메뉴에 연결했다. 외형 변경만 수행하며, 수치·이동·피해·무기 동작은 변경하지 않았다.

모델 선택은 전투 밸런스 저장 파일과 분리된 기존 `WISP-Visual-Loadouts.json`에 저장한다. 이전 버전에 헤드·코어 키가 없던 외형 저장본은 `HD01`·`BD01` 기본 외형으로 이어지도록 처리했다. WISP, BASTION, TARANTULA에 적용되며 HOUND는 신규 전용 가족 모델이 준비되지 않아 현재 개 전용 외형을 유지한다.

## 선택 가능한 신규 모델

| 부위 | 새 표시명 | 런타임 ID | 같은 각도 게임 캡처 |
|---|---|---|---|
| 헤드 | H-01 레이돔 | `VIS_H01_RADOME` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H01_RADOME.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H01_RADOME-Back.png) |
| 헤드 | H-02 롱사이트 | `VIS_H02_LONGSIGHT` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H02_LONGSIGHT.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H02_LONGSIGHT-Back.png) |
| 헤드 | H-03 스위프트핀 | `VIS_H03_SWIFTFIN` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H03_SWIFTFIN.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H03_SWIFTFIN-Back.png) |
| 헤드 | H-04 불워크 | `VIS_H04_BULWARK` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H04_BULWARK.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H04_BULWARK-Back.png) |
| 헤드 | H-05 니들 | `VIS_H05_NEEDLE` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H05_NEEDLE.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H05_NEEDLE-Back.png) |
| 헤드 | H-06 랜싯 | `VIS_H06_LANCET` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H06_LANCET.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Head-VIS_H06_LANCET-Back.png) |
| 코어 | C-01 스탠더드 | `VIS_C01_STANDARD` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C01_STANDARD.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C01_STANDARD-Back.png) |
| 코어 | C-02 라이트프레임 | `VIS_C02_LIGHTFRAME` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C02_LIGHTFRAME.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C02_LIGHTFRAME-Back.png) |
| 코어 | C-03 불워크 | `VIS_C03_BULWARK_CORE` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C03_BULWARK_CORE.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C03_BULWARK_CORE-Back.png) |
| 코어 | C-04 아크 | `VIS_C04_ARC_CORE` | [전면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C04_ARC_CORE.png) · [후면](../Validation/WispFamily-VisualCandidates-20261118/Core-VIS_C04_ARC_CORE-Back.png) |

캡처는 Unity 플레이어에서 공통 카메라·크기·조명 조건으로 실제 조립한 모델을 `RenderTexture`에 그려 저장했다. 각 앞·뒤 쌍의 구도는 같아 후보의 실루엣을 비교할 수 있다. Unity UI를 클릭하거나 PC 화면을 전환하지 않았다.

## 연결과 접합 검증

- Blender 5.2.3 제작 원본 10개를 Blender 4.5.15 백그라운드에서 읽어 게임용 메시 자원으로 변환했다. `.blend` 원본은 저장하지 않았다. Blender가 파일의 새 버전과 사용 불가 EEVEE 설정 경고를 냈지만, 모든 후보에서 메시 추출과 자원 생성을 마쳤다.
- 코어 4종은 공통 허리·목·팔꿈치 접합 수치를 handoff 매니페스트에서 읽고, 원래 왼팔·오른팔 어깨 피벗에 각각 연결했다. 기존 무기 조준·가드 애니메이션 코드가 쓰는 양쪽 어깨 노드도 유지했다.
- 기존 무장 외형 및 파츠 자원은 덮어쓰지 않았다. 새 후보는 충돌 방지용 `VIS_` ID로 등록했다. `Docs/Design/WISP_Parts/ID-MAPPING.json`에 소스와 런타임 경로를 기록했다.
- 커스터마이징 목록에 머리·몸통 줄을 더하고, 기존 백팩·하체·양손 외형 줄과 겹치지 않도록 버튼 영역을 아래로 재배치했다. 실제 마우스 조작은 요청대로 하지 않아 화면의 수동 확인은 남아 있다.

## 검증 결과

- Blender 변환: **15개 후보**(기존 백팩·하체 5 + 신규 헤드·코어 10) 출력 완료.
- ID·소스·카탈로그·Unity GUID 검사: `python Tools/verify_wisp_part_ids.py` 통과. 런타임 ID와 검토 ID가 분리되어 있고, 신규 선택 후보 15개 모두 소스·리소스·`.meta`가 존재한다. Assets 전체 355개 `.meta`를 검사한 결과 GUID 중복은 0개였고 신규 후보 15개의 GUID도 모두 고유하다.
- Unity 6000.6.7f1 Windows 빌드: `VOXSKY_BUILD_PASS 260554574 bytes` — `Logs/Legacy-Loadout-Compatibility-Build-20261118.log`.
- 실제 플레이어 외형 검사: 기본 85개 검사에 신규 헤드 6종×코어 4종을 WISP·BASTION·TARANTULA에 모두 장착한 **72개 조합 검사**를 더하고, 저장 프리셋의 저장/다시 읽기와 구버전 저장본의 기본값·기존 부품 ID·무기 슬롯 호환, 신규 후보 15종의 한국어 표시명, BASTION·TARANTULA의 새 하체와 코어 간 **허리 접합 48회 검사**를 포함해 `WISP_FAMILY_REVIEW_PASS 294 checks` (실패 0). BASTION은 새 무한궤도 하체, TARANTULA는 새 관절 사족 하체를 장착했다. 각 기체의 헤드·코어·하체·백팩·주무기 외형이 다른 기체의 프리셋을 덮지 않는지도 검증했다. 72개 조합의 전·후면 **144장** 캡처는 [세 기체 전체 비교 인덱스](../Validation/WispFamily-HeadCore-ComboMatrix-20261118/INDEX.md)에 정리했다. 접촉 시트에서 뒤쪽 백팩과 코어가 가리는 부분이 있는지도 확인했고 뚜렷한 겹침은 발견하지 못했다. 실행 로그: `%TEMP%\voxelon-legacy-compat-review-20261118\FamilyReview.log`.
- 플레이어 회귀 스모크: 최종 재빌드 실행본에서 `VOXSKY_SMOKE_PASS 377 checks`, 종료 코드 0. 로그: `%TEMP%\voxelon-legacy-compat-review-20261118\Smoke.log`.
- `Builds/Windows/Voxel-On.exe`를 생성했다. 빌드가 다시 저장한 `Assets/Scenes/Arena.unity`와 `ProjectSettings/ProjectSettings.asset`은 빌드 전 스냅샷과 동일한 내용으로 복원했다. `Data/Balance/PD-Balance.json` 해시는 작업 전후 `0F470499D4E21FB112A7E502EA5175C98777B110A7F8F87D0461A21A5A1201E9`로 동일하다.
- 작업 브랜치는 `codex/pd-review-fixes-20261114`이며 커밋·푸시는 하지 않았다. 플레이어를 실제로 열어 클릭하는 사용자 승인과 커스터마이징 UI 수동 검수는 남아 있다.

## 다음 단계

새 머리·코어의 전·후면 실루엣과 세 기체별 전체 72개 조합, 저장 분리, 허리·어깨 접합, 신규 후보의 한국어 UI 표시는 자동 플레이어 검사에서 통과했다. 렌더 캡처는 같은 각도·광원으로 구성했으며, 후보 차이가 주로 머리와 코어 덩어리 형태에 한정되는지 비교할 수 있다. 실제 UI를 열어 선택하는 수동 검수는 아직 남아 있다. 다음 단계에서는 기체 간 변형 크기와 인게임 시점에서의 가독성을 비교하고, HOUND용 신규 가족 모델이 제공되면 같은 외형 선택 체계에 연결한다. 이 변경에서는 스탯과 특수 기능 설계서를 구현하지 않았다.


## 2026-11-18 추가 호환성 재검증

- 구형 저장 JSON에 기존 `HD01`/`BD01`, `L-01_STRIDER`, `B-01_SKYWING` 및 두 번째 무기 `W-01_LUMEN`이 들어 있는 경우, 로드 결과가 동일하게 유지되는 플레이어 검사를 추가했다. 기존 로드아웃 ID를 새 후보 ID로 강제 치환하거나 원본 아트 자원을 덮어쓰지 않음을 확인했다.
- Unity 6000.6.7f1 Windows 빌드 `VOXSKY_BUILD_PASS 260554574 bytes`; 실행본 검사 `WISP_FAMILY_REVIEW_PASS 294 checks`, `VOXSKY_SMOKE_PASS 377 checks`, 모두 종료 코드 0.
- 정적 파츠 ID 검사 통과: 런타임·검토 ID 공간 분리, 후보 GUID 중복 없음. `git diff --check` 통과.
- 빌드 전후 `Arena.unity` SHA-256 `45394688702DBA33CA8555111CB91A97D558098B9B49E413F3CD38E4C122BE8B`, `ProjectSettings.asset` SHA-256 `4D41233DA1E1B48344AB34487458AD6F2DFC0E0A2E94F5548111AF0956998463`로 동일하다. PD 밸런스 v7 SHA-256도 `0F470499D4E21FB112A7E502EA5175C98777B110A7F8F87D0461A21A5A1201E9`로 유지됐다.
- 로그: `Logs/Legacy-Loadout-Compatibility-Build-20261118.log`; 실행 로그는 `%TEMP%\voxelon-legacy-compat-review-20261118\`에 보존했다. 화면 조작·커밋·푸시는 하지 않았다.
