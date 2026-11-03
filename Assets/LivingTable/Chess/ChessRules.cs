using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
namespace LivingTable.Chess
{
 public readonly struct ChessMove:IEquatable<ChessMove>
 {
  public readonly int From,To,Promotion;public readonly bool EnPassant;
  public ChessMove(int from,int to,int promotion=0,bool ep=false){From=from;To=to;Promotion=promotion;EnPassant=ep;}
  public bool Equals(ChessMove m)=>From==m.From&&To==m.To&&Promotion==m.Promotion&&EnPassant==m.EnPassant;
  public override bool Equals(object o)=>o is ChessMove m&&Equals(m);public override int GetHashCode()=>From|To<<6|Promotion<<12|(EnPassant?1<<16:0);
  public override string ToString()=>Square(From)+"–"+Square(To)+(Promotion==0?"":"="+" PNBRQK"[Promotion]);
  public static string Square(int s)=>((char)('a'+s%8)).ToString()+(s/8+1);
 }
 [Serializable] public sealed class ChessPosition
 {
  public int[] Board=new int[64];public int Side=1,Rights=15,Ep=-1,Halfmove,Ply;
  static readonly int[,] Knights={{1,2},{2,1},{2,-1},{1,-2},{-1,-2},{-2,-1},{-2,1},{-1,2}};
  static readonly int[,] Directions={{1,0},{-1,0},{0,1},{0,-1},{1,1},{1,-1},{-1,1},{-1,-1}};
  public static ChessPosition Initial()=>FromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
  public static ChessPosition FromFen(string fen){var p=new ChessPosition();var fields=fen.Split(' ');int rank=7,file=0;foreach(char c in fields[0]){if(c=='/'){rank--;file=0;}else if(char.IsDigit(c))file+=c-'0';else{int piece=" pnbrqk".IndexOf(char.ToLowerInvariant(c));if(piece<1||rank<0||file>7)throw new ArgumentException("Invalid FEN");p.Board[rank*8+file++]=char.IsUpper(c)?piece:-piece;}}p.Side=fields[1]=="w"?1:-1;p.Rights=0;for(int i=0;i<4;i++)if(fields[2].Contains("KQkq"[i]))p.Rights|=1<<i;p.Ep=fields[3]=="-"?-1:(fields[3][1]-'1')*8+fields[3][0]-'a';if(fields.Length>4)p.Halfmove=int.Parse(fields[4]);if(fields.Length>5)p.Ply=(int.Parse(fields[5])-1)*2+(p.Side<0?1:0);return p;}
  public ChessPosition Copy()=>new ChessPosition{Board=(int[])Board.Clone(),Side=Side,Rights=Rights,Ep=Ep,Halfmove=Halfmove,Ply=Ply};
  public bool InCheck(int side){int king=Array.IndexOf(Board,side*6);return king<0||Attacked(king,-side);}
  public bool Attacked(int square,int by)
  {
   int x=square%8,y=square/8;
   for(int dx=-1;dx<=1;dx+=2){int px=x+dx,py=y-by;if(px>=0&&px<8&&py>=0&&py<8&&Board[py*8+px]==by)return true;}
   for(int i=0;i<8;i++){int nx=x+Knights[i,0],ny=y+Knights[i,1];if(nx>=0&&nx<8&&ny>=0&&ny<8&&Board[ny*8+nx]==by*2)return true;}
   for(int d=0;d<8;d++)for(int step=1;step<8;step++){int nx=x+Directions[d,0]*step,ny=y+Directions[d,1]*step;if(nx<0||nx>7||ny<0||ny>7)break;int piece=Board[ny*8+nx];if(piece==0)continue;if(Math.Sign(piece)==by){int type=Math.Abs(piece);if(type==5||(d<4?type==4:type==3)||(step==1&&type==6))return true;}break;}return false;
  }
  void Add(List<ChessMove> list,int from,int to,bool ep=false){if(Math.Abs(Board[from])==1&&(to/8==0||to/8==7)){foreach(int promotion in new[]{5,4,3,2})list.Add(new ChessMove(from,to,promotion,ep));}else list.Add(new ChessMove(from,to,0,ep));}
  public List<ChessMove> LegalMoves()
  {
   var pseudo=new List<ChessMove>();
   for(int from=0;from<64;from++){int piece=Board[from];if(Math.Sign(piece)!=Side)continue;int type=Math.Abs(piece),x=from%8,y=from/8;
    if(type==1){int next=from+Side*8;if(next>=0&&next<64&&Board[next]==0){Add(pseudo,from,next);int twice=from+Side*16;if(y==(Side==1?1:6)&&Board[twice]==0)Add(pseudo,from,twice);}for(int dx=-1;dx<=1;dx+=2){int nx=x+dx,ny=y+Side;if(nx<0||nx>7||ny<0||ny>7)continue;int to=ny*8+nx;if(Board[to]*Side<0&&Math.Abs(Board[to])!=6)Add(pseudo,from,to);else if(to==Ep&&Board[to]==0&&Board[to-Side*8]==-Side)Add(pseudo,from,to,true);}continue;}
    if(type==2){for(int i=0;i<8;i++){int nx=x+Knights[i,0],ny=y+Knights[i,1];if(nx>=0&&nx<8&&ny>=0&&ny<8&&Board[ny*8+nx]*Side<=0&&Math.Abs(Board[ny*8+nx])!=6)Add(pseudo,from,ny*8+nx);}continue;}
    for(int d=0;d<8;d++){if(type==3&&d<4||type==4&&d>=4)continue;for(int step=1;step<=(type==6?1:7);step++){int nx=x+Directions[d,0]*step,ny=y+Directions[d,1]*step;if(nx<0||nx>7||ny<0||ny>7)break;int to=ny*8+nx;if(Board[to]*Side>0||Math.Abs(Board[to])==6)break;Add(pseudo,from,to);if(Board[to]!=0)break;}}
    if(type==6&&from==(Side==1?4:60)&&!InCheck(Side)){int home=Side==1?0:56,shift=Side==1?0:2;if((Rights&(1<<shift))!=0&&Board[home+7]==Side*4&&Board[home+5]==0&&Board[home+6]==0&&!Attacked(home+5,-Side)&&!Attacked(home+6,-Side))Add(pseudo,from,home+6);if((Rights&(2<<shift))!=0&&Board[home]==Side*4&&Board[home+1]==0&&Board[home+2]==0&&Board[home+3]==0&&!Attacked(home+3,-Side)&&!Attacked(home+2,-Side))Add(pseudo,from,home+2);}
   }
   return pseudo.Where(m=>!After(m).InCheck(Side)).ToList();
  }
  public ChessPosition After(ChessMove m)
  {
   var p=Copy();int moving=p.Board[m.From],captured=p.Board[m.To];p.Board[m.From]=0;p.Board[m.To]=m.Promotion==0?moving:Side*m.Promotion;if(m.EnPassant)p.Board[m.To-Side*8]=0;
   if(Math.Abs(moving)==6){p.Rights&=Side==1?12:3;if(Math.Abs(m.To-m.From)==2){int rook=m.To>m.From?m.From+3:m.From-4;int target=m.To>m.From?m.From+1:m.From-1;p.Board[target]=p.Board[rook];p.Board[rook]=0;}}
   int[] squares={7,0,63,56};for(int i=0;i<4;i++)if(m.From==squares[i]||m.To==squares[i])p.Rights&=~(1<<i);
   p.Ep=Math.Abs(moving)==1&&Math.Abs(m.To-m.From)==16?(m.To+m.From)/2:-1;p.Halfmove=Math.Abs(moving)==1||captured!=0||m.EnPassant?0:Halfmove+1;p.Ply++;p.Side=-Side;return p;
  }
  public string Key(){int ep=Ep>=0&&LegalMoves().Any(m=>m.EnPassant)?Ep:-1;return string.Join(",",Board)+":"+Side+":"+Rights+":"+ep;}
  public bool Insufficient(){var men=Board.Select((piece,square)=>new{piece,square}).Where(x=>x.piece!=0&&Math.Abs(x.piece)!=6).ToArray();if(men.Length==0)return true;if(men.Length==1)return Math.Abs(men[0].piece)==2||Math.Abs(men[0].piece)==3;return men.All(x=>Math.Abs(x.piece)==3)&&men.Select(x=>(x.square%8+x.square/8)%2).Distinct().Count()==1;}
 }
 public sealed class ChessMatch
 {
  [Serializable] public sealed class SavedMove { public int from,to,promotion;public bool ep; public ChessMove Move=>new ChessMove(from,to,promotion,ep);public SavedMove(ChessMove m){from=m.From;to=m.To;promotion=m.Promotion;ep=m.EnPassant;} }
  [Serializable] public sealed class State {public ChessPosition initial;public SavedMove[] moves;public bool drawClaimed;}
  readonly List<SavedMove> played=new List<SavedMove>();
  public State Capture()=>new State{initial=history[0].Copy(),moves=played.ToArray(),drawClaimed=Result=="Draw claimed"};
  public static ChessMatch Restore(State state){if(state==null||state.initial==null||state.moves==null)throw new ArgumentException("Missing chess state");ValidatePosition(state.initial);var match=new ChessMatch(state.initial.Copy());foreach(var move in state.moves)if(!match.Play(move.Move))throw new ArgumentException("Illegal saved move");if(state.drawClaimed){if(!match.CanClaim)throw new ArgumentException("Invalid saved draw");match.Claim();}return match;}
  public static void ValidatePosition(ChessPosition p){if(p.Board==null||p.Board.Length!=64||p.Board.Count(x=>x==6)!=1||p.Board.Count(x=>x==-6)!=1||p.Board.Any(x=>Math.Abs(x)>6)||p.Side!=1&&p.Side!=-1||p.Halfmove<0||p.Ply<0||p.Rights<0||p.Rights>15||p.Ep< -1||p.Ep>63)throw new ArgumentException("Invalid board or turn");if(p.Board.Count(x=>x>0)>16||p.Board.Count(x=>x<0)>16||p.Board.Count(x=>x==1)>8||p.Board.Count(x=>x==-1)>8)throw new ArgumentException("Too many pieces for standard chess");if(Enumerable.Range(0,64).Any(i=>(i/8==0||i/8==7)&&Math.Abs(p.Board[i])==1)||p.InCheck(-p.Side))throw new ArgumentException("Illegal kings, pawns or side to move");int[] rooks={7,0,63,56};for(int i=0;i<4;i++){int side=i<2?1:-1;if((p.Rights&(1<<i))!=0&&(p.Board[side==1?4:60]!=side*6||p.Board[rooks[i]]!=side*4))throw new ArgumentException("Castling rights do not match pieces");}if(p.Ep>=0&&(p.Ep/8!=(p.Side==1?5:2)||p.Board[p.Ep]!=0||p.Board[p.Ep-p.Side*8]!=-p.Side||p.Halfmove!=0))throw new ArgumentException("Invalid en passant square");}
  readonly List<ChessPosition> history=new List<ChessPosition>();public ChessPosition Position=>history[history.Count-1];public string Result {get;private set;}="";public ChessMove? LastMove {get;private set;}
  public ChessMatch(ChessPosition initial=null){history.Add(initial??ChessPosition.Initial());Evaluate();}
  int Repetitions()=>history.Count(p=>p.Key()==Position.Key());
  public bool CanClaim=>Result==""&&(Position.Halfmove>=100||Repetitions()>=3);
  public bool Play(ChessMove move){if(Result!=""||!Position.LegalMoves().Contains(move))return false;history.Add(Position.After(move));played.Add(new SavedMove(move));LastMove=move;Evaluate();return true;}
  void Evaluate(){var moves=Position.LegalMoves();if(moves.Count==0)Result=Position.InCheck(Position.Side)?(Position.Side==1?"Black":"White")+" wins by checkmate":"Draw — stalemate";else if(Position.Insufficient())Result="Draw — insufficient material";else if(Position.Halfmove>=150||Repetitions()>=5)Result="Draw — automatic repetition / move limit";}
  public void Claim(){if(CanClaim)Result="Draw claimed";}
  public void Undo(int plies){while(plies-->0&&history.Count>1){history.RemoveAt(history.Count-1);played.RemoveAt(played.Count-1);}Result="";LastMove=null;Evaluate();}
 }
 public static class ChessAi
 {
  static readonly int[] Values={0,100,320,330,500,900,20000};
  public static ChessMove Choose(ChessPosition position,int milliseconds=650,int maxDepth=3)
  {
   var watch=Stopwatch.StartNew();int nodes=0;var legal=position.LegalMoves();if(legal.Count==0)throw new InvalidOperationException("No legal move");var best=legal[0];
   for(int depth=1;depth<=maxDepth;depth++){int score=int.MinValue;var candidate=best;bool complete=true;foreach(var move in Order(position,legal)){if(watch.ElapsedMilliseconds>milliseconds||nodes>20000){complete=false;break;}int value=-Search(position.After(move),depth-1,-30000,30000,watch,milliseconds,ref nodes,1);if(value>score){score=value;candidate=move;}}if(complete)best=candidate;else break;}
   return best;
  }
  static IEnumerable<ChessMove> Order(ChessPosition p,List<ChessMove> moves)=>moves.OrderByDescending(m=>Values[Math.Abs(p.Board[m.To])]*10-Values[Math.Abs(p.Board[m.From])]+m.Promotion*100);
  static int Search(ChessPosition p,int depth,int alpha,int beta,Stopwatch watch,int limit,ref int nodes,int ply)
  {
   nodes++;var legal=p.LegalMoves();if(legal.Count==0)return p.InCheck(p.Side)?-29000+ply:0;if(p.Insufficient()||p.Halfmove>=150)return 0;
   if(depth<=0||watch.ElapsedMilliseconds>limit||nodes>20000)return Evaluate(p);
   foreach(var m in Order(p,legal)){int score=-Search(p.After(m),depth-1,-beta,-alpha,watch,limit,ref nodes,ply+1);if(score>=beta)return beta;alpha=Math.Max(alpha,score);}return alpha;
  }
  static int Evaluate(ChessPosition p){int value=0;for(int s=0;s<64;s++){int piece=p.Board[s];if(piece==0)continue;int type=Math.Abs(piece),rank=piece>0?s/8:7-s/8;int center=7-Math.Abs(2*(s%8)-7)-Math.Abs(2*(s/8)-7);value+=Math.Sign(piece)*(Values[type]+(type==1?rank*8:(type==2||type==3?center*5:0)));}return value*p.Side;}
 }
}
