"""Four design-only biped modules with the original waist socket and joint hierarchy."""
import bpy,bmesh,math,sys,json,hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.kdtree import KDTree
HERE=Path(__file__).resolve().parent;sys.path.insert(0,str(HERE))
import modeling_common as M
BASE=HERE.parent;SOURCE=BASE.parent/'WISP_Base_v1/LG01/LG01.blend'
UPPER=BASE/'CoreDepth-ShoulderFit-20261112/C-01_STANDARD/C-01_STANDARD-ShoulderFit-Mounted.blend'
OUT=BASE/'Biped-Four-20261112';OUT.mkdir(exist_ok=True)
CONFIG=[
 dict(id='LB-01_DART',label='고속 경량형',hip=.073,thigh=.019,shin=.023,joint=.021,foot=.052,toe=.074,heel=.038,pelvis=.057),
 dict(id='LB-02_STRIDER',label='표준형 업데이트',hip=.092,thigh=.031,shin=.035,joint=.027,foot=.072,toe=.087,heel=.046,pelvis=.073),
 dict(id='LB-03_BULWARK',label='중장갑형',hip=.107,thigh=.043,shin=.049,joint=.034,foot=.095,toe=.099,heel=.057,pelvis=.086),
 dict(id='LB-04_COLOSSUS',label='초중량형',hip=.128,thigh=.059,shin=.066,joint=.041,foot=.124,toe=.113,heel=.068,pelvis=.104)]
frozen={str(SOURCE):hashlib.sha256(SOURCE.read_bytes()).hexdigest(),str(UPPER):hashlib.sha256(UPPER.read_bytes()).hexdigest()}

def parent(o,node):
    world=o.matrix_world.copy();o.parent=node;o.matrix_world=world;return o
def mesh(n,v,f,mat,node):
    data=bpy.data.meshes.new(n+'_Mesh');data.from_pydata(v,[],f);data.update()
    bm=bmesh.new();bm.from_mesh(data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(data);bm.free()
    o=bpy.data.objects.new(n,data);COL.objects.link(o);data.materials.append(mat);M.bevel(o,.0012,1);return parent(o,node)
def box(n,loc,size,mat,node):
    x,y,z=loc;a,b,c=[t/2 for t in size]
    return mesh(n,[(x+xx*a,y+yy*b,z+zz*c) for zz in [-1,1] for yy in [-1,1] for xx in [-1,1]],[(0,1,3,2),(4,6,7,5),(0,4,5,1),(2,3,7,6),(0,2,6,4),(1,5,7,3)],mat,node)
def loft(n,levels,cx,mat,node):
    loop=[(-.65,-1),(.65,-1),(1,-.64),(1,.64),(.65,1),(-.65,1),(-1,.64),(-1,-.64)]
    v=[(cx+x*w,y*d+yc,z) for z,w,d,yc in levels for x,y in loop];f=[tuple(range(7,-1,-1))]
    for k in range(len(levels)-1):
        for i in range(8):j=(i+1)%8;f.append((k*8+i,k*8+j,(k+1)*8+j,(k+1)*8+i))
    f.append(tuple(range((len(levels)-1)*8,len(levels)*8)));return mesh(n,v,f,mat,node)
def plate(n,outline,y,depth,mat,node):
    count=len(outline);v=[(x,yy,z) for yy in [y-depth/2,y+depth/2] for x,z in outline];f=[tuple(range(count-1,-1,-1)),tuple(range(count,2*count))]
    for i in range(count):j=(i+1)%count;f.append((i,j,count+j,count+i))
    return mesh(n,v,f,mat,node)
def cyl(n,p,r,depth,mat,node):
    o=M.cylinder_x(n,p,r,depth,mat,24);o.modifiers[0].width=.001;return parent(o,node)
def world_center(o):return o.matrix_world@(sum((Vector(v) for v in o.bound_box),Vector())/8)
def waist_signature():
    return {o.name:[list(o.matrix_world@v.co) for v in o.data.vertices] for o in COL.objects if o.type=='MESH' and 'Waist_Socket' in o.name}
def motion_check(side):
    hip=bpy.data.objects[PART+'_'+side+'_HIP_PITCH_X'];splay=bpy.data.objects[PART+'_'+side+'_HIP_ABDUCTION_Y'];knee=bpy.data.objects[PART+'_'+side+'_KNEE_PITCH_X'];ankle=bpy.data.objects[PART+'_'+side+'_ANKLE_PITCH_X']
    foot=bpy.data.objects[side+'_Foot_Main'];bpy.context.view_layer.update();neutral=world_center(foot).copy();k0=knee.matrix_world.translation.copy();a0=ankle.matrix_world.translation.copy()
    knee.rotation_euler.x=math.radians(30);bpy.context.view_layer.update();a1=ankle.matrix_world.translation.copy();assert a1.y>a0.y+.04 and (knee.matrix_world.translation-k0).length<1e-6
    knee.rotation_euler.x=0;bpy.context.view_layer.update()
    hip.rotation_euler.x=math.radians(-20);bpy.context.view_layer.update();assert (world_center(foot)-neutral).length>.08
    hip.rotation_euler.x=0;bpy.context.view_layer.update()
    sign=-1 if side=='L' else 1;splay.rotation_euler.y=math.radians(-sign*12);bpy.context.view_layer.update();assert sign*ankle.matrix_world.translation.x>sign*a0.x+.05
    splay.rotation_euler.y=0;bpy.context.view_layer.update()
    ankle.rotation_euler.x=math.radians(20);bpy.context.view_layer.update();assert (world_center(foot)-neutral).length>.006
    ankle.rotation_euler.x=0;bpy.context.view_layer.update();assert (world_center(foot)-neutral).length<1e-6
    return {'hip_pitch':True,'hip_abduction':True,'knee_human_bend':True,'ankle_pitch':True,'returned_to_rest':True}

catalog=[]
for index,c in enumerate(CONFIG):
    PART=c['id'];dest=OUT/PART;dest.mkdir(exist_ok=True)
    bpy.ops.wm.open_mainfile(filepath=str(SOURCE));COL=bpy.data.collections['LG01_LEG_PAIR'];COL.name=PART+'_MODULE';M.COL=COL
    waist_before=waist_signature();root=bpy.data.objects['LG01_PAIR_ROOT'];root.name=PART+'_ROOT'
    for o in list(COL.objects):
        if o.type=='MESH' and 'Waist_Socket' not in o.name:bpy.data.objects.remove(o,do_unlink=True)
        elif o.type=='EMPTY' and o!=root:o.name=o.name.replace('LG01_',PART+'_')
    ARMOR=M.material('Biped_Warm_Ivory',(.60,.58,.55),.28,.35);SHADE=M.material('Biped_Secondary_Armor',(.28,.30,.33),.45,.38);DARK=M.material('Biped_Graphite_Frame',(.030,.039,.050),.65,.34);METAL=M.material('Biped_Joint_Metal',(.13,.15,.18),.75,.29);ORANGE=M.material('Biped_Orange_Inlay',(.93,.28,.025),.30,.33)
    hipz=.455;kneez=.254;anklez=.066;p=c['pelvis'];h=c['hip'];t=c['thigh'];s=c['shin'];r=c['joint']
    loft('Pelvis_Compact_Core',[(.418,p*.40,.032,0),(.447,p*.86,.047,0),(.482,p,.049,0),(.495,.046,.035,0)],0,DARK,root)
    cyl('Pelvis_Continuous_Hip_Shaft',(0,0,hipz),r*.66,2*h,METAL,root)
    plate('Pelvis_Short_Front_Armor',[(-p*.68,.484),(p*.68,.484),(p*.61,.447),(.023,.420),(-.023,.420),(-p*.61,.447)],-.052,.010,SHADE,root)
    box('Pelvis_Orange_Identity',(0,-.059,.476),(.019,.002,.004),ORANGE,root)
    for side,sign in [('L',-1),('R',1)]:
        x=sign*h
        gimbal=bpy.data.objects[PART+'_'+side+'_HIP_ABDUCTION_Y'];gimbal.location.x=x
        hip=bpy.data.objects[PART+'_'+side+'_HIP_PITCH_X'];knee=bpy.data.objects[PART+'_'+side+'_KNEE_PITCH_X'];ankle=bpy.data.objects[PART+'_'+side+'_ANKLE_PITCH_X'];bpy.context.view_layer.update()
        # Exact centers inherited from the proven human-knee hierarchy.
        H=hip.matrix_world.translation.copy();K=knee.matrix_world.translation.copy();A=ankle.matrix_world.translation.copy()
        for name,pt,node,rr in [('Hip',H,hip,r),('Knee',K,knee,r*.90),('Ankle',A,ankle,r*.78)]:
            depth=t*1.65 if name=='Hip' else s*1.65
            cyl(side+'_'+name+'_Functional_Can',pt,rr,depth,DARK,node)
            cyl(side+'_'+name+'_Outer_Rim',(pt.x+sign*(depth/2+.002),pt.y,pt.z),rr*.82,.005,METAL,node)
            cyl(side+'_'+name+'_Recessed_Center',(pt.x+sign*(depth/2+.005),pt.y,pt.z),rr*.51,.002,DARK,node)
        beam=.012+index*.005
        box(side+'_Thigh_Load_Beam',(x,-.002,.354),(beam*1.7,beam*1.6,.196),DARK,hip)
        box(side+'_Shin_Load_Beam',(x,-.004,.160),(beam*1.45,beam*1.5,.180),DARK,knee)
        # Distinct cuts and volumes, rather than simply enlarging a shared mesh.
        if index==0:
            thigh=[(.303,t*.60,.017,-.003),(.338,t,.022,-.005),(.411,t*.86,.018,-.003),(.424,t*.62,.015,0)]
            shin=[(.100,s*.50,.019,-.004),(.132,s*.74,.025,-.006),(.207,s,.025,-.009),(.227,s*.54,.019,-.005)]
        elif index==1:
            thigh=[(.297,t*.72,.025,-.002),(.325,t,.034,-.003),(.402,t,.032,-.002),(.425,t*.73,.025,0)]
            shin=[(.100,s*.61,.026,-.004),(.135,s*.88,.034,-.005),(.208,s,.036,-.007),(.228,s*.65,.025,-.004)]
        elif index==2:
            thigh=[(.295,t*.70,.033,-.002),(.321,t,.043,-.005),(.398,t,.046,-.003),(.427,t*.80,.031,0)]
            shin=[(.095,s*.72,.034,-.003),(.123,s,.049,-.005),(.204,s,.051,-.007),(.229,s*.77,.034,-.003)]
        else:
            thigh=[(.292,t*.78,.044,-.001),(.321,t,.063,-.007),(.398,t,.060,-.005),(.428,t*.82,.041,0)]
            shin=[(.092,s*.80,.047,-.004),(.116,s,.069,-.010),(.201,s,.070,-.009),(.231,s*.84,.045,-.003)]
        loft(side+'_Thigh_Angular_Armor',thigh,x,ARMOR,hip)
        loft(side+'_Shin_Angular_Armor',shin,x,ARMOR,knee)
        # Inset front facets use broad planes, not protruding decorative boxes.
        if index>0:
            td=thigh[1][2];sd=shin[1][2]
            plate(side+'_Thigh_Overlapping_Front',[(x-t*.60,.400),(x+t*.60,.400),(x+t*.66,.346),(x+t*.39,.316),(x-t*.39,.316),(x-t*.66,.346)],-td-.006,.008,SHADE if index==3 else ARMOR,hip)
            plate(side+'_Shin_Lower_Return',[(x-s*.68,.172),(x+s*.68,.172),(x+s*.65,.118),(x+s*.37,.101),(x-s*.37,.101),(x-s*.65,.118)],-sd-.009,.009,SHADE,knee)
        if index>=2:
            # Heavy shin and hip plates are split, leaving the joint circles clear.
            plate(side+'_Heavy_Upper_Shin_Lap',[(x-s*.83,.220),(x+s*.83,.220),(x+s*.88,.185),(x-s*.88,.185)],-shin[2][2]-.010,.016,ARMOR,knee)
        skirtw=t*(.95 if index<2 else 1.15)
        plate(side+'_Hip_Outer_Skirt',[(x-skirtw,.485),(x+skirtw,.485),(x+skirtw*1.05,.450),(x+skirtw*.64,.429),(x-skirtw*.64,.429),(x-skirtw*1.05,.450)],-.038-index*.009,.022+index*.006,ARMOR,root)
        # The knee guard follows the shin pivot; the cylindrical hinge stays visible in side view.
        kw=r*(.70 if index==0 else 1.0)
        plate(side+'_Knee_Guard',[(x-kw*.7,.273),(x+kw*.7,.273),(x+kw,.254),(x+kw*.6,.235),(x-kw*.6,.235),(x-kw,.254)],-.004-r*.87,.008+index*.003,SHADE if index==0 else ARMOR,knee)
        box(side+'_Thigh_Orange_Inlay',(x+sign*t*.36,-thigh[1][2]-.008,.383),(t*.45,.002,.006),ORANGE,hip)
        # Compact articulated feet; heel remains behind the ankle support line.
        toe=c['toe'];heel=c['heel'];fw=c['foot']
        profile=[(-toe,.008),(-toe,.027),(-.026,.060),(.022,.057),(heel,.028),(heel,.008)]
        foot=M.prism_yz(side+'_Foot_Main',profile,x,fw,ARMOR,.0015);parent(foot,ankle)
        sole=M.prism_yz(side+'_Foot_Sole',[(-toe,.001),(-toe,.010),(heel,.010),(heel,.001)],x,fw*1.01,DARK,.001);parent(sole,ankle)
        box(side+'_Foot_Toe_Bumper',(x,-toe+.003,.021),(fw*.70,.006,.020),SHADE,ankle)
        box(side+'_Foot_Orange_Inlay',(x,-toe+.022,.039),(fw*.40,.007,.003),ORANGE,ankle)
    bpy.context.view_layer.update();assert waist_signature()==waist_before
    checks={side:motion_check(side) for side in ['L','R']}
    # All rest meshes are true mirrored pairs, including their parenting transforms.
    meshpoints=[]
    for o in COL.objects:
        if o.type!='MESH':continue
        assert not o.data.validate(verbose=False),o.name
        for v in o.data.vertices:meshpoints.append(o.matrix_world@v.co)
    tree=KDTree(len(meshpoints))
    for j,p in enumerate(meshpoints):tree.insert(p,j)
    tree.balance();mirror_error=max(tree.find(Vector((-p.x,p.y,p.z)))[2] for p in meshpoints)
    assert mirror_error<.000002,('Rest geometry lost symmetry',mirror_error)
    scene=bpy.context.scene;scene.render.resolution_x=900;scene.render.resolution_y=900
    for label,cam in [('01-Front','Camera_LG01-01-Front'),('02-Right','Camera_LG01-02-Right'),('03-Back','Camera_LG01-03-Back'),('04-Left','Camera_LG01-04-Left'),('05-45deg','Camera_LG01-05-45deg')]:
        scene.camera=bpy.data.objects[cam];scene.render.filepath=str(dest/(PART+'-'+label+'.png'));bpy.ops.render.render(write_still=True)
    # A simple bend pose proves the modules are parented at the actual joints.
    for side in ['L','R']:
        bpy.data.objects[PART+'_'+side+'_HIP_PITCH_X'].rotation_euler.x=math.radians(-15)
        bpy.data.objects[PART+'_'+side+'_KNEE_PITCH_X'].rotation_euler.x=math.radians(35)
        bpy.data.objects[PART+'_'+side+'_ANKLE_PITCH_X'].rotation_euler.x=math.radians(-20)
    bpy.context.view_layer.update();scene.render.filepath=str(dest/(PART+'-06-Bend-Pose.png'));bpy.ops.render.render(write_still=True)
    for side in ['L','R']:
        for tag in ['HIP_PITCH_X','KNEE_PITCH_X','ANKLE_PITCH_X']:bpy.data.objects[PART+'_'+side+'_'+tag].rotation_euler.x=0
    bpy.context.view_layer.update();candidate=dest/(PART+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(candidate))
    # Use the latest shoulder-fitted C01 without changing upper body/head/weapons.
    bpy.ops.wm.open_mainfile(filepath=str(UPPER));old=bpy.data.collections['LG01_LEG_PAIR']
    other={o.name:[list(row) for row in o.matrix_world] for o in bpy.data.objects if o.type=='EMPTY' and o.name not in old.all_objects}
    for o in list(old.all_objects):bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.collections.remove(old)
    with bpy.data.libraries.load(str(candidate),link=False) as (a,b):b.collections=[PART+'_MODULE']
    bpy.context.scene.collection.children.link(b.collections[0]);bpy.context.view_layer.update()
    assert other=={o.name:[list(row) for row in o.matrix_world] for o in bpy.data.objects if o.name in other}
    scene=bpy.context.scene;scene.render.filepath=str(dest/(PART+'-07-Mounted-45deg.png'));bpy.ops.render.render(write_still=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(dest/(PART+'-Mounted.blend')))
    record={'id':PART,'label_ko':c['label'],'blender':bpy.app.version_string,'design_assumptions':['No new orthographic concept was supplied. These are silhouette proposals using the approved WISP material language.','Fast/heavy labels express visual intent; no gameplay speed, weight or balance values changed.'],'waist_mating_center_m':[0,0,.505],'waist_socket_vertices_preserved':True,'hip_center_x_abs_m':h,'hip_knee_ankle_z_m':[.455,.254,.066],'rig_motion_checks':checks,'symmetric_rest_geometry':True,'upper_body_preserved':True,'automation':'PASS','design_review':'PENDING_PD','unity_integration':'NOT_PERFORMED','configuration':c}
    (dest/'manifest.json').write_text(json.dumps(record,indent=2),encoding='utf-8');catalog.append(record)
    print('BIPED_PART_PASS',PART,flush=True)
for p,h in frozen.items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h,p
(OUT/'PART_CATALOG.json').write_text(json.dumps({'parts':catalog,'originals_sha256':frozen},indent=2),encoding='utf-8')
print('BIPED_FOUR_PASS',len(catalog),flush=True)
