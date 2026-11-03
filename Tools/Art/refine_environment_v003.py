"""Blender-only skyline/structure refinement; preserve v001/v002 sources.
Open environment v002, then run. Existing cover/ground meshes remain unchanged.
"""
import bpy, math, os
from mathutils import Vector
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
# Reuse the preserved v002 authoring primitives without executing asset generation.
source=open(os.path.join(BASE,'Tools/Art/refine_environment_v002.py'),encoding='utf-8-sig').read()
exec(source.split("for kind,name in enumerate")[0])

def beam(k,a,b,width,material):
 delta=Vector(b)-Vector(a);center=(Vector(a)+Vector(b))*.5
 rot=Vector((0,0,1)).rotation_difference(delta.normalized());n=len(k.v)
 for c in C:k.v.append(tuple(center+rot@Vector((c[0]*width/2,c[1]*width/2,c[2]*delta.length/2))))
 k.f.extend([tuple(n+i for i in face) for face in F]);k.mi.extend([material]*6)
old=bpy.data.objects['FoundryBackdrop'];mesh=old.data
k=Kit('FoundryBackdrop');k.v=[tuple(v.co) for v in mesh.vertices];k.f=[tuple(p.vertices) for p in mesh.polygons];k.mi=[p.material_index for p in mesh.polygons]
for axis in [0,1]:
 for side in [-1,1]:
  for j in range(-9,10):
   along=j*14
   # Shallow steelwork stays within the wall clearance; no new floor obstacles.
   p=(side*139.66,along,29) if axis==0 else (along,side*139.66,29)
   k.box(p,(.40,.65,58) if axis==0 else (.65,.40,58),1)
   for z in [8,22,36,50]:
    p=(side*139.42,along,z) if axis==0 else (along,side*139.42,z)
    k.box(p,(.12,1.3,.5) if axis==0 else (1.3,.12,.5),2)
   for z in [10,30]:
    a=(side*139.47,along-6,z) if axis==0 else (along-6,side*139.47,z)
    b=(side*139.47,along+6,z+8) if axis==0 else (along+6,side*139.47,z+8)
    beam(k,a,b,.22,0)
  # Tall vertical service-light strips break up the former flat checker wall.
  for j in range(-4,5):
   along=j*28
   for z in range(7,47,3):
    p=(side*139.34,along,z) if axis==0 else (along,side*139.34,z)
    k.box(p,(.08,.22,1.5) if axis==0 else (.22,.08,1.5),5 if j%2 else 3)
# Ceiling beams are well above the maximum combat jump envelope.
for y in range(-112,113,28):
 k.box((0,y,59),(279,.7,1.2),1)
 for x in range(-126,127,18):
  beam(k,(x,y,58.5),(x+9,y,55.5),.3,0)
  beam(k,(x+9,y,55.5),(x+18,y,58.5),.3,0)
 k.box((0,y,55.5),(279,.3,.35),2)
k.finish()
# Eroded mesa skyline: irregular buttresses, stepped shoulders, sparse grassy caps.
k=Kit('CanyonVista')
for i in range(72):
 a=i*math.tau/72;r=171+9*math.sin(i*2.1)
 x,y=math.sin(a)*r,math.cos(a)*r;h=34+23*(.5+.5*math.sin(i*.73))+12*(i%5==0)
 width=12+i%4*2
 for tier in range(12):
  z=(tier+.5)*h/12;w=width*(1-.035*tier)
  dx=math.sin(i*.9+tier*.37)*1.5;dy=math.cos(i+tier*.27)*1.5
  shade=7 if tier%5==0 else 8 if tier%5==1 else 6
  k.box((x+dx,y+dy,z),(w,w*(.8+.1*(i%3)),h/12-.025),shade)
  if tier in [3,7]:k.box((x+dx,y+dy,(tier+1)*h/12),(w+1,w*.95,.30),8)
 # Short buttress columns provide a geological rather than regular block outline.
 for j in [-1,1]:
  hh=h*(.37+.13*(i%3));xx=x+math.cos(a)*j*(width*.43);yy=y-math.sin(a)*j*(width*.43)
  k.box((xx,yy,hh/2),(width*.40,width*.42,hh),7)
 if i%3==0:k.box((x+math.sin(i)*1.5,y,h+.08),(width*.50,width*.53,.16),9)
k.finish()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'ArtSource/Environment/VoxelOn_Environment_v003.blend'))
print('VOXELON_ENVIRONMENT_V003_PASS 2 FBXs, original covers/ground preserved')
