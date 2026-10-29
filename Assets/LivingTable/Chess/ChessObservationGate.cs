using System;
using System.Linq;
namespace LivingTable.Chess
{
 // A model supplies observations, never authority to change the saved position.
 public static class ChessObservationGate
 {
  [Serializable] public sealed class Cell {public string square,piece;}
  [Serializable] public sealed class Observation {public int revision;public string model;public Cell[] cells;public int elapsedMs;}
  public static string Piece(int piece)=>piece==0?"empty":(piece>0?"white_":"black_")+new[]{"","pawn","knight","bishop","rook","queen","king"}[Math.Abs(piece)];
  public static bool Accept(ChessPosition position,ChessMove move,int revision,Observation observation)
  {
   if(position==null||observation==null||observation.revision!=revision||observation.cells==null||!position.LegalMoves().Contains(move))return false;
   var after=position.After(move);var changed=Enumerable.Range(0,64).Where(s=>position.Board[s]!=after.Board[s]).ToArray();
   if(observation.cells.Length!=changed.Length)return false;
   foreach(int square in changed){var cells=observation.cells.Where(c=>c!=null&&c.square==ChessMove.Square(square)).ToArray();if(cells.Length!=1||cells[0].piece!=Piece(after.Board[square]))return false;}
   return true;
  }
 }
}
