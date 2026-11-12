"""New family proposals using the PD-approved 163cfa5 base; never rewrite it.

No individual concept sheets were supplied for these IDs. Hidden structures,
wing articulation and lower-body stance are explicitly review assumptions.
"""
import bpy,math,os,sys,json,hashlib,argparse,bmesh
from mathutils import Vector,Matrix
sys.path.insert(0,os.path.dirname(os.path.abspath(__file__)))
from modeling_common import *
from assembly_layout import load_layout
from assembly_pose import pose_shoulders,set_forearm_angle,IDLE_DEGREES
from weapon_mount_style import style_weapon
ROOT=os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
BASE=os.path.abspath(os.path.join(ROOT,'../WISP_Base_v1'))
DESIGN=os.path.dirname(ROOT)
IDS=['B-02_SKYWING','B-03_CARGO_PACK','L-02_BASTION_TRACK','L-03_TARANTULAN','L-04_DRAFT_HOVER']
IDS+=['B-01_SKYWING','B-02_CROSSLANCE','B-03_OVERTHRUST','B-04_LONGTANK','B-05_RAMJET','L-01_STRIDER','L-05_SKYLIFT']
parser=argparse.ArgumentParser();parser.add_argument('--part',required=True,choices=IDS);parser.add_argument('--reference-batch',action='store_true')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:]);PART=args.part
REF=args.reference_batch
DELIVERY_ROOT=os.path.join(ROOT,'ReferenceBatch-20261112') if REF else ROOT
OUT=os.path.join(DELIVERY_ROOT,PART);os.makedirs(OUT,exist_ok=True)
BACK=PART.startswith('B-')
with open(os.path.join(ROOT,'APPROVED_BASELINE.json')) as f:baseline=json.load(f)
def sha(path):
    with open(path,'rb') as f:return hashlib.sha256(f.read()).hexdigest()
def check_base():
    for path,h in baseline['geometry_sha256'].items():assert sha(os.path.join(DESIGN,path))==h,('Approved geometry changed',path)
check_base()
COL.name=PART+'_MODULE'
layout=load_layout(BASE)
socket_center=Vector((0,0,0) if BACK else (0,0,.505))

def box(name,loc,dims,mat=GRAPHITE,chamfer=.002):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=link_object(bpy.context.object,name,mat);o.dimensions=dims
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return bevel(o,chamfer,1)

def loft_z(name,levels,mat=ARMOR,cx=0,cy=0):
    verts=[]
    for z,hx,hy in levels:
        for x,y in [(-.7,-1),(.7,-1),(1,-.7),(1,.7),(.7,1),(-.7,1),(-1,.7),(-1,-.7)]:verts.append((cx+x*hx,cy+y*hy,z))
    faces=[tuple(range(7,-1,-1))]
    for k in range(len(levels)-1):
        for i in range(8):j=(i+1)%8;faces.append((k*8+i,k*8+j,(k+1)*8+j,(k+1)*8+i))
    faces.append(tuple(range((len(levels)-1)*8,len(levels)*8)))
    me=bpy.data.meshes.new(name+'_facets');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);COL.objects.link(o);o.data.materials.append(mat)
    return bevel(o,.002,1)

def prism_xz(name,outline,y,depth,mat):
    # Reuse the YZ extruder then swap X/Y vertex axes.
    o=prism_yz(name,outline,y,depth,mat,.002)
    for v in o.data.vertices:v.co.x,v.co.y=v.co.y,v.co.x
    return o

def cylinder_z(name,loc,radius,depth,mat,vertices=24):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=loc)
    return bevel(link_object(bpy.context.object,name,mat),.001,1)

def ring_z(name,loc,outer,inner,depth,mat):
    o=annular_ring_y(name,loc,outer,inner,depth,mat,16)
    c=Vector(loc);rot=Matrix.Rotation(math.pi/2,3,'X')
    for v in o.data.vertices:v.co=c+rot @ (v.co-c)
    return o

def recess(name,x,y,z,width,height):
    # Thin embedded fins, never the old protruding radiator blocks.
    box(name+'_Dark_Well',(x,y,z),(width,.003,height),GRAPHITE,.001)
    for k in (-1,0,1):box(name+'_Inset_Slat',(x,y+.001,z+k*height*.23),(width*.78,.002,height*.085),GUNMETAL,.0004)

def back_interface():
    box('Common_Back_Mount_Plate',(0,.009,0),(.088,.018,.074),GUNMETAL)
    for x in (-.034,.034):
        for z in (-.025,.025):cylinder_y('Common_Back_Mount_Pin',(x,-.003,z),.006,.016,GRAPHITE,16)

def waist_interface():
    loft_z('Pelvis_Transition_Core',[(.385,.074,.059),(.432,.068,.054),(.479,.047,.039),(.493,.041,.036)],GRAPHITE)
    prism_xz('Pelvis_Front_Armor',[(-.057,.451),(-.041,.483),(.041,.483),(.057,.451),(.032,.405),(-.032,.405)],-.059,.012,ARMOR)
    box('Waist_Socket_Floor',(0,0,.499),(.083,.073,.012),GRAPHITE)
    for x in (-.043,.043):box('Waist_Socket_Side',(x,0,.510),(.006,.073,.012),GUNMETAL,.001)
    for y in (-.038,.038):box('Waist_Socket_End',(0,y,.510),(.072,.006,.012),GUNMETAL,.001)

def nozzle(name,loc,r=.042):
    ring_z(name+'_Rim',loc,r,r*.72,.023,GUNMETAL)
    cylinder_z(name+'_Recessed_Glass',(loc[0],loc[1],loc[2]+.008),r*.69,.003,CYAN_DARK)
    cylinder_z(name+'_Cyan_Core',(loc[0],loc[1],loc[2]+.006),r*.45,.002,CYAN)

def skywing():
    back_interface()
    loft_z('Central_Compact_Spine',[(-.093,.033,.034),(-.068,.044,.046),(.066,.044,.042),(.087,.027,.029)],GRAPHITE,cy=.060)
    for s,side in [(-1,'L'),(1,'R')]:
        cylinder_y('Wing_Root_Pivot_'+side,(s*.051,.071,.018),.026,.035,GUNMETAL,24)
        outline=[(s*x,z) for x,z in [(.045,.043),(.091,.092),(.238,.155),(.280,.137),(.257,.055),(.192,-.078),(.115,-.123),(.073,-.056)]]
        prism_xz('Swept_Wing_Core_'+side,outline,.091,.036,GRAPHITE)
        armor=[(s*x,z) for x,z in [(.077,.047),(.111,.088),(.238,.135),(.260,.123),(.230,.051),(.169,-.052),(.121,-.076),(.091,-.023)]]
        prism_xz('Wing_Broad_Faceted_Armor_'+side,armor,.117,.021,ARMOR)
        prism_xz('Wing_Shadow_Tip_'+side,[(s*x,z) for x,z in [(.178,-.035),(.233,.049),(.256,.061),(.210,-.061),(.155,-.109),(.123,-.096)]],.116,.016,ARMOR_SHADE)
        prism_xz('Wing_Root_Orange_Inlay_'+side,[(s*x,z) for x,z in [(.090,.044),(.117,.074),(.148,.084),(.128,.045),(.104,.007)]],.130,.003,ORANGE)
        recess('Flush_Wing_Cooling_'+side,s*.175,.126,.050,.044,.026)
        nozzle('Wing_Lift_Nozzle_'+side,(s*.157,.101,-.103),.030)
    box('Center_Service_Face',(0,.109,-.004),(.045,.012,.086),ARMOR_SHADE)

def cargo():
    back_interface()
    loft_z('Compact_Cargo_Chassis',[(-.112,.060,.036),(-.085,.081,.051),(.085,.081,.051),(.113,.062,.038)],GRAPHITE,cy=.068)
    loft_z('Single_Main_Armored_Case',[(-.095,.064,.043),(-.075,.078,.049),(.076,.078,.049),(.103,.059,.033)],ARMOR,cy=.082)
    prism_xz('Inset_Rear_Door',[(-.055,-.067),(-.062,.059),(-.043,.084),(.043,.084),(.062,.059),(.055,-.067),(.037,-.083),(-.037,-.083)],.134,.008,ARMOR_SHADE)
    for s,side in [(-1,'L'),(1,'R')]:
        loft_z('Narrow_Equipment_Side_Case_'+side,[(-.077,.019,.033),(-.058,.025,.040),(.063,.025,.040),(.080,.017,.030)],ARMOR_SHADE,cx=s*.096,cy=.068)
        prism_xz('Orange_Lock_Corner_'+side,[(s*x,z) for x,z in [(.040,.085),(.059,.075),(.069,.052),(.049,.052),(.032,.071)]],.139,.003,ORANGE)
        box('Embedded_Door_Latch_'+side,(s*.046,.142,-.012),(.012,.004,.027),GUNMETAL,.001)
    recess('Inset_Cargo_Vent',0,.1365,.034,.048,.027)
    box('Cyan_Inventory_Indicator',(0,.142,-.045),(.025,.002,.004),CYAN,.0003)

def track_ring(s):
    outer=[(-.26,.105),(-.20,.032),(.20,.032),(.26,.105),(.26,.225),(.20,.298),(-.20,.298),(-.26,.225)]
    center=Vector((0,.165));inner=[tuple(center+(Vector(v)-center)*.77) for v in outer]
    verts=[]
    for x in (s*.180-.052,s*.180+.052):
        for loop in (outer,inner):verts.extend((x,y,z) for y,z in loop)
    faces=[]
    for i in range(8):
        j=(i+1)%8;faces.extend([(i,j,8+j,8+i),(16+i,24+i,24+j,16+j),(i,16+i,16+j,j),(8+i,8+j,24+j,24+i)])
    me=bpy.data.meshes.new('Continuous_Track_Belt');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new('Continuous_Track_Belt',me);COL.objects.link(o);o.data.materials.append(GRAPHITE);bevel(o,.001,1)
    for i,a in enumerate(outer):
        b=outer[(i+1)%8];start=Vector((s*.180,*a));end=Vector((s*.180,*b))
        count=max(2,round((end-start).length/.045))
        for k in range(count):
            c=start.lerp(end,(k+.5)/count)
            shoe=box('Separate_Track_Shoe',c,(.113,(end-start).length/count*.83,.011),GUNMETAL,.001)
            shoe.rotation_euler.x=math.atan2((end-start).z,(end-start).y)
    for y,r,z in [(-.174,.080,.165),(-.085,.053,.112),(0,.053,.112),(.085,.053,.112),(.174,.080,.165)]:
        cylinder_x('Road_Wheel',(s*.180,y,z),r,.108,GRAPHITE,24)
        cylinder_x('Outer_Wheel_Hub',(s*.237,y,z),r*.62,.009,GUNMETAL,24)

def tracks():
    waist_interface()
    loft_z('Low_Central_Chassis',[(.265,.123,.162),(.297,.151,.191),(.352,.141,.174),(.382,.093,.119)],GRAPHITE)
    loft_z('Angled_Deck_Armor',[(.335,.139,.176),(.361,.135,.164),(.398,.078,.094)],ARMOR)
    for s,side in [(-1,'L'),(1,'R')]:
        track_ring(s)
        prism_yz('Chamfered_Track_Fender_'+side,[(-.230,.296),(-.198,.335),(.193,.335),(.230,.296),(.199,.280),(-.196,.280)],s*.184,.126,ARMOR,.002)
        prism_yz('Fender_Orange_Front_'+side,[(-.222,.296),(-.195,.325),(-.152,.325),(-.168,.300)],s*.251,.003,ORANGE,.0005)
    recess('Rear_Chassis_Cooling',0,.176,.315,.080,.024)

def segment(name,a,b,width_a,width_b,mat):
    a,b=Vector(a),Vector(b);d=(b-a).normalized()
    u=d.cross(Vector((0,1,0))).normalized();v=d.cross(u).normalized()
    verts=[]
    for c,w in [(a,width_a),(a.lerp(b,.18),width_a*1.06),(a.lerp(b,.82),width_b),(b,width_b*.7)]:
        for x,y in [(-.7,-1),(.7,-1),(1,-.7),(1,.7),(.7,1),(-.7,1),(-1,.7),(-1,-.7)]:verts.append(c+u*x*w+v*y*w*.70)
    faces=[tuple(range(7,-1,-1))]
    for k in range(3):
        for i in range(8):j=(i+1)%8;faces.append((8*k+i,8*k+j,8*k+8+j,8*k+8+i))
    faces.append(tuple(range(24,32)))
    me=bpy.data.meshes.new(name+'_taper');me.from_pydata(verts,[],faces);me.update()
    o=bpy.data.objects.new(name,me);COL.objects.link(o);o.data.materials.append(mat);bevel(o,.0015,1)

def quadruped():
    waist_interface()
    loft_z('Four_Leg_Central_Chassis',[(.307,.073,.085),(.340,.111,.129),(.401,.110,.121),(.433,.072,.082)],GRAPHITE)
    loft_z('Sloped_Central_Carapace',[(.387,.112,.126),(.414,.099,.111),(.444,.058,.062)],ARMOR)
    joints=[]
    for s,side in [(-1,'L'),(1,'R')]:
        for f,end in [(-1,'Front'),(1,'Rear')]:
            hip=Vector((s*.105,f*.103,.374));knee=Vector((s*.229,f*.181,.245));ankle=Vector((s*.281,f*.229,.063))
            tag=side+'_'+end
            segment('Exposed_Upper_Link_'+tag,hip,knee,.022,.019,GRAPHITE)
            segment('Upper_Faceted_Leg_Armor_'+tag,hip.lerp(knee,.14),hip.lerp(knee,.82),.039,.031,ARMOR)
            segment('Exposed_Lower_Link_'+tag,knee,ankle,.017,.016,GRAPHITE)
            segment('Lower_Faceted_Leg_Armor_'+tag,knee.lerp(ankle,.14),knee.lerp(ankle,.86),.032,.024,ARMOR_SHADE)
            for title,p,r in [('Hip',hip,.036),('Knee',knee,.029),('Ankle',ankle,.021)]:
                cylinder_x(title+'_Drum_'+tag,p,r,.062,GRAPHITE,24)
                cylinder_x(title+'_Outer_Cap_'+tag,(p.x+s*.034,p.y,p.z),r*.73,.007,GUNMETAL,24)
                joints.append({'leg':tag,'joint':title,'center':list(p),'axis':'X; stance only, no gait animation'})
            loft_z('Compact_Foot_Pad_'+tag,[(.010,.047,.057),(.027,.050,.061),(.052,.031,.040)],ARMOR_SHADE,ankle.x,ankle.y)
            box('Orange_Upper_Joint_Inlay_'+tag,(hip.x+s*.020,hip.y,.405),(.023,.023,.004),ORANGE,.0005)
    return joints

def hover():
    waist_interface()
    loft_z('Hover_Central_Frame',[(.245,.082,.095),(.291,.131,.155),(.345,.124,.144),(.403,.067,.074)],GRAPHITE)
    loft_z('Hover_Continuous_Deck',[(.322,.132,.157),(.343,.136,.152),(.386,.076,.095)],ARMOR)
    for s,side in [(-1,'L'),(1,'R')]:
        for f,end in [(-1,'Front'),(1,'Rear')]:
            x,y=s*.167,f*.136;tag=side+'_'+end
            loft_z('Octagonal_Hover_Pod_'+tag,[(.116,.058,.067),(.154,.077,.081),(.267,.071,.077),(.302,.049,.056)],GRAPHITE,x,y)
            loft_z('Swept_Pod_Armor_'+tag,[(.208,.077,.083),(.265,.071,.077),(.313,.042,.052)],ARMOR,x,y)
            nozzle('Recessed_Hover_Fan_'+tag,(x,y,.119),.055)
            box('Orange_Forward_Inlay_'+tag,(x,y-.056,.273),(.036,.008,.016),ORANGE,.001)
    recess('Hover_Rear_Service_Vent',0,.146,.303,.058,.024)

joint_notes=[]
if REF:
    from reference_family_geometry import build_reference
    joint_notes=build_reference(PART,globals())
elif PART=='B-02_SKYWING':skywing()
elif PART=='B-03_CARGO_PACK':cargo()
elif PART=='L-02_BASTION_TRACK':tracks()
elif PART=='L-03_TARANTULAN':joint_notes=quadruped()
else:hover()

module_root=bpy.data.objects.new(PART+'_ROOT',None);COL.objects.link(module_root)
interface=bpy.data.objects.new(PART+'_MATING_INTERFACE',None);COL.objects.link(interface);interface.location=socket_center
bpy.context.view_layer.update()
for o in list(COL.objects):
    if o==module_root:continue
    if o.type=='MESH':
        bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
    o.name=PART+'_'+o.name if not o.name.startswith(PART) else o.name
    wm=o.matrix_world.copy();o.parent=module_root;o.matrix_world=wm
bpy.context.view_layer.update()
meshes=[o for o in COL.objects if o.type=='MESH']
pts=[o.matrix_world @ Vector(v) for o in meshes for v in o.bound_box]
lo=Vector(tuple(min(v[i] for v in pts) for i in range(3)));hi=Vector(tuple(max(v[i] for v in pts) for i in range(3)));center=(lo+hi)/2
assert len(meshes)>10
studio=bpy.data.collections.new(PART+'_STUDIO');bpy.context.scene.collection.children.link(studio)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.eevee.taa_render_samples=96
scene.render.resolution_x=1100;scene.render.resolution_y=1100;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Family_Review_World');world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.012,.018,.028,1);world.node_tree.nodes['Background'].inputs['Strength'].default_value=.3;scene.world=world
for name,loc,power,color in [('Key',(-1,-1.3,1.2),85,(1,.89,.78)),('Fill',(1.2,-.9,.7),55,(.75,.86,1)),('Rim',(.5,1,.9),80,(.60,.78,1))]:
    d=bpy.data.lights.new(name,'AREA');d.energy=power;d.size=1.2;o=bpy.data.objects.new(name,d);studio.objects.link(o);o.location=loc;o.rotation_euler=(center-o.location).to_track_quat('-Z','Y').to_euler();d.color=color
def camera(name,loc,target,scale):
    d=bpy.data.cameras.new(name);d.type='ORTHO';d.ortho_scale=scale;o=bpy.data.objects.new(name,d);studio.objects.link(o);o.location=loc;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler();return o
view_scale=max(hi-lo)*1.4
views=[('01-Front',center+Vector((0,-2,0))),('02-Right',center+Vector((2,0,0))),('03-Rear',center+Vector((0,2,0))),('04-45deg',center+Vector((1.5,1.8,.65) if BACK else (1.5,-1.8,1.0))),('05-Top',center+Vector((0,0,2)))]
for name,loc in views:
    scene.camera=camera('Camera_'+name,loc,center,view_scale);scene.render.filepath=os.path.join(OUT,PART+'-'+name+'.png');bpy.ops.render.render(write_still=True)
scene.camera=camera('Camera_Mount',socket_center+Vector((.5,-.6,.4)),socket_center,.20 if BACK else .22);scene.render.filepath=os.path.join(OUT,PART+'-06-Mount-Closeup.png');bpy.ops.render.render(write_still=True)
scene.camera=bpy.data.objects['Camera_04-45deg'];bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,PART+'.blend'))

def append(path,col_name):
    with bpy.data.libraries.load(path,link=False) as (src,dst):dst.collections=[col_name]
    for col in dst.collections:scene.collection.children.link(col)
    return dst.collections[0]
for p,c in [('BD01','BD01_BODY_AND_SHOULDERS'),('HD01','HD01_SENSOR_HEAD')]+([('LG01','LG01_LEG_PAIR')] if BACK else []):append(os.path.join(BASE,p,p+'.blend'),c)
bpy.data.objects['BD01_ROOT'].location=layout['body_translation'];bpy.data.objects['HD01_ROOT'].location=layout['head_translation'];pose_shoulders(layout)
if BACK:module_root.location=layout['back_mount']
arms=append(os.path.join(ROOT,'W-01_LUMEN/W-01_LUMEN.blend'),'W01_LUMEN_MODULE')
arm=bpy.data.objects['W-01_LUMEN_ROOT'];arm.location=layout['right_elbow'];arm.scale=(.42,)*3;set_forearm_angle(arm,IDLE_DEGREES)
original=[o for o in arms.objects if o.type=='MESH']
other=arm.copy();other.name='W-01_LUMEN_LEFT_ROOT';arms.objects.link(other);other.location=layout['left_elbow'];set_forearm_angle(other,IDLE_DEGREES,left=True)
for obj in original:
    cp=obj.copy();cp.data=obj.data.copy();arms.objects.link(cp);cp.parent=other
style_weapon(arm,arms,'W-01_LUMEN');style_weapon(other,arms,'W-01_LUMEN',left=True)
bpy.context.view_layer.update()
def z_bounds(o):return [min((o.matrix_world @ Vector(v)).z for v in o.bound_box),max((o.matrix_world @ Vector(v)).z for v in o.bound_box)]
waist_floor=bpy.data.objects['LG01_Waist_Socket_Floor'] if BACK else bpy.data.objects[PART+'_Waist_Socket_Floor']
waist_error=abs(z_bounds(waist_floor)[1]-z_bounds(bpy.data.objects['Waist_Pelvis_Interface_Key'])[0])
neck_error=abs(z_bounds(bpy.data.objects['Neck_Head_Interface'])[1]-z_bounds(bpy.data.objects['HD01_Fixed_Neck_Interface'])[0])
assert waist_error<.00001 and neck_error<.00001
if BACK:assert (module_root.location-Vector(layout['back_mount'])).length<.00001
mounted=[]
for suffix,loc in [('Mounted-Front',(1.6,-2,.95)),('Mounted-Rear',(1.5,2,.95))]:
    scene.camera=camera('Camera_'+suffix,loc,(0,0,.53),1.60);scene.render.filepath=os.path.join(OUT,PART+'-'+suffix+'.png');bpy.ops.render.render(write_still=True);mounted.append(PART+'-'+suffix+'.png')
scene.camera=bpy.data.objects['Camera_Mounted-Rear' if BACK else 'Camera_Mounted-Front'];bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,PART+'-Mounted-Review.blend'))
check_base()
manifest={'asset':PART,'kind':'backpack' if BACK else 'complete pelvis-down replacement','blender':bpy.app.version_string,'source_reference':'NO INDIVIDUAL REFERENCE SUPPLIED; family design proposal','approved_base_commit':'163cfa5','base_geometry_sha256':baseline['geometry_sha256'],'bounds_m':{k:float(hi[i]-lo[i]) for i,k in enumerate(['width','depth','height'])},'module_mesh_count':len(meshes),'mating_center':list(socket_center),'mounted_location':list(module_root.location),'four_plus_top_same_ortho_scale':view_scale,'waist_contact_error_m':waist_error,'neck_contact_error_m':neck_error,'joint_stance_notes':joint_notes,'automation':'PASS','design_review':'PENDING_PD','unity_integration':'NOT_PERFORMED','assumptions':['No individual new concept sheets exist; this is a new proposal in the approved visual family.','Backpack interface retention and wing articulation are provisional; wings have a static deployed stance.','Lower-body waist mating plane is held at 0.505 m to preserve the approved upper-body proportions.','Tracks, four-leg stance and hover fans are visual blockouts with separated parts; no movement or gait animation is claimed.','Low-profile vents are inset; no protruding radiator fins are added.','Contact checks are static interface checks, not exhaustive collision or game validation.'],'renders':[PART+'-'+name+'.png' for name,_ in views]+[PART+'-06-Mount-Closeup.png']+mounted}
if REF:
    manifest['source_reference']='../References/'+PART+'.jpg'
    manifest['reference_sha256']=sha(os.path.join(DELIVERY_ROOT,'References',PART+'.jpg'))
    manifest['assumptions']=[x for x in manifest['assumptions'] if 'No individual' not in x]
    manifest['assumptions']+=['Supplied front and 45-degree silhouette control the design; concealed structure is a modeling assumption.','Supplied labels may conflict with the legacy catalog; this reference set is separately namespaced.']
    manifest['reference_nozzles']=globals().get('reference_nozzle_specs',[])
with open(os.path.join(OUT,'manifest.json'),'w',encoding='utf-8') as f:json.dump(manifest,f,indent=2)
print('EXPANSION_PART_PASS',PART,len(meshes),manifest['bounds_m'],waist_error,neck_error)
