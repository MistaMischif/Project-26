using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
namespace LivingTable.Chess {
 public static class ChessHostConnection {
  public static ChessMetaVision.Connection Load(){try{var p=Path.Combine(Application.persistentDataPath,"first-steps-connection.json");return File.Exists(p)?JsonUtility.FromJson<ChessMetaVision.Connection>(File.ReadAllText(p).TrimStart('\ufeff')):null;}catch{return null;}}
  public static bool Valid(ChessMetaVision.Connection c)=>c!=null&&Uri.TryCreate(c.url,UriKind.Absolute,out var u)&&!string.IsNullOrWhiteSpace(c.deviceToken)&&(u.Scheme=="http"&&u.IsLoopback||u.Scheme=="https"&&(u.IsLoopback||ValidPin(c.certificateSha256)));
  static bool ValidPin(string p)=>p!=null&&p.Length==64&&System.Text.RegularExpressions.Regex.IsMatch(p,"^[a-fA-F0-9]+$");
  public static void Configure(UnityWebRequest request,ChessMetaVision.Connection c){request.SetRequestHeader("Authorization","Bearer "+c.deviceToken);if(c.url.StartsWith("https://",StringComparison.OrdinalIgnoreCase)&&ValidPin(c.certificateSha256))request.certificateHandler=new PinnedCertificate(c.certificateSha256);}
  public static bool Matches(byte[] bytes,string pin){using(var sha=SHA256.Create())return string.Equals(BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-",""),pin,StringComparison.OrdinalIgnoreCase);}
  sealed class PinnedCertificate:CertificateHandler {readonly string pin;public PinnedCertificate(string value){pin=value;}protected override bool ValidateCertificate(byte[] data)=>Matches(data,pin);}
 }
}
