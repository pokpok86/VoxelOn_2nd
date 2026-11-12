"""Reopen direct-mounted scenes; verify removed bars and unchanged weapon size/roll."""
import bpy,os,json,subprocess,tempfile,math
from mathutils import Matrix,Vector
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
REPO=os.path.abspath(os.path.join(ROOT,'../../..'))
parts=['W-01_LUMEN','W-02_BREAKER','W-03_RATTLE','W-04_EDGE','W-05_AEGIS','B-01_TWIN_DRIVE']
def snapshot(path):
    bpy.ops.wm.open_mainfile(filepath=path)
    return {o.name:o.matrix_basis.copy() for o in bpy.data.objects if o.type=='MESH' and o.name.startswith(('W-01_','W-02_','W-03_','W-04_','W-05_'))}
def err(a,b,n):return max(abs(a[i][j]-b[i][j]) for i in range(n) for j in range(n))
report={}
with tempfile.TemporaryDirectory(prefix='wisp-direct-') as temp:
    for part in parts:
        rel=f'Docs/Design/WISP_Parts/{part}/{part}-Mounted-Review.blend'
        old=os.path.join(temp,'before.blend')
        with open(old,'wb') as f:f.write(subprocess.check_output(['git','show','41d69a6:'+rel],cwd=REPO))
        before=snapshot(old);after=snapshot(os.path.join(REPO,rel))
        removed=[n for n in before if 'Offset_Link' in n or 'Offset_Bridge' in n]
        assert not removed and set(after)==set(before)
        assert not any('Offset_Link' in n or 'Offset_Bridge' in n for n in after)
        collars=0
        for name,a in before.items():
            if name not in after:continue
            b=after[name]
            if 'Elbow_' in name:
                assert err(a,b,4)<.00001,(part,name,'collar changed');collars+=1
            else:assert err(Matrix.Scale(.8,4) @ a,b,3)<.00001,(part,name,'20 percent reduction or roll mismatch')
        contacts=[]
        for o in bpy.data.objects:
            if not o.name.startswith(('W-01_','W-02_','W-03_','W-04_','W-05_')) or not o.name.endswith('_ROOT'):continue
            side='L' if 'LEFT_ROOT' in o.name else 'R'
            pin=bpy.data.objects[side+'_UpperArm_Connector_Pin']
            assert (o.matrix_world.translation-pin.matrix_world.translation).length<.00001
            assert all(abs(v-.42)<.00001 for v in o.scale)
            core=next(m for m in bpy.data.objects if m.parent==o and any(t in m.name for t in ('Structural_Barrel_Core','Main_Structural_Housing','Longitudinal_Blade_Chassis','Shield_Structural_Core')))
            pts=[o.matrix_world.inverted() @ core.matrix_world @ Vector(v) for v in core.bound_box]
            rear=max(v.y for v in pts)
            assert abs(rear+.035)<.00001,(part,side,rear)
            contacts.append({'side':side,'rear_plane_distance_m':abs(rear)*.42,'drum_radius_m':.062*.42,'root_matches_upper_arm_pin':True})
        report[part]={'offset_bars_removed':removed,'collar_local_transforms_preserved':collars,'weapon_body_scale_relative_to_41d69a6':.8,'roll_preserved':True,'direct_contacts':contacts,'PASS':True}
with open(os.path.join(ROOT,'Weapon-Scale-80-Validation.json'),'w') as f:json.dump(report,f,indent=2)
print('DIRECT_MOUNT_SAVED_PASS',len(report))
