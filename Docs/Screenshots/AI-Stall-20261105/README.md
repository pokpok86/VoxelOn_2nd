# AI 벽 정지 수정 — 실제 재현 비교

3기체×경계벽/밀착한 긴 벽/U자 엄폐물의9상황입니다. 각30초 게임시간, 50Hz 고정 시뮬레이션/8틱마다 실제 렌더로 총270초를 검증했습니다. 벽시계270초 플레이나 FPS 측정은 아닙니다. 시험용 벽은 기존 Blender FoundryCargo 자산을 재사용했고 일반 Canyon 맵에 추가하지 않았습니다.

| 기체 | 경계에서 최장 정지: 이전→이후 | 경계 캡처 | U자 탈출 후 |
| --- | --- | --- | --- |
| WISP | 2.64→0.00초 | [전](Before/1-case-0.png) / [후](After/1-case-0.png) | [후](After/1-case-2.png) |
| BASTION | .24→.02초 | [전](Before/2-case-0.png) / [후](After/2-case-0.png) | [후](After/2-case-2.png) |
| TARANTULA | 2.88→0.00초 | [전](Before/3-case-0.png) / [후](After/3-case-0.png) | [후](After/3-case-2.png) |

사진은 시험 종료 위치가 달라진 실제 관전 화면입니다. 같은 포즈의 디자인 비교가 아니며 정지 시간은 로그/궤적을 근거로 합니다. Before/After에 각각9장이 있습니다. [수치 CSV](../../Validation/AI-Stall-20261105/scenario-comparison.csv), [원본 로그](../../Validation/AI-Stall-20261105/), [수정 결과](../../Development/AI-Stall-Fix-20261105.md).

초기57검사 중5실패→수정 후57통과. 경계 밖 탈출점3개와 지속 정지2개를 고쳤고, 다른 벽/U자 상황의 정상 결과도 유지했습니다. 변경 후 전체9상황의 최장 정지는 .34초 이하였습니다. 모든 맵/모든 경기에서 완전 무정지를 보장하지는 않습니다.

추가 실내/실외 전투·모서리 화면은 [Generated](Generated/)에 원래 경로와 함께 보관했습니다. 기존 야간/이전 조작 갤러리는 보존합니다. [새 자동 캡처 출처와 해시](generated-manifest.json).
