# VoxelOn local Codex task report: TASK-20261101-WEIGHT-HOLD-BOOST-JUMP-011

- Status: **completed**
- Mode: check
- Priority: 8
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T14:36:29.8743998Z
- Finished: 2026-11-01T14:38:46.5104266Z

## Summary

판정: **구현 대기—최신 소스 고정 확인이 하드 블로커입니다.** 변경 파일은 없습니다. 읽기 전용·외부 통신 금지에 따라 보고서 게시와 작업 큐 등록도 하지 않았습니다.

현재 환경에서 `git`, `rg`, `Unity` 명령을 찾지 못했습니다. `.git`은 작업트리 밖 Git 메타데이터를 가리키므로 접근하지 않았습니다. 따라서 후보 `codex/implementation/2026-11-01-sd-ground`의 `b757bcc` 전체 SHA, 현재 HEAD·브랜치·변경 상태를 검증하지 못했습니다. 작업트리 내 검색에서도 **JUMP-IMPLEMENT-009와 LANDING-STAGGER-010의 상태 기록은 발견되지 않았습니다.** 아래는 최신 후보 검증 결과가 아닌 현재 파일의 정적 분석입니다.

현재 구현과 기준값:

| 항목 | 확인 결과 |
|---|---|
| 입력 | `Assets/Scripts/VoxArena.cs:301`—Space `GetKeyDown`으로 점프, `GetKey`로 홀드 상승. AI는 `VoxArena.AI.cs:125`에서 별도 점프 시작 |
| 수직 운동 | `VoxArena.cs:11`—초속도 10.6, 중력 24, 상승 추력 34, 수직 에너지 소모 24/초. 하강 중력은 43.2 |
| 높이 제한 | `VoxArena.cs:430,448,480`, `VoxArena.World.cs:252`에 3.2 제한이 분산됨 |
| 에너지 | 수평·수직 부스트가 공유. 마지막 잔량 비례 추력과 0 하한 처리 있음. 비사용 시 0.6초 지연 후 40/초 회복하며 공중에서도 회복 가능 |
| 중량·다리 | `CraftCatalog.cs:11–22`에 실제 중량 없음. `fallGravityFactor`, `knockdownResistance`, `recoverySpeed`, 계단 높이는 존재. 충격 흡수 전용 값은 없음 |
| 조향·락온 | 공중에서도 수평 이동 경로 사용. `VoxArena.cs:303–325`에서 거리·화면·가림으로 락온 유지 판단 |
| 충돌·착지 | `VoxArena.World.cs:184–269`에서 분석적 충돌과 지붕 착지 처리. 착지 시 속도를 바로 0으로 만들어 충격 속도를 별도 보존해야 함 |
| 피드백 | `VoxArena.UI.cs:83`에 에너지 표시. `VoxArena.cs:550`의 상승 불꽃은 실제 추력 사용보다 상승 속도 기준 |
| 저장값 | `Assets/Scenes/Arena.unity:164–169`도 위 점프값 사용. `Assets/Editor/VoxBuild.cs`는 빌드 시 장면을 다시 생성 |

탭 점프의 연속식 예상 상승량은 **2.341**, 정점까지 **0.442초**입니다. 현재 적분 순서만 계산하면 시간 간격 0.01/0.02/0.05초에서 각각 **2.288/2.235/2.080**입니다. Unity 실측은 아닙니다. 현재 동작은 프레임 간격의 영향을 받습니다.

권장 설계는 다음과 같습니다.

- 접지 상태의 새 Space 누름에만 초기 속도를 한 번 부여하고, AI도 같은 시작 함수를 사용합니다.
- 점프 후 홀드·잔량 조건에서만 시간에 비례한 추력을 적용합니다. 해제는 추력만 멈추고 상승 관성은 유지합니다.
- 에너지 배분 순서를 명시하고, 고갈 후 자동 재점화를 막습니다. **공중 회복을 제한할지** 결정해야 반복 해제·재입력으로 무한 체공하는 문제를 막을 수 있습니다.
- 고정 시뮬레이션 간격에서 중량→탭 높이→초기 속도를 계산합니다. 외형 크기·장갑을 중량으로 추정하지 않습니다.
- 높은 상한은 최후 안전장치로 두고, 연료와 낙하 위험이 고도를 결정하게 합니다. 착지 직전 **지지면 대비 법선 접근 속도**를 기록하고 010의 다리별 흡수·경직 계산에 전달합니다.
- HUD에 상승 부스트·고갈·회복 상태를 표시하고, 추력 불꽃과 관성 상승 표현을 구분합니다.

승인 전 초기 밸런스 표는 수치 대신 아래 관계로 정의할 수 있습니다. `H₀`는 확인된 과거 기준 점프 높이, `Hᵣ≈2H₀`, `g`는 상승 중력입니다.

| 중량 | 일반 점프 높이 | 부스트 상승 순가속도 | 소모/초 | 예상 최대 상승량 | 착지 임계값 |
|---|---|---|---|---|---|
| 경량 | `Hᵣ×L`, `L>1` | `aL>0` | `qL` | `F(HL,aL,EL/qL)` | 다리별 `Vsafe`, `Vsevere` |
| 중량 기준 | `Hᵣ` | `aM>0` | `qM` | `F(Hᵣ,aM,EM/qM)` | 같은 다리 모델 |
| 중장량 | `Hᵣ×W`, `0<W<1` | `aH>0` | `qH` | `F(HH,aH,EH/qH)` | 같은 다리 모델 |

장애물·회복·상한이 없을 때 `v=√(2gH)`, `t=E/q`라면 `F=vt+½at²+(v+at)²/(2g)`로 비교할 수 있습니다. 실제 최대 높이는 충돌과 상한을 포함해 측정해야 합니다. **2배 높이는 같은 중력에서 초기 속도 √2배**에 해당합니다. 현재 값을 과거 기준이라고 확정할 수 없으므로 10.6을 바로 증폭하면 안 됩니다.

제작자 선택이 필요한 값은 기준 기체·과거 높이, 중량과 `L/W`, 추력·소모·높은 안전 상한, 공중 회복 정책, 다리별 충격 임계값·경직 시간입니다.

최소 후속 작업 순서:

1. 러너가 최신 후보의 **전체 SHA와 일치하는 HEAD**를 보장하고, 불일치 시 중단하도록 확인합니다. 009 결과와 010 설계를 확보해 고정 높이 변경의 충돌을 조정합니다.
2. 중량 필드·공통 점프 시작·홀드 상승·착지 충격 전달을 작은 범위로 구현합니다. 조립 UI는 추가하지 않습니다.
3. 기존 테스트를 보존하면서 중량별 탭, 홀드·해제·고갈, 반복 입력, 무한 체공 방지, 지붕 이륙·착지, 다리별 고낙하 경직, 회복, 수평 부스트 공유, 락온·충돌 회귀를 추가합니다. 기존 높이 assertion과의 충돌은 승인된 사양 변경으로 명시해야 합니다.
4. 쓰기가 허용된 검증 환경에서 Unity 종료 후 Windows 빌드를 한 번 실행하고 `VOXSKY_BUILD_PASS`·오류를 확인합니다. 이후 플레이어 smoke와 동일 조건 캡처를 비교합니다.

실제 수행한 검사는 문서·소스·장면 설정 읽기, 작업 기록 검색, 도구 가용성 확인, 독립 산술 계산입니다. PowerShell 산술 일부는 제한 언어 모드로 실패해 JavaScript 계산으로 확인했습니다. **Unity 컴파일·빌드·플레이·smoke·캡처는 실행하지 않았습니다.**

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.