import bpy,math,json
from pathlib import Path
from mathutils import Vector
base=Path.cwd();report=[]
for path in sorted((base/'Assets/Art/CombatEffects/v002').glob('*.fbx')):
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(path));meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];points=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box];assert meshes and all(math.isfinite(v) for p in points for v in p),path
 sockets=[o.name for o in bpy.context.scene.objects if o.type=='EMPTY'];assert any('FX_ORIGIN' in n for n in sockets),path
 size=[max(p[i] for p in points)-min(p[i] for p in points) for i in range(3)];assert .01<max(size)<2,path
 report.append({'file':path.name,'meshes':len(meshes),'boundsMeters':size,'faces':sum(len(o.data.polygons) for o in meshes)})
assert len(report)==5,len(report)
(base/'Docs/Design/CombatEffects/v002/FBX-Reimport-Validation.json').write_text(json.dumps(report,indent=2))
print('COMBAT_FX_REIMPORT_PASS',len(report))
