# 재사용 부스터 효과 / 상승 및 WISP 이동 — 2026-11-08

## 구현
- 모든 방패 외형을 종전 대비 .8 배율(20% 축소)로 생성한다. 방어율/사용자 무게 데이터는 변경하지 않는다.
- boostClimbSpeed 2.2→8m/s, boostClimbAcceleration 30→14m/s². 상승 목표 속도는 기존 기체 verticalBoostScale / jumpWeightFactor와 남은 연료 비율을 적용한다. 높이 상한 없이 공유 에너지를 소비하며 가속한다. 기존 사용자 파츠 수치는 보존한다.
- 공중에서는 TryDash를 거부하고 수평 boostHeld/pulse/dash 상태를 해제한다. 점프 상승 부스터만 공용 에너지를 사용한다.
- WISP 지상 전방 Shift는 고속 달리기, 측면/후방 Shift는 정해진 거리의 대시. WISP는 기존 무료 Runner 지상 대시 예외에서 제외해 dashFuelCost × dashCostFactor를 한 번 소비한다. 기존 dashDistance × dashDistanceFactor와 cooldown을 사용한다. WISP 측/후방 대시에는 기존 타 Runner의 작은 도약을 추가하지 않는다.

## 재사용 컴포넌트
Assets/Scripts/BoosterEmitter.cs + Assets/Resources/BoosterPlume.shader.

```csharp
var exhaust = new GameObject("Socket exhaust");
exhaust.transform.SetParent(socket, false);
var emitter = exhaust.AddComponent<BoosterEmitter>();
emitter.size = 1f;
emitter.SetWorldPose(nozzlePosition, exhaustDirection);
emitter.SetThrust(pressed ? 1f : 0f);
```

로컬 +Z가 배기 방향이다. 고정 장착은 socket 자식으로 두고 로컬 위치/회전을 지정해도 된다. 프레임마다 world pose를 갱신하는 이동 장착도 가능하다. size, riseSeconds(.18), fallSeconds(.24), pulseAmount(.025), pulseHz(2.8), outerColor/innerColor로 조절한다. 버튼 해제 때 gameObject를 비활성화하지 말고 SetThrust(0)을 호출해야 축소 애니메이션이 유지된다. ResetEmission은 즉시 초기화 용도다.

이중 메시의 옅은 청색/백색 반투명 불꽃, ZWrite Off, 끝으로 갈수록 투명. 누르면 0→1 성장, 최대 출력에서 작은 맥동, 떼면 1→0 축소한다. 점프 해제 때 남은 배기가 갑자기 뒤로 꺾이지 않도록 축소 중 방향을 유지한다. 기존 불투명 BoostCone과 청색 고정 재질/빛은 교체했다. 기체의 백팩 아래를 현재 출력 위치로 사용한다.

## 실제 검증
Unity6000.6.7f1, Logs/Reusable-Booster-Final-Build-20261108.log: VOXSKY_BUILD_PASS 237429074 bytes, exit0.
실행본 --automatic-balance-study: Logs/ReusableBoosterReview-20261108/Booster-Final.log, AUTOMATIC_BALANCE_PASS 49 checks, exit0. 기존38검사 + 방패 축소, 임의 위치/방향, 성장/맥동/축소/소멸, 반투명 shader, WISP 측면 대시 거리/연료 소비, 공중 대시 거부/수평 부스트 차단, 이전 상승속도를 넘는 가속과 공유 연료 소비를 검사했다.
실행본 camera.Render로 Booster-growth.png / Booster-full.png / Booster-release.png를 생성해 실제 렌더링과 크기 변화/투명도를 확인했다. 최종 효과의 미술 선호와 실제 키 입력 플레이 감각은 PD 확인 가능.
사용자 PD-Balance.json 해시 빌드/검증 전후 동일, Arena.unity 원본 바이트 보존, git diff --check 통과. 기존 미커밋 작업 보존, 자동 커밋/푸시 없음.
