"""Build one explicitly selected part from the PD-authorized five-part batch.

Usage: Blender --background --python-exit-code 1 --python build_batch_part.py
       -- --part W-02_BREAKER
References are visual targets. Concealed mechanisms and normalized mounting
scale are documented assumptions rather than measured manufacturing specs.
"""
import os
import sys
import math
import json
import hashlib
import argparse
import bmesh
from mathutils import Vector, Matrix

sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
from modeling_common import *

args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
parser=argparse.ArgumentParser()
parser.add_argument('--part',required=True,choices=['W-02_BREAKER','W-03_RATTLE','W-04_EDGE','W-05_AEGIS','B-01_TWIN_DRIVE'])
PART=parser.parse_args(args).part
PARTS_ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT=os.path.join(PARTS_ROOT,PART);os.makedirs(OUT,exist_ok=True)
BASE=os.path.abspath(os.path.join(PARTS_ROOT,'../WISP_Base_v1'))
BASE_FILES={p:os.path.join(BASE,p,p+'.blend') for p in ('BD01','HD01','LG01')}
def sha(path):
    with open(path,'rb') as f:return hashlib.sha256(f.read()).hexdigest()
before={p:sha(f) for p,f in BASE_FILES.items()}
REFERENCE=os.path.join(PARTS_ROOT,'References',PART+'.png')
assert os.path.isfile(REFERENCE),'Required concept sheet missing'
PIVOT=Vector((0,.282,.109))
IS_BACKPACK=PART.startswith('B-')
FACE_PLANE=PART in ('W-04_EDGE','W-05_AEGIS')
AMBER=material('WISP_Edge_Energy',(.95,.38,.025),.35,.3,1.3)
ORANGE.diffuse_color=(.80,.20,.015,1)
ORANGE.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.80,.20,.015,1)

def box(name,loc,dims,mat,chamfer=.002):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=link_object(bpy.context.object,name,mat);o.dimensions=dims
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return bevel(o,chamfer)

def loft_z(name,sections,mat,cx=0):
    points=[]
    for z,hx,hy,cy in sections:
        loop=[(-hx*.7,-hy),(hx*.7,-hy),(hx,-hy*.7),(hx,hy*.7),(hx*.7,hy),(-hx*.7,hy),(-hx,hy*.7),(-hx,-hy*.7)]
        points.extend((cx+x,cy+y,z) for x,y in loop)
    faces=[tuple(range(7,-1,-1))]
    for k in range(len(sections)-1):
        for i in range(8):j=(i+1)%8;faces.append((k*8+i,k*8+j,(k+1)*8+j,(k+1)*8+i))
    faces.append(tuple(range((len(sections)-1)*8,len(sections)*8)))
    me=bpy.data.meshes.new(name+'_facets');me.from_pydata(points,[],faces);me.update()
    o=bpy.data.objects.new(name,me);COL.objects.link(o);o.data.materials.append(mat)
    return bevel(o,.002)

def elbow_mount():
    # Same origin, X rotation axis, inner seat and mount envelope as LUMEN.
    cylinder_x('Common_Elbow_Drum',PIVOT,.062,.102,GRAPHITE,32)
    for side,s in (('L',-1),('R',1)):
        annular_ring_x('Common_Elbow_Rim_'+side,(s*.055,.282,.109),.059,.042,.009,GUNMETAL,32)
        cylinder_x('Common_Elbow_Seat_'+side,(s*.0555,.282,.109),.041,.003,GRAPHITE,32)
        annular_ring_x('Common_Elbow_Seal_'+side,(s*.058,.282,.109),.041,.039,.002,CYAN_DARK,32)
    prism_yz('Common_Elbow_Upper_Cover',[(.215,.117),(.229,.155),(.262,.176),(.302,.172),(.337,.143),(.346,.107),(.330,.103),(.320,.133),(.297,.153),(.267,.156),(.244,.141),(.233,.112)],0,.116,ARMOR,.002)
    for s in (-1,1):
        prism_yz('Common_Offset_Link',[(.156,.015),(.194,-.003),(.284,.078),(.276,.128),(.231,.103)],s*.028,.020,GRAPHITE,.002)

def armored_gun_body(rotary=False):
    front=-.110 if rotary else -.170
    loft_body('Main_Structural_Housing',[(front,.084,.082,-.007),(-.030,.103,.100,-.006),(.145,.101,.093,-.003),(.218,.078,.071,.004)],GRAPHITE,8)
    for side,s in (('L',-1),('R',1)):
        prism_yz('Main_Armor_'+side,[(front,.045),(-.038,.100),(.133,.104),(.205,.061),(.207,-.025),(.151,-.080),(.058,-.096),(-.078,-.067),(front,-.029)],s*.096,.020,ARMOR,.003)
        prism_yz('Orange_Forward_Armor_'+side,[(front-.014,.046),(front+.020,.073),(front+.064,.061),(front+.052,-.050),(front+.024,-.071),(front-.014,-.045)],s*.108,.011,ORANGE,.002)
        prism_yz('Lower_Armor_'+side,[(front+.01,-.057),(.020,-.067),(.072,-.046),(.168,-.046),(.194,-.079),(.140,-.114),(-.020,-.109),(front,-.083)],s*.087,.016,GUNMETAL,.002)
        # One vent slot and several internal fins belong to the supplied sheets.
        box('Vent_Recess_'+side,(s*.108,.090,-.013),(.004,.046,.037),GRAPHITE,.0007)
        for y in (.075,.088,.101):box('Vent_Fin_'+side,(s*.111,y,-.013),(.003,.004,.032),GUNMETAL,.0005)
    prism_xy('Top_Armor', [(-.060,front-.025),(.060,front-.025),(.067,-.015),(.055,.157),(.035,.204),(-.040,.196),(-.062,.140)],.107,.035,ARMOR,.003)
    prism_xy('Top_Orange_End',[(-.060,front-.024),(.060,front-.024),(.063,front+.039),(-.063,front+.039)],.127,.004,ORANGE,.001)
    box('Rear_Closed_Interface',(0,.220,-.018),(.132,.025,.125),GRAPHITE,.008)

def breaker():
    armored_gun_body(False)
    annular_ring_y('Large_Bore_Tube',(0,-.253,-.013),.087,.067,.170,GRAPHITE,32)
    annular_ring_y('Heavy_Muzzle_Collar',(0,-.353,-.013),.097,.068,.029,GUNMETAL,16)
    annular_ring_y('Muzzle_Inner_Step',(0,-.365,-.013),.077,.067,.009,ARMOR_SHADE,32)
    annular_ring_y('Rear_Barrel_Collar',(0,-.172,-.013),.095,.068,.033,GUNMETAL,16)
    cylinder_y('Deep_Bore_Back',(0,-.144,-.013),.066,.010,GRAPHITE,32)
    for s in (-1,1):
        for z in (-.054,.049):box('Barrel_External_Rib',(s*.071,-.250,z),(.018,.174,.012),GUNMETAL,.0015)
    box('Top_Barrel_Rail',(0,-.253,.075),(.052,.180,.017),GUNMETAL,.002)
    elbow_mount()

def rattle():
    armored_gun_body(True)
    cylinder_y('Rotary_Barrel_Hub',(0,-.117,-.012),.080,.062,GRAPHITE,32)
    locations=[(0,0)]+[(.044*math.cos(i*math.tau/6),.044*math.sin(i*math.tau/6)) for i in range(6)]
    for i,(x,z) in enumerate(locations,1):
        annular_ring_y(f'Barrel_{i}_Hollow_Tube',(x,-.248,z-.012),.0195,.0135,.217,GUNMETAL,24)
        annular_ring_y(f'Barrel_{i}_Front_Lip',(x,-.359,z-.012),.021,.0135,.012,ARMOR_SHADE,24)
        cylinder_y(f'Barrel_{i}_Deep_Recess',(x,-.140,z-.012),.0135,.004,GRAPHITE,24)
    for y in (-.176,-.298):annular_ring_y('Barrel_Bundle_Clamp',(0,y,-.012),.069,.064,.018,GRAPHITE,32)
    box('Small_Cyan_Status',(0,-.112,.083),(.022,.027,.006),CYAN,.001)
    for s in (-1,1):
        box('Lower_Cooling_Grille',(s*.066,.055,-.105),(.031,.118,.008),GRAPHITE,.001)
        for y in (.008,.024,.040,.056,.072,.088):box('Cooling_Grille_Fin',(s*.066,y,-.111),(.028,.005,.009),GUNMETAL,.0005)
    elbow_mount()

def edge():
    # Blade and armor are an elbow-down module, with no handle or wrist.
    loft_body('Longitudinal_Blade_Chassis',[(-.290,.035,.035,.005),(-.100,.060,.052,.005),(.115,.083,.066,.005),(.210,.057,.048,.012)],GRAPHITE,8)
    for side,s in (('L',-1),('R',1)):
        prism_xy('Rear_Upper_Armor_'+side,[(s*.023,.195),(s*.073,.184),(s*.096,.128),(s*.089,.039),(s*.049,-.014),(s*.026,.009)],.058,.039,ARMOR,.002)
        prism_xy('Blade_Flank_Armor_'+side,[(s*.026,.024),(s*.068,.008),(s*.066,-.118),(s*.043,-.270),(s*.022,-.293),(s*.024,-.142)],.046,.027,ARMOR,.002)
        prism_xy('Orange_Rear_Inset_'+side,[(s*.022,.081),(s*.052,.080),(s*.055,.040),(s*.026,.021)],.072,.003,ORANGE,.0007)
        prism_xy('Orange_Flank_Inset_'+side,[(s*.043,-.114),(s*.059,-.108),(s*.048,-.166),(s*.033,-.184)],.058,.003,ORANGE,.0007)
    prism_xy('Blade_Amber_Cutting_Edge',[(-.052,-.175),(-.042,-.390),(0,-.607),(.042,-.390),(.052,-.175)],-.002,.009,AMBER,.001)
    # The raised center spine supplies two real blade facets, rather than a flat decal.
    v=[(-.043,-.178,.006),(0,-.172,.021),(.043,-.178,.006),(-.032,-.390,.006),(0,-.385,.020),(.032,-.390,.006),(0,-.572,.006)]
    f=[(0,3,4,1),(1,4,5,2),(3,6,4),(4,6,5),(0,1,2)]
    me=bpy.data.meshes.new('Blade_Spine_Facets');me.from_pydata(v,[],f);me.update()
    o=bpy.data.objects.new('Dark_Blade_Ridged_Face',me);COL.objects.link(o);o.data.materials.append(GUNMETAL)
    box('Blade_Spine_Status',(0,-.247,.040),(.016,.029,.005),CYAN,.001)
    box('Orange_Integrated_Center_Slider',(0,-.063,.054),(.041,.090,.021),ORANGE,.002)
    for y in (.057,.080,.103):box('Recessed_Rear_Vent',(0,y,.057),(.027,.006,.005),GRAPHITE,.0006)
    elbow_mount()

def aegis():
    outline=[(-.058,.198),(.058,.198),(.111,.106),(.144,-.014),(.124,-.170),(.043,-.365),(-.043,-.365),(-.124,-.170),(-.144,-.014),(-.111,.106)]
    prism_xy('Shield_Structural_Core',outline,0,.067,GRAPHITE,.004)
    for side,s in (('L',-1),('R',1)):
        prism_xy('Upper_Shield_Armor_'+side,[(s*.026,.181),(s*.069,.173),(s*.117,.066),(s*.099,.015),(s*.038,.029)],.052,.024,ARMOR,.0025)
        prism_xy('Outer_Shield_Plate_'+side,[(s*.087,.048),(s*.135,.014),(s*.136,-.091),(s*.108,-.177),(s*.076,-.140),(s*.069,-.023)],.036,.020,ARMOR,.0025)
        prism_xy('Orange_Shield_Inlay_'+side,[(s*.040,.006),(s*.082,.020),(s*.080,-.135),(s*.050,-.191),(s*.034,-.117)],.053,.012,ORANGE,.0015)
        prism_xy('Lower_Shield_Cheek_'+side,[(s*.076,-.154),(s*.109,-.182),(s*.050,-.337),(s*.027,-.343),(s*.034,-.248)],.028,.022,ARMOR_SHADE,.002)
        prism_xy('Rear_Structural_Rail_'+side,[(s*.073,.098),(s*.114,.031),(s*.108,-.148),(s*.059,-.262),(s*.054,-.178)],-.043,.015,GUNMETAL,.002)
    prism_xy('Central_Shield_Spine',[(-.030,.110),(.030,.110),(.036,-.172),(.021,-.246),(-.021,-.246),(-.036,-.172)],.064,.023,ARMOR,.002)
    box('Central_Dark_Notch',(0,-.011,.078),(.036,.039,.004),GRAPHITE,.001)
    box('Central_Orange_Mark',(0,-.014,.081),(.023,.017,.002),ORANGE,.0005)
    box('Shield_Tip_Cyan_Status',(0,-.344,.037),(.042,.009,.005),CYAN,.001)
    box('Rear_Integrated_Stiffener',(0,-.055,-.045),(.055,.310,.033),GRAPHITE,.003)
    elbow_mount()

def thruster_nozzle(name,loc,direction):
    # Hollow nozzle follows a downward/rearward axis; the lens is recessed.
    axis=Vector(direction).normalized();center=Vector(loc)
    # Author along Y using the shared annulus, then rotate around its center.
    o=annular_ring_y(name+'_Hollow_Nozzle',center,.047,.033,.042,GUNMETAL,8)
    q=Vector((0,1,0)).rotation_difference(axis)
    for v in o.data.vertices:v.co=center+q @ (v.co-center)
    for title,r,depth,mat,offset in [('Inner_Seal',.032,.003,CYAN_DARK,.009),('Core',.024,.002,CYAN,-.010)]:
        lens=cylinder_y(name+'_'+title,center-axis*offset,r,depth,mat,16)
        lens.rotation_euler=Vector((0,0,1)).rotation_difference(axis).to_euler()

def twin_drive():
    global PIVOT
    PIVOT=Vector((0,0,0))
    box('Common_Back_Mount_Plate',(0,.009,0),(.088,.018,.074),GUNMETAL,.003)
    for x in (-.034,.034):
        for z in (-.025,.025):
            cylinder_y('Common_Back_Mount_Pin',(x,-.003,z),.006,.016,GRAPHITE,16)
    loft_z('Central_Backpack_Spine',[(-.096,.032,.034,.065),(-.077,.039,.040,.065),(.075,.039,.041,.065),(.104,.027,.032,.065)],GRAPHITE)
    box('Twin_Pod_Structural_Crossbeam',(0,.068,.010),(.266,.042,.046),GUNMETAL,.004)
    for side,s in (('L',-1),('R',1)):
        cx=s*.119
        loft_z('Pod_Core_'+side,[(-.091,.054,.049,.105),(-.050,.064,.059,.092),(.063,.063,.052,.075),(.107,.046,.039,.066)],GRAPHITE,cx)
        # Broad sloping white hood with chamfered upper shoulders.
        loft_z('Upper_Armor_Hood_'+side,[(-.012,.059,.067,.095),(.014,.068,.067,.095),(.088,.057,.055,.080),(.118,.040,.036,.051)],ARMOR,cx)
        for edge_sign in (-1,1):
            x=cx+edge_sign*.052
            prism_yz('Orange_Pod_Corner_'+side,[(-.001,-.018),(.035,-.036),(.058,-.013),(.048,.012),(.005,.024)],x,.016,ORANGE,.002)
        box('Rear_Cooling_Recess_'+side,(cx,.169,.021),(.072,.008,.091),GRAPHITE,.002)
        for z in (-.014,.002,.018,.034,.050):box('Rear_Cooling_Louver_'+side,(cx,.175,z),(.068,.017,.006),GUNMETAL,.001)
        prism_yz('Lower_Pod_Armor_'+side,[(.052,-.039),(.101,-.036),(.162,-.081),(.143,-.122),(.078,-.100),(.032,-.067)],cx,.117,ARMOR_SHADE,.003)
        thruster_nozzle('Down_Rear_Thruster_'+side,(cx,.178,-.103),(0,.80,-.60))
        cylinder_x('Pod_Outer_Service_Cap_'+side,(cx+s*.065,.075,-.014),.027,.009,GUNMETAL,16)
    box('Upper_Center_Service_Cover',(0,.078,.107),(.052,.065,.024),ARMOR_SHADE,.003)

{'W-02_BREAKER':breaker,'W-03_RATTLE':rattle,'W-04_EDGE':edge,'W-05_AEGIS':aegis,'B-01_TWIN_DRIVE':twin_drive}[PART]()

# Correct normals after mirroring polygon coordinates.
for obj in [o for o in COL.objects if o.type=='MESH']:
    bm=bmesh.new();bm.from_mesh(obj.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(obj.data);bm.free()
root=bpy.data.objects.new(PART+'_ROOT',None);COL.objects.link(root);root.location=PIVOT
root.empty_display_type='ARROWS';root.empty_display_size=.04
bpy.context.view_layer.update()
for obj in list(COL.objects):
    if obj==root:continue
    obj.name=PART+'_'+obj.name
    wm=obj.matrix_world.copy();obj.parent=root;obj.matrix_world=wm
bpy.context.view_layer.update()
meshes=[o for o in COL.objects if o.type=='MESH']
nozzle_visibility=[]
if IS_BACKPACK:
    axis=Vector((0,.80,-.60))
    for s in (-1,1):
        origin=Vector((s*.119,.178,-.103))+axis*.3
        hit,loc,normal,face,obj,wm=bpy.context.scene.ray_cast(bpy.context.evaluated_depsgraph_get(),origin,-axis)
        assert hit and 'Thruster_' in obj.name and '_Core' in obj.name,'Backpack thruster optic is occluded'
        nozzle_visibility.append(True)
pts=[o.matrix_world @ Vector(v) for o in meshes for v in o.bound_box]
lo=Vector(tuple(min(v[i] for v in pts) for i in range(3)));hi=Vector(tuple(max(v[i] for v in pts) for i in range(3)))
bounds={k:round(hi[i]-lo[i],4) for i,k in enumerate(('width_x','length_y','height_z'))}
center=(lo+hi)/2

studio=bpy.data.collections.new(PART+'_STUDIO');bpy.context.scene.collection.children.link(studio)
world=bpy.data.worlds.new(PART+'_WORLD');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.015,.020,.028,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.30
scene=bpy.context.scene;scene.world=world;scene.render.engine='BLENDER_EEVEE';scene.eevee.taa_render_samples=96
scene.render.resolution_x=1100;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
def area(name,loc,power,size,color):
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color
    o=bpy.data.objects.new(name,d);studio.objects.link(o);o.location=loc;o.rotation_euler=(center-o.location).to_track_quat('-Z','Y').to_euler()
area('Key',(-1,-1.3,1.2),90,1.25,(1,.89,.78));area('Fill',(1.2,-.9,.55),55,1.2,(.75,.86,1));area('Rim',(.5,1,.8),80,1,(.60,.78,1))
def camera(name,loc,target,scale):
    d=bpy.data.cameras.new(name);d.type='ORTHO';d.ortho_scale=scale
    o=bpy.data.objects.new(name,d);studio.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();return o
scale=max(hi-lo)*1.32
if FACE_PLANE:
    positions=[('01-Front',(0,center.y,2)),('02-Right',(2,center.y,0)),('03-Rear',(0,center.y,-2)),('04-45deg',(1.4,-1.7,1.4))]
elif IS_BACKPACK:
    positions=[('01-Front',(0,-2,0)),('02-Right',(2,0,0)),('03-Rear',(0,2,0)),('04-45deg',(1.4,1.8,.55))]
else:
    positions=[('01-Front',(0,-2,center.z)),('02-Right',(2,center.y,center.z)),('03-Rear',(0,2,center.z)),('04-45deg',(1.5,-1.8,.75))]
for name,loc in positions:
    scene.camera=camera('Camera_'+name,loc,center,scale)
    scene.render.filepath=os.path.join(OUT,PART+'-'+name+'.png');bpy.ops.render.render(write_still=True)
scene.camera=camera('Camera_Mount',(.7,-.9,.45) if IS_BACKPACK else (1.3,.8,.75),PIVOT,.24 if IS_BACKPACK else .34)
scene.render.filepath=os.path.join(OUT,PART+'-05-Mount-Closeup.png');bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['Camera_04-45deg']
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,PART+'.blend'))

collections={'BD01':'BD01_BODY_AND_SHOULDERS','HD01':'HD01_SENSOR_HEAD','LG01':'LG01_LEG_PAIR'}
for part,path in BASE_FILES.items():
    with bpy.data.libraries.load(path,link=False) as (src,dst):dst.collections=[collections[part]]
    for col in dst.collections:scene.collection.children.link(col)
bpy.data.objects['BD01_ROOT'].location.z+=.712;bpy.data.objects['HD01_ROOT'].location.z+=.929
if IS_BACKPACK:
    mount_scale=1.0;root.location=(0,.139,.707);root.scale=(1,1,1)
    # Keep the approved forearms visible in the backpack mounting review.
    approved_path=os.path.join(PARTS_ROOT,'W-01_LUMEN','W-01_LUMEN.blend')
    approved_hash=sha(approved_path)
    with bpy.data.libraries.load(approved_path,link=False) as (src,dst):dst.collections=['W01_LUMEN_MODULE']
    for col in dst.collections:scene.collection.children.link(col)
    arm=bpy.data.objects['W-01_LUMEN_ROOT'];arm.location=(.282,0,.604);arm.scale=(.42,)*3;arm.rotation_euler.x=math.radians(65)
    arm_col=dst.collections[0]
    other=arm.copy();other.name='W-01_LUMEN_LEFT_ROOT';arm_col.objects.link(other);other.location.x=-.282
    for obj in list(arm_col.objects):
        if obj.type=='MESH':
            cp=obj.copy();cp.data=obj.data.copy();arm_col.objects.link(cp);cp.parent=other
    assert approved_hash==sha(approved_path),'Approved LUMEN file changed'
    pose_angles=[('idle',0)]
    preview_cam=(1.5,2,.95)
else:
    mount_scale=.42;root.location=(.282,0,.604);root.scale=(mount_scale,)*3
    left=root.copy();left.name=PART+'_LEFT_ROOT';COL.objects.link(left);left.location.x=-.282
    for obj in meshes:
        cp=obj.copy();cp.data=obj.data.copy();COL.objects.link(cp);cp.parent=left
    pose_angles=[('idle',65),('guard' if PART=='W-05_AEGIS' else 'aimed',90 if PART=='W-05_AEGIS' else 0)]
    preview_cam=(1.6,-2,.95)

def aabb(obj):
    pts=[obj.matrix_world @ Vector(v) for v in obj.bound_box]
    return [(min(p[i] for p in pts),max(p[i] for p in pts)) for i in range(3)]
def overlaps(a,b):return all(min(a[i][1],b[i][1])-max(a[i][0],b[i][0])>.0001 for i in range(3))
body=[o for o in bpy.data.objects if o.name in ('Torso_Core','Chest_Armor_Main','L_Thigh_Armor','R_Thigh_Armor')]
checks={}
for pose,angle in pose_angles:
    if not IS_BACKPACK:root.rotation_euler.x=left.rotation_euler.x=math.radians(angle)
    bpy.context.view_layer.update()
    major=[o for o in COL.objects if o.type=='MESH' and any(t in o.name for t in ('Main_Structural_Housing','Shield_Structural_Core','Longitudinal_Blade_Chassis','Pod_Core_','Blade_Amber_Cutting_Edge'))]
    hits=[(m.name,b.name) for m in major for b in body if overlaps(aabb(m),aabb(b))]
    checks[pose]={'major_shell_vs_torso_thigh_bounds_clear':not hits,'overlaps':hits}
    assert not hits,f'{PART}: {pose} major-shell clearance failure: {hits}'
    scene.camera=camera('Camera_Mounted_'+pose,preview_cam,(0,0,.53),1.27)
    scene.render.filepath=os.path.join(OUT,PART+'-Mounted-'+pose+'.png');bpy.ops.render.render(write_still=True)
if not IS_BACKPACK:root.rotation_euler.x=left.rotation_euler.x=math.radians(65)
scene.camera=bpy.data.objects['Camera_Mounted_idle']
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,PART+'-Mounted-Review.blend'))
assert before=={p:sha(path) for p,path in BASE_FILES.items()},'Base file was modified'
manifest={'asset':PART,'reference':'../References/'+PART+'.png','reference_sha256':sha(REFERENCE),'blender':bpy.app.version_string,
 'working_bounds':bounds,'module_mesh_count':len(meshes),'connection_origin_working_units':list(PIVOT),
 'common_elbow_interface':'W-01_LUMEN X-axis origin, drum and seat' if not IS_BACKPACK else None,
 'back_mount_assumption':{'anchor_normalized_base':[0,.139,.707],'pin_rectangle_width_height':[.068,.050],'source':'provisional attachment to existing rear plate; concealed retention not supplied'} if IS_BACKPACK else None,
 'four_views_same_ortho_scale':scale,'face_view_axis':'+Z/-Z face and reverse' if FACE_PLANE else '-Y/+Y front and rear',
 'mounted_scale':mount_scale,'base_sha256_unchanged':before,'static_clearance_checks':checks,
 'thruster_lens_visibility_rays':nozzle_visibility,'automation':'PASS','design_approval':'PENDING_PD','unity_integration':'NOT_PERFORMED',
 'assumptions':['Front and 45-degree concept silhouettes take priority where views disagree.',
 'Concealed internals and connector retention are modeling assumptions, not verified CAD.',
 'Weapons use the LUMEN approved mount scale .42 on the normalized base; backpack uses the normalized torso scale directly.',
 'Clearance covers selected major shells versus torso/thigh AABBs in static poses, not exhaustive collision or animation validation.']}
with open(os.path.join(OUT,'manifest.json'),'w',encoding='utf-8') as f:json.dump(manifest,f,indent=2)
print('BATCH_PART_PASS',PART,bounds,len(meshes),checks)
