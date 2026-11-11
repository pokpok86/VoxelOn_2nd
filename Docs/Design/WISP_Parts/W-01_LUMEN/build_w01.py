"""Build the W-01 LUMEN first-gate module and non-destructive mounting review.

Coordinate system: X = width, Y = fore-aft (muzzle at -Y), Z = height.
All dimensions are meters and are normalized to WISP height L=1.0 m.
Run with Blender 5.2.3: blender --background --python build_la01l.py
"""
import bpy
import math
import os
import sys
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
    collection = bpy.data.collections.new("W01_LUMEN_MODULE")
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


ARMOR = material("W01_LUMEN_Armor_Warm_Ivory", (0.58, 0.54, 0.50), .28, .34)
ARMOR_SHADE = material("W01_LUMEN_Armor_Shadow_Grey", (0.34, 0.34, 0.35), .38, .37)
GRAPHITE = material("W01_LUMEN_Graphite_Core", (0.035, 0.045, 0.058), .72, .29)
GUNMETAL = material("W01_LUMEN_Muzzle_Gunmetal", (0.11, 0.13, 0.15), .78, .26)
ORANGE = material("W01_LUMEN_Safety_Orange", (0.92, 0.25, 0.025), .32, .32)
CYAN = material("W01_LUMEN_Cyan_Laser_Emitter", (0.015, .68, .88), .12, .22, 4.0)
CYAN_DARK = material("W01_LUMEN_Cyan_Glass", (.008, .12, .17), .3, .2, .25)


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


# W-01 is built from the new sheet; the old LA-01 scene is never executed.
import hashlib
import json
from mathutils import Matrix
BASE = os.path.abspath(os.path.join(HERE, '../../WISP_Base_v1'))
sys.path.insert(0,os.path.join(HERE,'../Tools'))
from assembly_layout import load_layout
layout=load_layout(BASE)
BASE_FILES = {p:os.path.join(BASE,p,p+'.blend') for p in ('BD01','HD01','LG01')}
def digest(path):
    with open(path,'rb') as f: return hashlib.sha256(f.read()).hexdigest()
base_hashes={p:digest(path) for p,path in BASE_FILES.items()}

def box(name,loc,dims,mat,chamfer=.002):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=link_object(bpy.context.object,name,mat);o.dimensions=dims
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return bevel(o,chamfer)

# A narrow, long barrel is exposed in front of the wider rear armor.
loft_body('Structural_Barrel_Core',[
    (-.312,.077,.075,-.012),(-.290,.081,.079,-.010),
    (-.165,.087,.082,-.005),(.080,.098,.087,-.003),
    (.205,.089,.077,-.008)],GRAPHITE,8)
loft_body('Lower_Structural_Chassis',[
    (-.220,.078,.061,-.033),(-.085,.102,.071,-.030),
    (.145,.101,.070,-.026),(.215,.077,.055,-.019)],GUNMETAL,8)

for side,s in (('L',-1),('R',1)):
    # Broad side plate, angled lower edge, and a separate orange forward cheek.
    prism_yz('Main_Armor_'+side,[
        (-.177,.041),(-.090,.090),(.123,.103),(.201,.065),
        (.206,.008),(.083,-.012),(.017,-.047),(-.048,-.042),(-.098,.004)
    ],s*.097,.017,ARMOR,.002)
    prism_yz('Orange_Forward_Cheek_'+side,[
        (-.227,.047),(-.135,.070),(-.073,.045),(-.099,-.010),
        (-.162,-.026),(-.224,-.013)
    ],s*.099,.014,ORANGE,.002)
    prism_yz('Lower_Angular_Armor_'+side,[
        (-.178,-.030),(-.089,-.017),(-.039,-.055),(.055,-.061),
        (.151,-.023),(.207,-.033),(.166,-.093),(-.114,-.101),(-.198,-.083)
    ],s*.092,.014,ARMOR_SHADE,.002)
    prism_yz('Recessed_Cooling_Window_'+side,[
        (-.029,-.048),(.059,-.025),(.109,-.039),(.073,-.073),(-.020,-.077)
    ],s*.102,.005,GRAPHITE,.001)
    prism_yz('Cyan_Cooling_Slit_'+side,[
        (-.012,-.057),(.057,-.039),(.068,-.047),(.001,-.066)
    ],s*.106,.002,CYAN,.0005)
    # Long dorsal armor rails are part of the crown rather than added stripes.
    prism_xy('Crown_Rail_'+side,[
        (s*.016,-.204),(s*.057,-.196),(s*.069,-.113),
        (s*.059,.158),(s*.042,.194),(s*.014,.172)
    ],.103,.023,ARMOR,.002)
    prism_xy('Crown_Orange_End_'+side,[
        (s*.019,-.202),(s*.055,-.195),(s*.060,-.165),(s*.021,-.170)
    ],.116,.003,ORANGE,.0006)

box('Dorsal_Recess',(0,.014,.094),(.026,.277,.014),GRAPHITE,.001)
for y in (-.115,.140):
    box('Crown_Structural_Saddle',(0,y,.078),(.113,.030,.033),GRAPHITE,.001)
box('Exposed_Barrel_Top_Rail',(0,-.264,.071),(.064,.185,.013),GUNMETAL,.0015)

# Stepped cut-corner muzzle with a recessed single central emitter.
def xz_profile(rx,rz):
    return [(-rx*.65,-rz),(rx*.65,-rz),(rx,-rz*.65),(rx,rz*.65),
            (rx*.65,rz),(-rx*.65,rz),(-rx,rz*.65),(-rx,-rz*.65)]
rings=[(-.327,.086,.084),(-.361,.086,.084),(-.363,.068,.066),(-.320,.061,.059)]
vs=[(x,y,z-.012) for y,rx,rz in rings for x,z in xz_profile(rx,rz)]
fs=[]
for k in range(4):
    a=k*8;b=((k+1)%4)*8
    for i in range(8):j=(i+1)%8;fs.append((a+i,a+j,b+j,b+i))
me=bpy.data.meshes.new('Muzzle_Stepped_Hollow_Mesh');me.from_pydata(vs,[],fs);me.update()
o=bpy.data.objects.new('Stepped_Hollow_Muzzle',me);COL.objects.link(o)
for m in (GUNMETAL,ARMOR_SHADE,GRAPHITE):o.data.materials.append(m)
for k,p in enumerate(me.polygons):p.material_index=(0,0,2,2)[k//8]
box('Bore_Back_Recess',(0,-.318,-.012),(.117,.006,.113),GRAPHITE,.006)
cylinder_y('Emitter_Glass',(0,-.324,-.012),.034,.006,CYAN_DARK,8)
cylinder_y('Single_Laser_Emitter',(0,-.329,-.012),.025,.003,CYAN,8)
annular_ring_y('Inner_Optic_Seat',(0,-.325,-.012),.048,.037,.005,CYAN_DARK,8)
for x in (-.045,.045):
    box('Bore_Internal_Fin',(x,-.334,-.012),(.008,.032,.067),GUNMETAL,.001)

# Elevated rear elbow socket and protected diagonal bridge share one X pivot.
PIVOT=Vector((0,.282,.109))
for s in (-1,1):
    prism_yz('Elbow_Offset_Bridge',[(.156,.015),(.194,-.003),(.284,.078),(.276,.128),(.231,.103)],s*.028,.020,GRAPHITE,.002)
cylinder_x('Elbow_Drum',PIVOT,.062,.102,GRAPHITE,32)
for side,s in (('L',-1),('R',1)):
    c=(s*.055,.282,.109)
    annular_ring_x('Elbow_Outer_Rim_'+side,c,.059,.042,.009,GUNMETAL,32)
    cylinder_x('Elbow_Socket_Recess_'+side,(s*.0555,.282,.109),.041,.003,GRAPHITE,32)
    annular_ring_x('Elbow_Cyan_Seal_'+side,(s*.058,.282,.109),.041,.039,.002,CYAN_DARK,32)
# Upper half of the joint is protected by a single C-shaped faceted armor shell.
prism_yz('Elbow_Upper_Armor',[
    (.215,.117),(.229,.155),(.262,.176),(.302,.172),(.337,.143),(.346,.107),
    (.330,.103),(.320,.133),(.297,.153),(.267,.156),(.244,.141),(.233,.112)
],0,.116,ARMOR,.002)
box('Rear_Interface_Block',(0,.213,.007),(.108,.022,.087),GRAPHITE,.005)

import bmesh
for mesh_obj in [o for o in COL.objects if o.type=='MESH']:
    bm=bmesh.new();bm.from_mesh(mesh_obj.data)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(mesh_obj.data);bm.free()
root=bpy.data.objects.new('W-01_LUMEN_ROOT',None);COL.objects.link(root)
root.location=PIVOT;root.empty_display_type='ARROWS';root.empty_display_size=.04
bpy.context.view_layer.update()
for obj in list(COL.objects):
    if obj==root:continue
    obj.name='W-01_LUMEN_'+obj.name
    wm=obj.matrix_world.copy();obj.parent=root;obj.matrix_world=wm
bpy.context.view_layer.update()
module_meshes=[o for o in COL.objects if o.type=='MESH']
emitter_visibility=[]
for x in (-.010,0,.010):
    hit,loc,normal,face,obj,wm=bpy.context.scene.ray_cast(bpy.context.evaluated_depsgraph_get(),Vector((x,-1,-.012)),Vector((0,1,0)))
    assert hit and 'Single_Laser_Emitter' in obj.name,'Front emitter is occluded'
    emitter_visibility.append(True)
pts=[o.matrix_world @ Vector(v) for o in module_meshes for v in o.bound_box]
bounds={a:round(max(p[i] for p in pts)-min(p[i] for p in pts),4) for i,a in enumerate(('width_x','length_y','height_z'))}

studio=bpy.data.collections.new('W-01_LUMEN_REVIEW_STUDIO');bpy.context.scene.collection.children.link(studio)
world=bpy.data.worlds.new('W01_dark_studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.015,.020,.028,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.3
scene=bpy.context.scene;scene.world=world;scene.render.engine='BLENDER_EEVEE';scene.eevee.taa_render_samples=96
scene.render.resolution_x=1100;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
def area(name,loc,power,size,color):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color
    o=bpy.data.objects.new(name,d);studio.objects.link(o);o.location=loc
    o.rotation_euler=(Vector((0,0,0))-o.location).to_track_quat('-Z','Y').to_euler()
area('Key',(-1,-1.3,1.2),105,1.25,(1,.89,.78))
area('Fill',(1.2,-.9,.55),65,1.2,(.75,.86,1))
area('Rim',(.5,1,.8),95,1,(.60,.78,1))
def camera(name,loc,target=(0,0,.020),scale=.96):
    d=bpy.data.cameras.new(name);d.type='ORTHO';d.ortho_scale=scale
    o=bpy.data.objects.new(name,d);studio.objects.link(o);o.location=loc
    o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();return o
views=[('01-Front',(0,-2,.020)),('02-Right',(2,0,.020)),('03-Rear',(0,2,.020)),('04-45deg',(1.5,-1.8,.75))]
for label,loc in views:
    scene.camera=camera('Camera_'+label,loc)
    scene.render.filepath=os.path.join(OUT,'W-01_LUMEN-'+label+'.png');bpy.ops.render.render(write_still=True)
scene.camera=camera('Camera_Mount_Closeup',(1.3,.8,.75),PIVOT,.36)
scene.render.filepath=os.path.join(OUT,'W-01_LUMEN-05-Mount-Closeup.png');bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['Camera_04-45deg']
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'W-01_LUMEN.blend'))

# Assemble copies of existing approved component files solely for mount review.
base_collections={'BD01':'BD01_BODY_AND_SHOULDERS','HD01':'HD01_SENSOR_HEAD','LG01':'LG01_LEG_PAIR'}
for part,path in BASE_FILES.items():
    with bpy.data.libraries.load(path,link=False) as (src,dst):dst.collections=[base_collections[part]]
    for collection in dst.collections:scene.collection.children.link(collection)
bpy.data.objects['BD01_ROOT'].location=layout['body_translation']
bpy.data.objects['HD01_ROOT'].location=layout['head_translation']
module_scale=.42
target=Vector(layout['right_elbow'])
root.location=target;root.scale=(module_scale,)*3;root.rotation_euler.x=math.radians(65)
# Copy the module for the opposite arm, preserving the existing base meshes.
left_root=root.copy();left_root.name='W-01_LUMEN_LEFT_ROOT';COL.objects.link(left_root);left_root.location=layout['left_elbow']
for obj in module_meshes:
    duplicate=obj.copy();duplicate.data=obj.data.copy();COL.objects.link(duplicate);duplicate.parent=left_root
bpy.context.view_layer.update()
def aabb(obj):
    points=[obj.matrix_world @ Vector(v) for v in obj.bound_box]
    return [(min(p[i] for p in points),max(p[i] for p in points)) for i in range(3)]
def overlap(a,b):return all(min(a[i][1],b[i][1])-max(a[i][0],b[i][0])>.0001 for i in range(3))
clearance={}
core=[o for o in bpy.data.objects if o.name in ('Torso_Core','Chest_Armor_Main','L_Thigh_Armor','R_Thigh_Armor')]
major=[o for o in COL.objects if o.type=='MESH' and ('Structural_Barrel_Core' in o.name or 'Main_Armor_' in o.name)]
for pose,angle in (('idle',65),('aimed',0)):
    root.rotation_euler.x=left_root.rotation_euler.x=math.radians(angle);bpy.context.view_layer.update()
    hits=[(m.name,b.name) for m in major for b in core if overlap(aabb(m),aabb(b))]
    clearance[pose]={'major_shell_vs_torso_thigh_aabb_clear':not hits,'overlaps':hits}
    assert not hits,f'{pose}: major weapon shell overlaps body or thigh bounds: {hits}'
    scene.camera=camera('Camera_Mounted_'+pose,(1.6,-2,.95),(0,0,.53),1.27)
    scene.render.filepath=os.path.join(OUT,'W-01_LUMEN-Mounted-'+pose+'.png');bpy.ops.render.render(write_still=True)
root.rotation_euler.x=left_root.rotation_euler.x=math.radians(65);bpy.context.view_layer.update()
scene.camera=bpy.data.objects['Camera_Mounted_idle']
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'W-01_LUMEN-Mounted-Review.blend'))
assert base_hashes=={p:digest(path) for p,path in BASE_FILES.items()},'Baseline file changed'
manifest={'asset':'W-01 LUMEN','reference':'../References/W-01_LUMEN.png','blender':bpy.app.version_string,
 'bounds_working_units':bounds,'elbow_pivot_working_units':list(PIVOT),'same_orthographic_scale_all_four_views':.96,
 'structural_armor_mesh_count':len(module_meshes),'front_emitter_visibility_rays':emitter_visibility,'mounted_scale_for_normalized_base':module_scale,
 'mounted_elbows_normalized_base':[layout['right_elbow'],layout['left_elbow']],'base_sha256_unchanged':base_hashes,
 'clearance_checks':clearance,'automation':'PASS','design_approval':'PENDING_PD','unity_integration':'NOT_PERFORMED',
 'assumptions':['Sheet views disagree: front and 45-degree silhouette prioritized.',
 'Internal elbow retention and hidden barrel internals are inferred, not supplied CAD.',
 'Working dimensions approximate the sheet envelope; mounted scale .42 adapts it to the pre-existing normalized WISP base.',
 'Clearance checks are static AABB checks for major shells against torso and thighs in two poses; no exhaustive animation/physics validation.']}
with open(os.path.join(OUT,'manifest.json'),'w',encoding='utf-8') as f:json.dump(manifest,f,indent=2)
print('W01_LUMEN_PASS',bounds,'module meshes',len(module_meshes),'baseline unchanged',clearance)
