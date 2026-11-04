import bpy,os,math,json
from mathutils import Matrix,Vector
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest';SOURCE=os.path.join(BASE,'Assets/Art/Robots/WISP/Source/WISP_v005.blend')
DOC=os.path.join(BASE,'Docs/Screenshots/NacchinStyle-20261105/Iteration02');os.makedirs(DOC,exist_ok=True)
PALETTE={'Ivory':(.84,.85,.80),'Orange':(.98,.33,.035),'Dark':(.045,.055,.052),'Trim':(.20,.24,.22),'Eyes':(.035,.66,.73),'Red':(.39,.075,.04)}
reports=[]
def build(kind):
 bpy.ops.wm.open_mainfile(filepath=SOURCE);rig=bpy.data.objects['WISP_Rig_v003']
 for p in rig.pose.bones:p.rotation_euler=(0,0,0)
 for o in list(bpy.data.objects):
  if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers):bpy.data.objects.remove(o,do_unlink=True)
 armDelta={}
 bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
 for bone in rig.data.edit_bones:
  if bone.name.startswith(('Left_','Right_')) or bone.name in ['RIFLE_SOCKET','SWORD_SOCKET','SHIELD_SOCKET']:
   arm=bone
   while arm.parent and arm.parent.name!='Arms':arm=arm.parent
   if arm.parent is None or arm.parent.name!='Arms':continue
   sign=1 if arm.name.startswith('Left') else -1
   amount=(.12 if 'shoulder' in bone.name else .42 if 'hand' in bone.name or bone.name in ['RIFLE_SOCKET','SWORD_SOCKET'] else .27) if kind=='BASTION' else -.03
   delta=Vector((sign*amount,0,0));bone.head+=delta;bone.tail+=delta;armDelta[bone.name]=delta
 bpy.ops.object.mode_set(mode='OBJECT')
 materials={}
 for n,c in PALETTE.items():
  m=bpy.data.materials.new('VN_'+n);m.diffuse_color=(*c,1);m.use_nodes=True;shader=m.node_tree.nodes.get('Principled BSDF');shader.inputs['Base Color'].default_value=(*c,1);shader.inputs['Metallic'].default_value=.08;shader.inputs['Roughness'].default_value=.65
  if n=='Eyes':shader.inputs['Emission Color'].default_value=(*c,1);shader.inputs['Emission Strength'].default_value=1.3
  materials[n]=m
 made=[]
 def finish(o,name,weight,mat):
  o.name='VN_'+name;world=o.matrix_world.copy();o.parent=rig;o.matrix_world=world;o.data.materials.append(materials[mat]);group=o.vertex_groups.new(name=weight);group.add(list(range(len(o.data.vertices))),1,'REPLACE');mod=o.modifiers.new('Mechanical rig','ARMATURE');mod.object=rig;made.append(o);return o
 def box(name,at,size,weight='Torso',mat='Ivory',bevel=.04,rotation=None):
  bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;matrix=Matrix.Translation(Vector(at)+armDelta.get(weight,Vector((0,0,0))))
  if rotation:matrix=matrix@rotation.to_matrix().to_4x4()
  o.matrix_world=rig.matrix_world@matrix@Matrix.Diagonal(Vector((*size,1)));bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
  if bevel:
   mod=o.modifiers.new('Broad armor edge','BEVEL');mod.width=min(bevel,min(size)*.22);mod.segments=1;bpy.ops.object.modifier_apply(modifier=mod.name)
  return finish(o,name,weight,mat)
 def rod(name,a,b,r,weight,mat='Dark'):
  a,b=Vector(a),Vector(b);d=b-a;bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=r,depth=d.length);o=bpy.context.object;o.matrix_world=rig.matrix_world@Matrix.Translation((a+b)/2)@d.to_track_quat('Z','Y').to_matrix().to_4x4();return finish(o,name,weight,mat)
 def newbone(name,parent,at):
  bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT');b=rig.data.edit_bones.new(name);b.head=at;b.tail=Vector(at)+Vector((0,.12,0));b.parent=rig.data.edit_bones[parent];bpy.ops.object.mode_set(mode='OBJECT');return name
 # Wide integrated hull, small protected face and deliberate broad panels.
 def prism(name,profile,halfwidth,weight='Torso',mat='Ivory'):
  verts=[(x,y,z) for x in [-halfwidth,halfwidth] for y,z in profile];n=len(profile);faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
  for i in range(n):j=(i+1)%n;faces.append((i,j,j+n,i+n))
  faces=[tuple(reversed(face)) for face in faces]
  mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.matrix_world=rig.matrix_world
  bpy.context.view_layer.objects.active=o;mod=o.modifiers.new('Large chamfer','BEVEL');mod.width=.04;mod.segments=1;bpy.ops.object.modifier_apply(modifier=mod.name);return finish(o,name,weight,mat)
 prism('Integrated sloping hull',[(.80,-.43),(.80,.46),(1.20,.46),(1.61,.20),(1.61,-.43)],.60)
 box('Front chest plate',(0,1.07,.475),(1.02,.27,.065),bevel=.025)
 from mathutils import Euler
 box('Sloped chest armor',(0,1.405,.335),(1.02,.46,.055),bevel=.02,rotation=Euler((math.radians(-32.4),0,0)).to_quaternion())
 box('Lower hull',(0,.77,.11),(.96,.56,.78),bevel=.08)
 box('Front apron',(0,.68,.545),(.48,.40,.075),bevel=.035)
 box('Apron vent',(0,.52,.590),(.24,.045,.015),mat='Dark',bevel=.006)
 box('Chest status slot',(0,.98,.510),(.21,.025,.012),mat='Red',bevel=.003)
 for side,sign in [('Left',1),('Right',-1)]:
  rod('Service cap '+side,(sign*.35,1.085,.47),(sign*.35,1.085,.51),.10,'Torso','Trim')
  rod('Service lid '+side,(sign*.35,1.085,.508),(sign*.35,1.085,.521),.079,'Torso','Ivory')
  box('Side vent '+side,(sign*.606,1.04,.12),(.018,.15,.24),mat='Dark',bevel=.004)
 box('Back equipment',(0,1.21,-.49),(.64,.67,.25),mat='Dark',bevel=.06)
 box('Back service panel',(0,1.24,-.635),(.50,.47,.045),bevel=.025)
 for x in [-.19,.19]:box('Rear socket '+str(x),(x,1.07,-.67),(.12,.12,.03),mat='Trim',bevel=.012)
 prism('Sloped sensor hood',[(1.555,-.375),(1.555,.435),(2.165,.305),(2.165,-.375)],.51,weight='Head')
 box('Face recess',(0,1.605,.437),(.72,.13,.065),weight='Head',mat='Dark',bevel=.014)
 box('Hood brow',(0,1.765,.427),(.94,.22,.14),weight='Head',bevel=.035)
 for x in [-.20,.20]:box('Small cyan eye '+str(x),(x,1.61,.478),(.044,.075,.015),weight='Head',mat='Eyes',bevel=.007)
 # Single service antenna; no crown, V-fin, horn or tall helmet wings.
 rod('Antenna mast',(-.32,1.70,-.35),(-.32,2.33,-.35),.012,'Head','Trim')
 rod('Antenna tip',(-.32,2.33,-.35),(-.32,2.36,-.35),.014,'Head','Orange')
 for side,sign in [('Left',1),('Right',-1)]:
  shoulder=rig.data.bones[side+'_shoulder'].head_local.copy();elbow=rig.data.bones[side+'_elbow'].head_local.copy();hand=rig.data.bones[side+'_hand'].head_local.copy()
  box('Orange shoulder '+side,(sign*.81,1.61,0),(.38,.37,.48),side+'_shoulder','Orange',.045)
  rod('Shoulder axle '+side,(sign*.55,1.60,0),(sign*.73,1.60,0),.14,side+'_shoulder','Dark')
  box('Shoulder endplate '+side,(sign*1.015,1.61,0),(.028,.27,.35),side+'_shoulder','Orange',.01)
  for z in [-.09,0,.09]:box('Shoulder service mark '+side+str(z),(sign*1.034,1.61,z),(.009,.12,.022),side+'_shoulder','Ivory',.002)
  delta=armDelta.get(side+'_shoulder',Vector((0,0,0)))
  for hx in [-.08,.08]:rod('Shoulder handle post '+side+str(hx),Vector((sign*.81+hx,1.79,-.06))+delta,Vector((sign*.81+hx,1.87,-.06))+delta,.016,side+'_shoulder','Dark')
  rod('Shoulder carry handle '+side,Vector((sign*.81-.08,1.87,-.06))+delta,Vector((sign*.81+.08,1.87,-.06))+delta,.016,side+'_shoulder','Dark')
  rod('Upper arm piston '+side,shoulder,elbow,.068,side+'_shoulder')
  box('Upper arm sleeve '+side,(sign*.75,1.41,.015),(.16,.27,.18),side+'_shoulder','Ivory',.025)
  rod('Elbow bearing '+side,(elbow.x-sign*.10,elbow.y,elbow.z),(elbow.x+sign*.10,elbow.y,elbow.z),.12,side+'_elbow')
  rod('Forearm piston '+side,elbow,hand,.061,side+'_elbow')
  box('Slender forearm '+side,(sign*.815,1.08,.035),(.20,.32,.22),side+'_elbow','Ivory',.035)
  box('Large palm '+side,(sign*.835,.81,.06),(.28,.25,.25),side+'_hand','Dark',.035)
  box('White palm face '+side,(sign*.835,.83,.198),(.23,.15,.035),side+'_hand','Ivory',.015)
  box('Hand back armor '+side,(sign*.835,.83,-.078),(.27,.22,.055),side+'_hand','Ivory',.02)
  for finger in range(4):
   x=sign*.835+(finger-1.5)*.067
   box('Finger upper '+side+str(finger),(x,.66,.10),(.061,.105,.13),side+'_hand','Ivory',.012)
   box('Finger lower '+side+str(finger),(x,.575,.145),(.059,.095,.12),side+'_hand','Ivory',.012)
  box('Thumb '+side,(sign*.66,.75,.105),(.09,.18,.13),side+'_hand','Ivory',.02)
 if kind=='WISP':
  for side,sign in [('Left',1),('Right',-1)]:
   hip=rig.data.bones[side+'_thigh'].head_local.copy();knee=rig.data.bones[side+'_knee'].head_local.copy();ankle=rig.data.bones[side+'_ankle'].head_local.copy()
   rod('Hip axle '+side,(sign*.18,.93,0),(sign*.41,.93,0),.13,side+'_thigh')
   rod('Thin thigh '+side,hip,knee,.065,side+'_thigh')
   box('Knee housing '+side,(sign*.30,.52,.045),(.26,.24,.27),side+'_knee','Dark',.045)
   rod('Knee bolt '+side,(sign*.42,.53,.045),(sign*.45,.53,.045),.065,side+'_knee','Trim')
   rod('Shin piston '+side,knee,ankle,.050,side+'_knee')
   box('Shin front strip '+side,(sign*.30,.31,.052),(.11,.25,.085),side+'_knee','Ivory',.018)
   box('Small foot '+side,(sign*.30,.05,.04),(.22,.10,.29),side+'_ankle','Dark',.03)
   for toe in range(3):box('Toe '+side+str(toe),(sign*.30+(toe-1)*.069,.037,.205),(.065,.074,.11),side+'_ankle','Trim',.018)
 elif kind=='BASTION':
  box('Low tracked chassis',(0,.69,0),(1.35,.26,1.55),'Legs','Ivory',.06)
  box('Chassis front apron',(0,.54,.77),(1.06,.37,.17),'Legs','Ivory',.04)
  box('Chassis orange service strip',(0,.71,.865),(.56,.06,.018),'Legs','Orange',.009)
  for side,sign in enumerate([-1,1]):
   x=sign*.81
   box('Track side housing '+str(side),(x,.42,0),(.40,.37,1.35),'Legs','Dark',.025)
   box('Track ivory cover '+str(side),(x,.86,0),(.57,.14,1.68),'Legs','Ivory',.035)
   box('Track orange front '+str(side),(x,.66,.78),(.34,.14,.04),'Legs','Orange',.015)
   for wheel in range(5):
    z=-.58+wheel*.29;w=newbone('Wheel_%d_%d'%(side,wheel),'Legs',(x,.40,z))
    rod('Road wheel %d %d'%(side,wheel),(x-.23,.4,z),(x+.23,.4,z),.155,w,'Trim')
   for i in range(40):
    a=i*math.tau/40;y=.43+.38*math.cos(a);z=.79*math.sin(a);w=newbone('Tread_%d_%02d'%(side,i),'Legs',(x,y,z))
    box('Tread %d %d'%(side,i),(x,y,z),(.53,.09,.14),w,'Dark',.009)
 else:
  box('Four leg abdomen',(0,.78,0),(1.15,.23,1.25),'Legs','Ivory',.06)
  for i,(sx,sz) in enumerate([(-1,1),(1,1),(-1,-1),(1,-1)]):
   h=(sx*.55,.85,sz*.40);k=(sx*.98,.51,sz*.78);t=(sx*1.25,.075,sz*1.04)
   hip=newbone('SpiderHip_'+str(i),'Legs',h);knee=newbone('SpiderKnee_'+str(i),hip,k);foot=newbone('SpiderFoot_'+str(i),knee,t)
   rod('Spider upper strut '+str(i),h,k,.065,hip)
   box('Spider hip shell '+str(i),h,(.23,.22,.24),hip,'Ivory',.03)
   box('Spider knee '+str(i),k,(.22,.23,.25),knee,'Dark',.035)
   box('Spider orange joint tab '+str(i),(k[0],k[1]+.13,k[2]),(.14,.035,.13),knee,'Orange',.01)
   rod('Spider lower piston '+str(i),k,t,.055,knee)
   mid=(Vector(k)+Vector(t))/2;box('Spider shin shell '+str(i),mid,(.12,.22,.14),knee,'Ivory',.025)
   box('Spider foot '+str(i),(t[0],.045,t[2]),(.24,.09,.31),foot,'Dark',.025)
 name='WISP_v008' if kind=='WISP' else kind+'_v003';out=os.path.join(BASE,'Assets/Art/Robots',kind);os.makedirs(os.path.join(out,'Source'),exist_ok=True)
 bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
 for o in made:o.select_set(True)
 bpy.context.view_layer.objects.active=rig
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,name+'.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL')
 bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out,'Source',name+'.blend'))
 inv=rig.matrix_world.inverted();points=[inv@o.matrix_world@v.co for o in made for v in o.data.vertices];lo=[min(p[i] for p in points) for i in range(3)];hi=[max(p[i] for p in points) for i in range(3)]
 report={'model':name,'meshCount':len(made),'vertices':sum(len(o.data.vertices) for o in made),'boneCount':len(rig.data.bones),'min':lo,'max':hi,'palette':PALETTE};reports.append(report)
 scene=bpy.context.scene;cam=scene.camera
 for view,pos in [('front',(0,-8,1.35)),('rear',(0,8,1.35)),('side',(8,0,1.35)),('three-quarter',(5,-8,3))]:
  cam.location=pos;cam.rotation_euler=(Vector((0,0,1.25))-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100;scene.render.filepath=os.path.join(DOC,name+'-'+view+'.png');bpy.ops.render.render(write_still=True)
 print('NACCHIN_STYLE_EXPORT_PASS',report)
for kind in ['WISP','BASTION','TARANTULA']:build(kind)
with open(os.path.join(DOC,'ModelReport.json'),'w',encoding='utf8') as f:json.dump(reports,f,indent=2)
