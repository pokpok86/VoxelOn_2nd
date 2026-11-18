import bpy,json,math
from pathlib import Path
from mathutils import Vector
base=Path.cwd();folder=base/'Docs/Design/DroneBackpacks/v003';results=[]
for path in sorted((base/'Assets/Art/DroneBackpacks/Concept_v003').glob('*.fbx')):
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(path))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];points=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
 assert meshes and points,path.name
 assert all(math.isfinite(v) for p in points for v in p),path.name
 sockets=[o.name for o in bpy.context.scene.objects if o.type=='EMPTY']
 if path.stem.endswith('_BACKPACK'):assert any('BODY_MOUNT' in n for n in sockets) and any('DRONE_DOCK' in n for n in sockets)
 if path.stem.endswith('_DRONE'):assert any('DOCK_CONTACT' in n for n in sockets)
 size=[max(p[i] for p in points)-min(p[i] for p in points) for i in range(3)]
 assert max(size)<2 and min(size)>.001,(path.name,size)
 results.append({'file':path.name,'meshCount':len(meshes),'sockets':sockets,'worldBoundsMeters':size,'finite':True})
assert len(results)==12,len(results)
(folder/'FBX-Reimport-Validation.json').write_text(json.dumps({'pass':True,'files':results,'limits':'Blender FBX reimport only; no Unity import/runtime behavior or physical docking clearance certification'},indent=2))
print('DRONE_FBX_REIMPORT_PASS',len(results))
