import bpy,os,json,sys,hashlib
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT=os.path.join(ROOT,'HeadCore-20261112');BASE=os.path.abspath(os.path.join(ROOT,'../WISP_Base_v1'))
items=json.load(open(os.path.join(OUT,'Reference-Manifest.json')));old=json.load(open(os.path.join(BASE,'BD01/manifest.json')))
baseline=json.load(open(os.path.join(ROOT,'APPROVED_BASELINE.json')));results=[]
if '--slim' in sys.argv:
    OUT=os.path.join(ROOT,'SlimHeads-20261112');items=[{'id':p,'sha256':None} for p in ['H-05_NEEDLE','H-06_LANCET']]
json.dump({'automation':'RUNNING','design_review':'PENDING_PD'},open(os.path.join(OUT,'Validation.json'),'w'),indent=2)
def sha(p):return hashlib.sha256(open(p,'rb').read()).hexdigest()
def bounds(o):
    e=o.evaluated_get(bpy.context.evaluated_depsgraph_get());m=e.to_mesh();v=[o.matrix_world@x.co for x in m.vertices];e.to_mesh_clear()
    return Vector(tuple(min(p[i] for p in v) for i in range(3))),Vector(tuple(max(p[i] for p in v) for i in range(3)))
def find(tag):return next(o for o in bpy.data.objects if o.type=='MESH' and tag in o.name)
for item in items:
    part=item['id'];folder=os.path.join(OUT,part);bpy.ops.wm.open_mainfile(filepath=os.path.join(folder,part+'.blend'))
    m=json.load(open(os.path.join(folder,'manifest.json')))
    if item.get('sha256') is not None:assert sha(os.path.join(OUT,'References',part+'.jpg'))==item['sha256']
    assert m['design_review']=='PENDING_PD' and m['unity_integration']=='NOT_PERFORMED'
    refs=[part+'-'+v+'.png' for v in ['01-Front','02-Right','03-Rear','04-Left','05-45deg','06-Interface','07-Mounted']]
    if part.startswith('C-'):refs += [part+'-08-Silhouette-Front.png',part+'-09-Silhouette-Side.png']
    for name in refs:assert os.path.getsize(os.path.join(folder,name))>5000,name
    assert os.path.getsize(os.path.join(folder,part+'-Mounted.blend'))>10000
    errors={}
    if part.startswith('H-'):
        errors['neck_bottom_m']=abs(bounds(find('Fixed_Neck_Interface'))[0].z+.046)
        if part in ('H-05_NEEDLE','H-06_LANCET'):
            shell=find('Long_Low_Inner_Sensor' if part=='H-05_NEEDLE' else 'Tall_Slender_Sensor_Core')
            errors['neck_to_shell_gap_m']=max(0,bounds(shell)[0].z-bounds(find('Short_Neck_Seat'))[1].z)
    else:
        errors['neck_contact_m']=abs(bounds(find('Neck_Head_Interface_Compatible'))[1].z-old['assembly_interfaces']['neck_contact_top_local'][2])
        errors['waist_contact_m']=abs(bounds(find('Waist_Pelvis_Interface_Key_Compatible'))[0].z-old['assembly_interfaces']['waist_bottom_local_z'])
        for side,key in [('L','left_elbow_local'),('R','right_elbow_local')]:
            lo,hi=bounds(find(side+'_UpperArm_Connector_Pin_Compatible'));errors[side+'_elbow_center_m']=((lo+hi)/2-Vector(old['assembly_interfaces'][key])).length
    assert all(e<.00001 for e in errors.values()),(part,errors)
    results.append({'id':part,'checks':'PASS','measured_interface_errors_m':errors,'render_count':len(refs),'editable_blend':True,'mounted_blend':True,'design_review':'PENDING_PD'});print('HEAD_CORE_VERIFY_PASS',part,errors,flush=True)
for p,h in baseline['geometry_sha256'].items():assert sha(os.path.join(os.path.dirname(ROOT),p))==h,p
json.dump({'automation':'PASS','blender':bpy.app.version_string,'parts':results,'original_base_files_unchanged':True,'unity_integration':'NOT_PERFORMED'},open(os.path.join(OUT,'Validation.json'),'w'),indent=2)
json.dump({'parts':[json.load(open(os.path.join(OUT,x['id'],'manifest.json'))) for x in items]},open(os.path.join(OUT,'PART_CATALOG.json'),'w'),indent=2)
print('HEAD_CORE_VERIFY_COMPLETE',len(results),flush=True)
