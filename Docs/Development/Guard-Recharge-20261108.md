# 방패 가드 / 에너지 실드 회복 조정 — 2026-11-08

PD 결정: 무기 특성에서 JUMP 제거. GUARD는 방패 분류이며 방패 슬롯은 숫자키/HUD 무기 선택에서 제외한다. 방패 장착 동안 우클릭은 가드, 수동 락온은 비활성화한다. 정면 피해를 방어율만큼 감소한다.

구현:
- SHIELD 특성을 선택하면 손 장착·양손 허용으로 설정한다. 기존 WISP 전용 제약을 제거하고 special 종류의 Future_Shield도 특성으로 장착 가능하게 했다. JUMP는 무기 선택 목록에서 제거하고 이전 무기 JUMP 태그는 DAMAGE로 읽는다. 파츠 JUMP는 유지한다.
- GUARD 장비를 런타임 방패로 분류하고 1~4 선택/HUD 목록에서 제외한다. 남은 무기 번호는 앞에서부터 배정한다. 방패가 하나라도 있으면 수동 락온을 차단하며 우클릭 가드를 사용한다.
- 정면 수평 반공간에서 들어오는 피해는 가드 방어율만큼 줄인 후 기존 장갑/에너지 실드/HP 처리를 따른다. 뒤쪽과 가드 해제 상태는 감소하지 않는다. 새 방어율 기본 .5, 0~1 편집 가능. 방패가 여러 개면 가장 높은 방어율을 사용하며 중첩하지 않는다. 방패 편집 화면은 방어율만 표시한다.
- 방패 외형을 선택한 장비 모델로 손에 표시한다. 기존 WISP 오른손 전용 모델은 해당 장비일 때만 표시해 중복을 피한다.
- 에너지 실드 회복은 완전 회복 6초에서 30초로 변경: 초당 회복량 1/5. 피격 후 3초 대기는 유지한다. CraftSpec.shieldRecoverySeconds 및 CORE 파츠의 '실드 완전 회복 시간'으로 편집 가능하다(1~180초). 사용자가 후속 편집한 값은 저장/재로드로 유지한다.
- 기존 데이터에 없는 새 guardReduction/shieldRecoverySeconds 항목만 추가해 읽는다. 기존 수치, 분류, 설계 및 사용자 원본 파일을 개발 과정에서 대체하지 않았다.

실제 검증:
- Logs/Guard-Recharge-Final-Build-20261108.log: Unity 6000.6.7f1, VOXSKY_BUILD_PASS 237415022 bytes, exit 0.
- Logs/WeaponGuardReview-20261108/Guard-Recharge-Final.log: 실제 실행본 --automatic-balance-study, AUTOMATIC_BALANCE_PASS 32 checks, exit 0. 이전 저장/락온/연결부 검사와 함께 GUARD 분류, 선택 제외, 락온 차단, 정면/후면/가드 해제 피해, 실제 Hit 피해 감소, 다른 기체의 실제 방패 렌더러, 중복 native 방패 숨김, 기존 파일 확장, 30초 회복 수식을 검사했다.
- 중간 Guard-Final.log에서 Future_Shield의 catalog kind가 special이어서 장착이 거부되는 실패를 발견했다. kind 대신 GUARD 특성을 허용하도록 수정 후 최종 재검증했다. 실패 로그 보존.
- 테스트는 임시 balanceDirectoryOverride만 사용했다. PD-Balance.json 빌드/검증 전후 해시 동일. Arena.unity 원본 바이트 보존. git diff --check 통과. 자동 커밋/푸시 없음.
- 실제 장시간 가상대전 밸런스 분석은 이번에 실행하지 않았다. RADAR 추가 능력은 아직 미정이며 이 작업에서 완료하지 않았다.
