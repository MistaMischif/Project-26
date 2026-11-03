using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  float nextAlignmentPreview;ChessCameraCalibration calibration;bool calibrationBusy;float[] pieceSizes=new float[24],proposedSizes;TextMesh turnHeading;
  public bool CalibrationBusy=>calibrationBusy;
  public string TurnTitle=>relocating?"ADJUSTING BOARD":!Playing||GameOver?"":Paused?"PAUSED":Recovering||alignStage>0?"ALIGN YOUR BOARD":TwoPlayers?SideName(Match.Position.Side).ToUpperInvariant()+" TURN":Match.Position.Side==HumanSide?"YOUR TURN":"OPPONENT TURN";
  public void BeginAlignment(){if(!Physical||Busy)return;Version++;CancelGrab();Vision.ClearEvidence();alignStage=5;Aligned=false;calibrationBusy=true;Message="Finding the checker pattern. Look down at the whole board and clear hands. Finding the tabletop depth and checker pattern automatically.";DrawScene();calibration.FitBoard();}
  public void CalibrationFailed(string reason){calibrationBusy=false;Message=reason;DrawUI();}
  public void PreviewAutoAlignment(ChessCheckerFit.Fit fit){calibrationBusy=false;Size=fit.size;transform.SetPositionAndRotation(fit.center,Quaternion.Euler(0,fit.yaw,0));alignStage=3;Message="Check all four corners. Rotate if needed, then ACCEPT BOARD. No game move has changed.";DrawScene();}
  public void AcceptAutoAlignment(){if(alignStage!=3)return;Aligned=true;Version++;if(Playing)Recovering=true;SaveSession("automatic board fit accepted");StartPieceScan();}
  void StartPieceScan(){proposedSizes=null;alignStage=4;calibrationBusy=true;Message="Measuring piece proportions. Keep pieces still and move your head slightly to one side. Uncertain sizes will stay unchanged.";DrawScene();calibration.SizePieces();}
  public void PreviewPieceSizes(float[] sizes){calibrationBusy=false;proposedSizes=sizes;int count=0;for(int i=0;i<24;i+=2)if(sizes[i]>0)count++;Message=count>0?count+" piece/color profiles measured. APPLY SIZES or keep standard proportions.":"No consistent piece measurements. Retry from a clearer angle or keep standard sizes.";DrawMen();DrawUI();}
  void FinishPieceScan(bool apply){calibration.Cancel();calibrationBusy=false;if(apply&&proposedSizes!=null)for(int i=0;i<24;i+=2)if(proposedSizes[i]>0){pieceSizes[i]=proposedSizes[i];pieceSizes[i+1]=proposedSizes[i+1];}proposedSizes=null;alignStage=0;Aligned=true;Version++;Vision.ClearEvidence();if(Playing)Recovering=true;Message="Board accepted. Piece sizing complete. Verify the real position before play.";SaveSession("physical geometry accepted");DrawScene();}
  Vector3 PieceScale(int piece)=>MeasuredPieceScale(piece)*setScale;
  Vector3 MeasuredPieceScale(int piece){int i=ChessCameraCalibration.ProfileIndex(piece)*2;var sizes=alignStage==4&&proposedSizes!=null&&proposedSizes[i]>0?proposedSizes:pieceSizes;if(sizes!=null&&sizes.Length==24&&sizes[i]>.02f&&sizes[i+1]>.01f){float height=new[]{.05f,.075f,.0705f,.059f,.0685f,.082f}[System.Math.Abs(piece)-1];return new Vector3(Mathf.Clamp(sizes[i+1]/.038f,.3f,3),Mathf.Clamp(sizes[i]/height,.3f,3),Mathf.Clamp(sizes[i+1]/.038f,.3f,3));}return Vector3.one*(Size/.56f);}
  bool CalibrationUI(){if(alignStage<3)return false;if(alignStage==3){Add("ACCEPT BOARD",0,0,AcceptAutoAlignment,Teal,true);Add("ROTATE 90",2,0,()=>{transform.Rotate(0,90,0,Space.World);DrawScene();},Purple);}else if(alignStage==4){if(!calibrationBusy){if(proposedSizes!=null&&System.Array.Exists(proposedSizes,value=>value>0))Add("APPLY SIZES",0,0,()=>FinishPieceScan(true),Teal,true);Add("RESCAN PIECES",2,0,StartPieceScan,Teal);}Add("KEEP SIZES",-2,0,()=>FinishPieceScan(false),Orange);}else if(!calibrationBusy)Add("RETRY SCAN",0,0,BeginAlignment,Teal,true);return true;}

  public bool AlignmentInProgress=>alignStage>0;
  public void BeginManualAlignment(){calibration?.Cancel();calibrationBusy=false;Version++;CancelGrab();Vision.ClearEvidence();alignStage=1;Aligned=false;Message="Match the grid to the PLAYING squares, not the frame. Pinch near-left, then near-right. Cyan = White; amber = Black.";DrawScene();}
  void DrawAlignmentGuide(){
   if(!relocating&&(!Physical||Playing&&!Recovering&&alignStage==0))return;
   var guide=new GameObject("Alignment grid").transform;guide.SetParent(board,false);
   for(int i=0;i<=8;i++){float at=-Size*.5f+i*Size/8;Shape(guide,"Grid file "+i,PrimitiveType.Cube,new Vector3(at,relocating?.012f:.004f,0),new Vector3(relocating?.004f:.0015f,.0015f,Size),relocating?adjustGrid:gold);Shape(guide,"Grid rank "+i,PrimitiveType.Cube,new Vector3(0,relocating?.012f:.004f,at),new Vector3(Size,.0015f,relocating?.004f:.0015f),relocating?adjustGrid:gold);}
   if(relocating)return;
  }
  bool AlignmentPose(Vector3 point){var edge=point-firstCorner;edge.y=0;if(edge.magnitude<.2f||edge.magnitude>1.2f||Mathf.Abs(point.y-firstCorner.y)>.12f)return false;Size=edge.magnitude;var forward=Vector3.Cross(edge.normalized,Vector3.up);transform.SetPositionAndRotation(firstCorner+edge*.5f+forward*Size*.5f,Quaternion.LookRotation(forward));return true;}
  void PreviewAlignment(Vector3 world){if(alignStage!=2||Time.unscaledTime<nextAlignmentPreview)return;nextAlignmentPreview=Time.unscaledTime+.12f;if(AlignmentPose(world))DrawScene();}
  void AcceptAlignmentPoint(Vector3 world){if(alignStage==1){firstCorner=world;alignStage=2;Message="First corner fixed. Move to near-right PLAYING corner: grid and pieces resize live. Pinch to set.";return;}if(!AlignmentPose(world)){Message="Use the other near playing corner: 20–120 cm across, on the same table.";return;}Aligned=true;alignStage=0;Vision.ClearEvidence();Message="Check the grid and guide pieces. START GAME begins. Cyan = White; amber = Black.";if(Playing){Recovering=true;SaveSession("board realigned");}DrawScene();}
 }
}
