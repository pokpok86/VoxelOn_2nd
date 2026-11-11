"""Generate the WISP blockout, save an editable Blender file, and render review views.

Run from the repository root with:
  blender --background --python Docs/Design/WISP_Base_v1/wisp_automate.py
"""
import json
import math
import os
import sys
import traceback
from datetime import datetime
from pathlib import Path

import bpy
from mathutils import Vector


HERE = Path(__file__).resolve().parent
RUNS = HERE / "runs"
VARIANT = json.loads(os.environ.get("WISP_VARIANT_JSON", "{}"))
VARIANT_ID = VARIANT.get("id", "REFERENCE")
RUN_ID = datetime.now().strftime("%Y%m%d-%H%M%S") + "-" + VARIANT_ID
OUT = RUNS / RUN_ID
OUT.mkdir(parents=True, exist_ok=False)
LOG = OUT / "automation.log"
_log = open(LOG, "w", encoding="utf-8")
sys.stdout = _log
sys.stderr = _log


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def make_material(name, color, roughness=0.82):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Metallic"].default_value = 0.12
    bsdf.inputs["Roughness"].default_value = roughness
    return mat


def new_object_collection(name):
    old = bpy.data.collections.get(name)
    if old:
        for obj in list(old.objects):
            bpy.data.objects.remove(obj, do_unlink=True)
        bpy.data.collections.remove(old)
    collection = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(collection)
    return collection


def link_to(obj, collection):
    for owner in tuple(obj.users_collection):
        owner.objects.unlink(obj)
    collection.objects.link(obj)
    return obj


def create_area_light(collection, name, position, target, power, size, color=(1.0, 1.0, 1.0)):
    data = bpy.data.lights.new(name, "AREA")
    data.energy = power
    data.shape = "DISK"
    data.size = size
    data.color = color
    obj = bpy.data.objects.new(name, data)
    collection.objects.link(obj)
    obj.location = position
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()
    return obj


def create_camera(collection, name, position, target):
    data = bpy.data.cameras.new(name)
    data.type = "ORTHO"
    data.ortho_scale = 1.98
    data.lens = 50
    obj = bpy.data.objects.new(name, data)
    collection.objects.link(obj)
    obj.location = position
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()
    return obj


def run():
    print("WISP_AUTO_BEGIN", RUN_ID)
    print("BLENDER_VERSION", bpy.app.version_string)
    print("VARIANT", json.dumps(VARIANT, ensure_ascii=False, sort_keys=True))
    print("SOURCE", HERE / "wisp_blender_v2.py")
    print("GEOMETRY", HERE / "wisp_geometry.json")

    spec = json.loads((HERE / "wisp_geometry.json").read_text(encoding="utf-8"))
    require(spec.get("version") == "2.0-draft", "Unexpected geometry schema version")
    require(spec.get("units") == "meters" and spec.get("axes", {}).get("forward") == "-Y",
            "WISP coordinate convention is missing or changed")
    dimensions = {item[0]: (item[1], item[2]) for item in spec.get("dimensions", [])}
    require(all(key in dimensions for key in ("head", "torso", "forearm_gun_L", "shin_L")),
            "Geometry data is missing required WISP body parts")
    require(abs(spec.get("idle", {}).get("chest_bob_m", 0) - 0.006) < 1e-6,
            "Expected the approved 6 mm idle breathing amplitude")

    # Start clean so the default startup cube/camera/light cannot leak into captures.
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        if collection.users == 0:
            bpy.data.collections.remove(collection)

    source = HERE / "wisp_blender_v2.py"
    code = compile(source.read_text(encoding="utf-8"), str(source), "exec")
    bpy.app.driver_namespace["WISP_VARIANT"] = VARIANT
    exec(code, {"__name__": "__wisp_generator__"})
    derived = bpy.app.driver_namespace.get("WISP_DERIVED", {})
    require(abs(float(derived.get("shoulder_axis_x", 0)) - (VARIANT.get("chest_size", [0.48])[0] / 2 + .052 + float(VARIANT.get("shoulder_gap_m", .015)))) < 1e-6,
            "Shoulder axis must follow the selected chest width")
    pose = VARIANT.get("pose", "IDLE")
    require(pose in ("IDLE", "AIM", "GUARD", "CROUCH"), "Unsupported articulated pose")
    armor_color = [float(v) for v in VARIANT.get("armor_color", [.72,.73,.72])]
    frame_color = [float(v) for v in VARIANT.get("frame_color", [.055,.068,.082])]
    accent_color = [float(v) for v in VARIANT.get("accent_color", [.82,.30,.07])]
    require(.30 <= min(armor_color) and max(armor_color) <= .44 and max(frame_color) <= .06 and
            .65 <= accent_color[0] <= .95 and accent_color[1] <= .4,
            "Reference warm-gray armor, dark frame and orange accent palette must stay in range")
    wisp = bpy.data.collections.get("WISP_V2")
    require(wisp is not None and len(wisp.objects) > 0, "Generator did not create WISP_V2")
    names = {obj.name for obj in wisp.objects}
    required = {"ROOT", "Pelvis_Frame", "Waist_Yaw_Z", "UpperBody_BreathIdle", "Chest_Armor", "Chest_Accent_Panel",
                "Head_FIXED", "Head_Small", "Face_Aperture", "Face_Visor", "Neck_Link",
                "Shoulder_L_PitchX", "Shoulder_R_PitchX", "Shoulder_Armor_L", "Shoulder_Armor_R",
                "Forearm_Accent_Panel_L", "Forearm_Accent_Panel_R",
                "Shoulder_Connector_Short_L", "Shoulder_Connector_Short_R",
                "Shoulder_L_Axle", "Shoulder_R_Axle", "Shoulder_L_Can", "Shoulder_R_Can",
                "Hip_L_Axle", "Hip_R_Axle", "Hip_L_Can", "Hip_R_Can",
                "Pelvis_Hip_Form_Link_L", "Pelvis_Hip_Form_Link_R",
                "ForearmGun_L", "ForearmGun_R", "Knee_L_PitchX", "Knee_R_PitchX",
                "Knee_Armor_L", "Knee_Armor_R", "Shin_L", "Shin_R", "Socket_Backpack"}
    missing = sorted(required - names)
    require(not missing, "Generated model misses named objects: " + ", ".join(missing))
    parts = {obj.name: obj for obj in wisp.objects}
    shoulder_armor_names = {name for name in names if name.startswith("Shoulder_Armor_")}
    require(shoulder_armor_names == {"Shoulder_Armor_L", "Shoulder_Armor_R"},
            "Reference armor caps should be present as a mirrored pair")
    for side in ("L", "R"):
        require(parts["Shoulder_Armor_"+side].parent.name == "Shoulder_"+side+"_AbductY",
                "Shoulder armor should move with the arm while leaving the can joint articulated")
    for joint_name in ("Shoulder_L_Can", "Hip_L_Can"):
        side_faces = [poly for poly in parts[joint_name].data.polygons if len(poly.vertices) == 4]
        require(len(side_faces) == 24,
                joint_name + " should remain a round 24-sided cylindrical can")
    for joint_name in ("Elbow_Joint_L", "KneeJoint_L", "AnkleJoint_L"):
        side_faces = [poly for poly in parts[joint_name].data.polygons if len(poly.vertices) == 4]
        require(len(side_faces) == 12,
                joint_name + " should remain a cylindrical joint cap")
    require(-.5 <= float(VARIANT.get("pelvis_armor_level", -.10)) <= .5 and
            -.5 <= float(VARIANT.get("torso_armor_level", .10)) <= .5,
            "Torso and pelvis material thresholds must remain within the generated shell")
    chest_armor_ratio = float(VARIANT.get("chest_armor_height_ratio", .82))
    require(0.0 < chest_armor_ratio <= 1.0,
            "The abdomen chest-armor shell must be retained but shortened")
    require(float(VARIANT.get("foot_offset_y", -.055)) > -.055,
            "Feet must shift rearward from their previous position")
    require(abs(float(VARIANT.get("forearm_size", [.22,.205,.35])[2]) / 2 - .005 - float(VARIANT.get("forearm_drop", .17))) < .002,
            "Reduced forearm top must still meet the elbow center")
    parts = {obj.name: obj for obj in wisp.objects}
    bpy.context.view_layer.update()
    torso_top_z = float(VARIANT.get("torso_center_z", .795)) + float(VARIANT.get("torso_height", .20)) / 2
    head_bottom_z = float(VARIANT.get("head_center_z", 1.395)) - float(VARIANT.get("head_size", [.16,.145,.125])[2]) / 2
    neck_emergence = head_bottom_z - (torso_top_z + .010)
    require(.014 <= neck_emergence <= .018,
            "Thin neck should emerge only 16 mm between the torso and head shells")
    if pose == "AIM":
        require(abs(parts["Shoulder_L_PitchX"].rotation_euler.x-math.radians(-62)) < 1e-5 and
                abs(parts["Elbow_L_PitchX"].rotation_euler.x-math.radians(-18)) < 1e-5,
                "AIM pose must articulate shoulder and elbow pivots")
    elif pose == "GUARD":
        require(abs(parts["Elbow_L_PitchX"].rotation_euler.x-math.radians(-72)) < 1e-5 and
                abs(parts["Shoulder_R_PitchX"].rotation_euler.x-math.radians(-55)) < 1e-5,
                "GUARD pose must articulate asymmetrical arm joints")
    elif pose == "CROUCH":
        require(abs(parts["Hip_L_PitchX"].rotation_euler.x-math.radians(-15)) < 1e-5 and
                abs(parts["Knee_L_PitchX"].rotation_euler.x-math.radians(32)) < 1e-5 and
                abs(parts["Ankle_L_PitchX"].rotation_euler.x-math.radians(-17)) < 1e-5,
                "CROUCH pose must articulate hip, knee and ankle pivots")
    head_size = [float(v) for v in VARIANT.get("head_size", [.16,.145,.125])]
    require(abs(head_size[0]-.132) < 1e-5 and abs(head_size[1]-.113) < 1e-5 and abs(head_size[2]-.108) < 1e-5,
            "Head dimensions must match the slightly reduced 6 percent silhouette")
    torso_bottom = float(VARIANT.get("torso_center_z", .795)) - float(VARIANT.get("torso_height", .20)) / 2
    pelvis_top = float(VARIANT.get("pelvis_center_z", .585)) + float(VARIANT.get("pelvis_size", [.34,.23,.17])[2]) / 2
    pelvis_bottom = float(VARIANT.get("pelvis_center_z", .585)) - float(VARIANT.get("pelvis_size", [.34,.23,.17])[2]) / 2
    hip_z = float(VARIANT.get("hip_z", .685))
    require(abs(torso_bottom - pelvis_top) < 1e-5,
            "Pelvis top must meet the unchanged waist bottom")
    hip_inset = hip_z - pelvis_bottom
    require(.035 <= hip_inset <= .045,
            "Hip centers must sit 40 mm inside the pelvis lower edge, not below it")
    pelvis_width = float(VARIANT.get("pelvis_size", [.34,.23,.17])[0])
    pelvis_height = float(VARIANT.get("pelvis_size", [.34,.23,.17])[2])
    require(.13 <= pelvis_width <= .15 and abs(pelvis_height-.16) < 1e-5,
            "Pelvis must stay narrow and be reduced to two-thirds of its previous height")
    pelvis_depth = float(VARIANT.get("pelvis_size", [.34,.23,.17])[1])
    require(abs(pelvis_depth - .075) <= .005,
            "Pelvis bridge depth should match the adjoining thigh section")
    pelvis_profile = {"TAPERED_HIP": [(-.50,.70),(-.24,.94),(.02,.98),(.27,.82),(.50,.64)],
                      "FLARED_HIP": [(-.50,.62),(-.30,.84),(-.06,1.0),(.20,.94),(.50,.70)],
                      "CRESTED_HIP": [(-.50,.58),(-.25,.86),(.04,1.0),(.30,.96),(.50,.72)],
                      "HIP_BRIDGE": [(-.50,.84),(-.36,.96),(.36,.96),(.50,.84)]}[VARIANT.get("pelvis_form", "TAPERED_HIP")]
    hip_profile_z = (hip_z - float(VARIANT.get("pelvis_center_z", .585))) / pelvis_height
    lower = max((point for point in pelvis_profile if point[0] <= hip_profile_z), key=lambda point: point[0])
    upper = min((point for point in pelvis_profile if point[0] >= hip_profile_z), key=lambda point: point[0])
    t = (hip_profile_z-lower[0])/(upper[0]-lower[0]) if upper[0] != lower[0] else 0.0
    pelvis_half_width_at_hip = pelvis_width*.5*(lower[1]+(upper[1]-lower[1])*t)
    require(.05 <= pelvis_half_width_at_hip < .11,
            "Narrow pelvis at hip height must leave a short embedded connector")
    for side, sign in (("L", -1), ("R", 1)):
        link = parts["Pelvis_Hip_Form_Link_"+side]
        require(link.parent.name == "Pelvis_Frame",
                "Visual pelvis-to-hip bridge must remain attached to the pelvis shape")
        expected_center_x = sign*(pelvis_half_width_at_hip+.11)/2
        require(abs(float(link.matrix_world.translation.x)-expected_center_x) < .002 and
                abs(float(link.matrix_world.translation.z)-hip_z) < 1e-5,
                "Pelvis form link must sit inside the shell at the raised hip center")
    chest_mesh_z = [float(vertex.co.z) for vertex in parts["Chest_Armor"].data.vertices]
    require(abs((max(chest_mesh_z) - min(chest_mesh_z)) -
                float(VARIANT.get("torso_height", .20)) * chest_armor_ratio * .92) < 1e-5,
            "Chest armor height must match the shortened design ratio")
    expected_chest_top = (float(VARIANT.get("torso_center_z", .795)) +
                          float(VARIANT.get("torso_height", .20)) / 2 + .002)
    chest_world_top = max(float((parts["Chest_Armor"].matrix_world @ vertex.co).z)
                          for vertex in parts["Chest_Armor"].data.vertices)
    require(abs(chest_world_top - expected_chest_top) < 1e-5,
            "Shortening the abdomen armor must preserve its upper edge")
    shoulder_can = parts["Shoulder_L_Can"].dimensions
    require(abs(float(shoulder_can.x)-.066) < .001 and
            abs(float(shoulder_can.z)-.066) < .001,
            "Cylindrical shoulder can diameter and depth must match the upper-arm section")
    expected_forearm_drop = float(VARIANT.get("forearm_size", [.22,.205,.35])[2]) / 2 - .005
    require(abs(float(VARIANT.get("forearm_drop", expected_forearm_drop)) - expected_forearm_drop) < .002,
            "Forearm upper end must reach the elbow pivot with only a 5 mm overlap")
    for side in ("L", "R"):
        require(parts["Shoulder_"+side+"_Can"].parent.name == "Shoulder_"+side+"_AbductY",
                "Shoulder tuna-can drum must follow the articulated limb pivots")
        require((parts["Shoulder_"+side+"_Can"].matrix_world.translation -
                 parts["Shoulder_"+side+"_AbductY"].matrix_world.translation).length < 1e-6,
                "Shoulder drum center must be exactly at the torso-side shoulder pivot")
        shoulder_z = 1.245 + float(VARIANT.get("torso_center_z", 1.05)) + float(VARIANT.get("torso_height", .40)) / 2 - 1.25 + float(VARIANT.get("shoulder_z_offset", 0.0))
        back_shift = float(VARIANT.get("joint_back_shift_m", .02))
        expected_shoulder = Vector(((-1 if side == "L" else 1) * (float(VARIANT.get("chest_size", [.48])[0]) / 2 + .052 + float(VARIANT.get("shoulder_gap_m", .015))), -.02 + back_shift, shoulder_z))
        require((parts["Shoulder_"+side+"_AbductY"].matrix_world.translation - expected_shoulder).length < 1e-6,
                "Shoulder pivot must be at the torso side/center-depth joint, not the front surface")
        require(parts["Shoulder_"+side+"_Axle"].parent.name == "UpperBody_BreathIdle",
                "Shoulder axle must be fixed to the upper body")
        connector = parts["Shoulder_Connector_Short_"+side]
        require(connector.parent.name == "UpperBody_BreathIdle",
                "Short shoulder connector must be fixed to the torso")
        expected_connector_center_x = (-1 if side == "L" else 1) * (float(VARIANT.get("chest_size", [.48])[0]) / 2 + (float(VARIANT.get("chest_size", [.48])[0]) / 2 + .052 + float(VARIANT.get("shoulder_gap_m", .015)))) / 2
        require(abs(connector.matrix_world.translation.x - expected_connector_center_x) < 1e-6,
                "Shoulder bridge must physically span the torso edge to the separated pivot")
        arm_root = parts["UpperArm_"+side].matrix_local.translation
        require(abs(arm_root.x) < 1e-6 and abs(arm_root.y) < 1e-6,
                "Upper-arm root centerline must meet the shoulder can center in its articulated frame")
        upper_arm_length = float(VARIANT.get("upper_arm_size", [.066,.066,.15])[2])
        require(.135 <= upper_arm_length <= .145,
                "Upper arm should be modestly shorter while retaining its section thickness")
        upper_arm_end_drop = float(VARIANT.get("upper_arm_center_drop", .1)) + upper_arm_length / 2
        elbow_drop = float(VARIANT.get("elbow_drop", .18))
        require(abs((elbow_drop-upper_arm_end_drop)-.010) < .002,
                "Shortened upper arm should stop 10 mm before the elbow-can center")
        elbow_pivot_obj = parts["Elbow_"+side+"_PitchX"]
        elbow_pivot = elbow_pivot_obj.matrix_world.translation
        require((parts["Elbow_Joint_"+side].matrix_world.translation - elbow_pivot).length < 1e-6,
                "Elbow cylinder center must be the elbow articulation point")
        elbow_root = elbow_pivot_obj.matrix_local.translation
        forearm_root = parts["ForearmGun_"+side].matrix_local.translation
        require(abs(elbow_root.x) < 1e-6 and abs(elbow_root.y) < 1e-6 and
                abs(forearm_root.x) < 1e-6 and abs(forearm_root.y) < 1e-6,
                "Elbow pivot and forearm root must share one centerline")
        require(parts["Hip_"+side+"_Can"].parent.name == "Hip_"+side+"_AbductY",
                "Hip tuna-can drum must follow the articulated leg pivots")
        require((parts["Hip_"+side+"_Can"].matrix_world.translation -
                 parts["Hip_"+side+"_AbductY"].matrix_world.translation).length < 1e-6,
                "Hip drum center must be exactly at the pelvis-side hip pivot")
        expected_hip = Vector(((-1 if side == "L" else 1) * .11, float(VARIANT.get("joint_back_shift_m", .02)), float(VARIANT.get("hip_z", .685))))
        require((parts["Hip_"+side+"_AbductY"].matrix_world.translation - expected_hip).length < 1e-6,
                "Hip pivot must be at pelvis center-depth, not the front surface")
        require(parts["Hip_"+side+"_Axle"].parent.name == "Pelvis_Frame",
                "Hip axle must be fixed to the pelvis")
        thigh_root = parts["Thigh_"+side].matrix_local.translation
        require(abs(thigh_root.x) < 1e-6 and abs(thigh_root.y) < 1e-6,
                "Thigh root centerline must meet the hip can center in its articulated frame")
        knee_pivot = parts["Knee_"+side+"_PitchX"].matrix_world.translation
        require((parts["KneeJoint_"+side].matrix_world.translation - knee_pivot).length < 1e-6,
                "Knee cylinder center must be the knee articulation point")
        knee_root = parts["Knee_"+side+"_PitchX"].matrix_local.translation
        shin_root = parts["Shin_"+side].matrix_local.translation
        require(abs(knee_root.x) < 1e-6 and abs(knee_root.y) < 1e-6 and
                abs(shin_root.x) < 1e-6 and abs(shin_root.y) < 1e-6,
                "Knee pivot and shin root must share one centerline")
        hip_can_size = parts["Hip_"+side+"_Can"].dimensions
        require(abs(float(hip_can_size.x)-.082) < .001 and
                abs(float(hip_can_size.z)-.075) < .001,
                "Cylindrical hip can diameter and depth must match the thigh section")
    require(not any(name.startswith("Face_Lens_") for name in names),
            "Separate eye lenses must remain absent; use one glass visor strip")
    decorative_lines = {"Orange_Stripe", "Chest_Vent_L", "Chest_Vent_R", "Gun_Rail_L",
                        "Gun_Rail_R", "Cannon_Outer_Rail_L", "Cannon_Outer_Rail_R",
                        "Shin_Front_Plate_L", "Shin_Front_Plate_R", "Toe_Accent_L",
                        "Toe_Accent_R", "Backpack_Status_Light"}
    unexpected_lines = sorted(decorative_lines & names)
    require(not unexpected_lines, "Blockout still contains bolt-on line detail: " + ", ".join(unexpected_lines))
    forbidden_mounts = {"Shoulder_Body_Socket_L", "Shoulder_Body_Socket_R",
                        "Shoulder_Joint_L", "Shoulder_Joint_R",
                        "Shoulder_Connector_Upper_L", "Shoulder_Connector_Upper_R",
                        "Shoulder_Connector_Lower_L", "Shoulder_Connector_Lower_R"}
    require(not (forbidden_mounts & names), "Shoulder still has detached body connectors")
    if VARIANT.get("muzzle_enabled", True):
        require({"Muzzle_Glass_L", "Muzzle_Glass_R"} <= names,
                "Flush terminal muzzle glass is missing")
        require(not any(name.startswith(("Muzzle_Recess_", "Muzzle_Collar_", "Muzzle_Bore_", "Muzzle_Emitter_", "Socket_Muzzle_")) for name in names),
                "Forearm must not have a dark protruding muzzle mechanism")
    else:
        require(not any(name.startswith(("Muzzle_", "Socket_Muzzle_")) for name in names),
                "Muzzle protrusion must be absent for this variant")

    bpy.context.scene.unit_settings.system = "METRIC"
    render_collection = new_object_collection("WISP_REVIEW_STUDIO")
    floor_mat = make_material("Review_Floor", (0.035, 0.046, 0.062))
    bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -0.015))
    floor = link_to(bpy.context.object, render_collection)
    floor.name = "Review_Ground"
    floor.data.materials.append(floor_mat)

    target = (0, 0, 0.79)
    create_area_light(render_collection, "Key_Light", (-3.2, -4.2, 4.3), target, 600, 3.0, (.82, .86, 1.0))
    create_area_light(render_collection, "Fill_Light", (3.2, -4.2, 4.3), target, 600, 3.0, (.82, .86, 1.0))
    create_area_light(render_collection, "Rim_Light", (0.0, 3.0, 3.7), target, 900, 2.5, (.72, .88, 1.0))

    world = bpy.data.worlds.new("WISP_Review_World") if bpy.data.worlds.get("WISP_Review_World") is None else bpy.data.worlds["WISP_Review_World"]
    world.use_nodes = True
    world.node_tree.nodes.get("Background").inputs["Color"].default_value = (0.018, 0.024, 0.035, 1)
    world.node_tree.nodes.get("Background").inputs["Strength"].default_value = 0.38
    bpy.context.scene.world = world

    views = [
        ("01-front.png", (0, -4.6, 1.18)),
        ("02-right.png", (4.6, 0, 1.18)),
        ("03-back.png", (0, 4.6, 1.18)),
        ("04-three-quarter-45.png", (3.25, -3.25, 1.18)),
    ]
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.eevee.taa_render_samples = 64
    scene.render.resolution_x = 1200
    scene.render.resolution_y = 1200
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.view_settings.view_transform = "AgX"
    scene.render.film_transparent = False
    scene.frame_set(1)

    for filename, position in views:
        camera = create_camera(render_collection, "Camera_" + filename.replace(".png", ""), position, target)
        scene.camera = camera
        scene.render.filepath = str(OUT / filename)
        bpy.ops.render.render(write_still=True)
        image_path = OUT / filename
        require(image_path.exists() and image_path.stat().st_size > 10_000,
                "Render missing or unexpectedly small: " + filename)
        print("RENDER_PASS", filename, image_path.stat().st_size)

    # Keep the 45-degree review camera active in the editable file.
    scene.camera = next(obj for obj in render_collection.objects if obj.name == "Camera_04-three-quarter-45")
    scene.render.filepath = str(OUT / "04-three-quarter-45.png")
    blend_path = OUT / "WISP_Base_v2.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    require(blend_path.exists() and blend_path.stat().st_size > 50_000, "Editable .blend file was not saved")

    manifest = {
        "run_id": RUN_ID,
        "pose": pose,
        "palette": {"armor_rgb": armor_color, "frame_rgb": frame_color, "accent_rgb": accent_color, "visor": "CYAN_RETAINED"},
        "joint_cylinder_sides": 24,
        "joint_cap_sides": 12,
        "shoulder_armor_status": "MIRRORED_OUTER_CAPS_ADDED",
        "accent_panels": sorted(name for name in names if "Accent_Panel" in name),
        "pose_joints": {name: round(math.degrees(float(parts[name].rotation_euler.x)), 3) for name in ("Shoulder_L_PitchX", "Elbow_L_PitchX", "Hip_L_PitchX", "Knee_L_PitchX", "Ankle_L_PitchX")},
        "variant": VARIANT,
        "derived": derived,
        "joint_centers_world": {
            name: [round(float(v), 6) for v in parts[name].matrix_world.translation]
            for name in ("Shoulder_L_AbductY", "Shoulder_R_AbductY", "Hip_L_AbductY", "Hip_R_AbductY")
        },
        "blender_version": bpy.app.version_string,
        "model_objects": len(wisp.objects),
        "required_hierarchy_objects": sorted(required),
        "bolt_on_line_geometry": "REMOVED",
        "shoulder_armor": "MIRRORED_REFERENCE_CAPS",
        "shoulder_can_local_dimensions": [round(float(v), 6) for v in parts["Shoulder_L_Can"].dimensions],
        "shoulder_can_front_to_back_depth": round(float(parts["Shoulder_L_Can"].dimensions.z), 6),
        "upper_arm_dimensions": [round(float(v), 6) for v in parts["UpperArm_L"].dimensions],
        "hip_can_local_dimensions": [round(float(v), 6) for v in parts["Hip_L_Can"].dimensions],
        "thigh_dimensions": [round(float(v), 6) for v in parts["Thigh_L"].dimensions],
        "shoulder_pivot_inset_from_chest_edge": round(float(VARIANT.get("chest_size", [.48])[0]) / 2 - float(derived["shoulder_axis_x"]), 6),
        "limb_and_joint_rearward_shift_m": round(float(VARIANT.get("joint_back_shift_m", .02)), 6),
        "requested_forearm_dimensions": list(VARIANT.get("forearm_size", [.22,.205,.35])),
        "requested_foot_dimensions": list(VARIANT.get("foot_size", [.175,.245,.13])),
        "requested_pelvis_dimensions": list(VARIANT.get("pelvis_size", [.34,.23,.17])),
        "pelvis_top_z": round(float(VARIANT.get("pelvis_center_z", .585)) + float(VARIANT.get("pelvis_size", [.34,.23,.17])[2]) / 2, 6),
        "pelvis_bottom_z": round(float(VARIANT.get("pelvis_center_z", .585)) - float(VARIANT.get("pelvis_size", [.34,.23,.17])[2]) / 2, 6),
        "pelvis_height_m": round(float(VARIANT.get("pelvis_size", [.34,.23,.17])[2]), 6),
        "pelvis_width_m": round(float(VARIANT.get("pelvis_size", [.34,.23,.17])[0]), 6),
        "pelvis_depth_m": round(float(VARIANT.get("pelvis_size", [.34,.23,.17])[1]), 6),
        "pelvis_half_width_at_hip_m": round(pelvis_half_width_at_hip, 6),
        "hip_center_z_m": round(hip_z, 6),
        "hip_inset_from_pelvis_bottom_m": round(hip_inset, 6),
        "leg_scale": round(float(VARIANT.get("leg_scale", 1.0)), 6),
        "head_center_z_m": round(float(VARIANT.get("head_center_z", 1.395)), 6),
        "neck_visible_emergence_m": round(neck_emergence, 6),
        "neck_radius_m": round(float(VARIANT.get("neck_radius", .035)), 6),
        "requested_head_dimensions": list(VARIANT.get("head_size", [.16,.145,.125])),
        "upper_arm_length_m": round(float(VARIANT.get("upper_arm_size", [.066,.066,.15])[2]), 6),
        "pelvis_link_centers_world": {side: [round(float(parts["Pelvis_Hip_Form_Link_"+side].matrix_world.translation.x), 6), round(float(parts["Pelvis_Hip_Form_Link_"+side].matrix_world.translation.y), 6), round(float(parts["Pelvis_Hip_Form_Link_"+side].matrix_world.translation.z), 6)] for side in ("L", "R")},
        "foot_rearward_offset_y": round(float(VARIANT.get("foot_offset_y", -.055)) - (-.055), 6),
        "shoulder_joint_gap_from_torso": round(float(VARIANT.get("shoulder_gap_m", .015)), 6),
        "abdomen_armor_status": "SHORTENED_PANEL_RETAINED",
        "chest_armor_dimensions": [round(float(v), 6) for v in parts["Chest_Armor"].dimensions],
        "chest_armor_height_ratio": chest_armor_ratio,
        "articulation_centerlines": "PASS: shoulder can, upper-arm root and elbow pivot share the articulated X/Y centerline; elbow can/forearm, hip can/thigh, knee can/shin are centered",
        "blend": str(blend_path),
        "images": [str(OUT / item[0]) for item in views],
        "automation": "PASS",
        "design_review": "PENDING_PD_VISUAL_REVIEW",
        "note": "Successful generation and rendering do not constitute design approval.",
    }
    (OUT / "manifest.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print("WISP_AUTO_PASS", json.dumps(manifest, ensure_ascii=False))


try:
    run()
except Exception:
    traceback.print_exc()
    print("WISP_AUTO_FAIL")
    raise
finally:
    _log.flush()
    _log.close()
