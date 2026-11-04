using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessCameraCalibration {
  ChessSurfaceSolver.Estimate snapReference;
  public void SnapCorners(ChessSurfaceSolver.Estimate manualSeed,Action<ChessSurfaceSolver.Estimate,int,string> done){Cancel();StartCoroutine(CheckCorners(manualSeed,done));}
  IEnumerator CheckCorners(ChessSurfaceSolver.Estimate seed,Action<ChessSurfaceSolver.Estimate,int,string> done){
   var fit=snapReference.valid?snapReference:seed;
   if(!fit.valid){done(default,0,"Place all four corners to define the board before camera verification.");yield break;}
   string failure="Playing edges unclear. Keep all four borders visible, lift hands away, then retry snapping or confirm manually.";
   int stable=0,edgeMask=15;
   for(int attempt=0;attempt<3;attempt++){
    ChessCheckerFit.Frame frame=null;yield return Capture(f=>frame=f);
    if(frame==null){failure="Camera unavailable or hands obscuring board. Retry snapping or confirm markers manually.";break;}
    var search=Task.Run(()=>{var corners=ChessSurfaceSolver.Detect(frame,new ChessCheckerFit.Fit{valid=true,center=fit.center,size=fit.size,yaw=fit.rotation.eulerAngles.y});return ChessSurfaceSolver.PlayableEdgeMask(fit,corners);});
    float deadline=Time.unscaledTime+4;while(!search.IsCompleted&&Time.unscaledTime<deadline)yield return null;
    if(!search.IsCompleted||search.IsFaulted||(search.Result&3)==0||(search.Result&12)==0||!table.HandsClear||table.LastHandAt>frame.capturedAt)break;
    edgeMask&=search.Result;stable++;yield return new WaitForSecondsRealtime(.15f);
   }
   if(cameraAccess!=null)cameraAccess.enabled=false;
   // Require consistent support in both axes. Missing edges can be inferred from four user markers.
   if(stable==3&&(edgeMask&3)!=0&&(edgeMask&12)!=0)done(fit,snapReference.valid?edgeMask:0,"");else done(default,0,failure);
  }
 }
}
