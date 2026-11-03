# WISP v003 — 비례와 관절 리그 후보

2026-11-03. v002 Blender 원본을 직접 수정한 새 버전입니다. v001/v002 원본 및 FBX는 수정하지 않았습니다. 최종 외형 승인을 뜻하지 않습니다.

## 파일과 참고

- 원본: `Assets/Art/Robots/WISP/Source/WISP_v003.blend`
- Unity 모델: `Assets/Art/Robots/WISP/WISP_v003.fbx`
- 공식 참고: `Docs/VisualReferences/WISP-turnaround-v001.png`
- 정면 전후 비교: `WISP-v003-front-comparison.png` (왼쪽 v002 / 오른쪽 v003, 같은 카메라와 크기 기준)
- 개별 렌더: `WISP-v003-front.png`, `WISP-v003-side.png`, `WISP-v003-rear.png`
- Unity 실제 실행 화면: `WISP-v003-unity-stand.png`, `WISP-v003-unity-stride-left.png`, `WISP-v003-unity-stride-right.png`, `WISP-v003-unity-jump.png`
- 재현 스크립트: `Tools/Art/build_wisp_v003.py`를 Blender `-b WISP_v002.blend -P ...`로 실행합니다. v003을 다시 생성하므로 수작업 수정이 추가된 이후에는 별도 버전으로 출력 경로를 변경해야 합니다.

## 비례 수정

머리 폭 12%, 머리 높이 16% 확대, 몸통 세로 13% 축소, 허리 폭을 아래쪽으로 좁혔습니다. 어깨를 넓히고 낮추고, 팔 길이를 줄여 바깥으로 편안하게 벌렸습니다. 허벅지 간격과 종아리 폭을 늘렸으며 발 깊이와 두꺼운 검은 밑창을 줄였습니다. 복셀 간격을 새로 줄이거나 원본을 고해상도로 다시 복셀화하지 않았습니다.

참고 그림과 비교하면 표면은 여전히 각진 복셀 형태이며, 헬멧·가슴·정강이의 세부 형상은 추가 조형이 필요합니다. 이번 버전은 비례/리그 검토 후보입니다.

## 실제 리그

- `WISP_Rig_v003/Root` 아래 24개 뼈대, 112개 메시를 실제 Armature modifier와 rigid vertex weights로 연결했습니다.
- Head / Core / Arms / Legs / Weapon 및 손/무릎/발목/장착점 이름을 유지합니다. 빈 Transform만 움직이는 기존 구조에서 실제 스킨 관절 구조로 전환했습니다.
- 기계 부품은 가장 가까운 관절에 100% 가중치를 줍니다. 장갑 자체가 고무처럼 휘어지지 않습니다.
- 기본 메시 자세는 엉덩관절 -7°, 무릎 +14°, 발목 -7°로 조형하고 실제 발바닥 정점을 기준으로 지면을 맞췄습니다. 이 자세를 스킨의 rest shape로 구웠기 때문에 Unity에서 뼈의 기본 회전은 0, 스케일은 1입니다.
- Blender에는 1–49 프레임의 관절 보행 미리보기 Action이 있습니다. FBX에는 애니메이션을 구워 넣지 않았으며, Unity 게임의 기존 보행/점프 코드가 동일한 뼈를 구동합니다.
- 정확한 bone 위치는 `WISP-v003-RigReport.json`과 `WISP-v003-UnityRig.txt`를 참고합니다. v001 좌표를 복사하지 않습니다.

## Unity 연결과 검증

Unity 모델 참조는 v003, 색상/발광 텍스처는 v002 아틀라스로 연결했습니다. 기존 공통 총·돌진 칼·외형 방패 및 전투 조작을 유지했습니다. 총에 잘못 씌워지던 로봇 아틀라스도 제외했습니다.

게임 외형 배율 1.75 × WISP 기존 크기 계수 0.85를 적용해 실제 높이 약 4.038을 유지합니다. 외형 크기 보정은 충돌 반경이나 전투 수치를 바꾸지 않습니다. 발바닥 세계 Y=-0.910과 전장 바닥 Y=-0.910을 확인했습니다.

- Unity 6000.6.7f1 Windows 빌드: 성공.
- 실제 Windows 플레이어 스모크 검사: 237개 통과. 실제 관절 가중치, 무릎 회전 시 스킨 정점 변형, 바닥 접지, 게임 크기, 이동/조준/공격/점프 회귀 검사를 포함합니다.
- 기체 방향 캡처 16장, WISP 정지/보행/점프 자세 캡처 4장. Blender 정면 비교 및 Unity 정지/보행/점프 화면을 시각적으로 확인했습니다.
- 로그: `Logs/Build-Wisp-v003.log`, `Logs/Smoke-Wisp-v003.log`, `Logs/Capture-Wisp-v003.log`.
- 실행: `Builds/Windows/VoxSky.exe`. 빌드/로그는 Git 제외입니다.

자동 보행 자세와 실제 메시 변형은 확인했습니다. 사람이 키보드로 조작하며 평가하는 움직임 느낌 및 최종 실루엣 승인은 남아 있습니다. Git 커밋/푸시는 수행하지 않았습니다.
