import bpy,math,os,bmesh
from mathutils import Matrix
root=r'D:\Codex\VoxSky\Builds\WorkLatest';bpy.ops.wm.read_factory_settings(use_empty=True);made=[]
mat=bpy.data.materials.new('Neutral projectile tint');mat.diffuse_color=(1,1,1,1)
def finish(mesh,name):
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh);bm.free();o=bpy.data.objects.new(name,mesh);collection.objects.link(o);o.matrix_world=Matrix.Rotation(math.pi/2,4,'X');o.data.materials.append(mat);made.append(o)
def cone(name,z0,z1,r0,r1):
 verts=[];faces=[]
 for z,r in [(z0,r0),(z1,r1)]:
  for i in range(8):a=i*math.tau/8;verts.append((math.cos(a)*r,math.sin(a)*r,z))
 faces=[tuple(range(7,-1,-1)),tuple(range(8,16))]
 for i in range(8):j=(i+1)%8;faces.append((i,j,j+8,i+8))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();finish(mesh,name)
def fin(name,axis,sign):
 coords=[(.2,-.08,-1),(.82,-.08,-1),(.4,-.08,-.2),(.2,.08,-1),(.82,.08,-1),(.4,.08,-.2)]
 verts=[(x*sign,y,z) if axis==0 else (y,x*sign,z) for x,y,z in coords];faces=[(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)]
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();finish(mesh,name)
for kind in ['Rifle','Cannon','Rocket','Rail']:
 made=[];collection=bpy.data.collections.new('Projectile / '+kind);bpy.context.scene.collection.children.link(collection)
 if kind=='Rifle':cone('Shell casing',-.8,.2,.42,.38);cone('Core tracer',.2,1.2,.38,.03)
 if kind=='Cannon':cone('Shell slug',-.8,.65,.78,.78);cone('Core nose',.65,1.3,.78,.04)
 if kind=='Rocket':
  cone('Shell motor',-1.1,.5,.42,.42);cone('Core nose',.5,1.4,.43,.025);cone('Core exhaust',-1.5,-1.1,.12,.33)
  for axis in [0,1]:
   for sign in [-1,1]:fin('Shell fin '+str(axis)+str(sign),axis,sign)
 if kind=='Rail':cone('Core streak',-3,3,.12,.04);cone('Shell collar',-.2,.2,.35,.35)
 bpy.ops.object.select_all(action='DESELECT')
 for o in made:o.select_set(True)
 bpy.context.view_layer.objects.active=made[0];path=os.path.join(root,'Assets/Resources/PDParts/Projectile'+kind+'_v002.fbx');bpy.ops.export_scene.fbx(filepath=path,use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False)
 print('PROJECTILE_EXPORT_PASS',kind,sum(len(o.data.vertices) for o in made))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(root,'ArtSource/PD14/Projectiles_v002.blend'))