import bpy, math, os, colorsys
from mathutils import Vector
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
ART=os.path.join(BASE,'Assets/Art/Robots/WISP'); DOC=os.path.join(BASE,'Docs/Models')
ENV=os.path.join(BASE,'Assets/Resources/Environment'); SRC=os.path.join(BASE,'ArtSource/Environment')
os.makedirs(ENV,exist_ok=True);os.makedirs(SRC,exist_ok=True)
rig=bpy.data.objects['WISP_Rig_v003']
for o in list(bpy.data.objects):
 if o.type=='MESH' and not any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers):bpy.data.objects.remove(o,do_unlink=True)
for o in list(bpy.data.objects):
 if o.type in {'CAMERA','LIGHT'}:bpy.data.objects.remove(o,do_unlink=True)
scene=bpy.context.scene;scene.frame_set(1)
for p in rig.pose.bones:p.rotation_euler=(0,0,0)
# Preserve the existing geometry and mechanical rig; only tone and small antenna silhouette changes.
for image in list(bpy.data.images):
 if image.source not in {'FILE','GENERATED'} or image.size[0]==0:continue
 key=(image.name+' '+image.filepath).lower()
 if 'wisp' not in key:continue
 emission='emission' in key
 out=bpy.data.images.new('WISP_Emission_v004' if emission else 'WISP_BaseColor_v004',width=image.size[0],height=image.size[1],alpha=True)
 pixels=list(image.pixels)
 for i in range(0,len(pixels),4):
  r,g,b=pixels[i:i+3];h,s,v=colorsys.rgb_to_hsv(r,g,b)
  if emission:
   power=max(r,g,b);new=(.12*power,.58*power,.48*power) if power>.02 else (0,0,0)
  elif v<.12:new=(v*.9,v*1.05,v*1.12)
  elif s<.18:new=(v*.68,v*.73,v*.75)
  elif h>.94 or h<.075:new=(v*.28,v*.33,v*.34)
  elif .08<h<.20:new=(v*.52,v*.43,v*.29)
  elif .2<=h<.5:new=(v*.15,v*.48,v*.40)
  else:new=(v*.22,v*.36,v*.44)
  pixels[i:i+3]=new
 out.pixels=pixels;out.filepath_raw=os.path.join(ART,'Textures',out.name+'.png');out.file_format='PNG';out.save();out.pack()
 for mat in bpy.data.materials:
  if mat.use_nodes:
   for n in mat.node_tree.nodes:
    if n.type=='TEX_IMAGE' and n.image==image:n.image=out
for o in bpy.data.objects:
 if o.type=='MESH' and 'horn' in o.name.lower():
  for v in o.data.vertices:
   if v.co.y>2.35:v.co.y=2.35+(v.co.y-2.35)*.85
# Studio renders use the genuine Blender mesh and packed v004 atlas.
def aim(o,at):o.rotation_euler=(Vector(at)-o.location).to_track_quat('-Z','Y').to_euler()
cam=bpy.data.objects.new('WISP v004 review camera',bpy.data.cameras.new('WISP v004 review'));bpy.context.collection.objects.link(cam)
cam.location=(0,-8,1.4);aim(cam,(0,0,1.4));cam.data.type='ORTHO';cam.data.ortho_scale=3.6;scene.camera=cam
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=850;scene.render.resolution_y=850;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
scene.world.color=(.16,.18,.21)
for name,loc,power in [('Key',(-3,-4,6),400),('Fill',(4,-2,4),240),('Rim',(0,4,5),350)]:
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.size=5;o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=loc;aim(o,(0,0,1.3))
bpy.ops.object.select_all(action='DESELECT');rig.select_set(True)
for o in bpy.data.objects:
 if o.type=='MESH' and any(m.type=='ARMATURE' and m.object==rig for m in o.modifiers):o.select_set(True)
bpy.context.view_layer.objects.active=rig
bpy.ops.export_scene.fbx(filepath=os.path.join(ART,'WISP_v004.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y',primary_bone_axis='Y',secondary_bone_axis='X',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',path_mode='AUTO')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ART,'Source/WISP_v004.blend'))
for label,loc in [('front',(0,-8,1.4)),('rear',(0,8,1.4)),('three-quarter',(5,-8,3.0))]:
 cam.location=loc;aim(cam,(0,0,1.4));scene.render.filepath=os.path.join(DOC,'WISP-v004-'+label+'.png');bpy.ops.render.render(write_still=True)
# Environment kit: Blender meshes, material slots and FBXs, never runtime voxel construction.
bpy.ops.wm.read_factory_settings(use_empty=True)
PALETTE=[(.22,.32,.36,1),(.09,.14,.17,1),(.4,.47,.48,1),(.68,.46,.23,1),(.035,.055,.065,1),(.14,.54,.49,1),(.4,.32,.26,1),(.31,.25,.22,1),(.5,.4,.31,1),(.30,.36,.20,1)]
mats=[]
for i,c in enumerate(PALETTE):
 m=bpy.data.materials.new('VoxelOn_Environment_'+str(i));m.diffuse_color=c;m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=c;p.inputs['Roughness'].default_value=.68;p.inputs['Metallic'].default_value=.35 if i<6 else 0
 if i==5:p.inputs['Emission Color'].default_value=c;p.inputs['Emission Strength'].default_value=.8
 mats.append(m)
CORNERS=[(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]
FACES=[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
class Kit:
 def __init__(self,name):self.name=name;self.v=[];self.f=[];self.mi=[]
 def box(self,p,s,color):
  n=len(self.v);self.v.extend([(p[0]+c[0]*s[0]/2,p[1]+c[1]*s[1]/2,p[2]+c[2]*s[2]/2) for c in CORNERS]);self.f.extend([tuple(n+i for i in f) for f in FACES]);self.mi.extend([color]*6)
 def export(self):
  mesh=bpy.data.meshes.new(self.name);mesh.from_pydata(self.v,[],self.f);mesh.update()
  obj=bpy.data.objects.new(self.name,mesh);bpy.context.collection.objects.link(obj)
  for m in mats:mesh.materials.append(m)
  for p,i in zip(mesh.polygons,self.mi):p.material_index=i
  bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
  bpy.ops.export_scene.fbx(filepath=os.path.join(ENV,self.name+'.fbx'),use_selection=True,object_types={'MESH'},bake_anim=False,axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',path_mode='AUTO')
  return obj
for kind,name in enumerate(['FoundryCargo','FoundryGenerator','FoundryBarricade','CanyonRock']):
 k=Kit(name);rock=kind==3;main=6 if rock else 0 if kind==0 else 1 if kind==1 else 2
 k.box((0,0,.5),(.98,.98,.98),7 if rock else 1)
 nx=24;ny=18
 for side in [-1,1]:
  for x in range(nx):
   for y in range(ny):
    shade=main if (x*11+y*7)%19>2 else (8 if rock else 2)
    if rock and y%5==0:shade=7
    if not rock and y==2:shade=3 if x%5<3 else 4
    if kind==1 and 6<y<12:shade=4 if y%2 else 2
    k.box((-.5+(x+.5)/nx,side*(.5-.014), (y+.5)/ny),(1/nx-.003,.028,1/ny-.004),shade)
    k.box((side*(.5-.014),-.5+(x+.5)/nx,(y+.5)/ny),(.028,1/nx-.003,1/ny-.004),shade)
 for x in range(nx):
  for y in range(nx):
   shade=main if (x*3+y*7)%23>3 else (8 if rock else 2)
   if kind==1 and 5<x<19 and 4<y<20:shade=4 if x%2 else 2
   if rock and (x+y)%17==0:shade=9
   k.box((-.5+(x+.5)/nx,-.5+(y+.5)/nx,.985),(1/nx-.002,1/nx-.002,.03),shade)
 if not rock:
  for sx in [-1,1]:
   for sy in [-1,1]:k.box((sx*.475,sy*.475,.5),(.05,.05,1),2)
  for i in range(8):k.box(((i-3.5)*.025,-.492,.74),(.018,.016,.025),5 if i else 3)
  for sx in [-1,1]:
   for z in [.1,.5,.9]:k.box((sx*.475,-.497,z),(.012,.006,.015),4)
 k.export()
# Broad outdoor ground, each cell has true thickness with upper faces precisely at Z=0.
k=Kit('OutdoorGround')
for x in range(-35,35):
 for y in range(-35,35):
  n=math.sin(x*.21)+math.cos(y*.17)+math.sin((x+y)*.11)
  shade=9 if n>1.85 else 8 if (x*11+y*7)%17<3 else 6
  k.box((x*4+2,y*4+2,-.006),(3.975,3.975,.012),shade)
k.export()
k=Kit('CanyonVista')
for i in range(64):
 a=i*math.pi/32;r=166+(i%4)*6;px=math.sin(a)*r;py=math.cos(a)*r;h=29+13*(math.sin(i*.71)+1)
 for tier in range(9):
  w=15-tier*.6+(i%3);th=h/9
  k.box((px,py,(tier+.5)*th),(w,w*1.2,th-.07),9 if tier==8 else 6+tier%3)
  if tier in [3,6]:k.box((px,py,(tier+1)*th),(w+1,w*1.2+1,.4),8)
k.export()
k=Kit('FoundryBackdrop')
for axis in [0,1]:
 for side in [-1,1]:
  for row in range(13):
   for col in range(-23,24):
    a=col*6;z=(row+.5)*5
    p=(side*139.98,a,z) if axis==0 else (a,side*139.98,z)
    s=(.16,5.85,4.85) if axis==0 else (5.85,.16,4.85)
    k.box(p,s,1 if (col+row)%4 else 0)
    if row%3==1:
     p=(side*139.86,a,z+.8) if axis==0 else (a,side*139.86,z+.8)
     k.box(p,(.15,3.8,.25) if axis==0 else (3.8,.15,.25),5 if col%3==0 else 3)
    if col%7==0:k.box((side*139.88,a,z) if axis==0 else (a,side*139.88,z),(.20,.5,5) if axis==0 else (.5,.20,5),2)
for side in [-1,1]:
 for i in range(5):
  x=side*(48+i*14);y=-(45+i%2*16);h=6+i%3*3
  # Small details surround existing analytical factory solids.
  for z in range(1,int(h)*2):
   for j in range(-8,9):
    k.box((x+j*.5,y+6.02,z*.5),(.46,.12,.44),0 if (j+z)%11 else 2)
  k.box((x,y,h+.15),(10,13,.3),2)
  for j in range(-3,4):k.box((x+j,y+6.10,h*.65),(.6,.08,.15),5 if j%3 else 3)
# Rooftop gantries and distant exhaust towers behind the perimeter.
for i in range(16):
 a=i*math.pi/8;r=155;p=(math.sin(a)*r,math.cos(a)*r);h=32+i%4*8
 k.box((p[0],p[1],h/2),(7,8,h),1)
 for z in range(4,int(h),4):k.box((p[0],p[1],z),(7.8,8.8,.3),2)
 k.box((p[0],p[1],h+.4),(2,3,.8),3)
k.export()
# Save every kit object and its material library in one editable environment source.
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(SRC,'VoxelOn_Environment_v001.blend'))
print('VOXELON_BLENDER_ASSETS_PASS WISP_v004 and 7 environment FBXs')
