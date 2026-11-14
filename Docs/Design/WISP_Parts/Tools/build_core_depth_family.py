"""C-01 only: redistribute armor depth within its existing replacement envelope."""
import bpy,bmesh,os,sys,json,hashlib,shutil,math
from mathutils import Vector,Matrix
HERE=os.path.dirname(os.path.abspath(__file__));sys.path.insert(0,HERE)
import modeling_common as M
ROOT=os.path.dirname(HERE)
PART=sys.argv[sys.argv.index('--part')+1]
assert PART in ['C-02_LIGHTFRAME','C-03_BULWARK_CORE','C-04_ARC_CORE']
SOURCE=os.path.join(ROOT,'HeadCore-20261112',PART)
OUT=os.path.join(ROOT,'CoreDepth-Family-20261112',PART);os.makedirs(OUT,exist_ok=True)
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
    shutil.copy2(os.path.join(SOURCE,PART+'-'+suffix+'.png'),os.path.join(OUT,'Before-'+suffix+'.png'))
bpy.ops.wm.open_mainfile(filepath=os.path.join(SOURCE,PART+'.blend'))
scene=bpy.context.scene;COL=bpy.data.collections[PART+'_MODULE'];M.COL=COL;root=bpy.data.objects[PART+'_ROOT']
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
before_front=before_bounds[0][1]
keep=['Neck_Head_Interface','Waist_Pelvis_Interface','Shoulder_Can','Shoulder_Cap_Inset','UpperArm_Connector','Common_Back_Mount','Shoulder_Main_Angular_Cap','Light_Swept_Underside','Light_Orange_Tip','Heavy_Overlapping','Heavy_Outer_Bumper','Heavy_Orange_Outer','Arc_Separated_Undershoulder','Arc_Recessed_Shoulder_Cyan','Shoulder_Recessed_Bolt','Torso_To_Shoulder_Rotary_Bridge','Arc_Rear_Open_Yoke']
for o in list(COL.objects):
    if o.type=='MESH' and not any(tag in o.name for tag in keep):bpy.data.objects.remove(o,do_unlink=True)

def loft(n,levels,mat):
    loop=[(-.7,-1),(.7,-1),(1,-.7),(1,.7),(.7,1),(-.7,1),(-1,.7),(-1,-.7)]
    v=[(x*rx,y*ry,z) for z,rx,ry in levels for x,y in loop];f=[tuple(range(7,-1,-1))]
    for k in range(len(levels)-1):
        for i in range(8):j=(i+1)%8;f.append((k*8+i,k*8+j,(k+1)*8+j,(k+1)*8+i))
    f.append(tuple(range((len(levels)-1)*8,len(levels)*8)));return mesh(n,v,f,mat)

# Build a connected chest-to-waist volume, not a forward applique on a box.
# Each ring shares edges with the next, so side and back follow the same taper.
def body_rings(n,levels,mat,armored=False):
    v=[]
    for z,w,front,rear in levels:
        v.extend([(-.70*w,front,z),(.70*w,front,z),(w,front+.022,z),(w,rear-.017,z),(.70*w,rear,z),(-.70*w,rear,z),(-w,rear-.017,z),(-w,front+.022,z)])
    f=[tuple(range(7,-1,-1))]
    for k in range(len(levels)-1):
        for i in range(8):j=(i+1)%8;f.append((k*8+i,k*8+j,(k+1)*8+j,(k+1)*8+i))
    f.append(tuple(range((len(levels)-1)*8,len(levels)*8)))
    o=mesh(n,v,f,mat,.0007)
    if armored:
        o.data.materials.append(SHADE);o.data.materials.append(DARK)
        for k in range(len(levels)-1):
            for i in range(8):o.data.polygons[1+k*8+i].material_index=0 if i in [0,1,7] else (1 if i in [2,6] else 2)
    return o
def plate3(n,front,depth,mat):
    count=len(front);v=[(x,y+depth,z) for x,y,z in front]+front
    f=[tuple(range(count-1,-1,-1)),tuple(range(count,2*count))]
    for i in range(count):j=(i+1)%count;f.append((i,j,count+j,count+i))
    return mesh(n,v,f,mat,.0007)

def spear_rings(levels):
    v=[]
    for z,w,front,rear in levels:
        v.extend([(0,front-.008,z),(.68*w,front+.014,z),(w,front+.035,z),(w,rear-.017,z),(.65*w,rear,z),(-.65*w,rear,z),(-w,rear-.017,z),(-w,front+.035,z),(-.68*w,front+.014,z)])
    f=[tuple(range(8,-1,-1))]
    for k in range(len(levels)-1):
        for i in range(9):j=(i+1)%9;f.append((k*9+i,k*9+j,(k+1)*9+j,(k+1)*9+i))
    f.append(tuple(range((len(levels)-1)*9,len(levels)*9)))
    return mesh('Core_Light_Continuous_Spear_Shell',v,f,ARMOR,.0007)

if PART=='C-02_LIGHTFRAME':
    body_rings('Core_Light_Internal_Frame',[(-.049,.040,-.035,.036),(-.018,.064,-.045,.044),(.026,.074,-.051,.050),(.056,.050,-.026,.033),(.066,.029,-.023,.027)],DARK)
    spear_rings([(-.049,.032,-.047,.035),(-.027,.061,-.065,.046),(.020,.082,-.073,.060),(.047,.059,-.049,.044),(.053,.047,-.032,.035)])
    for sign in [-1,1]:
        plate3('Core_Light_Receding_Flank_'+str(sign),[(sign*x,y,z) for x,y,z in [(.054,-.041,.032),(.079,-.034,.016),(.058,-.034,-.025),(.039,-.042,-.040)]],.013,SHADE)
elif PART=='C-03_BULWARK_CORE':
    body_rings('Core_Heavy_Internal_Frame',[(-.049,.059,-.043,.048),(-.013,.092,-.061,.064),(.032,.108,-.070,.072),(.057,.085,-.044,.050),(.066,.028,-.022,.027)],DARK)
    body_rings('Core_Heavy_Wrapped_UnderArmor',[(-.049,.067,-.065,.065),(-.022,.103,-.086,.080),(.026,.116,-.092,.083),(.049,.098,-.069,.068),(.055,.077,-.045,.044)],SHADE,True)
    plate3('Core_Heavy_Upper_Overlapping_Breastplate',[(-.071,-.086,.049),(.071,-.086,.049),(.097,-.100,.022),(.075,-.105,-.007),(-.075,-.105,-.007),(-.097,-.100,.022)],.017,ARMOR)
    plate3('Core_Heavy_Lower_Overlapping_Lip',[(-.080,-.093,-.013),(.080,-.093,-.013),(.068,-.094,-.033),(.044,-.083,-.049),(-.044,-.083,-.049),(-.068,-.094,-.033)],.014,ARMOR)
else:
    GLOW=bpy.data.materials['Cyan_Optics']
    # An actual open central bay: no chest face crosses the reactor aperture.
    body_rings('Core_Arc_Dorsal_Housing',[(-.049,.057,-.020,.060),(-.018,.079,-.024,.076),(.032,.088,-.026,.077),(.052,.069,-.023,.054),(.066,.028,-.021,.027)],DARK)
    for sign in [-1,1]:
        plate3('Core_Arc_Wrapped_Reactor_Cheek_'+str(sign),[(sign*x,-.082+.35*(x-.032),z) for x,y,z in [(.032,-.062,.030),(.073,-.053,.041),(.101,-.034,.020),(.089,-.044,-.027),(.054,-.063,-.048),(.032,-.074,-.032)]],.091,ARMOR)
        plate3('Core_Arc_Inset_Lower_Facet_'+str(sign),[(sign*x,-.0835+.35*(x-.032),z) for x,y,z in [(.043,-.073,-.012),(.084,-.057,-.008),(.067,-.061,-.037),(.046,-.073,-.045)]],.006,SHADE)
    plate3('Core_Arc_Upper_Enclosing_Bridge',[(-.036,-.048,.050),(.036,-.048,.050),(.036,-.078,.031),(.029,-.079,.025),(-.029,-.079,.025),(-.036,-.078,.031)],.055,ARMOR)
    outer=[(-.024,.025),(.024,.025),(.033,.017),(.033,-.030),(.023,-.040),(-.023,-.040),(-.033,-.030),(-.033,.017)]
    inner=[(x*.76,-.007+(z+.007)*.80) for x,z in outer]
    v=[(x,y,z) for y in [-.085,-.066] for ring in [outer,inner] for x,z in ring];f=[]
    for i in range(8):
        j=(i+1)%8;f.extend([(i,j,8+j,8+i),(16+i,24+i,24+j,16+j),(i,16+i,16+j,j),(8+i,8+j,24+j,24+i)])
    mesh('Core_Arc_Open_Reactor_Rim',v,f,METAL,.0008)
    plate3('Core_Arc_Deep_Reactor_Well',[(x,-.057,z) for x,z in outer],.010,DARK)
    for x in [-.016,-.008,0,.008,.016]:
        plate3('Core_Arc_Recessed_Glow_Rod_'+str(x),[(x-.002,-.080,.015),(x+.002,-.080,.015),(x+.002,-.080,-.030),(x-.002,-.080,-.030)],.004,GLOW)

# Exposed structural abdomen: gear housing, crossmembers and twin load rails.
# These members carry the chest to the waist rather than decorating a smooth shell.
def block(n,x0,x1,y0,y1,z0,z1,mat):
    return mesh(n,[(x,y,z) for z in [z0,z1] for y in [y0,y1] for x in [x0,x1]],[(0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3)],mat,.001)
block('Core_Frame_Upper_Load_Crossmember',-.057,.057,-.049,.052,-.066,-.058,METAL)
block('Core_Frame_Central_Actuator_Housing',-.024,.024,-.035,.038,-.116,-.066,DARK)
block('Core_Frame_Lower_Waist_Crossmember',-.030,.030,-.028,.028,-.126,-.115,METAL)
for sign in [-1,1]:
    outline=[(sign*x,z) for x,z in [(.043,-.065),(.057,-.065),(.041,-.098),(.030,-.117),(.020,-.117),(.030,-.095)]]
    verts=[(x,y,z) for y in [-.041,.043] for x,z in outline]
    faces=[tuple(range(5,-1,-1)),tuple(range(6,12))]
    for i in range(6):j=(i+1)%6;faces.append((i,j,6+j,6+i))
    mesh('Core_Frame_Structural_Side_Rail_'+str(sign),verts,faces,SHADE,.001)
# A recessed mechanical access cover is contained in the gearbox envelope.
block('Core_Frame_Actuator_Front_Recess',-.017,.017,-.0365,-.035,-.099,-.075,METAL)
block('Core_Frame_Recessed_Dark_Center',-.012,.012,-.037,-.0365,-.094,-.079,DARK)

# Compact dorsal bridge supports the unchanged standard backpack latch.
body_rings('C01_WholeForm_Rear_Mount_Bridge',[(-.033,.041,.040,.077),(.028,.050,.043,.080),(.051,.040,.039,.067)],DARK)

if PART=='C-04_ARC_CORE':
    for sign in [-1,1]:block('Core_Arc_Rear_Yoke_Seat_'+str(sign),sign*.061-.006,sign*.061+.006,.036,.057,.046,.076,DARK)
block('Core_Fixed_Neck_Support',-.026,.026,-.024,.027,.048,.067,DARK)

bpy.context.view_layer.update();after_interfaces=interface_state();assert before_interfaces==after_interfaces,'An interface moved'
after_bounds=bounds([o for o in COL.objects if o.type=='MESH'])
old_size=[b-a for a,b in zip(*before_bounds)];new_size=[b-a for a,b in zip(*after_bounds)]
assert abs(new_size[0]-old_size[0])<.00001 and abs(new_size[2]-old_size[2])<.00001
assert new_size[1]<=old_size[1]+.00001,(old_size,new_size)
assert after_bounds[0][1]>=before_bounds[0][1]-.00001

scene.render.resolution_x=900;scene.render.resolution_y=900
for suffix,cam in [('01-Front','Camera_01-Front'),('02-Side','Camera_02-Right'),('03-45deg','Camera_05-45deg')]:
    scene.camera=bpy.data.objects[cam];scene.render.filepath=os.path.join(OUT,PART+'-'+suffix+'.png');bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['Camera_05-45deg'];candidate=os.path.join(OUT,PART+'-Depth-v1.blend');bpy.ops.wm.save_as_mainfile(filepath=candidate)

# Reuse the actual old mounted scene: replace its core collection only.
bpy.ops.wm.open_mainfile(filepath=os.path.join(SOURCE,PART+'-Mounted.blend'))
old_root=bpy.data.objects[PART+'_ROOT'];body_at=old_root.location.copy();old_col=bpy.data.collections[PART+'_MODULE']
other_roots=['HD01_ROOT','LG01_PAIR_ROOT','W-01_LUMEN_ROOT','W-01_LUMEN_LEFT_ROOT']
unchanged_others={n:[list(row) for row in bpy.data.objects[n].matrix_world] for n in other_roots}
for o in list(old_col.all_objects):bpy.data.objects.remove(o,do_unlink=True)
bpy.data.collections.remove(old_col)
with bpy.data.libraries.load(candidate,link=False) as (src,dst):dst.collections=[PART+'_MODULE']
col=dst.collections[0];bpy.context.scene.collection.children.link(col);new_root=bpy.data.objects[PART+'_ROOT'];new_root.location=body_at;bpy.context.view_layer.update()
for side,sign in [('L',-1),('R',1)]:
    pivot=body_at+Vector((sign*.14994,0,.02016));rot=Matrix.Rotation(math.radians(-sign*30),4,'Y')
    for o in col.objects:
        if 'UpperArm_Connector' in o.name and side+'_Upper' in o.name:o.matrix_world=Matrix.Translation(pivot)@rot@Matrix.Translation(-pivot)@o.matrix_world
bpy.context.view_layer.update()
assert unchanged_others=={n:[list(row) for row in bpy.data.objects[n].matrix_world] for n in other_roots}
scene=bpy.context.scene;scene.render.filepath=os.path.join(OUT,PART+'-04-Mounted-45deg.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,PART+'-Depth-v1-Mounted.blend'))
for p,h in preserved.items():assert sha(p)==h,('Preserved source changed',p)
report={'candidate':PART+' Depth Revision','scope':'One torso only; shared interfaces and approved C01 mechanical waist unchanged','blender':bpy.app.version_string,'automation':'PASS','design_review':'PENDING_PD','unity_integration':'NOT_PERFORMED','source':os.path.relpath(SOURCE,OUT),'source_hashes':preserved,'before_bounds_m':before_bounds,'after_bounds_m':after_bounds,'before_size_xyz_m':old_size,'after_size_xyz_m':new_size,'interfaces_before':before_interfaces,'interfaces_after':after_interfaces,'unchanged_mounted_head_legs_weapons':True,'before_central_armor_front_y_m':before_front,'after_central_armor_front_y_m':{'C-02_LIGHTFRAME':-.081,'C-03_BULWARK_CORE':-.105,'C-04_ARC_CORE':-.085}[PART],'side_armor_front_y_range_m':None,'armor_separation_z_m':[-.058,-.049],'mechanical_abdomen_max_front_y_m':-.049,'mechanical_abdomen_max_back_y_m':.052,'back_y_at_chest_m':{'C-02_LIGHTFRAME':.060,'C-03_BULWARK_CORE':.083,'C-04_ARC_CORE':.077}[PART],'chest_scale_xyz':None,'chest_scale_z_anchor_m':-.049,'waist_geometry_basis':'approved C01 v8, identical construction','notes':['Mechanical abdomen replaces the smooth armor housing; upper crossmember depth stays within v6 envelope.','Original shoulder shells, short upper arms and shared axes are preserved; the chest shell and back volume are rebuilt.','Depth is redistributed within the existing outer envelope; the body is not uniformly scaled.','This is static design/interface review, not a new Unity joint/collision test.']}
json.dump(report,open(os.path.join(OUT,'Validation.json'),'w'),indent=2)
print('C01_DEPTH_REVIEW_PASS',old_size,new_size,flush=True)
