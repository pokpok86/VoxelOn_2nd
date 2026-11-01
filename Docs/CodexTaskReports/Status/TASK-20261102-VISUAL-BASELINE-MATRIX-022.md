# Task status: TASK-20261102-VISUAL-BASELINE-MATRIX-022

| Time (UTC) | Status | Detail |
| --- | --- | --- |
| 2026-11-01T16:06:02.2448958Z | started | Priority 19; isolated work branch detached/read-only; source f808e5952fee. |
| 2026-11-01T16:06:15.8376635Z | in-progress | Codex is executing the approved task in an isolated worktree. |
| 2026-11-01T16:07:46.5130425Z | completed | 읽기 전용 확인을 완료했습니다. **기존 36장은 모두 768×432이지만, 검증된 후보 이미지나 전후 비교 쌍은 없습니다.** 콘셉트 원화 일치나 개선을 입증하지 않습니다.

**정확한 이미지 목록**

아래 각 행의 네 파일은 모두 `Docs/Screenshots/Robots/` 아래에 있습니다.

\\| 기체 \\| 파일명 4개 \\|
\\|---\\|---\\|
\\| Kestrel \\| `01-kestrel-front.png`, `01-kestrel-back.png`, `01-kestrel-left.png`, `01-kestrel-right.png` \\|
\\| Bastion \\| `02-bastion-front.png`, `02-bastion-back.png`, `02-bastion-left.png`, `02-bastion-right.png` \\|
\\| Wisp \\| `03-wisp-front.png`, `03-wisp-back.png`, `03-wisp-left.png`, `03-wisp-right.png` \\|
\\| Nomad \\| `04-nomad-front.png`, `04-nomad-back.png`, `04-nomad-left.png`, `04-nomad-right.png` \\|
\\| Scatter \\| `05-scatter-front.png`, `05-scatter-back.png`, `05-scatter-left.png`, `05-scatter-right.png` \\|
\\| Lancer \\| `06-lancer-front.png`, `06-lancer-back.png`, `06-lancer-left.png`, `06-lancer-right.png` \\|
\\| Tempest \\| `07-tempest-front.png`, `07-tempest-back.png`, `07-tempest-left.png`, `07-tempest-right.png` \\|
\\| Anvil \\| `08-anvil-front.png`, `08-anvil-back.png`, `08-anvil-left.png`, `08-anvil-right.png` \\|

나머지 네 경로:

- `Docs/Screenshots/Arena/arena-north-sector.png`
- `Docs/Screenshots/Arena/arena-east-sector.png`
- `Docs/Screenshots/Arena/arena-south-sector.png`
- `Docs/Screenshots/Arena/arena-west-sector.png`

**출처와 비교 조건**

문서는 Windows 빌드 캡처라고 설명합니다. 대응 생성 코드는 `Assets/Scripts/VoxArena.Smoke.cs`의 `CaptureDocumentationGallery()`이며 실행 옵션은 `--capture-docs-gallery`입니다. 다만 기존 PNG의 생성 SHA·브랜치·빌드 해시·촬영 시각을 연결하는 증빙은 확인되지 않았습니다.

\\| 항목 \\| 현재 소스 설정 \\|
\\|---\\|---\\|
\\| 씬 \\| `Assets/Scenes/Arena.unity` \\|
\\| 출력 \\| 768×432, RGB24 PNG \\|
\\| 로봇 \\| 위치 `(0,1.1,0)`, +Z 방향, 속도 0 \\|
\\| 로봇 카메라 \\| 원근, FOV 40°, front/back/left/right 위치 `(0,4.1,8.7)` / `(0,4.1,-8.7)` / `(-8.7,4.1,0)` / `(8.7,4.1,0)`, 주시점 `(0,1.8,0)` \\|
\\| 경기장 카메라 \\| FOV 55°, 높이 36, 각 방위 거리 78, 원점 주시 \\|
\\| 조명 \\| 키 회전 `(40,-30,0)`, 강도 1.05; 림 `(28,145,0)`, 강도 .34; ambient `(.39,.45,.54)` \\|

향후에는 **동일 기체·동일 방향 파일끼리**, 경기장은 **동일 sector끼리** 대응시킬 수 있습니다. 현재 파일끼리는 전후 쌍이 아닙니다. `Animate()`가 `roundTime`으로 몸체 높이를 바꾸므로 카메라만 일치시켜서는 부족합니다. 기준·후보 모두 같은 애니메이션 시각과 관절 자세, 맵·조명·렌더 설정으로 재촬영해야 합니다.

**향후 캡처 manifest 제안**

비교 쌍마다 다음 레코드를 사용합니다. 미확인 값은 추정하지 않고 `null`로 둡니다.

```json
{
  "pair_id": "bastion-front-head-width",
  "visual_change_reason": "Bastion SD head width trial",
  "baseline_path": "Docs/QA/<baseline-sha>/02-bastion-front.png",
  "candidate_path": "Docs/QA/<candidate-sha>/02-bastion-front.png",
  "captures": {
    "baseline": {
      "source_sha": null,
      "branch": null,
      "unity_version": "6000.6.7f1",
      "unity_build_hash": null,
      "scene": "Assets/Scenes/Arena.unity",
      "robot": "BASTION",
      "camera_pose": {
        "position": [0, 4.1, 8.7],
        "look_at": [0, 1.8, 0],
        "projection": "perspective",
        "fov_degrees": 40
      },
      "resolution": [768, 432],
      "robot_pose": {},
      "animation_time": null,
      "lighting_map_render_settings_hash": null,
      "timestamp_utc": null,
      "original_png_sha256": null
    },
    "candidate": null
  },
  "pass_fail": "not_run",
  "producer_approval": "pending"
}
```

`candidate`도 baseline과 같은 필드를 필수로 사용합니다. 빌드 해시는 전체 배포 파일의 경로·SHA256 목록으로 만든 manifest의 SHA256으로 정의하면 됩니다. `pass_fail`은 `not_run/pass/fail`이며, 캡처 조건 불일치는 실패로 남깁니다.

**선택할 단일 변경: Bastion 머리 폭**

현재 정면 PNG에서 머리가 넓은 어깨에 비해 작게 읽힙니다. 향후 실험은 **Bastion에만 head module X 배율 1.10 적용**을 제안합니다. Y/Z·위치는 유지하고 기존 머리 부품만 함께 넓힙니다. 원시 Helmet 폭 `.70→.77`, Siege brow `.75→.825`에 해당합니다.

정면·후면의 머리 폭/가슴 폭 비율이 기준 대비 약 10% 증가하는지 측정하고, 측면 깊이와 목·어깨 간섭을 확인합니다. 막대나 장식은 추가하지 않습니다. 이는 원화 일치 판정이 아닌 제한된 실루엣 실험이며, 제작자 승인은 대기입니다.

**PM 검토 방법**

현재 파일은 GitHub의 해당 커밋 파일 화면에서 **Raw/Download original**로 받아 검토합니다. 향후에는 선택한 Bastion 네 방향의 기준·후보 PNG와 manifest만 GitHub artifact에 묶거나 `Docs/QA/<sha>/`에 보관하고, 기준 파일은 경로로 재사용합니다. PNG 원본 바이트를 리사이즈·재인코딩하지 않고 SHA256으로 확인합니다. PNG 자체는 무손실 압축 형식입니다. 원화 접근은 전제하지 않으며 개인 경로·로그·비공개 원화는 포함하지 않습니다. 이번에는 업로드하거나 외부에 접속하지 않았습니다.

변경 파일은 **없습니다**. 파일 목록·소스 검토, 36장 PNG 헤더 크기 확인, SHA256 계산, Bastion 정면 원본 육안 확인을 수행했습니다. Git 실행 도구가 없어 현재 브랜치·SHA·상태를 검증하지 못했고, 빌드 산출물·캡처 로그·검증된 후보도 없었습니다. Unity 빌드·플레이 테스트는 실행하지 않았습니다.

**이메일을 보내지 않았습니다. 제작자가 미룬 이메일 알림도 실행하지 않았습니다.** |
