# SOL 인계 — 길이 work 브랜치 독립 검수 (2026-11-14)

- **담당:** SOL (독립 검수 / 원인 분석)
- **요청:** PD / Max
- **상태:** 검토 요청서. 자동 실행 큐 아님; SOL 호출 시 이 파일을 명시적으로 전달
- **대상:** `pokpok86/VoxelOn_2nd` 의 `work` 브랜치
- **안전:** `main` 및 PD 사용자 밸런스 파일, Unity 씬, 기존 .blend 원본을 변경하지 말 것. 검수 1차는 읽기 전용. 수정은 PD가 결과 확인 후 별도 승인.

## 1. 길이 인계 커밋
- `0f9fc21` — WISP 실제 게임 연결 기록, 관절/파츠 실행 검증, 머리/몸통 다각도 이미지와 Blender 원본
- `b575a2a` — 무기 무게 편집/낙하 계산과 검증 기록; 재장전·반동 작업 누적
- `cf7ad13` — 확장 백팩 2종·하체 3종 Blender 원본/리뷰 PNG/매니페스트 및 검증 툴
- 시작 시 `work` 최신 SHA를 확인해 보고할 것. 위 SHA는 검수 기준이며 현재 HEAD라고 단정하지 말 것.

## 2. 솔의 1차 업무 (검수 및 리포트)
1. **재현/회귀 분류:** `Docs/Development/Weapon-Stats-Simplification-20261113.md`의 365 PASS / 20 FAIL을 확인. 최신 빌드와 기준 빌드를 같은 조건으로 비교하고, 신규 회귀 / 기존 실패 / 테스트가 구 설계를 가정하는 오류 / 미확인으로 분류. 통과라고 적당히 처리하지 말 것.
2. **편집/입력 검증:** 단일 재장전 `reloadTime`의 구버전 JSON 호환; 반동 카메라 효과; 무기 무게-낙하 계수; 밸런스 편집 테스트가 `BuildPortraits()`에서 정지한 원인과 재현성. 데이터 변경 없이 평가.
3. **무기 슬롯 3·4:** `Docs/Development/PROGRESS-20261112-WISP.md`에 따라 의도적으로 EMPTY 처리한 구 어깨 무장인지 확인. 의도적 비활성으로 인한 레거시 테스트 실패와 실제 버그를 구별. 향후 서브 무기는 어깨가 아니라 **백팩 장착** 예정이며, 기존 입력/데이터를 임의 수정하지 말 것.
4. **WISP 파츠 연결:** 신형 외형 16종의 실제 게임 장착, 관절, 바닥 접촉, 무기 교체, 저장과 UI. 머리·몸통 10종 후보는 Unity 미적용이므로 인게임 검증된 것으로 간주하지 말 것.
5. **신규 Blender 모델:** `Docs/Design/WISP_Parts/EXPANSION_DELIVERY.md` 및 `Expansion-Validation.json` 기반으로 B-02 SKYWING, B-03 CARGO PACK, L-02 BASTION TRACK, L-03 TARANTULAN, L-04 DRAFT HOVER의 .blend 열기/메시 계층/장착 접속/실루엣/렌더 간 일치 여부 확인. **정적 연결 검사와 애니메이션/Unity 검증을 명확히 구분.**
6. **파츠 ID 충돌 조사:** 런타임 WispFamily20261112에는 B-01 SKYWING 등 5개 백팩과 L-05 SKYLIFT가 있고, Blender 검토 카탈로그에는 B-01 TWIN DRIVE, B-02 SKYWING, B-03 CARGO PACK, L-04 DRAFT HOVER가 있음. 동일 기능에 다른 ID 또는 동일 ID에 다른 이름이 부여된 경우를 정리하고, **기존 파일을 rename하지 않은 채** 일관된 ID 매핑안을 제안.
7. **디자인 적합성:** PD가 강조한 WISP의 작은 머리·입체적인 몸통·짧은 상박·큰 하박 일체형 무장·상대적으로 짧은 하체. 원본 컨셉과 45도/정면/측면 및 실제 게임 화면에서 비교하되, 컨셉이 제공되지 않은 5개 신규 후보는 '원본 그대로'라고 주장하지 말 것.

## 3. 구현 전 참고할 PD 기획
- `main/Docs/Design/Weapon_Stats_Simplification_v01.md`: 무기 기본 수치 9개, 기존 데이터 호환 우선
- `main/Docs/Design/Parts_MinimalStats_Underwater_Design_v01.md`: 파츠 기본 수치 최소화, 몸통 피해 감소율 제외, 수중 화면 청색 피드백 수정 요구
- `main/Docs/Design/WISP_WeaponDesign_Draft_v01.md`: 무기 특수 기능 및 고유 우클릭 액션 초안
- **지금 검수 범위에서 위 문서를 핑계로 대규모 코드/수치 일괄 변경하지 말 것.**

## 4. 제출 형식
- 검수한 기준 SHA와 환경(Unity / Blender / 실행 빌드)
- 항목별: 재현 단계, 예상/실제, 근거 파일·로그·스크린샷, 심각도, 신규 회귀 여부
- 테스트: 통과/실패/미실행 각각 수, 기준 빌드 비교, 실행 가능한 재현 명령(기존 검사만)
- 파츠 ID 대응표 및 충돌/불명확 목록
- **먼저 고칠 문제 TOP 3**와 최소 수정 방안, 예상 영향, 변경 필요 파일
- 어떠한 수정 커밋도 진행하기 전에 PD 승인 요청

## 참고 경로 (work)
- `Docs/Development/PROGRESS-20261112-WISP.md`
- `Docs/Development/Weapon-Stats-Simplification-20261113.md`
- `Docs/Design/WISP_Parts/EXPANSION_DELIVERY.md`
- `Docs/Design/WISP_Parts/PART_CATALOG.json`
- `Docs/Validation/WispFamily-20261112/`

**주의:** 이 문서는 SOL 전용 검수 인계다. 자동 실행 상태 `ready`를 부여하는 작업 큐 파일이 아니며, SOL가 실제로 읽고 실행했다는 사실도 뜻하지 않는다.
