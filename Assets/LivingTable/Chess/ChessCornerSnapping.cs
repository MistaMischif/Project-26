using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessCameraCalibration {
  ChessSurfaceSolver.Estimate snapReference;
  public void ResetCornerReference(){snapReference=default;}
  public void SnapCorners(ChessSurfaceSolver.Estimate manualSeed,Action<ChessSurfaceSolver.Estimate,int,string> done){Cancel();StartCoroutine(CheckCorners(manualSeed,done));}
  IEnumerator CheckCorners(ChessSurfaceSolver.Estimate seed,Action<ChessSurfaceSolver.Estimate,int,string> done){
   var fit=snapReference.valid?snapReference:seed;
   if(!fit.valid){done(default,0,"Place all four corners to define the board before camera verification.");yield break;}
   string failure="Playing edges unclear. Keep all four borders visible, lift hands away, then retry snapping or confirm manually.";
   bool measured=snapReference.valid;
   if(!measured&&Meta.XR.EnvironmentRaycastManager.IsSupported){if(depth==null)depth=FindFirstObjectByType<Meta.XR.EnvironmentRaycastManager>();if(depth==null){depth=gameObject.AddComponent<Meta.XR.EnvironmentRaycastManager>();ownsDepth=true;}if(ownsDepth)depth.enabled=true;}
   int stable=0,edgeMask=15;
   for(int attempt=0;attempt<3;attempt++){
    ChessCheckerFit.Frame frame=null;yield return Capture(f=>frame=f);
    if(frame==null){failure="Camera unavailable or hands obscuring board. Retry snapping or confirm markers manually.";break;}
    var search=Task.Run(()=>{var corners=ChessSurfaceSolver.Detect(frame,new ChessCheckerFit.Fit{valid=true,center=fit.center,size=fit.size,yaw=fit.rotation.eulerAngles.y});return corners;});
    float deadline=Time.unscaledTime+4;while(!search.IsCompleted&&Time.unscaledTime<deadline)yield return null;
    if(!search.IsCompleted||search.IsFaulted||!table.HandsClear||table.LastHandAt>frame.capturedAt)break;
    if(!measured){var metric=DepthEstimate(search.Result);if(metric.valid){fit=metric;measured=true;}}
    int observed=ChessSurfaceSolver.PlayableEdgeMask(fit,search.Result);if((observed&3)==0||(observed&12)==0)break;
    edgeMask&=observed;stable++;yield return new WaitForSecondsRealtime(.15f);
   }
   if(cameraAccess!=null)cameraAccess.enabled=false;if(ownsDepth&&depth!=null)depth.enabled=false;
   // Require consistent support in both axes. Missing edges can be inferred from four user markers.
   if(stable==3&&(edgeMask&3)!=0&&(edgeMask&12)!=0)done(fit,measured?edgeMask:0,"");else done(default,0,failure);
  }
 }
}
