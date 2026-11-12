"""Reopen the five generated candidates and verify the immutable base."""
import bpy,os,json,hashlib,sys
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DESIGN=os.path.dirname(ROOT)
with open(os.path.join(ROOT,'APPROVED_BASELINE.json')) as f:baseline=json.load(f)
REF='--reference-batch' in sys.argv
DELIVERY=os.path.join(ROOT,'ReferenceBatch-20261112') if REF else ROOT
if REF:
    with open(os.path.join(DELIVERY,'References/manifest.json')) as f:references=json.load(f)
    parts=sorted(references)
else:parts=baseline['legacy_exploratory_candidate_ids']
report={}
for part in parts:
    folder=os.path.join(DELIVERY,part)
    with open(os.path.join(folder,'manifest.json')) as f:m=json.load(f)
    if REF:
        with open(os.path.join(DELIVERY,'References',part+'.jpg'),'rb') as f:assert hashlib.sha256(f.read()).hexdigest()==m['reference_sha256']
    bpy.ops.wm.open_mainfile(filepath=os.path.join(folder,part+'.blend'))
    root=bpy.data.objects[part+'_ROOT']
    meshes=[o for o in bpy.data.objects if o.type=='MESH']
    assert len(meshes)==m['module_mesh_count']
    assert all(o.name.startswith(part+'_') and o.parent==root for o in meshes)
    assert not any(n in bpy.data.objects for n in ('BD01_ROOT','HD01_ROOT','LG01_PAIR_ROOT'))
    for title in ('01-Front','02-Right','03-Rear','04-45deg','05-Top'):
        d=bpy.data.objects['Camera_'+title].data
        assert d.type=='ORTHO' and abs(d.ortho_scale-m['four_plus_top_same_ortho_scale'])<.00001
    if part.startswith('L-'):
        floor=bpy.data.objects[part+'_Waist_Socket_Floor']
        assert abs(max((floor.matrix_world @ Vector(v)).z for v in floor.bound_box)-m['mating_center'][2])<.00001
    else:
        pins=[o for o in meshes if 'Common_Back_Mount_Pin' in o.name]
        assert len(pins)==4
        centers=sorted((round(o.location.x,4),round(o.location.z,4)) for o in pins)
        assert centers==[(-.034,-.025),(-.034,.025),(.034,-.025),(.034,.025)]
    nozzle_checks=[]
    for nozzle in m.get('reference_nozzles',[]):
        c=Vector(nozzle['mouth']);axis=Vector(nozzle['axis'])
        offset=Vector((nozzle['width']*.235,0,0)) if nozzle['shape']=='round' else Vector((0,0,0))
        hit,loc,normal,face,obj,wm=bpy.context.scene.ray_cast(bpy.context.evaluated_depsgraph_get(),c+axis*.3+offset,-axis)
        assert hit and ('_Recessed_Emission' in obj.name or '_Inner_Wall_Heat' in obj.name),(part,nozzle['name'],'closed or obscured nozzle',obj.name if hit else None)
        nozzle_checks.append({'name':nozzle['name'],'ray_hits_recessed_emitter':True})
    assert all(os.path.getsize(os.path.join(folder,name))>10000 for name in m['renders'])
    bpy.ops.wm.open_mainfile(filepath=os.path.join(folder,part+'-Mounted-Review.blend'))
    assert bpy.data.objects.get('BD01_ROOT') and bpy.data.objects.get('HD01_ROOT')
    assert max(abs(bpy.data.objects['BD01_ROOT'].scale[i]-.63) for i in range(3))<.00001
    head_scale=bpy.data.objects['HD01_ROOT'].scale
    assert max(abs(v-1) for v in head_scale)<.00001
    assert abs(m['waist_contact_error_m'])<.00001 and abs(m['neck_contact_error_m'])<.00001
    assert bpy.data.objects.get('LG01_PAIR_ROOT') is None if part.startswith('L-') else True
    # Verify actual saved mating centers and both forearm sockets.
    module=bpy.data.objects[part+'_ROOT'];mount=bpy.data.objects[part+'_MATING_INTERFACE']
    if part.startswith('B-'):assert (module.location-Vector(m['mounted_location'])).length<.00001
    else:assert abs(mount.matrix_world.translation.z-m['mating_center'][2])<.00001
    for side,name in [('R','W-01_LUMEN_ROOT'),('L','W-01_LUMEN_LEFT_ROOT')]:
        assert (bpy.data.objects[name].matrix_world.translation-bpy.data.objects[side+'_UpperArm_Connector_Pin'].matrix_world.translation).length<.00001
    report[part]={'standalone_and_mounted_reopened':True,'open_nozzle_rays':nozzle_checks,'module_meshes':len(meshes),
                  'same_five_view_scale':True,'common_interface_verified':True,
                  'approved_torso_scale_preserved':True,'forearm_connections_verified':True,
                  'design_review':'PENDING_PD','unity':'NOT_PERFORMED'}
for path,h in baseline['geometry_sha256'].items():
    with open(os.path.join(DESIGN,path),'rb') as f:assert hashlib.sha256(f.read()).hexdigest()==h,path
with open(os.path.join(DELIVERY,'Validation.json' if REF else 'Expansion-Validation.json'),'w') as f:json.dump({'parts':report,'all_nine_original_blends_sha256_unchanged':True},f,indent=2)
print('EXPANSION_SAVED_CHECK_PASS',len(report))
