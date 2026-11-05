using System;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  [Serializable] public sealed class BoardRegistration {
   public Vector3[] pinchedCorners;public bool[] cameraSnapped,inferredCorners;public Vector3[] corners;public Vector3 center;public Quaternion rotation;public float size,setScale;public int playerColor;public string recordedUtc,anchorId,alignmentMethod;public int detectedCorners;public float fitErrorPixels;public Vector3 parentLocalCenter;public Quaternion parentLocalRotation;
  }
  bool quickCapture;int quickPhase;BoardRegistration registration;
  public bool CaptureInProgress=>quickCapture&&!Playing;
  public void BeginQuickCapture(){
   NewGame();if(StorageBlocked)return;Configure(true,false,1);quickCapture=true;quickPhase=0;SessionId=GameSessionStore.NewId();alignStage=11;Aligned=false;
   CaptureSetupHeading();Message="Look at your physical chessboard to begin setup.";SaveSession("physical capture started");DrawScene();BeginAutomaticAlignment();
  }
  void AnchorConfirmedPlane(){var experience=FindFirstObjectByType<LivingTableExperience>();if(experience!=null)_ = experience.AnchorChessCalibration();}
  public void SaveAnchoredRegistration(){if(BoardPoseLocked)RecordRegistration();}
  void RecordRegistration(){registration=new BoardRegistration{pinchedCorners=pinchedCorners.Take(cornerCount).ToArray(),cameraSnapped=cornerSnapped.ToArray(),inferredCorners=cornerInferred.ToArray(),alignmentMethod=metricFitReady?metricFit.source:"assisted",detectedCorners=metricFitReady?metricFit.corners:0,fitErrorPixels=metricFitReady?metricFit.rmsPixels:0,corners=setupCorners.Take(cornerCount).ToArray(),center=transform.position,rotation=transform.rotation,size=Size,setScale=setScale,playerColor=HumanSide,recordedUtc=DateTime.UtcNow.ToString("O"),anchorId=FindFirstObjectByType<LivingTableExperience>()?.RegisteredAnchorId,parentLocalCenter=transform.localPosition,parentLocalRotation=transform.localRotation};SaveSession("physical board registration updated");}
  void CompleteKingSizing(){
   RecordRegistration();
   if(quickCapture&&setupPosition!=null&&unknownSquares!=null&&!unknownSquares.Any(x=>x)&&turnChosen){StartObservedGameConfirmed();return;}
   if(entryPhase==4){entryPhase=3;PlayEntry();return;}
   entryWasPhysical=Physical;entryKeepPhysical=Physical;entryPhase=3;PlayEntry();
  }
  void ShowCapturedTurn(){
   int side=InferSide(setupPosition.Board);alignStage=9;turnChosen=side!=0;if(turnChosen)setupPosition.Side=side;
   Message=turnChosen?SideName(side)+" must move because it is in check. Confirm to continue virtually.":"Choose whose turn it is. The virtual AI will take the opposite color from you.";SaveSession("captured layout and geometry reviewed");DrawScene();
  }
  void ResumeCapture(SessionState state,string id){
   if(state.capturePhase<0||state.capturePhase>3||state.capturedPosition!=null&&(state.capturedPosition.Board==null||state.capturedPosition.Board.Length!=64||state.capturedPosition.Board.Any(x=>Math.Abs(x)>6)||state.unknownSquares==null||state.unknownSquares.Length!=64))throw new ArgumentException("Invalid saved capture");
   StopAllCoroutines();calibration?.Cancel();calibrationBusy=false;Vision.ClearEvidence();HomeScreen=false;Match=null;SessionId=id;quickCapture=true;quickPhase=state.capturePhase;setupPosition=state.capturedPosition;unknownSquares=state.unknownSquares;setupManual=state.captureManual;turnChosen=state.turnChosen;registration=state.registration;
   Physical=true;TwoPlayers=false;HumanSide=state.humanSide== -1?-1:1;setScale=state.setScale>0?Mathf.Clamp(state.setScale,.3f,3):1;Size=state.boardSize>=.2f&&state.boardSize<=1.2f?state.boardSize:.56f;
   CaptureSetupHeading();
   Paused=Busy=Recovering=StorageBlocked=standardMenu=sessionMenu=extraSettings=pieceSettings=false;Aligned=false;Version++;
   if(setupPosition!=null&&unknownSquares?.Length==64){quickPhase=1;alignStage=8;Message="Capture restored. Review the saved pieces, then register this physical board before starting virtual play.";}
   else{quickPhase=0;alignStage=11;Message="Capture restored. Look down at the whole physical board, then SCAN BOARD.";}
   DrawScene();
  }
 }
}
