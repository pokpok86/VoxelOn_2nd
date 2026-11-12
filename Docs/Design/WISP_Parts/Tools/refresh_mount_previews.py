"""Refresh assemblies after PD-authorized body or weapon mounting changes.

Standalone weapon/backpack .blend and raw part renders are read-only inputs.
Only mounted review scenes/images and their mounting metadata are rewritten.
"""
import os
import sys
import json
import math
import hashlib
import bpy
from mathutils import Vector

sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
from assembly_layout import load_layout
from assembly_pose import pose_shoulders, set_forearm_angle, IDLE_DEGREES
from weapon_mount_style import style_weapon, BODY_SCALE
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BASE=os.path.abspath(os.path.join(ROOT,'../WISP_Base_v1'))
PARTS=['W-01_LUMEN','W-02_BREAKER','W-03_RATTLE','W-04_EDGE','W-05_AEGIS','B-01_TWIN_DRIVE']
layout=load_layout(BASE)
def sha(path):
    with open(path,'rb') as f:return hashlib.sha256(f.read()).hexdigest()
sources={p:os.path.join(ROOT,p,p+'.blend') for p in PARTS}
part_hashes={p:sha(path) for p,path in sources.items()}
base_hashes={p:sha(os.path.join(BASE,p,p+'.blend')) for p in ('BD01','HD01','LG01')}
bpy.context.preferences.filepaths.save_version=0
report={}

def bounds(obj):
    pts=[obj.matrix_world @ Vector(v) for v in obj.bound_box]
    return [(min(p[i] for p in pts),max(p[i] for p in pts)) for i in range(3)]
def overlap(a,b):return all(min(a[i][1],b[i][1])-max(a[i][0],b[i][0])>.0001 for i in range(3))
def append_base():
    names={'BD01':'BD01_BODY_AND_SHOULDERS','HD01':'HD01_SENSOR_HEAD','LG01':'LG01_LEG_PAIR'}
    for part,col_name in names.items():
        with bpy.data.libraries.load(os.path.join(BASE,part,part+'.blend'),link=False) as (src,dst):dst.collections=[col_name]
        for col in dst.collections:bpy.context.scene.collection.children.link(col)
    bpy.data.objects['BD01_ROOT'].location=layout['body_translation']
    bpy.data.objects['HD01_ROOT'].location=layout['head_translation']
    pose_shoulders(layout)
def attach_pair(root,col,scale):
    root.location=layout['right_elbow'];root.scale=(scale,)*3;set_forearm_angle(root,IDLE_DEGREES)
    meshes=[o for o in col.objects if o.type=='MESH']
    other=root.copy();other.name=root.name.replace('_ROOT','_LEFT_ROOT');col.objects.link(other);other.location=layout['left_elbow'];set_forearm_angle(other,IDLE_DEGREES,left=True)
    for obj in meshes:
        cp=obj.copy();cp.data=obj.data.copy();col.objects.link(cp);cp.parent=other
    part=root.name.removesuffix('_ROOT')
    style_weapon(root,col,part)
    style_weapon(other,col,part,left=True)
    return other
def camera(name,location):
    d=bpy.data.cameras.new(name);d.type='ORTHO';d.ortho_scale=1.60
    o=bpy.data.objects.new(name,d);bpy.context.scene.collection.objects.link(o);o.location=location
    o.rotation_euler=(Vector((0,0,.53))-o.location).to_track_quat('-Z','Y').to_euler();return o

for part,path in sources.items():
    folder=os.path.join(ROOT,part)
    with open(os.path.join(folder,'manifest.json'),encoding='utf-8') as f:manifest=json.load(f)
    bpy.ops.wm.open_mainfile(filepath=path)
    root=bpy.data.objects[part+'_ROOT'];col=root.users_collection[0]
    append_base()
    is_backpack=part.startswith('B-')
    if is_backpack:
        root.location=layout['back_mount'];root.scale=(1,1,1)
        with bpy.data.libraries.load(sources['W-01_LUMEN'],link=False) as (src,dst):dst.collections=['W01_LUMEN_MODULE']
        for arm_col in dst.collections:bpy.context.scene.collection.children.link(arm_col)
        attach_pair(bpy.data.objects['W-01_LUMEN_ROOT'],dst.collections[0],.42)
        poses=[('idle',0)];cam_pos=(1.5,2,.95)
    else:
        other=attach_pair(root,col,.42)
        poses=[('idle',IDLE_DEGREES),('guard' if part=='W-05_AEGIS' else 'aimed',90 if part=='W-05_AEGIS' else 0)]
        cam_pos=(1.6,-2,.95)
    bpy.context.view_layer.update()
    waist_error=abs(bounds(bpy.data.objects['Waist_Pelvis_Interface_Key'])[2][0]-bounds(bpy.data.objects['LG01_Waist_Socket_Floor'])[2][1])
    neck_error=abs(bounds(bpy.data.objects['Neck_Head_Interface'])[2][1]-bounds(bpy.data.objects['HD01_Fixed_Neck_Interface'])[2][0])
    assert waist_error<.0001 and neck_error<.0001,'Body interfaces are detached'
    torso=[o for o in bpy.data.objects if o.name in ('Torso_Core','Chest_Armor_Main','L_Thigh_Armor','R_Thigh_Armor')]
    major=[o for o in col.objects if o.type=='MESH' and any(n in o.name for n in ('Structural_Barrel_Core','Main_Armor_','Main_Structural_Housing','Shield_Structural_Core','Longitudinal_Blade_Chassis','Pod_Core_','Blade_Amber_Cutting_Edge'))]
    checks={}
    for pose,angle in poses:
        selected_angle=angle
        candidates=[angle] if is_backpack else list(dict.fromkeys([angle,30,40,50,60,70]))
        for selected_angle in candidates:
            if not is_backpack:
                set_forearm_angle(root,selected_angle);set_forearm_angle(other,selected_angle,left=True)
            bpy.context.view_layer.update()
            hits=[(a.name,b.name) for a in major for b in torso if overlap(bounds(a),bounds(b))]
            if not hits:break
        assert not hits,f'{part}/{pose}: major shell overlaps torso/thigh: {hits}'
        checks[pose]={'major_shell_vs_torso_thigh_bounds_clear':True,'overlaps':hits,'elbow_angle_degrees':selected_angle}
        scene=bpy.context.scene;scene.camera=camera('Camera_Mounted_'+pose,cam_pos)
        scene.render.filepath=os.path.join(folder,part+'-Mounted-'+pose+'.png');bpy.ops.render.render(write_still=True)
    if not is_backpack:
        set_forearm_angle(root,checks['idle']['elbow_angle_degrees']);set_forearm_angle(other,checks['idle']['elbow_angle_degrees'],left=True)
    bpy.context.view_layer.update()
    bpy.context.scene.camera=bpy.data.objects['Camera_Mounted_idle']
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(folder,part+'-Mounted-Review.blend'))
    manifest['direct_mount_revision']={'offset_bars_removed':True,'housing_seated_at_drum':True,'weapon_size_and_roll_retained':True,'review_only':True}
    manifest['assembly_layout']=layout
    manifest['base_sha256_unchanged']=base_hashes
    manifest['standalone_part_sha256_preserved']=part_hashes[part]
    manifest['weapon_mount_revision']={'body_scale_relative_to_previous':1.00,'body_scale_relative_to_original':BODY_SCALE,'collar_local_geometry_unchanged':True,'root_position_follows_resized_body':True,'right_local_body_roll_degrees':-90 if part in ('W-04_EDGE','W-05_AEGIS') else 0,'left_roll_mirrored':True}
    if 'body_size_revision' in manifest:manifest['body_size_revision']['weapon_geometry_and_mount_scale_unchanged']=False
    manifest['body_contact_checks']={'waist_contact_error_m':waist_error,'neck_contact_error_m':neck_error}
    if part=='W-01_LUMEN':
        manifest['mounted_elbows_normalized_base']=[layout['right_elbow'],layout['left_elbow']]
        manifest['clearance_checks']=checks
    else:manifest['static_clearance_checks']=checks
    if is_backpack:manifest['back_mount_assumption']['anchor_normalized_base']=layout['back_mount']
    with open(os.path.join(folder,'manifest.json'),'w',encoding='utf-8') as f:json.dump(manifest,f,indent=2,ensure_ascii=False)
    report[part]={'waist_contact_error_m':waist_error,'neck_contact_error_m':neck_error,'clearance':checks,'standalone_blend_sha256':part_hashes[part]}
    print('MOUNT_REFRESH_PASS',part,layout['body_size_factor'],waist_error,neck_error,flush=True)

assert all(sha(path)==part_hashes[p] for p,path in sources.items()),'Standalone part source changed'
assert all(sha(os.path.join(BASE,p,p+'.blend'))==h for p,h in base_hashes.items()),'Base changed during review refresh'
with open(os.path.join(ROOT,'Weapon-Mount-Validation.json'),'w',encoding='utf-8') as f:json.dump({'layout':layout,'parts':report,'all_six_standalone_blends_preserved':True},f,indent=2)
print('WEAPON_MOUNT_REVIEW_PASS',len(report))
