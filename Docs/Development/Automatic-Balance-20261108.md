# 커스터마이징 즉시 적용 / PD 데이터 보존 — 2026-11-08

- 커스터마이징은 진입한 기체의 기존 설계를 선택한다. 내부 기체 전환, 설계 슬롯 전환/복사/초기화/비우기 UI를 제거했다.
- 장비 편집의 좌/우 허용 토글을 제거하고 손 전용 / 어깨 전용으로 통합했다. 최초 일반 실행에서 기존 장비를 손 전용으로 한 번 이관한다. 기존 어깨 슬롯 중 허용되지 않는 무기는 미장착 처리한다. UNARMED는 양쪽 빈 슬롯에 사용한다. 기존 WISP 방패의 오른손 가드 규칙은 유지한다.
- 이관 직전 PD-Balance.json.before-hand-only.bak을 별도로 남긴다. handMountDefaultsApplied를 저장해 이후 PD가 지정한 어깨 전용 분류를 다시 초기화하지 않는다. 수치와 다른 설계는 보존한다. 신규 사용자 화기도 손 전용으로 시작한다.
- 파일/실험 프리셋 탭과 수동 저장/불러오기/전투 적용 버튼을 제거했다. 편집된 유효 값은 LateUpdate에서 검증해 원자적 저장 후 런타임에 적용한다. 종료 때도 미저장 유효 변경을 저장한다. 유효하지 않은 입력이나 파일 오류는 표시하고 원본 파일을 기본값으로 대체하지 않는다.
- 일반 실행 시작 때 저장 데이터를 읽어 전투에도 적용한다. 개발자가 PD의 밸런스/분류를 임의로 재조정하지 않도록 AGENTS.md에 데이터 소유 규칙을 기록했다.

## 실제 검증

Unity 6000.6.7f1 Windows build: Logs/Automatic-Balance-Final-Build-20261108.log, VOXSKY_BUILD_PASS 237404426 bytes, exit 0.

실행본 --automatic-balance-study: Logs/AutomaticBalanceReview-20261108/AutomaticBalance-Final.log, AUTOMATIC_BALANCE_PASS 11 checks, exit 0. 임시 폴더에서 이관 전 데이터 백업, 사용자 수치/다른 설계 보존, 손 전용 초기화, 진입 기체 유지, 자동 적용/저장, 재로드 후 분류/수치 보존, 새 장비 손 전용 기본값을 검사했다. 실제 편집창 캡처에서 4개 탭, 손/어깨 전용 선택, 기체 전환/저장 버튼 제거를 확인했다.

현재 사용자 Data/Balance/PD-Balance.json은 개발/검증 과정에서 수정하지 않았다. SHA256 F2255B715D1B14125EB240B60A0C6A3B5CD51F46AF376F494F7E2B2DD279E99D. 위의 최초 이관은 사용자가 새 실행본을 일반 실행할 때 수행된다.

Arena.unity SHA256 CEEDF122D2B457A66C8CCAB65FEC3A88D992AC6B77506776254324A8AE52459A, 빌드 전후 원본 바이트 보존. git diff --check 통과. 기존 미커밋 변경을 유지했고 자동 커밋/푸시는 하지 않았다.
