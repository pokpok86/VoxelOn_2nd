import bpy
bpy.ops.wm.open_mainfile(filepath=r'D:\Codex\VoxSky\Builds\WorkLatest\Assets\Art\Robots\WISP\Source\WISP_v012.blend')
rig=next(o for o in bpy.data.objects if o.type=='ARMATURE')
print('RIG',rig.name,'bones',len(rig.data.bones),'matrix',list(rig.matrix_world))
for bone in ['Left_hand','Right_hand']:
 print('PIVOT',bone,list(rig.data.bones[bone].head_local),list(rig.data.bones[bone].tail_local))
 hand=[]
 for ob in bpy.data.objects:
  if ob.type!='MESH':continue
  g=ob.vertex_groups.get(bone)
  if g is None:continue
  selected=[v for v in ob.data.vertices if any(w.group==g.index and w.weight>.5 for w in v.groups)]
  if selected:hand.append(ob);print('HAND_MESH',ob.name,len(selected),len(ob.data.vertices),'mods',[(m.name,m.type) for m in ob.modifiers],'mats',[m.name for m in ob.data.materials])
 print('HAND_OBJECTS',bone,len(hand))
print('MESH_TOTAL',len([o for o in bpy.data.objects if o.type=='MESH']))
