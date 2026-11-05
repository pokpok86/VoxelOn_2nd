# Blender 무기 라이브러리 인계

최신 원본: `ArtSource/Weapons-20261105/WeaponLibrary_v003.blend`.

FBX/카탈로그: `Assets/Resources/PDParts/Weapons20261105/*_v003.fbx`, `catalog_v003.json`.

제작 스크립트: `Tools/Art/weapon_library_20261105.py`. v001/v002 스크립트와 원본도 보존합니다. 수정 시 REV를 새 버전으로 올리고 기존 소스를 유지합니다. Blender 실행에는 `--python-exit-code 2`를 지정하고 `WEAPON_LIBRARY_PASS`를 확인합니다.

- Unity 기준 +Y 위, +Z 총구 전방, 미터 단위. FBX에 저장된 축 보정은 유지합니다.
- 각 모델의 MODEL_ROOT, RECEIVER 및 필요 시 BARREL/MAGAZINE/BIPOD 등 기계별 루트가 분리되어 있습니다. 실제 가동 애니메이션은 후속 작업입니다.
- GRIP_MAIN: 손 중심. 현재 총기 공통 좌표는 (0, -0.23, -0.08)입니다.
- MUZZLE: 실제 발사 위치. 총알을 고정 거리에서 생성하지 않고 이 접속점을 사용합니다.
- GRIP_SUPPORT, MOUNT, OPTIC_MOUNT와 무기별 보조 접속점은 카탈로그에 기록합니다.
- 라이브러리 원본에서는 모델을 그리드에 배열했습니다. 개별 FBX는 루트를 원점에 둔 상태로 내보냅니다. 그리드 위치를 포함해 그대로 반출하지 마세요.
- 같은 파일에 새 형상을 덮어쓰지 말고 버전을 올린 후 Unity에서 축·치수·손 그립·총구·어깨 가동·정면 벽 차단을 검증합니다.

기체 손 변경: WISP v011→v012, BASTION v006→v007, TARANTULA v005→v006. `Tools/Art/glove_hands_20261105.py`는 손 가중 버텍스만 손목 피벗을 기준으로 확대합니다. `verify_glove_sources_20261105.py`로 손 이외 버텍스 및 모든 관절·바인드 행렬 동일성을 확인했습니다. [원본 검증](GloveHands-20261105/NativeIntegrity.json).

[사용법·현재 기능 범위](../Balance/README.md) · [실제 비교와 검증](../Development/Equipment-Editor-20261105.md)
