using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 // Board coordinates: X=file, Z=rank, Y=surface normal; metres in Unity world.
 // Image coordinates follow MRUK's bottom-left viewport convention.
 public static class ChessSurfaceSolver {
  public sealed class Corners {public ChessCheckerFit.Frame frame;public readonly Dictionary<int,Vector2> image=new Dictionary<int,Vector2>();}
  public struct Estimate {public bool valid;public Vector3 center;public Quaternion rotation;public float size,rmsMeters,rmsPixels;public int corners;public string source;}
  public static Vector2 BoardPoint(int id)=>new Vector2((id%7-3)/8f,(id/7-3)/8f);
  static float Gray(ChessCheckerFit.Frame f,float x,float y){int ix=(int)x,iy=(int)y;if(ix<1||iy<1||ix>=f.width-2||iy>=f.height-2)return -1;float Value(int a,int b){var c=f.pixels[b*f.width+a];return .299f*c.r+.587f*c.g+.114f*c.b;}return Mathf.Lerp(Mathf.Lerp(Value(ix,iy),Value(ix+1,iy),x-ix),Mathf.Lerp(Value(ix,iy+1),Value(ix+1,iy+1),x-ix),y-iy);}
  static Vector2 Pixel(ChessCheckerFit.Frame f,Vector2 uv)=>new Vector2(uv.x*(f.width-1),uv.y*(f.height-1));
  public static Corners Detect(ChessCheckerFit.Frame f,ChessCheckerFit.Fit seed){
   var result=new Corners{frame=f};if(!seed.valid)return result;var rotation=Quaternion.Euler(0,seed.yaw,0);
   for(int id=0;id<49;id++){
    var q=BoardPoint(id);var world=seed.center+rotation*new Vector3(q.x,0,q.y)*seed.size;var initial=Pixel(f,f.Project(world));
    var dx=Pixel(f,f.Project(world+rotation*Vector3.right*seed.size/8))-initial;var dy=Pixel(f,f.Project(world+rotation*Vector3.forward*seed.size/8))-initial;
    float pitch=Mathf.Min(dx.magnitude,dy.magnitude);if(pitch<8)continue;int radius=Mathf.Clamp(Mathf.RoundToInt(pitch*.22f),3,14);var p=initial;bool usable=true;
    // Iterative gradient intersection, with a bounded search around the coarse lattice.
    for(int iteration=0;iteration<12;iteration++){
     double xx=0,xy=0,yy=0,bx=0,by=0;
     for(int y=-radius;y<=radius;y++)for(int x=-radius;x<=radius;x++){
      var at=p+new Vector2(x,y);float l=Gray(f,at.x-1,at.y),r=Gray(f,at.x+1,at.y),d=Gray(f,at.x,at.y-1),u=Gray(f,at.x,at.y+1);if(Mathf.Min(l,r,d,u)<0)continue;
      double gx=(r-l)*.5,gy=(u-d)*.5,w=Math.Exp(-(x*x+y*y)/(double)(radius*radius));double aa=gx*gx*w,bb=gx*gy*w,cc=gy*gy*w;xx+=aa;xy+=bb;yy+=cc;bx+=aa*at.x+bb*at.y;by+=bb*at.x+cc*at.y;
     }
     double det=xx*yy-xy*xy;if(det<1e-8||det/(xx+yy)/(xx+yy)<.025){usable=false;break;}
     var next=new Vector2((float)((yy*bx-xy*by)/det),(float)((xx*by-xy*bx)/det));if(Vector2.Distance(next,initial)>pitch*.32f){usable=false;break;}float change=Vector2.Distance(p,next);p=next;if(change<.03f)break;
    }
    if(!usable)continue;var a1=p+(dx+dy)*.20f;var b1=p+(dx-dy)*.20f;var c1=p+(-dx+dy)*.20f;var d1=p-(dx+dy)*.20f;
    float a=Gray(f,a1.x,a1.y),b=Gray(f,b1.x,b1.y),c=Gray(f,c1.x,c1.y),d2=Gray(f,d1.x,d1.y);
    if(Mathf.Min(a,b,c,d2)<0||Mathf.Abs((a+d2-b-c)*.5f)-Mathf.Abs(a-d2)-Mathf.Abs(b-c)<30)continue;
    result.image[id]=new Vector2(p.x/(f.width-1),p.y/(f.height-1));
   }
   return result;
  }
  public static bool Coverage(IEnumerable<int> keys){var ids=keys.ToArray();if(ids.Length<16)return false;if(ids.Max(i=>i%7)-ids.Min(i=>i%7)<4||ids.Max(i=>i/7)-ids.Min(i=>i/7)<4)return false;for(int z=0;z<2;z++)for(int x=0;x<2;x++)if(ids.Count(i=>(i%7<3?0:1)==x&&(i/7<3?0:1)==z)<3)return false;return true;}
  public static bool Triangulate(Ray a,Ray b,out Vector3 point){
   point=default;float dot=Vector3.Dot(a.direction,b.direction),den=1-dot*dot;if(den<.0012f)return false;var w=a.origin-b.origin;float da=Vector3.Dot(a.direction,w),db=Vector3.Dot(b.direction,w);float ta=(dot*db-da)/den,tb=(db-dot*da)/den;if(ta<.15f||tb<.15f||ta>2||tb>2)return false;var pa=a.GetPoint(ta);var pb=b.GetPoint(tb);if(Vector3.Distance(pa,pb)>.008f)return false;point=(pa+pb)*.5f;return true;
  }
  public static Estimate FromViews(Corners first,Corners second){
   if(Vector3.Distance(first.frame.camera,second.frame.camera)<.045f)return default;
   var points=new Dictionary<int,Vector3>();foreach(var pair in first.image)if(second.image.TryGetValue(pair.Key,out var uv)&&Triangulate(first.frame.Ray(pair.Value),second.frame.Ray(uv),out var p))points[pair.Key]=p;
   var fit=Fit(points,first);if(fit.valid){fit.rmsPixels=Mathf.Max(fit.rmsPixels,Reprojection(fit,points.Keys,second));fit.valid=fit.rmsPixels<=2.5f;fit.source="tracked viewpoints";}return fit;
  }
  public static Estimate Fit(Dictionary<int,Vector3> observed,Corners image){
   var points=new Dictionary<int,Vector3>(observed);Estimate fit=default;
   for(int pass=0;pass<3;pass++){
    if(!Coverage(points.Keys))return default;Vector2 mean=Vector2.zero;Vector3 center=Vector3.zero;foreach(var kv in points){mean+=BoardPoint(kv.Key);center+=kv.Value;}mean/=points.Count;center/=points.Count;
    float xx=0,xz=0,zz=0;Vector3 bx=Vector3.zero,bz=Vector3.zero;foreach(var kv in points){var q=BoardPoint(kv.Key)-mean;var p=kv.Value-center;xx+=q.x*q.x;xz+=q.x*q.y;zz+=q.y*q.y;bx+=q.x*p;bz+=q.y*p;}float det=xx*zz-xz*xz;if(det<1e-6f)return default;
    var right=(zz*bx-xz*bz)/det;var forward=(xx*bz-xz*bx)/det;float size=(right.magnitude+forward.magnitude)*.5f;
    if(size<.2f||size>1.2f||Mathf.Abs(right.magnitude-forward.magnitude)>size*.06f||Mathf.Abs(Vector3.Dot(right.normalized,forward.normalized))>.08f)return default;
    var normal=Vector3.Cross(forward,right).normalized;if(Vector3.Dot(normal,Vector3.up)<.9f)return default;
    var rotation=Quaternion.LookRotation(Vector3.Cross(right.normalized,normal),normal);var origin=center-rotation*new Vector3(mean.x,0,mean.y)*size;
    fit=new Estimate{center=origin,rotation=rotation,size=size,corners=points.Count,valid=true};
    var bad=points.Where(kv=>Vector3.Distance(kv.Value,Position(fit,kv.Key))>.012f).Select(kv=>kv.Key).ToArray();if(bad.Length>0){foreach(int key in bad)points.Remove(key);continue;}
    fit.rmsMeters=Mathf.Sqrt(points.Average(kv=>(kv.Value-Position(fit,kv.Key)).sqrMagnitude));fit.rmsPixels=Reprojection(fit,points.Keys,image);fit.valid=fit.rmsMeters<=.006f&&fit.rmsPixels<=2.5f;return fit;
   }
   return default;
  }
  public static Vector3 Position(Estimate fit,int id){var q=BoardPoint(id);return fit.center+fit.rotation*new Vector3(q.x,0,q.y)*fit.size;}
  public static float Reprojection(Estimate fit,IEnumerable<int> ids,Corners image){return Mathf.Sqrt(ids.Average(id=>(Pixel(image.frame,image.frame.Project(Position(fit,id)))-Pixel(image.frame,image.image[id])).sqrMagnitude));}
  // Verify visible boundaries on every internal rank/file, not only a plausible coarse lattice.
  public static bool EdgesAgree(Estimate fit,Corners corners){
   if(!fit.valid||corners?.frame?.pixels==null||!Coverage(corners.image.Keys))return false;
   for(int axis=0;axis<2;axis++)for(int line=1;line<8;line++){
    int support=0;
    for(int segment=0;segment<8;segment++){
     var q=axis==0?new Vector3(line/8f-.5f,0,(segment+.5f)/8-.5f):new Vector3((segment+.5f)/8-.5f,0,line/8f-.5f);
     var world=fit.center+fit.rotation*q*fit.size;var f=corners.frame;var pixel=Pixel(f,f.Project(world));
     var direction=Pixel(f,f.Project(world+fit.rotation*(axis==0?Vector3.right:Vector3.forward)*fit.size/8))-pixel;
     float pitch=direction.magnitude;if(pitch<8)continue;direction.Normalize();
     float strongest=0,offset=99;
     for(float d=-Mathf.Min(6,pitch*.2f);d<=Mathf.Min(6,pitch*.2f);d+=.25f){var a=pixel+direction*(d-1);var b=pixel+direction*(d+1);float av=Gray(f,a.x,a.y),bv=Gray(f,b.x,b.y);if(av<0||bv<0)continue;float contrast=Mathf.Abs(av-bv);if(contrast>strongest+.01f||(Mathf.Abs(contrast-strongest)<.01f&&Mathf.Abs(d)<Mathf.Abs(offset))){strongest=contrast;offset=d;}}
     if(strongest>=35&&Mathf.Abs(offset)<=1.25f)support++;
    }
    if(support<3)return false;
   }
   // The outermost observed intersections constrain extrapolation to the playing boundary.
   return corners.image.Keys.Any(i=>i%7==0)&&corners.image.Keys.Any(i=>i%7==6)&&corners.image.Keys.Any(i=>i/7==0)&&corners.image.Keys.Any(i=>i/7==6);
  }
  // Require the 8x8 lattice to terminate at all four playing edges, not at a decorative frame.
  public static bool PlayableBoundary(Estimate fit,Corners corners)=>PlayableEdgeMask(fit,corners)==15;
  public static int PlayableEdgeMask(Estimate fit,Corners corners){
   if(!EdgesAgree(fit,corners))return 0;int mask=0;
   var f=corners.frame;
   for(int axis=0;axis<2;axis++)for(int side=-1;side<=1;side+=2){
    int supported=0;bool near=false,far=false,visible=true;float insideAlternation=0,outsideAlternation=0;float prevIn=0,prevOut=0;
    for(int segment=0;segment<8;segment++){
     Vector3 Q(float inset)=>axis==0?new Vector3(side*(.5f-inset/8),0,(segment+.5f)/8-.5f):new Vector3((segment+.5f)/8-.5f,0,side*(.5f-inset/8));
     float Sample(float inset)=>f.Gray(fit.center+fit.rotation*Q(inset)*fit.size);
     float inside=Sample(.22f),outside=Sample(-.22f);if(inside<0||outside<0){visible=false;break;}
     if(segment>0){insideAlternation+=Mathf.Abs(inside-prevIn);outsideAlternation+=Mathf.Abs(outside-prevOut);}prevIn=inside;prevOut=outside;
     var pixel=Pixel(f,f.Project(fit.center+fit.rotation*Q(0)*fit.size));var direction=Pixel(f,f.Project(fit.center+fit.rotation*Q(1)*fit.size))-pixel;float pitch=direction.magnitude;if(pitch<10){visible=false;break;}direction.Normalize();
     float strongest=0,offset=99;
     for(float d=-pitch*.28f;d<=pitch*.28f;d+=.25f){var a=pixel+direction*(d-1);var b=pixel+direction*(d+1);float av=Gray(f,a.x,a.y),bv=Gray(f,b.x,b.y);if(av<0||bv<0)continue;float contrast=Mathf.Abs(av-bv);if(contrast>strongest+.01f||(Mathf.Abs(contrast-strongest)<.01f&&Mathf.Abs(d)<Mathf.Abs(offset))){strongest=contrast;offset=d;}}
     if(strongest>=30&&Mathf.Abs(offset)<=1.25f&&Mathf.Abs(inside-outside)>=30){supported++;if(segment<2)near=true;if(segment>=6)far=true;}
    }
    // A checker pattern continuing outside is a shifted/subset lattice, not the playable border.
    if(visible&&supported>=4&&near&&far&&insideAlternation>=7*35&&outsideAlternation<=insideAlternation*.45f)mask|=1<<(axis*2+(side>0?1:0));
   }
   return mask;
  }
  public static bool Consistent(Estimate a,Estimate b)=>a.valid&&b.valid&&Vector3.Distance(a.center,b.center)<.008f&&Mathf.Abs(a.size-b.size)<.008f&&Quaternion.Angle(a.rotation,b.rotation)<1.2f;
 }
}
