using System;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  Action virtualSafety;
  public bool VirtualSafetyOpen=>virtualSafety!=null;
  public const string VirtualSafetyText="BEFORE VIRTUAL PLAY\nYour physical board, pieces and furniture are still in the real world. They do not move or disappear.\nClear a safe play area, or move to another clear space. Check your headset boundary and surroundings before continuing.";
  void RequestVirtualSafety(Action proceed){
   if(VirtualSafetyOpen)return;CancelGrab();Version++;Vision.CancelScan();virtualSafety=proceed;DrawUI();
  }
  public void CancelVirtualSafety(){virtualSafety=null;Vision.ClearEvidence();DrawUI();}
  public void ConfirmVirtualSafety(){var proceed=virtualSafety;if(proceed==null)return;virtualSafety=null;sessionMenu=extraSettings=pieceSettings=standardMenu=false;proceed();}
  bool DrawVirtualSafety(){
   if(!VirtualSafetyOpen)return false;
   status.transform.localPosition=new Vector3(0,.035f,-.015f);status.text=WrapStatus(VirtualSafetyText);status.fontStyle=FontStyle.Bold;
   Add("SPACE CLEAR",0,0,ConfirmVirtualSafety,Purple,true);
   Add("STAY HERE",-2,0,CancelVirtualSafety,Teal);
   int index=0;foreach(var button in buttons){button.root.SetParent(panel.transform,false);button.root.localPosition=new Vector3(index++==0?.10f:-.10f,-.10f,-.03f);button.root.localRotation=Quaternion.Euler(-90,0,0);}
   return true;
  }
 }
}
