"""Assemble four current WISP variants and add restrained surface markings."""
import bpy,math,json,hashlib,sys
from pathlib import Path
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
HERE=Path(__file__).resolve().parent;sys.path.insert(0,str(HERE))
from assembly_pose import set_forearm_angle
from weapon_mount_style import style_weapon
BASE=HERE.parent;OUT=BASE/'Combination-Review-20261112';OUT.mkdir(exist_ok=True)
CONFIG=[
 ('01','LIGHT ASSAULT','H-03_SWIFTFIN','C-02_LIGHTFRAME','LB-01_DART','W-03_RATTLE','W-04_EDGE','B-01_SKYWING'),
 ('02','STANDARD RANGER','H-02_LONGSIGHT','C-01_STANDARD','LB-02_STRIDER','W-01_LUMEN','W-05_AEGIS','B-02_CROSSLANCE'),
 ('03','ARMORED GUARD','H-04_BULWARK','C-03_BULWARK_CORE','LB-03_BULWARK','W-02_BREAKER','W-05_AEGIS','B-03_OVERTHRUST'),
 ('04','ARC ARTILLERY','H-01_RADOME','C-04_ARC_CORE','LB-04_COLOSSUS','W-02_BREAKER','W-03_RATTLE','B-04_LONGTANK')]
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def append(path,collection=None):
    with bpy.data.libraries.load(str(path),link=False) as (a,b):
        if collection is None:collection=next(n for n in a.collections if 'MODULE' in n)
        b.collections=[collection]
    c=b.collections[0];bpy.context.scene.collection.children.link(c);return c
def remove(c):
    for o in list(c.all_objects):bpy.data.objects.remove(o,do_unlink=True)
    bpy.data.collections.remove(c)
def mesh_points(o):
    ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get());me=ev.to_mesh();v=[o.matrix_world@p.co for p in me.vertices];ev.to_mesh_clear();return v
def center(o):
    p=mesh_points(o);return Vector(tuple((min(v[i] for v in p)+max(v[i] for v in p))/2 for i in range(3)))
def mat(name,color,emission=0):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Metallic'].default_value=.3;n.inputs['Roughness'].default_value=.43
    if emission:n.inputs['Emission Color'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=emission
    return m
def parent_world(o,p,world):o.parent=p;o.matrix_world=world
def surface(targets):
    deps=bpy.context.evaluated_depsgraph_get();trees=[]
    for o in targets:
        ev=o.evaluated_get(deps);me=ev.to_mesh();verts=[o.matrix_world@v.co for v in me.vertices];faces=[tuple(p.vertices) for p in me.polygons];trees.append((o,BVHTree.FromPolygons(verts,faces)));ev.to_mesh_clear()
    def ray(x,z,rear=False):
        start=Vector((x,2 if rear else -2,z));direction=Vector((0,-1 if rear else 1,0));best=None
        for o,tree in trees:
            p,n,face,d=tree.ray_cast(start,direction,4)
            if p is not None and (best is None or d<best[3]):
                if n.dot(direction)>0:n=-n
                best=(p,n,o,d)
        return best
    return ray
def patch(name,ray,outline,material,rear=False):
    hits=[ray(x,z,rear) for x,z in outline]
    if any(h is None for h in hits):return None
    if any(h[2]!=hits[0][2] or h[1].dot(hits[0][1])<.75 for h in hits):return None
    data=bpy.data.meshes.new(name+'_Mesh');data.from_pydata([p+n*.00035 for p,n,o,d in hits],[],[tuple(range(len(hits)))]);data.materials.append(material)
    o=bpy.data.objects.new(name,data);DECOR.objects.link(o);parent_world(o,hits[0][2],Matrix.Identity(4));return o
def rectangle(name,ray,x,z,w,h,m,rear=False):return patch(name,ray,[(x-w/2,z-h/2),(x+w/2,z-h/2),(x+w/2,z+h/2),(x-w/2,z+h/2)],m,rear)
def text_on(name,body,ray,x,z,size,material,rear=False):
    hit=ray(x,z,rear)
    if not hit:return None
    p,n,target,d=hit;xx=Vector((1,0,0));xx=(xx-n*xx.dot(n)).normalized();yy=n.cross(xx).normalized()
    if rear:xx=-xx;yy=n.cross(xx).normalized()
    data=bpy.data.curves.new(name+'_Font','FONT');data.body=body;data.size=size;data.extrude=0;data.resolution_u=2;data.materials.append(material)
    o=bpy.data.objects.new(name,data);DECOR.objects.link(o);world=Matrix((xx,yy,n)).transposed().to_4x4();world.translation=p+n*.0006;parent_world(o,target,world);return o
def stripes(name,ray,x,z,material,rear=False):
    count=0
    for i in range(3):
        a=x+(i-1)*.011
        if patch(name+str(i),ray,[(a-.003,z-.009),(a+.003,z-.009),(a+.008,z+.009),(a+.002,z+.009)],material,rear):count+=1
    return count
def camera(name,offset,target,scale):
    data=bpy.data.cameras.new(name);data.type='ORTHO';data.ortho_scale=scale;o=bpy.data.objects.new(name,data);bpy.context.scene.collection.objects.link(o);o.location=Vector(target)+Vector(offset);o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();return o
records=[]
for number,title,head,core,leg,right,left,pack in CONFIG:
    dest=OUT/number;dest.mkdir(exist_ok=True)
    legpath=BASE/'Biped-FeetFollowThigh-20261112'/leg/(leg+'-Mounted.blend');corepath=BASE/'CoreDepth-ShoulderFit-20261112'/core/(core+'-ShoulderFit-Mounted.blend');headpath=BASE/'HeadCore-20261112'/head/(head+'.blend');packpath=BASE/'ReferenceBatch-20261112'/pack/(pack+'.blend')
    sources=[legpath,corepath,headpath,packpath]+[BASE/w/(w+'.blend') for w in [right,left]];hashes={str(p):sha(p) for p in sources}
    bpy.ops.wm.open_mainfile(filepath=str(legpath));body_at=bpy.data.objects['C-01_STANDARD_ROOT'].location.copy();head_at=bpy.data.objects['HD01_ROOT'].location.copy()
    legcol=bpy.data.collections[leg+'_MODULE'];leg_state={o.name:[list(row) for row in o.matrix_world] for o in legcol.objects}
    remove(bpy.data.objects['W-01_LUMEN_ROOT'].users_collection[0]);remove(bpy.data.collections['C-01_STANDARD_MODULE']);remove(bpy.data.objects['HD01_ROOT'].users_collection[0])
    body=append(corepath,core+'_MODULE');bodyroot=next(o for o in body.objects if o.name==core+'_ROOT');bodyroot.location=body_at
    heads=append(headpath,head+'_MODULE');headroot=next(o for o in heads.objects if o.name==head+'_ROOT');headroot.location=head_at
    bpy.context.view_layer.update();elbows={}
    for side,part in [('R',right),('L',left)]:
        pin=next(o for o in body.objects if side+'_UpperArm_Connector_Pin' in o.name);elbow=center(pin);elbows[side]=list(elbow)
        col=append(BASE/part/(part+'.blend'));r=next(o for o in col.objects if o.type=='EMPTY' and o.name.endswith('_ROOT'));r.name=part+('_LEFT_ROOT' if side=='L' else '_ROOT');r.location=elbow;r.scale=(.42,)*3
        set_forearm_angle(r,50,left=side=='L');style_weapon(r,col,part,left=side=='L');bpy.context.view_layer.update();assert (r.matrix_world.translation-elbow).length<1e-6
    packs=append(packpath,pack+'_MODULE');pr=next(o for o in packs.objects if o.name==pack+'_ROOT');pr.location=body_at+Vector((0,.08757,-.00315));bpy.context.view_layer.update()
    assert leg_state=={o.name:[list(row) for row in o.matrix_world] for o in legcol.objects}
    # New markings belong only to this assembly scene and follow their parent part.
    DECOR=bpy.data.collections.new('Assembly_'+number+'_Surface_Points');bpy.context.scene.collection.children.link(DECOR)
    INK=mat('Marking_Graphite',(.035,.045,.060));WHITE=mat('Marking_Warm_White',(.74,.73,.68));ORANGE=mat('Marking_Amber',(.95,.30,.025));GLOW=mat('Point_Cyan_Glass',(.008,.54,.76),2.8)
    body_tags={'C-01_STANDARD':['Wrapped_Chest_Armor'],'C-02_LIGHTFRAME':['Continuous_Spear_Shell'],'C-03_BULWARK_CORE':['Upper_Overlapping_Breastplate'],'C-04_ARC_CORE':['Wrapped_Reactor_Cheek']}
    front=[o for o in body.objects if o.type=='MESH' and any(t in o.name for t in body_tags[core])];ray=surface(front)
    cx=.051 if core=='C-04_ARC_CORE' else -.036
    text_on('Chest_Unit_Number',number,ray,cx,body_at.z+.010,.011,INK)
    text_on('Chest_WISP_ID','WISP',ray,cx,body_at.z+.002,.0042,INK)
    if core!='C-04_ARC_CORE':
        rectangle('Chest_Light_Recess',ray,.034,body_at.z+.014,.024,.0042,INK)
        rectangle('Chest_Cyan_Point',ray,.034,body_at.z+.014,.019,.0018,GLOW)
    shoulders=[o for o in body.objects if o.type=='MESH' and 'Shoulder_Main_Angular_Cap' in o.name]
    for o in shoulders:
        p=center(o);sr=surface([o])
        if p.x>0:stripes('Shoulder_Identification_Stripe_',sr,p.x,p.z,ORANGE)
        else:text_on('Shoulder_Unit_Code',number,sr,p.x-.012,p.z-.004,.010,INK)
    for side in ['L','R']:
        thigh=next(o for o in legcol.objects if o.name==side+'_Thigh_Angular_Armor');tr=surface([thigh]);p=center(thigh)
        text_on(side+'_Thigh_Maintenance_ID',number+'-'+side,tr,p.x-.010,p.z,.0058,INK)
        shin=next(o for o in legcol.objects if o.name==side+'_Shin_Angular_Armor');p=center(shin);sr=surface([shin])
        rectangle(side+'_Shin_Light_Base',sr,p.x,p.z,.012,.004,INK);rectangle(side+'_Shin_Cyan_Point',sr,p.x,p.z,.009,.0014,GLOW)
    # One small weapon code on the broad housing, without covering the emitter.
    for side,part in [('R',right),('L',left)]:
        r=bpy.data.objects[part+('_LEFT_ROOT' if side=='L' else '_ROOT')]
        meshes=[o for o in r.children if o.type=='MESH' and any(t in o.name for t in ['Main_Armor','Main_Structural','Shield_Structural','Blade_Chassis'])]
        if meshes:
            candidate=max(meshes,key=lambda o:len(o.data.vertices));p=center(candidate);wr=surface([candidate]);text_on('Weapon_Code_'+side,part[:4],wr,p.x-.012,p.z,.007,WHITE)
    # Rear pack ID is attached flush to the original central chassis.
    candidates=[o for o in packs.objects if o.type=='MESH' and any(t in o.name for t in ['Central','Chassis','Control'])]
    if candidates:
        candidate=max(candidates,key=lambda o:len(o.data.vertices));p=center(candidate);rr=surface([candidate]);text_on('Backpack_Unit_ID','WISP '+number,rr,p.x+.022,p.z,.007,WHITE,rear=True)
    bpy.context.view_layer.update();assert leg_state=={o.name:[list(row) for row in o.matrix_world] for o in legcol.objects if o.name in leg_state}
    scene=bpy.context.scene;scene.render.resolution_x=800;scene.render.resolution_y=1120;scene.render.resolution_percentage=100
    # Emissive point materials remain restrained alongside existing lenses.
    scene.camera=camera('Combination_Front45',(1.6,-2.3,1.0),(0,.025,.43),1.38);scene.render.filepath=str(dest/(number+'-01-Front45.png'));bpy.ops.render.render(write_still=True)
    scene.camera=camera('Combination_Front',(0,-2.8,.30),(0,.025,.43),1.38);scene.render.filepath=str(dest/(number+'-02-Front.png'));bpy.ops.render.render(write_still=True)
    scene.camera=camera('Combination_Back45',(1.6,2.3,.95),(0,.045,.43),1.38);scene.render.filepath=str(dest/(number+'-03-Back45.png'));bpy.ops.render.render(write_still=True)
    scene.camera=camera('Combination_Points_Closeup',(1.1,-2,.65),(0,-.03,.62),.68);scene.render.filepath=str(dest/(number+'-04-Points-Closeup.png'));bpy.ops.render.render(write_still=True)
    scene.camera=bpy.data.objects['Combination_Front45'];path=dest/(number+'-'+title.replace(' ','_')+'.blend');bpy.ops.wm.save_as_mainfile(filepath=str(path));bpy.ops.wm.open_mainfile(filepath=str(path))
    assert 'Assembly_'+number+'_Surface_Points' in bpy.data.collections
    for p,h in hashes.items():assert sha(p)==h,p
    record={'number':number,'title':title,'head':head,'core':core,'lower':leg,'right_weapon':right,'left_weapon':left,'backpack':pack,'elbow_mount_world_m':elbows,'leg_joint_pose_preserved':True,'decoration_objects':len(bpy.data.collections['Assembly_'+number+'_Surface_Points'].objects),'sources_sha256':hashes,'blend':str(path.relative_to(OUT)),'automation':'PASS','design_review':'PENDING_PD','unity_test':'NOT_PERFORMED'}
    (dest/'manifest.json').write_text(json.dumps(record,indent=2),encoding='utf-8');records.append(record);print('COMBINATION_PASS',number,flush=True)
(OUT/'PART_COMBINATIONS.json').write_text(json.dumps({'combinations':records},indent=2),encoding='utf-8');print('COMBINATIONS_FOUR_PASS',len(records),flush=True)
