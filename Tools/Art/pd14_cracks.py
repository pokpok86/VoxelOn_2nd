import bpy,math,os
from mathutils import Matrix,Vector
B=r'D:\Codex\VoxSky\Builds\WorkLatest';bpy.ops.wm.read_factory_settings(use_empty=True);rot=Matrix.Rotation(math.pi/2,4,'X');m=bpy.data.materials.new('Fracture charcoal');m.diffuse_color=(.025,.03,.035,1)
for i,(x,z,a,l) in enumerate([(0,0,.5,.8),(.17,.15,-.8,.42),(-.12,-.2,-.65,.3)]):
 bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.name='Crack stroke '+str(i);o.matrix_world=rot@Matrix.Translation(Vector((x,0,z)))@Matrix.Rotation(a,4,'Y')@Matrix.Diagonal(Vector((.018,.006,l,1)));o.data.materials.append(m)
bpy.ops.object.select_all(action='SELECT');bpy.ops.export_scene.fbx(filepath=os.path.join(B,'Assets/Resources/PDParts/Crack.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(B,'ArtSource/PD14/Cracks_v001.blend'));print('CRACK_EXPORT_PASS')
