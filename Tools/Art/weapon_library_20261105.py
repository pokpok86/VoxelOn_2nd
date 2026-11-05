import bpy,math,json,os,sys
from mathutils import Matrix,Vector,Euler
BASE=r'D:\Codex\VoxSky\Builds\WorkLatest'
REV='v003'
OUT=os.path.join(BASE,'Assets/Resources/PDParts/Weapons20261105');SRC=os.path.join(BASE,'ArtSource/Weapons-20261105');DOC=os.path.join(BASE,'Docs/Screenshots/Weapons-20261105/Blender',REV)
for folder in [OUT,SRC,DOC]:os.makedirs(folder,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
ROT=Matrix.Rotation(math.pi/2,4,'X');materials={}
COLORS={'Recess':(.035,.045,.065,1),'Steel':(.095,.12,.17,1),'Machined':(.28,.33,.42,1),'Tan':(.44,.30,.16,1),'TanLight':(.58,.43,.27,1),'Olive':(.30,.35,.17,1),'Brass':(.95,.55,.075,1),'Safety':(.94,.58,.025,1),'White':(.82,.84,.89,1),'Purple':(.43,.36,.64,1),'PurpleDark':(.08,.075,.15,1),'Red':(.66,.035,.025,1),'Glow_Cyan':(.015,.70,1,1),'Glow_Pink':(.98,.015,.48,1),'Glow_Ice':(.50,.92,1,1),'Glass_Cyan':(.02,.66,.90,.24),'Glass_Pink':(.90,.025,.40,.20)}
for name,color in COLORS.items():
 m=bpy.data.materials.new(name);m.diffuse_color=color;m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=color;n.inputs['Metallic'].default_value=.38 if name in ['Steel','Machined','Brass'] else .12;n.inputs['Roughness'].default_value=.36 if name in ['Steel','Machined','Brass'] else .47
 if name.startswith(('Glow_','Glass_')):n.inputs['Emission Color'].default_value=(*color[:3],1);n.inputs['Emission Strength'].default_value=2 if name.startswith('Glow') else .4
 if color[3]<1:n.inputs['Alpha'].default_value=color[3];m.surface_render_method='DITHERED'
 materials[name]=m
catalog=[];root=None;collection=None;groups={};current=[];ident='';family='';group='RECEIVER';sockets={};socket_nodes={}
def begin(code,fam):
 global root,collection,groups,current,ident,family,group,sockets,socket_nodes
 ident=code;family=fam;groups={};current=[];group='RECEIVER';sockets={};socket_nodes={};collection=bpy.data.collections.new(code);bpy.context.scene.collection.children.link(collection)
 root=bpy.data.objects.new(code+'__MODEL_ROOT',None);collection.objects.link(root);root.matrix_world=ROT;root['asset_revision']=REV;root['canonical_forward']='+Z';root['canonical_up']='+Y'
def parent_part():
 if group not in groups:
  o=bpy.data.objects.new(ident+'__'+group+'_ROOT',None);collection.objects.link(o);o.parent=root;o.matrix_basis=Matrix.Identity(4);groups[group]=o
 return groups[group]
def register(o,name,mat,local):
 o.name=ident+'__'+name
 for c in list(o.users_collection):c.objects.unlink(o)
 collection.objects.link(o);o.parent=parent_part();o.matrix_basis=local;o.data.materials.append(materials[mat]);current.append(o);return o

def box(name,pos,size,mat='Steel',angle=(0,0,0),bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1);o=bpy.context.object;register(o,name,mat,Matrix.Translation(Vector(pos))@Euler(tuple(math.radians(a) for a in angle),'XYZ').to_matrix().to_4x4()@Matrix.Diagonal(Vector((*size,1))))
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Manufactured chamfer','BEVEL');mod.width=min(bevel,min(size)*.2);mod.segments=1;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o

def cyl(name,pos,radius,depth,mat='Steel',axis=(0,0,1),verts=12):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=1,depth=1);o=bpy.context.object;q=Vector((0,0,1)).rotation_difference(Vector(axis).normalized());register(o,name,mat,Matrix.Translation(Vector(pos))@q.to_matrix().to_4x4()@Matrix.Diagonal(Vector((radius,radius,depth,1))))
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for face in o.data.polygons:face.use_smooth=len(face.vertices)==4
 mod=o.modifiers.new('Rim chamfer','BEVEL');mod.width=min(.012,depth*.1,radius*.15);mod.segments=1;bpy.ops.object.modifier_apply(modifier=mod.name);return o

def rod(name,a,b,r=.035,mat='Steel'):
 a,b=Vector(a),Vector(b);return cyl(name,(a+b)*.5,r,(b-a).length,mat,b-a)
def mesh(name,verts,faces,mat):
 data=bpy.data.meshes.new(ident+'__'+name);data.from_pydata(verts,[],faces);data.update();o=bpy.data.objects.new(ident+'__'+name,data);collection.objects.link(o);o.parent=parent_part();o.matrix_basis=Matrix.Identity(4);o.data.materials.append(materials[mat]);current.append(o);return o

def ring(name,pos,outer,inner,depth,mat='Machined',segments=8):
 verts=[];faces=[]
 for z,r in [(-depth*.5,outer),(depth*.5,outer),(-depth*.5,inner),(depth*.5,inner)]:
  for i in range(segments):
   a=(i+.5)*math.tau/segments;verts.append((pos[0]+math.cos(a)*r,pos[1]+math.sin(a)*r,pos[2]+z))
 for i in range(segments):
  j=(i+1)%segments;faces.extend([(i,j,segments+j,segments+i),(2*segments+j,2*segments+i,3*segments+i,3*segments+j),(segments+i,segments+j,3*segments+j,3*segments+i),(j,i,2*segments+i,2*segments+j)])
 return mesh(name,verts,faces,mat)
def socket(name,pos,parent='RECEIVER'):
 global group
 previous=group;group=parent;node=socket_nodes.get(name)
 if node is None:node=bpy.data.objects.new(ident+'__'+name,None);collection.objects.link(node);socket_nodes[name]=node
 node.parent=parent_part();node.location=pos;node.empty_display_size=.08;group=previous;sockets[name]=list(pos);return node

def trim(side=1,front=.42):
 for s in [-1,1]:
  box('Ejection recess',(s*.178,.045,-.03),(.025,.12,.22),'Recess',bevel=.008);box('Bolt handle',(s*.205,.04,.02),(.065,.07,.095),'Machined',bevel=.012)
  for z in [-.19,.20]:cyl('Captive fastener',(s*.184,.10,z),.027,.014,'Machined',axis=(1,0,0),verts=8)
  for n in range(3):box('Serial marking',(s*.198,-.07,-.13+n*.035),(.008,.015,.023),'White',bevel=0)
 box('Top picatinny rail',(0,.185,.16),(.19,.045,.75),'Recess',bevel=.008)
 for z in [-.12,-.02,.08,.18,.28,.38,.48]:box('Rail cross tooth',(0,.22,z),(.22,.035,.045),'Machined',bevel=.005)

def optic(kind='scope',future=False):
 a='White' if future else 'TanLight';b='PurpleDark' if future else 'Steel';lamp='Glow_Cyan' if future else 'Safety'
 box('Optic saddle',(0,.26,-.03),(.20,.10,.26),b)
 if kind=='scope':
  cyl('Optic body',(0,.38,-.03),.095,.40,a,verts=8);ring('Optic front rim',(0,.38,.19),.12,.083,.07,b);cyl('Optic front glass',(0,.38,.209),.078,.015,'Glass_Cyan' if future else 'Recess',verts=12);ring('Lens retaining ring',(0,.38,.22),.082,.070,.018,'Safety');box('Lens highlight',(-.025,.40,.232),(.025,.03,.005),'Glow_Ice' if future else 'Machined');box('Turret block',(.13,.39,-.02),(.09,.11,.1),b)
 else:
  box('Sight base',(0,.32,-.02),(.25,.12,.22),a);box('Sight hood',(0,.43,-.02),(.27,.10,.22),b);box('Sight left',(-.125,.37,.03),(.04,.13,.10),b);box('Sight right',(.125,.37,.03),(.04,.13,.10),b);box('Sight glass',(0,.39,.075),(.18,.10,.012),'Glass_Cyan');box('Reticle',(0,.39,.084),(.035,.035,.012),lamp)

def magazine(drum=False,future=False,long=False):
 global group
 previous=group;group='MAGAZINE';case='White' if future else 'Tan';rib='PurpleDark' if future else 'Recess'
 if drum:
  cyl('Drum',(0,-.34,.08),.27,.36,rib,axis=(1,0,0));cyl('Drum outer cap',(-.20,-.34,.08),.235,.04,case,axis=(1,0,0));cyl('Drum hub',(-.23,-.34,.08),.075,.05,'Machined',axis=(1,0,0))
 else:
  count=4 if long else 3
  for i in range(count):
   box('Curved magazine segment',(0,-.23-i*.115,.15-i*.025),(.17,.145,.25),case,angle=(i*7,0,0),bevel=.014)
   for side in [-1,1]:box('Magazine rib',(side*.088,-.23-i*.115,.16-i*.025),(.012,.09,.15),rib,angle=(i*7,0,0),bevel=.003)
  box('Magazine floorplate',(0,-.30-(count-1)*.115,.15-(count-1)*.025),(.22,.06,.28),rib,angle=(count*5,0,0))
 socket('MAGAZINE_RELEASE',(0,-.2,.15),'MAGAZINE');group=previous

def basegun(future=False,stock=True,mag=True):
 color='White' if future else 'TanLight';accent='Purple' if future else 'Steel'
 box('Receiver',(0,0,0),(.34,.32,.64),accent);box('Receiver top armor',(0,.11,-.04),(.39,.12,.48),color);trim()
 box('Pistol grip',(0,-.23,-.08),(.19,.38,.20),'PurpleDark' if future else 'Recess',angle=(-9,0,0))
 box('Trigger guard front',(0,-.23,.12),(.045,.17,.04),'Machined');box('Trigger guard lower',(0,-.31,.055),(.055,.045,.17),'Machined');box('Trigger',(0,-.19,.04),(.035,.08,.035),'Recess',angle=(-20,0,0))
 if stock:
  box('Stock spine',(0,.05,-.61),(.22,.16,.53),color);box('Butt pad',(0,-.035,-.86),(.27,.37,.10),'PurpleDark' if future else 'Recess')
  box('Industrial stock lower brace',(0,-.14,-.61),(.14,.08,.43),color,angle=(-12,0,0));box('Stock adjustment',(0,-.02,-.43),(.27,.16,.11),accent)
 if mag:magazine(future=future)
 socket('GRIP_MAIN',(0,-.23,-.08));socket('GRIP_SUPPORT',(0,-.19,.49));socket('MOUNT',(0,0,0));socket('OPTIC_MOUNT',(0,.23,-.03))

def barrel(z=1.17,r=.11,future=False,length=.63):
 cyl('Barrel',(0,.015,z-length*.5),r*.70,length,'PurpleDark' if future else 'Recess');ring('Muzzle crown',(0,.015,z),r*1.30,r*.73,.18,'White' if future else 'Machined');cyl('Dark bore',(0,.015,z-.065),r*.70,.012,'Recess');socket('MUZZLE',(0,.015,z+.09),'BARREL')

def handguard(front=.50,length=.66,future=False):
 color='White' if future else 'Tan';box('Handguard body',(0,.01,front),(.30,.25,length),color)
 for side in [-1,1]:
  for j in range(5):box('Recessed cooling slot',(side*.153,.02,front-length*.35+j*length*.17),(.012,.12,.065),'PurpleDark' if future else 'Recess',bevel=.006)
 box('Lower rail',(0,-.145,front),(.21,.055,length*.8),'Recess')

def make_military(kind):
 begin('Military_'+kind,'military')
 if kind in ['AR','LMG','SR','SG','SMG']:
  basegun(mag=kind!='LMG');long=kind=='SR';short=kind=='SMG';front=.38 if short else .54;handguard(front,.44 if short else .70)
  if kind=='AR':box('Vertical support grip',(0,-.28,.45),(.13,.34,.14),'Recess');socket('GRIP_SUPPORT',(0,-.28,.45))
  if kind=='SG':
   box('Pump grip',(0,-.05,.58),(.34,.25,.46),'Steel')
   for z in [.42,.50,.58,.66,.74]:box('Pump rib',(0,-.055,z),(.36,.24,.025),'Recess',bevel=.003)
  barrel(1.50 if long else .97 if short else 1.20,.115,length=.85 if long else .52);optic('scope' if kind in ['AR','SR'] else 'dot')
  if kind in ['LMG','SR']:
   global group
   group='BIPOD'
   for s in [-1,1]:rod('Bipod leg',(s*.10,-.13,.79),(s*.36,-.70,.90),.036);box('Bipod foot',(s*.36,-.72,.90),(.17,.04,.17),'Recess')
   socket('BIPOD_HINGE',(0,-.13,.79),'BIPOD');group='RECEIVER'
  if kind=='LMG':
   box('Belt box',(.30,-.31,.03),(.46,.40,.44),'Olive');box('Feed cover',(.30,-.095,.03),(.50,.06,.47),'Steel')
   for j in range(7):
    x=[.22,.39,.56,.56,.56,.56,.56][j];y=[.10,.10,.03,-.04,-.11,-.18,-.25][j];cyl('Exposed linked brass cartridge',(x,y,.12),.035,.22,'Brass',axis=(0,0,1),verts=8);box('Belt link',(x,y,.06),(.045,.055,.05),'Steel',bevel=.005)
  if kind=='SG':
   for j in range(4):cyl('Side shell',(-.22,.02,-.18+j*.12),.046,.24,'Red',axis=(0,1,0),verts=8);cyl('Shell rim',(-.22,-.10,-.18+j*.12),.05,.03,'Brass',axis=(0,1,0),verts=8)
 elif kind=='HG':
  basegun(stock=False,mag=False);box('Pistol slide',(0,.075,.12),(.30,.20,.77),'TanLight');box('Slide relief',(0,.055,.40),(.315,.12,.15),'Recess');barrel(.59,.10,length=.38);box('Front sight',(0,.21,.42),(.06,.055,.08),'Safety');box('Rear sight',(0,.21,-.22),(.16,.055,.07),'Steel')
 elif kind=='GL':
  basegun(mag=False);box('Rotating drum shroud',(0,-.03,.42),(.58,.50,.48),'Steel');magazine(drum=True)
  for j in range(6):
   a=j*math.tau/6;cyl('Drum chamber',(.23*math.cos(a),-.04+.23*math.sin(a),.40),.085,.35,'Safety' if j%2 else 'Steel',verts=8)
  cyl('Heavy barrel',(0,.025,.90),.23,.70,'Steel',verts=8);ring('Heavy muzzle',(0,.025,1.27),.32,.22,.20,'Brass');cyl('Deep cannon bore',(0,.025,1.20),.214,.015,'Recess');socket('MUZZLE',(0,.025,1.38));optic()
 elif kind=='RL':
  basegun(stock=False,mag=False);cyl('Olive launch tube',(0,.08,.25),.245,1.95,'Olive',verts=12);ring('Large launch bell',(0,.08,1.26),.34,.247,.24,'Brass');cyl('Tube darkness',(0,.08,1.15),.24,.02,'Recess');ring('Rear collar',(0,.08,-.72),.28,.21,.12,'Steel')
  for z in [.67,.84]:ring('Warning band',(0,.08,z),.251,.242,.12,'Safety',segments=12)
  box('Top spine',(0,.32,.02),(.18,.12,.71),'Steel');optic();socket('MUZZLE',(0,.08,1.39))
 elif kind=='FL':
  basegun(stock=False,mag=False);handguard(.40,.50);barrel(.99,.16,length=.50);ring('Igniter collar',(0,.015,1.01),.21,.15,.13,'Safety');cyl('Pilot igniter',(-.15,-.15,.95),.035,.20,'Machined');box('Pilot tip',(-.15,-.15,1.065),(.055,.055,.035),'Glow_Pink')
  cyl('Red fuel cell',(-.39,-.02,-.23),.23,.78,'Red',axis=(0,1,0));cyl('Tank cap',(-.39,.40,-.23),.17,.08,'Steel',axis=(0,1,0));box('Tank warning label',(-.625,-.03,-.23),(.018,.28,.21),'Safety')
  curve=bpy.data.curves.new(ident+'__Hose','CURVE');curve.dimensions='3D';curve.bevel_depth=.042;curve.bevel_resolution=2;curve.use_fill_caps=True;spline=curve.splines.new('BEZIER');spline.bezier_points.add(3)
  for b,p in zip(spline.bezier_points,[(-.40,-.36,-.23),(-.35,-.51,.01),(-.10,-.48,.28),(0,-.14,.40)]):b.co=p;b.handle_left_type=b.handle_right_type='AUTO'
  ob=bpy.data.objects.new(ident+'__FuelHose',curve);collection.objects.link(ob);ob.parent=parent_part();ob.data.materials.append(materials['Brass']);bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob;bpy.ops.object.convert(target='MESH');current.append(bpy.context.object)
  socket('FUEL_CONNECTOR',(-.4,-.36,-.23));socket('MUZZLE',(0,.015,1.12))
 finish(kind,'firearm')

def blade_prism(name,outline,thickness,mat):
 n=len(outline);verts=[(x,y,z) for y in [-thickness/2,thickness/2] for x,z in outline];faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)];return mesh(name,verts,faces,mat)
def plate(name,width,height,depth,mat,z=0):
 x=width/2;y=height/2;c=min(width,height)*.16;outline=[(-x+c,-y),(x-c,-y),(x,-y+c),(x,y-c),(x-c,y),(-x+c,y),(-x,y-c),(-x,-y+c)];n=len(outline);verts=[(a,b,z+d) for d in [-depth/2,depth/2] for a,b in outline];faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)];return mesh(name,verts,faces,mat)

def make_future(kind):
 global group
 begin('Future_'+kind,'future')
 if kind in ['Rifle','Sniper','Beam','Shotgun']:
  basegun(True);handguard(.48,.62,True);optic('scope',True)
  if kind=='Sniper':barrel(1.57,.15,True,.95);box('Long barrel support',(0,-.075,.98),(.15,.12,.70),'PurpleDark')
  elif kind=='Beam':
   for side in [-1,1]:box('Energy acceleration rail',(side*.20,-.075,.76),(.10,.08,1.07),'White');box('Exposed pink core',(side*.255,.025,.76),(.025,.08,1.03),'Glow_Pink');box('Upper rail lip',(side*.20,.12,.76),(.10,.06,1.07),'PurpleDark')
   box('Beam crown',(0,.025,1.29),(.47,.39,.23),'Purple');box('Beam aperture',(0,.025,1.414),(.28,.23,.025),'Glow_Pink');box('Beam white core',(0,.025,1.430),(.12,.12,.008),'Glow_Ice');socket('MUZZLE',(0,.025,1.44))
  else:
   box('Square muzzle',(0,.025,1.10),(.43,.39,.28),'White');box('Aperture recess',(0,.025,1.248),(.29,.25,.025),'PurpleDark');box('Energy aperture',(0,.025,1.267),(.20,.16,.018),'Glow_Cyan' if kind=='Rifle' else 'Recess');socket('MUZZLE',(0,.025,1.29))
  for side in [-1,1]:box('Yellow release latch',(side*.19,-.015,-.08),(.055,.10,.14),'Safety')
  if kind=='Shotgun':
   for j in range(3):cyl('Red shell rack',(-.28,.055,-.2+j*.20),.075,.34,'Red',axis=(0,1,0));cyl('Shell yellow rim',(-.28,-.125,-.2+j*.2),.08,.045,'Safety',axis=(0,1,0))
 elif kind=='Gatling':
  basegun(True,False,False);box('Motor casing',(0,.04,.13),(.65,.56,.68),'White');ring('Yellow barrel clamp',(0,.04,.62),.39,.30,.17,'Safety',segments=12);group='BARREL'
  for j in range(3):
   a=(j/3+.25)*math.tau;x=.19*math.cos(a);y=.04+.19*math.sin(a);cyl('Rotary barrel',(x,y,.87),.105,.62,'PurpleDark');ring('Rotary muzzle',(x,y,1.21),.13,.075,.14,'Safety');cyl('Bore',(x,y,1.17),.072,.01,'Recess');socket('MUZZLE_'+str(j),(x,y,1.29),'BARREL')
  socket('MUZZLE',(0,.04,1.29),'BARREL');group='RECEIVER';box('Side power pack',(-.43,-.10,.15),(.24,.35,.42),'Purple');box('Pack latch',(-.56,-.10,.15),(.04,.16,.13),'Safety')
 elif kind=='Launcher':
  basegun(True,True,False);box('Four-cell housing',(0,.06,.35),(.88,.77,.94),'White');box('Front violet plate',(0,.06,.84),(.91,.80,.10),'Purple')
  for x in [-.20,.20]:
   for y in [-.12,.24]:ring('Red launch cell',(x,y,.94),.185,.125,.15,'Red');cyl('Cell recess',(x,y,.90),.12,.012,'Recess')
  box('Warning band',(0,.43,.30),(.74,.08,.21),'Safety');optic('dot',True);socket('MUZZLE',(0,.06,1.03))
 elif kind=='Sword':
  box('Handle',(0,0,-.34),(.16,.19,.52),'PurpleDark');box('Guard',(0,0,-.04),(.57,.25,.17),'White');box('Guard core',(0,.15,-.04),(.20,.10,.16),'Safety')
  outline=[(-.16,.05),(.16,.05),(.12,1.42),(0,1.83),(-.12,1.42)];blade_prism('Pink edge',outline,.075,'Glow_Pink');blade_prism('Blade white core',[(-.07,.07),(.07,.07),(.05,1.40),(0,1.69),(-.05,1.40)],.083,'Glow_Ice');socket('GRIP_MAIN',(0,0,-.33));socket('TIP',(0,0,1.84));socket('MOUNT',(0,0,-.33))
 elif kind=='Hammer':
  box('Handle',(0,0,.29),(.16,.19,1.23),'PurpleDark');box('Hand grip',(0,0,-.25),(.20,.23,.40),'Steel');box('Hammer core',(0,0,1.05),(.48,.56,.48),'Safety')
  for side in [-1,1]:box('Hammer cheek',(side*.39,0,1.05),(.34,.66,.58),'White');box('Impact face',(side*.59,0,1.05),(.10,.53,.48),'Purple');box('Pulse line',(side*.65,0,1.05),(.012,.12,.30),'Glow_Cyan')
  socket('GRIP_MAIN',(0,0,-.25));socket('TIP',(0,0,1.34));socket('MOUNT',(0,0,-.25))
 elif kind in ['Shield','HoloShield']:
  plate('Outer shield',1.06,1.60,.16,'White');plate('Inset panel',.84,1.35,.08,'Purple',.12);box('Center yellow spine',(0,.02,.19),(.22,1.10,.06),'Safety');box('Upper release',(0,.63,.17),(.27,.12,.08),'PurpleDark');box('Shield core',(0,-.48,.22),(.23,.18,.05),'Glow_Cyan')
  for side in [-1,1]:box('Panel armor',(side*.40,.0,.17),(.13,.74,.08),'White');rod('Back handle',(side*.19,-.18,-.18),(side*.19,.18,-.18),.04,'Steel')
  if kind=='HoloShield':
   plate('Projected cyan barrier',1.50,1.98,.015,'Glass_Cyan',.30)
   for row in range(-3,4):
    for col in range(-2,3):
     x=col*.29+(row%2)*.145;y=row*.252
     if abs(x)>.61 or abs(y)>.85:continue
     pts=[(x+math.cos(j*math.tau/6)*.168,y+math.sin(j*math.tau/6)*.168,.325) for j in range(6)]
     for j in range(6):rod('Hologram hex edge',pts[j],pts[(j+1)%6],.006,'Glow_Cyan')
  socket('GRIP_MAIN',(0,0,-.18));socket('MOUNT',(0,0,-.18))
 elif kind=='Drone':
  box('Core enclosure',(0,0,0),(.67,.67,.54),'White');plate('Sensor bezel',.53,.53,.07,'Purple',.32);box('Cyan sensor',(0,0,.375),(.33,.33,.035),'Glow_Cyan');box('Sensor white glint',(-.08,.06,.40),(.08,.12,.008),'Glow_Ice')
  for x in [-.52,.52]:
   for y in [-.43,.43]:rod('Drone strut',(0,0,0),(x,y,0),.06,'PurpleDark');box('Node pod',(x,y,0),(.30,.30,.34),'White');box('Node lamp',(x,y,.18),(.10,.10,.025),'Safety')
  socket('GRIP_MAIN',(0,0,-.3));socket('MOUNT',(0,0,-.3))
 finish(kind,'firearm' if kind in ['Rifle','Sniper','Beam','Shotgun','Gatling','Launcher'] else 'special')

def make_throwable(kind):
 begin('Throwable_'+kind,'throwable')
 if kind=='Grenade':
  cyl('Grenade body',(0,0,0),.20,.48,'Olive',axis=(0,1,0),verts=8)
  for y in [-.15,0,.15]:
   for j in range(8):
    a=j*math.tau/8;box('Body segmentation',(math.cos(a)*.19,y,math.sin(a)*.19),(.12,.12,.08),'Olive',angle=(0,-j*45,0),bevel=.015)
  cyl('Fuse collar',(0,.27,0),.105,.10,'Steel',axis=(0,1,0));box('Safety lever',(.10,.25,0),(.055,.25,.10),'Machined',angle=(0,0,18));ring('Pull ring',(0,.34,.10),.09,.06,.025,'Machined',segments=12)
 else:
  color={'Smoke':'White','EMP':'Purple','Incendiary':'Safety'}[kind];box('Canister case',(0,0,0),(.32,.54,.24),'Steel');box('Colour band',(0,-.02,.13),(.27,.35,.025),color);box('Label window',(0,-.02,.15),(.12,.18,.012),'Recess');box('Fuse lid',(0,.30,0),(.29,.08,.22),'Machined');box('Arming cap',(0,.36,0),(.10,.06,.10),'Safety')
 socket('GRIP_MAIN',(0,0,0));socket('MOUNT',(0,0,0));finish(kind,'special')

def make_module(kind):
 begin('Module_'+kind,'module')
 if kind in ['Scope','DotSight','HoloSight']:
  optic('scope' if kind=='Scope' else 'dot');
  if kind=='HoloSight':box('Wide holo hood',(0,.46,-.02),(.42,.10,.23),'TanLight');box('Holo lens',(0,.36,.09),(.31,.18,.012),'Glass_Cyan')
 elif kind=='Laser':
  box('Laser body',(0,0,0),(.24,.22,.53),'Steel');box('Laser top',(0,.13,0),(.19,.07,.25),'TanLight');ring('Laser lens mount',(0,0,.30),.095,.06,.08,'Machined');cyl('Laser lens',(0,0,.345),.052,.012,'Red')
 elif kind in ['ExtendedMag','DrumMag']:magazine(kind=='DrumMag',long=True)
 elif kind=='Suppressor':cyl('Suppressor body',(0,0,0),.17,.88,'Steel',verts=12);ring('Suppressor crown',(0,0,.47),.177,.09,.08,'Recess');cyl('Dark bore',(0,0,.43),.088,.01,'Recess');ring('Mount collar',(0,0,-.42),.18,.12,.11,'Machined')
 elif kind=='FlashHider':
  ring('Crown',(0,0,.18),.19,.115,.08,'Machined');ring('Rear mount',(0,0,-.17),.17,.09,.10,'Steel')
  for j in range(6):a=j*math.tau/6;rod('Slotted muzzle cage',(math.cos(a)*.15,math.sin(a)*.15,-.12),(math.cos(a)*.18,math.sin(a)*.18,.15),.035,'Steel')
 socket('GRIP_MAIN',(0,0,0));socket('MOUNT',(0,0,0));finish(kind,'module')

scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE_NEXT';scene.render.resolution_x=768;scene.render.resolution_y=512;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.world=bpy.data.worlds.new('WeaponStudioWorld');scene.world.color=(.055,.065,.085);scene.view_settings.view_transform='AgX'
for name,pos,power,size in [('Key',(-3,-4,6),900,4),('Fill',(4,-1,3),650,4),('Rim',(1,4,5),1000,3)]:
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.shape='DISK';data.size=size;o=bpy.data.objects.new(name,data);scene.collection.objects.link(o);o.location=pos;o.rotation_euler=(Vector((0,0,0))-o.location).to_track_quat('-Z','Y').to_euler()
cam_data=bpy.data.cameras.new('Asset inspection');cam=bpy.data.objects.new('Asset inspection',cam_data);scene.collection.objects.link(cam);scene.camera=cam;cam_data.type='ORTHO';cam.location=ROT@Vector((5,2.5,8));cam.rotation_euler=((ROT@Vector((0,0,.12)))-cam.location).to_track_quat('-Z','Y').to_euler();cam_data.ortho_scale=3.4

def finish(title,kind):
 # Join static parts by named mechanical group, retaining magazine/barrel/bipod roots.
 for part,parent in groups.items():
  meshes=[o for o in collection.objects if o.type=='MESH' and o.parent==parent]
  if not meshes:continue
  bpy.ops.object.select_all(action='DESELECT')
  for o in meshes:o.select_set(True)
  bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join();bpy.context.object.name=ident+'__'+part+'_MESH'
 bpy.context.view_layer.update();meshes=[o for o in collection.objects if o.type=='MESH'];inv=root.matrix_world.inverted();points=[inv@o.matrix_world@v.co for o in meshes for v in o.data.vertices];lo=[min(p[i] for p in points) for i in range(3)];hi=[max(p[i] for p in points) for i in range(3)];verts=sum(len(o.data.vertices) for o in meshes)
 assert verts>8 and verts<15000,(ident,verts)
 if kind=='firearm':assert 'MUZZLE' in sockets and sockets['MUZZLE'][2]>.5,ident
 bpy.ops.object.select_all(action='DESELECT')
 for o in collection.objects:o.select_set(True)
 bpy.context.view_layer.objects.active=root
 path=os.path.join(OUT,ident+'_'+REV+'.fbx');bpy.ops.export_scene.fbx(filepath=path,use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',bake_anim=False,add_leaf_bones=False,use_custom_props=True)
 record={'id':ident,'title':title,'family':family,'kind':kind,'resource':'PDParts/Weapons20261105/'+ident+'_'+REV,'version':3,'vertexCount':verts,'meshCount':len(meshes),'min':lo,'max':hi,'sockets':sockets,'movingRoots':list(groups)};catalog.append(record)
 cam_data.ortho_scale=max(1.05,3.4 if kind=='firearm' else max(hi[i]-lo[i] for i in range(3))*1.8);scene.render.filepath=os.path.join(DOC,ident+'.png');bpy.ops.render.render(write_still=True)
 collection.hide_render=True;root.location=Vector(((len(catalog)-1)%6*4,((len(catalog)-1)//6)*4,0));print('WEAPON_MODEL_PASS',ident,'vertices',verts,flush=True)

for kind in ['AR','LMG','SR','SG','GL','RL','HG','SMG','FL']:make_military(kind)
for kind in ['Rifle','Gatling','Launcher','Sniper','Beam','Shotgun','Sword','Hammer','Shield','HoloShield','Drone']:make_future(kind)
for kind in ['Grenade','Smoke','EMP','Incendiary']:make_throwable(kind)
for kind in ['Scope','DotSight','HoloSight','Laser','ExtendedMag','DrumMag','Suppressor','FlashHider']:make_module(kind)
for c in bpy.data.collections:c.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(SRC,'WeaponLibrary_'+REV+'.blend'))
with open(os.path.join(OUT,'catalog_'+REV+'.json'),'w',encoding='utf-8') as f:json.dump({'models':catalog},f,ensure_ascii=False,indent=2)
with open(os.path.join(DOC,'ModelReport.json'),'w',encoding='utf-8') as f:json.dump(catalog,f,ensure_ascii=False,indent=2)
print('WEAPON_LIBRARY_PASS',len(catalog),'models',sum(x['vertexCount'] for x in catalog),'native vertices',flush=True)
