# Canyon 경사 지형 검토 — 2026-11-08

## 변경

- 반복된 계단 모양의 자연석 덩어리 대신, 9개 능선을 불규칙한 저폴리 경사 메시로 표시합니다.
- 화면용 경사와 충돌용 높이 샘플을 분리했습니다. 가장 높은 경사에는 56개의 낮은 충돌 단계를 두어 WISP도 걸어서 오를 수 있게 했습니다.
- 높은 능선은 5m 높이로 만들고 지면 스폰 두 곳에서 떨어뜨렸습니다.

## 실제 검증

- Unity 6000.6.7f1 Windows 빌드 성공. `VOXSKY_BUILD_PASS`가 기록됐습니다.
- `--environment-smoke --outdoor`: 캐니언 경계, 스폰 충돌, 4.7m 이상 능선, 도보 등반, 자연석 충돌 검사가 통과했습니다.
- 전체 환경 검사는 27개 중 캐니언 관련 검사가 통과했습니다. WISP 모델의 정점 포락선 검사 1개가 실패해 전체 종료 코드는 2입니다. 이 검사는 능선 변경과 별도 모델 메시를 검사합니다.
- 빌드 전후 Arena 씬과 PD 밸런스 JSON의 SHA-256이 동일했습니다.
- 실제 캡처: `Docs/Screenshots/Canyon-Terrain-Review-20261108/Canyon-overview.png` 및 `Canyon-cover.png`.

로그: `Logs/Canyon-Ridge-Build-20261108.log`, `Logs/Canyon-Environment-Smoke-20261108.log`.
