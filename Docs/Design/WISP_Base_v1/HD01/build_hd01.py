"""Generate the standalone WISP HD-01 sensor head and review renders.

Coordinates are meters, Z-up, with the face pointing toward -Y. The model is a
small, faceted helmet with one continuous visor band and a short fixed mount.
No torso, arms, cannon, or legs are included.
"""
import bpy
import json
import math
import os
from mathutils import Vector

OUT = os.path.dirname(os.path.abspath(__file__))
bpy.context.preferences.filepaths.save_version = 0
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
MODEL = bpy.data.collections.new("HD01_SENSOR_HEAD")
bpy.context.scene.collection.children.link(MODEL)
STUDIO = bpy.data.collections.new("HD01_REVIEW_STUDIO")
bpy.context.scene.collection.children.link(STUDIO)

def mat(name, color, rough=.42, metallic=.28, emission=0):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value = (*color, 1)
    p.inputs["Roughness"].default_value = rough
    p.inputs["Metallic"].default_value = metallic
    if emission:
        p.inputs["Emission Color"].default_value = (*color, 1)
        p.inputs["Emission Strength"].default_value = emission
    return m

ARMOR = mat("HD01 | warm pale armor", (.58, .55, .51), .38, .30)
SHADE = mat("HD01 | angled armor shade", (.34, .34, .35), .42, .34)
GRAPHITE = mat("HD01 | graphite under-shell", (.035, .045, .058), .48, .55)
GUNMETAL = mat("HD01 | side sensor housings", (.10, .12, .14), .40, .55)
GLASS = mat("HD01 | continuous dark visor glass", (.008, .08, .105), .20, .25, .18)
CYAN = mat("HD01 | single cyan optical band", (.02, .78, .92), .22, .10, 3.2)
ANTENNA = mat("HD01 | antenna ceramic", (.47, .45, .42), .36, .26)

def link(obj, name, material):
    for c in list(obj.users_collection):
        c.objects.unlink(obj)
    MODEL.objects.link(obj)
    obj.name = name
    if material:
        obj.data.materials.clear()
        obj.data.materials.append(material)
    return obj

def bevel(obj, amount=.0025):
    b = obj.modifiers.new("crisp but softened armor edges", "BEVEL")
    b.width = amount
    b.segments = 1
    obj.modifiers.new("weighted face normals", "WEIGHTED_NORMAL")
    return obj

def box(name, loc, dims, material, amount=.0025):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    obj = link(bpy.context.object, name, material)
    obj.dimensions = dims
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return bevel(obj, amount)

def cyl(name, loc, radius, depth, axis, material, verts=24):
    rot = {"X": (0, math.pi/2, 0), "Y": (math.pi/2, 0, 0), "Z": (0, 0, 0)}[axis]
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth,
                                        location=loc, rotation=rot)
    return bevel(link(bpy.context.object, name, material), .0015)

def prism_xz(name, outline, y_center, depth, material, chamfer=.002):
    n = len(outline)
    verts = [(x, y, z) for y in (y_center-depth/2, y_center+depth/2) for x, z in outline]
    faces = [tuple(range(n-1, -1, -1)), tuple(range(n, 2*n))]
    for i in range(n):
        j = (i+1) % n
        faces.append((i, j, n+j, n+i))
    mesh = bpy.data.meshes.new(name + "_facets")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    MODEL.objects.link(obj)
    obj.data.materials.append(material)
    return bevel(obj, chamfer)

def clipped(hx, hz, cut):
    cx, cz = hx*cut, hz*cut
    return [(-hx+cx,-hz),(hx-cx,-hz),(hx,-hz+cz),(hx,hz-cz),
            (hx-cx,hz),(-hx+cx,hz),(-hx,hz-cz),(-hx,-hz+cz)]

def loft_y(name, sections, material):
    n = 8
    verts = []
    for y, hx, hz, cz, cut in sections:
        verts.extend((x, y, z+cz) for x, z in clipped(hx, hz, cut))
    faces = [tuple(range(n-1,-1,-1))]
    for k in range(len(sections)-1):
        a, b = k*n, (k+1)*n
        for i in range(n):
            j = (i+1) % n
            faces.append((a+i,a+j,b+j,b+i))
    faces.append(tuple(range((len(sections)-1)*n, len(sections)*n)))
    mesh = bpy.data.meshes.new(name + "_formed_shell")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    MODEL.objects.link(obj)
    obj.data.materials.append(material)
    return bevel(obj, .002)

# Small low-poly shell: cut brow, tapered cheeks and a short chin create a
# sensor/avian profile without making a round ball or a plain cube.
loft_y("HD01_Main_Faceted_Helmet_Shell", [
    (-.050,.046,.028,.000,.30),
    (-.035,.058,.040,.000,.28),
    (.028,.059,.041,.001,.25),
    (.055,.050,.033,.002,.27),
], ARMOR)

# One broad horizontal glass band, not two eye lenses. The wedge-like outline
# follows the sloping cheek line and slightly rises toward the outer corners.
visor = prism_xz("HD01_Continuous_Visor_Glass", [
    (-.048,-.009),(-.043,-.018),(0,-.022),(.043,-.018),(.048,-.009),
    (.042,.002),(0,.005),(-.042,.002)
], -.0522, .004, GLASS, .001)
prism_xz("HD01_Single_Cyan_Optical_Slit", [
    (-.038,-.006),(-.034,-.010),(0,-.012),(.034,-.010),(.038,-.006),
    (.033,-.002),(0,-.001),(-.033,-.002)
], -.0555, .002, CYAN, .0007)

# Side cheek housings are narrow low-profile pods seated into the helmet shell.
# They are structural side sensors, not extra eyes or decorative ear blocks.
for side, s in (("L",-1),("R",1)):
    pod = box(f"HD01_{side}_Side_Sensor_Pod", (s*.052,.002,.002), (.018,.040,.032), GUNMETAL, .0025)
    pod.rotation_euler[2] = s*math.radians(-8)
    # Single pale outer cheek plane keeps the visor mask visually framed.
    prism_xz(f"HD01_{side}_Cheek_Armor", [
        (s*.041,-.025),(s*.052,-.018),(s*.059,.010),(s*.044,.028)
    ], -.040, .018, SHADE, .001)

# Small rear-set antenna, kept thin and short relative to the helmet.
antenna = cyl("HD01_Short_Antenna_Base", (.032,.031,.045), .006, .012, "Z", GRAPHITE, 12)
antenna_rod = cyl("HD01_Antenna_Rod", (.034,.033,.061), .0028, .034, "Z", ANTENNA, 10)
antenna_rod.rotation_euler[1] = math.radians(-10)

# Fixed lower interface only; no articulated neck or complete neck geometry.
box("HD01_Fixed_Neck_Interface", (0,.003,-.050), (.058,.052,.018), GRAPHITE, .002)

root = bpy.data.objects.new("HD01_ROOT", None)
MODEL.objects.link(root)
root.empty_display_type = "PLAIN_AXES"
root.empty_display_size = .025
for obj in list(MODEL.objects):
    if obj != root:
        bpy.context.view_layer.update()
        world = obj.matrix_world.copy()
        obj.parent = root
        obj.matrix_world = world

world = bpy.data.worlds.new("HD01 studio world")
world.use_nodes = True
world.node_tree.nodes.get("Background").inputs["Color"].default_value = (.012,.018,.028,1)
world.node_tree.nodes.get("Background").inputs["Strength"].default_value = .35
scene = bpy.context.scene
scene.world = world
scene.render.engine = "BLENDER_EEVEE"
scene.eevee.taa_render_samples = 96
scene.render.resolution_x = 1100
scene.render.resolution_y = 1100
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
scene.view_settings.view_transform = "AgX"

def area(name, loc, power, size, color):
    d = bpy.data.lights.new(name, "AREA")
    d.energy = power
    d.shape = "DISK"
    d.size = size
    d.color = color
    o = bpy.data.objects.new(name, d)
    STUDIO.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector((0,0,0))-o.location).to_track_quat("-Z","Y").to_euler()

area("key",(-.55,-.75,.55),95,.65,(.78,.86,1))
area("fill",(.65,-.55,.18),48,.55,(1,.83,.68))
area("rim",(.4,.55,.48),80,.55,(.58,.77,1))

def camera(name, loc, target=(0,0,0), scale=.31):
    d = bpy.data.cameras.new(name)
    d.type = "ORTHO"
    d.ortho_scale = scale
    o = bpy.data.objects.new(name,d)
    STUDIO.objects.link(o)
    o.location = loc
    o.rotation_euler = (Vector(target)-o.location).to_track_quat("-Z","Y").to_euler()
    return o

views = [
    ("HD01-01-Front.png", (0,-1.1,0)),
    ("HD01-02-Right.png", (1.1,0,0)),
    ("HD01-03-Back.png", (0,1.1,0)),
    ("HD01-04-Left.png", (-1.1,0,0)),
    ("HD01-05-45deg.png", (.85,-.85,.52)),
    ("HD01-06-Top.png", (0,0,1.1)),
]
for filename, loc in views:
    scene.camera = camera("Camera_"+filename[:-4], loc, scale=.31)
    scene.render.filepath = os.path.join(OUT, filename)
    bpy.ops.render.render(write_still=True)

bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,"HD01.blend"))
meshes = [o for o in MODEL.objects if o.type == "MESH"]
points = [o.matrix_world @ Vector(v) for o in meshes for v in o.bound_box]
bounds = {
    "width_x": round(max(v.x for v in points)-min(v.x for v in points),3),
    "depth_y": round(max(v.y for v in points)-min(v.y for v in points),3),
    "height_z": round(max(v.z for v in points)-min(v.z for v in points),3),
}
manifest = {
    "asset": "WISP HD-01 sensor head only",
    "blender": bpy.app.version_string,
    "normalized_wisp_height_m": 1.0,
    "measured_bounds_m": bounds,
    "face_direction": "negative Y",
    "visor": "single continuous horizontal cyan optical slit; no separate eyes",
    "neck_interface": "small fixed connector; no head/neck articulation",
    "bd01_mating": {
        "body_interface_top_z_m": 0.158,
        "head_interface_bottom_local_z_m": -0.059,
        "head_root_z_for_mating_m": 0.217,
        "interface_width_depth_m": [0.058, 0.052]
    },
    "contains_torso": False,
    "contains_arms_or_forearm_cannon": False,
    "contains_legs": False,
    "mesh_count": len(meshes),
    "renders": [f for f,_ in views],
    "review_files": ["HD01-Concept-Comparison.png", "HD01-Review-Phone.png"],
    "references": ["HD01-Design-Guide.jpg", "WISP-Reference-Concept.png"],
    "automation": "PASS",
    "design_review": "PENDING_PD_VISUAL_REVIEW",
    "note": "Standalone head module only; unrelated WISP parts were not edited."
}
with open(os.path.join(OUT,"manifest.json"),"w",encoding="utf-8") as f:
    json.dump(manifest,f,ensure_ascii=False,indent=2)
print("HD01_GENERATION_PASS",bpy.app.version_string,"meshes",len(meshes),"bounds",bounds)
