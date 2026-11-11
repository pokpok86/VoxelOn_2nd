"""Build only WISP BD-01 body/shoulder unit, with articulated mounts and views.

Units are meters and Z-up. The robot faces -Y. Head, lower-arm weapons, and legs
are deliberately excluded. Shoulder pitch/abduction and waist yaw are authored
as named Blender empty pivots at their actual joint centers.
"""
import bpy
import json
import math
import os
from mathutils import Vector

OUT=os.path.dirname(os.path.abspath(__file__))
bpy.context.preferences.filepaths.save_version=0
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
BODY=bpy.data.collections.new("BD01_BODY_AND_SHOULDERS")
bpy.context.scene.collection.children.link(BODY)
GUIDE=bpy.data.collections.new("BD01_CONNECTION_REVIEW_GUIDES")
bpy.context.scene.collection.children.link(GUIDE)
GUIDE.hide_render=True; GUIDE.hide_viewport=True
STUDIO=bpy.data.collections.new("BD01_REVIEW_STUDIO")
bpy.context.scene.collection.children.link(STUDIO)

def material(name,color,rough=.48,metal=.28,emission=0):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*color,1)
    bs.inputs["Roughness"].default_value=rough; bs.inputs["Metallic"].default_value=metal
    if not emission:
        noise=m.node_tree.nodes.new('ShaderNodeTexNoise'); noise.inputs['Scale'].default_value=230
        bump=m.node_tree.nodes.new('ShaderNodeBump'); bump.inputs['Strength'].default_value=.16; bump.inputs['Distance'].default_value=.0007
        m.node_tree.links.new(noise.outputs['Fac'],bump.inputs['Height']); m.node_tree.links.new(bump.outputs['Normal'],bs.inputs['Normal'])
    if emission:
        bs.inputs["Emission Color"].default_value=(*color,1)
        bs.inputs["Emission Strength"].default_value=emission
    return m

ARMOR=material("BD01 | light gray main armor",(.52,.53,.55),.44,.28)
ARMOR_SHADE=material("BD01 | shaded edge facets",(.33,.35,.38),.48,.30)
GRAPHITE=material("BD01 | graphite chassis",(.04,.05,.065),.55,.40)
GUNMETAL=material("BD01 | shoulder and waist pivots",(.10,.12,.15),.43,.52)
JOINT_FACE=material("BD01 | joint face",(.19,.21,.24),.45,.48)
ORANGE=material("BD01 | single small accent",(.92,.28,.035),.42,.20)
AXIS_X=material("Guide | pitch X",(1.0,.20,.055),.3,0,2.5)
AXIS_Y=material("Guide | spread Y",(.06,.62,1.0),.3,0,2.5)
AXIS_Z=material("Guide | waist yaw Z",(.30,1.0,.35),.3,0,2.5)
PIVOT=material("Guide | centers",(1.0,.82,.07),.3,0,2.5)

def put(obj,collection=BODY,mat=None):
    for c in list(obj.users_collection): c.objects.unlink(obj)
    collection.objects.link(obj)
    if mat: obj.data.materials.clear(); obj.data.materials.append(mat)
    return obj

def edge_break(obj,amount=.004):
    b=obj.modifiers.new("small machined bevels","BEVEL"); b.width=amount; b.segments=1
    obj.modifiers.new("weighted face normals","WEIGHTED_NORMAL")
    return obj

def parent_keep(child,parent):
    bpy.context.view_layer.update(); world=child.matrix_world.copy()
    child.parent=parent; child.matrix_world=world

def empty(name,loc,parent=None):
    o=bpy.data.objects.new(name,None); BODY.objects.link(o); o.location=loc
    o.empty_display_type="ARROWS"; o.empty_display_size=.04
    if parent: parent_keep(o,parent)
    return o

def cube(name,loc,dim,mat,bevel=.004):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=put(bpy.context.object,BODY,mat); o.name=name; o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return edge_break(o,bevel)

def cylinder(name,loc,radius,depth,axis,mat,vertices=32):
    rot={"X":(0,math.pi/2,0),"Y":(math.pi/2,0,0),"Z":(0,0,0)}[axis]
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,
                                        location=loc,rotation=rot)
    o=put(bpy.context.object,BODY,mat); o.name=name
    return edge_break(o,.0025)

def prism_xz(name,outline,y_center,depth,mat,bevel=.003):
    n=len(outline)
    verts=[(x,y,z) for y in (y_center-depth/2,y_center+depth/2) for x,z in outline]
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
    for i in range(n):
        j=(i+1)%n; faces.append((i,j,n+j,n+i))
    me=bpy.data.meshes.new(name+"_facets"); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); BODY.objects.link(o); o.data.materials.append(mat)
    return edge_break(o,bevel)

def clipped_rect(hx,hy,cut=.22):
    cx,cy=hx*cut,hy*cut
    return [(-hx+cx,-hy),(hx-cx,-hy),(hx,-hy+cy),(hx,hy-cy),
            (hx-cx,hy),(-hx+cx,hy),(-hx,hy-cy),(-hx,-hy+cy)]

def loft_z(name,sections,mat):
    n=8; verts=[]
    for z,hx,hy,cy,cut in sections:
        verts.extend((x,y+cy,z) for x,y in clipped_rect(hx,hy,cut))
    faces=[tuple(range(n-1,-1,-1))]
    for k in range(len(sections)-1):
        a,b=k*n,(k+1)*n
        for i in range(n):
            j=(i+1)%n; faces.append((a+i,a+j,b+j,b+i))
    faces.append(tuple(range((len(sections)-1)*n,len(sections)*n)))
    me=bpy.data.meshes.new(name+"_loft"); me.from_pydata(verts,[],faces); me.update()
    o=bpy.data.objects.new(name,me); BODY.objects.link(o); o.data.materials.append(mat)
    return edge_break(o,.004)

def mirror(poly,s):
    return [(s*x,z) for x,z in poly]

ROOT=empty("BD01_ROOT",(0,0,0))
PARTS=[]
PIVOTS={}

# Compact, wide chest chassis with one broad continuous front armor surface.
core=loft_z("Torso_Core",[
    (-.126,.075,.065,.015,.24),(-.070,.105,.080,.012,.22),
    (.030,.174,.090,.014,.22),(.105,.175,.088,.014,.24)],GRAPHITE)
PARTS.append((core,ROOT))
chest=prism_xz("Chest_Armor_Main",[
    (-.180,.070),(-.150,.105),(.150,.105),(.180,.070),
    (.176,-.037),(.112,-.087),(.072,-.063),(.072,-.004),
    (-.072,-.004),(-.072,-.063),(-.112,-.087),(-.176,-.037)],-.105,.044,ARMOR,.003)
PARTS.append((chest,ROOT))
# A single small central accent plate; no extra vents, pipes, or panel grid.
accent=prism_xz("Chest_Center_Point_Armor",[
    (-.071,.082),(.071,.082),(.071,.035),(-.071,.035)],-.131,.007,ARMOR,.0015)
PARTS.append((accent,ROOT))
orange_point=prism_xz("Chest_Point_Orange_Inset",[
    (-.152,.065),(-.101,.054),(-.102,.020),(-.145,.030)],-.130,.004,ORANGE,.001)
PARTS.append((orange_point,ROOT))
for s in (-1,1):
    cheek=prism_xz(f"Chest_Lower_Chamfer_{s}",mirror([(.074,-.006),(.112,-.012),(.147,-.042),(.109,-.075),(.074,-.058)],s),-.116,.025,ARMOR_SHADE,.002)
    PARTS.append((cheek,ROOT))
belly=prism_xz("Recessed_Abdominal_Frame",[(-.062,-.003),(.062,-.003),(.052,-.092),(-.052,-.092)],-.067,.034,GUNMETAL,.003)
belly_inset=cube("Abdominal_Recess",(0,-.087,-.028),(.072,.009,.023),GRAPHITE,.001)
PARTS.extend([(belly,ROOT),(belly_inset,ROOT)])
# Rear panel is plain and recessed, matching the reference's one center hatch.
back=cube("Back_Core_Plate",(0,.116,-.005),(.265,.014,.145),ARMOR_SHADE,.008)
PARTS.append((back,ROOT))
back_recess=cube("Back_Center_Recess",(0,.126,.008),(.112,.010,.060),GRAPHITE,.006)
PARTS.append((back_recess,ROOT))

# Small fixed neck socket; no head and no neck articulation.
neck=cube("Neck_Fixed_Mount",(0,0,.132),(.086,.072,.026),GRAPHITE,.004)
neck_top=cube("Neck_Head_Interface",(0,0,.151),(.062,.058,.014),GUNMETAL,.003)
PARTS.extend([(neck,ROOT),(neck_top,ROOT)])

# Lower waist yaw housing and a short pelvis interface, centered on Z.
waist_z=-.132
waist_housing=cylinder("Waist_Yaw_Housing",(0,0,waist_z),.054,.040,"Z",GRAPHITE,32)
waist_ring=cylinder("Waist_Yaw_Ring",(0,0,waist_z-.020),.045,.008,"Z",GUNMETAL,32)
waist_pivot=empty("BD01_WAIST_YAW_Z",(0,0,waist_z),ROOT)
waist_pivot["rotation_axis"]="Z"; waist_pivot["motion"]="left/right waist yaw only"
waist_plug=cylinder("Waist_Rotating_Pelvis_Plug",(0,0,-.172),.039,.040,"Z",GUNMETAL,8)
waist_key=cube("Waist_Pelvis_Interface_Key",(0,0,-.197),(.070,.060,.020),GRAPHITE,.003)
PARTS.extend([(waist_housing,ROOT),(waist_ring,ROOT),(waist_plug,waist_pivot),(waist_key,waist_pivot)])
PIVOTS["waist"]=(0,0,waist_z)

# Two mirrored shoulder gimbals. Armor caps are fixed to the torso; the short
# upper-arm connector stubs follow the nested shoulder articulation pivots.
for side,s in (("L",-1),("R",1)):
    sx=s*.238; sz=.032
    spread=empty(f"BD01_{side}_SHOULDER_ABDUCTION_Y",(sx,0,sz),ROOT)
    spread["rotation_axis"]="Y"; spread["motion"]="slight outward arm spread"
    pitch=empty(f"BD01_{side}_SHOULDER_PITCH_X",(sx,0,sz),spread)
    pitch["rotation_axis"]="X"; pitch["motion"]="front/back arm swing"
    PIVOTS[side]=(sx,0,sz)
    # Main transverse can and its compact front-facing center face.
    can=cylinder(f"{side}_Shoulder_Can_X",(sx,0,sz),.052,.078,"X",GRAPHITE,32)
    cap=cylinder(f"{side}_Shoulder_Can_Face",(sx,-.006,sz),.038,.082,"Y",GUNMETAL,32)
    inset=cylinder(f"{side}_Shoulder_Cap_Inset",(sx,-.049,sz),.023,.006,"Y",JOINT_FACE,24)
    PARTS.extend([(can,ROOT),(cap,ROOT),(inset,ROOT)])
    # Small polygonal armor over the outside shoulder, not a large shoulder block.
    outline=mirror([(.250,.119),(.310,.132),(.350,.102),(.362,.012),(.326,-.030),(.266,-.012)],s)
    armor=prism_xz(f"{side}_Shoulder_Armor",outline,-.010,.166,ARMOR,.003)
    PARTS.append((armor,ROOT))
    from mathutils import Matrix
    cap_center=Vector((s*.302,0,.050))
    slope=Matrix.Rotation(-s*.20,4,'Y')
    for vertex in armor.data.vertices:
        vertex.co=cap_center+slope.to_3x3() @ (vertex.co-cap_center)
    # Upper-arm connector is a replaceable short socket, not an arm model.
    connector=cube(f"{side}_UpperArm_Connector_Stub",(s*.282,.000,-.064),(.060,.065,.096),GRAPHITE,.003)
    PARTS.append((connector,pitch))
    connector_cap=cylinder(f"{side}_UpperArm_Connector_Pin",(s*.282,.000,-.108),.024,.062,"X",GUNMETAL,24)
    PARTS.append((connector_cap,pitch))

for obj,parent in PARTS:
    parent_keep(obj,parent)

# Verify motion transforms, then restore the zero pose before renders and save.
def world_center(obj):
    return obj.matrix_world.translation.copy()

checks={}
for side,s in (("L",-1),("R",1)):
    spread=bpy.data.objects[f"BD01_{side}_SHOULDER_ABDUCTION_Y"]
    pitch=bpy.data.objects[f"BD01_{side}_SHOULDER_PITCH_X"]
    stub=bpy.data.objects[f"{side}_UpperArm_Connector_Stub"]
    base=world_center(stub)
    pitch.rotation_euler.x=-.18; bpy.context.view_layer.update()
    pitch_delta=(world_center(stub)-base).length
    pitch.rotation_euler.x=0; bpy.context.view_layer.update()
    spread.rotation_euler.y=-s*.12; bpy.context.view_layer.update()
    spread_delta=(world_center(stub).x-base.x)*s
    spread.rotation_euler.y=0; bpy.context.view_layer.update()
    assert pitch_delta>.005, f"{side} shoulder pitch does not move connector"
    assert spread_delta>.002, f"{side} shoulder abduction does not spread outward"
    checks[side]={"pitch_moves_connector":True,"abduction_moves_connector_outward":True}
waist_before=world_center(waist_key)
waist_axis_before=waist_key.matrix_world.to_3x3() @ Vector((1,0,0))
waist_pivot.rotation_euler.z=.25; bpy.context.view_layer.update()
waist_after=world_center(waist_key)
waist_axis_after=waist_key.matrix_world.to_3x3() @ Vector((1,0,0))
waist_delta=(waist_axis_after-waist_axis_before).length
waist_pivot.rotation_euler.z=0; bpy.context.view_layer.update()
assert (waist_after-waist_before).length<.001,"waist yaw should rotate in place, not translate"
assert waist_delta>.05,"waist yaw pivot does not rotate pelvis interface orientation"
checks["waist"]={"yaw_rotates_pelvis_interface":True}

# Review-only arrows are hidden in the clean editable viewport and normal renders.
def emission(name,color): return material(name,color,.3,0,2.5)
GUIDE_X=emission("Guide X shoulder pitch",(1.0,.18,.04))
GUIDE_Y=emission("Guide Y shoulder spread",(.04,.55,1.0))
GUIDE_Z=emission("Guide Z waist yaw",(.16,1.0,.22))
GUIDE_P=emission("Guide pivot center",(1.0,.78,.03))

def segment(name,a,b,r,mat):
    a,b=Vector(a),Vector(b); v=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=r,depth=v.length,location=(a+b)/2)
    o=put(bpy.context.object,GUIDE,mat); o.rotation_euler=v.to_track_quat("Z","Y").to_euler(); return o

def cone(name,p,d,r,depth,mat):
    bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=r,radius2=0,depth=depth,location=p)
    o=put(bpy.context.object,GUIDE,mat); o.rotation_euler=Vector(d).to_track_quat("Z","Y").to_euler(); return o

def axis_pair(name,c,d,half,mat):
    c=Vector(c); d=Vector(d).normalized(); a,b=c-d*half,c+d*half
    segment(name+"_line",a,b,.0023,mat); cone(name+"_a",a,-d,.007,.016,mat); cone(name+"_b",b,d,.007,.016,mat)

for side,s in (("L",-1),("R",1)):
    c=(s*.238,0,.032)
    axis_pair(side+"_SHOULDER_PITCH_X",c,(1,0,0),.096,GUIDE_X)
    axis_pair(side+"_SHOULDER_SPREAD_Y",c,(0,1,0),.091,GUIDE_Y)
axis_pair("WAIST_YAW_Z",(0,0,waist_z),(0,0,1),.090,GUIDE_Z)

world=bpy.data.worlds.new("BD01 studio world"); world.use_nodes=True
world.node_tree.nodes.get("Background").inputs["Color"].default_value=(.012,.018,.028,1)
world.node_tree.nodes.get("Background").inputs["Strength"].default_value=.30
scene=bpy.context.scene; scene.world=world; scene.render.engine="BLENDER_EEVEE"
scene.eevee.taa_render_samples=96
scene.render.resolution_x=1100; scene.render.resolution_y=1100; scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"; scene.render.image_settings.color_mode="RGBA"
scene.view_settings.view_transform="AgX"; scene.render.film_transparent=False

def area(name,loc,power,size,color):
    data=bpy.data.lights.new(name,"AREA"); data.energy=power; data.shape="DISK"; data.size=size; data.color=color
    o=bpy.data.objects.new(name,data); STUDIO.objects.link(o); o.location=loc
    o.rotation_euler=(Vector((0,0,0))-o.location).to_track_quat("-Z","Y").to_euler()

area("soft key",(-1.0,-1.5,1.1),125,1.1,(.78,.86,1.0))
area("front fill",(1.2,-1.4,.25),58,1.0,(1.0,.84,.70))
area("rear rim",(.7,1.1,.9),135,1.0,(.58,.77,1.0))

def camera(name,loc,target=(0,0,0),ortho=.82):
    d=bpy.data.cameras.new(name); d.type="ORTHO"; d.ortho_scale=ortho
    o=bpy.data.objects.new(name,d); STUDIO.objects.link(o); o.location=loc
    o.rotation_euler=(Vector(target)-o.location).to_track_quat("-Z","Y").to_euler(); return o

views=[
    ("BD01-01-Front.png",(0,-2.2,0)),("BD01-02-Right.png",(2.2,0,0)),
    ("BD01-03-Back.png",(0,2.2,0)),("BD01-04-Left.png",(-2.2,0,0)),
    ("BD01-05-45deg.png",(1.6,-1.6,.40)),
]
for filename,loc in views:
    scene.camera=camera("Camera_"+filename[:-4],loc)
    scene.render.filepath=os.path.join(OUT,filename); bpy.ops.render.render(write_still=True)

# Enlarged attachment close-up shows the shoulder gimbals and waist yaw plug.
scene.camera=camera("Camera_Connections_Closeup",(1.4,-1.7,.62),(0,0,-.005),.58)
GUIDE.hide_render=True; GUIDE.hide_viewport=True
scene.render.filepath=os.path.join(OUT,"BD01-06-Connections-Closeup.png")
bpy.ops.render.render(write_still=True)

# A second connector view with colored axes, distinct from the plain close-up.
scene.camera=camera("Camera_Connection_Axes",(1.4,-1.7,.62),(0,0,-.005),.58)
GUIDE.hide_render=False; GUIDE.hide_viewport=False
scene.render.filepath=os.path.join(OUT,"BD01-07-Connection-Axes-raw.png")
bpy.ops.render.render(write_still=True)
axis_pixels=[]
for key,c in [("shoulder_L",(-.238,0,.032)),("shoulder_R",(.238,0,.032)),("waist",(0,0,waist_z))]:
    from bpy_extras.object_utils import world_to_camera_view
    p=world_to_camera_view(scene,scene.camera,Vector(c))
    axis_pixels.append({"joint":key,"x":round(p.x*scene.render.resolution_x),
                        "y":round((1-p.y)*scene.render.resolution_y)})
GUIDE.hide_render=True; GUIDE.hide_viewport=True

scene.camera=next(o for o in STUDIO.objects if o.name=="Camera_BD01-05-45deg")
scene.render.filepath=os.path.join(OUT,"BD01-05-45deg.png")
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,"BD01.blend"))

mesh_objects=[o for o in BODY.objects if o.type=="MESH"]
coords=[o.matrix_world @ Vector(c) for o in mesh_objects for c in o.bound_box]
bounds={"width_x":round(max(v.x for v in coords)-min(v.x for v in coords),3),
        "depth_y":round(max(v.y for v in coords)-min(v.y for v in coords),3),
        "height_z":round(max(v.z for v in coords)-min(v.z for v in coords),3)}
manifest={
    "asset":"WISP BD-01 compact body, shoulders, and upper-arm connector stubs",
    "blender":bpy.app.version_string,"normalized_wisp_height_m":1.0,
    "measured_bounds_m":bounds,
    "main_torso_core_m":{"width_x":0.350,"depth_y":0.180,"height_z":0.231},
    "contains_head":False,"contains_forearm_weapon":False,"contains_legs":False,
    "neck":"fixed compact mount, no neck pivot",
    "joint_centers_m":{"shoulder_L":[-.238,0,.032],"shoulder_R":[.238,0,.032],"waist_yaw":[0,0,waist_z]},
    "axes":{"shoulder_pitch":"X front/back arm swing","shoulder_abduction":"Y slight lateral spread","waist":"Z left/right yaw only"},
    "connection_pixels_closeup":axis_pixels,
    "rig_motion_checks":checks,
    "mirrored_shoulders":True,
    "renders":[f for f,_ in views]+["BD01-06-Connections-Closeup.png","BD01-07-Connection-Axes.png"],
    "review_files":["BD01-Concept-Comparison.png","BD01-Review-Phone.png"],
    "references":["BD01-Design-Guide.jpg","WISP-Reference-Concept.png"],
    "automation":"PASS","design_review":"PENDING_PD_VISUAL_REVIEW",
    "note":"Concept refinement: open-bottom chest armor, recessed tapered abdomen, angled shoulder wraps and downward upper-arm sockets. Design approval remains pending."
}
with open(os.path.join(OUT,"manifest.json"),"w",encoding="utf-8") as f:
    json.dump(manifest,f,ensure_ascii=False,indent=2)
print("BD01_GENERATION_PASS",bpy.app.version_string,"meshes",len(mesh_objects),"bounds",bounds)
