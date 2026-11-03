"""Refine the validated v001 environment without changing its collision envelope.
Run Blender -b ArtSource/Environment/VoxelOn_Environment_v001.blend -P this_file.
All boxes are authored here in Blender meters; Z-up is exported as Unity Y-up.
"""
import bpy, math, os
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
ENV=os.path.join(BASE,'Assets/Resources/Environment')
materials=[bpy.data.materials['VoxelOn_Environment_'+str(i)] for i in range(10)]
C=[(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]
F=[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
class Kit:
 def __init__(self,name):self.name=name;self.v=[];self.f=[];self.mi=[]
 def box(self,p,s,m):
  n=len(self.v);self.v.extend([tuple(p[i]+a[i]*s[i]/2 for i in range(3)) for a in C]);self.f.extend([tuple(n+i for i in face) for face in F]);self.mi.extend([m]*6)
 def finish(self,bevel=0):
  old=bpy.data.objects.get(self.name)
  if old:bpy.data.objects.remove(old,do_unlink=True)
  mesh=bpy.data.meshes.new(self.name);mesh.from_pydata(self.v,[],self.f);mesh.update()
  obj=bpy.data.objects.new(self.name,mesh);bpy.context.collection.objects.link(obj)
  for m in materials:mesh.materials.append(m)
  for face,mat in zip(mesh.polygons,self.mi):face.material_index=mat
  bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
  if bevel:
   mod=obj.modifiers.new('Machined armor edge','BEVEL');mod.width=bevel;mod.segments=1;mod.affect='EDGES'
   bpy.ops.object.modifier_apply(modifier=mod.name)
  bpy.ops.export_scene.fbx(filepath=os.path.join(ENV,self.name+'.fbx'),use_selection=True,object_types={'MESH'},bake_anim=False,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',path_mode='AUTO')
  return obj
for kind,name in enumerate(['FoundryCargo','FoundryGenerator','FoundryBarricade']):
 k=Kit(name)
 k.box((0,0,.5),(.97,.97,.98),1)
 # Major frame corners remain exactly on normalized collision floor and roof.
 for x in [-1,1]:
  for y in [-1,1]:k.box((x*.46,y*.46,.46),(.08,.08,.92),2)
 for z in [.04]:
  k.box((0,0,z),(1,1,.08),1)
 for side in [-1,1]:
  k.box((0,side*.46,.97),(1,.08,.06),1)
  k.box((side*.46,0,.97),(.08,.84,.06),1)
 for side in [-1,1]:
  for axis in [0,1]:
   def face(a,z,w,h,mat):
    broad=w>.15 and h>.15;depth=.016 if broad else .01;edge=.480 if broad else .495
    p=(a,side*edge,z) if axis==0 else (side*edge,a,z)
    size=(w,depth,h) if axis==0 else (depth,w,h)
    k.box(p,size,mat)
   if kind==0:
    # Cargo: two reinforced doors, recessed seams and locking bars.
    for a in [-.205,.205]:face(a,.52,.39,.76,0)
    face(0,.5,.018,.78,4)
    for a in [-.34,.34]:
     face(a,.52,.035,.72,2)
     for z in [.23,.78]:face(a,z,.09,.045,3)
    for a in [-.18,.18]:face(a,.53,.07,.07,4)
   elif kind==1:
    # Generator: large vents and paired maintenance panels, not noisy tile wallpaper.
    face(0,.56,.80,.55,4)
    for z in range(7):face(0,.34+z*.071,.72,.035,2)
    face(0,.18,.78,.12,0)
    for a in [-.27,-.18,-.09,0,.09,.18,.27]:face(a,.80,.045,.027,5)
   else:
    # Barricade: broad armor plates and a diagonal yellow/dark identification band.
    for a in [-.30,0,.30]:face(a,.58,.27,.58,2)
    for i in range(18):face(-.425+i*.05,.16,.045,.10,3 if i%4<2 else 4)
    face(0,.38,.78,.025,1)
   for a in [-.41,.41]:
    for z in [.12,.88]:face(a,z,.026,.025,4)
 # Recessed roof service hatch and ventilation ribs retain max Z=1.
 k.box((0,0,.975),(.63,.60,.025),4)
 for i in range(9):k.box((-.25+i*.0625,0,.988),(.026,.50,.020),2 if kind==1 else 0)
 k.finish(.004)
# Rock: broad geological strata with local voxel chips, no checkerboard noise.
k=Kit('CanyonRock');k.box((0,0,.5),(1,1,1),6)
for side in [-1,1]:
 for axis in [0,1]:
  for row in range(12):
   for col in range(16):
    shade=[6,6,7,6,8,6][row%6]
    if (col*17+row*7)%37==0:shade=9 if row>8 else 7
    offset=.495 if (col+row)%7 else .487
    a=-.5+(col+.5)/16;z=(row+.5)/12
    p=(a,side*offset,z) if axis==0 else (side*offset,a,z)
    size=(.061,.01,.079) if axis==0 else (.01,.061,.079)
    k.box(p,size,shade)
for x in range(12):
 for y in range(12):
  shade=8 if math.sin(x*.32)+math.cos(y*.41)>1.0 else 6
  k.box((-.5+(x+.5)/12,-.5+(y+.5)/12,.995),(.081,.081,.01),shade)
k.finish()
# Continuous ground patches at a finer pitch with low color variance.
k=Kit('OutdoorGround')
for x in range(-70,70):
 for y in range(-70,70):
  n=math.sin(x*.065)+math.cos(y*.073)+.4*math.sin((x+y)*.12)
  shade=9 if n>2.0 else 8 if n>.8 else 6 if n>-.9 else 7
  k.box((x*2+1,y*2+1,-.006),(1.998,1.998,.012),shade)
k.finish()
# Preserve the previous backdrop. Add readable factory doors, structural ribs,
# service windows and roof equipment to every face, entirely inside its solid bounds.
old=bpy.data.objects['FoundryBackdrop'];mesh=old.data
k=Kit('FoundryBackdrop')
k.v=[tuple(v.co) for v in mesh.vertices];k.f=[tuple(p.vertices) for p in mesh.polygons];k.mi=[p.material_index for p in mesh.polygons]
for side in [-1,1]:
 for i in range(5):
  x=side*(48+i*14);y=-(45+i%2*16);h=6+i%3*3
  for axis in [0,1]:
   for edge in [-1,1]:
    span=9 if axis==0 else 12
    for col in range(3):
     a=(col-1)*span*.28
     p=(x+a,y+edge*6.02,h*.52) if axis==0 else (x+edge*4.52,y+a,h*.52)
     size=(span*.23,.10,h*.83) if axis==0 else (.10,span*.23,h*.83)
     k.box(p,size,0)
    for row in range(8):
     z=.5+row*(h-1)/8
     p=(x,y+edge*6.09,z) if axis==0 else (x+edge*4.59,y,z)
     size=(3.1,.07,.055) if axis==0 else (.07,3.1,.055)
     k.box(p,size,2)
    for a in [-.9,.9]:
     p=(x+a,y+edge*6.10,h*.75) if axis==0 else (x+edge*4.60,y+a,h*.75)
     k.box(p,(.5,.08,.18) if axis==0 else (.08,.5,.18),5)
k.finish()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'ArtSource/Environment/VoxelOn_Environment_v002.blend'))
print('VOXELON_ENVIRONMENT_V002_PASS six FBXs, normalized cover extents preserved')
