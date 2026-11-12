# B-05_RAMJET — 직선 가속형 백팩

박PD가 2026-11-12 전달한 사진을 기준으로 제작한 Blender 검토 후보입니다. **디자인은 PD 검토 대기**입니다. 기존 소체의 머리·몸통·상박·무기 비율은 163cfa5로 고정했습니다.

## 기준 그림과 결과

- `../References/B-05_RAMJET.jpg`: 제공된 원본 사진. 해시는 References/manifest.json에 기록.
- `B-05_RAMJET.blend`: 단독 수정 원본.
- `B-05_RAMJET-Mounted-Review.blend`: 승인 소체와 기존 LUMEN 장착 검토 장면.
- `01-Front`(몸통/장착면 쪽 -Y), `02-Right`, `03-Rear`(배기 쪽 +Y), `04-45deg`, `05-Top`: 같은 정사영 배율의 이미지.
- `06-Mount-Closeup`: 연결부 확대.
- `Mounted-Front`, `Mounted-Rear`: 현재 소체에 붙인 실제 조립 이미지.
- `B-05_RAMJET-Comparison-Phone.png`: 원본 그림과 제작 결과 및 조립 모습의 세로 비교 이미지.
- `manifest.json`: 규격·근거·가정 및 실제 검사.

## 구조

공통 후면 접속판과 4핀 간격을 유지합니다. 덕트는 주변 장갑에도 실제 통로를 내고, 일체형 외곽/경사진 입구 테두리/안쪽 벽/깊은 발광부로 만들었습니다. 단순한 덧붙임 그릴이나 발광 평판으로 구멍을 흉내 내지 않습니다.

사진에서 확인할 수 없는 내부 고정장치와 전개 기구는 제작 가정이며, 그림의 수치·성능 표기를 게임 데이터로 옮기지 않았습니다. 접이 날개/관절/궤도·부상 동작은 이번 모델의 정적 자세일 뿐 애니메이션 또는 게임 구현 완료가 아닙니다.

Blender 5.2.3에서 생성 및 저장 파일 재개방, 5방향 카메라 배율, 연결 규격, 원본 9개 .blend 해시 보존을 검사합니다. 노즐이 있는 모델은 입구에서 내부 발광부까지 광선이 통하는지 검사합니다. 실제 판정은 `../Validation.json` 기준입니다.

재생성: Blender Python으로 `Tools/build_expansion_part.py -- --reference-batch --part B-05_RAMJET` 실행. Unity/밸런스 변경 없음.
