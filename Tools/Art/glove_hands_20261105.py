import bpy,os,json
from mathutils import Vector
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
DOC=os.path.join(BASE,'Docs/Models/GloveHands-20261105');os.makedirs(DOC,exist_ok=True)
reports=[]
for kind,before,after in [('WISP','v011','v012'),('BASTION','v006','v007'),('TARANTULA','v005','v006')]:
 folder=os.path.join(BASE,'Assets/Art/Robots',kind);src=os.path.join(folder,'Source',kind+'_'+before+'.blend');dst=os.path.join(folder,'Source',kind+'_'+after+'.blend');assert not os.path.exists(dst),'Preserve existing target '+dst
 bpy.ops.wm.open_mainfile(filepath=src);rig=next(o for o in bpy.data.objects if o.type=='ARMATURE' and o.data.bones.get('Left_hand') is not None)
 meshes=[o for o in bpy.data.objects if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers)]
 bones_before={b.name:list(b.head_local)+list(b.tail_local) for b in rig.data.bones};per_hand={};unchanged=0
 for side in ['Left','Right']:
  bone=side+'_hand';pivot=rig.data.bones[bone].head_local;oldpts=[];newpts=[]
  for ob in meshes:
   vg=ob.vertex_groups.get(bone)
   if vg is None:continue
   to_rig=rig.matrix_world.inverted()@ob.matrix_world;from_rig=to_rig.inverted()
   for v in ob.data.vertices:
    amount=sum(g.weight for g in v.groups if g.group==vg.index)
    if amount<.001:continue
    old=to_rig@v.co;delta=old-pivot;scale=Vector((1+.38*amount,1+.18*amount,1+.52*amount));new=pivot+Vector((delta.x*scale.x,delta.y*scale.y,delta.z*scale.z));v.co=from_rig@new;oldpts.append(old);newpts.append(new)
   ob.data.update()
  assert len(oldpts)>50,(kind,bone,len(oldpts))
  def bounds(points):return {'min':[min(p[i] for p in points) for i in range(3)],'max':[max(p[i] for p in points) for i in range(3)]}
  a=bounds(oldpts);b=bounds(newpts);ratios=[(b['max'][i]-b['min'][i])/(a['max'][i]-a['min'][i]) for i in range(3)];assert max(abs(ratios[i]-[1.38,1.18,1.52][i]) for i in range(3))<.0001
  per_hand[side]={'vertices':len(oldpts),'before':a,'after':b,'size_ratio':ratios,'pivot_preserved':list(pivot)}
 assert bones_before=={b.name:list(b.head_local)+list(b.tail_local) for b in rig.data.bones}
 bpy.context.view_layer.update();bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
 for ob in meshes:ob.select_set(True)
 bpy.context.view_layer.objects.active=rig
 bpy.ops.export_scene.fbx(filepath=os.path.join(folder,kind+'_'+after+'.fbx'),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL')
 bpy.ops.wm.save_as_mainfile(filepath=dst);report={'model':kind+'_'+after,'source':kind+'_'+before,'bones_unchanged':True,'bone_count':len(rig.data.bones),'hands':per_hand};reports.append(report);print('GLOVE_HANDS_PASS',kind,per_hand,flush=True)
with open(os.path.join(DOC,'BlenderReport.json'),'w',encoding='utf-8') as f:json.dump(reports,f,indent=2)
print('GLOVE_HANDS_LIBRARY_PASS',len(reports),flush=True)
