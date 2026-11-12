"""PD review: larger weapon bodies, rolled blade/shield; fixed elbow collars."""
import math
from mathutils import Matrix

BODY_SCALE=1.95
ELBOW_SCALE=.42

def style_weapon(root, collection, part, left=False):
    # Apply only to the housing, never to the existing mating collar or root.
    angle=(90 if left else -90) if part in ('W-04_EDGE','W-05_AEGIS') else 0
    transform=Matrix.Rotation(math.radians(angle),4,'Y') @ Matrix.Scale(BODY_SCALE,4)
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
            'preserved_collar_objects':fixed}
