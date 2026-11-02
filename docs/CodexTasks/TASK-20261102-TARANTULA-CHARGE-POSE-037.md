# TASK-037 TARANTULA 2s crouch charge, forward lean, Shift cancellation

User rule: charge jump for 2 seconds, progressively bow torso and fold legs while holding Space; release launches; pressing booster Shift cancels without launch.

work commits:
- a6eebe494becc5780096609a208ecfc82b88d3e4: set TARANTULA prototype jumpChargeSeconds=2f.
- 01865046013745cd25481c08d7e7f2600af0d656: during charge rotate full body visual forward up to 28 degrees, keep body lowered and knee/ankle flex; Shift key-down latches jumpChargeCanceled and zeroes stored charge; Space release clears latch without jumping.
- 3297e3e1380cec7a9c83f0b960e85b1d896c30cf: prevent simultaneous Space+Shift from starting a charge.

QA: NOT Unity tested. Confirm 0.25/1/2s progressively deeper crouch and forward lean; max charge cap; Space release launches, Shift cancellation never launches, holding Space after cancellation does not resume, release then press begins new charge. Check body/leg clipping, weapon/head visual and camera; adjust rotations against full assembled silhouette at same scale/view. TARANTULA remains data-only preset not selectable in current 8-frame roster; true 4-leg geometry not yet implemented. Shift is cancellation input even though TARANTULA horizontal booster is disabled. main untouched.
