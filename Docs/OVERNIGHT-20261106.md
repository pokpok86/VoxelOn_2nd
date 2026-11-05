# Voxel-On 6시간 개발 — 2026-11-06

요청 시작 2026-11-05 23:57 KST. 종료 2026-11-06 05:57 KST (2026-11-05 20:57 UTC). 종료 시 기능 수정 중단, 진행 중 빌드/검증 마무리 후 한국어 결과 보고.

동일 대화 heartbeat `voxel-on-6` ACTIVE, 30분 주기 최대12회. 마지막에 pause/delete. PC와 앱이 켜져 있어야 한다. 예전 Windows 러너와 이전 야간 자동화는 재개하지 않는다. 박PD가 비용 절감 때문에 Luna를 선택했다고 명시했으므로 선택 모델 유지, 상위 모델 전환/서브에이전트/새 서비스 금지. 루틴 구현과 재시도는 승인되었다. main 병합/수정, 원본 삭제, 강제푸시는 하지 않는다. 새 Git 게시 요청 없이 자동 커밋/푸시하지 않는다.

## 실제 프로젝트와 도구

- `D:\Codex\VoxSky\Builds\WorkLatest`가 게임 프로젝트다. 대화 cwd인 OneDrive VoxelOn 2와 혼동하지 않는다.
- Unity `C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe`.
- Blender `C:\Users\User\OneDrive\문서\ChatGPT\VoxelOn\tools\blender-4.5.15-windows-x64\blender.exe`.
- D 프로젝트 작업은 현재 환경에서 require_escalated로 실행한다. 자동 검토 승인 경계를 따른다. 일반 cwd에서 WindowsApps python alias 실행이 거부된 적이 있어, D 작업은 승인된 도구 실행/작업 폴더를 사용한다.
- Unity 동시 빌드 및 빌드 중 코드/FBX 변경 금지. 그래픽은 Blender→새 버전 FBX→Unity 빌드→동일 각도 실제 촬영→참고/이전 비교→재수정.

## 출발점: 실제 통과한 현재 빌드

`Builds/Windows/Voxel-On.exe`, 최종 `Logs/Equipment-HUD-Final-Build.log`: VOXSKY_BUILD_PASS 231661550 bytes.
최종 smoke358, 편집/파일/양손/방패/주먹39, UI3장 통과. 직전 동일 전투 로직의 어깨145, 실내/실외24씩, 조종18, 손 확대6 통과. 근거는 `Docs/Development/Equipment-Editor-20261105.md`, `Docs/Validation/Equipment-20261105`.

HEAD `3a965069a070562bcdae96ffe6343d1b9ed62b63`, 기존 조작/AI/장비 수정 모두 미커밋 후보. 사용자 작업 보존. `Data/Balance/PD-Balance.json`의 사용자 편집값을 덮어쓰지 않는다. 기본 예제는 Docs/Balance에 별도. 백업 `Backups/Weapon-Art-20261105/Windows`, `Backups/Balance-Editor-20261105/Windows`. 새 손 교체 전 현재 검증 빌드도 새 백업한다.

## 최신 박PD 지시: 손을 다시 만들어야 한다

현재 WISP v012 / BASTION v007 / TARANTULA v006은 전체 손을 1.38×1.18×1.52로 키운 상태다. **박PD는 이 큰 손등 박스 형태를 최종 승인하지 않았다.** 새 사진처럼 굵고 짧은 분절 손가락과 엄지, 총 손잡이를 감싸 쥐는 손이 필요하다.

- `Docs/VisualReferences/Nacchin-Hands-20261106/Hand-Shape-Reference.png`
- `Docs/VisualReferences/Nacchin-Hands-20261106/Gun-Grip-Reference.png`

녹색 사진은 형태/파지 참고다. 별도 색 변경 지시가 없으므로 기존 흰색·주황색·어두운 관절 팔레트 유지.

다음 구현 방향(아직 새 손 구현 완료 아님):
1. Blender에서 손바닥을 덜 두툼한 형태로 만들고 굵은 3마디 손가락/엄지/어두운 관절을 재설계. WISP v013, BASTION v008, TARANTULA v007 등 새 버전. 기존 몸체·관절·바인드 보존.
2. 실제 파지/주먹 변형을 shape key 또는 보조 관절로 추가. 기존 관절은 변경하지 않는다. 손바닥 내부의 새 GRIP_SOCKET을 추가해 총을 손목이 아닌 손 안에 배치한다.
3. 맨손/총/방패에 맞는 손 포즈, 좌우 펀치, 어깨 가동/실제 총구를 Unity에 연결하고 실제 비교한다.
4. 의미 있는 게임 버그/편집 불편 개선과 그래픽을 균형 있게 이어가되, 새로운 핵심 전투 요구를 임의로 만들지 않는다.

## 유지할 구현과 주의

- 손 좌우·어깨 좌우 독립 슬롯. 총15종, 기존 WISP 오른손 방패, 미장착 사용 가능. 방패와 총은 같은 손에 겹치지 않음. 미장착 손은 펀치, 빈 어깨는 없음.
- 1~4 선택 후 LMB. WISP RMB는 방패 장착 시 가드, 2/3 기존 락온. 수동 조준/현재 카메라/3기체/이동·바닥·모듈 유지.
- 실제 어깨 뼈 자식인 무장은 팔 회전 상속. 활성 어깨는 조준 자세로 맞춤. 옆 벽 회피와 정면 벽 차단 유지.
- 무기34/파츠18 편집 창, 실제 JSON 저장/백업/로드/명시적 적용, 사용자 원본 보존, 독립 런타임 복사본과 실제 탄창/장전 유지.
- 주황색 무게·반동·과열·에너지·락온 옵션은 저장만 한다. 적용되는 것으로 보고하지 않는다.
- 새 손이 구조/스킨 개수를 바꾸면 기존 검사 의도를 유지하고 새 승인 목표를 검사한다. 실패 삭제/기준 완화 금지. 이전 손 크기 확대 검증 이력은 보존하고 새 손 디자인 기준에 맞는 검증을 추가한다.
- 다음 손/어깨 변경 후 `--fx-capture --module-roster`의 실제 분리·복원27검사도 수행 필요. 현재 새 장비 단계에서는 아직 이 검사를 재실행하지 않았다.
- 동일 통과 검사를 관련 변경/우려 없이 반복하지 않는다. 매 시기 시작에 최신 파일/로그/사용자 메시지를 읽는다. 실패와 미검증을 숨기지 않는다.
- 결과 문서, 실제 스크린샷/전후 비교, 소스/빌드 지문과 다음 작업을 기록한다. 수동 미술 최종 승인 대기로 표시한다.
## 00시대 1차 실제 진행

- 자동화 최대12회 설정은 도구 업데이트 성공 및 실제 TOML에서 확인했다.
- 최신 손 참고2장을 보존했고 `Backups/Hands-Reference-20261106/Windows`와 Scripts 백업을 만들었다.
- `Tools/Art/reference_hands_20261106.py` 실행 성공. 새 소스/FBX: WISP v013, BASTION v008, TARANTULA v007. 두 손을 각각 하나의 메시로 재설계(각1,152 원본 버텍스). 손가락12개 분절+엄지2분절, Grip/Fist shape key, 손바닥 내부 GRIP_SOCKET 보조 뼈2개 추가.
- 기존 모든 관절/바인드와 손 이외 버텍스 보존 검증 통과. 새 스킨 수75/154/84, 관절26/117/38. 원본 v012/v007/v006 보존.
- Blender 실제 Relaxed/Grip/Fist 렌더9장 생성. WISP3포즈를 보고 손가락/엄지가 드러나는 개선을 확인했으나 Unity 총 파지는 아직 검증 전.
- C# 연결을 작성하고 `Logs/ReferenceHands-v001-Build.log` 첫 빌드 실행 중이었다. 먼저 프로세스/로그 완료를 확인할 것. 작업 세션34755가 실행 중이었다. 중복 빌드 금지.
- 추가 파일: VoxArena.ReferenceHands.cs, Assets/Editor/ReferenceHandsBuild.cs. ArmGrip.gripSocket, pose cache, 장착별 shape key, 총 쥘 때 좌우 손목±90도, 손바닥 socket으로 실제 그립 정렬. 새 팔 준비 목표는 총 손잡이를 어깨 앞에 배치하며 손목 offset 보정. 활성 어깨 무장 사용 시 그쪽 손의 비활성 총을 아래로 내리는 운반 자세를 넣었다.
- 양손 같은 총의 크기가 달랐던 문제도 수정: 양손.95, 어깨.75. 수입 사족/궤도 기체의 왼손 weaponPart .55 배율을 상쇄해 같은 총은 좌우 물리 크기가 같게 함. 실제 비교/검사 필요.
- 새 손은 아직 성공한 게임 적용으로 보고하지 말 것. 첫 빌드와 실제 화면이 필요하다.

### 다음 필수 작업

1. 첫 빌드 컴파일/임포트/shape key/버텍스 수 확인. 15,000 기존 모델 버텍스 예산에 문제가 있으면 기준을 늘리지 말고 새 손 스무딩/토폴로지를 Blender 새 버전으로 최적화. 몸체는 변경하지 않는다.
2. 새 `--reference-hand-study` 같은 검증/사진 코루틴 구현 필요. 원본 큰 박스 손(referenceHandsBaseline)과 새 손, 편 손/그립/주먹, 실제 총 접촉을 동일 각도에서 비교. GPU 반영을 기다리고 실제 렌더 확인.
3. 새 shape 이름은 Unity에서 접두어가 붙을 수 있어 ReferenceShapeIndex는 이름 또는 .Grip/.Fist suffix를 찾는다. 손바닥 socket이 없으면 구형 리그는 기존 파지를 사용한다. 새 리그에서는 socket/shape가 실제로 있는지 반드시 검사.
4. 옛 --glove-review 성장 검사는 v011→v012 등 과거 스코프다. 코드가 referenceHandsBaseline을 phase2로 사용해 해당 구형 비교를 보존하고 실제 새 모델 참조는 마지막에 복원하도록 수정했다. 새 손의 품질은 별도 새 검증으로 검사해야 한다.
5. 새 손 파지 때문에 무기 검사145의 그립 기준은 손목이 아닌 HandGripPosition(실제 socket)으로 바꿨다. 의미를 유지하는 수정이다.
6. smoke358(새WISP75스킨·호스·높이 조건 보존), 편집39(양손 같은 무기 실제 크기 검사 추가 권장), 무기145, 실내외소크, controls18, module-roster27을 변경/실패 범위에 맞게 실제 실행.


## 01:08 KST 검증 체크포인트 (앞의 미검증 상태를 대체)

- 새 손 현재 WISP014/BAS009/TARA008. Native v001 평면 손은 버텍스 예산 실패→v002 공유 법선 최적화로 해결. 기존 테스트를 약화하지 않았음.
- ReferenceHands-v003: 빌드237206213, 손36/27장, 모듈27, 무기145, 조종18, 장비39, 실내외24씩 통과. 실제 내측/측면 사진으로 총 손잡이를 감싸는 세 기체 손가락과 주먹을 확인함. 외측에서 숨겨진 것을 실제 파지 실패로 오판하지 말 것.
- 갤러리 고정 기본슬롯 표시 오류를 실제 player.weaponArtIds로 수정, 양손 중복 장착 표시 지원. v004빌드237206914, smoke358/장비41/10장 통과. 실제 갤러리 슬롯1·2 표시 이미지 확인.
- 상세 변경/검증/한계: Docs/Development/ReferenceHands-20261106.md. 로그/현재 코드 지문 Docs/Validation/ReferenceHands-20261106. 원본/실패로그/앞선 증거 보존. 아직 commit/push 하지 않음.
- Unity.exe 프로세스 이름만으로 빌드 여부를 판단하지 말 것: UnityHub의 resources/unity.exe serve는 에디터가 아님. 실행경로 Editor/Unity.exe 및 인수를 확인할 것.
- 다음 의미 있는 작업: 다양한 총 크기의 손 접촉·오른손/방패 전환과 실제 조작 불편을 비교하고 범위 내 수정. 기존 통과 검사를 새 변경 없이 반복하지 않음. 비용 절약, 상위모델/서브에이전트/새 서비스 사용 금지. 05:57KST 종료 지침 유지.

## 커스터마이징 목업 구현 체크포인트 (07:20 KST)

- 박PD 승인 행거 목업을 실제 인게임 편집 화면으로 구현했다. 결과/스크린샷: `Docs/Development/Customization-BASTION-20261106.md`, `Docs/Screenshots/Customization-20261106/Bastion-Customization-Actual-v002.png`.
- Blender BASTION v010(회백색 외장, 주황 어깨, 센서, 궤도)을 추가하고 v009를 보존했다. `VoxBuild`도 v010을 가리킨다.
- 장착/무기 옵션/파츠 수치/파일 탭, 기체 좌우 회전, 4개 장착 위치, 실제 3D 무기 썸네일, 선택 무기 옵션 바로가기, WISP·BASTION·기존 TARANTULA 선택을 넣었다. TARANTULA 디자인은 변경하지 않았다.
- Unity Windows 빌드 v006 성공, 총 237,220,694 bytes. 실제 빌드 캡처 로그에서 종료코드0 및 캡처 마커 확인. 첫 백그라운드 숨김 캡처가 검정으로 나와 창 표시 실행으로 다시 캡처했고, 이후 같은 각도에서 프리뷰 배율/목록 위치를 보정했다.
- 이 단계에서 자동 스모크/테스트 스위트는 미실행. 수동 장착 후 저장/재시작 동작 확인 및 박PD의 미술 승인은 남아 있다. UI에서 `[보관]`으로 표시된 수치는 저장만 되고 전투에 미적용.
- 커밋/푸시 없음, main 변경 없음.
