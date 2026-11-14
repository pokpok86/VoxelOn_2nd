"""Reopen the four biped deliveries and verify saved articulation and socket geometry."""
import bpy,json,hashlib,math
from pathlib import Path
BASE=Path(__file__).resolve().parent.parent;OUT=BASE/'Biped-Four-Refined-20261112'
source=BASE.parent/'WISP_Base_v1/LG01/LG01.blend'
def socket():return {o.name:[list(o.matrix_world@v.co) for v in o.data.vertices] for o in bpy.data.objects if o.type=='MESH' and 'Waist_Socket' in o.name}
bpy.ops.wm.open_mainfile(filepath=str(source));reference=socket();results=[];part_geometry={}
catalog=json.loads((OUT/'PART_CATALOG.json').read_text())
for record in catalog['parts']:
    part=record['id'];folder=OUT/part;bpy.ops.wm.open_mainfile(filepath=str(folder/(part+'.blend')))
    assert socket()==reference,(part,'socket mismatch')
    assert abs(bpy.data.objects[part+'_WAIST_MATING_INTERFACE'].matrix_world.translation.z-.505)<1e-6
    part_geometry[part]={}
    for tag in ['Thigh_Angular_Armor','Shin_Angular_Armor','Foot_Main','Foot_Sole','Thigh_Load_Beam','Shin_Load_Beam']:
        o=bpy.data.objects['R_'+tag];h=record['hip_center_x_abs_m']
        part_geometry[part][tag]=[(float(p.x-h),float(p.y),float(p.z)) for v in o.data.vertices for p in [o.matrix_world@v.co]]
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
# The requested relationships are checked from saved mesh vertices.
def same_shape(a,b):return len(a)==len(b) and max(abs(x-y) for u,v in zip(a,b) for x,y in zip(u,v))<.000002
assert same_shape(part_geometry['LB-01_DART']['Thigh_Angular_Armor'],part_geometry['LB-02_STRIDER']['Thigh_Angular_Armor'])
for tag in ['Thigh_Angular_Armor','Shin_Angular_Armor','Foot_Main','Foot_Sole','Thigh_Load_Beam','Shin_Load_Beam']:
    assert same_shape(part_geometry['LB-02_STRIDER'][tag],part_geometry['LB-03_BULWARK'][tag]),tag
calf1=part_geometry['LB-01_DART']['Shin_Angular_Armor'];calf2=part_geometry['LB-02_STRIDER']['Shin_Angular_Armor']
assert min(p[2] for p in calf1)-min(p[2] for p in calf2)>.09
calf4=part_geometry['LB-04_COLOSSUS']['Shin_Angular_Armor']
lower=max(abs(p[0]) for p in calf4 if p[2]<.11);upper=max(abs(p[0]) for p in calf4 if p[2]>.22)
assert lower>upper*1.5,(lower,upper)
for p,h in catalog['originals_sha256'].items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h
(OUT/'Reopen-Validation.json').write_text(json.dumps({'automation':'PASS','checks':results,'unity_test':'NOT_PERFORMED','design_review':'PENDING_PD','requested_relations':{'one_thigh_equals_two':True,'three_contains_twos_base_geometry':True,'two_calf_armor_extends_lower_than_one':True,'four_ankle_cowl_wider_than_upper_calf':True}},indent=2))
print('BIPED_REOPEN_PASS',len(results),flush=True)
