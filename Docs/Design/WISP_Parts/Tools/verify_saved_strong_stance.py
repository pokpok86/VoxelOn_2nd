"""Check actual saved strong-stance angles and planted feet without rebuilding assets."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).resolve().parent.parent/'Biped-StrongStance-20261112'
data=json.loads((OUT/'Validation.json').read_text(encoding='utf-8'));results=[]
for record in data['parts']:
    part=record['id']
    for mounted in [False,True]:
        suffix='-Mounted' if mounted else '';path=OUT/part/(part+suffix+'.blend');bpy.ops.wm.open_mainfile(filepath=str(path))
        hips=[];feet=[]
        for side,sign in [('L',-1),('R',1)]:
            yaw=bpy.data.objects[part+'_'+side+'_HIP_YAW_Z'];ab=bpy.data.objects[part+'_'+side+'_HIP_ABDUCTION_Y'];knee=bpy.data.objects[part+'_'+side+'_KNEE_PITCH_X'];ankle=bpy.data.objects[part+'_'+side+'_ANKLE_PITCH_X']
            assert abs(math.degrees(yaw.rotation_euler.z)-sign*15)<.001
            assert abs(math.degrees(ab.rotation_euler.y)+sign*10)<.001
            assert abs(math.degrees(knee.rotation_euler.x)-12)<.001
            forward=ankle.matrix_world.to_3x3()@Vector((0,-1,0));up=ankle.matrix_world.to_3x3()@Vector((0,0,1))
            heading=math.degrees(math.atan2(forward.x,-forward.y));assert abs(heading+sign*10)<.001
            assert up.z>.999999
            sole=bpy.data.objects[side+'_Foot_Sole'];points=[sole.matrix_world@v.co for v in sole.data.vertices];bottom=sorted(p.z for p in points)[:8]
            assert max(abs(z) for z in bottom)<.000001
            hips.append(yaw.matrix_world.translation.x);feet.append({'side':side,'heading_deg':heading,'sole_horizontal':True,'grounded':True})
        expected=record['states'][int(mounted)]['hip_centers_after_m'];assert abs(hips[1]-expected['R'][0])<1e-6 and abs(hips[0]-expected['L'][0])<1e-6
        results.append({'file':str(path.relative_to(OUT)),'saved_pose_verified':True,'hip_spacing_m':hips[1]-hips[0],'feet':feet})
(OUT/'Saved-Pose-Recheck.json').write_text(json.dumps({'status':'PASS','files_checked':len(results),'results':results,'unity_test':'NOT_PERFORMED'},indent=2),encoding='utf-8')
print('SAVED_STANCE_RECHECK_PASS',len(results),flush=True)
