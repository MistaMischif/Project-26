using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
namespace LivingTable.Chess {
 public sealed partial class ChessPhysicalVision {
  bool destinationRequested,destinationBusy;ChessMove destinationMove;float destinationDeadline;
  public bool DestinationChecking=>destinationRequested||destinationBusy;
  public void CheckDestination(ChessMove move){
   if(destinationRequested&&destinationMove.Equals(move))return;
   StopAllCoroutines();meta.Cancel();generation++;pending=requested=scanning=false;approvedView=null;approved=null;stable.Reset();
   destinationMove=move;destinationRequested=true;destinationBusy=false;suspended=false;next=0;
   Status="Checking only "+ChessMove.Square(move.To)+". Place the physical piece there and lift your hand.";
  }
  public static bool? ReadPresence(ChessObservationGate.Observation observation,int revision,int square){
   if(observation==null||observation.revision!=revision||observation.cells==null||observation.cells.Length!=1||observation.cells[0]==null||observation.cells[0].square!=ChessMove.Square(square))return null;
   int piece=ParsePiece(observation.cells[0].piece);return piece==99?(bool?)null:piece!=0;
  }
  void CheckDestinationFrame(Texture source,Pose pose){
   if(destinationBusy||meta.Busy)return;
   var move=table.PendingAi??destinationMove;
   if(!destinationRequested){destinationRequested=true;destinationMove=move;}
   if(!meta.Configured){Status="Destination check unavailable. Restore the vision connection or use manual controls.";next=Time.unscaledTime+2;return;}
   pending=destinationBusy=true;destinationDeadline=Time.unscaledTime+25;
   int revision=table.Version,session=generation,w=source.width,h=source.height;double captured=Time.realtimeSinceStartupAsDouble;
   AsyncGPUReadback.Request(source,0,TextureFormat.RGBA32,readback=>{
    if(this==null||session!=generation)return;pending=false;
    if(!Current(revision,session,captured)||readback.hasError){destinationBusy=false;next=Time.unscaledTime+1;return;}
    var crop=CropSquare(move.To,readback.GetData<Color32>(),w,h,pose);
    if(crop==null){destinationBusy=false;Status="Keep destination "+ChessMove.Square(move.To)+" visible. No whole-board scan is needed.";next=Time.unscaledTime+1;return;}
    Status="Is "+ChessMove.Square(move.To)+" occupied? Checking this square only.";
    StartCoroutine(ObserveDestination(move,revision,session,captured,crop));
   });
  }
  IEnumerator ObserveDestination(ChessMove move,int revision,int session,double captured,ChessMetaVision.Crop crop){
   ChessObservationGate.Observation result=null;
   yield return meta.Observe(revision,new[]{crop},value=>result=value);
   if(this==null||session!=generation)yield break;
   destinationBusy=false;next=Time.unscaledTime+1;
   if(!Current(revision,session,captured)){Status="Hand moved during the check. Lift it; retrying the destination.";yield break;}
   bool? occupied=ReadPresence(result,revision,move.To);
   Status=occupied==true?"Occupied — continuing.":occupied==false?ChessMove.Square(move.To)+" is empty. Place the physical piece on the highlighted square.":"Cannot determine occupancy of "+ChessMove.Square(move.To)+". Clear the view; retrying.";
   table.AcceptDestinationPresence(move,revision,occupied);
  }
 }
}
