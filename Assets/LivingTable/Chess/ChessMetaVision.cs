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
  [Serializable] public sealed class Connection {public string url,deviceToken;}
  [Serializable] public sealed class Crop {public string square,image;}
  [Serializable] sealed class Request {public int revision;public Crop[] cells;}
  UnityWebRequest activeRequest;
  void OnDisable(){activeRequest?.Abort();Busy=false;}
  Connection connection;public bool Required;public bool Busy {get;private set;}public string Status {get;private set;}="Local Meta vision not configured";
  public bool Configured=>connection!=null&&Uri.TryCreate(connection.url,UriKind.Absolute,out var uri)&&(uri.Scheme=="https"||uri.Scheme=="http"&&uri.IsLoopback)&&!string.IsNullOrWhiteSpace(connection.deviceToken);
  void Awake(){try{var path=Path.Combine(Application.persistentDataPath,"first-steps-connection.json");if(File.Exists(path))connection=JsonUtility.FromJson<Connection>(File.ReadAllText(path));}catch{Status="Check First Steps connection file";}}
  public IEnumerator Observe(int revision,Crop[] cells,Action<ChessObservationGate.Observation> complete)
  {
   if(!Configured||Busy){complete(null);yield break;}Busy=true;Status="Meta AI is checking the moved squares…";
   using(var request=new UnityWebRequest(connection.url.TrimEnd('/')+"/observe","POST")){
    activeRequest=request;request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new Request{revision=revision,cells=cells})));request.downloadHandler=new DownloadHandlerBuffer();request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("Authorization","Bearer "+connection.deviceToken);request.timeout=185;
    yield return request.SendWebRequest();ChessObservationGate.Observation result=null;
    if(request.result==UnityWebRequest.Result.Success){try{result=JsonUtility.FromJson<ChessObservationGate.Observation>(request.downloadHandler.text);}catch{} }
    activeRequest=null;Busy=false;Status=result==null?"Meta vision unavailable · retry or choose MANUAL":"Meta observation received";complete(result);
   }
  }
 }
}
