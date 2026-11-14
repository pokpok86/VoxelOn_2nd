"""Four design-only biped modules with the original waist socket and joint hierarchy."""
import bpy,bmesh,math,sys,json,hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.kdtree import KDTree
HERE=Path(__file__).resolve().parent;sys.path.insert(0,str(HERE))
import modeling_common as M
BASE=HERE.parent;SOURCE=BASE.parent/'WISP_Base_v1/LG01/LG01.blend'
UPPER=BASE/'CoreDepth-ShoulderFit-20261112/C-01_STANDARD/C-01_STANDARD-ShoulderFit-Mounted.blend'
OUT=BASE/'Biped-Four-Refined-20261112';OUT.mkdir(exist_ok=True)
CONFIG=[
 dict(id='LB-01_DART',label='1번 · 경량 노출 프레임',hip=.073,thigh=.031,shin=.023,joint=.021,foot=.046,toe=.084,heel=.034,pelvis=.057),
 dict(id='LB-02_STRIDER',label='2번 · 표준 기계식 종아리',hip=.092,thigh=.031,shin=.035,joint=.027,foot=.0612,toe=.074,heel=.039,pelvis=.073),
 dict(id='LB-03_BULWARK',label='3번 · 2번 기반 추가 장갑',hip=.092,thigh=.031,shin=.035,joint=.027,foot=.0612,toe=.074,heel=.039,pelvis=.073),
 dict(id='LB-04_COLOSSUS',label='4번 · 플레어 초중량',hip=.128,thigh=.059,shin=.066,joint=.041,foot=.136,toe=.123,heel=.070,pelvis=.104)]
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
def rounded_loft(n,levels,cx,mat,node,hollow=False):
    count=12;v=[]
    for inner in ([False,True] if hollow else [False]):
        for z,w,d,yc in levels:
            for i in range(count):
                angle=2*math.pi*i/count+math.pi/count
                v.append((cx+(w-(.005 if inner else 0))*math.cos(angle),yc+(d-(.005 if inner else 0))*math.sin(angle),z))
    f=[];N=len(levels)*count
    for k in range(len(levels)-1):
        for i in range(count):
            j=(i+1)%count;a=k*count+i;b=k*count+j;c=(k+1)*count+j;d=(k+1)*count+i;f.append((a,b,c,d))
            if hollow:f.append((N+d,N+c,N+b,N+a))
    if hollow:
        for offset in [0,(len(levels)-1)*count]:
            for i in range(count):j=(i+1)%count;f.append((offset+i,N+offset+i,N+offset+j,offset+j))
    else:f.extend([tuple(range(count-1,-1,-1)),tuple(range(N-count,N))])
    return mesh(n,v,f,mat,node)
def foot_sections(n,sections,cx,mat,node,sole=False):
    v=[];count=6
    for y,w,top in sections:
        low=.001 if sole else .009;cut=.002 if sole else .005
        v.extend([(cx-w,y,low),(cx-w,y,top-cut),(cx-.70*w,y,top),(cx+.70*w,y,top),(cx+w,y,top-cut),(cx+w,y,low)])
    f=[tuple(range(5,-1,-1))]
    for k in range(len(sections)-1):
        for i in range(count):j=(i+1)%count;f.append((k*count+i,k*count+j,(k+1)*count+j,(k+1)*count+i))
    f.append(tuple(range((len(sections)-1)*count,len(sections)*count)));return mesh(n,v,f,mat,node)
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

selected=sys.argv[sys.argv.index('--part')+1] if '--part' in sys.argv else None
catalog=[r for r in json.loads((OUT/'PART_CATALOG.json').read_text(encoding='utf-8'))['parts'] if r['id']!=selected] if selected and (OUT/'PART_CATALOG.json').exists() else []
for index,c in enumerate(CONFIG):
    if selected and c['id']!=selected:continue
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
        beam=.012+(1 if index==2 else index)*.005
        box(side+'_Thigh_Load_Beam',(x,-.002,.354),(beam*1.7,beam*1.6,.196),DARK,hip)
        box(side+'_Shin_Load_Beam',(x,-.004,.160),(beam*1.45,beam*1.5,.180),DARK,knee)
        # Distinct cuts and volumes, rather than simply enlarging a shared mesh.
        standard_thigh=[(.297,.031*.72,.025,-.002),(.325,.031,.034,-.003),(.402,.031,.032,-.002),(.425,.031*.73,.025,0)]
        standard_shin=[(.082,.035*.57,.023,.012),(.119,.035*.72,.030,.009),(.195,.035*1.08,.041,.003),(.229,.035*.68,.027,-.004)]
        if index==0:
            thigh=standard_thigh
            shin=[(.185,s*.69,.019,-.003),(.211,s,.026,-.005),(.229,s*.62,.019,-.004)]
        elif index in [1,2]:
            thigh=standard_thigh;shin=standard_shin
        else:
            thigh=[(.292,t*.78,.044,-.001),(.321,t,.063,-.007),(.398,t,.060,-.005),(.428,t*.82,.041,0)]
            shin=[(.082,s*1.22,.079,.008),(.103,s*1.20,.077,.008),(.150,s,.063,.006),(.200,s*.82,.049,.001),(.231,s*.62,.039,-.003)]
        if index==3:
            rounded_loft(side+'_Thigh_Angular_Armor',thigh,x,ARMOR,hip)
            rounded_loft(side+'_Shin_Angular_Armor',shin,x,ARMOR,knee,hollow=True)
        else:
            loft(side+'_Thigh_Angular_Armor',thigh,x,ARMOR,hip)
            loft(side+'_Shin_Angular_Armor',shin,x,ARMOR,knee)
        if index==0:
            for dx in [-.009,.009]:box(side+'_Exposed_Calf_Rail_'+str(dx),(x+dx,.006,.143),(.005,.016,.112),METAL,knee)
            box(side+'_Calf_Upper_Tie',(x,.006,.181),(.026,.022,.009),SHADE,knee)
            box(side+'_Calf_Lower_Tie',(x,.006,.099),(.026,.022,.009),SHADE,knee)
            box(side+'_Calf_Recessed_Piston',(x,-.005,.139),(.009,.009,.092),DARK,knee)
        if index==2:
            outer_t=M.prism_yz(side+'_Added_Outer_Thigh_Shell',[(-.043,.412),(.040,.412),(.046,.353),(.025,.311),(-.031,.316),(-.047,.366)],x+sign*.034,.014,ARMOR,.0012);parent(outer_t,hip)
            loft(side+'_Added_Long_Shin_Cowl',[(.093,.030,.035,.008),(.137,.044,.044,.006),(.204,.043,.044,.004),(.229,.032,.031,0)],x,ARMOR,knee)
            for dx in [-.034,.034]:
                wing=M.prism_yz(side+'_Added_Ankle_Facet_'+str(dx),[(-.047,.169),(.023,.151),(.034,.099),(-.067,.078),(-.040,.116)],x+dx,.009,ARMOR,.0012);parent(wing,knee)
            plate(side+'_Added_Shin_Central_Ridge',[(x-.018,.208),(x+.018,.208),(x+.023,.150),(x+.012,.099),(x-.012,.099),(x-.023,.150)],-.045,.009,SHADE,knee)
        # Inset front facets use broad planes, not protruding decorative boxes.
        if index>=0:
            td=thigh[1][2];sd=shin[1][2]
            plate(side+'_Thigh_Overlapping_Front',[(x-t*.60,.400),(x+t*.60,.400),(x+t*.66,.346),(x+t*.39,.316),(x-t*.39,.316),(x-t*.66,.346)],-td-.006,.008,SHADE if index==3 else ARMOR,hip)
        skirtw=t*(.95 if index<3 else 1.15)
        plate(side+'_Hip_Outer_Skirt',[(x-skirtw,.485),(x+skirtw,.485),(x+skirtw*1.05,.450),(x+skirtw*.64,.429),(x-skirtw*.64,.429),(x-skirtw*1.05,.450)],-.038-(1 if index==2 else index)*.009,.022+(1 if index==2 else index)*.006,ARMOR,root)
        # The knee guard follows the shin pivot; the cylindrical hinge stays visible in side view.
        kw=r*(.70 if index==0 else 1.0)
        plate(side+'_Knee_Guard',[(x-kw*.7,.273),(x+kw*.7,.273),(x+kw,.254),(x+kw*.6,.235),(x-kw*.6,.235),(x-kw,.254)],-.004-r*.87,.008+index*.003,SHADE if index==0 else ARMOR,knee)
        box(side+'_Thigh_Orange_Inlay',(x+sign*t*.36,-thigh[1][2]-.008,.383),(t*.45,.002,.006),ORANGE,hip)
        # Compact articulated feet; heel remains behind the ankle support line.
        toe=c['toe'];heel=c['heel'];fw=c['foot']
        if index==0:
            sections=[(-toe,.010,.015),(-toe*.68,fw*.40,.027),(-.018,fw*.50,.042),(heel,fw*.43,.031)]
        elif index in [1,2]:
            sections=[(-toe,fw*.32,.025),(-toe*.65,fw*.49,.035),(-.019,fw*.50,.052),(heel,fw*.43,.040)]
        else:
            sections=[(-toe,fw*.33,.026),(-toe*.72,fw*.48,.042),(-.015,fw*.50,.052),(heel,fw*.41,.043)]
        foot=foot_sections(side+'_Foot_Main',sections,x,ARMOR,ankle)
        foot_sections(side+'_Foot_Sole',[(yy,w*1.01,.009) for yy,w,zz in sections],x,DARK,ankle,sole=True)
        if index==0:box(side+'_Foot_Ankle_Stem',(x,-.004,.048),(.022,.024,.036),DARK,ankle)
        if index==2:plate(side+'_Added_Instep_Cap',[(x-.020,.049),(x+.020,.049),(x+.023,.028),(x-.023,.028)],-.039,.013,ARMOR,ankle)
        if index==3:rounded_loft(side+'_Foot_Instep_Saddle',[(.027,.045,.037,-.011),(.051,.031,.023,-.007)],x,SHADE,ankle)
        box(side+'_Foot_Orange_Inlay',(x,-toe*.72,.028 if index==0 else .036),(fw*.29,.004,.002),ORANGE,ankle)
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
    record={'id':PART,'label_ko':c['label'],'blender':bpy.app.version_string,'design_assumptions':['No new orthographic concept was supplied. These are silhouette proposals using the approved WISP material language.','Numbered PD directions with Hyaku Shiki, Zeta Gundam and GP02 lower-leg photo reference; WISP proportions and colors retained. No gameplay values changed.'],'waist_mating_center_m':[0,0,.505],'waist_socket_vertices_preserved':True,'hip_center_x_abs_m':h,'hip_knee_ankle_z_m':[.455,.254,.066],'rig_motion_checks':checks,'symmetric_rest_geometry':True,'upper_body_preserved':True,'automation':'PASS','design_review':'PENDING_PD','unity_integration':'NOT_PERFORMED','configuration':c}
    (dest/'manifest.json').write_text(json.dumps(record,indent=2),encoding='utf-8');catalog.append(record)
    print('BIPED_PART_PASS',PART,flush=True)
for p,h in frozen.items():assert hashlib.sha256(Path(p).read_bytes()).hexdigest()==h,p
catalog.sort(key=lambda r:r['id'])
(OUT/'PART_CATALOG.json').write_text(json.dumps({'parts':catalog,'originals_sha256':frozen},indent=2),encoding='utf-8')
print('BIPED_FOUR_PASS',len(catalog),flush=True)
