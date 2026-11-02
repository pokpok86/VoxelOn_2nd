# Robot Model Handoff

## 전달 현황

2026-11-02: WISP 외형 시안 v001 전달. BASTION / TARANTULA / HOUND는 미제작이며 폴더만 준비했습니다. 현재 게임의 모델이나 프리팹 연결을 변경하지 않았습니다.

## 파일

`Assets/Art/Robots/WISP/`

- `WISP_v001.fbx`: 무기 미장착 기체
- `WISP_Sword_v001.fbx`: 분리된 칼
- `WISP_Shield_v001.fbx`: 분리된 방패
- `Textures/WISP_BaseColor.png`: 복셀 색상 아틀라스
- `Textures/WISP_Emission.png`: 식별등·바이저·부스터 발광 아틀라스
- `Source/WISP_v001.blend`: 수정용 Blender 원본. 미리보기에서는 무기를 장착했습니다.

외부 PNG에 실제 UV가 연결되어 있습니다. 임의 참고 이미지가 아닙니다. 색상별 다수 재질을 `WISP_Atlas` 한 재질로 통합했습니다. 복셀은 개별 게임 오브젝트가 아니라 부품 메시 안의 면입니다.

## 좌표·단위·기본 자세

- Unity 목표 기준: +Y 위, +Z 전방, +X 오른쪽, 1단위 = 1미터.
- Root는 원점, 발바닥 최하단은 Y=0. 기본 자세는 양팔을 내린 직립 자세입니다. T-pose나 Humanoid 리그가 아닙니다.
- 아래 좌표는 Root 기준 미터 단위이며, 회전은 기본 자세 대비 변화량으로 적용합니다. FBX가 가져온 기본 로컬 회전을 보존하세요. 임포트된 모든 관절 회전을 무조건 (0,0,0)으로 덮어쓰지 마세요.
- Blender 원본은 Z 위 / -Y 전방입니다. FBX 내보내기에서 축을 변환했습니다.

## 부품 계층

```text
Root
├─ Head
├─ Core
│  ├─ Torso
│  ├─ Pelvis
│  └─ BACKPACK_SOCKET
├─ Arms
│  ├─ Left_shoulder → Left_elbow → Left_hand
│  └─ Right_shoulder → Right_elbow → Right_hand
├─ Legs
│  ├─ Left_thigh → Left_knee → Left_ankle
│  └─ Right_thigh → Right_knee → Right_ankle
└─ Weapon
```

`Weapon`은 기체의 무기 관리용 빈 그룹입니다. 실제 장착점은 손/팔을 따라 움직이는 `SWORD_SOCKET`, `SHIELD_SOCKET`입니다. 소스에서의 Left/Right 이름은 기존 버전 호환을 위해 유지했으므로 이름만으로 측면을 추정하지 말고 아래 X 좌표로 확인하세요.

## 연결점 위치

| 이름 | 부모 | Unity X, Y, Z (m) |
|---|---|---|
| `Root` | `WISP_v001(Clone)` | 0.0000, 0.0000, 0.0000 |
| `Arms` | `Root` | 0.0000, 0.0000, 0.0000 |
| `Left_shoulder` | `Arms` | -0.6490, 1.6240, 0.0000 |
| `Left_elbow` | `Left_shoulder` | -0.7190, 1.2140, 0.0000 |
| `Left_hand` | `Left_elbow` | -0.7190, 0.8140, 0.0150 |
| `SHIELD_SOCKET` | `Left_elbow` | -0.8990, 1.1840, 0.0800 |
| `Right_shoulder` | `Arms` | 0.6490, 1.6240, 0.0000 |
| `Right_elbow` | `Right_shoulder` | 0.7190, 1.2140, 0.0000 |
| `Right_hand` | `Right_elbow` | 0.7190, 0.8140, 0.0150 |
| `SWORD_SOCKET` | `Right_hand` | 0.7190, 0.7990, 0.1150 |
| `Core` | `Root` | 0.0000, 1.3240, 0.0000 |
| `BACKPACK_SOCKET` | `Core` | 0.0000, 1.6040, -0.3500 |
| `Head` | `Root` | 0.0000, 1.9740, 0.0250 |
| `Legs` | `Root` | 0.0000, 0.0000, 0.0000 |
| `Left_thigh` | `Legs` | -0.2500, 0.9040, 0.0000 |
| `Left_knee` | `Left_thigh` | -0.2500, 0.5260, 0.0000 |
| `Left_ankle` | `Left_knee` | -0.2500, 0.0940, 0.0000 |
| `Right_thigh` | `Legs` | 0.2500, 0.9040, 0.0000 |
| `Right_knee` | `Right_thigh` | 0.2500, 0.5260, 0.0000 |
| `Right_ankle` | `Right_knee` | 0.2500, 0.0940, 0.0000 |
| `Weapon` | `Root` | 0.0000, 0.0000, 0.0000 |

## 회전축과 움직임

축은 Unity의 Root 기준이며, 부모를 회전한 뒤에는 회전축도 부모와 함께 움직입니다. 로컬 축 방향이 다를 수 있으므로 `Transform.InverseTransformDirection`으로 Root 축을 변환하거나 기본 localRotation에 상대 회전을 합성하세요.

| 부위 | 회전 중심 | 권장 축/용도 |
|---|---|---|
| Head | 목 중심 | Y: 좌우 시선, X: 위아래 시선 |
| Core | 몸통 하단 중심 | Y: 상체 조준. Pelvis와 부스터도 함께 움직임 |
| *_shoulder | 어깨 관절 | X: 전후 팔 스윙, Z: 옆으로 들기 |
| *_elbow | 팔꿈치 | X: 굽히기 |
| *_hand | 손목 | Z: 전완 방향 비틀기, X/Y: 손목 보정 |
| *_thigh | 고관절 | X: 전후 스윙, Z: 옆으로 벌리기 |
| *_knee | 무릎 | X: 굽히기 |
| *_ankle | 발목 | X: 발끝 높낮이, Z: 좌우 기울이기 |
| BACKPACK_SOCKET | 등 부스터 장착점 | Core를 따름. 분사 효과는 별도 연결 |

관절 제한, 스킨 웨이트, 스켈레톤 애니메이션과 모션 클립은 제공하지 않습니다. 부품을 직접 회전시키는 Transform 계층입니다. 플레이용 회전 범위와 장갑 간섭은 Unity에서 조정해야 합니다.

## 칼·방패 장착

두 FBX는 각각 `Root > Weapon > 메시` 구조이며, Root는 장착 원점입니다.

1. 칼 FBX의 최상위 오브젝트를 `SWORD_SOCKET` 아래로 넣습니다.
2. 방패 FBX의 최상위 오브젝트를 `SHIELD_SOCKET` 아래로 넣습니다.
3. 두 무기 모두 로컬 위치 (0,0,0), 로컬 회전 (0,0,0), 로컬 스케일 (1,1,1)에서 시작합니다.
4. 칼은 손잡이 중앙이 원점이며 칼날은 +Z 방향으로 뻗습니다. 방패 원점은 팔 장착 위치입니다. 무기는 미리보기용 기본 자세이며 실제 공격·방어 포즈는 별도입니다.

## 재질

- `WISP_Atlas`의 Base Map/Albedo에 `WISP_BaseColor.png`, Emission에 `WISP_Emission.png`를 지정하세요.
- 무광 시작값: Metallic 0.05, Smoothness 0.14. Emission HDR 배수는 4부터 조절하세요.
- PNG의 Filter Mode는 Point, Wrap Mode는 Clamp를 권장합니다. 작은 색상 아틀라스이므로 Mip Map을 끄면 인접 색 번짐을 줄일 수 있습니다.
- 발광 PNG의 검정 영역은 비발광입니다. 빨강·초록 항공 식별등, 주황 보조등, 바이저와 부스터 영역이 발광합니다. 점멸 애니메이션과 실제 광원은 포함하지 않습니다. 빛 번짐은 카메라 HDR과 Bloom으로 설정합니다.
- FBX의 자동 재질 변환에서 Emission이 누락되면 PNG를 직접 지정하세요.

## 다른 기체

BASTION 궤도 및 TARANTULA/HOUND 각 다리의 메시·피벗은 아직 존재하지 않습니다. 제작 시 움직이는 부품별 계층과 연결점을 별도로 기록해야 합니다. 이번 전달물을 네 기체 모두 완성된 것으로 취급하지 마세요.

## 적용 범위와 남은 작업

외형 FBX와 텍스처, 수정용 원본만 제공합니다. 게임 프리팹 연결, 이동·공격 애니메이션, 충돌체, LOD와 런타임 성능 검증은 포함하지 않습니다. 부품 오브젝트가 많으므로 다수 기체 표시 전 병합 최적화가 필요합니다. 기존 .meta와 GUID를 덮어쓰지 않는 신규 파일 전달입니다.

## 실제 Unity 가져오기 검증

Unity 6000.6.7f1의 별도 검증 프로젝트에서 세 FBX를 실제 가져왔습니다. 기체 크기는 X 2.1614 × Y 2.8016 × Z 1.3430m, 최저 Y는 0.0000이며 바이저가 +Z 방향에 위치합니다. Root와 관절 기본 회전은 (0,0,0)입니다. 위 연결점 표는 Unity에서 측정한 좌표입니다. BaseColor PNG 자동 연결도 확인했습니다. 검사 로그: `WISP-UnityImportReport.txt`. 게임 플레이 검증은 수행하지 않았습니다.

함께 전달한 신규 `.meta`에 Preserve Hierarchy 활성화 / Bake Axis Conversion 비활성화와 PNG의 Point / Clamp / 무압축 / Mipmap 끄기 설정을 보관했습니다. 처음 복사할 때 이 `.meta`도 함께 가져오세요. 기존 파일의 GUID를 덮어쓰지 마세요.

## 게임 연결 결과 (2026-11-03)

`Builds/WorkLatest` Unity 프로젝트에서 세 FBX 계층을 확인하고 WISP 플레이어/AI 모델로 연결했습니다. 기체의 `Head`, `Core`, `Arms`, `Legs`를 기존 `Modular robot` 바로 아래 별도 Transform으로 분리했으며, 팔·다리 피벗의 기본 변환과 `WISP_v001.fbx.meta`의 GUID를 유지했습니다. 기존 이동 애니메이션은 제공된 어깨/허벅지/무릎/발목 Transform을 사용합니다. FBX의 발바닥 원점이 y=0이므로 WISP 모델의 기존 +0.4m 절차형 오프셋만 제거해 지면에 놓았습니다.

- `WISP_BaseColor.png`와 `WISP_Emission.png`를 Standard 머티리얼의 기본색/발광 맵에 연결했습니다. 발광은 식별등·바이저 등 FBX UV 위치에 따라 표시됩니다.
- `WISP_Shield_v001.fbx`는 왼쪽 팔의 `SHIELD_SOCKET`에 장착했습니다. 이는 시각 부품이며 피해/실드 규칙은 바꾸지 않습니다.
- `WISP_Sword_v001.fbx`는 오른손 `SWORD_SOCKET`에 기본 비활성 상태로 연결하고, WISP의 기존 Saber Rush 특수 공격 중 표시하도록 했습니다. 돌진/피해 로직은 유지했습니다.
- 공통 30발 총은 오른손 위치에 남기고, 칼 돌진 중에만 표시를 숨깁니다. 다른 세 프레임과 기존 전투 입력은 바꾸지 않았습니다.
- Unity 빌드 로그: `Logs/Build-Wisp-FBX.log` (`VOXSKY_BUILD_PASS`가 최종 로그에 기록됨). Windows 플레이어 스모크: `Logs/Smoke-Wisp-FBX.log` (`VOXSKY_SMOKE_PASS 232 checks`). 시각 비교: `Docs/VisualReferences/Implemented/` (기체별 4방향, 16장).
- 실제 게임에서 WISP의 질주·피격·계단/장애물 통과를 수동으로 조작하는 테스트는 별도로 하지 않았습니다. 모델에 물리 콜라이더나 새 애니메이션 클립은 추가되지 않았습니다.
