import bpy, os, math
from mathutils import Vector
src = r"D:\Codex\VoxSky\Builds\WorkLatest\Assets\Art\Robots\BASTION\Source\BASTION_v009.blend"
out_blend = r"D:\Codex\VoxSky\Builds\WorkLatest\Assets\Art\Robots\BASTION\Source\BASTION_v010.blend"
out_fbx = r"D:\Codex\VoxSky\Builds\WorkLatest\Assets\Art\Robots\BASTION\BASTION_v010.fbx"
review_png = r"D:\Codex\VoxSky\Builds\WorkLatest\Docs\Validation\Customization-20261106\BASTION-v010-Blender.png"
os.makedirs(os.path.dirname(review_png), exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=src)
rig = bpy.data.objects.get('WISP_Rig_v003')
assert rig is not None and rig.type == 'ARMATURE'
required = ['Root','Arms','Left_shoulder','Left_elbow','Left_hand','Left_GRIP_SOCKET','Right_shoulder','Right_elbow','Right_hand','Right_GRIP_SOCKET','Core','BACKPACK_SOCKET','Pelvis','Head','Legs','Left_thigh','Left_knee','Left_ankle','Right_thigh','Right_knee','Right_ankle','Wheel_0_0','Wheel_1_0','Tread_0_00','Tread_1_00']
assert all(rig.data.bones.get(n) for n in required), 'required bone missing'
def scale_mesh(name, factors):
    obj=bpy.data.objects.get(name)
    assert obj is not None and obj.type=='MESH', name
    obj.data=obj.data.copy()
    mins=[min(v.co[i] for v in obj.data.vertices) for i in range(3)]
    maxs=[max(v.co[i] for v in obj.data.vertices) for i in range(3)]
    center=[(mins[i]+maxs[i])*.5 for i in range(3)]
    for v in obj.data.vertices:
        for i in range(3): v.co[i]=center[i]+(v.co[i]-center[i])*factors[i]
    obj.data.update()
# Preserve the complete rig/socket structure. Reshape only the Blender-authored surface meshes.
# The torso reads as a single sturdy compact shell, with a smaller integrated sensor hood.
scale_mesh('VN_Integrated chamfered hull',(1.18,1.12,1.12))
scale_mesh('VN_Faceted sensor hood',(1.08,1.04,1.08))
scale_mesh('VN_Sloped chest armor',(1.14,1.10,1.10))
scale_mesh('VN_Tapered lower hull',(1.10,1.08,1.08))
scale_mesh('VN_Chassis front apron',(1.14,1.08,1.08))
scale_mesh('VN_Sloped front apron',(1.12,1.06,1.08))
# Make the Nacchin-like safety-orange shoulder pods a defining silhouette, not tiny trim.
for side in ('Left','Right'):
    scale_mesh('VN_Orange shoulder '+side,(1.52,1.32,1.30))
    scale_mesh('VN_Shoulder endplate '+side,(1.24,1.18,1.18))
    scale_mesh('VN_Shoulder axle '+side,(1.14,1.14,1.12))
# Broaden the visible continuous running gear and ivory guards while retaining every tread/roller joint.
for name in ('VN_Track side housing 0','VN_Track side housing 1'):
    scale_mesh(name,(1.08,1.10,1.08))
for name in ('VN_Track ivory cover 0','VN_Track ivory cover 1'):
    scale_mesh(name,(1.08,1.10,1.04))
# Keep two small cyan optics, dark articulation and the original legible white/orange materials.
assert bpy.data.materials.get('VN_Ivory') and bpy.data.materials.get('VN_Orange') and bpy.data.materials.get('VN_Dark') and bpy.data.materials.get('VN_Eyes')
# Preserve the imported root and all bone names used by Unity weapon, shoulder and track animation.
# Remove presentation-only camera and lights from the exported model, but leave them in the source scene.
source_objects=list(bpy.context.scene.objects)
bpy.ops.wm.save_as_mainfile(filepath=out_blend)
for o in bpy.context.selected_objects: o.select_set(False)
for o in source_objects:
    if o==rig or o.parent==rig or (o.parent and o.parent.parent==rig): o.select_set(True)
# FBX axis and armature options match the established Unity-import convention.
bpy.ops.export_scene.fbx(filepath=out_fbx, use_selection=True, object_types={'ARMATURE','MESH','EMPTY'},
    global_scale=1.0, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y',
    use_mesh_modifiers=True, add_leaf_bones=False, bake_anim=False, use_custom_props=True,
    colors_type='SRGB', path_mode='AUTO')
# Render the revised Blender source through the preserved artist review camera for visual review.
cam=bpy.data.objects.get('WISP v004 review camera')
if cam:
    scene=bpy.context.scene; scene.camera=cam; scene.render.resolution_x=960; scene.render.resolution_y=720; scene.render.resolution_percentage=100
    scene.render.filepath=review_png; bpy.ops.render.render(write_still=True)
print('BASTION_V010_EXPORTED',out_blend,out_fbx)
print('BASTION_V010_BONES',len(rig.data.bones),'OBJECTS',len([o for o in bpy.data.objects if o.type=='MESH']))
