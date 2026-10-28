using System;
using System.Collections;
using System.Threading.Tasks;
using Meta.XR;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
namespace LivingTable.CandyLand
{
    public sealed class CandyPhysicalVision:MonoBehaviour
    {
        [Serializable] public sealed class Edition {public string edition;public CandyVisionFeatures.Reference board;public CandyVisionFeatures.Reference[] pictures;}
        CandyTable table;CandyVisionDetector detector;PassthroughCameraAccess cameraAccess;
        Vector3? previousCardPlace;bool requestHandNear;
        Task<CandyVisionDetector.Finding> processing;Color32[] background;bool requesting,permissionAsked,pausedBySystem,discardUntilClear;
        int generation,requestGeneration,requestTurn;CandyGame requestGame;CandyPhase requestPhase;Pose requestPose;double requestTimestamp;Vector2Int requestResolution;float nextFrame;
        readonly CandyVisionStability boardStable=new CandyVisionStability(),cardStable=new CandyVisionStability(),pieceStable=new CandyVisionStability(),clearStable=new CandyVisionStability();
        public string Status {get;private set;}="Look at your physical board";
        public bool Ready=>background!=null;
        public void Initialize(CandyTable owner){table=owner;var asset=Resources.Load<TextAsset>("Vision/candy-1984-features");if(asset!=null){var edition=JsonUtility.FromJson<Edition>(asset.text);detector=new CandyVisionDetector(edition.board,edition.pictures);}else Status="Recognition references unavailable · use manual controls";}
        public void Invalidate(bool boardChanged=true){generation++;if(boardChanged){background=null;previousCardPlace=null;}boardStable.Reset();cardStable.Reset();pieceStable.Reset();clearStable.Reset();discardUntilClear=true;}
        public void RetryPermission(){permissionAsked=false;Invalidate();}
        bool Active=>table!=null&&table.AutomaticPhysical&&!table.VisionFallback&&!table.Paused&&!pausedBySystem&&table.RecognitionAllowed;
        void ResetEvidence(){cardStable.Reset();pieceStable.Reset();boardStable.Reset();clearStable.Reset();}
        void Update()
        {
            if(processing!=null&&processing.IsCompleted){var finished=processing;processing=null;if(finished.IsFaulted){Status="Camera reading interrupted · try a clearer view";Debug.LogWarning("[CandyVision] Processing failed: "+finished.Exception?.GetBaseException().GetType().Name);}else if(Active&&generation==requestGeneration&&table.Game==requestGame&&table.Phase==requestPhase&&(table.Game==null||table.Game.Turn==requestTurn)&&Time.realtimeSinceStartupAsDouble-requestTimestamp<1.5)Apply(finished.Result);}
            if(!Active){if(cameraAccess!=null)cameraAccess.enabled=false;ResetEvidence();return;}
            if(detector==null||table.Desktop)return;
#if UNITY_ANDROID && !UNITY_EDITOR
            const string permission="horizonos.permission.HEADSET_CAMERA";
            if(!Permission.HasUserAuthorizedPermission(permission)){Status="Allow headset camera access, or use MANUAL";if(!permissionAsked){permissionAsked=true;Permission.RequestUserPermission(permission);}return;}
            if(!PassthroughCameraAccess.IsSupported){Status="Camera access unavailable on this headset · use MANUAL";return;}
#endif
            if(cameraAccess==null){var go=new GameObject("Local Candy Land recognition camera");go.transform.SetParent(transform,false);go.SetActive(false);cameraAccess=go.AddComponent<PassthroughCameraAccess>();cameraAccess.CameraPosition=PassthroughCameraAccess.CameraPositionType.Left;cameraAccess.RequestedResolution=new Vector2Int(1280,960);cameraAccess.MaxFramerate=15;go.SetActive(true);}
            cameraAccess.enabled=true;
            if(!cameraAccess.IsPlaying){Status="Starting headset camera…";return;}
            if(requesting||processing!=null||Time.unscaledTime<nextFrame||!cameraAccess.IsUpdatedThisFrame)return;
            var source=cameraAccess.GetTexture();if(source==null)return;
            requesting=true;nextFrame=Time.unscaledTime+.35f;int version=generation;var game=table.Game;int turn=game==null?-1:game.Turn;var phase=table.Phase;var pose=cameraAccess.GetCameraPose();var projectedBoard=new Vector2[4];for(int i=0;i<4;i++){float x=i==0||i==3?-.5f:.5f,z=i<2?-.5f:.5f;projectedBoard[i]=cameraAccess.WorldToViewportPoint(table.BoardFrame.TransformPoint(new Vector3(x*table.Width,0,z*table.Width*913f/1280f)),pose);}
            bool handNear=table.HandNearBoard;double captured=Time.realtimeSinceStartupAsDouble;int width=source.width,height=source.height;int color=table.Game==null?table.PlayerColor:(table.Game.CurrentPlayer+table.PlayerColor)%4;
            AsyncGPUReadback.Request(source,0,TextureFormat.RGBA32,request=>{
                requesting=false;if(this==null||!Active||version!=generation||request.hasError){if(this!=null&&request.hasError)Status="Camera frame unavailable · use MANUAL";return;}
                var native=request.GetData<Color32>();var raw=new Color32[native.Length];native.CopyTo(raw);
                requestHandNear=handNear;requestGame=game;requestTurn=turn;requestPhase=phase;requestGeneration=version;requestPose=pose;requestTimestamp=captured;requestResolution=new Vector2Int(640,Mathf.RoundToInt(640f*height/width));var size=requestResolution;var savedBackground=background;var engine=detector;
                processing=Task.Run(()=>{var reduced=Reduce(raw,width,height,size.x,size.y);var mask=new Vector2[4];for(int i=0;i<4;i++)mask[i]=Vector2.Scale(projectedBoard[i],size);return engine.Detect(reduced,size.x,size.y,savedBackground,color,savedBackground==null?null:mask);});
            });
        }
        public static Color32[] Reduce(Color32[] source,int width,int height,int w,int h){var data=new Color32[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)data[y*w+x]=source[Math.Min(height-1,y*height/h)*width+Math.Min(width-1,x*width/w)];return data;}
        void Apply(CandyVisionDetector.Finding finding)
        {
            double now=requestTimestamp;
            if(finding.board==null&&(background==null||table.Phase!=CandyPhase.Ready||!table.NeedsPhysicalCard)){ResetEvidence();Status="Look at the whole board in good light";return;}
            if(background==null){
                if(table.Game!=null&&!table.SessionRecovering){Status="Return to setup and look at the board to align";return;}
                if(requestHandNear||table.HandNearBoard||table.LastBoardHandSeen>requestTimestamp){boardStable.Reset();Status="Hands clear for a moment while the board aligns";return;}
                if(boardStable.Observe("board",now,1)){
                    var world=new Vector3[4];var plane=new Plane(table.BoardFrame.up,table.BoardFrame.position);bool valid=true;
                    for(int i=0;i<4;i++){var uv=finding.board[i]/requestResolution;var ray=cameraAccess.ViewportPointToRay(uv,requestPose);if(!plane.Raycast(ray,out float d)||d<.15f||d>2.5f){valid=false;break;}world[i]=ray.GetPoint(d);}
                    if(valid&&table.AlignRecognizedBoard(world)&&!table.HandNearBoard){background=finding.rectified;discardUntilClear=true;Status="Board found · draw a real card beside it";Debug.Log("[CandyVision] Board aligned from "+finding.boardInliers+" landmarks");}else Status="Place the table surface, then look at the full board";
                }return;
            }
            if(table.Game==null){Status="Ready · physical cards and pieces recognized locally";return;}
            if(table.SessionRecovering){Status="Recreate the saved layout, then MANUAL CHECK. Piece identities are not automatically verified.";return;}
            // Removal is required after every accepted card; consecutive identical cards still work after a clear interval.
            bool previousAreaVisible=true;if(previousCardPlace.HasValue){var uv=cameraAccess.WorldToViewportPoint(previousCardPlace.Value,requestPose);var view=Quaternion.Inverse(requestPose.rotation)*(previousCardPlace.Value-requestPose.position);previousAreaVisible=view.z>0&&uv.x>.12f&&uv.x<.88f&&uv.y>.12f&&uv.y<.88f;}
            if(!finding.card.HasValue){if(finding.board!=null&&previousAreaVisible){if(clearStable.Observe("clear",now,.6f))discardUntilClear=false;}else clearStable.Reset();cardStable.Reset();}else clearStable.Reset();
            if(table.Game==null){Status="Ready � physical cards and pieces recognized locally";return;}
            if(table.Phase==CandyPhase.Ready&&table.NeedsPhysicalCard){
                if(discardUntilClear){Status="Move the previous card out of view, then show the next";return;}
                if(!finding.card.HasValue){Status="Show one card face-up beside the board";return;}
                var card=finding.card.Value;Status="Reading "+card.Label+"…";
                if(cardStable.Observe(card.Label,now,.8f)&&table.AcceptCameraCard(card,requestGame,requestTurn)){discardUntilClear=true;cardStable.Reset();Status="Card read · move your piece to the glowing space";Debug.Log("[CandyVision] Stable card accepted: "+card.Label);}return;
            }
            if(table.Phase==CandyPhase.AwaitingMove){
                if(requestHandNear||table.HandNearBoard||table.LastBoardHandSeen>requestTimestamp||!finding.piece.HasValue){pieceStable.Reset();Status=table.HandNearBoard?"Move your piece, then lift your hand away":"Looking for your piece · keep the board visible";return;}
                int space=NearestSpace(table.Data,finding.piece.Value);if(space!=table.ExpectedManualSpace){pieceStable.Reset();Status=space<0?"Waiting for the piece to settle on a space":"Move your piece to the glowing space";return;}
                if(pieceStable.Observe(requestTurn+":"+space,now,1)&&table.AcceptCameraMove(space,requestGame,requestTurn)){pieceStable.Reset();Status="Move recognized";Debug.Log("[CandyVision] Stable physical move accepted: "+space);}else Status="Checking your piece has settled…";
            }
        }
        public static int NearestSpace(CandyBoardData data,Vector2 bottomUp)
        {
            var point=new Vector2(bottomUp.x,1-bottomUp.y);int nearest=-1;float best=float.MaxValue,second=float.MaxValue;
            for(int i=0;i<data.spaces.Length;i++){var s=data.spaces[i];float d=Vector2.Distance(point,new Vector2(s.x,s.y));if(d<best){second=best;best=d;nearest=i;}else second=Mathf.Min(second,d);}
            return best<.021f&&second-best>.005f?nearest:-1;
        }
        void OnApplicationPause(bool pause){pausedBySystem=pause;generation++;ResetEvidence();if(pause&&cameraAccess!=null)cameraAccess.enabled=false;}
        void OnDisable(){generation++;ResetEvidence();if(cameraAccess!=null)cameraAccess.enabled=false;}
        void OnDestroy(){generation++;if(cameraAccess!=null)Destroy(cameraAccess.gameObject);}
    }
}
