"""Build only the WISP LA-01L forearm laser cannon as a standalone Blender asset.

Coordinate system: X = width, Y = fore-aft (muzzle at -Y), Z = height.
All dimensions are meters and are normalized to WISP height L=1.0 m.
Run with Blender 5.2.3: blender --background --python build_la01l.py
"""
import bpy
import math
import os
from mathutils import Vector

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = HERE
UNIT = 1.0


def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.cameras, bpy.data.lights, bpy.data.materials):
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)
    collection = bpy.data.collections.new("LA01L_MODULE")
    bpy.context.scene.collection.children.link(collection)
    return collection


COL = clear_scene()


def material(name, color, metallic=0.25, roughness=0.38, emission=0.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1.0)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    if emission:
        shader.inputs["Emission Color"].default_value = (*color, 1.0)
        shader.inputs["Emission Strength"].default_value = emission
    return mat


ARMOR = material("LA01_Armor_Warm_Ivory", (0.58, 0.54, 0.50), .28, .34)
ARMOR_SHADE = material("LA01_Armor_Shadow_Grey", (0.34, 0.34, 0.35), .38, .37)
GRAPHITE = material("LA01_Graphite_Core", (0.035, 0.045, 0.058), .72, .29)
GUNMETAL = material("LA01_Muzzle_Gunmetal", (0.11, 0.13, 0.15), .78, .26)
ORANGE = material("LA01_Safety_Orange", (0.92, 0.25, 0.025), .32, .32)
CYAN = material("LA01_Cyan_Laser_Emitter", (0.015, .68, .88), .12, .22, 4.0)
CYAN_DARK = material("LA01_Cyan_Glass", (.008, .12, .17), .3, .2, .25)


def link_object(obj, name, mat):
    for old_collection in tuple(obj.users_collection):
        old_collection.objects.unlink(obj)
    COL.objects.link(obj)
    obj.name = name
    if mat:
        obj.data.materials.append(mat)
    return obj


def bevel(obj, amount=.006, segments=1):
    if amount > 0:
        mod = obj.modifiers.new("Crisp_Armor_Chamfer", "BEVEL")
        mod.width = amount
        mod.segments = segments
        obj.modifiers.new("Weighted_Facet_Normals", "WEIGHTED_NORMAL")
    return obj


def prism_yz(name, outline, x_center, thickness, mat, chamfer=.004):
    """Extrude an angular armor silhouette along X; outline entries are (Y,Z)."""
    count = len(outline)
    x0, x1 = x_center - thickness / 2, x_center + thickness / 2
    verts = [(x, y, z) for x in (x0, x1) for y, z in outline]
    faces = [tuple(range(count-1, -1, -1)), tuple(range(count, 2*count))]
    for i in range(count):
        j = (i + 1) % count
        faces.append((i, j, count+j, count+i))
    mesh = bpy.data.meshes.new(name + "_FacetedMesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    COL.objects.link(obj)
    obj.data.materials.append(mat)
    return bevel(obj, min(chamfer, thickness*.18), 1)


def prism_xy(name, outline, z_center, thickness, mat, chamfer=.004):
    """Extrude a top armor polygon along Z; outline entries are (X,Y)."""
    count = len(outline)
    z0, z1 = z_center - thickness/2, z_center + thickness/2
    verts = [(x, y, z) for z in (z0, z1) for x, y in outline]
    faces = [tuple(range(count-1, -1, -1)), tuple(range(count, 2*count))]
    for i in range(count):
        j = (i + 1) % count
        faces.append((i, j, count+j, count+i))
    mesh = bpy.data.meshes.new(name + "_FacetedMesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    COL.objects.link(obj)
    obj.data.materials.append(mat)
    return bevel(obj, min(chamfer, thickness*.18), 1)


def loft_body(name, sections, mat, sides=10):
    """Continuous polygonal cannon body. Sections are (Y, half-width, half-height, Z)."""
    verts = []
    for y, half_x, half_z, center_z in sections:
        for i in range(sides):
            angle = (2*math.pi*i/sides) + math.pi/sides
            verts.append((half_x*math.cos(angle), y, center_z + half_z*math.sin(angle)))
    faces = [tuple(range(sides-1, -1, -1))]
    for section in range(len(sections)-1):
        a, b = section*sides, (section+1)*sides
        for i in range(sides):
            j = (i+1) % sides
            faces.append((a+i, a+j, b+j, b+i))
    faces.append(tuple(range((len(sections)-1)*sides, len(sections)*sides)))
    mesh = bpy.data.meshes.new(name + "_ContinuousLoft")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    COL.objects.link(obj)
    obj.data.materials.append(mat)
    return bevel(obj, .004, 1)


def cylinder_x(name, center, radius, depth, mat, vertices=32):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                        location=center, rotation=(0, math.pi/2, 0))
    obj = link_object(bpy.context.object, name, mat)
    return bevel(obj, .003, 1)


def annular_ring_x(name, center, outer_r, inner_r, depth, mat, segments=32):
    """Mechanically clean annular hinge rim; its hole shares the X pivot axis."""
    verts = []
    for x in (center[0]-depth/2, center[0]+depth/2):
        for radius in (outer_r, inner_r):
            for i in range(segments):
                a = 2*math.pi*i/segments
                verts.append((x, center[1] + radius*math.cos(a), center[2] + radius*math.sin(a)))
    idx = lambda side, ring, i: side*2*segments + ring*segments + (i % segments)
    faces = []
    for i in range(segments):
        j = (i+1) % segments
        faces.extend([
            (idx(0,0,i),idx(0,0,j),idx(1,0,j),idx(1,0,i)),
            (idx(0,1,j),idx(0,1,i),idx(1,1,i),idx(1,1,j)),
            (idx(0,0,i),idx(0,1,i),idx(0,1,j),idx(0,0,j)),
            (idx(1,0,j),idx(1,1,j),idx(1,1,i),idx(1,0,i)),
        ])
    mesh = bpy.data.meshes.new(name + "_Annulus")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    COL.objects.link(obj)
    obj.data.materials.append(mat)
    return bevel(obj, .002, 1)


def annular_ring_y(name, center, outer_r, inner_r, depth, mat, segments=32):
    """Rear-facing socket rim in the X/Z plane; normal points along fore-aft Y."""
    verts = []
    for y in (center[1]-depth/2, center[1]+depth/2):
        for radius in (outer_r, inner_r):
            for i in range(segments):
                a = 2*math.pi*i/segments
                verts.append((center[0]+radius*math.cos(a), y, center[2]+radius*math.sin(a)))
    idx = lambda side, ring, i: side*2*segments + ring*segments + (i % segments)
    faces = []
    for i in range(segments):
        j = (i+1) % segments
        faces.extend([
            (idx(0,0,i),idx(0,0,j),idx(0,1,j),idx(0,1,i)),
            (idx(1,0,j),idx(1,0,i),idx(1,1,i),idx(1,1,j)),
            (idx(0,0,i),idx(1,0,i),idx(1,0,j),idx(0,0,j)),
            (idx(0,1,j),idx(1,1,j),idx(1,1,i),idx(0,1,i)),
        ])
    mesh = bpy.data.meshes.new(name + "_RearAnnulus")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    COL.objects.link(obj)
    obj.data.materials.append(mat)
    return bevel(obj, .002, 1)


def cylinder_y(name, center, radius, depth, mat, vertices=32):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                        location=center, rotation=(math.pi/2, 0, 0))
    obj = link_object(bpy.context.object, name, mat)
    return bevel(obj, .002, 1)


def muzzle_bezel():
    """Nested octagonal stepped rim facing the muzzle (-Y)."""
    # Three visible material steps echo the layered polygonal reference muzzle.
    rings = [(-.335, .145, .120), (-.355, .145, .120),
             (-.362, .128, .104), (-.358, .116, .093),
             (-.344, .104, .081)]
    sides = 8
    verts = []
    for y, rx, rz in rings:
        for i in range(sides):
            a = 2*math.pi*i/sides + math.pi/8
            verts.append((rx*math.cos(a), y, rz*math.sin(a)))
    # Bridge outer front perimeter to inner front perimeter, then return to rear.
    faces = []
    for band in range(len(rings)):
        a, b = band*sides, ((band+1) % len(rings))*sides
        for i in range(sides):
            j = (i+1) % sides
            faces.append((a+i, a+j, b+j, b+i))
    mesh = bpy.data.meshes.new("MuzzleOuterRim_SteppedOctagon")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new("MuzzleOuterRim", mesh)
    COL.objects.link(obj)
    for mat in (GUNMETAL, ARMOR_SHADE, GUNMETAL, GRAPHITE):
        obj.data.materials.append(mat)
    band_materials = (0, 1, 2, 3, 3)
    for band, material_index in enumerate(band_materials):
        for poly in obj.data.polygons[band*sides:(band+1)*sides]:
            poly.material_index = material_index
    return obj


def emitter_panel():
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, -.339, -.004))
    obj = link_object(bpy.context.object, "Socket_Muzzle", GRAPHITE)
    obj.dimensions = (.166, .008, .132)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel(obj, .013, 1)
    for i, z in enumerate((.036, -.004, -.044), start=1):
        bpy.ops.mesh.primitive_cube_add(size=1, location=(0, -.345, z))
        bar = link_object(bpy.context.object, f"Emitter_CyanBar_{i}", CYAN)
        bar.dimensions = (.088, .006, .010)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        bevel(bar, .003, 1)


def side_vent(side):
    sx = 1 if side == "R" else -1
    # Set of three inset luminous bars following the cannon's rising side facet.
    for i, y in enumerate((-.014, .036, .086), start=1):
        bpy.ops.mesh.primitive_cube_add(size=1, location=(sx*.145, y, -.018))
        obj = link_object(bpy.context.object, f"CoolingSideStrip_{side}_{i}", CYAN)
        obj.dimensions = (.004, .040, .010)
        obj.rotation_euler[0] = math.radians(-12)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        bevel(obj, .002, 1)


# One uninterrupted cannon core carries the complete weapon silhouette.
loft_body("CannonCore", [
    (-.318, .090, .084, -.004),
    (-.278, .125, .108, -.002),
    (-.205, .145, .118, .000),
    (-.055, .151, .122, .000),
    (.115, .137, .108, -.002),
    (.235, .118, .094, -.004),
    (.287, .101, .078, -.004),
], GRAPHITE, 10)

# Upper shell: a broad clipped crown with stepped, slanted side planes.
loft_body("ArmorTop", [
    (-.278, .104, .102, .012),
    (-.226, .137, .128, .018),
    (-.105, .143, .132, .020),
    (.080, .132, .119, .014),
    (.205, .106, .093, .006),
    (.255, .084, .075, .000),
], ARMOR, 8)

# Lower dark shell keeps the underside and bore connection visually distinct.
loft_body("ArmorBottom", [
    (-.283, .091, .072, -.012),
    (-.208, .116, .085, -.018),
    (-.045, .125, .088, -.018),
    (.120, .113, .079, -.016),
    (.245, .088, .065, -.010),
], GRAPHITE, 8)

# Faceted side armor: the taper and rising shoulder are part of the silhouette.
side_outline = [
    (-.279, -.026), (-.238, .068), (-.175, .104), (-.116, .114),
    (-.080, .083), (.065, .075), (.151, .052), (.198, .010),
    (.167, -.055), (.040, -.103), (-.152, -.095), (-.236, -.068),
]
for side, sx in (("L", -1), ("R", 1)):
    prism_yz(f"ArmorSide_{side}", side_outline, sx*.116, .034, ARMOR, .007)
    # Thin secondary cheek at the front, a hard-edged guard around the muzzle root.
    front_cheek = [(-.268, -.012), (-.235, .073), (-.190, .097), (-.170, .051), (-.202, -.055)]
    prism_yz(f"ArmorFrontCheek_{side}", front_cheek, sx*.137, .012, ARMOR_SHADE, .002)
    side_vent(side)
    # Orange inlay on the outer shell is small and flush, not a bolt-on block.
    logo = [(-.082, .028), (-.052, .060), (-.025, .026), (-.055, .012)]
    prism_yz(f"Safety_Insignia_{side}", logo, sx*.1375, .004, ORANGE, .001)

# Three orange slash-shaped inlays on the dorsal plate.
for i, (y, top_z) in enumerate(((-.209,.139),(-.167,.141),(-.125,.143)), start=1):
    x = -.055 + (i-1)*.006
    prism_xy(f"Dorsal_Orange_Slash_{i}", [
        (x, y), (x+.041, y+.014), (x+.036, y+.038), (x-.004, y+.024)
    ], top_z, .004, ORANGE, .001)

# Recessed, beveled muzzle and its three horizontal laser emitter bars.
muzzle_bezel()
emitter_panel()

# Rear elbow hinge/coupler is a real separate socket centered on the pivot axis.
# The transverse X axis can attach to WISP's existing elbow joint later.
cylinder_x("RearElbowHinge", (0, .245, .004), .094, .235, GRAPHITE, 32)
for side, sx in (("L", -1), ("R", 1)):
    annular_ring_x(f"Socket_Elbow_{side}", (sx*.1175, .245, .004), .092, .064, .020, GUNMETAL, 32)
    cylinder_x(f"Elbow_Recess_{side}", (sx*.129, .245, .004), .060, .008, GRAPHITE, 32)
    annular_ring_x(f"Elbow_Inner_Rim_{side}", (sx*.131, .245, .004), .048, .037, .006, ARMOR_SHADE, 24)
# The rear orthographic end is a recessed circular socket, while side trunnions
# define the transverse elbow pivot axis. This matches the concept's rear view.
annular_ring_y("Rear_Elbow_Socket_Rim", (0,.340,.004), .082, .052, .024, GUNMETAL, 32)
cylinder_y("Rear_Elbow_Socket_Recess", (0,.335,.004), .051, .010, GRAPHITE, 32)
annular_ring_y("Rear_Elbow_Socket_Inner_Ring", (0,.341,.004), .044, .035, .004, ARMOR_SHADE, 24)
# Short, protected dorsal connector bridges the cannon shell to the hinge.
prism_xy("Socket_Elbow_Bridge", [(-.080,.214),(.080,.214),(.060,.288),(-.060,.288)], .005, .090, ARMOR_SHADE, .006)

# Muted edge guards protect the lower rails without enlarging the silhouette.
for side, sx in (("L", -1), ("R", 1)):
    prism_yz(f"ArmorLowerRail_{side}", [(-.232,-.070),(-.181,-.104),(.064,-.102),(.170,-.053),(.137,-.076),(-.170,-.080)], sx*.125, .014, ARMOR_SHADE, .002)

# A single attach root at the elbow pivot keeps this a standalone replaceable module.
root = bpy.data.objects.new("LA01L_ROOT", None)
COL.objects.link(root)
root.location = (0,.245,.004)
root.empty_display_type = "ARROWS"
root.empty_display_size = .05
bpy.context.view_layer.update()
for obj in list(COL.objects):
    if obj == root:
        continue
    world_matrix = obj.matrix_world.copy()
    obj.parent = root
    obj.matrix_world = world_matrix
bpy.context.view_layer.update()

# Display studio: neutral dark ground and broad soft lights reveal all facets.
studio = bpy.data.collections.new("LA01L_REVIEW_STUDIO")
bpy.context.scene.collection.children.link(studio)
world = bpy.data.worlds.new("LA01L_Studio_World")
world.use_nodes = True
world.node_tree.nodes.get("Background").inputs["Color"].default_value = (.012, .018, .028, 1)
world.node_tree.nodes.get("Background").inputs["Strength"].default_value = .45
bpy.context.scene.world = world


def light(name, position, power, size, color):
    data = bpy.data.lights.new(name, "AREA")
    data.energy = power
    data.shape = "DISK"
    data.size = size
    data.color = color
    obj = bpy.data.objects.new(name, data)
    studio.objects.link(obj)
    obj.location = position
    obj.rotation_euler = (Vector((0,0,0))-obj.location).to_track_quat('-Z','Y').to_euler()


light("Key_Softbox", (-1.5,-2.2,2.2), 420, 2.0, (1.0,.88,.72))
light("Front_Fill", (1.8,-2.0,.8), 300, 1.6, (.72,.86,1.0))
light("Rear_Rim", (.6,1.5,1.8), 520, 1.3, (.56,.78,1.0))


def camera(name, position, target=(0,0,0), ortho=1.04):
    data = bpy.data.cameras.new(name)
    data.type = "ORTHO"
    data.ortho_scale = ortho
    obj = bpy.data.objects.new(name, data)
    studio.objects.link(obj)
    obj.location = position
    obj.rotation_euler = (Vector(target)-obj.location).to_track_quat('-Z','Y').to_euler()
    return obj


scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE"
scene.eevee.taa_render_samples = 96
scene.render.resolution_x = 1100
scene.render.resolution_y = 1100
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
scene.view_settings.view_transform = "AgX"
scene.render.film_transparent = False

views = [
    ("LA01L-01-Front.png", (0,-2.2,0)),
    ("LA01L-02-Right.png", (2.2,0,0)),
    ("LA01L-03-Back.png", (0,2.2,0)),
    ("LA01L-04-Top.png", (0,0,2.2)),
    ("LA01L-05-45deg.png", (1.8,-1.8,1.3)),
]
for filename, position in views:
    scene.camera = camera("Camera_" + filename[:-4], position)
    scene.render.filepath = os.path.join(OUT, filename)
    bpy.ops.render.render(write_still=True)

# Keep the most useful review view active in the editable project.
scene.camera = next(o for o in studio.objects if o.name == "Camera_LA01L-05-45deg")
scene.render.filepath = os.path.join(OUT, "LA01L-05-45deg.png")
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "LA01L.blend"))

# Manifest captures the actually built component set and mesh bounds.
mesh_objects = [o for o in COL.objects if o.type == "MESH"]
bpy.context.view_layer.update()
world_points = [o.matrix_world @ Vector(corner) for o in mesh_objects for corner in o.bound_box]
mins = [min(v[i] for v in world_points) for i in range(3)]
maxs = [max(v[i] for v in world_points) for i in range(3)]
manifest = {
    "asset": "WISP LA-01L Lower-Arm Laser Cannon",
    "blender": bpy.app.version_string,
    "unit_scale_wisp_height_m": UNIT,
    "bounds_m": {axis: round(maxs[i]-mins[i],4) for i,axis in enumerate(("width_x","length_y","height_z"))},
    "expected_approx_bounds_m": {"width_x":.32,"length_y":.72,"height_z":.28},
    "named_module_root": root.name,
    "mesh_count": len(mesh_objects),
    "components": sorted(o.name for o in COL.objects if o != root),
    "renders": [name for name,_ in views],
    "automation": "PASS",
    "design_review": "PENDING_PD_VISUAL_REVIEW",
    "wisp_base_modified": False,
    "note": "Standalone lower-arm cannon only; visual acceptance remains with PD."
}
import json
with open(os.path.join(OUT,"manifest.json"),"w",encoding="utf-8") as f:
    json.dump(manifest,f,ensure_ascii=False,indent=2)
print("LA01L_GENERATION_PASS", bpy.app.version_string, "meshes", len(mesh_objects), "bounds", manifest["bounds_m"])
