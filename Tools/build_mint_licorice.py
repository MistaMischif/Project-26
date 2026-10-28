import bpy, math, os, sys
from mathutils import Vector
ROOT=os.getcwd()
def material(name,c):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);return m
def sphere(name,p,s,m):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=p);o=bpy.context.object;o.name=name;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
 for f in o.data.polygons:f.use_smooth=True
 return o
def tube(name,pts,r,m):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.resolution_u=8;c.bevel_depth=r;c.bevel_resolution=3;s=c.splines.new('BEZIER');s.bezier_points.add(len(pts)-1)
 for b,p in zip(s.bezier_points,pts):b.co=p;b.handle_left_type='AUTO';b.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);bpy.context.collection.objects.link(o);o.data.materials.append(m);return o
def mesh(name,verts,faces,m):
 data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update();o=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(o);o.data.materials.append(m);return o
def striped(name,pts,r,a,b):
 # Closed tubular surface with real alternating spiral bands; no flat billboard parts.
 verts=[];faces=[];N=72;K=16
 for j in range(N+1):
  t=j/N;seg=min(len(pts)-2,int(t*(len(pts)-1)));u=t*(len(pts)-1)-seg;p=Vector(pts[seg]).lerp(Vector(pts[seg+1]),u);direction=(Vector(pts[seg+1])-Vector(pts[seg])).normalized();x=direction.cross(Vector((0,1,0))).normalized();y=direction.cross(x)
  for k in range(K):v=p+r*(x*math.cos(k*2*math.pi/K)+y*math.sin(k*2*math.pi/K));verts.append(v)
 for j in range(N):
  for k in range(K):faces.append((j*K+k,j*K+(k+1)%K,(j+1)*K+(k+1)%K,(j+1)*K+k))
 faces.extend([tuple(range(K-1,-1,-1)),tuple(N*K+k for k in range(K))]);o=mesh(name,verts,faces,a);o.data.materials.append(b)
 for j,f in enumerate(o.data.polygons):f.material_index=int(((j//K)/N*7+(j%K)/K)*2)%2;f.use_smooth=True
 return o
def face(z,skin,black,white,red,licorice=False):
 sphere('Sculpted face',(0,-.025,z),(.095,.09,.16) if licorice else (.115,.10,.145),skin)
 for side in [-1,1]:
  sphere('Ear',(side*.113,-.02,z-.005),(.028,.022,.044),skin)
  sphere('Eye',(side*.045,-.112,z+.032),(.029,.015,.043),white)
  sphere('Pupil',(side*.042,-.126,z+.03),(.012,.007,.022),black)
  sphere('Catchlight',(side*.042-.004,-.132,z+.04),(.004,.003,.007),white)
  tube('Expressive eyebrow',[(side*.02,-.123,z+.083),(side*.049,-.12,z+.094),(side*.079,-.103,z+.075)],.009,black if licorice else red)
 if licorice:
  nose=mesh('Long pointed theatrical nose',[(-.022,-.109,z+.025),(.022,-.109,z+.025),(.040,-.220,z-.025),(0,-.151,z-.039),(-.025,-.112,z-.02)],[(0,1,2),(0,2,4),(4,2,3),(3,2,1),(0,4,3,1)],skin);bev=nose.modifiers.new('Soft sculpted nose','BEVEL');bev.width=.008;bev.segments=3
  sphere('Angular chin',(0,-.055,z-.13),(.048,.06,.057),skin)
 else:
  sphere('Round red nose',(0,-.15,z-.008),(.052,.044,.046),red)
  sphere('Open laughing mouth',(0,-.12,z-.068),(.052,.018,.039),black)
  sphere('Happy tongue',(0,-.139,z-.083),(.028,.006,.012),red)
 tube('Smile',[(-.055,-.109,z-.053),(0,-.127,z-.077),(.055,-.109,z-.053)],.012,black)
 tube('Smile teeth',[(-.04,-.12,z-.055),(0,-.135,z-.065),(.04,-.12,z-.055)],.006,white)
 if licorice:
  for side in [-1,1]:tube('Curled moustache',[(0,-.14,z-.035),(side*.06,-.142,z-.048),(side*.10,-.13,z-.015),(side*.09,-.12,z)],.012,black)
  tube('Pointed goatee',[(0,-.11,z-.11),(.01,-.12,z-.147),(.023,-.095,z-.135)],.018,black)
def export(slug,folder):
 for o in list(bpy.context.scene.objects):
  bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
  if o.type=='CURVE':bpy.ops.object.convert(target='MESH')
  for mod in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
 for m in list(bpy.data.materials):
  obs=[o for o in bpy.context.scene.objects if o.type=='MESH' and len(o.data.materials)==1 and o.data.materials[0]==m]
  if not obs:continue
  bpy.ops.object.select_all(action='DESELECT')
  for o in obs:o.select_set(True)
  bpy.context.view_layer.objects.active=obs[0];bpy.ops.object.join();bpy.context.object.name=m.name
 bpy.ops.object.select_all(action='SELECT');out=os.path.join(ROOT,'Assets/LivingTable/CandyLand/Models',folder);art=os.path.join(ROOT,'Art',folder);os.makedirs(out,exist_ok=True);os.makedirs(art,exist_ok=True)
 bpy.ops.wm.save_as_mainfile(filepath=os.path.join(art,slug+'-sculpt.blend'))
 bpy.ops.export_scene.fbx(filepath=os.path.join(out,slug+'-sculpt.fbx'),use_selection=True,apply_unit_scale=True,bake_space_transform=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},add_leaf_bones=False)
 bpy.ops.export_scene.gltf(filepath=os.path.join(art,slug+'-sculpt.glb'),export_format='GLB',use_selection=True)
 print('CHARACTER_EXPORTED',slug,sum(len(o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH'))
for licorice in ([True] if '--licorice-only' in sys.argv else [False] if '--mint-only' in sys.argv else [False,True]):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 red=material('Candy red',(.78,.025,.055));white=material('Cream white',(1,.94,.83));black=material('Licorice black',(.022,.012,.028));pink=material('Mint suit pink',(.96,.24,.48));skin=material('Warm skin',(.95,.54,.34) if licorice else (1,.57,.65));darkpink=material('Pink piping',(.70,.06,.25))
 if not licorice:
  sphere('Puffed jacket',(0,0,.52),(.15,.105,.16),pink)
  for side in [-1,1]:
   sphere('Puffed shorts',(side*.08,0,.37),(.10,.10,.095),pink)
   striped('Peppermint stocking',[(side*.09,0,.35),(side*.13,-.005,.23),(side*.12,-.025,.08)],.031,red,white)
   sphere('Red rounded shoe',(side*.12,-.067,.055),(.065,.11,.05),red)
   tube('Shoe cuff',[(side*.12-.04,-.02,.10),(side*.12,-.065,.10),(side*.12+.04,-.02,.10)],.014,darkpink)
   sphere('Puff shoulder',(side*.16,0,.59),(.075,.075,.08),pink)
   tube('Sleeve',[(side*.17,0,.58),(side*.225,-.025,.50),(side*.255,-.08,.59)],.045,pink)
   sphere('Glove palm',(side*.26,-.08,.61),(.045,.032,.05),pink)
   for f in range(4):sphere('Gloved fingers',(side*.25+f*.012-.02,-.105,.62),(.010,.013,.028),pink)
   tube('Pink scalloped glove cuff',[(side*.23,-.08,.575),(side*.26,-.11,.58),(side*.29,-.08,.575)],.013,darkpink)
   sphere('Collar petal',(side*.058,-.07,.66),(.063,.035,.033),darkpink)
  for z in [.46,.52,.58]:sphere('Jacket button',(0,-.108,z),(.014,.009,.014),red)
  face(.79,skin,black,white,red)
  for side in [-1,1]:tube('Red curled hair',[(side*.07,-.035,.91),(side*.14,-.02,.83),(side*.14,-.045,.72),(side*.11,-.07,.74)],.025,red)
  # A genuinely curved, drooping cap, with rings perpendicular to the curved centerline.
  verts=[];faces=[];N=56;K=24
  for j in range(N+1):
   t=j/N;p=Vector((.30*t,.01,.92+.34*math.sin(math.pi*t)-.05*t));r=.12*(1-t)+.01
   tangent=Vector((.30,0,.34*math.pi*math.cos(math.pi*t)-.05)).normalized();normal=Vector((tangent.z,0,-tangent.x))
   for k in range(K):a=k*2*math.pi/K;verts.append(p+r*(normal*math.cos(a)+Vector((0,1,0))*math.sin(a)))
  for j in range(N):
   for k in range(K):faces.append((j*K+k,j*K+(k+1)%K,(j+1)*K+(k+1)%K,(j+1)*K+k))
  hat=mesh('Drooping peppermint cap',verts,faces,red);hat.data.materials.append(white)
  for f in hat.data.polygons:f.material_index=(f.index//K//7)%2;f.use_smooth=True
  sphere('Hat pompom',(.30,.01,.87),(.037,.037,.037),white)
  tube('White rolled hat brim',[(.12*math.cos(i*2*math.pi/24),.01+.10*math.sin(i*2*math.pi/24),.92) for i in range(25)],.015,white)
  for side in [-1,1]:tube('Swept red forelock',[(side*.025,-.08,.91),(side*.07,-.105,.93),(side*.115,-.075,.885)],.022,red)
  for x in [-.085,-.045,.045,.085]:tube('Soft jacket fold',[(x,-.075,.62),(x*.9,-.108,.54),(x*.85,-.083,.43)],.006,darkpink)
  tube('White candy belt',[(.14*math.cos(i*2*math.pi/32),.09*math.sin(i*2*math.pi/32),.405) for i in range(33)],.020,white)
  for x in [-.10,-.05,0,.05,.10]:tube('Red belt stripe',[(x-.008,-.093,.389),(x+.008,-.102,.421)],.009,red)
  striped('Peppermint axe handle',[(-.27,-.08,.14),(-.27,-.08,1.03)],.018,red,white)
  verts=[(-.28,-.10,.90),(-.45,-.10,.85),(-.44,-.10,1.08),(-.28,-.10,1.02),(-.28,-.05,.90),(-.45,-.05,.85),(-.44,-.05,1.08),(-.28,-.05,1.02)]
  axe=mesh('Candy axe blade',verts,[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)],white);bev=axe.modifiers.new('Rounded candy edges','BEVEL');bev.width=.012;bev.segments=3
  for i in range(3):tube('Axe red candy swirl',[(-.29,-.112,.94+i*.023),(-.36,-.115,.94+i*.025),(-.43,-.112,.89+i*.075)],.012,red)
  tube('Axe red candy edge',[(-.28,-.11,.90),(-.45,-.11,.85),(-.44,-.11,1.08),(-.28,-.11,1.02)],.009,red)
  export('mr-mint','MrMint')
 else:
  # Thick, pleated cape has front lining and back surface, not a plane.
  verts=[];faces=[];N=28
  for z,width in [(.22,.30),(.48,.25),(.81,.14)]:
   for k in range(N+1):t=k/N;verts.append(((t-.5)*2*width,.065+.08*math.sin(t*math.pi)+.018*math.cos(t*10*math.pi),z+.025*math.cos(t*6*math.pi)))
  for row in range(2):
   for k in range(N):a=row*(N+1)+k;faces.append((a,a+1,a+N+2,a+N+1))
  cape=mesh('Flowing red cape',verts,faces,red)
  for f in cape.data.polygons:f.use_smooth=True
  cape.data.materials.append(black);solid=cape.modifiers.new('Solid fabric thickness','SOLIDIFY');solid.thickness=.012;solid.material_offset=1;bevel=cape.modifiers.new('Soft fabric edges','BEVEL');bevel.width=.007;bevel.segments=2
  sphere('Tailored coat',(0,0,.64),(.105,.075,.18),black)
  for side in [-1,1]:
   sphere('Striped puff shorts',(side*.065,0,.45),(.08,.075,.09),black)
   for offset in [-.04,0,.04]:tube('Shorts candy stripe',[(side*.065+offset,-.042,.515),(side*.065+offset,-.075,.46),(side*.065+offset,-.035,.38)],.009,red)
   tube('Slender stocking',[(side*.06,0,.43),(side*.08,-.01,.28),(side*.11,-.02,.09)],.029,black)
   tube('Stocking red seam',[(side*.06,-.03,.43),(side*.08,-.04,.28),(side*.11,-.05,.09)],.006,red)
   sphere('Boot',(side*.11,-.04,.07),(.045,.075,.055),black)
   tube('Curled boot toe',[(side*.11,-.075,.04),(side*.13,-.15,.04),(side*.14,-.18,.09)],.027,black);sphere('Boot red tip',(side*.14,-.18,.10),(.02,.02,.02),red)
   sphere('Puff sleeve',(side*.12,0,.72),(.065,.063,.08),black)
   for dx in [-.025,0,.025]:tube('Sleeve red piping',[(side*.12+dx,-.03,.79),(side*.12+dx,-.063,.72),(side*.12+dx,-.03,.65)],.009,red)
   tube('Bent sleeve',[(side*.14,0,.70),(side*.22,-.02,.61),(side*.28,-.06,.69)],.035,black)
   sphere('Red glove',(side*.29,-.06,.71),(.035,.025,.035),red)
   for f in range(4):tube('Glove fingers',[(side*.29+(f-1.5)*.012,-.07,.72),(side*.31+(f-1.5)*.012,-.08,.755)],.008,red)
   tube('Red wavy coat trim',[(side*.035,-.07,.77),(side*.055,-.077,.67),(side*.03,-.072,.55),(side*.065,-.055,.47)],.011,red)
   collar=mesh('Raised pointed collar',[(side*.02,0,.79),(side*.20,.01,.87),(side*.12,-.06,.73),(side*.02,-.05,.78)],[(0,1,2,3)],red);sol=collar.modifiers.new('Collar thickness','SOLIDIFY');sol.thickness=.012
  face(.94,skin,black,white,red,True)
  sphere('Swept black hair',(0,.015,1.045),(.118,.09,.07),black)
  for side in [-1,1]:tube('Sideburn curl',[(side*.09,.01,1.04),(side*.115,-.02,.95),(side*.13,-.01,.91)],.018,black)
  # Broad upturned black hat with red edge, an asymmetric curl, and a sculpted feather.
  verts=[];faces=[];N=64
  for ring in [0,1]:
   for k in range(N):
    a=k*2*math.pi/N;rx=.115 if ring==0 else .205;ry=.07 if ring==0 else .12
    z=1.075+(0 if ring==0 else .055*abs(math.cos(a))+.012*math.sin(a))
    verts.append((rx*math.cos(a),.008+ry*math.sin(a),z))
  for k in range(N):faces.append((k,(k+1)%N,(k+1)%N+N,k+N))
  hat=mesh('Swept upturned hat brim',verts,faces,black);solid=hat.modifiers.new('Substantial hat brim','SOLIDIFY');solid.thickness=.012
  for f in hat.data.polygons:f.use_smooth=True
  tube('Hat red rim',[verts[N+k] for k in range(0,N,4)]+[verts[N]],.008,red)
  sphere('Black hat crown',(-.012,.025,1.115),(.132,.085,.075),black)
  tube('Crown red ribbon',[(-.12,-.005,1.117),(-.05,-.065,1.135),(.05,-.052,1.132),(.12,.005,1.12)],.009,red)
  tube('Sweeping hat curl',[(.12,-.04,1.105),(.23,-.05,1.14),(.25,-.04,1.215),(.21,-.02,1.23)],.018,black)
  tube('Red feather quill',[(-.07,.045,1.15),(-.13,.045,1.27),(-.24,.035,1.31),(-.27,.02,1.28)],.009,red)
  for i in range(11):
   t=i/10;x=-.09-.16*t;z=1.18+.14*math.sin(t*1.6)
   tube('Feather sculpted barbs',[(x,.045,z-.028),(x-.025,.04,z+.018),(x-.065,.035,z+.035)],.012*(1-.5*t),red)
  tube('Licorice belt',[(-.09,-.025,.535),(-.05,-.078,.53),(.05,-.078,.53),(.09,-.025,.535)],.016,red)
  sphere('Belt candy buckle',(0,-.096,.532),(.028,.012,.028),red)
  for side in [-1,1]:
   tube('Cloak curling red hem',[(side*.13,.09,.80),(side*.25,.09,.49),(side*.30,.07,.22),(side*.24,.02,.20),(side*.22,.01,.25)],.012,red)
   tube('Long curling coat flourish',[(side*.055,-.07,.62),(side*.08,-.075,.57),(side*.055,-.08,.52),(side*.085,-.07,.48)],.008,red)
  for k in range(3):
   a=k*2*math.pi/3;tube('Buckle black swirl',[(0,-.11,.532),(.018*math.cos(a),-.11,.532+.018*math.sin(a)),(.025*math.cos(a+.5),-.108,.532+.025*math.sin(a+.5))],.006,black)
  striped('Licorice cane',[(-.30,-.07,.025),(-.30,-.07,.75)],.013,red,black);sphere('Cane ruby',(-.30,-.07,.78),(.034,.034,.034),red)
  export('lord-licorice','LordLicorice')
