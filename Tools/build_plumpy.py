import bpy, math, random, os
from mathutils import Vector
random.seed(27)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
materials={}
def mat(name,rgb):
 m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);m.use_nodes=True;m.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(*rgb,1);m.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.48;materials[name]=m;return m
green=mat('Plumpy moss green',(.31,.62,.085));light=mat('Fur leaf green',(.42,.72,.12));dark=mat('Fur deep green',(.16,.38,.025));inner=mat('Inner ear',(.40,.55,.10));white=mat('Warm eye white',(1,.94,.73));black=mat('Pupil',(.012,.008,.004));iris=mat('Amber iris',(.38,.16,.025));mouth=mat('Smile',(.13,.025,.018));pink=mat('Tongue',(.82,.15,.26));wood=mat('Basket dark wood',(.27,.095,.025));wicker=mat('Basket wicker',(.58,.29,.055));plum=mat('Purple plums',(.31,.035,.39));plumlight=mat('Plum bloom',(.48,.10,.51));stem=mat('Plum stem',(.18,.09,.015))
def sphere(name,p,s,m,segments=24,rings=16):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,location=p);o=bpy.context.object;o.name=name;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
 for f in o.data.polygons:f.use_smooth=True
 return o
def tube(name,points,r,m):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.resolution_u=12;c.bevel_depth=r;c.bevel_resolution=3;s=c.splines.new('BEZIER');s.bezier_points.add(len(points)-1)
 for b,p in zip(s.bezier_points,points):b.co=p;b.handle_left_type='AUTO';b.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);bpy.context.collection.objects.link(o);o.data.materials.append(m);return o
def tuft(name,p,length,width,direction,m):
 vertices=[];faces=[];levels=7;steps=10
 for j in range(levels):
  t=j/(levels-1);radius=width*(math.sin(math.pi*t)**.65)*.5+.002
  for i in range(steps):
   a=i*2*math.pi/steps;vertices.append((math.cos(a)*radius+length*.17*t*t,math.sin(a)*radius*.55,length*t))
 for j in range(levels-1):
  for i in range(steps):a=j*steps+i;b=j*steps+(i+1)%steps;faces.append((a,b,b+steps,a+steps))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.location=p;o.rotation_mode='QUATERNION';o.rotation_quaternion=Vector((0,0,1)).rotation_difference(Vector(direction).normalized());o.data.materials.append(m)
 for f in mesh.polygons:f.use_smooth=True
 return o
skin=mat('Golden face and ears',(.95,.60,.18));blush=mat('Warm rosy cheeks',(.88,.30,.15))
body=[]
body.append(sphere('Pear shaped torso',(0,.015,.36),(.30,.22,.28),green))
body.append(sphere('Head',(0,-.015,.70),(.205,.16,.19),green))
for side in [-1,1]:
 body.append(sphere('Bent haunch',(side*.14,.015,.17),(.105,.12,.13),green))
 body.append(sphere('Broad foot',(side*.14,-.085,.065),(.105,.17,.065),green))
 body.append(sphere('Shoulder',(side*.205,-.005,.50),(.10,.095,.13),green))
 body.append(sphere('Forearm',(side*.265,-.095,.42),(.08,.09,.13),green))
 body.append(sphere('Palm',(side*.285,-.18,.40),(.08,.065,.08),green))
 for digit in range(3):body.append(sphere('Rounded fingers',(side*(.25+digit*.028),-.225,.40-digit*.015),(.022,.035,.037),green,16,12))
 for toe in range(3):body.append(sphere('Toes',(side*.14+(toe-1)*.045,-.215,.065),(.027,.035,.029),green,16,12))
 body.append(sphere('Cheek',(side*.105,-.14,.66),(.10,.075,.08),green))
# Face and nose are separate warm skin surfaces, as on the illustrated card.
bpy.ops.object.select_all(action='DESELECT')
for o in body:o.select_set(True)
bpy.context.view_layer.objects.active=body[0];bpy.ops.object.join();o=bpy.context.object;o.name='Plumpy continuous sculpted body';rem=o.modifiers.new('Organic fused anatomy','REMESH');rem.mode='VOXEL';rem.voxel_size=.007;bpy.ops.object.modifier_apply(modifier=rem.name);sm=o.modifiers.new('Smooth sculpt','SMOOTH');sm.factor=.7;sm.iterations=4;bpy.ops.object.modifier_apply(modifier=sm.name)
sphere('Golden expressive face',(0,-.155,.715),(.155,.065,.145),skin)
sphere('Round golden nose',(0,-.265,.71),(.086,.070,.064),skin)
# Pointed ears with inner panels, not spheres.
for side in [-1,1]:
 verts=[(side*.14,.02,.73),(side*.31,.025,.84),(side*.24,-.025,.65),(side*.21,-.065,.75),(side*.22,.06,.75)]
 faces=[(0,3,1),(1,3,2),(2,3,0),(0,1,4),(1,2,4),(2,0,4)]
 mesh=bpy.data.meshes.new('Pointed ear');mesh.from_pydata(verts,[],faces);mesh.update();ear=bpy.data.objects.new('Pointed ear',mesh);bpy.context.collection.objects.link(ear);ear.data.materials.append(green);bev=ear.modifiers.new('Soft ear rim','BEVEL');bev.width=.014;bev.segments=3
 for poly in mesh.polygons:poly.use_smooth=True
 tube('Inner ear ridge',[(side*.18,-.058,.735),(side*.285,-.018,.785),(side*.235,-.043,.70)],.015,skin)
 sphere('Eye socket',(side*.077,-.145,.752),(.069,.026,.077),dark)
 sphere('Large expressive eye',(side*.077,-.166,.756),(.057,.025,.067),white)
 sphere('Amber iris',(side*.069,-.189,.754),(.027,.012,.039),iris)
 sphere('Pupil',(side*.066,-.20,.754),(.015,.008,.027),black)
 sphere('Eye highlight',(side*.066-.006,-.208,.769),(.006,.004,.008),white,16,12)
 tube('Bushy eyebrow',[(side*.025,-.17,.81),(side*.08,-.177,.835),(side*.13,-.14,.82)],.015,dark)
 sphere('Rosy green cheek',(side*.135,-.19,.681),(.037,.009,.022),blush)
sphere('Happy open smile',(0,-.177,.626),(.082,.017,.047),mouth)
sphere('Tongue',(0,-.195,.609),(.041,.008,.014),pink)
tube('Smile upper lip',[(-.075,-.19,.641),(0,-.199,.629),(.075,-.19,.641)],.008,dark)
for i in [-1,1]:sphere('Little tooth',(i*.012,-.199,.643),(.011,.007,.012),white,16,12)
# Layered groom over belly, flanks and back, swept down into soft leaf-shaped locks.
for row in range(7):
 z=.19+row*.073;ry=.20*math.sqrt(max(.1,1-((z-.36)/.30)**2));rx=.27*math.sqrt(max(.1,1-((z-.36)/.31)**2))
 for j in range(18):
  a=j*2*math.pi/18+row*.14;x=rx*math.cos(a);y=.015+ry*math.sin(a)
  tuft('Layered body fur',(x,y,z),.085+random.random()*.025,.069,(math.cos(a)*.18,math.sin(a)*.18,-1),random.choice([green,green,light]))
for j in range(16):
 a=j*2*math.pi/16;tuft('Head hair',(.18*math.cos(a),.02+.13*math.sin(a),.80),.10,.065,(math.cos(a)*.4,math.sin(a)*.5,.35 if math.sin(a)>.2 else -.6),random.choice([green,light]))
for side in [-1,1]:
 tube('Soft swept forehead lock',[(side*.01,-.11,.88),(side*.06,-.15,.87),(side*.12,-.14,.83)],.022,light)
 tube('Curving belly fur',[(side*.045,-.215,.50),(side*.11,-.22,.41),(side*.09,-.22,.32)],.017,light)
for side in [-1,1]:
 for j in range(5):tuft('Side whisker',(side*(.14+j*.008),-.09,.71-j*.026),.085,.035,(side*.65,-.05,-.7),light if j%2 else green)
# Woven basket and actual individually modeled plums.
sphere('Basket bowl',(.37,-.11,.24),(.13,.105,.105),wood)
for k in range(5):
 z=.17+k*.029;r=.115*math.sqrt(max(.1,1-((z-.24)/.11)**2));tube('Wicker ring',[(.37+math.cos(a)*r,-.11+math.sin(a)*r*.85,z) for a in [i*2*math.pi/24 for i in range(25)]],.007,wicker)
for j in range(16):
 a=j*2*math.pi/16;tube('Vertical basket weave',[(.37+math.cos(a)*r,-.11+math.sin(a)*r*.85,z) for r,z in [(.055,.145),(.105,.19),(.125,.25),(.11,.315)]],.006,wicker)
tube('Basket handle',[(.26,-.11,.30),(.27,-.11,.43),(.37,-.11,.48),(.47,-.11,.43),(.48,-.11,.30)],.014,wicker)
for j in range(8):
 a=j*2.4;r=.065*(j%3)/2;x=.37+math.cos(a)*r;y=-.11+math.sin(a)*r;z=.322+(j%2)*.025
 sphere('Purple plum',(x,y,z),(.039,.035,.042),plum if j%2 else plumlight)
 tube('Plum stem',[(x,y,z+.037),(x+.008,y,z+.053)],.003,stem)
# A plum held in the opposite palm.
sphere('Held plum',(-.28,-.25,.445),(.053,.046,.055),plum);tube('Held plum stem',[(-.28,-.25,.49),(-.27,-.25,.52)],.004,stem)
# Bring the eyes, mouth and cheek details forward onto the golden face mask.
for o in list(bpy.context.scene.objects):
 if any(word in o.name.lower() for word in ['eye socket','expressive eye','amber iris','pupil','eye highlight','eyebrow','cheek','happy open smile','tongue','smile upper lip','little tooth']):o.location.y-=.05
# Convert curves and apply modifiers for portable full geometry.
bpy.ops.object.select_all(action='SELECT')
for obj in list(bpy.context.selected_objects):
 bpy.context.view_layer.objects.active=obj
 if obj.type=='CURVE':bpy.ops.object.convert(target='MESH')
 for mod in list(obj.modifiers):
  try:bpy.ops.object.modifier_apply(modifier=mod.name)
  except:pass
# Group geometry by material, retaining a small draw-call count.
for material in materials.values():
 objs=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.data.materials and o.data.materials[0]==material]
 if not objs:continue
 bpy.ops.object.select_all(action='DESELECT')
 for o in objs:o.select_set(True)
 bpy.context.view_layer.objects.active=objs[0];bpy.ops.object.join();bpy.context.object.name=material.name
bpy.ops.object.select_all(action='SELECT')
folder=os.path.abspath('Assets/LivingTable/CandyLand/Models/Plumpy')
os.makedirs('Art/Plumpy',exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.abspath('Art/Plumpy/plumpy-sculpt.blend'))
bpy.ops.export_scene.fbx(filepath=os.path.join(folder,'plumpy-sculpt.fbx'),use_selection=True,apply_unit_scale=True,bake_space_transform=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'},add_leaf_bones=False)
bpy.ops.export_scene.gltf(filepath=os.path.abspath('Art/Plumpy/plumpy-sculpt.glb'),export_format='GLB',use_selection=True)
print('PLUMPY_SCULPT_EXPORTED',sum(len(o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH'))
