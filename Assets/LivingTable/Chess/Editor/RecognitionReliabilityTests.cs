#if UNITY_EDITOR
using System;
using System.IO;
using LivingTable.Chess;
using LivingTable.CandyLand;
using UnityEngine;
namespace LivingTable.Editor {
 public static class RecognitionReliabilityTests {
  public static void Run(){int count=0;void Check(bool pass,string label){count++;if(!pass)throw new Exception("RECOGNITION: "+label);}
   foreach(var size in new[]{new Vector2(70,180),new Vector2(180,70),new Vector2(120,120),new Vector2(24,240)}){var fit=ChessPhysicalVision.FitCrop(size.x,size.y);Check(fit.xMin>=0&&fit.yMin>=0&&fit.xMax<=256&&fit.yMax<=256,"crop contained");Check(Mathf.Abs(fit.width/fit.height-size.x/size.y)<.03f,"physical shape proportions preserved");Check(Mathf.Max(fit.width,fit.height)==256,"image resolution used");}
   var stable=new CandyVisionStability();Check(!stable.Observe("fresh",10,.65f),"one frame cannot approve");Check(!stable.Observe("fresh",10.3,.65f),"two frames cannot approve");Check(stable.Observe("fresh",10.7,.65f),"three continuous frames can approve");stable.Reset();Check(!stable.Observe("fresh",11,.65f),"hand or view reset invalidates evidence");Check(!stable.Observe("fresh",13,.65f),"tracking gap cannot approve");Check(!stable.Observe("fresh",13,.65f),"duplicate timestamp cannot approve");
   var a=new float[64][];var b=new float[64][];for(int i=0;i<64;i++){a[i]=new float[12];b[i]=new float[12];}Check(ChessPhysicalVision.Matches(a,b),"unchanged appearance");b[42][0]=1;Check(!ChessPhysicalVision.Matches(a,b),"change outside proposed move invalidates fresh board");
   Directory.CreateDirectory("Builds");File.WriteAllText("Builds/Recognition-reliability-tests.txt",count+" synthetic crop/stability checks passed; physical recognition accuracy remains unmeasured.");Debug.Log("RECOGNITION_RELIABILITY_TESTS_PASSED "+count);
  }
 }
}
#endif
