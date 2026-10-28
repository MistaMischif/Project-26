using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // Shared toys are independent of cards, turn order, scoring and player-piece positions.
    public sealed class CandyTreatBucket:MonoBehaviour
    {
        sealed class Treat{public Transform shape;public int theme,hand=int.MinValue;public Vector3 velocity,last;public float sample,seen,age;public bool loose;}
        readonly List<Treat> treats=new List<Treat>();readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
        CandyTable table;Vector3 center,bowlOffset;float width,bowlSeen;Transform bowl;int bowlHand=int.MinValue;readonly List<Renderer> rim=new List<Renderer>();
        public bool BowlHeld=>bowlHand!=int.MinValue;
        void SelectBowl(bool selected){foreach(var r in rim)r.sharedMaterial=Mat(selected?Color.cyan:new Color(1,.72f,.18f));}
        void MoveBowl(Vector3 target){var delta=target-center;center=target;bowl.localPosition=center;foreach(var t in treats)if(!t.loose&&t.hand==int.MinValue)t.shape.localPosition+=delta;}
        public int Count=>treats.Count;
        public int CountFor(int theme)=>treats.Count(t=>t.theme==theme);
        public Vector3 BucketWorldPosition=>transform.TransformPoint(center);
        public Vector3 TreatWorldPosition(int i)=>treats[i].shape.position;
        public bool IsHolding(int hand)=>bowlHand==hand||treats.Any(t=>t.hand==hand);
        Material Mat(Color color){if(!materials.TryGetValue(color,out var m)){m=new Material(Resources.Load<Shader>("CandyPiecePlastic"));m.color=color;materials[color]=m;}return m;}
        Transform Part(Transform parent,string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color)
        {var g=GameObject.CreatePrimitive(type);g.name=name;Destroy(g.GetComponent<Collider>());g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=Mat(color);return g.transform;}
        public void Initialize(CandyTable owner,float boardWidth)
        {
            table=owner;width=boardWidth;center=new Vector3(width*.5f+.078f,.003f,width*.18f);
            Color purple=new Color(.5f,.12f,.7f),gold=new Color(1,.72f,.18f);
            bowl=new GameObject("Movable bowl").transform;bowl.SetParent(transform,false);bowl.localPosition=center;bowl.localScale=new Vector3(1,.5f,1);
            // Open bucket: cylindrical staves, solid bottom and rounded lip, no lid.
            Part(bowl,"Bucket bottom",PrimitiveType.Cylinder,Vector3.up*.003f,new Vector3(.091f,.003f,.091f),purple);
            for(int i=0;i<36;i++)
            {
                float a=i*Mathf.PI/18;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                Part(bowl,"Bucket curved wall",PrimitiveType.Capsule,radial*.043f+Vector3.up*.027f,new Vector3(.009f,.027f,.009f),purple);
                rim.Add(Part(bowl,"Golden bucket rim",PrimitiveType.Sphere,radial*.043f+Vector3.up*.054f,Vector3.one*.009f,gold).GetComponent<Renderer>());
            }
            for(int side=-1;side<=1;side+=2)Part(bowl,"Bucket handle lug",PrimitiveType.Sphere,new Vector3(side*.048f,.035f,0),new Vector3(.012f,.018f,.012f),gold);
        }
        public void Fill(int theme)
        {
            if(theme<0||theme>8||CountFor(theme)>=5)return;
            for(int n=0;n<1;n++)
            {
                var root=new GameObject("Throwable "+CandyCharacterModel.Names[theme]+" treat").transform;root.SetParent(transform,false);root.localScale=Vector3.one*.039f;
                Color c=new[]{new Color(.55f,.08f,.75f),Color.red,new Color(1,.23f,.6f),new Color(.85f,.52f,.18f),new Color(1,.3f,.7f),new Color(.4f,.85f,1),Color.yellow,new Color(1,.35f,.05f),new Color(.4f,.15f,.04f)}[theme];
                var cream=new Color(1,.96f,.84f);
                if(theme==3){for(int s=-1;s<=1;s+=2)Part(root,"Peanut half",PrimitiveType.Sphere,new Vector3(0,s*.23f,0),new Vector3(.65f,.8f,.65f),c);}
                else if(theme==4){Part(root,"Lollipop",PrimitiveType.Sphere,Vector3.up*.2f,new Vector3(1,1,.3f),c);Part(root,"Lollipop stick",PrimitiveType.Cube,Vector3.down*.5f,new Vector3(.12f,.8f,.12f),cream);}
                else if(theme==5){for(int s=0;s<3;s++){var ray=Part(root,"Ice crystal",PrimitiveType.Cube,Vector3.zero,new Vector3(.15f,1,.15f),c);ray.localRotation=Quaternion.Euler(0,0,s*60);}}
                else if(theme==1){Part(root,"Peppermint",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1,.15f,1),cream);for(int s=0;s<3;s++){var stripe=Part(root,"Mint stripe",PrimitiveType.Cube,Vector3.up*.16f,new Vector3(.16f,.06f,.9f),c);stripe.localRotation=Quaternion.Euler(0,s*60,0);}}
                else {Part(root,"Sweet",PrimitiveType.Sphere,Vector3.zero,Vector3.one,c);if(theme==0)Part(root,"Berry stem",PrimitiveType.Capsule,Vector3.up*.55f,new Vector3(.1f,.12f,.1f),new Color(.15f,.3f,.03f));if(theme==7)for(int s=-1;s<=1;s+=2)Part(root,"Twist wrapper",PrimitiveType.Cube,Vector3.right*s*.6f,new Vector3(.4f,.5f,.1f),new Color(.16f,.03f,.2f));}
                var treat=new Treat{shape=root,theme=theme};treats.Add(treat);Return(treat);
            }
        }
        void Return(Treat t)
        {
            int i=treats.IndexOf(t),layer=i/4,slot=i%4;float a=slot*Mathf.PI*2/4;
            t.hand=int.MinValue;t.loose=false;t.velocity=Vector3.zero;t.age=0;
            t.shape.localPosition=center+new Vector3(Mathf.Cos(a)*.029f,.025f+layer*.039f,Mathf.Sin(a)*.029f);
            t.shape.localRotation=Quaternion.Euler(i*23,i*71,i*17);
        }
        public bool BeginGrab(int hand,Vector3 world)
        {
            if(!gameObject.activeInHierarchy||table.Paused||IsHolding(hand))return false;
            var local=transform.InverseTransformPoint(world)-center;float radius=new Vector2(local.x,local.z).magnitude;
            if(!BowlHeld&&radius>.040f&&radius<.067f&&local.y>-.01f&&local.y<.045f){bowlHand=hand;bowlOffset=-local;bowlSeen=Time.unscaledTime;SelectBowl(true);return true;}
            Treat best=null;float distance=.045f;
            foreach(var t in treats){if(t.hand!=int.MinValue)continue;float d=Vector3.Distance(world,t.shape.position);if(d<distance){distance=d;best=t;}}
            if(best==null)return false;best.hand=hand;best.loose=false;best.velocity=Vector3.zero;best.last=transform.InverseTransformPoint(world);best.sample=best.seen=Time.unscaledTime;best.age=0;return true;
        }
        public bool UpdateGrab(int hand,Vector3 world,bool released)
        {
            if(bowlHand==hand){if(released||table.Paused){Release(hand);return true;}var target=transform.InverseTransformPoint(world)+bowlOffset;target=Vector3.ClampMagnitude(target,2);target.y=Mathf.Max(.003f,target.y);MoveBowl(target);bowlSeen=Time.unscaledTime;return true;}
            var t=treats.FirstOrDefault(x=>x.hand==hand);if(t==null)return false;
            if(released||table.Paused){Release(hand,table.Paused);return true;}
            var p=transform.InverseTransformPoint(world);float dt=Time.unscaledTime-t.sample;
            if(dt>.0001f)t.velocity=Vector3.Lerp(t.velocity,Vector3.ClampMagnitude((p-t.last)/dt,2.5f),.6f);
            t.last=p;t.sample=t.seen=Time.unscaledTime;t.shape.localPosition=p;return true;
        }
        public void Release(int hand,bool cancelled=false)
        {
            if(bowlHand==hand){bowlHand=int.MinValue;SelectBowl(false);MoveBowl(new Vector3(center.x,.003f,center.z));return;}
            var t=treats.FirstOrDefault(x=>x.hand==hand);if(t==null)return;
            if(cancelled){Return(t);return;}t.hand=int.MinValue;t.loose=true;t.age=0;
        }
        void Update()
        {
            if(BowlHeld&&(table.Paused||Time.unscaledTime-bowlSeen>.3f))Release(bowlHand);
            if(table.Paused){foreach(var t in treats)if(t.hand!=int.MinValue)Return(t);return;}
            float dt=Mathf.Min(Time.deltaTime,.035f);
            foreach(var t in treats)
            {
                if(t.hand!=int.MinValue){if(Time.unscaledTime-t.seen>.3f)Return(t);continue;}
                if(!t.loose)continue;t.age+=dt;t.velocity+=Vector3.down*1.7f*dt;t.shape.localPosition+=t.velocity*dt;
                var p=t.shape.localPosition;
                if(Mathf.Abs(p.x)<width*.5f&&Mathf.Abs(p.z)<width*.3566f&&p.y<.023f)
                {p.y=.023f;t.shape.localPosition=p;t.velocity=new Vector3(t.velocity.x*.83f,Mathf.Abs(t.velocity.y)*.35f,t.velocity.z*.83f);}
                t.shape.Rotate(new Vector3(120,180,70)*dt*Mathf.Clamp01(t.velocity.magnitude));
                if(t.age>8||p.y<-.6f||p.magnitude>2)Return(t);
            }
        }
        public void ResetTreats(){if(BowlHeld)Release(bowlHand);foreach(var t in treats){t.shape.gameObject.SetActive(false);Destroy(t.shape.gameObject);}treats.Clear();}
        void OnDestroy(){ResetTreats();foreach(var m in materials.Values)Destroy(m);}
    }
}
