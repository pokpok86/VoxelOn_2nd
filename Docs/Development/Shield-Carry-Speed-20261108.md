# 방패 휴대 자세와 이동 속도 — 2026-11-08

- 평상시 방패는 긴 축을 수직으로 유지하고 방패의 옆면이 정면을 향하도록 몸체 기준 ±90도 회전한다. 가드 블렌드가 올라가면 방패 면이 정면을 향하도록 보간한다. 좌/우 및 기존 WISP native 방패에 동일한 최종 자세 처리를 적용한다. 커스터마이징/선택창 초상화도 평상시 자세로 생성한다.
- 피해 감소 조건은 기존처럼 guarding=true일 때만 허용한다. 평상시 휴대는 방어 효과가 없다.
- 방패 장착 중 수평 이동 희망 속도에 .5를 곱한다. 평상시/가드 모두 동일하며 기존 2%/6% 감속과 중첩하지 않는다. 방패를 제거하면 배율1로 복원한다. ShieldMovementMultiplier에 분리했으며 무게 기반 감속 공식은 아직 적용하지 않았다. 무기 weight 데이터도 변경하지 않았다.
- Unity 6000.6.7f1 빌드: Logs/Shield-Carry-Speed-Final-Build-20261108.log, VOXSKY_BUILD_PASS 237416270 bytes, exit0.
- 실제 실행본 --automatic-balance-study: Logs/WeaponGuardReview-20261108/Shield-Carry-Speed-Final.log, AUTOMATIC_BALANCE_PASS 38 checks, exit0. 추가 검사는 idle 방패 수직/옆면 자세, 가드 정면 자세, 평상시 무방어, 가드 여부와 무관한 .5 배율, 제거 시1 복원을 확인했다. 실제 화면 캡처에서 세로 휴대 자세도 확인했다. 이동 거리의 수동 플레이 비교 검사는 별도로 실행하지 않았다.
- PD-Balance.json 개발/검증 전후 해시 동일. Arena.unity 빌드 전후 원본 바이트 보존. git diff --check 통과. 기존 미커밋 변경 보존, 자동 커밋/푸시 없음.
