import bpy,os,json
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest';reports=[]
for kind,before,after in [('WISP','v011','v012'),('BASTION','v006','v007'),('TARANTULA','v005','v006')]:
 def read(version):
  bpy.ops.wm.open_mainfile(filepath=os.path.join(BASE,'Assets/Art/Robots',kind,'Source',kind+'_'+version+'.blend'))
  rig=next(o for o in bpy.data.objects if o.type=='ARMATURE' and o.data.bones.get('Left_hand'));bones={b.name:{'head':list(b.head_local),'tail':list(b.tail_local),'parent':b.parent.name if b.parent else None,'matrix':[list(row) for row in b.matrix_local]} for b in rig.data.bones}
  nonhand={}
  for ob in bpy.data.objects:
   if ob.type!='MESH' or not any(m.type=='ARMATURE' and m.object==rig for m in ob.modifiers):continue
   indices={g.index for g in ob.vertex_groups if g.name in ['Left_hand','Right_hand']}
   for v in ob.data.vertices:
    if not any(g.group in indices and g.weight>.001 for g in v.groups):nonhand[(ob.name,v.index)]=list(v.co)
  return bones,nonhand
 a,points=read(before);b,other=read(after);assert a==b,(kind,'joints changed');assert points==other,(kind,'non-hand geometry changed')
 reports.append({'model':kind,'before':before,'after':after,'identical_bones_and_bind_matrices':len(a),'identical_nonhand_vertices':len(points)});print('GLOVE_INTEGRITY_PASS',reports[-1],flush=True)
with open(os.path.join(BASE,'Docs/Models/GloveHands-20261105/NativeIntegrity.json'),'w',encoding='utf-8') as f:json.dump(reports,f,indent=2)
