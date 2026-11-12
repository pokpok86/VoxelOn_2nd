"""Measure the saved upper arms against the prior approved proportions."""
import bpy,os,json,subprocess,tempfile
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPO=os.path.abspath(os.path.join(ROOT,'../../..'))
REL='Docs/Design/WISP_Base_v1/BD01/BD01.blend'
def snapshot(path):
    bpy.ops.wm.open_mainfile(filepath=path)
    return {o.name:[tuple(o.matrix_world @ v.co) for v in o.data.vertices]
            for o in bpy.data.objects if o.type=='MESH' and not o.name.startswith('GUIDE')}
def bounds(points):return [(min(p[i] for p in points),max(p[i] for p in points)) for i in range(3)]
with tempfile.TemporaryDirectory(prefix='wisp-arm-length-') as temp:
    prior=os.path.join(temp,'prior.blend')
    with open(prior,'wb') as f:f.write(subprocess.check_output(['git','show','9159094:'+REL],cwd=REPO))
    old=snapshot(prior);new=snapshot(os.path.join(REPO,REL))
    assert old.keys()==new.keys()
    report={}
    for side in ('L','R'):
        name=side+'_UpperArm_Connector_Stub'
        a,b=bounds(old[name]),bounds(new[name])
        ratio=(b[2][1]-b[2][0])/(a[2][1]-a[2][0])
        assert abs(ratio-1.5)<.00001
        assert abs(a[2][1]-b[2][1])<.00001,'Shoulder-side anchor moved'
        assert all(abs((a[i][1]-a[i][0])-(b[i][1]-b[i][0]))<.00001 for i in (0,1))
        pin=side+'_UpperArm_Connector_Pin'
        pa,pb=bounds(old[pin]),bounds(new[pin])
        assert all(abs((pa[i][1]-pa[i][0])-(pb[i][1]-pb[i][0]))<.00001 for i in range(3))
        assert abs((pb[2][0]-pa[2][0])-(b[2][0]-a[2][0]))<.00001
        report[side]={'measured_length_ratio':ratio,'new_upper_arm_height_m':b[2][1]-b[2][0],
                      'width_depth_and_top_anchor_preserved':True,'elbow_pin_follows_lower_end':True}
    for name,points in old.items():
        if 'UpperArm_Connector_' in name:continue
        assert len(points)==len(new[name])
        assert max(abs(a[i]-b[i]) for a,b in zip(points,new[name]) for i in range(3))<.00001,(name,'unrelated geometry changed')
with open(os.path.join(ROOT,'UpperArm-Length-Validation.json'),'w') as f:json.dump(report,f,indent=2)
print('UPPER_ARM_LENGTH_PASS',report)
