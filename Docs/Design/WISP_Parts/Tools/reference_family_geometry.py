"""Geometry for the nine PD-supplied design sheets; approved base stays fixed."""
import math
from mathutils import Vector,Matrix

def ref_color(mat,color,glow=False):
    mat.diffuse_color=(*color,1)
    p=mat.node_tree.nodes['Principled BSDF'];p.inputs['Base Color'].default_value=(*color,1)
    if glow:p.inputs['Emission Color'].default_value=(*color,1)

def ref_mesh(name,verts,faces,mat):
    me=bpy.data.meshes.new(name+'_mesh');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);COL.objects.link(o);o.data.materials.append(mat);return o

def ref_loop(rx,rz,n=8):
    if n!=8:return [(rx*math.cos(i*math.tau/n),rz*math.sin(i*math.tau/n)) for i in range(n)]
    return [(rx*x,rz*z) for x,z in [(-.65,-1),(.65,-1),(1,-.65),(1,.65),(.65,1),(-.65,1),(-1,.65),(-1,-.65)]]

def ref_duct(name,mouth,rx,rz,depth,axis=(0,1,-.25),round_port=False,bars=3):
    """A continuous outer case, chamfered lip, tapered cavity and recessed emitter."""
    c=Vector(mouth);d=Vector(axis).normalized();u=Vector((1,0,0));v=u.cross(d).normalized()
    n=16 if round_port else 8
    # Cut the actual inlet volume through surrounding armor, rather than
    # attaching a luminous label over a closed solid pod.
    existing=[o for o in COL.objects if o.type=='MESH']
    cut_verts=[c+d*t+u*x+v*z for t,w,h in [(.014,rx*.82,rz*.82),(-depth*.92,rx*.58,rz*.58)] for x,z in ref_loop(w,h,n)]
    cut_faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
    for i in range(n):j=(i+1)%n;cut_faces.append((i,j,n+j,n+i))
    cutter=ref_mesh('Temporary_Duct_Cavity_Cutter',cut_verts,cut_faces,GRAPHITE)
    bpy.context.view_layer.update()
    def bounds(obj):
        points=[obj.matrix_world @ Vector(v) for v in obj.bound_box]
        return [(min(p[i] for p in points),max(p[i] for p in points)) for i in range(3)]
    cb=bounds(cutter)
    for obj in existing:
        if 'Common_Back_Mount' in obj.name:continue
        bb=bounds(obj)
        if not all(min(cb[i][1],bb[i][1])-max(cb[i][0],bb[i][0])>.0001 for i in range(3)):continue
        for target in (obj,cutter):
            bm=bmesh.new();bm.from_mesh(target.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(target.data);bm.free()
        mod=obj.modifiers.new('Actual inset duct opening','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cutter
        bpy.context.view_layer.objects.active=obj;bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(cutter,do_unlink=True)
    rings=[(-depth,rx*.82,rz*.82),(0,rx,rz),(-.004,rx*.81,rz*.81),(-depth*.90,rx*.57,rz*.57)]
    verts=[c+d*t+u*x+v*z for t,w,h in rings for x,z in ref_loop(w,h,n)]
    faces=[]
    for k in range(3):
        for i in range(n):j=(i+1)%n;faces.append((k*n+i,k*n+j,(k+1)*n+j,(k+1)*n+i))
    for i in range(n):j=(i+1)%n;faces.append((i,3*n+i,3*n+j,j))
    o=ref_mesh(name+'_Integrated_Hollow_Case',verts,faces,ARMOR)
    o.data.materials.append(GUNMETAL);o.data.materials.append(GRAPHITE)
    for p in o.data.polygons:p.material_index=0 if p.index<n else (1 if p.index<2*n else 2)
    bevel(o,.001,1)
    end=c-d*depth*.87
    loop=ref_loop(rx*.56,rz*.56,n)
    ref_mesh(name+'_Recessed_Emission',[end+u*x+v*z for x,z in loop],[tuple(range(n))],CYAN)
    # Glow lives on the inside of the throat too, so a recessed outlet reads
    # naturally from an oblique camera rather than looking like a blind hole.
    front=ref_loop(rx*.79,rz*.79,n);back=ref_loop(rx*.58,rz*.58,n)
    for i in range(0,n,max(1,n//4)):
        j=(i+1)%n
        a=Vector(front[i]).lerp(Vector(front[j]),.20);b=Vector(front[i]).lerp(Vector(front[j]),.80)
        aa=Vector(back[i]).lerp(Vector(back[j]),.20);bb=Vector(back[i]).lerp(Vector(back[j]),.80)
        pts=[c-d*.010+u*a.x+v*a.y,c-d*.010+u*b.x+v*b.y,c-d*depth*.80+u*bb.x+v*bb.y,c-d*depth*.80+u*aa.x+v*aa.y]
        ref_mesh(name+'_Inner_Wall_Heat',pts,[(0,1,2,3)],CYAN)
    # Dark separators remain behind the lip, never protruding beyond the shell.
    if not round_port:
        for k in range(1,bars):
            z=(k/bars-.5)*rz*1.12
            pts=[end+d*.0006+u*x+v*(z+zz) for x,zz in [(-rx*.58,-.002),(rx*.58,-.002),(rx*.58,.002),(-rx*.58,.002)]]
            ref_mesh(name+'_Internal_Separator',pts,[(0,1,2,3)],GUNMETAL)
    else:
        # A smaller dark core exposes an annular orange/cyan emitter.
        ref_mesh(name+'_Dark_Inner_Core',[end+d*.0007+u*x+v*z for x,z in ref_loop(rx*.39,rz*.39,n)],[tuple(range(n))],GRAPHITE)
    nozzle_specs.append({'name':name,'mouth':list(c),'axis':list(d),'depth':depth,'width':rx*2,'height':rz*2,'shape':'round' if round_port else 'chamfered rectangular','recessed_emitter':True})

def ref_handle(x,y,z):
    outer=[(-.035,0),(-.022,.037),(.022,.037),(.035,0)]
    inner=[(-.019,.008),(-.012,.023),(.012,.023),(.019,.008)]
    verts=[(x+dx,y+yy,z+zz) for dx in (-.006,.006) for loop in (outer,inner) for yy,zz in loop]
    faces=[]
    for i in range(4):
        j=(i+1)%4;faces.extend([(i,j,4+j,4+i),(8+i,12+i,12+j,8+j),(i,8+i,8+j,j),(4+i,4+j,12+j,12+i)])
    bevel(ref_mesh('Open_Mount_Handle',verts,faces,GUNMETAL),.001,1)

def ref_center():
    back_interface()
    loft_z('Central_Power_Chassis',[(-.104,.033,.033),(-.070,.047,.047),(.076,.046,.044),(.108,.031,.029)],GRAPHITE,cy=.055)
    prism_xz('Tapered_Center_Armor',[(-.037,.089),(.037,.089),(.042,.040),(.023,-.083),(-.023,-.083),(-.042,.040)],.112,.020,ARMOR_SHADE)
    box('Small_Center_Status',(0,.124,.006),(.024,.003,.006),CYAN,.0005)

def ref_skywing():
    ref_center()
    for s in (-1,1):
        loft_z('Side_Thruster_Block',[(-.067,.036,.041),(-.045,.047,.054),(.081,.043,.047),(.102,.031,.033)],ARMOR,s*.071,.069)
        ref_handle(s*.059,.043,.101)
        cylinder_y('Folding_Wing_Hinge',(s*.113,.077,-.002),.025,.031,GUNMETAL,24)
        outline=[(s*x,z) for x,z in [(.111,.020),(.151,.018),(.275,-.065),(.286,-.096),(.224,-.113),(.145,-.074),(.110,-.022)]]
        prism_xz('Downswept_Folding_Wing',outline,.096,.025,ARMOR)
        prism_xz('Wing_Lower_Panel',[(s*x,z) for x,z in [(.145,-.050),(.226,-.087),(.276,-.082),(.280,-.101),(.224,-.106),(.147,-.068)]],.113,.005,ARMOR_SHADE)
        box('Flush_Side_Emission',(s*.071,.124,.035),(.008,.003,.042),CYAN,.0005)
        ref_duct('Main_Nozzle',(s*.065,.151,-.091),.045,.030,.040,(0,1,-.25))

def ref_crosslance():
    ref_center()
    for s in (-1,1):
        for t in (-1,1):
            a=Vector((s*.037,.063,t*.035));b=Vector((s*.167,.080,t*.165))
            segment('Diagonal_Cross_Arm',a,b,.025,.029,GRAPHITE)
            outline=[(s*x,t*z) for x,z in [(.113,.129),(.174,.225),(.212,.238),(.231,.202),(.198,.135),(.150,.094)]]
            prism_xz('Long_Diagonal_Pod_Armor',outline,.103,.060,ARMOR)
            prism_xz('Diagonal_Pod_Leading_Plane',[(s*x,t*z) for x,z in [(.143,.157),(.188,.226),(.208,.227),(.212,.203),(.181,.152)]],.138,.005,ARMOR_SHADE)
            ref_duct('Cross_Thruster',(s*.185,.146,t*.177),.032,.037,.055,(0,1,0))
    for s in (-1,1):box('Center_Vertical_Emission',(s*.034,.126,-.008),(.006,.003,.041),CYAN,.0005)

def ref_overthrust():
    ref_center()
    for s in (-1,1):
        loft_z('Upper_Engine_Chamber',[(-.050,.043,.047),(-.012,.058,.061),(.078,.055,.055),(.104,.040,.037)],ARMOR,s*.074,.065)
        ref_handle(s*.064,.035,.110)
        box('Inset_Upper_Intake',(s*.074,.122,.041),(.032,.003,.014),CYAN,.001)
        ref_duct('Heavy_Main_Bell',(s*.075,.183,-.075),.067,.070,.112,(0,1,-.40),True)
        prism_xz('Tall_Engine_Stabilizer',[(s*x,z) for x,z in [(.112,.005),(.119,.147),(.153,.176),(.160,.163),(.148,-.038)]],.059,.024,ARMOR_SHADE)
        ref_duct('Small_Outer_Vernier',(s*.164,.139,-.041),.020,.021,.045,(0,1,-.35),True)
        ref_duct('Small_Lower_Vernier',(s*.110,.104,-.137),.014,.019,.032,(0,1,-.5))

def ref_longtank():
    ref_center()
    for s in (-1,1):
        loft_z('Upper_Tank_Control_Pod',[(.005,.041,.041),(.032,.049,.052),(.091,.043,.045),(.111,.030,.031)],ARMOR,s*.066,.059)
        ref_handle(s*.061,.035,.112)
        ref_duct('Upper_Sustained_Nozzle',(s*.067,.128,.055),.034,.036,.059,(0,1,0),True)
        loft_z('Long_Faceted_Energy_Tank',[(-.305,.023,.025),(-.280,.038,.039),(-.238,.041,.042),(-.102,.041,.042),(-.071,.031,.032)],ARMOR,s*.066,.090)
        for z in (-.265,-.090):ring_z('Tank_Retaining_Collar',(s*.066,.090,z),.043,.039,.008,GUNMETAL)
        prism_xz('Tank_Long_Flat_Armor',[(s*.066-.027,-.111),(s*.066+.027,-.111),(s*.066+.031,-.242),(s*.066+.020,-.266),(s*.066-.024,-.266)],.134,.006,ARMOR_SHADE)
        box('Tank_Amber_Lock',(s*.066,.140,-.110),(.012,.003,.006),ORANGE,.0004)
        ref_duct('Tank_End_Port',(s*.066,.090,-.304),.021,.022,.025,(0,0,-1),True)

def ref_ramjet():
    ref_center()
    for s in (-1,1):
        loft_z('Straight_Acceleration_Block',[(-.091,.048,.059),(-.061,.057,.068),(.071,.057,.065),(.095,.039,.045)],ARMOR,s*.071,.068)
        ref_handle(s*.059,.042,.102)
        ref_duct('Straight_Main_Afterburner',(s*.069,.173,-.032),.057,.061,.113,(0,1,-.15))
        prism_xz('Swept_Upper_Stabilizer',[(s*x,z) for x,z in [(.081,.071),(.145,.172),(.213,.209),(.225,.197),(.191,.108),(.125,.040)]],.058,.026,ARMOR_SHADE)
        prism_xz('Red_Upper_Fin_Tip',[(s*x,z) for x,z in [(.180,.190),(.213,.209),(.225,.197),(.207,.160),(.191,.158)]],.074,.004,ORANGE)
        prism_xz('Lower_Stabilizer',[(s*x,z) for x,z in [(.100,-.068),(.192,-.129),(.230,-.141),(.219,-.157),(.150,-.145),(.101,-.111)]],.095,.020,ARMOR)
        prism_xz('Red_Lower_Fin_Tip',[(s*x,z) for x,z in [(.197,-.130),(.230,-.141),(.219,-.157),(.193,-.152)]],.108,.004,ORANGE)

def ref_pelvis():
    # Keep the exact approved socket while changing only the replacement module.
    box('Waist_Socket_Floor',(0,0,.499),(.083,.073,.012),GRAPHITE)
    for x in (-.043,.043):box('Waist_Socket_Side',(x,0,.510),(.006,.073,.012),GUNMETAL,.001)
    for y in (-.038,.038):box('Waist_Socket_End',(0,y,.510),(.072,.006,.012),GUNMETAL,.001)
    loft_z('Compact_Pelvis_Core',[(.363,.036,.039),(.418,.073,.058),(.470,.085,.064),(.493,.058,.045)],GRAPHITE)
    prism_xz('Dark_Pelvis_Front_Plate',[(-.062,.469),(.062,.469),(.046,.410),(.023,.365),(-.023,.365),(-.046,.410)],-.066,.012,ARMOR_SHADE)
    box('Pelvis_Cyan_Window',(0,-.074,.455),(.027,.003,.006),CYAN,.0005)

def ref_strider():
    ref_pelvis()
    for s in (-1,1):
        hip=Vector((s*.080,0,.422));knee=Vector((s*.085,0,.248));ankle=Vector((s*.090,0,.069))
        segment('Upper_Leg_Core',hip,knee,.021,.020,GRAPHITE)
        loft_z('Faceted_Thigh_Armor',[(.284,.024,.025),(.319,.035,.035),(.389,.035,.034),(.405,.026,.026)],ARMOR,s*.083)
        prism_xz('Hip_Skirt_Plate',[(s*x,z) for x,z in [(.053,.455),(.108,.449),(.125,.407),(.102,.388),(.060,.406)]],-.039,.036,ARMOR)
        prism_xz('Hip_Orange_Inlay',[(s*x,z) for x,z in [(.086,.421),(.094,.424),(.106,.414),(.100,.407)]],-.058,.002,ORANGE)
        segment('Shin_Core',knee,ankle,.017,.017,GRAPHITE)
        loft_z('Angular_Shin_Armor',[(.103,.023,.024),(.130,.033,.034),(.208,.039,.038),(.231,.024,.023)],ARMOR,s*.089)
        box('Shin_Dark_Sensor_Recess',(s*.104,-.038,.170),(.010,.004,.060),GRAPHITE,.001)
        box('Shin_Cyan_Sensor_Slit',(s*.104,-.041,.170),(.004,.002,.048),CYAN,.0005)
        for name,p,r in [('Hip',hip,.030),('Knee',knee,.027),('Ankle',ankle,.023)]:
            cylinder_x(name+'_Joint',p,r,.060,GRAPHITE,24);cylinder_x(name+'_Cap',(p.x+s*.033,p.y,p.z),r*.72,.006,GUNMETAL,24)
        prism_yz('Compact_Wedge_Foot',[(-.081,.012),(-.078,.036),(-.032,.065),(.027,.058),(.043,.019),(.033,.009)],s*.090,.074,ARMOR,.002)
        box('Foot_Orange_Toe',(s*.090,-.068,.037),(.033,.008,.005),ORANGE,.001)

def ref_tracks():
    ref_pelvis()
    for s in (-1,1):
        old=set(COL.objects);track_ring(s)
        affine=Matrix.Translation((s*(-.034),0,.010)) @ Matrix.Diagonal((.80,.56,.58,1))
        for o in set(COL.objects)-old:
            local=o.matrix_world.inverted() @ affine @ o.matrix_world
            for vertex in o.data.vertices:vertex.co=local @ vertex.co
        a=Vector((s*.079,0,.416));b=Vector((s*.110,0,.259))
        segment('Short_Track_Leg_Core',a,b,.030,.029,GRAPHITE)
        prism_xz('Track_Hip_Skirt',[(s*x,z) for x,z in [(.055,.459),(.110,.453),(.128,.413),(.104,.393),(.061,.412)]],-.037,.042,ARMOR)
        prism_yz('Track_Outer_Angular_Cowling',[(-.158,.189),(-.132,.250),(-.055,.290),(.071,.267),(.154,.201),(.145,.170),(-.063,.176)],s*.111,.110,ARMOR,.002)
        cylinder_x('Track_Link_Pivot',(s*.109,0,.267),.029,.079,GUNMETAL,24)
        box('Track_Side_Sensor_Recess',(s*.167,.030,.240),(.002,.037,.012),GRAPHITE,.0005)
        box('Track_Side_Cyan_Window',(s*.1685,.030,.240),(.001,.025,.004),CYAN,.0003)

def ref_tarantulan():
    ref_pelvis()
    loft_z('Low_Spider_Hub',[(.270,.070,.077),(.322,.123,.131),(.425,.108,.117),(.464,.063,.062)],GRAPHITE)
    prism_xz('Spider_Front_Point_Armor',[(-.055,.429),(.055,.429),(.042,.368),(.019,.333),(-.019,.333),(-.042,.368)],-.130,.019,ARMOR)
    for s in (-1,1):
        for f in (-1,1):
            hip=Vector((s*.102,f*.094,.391));knee=Vector((s*.213,f*.157,.281));tip=Vector((s*.295,f*.218,.017))
            segment('Spider_Upper_Bone',hip,knee,.030,.027,GRAPHITE)
            segment('Spider_Upper_Armor',hip.lerp(knee,.07),hip.lerp(knee,.78),.046,.041,ARMOR_SHADE)
            cylinder_x('Spider_Hip_Joint',hip,.034,.067,GRAPHITE,24)
            cylinder_x('Spider_Knee_Joint',knee,.033,.071,GUNMETAL,24)
            segment('Long_Pointed_Claw_Core',knee,tip,.034,.007,GRAPHITE)
            segment('Broad_Lower_Spider_Armor',knee.lerp(tip,.08),knee.lerp(tip,.65),.048,.031,ARMOR)
            box('Spider_Amber_Joint',(hip.x,hip.y,.427),(.022,.017,.005),ORANGE,.001)
    for s in (-1,1):box('Spider_Front_Cyan_Trace',(s*.035,-.142,.355),(.004,.003,.022),CYAN,.0005)

def ref_skylift():
    ref_pelvis()
    for s in (-1,1):
        loft_z('Hover_Upper_Hip_Case',[(.316,.054,.060),(.375,.064,.067),(.439,.049,.053),(.462,.030,.034)],ARMOR_SHADE,s*.080)
        loft_z('Bulky_Lift_Pod_Core',[(.119,.045,.050),(.162,.065,.077),(.291,.069,.078),(.335,.048,.052)],GRAPHITE,s*.090)
        prism_xz('Large_Front_Hover_Skirt',[(s*x,z) for x,z in [(.036,.323),(.119,.325),(.163,.217),(.150,.185),(.067,.201),(.042,.258)]],-.077,.028,ARMOR)
        prism_xz('Side_Lift_Armor',[(s*x,z) for x,z in [(.111,.345),(.154,.302),(.172,.232),(.155,.204),(.127,.235)]],.009,.118,ARMOR_SHADE)
        box('Lift_Cyan_Upper_Strip',(s*.114,-.065,.370),(.005,.003,.030),CYAN,.0005)
        ref_duct('Hover_Lift_Mouth',(s*.091,0,.111),.043,.047,.056,(0,0,-1),True)

def build_reference(part,api):
    globals().update({k:v for k,v in api.items() if not k.startswith('__')})
    global nozzle_specs
    nozzle_specs=[]
    if part.startswith('B-'):
        ref_color(ARMOR,(.070,.080,.108));ref_color(ARMOR_SHADE,(.14,.15,.18));ref_color(GRAPHITE,(.022,.028,.041))
        ref_color(CYAN,(1,.30,.025),True)
        if part=='B-03_OVERTHRUST':ref_color(CYAN,(.95,.055,.025),True)
        if part=='B-05_RAMJET':ref_color(ORANGE,(.48,.045,.055))
    factories={'B-01_SKYWING':ref_skywing,'B-02_CROSSLANCE':ref_crosslance,'B-03_OVERTHRUST':ref_overthrust,'B-04_LONGTANK':ref_longtank,'B-05_RAMJET':ref_ramjet,'L-01_STRIDER':ref_strider,'L-02_BASTION_TRACK':ref_tracks,'L-03_TARANTULAN':ref_tarantulan,'L-05_SKYLIFT':ref_skylift}
    factories[part]()
    api['reference_nozzle_specs']=nozzle_specs
    return []
