import bpy,json
from mathutils import Vector
out=[]
for o in bpy.data.objects:
 if o.type in {'MESH','EMPTY','ARMATURE'}:
  pts=[o.matrix_world@Vector(c) for c in o.bound_box] if o.type=='MESH' else []
  out.append(dict(name=o.name,type=o.type,parent=o.parent.name if o.parent else None,loc=list(o.location),rot=list(o.rotation_euler),scale=list(o.scale),verts=len(o.data.vertices) if o.type=='MESH' else 0,bounds=[[min(p[i] for p in pts),max(p[i] for p in pts)] for i in range(3)] if pts else None))
open(r'D:\Codex\VoxSky\Builds\WorkLatest\Logs\wisp-v002-objects.json','w').write(json.dumps(out,indent=2))
