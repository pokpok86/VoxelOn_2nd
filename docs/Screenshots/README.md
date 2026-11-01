# VoxSky 게임 화면 갤러리

Unity Windows 빌드에서 실제 게임 카메라로 캡처한 화면입니다. 콘셉트 아트가 아니라 현재 기체 모델과 전장을 렌더링합니다.

## 기체별 4방향

각 기체 이미지는 앞(front), 뒤(back), 왼쪽(left), 오른쪽(right) 순서입니다. 기체는 정면(+Z)을 향하고, 같은 거리와 카메라 구도를 사용합니다.

| 기체 | 화면 |
|---|---|
| Kestrel | [앞](Robots/01-kestrel-front.png) · [뒤](Robots/01-kestrel-back.png) · [왼쪽](Robots/01-kestrel-left.png) · [오른쪽](Robots/01-kestrel-right.png) |
| Bastion | [앞](Robots/02-bastion-front.png) · [뒤](Robots/02-bastion-back.png) · [왼쪽](Robots/02-bastion-left.png) · [오른쪽](Robots/02-bastion-right.png) |
| Wisp | [앞](Robots/03-wisp-front.png) · [뒤](Robots/03-wisp-back.png) · [왼쪽](Robots/03-wisp-left.png) · [오른쪽](Robots/03-wisp-right.png) |
| Nomad | [앞](Robots/04-nomad-front.png) · [뒤](Robots/04-nomad-back.png) · [왼쪽](Robots/04-nomad-left.png) · [오른쪽](Robots/04-nomad-right.png) |
| Scatter | [앞](Robots/05-scatter-front.png) · [뒤](Robots/05-scatter-back.png) · [왼쪽](Robots/05-scatter-left.png) · [오른쪽](Robots/05-scatter-right.png) |
| Lancer | [앞](Robots/06-lancer-front.png) · [뒤](Robots/06-lancer-back.png) · [왼쪽](Robots/06-lancer-left.png) · [오른쪽](Robots/06-lancer-right.png) |
| Tempest | [앞](Robots/07-tempest-front.png) · [뒤](Robots/07-tempest-back.png) · [왼쪽](Robots/07-tempest-left.png) · [오른쪽](Robots/07-tempest-right.png) |
| Anvil | [앞](Robots/08-anvil-front.png) · [뒤](Robots/08-anvil-back.png) · [왼쪽](Robots/08-anvil-left.png) · [오른쪽](Robots/08-anvil-right.png) |

## 전장 배경

카메라가 지도 중심을 향하도록 네 방향에서 촬영했습니다.

- [북쪽 구역](Arena/arena-north-sector.png)
- [동쪽 구역](Arena/arena-east-sector.png)
- [남쪽 구역](Arena/arena-south-sector.png)
- [서쪽 구역](Arena/arena-west-sector.png)

## 다시 촬영하기

프로젝트 루트에서 Windows 빌드를 실행해 다음 옵션을 전달하면 이 폴더의 이미지가 다시 생성됩니다.

```powershell
Start-Process -FilePath .\Builds\Windows\VoxSky.exe -ArgumentList @('--capture-docs-gallery') -WorkingDirectory (Get-Location).Path -Wait
```

이미지를 갱신한 뒤에는 `Docs/Screenshots`를 게임 코드·문서 변경과 함께 커밋에 포함합니다. 폴더에 기록된 PNG는 GitHub와 원격 대화방에서도 바로 미리 볼 수 있습니다.
