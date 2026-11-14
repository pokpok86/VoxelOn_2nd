"""Frame full assemblies without resizing parts or cropping weapon tips."""
import bpy,json,hashlib
from pathlib import Path
from mathutils import Vector
OUT=Path(__file__).resolve().parent.parent/'Combination-Review-20261112'
catalog=json.loads((OUT/'PART_COMBINATIONS.json').read_text(encoding='utf-8'))
for r in catalog['combinations']:
    n=r['number'];path=OUT/r['blend'];bpy.ops.wm.open_mainfile(filepath=str(path));scene=bpy.context.scene;points=[]
    for c in bpy.data.collections:
        if 'MODULE' not in c.name:continue
        for o in c.objects:
            if o.type=='MESH':points.extend(o.matrix_world@Vector(v) for v in o.bound_box)
    assert points
    cams=[]
    for camname,label in [('Combination_Front45','01-Front45'),('Combination_Front','02-Front'),('Combination_Back45','03-Back45')]:
        cam=bpy.data.objects[camname];q=cam.rotation_euler.to_quaternion();right=q@Vector((1,0,0));up=q@Vector((0,1,0));x=[p.dot(right) for p in points];y=[p.dot(up) for p in points]
        lo,hi=min(x),max(x);bottom,top=min(y),max(y)
        cam.location+=right*((lo+hi)/2-cam.location.dot(right))+up*((bottom+top)/2-cam.location.dot(up))
        aspect=scene.render.resolution_x/scene.render.resolution_y;cam.data.ortho_scale=max(top-bottom,(hi-lo)/aspect)*1.12
        assert (hi-lo)<cam.data.ortho_scale*aspect and (top-bottom)<cam.data.ortho_scale
        scene.camera=cam;scene.render.filepath=str(OUT/n/(n+'-'+label+'.png'));bpy.ops.render.render(write_still=True)
        cams.append({'name':camname,'ortho_scale':cam.data.ortho_scale,'full_bbox_inside_frame':True})
    scene.camera=bpy.data.objects['Combination_Front45'];bpy.ops.wm.save_as_mainfile(filepath=str(path))
    for p,h in r['sources_sha256'].items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h
    r['camera_framing']=cams
    (OUT/n/'manifest.json').write_text(json.dumps(r,indent=2),encoding='utf-8')
    print('COMBINATION_FULL_FRAME_PASS',n,flush=True)
(OUT/'PART_COMBINATIONS.json').write_text(json.dumps(catalog,indent=2),encoding='utf-8')
print('COMBINATION_FRAMING_FOUR_PASS',flush=True)
