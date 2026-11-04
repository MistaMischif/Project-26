using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meta.XR;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
namespace LivingTable.Chess {
 public sealed partial class ChessCameraCalibration:MonoBehaviour {
  PassthroughCameraAccess cameraAccess;ChessTable table;EnvironmentRaycastManager depth;bool ownsDepth;
  public enum AlignmentState {Searching,Calibrating,Aligned,TrackingLost,Recalibrating}
  public AlignmentState State{get;private set;}=AlignmentState.Searching;
  int generation;DateTime lastImage;
  public void Lock(){Cancel();State=AlignmentState.Aligned;}
  public void Initialize(ChessTable owner){table=owner;}
  public void Cancel(){generation++;StopAllCoroutines();if(ownsDepth&&depth!=null)depth.enabled=false;if(cameraAccess!=null)cameraAccess.enabled=false;}
  public void FitBoard(){snapReference=default;Cancel();State=State==AlignmentState.Aligned?AlignmentState.Recalibrating:AlignmentState.Searching;if(EnvironmentRaycastManager.IsSupported){if(depth==null)depth=FindFirstObjectByType<EnvironmentRaycastManager>();if(depth==null){depth=gameObject.AddComponent<EnvironmentRaycastManager>();ownsDepth=true;}if(ownsDepth)depth.enabled=true;}StartCoroutine(Fit());}
  public void SizePieces(){Cancel();StartCoroutine(Measure());}
  IEnumerator Capture(Action<ChessCheckerFit.Frame> done){
   if(TableLifecycle.Blocked){done(null);yield break;}
#if UNITY_ANDROID && !UNITY_EDITOR
   const string permission="horizonos.permission.HEADSET_CAMERA";
   if(!Permission.HasUserAuthorizedPermission(permission)){Permission.RequestUserPermission(permission);float until=Time.unscaledTime+12;while(!Permission.HasUserAuthorizedPermission(permission)&&Time.unscaledTime<until)yield return null;if(!Permission.HasUserAuthorizedPermission(permission)){done(null);yield break;}}
   if(!PassthroughCameraAccess.IsSupported){done(null);yield break;}
#else
   done(null);yield break;
#endif
#pragma warning disable CS0162
   if(cameraAccess==null){var go=new GameObject("Chess calibration camera");go.SetActive(false);go.transform.SetParent(transform,false);cameraAccess=go.AddComponent<PassthroughCameraAccess>();cameraAccess.RequestedResolution=new Vector2Int(1280,960);cameraAccess.MaxFramerate=15;go.SetActive(true);}cameraAccess.enabled=true;
   float deadline=Time.unscaledTime+8;while((!cameraAccess.IsPlaying||!cameraAccess.IsUpdatedThisFrame)&&Time.unscaledTime<deadline)yield return null;
   while(cameraAccess.Timestamp==lastImage&&Time.unscaledTime<deadline)yield return null;
   if(!cameraAccess.IsPlaying||cameraAccess.GetTexture()==null||cameraAccess.Timestamp==lastImage){done(null);yield break;}
   while(!table.HandsClear&&Time.unscaledTime<deadline)yield return null;
   if(!table.HandsClear){done(null);yield break;}
   var pose=cameraAccess.GetCameraPose();var inverse=Quaternion.Inverse(pose.rotation);var a=inverse*cameraAccess.ViewportPointToRay(Vector2.zero,pose).direction;var b=inverse*cameraAccess.ViewportPointToRay(Vector2.one,pose).direction;
   float fx=1/(b.x/b.z-a.x/a.z),fy=1/(b.y/b.z-a.y/a.z);var frame=new ChessCheckerFit.Frame{capturedAt=Time.realtimeSinceStartupAsDouble,imageTimestampTicks=cameraAccess.Timestamp.Ticks,width=320,height=240,camera=pose.position,rotation=pose.rotation,fx=fx,fy=fy,cx=-a.x/a.z*fx,cy=-a.y/a.z*fy};
   var source=cameraAccess.GetTexture();int nativeWidth=source.width,nativeHeight=source.height;lastImage=cameraAccess.Timestamp;int captureGeneration=generation;
   frame.width=640;frame.height=Mathf.RoundToInt(640f*nativeHeight/nativeWidth);bool complete=false;
   // Direct readback: MRUK warns that Graphics.Blit can return the previous image.
   AsyncGPUReadback.Request(source,0,TextureFormat.RGBA32,read=>{if(this!=null&&!TableLifecycle.Blocked&&!read.hasError&&captureGeneration==generation){var pixels=read.GetData<Color32>();frame.pixels=new Color32[frame.width*frame.height];for(int y=0;y<frame.height;y++)for(int x=0;x<frame.width;x++)frame.pixels[y*frame.width+x]=pixels[Mathf.Min(nativeHeight-1,Mathf.RoundToInt(y*(nativeHeight-1f)/(frame.height-1)))*nativeWidth+Mathf.Min(nativeWidth-1,Mathf.RoundToInt(x*(nativeWidth-1f)/(frame.width-1)))];}complete=true;});while(!complete)yield return null;done(frame.pixels==null?null:frame);
#pragma warning restore CS0162
  }
  IEnumerator Fit(){
   float deadline=Time.unscaledTime+45;ChessSurfaceSolver.Corners reference=null;ChessSurfaceSolver.Estimate previous=default,average=default;int consistent=0;bool haveGrid=false;float seedSize=table.Size,seedYaw=table.transform.eulerAngles.y;Vector3 seed=table.transform.position;
   while(Time.unscaledTime<deadline){
    ChessCheckerFit.Frame frame=null;yield return Capture(f=>frame=f);if(frame==null){Fail("Camera unavailable. Retry or choose assisted corners.");yield break;}
    State=AlignmentState.Calibrating;table.CalibrationProgress("Finding physical grid corners. Keep hands clear.");
    if(reference==null&&depth!=null&&TryTableSurface(frame,out var surface))seed=surface;
    var search=Task.Run(()=>{var coarse=ChessCheckerFit.Find(frame,seed,seedSize,seedYaw,!haveGrid);return ChessSurfaceSolver.Detect(frame,coarse);});while(!search.IsCompleted&&Time.unscaledTime<deadline)yield return null;
    if(!search.IsCompleted){Fail("Grid search timed out. Retry or use assisted corners.");yield break;}
    if(search.IsFaulted||!ChessSurfaceSolver.Coverage(search.Result.image.Keys)){consistent=0;State=AlignmentState.Searching;table.CalibrationIssue("Not enough visible grid corners. Look down at the whole board; move your head slightly to uncover corners.");yield return new WaitForSecondsRealtime(.3f);continue;}
    if(!table.HandsClear||table.LastHandAt>frame.capturedAt){consistent=0;reference=null;table.CalibrationIssue("Lift hands away from the grid. Restarting stability check.");continue;}
    haveGrid=true;var corners=search.Result;var estimate=DepthEstimate(corners);
    if(!estimate.valid){
     if(reference==null){reference=corners;table.CalibrationIssue("Depth is unclear. Keep the board still and move your head sideways about a hand's width.");yield return new WaitForSecondsRealtime(.3f);continue;}
     if(Vector3.Distance(reference.frame.camera,frame.camera)<.045f){table.CalibrationIssue("Move your head sideways a little further to measure the real board size.");yield return new WaitForSecondsRealtime(.3f);continue;}
     estimate=ChessSurfaceSolver.FromViews(reference,corners);
    }
    if(!estimate.valid){consistent=0;State=AlignmentState.TrackingLost;table.CalibrationIssue("Grid measurements disagree. Keep the board still and reveal more corners; last alignment held.");yield return new WaitForSecondsRealtime(.3f);continue;}
    consistent=ChessSurfaceSolver.Consistent(previous,estimate)&&ChessSurfaceSolver.Consistent(average,estimate)?consistent+1:1;previous=estimate;
    if(consistent==1)average=estimate;else{float weight=1f/consistent;average.center=Vector3.Lerp(average.center,estimate.center,weight);average.rotation=Quaternion.Slerp(average.rotation,estimate.rotation,weight);average.size=Mathf.Lerp(average.size,estimate.size,weight);average.corners=Mathf.Min(average.corners,estimate.corners);average.rmsMeters=Mathf.Max(average.rmsMeters,estimate.rmsMeters);}
    seed=estimate.center;seedSize=estimate.size;seedYaw=estimate.rotation.eulerAngles.y;
    table.CalibrationProgress("Checking alignment "+consistent+" / 3 — "+estimate.corners+" corners, "+estimate.rmsPixels.ToString("F1")+" px fit.");
    if(consistent>=3){average.rmsPixels=ChessSurfaceSolver.Reprojection(average,corners.image.Keys,corners);if(average.rmsPixels>1.25f||!ChessSurfaceSolver.EdgesAgree(average,corners)){consistent=0;table.CalibrationIssue("Square edges do not match yet. Look straight down, reveal all board edges and keep the board still.");continue;}State=AlignmentState.Aligned;if(cameraAccess!=null)cameraAccess.enabled=false;if(ownsDepth&&depth!=null)depth.enabled=false;snapReference=average;table.PreviewMetricAlignment(average);yield break;}
    yield return new WaitForSecondsRealtime(.25f);
   }
   Fail("Alignment could not be verified. Last grid position held. Retry or choose ASSISTED FIT.");
  }
  ChessSurfaceSolver.Estimate DepthEstimate(ChessSurfaceSolver.Corners corners){
   var points=new Dictionary<int,Vector3>();if(depth==null)return default;
   foreach(var kv in corners.image)if(depth.Raycast(corners.frame.Ray(kv.Value),out var hit,2)&&hit.normal.y>.9f&&Vector3.Distance(hit.point,corners.frame.camera)>.15f)points[kv.Key]=hit.point;
   var estimate=ChessSurfaceSolver.Fit(points,corners);estimate.source="physical depth";return estimate;
  }
  bool TryTableSurface(ChessCheckerFit.Frame frame,out Vector3 surface){
   surface=default;if(depth==null)return false;var hits=new List<Vector3>();
   for(int x=-2;x<=2;x++)for(int y=-2;y<=2;y++){if(depth.Raycast(frame.Ray(new Vector2(.5f+x*.065f,.5f+y*.065f)),out var hit,2)&&Mathf.Abs(hit.normal.y)>.92f&&frame.camera.y-hit.point.y>.15f&&frame.camera.y-hit.point.y<1.2f)hits.Add(hit.point);}
   int best=0;foreach(var point in hits){var group=hits.Where(h=>Mathf.Abs(h.y-point.y)<.012f).ToArray();if(group.Length>best){best=group.Length;surface=group.Aggregate(Vector3.zero,(a,b)=>a+b)/group.Length;}}
   return best>=5;
  }
  void Fail(string reason){State=AlignmentState.TrackingLost;if(ownsDepth&&depth!=null)depth.enabled=false;if(cameraAccess!=null)cameraAccess.enabled=false;table.CalibrationFailed(reason);}
  IEnumerator Measure(){ChessCheckerFit.Frame first=null,second=null;yield return Capture(f=>first=f);if(first==null){Fail("Piece scan unavailable. Keep standard sizes or retry.");yield break;}float deadline=Time.unscaledTime+20;while(Time.unscaledTime<deadline){yield return new WaitForSecondsRealtime(.6f);yield return Capture(f=>second=f);if(second!=null&&Vector3.Distance(first.camera,second.camera)>.04f)break;second=null;}if(second==null){Fail("A second viewing angle is needed. Move your head slightly and retry, or keep standard sizes.");yield break;}
   var position=table.Match?.Position??ChessPosition.Initial();var profile=new float[24];var h=new List<float>[12];var w=new List<float>[12];for(int i=0;i<12;i++){h[i]=new List<float>();w[i]=new List<float>();}
   for(int square=0;square<64;square++)if(position.Board[square]!=0){var a=MeasurePiece(first,table,square);var b=MeasurePiece(second,table,square);if(a.x<=0||b.x<=0||Mathf.Abs(a.x-b.x)>Mathf.Max(.012f,Mathf.Min(a.x,b.x)*.2f)||Mathf.Abs(a.y-b.y)>.012f)continue;int index=ProfileIndex(position.Board[square]);h[index].Add((a.x+b.x)*.5f);w[index].Add((a.y+b.y)*.5f);}
   for(int i=0;i<12;i++)if(h[i].Count>0){h[i].Sort();w[i].Sort();profile[i*2]=h[i][h[i].Count/2];profile[i*2+1]=w[i][w[i].Count/2];}cameraAccess.enabled=false;table.PreviewPieceSizes(profile);
  }
  public static int ProfileIndex(int piece)=>(piece>0?0:6)+Math.Abs(piece)-1;
  static Vector2 MeasurePiece(ChessCheckerFit.Frame frame,ChessTable board,int square){var center=board.SquareWorld(square);center.y=board.transform.position.y;var plane=new Plane(Vector3.up,center);float[] means=new float[2];int[] counts=new int[2];for(int s=0;s<64;s++){var c=board.SquareWorld(s);foreach(var offset in new[]{new Vector3(.36f,0,.36f),new Vector3(-.36f,0,-.36f)}){var p=c+board.transform.TransformDirection(offset)*board.Size/8;p.y=center.y;float v=frame.Gray(p);if(v>=0){int color=(s%8+s/8)%2;means[color]+=v;counts[color]++;}}}if(counts.Any(n=>n<10))return Vector2.zero;for(int i=0;i<2;i++)means[i]/=counts[i];if(Mathf.Abs(means[0]-means[1])<35)return Vector2.zero;
   bool Foreground(Vector3 p){var uv=frame.Project(p);var ray=frame.Ray(uv);if(!plane.Raycast(ray,out float distance))return false;var local=board.transform.InverseTransformPoint(ray.GetPoint(distance));float x=local.x/board.Size*8*board.HumanSide+4,z=local.z/board.Size*8*board.HumanSide+4;if(x<0||z<0||x>=8||z>=8||x%1<.12f||x%1>.88f||z%1<.12f||z%1>.88f)return false;float v=frame.Gray(p);return v>=0&&Mathf.Abs(v-means[((int)x+(int)z)%2])>Mathf.Max(28,Mathf.Abs(means[0]-means[1])*.28f);}
   float top=0;int hits=0;for(float y=.018f;y<=.18f;y+=.003f)if(Foreground(center+Vector3.up*y)){top=y;hits++;}if(top<.03f||hits<5||top>=.174f)return Vector2.zero;var right=Vector3.Cross(Vector3.up,frame.camera-center).normalized;float width=0;for(float x=.006f;x<board.Size/8*.44f;x+=.002f)if(Foreground(center+Vector3.up*top*.25f+right*x)||Foreground(center+Vector3.up*top*.25f-right*x))width=x*2;return width>=.014f?new Vector2(top,width):Vector2.zero;
  }
  bool interrupted;
  void Update(){if(TableLifecycle.Blocked){if(!interrupted){interrupted=true;Cancel();}return;}if(interrupted){interrupted=false;if(table!=null&&table.CalibrationBusy)Fail("Alignment paused. Last pose held. Retry when the whole board is visible.");}}
  void OnApplicationPause(bool paused){if(paused){interrupted=true;Cancel();}}
  void OnDisable()=>Cancel();void OnDestroy(){if(cameraAccess!=null)Destroy(cameraAccess.gameObject);}
 }
}
