"""Measure a 10% torso reduction while retaining approved upper arms."""
import bpy,os,json,subprocess,tempfile
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPO=os.path.abspath(os.path.join(ROOT,'../../..'))
REL='Docs/Design/WISP_Base_v1/BD01/BD01.blend'
def snapshot(path):
    bpy.ops.wm.open_mainfile(filepath=path)
    vertices={o.name:[tuple(o.matrix_world @ v.co) for v in o.data.vertices]
              for o in bpy.data.objects if o.type=='MESH' and any(c.name=='BD01_BODY_AND_SHOULDERS' for c in o.users_collection)}
    pivots={s:tuple(bpy.data.objects[f'BD01_{s}_SHOULDER_PITCH_X'].matrix_world.translation) for s in ('L','R')}
    return vertices,pivots
def bounds(points):return [(min(p[i] for p in points),max(p[i] for p in points)) for i in range(3)]
with tempfile.TemporaryDirectory(prefix='wisp-body90-') as temp:
    oldpath=os.path.join(temp,'old.blend')
    with open(oldpath,'wb') as f:f.write(subprocess.check_output(['git','show','397eb97:'+REL],cwd=REPO))
    old,op=snapshot(oldpath);new,np=snapshot(os.path.join(REPO,REL))
    assert old.keys()==new.keys()
    resized=[];arms=[];interfaces=[]
    for name,points in old.items():
        assert len(points)==len(new[name])
        if 'UpperArm_Connector_' in name:
            side=name[0]
            assert max(abs((a[i]-op[side][i])-(b[i]-np[side][i])) for a,b in zip(points,new[name]) for i in range(3))<.00001,name
            arms.append(name)
        elif name in ('Neck_Head_Interface','Waist_Pelvis_Interface_Key'):
            a,b=bounds(points),bounds(new[name])
            assert all(abs((a[i][1]-a[i][0])-(b[i][1]-b[i][0]))<.00001 for i in range(3)),name
            interfaces.append(name)
        else:
            assert max(abs(a[i]*.9-b[i]) for a,b in zip(points,new[name]) for i in range(3))<.00001,name
            resized.append(name)
report={'torso_scale_relative_to_397eb97':.9,'scaled_torso_shoulders':resized,
        'upper_arm_shape_and_offset_from_shoulder_preserved':arms,
        'mating_interface_dimensions_preserved':interfaces,'PASS':True}
with open(os.path.join(ROOT,'Body-90-Geometry-Validation.json'),'w') as f:json.dump(report,f,indent=2)
print('BODY_90_GEOMETRY_PASS',len(resized),len(arms),len(interfaces))
