import bpy, math, os, json
from mathutils import Vector, Matrix
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
OUT=os.path.join(BASE,'Assets/Art/Robots/WISP')
DOC=os.path.join(BASE,'Docs/Models')
root=bpy.data.objects['Root']
for img in bpy.data.images:
 key=(img.name+' '+img.filepath).lower()
 if img.source=='FILE':
  tex='WISP_Emission_v002.png' if 'emission' in key else 'WISP_BaseColor_v002.png'
  img.filepath=os.path.join(OUT,'Textures',tex)
  img.reload()
bpy.ops.file.pack_all()
# Static v002 comparison geometry, copied before edits.
before=[]
def under(obj, ancestor):
 p=obj
 while p:
  if p==ancestor:return True
  p=p.parent
 return False
def preview(obj):
 p=obj
 while p:
  if p.name in {'WISP_Rifle','WISP_Shield'}:return True
  p=p.parent
 return False
source=[o for o in bpy.data.objects if o.type=='MESH' and under(o,root) and not preview(o)]
for o in source:
 c=bpy.data.objects.new('BEFORE_'+o.name,o.data.copy()); bpy.context.collection.objects.link(c)
 c.matrix_world=o.matrix_world.copy(); c.location.x-=1.75; before.append(c)
# Root-local Unity coordinates: X lateral, Y up, Z forward.
head=bpy.data.objects['Head']; head.scale=(1.12,1.16,1.04); head.location.y=2.04
core=bpy.data.objects['Core']; core.scale=(1.06,.87,1.0); core.location.y=1.34
# Compact arms with shoulders below the jaw and a relaxed outward hang.
for side,sign in [('Left',1),('Right',-1)]:
 shoulder=bpy.data.objects[side+'_shoulder']; shoulder.location=(sign*.65,1.64,0)
 shoulder.scale=(1.02,.90,1.02)
 elbow=bpy.data.objects[side+'_elbow']; elbow.location.x=sign*.095
 shoulder.rotation_euler.z=math.radians(sign*7)
 hip=bpy.data.objects[side+'_thigh']; hip.location.x=sign*.30; hip.location.y=.96
 hip.scale=(1.10,.96,1.03)
 hip.rotation_euler.x=math.radians(-7)
 knee=bpy.data.objects[side+'_knee']; knee.rotation_euler.x=math.radians(14)
 knee.scale.x=1.12
 ankle=bpy.data.objects[side+'_ankle']; ankle.rotation_euler.x=math.radians(-7)
 ankle.scale=(1.06,1.0,.90)
 ankle.rotation_euler.y=math.radians(sign*5)
for o in source:
 if o.name.startswith('Boot sole'):o.scale.y*=.55
bpy.context.view_layer.update()
for o in source:
 if under(o,bpy.data.objects['Torso']):
  to_root=root.matrix_world.inverted()@o.matrix_world
  from_root=to_root.inverted()
  for v in o.data.vertices:
   p=to_root@v.co; p.x*=.82+.24*max(0,min(1,(p.y-1.05)/.6)); v.co=from_root@p
bpy.context.view_layer.update()
# Ground the bent rest stance using actual sole mesh vertices.
inv=root.matrix_world.inverted()
min_y=min((inv@o.matrix_world@v.co).y for o in source for v in o.data.vertices)
for name in ('Head','Core','Arms','Legs'):
 bpy.data.objects[name].location.y-=min_y
bpy.context.view_layer.update()
# Freeze the new rest shape into vertices, then bind rigid mechanical parts to bones.
inv=root.matrix_world.inverted()
joints=[o for o in bpy.data.objects if o.type=='EMPTY' and under(o,root) and not preview(o)]
positions={o.name:inv@o.matrix_world.translation for o in joints}
parents={o.name:o.parent.name if o.parent in joints else None for o in joints}
mesh_info=[]
for o in source:
 parent=o.parent
 while parent and parent.type!='EMPTY':parent=parent.parent
 bone=parent.name if parent and parent.name in positions else 'Root'
 coords=[inv@o.matrix_world@v.co for v in o.data.vertices]
 mesh_info.append((o,bone,coords))
arm=bpy.data.armatures.new('WISP_v003_ArticulatedRig')
rig=bpy.data.objects.new('WISP_Rig_v003',arm); bpy.context.collection.objects.link(rig)
rig.matrix_world=root.matrix_world.copy(); rig.show_in_front=True
bpy.context.view_layer.objects.active=rig; rig.select_set(True)
bpy.ops.object.mode_set(mode='EDIT')
for name,pos in positions.items():
 b=arm.edit_bones.new(name); b.head=pos; b.tail=pos+Vector((0,.12,0))
for name,parent in parents.items():
 if parent:arm.edit_bones[name].parent=arm.edit_bones[parent]
bpy.ops.object.mode_set(mode='OBJECT')
for o,bone,coords in mesh_info:
 o.parent=rig; o.matrix_parent_inverse=Matrix.Identity(4); o.matrix_basis=Matrix.Identity(4)
 for v,co in zip(o.data.vertices,coords):v.co=co
 o.data.update(); group=o.vertex_groups.new(name=bone); group.add(list(range(len(coords))),1,'REPLACE')
 mod=o.modifiers.new('Rigid mechanical bone skin','ARMATURE'); mod.object=rig
# Compatibility sockets are bones, and every visible part has a real weighted joint.
for o in list(joints):bpy.data.objects.remove(o,do_unlink=True)
# Remove weapon preview objects from this model-only deliverable; source v002 is untouched.
for o in list(bpy.data.objects):
 if preview(o):bpy.data.objects.remove(o,do_unlink=True)
# Renderer material moderation; retain atlas colors/UVs rather than changing voxel resolution.
for mat in bpy.data.materials:
 if mat.use_nodes:
  for node in mat.node_tree.nodes:
   if node.type=='BSDF_PRINCIPLED':
    node.inputs['Roughness'].default_value=.55
    if 'Emission Strength' in node.inputs:node.inputs['Emission Strength'].default_value=.4
# Blender walk preview action, neutral rig pose is frame 1.
scene=bpy.context.scene; scene.frame_start=1; scene.frame_end=49
for frame in (1,13,25,37,49):
 t=(frame-1)/48*2*math.pi
 for side,phase in [('Left',0),('Right',math.pi)]:
  for suffix,angle in [('thigh',math.sin(t+phase)*16),('knee',max(0,math.sin(t+phase))*24),('ankle',-max(0,math.sin(t+phase))*12),('shoulder',-math.sin(t+phase)*9)]:
   p=rig.pose.bones[side+'_'+suffix]; p.rotation_mode='XYZ'; p.rotation_euler.x=math.radians(angle)
   p.keyframe_insert('rotation_euler',frame=frame,group=side+' leg' if suffix!='shoulder' else side+' arm')
scene.frame_set(1)
# All bone animation deltas are zero in the saved standing pose.
for p in rig.pose.bones:p.rotation_euler=(0,0,0)
bpy.context.view_layer.update()
# Export the armature and weighted meshes. Keep v001/v002 intact.
bpy.ops.object.select_all(action='DESELECT'); rig.select_set(True)
for o,_,_ in mesh_info:o.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,'WISP_v003.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',path_mode='AUTO')
# Clean model file contains the armature, atlas meshes and walk preview only.
for o in before:o.hide_render=True; o.hide_viewport=True
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'Source/WISP_v003.blend'))
# Consistent front comparison, no perspective distortion.
for o in list(bpy.data.objects):
 if o.type in {'CAMERA','LIGHT'}:bpy.data.objects.remove(o,do_unlink=True)
for o in before:o.hide_render=False
rig.location.x+=1.75
scene.world.color=(.18,.18,.18)
scene.render.engine='CYCLES'; scene.cycles.samples=24
scene.render.resolution_x=1400; scene.render.resolution_y=850; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
# Camera sees source front (-Y world), vertical world Z.
def aim(obj,point):obj.rotation_euler=(Vector(point)-obj.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('Comparison camera'); cam=bpy.data.objects.new('Comparison camera',camdata); bpy.context.collection.objects.link(cam)
cam.location=(0,-9,1.4); aim(cam,(0,0,1.4)); camdata.type='ORTHO'; camdata.ortho_scale=7.3; scene.camera=cam
for name,loc,power,size in [('Key',(-3,-4,6),450,5),('Fill',(4,-2,4),250,4),('Rim',(0,4,5),350,4)]:
 d=bpy.data.lights.new(name,'AREA'); d.energy=power; d.shape='DISK'; d.size=size
 o=bpy.data.objects.new(name,d); bpy.context.collection.objects.link(o); o.location=loc; aim(o,(0,0,1.3))
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.008)); floor=bpy.context.object
m=bpy.data.materials.new('Neutral stage');m.diffuse_color=(.16,.18,.21,1);floor.data.materials.append(m)
for label,x in [('v002 / BEFORE',-1.75),('v003 / PROPORTION + RIG',1.75)]:
 d=bpy.data.curves.new(label,'FONT');d.body=label;d.align_x='CENTER';d.size=.15
 o=bpy.data.objects.new(label,d);bpy.context.collection.objects.link(o);o.location=(x,-.6,-.23);o.rotation_euler=(math.pi/2,0,0)
scene.render.filepath=os.path.join(DOC,'WISP-v003-front-comparison.png');bpy.ops.render.render(write_still=True)
# Separate front and side review images.
for o in before:o.hide_render=True
rig.location.x-=1.75
camdata.ortho_scale=3.6;scene.render.resolution_x=850;scene.render.resolution_y=850
for suffix,loc in [('front',(0,-8,1.35)),('side',(8,0,1.35)),('rear',(0,8,1.35))]:
 cam.location=loc;aim(cam,(0,0,1.35));scene.render.filepath=os.path.join(DOC,'WISP-v003-'+suffix+'.png');bpy.ops.render.render(write_still=True)
report={'bones':len(arm.bones),'weighted_meshes':len(mesh_info),'head_width_scale':1.12,'torso_height_scale':.87,'standing_hip_degrees':-7,'standing_knee_degrees':14,'standing_ankle_degrees':-7,'source_sole_y_before_grounding':min_y,'bone_positions_root_local':{k:list(v) for k,v in positions.items()}}
open(os.path.join(DOC,'WISP-v003-RigReport.json'),'w').write(json.dumps(report,indent=2))
print('WISP_V003_EXPORT_PASS',len(arm.bones),'bones',len(mesh_info),'weighted meshes')



