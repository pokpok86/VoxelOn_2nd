import bpy,math,os
from mathutils import Matrix
root=r'D:\Codex\VoxSky\Builds\WorkLatest'
bpy.ops.wm.read_factory_settings(use_empty=True)
mat=bpy.data.materials.new('Neutral exhaust tint');mat.diffuse_color=(1,1,1,1)
verts=[];faces=[]
for z,r in [(0,.20),(.18,.30),(.65,.17),(1.35,.015)]:
 for i in range(12):a=i*math.tau/12;verts.append((math.cos(a)*r,math.sin(a)*r,z))
for ring in range(3):
 for i in range(12):a=ring*12+i;b=ring*12+(i+1)%12;faces.append((a,b,b+12,a+12))
mesh=bpy.data.meshes.new('Tapered exhaust v2');mesh.from_pydata(verts,[],faces);mesh.update()
o=bpy.data.objects.new('BoostCone_v002',mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mat);o.matrix_world=Matrix.Rotation(math.pi/2,4,'X');o.select_set(True);bpy.context.view_layer.objects.active=o
bpy.ops.export_scene.fbx(filepath=os.path.join(root,'Assets/Resources/PDParts/BoostCone_v002.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'ArtSource/PD14/Exhaust_v002.blend'))
print('EXHAUST_V2_EXPORT_PASS vertices',len(verts))