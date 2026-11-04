import bpy,os,math,json
from mathutils import Vector,Matrix
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
SOURCE=os.path.join(BASE,'Assets/Art/Robots/WISP/Source/WISP_v005.blend')
def build(kind):
 bpy.ops.wm.open_mainfile(filepath=SOURCE)
 rig=bpy.data.objects['WISP_Rig_v003']
 for p in rig.pose.bones:p.rotation_euler=(0,0,0)
 for o in list(bpy.data.objects):
  if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers):
   if any(g.name.startswith(('Left_thigh','Right_thigh','Left_knee','Right_knee','Left_ankle','Right_ankle')) or g.name=='Legs' for g in o.vertex_groups):bpy.data.objects.remove(o,do_unlink=True)
 armor=bpy.data.materials.new(kind+'_slate');armor.diffuse_color=(.22,.29,.28,1) if kind=='BASTION' else (.27,.24,.34,1)
 dark=bpy.data.materials.new(kind+'_mechanical');dark.diffuse_color=(.055,.075,.09,1)
 trim=bpy.data.materials.new(kind+'_edge');trim.diffuse_color=(.41,.46,.47,1)
 def bone(name,parent,at):
  bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
  b=rig.data.edit_bones.new(name);b.head=at;b.tail=Vector(at)+Vector((0,.12,0));b.parent=rig.data.edit_bones[parent]
  bpy.ops.object.mode_set(mode='OBJECT');return name
 def box(name,at,size,mat,weight='Legs',rot=None):
  bpy.ops.mesh.primitive_cube_add(size=1)
  o=bpy.context.object;o.name='PROTO_'+name
  # Geometry is expressed in rig local Y-up; apply the rig transform to world.
  transform=Matrix.Translation(Vector(at))
  if rot is not None:transform=transform@rot.to_matrix().to_4x4()
  transform=transform@Matrix.Diagonal(Vector((*size,1)))
  o.matrix_world=rig.matrix_world@transform
  bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
  bevel=o.modifiers.new('Machined edges','BEVEL');bevel.width=.025;bevel.segments=1
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name)
  o.data.materials.append(mat);g=o.vertex_groups.new(name=weight);g.add(list(range(len(o.data.vertices))),1,'REPLACE')
  arm=o.modifiers.new('Chassis articulation','ARMATURE');arm.object=rig;o.parent=rig;o.matrix_world=rig.matrix_world@transform.normalized() # scale was baked above
  return o
 def strut(name,a,b,width,mat,weight):
  a,b=Vector(a),Vector(b);delta=b-a
  return box(name,(a+b)/2,(width,delta.length,width),mat,weight,delta.to_track_quat('Y','Z'))
 # Structural shoulder housings support the reused upper rig without replacing its geometry.
 for side,sign in [('Left',1),('Right',-1)]:
  box('Shoulder axle '+side,(sign*.59,1.60,0),(.22,.30,.34),dark,side+'_shoulder')
  box('Shoulder collar '+side,(sign*.62,1.63,0),(.12,.34,.39),trim,side+'_shoulder')
 if kind=='BASTION':
  box('Tank chassis',(0,.77,0),(1.38,.35,1.55),armor)
  box('Tank forward glacis',(0,.69,.81),(1.05,.26,.24),trim)
  for side,sign in enumerate([-1,1]):
   x=sign*.83
   box('Track side housing '+str(side),(x,.46,0),(.43,.42,1.35),dark)
   box('Track upper armor '+str(side),(x,.91,0),(.64,.15,1.72),armor)
   for j in range(5):
    z=-.58+j*.29
    w=bone('Wheel_%d_%d'%(side,j),'Legs',(x,.4,z))
    box('Road wheel %d %d'%(side,j),(x,.40,z),(.47,.34,.27),trim,w)
   for j in range(40):
    a=2*math.pi*j/40;y=.43+.39*math.cos(a);z=.80*math.sin(a)
    w=bone('Tread_%d_%02d'%(side,j),'Legs',(x,y,z))
    box('Tread link %d %02d'%(side,j),(x,y,z),(.58,.09,.14),dark,w)
 else:
  box('Spider chassis',(0,.91,0),(1.30,.30,1.25),armor)
  box('Spider thorax',(0,1.08,.20),(.90,.13,1.05),trim)
  for i,(sx,sz) in enumerate([(-1,1),(1,1),(-1,-1),(1,-1)]):
   hip=(sx*.55,.88,sz*.40);knee=(sx*1.04,.64,sz*.78);toe=(sx*1.30,.075,sz*1.04)
   h=bone('SpiderHip_'+str(i),'Legs',hip);k=bone('SpiderKnee_'+str(i),h,knee);t=bone('SpiderFoot_'+str(i),k,toe)
   box('Hip casing '+str(i),hip,(.32,.30,.33),dark,h)
   strut('Upper leg '+str(i),hip,knee,.24,armor,h)
   box('Knee armor '+str(i),knee,(.32,.28,.34),trim,k)
   strut('Lower leg '+str(i),knee,toe,.20,armor,k)
   box('Foot contact '+str(i),(toe[0],.045,toe[2]),(.29,.09,.36),dark,t)
 out=os.path.join(BASE,'Assets/Art/Robots',kind);os.makedirs(os.path.join(out,'Source'),exist_ok=True)
 bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
 for o in bpy.data.objects:
  if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers):o.select_set(True)
 bpy.context.view_layer.objects.active=rig
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,kind+'_v001.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL')
 bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out,'Source',kind+'_v001.blend'))
 print('CHASSIS_EXPORT_PASS',kind,len(rig.data.bones))
for kind in ['BASTION','TARANTULA']:build(kind)
