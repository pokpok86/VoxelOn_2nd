"""Reopen saved mounted scenes and compare fixed connectors to Git HEAD."""
import bpy,os,sys,json,subprocess,tempfile,math
from mathutils import Matrix
sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
from weapon_mount_style import BODY_SCALE
from assembly_layout import load_layout
from assembly_pose import SPREAD_DEGREES, IDLE_DEGREES
layout=load_layout(os.path.abspath(os.path.join(os.path.dirname(__file__),'../../WISP_Base_v1')))
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPO=os.path.abspath(os.path.join(ROOT,'../../..'))
parts=['W-01_LUMEN','W-02_BREAKER','W-03_RATTLE','W-04_EDGE','W-05_AEGIS','B-01_TWIN_DRIVE']
report={}
def snapshot(path):
    bpy.ops.wm.open_mainfile(filepath=path)
    return {o.name:{'matrix':o.matrix_basis.copy(),'parent':o.parent.name if o.parent else None}
            for o in bpy.data.objects if o.name.startswith(('W-01_','W-02_','W-03_','W-04_','W-05_')) and o.type in ('MESH','EMPTY')}
def error(a,b):return max(abs(a[i][j]-b[i][j]) for i in range(4) for j in range(4))
with tempfile.TemporaryDirectory(prefix='wisp-mount-check-') as temp:
    for part in parts:
        rel=f'Docs/Design/WISP_Parts/{part}/{part}-Mounted-Review.blend'
        old=os.path.join(temp,'prior.blend')
        with open(old,'wb') as f:f.write(subprocess.check_output(['git','show','107ca7c:'+rel],cwd=REPO))
        before=snapshot(old);after=snapshot(os.path.join(REPO,rel))
        with open(os.path.join(ROOT,part,'manifest.json'),encoding='utf-8') as f:layout=json.load(f)['assembly_layout']
        assert before.keys()==after.keys(),'Mounted object names changed'
        fixed=changed=0;largest=0
        for name,a in before.items():
            b=after[name]
            assert a['parent']==b['parent']
            if a['parent'] is None:
                expected=Matrix.Rotation(math.radians(SPREAD_DEGREES if 'LEFT_ROOT' in name else -SPREAD_DEGREES),4,'Y') @ Matrix.Rotation(math.radians(IDLE_DEGREES),4,'X') @ Matrix.Scale(.42,4)
                expected.translation=layout['left_elbow' if 'LEFT_ROOT' in name else 'right_elbow']
                fixed+=1
            elif 'Elbow_' in name:
                expected=a['matrix'];fixed+=1
            else:
                left='LEFT_ROOT' in a['parent']
                angle=(90 if left else -90) if name.startswith(('W-04_','W-05_')) else 0
                expected=Matrix.Scale(1.50,4) @ a['matrix'];changed+=1
            e=error(expected,b['matrix']);largest=max(largest,e)
            assert e<.00001,(part,name,e)
        report[part]={'validated_root_and_fixed_local_collar_count':fixed,'scaled_housing_count':changed,'saved_transform_max_error':largest,'PASS':True}
with open(os.path.join(ROOT,'Weapon-Mount-Saved-Validation.json'),'w') as f:json.dump(report,f,indent=2)
print('SAVED_MOUNT_TRANSFORMS_PASS',len(report))
