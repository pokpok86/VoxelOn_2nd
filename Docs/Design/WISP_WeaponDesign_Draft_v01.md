# VoxelOn_2nd — WISP 무기 공동 설계서 (초안)

- 상태: draft / PD 검토 대기
- 목적: PD와 맥스가 무기 성능, 고유 발사 방식, 특수 기능, 장착 제약을 직접 설계·승인한 뒤 개발자에게 구현 인계
- 범위: 게임 디자인 사양 문서. 현재 Unity 런타임 무기 수치 및 기존 밸런스 JSON을 수정하지 않음.

## 데이터 소스와 원칙
- 기존 장비 ID와 현행 수치는 `Docs/Balance/Weapon-List.md` 및 `Docs/Balance/PD-Balance-Template.json` 참조.
- 외형 라이브러리와 무기 접속점은 `Docs/Models/Weapon-Library-20261105.md` 참고.
- 새로운 WISP 하박 일체형 무기 W-01~W-05는 기존 `Military_*`, `Future_*` ID와 혼동하지 말고 추후 매핑을 확정한다.
- 기본 성능(데미지, 발사 간격, 탄창, 사거리, 탄속, 반동, 무게), 무기 고유 특성(연사/버스트 등), 부착 특수 기능(아이콘 부여), 장착 조건을 구분한다.
- `락온 기능`은 락온 사용 가능 여부를 부여한다. `락온 향상`·`락온 안정화`·`조준 보정 강화`는 별도 옵션으로 구분한다.
- `부스터 강화` 등 공통 옵션은 머리/몸통/백팩 등 다양한 부위에 붙을 수 있다.
- 드론 3종은 무기/서브웨폰이 아닌 전용 백팩 고유 기능이며, 해당 백팩의 서브웨폰 슬롯은 0개. 공격 드론은 적에게 가서 공격, 요격 드론은 주위에서 호밍 미사일 요격, 교란 드론은 근처 적의 락온 성능 저하.

## 승인된 기능 목록 v0.4 (검토 후보 포함)
- 공격: 차지 샷, 관통, 확산, 유도, 폭발, 지속 피해, 드레인, 락온 기능
- 방어: 실드
- 기동: 부스터 강화, 점프, 체공, 낙하 완충, 고속 이동
- 지원: 락온 향상, 락온 안정화, 조준 보정 강화
- 전자전: 재밍, 유도 방해, 락온 교란
- 드론 백팩 전용: 공격 드론, 요격 드론, 교란 드론
- 삭제/제외: 연사 강화, 버스트(무기 고유 발사 특성으로 분류), 실드 외 방어 기능, 부스터 효율, 쿨타임 단축, 대시, 점프 강화, 자동 조준, 락온 해제 조건 완화(안정화에 통합), 정찰, 표식, 스텔스.

## 첫 번째 디자인 후보 — 아직 PD 미승인
| ID | 명칭 | 무기 형태 | 고유 전투 특성 제안 | 특수 기능 제안 | 상태 |
|---|---|---|---|---|---|
| W-01 | LUMEN | 팔뚝 일체형 레이저 | 정밀 단발 사격 | 락온 기능 | proposal |
| W-02 | BREAKER | 팔뚝 일체형 대형 포 | 묵직한 단발 | 폭발 | proposal |
| W-03 | RATTLE | 팔뚝 일체형 다연장 무장 | 연속 사격 | 관통 | proposal |
| W-04 | EDGE | 팔뚝 일체형 블레이드 | 근접 타격 | 드레인 | proposal |
| W-05 | AEGIS | 팔뚝 일체형 방어 장비 | 방어 위주 | 실드와의 관계 검토 필요 | proposal |

### 각 무기별 필수 작성 필드
- weaponId, displayName, weaponClass, modelRef, mountType, handRequirement
- damage, attackInterval, range, projectileSpeed, capacity, reload, recoil, weight
- inherentBehavior (무기 고유 특성)
- specialFunctions[] (기능 ID + 효과 수치 / 적용 조건)
- lockOnEnabled, aimAssistBase
- equipRestrictions (다리/몸통/백팩 무게 및 양팔 장착 조건)
- visualRequirements (실루엣, 하박 결합, 가동 관절, 총구 및 이펙트)
- reviewStatus: proposal → approved → implementation_ready → verified

## 인계 규칙
1. PD가 각 무기의 특수 기능과 수치를 검토한 뒤 approved 처리한다.
2. 확정 무기만 런타임 밸런스/카탈로그 ID에 매핑한다.
3. 구현 담당은 변경 전 원본 보존, Unity 인게임 캡처, 동작 테스트, 결과 보고를 수행한다.
4. 이 문서가 존재하는 것만으로 자동 실행 작업이 생성되거나 승인된 것으로 간주하지 않는다.
