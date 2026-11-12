"""Use existing shoulder pivots to clear the larger forearms for review."""
import bpy,math
from mathutils import Matrix
SPREAD_DEGREES=20
IDLE_DEGREES=50

def pose_shoulders(layout):
    for side,s in [('R',1),('L',-1)]:
        bpy.data.objects[f'BD01_{side}_SHOULDER_ABDUCTION_Y'].rotation_euler.y=math.radians(-s*SPREAD_DEGREES)
    bpy.context.view_layer.update()
    for side,key in [('R','right_elbow'),('L','left_elbow')]:
        layout[key]=list(bpy.data.objects[side+'_UpperArm_Connector_Pin'].matrix_world.translation)
    layout['review_shoulder_spread_degrees']=SPREAD_DEGREES
    layout['review_idle_elbow_degrees']=IDLE_DEGREES

def set_forearm_angle(root,degrees,left=False):
    spread=SPREAD_DEGREES if left else -SPREAD_DEGREES
    root.rotation_euler=(Matrix.Rotation(math.radians(spread),4,'Y') @ Matrix.Rotation(math.radians(degrees),4,'X')).to_euler()
