# WISP 머리와 중앙 조준점 분리 — 어깨 시점

2026-11-03. 기체 크기를 유지하면서 기본 카메라를 오른쪽 어깨 뒤로 2.4 이동하고, 시선도 같은 방향으로 이동해 WISP가 화면 왼쪽에 위치하도록 했습니다. 위에서 내려다보는 기본 궤도 각도 26도와 마우스 시점 조작을 유지합니다.

벽 근처에서 카메라가 회피할 때도 현재 카메라의 오른쪽으로 시선을 1.7 이동시켜 머리가 중앙 조준점을 가리지 않도록 했습니다. 벽 충돌 검사와 최종 위치 보정은 계속 적용합니다. WISP 수동 조준·양손 입력·방패 모션·전용 출격 제한은 유지합니다.

검증:
- Unity Windows 빌드 성공: `VOXSKY_BUILD_PASS 164321158 bytes`.
- 실제 실행 파일 스모크: `VOXSKY_SMOKE_PASS 253 checks`, 종료 코드 0.
- 실제 스킨 메시의 머리 정점을 화면에 투영해 중앙 조준점 전체 영역과 겹치는지 검사했습니다. 피치 -5/26/45도 × 요 0/90도 6가지와 벽 회피 상황이 모두 통과했습니다.
- `WISP-crosshair-camera.png`와 `WISP-camera-wall.png`를 실제 빌드에서 캡처하고 시각적으로 확인했습니다. 기본 화면에서 머리와 조준점이 좌우로 분리됩니다. 이전 정면 구도는 `WISP-camera-before-shoulder.png`에 보존했습니다.
- 로그: `Logs/Build-Shoulder-Camera.log`, `Logs/Smoke-Shoulder-Camera.log`, `Logs/Capture-Shoulder-Camera.log`, `Logs/Capture-Shoulder-Wall.log`.

자동 재현 검사와 화면 확인 결과입니다. 전체 맵을 사람이 조작하며 순회한 검사는 아닙니다. 커밋/푸시는 하지 않았습니다.
