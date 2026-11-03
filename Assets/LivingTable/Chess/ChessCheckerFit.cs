using System;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 // Fits the visible checker pattern on the already placed table plane; no move or identity inference.
 public static class ChessCheckerFit {
  public sealed class Frame {
   public double capturedAt;public long imageTimestampTicks;public Color32[] pixels;public int width,height;public Vector3 camera;public Quaternion rotation;public float fx,fy,cx,cy;
   public Vector2 Project(Vector3 world){var p=Quaternion.Inverse(rotation)*(world-camera);return p.z<=.02f?new Vector2(-1,-1):new Vector2(fx*p.x/p.z+cx,fy*p.y/p.z+cy);}
   public Ray Ray(Vector2 uv)=>new Ray(camera,rotation*new Vector3((uv.x-cx)/fx,(uv.y-cy)/fy,1).normalized);
   public float Gray(Vector3 world){var uv=Project(world);if(uv.x<.01f||uv.y<.01f||uv.x>.99f||uv.y>.99f)return -1;var c=pixels[(int)(uv.y*(height-1))*width+(int)(uv.x*(width-1))];return .299f*c.r+.587f*c.g+.114f*c.b;}
  }
  public struct Fit {public Vector3 center;public float yaw,size,score;public bool valid;}
  static Vector3 Point(Vector3 center,Vector3 right,Vector3 forward,float x,float z)=>center+right*x+forward*z;
  static float Cell(Frame frame,Vector3 center,Vector3 right,Vector3 forward,float x,float z){float a=frame.Gray(Point(center,right,forward,x-.026f,z-.026f)),b=frame.Gray(Point(center,right,forward,x+.026f,z-.026f)),c=frame.Gray(Point(center,right,forward,x-.026f,z+.026f)),d=frame.Gray(Point(center,right,forward,x+.026f,z+.026f));if(a<0||b<0||c<0||d<0)return -1;return (a+b+c+d-Mathf.Min(a,Mathf.Min(b,Mathf.Min(c,d)))-Mathf.Max(a,Mathf.Max(b,Mathf.Max(c,d))))*.5f;}
  public static float Score(Frame frame,Fit f){if(f.size<.20f||f.size>1.2f)return -1;var rotation=Quaternion.Euler(0,f.yaw,0);var center=f.center;var right=rotation*Vector3.right*f.size;var forward=rotation*Vector3.forward*f.size;float even=0,odd=0;var values=new float[64];for(int z=0;z<8;z++)for(int x=0;x<8;x++){float v=Cell(frame,center,right,forward,(x-3.5f)/8,(z-3.5f)/8);if(v<0)return -1;values[z*8+x]=v;if((x+z)%2==0)even+=v;else odd+=v;}even/=32;odd/=32;float contrast=Mathf.Abs(even-odd);if(contrast<25)return 0;float mid=(even+odd)*.5f;int correct=0;for(int z=0;z<8;z++)for(int x=0;x<8;x++)if((values[z*8+x]>mid)==((x+z)%2==0?even>odd:odd>even))correct++;
   float score=contrast*Mathf.Clamp01((correct/64f-.65f)/.35f);float spill=0;for(int edge=0;edge<4;edge++){float e=0,o=0;int count=0;for(int i=0;i<8;i++){float x=edge<2?(i-3.5f)/8:(edge==2?-.5625f:.5625f),z=edge<2?(edge==0?-.5625f:.5625f):(i-3.5f)/8;float v=frame.Gray(Point(center,right,forward,x,z));if(v<0)continue;if(i%2==0)e+=v;else o+=v;count++;}if(count==8)spill+=Mathf.Abs(e-o)/4;}float edges=0;int edgeCount=0;for(int i=1;i<8;i++)for(int j=0;j<8;j++){float along=(j-3.5f)/8,cross=i/8f-.5f;foreach(bool vertical in new[]{true,false}){float a=frame.Gray(Point(center,right,forward,vertical?cross-.008f:along,vertical?along:cross-.008f));float b=frame.Gray(Point(center,right,forward,vertical?cross+.008f:along,vertical?along:cross+.008f));if(a>=0&&b>=0){edges+=Mathf.Abs(a-b);edgeCount++;}}}return score-spill*.25f+(edgeCount>0?edges/edgeCount*.5f:0);
  }
  public static Fit Find(Frame frame,Vector3 seed,float size,float yaw,bool broad=false){var best=new Fit{center=seed,size=size,yaw=yaw,score=-1};var centerRay=frame.Ray(new Vector2(.5f,.5f));var plane=new Plane(Vector3.up,seed);Vector3 look=plane.Raycast(centerRay,out float distance)&&distance<2?centerRay.GetPoint(distance):seed;
   var widths=broad?new[]{.2f,.26f,.32f,.4f,.48f,.56f,.64f,.72f,.8f,.9f,1f,1.1f,1.2f,size}:Enumerable.Range(-3,7).Select(s=>Mathf.Clamp(size+s*.065f,.2f,1.2f)).ToArray();
   foreach(var origin in new[]{seed,look})for(int x=-4;x<=4;x++)for(int z=-4;z<=4;z++)foreach(float width in widths)for(int angle=-4;angle<=4;angle++){var f=new Fit{center=origin+new Vector3(x*.045f,0,z*.045f),size=width,yaw=yaw+angle*10};f.score=Score(frame,f);if(f.score>best.score)best=f;}
   for(int level=0;level<5;level++){float step=.024f/Mathf.Pow(2,level),angle=3f/Mathf.Pow(2,level);for(int repeat=0;repeat<4;repeat++){var previous=best;for(int axis=0;axis<4;axis++)for(int sign=-1;sign<=1;sign+=2){var f=best;if(axis==0)f.center.x+=step*sign;if(axis==1)f.center.z+=step*sign;if(axis==2)f.size+=step*sign;if(axis==3)f.yaw+=angle*sign;f.score=Score(frame,f);if(f.score>best.score)best=f;}if(best.score<=previous.score)break;}}
   best.valid=best.score>=40;return best;
  }
  public static bool Consistent(Fit a,Fit b)=>a.valid&&b.valid&&Vector3.Distance(a.center,b.center)<.018f&&Mathf.Abs(a.size-b.size)<.02f&&Mathf.Abs(Mathf.DeltaAngle(a.yaw,b.yaw))<3;
 }
}
