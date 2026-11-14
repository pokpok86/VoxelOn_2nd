"""Reopen actual assembly files and check fixed interfaces, stance, and surface points."""
import bpy,json,hashlib,math
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).resolve().parent.parent/'Combination-Review-20261112'
catalog=json.loads((OUT/'PART_COMBINATIONS.json').read_text(encoding='utf-8'));checks=[]
def bounds(o):
    p=[o.matrix_world@v.co for v in o.data.vertices];return [[min(v[i] for v in p),max(v[i] for v in p)] for i in range(3)]
def center(o):return Vector([(lo+hi)/2 for lo,hi in bounds(o)])
for r in catalog['combinations']:
    bpy.ops.wm.open_mainfile(filepath=str(OUT/r['blend']));body=bpy.data.collections[r['core']+'_MODULE'];head=bpy.data.collections[r['head']+'_MODULE'];legs=bpy.data.collections[r['lower']+'_MODULE']
    bodyneck=next(o for o in body.objects if 'Neck_Head_Interface' in o.name);headneck=next(o for o in head.objects if 'Fixed_Neck_Interface' in o.name)
    waist=next(o for o in body.objects if 'Waist_Pelvis_Interface_Key' in o.name);socket=next(o for o in legs.objects if 'Waist_Socket_Floor' in o.name)
    neck_error=abs(bounds(bodyneck)[2][1]-bounds(headneck)[2][0]);waist_error=abs(bounds(waist)[2][0]-bounds(socket)[2][1])
    assert neck_error<.0001 and waist_error<.0001,(r['number'],neck_error,waist_error)
    for side,part in [('R',r['right_weapon']),('L',r['left_weapon'])]:
        pin=next(o for o in body.objects if side+'_UpperArm_Connector_Pin' in o.name);root=bpy.data.objects[part+('_LEFT_ROOT' if side=='L' else '_ROOT')];assert (root.matrix_world.translation-center(pin)).length<1e-6
    for side in ['L','R']:
        part=r['lower'];thigh=bpy.data.objects[part+'_'+side+'_HIP_PITCH_X'];ankle=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X'];t=thigh.matrix_world.to_3x3()@Vector((0,-1,0));f=ankle.matrix_world.to_3x3()@Vector((0,-1,0))
        assert abs(math.atan2(t.x,-t.y)-math.atan2(f.x,-f.y))<.00001
        sole=bpy.data.objects[side+'_Foot_Sole'];low=sorted((sole.matrix_world@v.co).z for v in sole.data.vertices)[:8];assert max(abs(z) for z in low)<1e-6
    decor=bpy.data.collections['Assembly_'+r['number']+'_Surface_Points'];texts=[o for o in decor.objects if o.type=='FONT'];lights=[o for o in decor.objects if o.type=='MESH' and 'Cyan_Point' in o.name]
    assert len(texts)>=3 and len(lights)>=2,(r['number'],len(texts),len(lights))
    assert all(o.parent is not None for o in decor.objects)
    for p,h in r['sources_sha256'].items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h,p
    checks.append({'number':r['number'],'neck_contact_error_m':neck_error,'waist_contact_error_m':waist_error,'weapon_roots_at_elbow_pins':True,'feet_follow_thigh_and_flat_on_ground':True,'surface_texts':len(texts),'small_emissive_strips':len(lights),'decorations_parented_to_original_parts':True,'sources_unchanged':True})
(OUT/'Reopen-Validation.json').write_text(json.dumps({'automation':'PASS','checks':checks,'design_review':'PENDING_PD','unity_test':'NOT_PERFORMED','collision_sweep':'NOT_PERFORMED'},indent=2),encoding='utf-8')
print('COMBINATIONS_REOPEN_PASS',len(checks),flush=True)
