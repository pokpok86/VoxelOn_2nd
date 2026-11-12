import bpy,os,sys
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
part=sys.argv[sys.argv.index('--part')+1];folder=os.path.join(ROOT,'HeadCore-20261112',part)
path=os.path.join(folder,part+'-Mounted.blend');bpy.ops.wm.open_mainfile(filepath=path)
bpy.context.scene.camera.data.ortho_scale=1.70
bpy.context.scene.render.filepath=os.path.join(folder,part+'-07-Mounted.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=path);print('HEAD_CORE_MOUNT_FRAMING_PASS',part,flush=True)
