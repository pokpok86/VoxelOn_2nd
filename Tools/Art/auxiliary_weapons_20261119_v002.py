import bpy, math, json
from pathlib import Path
from mathutils import Vector
BASE=Path.cwd(); OUT=BASE/'Docs/Design/AuxiliaryWeapons/v002'; EXPORT=BASE/'Assets/Art/AuxiliaryWeapons/v002'
OUT.mkdir(parents=True,exist_ok=True);EXPORT.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
def mat(name,color,emit=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=.35;p.inputs['Roughness'].default_value=.4
 if emit:p.inputs['Emission Color'].default_value=(*color,1);p.inputs['Emission Strength'].default_value=emit
 return m
M={k:mat(k,c,e) for k,c,e in [('Ivory',(.70,.72,.67),0),('Graphite',(.045,.06,.075),0),('Orange',(.9,.22,.035),0),('Metal',(.22,.26,.30),0),('Cyan',(.02,.7,.95),3),('Violet',(.52,.06,.95),3),('Spark',(1,.42,.035),4)]}
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

scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=900;scene.render.resolution_y=900;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new("Studio World");scene.world.color=(.04,.04,.04)
bpy.ops.object.camera_add(location=(.65,-.95,.58));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=.53;scene.camera=cam
for loc,power,size in [((1,-2,2),180,2),((-1,-.4,1),100,1.5),((0,1,1),160,1)]:
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.data.energy=power*.3;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()

def hollow_tube(name,pos,outer=.052,inner=.039,depth=.05):
 verts=[];faces=[];segments=16
 for y,r in [(-depth/2,outer),(-depth/2,inner),(depth/2,outer),(depth/2,inner)]:
  for i in range(segments):
   a=i*math.tau/segments;verts.append((pos[0]+math.cos(a)*r,pos[1]+y,pos[2]+math.sin(a)*r))
 for i in range(segments):
  j=(i+1)%segments
  faces.extend([(i,j,16+j,16+i),(i,32+i,32+j,j),(16+i,16+j,48+j,48+i)])
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);current.objects.link(o);o.parent=root;o.data.materials.append(M['Metal']);return o

def tail_fin(name,side,vertical=False):
 # Swept polygon instead of a rectangular plate, authored at real missile scale.
 points=[(.027,.035),(.080,.085),(.074,.145),(.027,.125)]
 verts=[]
 for z in [-.003,.003]:
  for x,y in points:verts.append((z,y,side*x) if vertical else (side*x,y,z))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]);mesh.update();o=bpy.data.objects.new(name,mesh);current.objects.link(o);o.parent=root;o.data.materials.append(M['Metal']);return o

manifest=[]
for kind in ['MISSILE_POD','PULSE_GUN','HOMING_MISSILE','PULSE_BOLT','MOUNT_BRACKET']:
 col=begin('VXAUX_'+kind+'_v002')
 if kind=='MISSILE_POD':
  box('Graphite tube housing',(0,0,0),(.24,.27,.24),'Graphite',.015)
  for side in [-1,1]:
   box('Split cheek armor',(side*.126,0,.01),(.035,.23,.25),'Ivory',.008)
   box('Orange cheek stripe',(side*.147,-.03,.02),(.006,.10,.018),'Orange',.002)
  box('Top armor',(0,.01,.14),(.24,.25,.035),'Ivory',.009)
  box('Bottom armor',(0,.01,-.14),(.24,.25,.035),'Ivory',.009)
  for x in [-.06,.06]:
   for z in [-.065,.065]:
    hollow_tube('Recessed launch tube',(x,-.17,z));cyl('Deep tube back',(x,-.143,z),.039,.003,'Graphite');empty('MUZZLE_'+str(x)+'_'+str(z),(x,-.204,z))
  empty('MUZZLE',(0,-.205,0))
  for side in [-1,1]:
   for n in range(3):box('Pod service vent',(side*.147,.025+n*.025,.035),(.005,.012,.07),'Graphite',.001)
   box('Armor stripe',(side*.148,-.055,.055),(.005,.06,.013),'Orange',.001)
 elif kind=='PULSE_GUN':
  box('Pulse receiver',(0,.015,0),(.16,.32,.15),'Graphite',.012)
  for side in [-1,1]:
   box('Emitter shell',(side*.086,-.04,.01),(.03,.30,.14),'Ivory',.006)
   box('Cyan emitter rail',(side*.104,-.08,.015),(.009,.20,.018),'Cyan',.002)
  box('Top armor',(0,.015,.091),(.16,.30,.032),'Ivory',.006)
  box('Orange service hatch',(0,.10,.111),(.10,.09,.009),'Orange',.002)
  box('Emitter recess',(0,-.17,0),(.10,.02,.09),'Graphite',.003);box('Emitter lens',(0,-.185,0),(.055,.008,.055),'Cyan',.003)
  for side in [-1,1]:
   cap=box('Emitter cheek slope',(side*.069,-.135,.065),(.035,.10,.03),'Ivory',.003);cap.rotation_euler.y=side*.3
   for n in range(3):box('Pulse heat vent',(side*.106,.025+n*.025,.025),(.005,.012,.05),'Graphite',.001)
  empty('MUZZLE',(0,-.20,0))
 elif kind=='HOMING_MISSILE':
  cyl('Missile body',(0,0,0),.035,.28,'Ivory');cyl('Orange nose ring',(0,-.15,0),.038,.035,'Orange')
  bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=.034,radius2=0,depth=.09,location=(0,-.21,0),rotation=(math.pi/2,0,0));move(bpy.context.object,'Faceted missile nose','Graphite')
  for side in [-1,1]:
   tail_fin('Swept horizontal fin',side);tail_fin('Swept vertical fin',side,True)
  cyl('Exhaust nozzle',(0,.16,0),.027,.03,'Graphite');cyl('Exhaust core',(0,.18,0),.016,.013,'Cyan');empty('EXHAUST',(0,.19,0))
 elif kind=='PULSE_BOLT':
  # Faceted emissive projectile, physically separated from muzzle in gameplay.
  bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,radius=1);o=move(bpy.context.object,'Flying pulse core','Cyan');o.scale=(.024,.18,.024)
  empty('TRAIL_ORIGIN',(0,.18,0))
 else:
  box('ExtensionBeam',(.06,0,0),(.12,.025,.025),'Metal',.003)
  cyl('Base swivel',(0,0,0),.029,.04,'Graphite');cyl('Base bolt',(0,-.024,0),.015,.009,'Orange')
  tip=empty('TIP_PIVOT',(.12,0,0))
  for name,loc,r,depth,material in [('End swivel',(.12,0,0),.026,.04,'Graphite'),('End bolt',(.12,-.025,0),.013,.009,'Orange')]:
   part=cyl(name,loc,r,depth,material);bpy.context.view_layer.update();world=part.matrix_world.copy();part.parent=tip;part.matrix_world=world
  empty('BACKPACK_CONTACT');empty('WEAPON_CONTACT',(.12,0,0))
 if kind in ['MISSILE_POD','PULSE_GUN']:
  box('Universal mounting shoe',(0,.05,-.21),(.12,.12,.035),'Metal',.006);box('Mount neck',(0,.05,-.17),(.07,.07,.065),'Graphite',.004);empty('BACKPACK_MOUNT',(0,.05,-.23))
  for x in [-.04,0,.04]:box('Contact pin',(x,.10,-.23),(.018,.01,.012),'Orange',.001)
 manifest.append(export(col,'VXAUX_'+kind+'_v002'))
 scene.camera.data.ortho_scale=.65 if kind in ['MISSILE_POD','PULSE_GUN'] else .48
 scene.render.filepath=str(OUT/(kind+'.png'));bpy.ops.render.render(write_still=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(OUT/('VXAUX_'+kind+'_v002.blend')))
 for o in list(col.objects):bpy.data.objects.remove(o,do_unlink=True)
 bpy.data.collections.remove(col)
(OUT/'manifest.json').write_text(json.dumps({'revision':'v002','units':'meter','assets':manifest},indent=2))
print('AUX_ART_EXPORT_PASS',len(manifest))
