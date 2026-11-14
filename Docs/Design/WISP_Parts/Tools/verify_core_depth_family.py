"""Reopen saved torso files and compare their shared mechanical waist."""
import bpy,json,hashlib,os
from pathlib import Path
BASE=Path(__file__).resolve().parent.parent
OUT=BASE/'CoreDepth-Family-20261112'
approved=BASE/'C01-CompactUpper-20261112/C-01_STANDARD-CompactUpper-v8.blend'
def frame_signature():
    return {o.name.split('_Frame_',1)[1]:[list(v.co) for v in o.data.vertices] for o in bpy.data.objects if o.type=='MESH' and '_Frame_' in o.name and (o.name.startswith('C01_') or o.name.startswith('Core_'))}
bpy.ops.wm.open_mainfile(filepath=str(approved));waist=frame_signature();assert len(waist)==7,len(waist)
results=[]
for part in ['C-02_LIGHTFRAME','C-03_BULWARK_CORE','C-04_ARC_CORE']:
    folder=OUT/part
    report=json.loads((folder/'Validation.json').read_text())
    for p,h in report['source_hashes'].items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h,p
    assert report['interfaces_before']==report['interfaces_after'],part
    path=folder/(part+'-Depth-v1.blend')
    bpy.ops.wm.open_mainfile(filepath=str(path));assert frame_signature()==waist,(part,'waist changed')
    col=bpy.data.collections[part+'_MODULE'];points=set();mesh_count=0
    for o in col.objects:
        if o.type!='MESH' or not o.name.startswith(('Core_','C01_')):continue
        mesh_count+=1
        assert not o.data.validate(verbose=False),o.name
        for v in o.data.vertices:points.add(tuple(round(float(n),6) for n in o.matrix_world@v.co))
    for x,y,z in points:assert (-x,y,z) in points,(part,'new body asymmetric',x,y,z)
    bpy.ops.wm.open_mainfile(filepath=str(folder/(part+'-Depth-v1-Mounted.blend')))
    assert bpy.data.objects[part+'_ROOT'].location.z>.5
    for name in ['HD01_ROOT','LG01_PAIR_ROOT','W-01_LUMEN_ROOT','W-01_LUMEN_LEFT_ROOT']:assert name in bpy.data.objects,name
    results.append({'part':part,'reopened_standalone_and_mounted':True,'waist_meshes_equal_to_C01_v8':len(waist),'new_meshes_valid_and_mirror_symmetric':mesh_count,'source_hashes_unchanged':True,'interfaces_unchanged':True})
data={'automation':'PASS','design_review':'PENDING_PD_FOR_C02_C03_C04','checks':results,'unity_integration':'NOT_PERFORMED'}
(OUT/'Reopen-Validation.json').write_text(json.dumps(data,indent=2),encoding='utf-8')
print('CORE_DEPTH_FAMILY_REOPEN_PASS',len(results),flush=True)
