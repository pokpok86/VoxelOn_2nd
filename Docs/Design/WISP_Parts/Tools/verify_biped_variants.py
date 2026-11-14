"""Reopen the four biped deliveries and verify saved articulation and socket geometry."""
import bpy,json,hashlib,math
from pathlib import Path
BASE=Path(__file__).resolve().parent.parent;OUT=BASE/'Biped-Four-20261112'
source=BASE.parent/'WISP_Base_v1/LG01/LG01.blend'
def socket():return {o.name:[list(o.matrix_world@v.co) for v in o.data.vertices] for o in bpy.data.objects if o.type=='MESH' and 'Waist_Socket' in o.name}
bpy.ops.wm.open_mainfile(filepath=str(source));reference=socket();results=[]
catalog=json.loads((OUT/'PART_CATALOG.json').read_text())
for record in catalog['parts']:
    part=record['id'];folder=OUT/part;bpy.ops.wm.open_mainfile(filepath=str(folder/(part+'.blend')))
    assert socket()==reference,(part,'socket mismatch')
    assert abs(bpy.data.objects[part+'_WAIST_MATING_INTERFACE'].matrix_world.translation.z-.505)<1e-6
    for side in ['L','R']:
        knee=bpy.data.objects[part+'_'+side+'_KNEE_PITCH_X'];ankle=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X']
        assert abs(knee.rotation_euler.x)<1e-6
        start=ankle.matrix_world.translation.copy();pivot=knee.matrix_world.translation.copy()
        knee.rotation_euler.x=math.radians(30);bpy.context.view_layer.update()
        assert ankle.matrix_world.translation.y>start.y+.04
        assert (knee.matrix_world.translation-pivot).length<1e-6
        knee.rotation_euler.x=0;bpy.context.view_layer.update()
        assert (ankle.matrix_world.translation-start).length<1e-6
    bpy.ops.wm.open_mainfile(filepath=str(folder/(part+'-Mounted.blend')))
    assert socket()==reference
    assert bpy.data.objects['C-01_STANDARD_ROOT'].location.z>.5
    assert 'LG01_PAIR_ROOT' not in bpy.data.objects
    results.append({'part':part,'standalone_and_mounted_reopened':True,'original_socket_exactly_preserved':True,'both_saved_knees_bend_and_reset':True,'old_leg_module_removed_from_mounted_preview':True})
for p,h in catalog['originals_sha256'].items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h
(OUT/'Reopen-Validation.json').write_text(json.dumps({'automation':'PASS','checks':results,'unity_test':'NOT_PERFORMED','design_review':'PENDING_PD'},indent=2))
print('BIPED_REOPEN_PASS',len(results),flush=True)
