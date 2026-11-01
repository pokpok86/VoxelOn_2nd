# VoxelOn local Codex task report: TASK-20261101-LEG-LANDING-STAGGER-010

- Status: **completed**
- Mode: check
- Priority: 35
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T14:26:07.2659782Z
- Finished: 2026-11-01T14:28:02.6131634Z

## Summary

TASK-010 읽기 전용 검토를 완료했습니다. **설계 초안만 제공하며, 최신 후보와 2배 점프 작업 확인 전 구현 판단은 보류합니다.**

- 변경 파일: 없음.
- 실행한 확인: 필수 문서 읽기, 작업트리 내 보고서 검색, 점프·부스트·착지·AI·기존 Smoke assertion 정적 검토.
- 차단 사항: `git`·`rg`가 PATH에 없고, `codex/task-reports`와 TASK-009 보고서를 찾지 못했습니다. 작업트리 밖 Git 메타데이터에는 접근하지 않아 **후보 브랜치 SHA와 현재 소속 브랜치를 확인하지 못했습니다.**
- Unity 빌드·플레이테스트·Smoke 실행은 하지 않았습니다. 읽기 전용이므로 보고서 파일도 게시하지 않았습니다.

**확인한 현재 소스**

| 근거 경로 | 확인 내용 |
|---|---|
| `Assets/Scripts/CraftCatalog.cs` | 다리 관련 이동 배율, `fallGravityFactor`, `knockdownResistance`, `recoverySpeed`, `stepHeight` 존재. 독립 다리 장착 시스템은 미래 작업 |
| `Assets/Scripts/VoxArena.cs:427` | 수평·수직 부스트가 연료 공유. 고갈 시 잠금, 두 입력을 놓으면 재무장 |
| `Assets/Scripts/VoxArena.cs:479` | 이동 후 지지면 기준 `jumpHeight` 계산 |
| `Assets/Scripts/VoxArena.World.cs:184` | `LandingHeight`가 지면과 위에서 접근한 지붕을 지지면으로 선택 |
| `Assets/Scripts/VoxArena.World.cs:259` | 하강 중 지지면 도달 시 위치 보정 후 수직속도를 0으로 만듦 |
| `Assets/Scripts/VoxArena.Smoke.cs:298` | 기존 점프 높이·연료 공유·낙하 검사 존재 |
| `Assets/Scripts/VoxArena.AI.cs:122` | 봇도 점프·부스트 상태 사용 |

현재 선언된 점프 상한은 3.2입니다. 이것이 최신 TASK-009 결과라는 증거는 없습니다.

**PM 설계 초안 — 아래 수치는 모두 플레이테스트가 필요한 초기 제안입니다.**

다리 속성 `landingShockAbsorption` 하나를 추가하고 기존 8개 프리셋과 분리된 파츠 루트를 유지합니다. 방어력·무게로 값을 자동 추론하지 않습니다.

착지 판정은 공중→지지면 접촉 전이에서 한 번만 발생시킵니다. 충돌 코드가 속도를 0으로 만들기 **직전**의 하강속도와 실제 접촉면 높이를 보존해야 합니다. 낙하량은 공중 구간 최고점에서 실제 착지면까지의 차이로 계산하며 절대 월드 높이를 사용하지 않습니다.

TASK-009 검증 후, 일반 2배 점프의 최대 낙하량·착지속도를 각각 `H`, `V`로 측정합니다. 초기 위험 문턱은 `D > 1.25H` **그리고** `impactSpeed > 1.15V`를 제안합니다. 낮은 장애물과 정상 점프는 안전하며, 실제 하강 제어로 충돌속도를 낮춘 경우에도 안전해야 합니다. 현 코드의 추력이 가능한 고도 조건은 제한되어 있으므로 고고도 하강 제어 가능 여부는 별도 확인이 필요합니다.

| 예시 다리 유형 | 충격 흡수율 | 같은 최대 위험 착지의 회복시간 |
|---|---:|---:|
| 속도형 | 0% | 0.24초 |
| 균형형 | 25% | 0.18초 |
| 중량형 | 50% | 0.12초 |

문턱 초과량으로 기본 회복시간을 0.08~0.24초 사이에서 계산한 뒤 흡수율을 적용합니다. 중량형이 더 안전하다는 방향과 프리셋별 배정은 생산자 승인 사항입니다.

- 회복 중 걷기는 50%, 부스트·대시·재점프는 잠시 제한합니다. 조준·사격·재장전은 유지하고 무적은 추가하지 않는 안입니다.
- 예상 위험 착지 표시, 접촉음, 짧은 무릎 굽힘으로 알립니다. 회복 종료 즉시 정상 이동하며 카메라·조준을 흔들지 않습니다.
- 연료 고갈 자체로 경직시키거나 연료를 추가 차감하지 않습니다. 낙하 피해도 추가하지 않습니다.
- 같은 공중 구간의 충돌 서브스텝·미세 튕김은 중복 발동하지 않습니다. 초기 제안으로 안정 접지 0.1초 후 새 공중 구간을 허용하고, 회복 종료 뒤 0.3초 재발동 유예를 둡니다.
- 기존 다운과 회복시간을 합산하지 않습니다. 리스폰·순간이동 시 낙하 기록을 초기화합니다.
- 봇에도 동일 판정을 적용합니다. 온라인 구현은 추가하지 않고, 향후 권위 시뮬레이션이 착지 이벤트·회복 종료 틱을 결정하도록 설계합니다.

**작게 나눌 후속 작업과 회귀검사**

1. 최신 후보 SHA와 TASK-009 보고서를 확보하고 8개 프레임의 탭·홀드·연료 부족 점프 `H/V`를 측정합니다. 기존 assertion은 유지하며 충돌하는 기대값은 근거를 검토합니다.
2. 착지 이벤트와 충돌 직전 속도 기록만 추가합니다. 지면·지붕·모서리·계단·벽·천장·리스폰에서 이벤트 중복과 오탐을 검사합니다.
3. 승인 후 다리 속성과 회복 상태를 추가합니다. 정상 2배 점프·낮은 단차는 0회, 높은 낙하는 1회, 같은 충격에서 유형별 회복시간 차이와 상한을 검사합니다.
4. 부스트로 속도를 낮춘 착지, 연료 고갈만 발생한 상황, 반복 튕김·연속 낙하, 다운 중 착지, 봇 동일 동작을 검사합니다. 이후 Unity에서 지붕 충돌과 애니메이션 가독성을 확인합니다.

미결정 사항은 유형별 배정, 위험 문턱, 이동 제한 강도, 재발동 유예, 특수 돌진 중 착지 처리입니다. 최신 의존성 확인과 생산자 승인 전에는 게임플레이 구현을 진행하지 않는 상태입니다.

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.