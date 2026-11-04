import bpy,math,os
from mathutils import Vector,Matrix,noise
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest';OUT=os.path.join(BASE,'Assets/Resources/PDParts');ROT=Matrix.Rotation(math.pi/2,4,'X')
bpy.ops.wm.read_factory_settings(use_empty=True)
def mat(n,c):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);return m
white=mat('FX neutral tint',(1,1,1));blue=mat('Distant blue sky',(.5,.7,.9))
def export(o,n):
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,n+'.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False)
for n in ['SmokePuff','WaterDrop','SparkChunk']:
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.5);o=bpy.context.object;o.name=n;o.data.materials.append(white);export(o,n)
verts=[];faces=[]
for z,r in [(0,.12),(.25,.20),(1,.025)]:
 for i in range(12):a=i*math.tau/12;verts.append((math.cos(a)*r,math.sin(a)*r,z))
for ring in range(2):
 for i in range(12):a=ring*12+i;b=ring*12+(i+1)%12;faces.append((a,b,b+12,a+12))
mesh=bpy.data.meshes.new('Energy exhaust');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('BoostCone',mesh);bpy.context.collection.objects.link(o);o.data.materials.append(white);o.matrix_world=ROT;export(o,'BoostCone')
# Inward sky dome and a cloud atlas authored entirely in Blender.
W,H=512,256;image=bpy.data.images.new('SkyGradient',width=W,height=H);pixels=[]
for j in range(H):
 v=j/(H-1)
 for i in range(W):
  u=i/(W-1);n=noise.noise(Vector((math.cos(u*math.tau)*3,math.sin(u*math.tau)*3,v*7)))
  cloud=max(0,min(1,(n-.05)*3))*max(0,min(1,(v-.32)*4))
  c=(.64*(1-v)+.16*v,.77*(1-v)+.45*v,.84*(1-v)+.76*v)
  pixels.extend([c[k]*(1-cloud*.65)+.97*cloud*.65 for k in range(3)]+[1])
image.pixels=pixels;image.filepath_raw=os.path.join(OUT,'SkyGradient.png');image.file_format='PNG';image.save()
verts=[];faces=[];N,M=64,32
for j in range(M+1):
 p=j*math.pi/M
 for i in range(N+1):
  a=i*math.tau/N;verts.append((math.sin(p)*math.cos(a),math.cos(p),math.sin(p)*math.sin(a)))
for j in range(M):
 for i in range(N):
  k=j*(N+1)+i;faces.append((k,k+1,k+N+2,k+N+1))
mesh=bpy.data.meshes.new('Sky dome');mesh.from_pydata(verts,[],faces);mesh.update();uv=mesh.uv_layers.new()
for poly in mesh.polygons:
 for loop in poly.loop_indices:
  vi=mesh.loops[loop].vertex_index;uv.data[loop].uv=(vi%(N+1)/N,1-vi//(N+1)/M)
o=bpy.data.objects.new('SkyDome',mesh);bpy.context.collection.objects.link(o);o.matrix_world=ROT;o.data.materials.append(blue);export(o,'SkyDome')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(BASE,'ArtSource/PD14/SkyFX_v001.blend'));print('SKY_FX_EXPORT_PASS')
