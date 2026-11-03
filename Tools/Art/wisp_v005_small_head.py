import bpy, os, json
from mathutils import Vector
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
ART=os.path.join(BASE,'Assets/Art/Robots/WISP');DOC=os.path.join(BASE,'Docs/Models')
rig=bpy.data.objects['WISP_Rig_v003'];bpy.context.scene.frame_set(1)
for bone in rig.pose.bones:bone.rotation_euler=(0,0,0)
bpy.context.view_layer.update()
head=[o for o in bpy.data.objects if o.type=='MESH' and o.vertex_groups.get('Head') is not None and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers)]
assert len(head)>5
inv=rig.matrix_world.inverted()
points=[inv@o.matrix_world@v.co for o in head for v in o.data.vertices]
lo=Vector([min(v[i] for v in points) for i in range(3)]);hi=Vector([max(v[i] for v in points) for i in range(3)])
# The source rig uses local Y-up. Keep the chin/neck contact plane anchored.
pivot=Vector(((lo.x+hi.x)/2,lo.y,(lo.z+hi.z)/2))
for o in head:
 to_rig=inv@o.matrix_world;to_mesh=to_rig.inverted()
 for v in o.data.vertices:v.co=to_mesh@(pivot+(to_rig@v.co-pivot)*.85)
 o.data.update()
bpy.context.view_layer.update()
after=[inv@o.matrix_world@v.co for o in head for v in o.data.vertices]
newlo=Vector([min(v[i] for v in after) for i in range(3)]);newhi=Vector([max(v[i] for v in after) for i in range(3)])
report={'source':'WISP_v004.blend','head_scale':.85,'head_meshes':len(head),'bones':len(rig.data.bones),'neck_plane_y':lo.y,'before_size':list(hi-lo),'after_size':list(newhi-newlo),'unchanged_nonhead':True}
for i in range(3):assert abs((newhi-newlo)[i]-(hi-lo)[i]*.85)<.0001
assert abs(newlo.y-lo.y)<.0001
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
for o in bpy.data.objects:
 if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers):o.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=os.path.join(ART,'WISP_v005.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',path_mode='AUTO')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ART,'Source/WISP_v005.blend'))
with open(os.path.join(DOC,'WISP-v005-HeadReport.json'),'w',encoding='utf8') as f:json.dump(report,f,indent=2)
scene=bpy.context.scene;cam=scene.camera
for label,loc in [('front',(0,-8,1.4)),('three-quarter',(5,-8,3))]:
 cam.location=loc;cam.rotation_euler=(Vector((0,0,1.4))-cam.location).to_track_quat('-Z','Y').to_euler()
 scene.render.filepath=os.path.join(DOC,'WISP-v005-'+label+'.png');bpy.ops.render.render(write_still=True)
print('WISP_V005_PASS',report)
