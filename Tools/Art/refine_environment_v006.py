"""Preserve diagonal beam topology while replacing foundry checker skins with Blender-authored service bays.
Open preserved environment v004. Existing structural beams/factories stay intact.
"""
import bpy, math, os
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
source=open(os.path.join(BASE,'Tools/Art/refine_environment_v002.py'),encoding='utf-8-sig').read()
exec(source.split("for kind,name in enumerate")[0])
wall=bpy.data.materials.new('Foundry satin slate wall');wall.diffuse_color=(.13,.18,.22,1);wall.use_nodes=True
p=wall.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=wall.diffuse_color;p.inputs['Roughness'].default_value=.72;p.inputs['Metallic'].default_value=.25
materials.append(wall)
old=bpy.data.objects['FoundryBackdrop'];mesh=old.data;k=Kit('FoundryBackdrop');removed=0
# The preserved kit stores eight vertices/six faces per authored box.
assert len(mesh.vertices)%8==0 and len(mesh.polygons)*8==len(mesh.vertices)*6
for start in range(0,len(mesh.vertices),8):
 points=[tuple(mesh.vertices[start+i].co) for i in range(8)]
 lo=[min(v[a] for v in points) for a in range(3)];hi=[max(v[a] for v in points) for a in range(3)]
 center=[(lo[a]+hi[a])/2 for a in range(3)];size=[hi[a]-lo[a] for a in range(3)]
 checker=(abs(abs(center[0])-139.98)<.02 and abs(size[0]-.16)<.02 and abs(size[1]-5.85)<.02) or (abs(abs(center[1])-139.98)<.02 and abs(size[1]-.16)<.02 and abs(size[0]-5.85)<.02)
 oldlamp=(abs(abs(center[0])-139.86)<.02 and abs(size[1]-3.8)<.02) or (abs(abs(center[1])-139.86)<.02 and abs(size[0]-3.8)<.02)
 if checker or oldlamp:removed+=1;continue
 n=len(k.v);k.v.extend(points)
 for face in mesh.polygons[(start//8)*6:(start//8+1)*6]:
  k.f.append(tuple(n+i-start for i in face.vertices));k.mi.append(face.material_index)
assert removed>2000
for axis in [0,1]:
 for side in [-1,1]:
  for bay in range(-9,10):
   along=bay*14
   for row in range(4):
    z=7.75+row*15.5
    center=(side*139.96,along,z) if axis==0 else (along,side*139.96,z)
    k.box(center,(.12,13.85,15.35) if axis==0 else (13.85,.12,15.35),10)
   # Lower service hatch, rivets, recessed vents and restrained ochre markings.
   for a in [-3.5,3.5]:
    center=(side*139.82,along+a,3.5) if axis==0 else (along+a,side*139.82,3.5)
    k.box(center,(.12,3.5,5) if axis==0 else (3.5,.12,5),1)
    for z in range(2,6):
     center=(side*139.74,along+a,z) if axis==0 else (along+a,side*139.74,z)
     k.box(center,(.04,2.6,.10) if axis==0 else (2.6,.04,.10),0)
   for i in range(6):
    center=(side*139.72,along-1.25+i*.5,.8) if axis==0 else (along-1.25+i*.5,side*139.72,.8)
    k.box(center,(.04,.35,.3) if axis==0 else (.35,.04,.3),3 if i%2 else 4)
   for z in [12,28,44]:
    center=(side*139.71,along,z) if axis==0 else (along,side*139.71,z)
    k.box(center,(.08,2.2,.30) if axis==0 else (2.2,.08,.30),4)
    center=(side*139.65,along,z) if axis==0 else (along,side*139.65,z)
    k.box(center,(.04,1.6,.12) if axis==0 else (1.6,.04,.12),5)
k.finish()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'ArtSource/Environment/VoxelOn_Environment_v006.blend'))
print('VOXELON_ENVIRONMENT_V006_PASS foundry service bays; removed',removed,'old checker/lamp boxes')
