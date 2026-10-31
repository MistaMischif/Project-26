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
   return AcceptCells(after,changed,revision,observation,out _);
  }
  // Reconciliation never changes the authoritative position, including its hidden chess rights.
  public static bool AcceptBoard(ChessPosition position,int revision,Observation observation,out int mismatch)=>AcceptCells(position,Enumerable.Range(0,64).ToArray(),revision,observation,out mismatch);
  public static bool AcceptCells(ChessPosition position,int[] squares,int revision,Observation observation,out int mismatch)
  {
   mismatch=-1;
   if(position==null||squares==null||squares.Length==0||squares.Distinct().Count()!=squares.Length||squares.Any(s=>s<0||s>63)||observation==null||observation.revision!=revision||observation.cells==null||observation.cells.Length!=squares.Length)return false;
   foreach(int square in squares){var cells=observation.cells.Where(c=>c!=null&&c.square==ChessMove.Square(square)).ToArray();if(cells.Length!=1||cells[0].piece!=Piece(position.Board[square])){mismatch=square;return false;}}
   return true;
  }
 }
}
