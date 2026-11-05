using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
using LivingTable.CandyLand;
namespace LivingTable.Chess {
 public sealed class ChessPhysicalVision:MonoBehaviour {
  ChessTable table;ChessMetaVision meta;PassthroughCameraAccess cameraAccess;
  bool permissionAsked,pending,requested,suspended,layoutScan,scanning,confirmHuman,approvedFull,pendingLayout;
  int generation,approvedVersion;float next,deadline;double approvedCapture,lastFrame;
  float[][] currentShapes;Vector3 currentView;
  float[][] baseline,candidate,approvedView,surveyView,lastScanView;double lastScanHand;float nextAutoProbe;ChessMove? approved;int[] surveyPieces;
  public readonly byte[] CellProgress=new byte[64];
  public void ClearProgress()=>Array.Clear(CellProgress,0,64);
  readonly CandyVisionStability stable=new CandyVisionStability();
  public void EnrollConfirmedPieces(){table.LearnPhysicalPieces(currentShapes,currentView);}
  public bool TrackingSuspended=>suspended;
  public void ResumeTracking(){ClearEvidence();next=0;Status="Camera watching automatically. Move your piece, then lift your hand.";}
  public bool Ready=>baseline!=null;
  public bool RequiresIdentity=>meta!=null&&meta.Required;
  public bool Checking=>requested||scanning;
  public bool NeedsAttention{get;private set;}
  public string Status{get;private set;}="Look at the aligned board";
  public bool SurveyStable=>surveyView!=null&&candidate!=null&&Time.realtimeSinceStartupAsDouble-lastFrame<1&&Matches(surveyView,candidate)&&table.HandsClear;
  public float[] SurveyAppearance=>SurveyStable?surveyView.SelectMany(x=>x).ToArray():null;
  public void Initialize(ChessTable owner){table=owner;meta=gameObject.AddComponent<ChessMetaVision>();meta.Required=FindFirstObjectByType<LivingTableExperience>()?.firstStepsMode==true;}
  public void ClearEvidence(){lastScanView=null;lastScanHand=table!=null?table.LastHandAt:0;nextAutoProbe=Time.unscaledTime+8;table?.ClearPhysicalMismatch();NeedsAttention=false;StopAllCoroutines();meta?.Cancel();generation++;pending=false;stable.Reset();candidate=null;approved=null;approvedView=null;surveyView=null;requested=scanning=layoutScan=confirmHuman=pendingLayout=false;suspended=false;table?.CancelHumanReview();}
  public void CancelScan(){ClearEvidence();ClearProgress();suspended=true;Status="Tracking paused. Use camera controls in MENU to retry; your saved position is unchanged.";table.RefreshScanControls();}
  public void ResetBoard(){ClearProgress();baseline=null;ClearEvidence();Status="Keep the board visible and hands clear";}
  public void RetryRecovery()=>CheckNow();
  public void CheckNow(){if(table.Playing&&!table.Recovering){Status="Press MOVED IT to finish the turn. Gameplay scanning is off.";table.RefreshScanControls();return;}ClearEvidence();ClearProgress();requested=true;deadline=Time.unscaledTime+120;next=0;Status=table.PendingAi.HasValue?"Checking AI move: "+table.PendingAi.Value+". Lift your hands.":"Checking the physical board. Lift your hands.";table.RefreshScanControls();}
  public void BeginLayoutScan(){CheckNow();layoutScan=true;surveyPieces=Enumerable.Repeat(99,64).ToArray();Status="Reading 64 squares. Keep the whole board visible.";}
  public void ConfirmHuman(){if(approved.HasValue&&table.AwaitingMoveAcceptance){confirmHuman=true;deadline=Time.unscaledTime+12;Status="Confirming the board has not changed…";}}
  public float[] CaptureAppearance()=>baseline==null?null:baseline.SelectMany(x=>x).ToArray();
  public void RestoreAppearance(float[] values){ClearEvidence();baseline=values!=null&&values.Length==768?Enumerable.Range(0,64).Select(i=>values.Skip(i*12).Take(12).ToArray()).ToArray():null;}
  public static bool Matches(float[][] expected,float[][] actual)=>expected!=null&&actual!=null&&expected.Length==64&&actual.Length==64&&Enumerable.Range(0,64).All(s=>Difference(expected[s],actual[s])<.075f);
  bool Active=>!TableLifecycle.Blocked&&table!=null&&table.isActiveAndEnabled&&table.Physical&&table.Aligned&&!table.AwaitingDone&&!table.PieceHeld&&!table.CalibrationBusy&&!table.StorageBlocked&&!table.Paused&&!table.SessionMenuOpen&&!suspended&&(layoutScan||table.SetupReviewing||(!table.AlignmentInProgress&&table.Playing&&table.Recovering));
  void Failed(string message){bool wasLayout=layoutScan;var partial=surveyPieces;var watched=lastScanView;double hand=lastScanHand;ClearEvidence();lastScanView=watched;lastScanHand=hand;nextAutoProbe=Time.unscaledTime+8;next=Time.unscaledTime+2;NeedsAttention=true;suspended=wasLayout;Status=message;if(wasLayout)table.ReceiveLayout(partial,message);else table.RefreshScanControls();}
  void RejectLayout(string message,ChessPosition target,ChessObservationGate.Observation observation){
   Failed(message);var seen=Enumerable.Repeat(100,64).ToArray();for(int square=0;square<64;square++){var cell=observation?.cells?.FirstOrDefault(c=>c!=null&&c.square==ChessMove.Square(square));if(cell==null)continue;seen[square]=ParsePiece(cell.piece);CellProgress[square]=(byte)(seen[square]==99?3:seen[square]==target.Board[square]?2:4);}table.ShowPhysicalMismatch(seen,target);
  }
  public static bool AppearanceChanged(float[][] before,float[][] after,float threshold=.025f)=>before!=null&&after!=null&&Enumerable.Range(0,64).Any(s=>Difference(before[s],after[s])>threshold);
  bool lifecycleHeld;
  void Update(){
   if(TableLifecycle.Blocked){if(!lifecycleHeld){lifecycleHeld=true;ClearEvidence();if(cameraAccess!=null)cameraAccess.enabled=false;}return;}
   if(lifecycleHeld){lifecycleHeld=false;lastFrame=0;next=Time.unscaledTime+1;pending=false;}
   if((requested||scanning||confirmHuman||(approvedView!=null&&!table.AwaitingMoveAcceptance))&&Time.unscaledTime>deadline){Failed("Scan timed out. Retry with the whole board visible, or use manual controls.");return;}
   if(!Active){if(cameraAccess!=null)cameraAccess.enabled=false;stable.Reset();return;}
#if UNITY_ANDROID && !UNITY_EDITOR
   const string permission="horizonos.permission.HEADSET_CAMERA";if(!Permission.HasUserAuthorizedPermission(permission)){Status="Allow camera access or choose MANUAL";if(!permissionAsked){permissionAsked=true;Permission.RequestUserPermission(permission);}return;}
   if(!PassthroughCameraAccess.IsSupported){Failed("Camera unavailable. Use manual controls.");return;}
#else
   return;
#endif
#pragma warning disable CS0162
   if(cameraAccess==null){var go=new GameObject("Chess local board camera");go.transform.SetParent(transform,false);go.SetActive(false);cameraAccess=go.AddComponent<PassthroughCameraAccess>();cameraAccess.RequestedResolution=new Vector2Int(1280,960);cameraAccess.MaxFramerate=15;go.SetActive(true);}cameraAccess.enabled=true;
   if(!cameraAccess.IsPlaying){Status="Starting camera…";return;}if(pending||Time.unscaledTime<next||!cameraAccess.IsUpdatedThisFrame)return;
   if(!table.HandsClear){stable.Reset();Status="Lift your hand away from the playing squares";return;}
   var source=cameraAccess.GetTexture();if(source==null)return;var pose=cameraAccess.GetCameraPose();var samples=new Vector2[64][];
   for(int square=0;square<64;square++){samples[square]=new Vector2[144];var center=table.SquareLocal(square);for(int y=0;y<12;y++)for(int x=0;x<12;x++){var local=center+new Vector3((x/11f-.5f)*table.Size/8*.7f,0,(y/11f-.5f)*table.Size/8*.7f);var world=table.transform.TransformPoint(local);var uv=cameraAccess.WorldToViewportPoint(world,pose);if(uv.x<.02f||uv.y<.02f||uv.x>.98f||uv.y>.98f||Vector3.Dot(world-pose.position,pose.rotation*Vector3.forward)<0){Status="Camera tracking paused until the board is visible";stable.Reset();return;}samples[square][y*12+x]=uv;}}
   pending=true;next=Time.unscaledTime+.3f;int version=table.Version,session=generation,w=source.width,h=source.height;double captured=Time.realtimeSinceStartupAsDouble;
   AsyncGPUReadback.Request(source,0,TextureFormat.RGBA32,request=>{if(this==null||session!=generation)return;pending=false;if(!Active||version!=table.Version||request.hasError||table.LastHandAt>captured||!table.HandsClear||Time.realtimeSinceStartupAsDouble-captured>.8)return;var pixels=request.GetData<Color32>();var descriptor=new float[64][];var shapes=new float[64][];
    for(int square=0;square<64;square++){var values=new float[12];var patch=new Color32[144];int patchIndex=0;foreach(var uv in samples[square]){var c=pixels[Mathf.Clamp((int)(uv.y*h),0,h-1)*w+Mathf.Clamp((int)(uv.x*w),0,w-1)];patch[patchIndex++]=c;values[Mathf.Min(3,c.r/64)]++;values[4+Mathf.Min(3,c.g/64)]++;values[8+Mathf.Min(3,c.b/64)]++;}for(int i=0;i<12;i++)values[i]/=144;descriptor[square]=values;shapes[square]=ChessPieceRegistry.Describe(patch);}
    lastFrame=captured;currentShapes=shapes;currentView=table.transform.InverseTransformDirection(pose.position-table.transform.position).normalized;
    if(table.SetupReviewing){candidate=descriptor;return;}
    if(approvedView!=null){
     if(approvedVersion!=version||table.LastHandAt>approvedCapture||!Matches(approvedView,descriptor)){Failed("Board changed during verification. Hold still; camera will recheck.");return;}
     // Inference can take seconds. Never accept its old image until multiple fresh,
     // hands-clear frames agree with the captured board for a further stable interval.
     if(!stable.Observe("verified-board-still",captured,.65f)){Status="Pieces read. Checking the board is still unchanged…";return;}
     if(pendingLayout){pendingLayout=false;layoutScan=false;approvedView=null;surveyView=descriptor;candidate=descriptor;table.ReceiveLayout(surveyPieces,surveyPieces.Any(x=>x==99)?"Amber squares need correction or a clearer view.":"64 squares read and stability checked.");Status="Review every piece, then choose whose turn it is.";stable.Reset();table.RefreshScanControls();return;}
     if(!approved.HasValue){baseline=descriptor;approvedView=null;requested=false;table.LearnPhysicalPieces(shapes,currentView);if(table.AcceptObservedBaseline(version)){ClearProgress();table.ClearPhysicalMismatch();Status="Saved position verified. Ready to play.";}return;}
     var move=approved.Value;bool fullApproval=approvedFull;approved=null;approvedView=null;confirmHuman=requested=false;
     var previous=baseline;baseline=descriptor;if(table.CommitObservedMove(move,version,fullApproval,true)){table.LearnPhysicalPieces(shapes,currentView);table.SaveSession("verified physical piece profiles updated");ClearProgress();table.ClearPhysicalMismatch();Status="Move verified. "+(table.Match.Position.Side==table.HumanSide?"Your turn.":"Opponent's turn.");stable.Reset();}else{baseline=previous;Failed("Position changed. Camera will recheck.");}return;
    }
    if(candidate==null||!Matches(candidate,descriptor)){candidate=descriptor;stable.Reset();Status="Keep the board still for a moment";return;}
    if(!stable.Observe("board-still",captured,.65f))return;
    // An unknown baseline must be identity-verified, never learned blindly after a first move.
    ChessMove? proposed=table.Playing?(table.PendingAi??table.TrackingMoveHint??Infer(table.Match.Position,baseline,descriptor,table.Promotion)):null;
    if(proposed.HasValue&&!table.PendingAi.HasValue&&!table.Recovering)table.PreviewTrackedMove(proposed.Value,version,shapes[proposed.Value.To],currentView);
    else if(!proposed.HasValue&&table.VisualMovePreview)table.CancelHumanReview();
    if(meta.Busy||scanning)return;
    // Appearance/hand activity triggers a read; only legal identity observations authorize a move.
    bool handChanged=table.LastHandAt>lastScanHand,visualChanged=AppearanceChanged(lastScanView??baseline,descriptor);
    if(!requested&&!layoutScan){
     if((table.PendingAi.HasValue||table.Recovering)&&!handChanged&&!visualChanged&&Time.unscaledTime<nextAutoProbe){Status=table.PendingAi.HasValue?"Move the pulsing piece to the pulsing destination. Camera watching.":"Match the pulsing guides. Camera watching.";return;}
     if(baseline!=null&&!table.PendingAi.HasValue&&!table.Recovering&&!proposed.HasValue&&!handChanged&&!visualChanged){if(!NeedsAttention)Status="Your move — camera watching automatically";return;}
    }
    if(!meta.Configured){Failed("Local vision is unavailable. Reconnect it, retry, or select MANUAL MODE.");return;}
    bool full=layoutScan||table.Recovering||table.ManualFallback||baseline==null||!proposed.HasValue;
    if(!full&&proposed.HasValue){var changed=ChangedSquares(table.Match.Position,proposed.Value);if(Enumerable.Range(0,64).Any(s=>!changed.Contains(s)&&Difference(baseline[s],descriptor[s])>.075f))full=true;}
    int[] squares=full?Enumerable.Range(0,64).ToArray():ChangedSquares(table.Match.Position,proposed.Value);var crops=new ChessMetaVision.Crop[squares.Length];
    for(int i=0;i<squares.Length;i++){crops[i]=CropSquare(squares[i],pixels,w,h,pose);if(crops[i]==null){Status="Look down at all pieces, including their tops";stable.Reset();return;}}
    if(!requested)deadline=Time.unscaledTime+120;scanning=true;NeedsAttention=false;lastScanView=descriptor;lastScanHand=table.LastHandAt;nextAutoProbe=Time.unscaledTime+8;table.RefreshScanControls();stable.Reset();StartCoroutine(Scan(version,session,captured,descriptor,table.Match?.Position.Copy(),proposed,full,squares,crops));
   });
#pragma warning restore CS0162
  }
  IEnumerator Scan(int version,int session,double captured,float[][] view,ChessPosition position,ChessMove? proposed,bool full,int[] squares,ChessMetaVision.Crop[] crops){
   var cells=new List<ChessObservationGate.Cell>();bool layout=layoutScan;
   for(int start=0;start<crops.Length;start+=4){
    foreach(int square in squares.Skip(start).Take(4))CellProgress[square]=1;
    if(!Current(version,session,captured)){Failed("Scan interrupted. Lift your hands; camera will recheck.");yield break;}
    Status="Checking "+(proposed.HasValue?proposed.Value.ToString():"board")+": "+start+" / "+crops.Length+" squares. Hands clear.";
    ChessObservationGate.Observation observation=null;
    yield return meta.Observe(version,crops.Skip(start).Take(4).ToArray(),result=>observation=result);
    if(!Current(version,session,captured)){Failed("Board or session changed. Camera will recheck.");yield break;}
    var batch=squares.Skip(start).Take(4).ToArray();
    if(observation==null||observation.revision!=version||observation.cells==null||observation.cells.Length!=batch.Length){Failed(observation==null?meta.Status:"Incomplete response. Retry the scan.");yield break;}
    foreach(int square in batch){var matching=observation.cells.Where(c=>c!=null&&c.square==ChessMove.Square(square)).ToArray();if(matching.Length!=1){Failed("Missing or duplicate square. Retry the scan.");yield break;}var cell=matching[0];cells.Add(cell);int piece=ParsePiece(cell.piece);CellProgress[square]=(byte)(piece==99||!layout&&position!=null&&(proposed.HasValue||table.Recovering)&&piece!=(proposed.HasValue?position.After(proposed.Value):position).Board[square]?3:2);if(layout)surveyPieces[square]=ParsePiece(cell.piece);}
   }
   scanning=requested=false;
   if(layout){pendingLayout=true;approvedView=view;approvedVersion=version;approvedCapture=captured;stable.Reset();deadline=Time.unscaledTime+12;Status="All squares read. Checking fresh camera frames before review…";yield break;}
   layoutScan=false;
   var result=new ChessObservationGate.Observation{revision=version,cells=cells.ToArray()};
   if(full&&table.Recovering){var observed=new int[64];for(int s=0;s<64;s++){var cell=result.cells.FirstOrDefault(c=>c.square==ChessMove.Square(s));observed[s]=cell==null?99:ParsePiece(cell.piece);}if(table.OrientRecoveredBoard(observed))yield break;}
   if(proposed.HasValue){
    bool valid=full?ChessObservationGate.AcceptBoard(position.After(proposed.Value),version,result,out _):ChessObservationGate.Accept(position,proposed.Value,version,result);
    if(!valid){var target=position.After(proposed.Value);int mismatch;ChessObservationGate.AcceptCells(target,squares,version,result,out mismatch);RejectLayout(mismatch>=0?ChessMove.Square(mismatch)+": expected "+ChessObservationGate.Piece(target.Board[mismatch]).Replace('_',' ')+". Follow the pulsing guides; camera will recheck.":"Uncertain board. Amber squares need a clearer view.",target,result);yield break;}
   }else{
    var legal=position.LegalMoves().Where(m=>!table.Recovering).Where(m=>ChessObservationGate.AcceptBoard(position.After(m),version,result,out _)).ToArray();
    if(legal.Length==1)proposed=legal[0];
    else if(ChessObservationGate.AcceptBoard(position,version,result,out _)){approvedView=view;approvedVersion=version;approvedCapture=captured;approved=null;stable.Reset();deadline=Time.unscaledTime+12;Status="Board matches. Checking fresh frames before tracking resumes...";yield break;}
    else{if(table.Recovering){ChessObservationGate.AcceptBoard(position,version,result,out int mismatch);RejectLayout(mismatch>=0?"Restore "+ChessMove.Square(mismatch)+" to "+ChessObservationGate.Piece(position.Board[mismatch]).Replace('_',' ')+". Camera will recheck.":"Restore the pulsing guides to the saved layout.",position,result);}else RejectLayout("No legal move matches this scan. Red squares differ from the save; amber squares are unreadable. Restore the pulsing guides.",position,result);yield break;}
   }
   approved=proposed;approvedFull=full;approvedVersion=version;approvedCapture=captured;approvedView=view;stable.Reset();deadline=Time.unscaledTime+12;Status="Pieces match. Checking fresh camera frames…";
  }
  bool Current(int version,int session,double captured)=>this!=null&&Active&&generation==session&&table.Version==version&&table.HandsClear&&table.LastHandAt<=captured;
  public static int ParsePiece(string value){for(int p=-6;p<=6;p++)if(ChessObservationGate.Piece(p)==value)return p;return 99;}
  ChessMetaVision.Crop CropSquare(int square,Unity.Collections.NativeArray<Color32> pixels,int w,int h,Pose pose)
  {
   var center=table.SquareLocal(square);float half=table.Size/8*.48f;Vector2 min=Vector2.one,max=Vector2.zero;
   for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)for(int y=0;y<=1;y++){var world=table.transform.TransformPoint(center+new Vector3(x*half,y*table.MaxPhysicalPieceHeight,z*half));if(Vector3.Dot(world-pose.position,pose.rotation*Vector3.forward)<=0)return null;var uv=cameraAccess.WorldToViewportPoint(world,pose);min=Vector2.Min(min,uv);max=Vector2.Max(max,uv);}
   if(min.x<0||min.y<0||max.x>1||max.y>1||(max.x-min.x)*w<24||(max.y-min.y)*h<24)return null;
   var fitted=FitCrop((max.x-min.x)*w,(max.y-min.y)*h);
   var image=new Texture2D(256,256,TextureFormat.RGB24,false);try{var colors=new Color32[256*256];
    for(int y=0;y<256;y++)for(int x=0;x<256;x++){if(x<fitted.xMin||x>=fitted.xMax||y<fitted.yMin||y>=fitted.yMax){colors[y*256+x]=new Color32(127,127,127,255);continue;}int px=Mathf.Clamp((int)(Mathf.Lerp(min.x,max.x,(x-fitted.xMin)/(fitted.width-1))*w),0,w-1),py=Mathf.Clamp((int)(Mathf.Lerp(min.y,max.y,(y-fitted.yMin)/(fitted.height-1))*h),0,h-1);colors[y*256+x]=pixels[py*w+px];}
    image.SetPixels32(colors);image.Apply();return new ChessMetaVision.Crop{square=ChessMove.Square(square),image=Convert.ToBase64String(image.EncodeToJPG(85))};
   }finally{Destroy(image);}
  }
  // Letterbox rather than stretching a tall king or wide projected square into a new shape.
  public static Rect FitCrop(float width,float height){float scale=256/Mathf.Max(width,height);float fw=Mathf.Max(2,Mathf.Round(width*scale)),fh=Mathf.Max(2,Mathf.Round(height*scale));return new Rect(Mathf.Floor((256-fw)*.5f),Mathf.Floor((256-fh)*.5f),fw,fh);}
  public static float Difference(float[] a,float[] b){float total=0;for(int i=0;i<a.Length;i++)total+=Mathf.Abs(a[i]-b[i]);return total/6;}
  static float AverageChange(float[][] a,float[][] b){float sum=0;for(int s=0;s<64;s++)sum+=Difference(a[s],b[s]);return sum/64;}
  public static int[] ChangedSquares(ChessPosition position,ChessMove move){var after=position.After(move);return Enumerable.Range(0,64).Where(i=>position.Board[i]!=after.Board[i]).ToArray();}
  public static ChessMove? Infer(ChessPosition position,float[][] before,float[][] after,int promotion=5,ChessMove? expected=null)
  {
   if(before==null||after==null||before.Length!=64||after.Length!=64)return null;var delta=Enumerable.Range(0,64).Select(i=>Difference(before[i],after[i])).ToArray();ChessMove? found=null;
   foreach(var move in position.LegalMoves()){if(move.Promotion!=0&&move.Promotion!=promotion||expected.HasValue&&!move.Equals(expected.Value))continue;var changed=ChangedSquares(position,move);if(changed.Any(s=>delta[s]<.10f))continue;if(Enumerable.Range(0,64).Any(s=>!changed.Contains(s)&&delta[s]>.075f))continue;if(found.HasValue)return null;found=move;}return found;
  }
  void OnDisable(){ClearEvidence();if(cameraAccess!=null)cameraAccess.enabled=false;}
  void OnApplicationPause(bool paused){if(paused){lifecycleHeld=true;ClearEvidence();if(cameraAccess!=null)cameraAccess.enabled=false;}}
  void OnDestroy(){if(cameraAccess!=null)Destroy(cameraAccess.gameObject);}
 }
}
