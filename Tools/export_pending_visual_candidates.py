"""Export pending Blender-only visual candidates under collision-free runtime IDs.

The original .blend files and existing runtime assets are read-only.  Lower body
mesh selection is mapped to explicit quadruped pivots; other candidate surfaces
remain rigidly attached to the shared art root as their source manifests specify.
"""
import bpy, json, os, sys, uuid
from mathutils import Vector

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
OUT = os.path.join(ROOT, "Assets/Resources/WispFamily20261112")
PARTS = {
    "VIS_B02_SKYWING": ("B-02_SKYWING", "backpack", "B-02_SKYWING/B-02_SKYWING.blend", None),
    "VIS_B03_CARGO_PACK": ("B-03_CARGO_PACK", "backpack", "B-03_CARGO_PACK/B-03_CARGO_PACK.blend", None),
    "VIS_L02_BASTION_TRACK": ("L-02_BASTION_TRACK", "lower_body", "L-02_BASTION_TRACK/L-02_BASTION_TRACK.blend", .505),
    "VIS_L03_TARANTULAN": ("L-03_TARANTULAN", "lower_body", "L-03_TARANTULAN/L-03_TARANTULAN.blend", .505),
    "VIS_L04_DRAFT_HOVER": ("L-04_DRAFT_HOVER", "lower_body", "L-04_DRAFT_HOVER/L-04_DRAFT_HOVER.blend", .505),
}
STRUCTURAL_PARTS = {
    "VIS_H01_RADOME": ("H-01_RADOME", "head", "HeadCore-20261112/H-01_RADOME/H-01_RADOME.blend"),
    "VIS_H02_LONGSIGHT": ("H-02_LONGSIGHT", "head", "HeadCore-20261112/H-02_LONGSIGHT/H-02_LONGSIGHT.blend"),
    "VIS_H03_SWIFTFIN": ("H-03_SWIFTFIN", "head", "HeadCore-20261112/H-03_SWIFTFIN/H-03_SWIFTFIN.blend"),
    "VIS_H04_BULWARK": ("H-04_BULWARK", "head", "HeadCore-20261112/H-04_BULWARK/H-04_BULWARK.blend"),
    "VIS_H05_NEEDLE": ("H-05_NEEDLE", "head", "SlimHeads-20261112/H-05_NEEDLE/H-05_NEEDLE.blend"),
    "VIS_H06_LANCET": ("H-06_LANCET", "head", "SlimHeads-20261112/H-06_LANCET/H-06_LANCET.blend"),
    "VIS_C01_STANDARD": ("C-01_STANDARD", "body", "HeadCore-20261112/C-01_STANDARD/C-01_STANDARD.blend"),
    "VIS_C02_LIGHTFRAME": ("C-02_LIGHTFRAME", "body", "HeadCore-20261112/C-02_LIGHTFRAME/C-02_LIGHTFRAME.blend"),
    "VIS_C03_BULWARK_CORE": ("C-03_BULWARK_CORE", "body", "HeadCore-20261112/C-03_BULWARK_CORE/C-03_BULWARK_CORE.blend"),
    "VIS_C04_ARC_CORE": ("C-04_ARC_CORE", "body", "HeadCore-20261112/C-04_ARC_CORE/C-04_ARC_CORE.blend"),
}

def unity(v): return [round(v.x, 6), round(v.z, 6), round(-v.y, 6)]

def export(runtime_id, source_id, category, relpath, waist=None):
    path = os.path.join(ROOT, "Docs/Design/WISP_Parts", relpath)
    bpy.ops.wm.open_mainfile(filepath=path)
    root = bpy.data.objects.get(source_id + "_ROOT")
    if root is None: raise RuntimeError("missing source root " + source_id)
    deps = bpy.context.evaluated_depsgraph_get()
    inverse = root.matrix_world.inverted()
    nodes = {"Root": {"name":"Root", "parent":"", "position":[0,0,0], "motion":""}}
    pivots = {"Root": Vector((0,0,0))}

    def node(name, pivot, parent="Root", motion="", side=0, front=0):
        if name not in nodes:
            pivots[name] = Vector(pivot)
            nodes[name] = {"name":name, "parent":parent, "position":unity(Vector(pivot)-pivots[parent]), "motion":motion, "side":side, "front":front}
        return name

    # The quadruped candidate includes explicit hip/knee/ankle interface centers.
    if runtime_id == "VIS_L03_TARANTULAN":
        for side, sx in (("L",-1),("R",1)):
            for front, fy in (("F",-1),("B",1)):
                tag=side+front
                hip=node(tag+"_Hip",(sx*.105,fy*.103,.374),motion="quadHip",side=sx,front=fy)
                knee=node(tag+"_Knee",(sx*.229,fy*.181,.245),parent=hip,motion="quadKnee",side=sx,front=fy)
                node(tag+"_Ankle",(sx*.281,fy*.229,.063),parent=knee,motion="quadAnkle",side=sx,front=fy)
    elif runtime_id.startswith("VIS_C"):
        # Preserve the common shoulder hinges used by the existing weapon/guard pose driver.
        node("LeftShoulder",(-.14994,.02016,0),motion="arm",side=-1)
        node("RightShoulder",(.14994,.02016,0),motion="arm",side=1)

    mats, mat_ids, groups = [], {}, {}
    mesh_objects=[o for o in bpy.data.objects if o.type=="MESH" and o.parent==root]
    if not mesh_objects: raise RuntimeError("no direct candidate meshes: "+source_id)
    def selected_node(o, center):
        if runtime_id != "VIS_L03_TARANTULAN": return "Root"
        name=o.name
        import re
        match=re.search(r"_(L|R)_(Front|Rear)(?:_|$)",name)
        if not match: return "Root"
        tag=match.group(1)+("F" if match.group(2)=="Front" else "B")
        if any(x in name for x in ("Upper_Faceted_Leg", "Hip_Drum", "Hip_Outer_Cap")): return tag+"_Hip"
        if any(x in name for x in ("Lower_Faceted_Leg", "Knee_Drum", "Knee_Outer_Cap")): return tag+"_Knee"
        if any(x in name for x in ("Ankle_Drum", "Ankle_Outer_Cap", "Foot_Pad")): return tag+"_Ankle"
        return "Root"

    def selected_structural_node(o, center):
        if not runtime_id.startswith("VIS_C"):
            return "Root"
        name=o.name.lower()
        if not any(token in name for token in ("shoulder", "upperarm", "arm_connector", "elbow")):
            return "Root"
        if "_l_" in name or name.endswith("_l"):
            return "LeftShoulder"
        if "_r_" in name or name.endswith("_r"):
            return "RightShoulder"
        return "LeftShoulder" if center.x < 0 else "RightShoulder"

    for obj in mesh_objects:
        evaluated=obj.evaluated_get(deps); mesh=evaluated.to_mesh(); mesh.calc_loop_triangles()
        transform=inverse @ obj.matrix_world
        points=[transform @ v.co for v in mesh.vertices]
        center=sum(points,Vector())/max(1,len(points))
        node_name=selected_node(obj,center) if runtime_id=="VIS_L03_TARANTULAN" else selected_structural_node(obj,center)
        pivot=pivots[node_name]
        normal_matrix=transform.to_3x3().inverted().transposed()
        for tri in mesh.loop_triangles:
            mat=obj.material_slots[min(tri.material_index,len(obj.material_slots)-1)].material if obj.material_slots else None
            key=mat.name if mat else "Default"
            if key not in mat_ids:
                color=[.5,.5,.5,1]; emission=[0,0,0]; metal=.2; rough=.5
                if mat and mat.use_nodes:
                    p=next((n for n in mat.node_tree.nodes if n.type=="BSDF_PRINCIPLED"),None)
                    if p:
                        color=list(p.inputs["Base Color"].default_value); metal=p.inputs["Metallic"].default_value; rough=p.inputs["Roughness"].default_value
                        emission=[v*p.inputs["Emission Strength"].default_value for v in p.inputs["Emission Color"].default_value[:3]]
                mat_ids[key]=len(mats);mats.append({"name":key,"color":color,"emission":emission,"metallic":metal,"smoothness":1-rough})
            mid=mat_ids[key];g=groups.setdefault((node_name,mid),{"node":node_name,"material":mid,"positions":[],"normals":[],"triangles":[]})
            start=len(g["positions"])//3
            for loop, vi in zip(tri.loops,tri.vertices):
                g["positions"]+=unity(points[vi]-pivot)
                g["normals"]+=unity((normal_matrix @ mesh.corner_normals[loop].vector).normalized())
            g["triangles"] += [start,start+1,start+2]
        evaluated.to_mesh_clear()
    data={"id":runtime_id,"category":category,"waistHeight":waist or .505,"nodes":list(nodes.values()),"materials":mats,"groups":list(groups.values())}
    if runtime_id.startswith("VIS_C"):
        manifest_path=os.path.join(os.path.dirname(path),"manifest.json")
        with open(manifest_path,encoding="utf-8") as f: interfaces=json.load(f)["body_interfaces"]
        data.update({
            "waistBottom":interfaces["waist_bottom_local_z"],
            "neckTop":interfaces["neck_contact_top_local"][2],
            "leftElbow":unity(Vector(interfaces["left_elbow_local"])),
            "rightElbow":unity(Vector(interfaces["right_elbow_local"])),
            "backMount":unity(Vector(interfaces["back_mount_local"])),
        })
    os.makedirs(OUT,exist_ok=True)
    with open(os.path.join(OUT,runtime_id+".json"),"w",encoding="utf-8") as f: json.dump(data,f,separators=(",",":"))
    guid=uuid.uuid5(uuid.NAMESPACE_URL,"voxelon:wisp-visual:"+runtime_id).hex
    with open(os.path.join(OUT,runtime_id+".json.meta"),"w",encoding="utf-8",newline="\n") as f:
        f.write("fileFormatVersion: 2\nguid: "+guid+"\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
    return {"id":runtime_id,"category":category,"resource":"WispFamily20261112/"+runtime_id,"waistHeight":waist or .505,"vertices":sum(len(g["positions"])//3 for g in groups.values()),"groups":len(groups)}

if __name__=="__main__":
    catalog_path=os.path.join(OUT,"catalog.json")
    with open(catalog_path,encoding="utf-8") as f: catalog=json.load(f)
    for runtime_id,(source_id,category,relpath,waist) in PARTS.items():
        catalog["assets"]=[a for a in catalog["assets"] if a["id"]!=runtime_id]
        catalog["assets"].append(export(runtime_id,source_id,category,relpath,waist))
        print("VISUAL_CANDIDATE_EXPORTED",runtime_id,flush=True)
    for runtime_id,(source_id,category,relpath) in STRUCTURAL_PARTS.items():
        catalog["assets"]=[a for a in catalog["assets"] if a["id"]!=runtime_id]
        catalog["assets"].append(export(runtime_id,source_id,category,relpath))
        print("VISUAL_CANDIDATE_EXPORTED",runtime_id,flush=True)
    with open(catalog_path,"w",encoding="utf-8") as f: json.dump(catalog,f,indent=2)
    print("VISUAL_CANDIDATE_EXPORT_PASS",len(PARTS)+len(STRUCTURAL_PARTS),flush=True)
