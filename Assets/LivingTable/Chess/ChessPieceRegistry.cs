using System;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 [Serializable] public sealed class ChessPieceRegistry {
  [Serializable] public sealed class Piece {public string id;public int kind,square;public bool captured;public float[] shape;public Vector3 view;}
  public Piece[] pieces=Array.Empty<Piece>();
  public void Enroll(ChessPosition position){if(pieces!=null&&pieces.Length>0)return;pieces=Enumerable.Range(0,64).Where(s=>position.Board[s]!=0).Select(s=>new Piece{id=Guid.NewGuid().ToString("N"),kind=position.Board[s],square=s}).ToArray();}
  public Piece At(int square)=>pieces?.FirstOrDefault(p=>!p.captured&&p.square==square);
  public void Advance(ChessPosition before,ChessMove move){
   Enroll(before);var moving=At(move.From);if(moving==null)return;var victim=At(move.EnPassant?move.To-before.Side*8:move.To);if(victim!=null){victim.captured=true;victim.square=-1;}
   if(Math.Abs(moving.kind)==6&&Math.Abs(move.To-move.From)==2){var rook=At(move.To>move.From?move.From+3:move.From-4);if(rook!=null)rook.square=move.To>move.From?move.From+1:move.From-1;}
   moving.square=move.To;if(move.Promotion!=0){moving.kind=before.Side*move.Promotion;moving.shape=null;}
  }
  public void Learn(ChessPosition position,float[][] shapes,Vector3 view){Enroll(position);if(shapes==null||shapes.Length!=64)return;foreach(var p in pieces)if(!p.captured&&position.Board[p.square]==p.kind&&shapes[p.square]!=null){p.shape=(float[])shapes[p.square].Clone();p.view=view.normalized;}}
  public bool Recognizes(int source,float[] shape,Vector3 view){var p=At(source);return p?.shape!=null&&shape!=null&&Vector3.Angle(p.view,view.normalized)<=12&&Distance(p.shape,shape)<.12f;}
  public static float Distance(float[] a,float[] b){if(a==null||b==null||a.Length!=b.Length||a.Length!=288)return float.PositiveInfinity;float sum=0;for(int i=0;i<a.Length;i++){if(!float.IsFinite(a[i])||!float.IsFinite(b[i]))return float.PositiveInfinity;sum+=Mathf.Abs(a[i]-b[i]);}return sum/a.Length;}
  public static float[] Describe(Color32[] patch){if(patch==null||patch.Length!=144)return null;var v=patch.Select(c=>(.299f*c.r+.587f*c.g+.114f*c.b)/255f).ToArray();float mean=v.Average(),spread=Mathf.Sqrt(v.Average(x=>(x-mean)*(x-mean)));if(spread<.035f)return null;var result=new float[288];for(int i=0;i<144;i++){result[i]=Mathf.Clamp((v[i]-mean)/(spread*4),-1,1);int x=i%12,y=i/12;result[144+i]=(x>0&&x<11&&y>0&&y<11)?Mathf.Clamp01((Mathf.Abs(v[i+1]-v[i-1])+Mathf.Abs(v[i+12]-v[i-12]))/(spread*4)):0;}return result;}
 }
 public sealed class ChessPhysicalPieceTag:MonoBehaviour {public string PieceId;public int PieceKind;}
}
