using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
using LivingTable.CandyLand;
namespace LivingTable.Chess
{
 // Registered cell appearance changes + legal-move constraints. Does not claim to classify arbitrary piece identities.
 public sealed class ChessPhysicalVision:MonoBehaviour
 {
  bool recoveryAttempted,recoveryApproved;float[][] recoveryView;double recoveryCapture;int recoveryVersion;
  ChessMetaVision meta;ChessMove? approved;float[][] approvedView;int approvedVersion;double approvedCapture;
  ChessTable table;PassthroughCameraAccess cameraAccess;bool permissionAsked,pending;int generation;float next;float[][] baseline,candidate;
  readonly CandyVisionStability stable=new CandyVisionStability();public bool Ready=>baseline!=null;public string Status {get;private set;}="Look at the aligned board";
  public void Initialize(ChessTable owner){table=owner;meta=gameObject.AddComponent<ChessMetaVision>();meta.Required=FindFirstObjectByType<LivingTableExperience>()?.firstStepsMode==true;}
  public bool RequiresIdentity=>meta!=null&&meta.Required;
  public void RetryRecovery(){ClearEvidence();Status="Keep the whole board visible and hands clear";}
  public void ClearEvidence(){StopAllCoroutines();meta?.Cancel();recoveryAttempted=recoveryApproved=false;recoveryView=null;generation++;stable.Reset();candidate=null;approved=null;approvedView=null;}
  public void ResetBoard(){baseline=null;ClearEvidence();Status="Standard position · hands clear while the camera settles";}
  public float[] CaptureAppearance()=>baseline==null?null:baseline.SelectMany(x=>x).ToArray();
  public void RestoreAppearance(float[] values){ClearEvidence();baseline=values!=null&&values.Length==768?Enumerable.Range(0,64).Select(i=>values.Skip(i*12).Take(12).ToArray()).ToArray():null;}
  public static bool Matches(float[][] expected,float[][] actual)=>expected!=null&&actual!=null&&expected.Length==64&&actual.Length==64&&Enumerable.Range(0,64).All(s=>Difference(expected[s],actual[s])<.075f);
  bool Active=>table!=null&&table.isActiveAndEnabled&&!table.CalibrationBusy&&!table.AlignmentInProgress&&table.Physical&&table.Aligned&&table.Playing&&(table.Match.Result==""||table.Recovering)&&!table.StorageBlocked&&!table.Paused&&!table.SessionMenuOpen&&(!table.ManualFallback||table.Recovering);
  void Update()
  {
   if(!Active){if(cameraAccess!=null)cameraAccess.enabled=false;stable.Reset();return;}
#if UNITY_ANDROID && !UNITY_EDITOR
   const string permission="horizonos.permission.HEADSET_CAMERA";if(!Permission.HasUserAuthorizedPermission(permission)){Status="Allow camera access or choose MANUAL";if(!permissionAsked){permissionAsked=true;Permission.RequestUserPermission(permission);}return;}
   if(!PassthroughCameraAccess.IsSupported){Status="Camera unavailable · choose MANUAL";return;}
#else
   return;
#endif
#pragma warning disable CS0162
   if(cameraAccess==null){var go=new GameObject("Chess local board camera");go.transform.SetParent(transform,false);go.SetActive(false);cameraAccess=go.AddComponent<PassthroughCameraAccess>();cameraAccess.RequestedResolution=new Vector2Int(1280,960);cameraAccess.MaxFramerate=15;go.SetActive(true);}cameraAccess.enabled=true;
   if(!cameraAccess.IsPlaying){Status="Starting camera…";return;}if(meta.Busy){if(!table.Recovering)Status=meta.Status;return;}if(pending||Time.unscaledTime<next||!cameraAccess.IsUpdatedThisFrame)return;
   if(!table.HandsClear){stable.Reset();Status="Move your piece, then lift your hand away";return;}
   var source=cameraAccess.GetTexture();if(source==null)return;var pose=cameraAccess.GetCameraPose();var samples=new Vector2[64][];
   for(int square=0;square<64;square++){samples[square]=new Vector2[144];var center=table.SquareLocal(square);for(int y=0;y<12;y++)for(int x=0;x<12;x++){var local=center+new Vector3((x/11f-.5f)*table.Size/8*.7f,.005f,(y/11f-.5f)*table.Size/8*.7f);var world=table.transform.TransformPoint(local);var uv=cameraAccess.WorldToViewportPoint(world,pose);if(uv.x<.02f||uv.y<.02f||uv.x>.98f||uv.y>.98f||Vector3.Dot(world-pose.position,pose.rotation*Vector3.forward)<0){Status="Look at the entire board from above";stable.Reset();return;}samples[square][y*12+x]=uv;}}
   pending=true;next=Time.unscaledTime+.3f;int version=table.Version,session=generation,w=source.width,h=source.height;double captured=Time.realtimeSinceStartupAsDouble;
   AsyncGPUReadback.Request(source,0,TextureFormat.RGBA32,request=>{pending=false;if(this==null||!Active||session!=generation||version!=table.Version||request.hasError||table.LastHandAt>captured||!table.HandsClear||Time.realtimeSinceStartupAsDouble-captured>.8)return;var pixels=request.GetData<Color32>();var descriptor=new float[64][];
    for(int square=0;square<64;square++){var values=new float[12];foreach(var uv in samples[square]){var c=pixels[Mathf.Clamp((int)(uv.y*h),0,h-1)*w+Mathf.Clamp((int)(uv.x*w),0,w-1)];values[Mathf.Min(3,c.r/64)]++;values[4+Mathf.Min(3,c.g/64)]++;values[8+Mathf.Min(3,c.b/64)]++;}for(int i=0;i<12;i++)values[i]/=144;descriptor[square]=values;}
    if(table.Recovering){
     if(meta.Required){
      if(recoveryApproved){bool valid=recoveryVersion==version&&table.LastHandAt<=recoveryCapture&&Matches(recoveryView,descriptor);recoveryApproved=false;recoveryView=null;
       if(valid){var previous=baseline;baseline=descriptor;if(!table.AcceptRecovery(version,true))baseline=previous;else Status="Saved position verified. Ready to continue.";return;}
       Status="Board changed during scan. Keep it still and press SCAN BOARD.";recoveryAttempted=true;return;
      }
      if(!meta.Configured){Status="Connect local vision or use labeled MANUAL CHECK.";return;}
      if(recoveryAttempted)return;
      if(candidate==null||!Matches(candidate,descriptor)){candidate=descriptor;stable.Reset();Status="Match guide pieces; keep the board still for scanning.";return;}
      if(!stable.Observe("identity-reconcile",captured,1))return;
      var recoveryCrops=new ChessMetaVision.Crop[64];
      for(int square=0;square<64;square++){recoveryCrops[square]=CropSquare(square,pixels,w,h,pose);if(recoveryCrops[square]==null){stable.Reset();Status="Look down at the entire board, including tall pieces.";return;}}
      recoveryAttempted=true;stable.Reset();StartCoroutine(Reconcile(version,session,captured,descriptor,table.Match.Position.Copy(),recoveryCrops));return;
     }
     if(baseline==null){Status="Match the saved layout, then use MANUAL CHECK. No camera reference exists.";return;}if(!Matches(baseline,descriptor)){stable.Reset();Status="Board differs from the saved view. Restore pieces or use an explicit manual check.";return;}if(stable.Observe("reconcile",captured,1))table.AcceptRecovery(version);return;}
    if(baseline==null){if(candidate==null||AverageChange(candidate,descriptor)>.035f){candidate=descriptor;stable.Reset();Status="Keep the starting position still for a moment";return;}if(stable.Observe("baseline",captured,.8f)){baseline=descriptor;candidate=null;stable.Reset();Status="Camera ready - make your move";table.SaveSession("camera baseline captured");}return;}
    var move=Infer(table.Match.Position,baseline,descriptor,table.Promotion,table.PendingAi);if(!move.HasValue){stable.Reset();Status=Enumerable.Range(0,64).Any(s=>Difference(baseline[s],descriptor[s])>.075f)?"Incomplete or unexpected move. Finish the requested move, or restore the last position.":"Your move - camera watching";return;}
    if(approved.HasValue){var checkedMove=approved.Value;bool valid=approvedVersion==version&&table.LastHandAt<=approvedCapture&&checkedMove.Equals(move.Value)&&Matches(approvedView,descriptor);approved=null;approvedView=null;if(valid){Commit(checkedMove,descriptor,version);return;}stable.Reset();Status="Board changed during vision check - checking again";return;}
    if(meta.Required&&!meta.Configured){Status="Connect local vision, or choose MANUAL";return;}
    Status="Checking "+move.Value+"...";if(stable.Observe(move.Value.ToString(),captured,.8f)){
     if(meta.Required){
      var proposed=move.Value;var position=table.Match.Position.Copy();var changed=ChangedSquares(position,proposed);var crops=new ChessMetaVision.Crop[changed.Length];
      for(int i=0;i<changed.Length;i++){
       crops[i]=CropSquare(changed[i],pixels,w,h,pose);if(crops[i]==null){stable.Reset();Status="Look down at all moved squares";return;}
      }
      stable.Reset();StartCoroutine(meta.Observe(version,crops,observation=>{
       if(this==null||!Active||generation!=session||table.Version!=version||table.LastHandAt>captured)return;
       if(ChessObservationGate.Accept(position,proposed,version,observation)){approved=proposed;approvedView=descriptor;approvedVersion=version;approvedCapture=captured;Status="Vision agrees - checking the board is still unchanged";}
       else{Status=observation==null?meta.Status:"Vision unclear or piece mismatch - hold still to retry, or choose MANUAL";next=Time.unscaledTime+4;}
      }));return;
     }Commit(move.Value,descriptor,version);
    }
   });
#pragma warning restore CS0162
  }
  ChessMetaVision.Crop CropSquare(int square,Unity.Collections.NativeArray<Color32> pixels,int w,int h,Pose pose)
  {
   var center=table.SquareLocal(square);float half=table.Size/8*.48f;Vector2 min=Vector2.one,max=Vector2.zero;
   for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)for(int y=0;y<=1;y++){var uv=cameraAccess.WorldToViewportPoint(table.transform.TransformPoint(center+new Vector3(x*half,y*.085f,z*half)),pose);min=Vector2.Min(min,uv);max=Vector2.Max(max,uv);}
   if(min.x<0||min.y<0||max.x>1||max.y>1)return null;
   var image=new Texture2D(256,256,TextureFormat.RGB24,false);try{var colors=new Color32[256*256];
    for(int y=0;y<256;y++)for(int x=0;x<256;x++){int px=Mathf.Clamp((int)(Mathf.Lerp(min.x,max.x,x/255f)*w),0,w-1),py=Mathf.Clamp((int)(Mathf.Lerp(min.y,max.y,y/255f)*h),0,h-1);colors[y*256+x]=pixels[py*w+px];}
    image.SetPixels32(colors);image.Apply();return new ChessMetaVision.Crop{square=ChessMove.Square(square),image=Convert.ToBase64String(image.EncodeToJPG(85))};
   }finally{Destroy(image);}
  }
  IEnumerator Reconcile(int version,int session,double captured,float[][] view,ChessPosition position,ChessMetaVision.Crop[] crops)
  {
   var cells=new List<ChessObservationGate.Cell>();
   for(int start=0;start<64;start+=4){
    if(!RecoveryCurrent(version,session,captured))yield break;
    Status="Checking saved position: "+start+" / 64 squares. Keep hands clear.";
    ChessObservationGate.Observation observation=null;
    yield return meta.Observe(version,crops.Skip(start).Take(4).ToArray(),result=>observation=result);
    if(!RecoveryCurrent(version,session,captured)){Status="Scan interrupted. Keep hands clear and press SCAN BOARD.";yield break;}
    if(!ChessObservationGate.AcceptCells(position,Enumerable.Range(start,4).ToArray(),version,observation,out int mismatch)){
     Status=observation==null?"Vision unavailable. Retry SCAN BOARD or use MANUAL CHECK.":mismatch<0?"Incomplete scan. Retry SCAN BOARD.":ChessMove.Square(mismatch)+": expected "+ChessObservationGate.Piece(position.Board[mismatch]).Replace('_',' ')+". Check that square, then SCAN BOARD.";
     yield break;
    }
    cells.AddRange(observation.cells);
   }
   if(ChessObservationGate.AcceptBoard(position,version,new ChessObservationGate.Observation{revision=version,cells=cells.ToArray()},out _)){
    recoveryApproved=true;recoveryView=view;recoveryCapture=captured;recoveryVersion=version;Status="64 squares checked. Confirming the board stayed unchanged...";
   }
  }
  bool RecoveryCurrent(int version,int session,double captured)=>this!=null&&Active&&table.Recovering&&table.Version==version&&generation==session&&table.LastHandAt<=captured;
  void Commit(ChessMove move,float[][] descriptor,int version){var previous=baseline;baseline=descriptor;if(table.ApplyCameraMove(move,version)){stable.Reset();Status=meta.Required?"Vision + legal rules confirmed the move":"Move recognized";}else if(table.Version==version)baseline=previous;}
  public static float Difference(float[] a,float[] b){float total=0;for(int i=0;i<a.Length;i++)total+=Mathf.Abs(a[i]-b[i]);return total/6;}
  static float AverageChange(float[][] a,float[][] b){float sum=0;for(int s=0;s<64;s++)sum+=Difference(a[s],b[s]);return sum/64;}
  public static int[] ChangedSquares(ChessPosition position,ChessMove move){var after=position.After(move);return Enumerable.Range(0,64).Where(i=>position.Board[i]!=after.Board[i]).ToArray();}
  public static ChessMove? Infer(ChessPosition position,float[][] before,float[][] after,int promotion=5,ChessMove? expected=null)
  {
   if(before==null||after==null||before.Length!=64||after.Length!=64)return null;var delta=Enumerable.Range(0,64).Select(i=>Difference(before[i],after[i])).ToArray();ChessMove? found=null;
   foreach(var move in position.LegalMoves()){if(move.Promotion!=0&&move.Promotion!=promotion||expected.HasValue&&!move.Equals(expected.Value))continue;var changed=ChangedSquares(position,move);if(changed.Any(s=>delta[s]<.10f))continue;if(Enumerable.Range(0,64).Any(s=>!changed.Contains(s)&&delta[s]>.075f))continue;if(found.HasValue)return null;found=move;}return found;
  }
  void OnDisable(){ClearEvidence();if(cameraAccess!=null)cameraAccess.enabled=false;}
  void OnApplicationPause(bool paused){if(paused){ClearEvidence();if(cameraAccess!=null)cameraAccess.enabled=false;}}
  void OnDestroy(){if(cameraAccess!=null)Destroy(cameraAccess.gameObject);}
 }
}
