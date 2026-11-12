# WISP 진행 현황 — 2026-11-12

맥스/후속 개발자가 현재 적용 상태와 디자인 검토 상태를 구분하기 위한 인계 문서다. 이번 커밋은 기존 work 위에 누적된 게임 변경, 신규 파츠 원본, 실제 실행 사진과 디자인 비교 자료를 보존한다.

## 게임에 적용된 외형 16종

| 부위 | 수량 | 모델 |
|---|---:|---|
| 기본 머리 | 1 | HD01 |
| 기본 몸통·어깨·상박 | 1 | BD01 |
| 하박 무장 | 5 | LUMEN / BREAKER / RATTLE / EDGE / AEGIS |
| 백팩 | 5 | SKYWING / CROSSLANCE / OVERTHRUST / LONGTANK / RAMJET |
| 하체·골반 | 4 | STRIDER / BASTION TRACK / TARANTULAN / SKYLIFT |

새 외형은 커스터마이징의 **신형 외형**에서 선택하며 설계/기체별 별도 파일에 자동 저장한다. 기존 PD 수치와 장비 ID를 덮어쓰지 않는다. 현재는 외형과 기존 동작의 연결 단계다. 하체 외형 교체가 별도의 이동 물리나 무게/부스터 성능 변경을 뜻하지 않는다. 특히 SKYLIFT 전용 호버 물리는 추가하지 않았다.

어깨 무장은 향후 백팩 장착 방식으로 옮길 예정이다. 현재 일반 실행에서 3·4번 슬롯은 미장착으로 처리하여 모델·포구·선택·사격을 차단하고 HUD에 EMPTY로 표시한다. 저장된 과거 장비 데이터는 보존한다.

### 관절 수정 및 실행 확인

- 좌우 이동에 맞는 고관절 회전, 발목 자세 보정, 발/궤도 지지점 높이 보정.
- 조준·가드 시 상박/하박 원통 관절의 중심뿐 아니라 축 방향도 정렬.
- 실제 Windows 실행 파일의 관절 검사 **60개 통과**: 4종 하체, 10가지 상태, 실제 Move/Animate 2,560회와 지지점 표본 320회. 평지 기준이며 모든 지형의 발 IK나 모든 장갑 관통을 검증한 것은 아니다.
- 같은 최종 빌드에서 신형 파츠 검사 **41개 통과**. 무기 교체/제거, 어깨 발사 차단, 별도 설정 저장 등 확인.
- Unity **6000.6.7f1**, 마지막 게임 빌드 `VOXSKY_BUILD_PASS 250389802 bytes`.
- 이전 외형 경로의 383개 스모크 통과는 앞선 빌드의 기록이다. 이를 신형 모드 전체 회귀 통과로 해석하지 않는다. 이전 FBX 이름·고정 치수를 검사하는 일반 스모크의 신형 대응은 미완료다.

[실제 게임 사진](../Validation/WispFamily-20261112/Gameplay.png) · [커스터마이징](../Validation/WispFamily-20261112/Customization.png) · [걷기 GIF](../Validation/WispJoints-20261112/Actual-Unity-Walk.gif) · [관절 검사 상세](WISP-Joints-20261112.md) · [게임 연결 상세](WISP-Family-Ingame-20261112.md)

## Blender 검토 중인 머리·몸통 10종

| 구분 | 모델 | 상태 |
|---|---|---|
| 제공 컨셉 기반 머리 4종 | RADOME / LONGSIGHT / SWIFTFIN / BULWARK | 1차 제작안, PD 디자인 검토 대기 |
| 제공 컨셉 기반 몸통 4종 | STANDARD / LIGHTFRAME / BULWARK CORE / ARC CORE | 실루엣 후속 수정, PD 디자인 검토 대기 |
| 추가 슬림 머리 2종 | NEEDLE / LANCET | 별도 컨셉 없는 방향 비교 초안, 미승인 |

몸통 4종은 가슴 장식만 바꾸던 방식에서 **몸통 폭·깊이·허리 외곽과 어깨 윤곽 자체가 다른 형태**로 수정했다. 표준형은 낮은 사각 어깨, 경량형은 얇은 쐐기형, 중장갑형은 넓고 아래로 덮는 장갑, 아크형은 높고 갈라진 어깨를 사용한다. 공통 목/허리/팔꿈치 위치와 짧은 상박을 유지했다. 동일한 촬영 크기의 검은 실루엣도 제출한다.

추가 슬림 머리는 앞뒤로 긴 납작형과 세로로 긴 갸름형을 비교하기 위해 만들었다. PD가 “머리가 너무 길어지면 좀 그럴 수 있겠다”고 의견을 주었다. **이 2종은 승인된 디자인이 아니다.** 작은 머리를 유지하면서 폭과 턱선만 조금 줄이는 방향을 대화에서 제안했으나, 그 추가 수정은 아직 수행하지 않았다.

Blender 5.2.3 LTS로 저장/렌더 후 파일을 다시 열어 공통 연결면과 접속 위치를 확인했다. 생성·파일·접속 검사 통과와 PD의 디자인 승인은 별개다. 신규 10종은 **Unity 미적용**이며, 이전 게임 관절 검사를 이 모델들의 가동 검증으로 재사용하지 않는다.

[머리 4종 사진](../Design/WISP_Parts/HeadCore-20261112/Heads-Review-Phone.png) · [몸통 4종 사진](../Design/WISP_Parts/HeadCore-20261112/Cores-Outline-v2-Phone.png) · [실루엣 비교](../Design/WISP_Parts/HeadCore-20261112/Cores-Outline-v2-Silhouettes-Phone.png) · [장착 비교](../Design/WISP_Parts/HeadCore-20261112/Cores-Mounted-Phone.png)

[Blender 원본·검사 안내](../Design/WISP_Parts/HeadCore-20261112/README.md) · [슬림 머리 초안 사진](../Design/WISP_Parts/SlimHeads-20261112/Slim-Heads-Review-Phone.png) · [슬림 머리 원본·검사 안내](../Design/WISP_Parts/SlimHeads-20261112/README.md)

## 함께 보존한 앞선 개발 기록

커밋 전부터 남아 있던 외골격 팔, 활성 무기 HUD, 비락온 유도, 가상 대전/검사 보정 코드를 관련 보고서·기존 사진·CSV와 함께 보존했다. 각 보고서는 당시 실제 실행 범위와 실패/미완료 항목을 구분한다. 커밋을 위해 새 가상 대전이나 전체 검사기를 다시 실행한 것은 아니다.

[외골격 팔](Arm-Exoskeleton-20261110.md) · [무기 HUD](Weapon-HUD-20261110.md) · [비락온 유도](NonLock-Guidance-20261110.md) · [기체별 가상 대전](Weapon-Duel-Chassis-Extension-20261108.md) · [탄속 검사 보정](Projectile-Speed-Study-20261110.md) · [4족 검사 보정](Quadruped-Leg-Study-20261110.md)

## 보존 원칙

Arena.unity와 PD-Balance.json은 기존 바이트/사용자 값을 유지했다. 이번 자료 저장에는 실행 빌드, Library/캐시, 백업, 과거 반복 렌더 중간본을 포함하지 않는다. main 병합이나 예약 작업 재개는 하지 않는다.
