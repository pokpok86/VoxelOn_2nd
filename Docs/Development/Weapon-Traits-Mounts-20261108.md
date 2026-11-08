# 무기 기반 락온과 어깨 연결부 — 2026-11-08

이번 완료 범위:
- BalanceWeapon.traitIconId와 lockStrength/lockPenalty를 런타임 WeaponProfile에 전달한다. 선택 무기가 LOCK이면 기체 종류와 무관하게 기존 우클릭 락온 획득/가림/거리 해제 로직을 사용한다.
- 기체 supportsLockOn은 기존 비락온 이동 방향 성향 유지에 사용하며 무기 락온 허용 판정에서 제외한다. 일반 이동·기체 수치를 재조정하지 않았다.
- 유도 지속 비율 lockStrength와 락온 이동 페널티 lockPenalty를 편집 가능/실제 적용 항목으로 변경했다. 유도 지속 시간은 수명(range/speed) × 입력한 비율. 0은 직진, 락온 조준 자체는 가능. 다른 특성을 고르면 LOCK 기능을 상속하지 않는다.
- LOCK 선택 중 우클릭은 락온을 우선해 WISP 가드와 동시 발동하지 않는다. HUD 조작 안내도 선택 무기 기준으로 표시한다.
- 어깨 무장에 Dedicated shoulder weapon connector 시각 파츠를 추가했다. 기존 회전축에서 바깥 .38, 위 .48, 뒤 .24 오프셋을 사용한다. 연결부는 어깨와 무장 사이를 잇고, 무장 조준 시 팔 관절을 강제로 회전시키지 않는다. 빈 슬롯에는 연결부를 만들지 않는다.

미완료/PD 규칙 확인:
- DAMAGE/RAPID/RELOAD/AREA의 공격력·발사 간격·장전·폭발 반경 수치는 이미 실제 발사에 사용한다. 아이콘 선택으로 임의 배율이나 자동 밸런스 변경을 추가하지 않았다. AREA의 폭발 반경이 0이면 폭발하지 않는다.
- RADAR/JUMP/GUARD 아이콘에 부여할 추가 능력과 발동 조건은 아직 미정이다. JUMP/GUARD에 대한 질문을 보냈으며 이 세 항목까지 모두 구현됐다고 보고하면 안 된다. 파츠 특성 아이콘도 별도 능력으로 구현하지 않았다.

검증:
- Unity 6000.6.7f1 Windows 빌드 성공, exit 0. Logs/Weapon-Traits-Mounts-Final-Build-20261108.log: VOXSKY_BUILD_PASS 237406874 bytes.
- 실제 실행본 --automatic-balance-study: Logs/WeaponTraitsReview-20261108/Traits-Mounts-Final.log: AUTOMATIC_BALANCE_PASS 18 checks, exit 0. WISP/BASTION/TARANTULA 무기 기반 락온, 실제 유도탄 생성, 비LOCK 무기 배제, 어깨 연결부 생성/팔 관절 독립 조준 및 기존 저장 보존 검증을 수행했다.
- 임시 데이터만 사용했다. 현재 PD-Balance.json은 사용자가 편집한 변경이 있으며 이번 개발/검증에서는 수정하지 않았다. 기존 미커밋 변경 보존. Arena.unity 원본 바이트 빌드 전후 보존, git diff --check 통과. 커밋/푸시 없음.
