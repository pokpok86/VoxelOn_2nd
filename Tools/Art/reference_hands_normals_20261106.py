import bpy,os,json
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest';records=[]
for kind,before,after in [('WISP','v013','v014'),('BASTION','v008','v009'),('TARANTULA','v007','v008')]:
 folder=os.path.join(BASE,'Assets/Art/Robots',kind);target=os.path.join(folder,'Source',kind+'_'+after+'.blend');assert not os.path.exists(target),target
 bpy.ops.wm.open_mainfile(filepath=os.path.join(folder,'Source',kind+'_'+before+'.blend'));rig=next(o for o in bpy.data.objects if o.type=='ARMATURE');hands=[]
 for ob in bpy.data.objects:
  if ob.type=='MESH' and 'ArticulatedGlove_' in ob.name:
   for poly in ob.data.polygons:poly.use_smooth=True
   hands.append(ob)
 assert len(hands)==2
 bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
 meshes=[o for o in bpy.data.objects if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers)]
 for ob in meshes:ob.select_set(True)
 bpy.context.view_layer.objects.active=rig;bpy.ops.export_scene.fbx(filepath=os.path.join(folder,kind+'_'+after+'.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=False,use_mesh_modifiers=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL')
 bpy.ops.wm.save_as_mainfile(filepath=target);info={'model':kind+'_'+after,'before':kind+'_'+before,'change':'Smooth hand normals only; geometry, pose keys and original rig unchanged','hand_vertices':sum(len(o.data.vertices) for o in hands),'skin_mesh_count':len(meshes)};records.append(info);print('REFERENCE_HANDS_NORMALS_PASS',info,flush=True)
with open(os.path.join(BASE,'Docs/Models/ReferenceHands-20261106/Normals-v002.json'),'w',encoding='utf-8') as f:json.dump(records,f,indent=2)
