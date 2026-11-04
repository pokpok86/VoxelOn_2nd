# 네 무장 탄환 — Blender 전후 비교

4슬롯 모두 보관된 기체 PRIMARY 모양을 사용하던 상태에서, 슬롯별로 구분되는 Blender FBX를 적용했다. 라이플32정점/캐논32/로켓72/레일32. 원본 `ArtSource/PD14/Projectiles_v002.blend`, 재생성 `Tools/Art/pd14_projectiles_v002.py`. 기체 외장·전투 수치는 변경하지 않았다.

| 무장 | 원래 렌더 | 새 Blender 렌더 |
| --- | --- | --- |
| 1 라이플 | [Before](Before/1-RIFLE.png) | [After](After/1-RIFLE.png) |
| 2 캐논 | [Before](Before/2-CANNON.png) | [After](After/2-CANNON.png) |
| 3 로켓 | [Before](Before/3-ROCKET.png) | [After](After/3-ROCKET.png) |
| 4 레일 | [Before](Before/4-RAIL.png) | [After](After/4-RAIL.png) |

이8장은 실제 FireEquippedWeapon으로 생성된 메시를 같은 위치·각도·배율에서 촬영한 **확대 검사뷰**다. UI/트레일은 검사뷰에서 제외했고 게임 중 크기로 오해하지 않는다. Before는 보존된 원래 렌더 함수로 촬영했다. 로켓 꼬리날개/추진부와 레일의 긴 광심을 직접 비교했다.

실제 게임에서는 기존 팀 색/트레일과 탄속을 유지하며 시각15% 확대와 물리 판정 반경을 분리한다. 실제 발사체의 가져온 전방 축과 횡단 크기, 실제 반경·피해·사거리 검사가 전체353개에 포함돼 통과했다. 기본값을 바꾸지 않았으며 최종 미감·플레이 체감 승인은 대기다.

실행: `--capture-projectiles` (새), `--capture-projectiles --legacy-projectiles` (보존된 전 렌더). 일반 게임에는 옛 렌더 선택 UI를 추가하지 않았다.