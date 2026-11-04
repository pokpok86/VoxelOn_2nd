# 효과 / HUD 반복 02 — 실제 비교

| 항목 | 이전 | 이번 확인 |
| --- | --- | --- |
| HUD 오른쪽 아래 | 구형 조작·방패 텍스트가 4슬롯 패널 아래 겹침 | 구형 중복 제거, 중앙 도움말/가드 표시, 슬롯별 실제 쿨타임 표시 |
| 공중 분사 | 작은 단면/몸 중심에 숨음 | Blender 새 BoostCone_v002 FBX, 몸 아래 배치; 실제 화면에서 청록 분사 확인 |
| 부위 연기 | 고정55프레임 촬영으로 .55초 발생 전 저장 가능 | 발생을 실제 시간으로 기다리고 활성 메시/크기 로그 추가. 아직 일반 시점에서 선명하지 않아 시각 완료 아님 |

[이전 연기/하단 UI](../PD14-Effects/01-arm-damage-smoke.png) / [현재 동일 시점](01-arm-damage-smoke.png)

[이전 공중 분사](../PD14-Effects/04-air-held-space-exhaust.png) / [현재 동일 시점](04-air-held-space-exhaust.png)

- 실제 최신 Windows 빌드 `Logs/FX-HUD-Iteration02-Final-Build.log`: 204325770 bytes, exit0.
- `Logs/FX-HUD-Iteration02-Final-smoke-test.log`: 335개 회귀 통과. 지연 후 실제 풀 연기 메시 생성 검사 추가.
- `Logs/FX-HUD-Iteration02-Final-fx-capture.log`: 6장 생성, 손상/균열/엄폐물파괴/공중분사/기체분리/복원 상태 검사 통과.
- 실제 이미지 직접 확인: HUD 중복은 사라짐, 분사 가시성 개선. 연기는 계속 가려지거나 대비가 부족함. 크기0.654×0.650×0.632, 화면중심(0.464,0.409), 월드중심(-1.244,1.671,0.511). 메시 활성만으로 시각 통과 처리하지 않는다.
- 근접 진단 01b는 PAUSED 패널에 가려져 비교 자료로 사용할 수 없다. 다음에 `view.Render`/RenderTexture로 UI 없이 직접 촬영하고 외장과 배출 위치를 대조할 것. 플레이 카메라 각도는 변경하지 않았다.
- 원본 자산/이전 이미지 및 `Backups/FX-HUD-20261105/Windows` 실행본 보존. 최종 시각/밸런스 수동 승인 대기.