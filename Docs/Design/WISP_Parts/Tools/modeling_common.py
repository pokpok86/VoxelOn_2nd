"""Shared Blender geometry and materials for the authorized WISP part batch.

Coordinate system: X = width, Y = fore-aft (muzzle at -Y), Z = height.
Weapon working dimensions follow illustrative concept envelopes; mounted
scale is defined separately in build_batch_part.py for the normalized base.
Imported by build_batch_part.py under Blender 5.2.3.
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
    collection = bpy.data.collections.new("WISP_BATCH_MODULE")
    bpy.context.scene.collection.children.link(collection)
    return collection


COL = clear_scene()
bpy.context.preferences.filepaths.save_version = 0


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


ARMOR = material("WISP_BATCH_Armor_Warm_Ivory", (0.58, 0.54, 0.50), .28, .34)
ARMOR_SHADE = material("WISP_BATCH_Armor_Shadow_Grey", (0.34, 0.34, 0.35), .38, .37)
GRAPHITE = material("WISP_BATCH_Graphite_Core", (0.035, 0.045, 0.058), .72, .29)
GUNMETAL = material("WISP_BATCH_Muzzle_Gunmetal", (0.11, 0.13, 0.15), .78, .26)
ORANGE = material("WISP_BATCH_Safety_Orange", (0.92, 0.25, 0.025), .32, .32)
CYAN = material("WISP_BATCH_Cyan_Laser_Emitter", (0.015, .68, .88), .12, .22, 4.0)
CYAN_DARK = material("WISP_BATCH_Cyan_Glass", (.008, .12, .17), .3, .2, .25)


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
