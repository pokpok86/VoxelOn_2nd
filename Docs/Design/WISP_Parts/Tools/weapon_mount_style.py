"""PD review: larger weapon bodies, rolled blade/shield; fixed elbow collars."""
import math
import bpy
from mathutils import Matrix, Vector

BODY_SCALE=1.56
ELBOW_SCALE=.42

def style_weapon(root, collection, part, left=False):
    # Seat the housing directly against the original drum, without an offset bar.
    angle=(90 if left else -90) if part in ('W-04_EDGE','W-05_AEGIS') else 0
    transform=Matrix.Rotation(math.radians(angle),4,'Y') @ Matrix.Scale(BODY_SCALE,4)
    meshes=[o for o in collection.objects if o.type=='MESH' and o.parent==root]
    removed=[]
    for obj in meshes[:]:
        if 'Offset_Link' in obj.name or 'Offset_Bridge' in obj.name:
            removed.append(obj.name);meshes.remove(obj);bpy.data.objects.remove(obj,do_unlink=True)
    core=next(o for o in meshes if any(t in o.name for t in
        ('Structural_Barrel_Core','Main_Structural_Housing','Longitudinal_Blade_Chassis','Shield_Structural_Core')))
    corners=[core.matrix_basis @ Vector(v) for v in core.bound_box]
    rear=max(v.y for v in corners)
    center_z=(min(v.z for v in corners)+max(v.z for v in corners))/2
    shift=Vector((0,-.035/BODY_SCALE-rear,-center_z))
    transform=transform @ Matrix.Translation(shift)
    fixed=[]
    for obj in collection.objects:
        if obj.type!='MESH' or obj.parent!=root:continue
        if 'Common_Elbow_' in obj.name or 'Elbow_' in obj.name:
            fixed.append(obj.name)
            continue
        obj.matrix_basis=transform @ obj.matrix_basis
    assert fixed,'Elbow collar classification found no preserved objects'
    return {'body_scale_relative_to_original':BODY_SCALE,
            'root_and_collar_scale':ELBOW_SCALE,
            'effective_body_scale':ELBOW_SCALE*BODY_SCALE,
            'local_longitudinal_roll_degrees':angle,
            'direct_seat_shift_local':list(shift),
            'offset_bars_removed':removed,
            'preserved_collar_objects':fixed}
