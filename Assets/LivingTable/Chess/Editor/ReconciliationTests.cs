#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using LivingTable.Chess;
using UnityEngine;
namespace LivingTable.Editor
{
 public static class ReconciliationTests
 {
  public static void Run(){int count=0;void Check(bool ok,string name){count++;if(!ok)throw new Exception("RECONCILIATION: "+name);}
   foreach(var fen in new[]{"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1","4k3/8/8/8/8/8/4Q3/3QK3 b - - 14 29","r3k2r/8/8/3pP3/8/8/8/R3K2R w KQkq d6 0 12"}){
    var p=ChessPosition.FromFen(fen);string key=p.Key();int ply=p.Ply,rights=p.Rights,ep=p.Ep;
    ChessObservationGate.Observation Full()=>new ChessObservationGate.Observation{revision=9,cells=Enumerable.Range(0,64).Select(i=>new ChessObservationGate.Cell{square=ChessMove.Square(i),piece=ChessObservationGate.Piece(p.Board[i])}).ToArray()};
    var o=Full();Check(ChessObservationGate.AcceptBoard(p,9,o,out _),"Full saved position accepted");Check(!ChessObservationGate.AcceptBoard(p,10,o,out _),"Stale revision rejected");
    o.cells=o.cells.Take(63).ToArray();Check(!ChessObservationGate.AcceptBoard(p,9,o,out _),"Missing empty or occupied square rejected");
    o=Full();o.cells[20].piece="white_queen";Check(!ChessObservationGate.AcceptBoard(p,9,o,out int at)&&at==20,"Extra piece identified precisely");
    o=Full();o.cells[4].piece="uncertain";Check(!ChessObservationGate.AcceptBoard(p,9,o,out at)&&at==4,"Uncertain piece cannot pass");
    o=Full();o.cells[5].square=o.cells[4].square;Check(!ChessObservationGate.AcceptBoard(p,9,o,out _),"Duplicate square rejected");
    o=Full();o.cells[0]=null;Check(!ChessObservationGate.AcceptBoard(p,9,o,out _),"Null observation rejected");
    o=Full();o.cells=o.cells.Reverse().ToArray();Check(ChessObservationGate.AcceptBoard(p,9,o,out _),"Order independent");
    o=Full();o.cells=o.cells.Take(4).ToArray();Check(ChessObservationGate.AcceptCells(p,new[]{0,1,2,3},9,o,out _),"Valid batch");Check(!ChessObservationGate.AcceptBoard(p,9,o,out _),"Partial batch never unlocks full board");
    Check(p.Key()==key&&p.Ply==ply&&p.Rights==rights&&p.Ep==ep,"Position and hidden rights unchanged");
   }
   File.WriteAllText("Builds/Reconciliation-tests.txt",count+" full-board identity gate checks passed; synthetic observations, not camera accuracy.");Debug.Log("RECONCILIATION_TESTS_PASSED "+count);
  }
 }
}
#endif
