import bpy,math,random,os
from mathutils import Matrix,Vector
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
OUT=os.path.join(BASE,'Assets/Resources/PDParts');os.makedirs(OUT,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True);ROT=Matrix.Rotation(math.pi/2,4,'X')
def material(n,c,emission=0):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=.35;p.inputs['Roughness'].default_value=.6
 if emission:p.inputs['Emission Color'].default_value=(*c,1);p.inputs['Emission Strength'].default_value=emission
 return m
metal=material('Slate metal',(.20,.25,.27));dark=material('Recess',(.045,.065,.075));edge=material('Machined rim',(.42,.48,.49));lamp=material('Cyan energy',(.03,.66,.71),2)
stone=[material('Rock tone '+str(i),(.32+i*.018,.29+i*.012,.23+i*.009)) for i in range(6)]
assets={};current=[]
def cube(n,p,s,mat):
 bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.name=n;o.matrix_world=ROT@Matrix.Translation(Vector(p))@Matrix.Diagonal(Vector((*s,1)));bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 bevel=o.modifiers.new('Beveled manufactured edge','BEVEL');bevel.width=min(s)*.10;bevel.segments=1;bpy.ops.object.modifier_apply(modifier=bevel.name);o.data.materials.append(mat);current.append(o);return o
def export(n):
 bpy.ops.object.select_all(action='DESELECT')
 for o in current:o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,n+'.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False)
 assets[n]=list(current);current.clear()
for i,n in enumerate(['Rifle','Cannon','Rocket','Rail']):
 if i<2:
  cube(n+' receiver',(0,0,0),(.24+i*.12,.23+i*.1,.65),metal);cube('Grip',(0,-.23,-.08),(.14,.30,.16),dark)
  cube('Barrel',(0,.02,.65),(.10+i*.12,.11+i*.12,.65+i*.3),edge);cube('Muzzle',(0,.02,1+i*.15),(.18+i*.13,.18+i*.13,.12),dark)
  cube('Sight',(0,.17,.1),(.09,.08,.15),lamp)
 elif i==2:
  cube('Shoulder launcher',(0,0,0),(.56,.42,.75),metal)
  for x in [-.17,0,.17]:
   for y in [-.1,.1]:cube('Launch tube',(x,y,.43),(.12,.12,.15),dark);cube('Rocket tip',(x,y,.52),(.07,.07,.06),lamp)
 else:
  cube('Rail capacitor',(0,0,-.05),(.35,.32,.60),metal)
  for x in [-.14,.14]:cube('Acceleration rail',(x,0,.75),(.09,.18,1.4),edge);cube('Rail luminous slot',(x,.10,.65),(.04,.025,1.15),lamp)
 export('Weapon'+n)
random.seed(114)
# Rock shares the exact unit footprint/floor/top used by the analytical cover volume.
for ix in range(4):
 for iz in range(4):
  cube('Fractured stone mass',(ix/4-.375,.5,iz/4-.375),(.25,1,.25),stone[random.randrange(6)])
export('NaturalRock')
cube('Fragment',(0,0,0),(1,1,1),stone[2]);export('RockFragment')
cube('Armor fragment',(0,0,0),(1,.25,.7),metal);export('ArmorFragment')
cube('Rain streak',(0,0,0),(.014,1,.014),edge);export('RainStreak')
# Authored low-cost circular ripple and pond surfaces; Unity controls their materials and scale.
for n,radius in [('Pond',1),('Ripple',1)]:
 verts=[];faces=[]
 if n=='Pond':
  verts=[(0,0,0)]+[(math.cos(i*math.tau/48)*radius,0,math.sin(i*math.tau/48)*radius) for i in range(48)]
  faces=[(0,i+1,(i+1)%48+1) for i in range(48)]
 else:
  for i in range(48):
   a=i*math.tau/48
   for r in [.96,1]:verts.append((math.cos(a)*r,0,math.sin(a)*r))
  faces=[(i*2,i*2+1,((i+1)%48)*2+1,((i+1)%48)*2) for i in range(48)]
 faces=[tuple(reversed(f)) for f in faces]
 mesh=bpy.data.meshes.new(n);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(n,mesh);bpy.context.collection.objects.link(o);o.matrix_world=ROT;o.data.materials.append(edge);current.append(o);export(n)
def height(x,z):
 d=math.sqrt(x*x+z*z);t=max(0,min(1,(d-40)/60));t=t*t*(3-2*t)
 return t*(math.sin(x*.013)*math.cos(z*.011)*18+math.sin(z*.027+x*.009)*7)
verts=[(x,height(x,z),z) for z in range(-500,501,5) for x in range(-500,501,5)]
faces=[];N=201
for z in range(N-1):
 for x in range(N-1):
  k=z*N+x;faces.append((k,k+N,k+N+1,k+1))
mesh=bpy.data.meshes.new('Rolling plain terrain');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('Rolling plain terrain',mesh);bpy.context.collection.objects.link(o);o.matrix_world=ROT
for m in stone:o.data.materials.append(m)
for p in o.data.polygons:p.material_index=(p.index//N+p.index%N)//9%len(stone)
current.append(o);export('PlainsTerrain')
os.makedirs(os.path.join(BASE,'ArtSource/PD14'),exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'ArtSource/PD14/Props_v001.blend'))
print('PD_PARTS_PASS',list(assets))
