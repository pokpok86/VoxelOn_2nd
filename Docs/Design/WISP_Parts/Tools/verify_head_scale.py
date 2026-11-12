"""Check helmet narrowing and preserve the optical camera at its prior size."""
import bpy,os,json,subprocess,tempfile
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPO=os.path.abspath(os.path.join(ROOT,'../../..'))
REL='Docs/Design/WISP_Base_v1/HD01/HD01.blend'
def snapshot(path):
    bpy.ops.wm.open_mainfile(filepath=path)
    return {o.name:[tuple(o.matrix_world @ v.co) for v in o.data.vertices]
            for o in bpy.data.objects if o.type=='MESH' and any(c.name=='HD01_SENSOR_HEAD' for c in o.users_collection)}
scale=(.81,.9,.9);offset=(0,.005,-.0037)
with tempfile.TemporaryDirectory(prefix='wisp-head90-') as temp:
    oldpath=os.path.join(temp,'prior.blend')
    with open(oldpath,'wb') as f:f.write(subprocess.check_output(['git','show','33d4df0:'+REL],cwd=REPO))
    old=snapshot(oldpath);new=snapshot(os.path.join(REPO,REL))
    assert old.keys()==new.keys()
    report={}
    for name,vertices in old.items():
        assert len(vertices)==len(new[name])
        is_camera=name in ('HD01_Continuous_Visor_Glass','HD01_Single_Cyan_Optical_Slit') or name.startswith('Sensor_Bezel_')
        if is_camera:
            expected=[tuple(v[i]+offset[i] for i in range(3)) for v in vertices]
            rule='camera size and relative geometry unchanged; group translated'
        elif name=='HD01_Fixed_Neck_Interface':
            expected=vertices;rule='neck connector unchanged'
        else:
            expected=[(v[0]*.81,v[1]*.9,-.04+(v[2]+.04)*.9) for v in vertices]
            rule='helmet width 81%, depth/height 90% around fixed lower contact'
        error=max(abs(a[i]-b[i]) for a,b in zip(expected,new[name]) for i in range(3))
        assert error<.00001,(name,error)
        report[name]={'rule':rule,'maximum_vertex_error_m':error,'PASS':True}
with open(os.path.join(ROOT,'Head-90-Width-90-Validation.json'),'w') as f:json.dump(report,f,indent=2)
print('HEAD_90_WIDTH_90_PASS',len(report))
