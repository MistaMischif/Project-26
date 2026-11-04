#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using LivingTable.Chess;
namespace LivingTable.Editor {
 public static class SurfaceCalibrationTests {
  static int count;static void Check(bool ok,string name){count++;if(!ok)throw new Exception("SURFACE CALIBRATION: "+name);}
  static ChessCheckerFit.Frame Frame(Vector3 eye,Vector3 center,float size,Quaternion rotation,bool checker=true,bool extended=false,bool occluded=false){
   var f=new ChessCheckerFit.Frame{width=640,height=480,camera=eye,rotation=Quaternion.LookRotation(center-eye),fx=.9f,fy=1.2f,cx=.5f,cy=.5f,pixels=new Color32[640*480]};var plane=new Plane(rotation*Vector3.up,center);
   for(int y=0;y<f.height;y++)for(int x=0;x<f.width;x++){byte value=110;if(plane.Raycast(f.Ray(new Vector2(x/(f.width-1f),y/(f.height-1f))),out float distance)){var p=Quaternion.Inverse(rotation)*(f.Ray(new Vector2(x/(f.width-1f),y/(f.height-1f))).GetPoint(distance)-center);if(checker&&Mathf.Abs(p.x)<size*(extended?.75f:.5f)&&Mathf.Abs(p.z)<size*(extended?.75f:.5f)){int col=Mathf.FloorToInt((p.x/size+.5f)*8),row=Mathf.FloorToInt((p.z/size+.5f)*8);value=(byte)((col+row)%2==0?30:225);if(occluded&&p.x>size*.27f&&p.z>size*.27f)value=110;}}f.pixels[y*f.width+x]=new Color32(value,value,value,255);}return f;
  }
  static ChessSurfaceSolver.Corners Exact(ChessCheckerFit.Frame frame,ChessSurfaceSolver.Estimate truth){var result=new ChessSurfaceSolver.Corners{frame=frame};for(int id=0;id<49;id++)result.image[id]=frame.Project(ChessSurfaceSolver.Position(truth,id));return result;}
  public static void Run(){
   foreach(float size in new[]{.32f,.56f,.9f}){
    var center=new Vector3(.04f,.75f,.03f);var rotation=Quaternion.Euler(0,12,0);var truth=new ChessSurfaceSolver.Estimate{valid=true,center=center,size=size,rotation=rotation};
    var first=Frame(center+new Vector3(.1f,size*1.3f,-size*.9f),center,size,rotation);var second=Frame(center+new Vector3(.22f,size*1.3f,-size*.9f),center,size,rotation);
    var coarse=ChessCheckerFit.Find(first,center,.56f,0,true);Check(coarse.valid&&Mathf.Abs(coarse.size-size)<.025f,"Broad localization finds board width "+size);
    var detected=ChessSurfaceSolver.Detect(first,new ChessCheckerFit.Fit{valid=true,center=center+new Vector3(.003f,0,.002f),size=size,yaw=12});Check(ChessSurfaceSolver.Coverage(detected.image.Keys),"Image corners detected for board "+size+" ("+detected.image.Count+")");
    float pixelError=Mathf.Sqrt(detected.image.Average(kv=>{var d=kv.Value-first.Project(ChessSurfaceSolver.Position(truth,kv.Key));return d.x*d.x*640*640+d.y*d.y*480*480;}));Check(pixelError<1.6f,"Subpixel image fit "+size+": "+pixelError);
    var exact=Exact(first,truth);Check(ChessSurfaceSolver.EdgesAgree(truth,exact),"Every checker line agrees");
    var fullFrame=Frame(center+new Vector3(.03f,size*2f,-size*.4f),center,size,rotation);var perimeter=Exact(fullFrame,truth);
    Check(ChessSurfaceSolver.PlayableBoundary(truth,perimeter),"Playable perimeter verified at size "+size+" mask="+ChessSurfaceSolver.PlayableEdgeMask(truth,perimeter));
    var hidden=Frame(fullFrame.camera,center,size,rotation,true,false,true);var hiddenMask=ChessSurfaceSolver.PlayableEdgeMask(truth,Exact(hidden,truth));Check((hiddenMask&1)!=0&&(hiddenMask&4)!=0&&hiddenMask!=15,"Occluded corner keeps visible perpendicular edges without claiming all observed");
    var extended=Frame(fullFrame.camera,center,size,rotation,true,true);Check(ChessSurfaceSolver.PlayableEdgeMask(truth,Exact(extended,truth))==0,"Continuing checker outside is not a playing boundary");
    var frameFit=truth;frameFit.size+=size/4;Check(!ChessSurfaceSolver.PlayableBoundary(frameFit,perimeter),"Decorative frame perimeter rejected");
    var shifted=truth;shifted.center+=rotation*Vector3.right*(size/8);Check(!ChessSurfaceSolver.PlayableBoundary(shifted,perimeter),"One-square displaced boundary rejected");
    var blurred=Frame(first.camera,center,size,rotation,false);Check(!ChessSurfaceSolver.PlayableBoundary(truth,Exact(blurred,truth)),"No-contrast table cannot snap");
    var expectedCorner=center+rotation*new Vector3(-.5f,0,.5f)*size;var selected=ChessTable.NearestVerifiedCorner(truth,expectedCorner+Vector3.right*.009f,out float snapDistance);Check(Vector3.Distance(selected,expectedCorner)<.0001f&&snapDistance<.01f,"Nearby point maps to playable corner");
var offsetFit=truth;offsetFit.center+=rotation*Vector3.right*size/32;Check(!ChessSurfaceSolver.EdgesAgree(offsetFit,exact),"Offset grid rejected by edges");var points=Enumerable.Range(0,49).ToDictionary(i=>i,i=>ChessSurfaceSolver.Position(truth,i));var fit=ChessSurfaceSolver.Fit(points,exact);Check(fit.valid&&Vector3.Distance(fit.center,center)<.0001f&&Mathf.Abs(fit.size-size)<.0001f,"Depth correspondence metric pose "+size);
    var stereo=ChessSurfaceSolver.FromViews(exact,Exact(second,truth));Check(stereo.valid&&Mathf.Abs(stereo.size-size)<.001f&&Vector3.Distance(stereo.center,center)<.001f,"Tracked-view metric scale without configured square size "+size);
    Check(!ChessSurfaceSolver.FromViews(exact,exact).valid,"Stationary viewpoint cannot invent metric scale");
    var blank=Frame(first.camera,center,size,rotation,false);Check(!ChessSurfaceSolver.Coverage(ChessSurfaceSolver.Detect(blank,new ChessCheckerFit.Fit{valid=true,center=center,size=size,yaw=12}).image.Keys),"Blank surface rejected");
    var sparse=points.Where(kv=>kv.Key%7<2).ToDictionary(kv=>kv.Key,kv=>kv.Value);Check(!ChessSurfaceSolver.Fit(sparse,exact).valid,"One-sided coverage rejected");
    var moved=fit;moved.center+=Vector3.right*.03f;Check(!ChessSurfaceSolver.Consistent(fit,moved),"Moving board rejected during lock");
    var stretched=points.ToDictionary(kv=>kv.Key,kv=>center+new Vector3((kv.Value.x-center.x)*1.4f,kv.Value.y-center.y,kv.Value.z-center.z));Check(!ChessSurfaceSolver.Fit(stretched,exact).valid,"Non-square lattice rejected");
   }
   var c=new Vector3(0,.8f,0);var tilted=new ChessSurfaceSolver.Estimate{valid=true,center=c,size=.56f,rotation=Quaternion.Euler(8,17,5)};var view=Frame(new Vector3(.1f,1.6f,-.6f),c,.56f,tilted.rotation);var image=Exact(view,tilted);var plane=Enumerable.Range(0,49).ToDictionary(i=>i,i=>ChessSurfaceSolver.Position(tilted,i));var pose=ChessSurfaceSolver.Fit(plane,image);Check(pose.valid&&Quaternion.Angle(pose.rotation,tilted.rotation)<.01f,"Surface normal and full rotation recovered");
   Check(!ChessSurfaceSolver.Triangulate(new Ray(Vector3.zero,Vector3.forward),new Ray(Vector3.right*.1f,Vector3.forward),out _),"Parallel rays rejected");
   File.WriteAllText("Builds/Surface-calibration-tests.txt",count+" synthetic surface calibration checks passed; image corner accuracy, metric scale across three board sizes, stereo/depth geometry, tilt and rejection cases. Physical camera accuracy and drift remain unmeasured.\n");Debug.Log("SURFACE_CALIBRATION_TESTS_PASSED "+count);
  }
 }
}
#endif
