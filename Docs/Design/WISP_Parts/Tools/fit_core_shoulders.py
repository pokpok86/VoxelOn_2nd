"""Fit shoulder spacing to each compact chest, translating complete arm chains."""
import bpy,json,hashlib
from pathlib import Path
from mathutils import Matrix
BASE=Path(__file__).resolve().parent.parent
OUT=BASE/'CoreDepth-ShoulderFit-20261112';OUT.mkdir(exist_ok=True)
CONFIG=[('C-01_STANDARD',.012,.083),('C-02_LIGHTFRAME',.035,.067),('C-03_BULWARK_CORE',.010,.099),('C-04_ARC_CORE',.020,.086)]
TAGS=['L_Shoulder_','R_Shoulder_','L_UpperArm_','R_UpperArm_','Shoulder_Main_Angular_Cap','Standard_Lower_Return','Standard_Orange_Corner','Light_Swept_Underside','Light_Orange_Tip','Heavy_Overlapping_Downward_Skirt','Heavy_Outer_Bumper','Heavy_Orange_Outer_Inlay','Arc_Separated_Undershoulder','Arc_Recessed_Shoulder_Cyan','Shoulder_Recessed_Bolt']
def pts(o):
    ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get());data=ev.to_mesh();v=[o.matrix_world@p.co for p in data.vertices];ev.to_mesh_clear();return v
def center(o):
    p=pts(o);return [(min(v[i] for v in p)+max(v[i] for v in p))/2 for i in range(3)]
def connectors(col):
    result={}
    for side in ['L','R']:
        for tag in ['Shoulder_Can_X','UpperArm_Connector_Pin']:
            o=next(o for o in col.objects if side+'_'+tag in o.name);result[side+'_'+tag]=center(o)
    return result
def static_signature(o):return {'vertices':[list(v.co) for v in o.data.vertices],'world':[list(row) for row in o.matrix_world]}
records=[]
for part,delta,inner in CONFIG:
    source=(BASE/'CoreDepth-Family-20261112' if part=='C-01_STANDARD' else BASE/'CoreDepth-NarrowAbdomen-20261112')/part
    oldver='v1' if part=='C-01_STANDARD' else 'v2'
    dest=OUT/part;dest.mkdir(exist_ok=True)
    hashes={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in source.glob('*.blend')}
    part_record={'part':part,'each_shoulder_inward_m':delta,'sources_sha256':hashes,'states':[]}
    for mounted in [False,True]:
        suffix='-Mounted' if mounted else ''
        bpy.ops.wm.open_mainfile(filepath=str(source/(part+'-Depth-'+oldver+suffix+'.blend')))
        col=bpy.data.collections[part+'_MODULE'];before=connectors(col)
        moving=[o for o in col.objects if o.type=='MESH' and any(tag in o.name for tag in TAGS)]
        bridges=[o for o in col.objects if o.type=='MESH' and 'Torso_To_Shoulder_Rotary_Bridge' in o.name]
        fixed={o.name:static_signature(o) for o in col.objects if o.type=='MESH' and o not in moving and o not in bridges}
        roots={o.name:[list(row) for row in o.matrix_world] for o in bpy.data.objects if o.type=='EMPTY'}
        weapon_before={}
        for o in moving:
            sign=-1 if center(o)[0]<0 else 1
            world=o.matrix_world.copy();world.translation.x-=sign*delta;o.matrix_world=world
        # Rebuild only the connector span; its outer end follows the new joint.
        deps=bpy.context.evaluated_depsgraph_get()
        for o in bridges:
            data=bpy.data.meshes.new_from_object(o.evaluated_get(deps));data.transform(o.matrix_world)
            sign=-1 if center(o)[0]<0 else 1
            lo=min(abs(v.co.x) for v in data.vertices);hi=max(abs(v.co.x) for v in data.vertices)
            for v in data.vertices:
                t=(abs(v.co.x)-lo)/(hi-lo);v.co.x=sign*(inner+t*(hi-delta-inner))
            o.modifiers.clear();o.data=data;o.matrix_world=Matrix.Identity(4)
        if mounted:
            for name in ['W-01_LUMEN_ROOT','W-01_LUMEN_LEFT_ROOT']:
                o=bpy.data.objects[name];weapon_before[name]=list(o.matrix_world.translation)
                sign=-1 if o.matrix_world.translation.x<0 else 1
                world=o.matrix_world.copy();world.translation.x-=sign*delta;o.matrix_world=world
        bpy.context.view_layer.update();after=connectors(col)
        for name,p in before.items():
            q=after[name];sign=-1 if p[0]<0 else 1
            assert abs(q[0]-(p[0]-sign*delta))<1e-6,(part,name,p,q)
            assert max(abs(q[i]-p[i]) for i in [1,2])<1e-6
        assert fixed=={o.name:static_signature(o) for o in col.objects if o.name in fixed},'Chest/waist changed'
        for name,world in roots.items():
            if name not in weapon_before:assert world==[list(row) for row in bpy.data.objects[name].matrix_world],name
        for name,p in weapon_before.items():
            q=bpy.data.objects[name].matrix_world.translation;sign=-1 if p[0]<0 else 1
            assert abs(q.x-p[0]+sign*delta)<1e-6
            assert abs(q.y-p[1])<1e-6 and abs(q.z-p[2])<1e-6
        scene=bpy.context.scene
        views=[('04-Mounted-45deg','Camera_Mounted')] if mounted else [('01-Front','Camera_01-Front'),('02-Side','Camera_02-Right'),('03-45deg','Camera_05-45deg')]
        for label,cam in views:
            scene.camera=bpy.data.objects[cam];scene.render.filepath=str(dest/(part+'-'+label+'.png'));bpy.ops.render.render(write_still=True)
        path=dest/(part+'-ShoulderFit'+suffix+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(path))
        bpy.ops.wm.open_mainfile(filepath=str(path));col=bpy.data.collections[part+'_MODULE']
        reopened=connectors(col)
        assert all(max(abs(a-b) for a,b in zip(after[k],reopened[k]))<1e-6 for k in after)
        part_record['states'].append({'mounted':mounted,'connectors_before_world_m':before,'connectors_after_world_m':after,'other_geometry_fixed':True,'weapons_follow_elbows':bool(weapon_before),'saved_file_reopened':True})
    for p,h in hashes.items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h,p
    (dest/'assembly_interfaces.json').write_text(json.dumps({'part':part,'shoulder_fit_revision':True,'local_connectors':part_record['states'][0]['connectors_after_world_m'],'shared_neck_waist_backpack':'unchanged','note':'Shoulder/elbow X coordinates are revised by explicit PD request; consumers must use these per-core anchors.'},indent=2))
    records.append(part_record)
(OUT/'Validation.json').write_text(json.dumps({'automation':'PASS','design_review':'PENDING_PD','parts':records,'unity_integration':'NOT_PERFORMED','limitations':['Static Blender geometry and alignment checks, not animation/collision tests.']},indent=2))
print('SHOULDER_FIT_PASS',len(records),flush=True)
