# WISP HD-01 sensor head

Standalone small sensor-head module only. It contains no torso, shoulder parts, arms, forearm cannon, or legs.

## Deliverables

- `HD01.blend`: editable Blender model
- `HD01-01-Front.png` through `HD01-05-45deg.png`: front, right, back, left, and three-quarter views
- `HD01-06-Top.png`: top view for shell depth and antenna placement
- `HD01-Concept-Comparison.png`: supplied head guide beside the generated head
- `HD01-Review-Phone.png`: portrait review board
- `HD01-Design-Guide.jpg`, `WISP-Reference-Concept.png`: source references
- `build_hd01.py`: repeatable Blender generator and render setup
- `make_hd01_review.ps1`: creates review boards
- `manifest.json`: dimensions and generation scope

## Design

The latest full-body concept is the reference for this revision. A single rectangular cyan sensor sits within a shallow dark bezel. The shell has clipped brow and cheek planes, dark rear-side sensor pods and a short flattened antenna. The rectangular sensor replaces the previous horizontal strip to match the newly supplied reference. A small fixed mount is included at the bottom.

Coordinates use meters, Z-up, facing -Y. Current measured dimensions are recorded in `manifest.json`; the bounds include the antenna and fixed connector. The render lighting was lowered to preserve material color, and a subtle procedural surface normal was added.

The face adjustments narrow the foremost shell section by about 13% relative to the initial concept-refinement pass and taper the lower cheeks by up to 14%. The rectangular sensor, head height, fore/aft depth, and fixed neck interface retain their previous dimensions. Side pods remain seated against the shell.

## Rebuild

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python Docs/Design/WISP_Base_v1/HD01/build_hd01.py
& Docs/Design/WISP_Base_v1/HD01/make_hd01_review.ps1
```

Blender generation completed with Blender 5.2.3 LTS and all generated views were visually inspected. **Design approval remains pending PD review.** No Unity import or whole-robot edit was made.


## 추가 PD 비율 수정 — 2026-11-12

머리 외형은 유지하고 아래 고정 연결부의 높이를 18 mm에서 6 mm로 줄였습니다. 몸통의 목 받침도 함께 낮춰 떠 보이는 간격을 줄였습니다. 최신 조립 검토 결과는 `../../WISP_Parts/Proportion-Revision-02.md`를 참고하세요.


최신 머리 수정: 외장 전체 10% 축소 후 양옆 폭을 추가 10% 축소했습니다(폭 81%, 앞뒤/높이 90%). 카메라 창·발광부·테두리 크기와 목 접속부는 유지했습니다. `../../WISP_Parts/Head-90-Width-90-Revision.md` 참고.
