"""C-01 only: redistribute armor depth within its existing replacement envelope."""
import bpy,bmesh,os,sys,json,hashlib,shutil,math
from mathutils import Vector,Matrix
HERE=os.path.dirname(os.path.abspath(__file__));sys.path.insert(0,HERE)
import modeling_common as M
ROOT=os.path.dirname(HERE);SOURCE=os.path.join(ROOT,'HeadCore-20261112','C-01_STANDARD')
OUT=os.path.join(ROOT,'C01-Depth-Review-20261112');os.makedirs(OUT,exist_ok=True)
def sha(p):return hashlib.sha256(open(p,'rb').read()).hexdigest()
preserved={}
for folder in ['HeadCore-20261112','SlimHeads-20261112']:
    for current,dirs,files in os.walk(os.path.join(ROOT,folder)):
        for name in files:
            if name.endswith('.blend'):preserved[os.path.join(current,name)]=sha(os.path.join(current,name))
baseline=json.load(open(os.path.join(ROOT,'APPROVED_BASELINE.json')))
for rel,h in baseline['geometry_sha256'].items():preserved[os.path.join(os.path.dirname(ROOT),rel)]=h
for p,h in preserved.items():assert sha(p)==h,p
old_manifest=json.load(open(os.path.join(SOURCE,'manifest.json')))
for suffix in ['01-Front','02-Right','05-45deg','07-Mounted']:
    shutil.copy2(os.path.join(SOURCE,'C-01_STANDARD-'+suffix+'.png'),os.path.join(OUT,'Before-'+suffix+'.png'))
bpy.ops.wm.open_mainfile(filepath=os.path.join(SOURCE,'C-01_STANDARD.blend'))
scene=bpy.context.scene;COL=bpy.data.collections['C-01_STANDARD_MODULE'];M.COL=COL;root=bpy.data.objects['C-01_STANDARD_ROOT']
ARMOR=bpy.data.materials['Ivory_Armor'];SHADE=bpy.data.materials['Secondary_Armor'];DARK=bpy.data.materials['Graphite_Inner_Frame'];METAL=bpy.data.materials['Machined_Joint_Metal'];ORANGE=bpy.data.materials['Flush_Orange_Inlay']
def mesh(n,verts,faces,mat,ch=.0008):
    data=bpy.data.meshes.new(n+'_mesh');data.from_pydata(verts,[],faces);data.update()
    o=bpy.data.objects.new(n,data);COL.objects.link(o);data.materials.append(mat);o.parent=root
    bm=bmesh.new();bm.from_mesh(data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(data);bm.free();return M.bevel(o,ch,1)
def bounds(objects):
    points=[];deps=bpy.context.evaluated_depsgraph_get()
    for o in objects:
        evaluated=o.evaluated_get(deps);me=evaluated.to_mesh();points.extend(o.matrix_world@v.co for v in me.vertices);evaluated.to_mesh_clear()
    return [min(p[i] for p in points) for i in range(3)],[max(p[i] for p in points) for i in range(3)]
def center(o):a,b=bounds([o]);return [(x+y)/2 for x,y in zip(a,b)]
def find(tag):return next(o for o in COL.objects if o.type=='MESH' and tag in o.name)
def interface_state():
    result={}
    for side in ['L','R']:
        for part in ['Shoulder_Can_X','UpperArm_Connector_Pin','UpperArm_Connector_Stub']:
            o=find(side+'_'+part);result[side+'_'+part]={'center':center(o),'bounds':bounds([o])}
    result['neck_plane']=bounds([find('Neck_Head_Interface_Compatible')])[1][2]
    result['waist_plane']=bounds([find('Waist_Pelvis_Interface_Key_Compatible')])[0][2]
    return result
before_interfaces=interface_state();before_bounds=bounds([o for o in COL.objects if o.type=='MESH'])
before_front=bounds([find('Continuous_Chest_Main_Plate')])[0][1]
remove=['Continuous_Chest_Main_Plate','Chest_Lower_Faceted_Return','Compact_Continuous_Inner_Torso','Lower_Abdomen_Armor','Short_Abdomen_Core','Lower_Side_Brace','Small_Central_Chest_Keystone','Chest_Recessed_Duct','Three_Flush_Chest_Indicators','Chest_Fastener']
for o in list(COL.objects):
    if o.type=='MESH' and any(tag in o.name for tag in remove):bpy.data.objects.remove(o,do_unlink=True)

def loft(n,levels,mat):
    loop=[(-.7,-1),(.7,-1),(1,-.7),(1,.7),(.7,1),(-.7,1),(-1,.7),(-1,-.7)]
    v=[(x*rx,y*ry,z) for z,rx,ry in levels for x,y in loop];f=[tuple(range(7,-1,-1))]
    for k in range(len(levels)-1):
        for i in range(8):j=(i+1)%8;f.append((k*8+i,k*8+j,(k+1)*8+j,(k+1)*8+i))
    f.append(tuple(range((len(levels)-1)*8,len(levels)*8)));return mesh(n,v,f,mat)

# The original frontmost detail reached -0.0915m. The broad breastplate now
# occupies that depth, while the flanking panels recede; no global scaling.
loft('C01_Depth_Inverted_Triangle_Inner_Frame',[(-.125,.026,.026),(-.088,.039,.029),(-.047,.061,.038),(.005,.099,.049),(.050,.097,.047),(.063,.081,.040)],DARK)
outer=[(-.047,-.060,.059),(.047,-.060,.059),(.074,-.047,.030),(.060,-.055,-.011),(.032,-.063,-.026),(-.032,-.063,-.026),(-.060,-.055,-.011),(-.074,-.047,.030)]
inner=[(-.034,-.0905,.044),(.034,-.0905,.044),(.048,-.0905,.024),(.037,-.0905,-.003),(.023,-.0905,-.014),(-.023,-.0905,-.014),(-.037,-.0905,-.003),(-.048,-.0905,.024)]
back=[(x,y+.018,z) for x,y,z in outer];v=back+outer+inner;f=[tuple(range(7,-1,-1)),tuple(range(16,24))]
for i in range(8):j=(i+1)%8;f.extend([(i,j,8+j,8+i),(8+i,8+j,16+j,16+i)])
breast=mesh('C01_Depth_Forward_Central_Breastplate',v,f,ARMOR)
breast.data.materials.append(SHADE)
for i in [3,4,5]:breast.data.polygons[3+i*2].material_index=1
side_panels=[]
for sign in [-1,1]:
    front=[(sign*x,y,z) for x,y,z in [(.060,-.055,.049),(.095,-.043,.042),(.108,-.034,.012),(.091,-.035,-.030),(.061,-.048,-.040),(.047,-.060,-.008)]]
    v=[(x,y+.024,z) for x,y,z in front]+front;f=[tuple(range(5,-1,-1)),tuple(range(6,12))]
    for i in range(6):j=(i+1)%6;f.append((i,j,6+j,6+i))
    side=mesh('C01_Depth_Recessed_Side_Armor_'+('L' if sign<0 else 'R'),v,f,ARMOR);side_panels.append(side)
    side.data.materials.append(SHADE);side.data.materials.append(ORANGE);side.data.polygons[2].material_index=2;side.data.polygons[5].material_index=1
    # A lower oblique sidewall carries the breast volume into the waist taper.
    front=[(sign*x,y,z) for x,y,z in [(.056,-.044,-.035),(.075,-.027,-.044),(.053,-.025,-.073),(.029,-.027,-.104),(.028,-.037,-.075)]]
    v=[(x,y+.015,z) for x,y,z in front]+front;f=[tuple(range(4,-1,-1)),tuple(range(5,10))]
    for i in range(5):j=(i+1)%5;f.append((i,j,5+j,5+i))
    mesh('C01_Depth_Tapered_Flank_'+('L' if sign<0 else 'R'),v,f,SHADE)
front=[(-.042,-.051,-.052),(.042,-.051,-.052),(.033,-.039,-.083),(.023,-.028,-.119),(-.023,-.028,-.119),(-.033,-.039,-.083)]
v=[(x,y+.012,z) for x,y,z in front]+front;f=[tuple(range(5,-1,-1)),tuple(range(6,12))]
for i in range(6):j=(i+1)%6;f.append((i,j,6+j,6+i))
mesh('C01_Depth_Sloped_Abdomen_Armor',v,f,ARMOR)

# Sweep the existing shoulder shells back, leaving the mechanical interfaces
# and the short upper-arm meshes precisely where they were.
deps=bpy.context.evaluated_depsgraph_get()
for o in list(COL.objects):
    if o.type!='MESH' or not any(tag in o.name for tag in ['Shoulder_Main_Angular_Cap','Standard_Lower_Return','Standard_Orange_Corner','Shoulder_Recessed_Bolt']):continue
    data=bpy.data.meshes.new_from_object(o.evaluated_get(deps));data.transform(o.matrix_world)
    for vertex in data.vertices:vertex.co.y+=max(0,min(1,(abs(vertex.co.x)-.165)/.067))*.008
    o.modifiers.clear();o.data=data;o.matrix_world=Matrix.Identity(4)
bpy.context.view_layer.update();after_interfaces=interface_state();assert before_interfaces==after_interfaces,'An interface moved'
after_bounds=bounds([o for o in COL.objects if o.type=='MESH'])
old_size=[b-a for a,b in zip(*before_bounds)];new_size=[b-a for a,b in zip(*after_bounds)]
assert abs(new_size[0]-old_size[0])<.00001 and abs(new_size[2]-old_size[2])<.00001
assert new_size[1]<=old_size[1]+.00001,(old_size,new_size)
assert after_bounds[0][1]>=before_bounds[0][1]-.00001

scene.render.resolution_x=900;scene.render.resolution_y=900
for suffix,cam in [('01-Front','Camera_01-Front'),('02-Side','Camera_02-Right'),('03-45deg','Camera_05-45deg')]:
    scene.camera=bpy.data.objects[cam];scene.render.filepath=os.path.join(OUT,'C01-Depth-'+suffix+'.png');bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['Camera_05-45deg'];candidate=os.path.join(OUT,'C-01_STANDARD-Depth-v1.blend');bpy.ops.wm.save_as_mainfile(filepath=candidate)

# Reuse the actual old mounted scene: replace its core collection only.
bpy.ops.wm.open_mainfile(filepath=os.path.join(SOURCE,'C-01_STANDARD-Mounted.blend'))
old_root=bpy.data.objects['C-01_STANDARD_ROOT'];body_at=old_root.location.copy();old_col=bpy.data.collections['C-01_STANDARD_MODULE']
other_roots=['HD01_ROOT','LG01_PAIR_ROOT','W-01_LUMEN_ROOT','W-01_LUMEN_LEFT_ROOT']
unchanged_others={n:[list(row) for row in bpy.data.objects[n].matrix_world] for n in other_roots}
for o in list(old_col.all_objects):bpy.data.objects.remove(o,do_unlink=True)
bpy.data.collections.remove(old_col)
with bpy.data.libraries.load(candidate,link=False) as (src,dst):dst.collections=['C-01_STANDARD_MODULE']
col=dst.collections[0];bpy.context.scene.collection.children.link(col);new_root=bpy.data.objects['C-01_STANDARD_ROOT'];new_root.location=body_at;bpy.context.view_layer.update()
for side,sign in [('L',-1),('R',1)]:
    pivot=body_at+Vector((sign*.14994,0,.02016));rot=Matrix.Rotation(math.radians(-sign*30),4,'Y')
    for o in col.objects:
        if 'UpperArm_Connector' in o.name and side+'_Upper' in o.name:o.matrix_world=Matrix.Translation(pivot)@rot@Matrix.Translation(-pivot)@o.matrix_world
bpy.context.view_layer.update()
assert unchanged_others=={n:[list(row) for row in bpy.data.objects[n].matrix_world] for n in other_roots}
scene=bpy.context.scene;scene.render.filepath=os.path.join(OUT,'C01-Depth-04-Mounted-45deg.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'C-01_STANDARD-Depth-v1-Mounted.blend'))
for p,h in preserved.items():assert sha(p)==h,('Preserved source changed',p)
report={'candidate':'C-01_STANDARD Depth v1','scope':'C-01 only; C-02/C-03/C-04 and heads untouched','blender':bpy.app.version_string,'automation':'PASS','design_review':'PENDING_PD','unity_integration':'NOT_PERFORMED','source':os.path.relpath(SOURCE,OUT),'source_hashes':preserved,'before_bounds_m':before_bounds,'after_bounds_m':after_bounds,'before_size_xyz_m':old_size,'after_size_xyz_m':new_size,'interfaces_before':before_interfaces,'interfaces_after':after_interfaces,'unchanged_mounted_head_legs_weapons':True,'before_central_armor_front_y_m':before_front,'after_central_armor_front_y_m':-.0905,'side_armor_front_y_range_m':[-.060,-.034],'notes':['The shared shoulder axes are fixed; receding side armor and a forward central chest make the fixed shoulder axes read as outboard and behind the chest.','Depth is redistributed within the existing outer envelope; the body is not uniformly scaled.','This is static design/interface review, not a new Unity joint/collision test.']}
json.dump(report,open(os.path.join(OUT,'Validation.json'),'w'),indent=2)
print('C01_DEPTH_REVIEW_PASS',old_size,new_size,flush=True)
