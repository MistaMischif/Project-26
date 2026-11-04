using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace LivingTable.Chess
{
 public sealed class ChessMetaVision:MonoBehaviour
 {
  [Serializable] public sealed class Connection {public string url,deviceToken,certificateSha256;}
  [Serializable] public sealed class Crop {public string square,image;}
  [Serializable] sealed class Request {public int revision;public Crop[] cells;}
  UnityWebRequest activeRequest;
  public void Cancel(){activeRequest?.Abort();activeRequest?.Dispose();activeRequest=null;Busy=false;}
  void OnDisable()=>Cancel();
  Connection connection;public bool Required;public bool Busy {get;private set;}public string Status {get;private set;}="Local vision not configured";
  public bool Configured {get{if(!ChessHostConnection.Valid(connection))connection=ChessHostConnection.Load();return ChessHostConnection.Valid(connection);}}
  void Awake(){connection=ChessHostConnection.Load();}
  public IEnumerator Observe(int revision,Crop[] cells,Action<ChessObservationGate.Observation> complete)
  {
   connection=ChessHostConnection.Load();if(!Configured||Busy){complete(null);yield break;}Busy=true;Status="Local vision is checking the moved squares…";
   using(var probe=UnityWebRequest.Get(connection.url.TrimEnd('/')+"/ready/vision")){
    activeRequest=probe;ChessHostConnection.Configure(probe,connection);probe.timeout=5;yield return probe.SendWebRequest();activeRequest=null;
    if(probe.result!=UnityWebRequest.Result.Success){Busy=false;Status="Open Prepare-Local-AI on your computer. Camera will retry when host returns.";complete(null);yield break;}
   }
   using(var request=new UnityWebRequest(connection.url.TrimEnd('/')+"/observe","POST")){
    activeRequest=request;request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Request{revision=revision,cells=cells})));request.downloadHandler=new DownloadHandlerBuffer();request.SetRequestHeader("Content-Type","application/json");ChessHostConnection.Configure(request,connection);request.timeout=185;
    yield return request.SendWebRequest();ChessObservationGate.Observation result=null;
    if(request.result==UnityWebRequest.Result.Success){try{result=JsonUtility.FromJson<ChessObservationGate.Observation>(request.downloadHandler.text);}catch{} }
    activeRequest=null;Busy=false;Status=result==null?"Local vision unavailable · retry or choose MANUAL":"Vision observation received";complete(result);
   }
  }
 }
}
