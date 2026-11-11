"""Open every delivered standalone .blend and check saved-file integrity."""
import bpy
import os
import json
import hashlib
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
parts=['W-02_BREAKER','W-03_RATTLE','W-04_EDGE','W-05_AEGIS','B-01_TWIN_DRIVE']
report={}
for part in parts:
    folder=os.path.join(ROOT,part)
    with open(os.path.join(folder,'manifest.json'),encoding='utf-8') as f:m=json.load(f)
    bpy.ops.wm.open_mainfile(filepath=os.path.join(folder,part+'.blend'))
    root=bpy.data.objects.get(part+'_ROOT');assert root is not None
    assert (root.location-Vector(m['connection_origin_working_units'])).length<.00001
    meshes=[o for o in bpy.data.objects if o.type=='MESH']
    assert len(meshes)==m['module_mesh_count']
    assert all(o.name.startswith(part+'_') for o in meshes)
    assert not any(n in bpy.data.objects for n in ('BD01_ROOT','HD01_ROOT','LG01_PAIR_ROOT'))
    cameras=[bpy.data.objects['Camera_'+n] for n in ('01-Front','02-Right','03-Rear','04-45deg')]
    assert all(c.data.type=='ORTHO' and abs(c.data.ortho_scale-m['four_views_same_ortho_scale'])<.00001 for c in cameras)
    ref=os.path.join(ROOT,'References',part+'.png')
    with open(ref,'rb') as f:assert hashlib.sha256(f.read()).hexdigest()==m['reference_sha256']
    for b,h in m['base_sha256_unchanged'].items():
        with open(os.path.join(ROOT,'../WISP_Base_v1',b,b+'.blend'),'rb') as f:assert hashlib.sha256(f.read()).hexdigest()==h
    assert all(c['major_shell_vs_torso_thigh_bounds_clear'] for c in m['static_clearance_checks'].values())
    if part.startswith('B-'):assert m['thruster_lens_visibility_rays']==[True,True]
    report[part]={'saved_blend_reopened':True,'standalone_mesh_count':len(meshes),'reference_sha256_match':True,'base_sha256_match':True,'same_scale_orthographic_views':True,'recorded_static_checks_pass':True,'design_approval':'PENDING_PD'}
with open(os.path.join(ROOT,'FirstBatch-Validation.json'),'w',encoding='utf-8') as f:json.dump(report,f,indent=2)
print('FIVE_SAVED_BLENDS_VERIFIED',len(report))
