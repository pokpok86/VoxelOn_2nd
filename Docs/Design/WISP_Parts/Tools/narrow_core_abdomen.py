"""Narrow upper abdominal frames to each chest; keep waist mating blocks fixed."""
import bpy,sys,json,hashlib,shutil
from pathlib import Path
BASE=Path(__file__).resolve().parent.parent
SRC=BASE/'CoreDepth-Family-20261112';OUT=BASE/'CoreDepth-NarrowAbdomen-20261112';OUT.mkdir(exist_ok=True)
def signature(o):return [list(v.co) for v in o.data.vertices]
checks=[]
for part,target in [('C-02_LIGHTFRAME',.024),('C-03_BULWARK_CORE',.046),('C-04_ARC_CORE',.041)]:
    folder=OUT/part;folder.mkdir(exist_ok=True)
    hashes={str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in (SRC/part).glob('*.blend')}
    for mounted in [False,True]:
        suffix='-Mounted' if mounted else ''
        bpy.ops.wm.open_mainfile(filepath=str(SRC/part/(part+'-Depth-v1'+suffix+'.blend')))
        col=bpy.data.collections[part+'_MODULE']
        unchanged={o.name:signature(o) for o in col.objects if o.type=='MESH' and ('_Frame_' not in o.name or 'Lower_Waist_Crossmember' in o.name)}
        others={o.name:[list(row) for row in o.matrix_world] for o in bpy.data.objects if o.type=='EMPTY'}
        for o in col.objects:
            if o.type!='MESH' or '_Frame_' not in o.name:continue
            for v in o.data.vertices:
                t=max(0,min(1,(v.co.z+.115)/.057))
                v.co.x*=1-t*(1-target/.057)
        assert unchanged=={o.name:signature(o) for o in col.objects if o.name in unchanged}
        assert others=={o.name:[list(row) for row in o.matrix_world] for o in bpy.data.objects if o.type=='EMPTY'}
        bpy.context.view_layer.update()
        scene=bpy.context.scene
        if mounted:views=[('04-Mounted-45deg','Camera_Mounted')]
        else:views=[('01-Front','Camera_01-Front'),('02-Side','Camera_02-Right'),('03-45deg','Camera_05-45deg')]
        for label,cam in views:
            scene.camera=bpy.data.objects[cam];scene.render.filepath=str(folder/(part+'-'+label+'.png'));bpy.ops.render.render(write_still=True)
        path=folder/(part+'-Depth-v2'+suffix+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(path))
        bpy.ops.wm.open_mainfile(filepath=str(path))
        assert unchanged=={o.name:signature(o) for o in bpy.data.collections[part+'_MODULE'].objects if o.name in unchanged}
    for p,h in hashes.items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h
    checks.append({'part':part,'upper_crossmember_half_width_m':target,'waist_connector_and_lower_crossmember':'UNCHANGED','chest_and_all_other_geometry':'UNCHANGED','joint_and_other_part_transforms':'UNCHANGED','saved_files_reopened':True,'originals_preserved':True})
(OUT/'Validation.json').write_text(json.dumps({'automation':'PASS','design_review':'PENDING_PD','checks':checks,'unity_integration':'NOT_PERFORMED'},indent=2))
print('NARROW_ABDOMEN_PASS',len(checks),flush=True)
