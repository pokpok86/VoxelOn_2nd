# 2026-11-19 게시 검증 근거

박PD가 최신 작업의 커밋·푸시 및 main 반영을 승인했다. 야간 작업의 main 금지/미커밋 기록은 그 당시의 경계이며 이번 게시 승인보다 이전 기록이다.

- 빌드: UIReview-Iter14-Build3.log, 281071450 bytes, 성공.
- 최신 플레이어 회귀: 693개 통과.
- ZIP 압축 해제본에서도 693개 통과. 이 PC에서 실제 압축 해제본을 검사한 것이며 다른 물리 PC에서의 실험은 아니다.
- 실제 Windows UI 증거9장은 ../Overnight-20261119/Iteration14에 있다.
- 첨부 txt는 원래 로그에서 로컬 프로젝트 경로·IP·기기 이름을 가린 공개용 사본이다.
- 패키지: Voxel-On-Windows-x64-20261119-0600.zip, 86831424 bytes.
- ZIP SHA256: `608bf1430829b1f179f884c1e683244cdc09bbd029e4986b5c5a0aa981fbdb86`.
- 소스·Blender 원본·FBX·PD 설정·검증 증거를 커밋한다. Builds/Logs/캐시/자동 백업은 기존 제외 규칙을 유지한다. ZIP은 로컬 패키지 산출물이며 Git 소스 커밋에 넣지 않는다.

상세 기능/임시 수치/미완료: ../../Development/Drone-Auxiliary-Handoff-20261119.md. 최종 수동 외형·밸런스 승인 및 모든 모드/카메라 장시간 플레이 검증은 별도이다.

## main 문서 병합 판단

main의 기존 기획/인계 문서를 병합한다. add/add 충돌4개는 work의 이후 승인·제작·검증 기록을 유지했다.

- TASK-20261112-WISP-PARTS-FIRSTBATCH-001.md: main의 참고 이미지 미도착 draft 대신, 이미지 도착과 PD 승인/제작 결과가 기록된 work 버전 유지.
- WISP_Base_v1/README.md 및 RIG_AND_REVIEW.md: work의 실제 Blender 실행·연결 구조·검토 범위를 유지.
- WISP_Base_v1/wisp_blender_v2.py: main의123행 초기 블록아웃보다 발전한 work의558행 생성기를 보존. Blender 원본/기존 자산 삭제 없음.
- main 초안은 병합 부모의 Git 이력으로 보존한다. 새 코드나 수치 변경을 이 병합에 섞지 않는다.
