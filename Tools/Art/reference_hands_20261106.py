import bpy,bmesh,math,os,json
from mathutils import Matrix,Vector,Quaternion
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest';DOC=os.path.join(BASE,'Docs/Models/ReferenceHands-20261106');RENDER=os.path.join(BASE,'Docs/Screenshots/ReferenceHands-20261106/Blender-v001')
for p in [DOC,RENDER]:os.makedirs(p,exist_ok=True)
primitive_cache={};report=[]
def primitive(kind,size,bevel=0):
 key=(kind,tuple(size),bevel)
 if key in primitive_cache:return primitive_cache[key]
 bm=bmesh.new()
 if kind=='box':
  bmesh.ops.create_cube(bm,size=1)
  for v in bm.verts:v.co.x*=size[0];v.co.y*=size[1];v.co.z*=size[2]
  if bevel:bmesh.ops.bevel(bm,geom=list(bm.edges),offset=min(bevel,min(size)*.23),segments=1,affect='EDGES',profile=.5,clamp_overlap=True)
 else:bmesh.ops.create_cone(bm,cap_ends=True,cap_tris=False,segments=12,radius1=size[0],radius2=size[0],depth=size[1])
 bm.verts.ensure_lookup_table();bm.verts.index_update();verts=[v.co.copy() for v in bm.verts];faces=[tuple(v.index for v in f.verts) for f in bm.faces];bm.free();primitive_cache[key]=(verts,faces);return verts,faces

def hand_mesh(rig,side,materials):
 sign=1 if side=='Left' else -1;pivot=rig.data.bones[side+'_hand'].head_local.copy();verts=[[],[],[]];faces=[];face_materials=[];components=[]
 def element(name,kind,size,frames,material,bevel=0):
  vv,ff=primitive(kind,size,bevel);offset=len(verts[0])
  for phase,frame in enumerate(frames):verts[phase].extend([frame@v for v in vv])
  faces.extend([tuple(i+offset for i in f) for f in ff]);face_materials.extend([material]*len(ff));components.append(name)
 def static_box(name,pos,size,mat,bevel=.012):element(name,'box',size,[Matrix.Translation(pivot+Vector(pos))]*3,mat,bevel)
 def pose_box(name,size,frames,mat,bevel=.012):element(name,'box',size,frames,mat,bevel)
 def cylinder(name,radius,depth,frames,mat):element(name,'cylinder',(radius,depth),frames,mat)
 # Tapered-looking separate armor strips reveal the joints instead of hiding the hand in one slab.
 static_box('Rubber palm',(sign*.012,-.12,0),(.335,.22,.16),1,.028)
 static_box('Palm grip pad',(sign*.012,-.13,.087),(.27,.145,.026),1,.012)
 static_box('Left back strip',(-.105,-.125,-.093),(.085,.17,.032),0,.016)
 static_box('Right back strip',(.105,-.125,-.093),(.085,.17,.032),0,.016)
 static_box('Knuckle back bridge',(0,-.208,-.078),(.365,.065,.045),0,.014)
 static_box('Center service panel',(0,-.095,-.097),(.105,.09,.025),0,.012)
 rot_y=Matrix.Rotation(math.pi/2,4,'X');cylinder('Wrist flexible joint',.112,.085,[Matrix.Translation(pivot+Vector((0,.002,0)))@rot_y]*3,1)
 cylinder('Wrist cuff',.133,.026,[Matrix.Translation(pivot+Vector((0,-.025,0)))@rot_y]*3,2)
 angles=[(12,18,12),(20,60,20),(80,90,45)]
 for finger,x in enumerate([-.15,-.05,.05,.15]):
  lengths=[.12,.10,.08] if finger<3 else [.105,.09,.075]
  starts=[pivot+Vector((x,-.23,.02)) for _ in range(3)];cumulative=[0,0,0]
  for segment,length in enumerate(lengths):
   frames=[];joint_frames=[]
   for phase in range(3):
    cumulative[phase]+=angles[phase][segment];rotation=Matrix.Rotation(math.radians(-cumulative[phase]),4,'X');direction=rotation.to_3x3()@Vector((0,-1,0));center=starts[phase]+direction*(length/2)
    frames.append(Matrix.Translation(center)@rotation);joint_frames.append(Matrix.Translation(starts[phase])@Matrix.Rotation(math.pi/2,4,'Y'));starts[phase]=starts[phase]+direction*length
   pose_box('Finger %d armor %d'%(finger,segment),(.092,length*.88,.101),frames,0,.014);cylinder('Finger hinge %d %d'%(finger,segment),.035,.075,joint_frames,1)
   # Small bolt on the exposed side keeps each finger joint legible.
   bolt_frames=[f@Matrix.Translation((0,0,.042)) for f in joint_frames];cylinder('Hinge cap %d %d'%(finger,segment),.017,.008,bolt_frames,2)
 # Two articulated thumb sections wrap across the grip, rather than being a side cube.
 thumb=[pivot+Vector((-sign*.218,-.085,.035)) for _ in range(3)]
 deltas=[[(sign*.020,-.105,.020),(sign*.035,-.080,.020)],[(sign*.040,-.070,.095),(sign*.110,.005,.055)],[(sign*.035,-.050,.110),(sign*.120,.005,.025)]]
 for segment in range(2):
  frames=[];hinges=[]
  for phase in range(3):
   d=Vector(deltas[phase][segment]);center=thumb[phase]+d*.5;rotation=Vector((0,-1,0)).rotation_difference(d.normalized()).to_matrix().to_4x4();frames.append(Matrix.Translation(center)@rotation);hinges.append(Matrix.Translation(thumb[phase])@Matrix.Rotation(math.pi/2,4,'Y'));thumb[phase]+=d
  # Use one constant segment length so shape deformation bends rather than adding a new object.
  pose_box('Thumb armor '+str(segment),(.112,.112 if segment==0 else .10,.11),frames,0,.016);cylinder('Thumb hinge '+str(segment),.041,.09,hinges,1)
 mesh=bpy.data.meshes.new('Reference glove '+side);mesh.from_pydata(verts[0],[],faces);mesh.update();ob=bpy.data.objects.new('VN_ArticulatedGlove_'+side,mesh);bpy.context.collection.objects.link(ob);ob.parent=rig;ob.matrix_world=rig.matrix_world
 for mat in materials:mesh.materials.append(mat)
 for poly,index in zip(mesh.polygons,face_materials):poly.material_index=index
 group=ob.vertex_groups.new(name=side+'_hand');group.add(list(range(len(mesh.vertices))),1,'REPLACE');modifier=ob.modifiers.new('Preserved hand rig','ARMATURE');modifier.object=rig
 ob.shape_key_add(name='Basis')
 for phase,name in [(1,'Grip'),(2,'Fist')]:
  shape=ob.shape_key_add(name=name)
  for point,co in zip(shape.data,verts[phase]):point.co=co
  shape.value=0
 ob['hand_reference']='Nacchin-Hands-20261106';ob['finger_segments']=12;ob['thumb_segments']=2
 return ob,{'side':side,'vertices':len(verts[0]),'components':len(components),'finger_segments':12,'thumb_segments':2,'socket_rig_position':list(pivot+Vector((0,-.23,.16)))}

for kind,old,new in [('WISP','v012','v013'),('BASTION','v007','v008'),('TARANTULA','v006','v007')]:
 folder=os.path.join(BASE,'Assets/Art/Robots',kind);output=os.path.join(folder,'Source',kind+'_'+new+'.blend');assert not os.path.exists(output),'Choose a fresh source version: '+output
 bpy.ops.wm.open_mainfile(filepath=os.path.join(folder,'Source',kind+'_'+old+'.blend'));rig=next(o for o in bpy.data.objects if o.type=='ARMATURE' and o.data.bones.get('Left_hand'))
 old_bones={b.name:(b.head_local.copy(),b.tail_local.copy(),b.matrix_local.copy(),b.parent.name if b.parent else None) for b in rig.data.bones};body={};removed=0
 for ob in list(bpy.data.objects):
  if ob.type!='MESH' or not any(m.type=='ARMATURE' and m.object==rig for m in ob.modifiers):continue
  ids={g.index for g in ob.vertex_groups if g.name in ['Left_hand','Right_hand']};hand_count=sum(any(g.group in ids and g.weight>.5 for g in v.groups) for v in ob.data.vertices)
  if hand_count:
   assert hand_count==len(ob.data.vertices),'Mixed body/hand mesh requires targeted editing: '+ob.name;bpy.data.objects.remove(ob,do_unlink=True);removed+=1
  else:body[ob.name]=[v.co.copy() for v in ob.data.vertices]
 assert removed==24,(kind,removed)
 mats=[]
 for name in ['VN_Ivory','VN_Dark','VN_Trim']:
  mat=bpy.data.materials.get(name);assert mat is not None,name;mats.append(mat)
 hands=[];details=[]
 for side in ['Left','Right']:
  ob,info=hand_mesh(rig,side,mats);hands.append(ob);details.append(info)
 bpy.context.view_layer.objects.active=rig;bpy.ops.object.mode_set(mode='EDIT')
 for side,info in zip(['Left','Right'],details):
  bone=rig.data.edit_bones.new(side+'_GRIP_SOCKET');bone.head=Vector(info['socket_rig_position']);bone.tail=bone.head+Vector((0,.05,0));bone.parent=rig.data.edit_bones[side+'_hand'];bone.use_deform=False
 bpy.ops.object.mode_set(mode='OBJECT');bpy.context.view_layer.update()
 for name,(head,tail,matrix,parent) in old_bones.items():
  bone=rig.data.bones[name];assert (bone.head_local-head).length<1e-6 and (bone.tail_local-tail).length<1e-6 and max(abs(bone.matrix_local[i][j]-matrix[i][j]) for i in range(4) for j in range(4))<1e-6 and (bone.parent.name if bone.parent else None)==parent,(kind,name)
 for name,coords in body.items():assert all((a-v.co).length<1e-7 for a,v in zip(coords,bpy.data.objects[name].data.vertices))
 meshes=[o for o in bpy.data.objects if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers)]
 bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
 for ob in meshes:ob.select_set(True)
 bpy.context.view_layer.objects.active=rig;bpy.ops.export_scene.fbx(filepath=os.path.join(folder,kind+'_'+new+'.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=False,use_mesh_modifiers=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL')
 bpy.ops.wm.save_as_mainfile(filepath=output)
 info={'model':kind+'_'+new,'before':kind+'_'+old,'original_bones_preserved':len(old_bones),'new_grip_socket_bones':2,'bone_count':len(rig.data.bones),'skin_mesh_count':len(meshes),'nonhand_vertices_preserved':sum(len(x) for x in body.values()),'removed_hand_meshes':removed,'hands':details};report.append(info)
 with open(os.path.join(DOC,'ModelReport-v001.json'),'w',encoding='utf-8') as f:json.dump(report,f,indent=2)
 # Native studio comparison of actual pose meshes, one fixed camera angle and scale.
 scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE_NEXT';scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG'
 if scene.world is None:scene.world=bpy.data.worlds.new('Reference hands world')
 scene.world.color=(.045,.055,.065)
 for light in [o for o in bpy.data.objects if o.type=='LIGHT']:light.hide_render=True
 center=rig.matrix_world@rig.data.bones['Left_hand'].head_local
 for n,delta,power in [('Hand key',(-2,-3,4),700),('Hand fill',(3,-1,2),450)]:
  data=bpy.data.lights.new(n,'AREA');data.energy=power;data.size=3;o=bpy.data.objects.new(n,data);scene.collection.objects.link(o);o.location=center+Vector(delta);o.rotation_euler=(center-o.location).to_track_quat('-Z','Y').to_euler()
 data=bpy.data.cameras.new('Hand reference inspection');camera=bpy.data.objects.new('Hand reference inspection',data);scene.collection.objects.link(camera);scene.camera=camera;data.type='ORTHO';data.ortho_scale=.85
 center=rig.matrix_world@(rig.data.bones['Left_hand'].head_local+Vector((0,-.23,.08)));camera.location=center+rig.matrix_world.to_3x3()@Vector((-.8,.5,2));camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler()
 hidden={o:o.hide_render for o in bpy.data.objects if o.type=='MESH'}
 for ob in hidden:ob.hide_render=ob!=hands[0]
 for pose in ['Relaxed','Grip','Fist']:
  for ob in hands:
   ob.data.shape_keys.key_blocks['Grip'].value=1 if pose=='Grip' else 0;ob.data.shape_keys.key_blocks['Fist'].value=1 if pose=='Fist' else 0
  scene.render.filepath=os.path.join(RENDER,kind+'-'+pose+'.png');bpy.ops.render.render(write_still=True)
 for ob,state in hidden.items():ob.hide_render=state
 print('REFERENCE_HANDS_PASS',info,flush=True)
print('REFERENCE_HANDS_LIBRARY_PASS',len(report),flush=True)
