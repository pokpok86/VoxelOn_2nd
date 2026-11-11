# WISP Base Blender experiment — 2026-11-11

## Status
- PD asked to make the attached WISP four-view concept the visual target, beginning with the base pose and shoulder silhouette.
- The source package was copied from GitHub commit `3c15b3b` into `Docs/Design/WISP_Base_v1/`; it is preserved separately from the existing gameplay work.
- Blender 5.2.3 LTS was available and the one-command local runner successfully generated three editable `.blend` candidates and four views for each. Automation passed; design approval remains pending PD review.
- The first comparison was visually inspected. It is still a low-poly blockout and not close enough to call finished. The next iteration should focus on silhouette and joint posture before adding detail.

## Latest silhouette passes
- A second generated set replaces soft shoulder boxes with three faceted profiles: `NARROW_WEDGE`, `TRAPEZOID`, and `LOW_SWEPT`. The pass also opens the arm-cannon hang slightly. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Shoulder-Profiles-20261111-002955.png`.
- A third set tests smaller head/sensor proportions alongside those shoulder profiles. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Silhouette-Refine-20261111-003254.png`.
- Each set produced three editable `.blend` files and 12 PNG renders. The actual comparison images were inspected after rendering. The shoulders are smaller and less box-like than the prior set, but the body remains a simple blockout and is not yet visually equivalent to the supplied concept. Design approval remains with PD.

| Latest candidate | Shoulder profile / size (m) | Head size (m) | Antenna height (m) |
| --- | --- | --- | ---: |
| A_WEDGE | Narrow wedge / 0.14 × 0.145 × 0.11 | 0.13 × 0.12 × 0.10 | 0.075 |
| B_TRAPEZOID | Trapezoid / 0.165 × 0.15 × 0.12 | 0.14 × 0.125 × 0.105 | 0.085 |
| C_SWEPT | Low swept / 0.18 × 0.145 × 0.125 | 0.145 × 0.13 × 0.11 | 0.09 |

## Shoulder / neutral-pose choices
- A fourth set directly compares the design note's no-armor base against a micro wedge and a swept cap, with a looser-to-tighter arm hang. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Shoulder-Pose-Choices-20261111-003648.png`.
- `A_BARE_JOINT`: no shoulder cap, arm spread 28°, elbow bend −50°.
- `B_MICRO_WEDGE`: 0.13 × 0.14 × 0.10 m cap, arm spread 25°, elbow bend −45°.
- `C_SWEPT_CAP`: 0.18 × 0.15 × 0.12 m cap, arm spread 22°, elbow bend −40°.
- All three retain the smaller-head settings from the preceding pass. Blender generated 3 editable files and 12 views; the contact sheet was inspected. This remains a blockout and needs PD's visual direction before detailing.

## Torso size comparison
- PD noted that the torso still looked too large beside the supplied concept. The next set holds head, shoulders, arms and legs constant and scales the central core, chest and pelvis together, so the body-size difference is easy to judge.
- `A_COMPACT_CORE`: torso core 0.32 × 0.19 × 0.34 m; chest 0.36 × 0.14 × 0.20 m; pelvis 0.27 × 0.18 × 0.14 m.
- `B_BALANCED_CORE`: core 0.35 × 0.205 × 0.37 m; chest 0.395 × 0.15 × 0.215 m; pelvis 0.29 × 0.195 × 0.145 m.
- `C_LIGHT_CORE`: core 0.385 × 0.23 × 0.40 m; chest 0.44 × 0.165 × 0.23 m; pelvis 0.32 × 0.21 × 0.157 m.
- Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Body-Proportions-20261111-005832.png`. Blender generated three editable files and four views per candidate; the comparison image was inspected. The smaller bodies are easier to distinguish, but the chest is still a boxy blockout and needs further silhouette work.

## Deeper head, shaped chest and shoulder link
- After PD noted the head looked too shallow, the body was boxy and the shoulder connector was missing, the generator gained a deeper head, faceted/tapered chest profiles, shaped forearm and leg shells, and a visible two-strut shoulder link from the torso socket to the shoulder axle.
- Candidate head depth (front-to-back): `A_TAPERED` 0.18 m, `B_VEST` 0.20 m, `C_FLARED` 0.22 m. Chest profiles are `TAPERED`, `SOFT_VEST`, and `FLARED_TOP`.
- Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Head-Chest-Shoulder-Links-20261111-010813.png`.
- Blender generated three editable files and 12 views. The generated front, side, back and 45-degree images were inspected; the connector and chest contour are visible. The overall model is still a procedural blockout and requires further PD review against the supplied image.

## Shoulder axis follows torso width
- PD pointed out that shrinking the torso without moving the shoulder axis left an oversized gap. The shoulder axle now derives from the selected chest width (`chest width / 2 + 0.07 m`); the body socket and both connecting struts are derived from the same width and axle.
- In the latest trio, chest widths 0.36 / 0.385 / 0.41 m produce shoulder-axis offsets 0.25 / 0.2625 / 0.275 m. The three editable files and 12 renders all generated successfully.
- Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Torso-Shoulder-Axis-20261111-011237.png`. The actual sheet was inspected. The axis and torso scale now move together; visual acceptance still belongs to PD.

## Part form and surface pass
- Following PD's direction, removed the chest stripe/vents, cannon rails and trim, shin plates, toe accents and backpack status boxes from this blockout. Large armor pieces now use tapered multi-face shells with rounded transitions, and the finish uses subtle ceramic/gunmetal roughness and surface texture.
- Candidates `A_LEAN_FORM`, `B_SOFT_VEST`, and `C_CHISELED_FORM` vary the chest/limb silhouette, while keeping the chest-derived shoulder axis and visible connector.
- Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Form-and-Material-20261111-011730.png`. The Blender-generated 3-by-4 comparison was visually inspected. It reads more as shaped armor than bolt-on detail, though it is still a blockout and not yet a match for the concept-art finish.

## Short neck, sensor face and integrated muzzle
- PD noted that the neck must join the torso, remain slim/short under the small head, and the hand replacement must read clearly as a gun muzzle. The current pass lowers the head, replaces the box head with a faceted sensor shell and oval cyan lens, and sizes a tapered neck link into the torso top.
- Head front-to-back depth varies 0.18 / 0.20 / 0.22 m; neck radii are 0.028 / 0.031 / 0.034 m. The computed neck bridges from the core top to the head underside instead of floating between separate parts.
- The forearm cannon now ends in a forward-facing rounded collar, dark bore and cyan emitter at the end of the armor shell. No hand or detached wrist box is present.
- Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Neck-Face-Muzzle-20261111-012638.png`. The three editable models and four views each were rendered and inspected. Each manifest reports automation pass, design review pending PD, and no bolt-on line geometry.

## Current comparison candidates
The runner compares progressively fuller shoulder caps while keeping the approved small head, forearm cannons, short-leg intent, and neutral pose:

| Candidate | Shoulder size (m) | Arm spread | Elbow bend | Leg spread |
| --- | --- | ---: | ---: | ---: |
| A_SLIM | 0.19 × 0.19 × 0.16 | 21° | −40° | 10° |
| B_REFERENCE | 0.215 × 0.20 × 0.17 | 19° | −38° | 8° |
| C_SOFT | 0.235 × 0.215 × 0.18 | 17° | −35° | 7° |

- Comparison image: `Docs/Design/WISP_Base_v1/runs/WISP-3-Candidates-20261111-001949.png`.
- Each candidate folder contains `WISP_Base_v2.blend`, four 1200×1200 PNGs, a manifest and both Blender logs.
- The original reference generator is preserved as `wisp_blender_v2.py`; automation, camera setup and rendering are in `wisp_automate.py` and `run_wisp.ps1`.
- Run from the repository root with `powershell -NoProfile -ExecutionPolicy Bypass -File .\Docs\Design\WISP_Base_v1\run_wisp.ps1`.

## Guardrails
- The manifests report `automation: PASS` separately from `design_review: PENDING_PD_VISUAL_REVIEW` for all generated candidates.
- No model was imported into Unity. `Assets/Scenes/Arena.unity` and `Data/Balance/PD-Balance.json` were not touched.
- These local results are not committed or pushed. Keep refining the silhouette and offer choices for PD to select; do not treat a generated image as final design approval.

## Sensor helmet and face-form comparison — 2026-11-11
- Compared three new head silhouettes: `ANGULAR_SENSOR`, `RAISED_BROW`, and `DEEP_WEDGE`. Each uses different front/mid/rear contours to avoid a plain extruded box; the face aperture carries a paired optic layout instead of one horizontal lens.
- Kept the prior compact torso, chest-derived shoulder axes/connectors, short-neck setting, and integrated forward muzzle values so the review isolates the head form while retaining the assembly context.
- Automation completed with Blender 5.2.3: three editable `.blend` files, twelve 1200×1200 PNG renders, per-candidate logs and manifests, and the comparison sheet `Docs/Design/WISP_Base_v1/runs/WISP-3-Sensor-Helmet-Forms-20261111-013313.png`. All manifests record `automation: PASS` and `design_review: PENDING_PD_VISUAL_REVIEW`.
- I opened and inspected both the contact sheet and a full-size front render. The paired optics and tapered crown are visible, but the head still reads too small relative to the exposed neck, while the shoulder/forearm mass remains dominant compared with the concept. No design-success claim is made; the next proportion pass should shorten the exposed neck and rebalance head against shoulder/arm mass before adding detail.
- Unity import was not performed. Gameplay and PD balance files remain untouched.

## Head, neck, and forearm balance pass — 2026-11-11
- After inspecting the sensor-head contact sheet, the exposed neck still looked too long and the forearm mass dominated the small head. This pass kept the three sculpted helmet profiles but compared shorter neck bridges, slightly fuller/deeper heads, thinner forearm shells, and muzzle positions derived from each forearm's depth and height.
- Candidates: `A_SHORT_NECK` (0.15 × 0.205 × 0.13 m head, 0.20 × 0.19 × 0.34 m forearms), `B_SWEPT_BROW` (0.16 × 0.225 × 0.14 m head, 0.21 × 0.20 × 0.355 m forearms), and `C_BALANCED_HELMET` (0.17 × 0.24 × 0.15 m head, 0.22 × 0.21 × 0.37 m forearms). Exposed neck radii were reduced to 0.017–0.021 m.
- Blender 5.2.3 generated three editable `.blend` models, twelve 1200×1200 renders, logs, and manifests. The comparison sheet is `Docs/Design/WISP_Base_v1/runs/WISP-3-Head-Neck-Arm-Balance-20261111-020152.png`; automation is PASS and design review remains pending PD.
- The contact sheet was opened and visually inspected. The neck is visibly shorter and the forearms smaller than the previous set, but the head still reads small beside the shoulder span, and the antenna appears detached in these renders. Record those as next silhouette/attachment issues; no design-success claim is made.
- No Unity import was performed; gameplay and PD balance files remain untouched.

## Helmet crown attachment and neck continuation — 2026-11-11
- The previous sheet showed the antenna floating above the shell. Replaced the detached rectangular stalk with a short angled stem rooted at the crown, and added three distinct helmet contour profiles: `CROWNED_CHEEK`, `SWEPT_COCKPIT`, and `SHARK_FIN_HELMET`.
- Compared three head/neck/muzzle proportions with the short neck retained: A head 0.165 × 0.22 × 0.14 m, B 0.175 × 0.24 × 0.15 m, C 0.18 × 0.25 × 0.155 m; neck radii 0.014–0.018 m. The forearm muzzle location is now derived from each forearm shell's depth and height, so the reduced shell still visibly joins its gun barrel.
- One Blender launch first exposed an indentation error in the new paired-lens loop. The error was captured in that failed run's log, corrected, and the set was rerun. The successful Blender 5.2.3 run generated three editable `.blend` models, twelve 1200×1200 PNG views, logs, manifests, and comparison sheet `Docs/Design/WISP_Base_v1/runs/WISP-3-Helmet-Crown-Attachment-20261111-023205.png`.
- I opened the comparison and full-size front render. The antenna now touches the helmet crown and the head/neck join is more compact; the head still reads like a small sensor pod, and the broad arm silhouettes remain unlike the concept proportions. Design remains pending PD review.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Short waist with compact legs — 2026-11-11
- Responding to the elongated waist in the compact-leg candidate, held its hip height and leg scale fixed and compared torso heights of 0.50, 0.48, and 0.46 m. The lower torso edge remains joined to the pelvis; the shoulder, arm, head, neck, and backpack socket all shift with the torso top so shortening the waist does not leave detached parts.
- Blender 5.2.3 produced three editable `.blend` files and twelve 1200×1200 four-view renders. Comparison: `Docs/Design/WISP_Base_v1/runs/WISP-3-Short-Waist-20261111-071905.png`. All three runs have automation `PASS`; design review remains `PENDING_PD_VISUAL_REVIEW`.
- Opened the comparison and the full-size C front render. The waist is shorter than the previous C stance, and the shoulder/head cluster follows it while the feet stay grounded. C is the shortest option, though the model remains a blockout and is not being marked as a design match.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Shorter grounded leg-proportion study — 2026-11-11
- Compared three lower-body proportions by lowering the hip anchor and scaling thigh, shin, ankle, and foot lengths together. For each candidate, extended the shaped torso only enough to meet the repositioned pelvis; head, arms, shoulders, and chest width were held constant.
- Candidate A uses hip z 0.58 m and 0.84 leg scale; B uses 0.56 m and 0.81; C uses 0.54 m and 0.78. Calculated foot bottoms are approximately 4.6–6.0 mm above ground, so the compacted feet remain grounded in the review pose.
- Blender 5.2.3 produced three editable `.blend` files, twelve 1200×1200 four-view renders, logs, and manifests. Comparison: `Docs/Design/WISP_Base_v1/runs/WISP-3-Short-Leg-Stance-20261111-070336.png`. Every manifest separates automation pass from design review pending PD.
- Opened the comparison and C's full-size front render. The legs are visibly shorter, especially C, and the feet remain on the floor. The torso now occupies more height to close the waist-to-chest join; the result still reads as a simple blockout with a large flat chest and is not yet close enough to claim design success. This is a proportion option for PD review, not an approved design.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Extra-short waist follow-up — 2026-11-11
- The requested follow-up keeps the compact C leg/hip dimensions fixed and compares torso heights of 0.40, 0.36, and 0.32 m. The torso bottom stays joined to the pelvis; upper-body parts move down with the torso top.
- Blender 5.2.3 generated three editable `.blend` models, twelve 1200×1200 views, logs, and manifests. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Extra-Short-Waist-20261111-072225.png`. All automation checks passed and visual design review remains pending PD.
- Opened the contact sheet and C's full-size front render. C is markedly shorter through the torso while retaining the compact legs and grounded feet. The very compressed body may be too short, so this remains a visual option rather than design approval.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Sculpted visor and helmet-face comparison — 2026-11-11
- Continued from the crown-attachment pass with three faceplate profiles: `VISOR_CHEVRON`, `VISOR_WRAP`, and `VISOR_SWEEP`. Each sits inside a different sculpted helmet shell; the dark opening is larger and the two cyan emitters are smaller so they read as sensors inside one visor rather than a pair of cartoon eyes.
- Kept the crown-rooted angled antenna and short neck, and retained the three previous head proportions for a direct comparison of head silhouettes and faceplate shapes.
- Blender 5.2.3 generated three editable `.blend` files, twelve 1200×1200 PNGs, logs, manifests, and `Docs/Design/WISP_Base_v1/runs/WISP-3-Sculpted-Visors-20261111-030106.png`. All manifests separate automation pass from PD design review pending.
- The contact sheet and full-size front render were opened. The visor reads as one faceted dark opening with two smaller optics, and the antenna is visibly anchored. The full figure still differs strongly from the concept: it reads as a segmented blockout, with arm volume and torso silhouette needing broader structural redesign. This iteration is a face-shape comparison, not design approval.
- No Unity import was performed; gameplay and PD balance files remain untouched.

## Integrated forearm cannon volume comparison — 2026-11-11
- The previous comparison showed the forearms still looked like constant-depth plates. Added a ring-built shell generator with three tapered volume profiles: `TAPERED_BARREL`, `OVAL_CANNON`, and `SLENDER_LANCE`. Each profile reshapes the shell itself while the forward collar, bore, and emitter remain aligned from the shell's dimensions.
- Blender 5.2.3 produced three editable `.blend` models, twelve 1200×1200 views, logs, and manifests. Comparison sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Integrated-Cannon-Silhouettes-20261111-033147.png`. Automation passed; visual design approval remains pending PD.
- The comparison sheet and A full-size front render were opened. The new forearms have rounded/tapered volume and retain forward-facing muzzles, but A reads too much like a capsule and all candidates still have a segmented blockout torso/limb structure unlike the concept. The next pass should combine a shaped, soft-edged armor shell with a flatter front silhouette, and address shoulder-to-arm continuity. No design approval is implied.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Soft-edged armor and flatter cannon faces — 2026-11-11
- To address the capsule-like appearance in the preceding forearm pass, replaced the circular cross-section with superellipse variants that produce flatter front planes and softened corners: `SOFT_RECT_CANNON`, `BEVELED_D_CANNON`, and `SWEPT_CANNON`.
- Removed the separate thin upper-arm plate so the arm silhouette comes from the main shells and joint structure. Kept each forward muzzle aligned to the reshaped shell profile.
- Blender 5.2.3 generated three editable `.blend` files, twelve 1200×1200 renders, logs and manifests. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Soft-Armor-Cannon-Profiles-20261111-040123.png`. All generated runs report automation pass and design review pending PD.
- I opened the comparison sheet. The forearms are less capsule-like and the upper-arm slab is gone. The full body still reads as a segmented blockout; this is a bounded armor-form comparison, not a claim that the concept match is complete.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Continuous torso silhouette comparison — 2026-11-11
- Replaced the three overlapping torso surfaces (core, chest, abdomen) with one continuous waist-to-chest mesh. Its cross-section varies up the body to form a narrow waist and shaped chest; front ceramic and dark side/frame regions are materials on that same shell, not added plates.
- Compared `VEST_TAPER`, `WAIST_SCULPT`, and `CORE_FLARE` profiles with chest widths 0.34, 0.36 and 0.38 m. Shoulder axes and sockets remain derived from chest width, so the axis offsets move with each body (0.24, 0.25 and 0.26 m).
- Blender 5.2.3 generated three editable `.blend` files and twelve four-direction renders. Comparison image: `Docs/Design/WISP_Base_v1/runs/WISP-3-Continuous-Torso-Profiles-20261111-043259.png`. All manifests report automation pass and design review pending PD.
- The comparison image was opened and inspected. The chest/core/abdomen seams are gone and the torso reads as a continuous shell; the overall model remains a blockout and still differs from the concept. Design success is not claimed.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Shoulder-cap shell and linked-axis comparison — 2026-11-11
- Rebuilt the shoulder armor surface as a three-section shell through its depth rather than a flat extruded polygon. Compared swept, narrow-wedge, and trapezoid silhouettes at 0.135–0.150 m sizes while holding the continuous torso, arms, and head constant.
- Kept the shoulder axis and torso socket derived from the same chest width (0.36 m; axis offset 0.25 m). This confirms the attachment position stays aligned while the cap profile changes.
- Blender 5.2.3 generated three editable `.blend` files, twelve four-view PNGs, logs and manifests. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Shoulder-Cap-Silhouettes-20261111-050247.png`. Automation passed and design review remains pending PD.
- I opened the comparison sheet and a full-size front render. The cap shells have a more rounded crown with sloped sides, but the broad torso and forearm volumes still dominate and the overall model remains far from the concept. These are shoulder-shape options only; no design-success claim is made.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Compact torso and derived shoulder-axis study — 2026-11-11
- Compared torso widths 0.30, 0.32 and 0.34 m, paired with heights 0.36, 0.39 and 0.42 m and matching narrowed pelvis widths. Head and arms were held constant so the body-size change is visible.
- Shoulder axis offsets were recalculated from each chest width: 0.22, 0.23 and 0.24 m; body sockets follow the corresponding half-width plus 0.004 m. No scene or runtime data was changed.
- Blender 5.2.3 generated three editable `.blend` models, twelve 1200×1200 renders, logs and manifests. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Compact-Torso-Axis-Study-20261111-053040.png`. Automation passed and design review remains pending PD.
- The contact sheet was opened and visually inspected. The narrowest torso has visibly less mass, with the shoulder/arm mounts following inward; however, the head still looks small beside the arms and the whole model remains a blockout, not a design match.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Sculpted pelvis and torso join — 2026-11-11
- Replaced the beveled-box pelvis and exposed cylindrical waist bearing with one ring-built, shaped hip shell. The waist yaw pivot remains in the hierarchy, while the pelvis surface overlaps the lower torso shell so the joint area no longer reads as a floating stack of boxes.
- Compared `FLARED_HIP`, `TAPERED_HIP`, and `CRESTED_HIP` with the same torso and shoulder setup. The 0.36 m chest keeps the shoulder axis derived at 0.25 m for all candidates.
- Blender 5.2.3 generated three editable `.blend` models, twelve four-view renders, logs and manifests. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Sculpted-Pelvis-Profiles-20261111-060150.png`. Automation passed; design review remains pending PD.
- I opened the comparison sheet and A full-size front render. The waist gap and box-like pelvis are gone and the leg sockets remain exposed, but the light front hip surface looks broad and still needs proportion review against the concept. No design-success claim is made.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Narrow hip and reduced front armor comparison — 2026-11-11
- Followed the previous sheet's oversized bright pelvis face with three narrower hip shells (0.23, 0.25 and 0.27 m) and progressively larger front ceramic regions. The front armor boundary is now a per-candidate proportion, separate from the underlying pelvis silhouette.
- Kept the same 0.36 m torso and 0.25 m chest-derived shoulder axis, head, arms and leg roots to isolate the pelvis shape. The waist pivot remains structural while no full-width cylinder ring is shown.
- Blender 5.2.3 generated three editable `.blend` files, twelve four-view PNGs, logs and manifests. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-3-Compact-Hip-Proportions-20261111-063049.png`. Automation passed; design review pending PD.
- The comparison sheet was opened and inspected. The light hip face is smaller and the transition into the legs is clearer, but A exposes more of the dark hip structure and all candidates remain segmented blockouts rather than a match to the supplied concept.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Shorter shoulder connectors and continuous light belly — 2026-11-11
- Reduced the shoulder bridge span with three inset settings (0.020, 0.024, 0.028 m) while holding the latest compact-waist stance constant.
- Removed the dark material band from the entire front-facing torso surface; dark structural shading remains on the sides and back. Shoulder/torso connection geometry follows the shorter connector span.
- Blender 5.2.3 created three editable `.blend` models and twelve 1200×1200 renders. Comparison: `Docs/Design/WISP_Base_v1/runs/WISP-3-Short-Shoulder-Link-No-Black-Belly-20261111-072727.png`. All manifests report automation `PASS` and design review `PENDING_PD_VISUAL_REVIEW`.
- Opened the comparison sheet. The front belly is now one continuous light surface, and the shoulder links are shorter in each candidate. The shapes remain blockout forms; no design approval is implied.
- No Unity import was performed. Gameplay and PD balance files remain untouched.
## Half-width inverted shoulder cap — 2026-11-11
- Applied only the shoulder-cap edit requested: halved its lateral dimension from 0.14 m to 0.07 m and vertically inverted the existing low-swept trapezoid outline. Kept torso, belly material, connectors, head, arms, and legs at the latest compact-waist baseline.
- Blender 5.2.3 generated the editable `.blend`, four 1200×1200 views, logs, and manifest. Review sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Half-Width-Inverted-Shoulder-20261111-074010.png`. Automation passed; design review remains pending PD.
- Opened the four-view sheet. The shoulder cap is visibly much narrower, with the broad end now lower; no other silhouette proportions were intentionally changed.
- No Unity import was performed.
## Half-height inverted torso and slimmer upper-arm fit — 2026-11-11
- Applied the clarified torso change: retained the compact-waist lower-body stance, halved torso height from 0.32 to 0.16 m, and inverted the cross-section so its lower end is wider than its upper end. The front-facing torso material is continuous light armor; dark side/rear structural faces remain.
- Refit the upper-arm joints by aligning upper-arm, elbow, and forearm centerlines, shortened/thinned the upper-arm shell to 0.095 × 0.10 × 0.17 m, and reduced the elbow bend from 44° to 30°.
- Blender 5.2.3 generated one editable `.blend`, four 1200×1200 views, log, and manifest: `Docs/Design/WISP_Base_v1/runs/20261111-074551-HALF_INVERTED_TORSO`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Half-Height-Inverted-Torso-20261111-074604.png`.
- Opened the four-view sheet and full-size front render. The torso is now a short inverted wedge and the upper arms are thinner/shorter with aligned centerlines. This changes the silhouette substantially and remains pending PD review; it is not marked design-approved.
- No Unity import was performed.
## Direct shoulder-to-torso fit and narrower pelvis — 2026-11-11
- Restored torso height from 0.16 to 0.24 m while keeping its inverted silhouette. Reduced shoulder axis offset from 0.07 to 0.01 m and shoulder-cap width to 0.105 m; removed the visible body socket, joint ring, and connector rods so the cap meets the torso directly.
- Narrowed pelvis width from 0.25 to 0.20 m and brought hip, knee, shin, ankle, and foot centerlines inward. Kept the slimmer upper arms and their aligned centers.
- Blender 5.2.3 produced the editable `.blend`, four 1200×1200 images, log, and manifest. Comparison: `Docs/Design/WISP_Base_v1/runs/WISP-1-Direct-Shoulder-Narrow-Pelvis-20261111-075024.png`. Automation passed; design review remains pending PD.
- Opened the four-view sheet. The shoulder shells now sit against the torso without separate connector pieces, and the pelvis/leg roots are closer together. Design approval is not claimed.
- No Unity import was performed.
## Balanced proportions and shoulder contact follow-up — 2026-11-11
- Rebalanced only part sizes/placements from the previous extreme: restored torso height to 0.30 m and the milder existing waist profile, reduced head to 0.14 × 0.19 × 0.115 m, forearm cannon to 0.18 × 0.17 × 0.30 m, pelvis width to 0.18 m, and shoulder caps to 0.09 × 0.14 × 0.095 m.
- Corrected the shoulder attachment after visual inspection showed the first pass had sunk the caps behind the torso. Moved the caps outward and forward, then lowered them to overlap both torso and upper arms. The visible shoulder body socket, ring, and rods remain removed. Upper-arm centerlines stay aligned.
- Blender 5.2.3 generated an editable `.blend`, four 1200×1200 PNG views, logs and manifest in `Docs/Design/WISP_Base_v1/runs/20261111-075923-BALANCED_SILHOUETTE`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Balanced-Silhouette-20261111-075938.png`. Automation passed; design review remains pending PD.
- Opened the four-view sheet and front render. The shoulder caps now visibly overlap the torso/upper-arm seam instead of floating; the head, cannon and pelvis are smaller. Concept fidelity is still not claimed.
- No Unity import was performed.
## Shorter waist and aligned knee chain — 2026-11-11
- Reduced torso height from 0.30 to 0.24 m and narrowed chest width from 0.32 to 0.30 m, retaining the normal waist taper rather than the exaggerated inverted belly. The shoulder/head assembly follows the shortened torso; shoulders remain directly attached.
- Kept pelvis width at 0.18 m and placed hip, thigh, knee, shin, ankle, and foot centerlines on one shared lateral axis to prevent the knee chain from stepping sideways. Shortened upper arms again to 0.14 m and adjusted elbow position to match.
- Blender 5.2.3 generated an editable `.blend` and four 1200×1200 views in `Docs/Design/WISP_Base_v1/runs/20261111-080358-SHORT_WAIST_ALIGNED_KNEES`; contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Short-Waist-Aligned-Knees-20261111-080414.png`. Automation passed and design review remains pending PD.
- Opened the contact and front render. Torso and shoulder region are shorter/narrower; the knee components share the same lateral coordinates in the model. Visual design approval is not claimed.
- No Unity import was performed.
## Flush forearm tips, shorter torso and restored upper-arm length — 2026-11-11
- Removed the three protruding muzzle rings and muzzle socket from the forearm ends for this candidate, leaving the enlarged faceted forearm shells with flat tips. The generator keeps muzzle geometry optional and the automation validates that it is absent when disabled.
- Shortened the torso shell from 0.24 m to 0.20 m while moving its center down by 0.02 m, preserving the lower join to the pelvis. Increased upper-arm length from 0.14 m to 0.155 m after PD said the last shortening went too far. Kept the narrow torso/shoulder setup and larger forearms.
- Blender 5.2.3 created an editable `.blend` and front/right/back/45-degree renders in `Docs/Design/WISP_Base_v1/runs/20261111-081353-NO_MUZZLE_SHORT_TORSO_LONGER_UPPER_ARM`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-No-Muzzle-Short-Torso-Longer-Upper-Arm-20261111-081410.png`. Automation passed; design review remains pending PD.
- Opened the four-view sheet and the front render. The circular hand-like protrusion is gone, the torso is shorter, and the upper-arm shell is slightly longer. The flat forearm tips no longer show the muzzle aperture; PD should confirm whether this reads better or whether a flush inset opening is desired. The forearms still dominate the silhouette.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Preserved proportion, chest depth, articulated can joints, visor and knee armor — 2026-11-11
- Kept the selected torso length (0.20 m), width (0.25 m), pelvis (0.18 m) and limb dimensions unchanged. Increased only torso front-to-back depth from 0.19 m to 0.23 m, and added a distinct angular chest plate that projects forward and overlaps the abdomen surface.
- Added matching front-facing can-top shells at both shoulder and hip pivots. Each shoulder and hip retains separate pitch and abduction pivots so the joint pieces are carried by a two-axis hierarchy. Reduced the can-joint diameter after the first render made the circles too dominant.
- Replaced the two face lenses with one horizontal glass visor strip inside a beaked angular face aperture. Added fitted angular knee guards over the knee pivots. Forearm muzzle pieces remain disabled per PD request; forearm proportions remain unchanged.
- Blender 5.2.3 generated an editable `.blend` and four views in `Docs/Design/WISP_Base_v1/runs/20261111-082403-INSET_CAN_JOINTS_ANGULAR_ARMOR`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Inset-Can-Joints-Angular-Armor-20261111-082419.png`. Automation passed; design review remains pending PD.
- Opened the contact sheet, full-size front and side renders. The round can faces and matching hip joints are visible; the side view shows the added chest depth; the face now has one cyan slit and the knees have small armor shells. The chest plate still reads as a broad separate shell and forearms dominate the front silhouette; PD should judge whether these volumes suit the reference. No design approval is claimed.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Short neck, thinner can pivots, egg forearm cannons and compact head depth — 2026-11-11
- Kept the approved overall body and limb silhouette measurements. Shortened only the visible neck by embedding its stem farther into the torso and head shells. Reduced head depth from 0.15 m to 0.12 m while preserving head width and height.
- Cut shoulder and hip can-joint depth from 0.060 m to 0.032 m; the front circular lid and two-axis pivot hierarchy remain. The front and side views were opened to inspect both thickness and attachment.
- Changed the forearm from a uniform faceted shell to a subtly faceted egg profile: narrower at the elbow connection, gradually fuller toward the distal end, and gently tapered at the tip. Added a small flush dark recess and glass aperture instead of the previous protruding muzzle rings/socket.
- Blender 5.2.3 generated the editable `.blend` and four review images in `Docs/Design/WISP_Base_v1/runs/20261111-090234-SHORT_NECK_THIN_JOINT_EGG_CANNON`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Short-Neck-Thin-Joint-Egg-Cannon-20261111-090251.png`. Automation passed; design review remains pending PD.
- Opened the contact sheet plus full-size front and right views. The head is visibly shallower, the neck bridge shorter, and can-joint depth slimmer. The forearms now swell toward an egg-shaped lower end and have small forward apertures. PD should judge whether the muzzle still reads as an attached hand-like piece and whether the narrowed head still has enough concept character; no design approval is claimed.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Symmetric head/chest, shorter neck, thin elbow link and clean muzzle — 2026-11-11
- Corrected the previously asymmetric left/right vertex lists in the head sensor, beaked face aperture, chest overlay and knee guards. Centered the antenna on the head for a symmetric front silhouette. Core measurements and body ratios remain unchanged.
- Shortened the visible neck further by overlapping its stem into the torso and head shells. Reduced the elbow cylinder from 0.12 m to 0.055 m depth and from 0.060 m to 0.038 m radius so the upper/lower arm link is slimmer.
- Removed the dark forearm muzzle recess/holder; retained only a tiny flush cyan glass aperture on the expanding egg-shaped lower-arm end.
- Blender 5.2.3 generated an editable `.blend` plus front/right/back/45-degree views in `Docs/Design/WISP_Base_v1/runs/20261111-090828-SYMMETRIC_HEAD_SHORT_NECK_SLIM_ARM_LINKS`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Symmetric-Head-Short-Neck-Slim-Arm-Links-20261111-090845.png`. Automation passed and design review remains pending PD.
- Opened the contact sheet and full-size front/right renders. Geometry is now mirrored across the centerline for the marked head and chest shell; the front light remains slightly asymmetric, so shading still makes the chest side values differ. The neck bridge, shoulder/hip depth, and elbow connector are thinner; the dark muzzle hardware is gone. No design approval is claimed.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Balanced front lighting for symmetry review — 2026-11-11
- Kept the previously generated geometry and parameters unchanged. Replaced the uneven warm key/cool fill pair with matched front lights on the left and right and centered the rear rim light, so the front render does not imply a crooked mesh through unequal shading.
- Blender 5.2.3 saved the lighting-adjusted editable file and four views in `Docs/Design/WISP_Base_v1/runs/20261111-090954-SYMMETRIC_LIGHT_REVIEW`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Symmetric-Light-Review-20261111-091010.png`. Automation passed; design review remains pending PD.
- Opened front and right renders. The marked head and chest now read symmetrically under balanced lighting. The neck and arm link remain shortened/thinned, and the forearm has only the tiny flush cyan aperture with no dark holder. No design approval is claimed.

## Shoulder and hip joints as articulated ball/socket mechanisms — 2026-11-11
- Replaced the front-facing decorative discs with actual spherical balls inside torus socket housings. Shoulder sockets are parented to the upper-body pivot while balls follow each shoulder's pitch/abduction pivot; hip sockets are parented to the pelvis while balls follow each hip's pitch/abduction pivot. The same geometry/function is used for both joint locations.
- Blender automation now checks these parent relationships, so the balls are structurally attached to moving limb pivots rather than being surface ornaments. Overall body dimensions and contour were held unchanged.
- Blender 5.2.3 generated the editable `.blend` and four views in `Docs/Design/WISP_Base_v1/runs/20261111-091550-ARTICULATED_BALL_SOCKET_JOINTS`. Horizontal contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Articulated-Ball-Socket-Joints-20261111-091606.png`; phone portrait sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Articulated-Ball-Socket-Joints-Phone-20261111-091606.png`. Automation passed; design review remains pending PD.
- Opened front and side views. The spherical centers visibly protrude from their sockets and have separate moving/fixed parents. No Unity import was performed. Gameplay and PD balance files remain untouched.

## Cylindrical tuna-can axis joints — 2026-11-11
- Replaced the mistaken spherical balls with short cylindrical joint drums oriented front-to-back. Their circular front faces read like can lids; the side view shows the shallow can body. A smaller axle is fixed to the upper body or pelvis, while each drum is attached to its shoulder or hip abduction pivot.
- Automation asserts those exact parent links; it no longer requires any ball or spherical socket objects. Existing silhouette, body dimensions and other details were kept unchanged.
- Blender 5.2.3 generated an editable `.blend` and four views in `Docs/Design/WISP_Base_v1/runs/20261111-093251-TUNA_CAN_AXIS_JOINTS`. Contact sheet: `Docs/Design/WISP_Base_v1/runs/WISP-1-Tuna-Can-Axis-Joints-20261111-093306.png`; phone portrait: `Docs/Design/WISP_Base_v1/runs/WISP-1-Tuna-Can-Axis-Joints-Phone-20261111-093306.png`. Automation passed; design review remains pending PD.
- Opened front and side renders. The front face is circular and the side profile is cylindrical; the drum/axle are attached to separate moving and fixed pivots. PD should confirm this matches the intended tuna-can joint shape. No design approval is claimed.
- No Unity import was performed. Gameplay and PD balance files remain untouched.

## Shoulder and hip cans moved onto the actual body-side pivot centers — 2026-11-11
- Corrected the placement error called out by PD: the shoulder can and pivot had been at the torso's front face (`y=-0.135 m`), and the hip can/pivot at the pelvis front (`y=-0.085 m`). They now sit at the center depth of their actual body intersections: shoulder `y=-0.020 m` and hip `y=0.000 m`. Their X/Z positions stay at each limb's body-side articulation center.
- Added Blender automation checks that each can center equals its moving pivot and that the pivot coordinates match the torso/pelvis joint locations (not the front surface). The world-space centers are recorded in `manifest.json`.
- Blender 5.2.3 generated a new editable `.blend` and all four views in `Docs/Design/WISP_Base_v1/runs/20261111-093829-TUNA_CAN_AT_PIVOT`. Phone portrait sheet: `Docs/Design/WISP_Base_v1/runs/20261111-093829-TUNA_CAN_AT_PIVOT/WISP-1-Tuna-Can-At-Pivot-Phone.png`. Automation passed; design review remains pending PD.
- Opened front and side renders: the cans now occupy the torso/pelvis joint depth rather than the front surfaces. No Unity import or design approval was performed.

## Thick can joints, slim upper arms, shoulder armor removed — 2026-11-11
- Increased the can-joint drum radius to 0.052 m and its front-to-back depth to 0.085 m. Reduced the upper-arm section to 0.072 × 0.072 m (length 0.150 m), so the joint face and its side depth are both visibly larger than the upper-arm thickness.
- Set the shoulder armor profile to `NONE` and added an automation assertion that no `Shoulder_Armor_*` object remains. The cylindrical joint remains at the body-side pivot established in the previous pass.
- Blender 5.2.3 generated the editable `.blend` and front/right/back/45° images in `Docs/Design/WISP_Base_v1/runs/20261111-094442-THICK_CAN_SLIM_ARM_NO_SHOULDER_ARMOR`. Blender automation confirms the can cross-section is larger than the upper arm, the shoulder armor is absent, and the can center matches the pivot. The `.blend` reports a can diameter of 0.104 m and depth 0.085 m, compared with the upper arm's roughly 0.069 × 0.072 m section. Phone portrait sheet: `Docs/Design/WISP_Base_v1/runs/20261111-094442-THICK_CAN_SLIM_ARM_NO_SHOULDER_ARMOR/WISP-1-Phone-Review.png`.
- Opened and visually checked the front and side renders. Shoulder armor plates are gone; the thick cylindrical joints read as the main shoulder/hip connectors, with slimmer upper arms. Design review remains pending PD; no Unity import or design approval was performed.

## Bodyward shoulder pivot and slimmer upper arms/thighs — 2026-11-11
- Moved both shoulder pivots 12 mm inward from the chest side edge (X = ±0.113 m versus the 0.125 m chest half-width). The upper-arm anchors derive from that pivot, so the arm root moves inward with it rather than only shifting a decorative cap.
- Reduced the upper-arm section from 0.072 × 0.072 m to 0.066 × 0.066 m. Reduced each thigh from 0.130 × 0.145 m to 0.082 × 0.075 m. The shoulder and hip tuna-can joint dimensions remain 0.104 m face diameter × 0.085 m depth, larger in both visible measures than the adjoining arm/thigh sections.
- Blender automation now independently checks hip can versus thigh width/depth, shoulder can versus upper-arm width/depth, the 12 mm bodyward shoulder inset, both joint centers, and the absence of all `Shoulder_Armor_*` objects.
- Blender 5.2.3 generated `.blend` and four view renders in `Docs/Design/WISP_Base_v1/runs/20261111-095131-BODYWARD_SHOULDER_THICK_HIPS_SLIM_LIMBS`. The phone portrait sheet is `Docs/Design/WISP_Base_v1/runs/20261111-095131-BODYWARD_SHOULDER_THICK_HIPS_SLIM_LIMBS/WISP-1-Phone-Review.png`. Automation passed; design review remains pending PD.
- Opened the portrait sheet and checked all four views. Shoulder and hip cans visibly dominate the thinner limb connections; the old shoulder armor pieces are absent. No Unity import or design approval was performed.

## Rearward shift of the marked side-view joints — 2026-11-11
- Read the arrows in the side-view markup as a request to move toward the robot's back (+Y). Shifted shoulder, hip and knee pivots by 20 mm and moved the attached arm/leg sections by the same amount, preserving their connections rather than offsetting only the visible cylinders.
- Blender automation passed. It confirms shoulder and hip drums remain centered on their pivots, joint diameters/depths exceed the adjacent limbs, shoulder armor is absent, and the rearward shift is 0.020 m. Four renders, editable `.blend`, logs, manifest and portrait image are in `Docs/Design/WISP_Base_v1/runs/20261111-095619-REARWARD_JOINTS_20MM`.
- Opened the portrait sheet and checked all four views. In the side view the selected joint/limb assemblies sit farther toward the back. Design review remains pending PD; no Unity import or approval was performed.

## Actual centerline alignment of shoulder, elbow, hip and knee links — 2026-11-11
- PD was right that the previous rearward shift did not fix the root cause: the generator preserved each mesh's world transform while reparenting it. That baked offsets into child transforms, so visually nearby parts did not share the articulation frame.
- Rebuilt the limb links with explicit pivot-local transforms. The shoulder can and upper-arm root now share one axis; the elbow pivot is placed 0.180 m down that same arm frame; the forearm begins 0.170 m down from the elbow center, leaving a 5 mm overlap at its upper end. Hip/thigh, knee/shin and ankle/foot links are likewise positioned from their parent joint frames. Cylindrical joint centers remain the articulation origins.
- Blender 5.2.3 automation passed checks for local X/Y centerline alignment, exact cylinder-center/pivot-center equality, limb sections thinner than the can joints, and no legacy shoulder armor. Four renders and the editable `.blend` are in `Docs/Design/WISP_Base_v1/runs/20261111-102315-REARWARD_JOINTS_20MM`; phone sheet: `Docs/Design/WISP_Base_v1/runs/20261111-102315-REARWARD_JOINTS_20MM/WISP-1-Phone-Review.png`.
- Opened the portrait sheet and inspected all four views. The elbow/forearm and knee/shin connections now follow the actual joint frames. Design review remains pending PD. No Unity import was performed.

## Slightly smaller forearms and feet, shorter pelvis — 2026-11-11
- Reduced each forearm from 0.220 × 0.205 × 0.350 m to 0.200 × 0.190 × 0.320 m. The elbow-to-forearm overlap was retuned to 5 mm so the smaller cannon still meets the elbow pivot.
- Reduced each foot from 0.175 × 0.245 × 0.130 m to 0.160 × 0.220 × 0.120 m.
- Shortened the pelvis height from 0.220 m to 0.195 m while raising its center from 0.585 m to 0.5975 m. That keeps the pelvis top at 0.695 m, flush with the torso bottom, and trims the lower end without opening a gap at the waist.
- Blender 5.2.3 generated the editable `.blend` and four views at `Docs/Design/WISP_Base_v1/runs/20261111-103250-SMALLER_FOREARMS_FEET_SHORTER_PELVIS`. The manifest records the changed dimensions and confirms the articulation checks still pass. Portrait sheet: `Docs/Design/WISP_Base_v1/runs/20261111-103250-SMALLER_FOREARMS_FEET_SHORTER_PELVIS/WISP-1-Phone-Review.png`. Design review remains pending PD.
- Opened the phone sheet and checked all four views. The integrated forearms and feet read somewhat smaller, and the pelvis height is reduced without a waist seam gap. No Unity import or design approval was performed.

## Correction: shorten the marked abdomen armor, do not remove it — 2026-11-11
- PD clarified that the marked waist armor should be shorter, not deleted. The previous `REMOVE_ABDOMEN_ARMOR_BACK_FEET_SHOULDER_LINKS` preview interpreted that incorrectly by making the abdomen fully dark; that interpretation is superseded.
- Kept the ceramic waist/chest armor and reduced its vertical height ratio from 0.82 to 0.60 (the visible shell span is 0.1509 m to 0.1104 m). Kept the upper edge fixed, so the reduction trims the lower edge by about 40 mm instead of shifting the armor up or down.
- Moved each foot 30 mm rearward relative to the previous candidate (local Y offset −0.055 m to −0.025 m; the robot faces −Y). Kept ankle and foot attached on their existing centerline.
- Added a short torso-to-shoulder connector on each side. The shoulder pivots are 15 mm beyond the chest edge, and each connector bridges that gap to the can-joint center. The connector follows the upper-body breathing pivot; the can remains centered on the articulated shoulder pivot.
- Blender 5.2.3 produced the corrected editable model and front/right/back/45° renders in `Docs/Design/WISP_Base_v1/runs/20261111-111641-SHORTER_ABDOMEN_ARMOR_BACK_FEET_SHOULDER_LINKS`. The phone portrait review sheet is `WISP-1-Phone-Review.png`. Automation checks passed for a retained shortened armor shell, its fixed upper edge, both connector objects and endpoints, the 15 mm shoulder gap, 30 mm rearward foot offset, and articulation centerlines.
- Opened the portrait sheet and inspected all four views. The waist armor remains present but is shorter; the side view shows the short connector span and the feet remain shifted back. Automation success is separate from design approval; PD visual review remains pending. No Unity import was performed.

## Shorten the marked waist body section — 2026-11-11
- PD clarified the blue outline marks the full dark waist/body section under the chest. Shortened the torso core from 0.20 m to 0.16 m (20%) while keeping its top at 0.895 m. The chest armor remains intact and retains its previous 0.1104 m visible height.
- Raised the pelvis top by 40 mm to meet the shortened torso without a gap, keeping the pelvis bottom and hip pivot positions fixed. Shoulder links and the 30 mm rearward foot offset are unchanged.
- Blender 5.2.3 generated the editable file and four views in `Docs/Design/WISP_Base_v1/runs/20261111-112239-SHORTER_WAIST_TORSO_160MM`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed for the 0.16 m torso, continuous torso/pelvis join, retained chest armor, shoulder connections and aligned joints. Design review remains pending PD; no Unity import was performed.
- Opened the portrait sheet and visually checked the front, right, back and 45° views. The waist section is shorter while the chest armor, shoulder links and feet remain in place.


## Shorten the waist again — 2026-11-11
- The 0.16 m torso still read too long. Reduced the core to 0.12 m total height and raised its center to 0.835 m, keeping the chest top at 0.895 m. This is 40% shorter than the original 0.20 m torso.
- Raised the pelvis top and hip pivots 40 mm to shorten the visible chest-to-hip waist region; scaled the leg chain from 0.78 to 0.85 so the feet remain planted. The final pelvis top is 0.775 m, flush with the shortened torso bottom. Preserved chest armor at its existing 0.1104 m visible height.
- Blender 5.2.3 generated and checked the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-113512-SHORT_WAIST_RAISED_HIPS_120MM`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed; PD visual review remains pending. No Unity import.
- Opened the portrait sheet and checked the shortened dark waist section, chest plate, hip placement and grounded feet in the front, side, back and 45° views.


## Restore the waist; shorten the pelvis — 2026-11-11
- PD clarified the previous 0.12 m torso change shortened the wrong section. Restored the torso to the prior 0.16 m height and 0.815 m center; did not keep the extra waist compression.
- Shortened the pelvis from 0.275 m to 0.195 m (about 29%) while keeping its top flush with the restored torso bottom at 0.735 m. The pelvis bottom now meets the unchanged hip pivot at 0.54 m. Restored the earlier 0.78 leg scale, so the legs and feet return to the prior grounded proportions.
- Blender 5.2.3 generated and checked the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-113929-SHORTER_PELVIS_RESTORE_WAIST_160MM`. The portrait image is titled `WISP-1-Phone-Review.png`. Automation passed; design review remains with PD. No Unity import.
- Opened and visually checked the front, right, back and 45° views. The waist is restored; the pelvis shell is shorter and ends at the hip axis.


## Shorten the pelvis region the PD marked — 2026-11-11
- Confirmed the cyan outline means the lower-abdomen/pelvis shell between the waist and both hip joints. Kept the torso waist at its restored 0.16 m and shortened only the pelvis shell from 0.195 m to 0.150 m (about 23%).
- Kept the pelvis top at 0.735 m to meet the waist. Raised its center to 0.660 m, placing the shortened lower edge at 0.585 m; the hip cans centered at 0.540 m extend to 0.592 m, so the shell still overlaps the hip joint. Leg scale and grounded feet are unchanged.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-115049-SHORTER_PELVIS_CORE_150MM`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed for the reduced pelvis height, waist join, hip-joint overlap, and articulation alignment. Design review remains pending PD; no Unity import.
- Opened the portrait sheet and verified the central lower-abdomen shell is visibly shorter in the front and back views; side and 45° views also generated.


## Extend the pelvis to bridge both hip joints — 2026-11-11
- PD clarified that the shorter isolated center shell left the hip cans visually detached. Kept the 0.16 m waist, then extended the pelvis shell downward and widened it so it spans both hip-joint centerlines. Pelvis dimensions are now 0.28 m wide × 0.17 m deep × 0.24 m high; its top remains flush with the waist at 0.735 m and its lower edge reaches 0.495 m, overlapping the hip cans centered at 0.540 m.
- Blender automation now checks that the pelvis reaches below the hip centers into the can radius and that its tapered front width reaches both hip centerlines. Hip and leg positions/scales, foot placement and shoulder links were preserved.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-115508-PELVIS_BRIDGE_TO_HIP_JOINTS_240MM`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed; design approval remains pending PD. No Unity import.
- Opened the phone sheet and checked front/back width and the 45° and side relationship between pelvis and hip cans.


## Flatten the pelvis bridge to thigh depth — 2026-11-11
- PD clarified the long pelvis bridge should not bulge forward like a hanging belly. Kept its 0.28 m cross-body span and 0.24 m height, but reduced front-to-back depth from 0.17 m to 0.075 m to match the thigh section. Replaced the swollen tapered profile with a clipped, nearly constant-depth bridge profile.
- Hip centers, waist height, leg scale, and feet remain unchanged. Automation verifies the bridge reaches both hip centerlines, extends into the hip-can radius, meets the waist, and has 75 mm depth.
- Blender 5.2.3 generated the editable model and four renders in `Docs/Design/WISP_Base_v1/runs/20261111-115915-SLIM_HIP_BRIDGE_THIGH_DEPTH_75MM`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed; PD design approval remains pending. No Unity import.
- Opened the portrait sheet and checked front, side, rear and 45° views. The side view no longer has the deep forward belly bulge, and the front view spans both hip joints.


## Separate waist and pelvis by shape; keep hip layout fixed — 2026-11-11
- PD cancelled the new rotation-axis interpretation and asked for visual form separation only. Kept the waist at 0.16 m and retained both existing hip pivot centers; no rotation behavior or pivot positions were changed.
- Narrowed the central pelvis shell from 0.28 m to 0.14 m wide, retaining 0.075 m front-to-back depth and 0.24 m vertical span. Added fixed, subtle shape links from the narrow shell to the unchanged hip-can centers, so the pelvis reads as a central piece connected to both sides without spreading the body outward. The pelvis front receives a ceramic material zone to make the waist/pelvis boundary legible.
- Blender automation checks the narrow shell dimensions, pelvis-to-waist join, hip centerline span and fixed link placements. Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-120926-WAIST_PELVIS_SHAPE_SPLIT_NARROW`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed; design review remains pending PD. No Unity import.
- Opened the portrait sheet. The front view now shows a narrow, distinct pelvis and fixed connectors reaching both hip cans; the waist and pelvis are visually separated by their form/material transition.


## Reduce pelvis height to two-thirds and relocate hip connectors — 2026-11-11
- Reduced pelvis height from 0.24 m to 0.16 m (exactly two-thirds). Kept its top flush with the waist at 0.735 m; the new lower edge is 0.575 m. Pelvis width/depth and waist height are unchanged.
- Kept both hip pivot/can centers at their existing positions. Repositioned each static shape connector from the shortened pelvis lower edge (z=0.575 m) diagonally to its hip-can center (z=0.540 m), so the shortened shell still meets the joint without shifting the rotation centers or leg chain.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-121222-PELVIS_TWO_THIRDS_160MM_RELINKED`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed checks for two-thirds height, waist seam, connector endpoints and unchanged hip pivots. Design review remains pending PD; no Unity import.
- Opened the portrait sheet and visually checked front, side, back and 45° renders.


## Seat shoulder joints and hip joints on the body ends — 2026-11-11
- Lowered the shoulder pivots by 20 mm so the shoulder cans overlap the torso edge instead of appearing to float above it; upper-arm roots and shoulder connector geometry follow the lowered pivot.
- Raised the hip pivots from z=0.540 m to the pelvis lower edge at z=0.575 m. The hip cans and leg chains follow the pivot. Increased leg scale from 0.780 to 0.831 to keep the feet at floor height; repositioned the pelvis-to-hip form links to end at the new joint centers.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-121524-PELVIS_HIP_AND_SHOULDER_ALIGNMENT`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed pivot/connector alignment; design review remains pending PD. No Unity import.
- Opened and checked the front, side, back and 45° views. Both hip centers align with the pelvis lower edge; the shoulders sit slightly lower into the torso; feet remain planted.


## Match the cylindrical joint sizes to the adjoining limbs — 2026-11-11
- Reduced the shoulder-can diameter/depth to 0.066 × 0.066 m, matching the 0.066 m upper-arm section. Reduced the hip-can diameter/depth to 0.082 × 0.075 m, matching the 0.082 × 0.075 m thigh section.
- Kept all shoulder/hip pivot centers and connector placements fixed while resizing the cylinders; the shoulder axis standoff stays unchanged.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-121720-JOINT_CANS_MATCH_ARM_AND_THIGH`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed exact diameter/depth checks and preserved pivot positions. Design review remains pending PD; no Unity import.
- Opened the portrait sheet and inspected all four views; the smaller shoulder and hip cans no longer dominate their adjoining limbs.


## Raise the hip pivots farther into the pelvis — 2026-11-11
- The 20 mm inset still read too low in the actual phone sheet, so raised both hip-can centers and articulation pivots another 20 mm: z=0.595 m to z=0.615 m. The pelvis remains unchanged (bottom z=0.575 m), leaving the pivots 40 mm inside its lower edge.
- Repositioned the short pelvis-to-hip form links to the same raised center height and the pelvis side contour at that height. Increased leg-chain scale from 0.860 to 0.890 to compensate for the higher starting pivots and keep the feet at floor level; shoulder pivots and other body dimensions are unchanged.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-122732-HIP_JOINTS_RAISED_40MM`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed the 40 mm pelvis inset, hip/link alignment, joint/limb dimensions and model generation checks. Design review remains pending PD; no Unity import.
- Opened the phone sheet and checked front, side, back and 45° views. Hip centers visibly sit higher within the pelvis silhouette; the feet remain aligned to the floor plane.


## Shorten the exposed neck — 2026-11-11
- Shortened the thin visible neck by lowering the head assembly 16 mm while keeping its shape and torso/pelvis fixed. This leaves about 16.5 mm of exposed neck between the torso shell and head shell (previously about 32.5 mm); the neck itself remains attached under both shells.
- Kept the hip joints 40 mm inside the pelvis lower edge from the preceding review. The leg scale remains 0.890 to preserve foot placement.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-124307-HIP_JOINTS_RAISED_40MM_NECK_SHORTER_16MM`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed; design review remains pending PD. No Unity import.
- Opened the phone sheet to inspect front, right-side, back and 45° views. The head sits slightly lower, leaving a visibly shorter neck.


## Slightly reduce the head and upper-arm length — 2026-11-11
- Reduced the head dimensions from 0.140 × 0.120 × 0.115 m to 0.132 × 0.113 × 0.108 m (about 6%); lowered its center by 4.5 mm so the shorter neck gap stays consistent. Head shape, torso and all joint centers are otherwise unchanged.
- Shortened each upper arm from 0.150 m to 0.140 m while keeping its 0.066 m section thickness. Shoulder and elbow pivots stay fixed; the upper-arm end remains within the elbow-can overlap zone. Forearms and legs are unchanged.
- Blender 5.2.3 generated the editable model and four views in `Docs/Design/WISP_Base_v1/runs/20261111-125109-HEAD_AND_UPPER_ARM_REDUCED_6PCT`. Phone sheet: `WISP-1-Phone-Review.png`. Automation passed head-size, neck-gap, arm-to-joint and render checks. Design review remains pending PD; no Unity import.
- Opened the phone sheet and checked front, side, back and 45° views.


## Four joint-articulated pose studies — 2026-11-11
- Added pose selection to the Blender generator and created four editable variants from the same body proportions: IDLE, AIM, GUARD, and CROUCH. AIM rotates both shoulders/elbows; GUARD uses asymmetric shoulder/elbow angles; CROUCH bends hip, knee and ankle pivots.
- Each pose has its own `.blend`, four camera renders (front, right, back, 45°), automation log and manifest. A portrait phone sheet compares the four front views; a separate contact sheet compares all four angles.
- Blender 5.2.3 completed all four generations and renders. Automation verifies the requested joint rotations and keeps the design-review state pending PD. No Unity import.
- Opened the phone comparison image and checked the visible pose differences. IDLE remains the relaxed baseline, AIM raises both forearms, GUARD bends the arms asymmetrically, and CROUCH flexes the leg chain.


## Dark graphite armor material pass — 2026-11-11
- Changed the main armor surfaces to a deep graphite gray (RGB 0.075, 0.085, 0.095) and the structural frame to a darker charcoal (0.035, 0.042, 0.052). Reduced metallic response and increased roughness so large plates read as gray rather than bright silver under the review lights; retained the cyan visor and muzzle-glass accents.
- Kept the current head, torso, joint, limb proportions and four articulated poses unchanged. Blender 5.2.3 regenerated editable `.blend` files and four-angle renders for IDLE, AIM, GUARD, and CROUCH in `Docs/Design/WISP_Base_v1/runs/20261111-131404-GRAPHITE_IDLE`, `.../20261111-131419-GRAPHITE_AIM`, `.../20261111-131434-GRAPHITE_GUARD`, and `.../20261111-131449-GRAPHITE_CROUCH`.
- Automation and all render checks passed; design review remains pending PD. No Unity import. Opened the mobile pose sheet to check the darker material across the four poses.


## Facet the rounded mechanical joints — 2026-11-11
- Replaced the 24-sided shoulder/hip cans and 12-sided joint caps with eight-sided cylinders, giving the joint faces visible flats and chamfered edges. The pivot centers, axes, poses and graphite materials are unchanged.
- Kept can depth and limb dimensions; the eight-facet contour causes less than 3 mm apparent width reduction from angled views, which the geometry check allows. Automation verifies eight side faces on shoulder, hip, elbow, knee and ankle pieces.
- Blender 5.2.3 generated four editable pose models and four-angle renders in `Docs/Design/WISP_Base_v1/runs/20261111-131901-FACETED_GRAPHITE_IDLE`, `.../20261111-131916-FACETED_GRAPHITE_AIM`, `.../20261111-131931-FACETED_GRAPHITE_GUARD`, and `.../20261111-131946-FACETED_GRAPHITE_CROUCH`. Phone pose sheet: `WISP-Pose-Review-Phone.png`.
- Automation passed and the phone sheet was opened for visual inspection. Design review remains pending PD; no Unity import.


## Keep articulated joints cylindrical — 2026-11-11
- Reverted the faceted joint experiment after PD clarified that the articulation cans must remain cylindrical. Shoulder/hip cans are restored to 24-sided cylinders and elbow/knee/ankle caps to 12-sided cylinders. Their pivot centers, axes, sizes and articulation angles remain unchanged.
- Kept the dark graphite armor palette and the rest of the body geometry unchanged. Automation now checks cylindrical side counts and the original can-to-limb dimensions.
- Blender 5.2.3 regenerated editable models and four-angle views for IDLE, AIM, GUARD and CROUCH in `Docs/Design/WISP_Base_v1/runs/20261111-132118-CYLINDRICAL_GRAPHITE_IDLE`, `.../20261111-132133-CYLINDRICAL_GRAPHITE_AIM`, `.../20261111-132148-CYLINDRICAL_GRAPHITE_GUARD`, and `.../20261111-132202-CYLINDRICAL_GRAPHITE_CROUCH`. Automation passed; design review remains pending PD. No Unity import.
- Opened the phone pose sheet and verified that the shoulder and hip cans read as cylinders again.

## Match the industrial WISP reference finish — 2026-11-11
- Used the newly supplied WISP render as the appearance target: light neutral armor, dark graphite frame, orange armor accents and cyan optics. Restored the paired shoulder armor caps and added chest/forearm accent panels while keeping shoulder and hip cans cylindrical.
- Generated IDLE, AIM, GUARD and CROUCH models with Blender 5.2.3. Each has an editable `.blend`, four camera renders, an automation manifest and log under `Docs/Design/WISP_Base_v1/runs/20261111-132528-INDUSTRIAL_WISP_IDLE` through `.../20261111-132613-INDUSTRIAL_WISP_CROUCH`.
- Opened the front and 45-degree renders. The palette and broad armor placement now follow the reference direction, but the geometry remains a simple blockout and is visibly less detailed than the target render. Automation passed; design review is still pending PD. No Unity import.
