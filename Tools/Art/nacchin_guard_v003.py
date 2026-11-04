import bpy,os,math
from mathutils import Matrix,Vector
B=r'D:\Codex\VoxSky\Builds\WorkLatest';bpy.ops.wm.read_factory_settings(use_empty=True)
rot=Matrix.Rotation(math.pi/2,4,'X');parts=[]
for n,c in [('VN_Ivory',(.84,.85,.8)),('VN_Orange',(.98,.33,.035)),('VN_Dark',(.045,.055,.052))]:m=bpy.data.materials.new(n);m.diffuse_color=(*c,1)
for n,p,s,mat in [('Guard body',(0,-.12,0),(.48,.67,.10),'VN_Ivory'),('Guard inner',(0,-.12,-.063),(.36,.53,.035),'VN_Dark'),('Guard orange stripe',(0,.02,.058),(.35,.06,.025),'VN_Orange')]:
 bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;o.name=n;o.matrix_world=rot@Matrix.Translation(Vector(p))@Matrix.Diagonal(Vector((*s,1)));bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);mod=o.modifiers.new('Soft guard edges','BEVEL');mod.width=.02;mod.segments=2;bpy.ops.object.modifier_apply(modifier=mod.name);o.data.materials.append(bpy.data.materials[mat]);parts.append(o)
bpy.ops.object.select_all(action='SELECT');out=os.path.join(B,'Assets/Art/Robots/WISP');bpy.ops.export_scene.fbx(filepath=os.path.join(out,'WISP_Shield_v003.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL');bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out,'Source/WISP_Shield_v003.blend'));print('NATIVE_GUARD_EXPORT_PASS')
