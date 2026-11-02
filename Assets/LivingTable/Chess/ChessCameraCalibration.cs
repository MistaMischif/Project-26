using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meta.XR;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Rendering;
namespace LivingTable.Chess {
 public sealed class ChessCameraCalibration:MonoBehaviour {
  PassthroughCameraAccess cameraAccess;ChessTable table;
  public void Initialize(ChessTable owner){table=owner;}
  public void Cancel(){StopAllCoroutines();if(cameraAccess!=null)cameraAccess.enabled=false;}
  public void FitBoard(){Cancel();StartCoroutine(Fit());}
  public void SizePieces(){Cancel();StartCoroutine(Measure());}
  IEnumerator Capture(Action<ChessCheckerFit.Frame> done){
#if UNITY_ANDROID && !UNITY_EDITOR
   const string permission="horizonos.permission.HEADSET_CAMERA";
   if(!Permission.HasUserAuthorizedPermission(permission)){Permission.RequestUserPermission(permission);float until=Time.unscaledTime+12;while(!Permission.HasUserAuthorizedPermission(permission)&&Time.unscaledTime<until)yield return null;if(!Permission.HasUserAuthorizedPermission(permission)){done(null);yield break;}}
   if(!PassthroughCameraAccess.IsSupported){done(null);yield break;}
#else
   done(null);yield break;
#endif
#pragma warning disable CS0162
   if(cameraAccess==null){var go=new GameObject("Chess calibration camera");go.SetActive(false);go.transform.SetParent(transform,false);cameraAccess=go.AddComponent<PassthroughCameraAccess>();cameraAccess.RequestedResolution=new Vector2Int(1280,960);cameraAccess.MaxFramerate=15;go.SetActive(true);}cameraAccess.enabled=true;
   float deadline=Time.unscaledTime+8;while((!cameraAccess.IsPlaying||!cameraAccess.IsUpdatedThisFrame)&&Time.unscaledTime<deadline)yield return null;
   if(!cameraAccess.IsPlaying||cameraAccess.GetTexture()==null){done(null);yield break;}
   while(!table.HandsClear&&Time.unscaledTime<deadline)yield return null;
   if(!table.HandsClear){done(null);yield break;}
   var pose=cameraAccess.GetCameraPose();var inverse=Quaternion.Inverse(pose.rotation);var a=inverse*cameraAccess.ViewportPointToRay(Vector2.zero,pose).direction;var b=inverse*cameraAccess.ViewportPointToRay(Vector2.one,pose).direction;
   float fx=1/(b.x/b.z-a.x/a.z),fy=1/(b.y/b.z-a.y/a.z);var frame=new ChessCheckerFit.Frame{width=320,height=240,camera=pose.position,rotation=pose.rotation,fx=fx,fy=fy,cx=-a.x/a.z*fx,cy=-a.y/a.z*fy};
   var rt=RenderTexture.GetTemporary(320,240,0,RenderTextureFormat.ARGB32);Graphics.Blit(cameraAccess.GetTexture(),rt);bool complete=false;AsyncGPUReadback.Request(rt,0,TextureFormat.RGBA32,read=>{if(!read.hasError)frame.pixels=read.GetData<Color32>().ToArray();RenderTexture.ReleaseTemporary(rt);complete=true;});while(!complete)yield return null;done(frame.pixels==null?null:frame);
#pragma warning restore CS0162
  }
  IEnumerator Fit(){ChessCheckerFit.Frame first=null,second=null;yield return Capture(f=>first=f);if(first==null){Fail("Camera unavailable. Allow camera access, retry, or use manual corners.");yield break;}var seed=table.transform.position;float size=table.Size,yaw=table.transform.eulerAngles.y;var task=Task.Run(()=>ChessCheckerFit.Find(first,seed,size,yaw));while(!task.IsCompleted)yield return null;if(task.IsFaulted||!task.Result.valid){Fail("No confident checker pattern found. Look down at the full board, clear hands, and retry; manual corners are available.");yield break;}var fit=task.Result;yield return new WaitForSecondsRealtime(.25f);yield return Capture(f=>second=f);if(second==null){Fail("Camera interrupted. Retry alignment.");yield break;}var check=Task.Run(()=>ChessCheckerFit.Find(second,fit.center,fit.size,fit.yaw));while(!check.IsCompleted)yield return null;if(check.IsFaulted||!ChessCheckerFit.Consistent(fit,check.Result)){Fail("The board fit was unstable. Keep the whole board still and retry.");yield break;}cameraAccess.enabled=false;table.PreviewAutoAlignment(check.Result);}
  void Fail(string reason){if(cameraAccess!=null)cameraAccess.enabled=false;table.CalibrationFailed(reason);}
  IEnumerator Measure(){ChessCheckerFit.Frame first=null,second=null;yield return Capture(f=>first=f);if(first==null){Fail("Piece scan unavailable. Keep standard sizes or retry.");yield break;}float deadline=Time.unscaledTime+20;while(Time.unscaledTime<deadline){yield return new WaitForSecondsRealtime(.6f);yield return Capture(f=>second=f);if(second!=null&&Vector3.Distance(first.camera,second.camera)>.04f)break;second=null;}if(second==null){Fail("A second viewing angle is needed. Move your head slightly and retry, or keep standard sizes.");yield break;}
   var position=table.Match?.Position??ChessPosition.Initial();var profile=new float[24];var h=new List<float>[12];var w=new List<float>[12];for(int i=0;i<12;i++){h[i]=new List<float>();w[i]=new List<float>();}
   for(int square=0;square<64;square++)if(position.Board[square]!=0){var a=MeasurePiece(first,table,square);var b=MeasurePiece(second,table,square);if(a.x<=0||b.x<=0||Mathf.Abs(a.x-b.x)>Mathf.Max(.012f,Mathf.Min(a.x,b.x)*.2f)||Mathf.Abs(a.y-b.y)>.012f)continue;int index=ProfileIndex(position.Board[square]);h[index].Add((a.x+b.x)*.5f);w[index].Add((a.y+b.y)*.5f);}
   for(int i=0;i<12;i++)if(h[i].Count>0){h[i].Sort();w[i].Sort();profile[i*2]=h[i][h[i].Count/2];profile[i*2+1]=w[i][w[i].Count/2];}cameraAccess.enabled=false;table.PreviewPieceSizes(profile);
  }
  public static int ProfileIndex(int piece)=>(piece>0?0:6)+Math.Abs(piece)-1;
  static Vector2 MeasurePiece(ChessCheckerFit.Frame frame,ChessTable board,int square){var center=board.SquareWorld(square);center.y=board.transform.position.y;var plane=new Plane(Vector3.up,center);float[] means=new float[2];int[] counts=new int[2];for(int s=0;s<64;s++){var c=board.SquareWorld(s);foreach(var offset in new[]{new Vector3(.36f,0,.36f),new Vector3(-.36f,0,-.36f)}){var p=c+board.transform.TransformDirection(offset)*board.Size/8;p.y=center.y;float v=frame.Gray(p);if(v>=0){int color=(s%8+s/8)%2;means[color]+=v;counts[color]++;}}}if(counts.Any(n=>n<10))return Vector2.zero;for(int i=0;i<2;i++)means[i]/=counts[i];if(Mathf.Abs(means[0]-means[1])<35)return Vector2.zero;
   bool Foreground(Vector3 p){var uv=frame.Project(p);var ray=frame.Ray(uv);if(!plane.Raycast(ray,out float distance))return false;var local=board.transform.InverseTransformPoint(ray.GetPoint(distance));float x=local.x/board.Size*8*board.HumanSide+4,z=local.z/board.Size*8*board.HumanSide+4;if(x<0||z<0||x>=8||z>=8||x%1<.12f||x%1>.88f||z%1<.12f||z%1>.88f)return false;float v=frame.Gray(p);return v>=0&&Mathf.Abs(v-means[((int)x+(int)z)%2])>Mathf.Max(28,Mathf.Abs(means[0]-means[1])*.28f);}
   float top=0;int hits=0;for(float y=.018f;y<=.18f;y+=.003f)if(Foreground(center+Vector3.up*y)){top=y;hits++;}if(top<.03f||hits<5||top>=.174f)return Vector2.zero;var right=Vector3.Cross(Vector3.up,frame.camera-center).normalized;float width=0;for(float x=.006f;x<board.Size/8*.44f;x+=.002f)if(Foreground(center+Vector3.up*top*.25f+right*x)||Foreground(center+Vector3.up*top*.25f-right*x))width=x*2;return width>=.014f?new Vector2(top,width):Vector2.zero;
  }
  void OnDisable()=>Cancel();void OnDestroy(){if(cameraAccess!=null)Destroy(cameraAccess.gameObject);}
 }
}
