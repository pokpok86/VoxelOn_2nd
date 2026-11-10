# WISP V2 BLOCKOUT / Blender Scripting > Run Script
# NOT production-ready, Blender/Unity runtime validation pending.
# X right; -Y front; Z up. Meters.
import bpy
from math import radians
POSE="IDLE" # change to FIRE and rerun for firing preview
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
def mat(n,c,glow=0):
    m=bpy.data.materials.get("WISP_"+n) or bpy.data.materials.new("WISP_"+n)
    m.use_nodes=True; m.diffuse_color=(*c,1)
    bs=m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value=(*c,1)
    bs.inputs["Metallic"].default_value=.25
    bs.inputs["Roughness"].default_value=.62
    if glow:
        bs.inputs["Emission Color"].default_value=(*c,1)
        bs.inputs["Emission Strength"].default_value=glow
    return m
armor=mat("Armor",(.48,.50,.52)); frame=mat("Frame",(.055,.067,.08))
orange=mat("Accent",(.77,.28,.06)); cyan=mat("Cyan",(.04,.65,.85),2)
def pivot(n,p,parent=None):
    o=bpy.data.objects.new(n,None);col.objects.link(o);o.location=p
    o.empty_display_type='ARROWS';o.empty_display_size=.04
    return par(o,parent)
def cube(n,p,d,m,parent,bevel=.007):
    bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object
    for group in tuple(o.users_collection):group.objects.unlink(o)
    col.objects.link(o);o.name=n;o.dimensions=d
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(m)
    if bevel:
        b=o.modifiers.new("Chamfer","BEVEL");b.width=min(bevel,min(d)*.12);b.segments=1
        o.modifiers.new("Normals","WEIGHTED_NORMAL")
    return par(o,parent)
def ring(n,p,r,depth,m,parent,angle=(0,0,0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=12,radius=r,depth=depth,location=p,rotation=angle)
    o=bpy.context.object
    for group in tuple(o.users_collection):group.objects.unlink(o)
    col.objects.link(o);o.name=n;o.data.materials.append(m)
    return par(o,parent)

root=pivot("ROOT",(0,0,0))
pelvis=pivot("Pelvis_Frame",(0,0,.69),root)
waist=pivot("Waist_Yaw_Z",(0,0,.81),pelvis)
breath=pivot("UpperBody_BreathIdle",(0,0,1.05),waist)
cube("Pelvis",(0,0,.73),(.34,.23,.17),frame,pelvis)
cube("Pelvis_Armor",(0,-.135,.74),(.24,.06,.14),armor,pelvis)
cube("Waist_Hinge",(0,0,.84),(.19,.18,.10),frame,pelvis)
cube("Torso_Core",(0,0,1.08),(.42,.25,.43),frame,breath)
cube("Chest",(0,-.11,1.13),(.48,.18,.25),armor,breath)
cube("Chest_Bottom",(0,-.105,.965),(.34,.17,.10),armor,breath)
cube("Orange_Stripe",(0,-.206,1.14),(.045,.018,.20),orange,breath)
cube("Chest_Light",(0,-.205,.98),(.09,.017,.04),cyan,breath)
head=pivot("Head_FIXED",(0,-.012,1.365),breath)
cube("Head_Small",(0,-.02,1.405),(.16,.145,.125),armor,head)
cube("Head_Visor",(0,-.099,1.413),(.10,.016,.055),cyan,head,.001)
cube("Head_Antenna",(.045,.012,1.51),(.021,.018,.12),armor,head)

shoulders={};elbows={}
for side,s in (("L",-1),("R",1)):
    shoulder=pivot("Shoulder_"+side+"_PitchX",(s*.335,.005,1.245),breath)
    spread=pivot("Shoulder_"+side+"_AbductY",(s*.335,.005,1.245),shoulder)
    ring("Shoulder_Joint_"+side,(s*.335,.005,1.245),.082,.09,frame,spread,(0,radians(90),0))
    cube("UpperArm_"+side,(s*.355,.01,1.105),(.125,.13,.235),frame,spread)
    cube("UpperArm_Plate_"+side,(s*.355,-.067,1.11),(.126,.023,.16),armor,spread)
    elbow=pivot("Elbow_"+side+"_PitchX",(s*.365,.01,.975),spread)
    ring("Elbow_Joint_"+side,(s*.365,.01,.975),.06,.12,frame,elbow,(0,radians(90),0))
    cube("ForearmGun_"+side,(s*.402,-.015,.755),(.245,.24,.39),armor,elbow)
    cube("Gun_Rail_"+side,(s*.402,-.154,.765),(.11,.06,.28),frame,elbow)
    cube("Gun_Cyan_"+side,(s*.402,-.192,.765),(.052,.018,.17),cyan,elbow,.002)
    cube("Gun_Trim_"+side,(s*.52,-.012,.83),(.023,.13,.11),orange,elbow)
    ring("Muzzle_"+side,(s*.402,-.015,.54),.083,.088,frame,elbow)
    ring("Muzzle_Glow_"+side,(s*.402,-.015,.489),.052,.009,cyan,elbow)
    pivot("Socket_Muzzle_"+side,(s*.402,-.015,.477),elbow)
    spread.rotation_euler[1]=radians(s*8)
    shoulders[side]=shoulder;elbows[side]=elbow

for side,s in (("L",-1),("R",1)):
    hip=pivot("Hip_"+side+"_PitchX_AbductY",(s*.145,0,.685),pelvis)
    ring("HipJoint_"+side,(s*.145,0,.685),.062,.10,frame,hip,(0,radians(90),0))
    cube("Thigh_"+side,(s*.155,0,.54),(.13,.145,.28),armor,hip)
    knee=pivot("Knee_"+side+"_PitchX",(s*.16,-.015,.405),hip)
    ring("KneeJoint_"+side,(s*.16,-.015,.405),.055,.11,frame,knee,(0,radians(90),0))
    cube("Shin_"+side,(s*.17,0,.25),(.12,.14,.265),armor,knee)
    ankle=pivot("Ankle_"+side+"_PitchX",(s*.17,0,.115),knee)
    cube("Foot_"+side,(s*.17,-.055,.065),(.175,.245,.13),armor,ankle)
pivot("Socket_Backpack",(0,.166,1.15),breath)
pivot("Socket_LowerBody",(0,0,.81),pelvis)

for side in ("L","R"):
    shoulders[side].rotation_euler[0]=radians(-7)
    elbows[side].rotation_euler[0]=radians(13)
if POSE=="FIRE":
    for side in ("L","R"):
        shoulders[side].rotation_euler[0]=radians(-68)
        elbows[side].rotation_euler[0]=radians(-20)
else:
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
bpy.ops.object.select_all(action='DESELECT')
root.select_set(True);bpy.context.view_layer.objects.active=root
print("WISP_V2 blockout ready; pose:",POSE)
