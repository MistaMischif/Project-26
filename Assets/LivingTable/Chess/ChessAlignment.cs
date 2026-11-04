using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  public bool BoardPoseLocked {get;private set;}
  bool boardNeedsMove;
  public void UnlockBoardForMove(){BoardPoseLocked=false;boardNeedsMove=false;metricFitReady=false;if(quickCapture)quickGridConfirmed=false;}
  public void ReportBoardOutOfReach(){boardNeedsMove=true;Message="Board remains locked. Open MENU and choose MOVE BOARD to place it in your new space.";DrawUI();}
  bool metricFitReady,assistedFitAvailable;ChessSurfaceSolver.Estimate metricFit;
  float nextAlignmentPreview;ChessCameraCalibration calibration;bool calibrationBusy;float[] pieceSizes=new float[24],proposedSizes;TextMesh turnHeading;
  public bool CalibrationBusy=>calibrationBusy;
  public string TurnTitle=>relocating?"ADJUSTING BOARD":!Playing||GameOver?"":Paused?"PAUSED":Recovering||alignStage>0?"ALIGN YOUR BOARD":TwoPlayers?SideName(Match.Position.Side).ToUpperInvariant()+" TURN":Match.Position.Side==HumanSide?"YOUR TURN":"OPPONENT TURN";
  public void BeginAlignment(){if(BoardPoseLocked)FindFirstObjectByType<LivingTableExperience>()?.MoveCandyTable();else BeginAutomaticAlignment();}
  public void BeginAutomaticAlignment(){if(!Physical||Busy||relocating||BoardPoseLocked)return;ResetAlignmentMessages();metricFitReady=false;if(Playing)Recovering=true;Version++;CancelGrab();Vision.ClearEvidence();alignStage=5;Aligned=false;calibrationBusy=true;Message=AlignmentInstructions;DrawScene();calibration.FitBoard();}
  public void CalibrationFailed(string reason){calibrationBusy=false;alignmentError=reason;lastAlignmentIssue=Time.unscaledTime;DrawUI();}
  public void PreviewAutoAlignment(ChessCheckerFit.Fit fit){CalibrationFailed("A coarse pattern is not a verified surface. Retry automatic refinement or use assisted corners.");}
  public void AcceptAutoAlignment(){if(quickCapture){BeginFourCorners();return;}if(alignStage!=3||!metricFitReady)return;calibration.Lock();Aligned=true;Version++;if(Playing)Recovering=true;RecordRegistration();SaveSession("physical checker fit locked");BeginKingSizing();}
  public void CalibrationProgress(string text){if(!calibrationBusy)return;alignmentProgress=text;}
  public void PreviewMetricAlignment(ChessSurfaceSolver.Estimate fit){
   if(BoardPoseLocked||quickCapture&&(SelectingCorners||ReviewingQuickGrid||quickGridConfirmed))return;
   if(!fit.valid||fit.corners<16||!float.IsFinite(fit.rmsPixels)||fit.rmsPixels>1.25f||!float.IsFinite(fit.rmsMeters)||fit.rmsMeters>.006f||!float.IsFinite(fit.size)||fit.size<.2f||fit.size>1.2f||!float.IsFinite(fit.center.sqrMagnitude)||!float.IsFinite(fit.rotation.x+fit.rotation.y+fit.rotation.z+fit.rotation.w)){CalibrationFailed("Physical grid fit rejected; last pose held.");return;}
   if(quickCapture){calibrationBusy=false;Size=fit.size;transform.SetPositionAndRotation(fit.center,fit.rotation);BeginQuickCorners();return;}
   // Preserve piece dimensions; changing board scale does not resize the pieces.
   for(int piece=-6;piece<=6;piece++)if(piece!=0){int i=ChessCameraCalibration.ProfileIndex(piece)*2;if(pieceSizes[i]<=.02f){var scale=MeasuredPieceScale(piece);pieceSizes[i]=scale.y*new[]{.05f,.075f,.0705f,.059f,.0685f,.082f}[System.Math.Abs(piece)-1];pieceSizes[i+1]=scale.x*.038f;}}
   calibrationBusy=false;BoardPoseLocked=true;alignmentError="";alignmentProgress="Grid ready. Check all edges, then ACCEPT BOARD.";metricFitReady=true;metricFit=fit;Size=fit.size;transform.SetPositionAndRotation(fit.center,fit.rotation);alignStage=3;Aligned=false;
   Message="GRID ALIGNED — "+fit.corners+" corners verified. Check the outlined squares, then ACCEPT BOARD. White's home edge must match your chosen orientation.";DrawScene();
  }
  public void AcceptAssistedFit(){if(!assistedFitAvailable||calibrationBusy)return;BoardPoseLocked=true;calibration.Cancel();metricFitReady=false;Aligned=true;Version++;Message="ASSISTED alignment accepted. Camera geometry was not verified.";RecordRegistration();BeginKingSizing();}
  void RefineAdjustedBoard(){metricFitReady=false;assistedFitAvailable=true;if(desktop){alignStage=3;return;}BeginAutomaticAlignment();}

  void StartPieceScan(){proposedSizes=null;alignStage=4;calibrationBusy=true;Message="Measuring piece proportions. Keep pieces still and move your head slightly to one side. Uncertain sizes will stay unchanged.";DrawScene();calibration.SizePieces();}
  public void PreviewPieceSizes(float[] sizes){calibrationBusy=false;proposedSizes=sizes;int count=0;for(int i=0;i<24;i+=2)if(sizes[i]>0)count++;Message=count>0?count+" piece/color profiles measured. APPLY SIZES or keep standard proportions.":"No consistent piece measurements. Retry from a clearer angle or keep standard sizes.";DrawMen();DrawUI();}
  void FinishPieceScan(bool apply){calibration.Cancel();calibrationBusy=false;if(apply&&proposedSizes!=null)for(int i=0;i<24;i+=2)if(proposedSizes[i]>0){pieceSizes[i]=proposedSizes[i];pieceSizes[i+1]=proposedSizes[i+1];}proposedSizes=null;alignStage=0;Aligned=true;Version++;Vision.ClearEvidence();if(Playing)Recovering=true;Message="Board accepted. Piece sizing complete. Verify the real position before play.";SaveSession("physical geometry accepted");DrawScene();}
  Vector3 PieceScale(int piece)=>MeasuredPieceScale(piece)*setScale;
  Vector3 MeasuredPieceScale(int piece){int i=ChessCameraCalibration.ProfileIndex(piece)*2;var sizes=alignStage==4&&proposedSizes!=null&&proposedSizes[i]>0?proposedSizes:pieceSizes;if(sizes!=null&&sizes.Length==24&&sizes[i]>.02f&&sizes[i+1]>.01f){float height=new[]{.05f,.075f,.0705f,.059f,.0685f,.082f}[System.Math.Abs(piece)-1];return new Vector3(Mathf.Clamp(sizes[i+1]/.038f,.3f,3),Mathf.Clamp(sizes[i]/height,.3f,3),Mathf.Clamp(sizes[i+1]/.038f,.3f,3));}return Vector3.one*(Size/.56f);}
  bool CalibrationUI(){if(alignStage<3||alignStage>5)return false;if(!BoardPoseLocked)Add("MANUAL CORNERS",-2,1,BeginFourCorners,Teal);else Add("MOVE BOARD",-2,1,()=>FindFirstObjectByType<LivingTableExperience>()?.MoveCandyTable(),Teal);if(alignStage==3){if(metricFitReady)Add("ACCEPT BOARD",0,0,AcceptAutoAlignment,Teal,true);else Add("REFINE GRID",0,0,BeginAutomaticAlignment,Purple,true);if(!BoardPoseLocked)Add("ROTATE 90",2,0,()=>{if(BoardPoseLocked){Message="Use MOVE BOARD before changing grid orientation.";return;}transform.Rotate(0,90,0,Space.World);if(metricFitReady)metricFit.rotation=transform.rotation;DrawScene();},Purple);}else if(alignStage==4){if(!calibrationBusy){if(proposedSizes!=null&&System.Array.Exists(proposedSizes,value=>value>0))Add("APPLY SIZES",0,0,()=>FinishPieceScan(true),Teal,true);Add("RESCAN PIECES",2,0,StartPieceScan,Teal);}Add("KEEP SIZES",-2,0,()=>FinishPieceScan(false),Orange);}else if(!calibrationBusy)Add("RETRY SCAN",0,0,BeginAutomaticAlignment,Teal,true);if(calibrationBusy)Add("CANCEL",-1,0,()=>{calibration.Cancel();CalibrationFailed("Alignment cancelled. Last pose held; retry or use assisted corners.");},Teal);else if(assistedFitAvailable&&!metricFitReady)Add("ASSISTED FIT",-1,1,AcceptAssistedFit,Orange);return true;}

  public bool AlignmentInProgress=>alignStage>0;
  public void BeginManualAlignment()=>BeginFourCorners();
  void DrawAlignmentGuide(){
   if(alignStage==11||quickCapture&&alignStage==5)return;
   if(!relocating&&(!Physical||Playing&&!Recovering&&alignStage==0&&!BoardPoseLocked))return;
   var guide=new GameObject("Alignment grid").transform;guide.SetParent(board,false);
   if(quickCapture&&(SelectingCorners||ReviewingQuickGrid))DrawQuickCornerMarkers(guide);
   else if(SelectingCorners){for(int i=0;i<cornerCount;i++){var point=transform.InverseTransformPoint(setupCorners[i]);Shape(guide,"Selected playing corner "+(i+1),PrimitiveType.Sphere,point,Vector3.one*.018f,gold);}return;}
   for(int i=0;i<=8;i++){float at=-Size*.5f+i*Size/8;Shape(guide,"Grid file "+i,PrimitiveType.Cube,new Vector3(at,relocating?.012f:.0002f,0),new Vector3(relocating?.004f:.0015f,.0004f,Size),relocating?adjustGrid:gold);Shape(guide,"Grid rank "+i,PrimitiveType.Cube,new Vector3(0,relocating?.012f:.0002f,at),new Vector3(Size,.0004f,relocating?.004f:.0015f),relocating?adjustGrid:gold);}
   if(quickCapture&&(SelectingCorners||ReviewingQuickGrid)){
    for(int level=0;level<5;level++){
     float y=level*.035f;float span=level==0?.105f:.054f;
     Shape(guide,"Center plus horizontal "+level,PrimitiveType.Cube,new Vector3(0,y,0),new Vector3(span,.0015f,.0025f),blue);
     Shape(guide,"Center plus vertical "+level,PrimitiveType.Cube,new Vector3(0,y,0),new Vector3(.0025f,.0015f,span),blue);
    }
   }
   if(quickCapture&&(SelectingCorners||ReviewingQuickGrid)){viewGuidePrompt=Label(guide,"Line up the blue + centers, then hold still.",new Vector3(0,.17f,-.15f),.002f);viewGuidePrompt.color=Color.cyan;}else viewGuidePrompt=null;
   if(relocating)return;
  }
  TextMesh viewGuidePrompt;float viewAlignedSince=-1;bool viewAttempted;
  public static bool ViewAlignedOverBoard(Vector3 eye,Vector3 look,Vector3 center,Vector3 normal){
   var offset=eye-center;float height=Vector3.Dot(offset,normal);return height>.2f&&Vector3.ProjectOnPlane(offset,normal).magnitude<=height*Mathf.Tan(5*Mathf.Deg2Rad)&&Vector3.Angle(look,-normal)<=8;
  }
  void UpdateAlignmentViewGuide(){
   if(viewGuidePrompt==null||Camera.main==null||!quickCapture||!(SelectingCorners||ReviewingQuickGrid))return;
   var eye=Camera.main.transform;bool aligned=ViewAlignedOverBoard(eye.position,eye.forward,transform.position,transform.up);
   if(!aligned){viewAlignedSince=-1;viewAttempted=false;}else if(viewAlignedSince<0)viewAlignedSince=Time.unscaledTime;
   foreach(var child in board.GetComponentsInChildren<Renderer>())if(child.name.StartsWith("Center plus "))child.sharedMaterial=aligned?green:blue;
   viewGuidePrompt.transform.rotation=eye.rotation;viewGuidePrompt.color=aligned?Color.green:Color.cyan;
   viewGuidePrompt.text=!aligned?"Line up the blue + centers. Hold that view.":cornerCount<4?"View aligned. Place all four corners.":cornerSnapBusy?"View aligned. Refining grid...":viewAttempted?"View aligned. Check edges, then ALIGN.":"View aligned. Hold still...";
   if(aligned&&cornerCount==4&&cornerSnapBusy)viewAttempted=true;
   if(aligned&&cornerCount==4&&!viewAttempted&&!cornerSnapBusy&&HandsClear&&Time.unscaledTime-viewAlignedSince>=.35f){viewAttempted=true;RequestCornerSnapping();}
  }
  bool AlignmentPose(Vector3 point){var edge=point-firstCorner;edge.y=0;if(edge.magnitude<.2f||edge.magnitude>1.2f||Mathf.Abs(point.y-firstCorner.y)>.12f)return false;Size=edge.magnitude;var forward=Vector3.Cross(edge.normalized,Vector3.up);transform.SetPositionAndRotation(firstCorner+edge*.5f+forward*Size*.5f,Quaternion.LookRotation(forward));return true;}
  void PreviewAlignment(Vector3 world){if(alignStage!=2||Time.unscaledTime<nextAlignmentPreview)return;nextAlignmentPreview=Time.unscaledTime+.12f;if(AlignmentPose(world))DrawScene();}
  void AcceptAlignmentPoint(Vector3 world){if(alignStage==1){firstCorner=world;alignStage=2;Message="First corner fixed. Move to near-right PLAYING corner: grid and pieces resize live. Pinch to set.";return;}if(!AlignmentPose(world)){Message="Use the other near playing corner: 20–120 cm across, on the same table.";return;}Aligned=true;alignStage=0;Vision.ClearEvidence();Message="Check the grid and guide pieces. START GAME begins. Cyan = White; amber = Black.";if(Playing){Recovering=true;SaveSession("board realigned");}DrawScene();}
 }
}
