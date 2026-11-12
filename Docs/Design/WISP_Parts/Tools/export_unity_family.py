"""Export evaluated Blender surfaces and explicit animation pivots to Unity buffers."""
import bpy,json,os,sys,math,hashlib
from mathutils import Vector
HERE=os.path.dirname(os.path.abspath(__file__))
ROOT=os.path.dirname(HERE);BASE=os.path.abspath(os.path.join(ROOT,'../WISP_Base_v1'))
REPO=os.path.abspath(os.path.join(ROOT,'../../..'))
OUT=os.path.join(REPO,'Assets/Resources/WispFamily20261112');os.makedirs(OUT,exist_ok=True)
with open(os.path.join(ROOT,'APPROVED_BASELINE.json')) as f:baseline=json.load(f)
def sha(p):
    with open(p,'rb') as f:return hashlib.sha256(f.read()).hexdigest()
def check_base():
    for p,h in baseline['geometry_sha256'].items():assert sha(os.path.join(os.path.dirname(ROOT),p))==h,p
check_base()
def unity(p):return [round(p[0],6),round(p[2],6),round(-p[1],6)]
catalog=[]
def export(asset,path,category,waist=.505):
    bpy.ops.wm.open_mainfile(filepath=path)
    deps=bpy.context.evaluated_depsgraph_get()
    if asset=='BD01':objects=[o for o in bpy.data.objects if o.type=='MESH' and any(c.name=='BD01_BODY_AND_SHOULDERS' for c in o.users_collection)];origin=Vector((0,0,0))
    elif asset=='HD01':objects=[o for o in bpy.data.objects if o.type=='MESH' and any(c.name=='HD01_SENSOR_HEAD' for c in o.users_collection)];origin=Vector((0,0,0))
    else:
        root=bpy.data.objects[asset+'_ROOT'];objects=[o for o in bpy.data.objects if o.type=='MESH' and o.parent==root]
        origin=root.matrix_world.translation if category=='weapon' else Vector((0,0,0))
    inverse=bpy.data.objects[asset+'_ROOT'].matrix_world.inverted() if category=='weapon' else None
    nodes={'Root':{'name':'Root','parent':'','position':[0,0,0],'motion':''}}
    pivots={'Root':Vector((0,0,0))};materials=[];material_ids={};groups={}
    def node(name,pivot,parent='Root',motion='',side=0,front=0):
        if name not in nodes:
            pivots[name]=Vector(pivot);nodes[name]={'name':name,'parent':parent,'position':unity(Vector(pivot)-pivots[parent]),'motion':motion,'side':side,'front':front}
        return name
    if asset=='BD01':
        for side in ('L','R'):
            p=bpy.data.objects['BD01_'+side+'_SHOULDER_PITCH_X'].matrix_world.translation
            node('LeftShoulder' if side=='L' else 'RightShoulder',p,motion='arm',side=-1 if side=='L' else 1)
    def choose(o,center):
        n=o.name
        if asset=='BD01':return 'LeftShoulder' if n.startswith('L_UpperArm_') else 'RightShoulder' if n.startswith('R_UpperArm_') else 'Root'
        if asset=='L-01_STRIDER' and not any(x in n for x in ('Pelvis','Waist_Socket')):
            side=-1 if center.x<0 else 1;s='L' if side<0 else 'R'
            hip=node(s+'_Hip',(side*.080,0,.422),motion='hip',side=side)
            knee=node(s+'_Knee',(side*.085,0,.248),hip,'knee',side)
            ankle=node(s+'_Ankle',(side*.090,0,.069),knee,'ankle',side)
            return ankle if any(x in n for x in ('Foot','Ankle_')) else knee if any(x in n for x in ('Shin','Knee_')) else hip
        if asset=='L-03_TARANTULAN' and any(x in n for x in ('Spider_Upper_','Spider_Hip_','Spider_Knee_','Long_Pointed_Claw','Broad_Lower_Spider','Spider_Amber_Joint')):
            side=-1 if center.x<0 else 1;front=-1 if center.y<0 else 1;tag=('L' if side<0 else 'R')+('F' if front<0 else 'B')
            hip=node(tag+'_Hip',(side*.102,front*.094,.391),motion='quadHip',side=side,front=front)
            knee=node(tag+'_Knee',(side*.213,front*.157,.281),hip,'quadKnee',side,front)
            return knee if any(x in n for x in ('Spider_Knee_','Long_Pointed_Claw','Broad_Lower_Spider')) else hip
        if asset=='L-02_BASTION_TRACK' and any(x in n for x in ('Road_Wheel','Outer_Wheel_Hub','Separate_Track_Shoe')):
            return node(n,center,motion='tread' if 'Track_Shoe' in n else 'wheel',side=-1 if center.x<0 else 1)
        return 'Root'
    for o in objects:
        evaluated=o.evaluated_get(deps);mesh=evaluated.to_mesh();mesh.calc_loop_triangles()
        transform=inverse @ o.matrix_world if inverse is not None else o.matrix_world
        points=[transform @ v.co for v in mesh.vertices];center=sum(points,Vector())/max(1,len(points))
        group_node=choose(o,center);pivot=pivots[group_node]
        normal_matrix=transform.to_3x3().inverted().transposed()
        for tri in mesh.loop_triangles:
            mat=o.material_slots[min(tri.material_index,len(o.material_slots)-1)].material if o.material_slots else None
            key=mat.name if mat else 'Default'
            if key not in material_ids:
                color=[.5,.5,.5,1];emission=[0,0,0];metal=.2;rough=.5
                if mat and mat.use_nodes:
                    p=next((n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED'),None)
                    if p:
                        color=list(p.inputs['Base Color'].default_value);metal=p.inputs['Metallic'].default_value;rough=p.inputs['Roughness'].default_value
                        emission=[v*p.inputs['Emission Strength'].default_value for v in p.inputs['Emission Color'].default_value[:3]]
                material_ids[key]=len(materials);materials.append({'name':key,'color':color,'emission':emission,'metallic':metal,'smoothness':1-rough})
            mid=material_ids[key];g=groups.setdefault((group_node,mid),{'node':group_node,'material':mid,'positions':[],'normals':[],'triangles':[]})
            start=len(g['positions'])//3
            for loop_idx,vertex_idx in zip(tri.loops,tri.vertices):
                g['positions']+=unity(points[vertex_idx]-pivot)
                normal=normal_matrix @ mesh.corner_normals[loop_idx].vector
                g['normals']+=unity(normal.normalized())
            g['triangles'] += [start,start+1,start+2]
        evaluated.to_mesh_clear()
    data={'id':asset,'category':category,'waistHeight':waist,'nodes':list(nodes.values()),'materials':materials,'groups':list(groups.values())}
    if asset=='BD01':
        with open(os.path.join(BASE,'BD01/manifest.json')) as f:m=json.load(f)
        data['leftElbow']=unity(m['assembly_interfaces']['left_elbow_local']);data['rightElbow']=unity(m['assembly_interfaces']['right_elbow_local'])
        data['waistBottom']=m['assembly_interfaces']['waist_bottom_local_z'];data['neckTop']=m['assembly_interfaces']['neck_contact_top_local'][2];data['backMount']=unity(m['assembly_interfaces']['back_mount_local'])
    with open(os.path.join(OUT,asset+'.json'),'w') as f:json.dump(data,f,separators=(',',':'))
    catalog.append({'id':asset,'category':category,'resource':'WispFamily20261112/'+asset,'waistHeight':waist,'vertices':sum(len(g['positions'])//3 for g in groups.values()),'groups':len(groups)})
    print('UNITY_MESH_EXPORT_PASS',asset,len(groups),catalog[-1]['vertices'],flush=True)
export('BD01',os.path.join(BASE,'BD01/BD01.blend'),'body')
export('HD01',os.path.join(BASE,'HD01/HD01.blend'),'head')
for part in ['W-01_LUMEN','W-02_BREAKER','W-03_RATTLE','W-04_EDGE','W-05_AEGIS']:export(part,os.path.join(ROOT,part,part+'-Mounted-Review.blend'),'weapon')
batch=os.path.join(ROOT,'ReferenceBatch-20261112')
with open(os.path.join(batch,'PART_CATALOG.json'),encoding='utf-8') as f:parts=json.load(f)['parts']
for p in parts:
    part=p['name']
    with open(os.path.join(batch,part,'manifest.json')) as f:m=json.load(f)
    export(part,os.path.join(batch,part,part+'.blend'),p['category'],m['mating_center'][2] if p['category']=='lower_body' else .505)
with open(os.path.join(OUT,'catalog.json'),'w') as f:json.dump({'assets':catalog},f,indent=2)
check_base();print('UNITY_FAMILY_EXPORT_COMPLETE',len(catalog))
