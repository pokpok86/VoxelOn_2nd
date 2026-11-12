"""Eight PD reference candidates. Preserve approved Blender sources and interfaces."""
import bpy,bmesh,math,os,sys,json,hashlib
from mathutils import Vector,Matrix
sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
import modeling_common as M
from assembly_layout import load_layout
from assembly_pose import pose_shoulders,set_forearm_angle,IDLE_DEGREES
from weapon_mount_style import style_weapon
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BASE=os.path.abspath(os.path.join(ROOT,'../WISP_Base_v1'))
OUT=os.path.join(ROOT,'HeadCore-20261112')
with open(os.path.join(ROOT,'APPROVED_BASELINE.json')) as f:baseline=json.load(f)
with open(os.path.join(OUT,'Reference-Manifest.json')) as f:items=json.load(f)
if '--part' in sys.argv:
    selected=sys.argv[sys.argv.index('--part')+1]
    if selected in ('H-05_NEEDLE','H-06_LANCET'):
        items=[{'id':selected,'sha256':None}];OUT=os.path.join(ROOT,'SlimHeads-20261112');os.makedirs(OUT,exist_ok=True)
    else:items=[x for x in items if x['id']==selected]
def sha(p):return hashlib.sha256(open(p,'rb').read()).hexdigest()
def preserve():
    for p,h in baseline['geometry_sha256'].items():assert sha(os.path.join(os.path.dirname(ROOT),p))==h,p
preserve()

def box(n,c,d,mat,ch=.001):
    bpy.ops.mesh.primitive_cube_add(size=1,location=c);o=M.link_object(bpy.context.object,n,mat);o.dimensions=d;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);return M.bevel(o,ch,1)
def mesh(n,v,f,mat,ch=.001):
    me=bpy.data.meshes.new(n+'_mesh');me.from_pydata(v,[],f);me.update();o=bpy.data.objects.new(n,me);M.COL.objects.link(o);o.data.materials.append(mat);return M.bevel(o,ch,1)
def plate(n,outline,y,d,mat):
    # Front armor polygon extruded in fore/aft; silhouette is authored explicitly.
    if n in ('Continuous_Chest_Main_Plate','Shoulder_Main_Angular_Cap'):
        k=len(outline);cx=sum(p[0] for p in outline)/k;cz=sum(p[1] for p in outline)/k
        inner=[(cx+(x-cx)*.72,cz+(z-cz)*.69) for x,z in outline]
        v=[(x,y+d/2,z) for x,z in outline]+[(x,y-d/2,z) for x,z in outline]+[(x,y-d/2-.008,z) for x,z in inner]
        f=[tuple(range(k-1,-1,-1)),tuple(range(2*k,3*k))]
        for i in range(k):j=(i+1)%k;f.extend([(i,j,k+j,k+i),(k+i,k+j,2*k+j,2*k+i)])
        o=mesh(n,v,f,mat,.0007)
        if n=='Continuous_Chest_Main_Plate':
            o.data.materials.append(ORANGE)
            for i in [1,7]:o.data.polygons[3+i*2].material_index=1
        return o
    v=[(x,yy,z) for yy in [y-d/2,y+d/2] for x,z in outline];k=len(outline);f=[tuple(range(k-1,-1,-1)),tuple(range(k,2*k))]
    for i in range(k):j=(i+1)%k;f.append((i,j,k+j,k+i))
    return mesh(n,v,f,mat,.001)
def loft(n,levels,mat,cx=0,cy=0):
    loop=[(-.70,-1),(.70,-1),(1,-.70),(1,.70),(.70,1),(-.70,1),(-1,.70),(-1,-.70)]
    v=[(cx+x*rx,cy+y*ry,z) for z,rx,ry in levels for x,y in loop];f=[tuple(range(7,-1,-1))]
    for s in range(len(levels)-1):
        for i in range(8):j=(i+1)%8;f.append((s*8+i,s*8+j,(s+1)*8+j,(s+1)*8+i))
    f.append(tuple(range((len(levels)-1)*8,len(levels)*8)));return mesh(n,v,f,mat,.001)
def cz(n,c,r,d,mat,sides=32):
    bpy.ops.mesh.primitive_cylinder_add(vertices=sides,radius=r,depth=d,location=c);return M.bevel(M.link_object(bpy.context.object,n,mat),.0007,1)
def vent(n,x,y,z,w,h,count=3):
    box(n+'_recess',(x,y,z),(w,.002,h),DARK,.0004)
    for k in range(count):box(n+'_inset_fin',(x,y-.001,z+(k-(count-1)/2)*h/(count+1)),(w*.82,.001,h*.09),METAL,.0002)
def bolt(n,x,y,z,r=.0017):M.cylinder_y(n,(x,y,z),r,.0012,METAL,12)
def visor(n,pts,y):
    plate(n+'_black_optic_well',[(x*1.09,z*1.04) for x,z in pts],y+.002,.004,DARK)
    return plate(n+'_one_piece_glass',pts,y-.001,.002,GLOW)
def head_connector():
    cz('Fixed_Neck_Interface',(0,0,-.0415),.027,.009,METAL,8)
    cz('Short_Neck_Seat',(0,0,-.030),.018,.014,DARK,16)
def cheeks(width=.050,lower=-.020,top=.020):
    for s in [-1,1]:
        plate('Faceted_Cheek',[(s*x,z) for x,z in [(width-.011,top),(width+.005,top-.008),(width+.010,lower+.009),(width-.001,lower),(width-.016,lower+.004)]],-.019,.043,ARMOR)
        plate('Flush_Cheek_Orange',[(s*x,z) for x,z in [(width-.006,top-.009),(width-.002,top-.010),(width+.002,lower+.009),(width-.002,lower+.007)]],-.042,.0015,ORANGE)
def head_base(w=.047,d=.040,h=.032):
    head_connector();loft('Sensor_Head_Inner_Core',[(-.024,w*.7,d*.7),(-.013,w,d),(h*.7,w,d), (h,w*.78,d*.8)],DARK,cy=.007)
def brow(width=.049,front=-.044,back=.029,top=.049):
    # Sloping tapered forehead, not a rounded block or a decorative cube.
    loop=[(-.72,-1),(.72,-1),(1,-.5),(1,.45),(.70,1),(-.70,1),(-1,.45),(-1,-.5)]
    sections=[(front,width*.74,.020,.034),(front+.014,width,.019,top),(back-.005,width*.88,.018,top),(back,width*.67,.020,top-.005)]
    v=[(x*w,y,(bottom+roof)/2+z*(roof-bottom)/2) for y,w,bottom,roof in sections for x,z in loop];f=[tuple(range(7,-1,-1))]
    for k in range(3):
        for i in range(8):j=(i+1)%8;f.append((k*8+i,k*8+j,(k+1)*8+j,(k+1)*8+i))
    f.append(tuple(range(24,32)));mesh('Continuous_Faceted_Brow',v,f,ARMOR,.0007)

def radome():
    head_base();brow();cheeks()
    visor('Trapezoid_Sensor',[(-.025,.010),(.025,.010),(.018,.001),(-.018,.001)],-.043)
    plate('Small_Chin',[(-.024,-.012),(.024,-.012),(.018,-.026),(-.018,-.026)],-.031,.024,METAL)
    for s in [-1,1]:
        box('Antenna_Base',(s*.055,.008,.010),(.010,.014,.025),METAL)
        loft('Short_Side_Antenna',[(.018,.003,.003),(.059,.0022,.0022)],ARMOR,cx=s*.055,cy=.009)
    M.cylinder_x('Radome_Pitch_Hinge',(0,.014,.064),.013,.040,METAL,24)
    box('Radome_Support_Yoke',(0,.012,.065),(.025,.023,.033),DARK)
    dish=[]
    dish.append(cz('Radome_Underside',(0,.008,.090),.053,.009,DARK,48))
    dish.append(cz('Radome_Ivory_Rim',(0,.008,.096),.055,.008,ARMOR,48))
    dish.append(cz('Radome_Continuous_Top',(0,.008,.101),.051,.004,ARMOR,48))
    for s in [-1,1]:dish.append(box('Radome_Orange_Inlay',(s*.032,-.017,.104),(.012,.018,.001),ORANGE,.0004))
    pivot=Vector((0,.008,.095));rot=Matrix.Rotation(math.radians(12),4,'X')
    for o in dish:o.matrix_world=Matrix.Translation(pivot)@rot@Matrix.Translation(-pivot)@o.matrix_world
def longsight():
    head_base(.044,.037,.034);brow(.046,-.037,.034,.048)
    M.loft_body('Optical_Barrel_Case',[(-.099,.025,.024,.006),(-.088,.031,.030,.006),(-.040,.029,.027,.006),(-.018,.025,.023,.006)],DARK,8)
    M.annular_ring_y('Octagonal_Lens_Hood',(0,-.100,.006),.033,.026,.014,ARMOR,8)
    M.annular_ring_y('Recessed_Optic_Rim',(0,-.103,.006),.026,.021,.004,METAL,24)
    M.cylinder_y('Deep_Blue_Lens',(0,-.102,.006),.021,.002,GLASS,40)
    M.cylinder_y('Single_Cyan_Lens_Core',(0,-.104,.006),.005,.001,GLOW,32)
    M.annular_ring_y('Optical_Concentric_Lens',(0,-.1035,.006),.018,.016,.001,GLASS,32)
    cheeks(.048,-.021,.015)
    plate('Lower_Optical_Chin',[(-.034,-.014),(.034,-.014),(.026,-.028),(-.022,-.028)],-.024,.041,ARMOR)
    for s in [-1,1]:
        box('Lens_Side_Status',(s*.030,-.067,.007),(.001,.008,.011),ORANGE,.0003)
    box('Auxiliary_Sensor_Case',(.043,-.039,-.002),(.022,.025,.032),DARK)
    M.cylinder_y('Auxiliary_Blue_Lens',(.043,-.053,.002),.0065,.002,GLOW,24)
    box('Auxiliary_Sensor_Bar',(.043,-.053,-.009),(.012,.001,.003),GLOW,.0003)
    o=loft('Rear_Slanted_Antenna',[(.025,.003,.004),(.095,.0023,.0028)],ARMOR,cx=.032,cy=.039);o.rotation_euler.x=math.radians(-15)
def swiftfin():
    head_base(.043,.036,.034);brow(.043,-.054,.032,.049);cheeks(.044,-.022,.016)
    visor('Swept_Unbroken_Visor',[(-.030,.010),(0,.004),(.030,.010),(.025,.002),(0,-.005),(-.025,.002)],-.040)
    plate('Pointed_Face_Mask',[(-.017,-.005),(.017,-.005),(.009,-.025),(-.009,-.025)],-.034,.018,METAL)
    for s in [-1,1]:
        M.cylinder_x('Fin_Root_Ear',(s*.047,.005,.010),.018,.009,METAL,24)
        for k in range(2):
            y=.012+k*.015;z=.032-k*.018
            M.prism_yz('Long_Swept_Sensor_Fin',[(y,z),(y+.020,z+.022),(y+.098,z+.074-k*.013),(y+.078,z+.035-k*.010),(y+.032,z+.003)],s*(.027+k*.010),.0055,ARMOR,.0006)
            M.prism_yz('Fin_Orange_Tip',[(y+.098,z+.074-k*.013),(y+.087,z+.065-k*.013),(y+.080,z+.040-k*.010),(y+.088,z+.047-k*.010)],s*(.027+k*.010),.0058,ORANGE,.0001)
def bulwark_head():
    head_base(.054,.043,.043)
    brow(.059,-.051,.044,.060)
    loft('Raised_Crown_Keystone',[(.056,.019,.021),(.078,.017,.018)],SHADE,cy=.008)
    visor('Thin_Protected_Visor',[(-.030,.008),(.030,.008),(.028,.003),(-.028,.003)],-.048)
    for s in [-1,1]:
        plate('Heavy_Jaw_Guard',[(s*x,z) for x,z in [(.023,-.004),(.049,.003),(.055,-.005),(.049,-.032),(.019,-.025)]],-.029,.046,ARMOR)
        M.cylinder_x('Recessed_Ear_Drum',(s*.061,.010,-.003),.023,.012,METAL,24)
        box('Jaw_Embedded_Vent',(s*.033,-.054,-.012),(.006,.002,.020),DARK,.0005)
        box('Small_Ear_Orange',(s*.066,-.005,.002),(.0015,.008,.019),ORANGE,.0002)
        bolt('Brow_Fastener',s*.035,-.053,.035)
    plate('Central_Chin_Wedge',[(-.021,-.004),(.021,-.004),(.018,-.028),(-.018,-.028)],-.047,.014,SHADE)

def needle_head():
    # Long fore/aft and low profile, with a narrow single visor, not a telescope barrel.
    head_connector()
    M.loft_body('Long_Low_Inner_Sensor',[(-.080,.022,.012,.003),(-.057,.031,.022,.008),(.042,.030,.025,.012),(.066,.018,.017,.014)],DARK,8)
    brow(.034,-.086,.061,.043)
    visor('Single_Narrow_Visor',[(-.023,.014),(.023,.014),(.018,.009),(-.018,.009)],-.081)
    plate('Tapered_Low_Chin',[(-.023,.003),(.023,.003),(.014,-.017),(-.014,-.017)],-.059,.022,METAL)
    for sign in [-1,1]:
        M.prism_yz('Long_Swept_Cheek',[(-.066,.003),(-.030,.009),(.043,.016),(.054,-.003),(.013,-.013),(-.050,-.015)],sign*.032,.008,ARMOR,.0007)
        M.prism_yz('Flush_Cheek_Orange',[(-.021,.003),(-.013,.004),(.008,.000),(.005,-.004),(-.020,-.003)],sign*.0365,.001,ORANGE,.0001)
        vent('Rear_Side_Optical_Cooling',sign*.017,.059,.009,.015,.011)

def lancet_head():
    # Tall narrow face; neck itself remains the same short shared connector.
    head_connector()
    loft('Tall_Slender_Sensor_Core',[(-.022,.016,.021),(.015,.027,.028),(.068,.026,.028),(.084,.017,.020)],DARK,cy=.006)
    loft('Faceted_Tall_Forehead',[(.036,.028,.027),(.066,.031,.029),(.093,.018,.021)],ARMOR,cy=-.003)
    visor('Single_Thin_Visor',[(-.022,.031),(.022,.031),(.018,.025),(-.018,.025)],-.029)
    for sign in [-1,1]:
        plate('Tapered_Long_Face_Guard',[(sign*x,z) for x,z in [(.018,.024),(.031,.017),(.027,-.009),(.012,-.021),(.014,.002)]],-.019,.025,ARMOR)
        plate('Narrow_Orange_Cheek',[(sign*x,z) for x,z in [(.026,.014),(.029,.012),(.023,-.005),(.020,-.008)]],-.033,.0015,ORANGE)
        M.cylinder_x('Small_Flush_Ear',(sign*.031,.010,.036),.012,.005,METAL,24)
    plate('Long_Point_Face_Mask',[(-.012,.022),(.012,.022),(.008,-.013),(0,-.025),(-.008,-.013)],-.026,.014,SHADE)

def common_body_interfaces():
    # Append only approved interface and upper-arm geometry, with evaluated transforms baked.
    path=os.path.join(BASE,'BD01/BD01.blend')
    with bpy.data.libraries.load(path,link=False) as (src,dst):dst.collections=['BD01_BODY_AND_SHOULDERS']
    source=dst.collections[0];bpy.context.scene.collection.children.link(source);bpy.context.view_layer.update();deps=bpy.context.evaluated_depsgraph_get()
    keep=[]
    for old in list(source.all_objects):
        if old.type!='MESH':continue
        if not any(s in old.name for s in ('UpperArm_Connector','Shoulder_Can_X','Shoulder_Can_Face','Shoulder_Cap_Inset','Neck_Head_Interface','Waist_Pelvis_Interface_Key')):continue
        me=bpy.data.meshes.new_from_object(old.evaluated_get(deps));me.transform(old.matrix_world);o=bpy.data.objects.new(old.name+'_Compatible',me);M.COL.objects.link(o);keep.append(o)
    for old in list(source.all_objects):bpy.data.objects.remove(old,do_unlink=True)
    bpy.data.collections.remove(source);assert len(keep)>=10
    for s in [-1,1]:
        # Small flush rear latch retains the existing common backpack plane.
        box('Common_Back_Mount', (s*.030,.082,-.003),(.019,.012,.056),METAL)
def shoulder_shells(kind):
    # Each is a separately drawn outline, not a scaled copy of a shared cap.
    designs={
        'C-01_STANDARD':{'z':.027,'depth':.089,'outline':[(-.035,.038),(.012,.045),(.047,.023),(.043,-.031),(.008,-.032),(-.026,-.013)]},
        'C-02_LIGHTFRAME':{'z':.021,'depth':.061,'outline':[(-.030,.027),(.006,.030),(.058,-.002),(.038,-.029),(.010,-.016),(-.025,-.021)]},
        'C-03_BULWARK_CORE':{'z':.027,'depth':.127,'outline':[(-.047,.056),(.037,.061),(.079,.028),(.077,-.043),(.047,-.062),(-.018,-.047),(-.044,-.014)]},
        'C-04_ARC_CORE':{'z':.025,'depth':.078,'outline':[(-.034,-.020),(-.031,.045),(-.004,.065),(.034,.053),(.067,.016),(.059,-.020),(.032,-.004),(.009,-.036)]}
    }
    d=designs[kind]
    for sign in [-1,1]:
        cx=sign*(.165 if kind=='C-02_LIGHTFRAME' else .185);z=d['z'];outline=[(cx+sign*x,z+zz) for x,zz in d['outline']]
        plate('Shoulder_Main_Angular_Cap',outline,-.003,d['depth'],ARMOR)
        if kind=='C-02_LIGHTFRAME':
            plate('Light_Swept_Underside',[(cx+sign*x,z+zz) for x,zz in [(-.017,-.015),(.010,-.014),(.039,-.031),(.018,-.041),(-.015,-.029)]],.001,.043,SHADE)
            plate('Light_Orange_Tip',[(cx+sign*x,z+zz) for x,zz in [(.032,.010),(.053,-.003),(.044,-.015),(.029,-.004)]],-.035,.0015,ORANGE)
        elif kind=='C-03_BULWARK_CORE':
            plate('Heavy_Overlapping_Downward_Skirt',[(cx+sign*x,z+zz) for x,zz in [(-.026,-.019),(.059,-.024),(.060,-.057),(.025,-.078),(-.027,-.055)]],.004,.098,SHADE)
            plate('Heavy_Outer_Bumper',[(cx+sign*x,z+zz) for x,zz in [(.058,.019),(.077,.018),(.074,-.038),(.056,-.049),(.047,-.017)]],-.069,.007,ARMOR)
            plate('Heavy_Orange_Outer_Inlay',[(cx+sign*x,z+zz) for x,zz in [(.056,.039),(.073,.025),(.070,.003),(.053,.016)]],-.070,.0015,ORANGE)
        elif kind=='C-04_ARC_CORE':
            plate('Arc_Separated_Undershoulder',[(cx+sign*x,z+zz) for x,zz in [(-.026,-.020),(.005,-.022),(.024,-.047),(-.003,-.066),(-.029,-.044)]],.012,.050,SHADE)
            plate('Arc_Recessed_Shoulder_Cyan',[(cx+sign*x,z+zz) for x,zz in [(-.018,.034),(-.002,.046),(.014,.041),(-.005,.024)]],-.043,.0015,GLOW)
        else:
            plate('Standard_Lower_Return',[(cx+sign*x,z+zz) for x,zz in [(-.017,-.013),(.033,-.022),(.029,-.041),(-.008,-.039),(-.027,-.023)]],.004,.067,SHADE)
            plate('Standard_Orange_Corner',[(cx+sign*x,z+zz) for x,zz in [(.031,.023),(.043,.015),(.041,-.005),(.029,.004)]],-.050,.0015,ORANGE)
        for dx,dz in [(-.014,.022),(.022,.018)]:bolt('Shoulder_Recessed_Bolt',cx+sign*dx,-d['depth']/2-.006,z+dz)

def core(kind):
    common_body_interfaces();shoulder_shells(kind)
    w=.084 if kind=='C-02_LIGHTFRAME' else .128 if kind=='C-03_BULWARK_CORE' else .105 if kind=='C-04_ARC_CORE' else .110
    depth=.066 if kind=='C-03_BULWARK_CORE' else .043 if kind=='C-02_LIGHTFRAME' else .054
    loft('Compact_Continuous_Inner_Torso',[(-.087,.032 if kind=='C-02_LIGHTFRAME' else .049 if kind=='C-03_BULWARK_CORE' else .041,.036),(-.049,w*.66 if kind=='C-02_LIGHTFRAME' else w*.88 if kind=='C-03_BULWARK_CORE' else w*.77,depth*.85),(.026,w,depth),(.063,w*.79,depth*.85)],DARK)
    for s in [-1,1]:M.cylinder_x('Torso_To_Shoulder_Rotary_Bridge',(s*(w-.005+.155)/2,0,.02016),.021,.160-w,DARK,24)
    if kind=='C-04_ARC_CORE':
        plate('Arc_Upper_Bridge',[(-.091,.057),(.091,.057),(.098,.023),(.037,.015),(.025,.002),(-.025,.002),(-.037,.015),(-.098,.023)],-.059,.025,ARMOR)
        for s in [-1,1]:
            plate('Arc_Flared_Chest_Side',[(s*x,z) for x,z in [(.048,.045),(.091,.053),(.113,.015),(.100,-.037),(.052,-.026),(.036,-.002)]],-.052,.038,ARMOR)
            plate('Arc_Lower_Dark_Facet',[(s*x,z) for x,z in [(.048,.002),(.103,.012),(.105,-.034),(.061,-.052),(.044,-.027)]],-.077,.012,SHADE)
            visor('Small_Arc_Side_Sensor',[(s*.064,-.010),(s*.087,-.003),(s*.084,-.009),(s*.066,-.017)],-.085)
        # Open framed reactor bay, cyan rods sit inside the dark well.
        box('Reactor_Black_Well',(0,-.055,-.032),(.061,.023,.064),DARK)
        for x in [-.030,.030]:box('Reactor_Vertical_Frame',(x,-.074,-.032),(.007,.008,.065),METAL)
        for z in [-.066,.003]:box('Reactor_Horizontal_Frame',(0,-.074,z),(.056,.009,.007),METAL)
        for x in [-.020,-.010,0,.010,.020]:cz('Recessed_Arc_Cyan_Rod',(x,-.070,-.032),.0028,.047,GLOW,12)
        for s in [-1,1]:plate('Arc_Orange_Reactor_Strut',[(s*.044,-.039),(s*.052,-.042),(s*.043,-.077),(s*.034,-.076)],-.060,.008,ORANGE)
        vent('Reactor_Upper_Vent',0,-.078,.013,.039,.013)
        for sign in [-1,1]:
            M.prism_yz('Arc_Rear_Open_Yoke',[(.030,.068),(.045,.112),(.059,.118),(.068,.114),(.059,.075),(.052,.078),(.058,.108),(.050,.108),(.041,.068)],sign*.061,.008,METAL,.0007)
    else:
        outline=[(-w*.78,.059),(w*.78,.059),(w,.035),(w*.91,-.014),(w*.59,-.030),(-w*.59,-.030),(-w*.91,-.014),(-w,.035)]
        if kind=='C-02_LIGHTFRAME':outline=[(-w*.65,.055),(w*.65,.055),(w,.027),(w*.82,-.003),(.018,-.038),(-.018,-.038),(-w*.82,-.003),(-w,.027)]
        plate('Continuous_Chest_Main_Plate',outline,-.070 if kind=='C-03_BULWARK_CORE' else -.059,.037 if kind=='C-03_BULWARK_CORE' else .029,ARMOR)
        plate('Chest_Lower_Faceted_Return',[(-w*.88,.000),(-w*.59,-.030),(w*.59,-.030),(w*.88,.000),(w*.82,-.040),(.044,-.057),(-.044,-.057),(-w*.82,-.040)],-.051,.026,SHADE)
        for s in [-1,1]:
            if kind=='C-03_BULWARK_CORE':
                plate('Heavy_Breast_Side_Armor',[(s*x,z) for x,z in [(.068,.049),(.105,.044),(.122,.016),(.105,-.032),(.079,-.026),(.058,.006)]],-.098,.021,ARMOR)
                vent('Protected_Chest_Vent',s*.083,-.112,-.014,.018,.019)
        if kind=='C-02_LIGHTFRAME':
            plate('Shallow_Lightframe_Chest_Spear',[(-.080,.033),(.080,.033),(.064,.001),(0,-.028),(-.064,.001)],-.077,.012,ARMOR)
            for x in [-.013,0,.013]:box('Lightframe_Orange_Recess',(x,-.060,-.046),(.002,.001,.012),ORANGE,.0002)
        elif kind=='C-03_BULWARK_CORE':plate('Small_Chest_Keystone',[(-.039,.018),(.039,.018),(.033,-.006),(-.033,-.006)],-.101,.008,ARMOR)
        else:
            plate('Small_Central_Chest_Keystone',[(-.033,-.006),(.033,-.006),(.033,-.036),(-.033,-.036)],-.084,.010,SHADE)
            vent('Chest_Recessed_Duct',0,-.090,-.016,.043,.010)
            for x in [-.008,0,.008]:box('Three_Flush_Chest_Indicators',(x,-.083,.038),(.004,.0015,.006),ORANGE,.0003)
    # Short abdomen within the approved waist plane; never elongate the belly.
    loft('Short_Abdomen_Core',[(-.115,.029,.028),(-.077,.038,.032),(-.049,.046,.039)],DARK)
    a=.022 if kind=='C-02_LIGHTFRAME' else .054 if kind=='C-03_BULWARK_CORE' else .034
    plate('Lower_Abdomen_Armor',[(-a,-.058),(a,-.058),(a*.94,-.094),(a*.60,-.113),(-a*.60,-.113),(-a*.94,-.094)],-.045 if kind=='C-03_BULWARK_CORE' else -.034,.024 if kind=='C-03_BULWARK_CORE' else .018,SHADE)
    for s in [-1,1]:
        if kind=='C-02_LIGHTFRAME':outline=[(s*.025,-.067),(s*.039,-.061),(s*.042,-.084),(s*.028,-.099),(s*.024,-.096)]
        elif kind=='C-03_BULWARK_CORE':outline=[(s*.043,-.060),(s*.075,-.047),(s*.082,-.078),(s*.059,-.110),(s*.041,-.105)]
        elif kind=='C-04_ARC_CORE':outline=[(s*.035,-.063),(s*.074,-.044),(s*.086,-.071),(s*.059,-.110),(s*.033,-.098)]
        else:outline=[(s*.036,-.067),(s*.057,-.057),(s*.065,-.078),(s*.044,-.100),(s*.033,-.099)]
        plate('Lower_Side_Brace',outline,-.012,.056 if kind=='C-03_BULWARK_CORE' else .039 if kind=='C-02_LIGHTFRAME' else .049,ARMOR)
    vent('Top_Integrated_Cooling',0,-.016,.064,.047,.012)
    for s in [-1,1]:
        bolt('Chest_Fastener',s*.060,-.084 if kind=='C-01_STANDARD' else -.076,.044)
        vent('Rear_Inset_Cooling',s*.039,.055,.011,.032,.026)

def append(path,col):
    with bpy.data.libraries.load(path,link=False) as (src,dst):dst.collections=[col]
    for c in dst.collections:bpy.context.scene.collection.children.link(c)
    return dst.collections[0]
catalog=[]
for item in items:
    PART=item['id'];HEAD=PART.startswith('H-');dest=os.path.join(OUT,PART);os.makedirs(dest,exist_ok=True)
    M.COL=M.clear_scene();M.COL.name=PART+'_MODULE'
    ARMOR=M.material('Ivory_Armor',(.60,.58,.55),.28,.35);SHADE=M.material('Secondary_Armor',(.28,.30,.33),.45,.38);DARK=M.material('Graphite_Inner_Frame',(.030,.039,.050),.65,.34);METAL=M.material('Machined_Joint_Metal',(.13,.15,.18),.75,.29);ORANGE=M.material('Flush_Orange_Inlay',(.93,.28,.025),.30,.33);GLOW=M.material('Cyan_Optics',(.006,.62,.81),.15,.22,3.5);GLASS=M.material('Deep_Optical_Glass',(.008,.12,.17),.45,.15,.3)
    if HEAD:{'H-01_RADOME':radome,'H-02_LONGSIGHT':longsight,'H-03_SWIFTFIN':swiftfin,'H-04_BULWARK':bulwark_head,'H-05_NEEDLE':needle_head,'H-06_LANCET':lancet_head}[PART]()
    else:core(PART)
    if PART in ('H-05_NEEDLE','H-06_LANCET'):
        # Seat the new housing on the unchanged short neck; do not elongate the neck.
        for o in M.COL.objects:
            if o.type=='MESH' and o.name not in ('Fixed_Neck_Interface','Short_Neck_Seat'):
                o.location.z+=-.013 if PART=='H-05_NEEDLE' else -.002
        bpy.context.view_layer.update()
    root=bpy.data.objects.new(PART+'_ROOT',None);M.COL.objects.link(root)
    for o in list(M.COL.objects):
        if o.type!='MESH':continue
        bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
        o.name=PART+'_'+o.name;world=o.matrix_world.copy();o.parent=root;o.matrix_world=world
    bpy.context.view_layer.update();meshes=[o for o in M.COL.objects if o.type=='MESH'];points=[o.matrix_world@Vector(v) for o in meshes for v in o.bound_box];lo=Vector(tuple(min(p[i] for p in points) for i in range(3)));hi=Vector(tuple(max(p[i] for p in points) for i in range(3)));center=(lo+hi)/2
    scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.eevee.taa_render_samples=64;scene.render.resolution_x=900;scene.render.resolution_y=900;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
    studio=bpy.data.collections.new(PART+'_STUDIO');scene.collection.children.link(studio)
    world=bpy.data.worlds.new('Neutral_Review_World');world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.013,.018,.027,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.32;scene.world=world
    for name,loc,power,color in [('Key',(-1,-1.3,1.1),70,(1,.91,.82)),('Fill',(1,-.8,.5),45,(.75,.86,1)),('Rim',(.4,1,.9),65,(.65,.81,1))]:
        d=bpy.data.lights.new(name,'AREA');d.energy=power;d.size=1.1;d.color=color;o=bpy.data.objects.new(name,d);studio.objects.link(o);o.location=loc;o.rotation_euler=(center-o.location).to_track_quat('-Z','Y').to_euler()
    def camera(n,loc,target,scale):
        d=bpy.data.cameras.new(n);d.type='ORTHO';d.ortho_scale=scale;o=bpy.data.objects.new(n,d);studio.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();return o
    scale=max(hi-lo)*1.30 if HEAD else .70
    if not HEAD:center=Vector((0,0,-.024))
    for name,off in [('01-Front',(0,-2,0)),('02-Right',(2,0,0)),('03-Rear',(0,2,0)),('04-Left',(-2,0,0)),('05-45deg',(1.4,-2,.8))]:
        scene.camera=camera('Camera_'+name,center+Vector(off),center,scale);scene.render.filepath=os.path.join(dest,PART+'-'+name+'.png');bpy.ops.render.render(write_still=True)
    if not HEAD:
        silhouette=M.material('Review_Only_Black_Silhouette',(0,0,0),0,1)
        silhouette.node_tree.nodes['Principled BSDF'].inputs['Specular IOR Level'].default_value=0
        bpy.context.view_layer.material_override=silhouette
        background=world.node_tree.nodes['Background'];saved=tuple(background.inputs['Color'].default_value)
        background.inputs['Color'].default_value=(.82,.82,.82,1);background.inputs['Strength'].default_value=.8
        for suffix,cam in [('08-Silhouette-Front','Camera_01-Front'),('09-Silhouette-Side','Camera_02-Right')]:
            scene.camera=bpy.data.objects[cam];scene.render.filepath=os.path.join(dest,PART+'-'+suffix+'.png');bpy.ops.render.render(write_still=True)
        bpy.context.view_layer.material_override=None;background.inputs['Color'].default_value=saved;background.inputs['Strength'].default_value=.32
    target=Vector((0,0,-.035)) if HEAD else Vector((.166,0,-.066));scene.camera=camera('Camera_Interface',target+Vector((.5,-.6,.35)),target,.14 if HEAD else .25);scene.render.filepath=os.path.join(dest,PART+'-06-Interface.png');bpy.ops.render.render(write_still=True)
    scene.camera=bpy.data.objects['Camera_05-45deg'];bpy.ops.wm.save_as_mainfile(filepath=os.path.join(dest,PART+'.blend'))
    # Mounted preview uses the existing approved body/head and legs, never resizes them.
    layout=load_layout(BASE)
    if HEAD:
        append(os.path.join(BASE,'BD01/BD01.blend'),'BD01_BODY_AND_SHOULDERS');bpy.data.objects['BD01_ROOT'].location=layout['body_translation'];pose_shoulders(layout);root.location=layout['head_translation']
    else:
        root.location=layout['body_translation'];append(os.path.join(BASE,'HD01/HD01.blend'),'HD01_SENSOR_HEAD');bpy.data.objects['HD01_ROOT'].location=layout['head_translation']
    append(os.path.join(BASE,'LG01/LG01.blend'),'LG01_LEG_PAIR')
    bpy.context.view_layer.update()
    # Use real shoulder pivots to spread only the copied short upper arms.
    if not HEAD:
        for side,sign,key in [('L',-1,'left_elbow'),('R',1,'right_elbow')]:
            pivot=Vector((sign*.14994,0,.02016));rot=Matrix.Rotation(math.radians(-sign*30),4,'Y')
            for o in M.COL.objects:
                if 'UpperArm_Connector' in o.name and side+'_Upper' in o.name:
                    o.matrix_world=Matrix.Translation(Vector(layout['body_translation'])+pivot)@rot@Matrix.Translation(-Vector(layout['body_translation'])-pivot)@o.matrix_world
            p=Vector(layout[key]);layout[key]=list(Vector(layout['body_translation'])+pivot+rot.to_3x3()@(p-Vector(layout['body_translation'])-pivot))
    arms=append(os.path.join(ROOT,'W-01_LUMEN/W-01_LUMEN.blend'),'W01_LUMEN_MODULE');arm=bpy.data.objects['W-01_LUMEN_ROOT'];arm.location=layout['right_elbow'];arm.scale=(.42,)*3;set_forearm_angle(arm,IDLE_DEGREES)
    other=arm.copy();other.name='W-01_LUMEN_LEFT_ROOT';arms.objects.link(other);other.location=layout['left_elbow'];set_forearm_angle(other,IDLE_DEGREES,left=True)
    for o in [o for o in arms.objects if o.type=='MESH']:
        cp=o.copy();cp.data=o.data.copy();arms.objects.link(cp);cp.parent=other
    style_weapon(arm,arms,'W-01_LUMEN');style_weapon(other,arms,'W-01_LUMEN',left=True)
    scene.render.resolution_x=720;scene.render.resolution_y=960;scene.camera=camera('Camera_Mounted',(1.4,-2.1,1.15),(0,0,.48),1.70);scene.render.filepath=os.path.join(dest,PART+'-07-Mounted.png');bpy.ops.render.render(write_still=True);bpy.ops.wm.save_as_mainfile(filepath=os.path.join(dest,PART+'-Mounted.blend'))
    manifest={'id':PART,'category':'head' if HEAD else 'core','reference':'../References/'+PART+'.jpg','reference_sha256':item['sha256'],'blender':bpy.app.version_string,'mesh_count':len(meshes),'bounds_m':list(hi-lo),'neck_bottom_local_z':-.046 if HEAD else None,'body_interfaces':None if HEAD else json.load(open(os.path.join(BASE,'BD01/manifest.json')))['assembly_interfaces'],'silhouette_revision':'distinct body and shoulder contours, fixed shared camera scale 0.70m' if not HEAD else None,'short_upper_arms':'copied evaluated geometry from approved BD01; same elbow centers' if not HEAD else None,'automation':'PASS','design_review':'PENDING_PD','unity_integration':'NOT_PERFORMED','assumptions':['Rear surfaces not shown in concept are conservative closure panels using the common mount.','References are illustrative; features retain the approved neck/waist/shoulder/elbow positions.','No new gameplay values or articulation rules are inferred.']}
    if item['sha256'] is None:
        manifest['reference']='PD chat request: a slimmer elongated head; two directional proposals for comparison'
        manifest['assumptions']=['No dedicated concept image was supplied for these two proposals.','H-05 elongates fore/aft; H-06 elongates the face vertically. Direction choice is pending PD.','The neck connector and approved body/weapon/leg models stay unchanged.','This is a static Blender design proposal, not a new gameplay sensor specification.']
    with open(os.path.join(dest,'manifest.json'),'w') as f:json.dump(manifest,f,indent=2)
    catalog.append(manifest);preserve();print('HEAD_CORE_PART_PASS',PART,len(meshes),flush=True)
with open(os.path.join(OUT,'PART_CATALOG.json'),'w') as f:json.dump({'parts':catalog},f,indent=2)
print('HEAD_CORE_FAMILY_COMPLETE',len(catalog),flush=True)
