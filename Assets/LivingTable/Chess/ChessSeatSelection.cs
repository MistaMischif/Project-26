using System;
using System.Linq;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  public static int NearestColor(int[] pieces){float white=0,black=0;int wc=0,bc=0;for(int s=0;s<64;s++){if(pieces[s]>0){white+=s/8;wc++;}else if(pieces[s]<0){black+=s/8;bc++;}}if(wc==0||bc==0)return 0;return white/wc<=black/bc?1:-1;}
  void AutoSeatCapturedLayout(){if(setupPosition==null||unknownSquares.Any(x=>x))return;int closest=NearestColor(setupPosition.Board);if(closest<0){Array.Reverse(setupPosition.Board);Array.Reverse(unknownSquares);HumanSide=-1;}else HumanSide=1;
   var initial=ChessPosition.Initial();if(setupPosition.Board.SequenceEqual(initial.Board)){setupPosition.Side=1;setupPosition.Rights=15;turnChosen=true;}
  }
  public bool OrientRecoveredBoard(int[] observed){if(Match==null||observed==null||observed.Length!=64||!Recovering)return false;var expected=Match.Position.Board;if(!observed.Reverse().SequenceEqual(expected))return false;HumanSide=-HumanSide;Version++;Vision.ClearEvidence();DrawScene();return true;}
 }
}
