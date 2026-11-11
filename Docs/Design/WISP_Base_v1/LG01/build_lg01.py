"""Build only the mirrored WISP LG-01 biped leg pair and review renders.

Coordinates use meters, Z up, forward along -Y. The pelvis is deliberately not
included. A gimbal at each hip provides X-axis pitch and Y-axis abduction; knee
and ankle pivots provide X-axis sagittal rotation.
"""
import bpy
from bpy_extras.object_utils import world_to_camera_view
import json
import math
import os
from mathutils import Vector

OUT = os.path.dirname(os.path.abspath(__file__))
# Start from an empty scene so the default Blender cube cannot contaminate output.
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
MODEL = bpy.data.collections.new("LG01_LEG_PAIR")
bpy.context.scene.collection.children.link(MODEL)
GUIDE = bpy.data.collections.new("LG01_JOINT_AXIS_GUIDE")
bpy.context.scene.collection.children.link(GUIDE)
GUIDE.hide_render=True
GUIDE.hide_viewport=True
STUDIO = bpy.data.collections.new("LG01_REVIEW_STUDIO")
bpy.context.scene.collection.children.link(STUDIO)

def mat(name, color, roughness=.55, metallic=0.0, emission=None):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*color, 1)
    p.inputs["Roughness"].default_value = roughness
    p.inputs["Metallic"].default_value = metallic
    if emission:
        p.inputs["Emission Color"].default_value = (*color, 1)
        p.inputs["Emission Strength"].default_value = emission
    return m

ARMOR = mat("LG01 | warm light gray armor", (.53,.54,.56), .42, .30)
ARMOR_SHADE = mat("LG01 | armor bevel shade", (.30,.32,.35), .48, .34)
GRAPHITE = mat("LG01 | graphite structure", (.045,.055,.068), .57, .43)
GUNMETAL = mat("LG01 | joint gunmetal", (.075,.09,.11), .48, .50)
JOINT_FACE = mat("LG01 | joint face", (.095,.11,.13), .50, .38)
ORANGE = mat("LG01 | restrained safety orange", (.96,.31,.045), .4, .18)
CYAN = mat("LG01 | small cyan status light", (.10,.83,.92), .26, .12, 2.2)
AXIS_PITCH = mat("Guide | pitch axis orange", (1.0,.20,.055), .3, 0, 3.0)
AXIS_ABDUCT = mat("Guide | hip abduction cyan", (.06,.62,1.0), .3, 0, 3.0)

def put(obj, collection=MODEL, material=None):
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    collection.objects.link(obj)
    if material:
        obj.data.materials.clear()
        obj.data.materials.append(material)
    return obj

def bevel(obj, amount=.003):
    mod = obj.modifiers.new("small edge breaks", "BEVEL")
    mod.width = amount
    mod.segments = 1
    obj.modifiers.new("weighted corner normals", "WEIGHTED_NORMAL")
    return obj

def make_empty(name, loc, parent=None):
    o = bpy.data.objects.new(name, None)
    MODEL.objects.link(o)
    o.empty_display_type = "ARROWS"
    o.empty_display_size = .035
    o.location = loc
    if parent:
        keep_parent(o, parent)
    return o

def keep_parent(child, parent):
    bpy.context.view_layer.update()
    world = child.matrix_world.copy()
    child.parent = parent
    child.matrix_world = world

def box(name, loc, dims, material, bevel_amt=.003):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    o = put(bpy.context.object, MODEL, material)
    o.dimensions = dims
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return bevel(o, bevel_amt)

def cylinder_axis(name, loc, radius, depth, axis, material, verts=24):
    rot = {"X": (0,math.pi/2,0), "Y": (math.pi/2,0,0), "Z": (0,0,0)}[axis]
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth,
                                        location=loc, rotation=rot)
    return bevel(put(bpy.context.object, MODEL, material), .002)

def clipped_rect(hx, hy, cut=.25):
    cx, cy = hx*cut, hy*cut
    return [(-hx+cx,-hy),(hx-cx,-hy),(hx,-hy+cy),(hx,hy-cy),
            (hx-cx,hy),(-hx+cx,hy),(-hx,hy-cy),(-hx,-hy+cy)]

def loft_z(name, sections, material, x_center=0.0):
    """Clipped-rectangle armor loft; section=(z,hx,hy,center_y,corner_cut)."""
    n = 8
    verts = []
    for z,hx,hy,cy,cut in sections:
        verts.extend((x+x_center,y+cy,z) for x,y in clipped_rect(hx,hy,cut))
    faces = [tuple(range(n-1,-1,-1))]
    for k in range(len(sections)-1):
        a,b = k*n,(k+1)*n
        for i in range(n):
            j=(i+1)%n
            faces.append((a+i,a+j,b+j,b+i))
    faces.append(tuple(range((len(sections)-1)*n,len(sections)*n)))
    mesh=bpy.data.meshes.new(name+"_faceted_mesh")
    mesh.from_pydata(verts,[],faces); mesh.update()
    obj=bpy.data.objects.new(name,mesh); MODEL.objects.link(obj)
    obj.data.materials.append(material)
    return bevel(obj,.0025)

def prism_yz(name, x_center, thickness, outline, material, bevel_amt=.0015):
    n=len(outline)
    verts=[(x,y,z) for x in (x_center-thickness/2,x_center+thickness/2) for y,z in outline]
    faces=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]
    for i in range(n):
        j=(i+1)%n; faces.append((i,j,n+j,n+i))
    me=bpy.data.meshes.new(name+"_mesh"); me.from_pydata(verts,[],faces); me.update()
    ob=bpy.data.objects.new(name,me); MODEL.objects.link(ob); ob.data.materials.append(material)
    return bevel(ob,bevel_amt)

def parent_at(obj,parent):
    keep_parent(obj,parent)
    return obj

# WISP height is normalized to 1 m. The leg pair occupies about the lower 46%,
# keeping the legs visually subordinate to the compact head and torso.
HIP_Z, KNEE_Z, ANKLE_Z = .455, .254, .066
HIP_X = .092
ROOT=make_empty("LG01_PAIR_ROOT",(0,0,0))
PIVOTS={}
PARTS=[]

for side, s in (("L",-1),("R",1)):
    hx=s*HIP_X
    # Hip gimbal: two perpendicular, co-centered axes. X is sagittal pitch;
    # Y is lateral splay. The daughter X pivot carries the leg below it.
    hip_y=make_empty(f"LG01_{side}_HIP_ABDUCTION_Y",(hx,0,HIP_Z),ROOT)
    hip_x=make_empty(f"LG01_{side}_HIP_PITCH_X",(hx,0,HIP_Z),hip_y)
    hip_y["rotation_axis"]="Y"
    hip_y["motion"]="lateral hip spread"
    hip_y["outward_splay_sign"]=-s
    hip_x["rotation_axis"]="X"
    hip_x["motion"]="front/back hip pitch"
    PIVOTS[(side,"hip")]=(hx,0,HIP_Z)
    # compact transverse gimbal drums (main axis housings)
    hip_outer=cylinder_axis(f"{side}_Hip_Can_X",(hx,0,HIP_Z),.047,.082,"X",GRAPHITE,32)
    hip_cap=cylinder_axis(f"{side}_Hip_Can_Face",(hx,-.002,HIP_Z),.038,.084,"Y",GUNMETAL,32)
    hip_face=cylinder_axis(f"{side}_Hip_Cap_Inset",(hx,-.047,HIP_Z),.025,.006,"Y",JOINT_FACE,24)
    PARTS.extend([(hip_outer,ROOT),(hip_cap,ROOT),(hip_face,ROOT)])
    # Hip cross-axis stub remains at the gimbal pivot; its end stops are small.
    hip_pin=cylinder_axis(f"{side}_Hip_Abduction_Pin",(hx,0,HIP_Z),.027,.092,"Y",GUNMETAL,24)
    PARTS.append((hip_pin,ROOT))

    # Upper mechanical link is narrow and mostly recessed behind the armor.
    upper_link=loft_z(f"{side}_Thigh_Core",[
        (.273,.026,.030,.010,.22),(.292,.030,.034,.005,.22),
        (.414,.031,.035,.000,.22),(.438,.027,.031,.000,.22)],GRAPHITE,x_center=hx)
    PARTS.append((upper_link,hip_x))
    # Slim angular thigh plate, subtly tapered at both ends.
    thigh=loft_z(f"{side}_Thigh_Armor",[
        (.286,.034,.044,-.012,.28),(.300,.044,.054,-.012,.27),
        (.397,.041,.052,-.008,.25),(.420,.031,.043,-.004,.28)],ARMOR,x_center=hx)
    PARTS.append((thigh,hip_x))
    # Small side inset, mirrored exactly; no bulky thigh fins.
    thigh_side=prism_yz(f"{side}_Thigh_Safety_Inset",s*.044,.003,
        [(-.052,.331),(-.048,.321),(-.018,.329),(-.020,.344)],ORANGE)
    PARTS.append((thigh_side,hip_x))

    # Knee pivot at the meeting point; normal humanoid, forward-bending leg.
    knee=make_empty(f"LG01_{side}_KNEE_PITCH_X",(hx,-.004,KNEE_Z),hip_x)
    knee["rotation_axis"]="X"
    knee["motion"]="forward human-knee bend"
    PIVOTS[(side,"knee")]=(hx,-.004,KNEE_Z)
    knee_shell=cylinder_axis(f"{side}_Knee_Can_X",(hx,-.004,KNEE_Z),.040,.076,"X",GRAPHITE,32)
    knee_cap=cylinder_axis(f"{side}_Knee_Can_Face",(hx,-.005,KNEE_Z),.031,.078,"Y",GUNMETAL,24)
    knee_inset=cylinder_axis(f"{side}_Knee_Inset",(hx,-.047,KNEE_Z),.021,.006,"Y",JOINT_FACE,24)
    PARTS.extend([(knee_shell,hip_x),(knee_cap,hip_x),(knee_inset,hip_x)])
    # A small faceted knee guard sits over the top of the cylindrical hinge.
    knee_guard=loft_z(f"{side}_Knee_Guard",[
        (.256,.030,.040,-.035,.28),(.264,.037,.044,-.035,.26),
        (.286,.031,.040,-.032,.28)],ARMOR_SHADE,x_center=hx)
    PARTS.append((knee_guard,hip_x))

    # Shin armor overlaps the knee slightly and tapers to the ankle.
    lower_link=loft_z(f"{side}_Shin_Core",[
        (.085,.022,.025,.009,.22),(.104,.026,.029,.006,.22),
        (.216,.029,.033,.000,.22),(.238,.026,.030,.000,.22)],GRAPHITE,x_center=hx)
    PARTS.append((lower_link,knee))
    shin=loft_z(f"{side}_Shin_Armor",[
        (.093,.031,.040,-.012,.28),(.108,.039,.047,-.011,.26),
        (.208,.036,.046,-.008,.26),(.228,.029,.039,-.004,.28)],ARMOR,x_center=hx)
    PARTS.append((shin,knee))
    shin_side=prism_yz(f"{side}_Shin_Safety_Inset",s*.037,.003,
        [(-.045,.129),(-.041,.120),(-.010,.128),(-.013,.143)],ORANGE)
    PARTS.append((shin_side,knee))

    # Ankle pivot and small two-part mechanical foot; foot stays short and low.
    ankle=make_empty(f"LG01_{side}_ANKLE_PITCH_X",(hx,-.004,ANKLE_Z),knee)
    ankle["rotation_axis"]="X"
    ankle["motion"]="front/back ankle pitch"
    PIVOTS[(side,"ankle")]=(hx,-.004,ANKLE_Z)
    ankle_shell=cylinder_axis(f"{side}_Ankle_Can_X",(hx,-.004,ANKLE_Z),.030,.066,"X",GRAPHITE,28)
    ankle_cap=cylinder_axis(f"{side}_Ankle_Can_Face",(hx,-.005,ANKLE_Z),.022,.068,"Y",GUNMETAL,24)
    ankle_inset=cylinder_axis(f"{side}_Ankle_Inset",(hx,-.042,ANKLE_Z),.014,.006,"Y",JOINT_FACE,20)
    PARTS.extend([(ankle_shell,knee),(ankle_cap,knee),(ankle_inset,knee)])
    foot=prism_yz(f"{side}_Angular_Foot",hx,.108,
        [(.055,.017),(.047,.062),(.025,.078),(-.060,.074),(-.100,.052),(-.106,.020),(-.080,.010),(.043,.010)],ARMOR)
    PARTS.append((foot,ankle))
    toe=prism_yz(f"{side}_Toe_Graphite_Sole",hx,.110,
        [(-.107,.020),(-.099,.010),(-.071,.008),(-.068,.023)],GRAPHITE)
    PARTS.append((toe,ankle))
    foot_accent=prism_yz(f"{side}_Foot_Heel_Accent",s*.036,.003,
        [(.018,.022),(.023,.017),(.043,.019),(.039,.026)],ORANGE)
    PARTS.append((foot_accent,ankle))

# Parent each designed part to its articulation while retaining world placement.
for obj,parent in PARTS:
    parent_at(obj,parent)

# Nested pivot hierarchy uses object origins as actual mechanical centers.
# Hip Y abduction -> Hip X flexion -> Knee X flexion -> Ankle X rotation.
def mesh_center(obj):
    corners=[obj.matrix_world @ Vector(v) for v in obj.bound_box]
    return sum(corners,Vector((0,0,0)))/len(corners)

rig_checks={}
for side,s in (("L",-1),("R",1)):
    hip_y=bpy.data.objects[f"LG01_{side}_HIP_ABDUCTION_Y"]
    hip_x=bpy.data.objects[f"LG01_{side}_HIP_PITCH_X"]
    knee=bpy.data.objects[f"LG01_{side}_KNEE_PITCH_X"]
    ankle=bpy.data.objects[f"LG01_{side}_ANKLE_PITCH_X"]
    foot=bpy.data.objects[f"{side}_Angular_Foot"]
    base=mesh_center(foot)
    hip_x.rotation_euler.x=-.18; bpy.context.view_layer.update()
    pitch_delta=(mesh_center(foot)-base).length
    hip_x.rotation_euler.x=0; bpy.context.view_layer.update()
    hip_y.rotation_euler.y=-s*.18; bpy.context.view_layer.update()
    splay_delta=(mesh_center(foot).x-base.x)*s
    hip_y.rotation_euler.y=0; bpy.context.view_layer.update()
    knee.rotation_euler.x=-.18; bpy.context.view_layer.update()
    knee_delta_y=mesh_center(foot).y-base.y
    knee.rotation_euler.x=0; bpy.context.view_layer.update()
    ankle.rotation_euler.x=.18; bpy.context.view_layer.update()
    ankle_delta=(mesh_center(foot)-base).length
    ankle.rotation_euler.x=0; bpy.context.view_layer.update()
    assert pitch_delta>.005, f"{side} hip pitch pivot did not move leg"
    assert splay_delta>.005, f"{side} hip abduction pivot did not spread outward"
    assert knee_delta_y<-.005, f"{side} knee did not bend forward"
    assert ankle_delta>.001, f"{side} ankle pivot did not move foot"
    rig_checks[side]={"hip_pitch_moves_leg":True,"hip_abduction_moves_leg_outward":True,
                      "knee_bends_forward":True,"ankle_pitch_moves_foot":True}

def emission_material(name,color,strength=2.0):
    m=mat(name,color,.35,0,strength)
    return m

def segment(name,a,b,radius,material):
    a,b=Vector(a),Vector(b); v=b-a
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=radius, depth=v.length,
        location=(a+b)/2)
    obj=put(bpy.context.object,GUIDE,material)
    obj.rotation_euler=v.to_track_quat("Z","Y").to_euler()
    return obj

def cone_tip(name,point,direction,radius,depth,material):
    bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=radius, radius2=0, depth=depth, location=point)
    obj=put(bpy.context.object,GUIDE,material)
    obj.rotation_euler=Vector(direction).to_track_quat("Z","Y").to_euler()
    return obj

def axis_pair(name,center,axis,half_len,material):
    c=Vector(center); d=Vector(axis).normalized()
    a,b=c-d*half_len,c+d*half_len
    segment(name+"_axis",a,b,.0024,material)
    cone_tip(name+"_minus",a,-d,.007,.018,material)
    cone_tip(name+"_plus",b,d,.007,.018,material)

for side,s in (("L",-1),("R",1)):
    hx=s*HIP_X
    axis_pair(f"{side}_HIP_PITCH_X",(hx,-.003,HIP_Z),(1,0,0),.090,AXIS_PITCH)
    axis_pair(f"{side}_HIP_ABDUCTION_Y",(hx,-.003,HIP_Z),(0,1,0),.095,AXIS_ABDUCT)
    axis_pair(f"{side}_KNEE_PITCH_X",(hx,-.007,KNEE_Z),(1,0,0),.084,AXIS_PITCH)
    axis_pair(f"{side}_ANKLE_PITCH_X",(hx,-.007,ANKLE_Z),(1,0,0),.074,AXIS_PITCH)

# Neutral dark studio.
world=bpy.data.worlds.new("LG01 review world"); world.use_nodes=True
world.node_tree.nodes.get("Background").inputs["Color"].default_value=(.012,.018,.028,1)
world.node_tree.nodes.get("Background").inputs["Strength"].default_value=.28
scene=bpy.context.scene; scene.world=world; scene.render.engine="BLENDER_EEVEE"
scene.eevee.taa_render_samples=96
scene.render.resolution_x=1100; scene.render.resolution_y=1100; scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"; scene.render.image_settings.color_mode="RGBA"
scene.view_settings.view_transform="AgX"; scene.render.film_transparent=False

def area(name,loc,power,size,color):
    data=bpy.data.lights.new(name,"AREA"); data.energy=power; data.shape="DISK"; data.size=size; data.color=color
    o=bpy.data.objects.new(name,data); STUDIO.objects.link(o); o.location=loc
    o.rotation_euler=(Vector((0,0,.25))-o.location).to_track_quat("-Z","Y").to_euler()
    return o
area("large cool key",(-1.2,-1.5,1.3),110,1.2,(.78,.86,1.0))
area("front soft fill",(1.1,-1.5,.45),58,1.0,(1.0,.82,.66))
area("rear rim",(.8,1.0,1.1),125,1.0,(.60,.78,1.0))

def camera(name,loc,target=(0,0,.25),ortho=.73):
    data=bpy.data.cameras.new(name); data.type="ORTHO"; data.ortho_scale=ortho
    o=bpy.data.objects.new(name,data); STUDIO.objects.link(o); o.location=loc
    o.rotation_euler=(Vector(target)-o.location).to_track_quat("-Z","Y").to_euler()
    return o

views=[
    ("LG01-01-Front.png",(0,-2,.25)),
    ("LG01-02-Right.png",(2,0,.25)),
    ("LG01-03-Back.png",(0,2,.25)),
    ("LG01-04-Left.png",(-2,0,.25)),
    ("LG01-05-45deg.png",(1.6,-1.6,.95)),
]
for filename,loc in views:
    scene.camera=camera("Camera_"+filename[:-4],loc)
    scene.render.filepath=os.path.join(OUT,filename)
    bpy.ops.render.render(write_still=True)

# Render the 45-degree view again with colored, double-ended axis arrows.
scene.camera=next(o for o in STUDIO.objects if o.name=="Camera_LG01-05-45deg")
GUIDE.hide_render=False
GUIDE.hide_viewport=False
scene.render.filepath=os.path.join(OUT,"LG01-06-Joint-Centers-Axes-raw.png")
bpy.ops.render.render(write_still=True)
axis_pixels=[]
for (side,joint),center in PIVOTS.items():
    p=world_to_camera_view(scene,scene.camera,Vector(center))
    axis_pixels.append({"side":side,"joint":joint,"x":round(p.x*scene.render.resolution_x),
                        "y":round((1-p.y)*scene.render.resolution_y)})
GUIDE.hide_render=True
GUIDE.hide_viewport=True
scene.render.filepath=os.path.join(OUT,"LG01-05-45deg.png")

# Leave the editable file in a clean neutral pose with the axis guide collection
# available but hidden. All leg assemblies remain independent and mirrored.
scene.camera=next(o for o in STUDIO.objects if o.name=="Camera_LG01-05-45deg")
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,"LG01.blend"))

mesh_objects=[o for o in MODEL.objects if o.type=="MESH"]
coords=[o.matrix_world @ Vector(corner) for o in mesh_objects for corner in o.bound_box]
bounds={"width_x":round(max(v.x for v in coords)-min(v.x for v in coords),3),
        "depth_y":round(max(v.y for v in coords)-min(v.y for v in coords),3),
        "height_z":round(max(v.z for v in coords)-min(v.z for v in coords),3)}
manifest={
    "asset":"WISP LG-01 mirrored biped leg pair",
    "blender":bpy.app.version_string,
    "normalized_wisp_height_m":1.0,
    "measured_pair_bounds_m":bounds,
    "hip_center_x_abs_m":HIP_X,
    "joint_centers_m":{"hip_z":HIP_Z,"knee_z":KNEE_Z,"ankle_z":ANKLE_Z},
    "joint_center_pixels_in_axis_render":axis_pixels,
    "axes":{"hip_pitch":"X, front/back swing","hip_abduction":"Y, lateral splay",
            "knee_pitch":"X, forward human knee bend","ankle_pitch":"X, front/back foot rotation"},
    "rig_motion_checks":rig_checks,
    "mirrored_left_right":True,
    "contains_pelvis_or_torso":False,
    "renders":[f for f,_ in views]+["LG01-06-Joint-Centers-Axes.png","LG01-Review-Phone.png"],
    "references":["LG01-Design-Guide.jpg","WISP-Reference-Concept.png"],
    "automation":"PASS",
    "design_review":"PENDING_PD_VISUAL_REVIEW",
    "note":"Standalone paired legs only; PD retains design acceptance. Joint names mark Blender pivot origins."
}
with open(os.path.join(OUT,"manifest.json"),"w",encoding="utf-8") as f:
    json.dump(manifest,f,ensure_ascii=False,indent=2)
print("LG01_GENERATION_PASS",bpy.app.version_string,"mesh_count",len(mesh_objects),"bounds",bounds)
