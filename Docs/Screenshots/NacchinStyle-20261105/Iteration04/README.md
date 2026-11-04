# 외장 반복 04 / 피격 표시

기준은 [박PD 지정 사진](../../../VisualReferences/Nacchin-20261105/Reference-5.png)이다. 반복03의 비례·색을 유지하며 플레이 중 자주 보이는 뒤쪽에 정비 기능을 읽을 수 있게 했다.

- Blender 원본/FBX 새 버전: WISP_v010(97메시,2694정점,24뼈), BASTION_v005(176/4440/114), TARANTULA_v005(106/2760/36). 이전 버전 모두 보존.
- 후면 환기 슬릿·고정 볼트·작은 주황 서비스 표식, WISP 유연한 허리 정비 호스. 추가 부품을 Torso 가중치 메시 하나로 합쳐 렌더러 증가는 기체별1개다.
- 실제 Unity 동일 각도20장 직접 확인: [03 뒤](../Iteration03/Unity/1-WISP-rear.png) → [04 뒤](Unity/1-WISP-rear.png), [04 정면](Unity/1-WISP-front.png), [3번 전신](Unity/3-TARANTULA-three-quarter-wide.png).
- Unity WISP 정점9519로15000예산 통과. 발·키·손/무릎 관절·방패 연결 회귀 검사를 유지했다. 장식의 최종 승인과 수동 움직임 평가는 대기다.
- 연기 상승을 높여 성숙 단계에서 후방 일반 시점에도 짙은 작은 연기가 드러난다. [전 반복](../../PD14-Effects-Iteration03/01-arm-damage-smoke.png) → [이번 실제 화면](../../PD14-Effects-Iteration04/01-arm-damage-smoke.png). 근접 진단도 UI 없이 렌더했다. 아직 단순한 저면수 연기이며 최종 효과 품질 승인 전이다.

검증: `Logs/Nacchin-Iteration04-CornerJumpFix-Build.log` 빌드 성공204492921bytes. 최신 회귀335개 통과. 이미지/효과는 직전 동일 모델/효과 빌드에서20장+6장 촬영했으며 이후 변경은 AI 우회 순서 수정이다. AI 모서리30개 검증과 실제 실패/수정은 [모서리 보고](../../PD14-Corner-Recovery/README.md) 참조.