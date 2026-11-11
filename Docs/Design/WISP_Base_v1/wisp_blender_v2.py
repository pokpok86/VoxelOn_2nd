# WISP V2 BLOCKOUT / Blender Scripting > Run Script
# NOT production-ready, Blender/Unity runtime validation pending.
# X right; -Y front; Z up. Meters.
import bpy
from math import radians,copysign,sin,cos
from mathutils import Vector,Matrix
VARIANT=bpy.app.driver_namespace.get("WISP_VARIANT",{})
POSE=VARIANT.get("pose","IDLE")
ARM_SPLAY=float(VARIANT.get("arm_splay_deg",18))
ELBOW_BEND=float(VARIANT.get("elbow_bend_deg",-38))
LEG_SPLAY=float(VARIANT.get("leg_splay_deg",8))
SHOULDER_SIZE=tuple(VARIANT.get("shoulder_size",(.235,.215,.185)))
SHOULDER_BEVEL=float(VARIANT.get("shoulder_bevel",.024))
SHOULDER_BEVEL_SEGMENTS=int(VARIANT.get("shoulder_bevel_segments",1))
SHOULDER_TILT=float(VARIANT.get("shoulder_tilt_deg",18))
SHOULDER_PROFILE=VARIANT.get("shoulder_profile","TRAPEZOID")
HEAD_SIZE=tuple(VARIANT.get("head_size",(.16,.145,.125)))
HEAD_FORM=VARIANT.get("head_form","ANGULAR_SENSOR")
FACE_FORM=VARIANT.get("face_form","SENSOR_BAND")
FACE_WIDTH=float(VARIANT.get("face_width",.82))
FACE_HEIGHT=float(VARIANT.get("face_height",.48))
HEAD_ANTENNA_HEIGHT=float(VARIANT.get("head_antenna_height",.12))
HEAD_CENTER_Z=float(VARIANT.get("head_center_z",1.395))
NECK_RADIUS=float(VARIANT.get("neck_radius",.035))
ARMOR_COLOR=tuple(VARIANT.get("armor_color",(.72,.73,.72)))
FRAME_COLOR=tuple(VARIANT.get("frame_color",(.055,.068,.082)))
MUZZLE_RADIUS=float(VARIANT.get("muzzle_radius",.095))
MUZZLE_ENABLED=bool(VARIANT.get("muzzle_enabled",True))
FOREARM_SIZE=tuple(VARIANT.get("forearm_size",(.22,.205,.36)))
FOREARM_FORM=VARIANT.get("forearm_form","FACETED")
UPPER_ARM_SIZE=tuple(VARIANT.get("upper_arm_size",(.125,.13,.235)))
UPPER_ARM_CENTER_DROP=float(VARIANT.get("upper_arm_center_drop",.14))
ELBOW_DROP=float(VARIANT.get("elbow_drop",.27))
FOREARM_DROP=float(VARIANT.get("forearm_drop",.22))
TORSO_FORM=VARIANT.get("torso_form","VEST_TAPER")
TORSO_HEIGHT=float(VARIANT.get("torso_height",.40))
TORSO_CENTER_Z=float(VARIANT.get("torso_center_z",1.05))
UPPER_SHIFT=TORSO_CENTER_Z+TORSO_HEIGHT/2-1.25
HIP_Z=float(VARIANT.get("hip_z",.685))
LEG_SCALE=float(VARIANT.get("leg_scale",1.0))
JOINT_BACK_SHIFT=float(VARIANT.get("joint_back_shift_m",.02))
PELVIS_CENTER_Z=float(VARIANT.get("pelvis_center_z",HIP_Z+.045))
PELVIS_FORM=VARIANT.get("pelvis_form","FLARED_HIP")
PELVIS_ARMOR_LEVEL=float(VARIANT.get("pelvis_armor_level",-.24))
CORE_SIZE=tuple(VARIANT.get("core_size",(.42,.25,.43)))
CHEST_SIZE=tuple(VARIANT.get("chest_size",(.48,.18,.25)))
PELVIS_SIZE=tuple(VARIANT.get("pelvis_size",(.34,.23,.17)))
FOOT_SIZE=tuple(VARIANT.get("foot_size",(.175,.245,.13)))
FOOT_OFFSET_Y=float(VARIANT.get("foot_offset_y",-.055))
TORSO_ARMOR_LEVEL=float(VARIANT.get("torso_armor_level",.10))
CHEST_ARMOR_HEIGHT_RATIO=float(VARIANT.get("chest_armor_height_ratio",.82))
SHOULDER_JOINT_RADIUS=.033
SHOULDER_AXIS_STANDOFF=.052
SHOULDER_JOINT_DEPTH=.066
HIP_JOINT_RADIUS=.041
HIP_JOINT_DEPTH=.075
SHOULDER_GAP=float(VARIANT.get("shoulder_gap_m",.015))
SHOULDER_Z_OFFSET=float(VARIANT.get("shoulder_z_offset",0.0))
CHEST_PROFILE=VARIANT.get("chest_profile","TAPERED")
LIMB_PROFILE=VARIANT.get("limb_profile","CANNON_TAPER")
SHOULDER_BODY_EDGE_X=CHEST_SIZE[0]/2
SHOULDER_AXIS_X=SHOULDER_BODY_EDGE_X+SHOULDER_AXIS_STANDOFF+SHOULDER_GAP
SHOULDER_SOCKET_X=SHOULDER_BODY_EDGE_X
ARM_UPPER_X=SHOULDER_AXIS_X+sin(radians(ARM_SPLAY))*UPPER_ARM_CENTER_DROP
ELBOW_X=SHOULDER_AXIS_X+sin(radians(ARM_SPLAY))*ELBOW_DROP
FOREARM_X=ELBOW_X
bpy.app.driver_namespace["WISP_DERIVED"]={"shoulder_axis_x":SHOULDER_AXIS_X,"shoulder_socket_x":SHOULDER_SOCKET_X,"shoulder_gap_m":SHOULDER_GAP,"joint_back_shift_m":JOINT_BACK_SHIFT,"arm_chain_x":ARM_UPPER_X,"elbow_axis_x":ELBOW_X}
cn="WISP_V2"
old=bpy.data.collections.get(cn)
if old:
    for o in list(old.objects): bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.collections.remove(old)
col=bpy.data.collections.new(cn); bpy.context.scene.collection.children.link(col)
bpy.context.scene.unit_settings.system="METRIC"
def par(o,p):
    if p:
        bpy.context.view_layer.update(); world=o.matrix_world.copy()
        o.parent=p; o.matrix_world=world
    return o
def par_local(o,p,location,rotation=None):
    # Parent a skeletal link using the pivot's actual local frame. This avoids
    # baking display/world offsets into the hinge and misaligning link axes.
    o.parent=p
    o.matrix_parent_inverse=Matrix.Identity(4)
    o.location=location
    o.rotation_euler=rotation if rotation is not None else (0,0,0)
    return o
def mat(n,c,glow=0,metallic=.32,roughness=.34):
    m=bpy.data.materials.get("WISP_"+n) or bpy.data.materials.new("WISP_"+n)
    m.use_nodes=True; m.diffuse_color=(*c,1)
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*c,1)
    bs.inputs["Metallic"].default_value=metallic
    bs.inputs["Roughness"].default_value=roughness
    if bs.inputs.get("Coat Weight"):
        bs.inputs["Coat Weight"].default_value=.18
        bs.inputs["Coat Roughness"].default_value=.24
    noise=m.node_tree.nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value=150
    noise.inputs["Detail"].default_value=2
    bump=m.node_tree.nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value=.045
    bump.inputs["Distance"].default_value=.006
    m.node_tree.links.new(noise.outputs["Fac"],bump.inputs["Height"])
    m.node_tree.links.new(bump.outputs["Normal"],bs.inputs["Normal"])
    if glow:
        bs.inputs["Emission Color"].default_value=(*c,1)
        bs.inputs["Emission Strength"].default_value=glow
    return m
armor=mat("Armor",ARMOR_COLOR,metallic=.24,roughness=.42)
frame=mat("Frame",FRAME_COLOR,metallic=.62,roughness=.40)
orange=mat("Accent",tuple(VARIANT.get("accent_color",(.82,.30,.07))),metallic=.28,roughness=.38)
cyan=mat("Cyan",(.025,.58,.78),3,metallic=.12,roughness=.26)
def pivot(n,p,parent=None):
    o=bpy.data.objects.new(n,None);col.objects.link(o);o.location=p
    o.empty_display_type='ARROWS';o.empty_display_size=.04
    return par(o,parent)
def cube(n,p,d,m,parent,bevel=.007,bevel_segments=2):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object
    for group in tuple(o.users_collection):group.objects.unlink(o)
    col.objects.link(o);o.name=n;o.dimensions=d
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(m)
    if bevel:
        b=o.modifiers.new("Chamfer","BEVEL");b.width=min(bevel,min(d)*.28);b.segments=bevel_segments
        o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def shoulder_pad(n,p,d,m,parent,side,profile):
    # A small extruded silhouette, not a softened box. Coordinates are local
    # to the joint and mirrored so each cap slopes outward symmetrically.
    outlines={
        "NARROW_WEDGE":[(-.36,.50),(.18,.50),(.50,.10),(.28,-.40),(-.08,-.50),(-.50,-.04)],
        "TRAPEZOID":[(-.43,.48),(.32,.48),(.50,.16),(.32,-.46),(-.30,-.50),(-.50,.02)],
        "LOW_SWEPT":[(-.40,.42),(.48,.30),(.42,-.12),(.12,-.50),(-.43,-.34),(-.50,.10)],
        "INVERTED_LOW_SWEPT":[(-.40,-.42),(.48,-.30),(.42,.12),(.12,.50),(-.43,.34),(-.50,-.10)],
    }
    outline=outlines[profile]
    count=len(outline)
    sections=[(-d[1]/2,.72),(-d[1]*.12,1.0),(d[1]/2,.76)]
    verts=[]
    for y,scale in sections:
        for x,z in outline:
            verts.append((side*x*d[0]*scale,y,z*d[2]*scale))
    faces=[tuple(range(count-1,-1,-1))]
    for section in range(len(sections)-1):
        a=section*count; b=(section+1)*count
        for i in range(count):
            j=(i+1)%count
            faces.append((a+i,a+j,b+j,b+i))
    faces.append(tuple(range((len(sections)-1)*count,len(sections)*count)))
    mesh=bpy.data.meshes.new(n+"_Mesh")
    mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new(n,mesh);col.objects.link(o);o.location=p
    o.data.materials.append(m)
    bevel=o.modifiers.new("Single_Facet_Chamfer","BEVEL")
    bevel.width=min(SHOULDER_BEVEL,min(d)*.18);bevel.segments=1
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def ring(n,p,r,depth,m,parent,angle=(0,0,0),bevel=0):
    bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=r,depth=depth,location=p,rotation=angle)
    o=bpy.context.object
    for group in tuple(o.users_collection):group.objects.unlink(o)
    col.objects.link(o);o.name=n;o.data.materials.append(m)
    if bevel:
        b=o.modifiers.new("Round_Edge","BEVEL");b.width=min(bevel,r*.18,depth*.18);b.segments=1
        o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def can_axis_joint(n,p,r,depth,moving_parent,fixed_parent,can_mat,axle_mat):
    # Smooth cylindrical tuna-can drum oriented front-to-back. The rear axle
    # and pivot centers remain separate and mechanically aligned.
    rotation=(radians(90),0,0)
    bpy.ops.mesh.primitive_cylinder_add(vertices=24,radius=r*.58,depth=depth*1.30,
                                        location=(p[0],p[1]+depth*.06,p[2]),rotation=rotation)
    axle=bpy.context.object
    for group in tuple(axle.users_collection): group.objects.unlink(axle)
    col.objects.link(axle);axle.name=n+"_Axle";axle.data.materials.append(axle_mat)
    bevel=axle.modifiers.new("Axle_Edge","BEVEL");bevel.width=min(.002,r*.07,depth*.12);bevel.segments=1
    axle.modifiers.new("Normals","WEIGHTED_NORMAL")
    par(axle,fixed_parent)
    bpy.ops.mesh.primitive_cylinder_add(vertices=24,radius=r,depth=depth,
                                        location=p,rotation=rotation)
    drum=bpy.context.object
    for group in tuple(drum.users_collection): group.objects.unlink(drum)
    col.objects.link(drum);drum.name=n+"_Can";drum.data.materials.append(can_mat)
    bevel=drum.modifiers.new("Can_Lid_Beveled_Edge","BEVEL");bevel.width=min(.003,r*.09,depth*.15);bevel.segments=1
    drum.modifiers.new("Normals","WEIGHTED_NORMAL")
    par_local(drum,moving_parent,(0,0,0),rotation)
    return axle,drum
def lens(n,p,d,m,parent):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,radius=1,location=p)
    o=bpy.context.object
    for group in tuple(o.users_collection): group.objects.unlink(o)
    col.objects.link(o);o.name=n;o.scale=(d[0]/2,d[1]/2,d[2]/2)
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for poly in o.data.polygons: poly.use_smooth=True
    o.data.materials.append(m)
    return par(o,parent)
def neck_stem(n,z0,z1,r,m,parent):
    length=z1-z0
    bpy.ops.mesh.primitive_cone_add(vertices=16,radius1=r*1.12,radius2=r,depth=length,location=(0,0,(z0+z1)/2))
    o=bpy.context.object
    for group in tuple(o.users_collection): group.objects.unlink(o)
    col.objects.link(o);o.name=n;o.data.materials.append(m)
    for poly in o.data.polygons: poly.use_smooth=True
    bevel=o.modifiers.new("Neck_Edge_Softening","BEVEL");bevel.width=min(.004,length*.04);bevel.segments=2
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def profile_shell(n,p,d,m,parent,profile):
    # Eight-sided chest outline gives a shaped armor silhouette while keeping
    # a separate, narrower frame core visible behind it.
    outlines={
        "TAPERED":[(-.36,.50),(.36,.50),(.50,.18),(.40,-.28),(.26,-.50),(-.26,-.50),(-.40,-.28),(-.50,.18)],
        "SOFT_VEST":[(-.38,.50),(.38,.50),(.50,.12),(.36,-.42),(.24,-.50),(-.24,-.50),(-.36,-.42),(-.50,.12)],
        "FLARED_TOP":[(-.34,.50),(.34,.50),(.50,.05),(.33,-.38),(.22,-.50),(-.22,-.50),(-.33,-.38),(-.50,.05)],
        "HEAD":[(-.28,.50),(.28,.50),(.48,.20),(.43,-.26),(.20,-.50),(-.20,-.50),(-.43,-.26),(-.48,.20)],
        "SENSOR_BAND":[(-.42,.12),(-.30,.46),(.30,.46),(.42,.12),(.30,-.38),(.16,-.50),(-.16,-.50),(-.30,-.38)],
        "VISOR_CHEVRON":[(-.50,.30),(-.30,.48),(.22,.48),(.50,.14),(.34,-.32),(.14,-.50),(-.18,-.46),(-.40,-.20)],
        "VISOR_SWEEP":[(-.50,.12),(-.34,.44),(.16,.50),(.48,.22),(.38,-.18),(.14,-.44),(-.20,-.48),(-.42,-.24)],
        "VISOR_WRAP":[(-.48,.26),(-.26,.50),(.26,.50),(.48,.26),(.42,-.18),(.24,-.46),(-.24,-.46),(-.42,-.18)],
        "BEAKED_VISOR":[(-.50,.28),(-.34,.48),(.34,.48),(.50,.16),(.30,-.12),(0,-.50),(-.30,-.12)],
        "CHEST_CROWN":[(-.46,.22),(-.34,.50),(.34,.50),(.46,.22),(.36,-.22),(.18,-.42),(-.18,-.42),(-.36,-.22)],
        "KNEE_GUARD":[(-.34,.38),(.34,.38),(.50,.08),(.28,-.50),(-.28,-.50),(-.50,.08)],
    }
    outline=outlines[profile]; count=len(outline)
    verts=[(x*d[0],y,z*d[2]) for y in (-d[1]/2,d[1]/2) for x,z in outline]
    faces=[tuple(range(count-1,-1,-1)),tuple(range(count,2*count))]
    for i in range(count):
        j=(i+1)%count; faces.append((i,j,count+j,count+i))
    mesh=bpy.data.meshes.new(n+"_Mesh"); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(n,mesh); col.objects.link(o); o.location=p; o.data.materials.append(m)
    bevel=o.modifiers.new("Armor_Edge_Bevel","BEVEL"); bevel.width=min(.005,min(d)*.04); bevel.segments=1
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def sensor_head(n,p,d,m,parent,form):
    # A compact helmet-like shell: sloped crown, cheek taper and different
    # front/mid/rear contours keep the sensor housing from reading as a box.
    profiles={
        "ANGULAR_SENSOR":([(-.22,.50),(.22,.50),(.43,.24),(.43,-.10),(.24,-.40),(0,-.50),(-.24,-.40),(-.43,-.10),(-.43,.24)],.72,.82),
        "RAISED_BROW":([(-.24,.34),(-.20,.50),(.18,.50),(.24,.34),(.45,.14),(.34,-.26),(.16,-.48),(-.16,-.48),(-.34,-.26),(-.44,.14)],.70,.78),
        "DEEP_WEDGE":([(-.20,.44),(.18,.50),(.40,.18),(.34,-.28),(.13,-.50),(-.13,-.50),(-.36,-.26),(-.42,.10)],.78,.70),
        "CROWNED_CHEEK":([(-.18,.35),(-.08,.50),(.16,.48),(.30,.27),(.46,.10),(.38,-.22),(.20,-.44),(.02,-.50),(-.20,-.43),(-.40,-.18),(-.44,.12)],.76,.84),
        "SWEPT_COCKPIT":([(-.26,.42),(.04,.50),(.25,.38),(.42,.11),(.36,-.18),(.15,-.49),(-.10,-.50),(-.33,-.31),(-.45,-.02),(-.39,.22)],.72,.76),
        "SHARK_FIN_HELMET":([(-.30,.26),(-.12,.44),(.12,.50),(.31,.31),(.44,.06),(.32,-.30),(.12,-.50),(-.12,-.46),(-.38,-.20),(-.46,.02)],.80,.72),
    }
    outline,mid_scale,rear_scale=profiles[form]
    count=len(outline); verts=[]
    for y,scale in ((-d[1]/2,1.0),(-d[1]*.20,mid_scale),(d[1]/2,rear_scale)):
        for x,z in outline: verts.append((x*d[0]*scale,y,z*d[2]*scale))
    faces=[]
    faces.append(tuple(range(count-1,-1,-1)))
    for ring_index in range(2):
        a=ring_index*count; b=(ring_index+1)*count
        for i in range(count):
            j=(i+1)%count; faces.append((a+i,a+j,b+j,b+i))
    faces.append(tuple(range(2*count,3*count)))
    mesh=bpy.data.meshes.new(n+"_SculptedMesh"); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(n,mesh); col.objects.link(o); o.location=p; o.data.materials.append(m)
    bevel=o.modifiers.new("Soft_Form_Edges","BEVEL"); bevel.width=min(.005,min(d)*.035); bevel.segments=1
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def limb_shell(n,p,d,m,parent,profile):
    outlines={
        "CANNON_TAPER":[(-.32,.50),(.32,.50),(.46,.25),(.50,-.18),(.42,-.42),(.29,-.50),(-.29,-.50),(-.42,-.42),(-.50,-.18),(-.46,.25)],
        "CHISELED":[(-.27,.50),(.27,.50),(.48,.22),(.48,-.22),(.34,-.50),(-.34,-.50),(-.48,-.22),(-.48,.22)],
        "FLARED_LOWER":[(-.36,.50),(.36,.50),(.40,.12),(.50,-.22),(.46,-.50),(-.46,-.50),(-.50,-.22),(-.40,.12)],
    }
    outline=outlines[profile]; count=len(outline)
    verts=[(x*d[0],y,z*d[2]) for y in (-d[1]/2,d[1]/2) for x,z in outline]
    faces=[tuple(range(count-1,-1,-1)),tuple(range(count,2*count))]
    for i in range(count):
        j=(i+1)%count; faces.append((i,j,count+j,count+i))
    mesh=bpy.data.meshes.new(n+"_Mesh"); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(n,mesh); col.objects.link(o); o.location=p; o.data.materials.append(m)
    bevel=o.modifiers.new("Armor_Edge_Bevel","BEVEL"); bevel.width=min(.006,min(d)*.045); bevel.segments=1
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def round_cannon_shell(n,p,d,m,parent,form):
    # Ring-built shell gives the cannon a shaped, tapered volume instead of
    # the constant-depth slab produced by an extruded front silhouette.
    bands={
        "TAPERED_BARREL":[(.50,.62,.66),(.30,.84,.84),(.02,1.0,1.0),(-.27,.88,.86),(-.50,.62,.72)],
        "OVAL_CANNON":[(.50,.72,.76),(.27,.94,.94),(-.03,.98,1.0),(-.30,.90,.88),(-.50,.72,.70)],
        "SLENDER_LANCE":[(.50,.60,.62),(.26,.78,.78),(-.08,.87,.90),(-.34,.76,.78),(-.50,.58,.62)],
        "SOFT_RECT_CANNON":[(.50,.58,.62),(.30,.86,.82),(.02,1.0,1.0),(-.27,.86,.82),(-.50,.60,.64)],
        "BEVELED_D_CANNON":[(.50,.58,.58),(.30,.82,.78),(.04,.94,1.0),(-.24,1.0,.86),(-.50,.72,.66)],
        "SWEPT_CANNON":[(.50,.52,.64),(.30,.78,.91),(.02,1.0,1.0),(-.24,.94,.76),(-.50,.66,.52)],
        "EGG_GUN":[(.50,.46,.58),(.28,.66,.74),(.02,.88,.90),(-.22,1.0,.88),(-.40,.92,.78),(-.50,.68,.62)],
    }
    exponent={"TAPERED_BARREL":1.0,"OVAL_CANNON":1.0,"SLENDER_LANCE":1.0,
              "SOFT_RECT_CANNON":.62,"BEVELED_D_CANNON":.72,"SWEPT_CANNON":.66,"EGG_GUN":.90}[form]
    profile=bands[form]; sides=10 if form=="EGG_GUN" else 8; verts=[]
    from math import cos,sin,pi
    for z_ratio,x_scale,y_scale in profile:
        for i in range(sides):
            angle=2*pi*i/sides
            cx=cos(angle); sy=sin(angle)
            x=copysign(abs(cx)**exponent,cx)
            y=copysign(abs(sy)**exponent,sy)
            verts.append((d[0]*.5*x_scale*x,d[1]*.5*y_scale*y,d[2]*z_ratio))
    faces=[tuple(range(sides-1,-1,-1))]
    for band in range(len(profile)-1):
        a=band*sides; b=(band+1)*sides
        for i in range(sides):
            j=(i+1)%sides; faces.append((a+i,a+j,b+j,b+i))
    faces.append(tuple(range((len(profile)-1)*sides,len(profile)*sides)))
    mesh=bpy.data.meshes.new(n+"_RingShellMesh"); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(n,mesh); col.objects.link(o); o.location=p; o.data.materials.append(m)
    bevel=o.modifiers.new("Continuous_Form_Edges","BEVEL"); bevel.width=min(.004,min(d)*.025); bevel.segments=2
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def sculpted_torso(n,p,d,armor_mat,frame_mat,parent,form):
    # One continuous waist-to-chest surface replaces the stacked core, chest,
    # and abdomen boxes. Front ceramic and side frame are material zones on the
    # same shaped volume, not separate plates laid over it.
    bands={
        "VEST_TAPER":[(-.50,.54,.68),(-.30,.70,.80),(-.08,.73,.86),(.12,.91,.94),(.34,1.0,1.0),(.50,.95,.88)],
        "WAIST_SCULPT":[(-.50,.48,.64),(-.29,.70,.78),(-.04,.74,.88),(.12,.90,.94),(.34,1.0,1.0),(.50,.96,.90)],
        "INVERTED_WAIST_SCULPT":[(-.50,.96,.90),(-.34,1.0,1.0),(-.12,.90,.94),(.04,.74,.88),(.29,.70,.78),(.50,.48,.64)],
        "CORE_FLARE":[(-.50,.62,.72),(-.28,.67,.82),(-.03,.73,.90),(.18,.92,.96),(.38,1.0,1.0),(.50,.98,.92)],
    }
    profile=bands[form]; sides=8; verts=[]
    from math import cos,sin,pi
    exponent=.90
    for z_ratio,x_scale,y_scale in profile:
        for i in range(sides):
            angle=2*pi*i/sides; cx=cos(angle); sy=sin(angle)
            x=copysign(abs(cx)**exponent,cx); y=copysign(abs(sy)**exponent,sy)
            verts.append((d[0]*.5*x_scale*x,y*d[1]*.5*y_scale,d[2]*z_ratio))
    faces=[tuple(range(sides-1,-1,-1))]; face_z=[]; face_front=[]
    for band in range(len(profile)-1):
        a=band*sides; b=(band+1)*sides
        z_mid=(profile[band][0]+profile[band+1][0])*.5
        for i in range(sides):
            j=(i+1)%sides; faces.append((a+i,a+j,b+j,b+i))
            angle=2*pi*(i+.5)/sides
            face_z.append(z_mid); face_front.append(sin(angle)<-.45)
    faces.append(tuple(range((len(profile)-1)*sides,len(profile)*sides)))
    mesh=bpy.data.meshes.new(n+"_ContinuousMesh"); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(n,mesh); col.objects.link(o); o.location=p
    o.data.materials.append(armor_mat); o.data.materials.append(frame_mat)
    for index,poly in enumerate(mesh.polygons):
        if index>0 and index<=len(face_z) and face_front[index-1] and face_z[index-1]>TORSO_ARMOR_LEVEL:
            poly.material_index=0
        else:
            poly.material_index=1
    bevel=o.modifiers.new("Form_Edge_Soften","BEVEL"); bevel.width=min(.004,min(d)*.015); bevel.segments=1
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def sculpted_pelvis(n,p,d,armor_mat,frame_mat,parent,form):
    # Shape the hip shell as part of the torso silhouette instead of a beveled
    # box. The top contour meets the lower torso; side faces remain dark frame.
    bands={
        "FLARED_HIP":[(-.50,.62,.66),(-.30,.84,.82),(-.06,1.0,.96),(.20,.94,1.0),(.50,.70,.78)],
        "TAPERED_HIP":[(-.50,.70,.64),(-.24,.94,.82),(.02,.98,.94),(.27,.82,1.0),(.50,.64,.78)],
        "CRESTED_HIP":[(-.50,.58,.68),(-.25,.86,.84),(.04,1.0,.98),(.30,.96,.94),(.50,.72,.76)],
        # Slim transverse bridge: clipped ends, nearly constant front-back depth,
        # so the pelvis spans both hip cans without a hanging-belly bulge.
        "HIP_BRIDGE":[(-.50,.84,1.0),(-.36,.96,1.0),(.36,.96,1.0),(.50,.84,1.0)],
    }
    profile=bands[form]; sides=8; verts=[]
    from math import cos,sin,pi
    exponent=.90
    for z_ratio,x_scale,y_scale in profile:
        for i in range(sides):
            angle=2*pi*i/sides; cx=cos(angle); sy=sin(angle)
            x=copysign(abs(cx)**exponent,cx); y=copysign(abs(sy)**exponent,sy)
            verts.append((d[0]*.5*x_scale*x,y*d[1]*.5*y_scale,d[2]*z_ratio))
    faces=[tuple(range(sides-1,-1,-1))]; face_z=[]; face_front=[]
    for band in range(len(profile)-1):
        a=band*sides; b=(band+1)*sides
        z_mid=(profile[band][0]+profile[band+1][0])*.5
        for i in range(sides):
            j=(i+1)%sides; faces.append((a+i,a+j,b+j,b+i))
            face_z.append(z_mid); face_front.append(sin(2*pi*(i+.5)/sides)<-.45)
    faces.append(tuple(range((len(profile)-1)*sides,len(profile)*sides)))
    mesh=bpy.data.meshes.new(n+"_SculptedMesh"); mesh.from_pydata(verts,[],faces); mesh.update()
    o=bpy.data.objects.new(n,mesh); col.objects.link(o); o.location=p
    o.data.materials.append(armor_mat); o.data.materials.append(frame_mat)
    for index,poly in enumerate(mesh.polygons):
        if index>0 and index<=len(face_z) and face_front[index-1] and face_z[index-1]>PELVIS_ARMOR_LEVEL:
            poly.material_index=0
        else:
            poly.material_index=1
    bevel=o.modifiers.new("Hip_Form_Edge_Soften","BEVEL"); bevel.width=min(.004,min(d)*.015); bevel.segments=1
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def link_between(n,start,end,r,m,parent):
    a=Vector(start); b=Vector(end); direction=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=r,depth=direction.length,location=(a+b)/2)
    o=bpy.context.object
    for group in tuple(o.users_collection): group.objects.unlink(o)
    col.objects.link(o); o.name=n; o.rotation_euler=direction.to_track_quat('Z','Y').to_euler()
    o.data.materials.append(m)
    bevel=o.modifiers.new("Link_Rim","BEVEL"); bevel.width=min(r*.32,direction.length*.08); bevel.segments=2
    o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)

root=pivot("ROOT",(0,0,0))
pelvis=pivot("Pelvis_Frame",(0,0,PELVIS_CENTER_Z),root)
waist=pivot("Waist_Yaw_Z",(0,0,HIP_Z+.125),pelvis)
breath=pivot("UpperBody_BreathIdle",(0,0,TORSO_CENTER_Z),waist)
pelvis_x=PELVIS_SIZE[0]/.34; pelvis_y=PELVIS_SIZE[1]/.23; pelvis_z=PELVIS_SIZE[2]/.17
core_x=CORE_SIZE[0]/.42; core_y=CORE_SIZE[1]/.25; core_z=CORE_SIZE[2]/.43
chest_x=CHEST_SIZE[0]/.48; chest_y=CHEST_SIZE[1]/.18; chest_z=CHEST_SIZE[2]/.25
sculpted_pelvis("Pelvis",(0,0,PELVIS_CENTER_Z),PELVIS_SIZE,armor,frame,pelvis,PELVIS_FORM)
# Seat the hip-can centers slightly inside the pelvis silhouette. Keep these
# short visual links at the actual joint height; they do not change articulation.
hip_bridge_half_width=PELVIS_SIZE[0]*.5*.96
for side,sign in (("L",-1),("R",1)):
    link_between("Pelvis_Hip_Form_Link_"+side,
                 (sign*hip_bridge_half_width,JOINT_BACK_SHIFT,HIP_Z),
                 (sign*.11,JOINT_BACK_SHIFT,HIP_Z),.012,frame,pelvis)
sculpted_torso("Torso_Core",(0,-.02,TORSO_CENTER_Z),(CHEST_SIZE[0],CORE_SIZE[1],TORSO_HEIGHT),
               armor,frame,breath,TORSO_FORM)
chest_armor_height=TORSO_HEIGHT*CHEST_ARMOR_HEIGHT_RATIO
chest_armor_top=TORSO_CENTER_Z+TORSO_HEIGHT/2+.002
profile_shell("Chest_Armor",(0,-.02-CORE_SIZE[1]*.28,chest_armor_top-chest_armor_height/2),
              (CHEST_SIZE[0]*.94,.12,chest_armor_height),armor,breath,"CHEST_CROWN")
profile_shell("Chest_Accent_Panel",(.065,-.151,TORSO_CENTER_Z+TORSO_HEIGHT*.30),
              (.065,.006,.055),orange,breath,"CHEST_CROWN")
head=pivot("Head_FIXED",(0,-.012,HEAD_CENTER_Z),breath)
sensor_head("Head_Small",(0,-.02,HEAD_CENTER_Z),HEAD_SIZE,armor,head,HEAD_FORM)
profile_shell("Face_Aperture",(0,-.02-HEAD_SIZE[1]*.48-.004,HEAD_CENTER_Z),
              (HEAD_SIZE[0]*FACE_WIDTH,.010,HEAD_SIZE[2]*FACE_HEIGHT),frame,head,FACE_FORM)
lens("Face_Visor",(0,-.02-HEAD_SIZE[1]*.50-.012,HEAD_CENTER_Z+HEAD_SIZE[2]*.02),
     (HEAD_SIZE[0]*.76,.014,HEAD_SIZE[2]*.14),cyan,head)
antenna_side=float(VARIANT.get("antenna_side",1))
antenna_root=(0,-.015,HEAD_CENTER_Z+HEAD_SIZE[2]*.485)
antenna_tip=(antenna_side*HEAD_SIZE[0]*.28,-.015,antenna_root[2]+HEAD_ANTENNA_HEIGHT)
ring("Head_Antenna_Base",antenna_root,.010,.012,frame,head)
link_between("Head_Antenna",antenna_root,antenna_tip,.005,armor,head)
# Hide more of the neck inside both neighboring shells to shorten the
# visible stem without shifting the approved body silhouette.
neck_bottom=TORSO_CENTER_Z+TORSO_HEIGHT/2+.010
neck_top=HEAD_CENTER_Z-HEAD_SIZE[2]/2+.030
neck_stem("Neck_Link",neck_bottom,neck_top,NECK_RADIUS,frame,breath)

shoulders={};elbows={}
for side,s in (("L",-1),("R",1)):
    shoulder_z=1.245+UPPER_SHIFT+SHOULDER_Z_OFFSET
    shoulder_joint_y=-.02+JOINT_BACK_SHIFT
    shoulder=pivot("Shoulder_"+side+"_PitchX",(s*SHOULDER_AXIS_X,shoulder_joint_y,shoulder_z),breath)
    spread=pivot("Shoulder_"+side+"_AbductY",(s*SHOULDER_AXIS_X,shoulder_joint_y,shoulder_z),shoulder)
    # Set abduction before parenting the chain so authored positions
    # follow the real shoulder frame and its cylinder pivot.
    spread.rotation_euler[1]=radians(-s*ARM_SPLAY)
    can_axis_joint("Shoulder_"+side,
                   (s*SHOULDER_AXIS_X,shoulder_joint_y,shoulder_z),
                   SHOULDER_JOINT_RADIUS,SHOULDER_JOINT_DEPTH,spread,breath,armor,frame)
    link_between("Shoulder_Connector_Short_"+side,
                 (s*SHOULDER_BODY_EDGE_X,shoulder_joint_y,shoulder_z),
                 (s*SHOULDER_AXIS_X,shoulder_joint_y,shoulder_z),
                 .018,frame,breath)
    if SHOULDER_PROFILE!="NONE":
        shoulder_shell=shoulder_pad("Shoulder_Armor_"+side,
                                    (s*(SHOULDER_AXIS_X+SHOULDER_SIZE[0]/2),-CORE_SIZE[1]*.29,shoulder_z-.010),
                                    SHOULDER_SIZE,armor,spread,s,SHOULDER_PROFILE)
        shoulder_shell.rotation_euler[1]=radians(s*SHOULDER_TILT)
    upper_arm_z=shoulder_z-cos(radians(ARM_SPLAY))*UPPER_ARM_CENTER_DROP
    elbow_z=shoulder_z-cos(radians(ARM_SPLAY))*ELBOW_DROP
    forearm_z=elbow_z-FOREARM_DROP
    arm_joint_y=shoulder_joint_y
    upper_arm=limb_shell("UpperArm_"+side,(s*ARM_UPPER_X,arm_joint_y,upper_arm_z),UPPER_ARM_SIZE,frame,spread,"CHISELED")
    par_local(upper_arm,spread,(0,0,-UPPER_ARM_CENTER_DROP))
    elbow=pivot("Elbow_"+side+"_PitchX",(s*ELBOW_X,arm_joint_y,elbow_z),spread)
    par_local(elbow,spread,(0,0,-ELBOW_DROP))
    elbow_joint=ring("Elbow_Joint_"+side,(s*ELBOW_X,arm_joint_y,elbow_z),.038,.055,frame,elbow,(0,radians(90),0))
    par_local(elbow_joint,elbow,(0,0,0),(0,radians(90),0))
    if FOREARM_FORM=="FACETED":
        forearm=limb_shell("ForearmGun_"+side,(s*FOREARM_X,arm_joint_y,forearm_z),FOREARM_SIZE,armor,elbow,LIMB_PROFILE)
    else:
        forearm=round_cannon_shell("ForearmGun_"+side,(s*FOREARM_X,arm_joint_y,forearm_z),FOREARM_SIZE,armor,elbow,FOREARM_FORM)
    par_local(forearm,elbow,(0,0,-FOREARM_DROP))
    accent_panel=profile_shell("Forearm_Accent_Panel_"+side,
                               (s*FOREARM_X,arm_joint_y,forearm_z),(.090,.006,.060),orange,elbow,"CHEST_CROWN")
    par_local(accent_panel,elbow,(0,-FOREARM_SIZE[1]*.41,-FOREARM_DROP+FOREARM_SIZE[2]*.11))
    if MUZZLE_ENABLED:
        # Small, flush barrel glass at the growing egg-shaped distal end.
        muzzle_y=-FOREARM_SIZE[1]*.5*.62-.004-.003
        muzzle_z=-FOREARM_DROP-FOREARM_SIZE[2]*.43
        muzzle=ring("Muzzle_Glass_"+side,(s*FOREARM_X,muzzle_y,muzzle_z),MUZZLE_RADIUS*.24,.002,cyan,elbow,(radians(90),0,0),.0005)
        par_local(muzzle,elbow,(0,muzzle_y,muzzle_z),(radians(90),0,0))
    spread.rotation_euler[1]=radians(-s*ARM_SPLAY)
    shoulders[side]=shoulder;elbows[side]=elbow

for side,s in (("L",-1),("R",1)):
    knee_z=HIP_Z-cos(radians(LEG_SPLAY))*.280*LEG_SCALE
    ankle_z=HIP_Z-cos(radians(LEG_SPLAY))*.570*LEG_SCALE
    thigh_x=s*(.11+sin(radians(LEG_SPLAY))*.145*LEG_SCALE)
    knee_x=s*(.11+sin(radians(LEG_SPLAY))*.280*LEG_SCALE)
    shin_x=s*(.11+sin(radians(LEG_SPLAY))*.435*LEG_SCALE)
    ankle_x=s*(.11+sin(radians(LEG_SPLAY))*.570*LEG_SCALE)
    foot_x=s*(.11+sin(radians(LEG_SPLAY))*.620*LEG_SCALE)
    hip_joint_y=JOINT_BACK_SHIFT
    hip_pitch=pivot("Hip_"+side+"_PitchX",(s*.11,hip_joint_y,HIP_Z),pelvis)
    hip=pivot("Hip_"+side+"_AbductY",(s*.11,hip_joint_y,HIP_Z),hip_pitch)
    hip.rotation_euler[1]=radians(-s*LEG_SPLAY)
    can_axis_joint("Hip_"+side,(s*.11,hip_joint_y,HIP_Z),HIP_JOINT_RADIUS,HIP_JOINT_DEPTH,hip,pelvis,armor,frame)
    thigh=limb_shell("Thigh_"+side,(thigh_x,JOINT_BACK_SHIFT,HIP_Z-cos(radians(LEG_SPLAY))*.145*LEG_SCALE),(.082,.075,.28*LEG_SCALE),armor,hip,"FLARED_LOWER")
    par_local(thigh,hip,(0,0,-.145*LEG_SCALE))
    knee_joint_y=hip_joint_y
    knee=pivot("Knee_"+side+"_PitchX",(knee_x,knee_joint_y,knee_z),hip)
    par_local(knee,hip,(0,0,-.280*LEG_SCALE))
    knee_joint=ring("KneeJoint_"+side,(knee_x,knee_joint_y,knee_z),.055,.11*LEG_SCALE,frame,knee,(0,radians(90),0))
    par_local(knee_joint,knee,(0,0,0),(0,radians(90),0))
    knee_armor=profile_shell("Knee_Armor_"+side,(knee_x,knee_joint_y-.075,knee_z),(.112,.040,.105*LEG_SCALE),armor,knee,"KNEE_GUARD")
    par_local(knee_armor,knee,(0,-.075,0))
    shin=limb_shell("Shin_"+side,(shin_x,knee_joint_y,knee_z-cos(radians(LEG_SPLAY))*.155*LEG_SCALE),(.12,.14,.265*LEG_SCALE),armor,knee,"CHISELED")
    par_local(shin,knee,(0,0,-.155*LEG_SCALE))
    ankle_joint=ring("AnkleJoint_"+side,(ankle_x,JOINT_BACK_SHIFT,ankle_z),.043,.07*LEG_SCALE,frame,knee,(0,radians(90),0))
    par_local(ankle_joint,knee,(0,0,-.290*LEG_SCALE),(0,radians(90),0))
    ankle=pivot("Ankle_"+side+"_PitchX",(ankle_x,JOINT_BACK_SHIFT,ankle_z),knee)
    par_local(ankle,knee,(0,0,-.290*LEG_SCALE))
    foot=limb_shell("Foot_"+side,(foot_x,FOOT_OFFSET_Y+JOINT_BACK_SHIFT,ankle_z-cos(radians(LEG_SPLAY))*.050*LEG_SCALE),(FOOT_SIZE[0],FOOT_SIZE[1],FOOT_SIZE[2]*LEG_SCALE),armor,ankle,"FLARED_LOWER")
    par_local(foot,ankle,(0,FOOT_OFFSET_Y,-.050*LEG_SCALE))
pivot("Socket_Backpack",(0,.166,1.15+UPPER_SHIFT),breath)
pivot("Socket_LowerBody",(0,0,.81),pelvis)

for side in ("L","R"):
    shoulders[side].rotation_euler[0]=radians(-7)
    elbows[side].rotation_euler[0]=radians(ELBOW_BEND)
if POSE=="AIM":
    for side in ("L","R"):
        shoulders[side].rotation_euler[0]=radians(-62)
        elbows[side].rotation_euler[0]=radians(-18)
elif POSE=="GUARD":
    shoulders["L"].rotation_euler[0]=radians(-32)
    elbows["L"].rotation_euler[0]=radians(-72)
    shoulders["R"].rotation_euler[0]=radians(-55)
    elbows["R"].rotation_euler[0]=radians(-25)
elif POSE=="CROUCH":
    for side in ("L","R"):
        shoulders[side].rotation_euler[0]=radians(-20)
        elbows[side].rotation_euler[0]=radians(-45)
        bpy.data.objects["Hip_"+side+"_PitchX"].rotation_euler[0]=radians(-15)
        bpy.data.objects["Knee_"+side+"_PitchX"].rotation_euler[0]=radians(32)
        bpy.data.objects["Ankle_"+side+"_PitchX"].rotation_euler[0]=radians(-17)
elif POSE!="IDLE":
    raise ValueError("Unsupported WISP articulation pose: "+str(POSE))
if POSE=="IDLE":
    home=breath.location.copy()
    # 49 frames at 24fps = ~2s loop. No neck/waist bending.
    for f,dz,sc,pitch in [(1,0,1,-7),(25,.006,1.006,-6.1),(49,0,1,-7)]:
        breath.location=(home.x,home.y,home.z+dz)
        breath.scale=(1,1,sc)
        breath.keyframe_insert(data_path="location",frame=f)
        breath.keyframe_insert(data_path="scale",frame=f)
        for side in ("L","R"):
            shoulders[side].rotation_euler[0]=radians(pitch)
            shoulders[side].keyframe_insert(data_path="rotation_euler",frame=f)
    bpy.context.scene.frame_start=1;bpy.context.scene.frame_end=49
    bpy.context.scene.render.fps=24;bpy.context.scene.frame_set(1)
else:
    bpy.context.scene.frame_set(1)
bpy.context.view_layer.update()
bpy.ops.object.select_all(action='DESELECT')
root.select_set(True);bpy.context.view_layer.objects.active=root
print("WISP_V2 blockout ready; pose:",POSE)
