using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace LivingTable.Chess {
 // App-lifetime reconnection checks; no startup task or OS service is installed.
 public sealed class ChessHostMonitor:MonoBehaviour {
  IEnumerator Start(){while(true){var config=ChessHostConnection.Load();if(ChessHostConnection.Valid(config)){using(var request=UnityWebRequest.Get(config.url.TrimEnd('/')+"/ready")){ChessHostConnection.Configure(request,config);request.timeout=5;yield return request.SendWebRequest();Debug.Log("FIRST_STEPS_WIRELESS_READY: "+(request.result==UnityWebRequest.Result.Success?"CONNECTED":"RETRYING")+" HTTP "+request.responseCode);}}yield return new WaitForSecondsRealtime(20);}}
 }
}
