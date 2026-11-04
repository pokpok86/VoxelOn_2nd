# 우천 저녁 시인성 / 얕은 물 반복 비교

기준은 동일 WISP_v011/BAS_v005, 같은 위치·카메라·8개 고정 날씨/시간대다. 실제 Windows 플레이어가 촬영한 원본이다. 비·파문은 시간에 따라 위치가 조금 달라진다.

| 항목 | 전 | 1차 | 2차 |
| --- | --- | --- | --- |
| 비 오는 저녁 | [Before](Before/Rain-Evening.png) | [After01](After01/Rain-Evening.png) | [After02](After02/Rain-Evening.png) |
| 뇌우 저녁 | [Before](Before/Thunderstorm-Evening.png) | [After01](After01/Thunderstorm-Evening.png) | [After02](After02/Thunderstorm-Evening.png) |
| 맑은 아침 / 얕은 물 | [Before](Before/Clear-Morning.png) | [After01](After01/Clear-Morning.png) | [After02](After02/Clear-Morning.png) |

직접 비교한 결과:
- 이전 저녁은 기체 뒷면과 상대가 어두운 바닥에 묻혔다. 1차 주변광만으로는 부족해 2차에서 기존 차가운 보조광 방향을 저녁에 조정했다. 흰 외장·주황 어깨·손/허리 형태와 먼 상대가 더 잘 읽힌다. 아침 조명/카메라/게임 규칙은 유지한다.
- 어두운 파란 타원처럼 보이던 물은 Blender `ShallowWater_v002.blend` → `ShallowPond_v002.fbx`로 새 제작했다. 불규칙한 가장자리, 중심~외곽 알파1→0의129정점 표면을 사용한다. 얕은 바닥이 보이고 가장자리가 부드럽게 섞이며 원본 Pond는 보존한다.
- 얕은 물용 Standard 표면 셰이더를 연결했으며 지면 높이/충돌은 변경하지 않는다. 물보라·파문은 기존 고정 풀을 재사용한다. 야간 최종 분위기와 물의 미감은 수동 승인 대기다.

실행/검증:
- 수정 전 최신 모델 기준 실제8장, `Lighting-Iteration01-Before.log`: 기존25검사 통과.
- 1차 Windows빌드204570861bytes 성공 / 전체 회귀336개 통과 / 날씨·접지·입출수28개 통과.
- 2차 Windows빌드204570870bytes 성공 / 관련 날씨·접지·입출수28개 통과 /8장 재촬영·직접 비교. 2차 변경은 기존 조명 파라미터뿐이므로 무관한 통과 검사는 반복하지 않았다.
- 새 실제 검사3개: 물 진입 시 풀 물방울+파문 생성, 밖으로 나가면 접촉 해제, 물/마른 지면 이동 거리 차이. 이동 페널티 없음.
- 실행: `Voxel-On.exe --climate-smoke --outdoor -logFile <절대경로>`.

원본 Blender/이전 모델/이전 화면/실행 백업 `Backups/Lighting-20261105/Windows` 보존. 신규 선택 맵이나 실외 장애물을 추가하지 않았다.