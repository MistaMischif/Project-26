using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace LivingTable.Chess {
 public sealed class ChessCompanion:MonoBehaviour {
  [Serializable] public sealed class Fact {public string id,text;public Fact(string i,string t){id=i;text=t;}}
  [Serializable] public sealed class Context {public string sessionId,positionKey,kind,suggestedMove;public int revision;public string[] legalMoves;public Fact[] facts;}
  [Serializable] public sealed class Answer {public string sessionId,positionKey,answer,move,model;public int revision;public string[] factIds;}
  UnityWebRequest request;ChessMetaVision.Connection connection;public bool Configured=>connection!=null&&Uri.TryCreate(connection.url,UriKind.Absolute,out var u)&&(u.Scheme=="https"||u.Scheme=="http"&&u.IsLoopback)&&!string.IsNullOrWhiteSpace(connection.deviceToken);
  void Awake(){try{string p=Path.Combine(Application.persistentDataPath,"first-steps-connection.json");if(File.Exists(p))connection=JsonUtility.FromJson<ChessMetaVision.Connection>(File.ReadAllText(p));}catch{}}
  public void Cancel(){StopAllCoroutines();request?.Abort();request?.Dispose();request=null;}
  void OnDisable()=>Cancel();
  public void Ask(Context context,Action<Answer> done){Cancel();if(!Configured){done(null);return;}StartCoroutine(Send(context,done));}
  IEnumerator Send(Context context,Action<Answer> done){
   request=new UnityWebRequest(connection.url.TrimEnd('/')+"/companion","POST");request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(context)));request.downloadHandler=new DownloadHandlerBuffer();request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("Authorization","Bearer "+connection.deviceToken);request.timeout=50;
   yield return request.SendWebRequest();Answer answer=null;if(request.result==UnityWebRequest.Result.Success){try{answer=JsonUtility.FromJson<Answer>(request.downloadHandler.text);}catch{}}request.Dispose();request=null;done(answer);
  }
 }
}
