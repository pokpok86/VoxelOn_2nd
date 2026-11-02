# TASK-20261102-WISP-THIGH-043
WHY: WISP runner has a narrow exposed thigh relative to swept calf. Improve the coherent SD silhouette, not add rods.
DESIGN INTENT: Beveled fairing on each upper leg socket. Center=(side*0.11,-0.25,0.10), size=(0.45,0.50,0.53), pitch=-9 degrees, bevel=.17, taper=.79. Knee and ankle remain articulated.
INVARIANTS: No physics, collider, stats, boost, weapons, pivot or foot changes; main untouched.
VALIDATION: Same neutral camera/scale/pose before-after; inspect hip-arm and knee intersections during running. Unity build/smoke/play not run (PC off).
