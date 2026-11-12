# WISP BD-01 compact body and shoulder module

Standalone torso/shoulder module. It does not include a head, upper arms, forearm weapons, pelvis/legs, or a complete WISP assembly.

## Review files

- `BD01.blend`: editable Blender 5.2.3 scene; metric units, Z up, front toward -Y
- `BD01-01-Front.png` through `BD01-05-45deg.png`: front, right, back, left, and three-quarter review views
- `BD01-06-Connections-Closeup.png`: clean shoulder/waist connection close-up
- `BD01-07-Connection-Axes.png`: the same connection area with the actual pivot axes and centers marked
- `BD01-Concept-Comparison.png`: supplied BD-01 sheet beside the generated 45-degree view
- `BD01-Review-Phone.png`: portrait review board for mobile viewing
- `build_bd01.py`: rebuilds the module, all Blender renders, and JSON manifest
- `make_bd01_review.ps1`: generates the annotated pivot image and comparison boards
- `manifest.json`: dimensions, joint centers, render names, and automated checks

## Shape and attachment points

The chest has an open lower outline: its lower cheek plates flank a recessed, tapered dark abdomen. A small center chest plate and restrained orange inset follow the supplied concept. Angled shoulder caps wrap around the cylindrical joint, and the short upper-arm connectors now point downward. The bottom waist interface was shortened. A subtle procedural material normal adds surface grain without extra geometric decoration.

The central chest plate now has an 18 mm forward ridge, forming two broad angled faces. The lower chest cheek plates also project more strongly to make the surrounding chest volume readable. The subsequent PD-requested size correction below scales this chest shaping together with the body.

Current bounds and pivot centers are stored in `manifest.json` and named Blender empties. The main torso core is about 0.350 m wide. The generator temporarily rotates each shoulder and the waist interface, checks that the connector moves while the waist rotates in place, and restores the neutral pose before rendering. Visual resemblance remains a review task; generation success does not approve the design.

## Rebuild and validation

From the project root:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' --background --python Docs/Design/WISP_Base_v1/BD01/build_bd01.py
& Docs/Design/WISP_Base_v1/BD01/make_bd01_review.ps1
```

Blender generation and the pivot motion checks passed with Blender 5.2.3 LTS. The renders were opened and visually inspected. **Design approval remains pending PD review.** No Unity import or full-robot edits were made.


## PD body size correction — 2026-11-12

PD reported that the body looked too large in assembled previews. The BD01 root is now uniformly 0.82 of its prior size, including shoulder caps and upper-arm stubs. Measured overall bounds are 0.602 × 0.232 × 0.302 m. Neck, waist and elbow mating pin dimensions are preserved; their positions follow the smaller body.

Head, leg, standalone weapon and backpack geometry are unchanged. Assembly positions are derived from the saved interface metadata rather than the former fixed offsets. See `../../WISP_Parts/Body-Size-Revision.md` for validation.

`BD01-Size-Comparison-Phone.png` compares before and after with the same camera, framing and weapon scale. This is a size candidate for PD review, not a declaration of design approval.


## 추가 PD 비율 수정 — 2026-11-12

몸통/어깨 루트를 초기 크기의 0.70배로 조정했습니다. 직전 0.82배 버전 대비 약 14.6% 축소입니다. 목 받침도 낮췄습니다. 최신 외곽은 0.514 × 0.198 × 0.239 m입니다. 최신 조립 검토 결과는 `../../WISP_Parts/Proportion-Revision-02.md`를 참고하세요.


최신 상박 조정: 상박 길이만 1.5배(0.0672→0.1008 m) 늘리고 팔꿈치 핀을 함께 이동했습니다. 폭·깊이와 어깨 상단, 다른 몸통 형상은 유지했습니다. `../../WISP_Parts/UpperArm-150-Weapon-90-Revision.md` 참고.


최신 몸통 수정: 몸통/어깨 외장만 직전의 90%로 줄였습니다. 상박 길이·굵기와 팔꿈치 접속 치수는 보정하여 유지합니다. `../../WISP_Parts/Body-90-Revision.md` 참고.
