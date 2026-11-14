"""Correct only foot heading while preserving the narrowed pelvis and strong stance."""
import bpy,json,math,hashlib
from pathlib import Path
from mathutils import Matrix,Vector
BASE=Path(__file__).resolve().parent.parent;SRC=BASE/'Biped-OutwardFeet-20261112';OUT=BASE/'Biped-FeetFollowThigh-20261112';OUT.mkdir(exist_ok=True)
records=[]
for part in ['LB-01_DART','LB-02_STRIDER','LB-03_BULWARK','LB-04_COLOSSUS']:
    dest=OUT/part;dest.mkdir(exist_ok=True);states=[];hashes={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in (SRC/part).glob('*.blend')}
    for mounted in [False,True]:
        suffix='-Mounted' if mounted else '';bpy.ops.wm.open_mainfile(filepath=str(SRC/part/(part+suffix+'.blend')))
        moving={part+'_'+s+'_'+tag for s in ['L','R'] for tag in ['ANKLE_YAW_Z','ANKLE_ROLL_Y','ANKLE_PITCH_X']}
        fixed={o.name:(tuple(o.location),tuple(o.rotation_euler),tuple(o.scale)) for o in bpy.data.objects if o.type=='EMPTY' and o.name not in moving}
        checks=[]
        for side,sign in [('L',-1),('R',1)]:
            yaw=bpy.data.objects[part+'_'+side+'_ANKLE_YAW_Z'];roll=bpy.data.objects[part+'_'+side+'_ANKLE_ROLL_Y'];pitch=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X']
            center=yaw.matrix_world.translation.copy()
            thigh_forward=bpy.data.objects[part+'_'+side+'_HIP_PITCH_X'].matrix_world.to_3x3()@Vector((0,-1,0))
            thigh_heading=math.atan2(thigh_forward.x,-thigh_forward.y)
            desired=Matrix.Rotation(thigh_heading,3,'Z')
            e=(yaw.parent.matrix_world.to_3x3().inverted()@desired).to_euler('XYZ')
            yaw.rotation_euler.z=e.z;roll.rotation_euler.y=e.y;pitch.rotation_euler.x=e.x;bpy.context.view_layer.update()
            up=pitch.matrix_world.to_3x3()@Vector((0,0,1));forward=pitch.matrix_world.to_3x3()@Vector((0,-1,0))
            heading=math.degrees(math.atan2(forward.x,-forward.y))
            assert abs(heading-math.degrees(thigh_heading))<.001 and up.z>.999999
            assert (yaw.matrix_world.translation-center).length<1e-6
            sole=bpy.data.objects[side+'_Foot_Sole'];bottom=sorted((sole.matrix_world@v.co).z for v in sole.data.vertices)[:8];assert max(abs(z) for z in bottom)<1e-6
            checks.append({'side':side,'foot_heading_deg':heading,'thigh_projected_heading_deg':math.degrees(thigh_heading),'foot_matches_thigh':True,'sole_flat_and_grounded':True,'ankle_pivot_unchanged':True})
        assert fixed=={o.name:(tuple(o.location),tuple(o.rotation_euler),tuple(o.scale)) for o in bpy.data.objects if o.name in fixed}
        scene=bpy.context.scene;views=[('07-Mounted-45deg','Camera_Mounted')] if mounted else [('01-Front','Camera_LG01-01-Front'),('02-Right','Camera_LG01-02-Right'),('05-45deg','Camera_LG01-05-45deg')]
        for label,cam in views:
            scene.camera=bpy.data.objects[cam];scene.render.filepath=str(dest/(part+'-'+label+'.png'));bpy.ops.render.render(write_still=True)
        path=dest/(part+suffix+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(path));bpy.ops.wm.open_mainfile(filepath=str(path))
        for side,sign in [('L',-1),('R',1)]:
            f=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X'].matrix_world.to_3x3()@Vector((0,-1,0))
            t=bpy.data.objects[part+'_'+side+'_HIP_PITCH_X'].matrix_world.to_3x3()@Vector((0,-1,0))
            assert abs(math.atan2(f.x,-f.y)-math.atan2(t.x,-t.y))<.00001
        states.append({'mounted':mounted,'feet':checks,'all_other_joint_and_root_transforms_unchanged':True,'saved_file_reopened':True})
    for p,h in hashes.items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h
    record={'id':part,'states':states,'source_hashes_preserved':hashes};records.append(record)
    (dest/'manifest.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
    print('OUTWARD_FEET_PART_PASS',part,flush=True)
(OUT/'Validation.json').write_text(json.dumps({'automation':'PASS','design_review':'PENDING_PD','parts':records,'unity_test':'NOT_PERFORMED'},indent=2),encoding='utf-8')
print('OUTWARD_FEET_FOUR_PASS',len(records),flush=True)
