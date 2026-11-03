# 2026-11-04 Blender 환경 FBX 검증

첫 야간 체크포인트. 실제 Windows 플레이어에서 렌더했습니다. 기본 소스와 원본 모델은 보존했습니다. 품질 개선 진행 중이며 최종 컨셉 완성본은 아닙니다.

| 실내 Foundry | 실외 Canyon |
|---|---|
| ![Foundry](Foundry-overview.png) | ![Canyon](Canyon-overview.png) |
| ![Cover](Foundry-cover.png) | ![Rock](Canyon-cover.png) |

- Unity 빌드 성공, 전체 스모크268/268, Foundry36/36+Canyon15/15 환경 검사.
- FBX 축/단위 변환 오류를 수정한 뒤 촬영했습니다. 장애물의 바닥·상단과 충돌 높이를 비교했습니다.
- [실내 WISP 실제 게임](../../Models/Foundry-WISP-crosshair-camera.png), [실외 WISP 실제 게임](../../Models/Canyon-WISP-crosshair-camera.png), [WISP v004 Blender 렌더](../../Models/WISP-v004-three-quarter.png).
- 수동 장시간 플레이와 아트 최종 승인은 남아 있습니다. main 미반영.

## 두 번째 비교

현재 이미지는 Blender 환경 v002입니다. [이전 실내](Before-v002/Foundry-cover.png)와 비교해 문/프레임/환기구 구조를 구분했고, [이전 실외](Before-v002/Canyon-overview.png)의 반복 바닥 무늬를 완화했습니다. 패널의 겹친 면도 수정하고 다시 빌드·검증했습니다. 최신 빌드268/268, 환경51/51 통과. [WISP v004 모델](../../Models/WISP-v004-Handoff.md).

## 01시 배경·동작 비교

Blender 환경 v003은 실내 구조 보강과 외곽 협곡 실루엣을 다듬었습니다. [v002 실내](Before-v003/Foundry-overview.png), [v002 협곡](Before-v003/Canyon-overview.png)과 현재 위 이미지를 비교할 수 있습니다. 기본 장애물과 이동 충돌은 유지했습니다. 바깥 경계의 단조로운 면은 다음 개선 대상으로 남아 있습니다.

| WISP 실제 동작 | 이미지 |
|---|---|
| 정지 / 좌우 옆걸음 | [정지](Foundry-motion-stand.png) · [왼쪽](Foundry-motion-strafe-left.png) · [오른쪽](Foundry-motion-strafe-right.png) |
| 전방 달리기 | [달리기](Foundry-motion-run.png) |
| 공중 Space 유지 / 해제 | [유지: 분사](Foundry-motion-jump-held.png) · [해제: 분사 없음](Foundry-motion-jump-released.png) |
| 실외 비교 | [정지](Canyon-motion-stand.png) · [옆걸음](Canyon-motion-strafe-left.png) · [점프](Canyon-motion-jump-held.png) |

이 이미지는 실행 파일의 Move/Animate를 실제로 진행한 프레임을 렌더한 자료입니다. 수동 플레이 감각 및 최종 애니메이션 승인은 별도로 남아 있습니다. [실제 검사 로그 발췌](../../Validation/20261104-Checkpoint3.md).

## 02시 협곡 경계 개선

기존 경계의 어두운 평판과 내부로 튀어나온 장식 암석을 Blender 암벽 FBX로 교체했습니다. [이전 협곡](Before-v004/Canyon-overview.png)과 현재 위 이미지를 비교할 수 있습니다. 조종 가능한 경계는 동일하며, 카메라는 네 방향 벽을 통과하지 않는지 검사했습니다. 새 파일은 ArtSource/Environment/VoxelOn_Environment_v004.blend와 Assets/Resources/Environment/CanyonBoundary.fbx입니다.

최신 Windows 빌드 성공, 전체268개+환경59개 통과. [실제 로그 발췌](../../Validation/20261104-Checkpoint4.md). 최종 아트·수동 플레이 검토는 남아 있으며 main 미반영입니다.

## 03시 공장 정비 패널과 실제 크기 측정

공장 벽을 Blender에서 정비 베이·환기구·절제된 조명으로 다듬었습니다. [이전 실내](Before-v005/Foundry-overview.png), [벽 근처 실제 화면](Foundry-clearance-wall.png), [실외 벽 근처](Canyon-clearance-wall.png)를 비교할 수 있습니다. 원본은 ArtSource/Environment/VoxelOn_Environment_v005.blend에 있습니다.

실제 스킨100760개 정점으로 측정한 WISP 높이는4.038m이며 독립 관절 계산과0.000001m 이내로 일치했습니다. 몸통은 기존 충돌 반경 안에 들어갑니다. 팔·장식의 최대 폭은 더 넓어서 벽에 일부 겹칠 수 있으며 해결됐다고 보고하지 않습니다. 비례나 피격 범위는 바꾸지 않았습니다. 환경/시각 검사61개 통과; [검증 증거와 한계](../../Validation/20261104-Checkpoint5.md). 최종 아트 승인은 남아 있습니다.

## 04시 벽의 판 겹침 수정과 실내 조명

같은 카메라로 [기준](Foundry-lighting-01-baseline.png) · [기존 구조물 그림자 제외](Foundry-lighting-02-no-legacy-shadows.png) · [배경 그림자 수신 제외](Foundry-lighting-03-no-backdrop-receive.png)를 비교했습니다. 이 세 진단 이미지는 수정 전 v005 기준이며 비교 후 설정을 복원했습니다.

원인은 Blender 수정 과정에서 대각 지지대를 큰 직육면체 판으로 재구성한 것이었습니다. v006은 원래 정점·면을 보존해 얇은 대각 지지대를 복원했습니다. 현재 위 실내 이미지에는 큰 체크형 판이 사라지고 지지대 구조가 보입니다. 기존 지붕의 그림자가 실내 조명을 모두 가리던 설정도 고쳐 바닥의 구조물 그림자가 나타납니다.

실내 실제 검사38개 통과. 실외는 변경하지 않았습니다. [검증 기록](../../Validation/20261104-Checkpoint6.md), [현재 Blender 환경 원본 안내](../../../ArtSource/Environment/README.md).
