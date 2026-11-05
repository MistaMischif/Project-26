using System;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  readonly Vector3[] setupCorners=new Vector3[4],pinchedCorners=new Vector3[4];int cornerCount,kingSquare=4,setupSelected=-1;bool standardMenu,setupManual,turnChosen;float panelScale=1;
  ChessPosition setupPosition;bool[] unknownSquares;Transform panelSlider,sliderKnob;int setupHand=int.MinValue;bool draggingPanel;float setupGrabY,setupGrabScale,setupSeen;
  public bool SelectingCorners=>alignStage==1;
  public bool SizingKing=>alignStage==7;
  public bool SetupReviewing=>alignStage>=8&&alignStage<=10;
  public bool StandardMenuOpen=>standardMenu;
  public static bool FitCorners(Vector3[] corners,out Vector3 center,out Quaternion rotation,out float size){
   center=Vector3.zero;rotation=Quaternion.identity;size=0;if(corners==null||corners.Length!=4)return false;
   foreach(var p in corners)center+=p*.25f;
   var right=(corners[1]-corners[0]+corners[2]-corners[3])*.5f;var forward=(corners[3]-corners[0]+corners[2]-corners[1])*.5f;
   if(right.magnitude<.2f||right.magnitude>1.2f||forward.magnitude<.2f||forward.magnitude>1.2f||Mathf.Abs(Vector3.Dot(right.normalized,forward.normalized))>.12f)return false;
   var normal=Vector3.Cross(forward,right).normalized;if(normal.y<.9f)return false;size=(right.magnitude+forward.magnitude)*.5f;
   if(Mathf.Abs(right.magnitude-forward.magnitude)>size*.12f)return false;
   rotation=Quaternion.LookRotation(Vector3.Cross(right,normal).normalized,normal);
   for(int i=0;i<4;i++){var expected=center+rotation*new Vector3(i==0||i==3?-size*.5f:size*.5f,0,i<2?-size*.5f:size*.5f);if(Vector3.Distance(expected,corners[i])>Mathf.Max(.015f,size*.05f))return false;}
   return true;
  }
  public void BeginFourCorners(){
   if(BoardPoseLocked){Message="Grid locked. Use MOVE BOARD before adjusting corners.";return;}
   if(quickCapture){BeginQuickCorners();return;}
   metricFitReady=false;assistedFitAvailable=false;
   if(quickCapture&&quickPhase==0)quickPhase=2;
   calibration?.Cancel();calibrationBusy=false;Vision.CancelScan();Vision.ClearProgress();CancelHumanReview();Version++;CancelGrab();HomeScreen=false;standardMenu=extraSettings=pieceSettings=sessionMenu=false;cornerCount=0;alignStage=1;Aligned=false;
   Message="1 / 4: pinch the NEAR LEFT outer corner of the 8 x 8 playing squares. Exclude the frame. Sit behind your chosen color.";DrawScene();
  }
  public bool SelectBoardCorner(Vector3 point){
   if(!SelectingCorners||cornerCount>=4)return false;if(cornerCount>0&&Vector3.Distance(point,setupCorners[cornerCount-1])<.12f){Message="Choose the next corner, not the same point.";return false;}
   setupCorners[cornerCount]=pinchedCorners[cornerCount]=point;cornerSnapped[cornerCount]=cornerInferred[cornerCount]=false;cornerCount++;LockPassingCorners();PreviewMarkedSquare();
   if(cornerCount<4){Message=(cornerCount+1)+" / 4: pinch the "+new[]{"NEAR LEFT","NEAR RIGHT","FAR RIGHT","FAR LEFT"}[cornerCount]+" outer playing corner."+(quickCapture?" "+ManualCornerNotice:"");DrawScene();if(quickCapture)RequestCornerSnapping();return true;}
   if(quickCapture){bool valid=FinishQuickCorners();RequestCornerSnapping();return valid;}
   if(!FitCorners(setupCorners,out var center,out var rotation,out var size)){cornerCount=0;Message="Corners did not form a flat square. Start again at NEAR LEFT. MENU is always available.";DrawScene();return false;}
   Size=size;transform.SetPositionAndRotation(center,rotation);Aligned=true;alignStage=7;pieceSizes=new float[24];kingSquare=HumanSide>0?4:60;
   RecordRegistration();Message="Checking your selected board against the physical grid.";DrawScene();if(!desktop)RefineAdjustedBoard();return true;
  }
  void BeginKingSizing(){calibration?.Cancel();calibrationBusy=false;alignStage=7;Aligned=true;kingSquare=HumanSide>0?4:60;Message="Place the guide on your king's square, then pinch/pull its top to match size. Press PLAY when the size matches.";DrawScene();}
  public void BeginPositionScan(){
   if(quickCapture&&!quickGridConfirmed){BeginFourCorners();return;}
   if(!Physical||!Aligned){BeginFourCorners();return;}standardMenu=extraSettings=pieceSettings=sessionMenu=false;
   if(Playing){alignStage=0;Recovering=true;Vision.CheckNow();DrawScene();return;}
   setupPosition=new ChessPosition{Board=new int[64],Rights=0,Ep=-1};unknownSquares=Enumerable.Repeat(true,64).ToArray();setupManual=false;alignStage=6;Message="Reading all 64 physical squares. Keep hands clear. CANCEL or MENU remains available.";DrawScene();Vision.BeginLayoutScan();
  }
  public void ReceiveLayout(int[] pieces,string note){
   if(Playing||alignStage!=6)return;setupPosition=new ChessPosition{Board=new int[64],Rights=0,Ep=-1};unknownSquares=new bool[64];
   for(int i=0;i<64;i++){unknownSquares[i]=pieces==null||pieces.Length!=64||pieces[i]==99;setupPosition.Board[i]=unknownSquares[i]?0:pieces[i];}
   alignStage=8;setupSelected=-1;turnChosen=false;AutoSeatCapturedLayout();SaveSession("physical board capture observed");Message=note+" Review every virtual piece. Tap a square to correct it; unknown squares must be resolved.";DrawScene();
  }
  public static int InferSide(int[] pieces){
   bool white=LegalLayout(pieces,1),black=LegalLayout(pieces,-1);return white==black?0:white?1:-1;
  }
  static bool LegalLayout(int[] pieces,int side){try{ChessMatch.ValidatePosition(new ChessPosition{Board=(int[])pieces.Clone(),Side=side,Rights=0,Ep=-1});return true;}catch{return false;}}
  void ConfirmLayout(){
   if(unknownSquares.Any(x=>x)){Message="Resolve every unknown square, or rescan. Tap a square, then set its piece.";return;}
   int side=setupPosition.Board.SequenceEqual(ChessPosition.Initial().Board)?1:InferSide(setupPosition.Board);if(side==0&&!LegalLayout(setupPosition.Board,1)&&!LegalLayout(setupPosition.Board,-1)){Message="This layout is not a legal position. Check kings, pawns, colors and selected squares.";return;}
   if(quickCapture&&quickPhase==1){quickPhase=2;SaveSession("captured position validated");ShowCapturedTurn();return;}
   alignStage=9;turnChosen=side!=0;if(turnChosen)setupPosition.Side=side;Message=turnChosen?SideName(side)+" to move. Confirm before continuing.":"A piece layout cannot reveal whose turn it is. Choose WHITE or BLACK to move.";DrawUI();
  }
  static string RightsName(int bits)=>new[]{"NONE","KING","QUEEN","BOTH"}[bits];
  void ConfirmTurn(){if(!turnChosen){Message="Choose whose turn it is first.";return;}if(quickCapture&&quickPhase==2&&Aligned){RequestVirtualSafety(()=>{quickPhase=3;BeginKingSizing();});return;}alignStage=10;Message="Setup ready. Confirm known castling rights, then press the purple START GAME button. Earlier history is unknown.";DrawUI();}
  void StartObservedGame(){if(quickCapture&&!entryKeepPhysical){RequestVirtualSafety(StartObservedGameConfirmed);return;}StartObservedGameConfirmed();}
  void StartObservedGameConfirmed(){
   if(quickCapture&&!quickGridConfirmed){Message="Confirm all four corners and the grid before starting.";return;}
   if(setupPosition==null||!turnChosen||unknownSquares.Any(x=>x))return;bool verified=!setupManual&&Vision.SurveyStable;var appearance=Vision.SurveyAppearance;
   if(!quickCapture&&!verified&&!setupManual){Message="The board moved or the camera lost its view. Press RESCAN to verify it again.";return;}
   var position=setupPosition.Copy();bool capturedVirtual=quickCapture&&!entryKeepPhysical;if(capturedVirtual){Physical=false;TwoPlayers=false;}alignStage=0;if(!ImportPosition(position)){if(capturedVirtual)Physical=true;alignStage=10;DrawUI();return;}
   if(capturedVirtual){Recovering=false;ManualFallback=false;quickCapture=false;quickPhase=0;}else if(verified){Vision.RestoreAppearance(appearance);Recovering=false;ManualFallback=false;}else if(setupManual){Recovering=false;ManualFallback=true;}
   if(verified)Vision.EnrollConfirmedPieces();quickCapture=false;quickPhase=0;setupPosition=null;unknownSquares=null;if(capturedVirtual&&!SaveCaptureOrigin())return;SaveSession(verified?"observed physical position imported":"manually reviewed physical position imported");DrawScene();
  }
  void CorrectSelectedPiece(){if(setupSelected<0){Message="Tap the square you want to correct first.";return;}int value=setupPosition.Board[setupSelected];setupPosition.Board[setupSelected]=value>=6?-6:value+1;unknownSquares[setupSelected]=false;setupManual=true;Message=ChessMove.Square(setupSelected)+": "+ChessObservationGate.Piece(setupPosition.Board[setupSelected]);DrawMen();DrawUI();}
  void CancelSetupScan(){Vision.CancelScan();if(!Playing&&alignStage==6)ReceiveLayout(null,"Scan stopped. Rescan or enter the position manually.");else{Message="Scan stopped. Your saved game is unchanged.";DrawUI();}}
  public void OpenStandardMenu(){
   if(VirtualSafetyOpen){CancelVirtualSafety();return;}
   if(standardMenu){CloseStandardMenu();return;}Version++;CancelGrab();CancelHumanReview();Vision.CancelScan();calibration?.Cancel();calibrationBusy=false;if(alignStage==6)ReceiveLayout(null,"Scan stopped by menu.");standardMenu=true;DrawUI();
  }
  void CloseStandardMenu(){standardMenu=false;if(Physical&&Playing&&!AlignmentInProgress)Vision.ResumeTracking();DrawUI();}
  void MenuHome(){standardMenu=false;relocating=false;CancelSetupScan();ShowHome();}
  void QuitGame(){if(!SaveSession("quit from menu"))return;Vision.CancelScan();calibration?.Cancel();Application.Quit();}
  bool SetupUI(){
   if(DrawVirtualSafety())return true;
   if(standardMenu){status.text="MENU\nReturn, resize the panel, or leave the table.";Add("BACK",0,0,CloseStandardMenu,Teal,true);Add("MAIN MENU",-2,0,MenuHome,Teal);Add("QUIT",-1,0,QuitGame,Teal);Add("STOP SCAN",-2,1,CancelSetupScan,Teal);Add("ALIGN",-1,1,MenuAlign,Teal);DrawPanelSlider();DrawOpacitySlider();if(!gameObject.activeInHierarchy){int index=0;foreach(var b in buttons){b.root.SetParent(panel.transform,false);b.root.localPosition=new Vector3((index%3-1)*.10f,-.01f-(index/3)*.07f,-.03f);b.root.localRotation=Quaternion.Euler(-90,0,0);index++;}}return true;}
   if(QuickCornerUI())return true;
   if(alignStage==11){Add("SCAN BOARD",0,0,BeginAutomaticAlignment,Purple,true);Add("CHOOSE CORNERS",-2,0,BeginFourCorners,Teal);Add("HOME",-1,0,ShowHome,Teal);return true;}
   if(alignStage==1){Add("SELECT CORNER "+(cornerCount+1),0,0,()=>{Message="Pinch the "+new[]{"NEAR LEFT","NEAR RIGHT","FAR RIGHT","FAR LEFT"}[cornerCount]+" outer playing corner.";},Purple,true);Add("RESTART",-2,0,BeginFourCorners,Teal);if(cornerCount>0)Add("UNDO CORNER",-2,0,()=>{cornerCount--;Message="Select corner "+(cornerCount+1)+" again.";DrawScene();},Teal);return true;}
   if(alignStage==7){Add("PLAY",0,0,CompleteKingSizing,Purple,true);Add("KING +",-2,0,()=>ResizePieceSet(1.08f),Teal);Add("KING -",-1,0,()=>ResizePieceSet(1/1.08f),Teal);return true;}
   if(alignStage==6){Message=Vision.Status;Add("CANCEL SCAN",0,0,CancelSetupScan,Teal,true);return true;}
   if(alignStage==8){Add(quickCapture&&quickPhase==1?"ACCEPT POSITION":"CONFIRM LAYOUT",0,0,ConfirmLayout,Teal,true);Add("RESCAN",-2,0,BeginPositionScan,Teal);Add("SET PIECE",-1,0,CorrectSelectedPiece,Teal);Add("SET EMPTY",-2,1,()=>{if(setupSelected<0)return;setupPosition.Board[setupSelected]=0;unknownSquares[setupSelected]=false;setupManual=true;DrawMen();},Teal);Add("STANDARD SET",-1,1,()=>{setupPosition=ChessPosition.Initial();setupPosition.Rights=0;unknownSquares=new bool[64];setupManual=true;Message="Manual standard layout selected. Check every piece before confirming.";DrawMen();},Teal);return true;}
   if(alignStage==9){Add("CONFIRM TURN",0,0,ConfirmTurn,Teal,true);foreach(int side in new[]{1,-1}){int chosen=side;Add(SideName(side)+" TO MOVE",-2,0,()=>{if(!LegalLayout(setupPosition.Board,chosen)){Message="That side is inconsistent with check. Review the layout.";return;}setupPosition.Side=chosen;turnChosen=true;Message=SideName(chosen)+" to move.";},Teal);}Add("REVIEW PIECES",-2,1,()=>{alignStage=8;DrawUI();},Teal);return true;}
   if(alignStage==10){Add("START GAME",0,0,StartObservedGame,Purple,true);Add("WHITE CASTLE "+RightsName(setupPosition.Rights&3),-2,0,()=>{setupPosition.Rights=(setupPosition.Rights&12)|((setupPosition.Rights+1)&3);DrawUI();},Teal);Add("BLACK CASTLE "+RightsName((setupPosition.Rights>>2)&3),-1,0,()=>{setupPosition.Rights=(setupPosition.Rights&3)|((((setupPosition.Rights>>2)+1)&3)<<2);DrawUI();},Teal);Add("EP "+(setupPosition.Ep<0?"NONE":ChessMove.Square(setupPosition.Ep)),-2,1,()=>{int file=setupPosition.Ep<0?0:setupPosition.Ep%8+1;setupPosition.Ep=file>7?-1:(setupPosition.Side==1?40:16)+file;DrawUI();},Teal);Add("RESCAN",-1,1,BeginPositionScan,Teal);return true;}
   return false;
  }
  public bool BeginSetupGrab(int hand,Vector3 world){
   if(!standardMenu&&GrabQuickCorner(hand,world))return true;
   if(standardMenu&&OpacityPoint(world,out float opacity)){setupHand=hand;draggingOpacity=true;draggingPanel=false;setupSeen=Time.unscaledTime;SetPieceOpacity(opacity);return true;}
   if(standardMenu&&PanelSliderPoint(world,out float value)){setupHand=hand;draggingPanel=true;setupSeen=Time.unscaledTime;SetPanelScale(value);return true;}
   if(!SizingKing||figures[kingSquare]==null)return false;var p=transform.InverseTransformPoint(world);var basePoint=SquareLocal(kingSquare);if(Vector2.Distance(new Vector2(p.x,p.z),new Vector2(basePoint.x,basePoint.z))>Size/8*.55f||p.y<.02f||p.y>.35f)return false;
   setupHand=hand;draggingPanel=false;setupSeen=Time.unscaledTime;setupGrabY=p.y;setupGrabScale=setScale;return true;
  }
  public bool UpdateSetupGrab(int hand,Vector3 world,bool release){if(setupHand!=hand)return false;setupSeen=Time.unscaledTime;if(editingSetupCorner>=0){setupCorners[editingSetupCorner]=pinchedCorners[editingSetupCorner]=world;UpdateCornerVisuals();if(release){editingSetupCorner=-1;setupHand=int.MinValue;if(cornerCount==4)FinishQuickCorners();else {PreviewMarkedSquare();DrawScene();}RequestCornerSnapping();}return true;}if(draggingOpacity){if(OpacityPoint(world,out float opacity,true))SetPieceOpacity(opacity);}else if(draggingPanel){if(PanelSliderPoint(world,out float value,true))SetPanelScale(value);}else{setScale=Mathf.Clamp(setupGrabScale+(transform.InverseTransformPoint(world).y-setupGrabY)/(.082f*Size/.56f),.3f,3);for(int s=0;s<64;s++)if(figures[s]!=null)figures[s].localScale=PieceScale(HumanSide*6);}if(release)EndSetupGrab();return true;}
  void EndSetupGrab(){if(setupHand==int.MinValue)return;setupHand=int.MinValue;draggingOpacity=false;if(editingSetupCorner>=0){editingSetupCorner=-1;quickGridConfirmed=false;Message="Corner adjustment interrupted. Review all markers before confirming.";DrawScene();return;}PlayerPrefs.SetFloat("LivingTable.PieceSetScale",setScale);PlayerPrefs.SetFloat("LivingTable.PanelScale",panelScale);PlayerPrefs.Save();SaveSession("manual sizing changed");}
 }
}
