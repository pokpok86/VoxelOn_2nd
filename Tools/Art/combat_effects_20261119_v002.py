import bpy, math, json
from pathlib import Path
from mathutils import Vector
BASE=Path.cwd(); OUT=BASE/'Docs/Design/CombatEffects/v002'; EXPORT=BASE/'Assets/Art/CombatEffects/v002'
OUT.mkdir(parents=True,exist_ok=True);EXPORT.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
def mat(name,color,emit=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=.35;p.inputs['Roughness'].default_value=.4
 if emit:p.inputs['Emission Color'].default_value=(*color,1);p.inputs['Emission Strength'].default_value=emit
 return m
M={k:mat(k,c,e) for k,c,e in [('Ivory',(.70,.72,.67),0),('Graphite',(.045,.06,.075),0),('Orange',(.9,.22,.035),0),('Metal',(.22,.26,.30),0),('Cyan',(.02,.7,.95),3),('Violet',(.52,.06,.95),3),('Spark',(.95,.20,.012),1.4)]}
current=None;root=None
def move(o,name,material=None,parent=True):
 o.name=name
 for c in list(o.users_collection):c.objects.unlink(o)
 current.objects.link(o)
 if parent:o.parent=root
 if material:o.data.materials.append(M[material])
 return o
def empty(name,pos=(0,0,0)):
 o=bpy.data.objects.new(name,None);current.objects.link(o);o.parent=root;o.location=pos;o.empty_display_size=.025;return o
def box(name,pos,size,material='Ivory',bevel=.008):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=move(bpy.context.object,name,material);o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:mod=o.modifiers.new('Machined edge','BEVEL');mod.width=bevel;mod.segments=1;o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return o
def cyl(name,pos,radius,depth,material='Metal',vertices=12):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=pos,rotation=(math.pi/2,0,0));return move(bpy.context.object,name,material)
def begin(name):
 global current,root
 current=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(current);root=bpy.data.objects.new(name+'__ROOT',None);current.objects.link(root);root['unit']='meter';root['blender_forward']='-Y';return current

def drone(kind):
 body=box('Armored sensor shell',(0,0,0),(.17,.18,.085),bevel=.009)
 top=box('Top identification',(0,-.025,.047),(.10,.10,.012),'Orange',.003);top.rotation_euler.x=.10
 for side in [-1,1]:
  armor=box('Sloped cheek armor',(side*.072,-.018,.025),(.045,.11,.055),'Ivory',.004);armor.rotation_euler.y=side*.3
  for n in range(3):box('Heat vent',(side*.065,.04+n*.012,.048),(.025,.005,.005),'Graphite',.001)
  box('Nose marking',(side*.065,-.10,.015),(.014,.008,.018),'Orange',.002)
 box('Sensor recess',(0,-.096,-.005),(.09,.015,.036),'Graphite',.003);box('Optical sensor',(0,-.106,0),(.045,.008,.019),'Cyan',.002)
 for side in [-1,1]:
  cyl('Dock contact',(side*.043,.085,0),.012,.012,'Orange')
  if kind=='ATTACK':
   box('Gun carrier',(side*.104,-.025,-.02),(.055,.14,.05),'Graphite')
   cyl('Cannon barrel',(side*.104,-.12,-.02),.017,.13);cyl('Muzzle bore',(side*.104,-.189,-.02),.011,.005,'Graphite')
   empty('MUZZLE_'+('L' if side<0 else 'R'),(side*.104,-.195,-.02))
  elif kind=='INTERCEPT':
   box('Interceptor pylon',(side*.107,-.035,0),(.05,.25,.055));box('Pylon nose',(side*.107,-.164,0),(.05,.018,.04),'Orange',.003)
   cyl('Interceptor barrel',(0,-.16,-.027),.012,.12);empty('INTERCEPT_MUZZLE',(0,-.225,-.027)) if side==1 else None
  for front in [-1,1]:
   x=side*.132;y=front*.076
   pivot=empty('FIN_PIVOT_'+str(side)+'_'+str(front),(x*.7,y,0));fin=box('Foldable stabilizer',(x,y,.005),(.08,.055,.018),bevel=.006);fin.rotation_euler.z=side*front*.32
   world=fin.matrix_world.copy();fin.parent=pivot;fin.matrix_world=world
   if kind=='JAMMER':
    antenna=box('Jamming aerial',(side*.13,front*.125,0),(.009,.10,.009),'Violet',.002);empty('EMITTER_'+str(side)+'_'+str(front),(side*.13,front*.17,0))
 if kind=='JAMMER':
  # Hexagonal resonator on top, identity distinct from the twin cannon drone.
  bpy.ops.mesh.primitive_cylinder_add(vertices=6,radius=.09,depth=.025,location=(0,0,.06));move(bpy.context.object,'Hexagonal resonator','Ivory')
  box('Resonator light',(0,0,.077),(.07,.007,.004),'Violet',.001)
 empty('DOCK_CONTACT',(0,.095,0));empty('DRONE_CENTER');return current

def pack(kind):
 box('Body mounting plate',(0,.105,0),(.30,.045,.29),'Graphite');empty('BODY_MOUNT',(0,.131,0));empty('DRONE_DOCK',(0,-.015,.01));empty('LAUNCH_EXIT',(0,-.24,.08))
 for side in [-1,1]:
  rail=box('Cradle armored rail',(side*.162,.01,.005),(.055,.14,.27),bevel=.012);rail.rotation_euler.y=side*.12
  box('Rail orange latch',(side*.166,-.067,-.07),(.059,.025,.065),'Orange',.005)
  for z in [-.085,.085]:cyl('Captive fastener',(side*.165,-.079,z),.009,.008,'Metal')
 box('Top bridge',(0,.015,.158),(.31,.14,.047),bevel=.014)
 for side in [-1,1]:
  brow=box('Angled cradle brow',(side*.105,-.025,.124),(.09,.09,.045),'Graphite',.008);brow.rotation_euler.y=side*.35
  box('Dock status',(side*.163,-.07,.07),(.018,.008,.022),'Cyan',.002)
  for n in range(3):box('Rail vent',(side*.19,.025,-.03+n*.023),(.006,.05,.006),'Graphite',.001)
 box('Bottom charging bridge',(0,.025,-.139),(.30,.12,.045))
 for side in [-1,1]:box('Charge contact',(side*.043,.061,.01),(.023,.01,.036),'Orange',.002)
 box('Charge indicator',(0,-.04,-.164),(.06,.007,.009),'Cyan',.002)
 root['aerial_booster']=False if kind=='ATTACK' else 'unspecified_PD';root['subweapon_mounts']=0
 return current

def effect(kind):
 empty('FX_ORIGIN')
 if kind=='JAMMER':
  for n in range(3):
   curve=bpy.data.curves.new('Pulse arc','CURVE');curve.dimensions='3D';curve.bevel_depth=.002;curve.bevel_resolution=1;sp=curve.splines.new('POLY');sp.points.add(35)
   for i,p in enumerate(sp.points):a=i/35*math.pi*1.5;p.co=(math.cos(a)*(.18+n*.065),math.sin(a)*(.18+n*.065),n*.015,1)
   o=bpy.data.objects.new('Jamming pulse arc',curve);current.objects.link(o);o.parent=root;o.data.materials.append(M['Violet']);bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o.select_set(False)
 else:
  for i in range(12 if kind=='INTERCEPT' else 6):
   a=i*2.399;length=.08 if kind=='ATTACK' else .14
   o=box('Impact spark',(math.cos(a)*length*.5,-.02,math.sin(a)*length*.5),(.005,.005,length),'Spark',.001);o.rotation_euler.y=a
 empty('FX_END',(0,-.20,0));return current

def export(col,name):
 bpy.ops.object.select_all(action='DESELECT')
 for o in col.objects:o.hide_set(False);o.select_set(True)
 bpy.ops.export_scene.fbx(filepath=str(EXPORT/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,bake_anim=False)
 return {'id':name,'file':str((EXPORT/(name+'.fbx')).relative_to(BASE)),'objects':len(col.objects),'sockets':[o.name for o in col.objects if o.type=='EMPTY']}

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=900;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new("Studio World");scene.world.color=(.04,.04,.04)
bpy.ops.object.camera_add(location=(.65,-.95,.58));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=.53;scene.camera=cam
for loc,power,size in [((1,-2,2),180,2),((-1,-.4,1),100,1.5),((0,1,1),160,1)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power*.3;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()


M['WhiteFlash']=mat('WarmFlash',(.98,.68,.22),2)
M['ColdCore']=mat('ColdCore',(.35,.86,1),2)
M['Smoke']=mat('Smoke',(.065,.055,.045),0)
manifest=[]
for kind in ['MUZZLE','MISSILE_BURST','INTERCEPT','JAM_FIELD','THRUSTER']:
 col=begin('VXFX_'+kind+'_v002');empty('FX_ORIGIN')
 if kind=='MUZZLE':
  bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=.035,radius2=.002,depth=.15,location=(0,-.075,0),rotation=(math.pi/2,0,0));move(bpy.context.object,'Orange muzzle flare','Spark')
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.017,location=(0,-.03,0));move(bpy.context.object,'White ignition core','WhiteFlash')
 elif kind in ['MISSILE_BURST','INTERCEPT']:
  if kind=='MISSILE_BURST':
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.08);move(bpy.context.object,'Expanding impact shell','Spark')
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.035,location=(0,-.06,0));move(bpy.context.object,'Small warm ignition core','WhiteFlash')
   for cloud in range(4):
    a=cloud*math.tau/4;bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=.032,location=(math.cos(a)*.1,.025,math.sin(a)*.1));move(bpy.context.object,'Faceted smoke fragment','Smoke')
  for i in range(12):
   a=i*2.3999;r=.13 if kind=='INTERCEPT' else .19;v=Vector((math.cos(a)*r,math.sin(a)*r,math.sin(a*1.7)*r*.6));o=box('Radial impact shard',tuple(v),(.008,.008,.075),'WhiteFlash' if i%3==0 else 'Spark',.001);o.rotation_euler=v.to_track_quat('Z','Y').to_euler()
 elif kind=='JAM_FIELD':
  for layer in range(3):
   curve=bpy.data.curves.new('Broken interference arc','CURVE');curve.dimensions='3D';curve.bevel_depth=.002;curve.bevel_resolution=1;line=curve.splines.new('POLY');line.points.add(59)
   for i,p in enumerate(line.points):a=i/59*math.tau*.82+layer*.7;p.co=(math.cos(a)*(.25+layer*.06),math.sin(a)*(.25+layer*.06),.005*layer,1)
   obj=bpy.data.objects.new('Jamming pulse arc',curve);current.objects.link(obj);obj.parent=root;obj.data.materials.append(M['Violet']);bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj;bpy.ops.object.convert(target='MESH')
  for i in range(16):a=i*math.tau/16;tick=box('Interference marker',(math.cos(a)*.32,math.sin(a)*.32,.015),(.009,.009,.025),'Violet',.001)
 else:
  bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=.024,radius2=.001,depth=.22,location=(0,.11,0),rotation=(-math.pi/2,0,0));move(bpy.context.object,'Missile blue plume','Cyan')
  bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=.014,radius2=.001,depth=.13,location=(0,.065,0),rotation=(-math.pi/2,0,0));move(bpy.context.object,'Missile hot core','ColdCore')
 empty('FX_FORWARD',(0,-.12,0));manifest.append(export(col,'VXFX_'+kind+'_v002'))
 scene.camera.data.ortho_scale=.85 if kind=='JAM_FIELD' else .52;scene.render.filepath=str(OUT/(kind+'.png'));bpy.ops.render.render(write_still=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(OUT/('VXFX_'+kind+'_v002.blend')))
 for o in list(col.objects):bpy.data.objects.remove(o,do_unlink=True)
 bpy.data.collections.remove(col)
(OUT/'manifest.json').write_text(json.dumps({'revision':'v002','assets':manifest},indent=2))
print('COMBAT_FX_EXPORT_PASS',len(manifest))
