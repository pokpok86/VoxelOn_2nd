import bpy,math,os
from mathutils import Matrix
root=r'D:\Codex\VoxSky\Builds\WorkLatest'
bpy.ops.wm.read_factory_settings(use_empty=True)
N=64;verts=[(0,0,0)];alphas=[1]
for radius,alpha in [(.72,1),(1,0)]:
 for i in range(N):
  a=i*math.tau/N;edge=1+.025*math.sin(a*5)+.018*math.cos(a*7);verts.append((math.cos(a)*radius*edge,0,math.sin(a)*radius*edge));alphas.append(alpha)
faces=[]
for i in range(N):
 j=(i+1)%N;faces.append((0,j+1,i+1));faces.append((i+1,j+1,N+j+1,N+i+1))
mesh=bpy.data.meshes.new('Soft edge shallow water');mesh.from_pydata(verts,[],faces);mesh.update();colors=mesh.color_attributes.new(name='WaterEdge',type='FLOAT_COLOR',domain='POINT')
for i,alpha in enumerate(alphas):colors.data[i].color=(1,1,1,alpha)
o=bpy.data.objects.new('ShallowPond_v002',mesh);bpy.context.collection.objects.link(o);o.matrix_world=Matrix.Rotation(math.pi/2,4,'X');o.select_set(True);bpy.context.view_layer.objects.active=o
bpy.ops.export_scene.fbx(filepath=os.path.join(root,'Assets/Resources/PDParts/ShallowPond_v002.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'ArtSource/PD14/ShallowWater_v002.blend'));print('SHALLOW_WATER_EXPORT_PASS',len(verts),len(faces),'alpha',min(alphas),max(alphas))