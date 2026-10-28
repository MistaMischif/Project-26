import bpy, math, os
from mathutils import Vector
# Reuse the project's curve, spiral stripe, export and mesh primitives, not its old models.
exec(open('Tools/build_mint_licorice.py',encoding='utf8').read().split('for licorice in')[0])

def lathe(name,rings,mat,pleat=0):
 verts=[];faces=[];N=48
 for row,(z,rx,ry) in enumerate(rings):
  for j in range(N):
   a=j*math.pi*2/N;r=1+pleat*math.cos(a*12);verts.append((rx*math.cos(a)*r,ry*math.sin(a)*r,z))
 for row in range(len(rings)-1):
  for j in range(N):k=row*N+j;n=row*N+(j+1)%N;faces.append((k,n,n+N,k+N))
 faces.extend([tuple(range(N-1,-1,-1)),tuple((len(rings)-1)*N+j for j in range(N))]);o=mesh(name,verts,faces,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o

def ring(name,p,rx,ry,r,mat):
 return tube(name,[(p[0]+rx*math.cos(i*math.pi/16),p[1]+ry*math.sin(i*math.pi/16),p[2]) for i in range(33)],r,mat)

def curl(name,p,r,mat,turns=1.5):
 pts=[]
 for i in range(27):
  t=i/26;a=t*turns*math.pi*2;rr=r*(1-t*.87);pts.append((p[0]+math.cos(a)*rr,p[1]-.012*t,p[2]+math.sin(a)*rr))
 return tube(name,pts,r*.27,mat)

def hand(p,mat,open=True):
 sphere('Sculpted palm',p,(.035,.025,.042),mat)
 for k in range(4):
  x=p[0]+(k-1.5)*.014;z=p[2]+.015
  tube('Individual rounded finger',[(x,p[1]-.006,z),(x+(k-1.5)*(.009 if open else .003),p[1]-.012,z+.052-(abs(k-1.5)*.008)),(x+(k-1.5)*.01,p[1]-.017,z+.055-(abs(k-1.5)*.008))],.008,mat)
 tube('Thumb',[(p[0]-.025,p[1],p[2]),(p[0]-.055,p[1]-.005,p[2]+.025)],.011,mat)

def eyes(z,y,spacing,skin,white,black,iris,smile=.07):
 for s in [-1,1]:
  sphere('Warm cheek',(s*spacing*1.4,y+.012,z-.045),(.045,.018,.025),blush)
  sphere('Eye white',(s*spacing,y,z+.025),(.035,.018,.048),white)
  sphere('Colored iris',(s*spacing,y-.016,z+.021),(.018,.009,.028),iris)
  sphere('Black pupil',(s*spacing,y-.024,z+.021),(.010,.005,.021),black)
  sphere('Eye glint',(s*spacing-.005,y-.028,z+.035),(.005,.003,.007),white)
  tube('Eyebrow',[(s*(spacing-.027),y+.005,z+.083),(s*spacing,y-.003,z+.093),(s*(spacing+.027),y+.008,z+.08)],.008,iris)
 sphere('Button nose',(0,y-.018,z-.018),(.028,.026,.024),skin)
 sphere('Open smile',(0,y+.001,z-.081),(smile,.017,.041),black)
 tube('Smiling teeth',[(-smile*.7,y-.019,z-.064),(0,y-.022,z-.072),(smile*.7,y-.019,z-.06)],.009,white)
 sphere('Tongue',(0,y-.018,z-.10),(.028,.007,.013),pink)

def woman_face(z):
 sphere('Soft sculpted face',(0,-.02,z),(.115,.095,.145),skin)
 eyes(z,-.112,.045,skin,white,black,brown,.042)
 # Softer illustrated smiles, rather than a large identical open mouth on every face.
 for o in list(bpy.context.scene.objects):
  if o.name.startswith('Open smile'):o.scale.z=.62
  if o.name.startswith('Tongue'):o.scale.z=.55
 for s in [-1,1]:
  sphere('Ear',(s*.112,-.015,z),(.021,.018,.033),skin)
  for j in range(3):tube('Eyelash',[(s*.070,-.125,z+.053),(s*(.076+j*.006),-.126,z+.063+j*.004)],.0035,black)

def basket(p):
 x,y,z=p;sphere('Wicker bowl',p,(.10,.08,.075),brown)
 for k in range(5):ring('Basket woven band',(x,y,z-.045+k*.024),.087,.068,.005,gold)
 for j in range(14):
  a=j*math.pi/7;tube('Wicker upright',[(x+.07*math.cos(a),y+.055*math.sin(a),z-.055),(x+.10*math.cos(a),y+.08*math.sin(a),z+.05)],.005,gold)
 tube('Basket handle',[(x-.09,y,z+.04),(x-.08,y,z+.16),(x+.08,y,z+.16),(x+.09,y,z+.04)],.012,brown)
 for j in range(9):
  a=j*2.4;xx=x+math.cos(a)*.06;yy=y+math.sin(a)*.04
  for s in [-1,1]:sphere('Peanut',(xx+s*.013,yy,z+.065+(j%2)*.02),(.021,.016,.016),gold)

def gown(color,bottom=.25,top=.105,z0=.08,z1=.57):
 lathe('Soft pleated skirt',[(z0,bottom,bottom*.72),(z0+.035,bottom*1.02,bottom*.75),(z0+.13,bottom*.85,bottom*.64),(z1-.13,top*1.4,top),(z1,top,top*.75)],color,.06)
 sphere('Fitted bodice',(0,0,z1+.04),(.115,.085,.12),color)

names=[('jolly','Jolly'),('gramma-nutt','GrammaNutt'),('princess-lolly','PrincessLolly'),('queen-frostine','QueenFrostine'),('king-kandy','KingKandy'),('gloppy','Gloppy')]
for slug,folder in names:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 white=material('Vanilla icing',(1,.95,.82));black=material('Deep pupil',(.025,.009,.018));skin=material('Warm face',(1,.65,.39));blush=material('Rosy cheeks',(.98,.30,.24));pink=material('Rose candy',(.98,.19,.46));purple=material('Plum purple',(.48,.07,.57));blue=material('Bonnet blue',(.025,.43,.85));ice=material('Icy blue',(.59,.85,1));gold=material('Golden candy',(1,.64,.06));brown=material('Wicker brown',(.36,.14,.035));red=material('Cherry red',(.87,.025,.045));green=material('Emerald candy',(.05,.60,.14))
 if slug=='jolly':
  lathe('Continuous gumdrop silhouette',[(.08,.17,.13),(.11,.24,.18),(.18,.30,.23),(.30,.34,.25),(.42,.33,.25),(.53,.29,.23),(.65,.24,.19),(.75,.19,.16),(.82,.12,.10),(.86,.05,.05)],pink)
  for s in [-1,1]:sphere('Gumdrop foot',(s*.16,-.10,.07),(.12,.16,.07),pink)
  for i in range(28):
   a=i*2.39996;z=.20+(i%5)*.105;r=.34-max(0,z-.35)*.36
   sphere('Purple sugar spot',(math.cos(a)*r,math.sin(a)*r*.76,z),(.042,.018,.044),purple).rotation_euler.z=a+math.pi/2
  for s in [-1,1]:tube('Happy arm',[(s*.24,0,.52),(s*.34,-.02,.57),(s*.39,-.04,.66 if s<0 else .43)],.045,pink);hand((s*.40,-.04,.68 if s<0 else .45),pink)
  for i in range(18):a=i*math.pi/9;sphere('Swirled icing ruff',(.25*math.cos(a),.19*math.sin(a),.59+.025*math.cos(a*3)),(.049,.05,.042),white)
  ring('Icing cap',(0,0,.84),.065,.06,.021,white);sphere('Pink gumdrop cap',(0,0,.90),(.052,.052,.062),pink)
  eyes(.70,-.184,.064,pink,white,black,gold,.09);sphere('Big pink nose',(0,-.225,.69),(.055,.036,.045),pink)
 elif slug in ['gramma-nutt','princess-lolly','queen-frostine']:
  grandma=slug=='gramma-nutt';lolly=slug=='princess-lolly';frost=not grandma and not lolly
  color=blue if grandma else gold if lolly else ice
  if lolly:
   # The illustration has a short bell skirt with visible bare legs and golden shoes.
   gown(color,.265,.095,.26,.56)
   for side in [-1,1]:tube('Bare leg',[(side*.07,0,.30),(side*.08,-.01,.18),(side*.10,-.035,.07)],.027,skin)
  else:gown(color,.265 if grandma else .29,.10,.08,.56)
  for s in [-1,1]:
   sphere('Sleeve puff',(s*.14,0,.61),(.07,.07,.085),color)
   tube('Bent sleeve',[(s*.15,0,.6),(s*.23,-.015,.54),(s*.29,-.06,.59)],.037,color if grandma else skin)
   hand((s*.30,-.07,.60),skin)
   sphere('Dress shoe',(s*.10,-.05,.045),(.055,.095,.04),brown if grandma else gold if lolly else ice)
  woman_face(.78 if frost else .76)
  if grandma:
   orange=material('Copper curls',(.94,.27,.02))
   sphere('Blue bonnet dome',(0,.055,.81),(.20,.085,.21),blue)
   for i in range(17):a=i*math.pi/16;sphere('White bonnet ruffle',(.20*math.cos(a),-.018,.74+.22*math.sin(a)),(.035,.028,.035),white)
   for i in range(12):a=i*math.pi/6;curl('Copper ringlet',(.13*math.cos(a),-.087,.77+.13*math.sin(a)),.038,orange)
   # Curved apron follows the front of the skirt rather than a floating oval.
   verts=[];faces=[]
   for row in range(12):
    t=row/11;z=.15+t*.38;r=.255-t*.15
    for j in range(25):a=-math.pi/2+(j/24-.5)*1.7;verts.append((r*math.cos(a),r*.76*math.sin(a)-.008,z))
   for row in range(11):
    for j in range(24):k=row*25+j;faces.append((k,k+1,k+26,k+25))
   apron=mesh('Flowing white apron',verts,faces,white);apron.modifiers.new('Apron thickness','SOLIDIFY').thickness=.005
   ring('White skirt hem',(0,0,.10),.268,.20,.018,white)
   for s in [-1,1]:sphere('Blue neck bow',(s*.032,-.10,.64),(.038,.015,.025),blue)
   basket((.29,-.10,.38))
  elif lolly:
   for i in range(16):a=i*math.pi/8;curl('Purple candy curl',(.145*math.cos(a),-.045,.79+.13*math.sin(a)),.045,purple)
   for i in range(26):
    a=i*2.39996;z=.30+(i%4)*.06;r=.27-(z-.26)*.55;sphere('Colorful dress jewel',(math.cos(a)*r,math.sin(a)*r*.76,z),(.025,.024,.025),[red,blue,green,purple][i%4])
   for i in range(18):a=i*math.pi/9;sphere('Golden scalloped hem',(.25*math.cos(a),.19*math.sin(a),.28),(.05,.04,.035),gold)
   ring('Golden crown band',(0,.01,.91),.12,.09,.018,gold)
   for x in [-.10,0,.10]:tube('Crown candy stalk',[(x,.01,.91),(x*1.2,.01,1.02)],.008,gold);sphere('Crown lollipop',(x*1.2,.01,1.04),(.04,.015,.045),[red,gold,blue][round(x*10)+1])
   for j in range(4):
    x=-.30+(j-1.5)*.038;z=.82+(j%2)*.06;tube('Bouquet stick',[(-.30,-.07,.59),(x,-.07,z)],.006,white);sphere('Bouquet lollipop',(x,-.07,z),(.043,.012,.043),[red,blue,green,gold][j])
  else:
   # Long blue-white locks with a heart-shaped face opening.
   for i in range(19):
    a=i*math.pi/9;x=.16*math.cos(a);y=.07+.095*math.sin(a)
    tube('Flowing ice hair',[(x*.65,y,.94),(x,y,.84),(x*1.1,y+.01,.65),(x*.95,y,.43),(x*1.2,y-.01,.36)],.024,white if i%4==0 else ice)
   for s in [-1,1]:tube('Face framing hair',[(s*.02,-.05,.94),(s*.11,-.08,.91),(s*.16,-.08,.72),(s*.17,-.07,.53)],.022,ice)
   ring('Snow gown hem',(0,0,.10),.29,.21,.021,white)
   for i in range(18):
    a=i*2.4;z=.18+(i%4)*.08;r=.29-(z-.08)*.37;sphere('Snowflake pearl',(r*math.cos(a),r*.75*math.sin(a),z),(.009,.009,.009),white)
   ring('Ice crown band',(0,0,.96),.12,.085,.013,ice)
   for i in range(5):
    a=i*math.pi/2.5;x=.12*math.cos(a);y=.085*math.sin(a)
    gem=mesh('Faceted crystal crown point',[(x-.025,y-.012,.97),(x+.025,y-.012,.97),(x,y-.020,1.07),(x,y+.018,1.01),(x,y-.032,1.01)],[(0,1,4),(0,4,2),(1,2,4),(0,2,3),(1,3,2),(0,3,1)],ice)
    sphere('Emerald crown jewel',(x,y,.975),(.025,.018,.018),green)
   tube('Snowflake wand',[(-.30,-.07,.60),(-.32,-.07,1.06)],.008,blue)
   for i in range(6):a=i*math.pi/3;end=(-.32+math.cos(a)*.095,-.07,1.06+math.sin(a)*.095);tube('Ice wand ray',[(-.32,-.07,1.06),end],.006,ice);sphere('Ice gem',end,(.014,.014,.014),blue)
 elif slug=='king-kandy':
  # Layered golden candy tunic, striped sleeves/stockings, broad red velvet cape.
  lathe('Golden tunic',[(.21,.22,.16),(.35,.22,.16),(.54,.17,.14),(.67,.15,.12)],gold,.08)
  for s in [-1,1]:
   striped('Royal striped stocking',[(s*.10,0,.27),(s*.11,0,.12)],.043,red,white)
   sphere('Purple boot',(s*.11,-.06,.07),(.065,.10,.055),purple);tube('Curled purple boot toe',[(s*.11,-.10,.05),(s*.14,-.18,.05),(s*.16,-.19,.10)],.035,purple)
   striped('Royal sleeve',[(s*.15,0,.62),(s*.25,-.01,.56),(s*.31,-.05,.65)],.055,red,white);hand((s*.32,-.05,.67),white)
  verts=[];faces=[]
  for row in range(9):
   t=row/8;z=.08+t*.60;r=.35-t*.19
   for j in range(33):a=-.3+(j/32)*(math.pi+.6);verts.append((r*math.cos(a),r*.62*math.sin(a)+.02,z))
  for row in range(8):
   for j in range(32):k=row*33+j;faces.append((k,k+1,k+34,k+33))
  cape=mesh('Sweeping royal red cape',verts,faces,red);cape.modifiers.new('Velvet thickness','SOLIDIFY').thickness=.012
  for side in [-1,1]:
   pts=[(side*(.35-t*.19),-.07,.08+t*.60) for t in [i/12 for i in range(13)]];tube('Royal ermine trim',pts,.025,white)
   for j in range(6):t=j/5;sphere('Trim gumdrop',(side*(.35-t*.19),-.09,.08+t*.60),(.017,.013,.018),[red,gold,green][j%3])
  sphere('Round royal face',(0,-.01,.80),(.13,.10,.15),skin);eyes(.80,-.116,.048,skin,white,black,purple,.06)
  for i in range(12):a=i*math.pi/6;curl('Royal purple ringlet',(.15*math.cos(a),-.065,.80+.11*math.sin(a)),.045,purple)
  for i in range(9):a=math.pi+i*math.pi/8;curl('Curly purple beard',(.10*math.cos(a),-.13,.70+.09*math.sin(a)),.037,purple)
  for s in [-1,1]:tube('Royal curled moustache',[(0,-.15,.76),(s*.07,-.15,.74),(s*.10,-.14,.79)],.018,purple)
  ring('Icing crown band',(0,0,.94),.16,.12,.030,white)
  lathe('Golden candy crown',[(.94,.145,.11),(1.04,.13,.10),(1.08,.045,.045)],gold)
  for i in range(7):a=i*math.pi*2/7;sphere('Crown gumdrop',(.13*math.cos(a),.10*math.sin(a),1.045),(.032,.032,.045),[red,green,gold][i%3])
  sphere('Crown cherry',(0,0,1.12),(.04,.04,.05),red)
  for i in range(12):a=i*2.4;sphere('Tunic candy jewel',(.21*math.cos(a),.165*math.sin(a),.29+(i%3)*.09),(.025,.02,.028),[red,purple,green][i%3])
  striped('Candy scepter shaft',[(-.32,-.05,.12),(-.32,-.05,.94)],.014,red,white);sphere('Scepter icing',(-.32,-.05,.99),(.08,.07,.07),white);sphere('Scepter cherry',(-.32,-.05,1.07),(.025,.025,.03),red)
 elif slug=='gloppy':
  chocolate=material('Glossy chocolate',(.38,.11,.025));caramel=material('Caramel raised folds',(.56,.20,.045))
  lathe('Flowing molasses body',[(.02,.37,.27),(.045,.36,.265),(.09,.31,.24),(.18,.27,.20),(.30,.245,.185),(.45,.225,.18),(.57,.20,.16),(.66,.17,.14),(.72,.125,.11),(.755,.075,.065),(.77,.025,.025)],chocolate,.07)
  for i in range(24):
   a=i*2.4;z=.12+(i%5)*.105;r=.32-z*.23;x=math.cos(a)*r;y=math.sin(a)*r*.76
   tube('Flowing chocolate ridge',[(x*.8,y*.8,z+.10),(x,y,z+.04),(x*1.1,y*1.1,z-.035)],.022,caramel if i%4==0 else chocolate)
   sphere('Hanging molasses drop',(x*1.1,y*1.1,z-.045),(.025,.023,.039),chocolate)
  for side in [-1,1]:
   end=(side*.34,-.02,.61 if side>0 else .35);tube('Stretchy waving chocolate arm',[(side*.17,0,.43),(side*.27,-.01,.43),end],.045,chocolate);hand(end,chocolate)
  eyes(.60,-.16,.054,chocolate,white,black,brown,.09)
  for o in list(bpy.context.scene.objects):
   if o.name.startswith('Warm cheek'):o.data.materials.clear();o.data.materials.append(caramel)
   if o.name.startswith('Open smile') or o.name.startswith('Smiling teeth') or o.name.startswith('Tongue'):bpy.data.objects.remove(o,do_unlink=True)
  sphere('Huge happy mouth',(0,-.181,.45),(.098,.021,.09),black);sphere('Big pink tongue',(0,-.208,.424),(.064,.01,.043),pink)
  tube('Curved chocolate lower lip',[(-.09,-.18,.47),(-.04,-.20,.36),(.05,-.20,.37),(.09,-.18,.48)],.018,caramel)
 export(slug,folder)
