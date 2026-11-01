# VoxelOn local Codex task report: TASK-20261101-MOBILITY-ARCHETYPES-017

- Status: **completed**
- Mode: check
- Priority: 4
- Source commit: f808e5952fee
- Local work branch: detached/read-only
- Review branch: (not published)
- Review commit: (none)
- Validation: (not run)
- Started: 2026-11-01T15:04:20.7321914Z
- Finished: 2026-11-01T15:06:20.1536539Z

## Summary

읽기 전용 설계 검토를 완료했습니다. **BASTION 1대를 상체 독립 조준이 가능한 특수 이족 비교군으로 선정**하는 안을 권장합니다. 현재 궤도·사족 형상은 없어 탱크나 사족으로 분류할 근거가 없습니다.

확인된 소스와 현재 동작:

| 파일 | 확인 내용 |
| --- | --- |
| `Assets/Scripts/CraftCatalog.cs` | KESTREL, BASTION, WISP, NOMAD, SCATTER, LANCER, TEMPEST, ANVIL 총 8대. 독립 코어·다리 부품 ID나 이동형 분류는 없음 |
| `Assets/Scripts/VoxArena.Robot.cs` | 전 기체 공통 코어와 좌우 양다리 생성. BASTION·ANVIL은 중장갑 치수 적용 |
| `Assets/Scripts/VoxArena.FrameIdentity.cs` | 기체별 장식 추가. ANVIL의 안정판도 궤도는 아님 |
| `Assets/Scripts/VoxArena.cs` | 카메라 기준 WASD, Shift 연료 부스트, 방향키 두 번 입력 대시. 대시는 기본 연료 30, 거리 9, 재사용 0.7초이며 접지 제한 없음 |
| 같은 파일 | 비락온 시 이동 방향 회전, 락온 시 상대 방향 회전. `Animate`가 루트 전체를 돌려 상하체 독립 회전은 없음 |
| `Assets/Scripts/VoxArena.World.cs` | 분할 이동 충돌, 낮은 턱 오르기, 착지·지붕 지지 처리 |
| `Assets/Scripts/VoxArena.AI.cs`, `VoxArena.Combat.cs`, `VoxArena.Smoke.cs` | AI 이동 입력, 무기 동작, 기존 점프·부스트·충돌 회귀 검사 확인 |

문서의 부품 조합은 구상 단계입니다. 실제 형상은 모두 양다리이며, `BoostKind`는 Glide/Cruise/Pulse 동작 구분입니다.

구현안은 다음처럼 제한합니다.

- **비교 대상:** BASTION만 특수 이족으로 전환하고 KESTREL을 휴머노이드 대조군으로 사용합니다. 나머지 6대는 기존 동작을 유지합니다. 전역 프로토타입 플래그를 끄면 기존 동작으로 복귀합니다.
- **설정:** 기체별 `movementMode`, `facingMode`, 지상 대시 비용·거리·쿨다운만 추가합니다. 양다리 형상과 휴머노이드 분류는 별도로 둡니다.
- **휴머노이드:** 마우스/카메라의 수평 조준 방향과 몸체를 정렬합니다. 지상 대시는 연료 0으로, 확실한 접지·비상승 상태에서만 시작합니다. 기존 0.7초 쿨다운을 초기값으로 유지하고, 착지 전 재발동·공중 연쇄를 막습니다. 도약감은 작은 단발 상승 충격으로 만들되 지속 수직 추력은 주지 않습니다. 연료 고갈 잠금은 무료 대시의 차단 조건에서 분리합니다.
- **BASTION 특수 이족:** 하체는 이동 방향, 상체는 조준 방향을 사용합니다. 머리·코어·팔·무기 루트를 보존하며 회전값을 따로 적용합니다. 발사각 검사는 상체 조준을 기준으로 합니다.
- **향후 탱크:** A/D로 차체를 회전하고 W/S로 전후 이동합니다. 모든 고속 이동은 시작 시점 차체 전방으로 제한합니다. 두 번 입력 대시뿐 아니라 Shift·AI 경로도 같은 제한을 적용합니다.
- **향후 사족:** 일반 이동과 독립 상체 조준만 허용하고 대시·Shift 고속 이동을 차단합니다. 실제 사족 형상 제작은 별도 작업입니다.
- **Space:** 탭 점프, 홀드 연료 상승, 공중 재부스트는 별도 경로로 유지합니다. 기존 높이 제한과 연료 소모를 무료 지상 대시와 섞지 않습니다.
- **락온:** 다리별 장착 제한은 추가하지 않습니다. 추적 성능은 머리 센서와 무기 설정에 둡니다.
- **선택적 중량 제안:** `supportedMass`, `coreMass`, 코어·다리 호환 태그를 향후 조정값으로만 제안합니다. 수치 확정이나 장착 차단은 이번 프로토타입에 넣지 않습니다.

후속 작업은 ① 후보 SHA 고정 및 기존 빌드 보존, ② 두 기체 설정과 최소 이동·회전 변경, ③ 회귀 검사 추가 및 독립 빌드 검증으로 분리합니다. 구현용 고유 브랜치 예시는 `codex/prototype/TA[REDACTED]`입니다. 생성하지 않았습니다. 2회 점프·Space 부스트·타깃 방향 작업과는 `PlayerInput`, `Move`, `Animate`, 점프 원점 및 높이 제한의 소유권을 먼저 맞춰야 합니다.

필수 인수 검사는 무료 지상 대시의 연료 불변·공중 재발동 차단, Space 상승 연료 소모·재부스트, 탱크 횡/후방 고속 이동 차단, 사족 고속 이동 차단, 벽·모서리·천장·지붕 착지, 마우스 조준·상하체 회전·락온·머리/무기 추적입니다. 기존 검사는 유지하고 추가해야 합니다. 제작자 A/B 확인은 같은 맵·상대·무기로 KESTREL과 BASTION의 달리기, 횡이동 조준, 지상 대시, Space 상승을 비교합니다.

**변경 파일:** 없음.  
**실제 검사:** 도구 가용성 조회, 문서·소스 정적 검토, Unity 버전 파일 확인(6000.6.7f1).  
**차단 사항:** Git 명령을 사용할 수 없어 현재 브랜치·전체 SHA·요청 후보와의 일치를 확인하지 못했습니다. `.git`이 가리키는 작업 트리 밖 경로는 열지 않았습니다. Unity는 PATH에서 확인되지 않았으며 빌드·플레이·스모크 검사는 실행하지 않았습니다. 읽기 전용 규칙으로 `codex/task-reports` 작성·게시도 수행하지 않았습니다.

## Changed files

- (none)

## Screenshots

- (none generated)

## Error

None.

> Change-mode code is committed and pushed only to its unique codex/implementation/<task-id> review branch after the Windows build and player smoke test pass. The runner never writes to main, merges, or force-pushes. Task reports go to the separate report branch.