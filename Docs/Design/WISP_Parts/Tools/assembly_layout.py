"""Resolve mounted review positions from saved body interfaces, not old offsets."""
import os
import json

def load_layout(base_dir,waist_height=None):
    def read(part):
        with open(os.path.join(base_dir,part,'manifest.json'),encoding='utf-8') as f:return json.load(f)
    bd,hd,lg=read('BD01'),read('HD01'),read('LG01')
    interfaces=bd['assembly_interfaces']
    waist=list(lg['pelvis_hip_connection']['waist_mating_center_m'])
    if waist_height is not None:waist[2]=waist_height
    body_z=waist[2]-interfaces['waist_bottom_local_z']
    neck=interfaces['neck_contact_top_local']
    head_bottom=hd['bd01_mating']['head_interface_bottom_local_z_m']
    def world(local):return [local[0],local[1],local[2]+body_z]
    return {
        'body_translation':[0,0,body_z],
        'head_translation':[neck[0],neck[1]-.003,neck[2]+body_z-head_bottom],
        'right_elbow':world(interfaces['right_elbow_local']),
        'left_elbow':world(interfaces['left_elbow_local']),
        'back_mount':world(interfaces['back_mount_local']),
        'waist_world_z':waist[2],
        'neck_contact_world_z':neck[2]+body_z,
        'body_size_factor':bd['body_size_factor'],
        'mating_pin_dimensions_preserved':interfaces['mating_pin_dimensions_preserved']
    }
