# 3기체 실제 사망·반복 재시작·렌더 복원

WISP만 검사했던 범위를 궤도/사족의 서로 다른 리그로 확장했습니다. 기체마다 실제 치명타 → 머리/양팔/하체 분리 → .7초 낙하 → ResetRound를 두 번 실행했습니다. 게임 전투 규칙이나 모델은 바꾸지 않았습니다.

첫 probe에서 CPU 정점/상태24검사는 통과했지만 즉시 Camera.Render한 복원 그림이 이전 분리 포즈로 보였습니다. [첫 probe 보존](FirstProbe/)은 시각 통과 자료가 아닙니다. 렌더러가 스킨을 갱신하는 정상 프레임까지 기다리는 촬영 fixture로 수정했고, **실제 복원 이미지 실루엣 검사3개를 추가**했습니다. CPU 수치만 보고 시각 성공으로 처리하지 않았습니다. 정상 프레임 뒤에는 3기체 모두 완전히 복원되어 영구적인 리그 손상은 재현되지 않았습니다.

| 기체 | 정상 | 실제 분리 | 실제 복원 | 스킨 정점 수 / 복원 오차 | 렌더 실루엣 IoU |
| --- | --- | --- | --- | --- | --- |
| WISP | [이전](Iteration02/WISP-before.png) | [분리](Iteration02/WISP-defeated.png) | [복원](Iteration02/WISP-restored.png) | 9519 / 0m | 1.00000 |
| BASTION | [이전](Iteration02/BASTION-before.png) | [분리](Iteration02/BASTION-defeated.png) | [복원](Iteration02/BASTION-restored.png) | 17360 / 0m | 1.00000 |
| TARANTULA | [이전](Iteration02/TARANTULA-before.png) | [분리](Iteration02/TARANTULA-defeated.png) | [복원](Iteration02/TARANTULA-restored.png) | 10208 / 0m | 1.00000 |

**27 PASS / 실제 화면9장.** 스킨 정점 오차3mm 미만, 실루엣 IoU>.97 기준을 사용했고 실제0m/1.0을 확인했습니다. 정상/복원은 같은3.6카메라, 분리는 흩어진 부품 확인용4.8카메라입니다. 이는 고정 조명·정적 복원 장면의 검증이며 다른 모션/모든 플레이 상황의 시각 승인이나 GPU 성능 측정이 아닙니다.

실행: `Voxel-On.exe --fx-capture --module-roster` (프로젝트 루트를 작업 디렉터리로 실행).

[원본 로그](../../Validation/Overnight-20261105/Module-Recovery-RenderSync-Actual.txt), [빌드](../../Validation/Overnight-20261105/Module-Recovery-RenderSync-Build.txt), [최종 전체357회귀](../../Validation/Overnight-20261105/Module-Recovery-Final-Smoke.txt). 마지막 Windows 빌드204682942bytes. 첫 probe의 원본 로그도 같은 검증 폴더에 보존했습니다.
