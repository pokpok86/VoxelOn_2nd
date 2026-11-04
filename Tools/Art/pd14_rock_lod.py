import bpy,os,math
from mathutils import Matrix,Vector
B=r'D:\Codex\VoxSky\Builds\WorkLatest';bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.name='Natural rock distant hull';o.matrix_world=Matrix.Rotation(math.pi/2,4,'X')@Matrix.Translation(Vector((0,.5,0)))
m=bpy.data.materials.new('Distant warm stone');m.diffuse_color=(.35,.31,.25,1);o.data.materials.append(m)
bev=o.modifiers.new('Broad eroded corners','BEVEL');bev.width=.08;bev.segments=1;bpy.ops.object.modifier_apply(modifier=bev.name)
bpy.ops.export_scene.fbx(filepath=os.path.join(B,'Assets/Resources/PDParts/NaturalRockLOD.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL')
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(B,'ArtSource/PD14/RockLOD_v001.blend'));print('ROCK_LOD_PASS')
