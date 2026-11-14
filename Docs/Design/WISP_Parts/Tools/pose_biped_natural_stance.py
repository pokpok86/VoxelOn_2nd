"""Narrow pelvis spacing and add hip/ankle yaw for a planted natural stance."""
import bpy,math,json,hashlib
from pathlib import Path
from mathutils import Matrix,Vector
BASE=Path(__file__).resolve().parent.parent;SRC=BASE/'Biped-Four-Refined-20261112';OUT=BASE/'Biped-NaturalStance-20261112';OUT.mkdir(exist_ok=True)
catalog=json.loads((SRC/'PART_CATALOG.json').read_text(encoding='utf-8'));records=[]
def insert_before(child,name,col):
    p=child.parent;world=child.matrix_world.copy();o=bpy.data.objects.new(name,None);col.objects.link(o);o.parent=p;o.matrix_world=world;child.parent=o;child.matrix_world=world;return o
def vertices(o):return [o.matrix_world@v.co for v in o.data.vertices]
def local_socket(col):return {o.name:[list(v.co) for v in o.data.vertices] for o in col.objects if o.type=='MESH' and 'Waist_Socket' in o.name}
def pose(side,part):
    sign=-1 if side=='L' else 1
    bpy.data.objects[part+'_'+side+'_HIP_YAW_Z'].rotation_euler.z=math.radians(sign*4)
    bpy.data.objects[part+'_'+side+'_HIP_ABDUCTION_Y'].rotation_euler.y=math.radians(-sign*3)
    bpy.data.objects[part+'_'+side+'_HIP_PITCH_X'].rotation_euler.x=math.radians(-3)
    bpy.data.objects[part+'_'+side+'_KNEE_PITCH_X'].rotation_euler.x=math.radians(6)
    bpy.context.view_layer.update()
    yaw=bpy.data.objects[part+'_'+side+'_ANKLE_YAW_Z'];roll=bpy.data.objects[part+'_'+side+'_ANKLE_ROLL_Y'];pitch=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X']
    desired=Matrix.Rotation(math.radians(-sign*4),3,'Z')
    relative=yaw.parent.matrix_world.to_3x3().inverted()@desired;e=relative.to_euler('XYZ')
    yaw.rotation_euler.z=e.z;roll.rotation_euler.y=e.y;pitch.rotation_euler.x=e.x
    bpy.context.view_layer.update()
    up=pitch.matrix_world.to_3x3()@Vector((0,0,1));forward=pitch.matrix_world.to_3x3()@Vector((0,-1,0))
    assert up.dot(Vector((0,0,1)))>.999999
    assert sign*forward.x<-.05
    return {'hip_splay_deg':3,'thigh_outward_yaw_deg':4,'knee_flex_deg':6,'foot_inward_heading_deg':4,'sole_up_dot_world_z':float(up.z),'solved_ankle_yaw_roll_pitch_deg':[math.degrees(e.z),math.degrees(e.y),math.degrees(e.x)]}
for entry in catalog['parts']:
    part=entry['id'];dest=OUT/part;dest.mkdir(exist_ok=True);hashes={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in (SRC/part).glob('*.blend')};states=[]
    for mounted in [False,True]:
        suffix='-Mounted' if mounted else '';bpy.ops.wm.open_mainfile(filepath=str(SRC/part/(part+suffix+'.blend')))
        col=bpy.data.collections[part+'_MODULE'];root=bpy.data.objects[part+'_ROOT'];socket_before=local_socket(col)
        hip_before={s:list(bpy.data.objects[part+'_'+s+'_HIP_ABDUCTION_Y'].matrix_world.translation) for s in ['L','R']}
        # Narrow central pelvis and cross-shaft without changing the mating socket.
        for o in col.objects:
            if o.type!='MESH':continue
            if o.name.startswith('Pelvis_'):
                world=o.matrix_world.copy();factor=.90 if 'Continuous_Hip_Shaft' in o.name else .92
                for v in o.data.vertices:
                    p=world@v.co;p.x*=factor;v.co=world.inverted()@p
            if 'Hip_Outer_Skirt' in o.name:
                sign=-1 if o.name.startswith('L_') else 1;world=o.matrix_world.copy();world.translation.x-=sign*entry['hip_center_x_abs_m']*.10;o.matrix_world=world
        new_nodes=[]
        for side in ['L','R']:
            hip=bpy.data.objects[part+'_'+side+'_HIP_ABDUCTION_Y'];hip.location.x*=.90;bpy.context.view_layer.update()
            hy=insert_before(hip,part+'_'+side+'_HIP_YAW_Z',col)
            ankle=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X'];ay=insert_before(ankle,part+'_'+side+'_ANKLE_YAW_Z',col);ar=insert_before(ankle,part+'_'+side+'_ANKLE_ROLL_Y',col)
            new_nodes.extend([hy.name,ay.name,ar.name]);bpy.context.view_layer.update()
            # Verify the new joints rotate descendants around their own centers.
            foot=bpy.data.objects[side+'_Foot_Main'];rest=foot.matrix_world.to_3x3().copy();center=ay.matrix_world.translation.copy()
            hy.rotation_euler.z=math.radians(12);bpy.context.view_layer.update();assert (rest.inverted()@foot.matrix_world.to_3x3()).to_quaternion().angle>.15
            hy.rotation_euler.z=0;ay.rotation_euler.z=math.radians(12);bpy.context.view_layer.update();assert (ay.matrix_world.translation-center).length<1e-6
            assert (rest.inverted()@foot.matrix_world.to_3x3()).to_quaternion().angle>.15;ay.rotation_euler.z=0;bpy.context.view_layer.update()
        standing={s:pose(s,part) for s in ['L','R']}
        foot_vertices={s:vertices(bpy.data.objects[s+'_Foot_Sole']) for s in ['L','R']}
        lows=[min(p.z for p in foot_vertices[s]) for s in ['L','R']];assert abs(lows[0]-lows[1])<1e-6
        dz=-min(lows);root.location.z+=dz
        if mounted:
            for name in ['C-01_STANDARD_ROOT','HD01_ROOT','W-01_LUMEN_ROOT','W-01_LUMEN_LEFT_ROOT']:
                o=bpy.data.objects[name];world=o.matrix_world.copy();world.translation.z+=dz;o.matrix_world=world
        bpy.context.view_layer.update();assert local_socket(col)==socket_before
        for side in ['L','R']:
            sole=vertices(bpy.data.objects[side+'_Foot_Sole']);assert abs(min(p.z for p in sole))<1e-6
            # The whole flat bottom is planted, not just one tilted corner.
            bottom=sorted(p.z for p in sole)[:8];assert max(bottom)-min(bottom)<1e-6
        # Foot pivots stay coaxial despite the new rotation hierarchy.
        for side in ['L','R']:
            p=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X'].matrix_world.translation
            for tag in ['ANKLE_YAW_Z','ANKLE_ROLL_Y']:assert (p-bpy.data.objects[part+'_'+side+'_'+tag].matrix_world.translation).length<1e-6
        scene=bpy.context.scene
        views=[('07-Mounted-45deg','Camera_Mounted')] if mounted else [('01-Front','Camera_LG01-01-Front'),('02-Right','Camera_LG01-02-Right'),('03-Back','Camera_LG01-03-Back'),('05-45deg','Camera_LG01-05-45deg')]
        for label,cam in views:
            scene.camera=bpy.data.objects[cam];scene.render.filepath=str(dest/(part+'-'+label+'.png'));bpy.ops.render.render(write_still=True)
        path=dest/(part+suffix+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(path));bpy.ops.wm.open_mainfile(filepath=str(path))
        assert all(n in bpy.data.objects for n in new_nodes)
        states.append({'mounted':mounted,'hip_centers_before_m':hip_before,'hip_centers_after_m':{s:list(bpy.data.objects[part+'_'+s+'_HIP_YAW_Z'].matrix_world.translation) for s in ['L','R']},'stance':standing,'root_floor_shift_z_m':dz,'waist_mating_world_m':list(bpy.data.objects[part+'_WAIST_MATING_INTERFACE'].matrix_world.translation),'new_joint_nodes':new_nodes,'new_axes_rotate_descendants':True,'both_flat_soles_on_ground':True,'socket_local_vertices_preserved':True,'saved_file_reopened':True})
    for p,h in hashes.items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h,p
    record={'id':part,'number':len(records)+1,'pelvis_width_factor':.92,'hip_spacing_factor':.90,'states':states,'source_sha256':hashes,'automation':'PASS','design_review':'PENDING_PD','unity_test':'NOT_PERFORMED'}
    (dest/'manifest.json').write_text(json.dumps(record,indent=2),encoding='utf-8');records.append(record);print('STANCE_PART_PASS',part,flush=True)
(OUT/'Validation.json').write_text(json.dumps({'automation':'PASS','parts':records,'limitations':['Blender articulation and static stance checks; no gameplay or full collision sweep.']},indent=2))
print('NATURAL_STANCE_FOUR_PASS',len(records),flush=True)
