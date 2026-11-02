using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
namespace LivingTable.Chess {
 public sealed partial class ChessTable {
  bool companionOpen;string companionText="Choose a question. Your game is held while we talk.";ChessCompanion companion;int companionRequest;
  public bool CompanionOpen=>companionOpen;public string CompanionDisplay=>companionText;
  public static string MoveCode(ChessMove m)=>ChessMove.Square(m.From)+ChessMove.Square(m.To)+(m.Promotion==0?"":" pnbrqk"[m.Promotion].ToString());
  public void OpenCompanion(){if(!Playing||Busy||StorageBlocked)return;companionOpen=true;sessionMenu=true;Version++;CancelGrab();Vision.ClearEvidence();companionText="Choose a question. Your game is held while we talk.";SaveSession("companion opened");DrawUI();CheckCompanionConnection();}
  void CheckCompanionConnection(){int requestId=++companionRequest;companionText="Checking local Meta connection...";companion.CheckConnection(ok=>{if(companionOpen&&requestId==companionRequest)companionText=companion.Status;});}
  public void CloseCompanion(){companionRequest++;companion?.Cancel();companionOpen=false;sessionMenu=false;Version++;SaveSession("companion closed");DrawUI();}
  void AskCompanion(string kind){if(!companionOpen||StorageBlocked)return;companion?.Cancel();int requestId=++companionRequest;companionText="Meta Llama is thinking... Your board stays unchanged.";StartCoroutine(PrepareQuestion(kind,requestId));}
  IEnumerator PrepareQuestion(string kind,int requestId){
   int revision=Version;string id=SessionId;var position=Match.Position.Copy();string key=position.Key();var legal=position.LegalMoves();ChessMove? chosen=PendingAi;
   if(!chosen.HasValue&&legal.Count>0&&!GameOver){var task=Task.Run(()=>ChessAi.Choose(position,350,3));yield return new WaitUntil(()=>task.IsCompleted);if(task.IsFaulted){companionText="The chess engine is unavailable. Close this panel and retry.";yield break;}chosen=task.Result;}
   if(!companionOpen||requestId!=companionRequest)yield break;if(Version!=revision||SessionId!=id){companionText="The game changed. Ask again for the current position.";yield break;}
   var facts=new List<ChessCompanion.Fact>();Action<string> add=t=>facts.Add(new ChessCompanion.Fact("f"+facts.Count,t));
   add("Side to move: "+SideName(position.Side)+". Result: "+(GameOver?Match.Result:"game ongoing")+".");
   add("Player color: "+SideName(HumanSide)+". Opponent: "+(TwoPlayers?"human":"chess AI")+". Board: "+(Physical?"physical":"virtual")+".");
   add("Last move: "+(Match.LastMove.HasValue?Match.LastMove.Value.ToString():"no move recorded in this game")+".");
   add("In check: "+position.InCheck(position.Side)+". Completed plies: "+position.Ply+".");
   add("Continue: "+(GameOver?"press PLAY AGAIN for a fresh game":Paused?"close this panel and press RESUME":Recovering?"close this panel, align the real board if needed, arrange the saved position and press SCAN BOARD; do not move until verified":PendingAi.HasValue?"move the physical AI piece exactly as instructed, then clear hands":!TwoPlayers&&position.Side!=HumanSide?"close this panel and let the chess AI prepare its turn":Physical?(ManualFallback?"use the explicitly marked manual source/destination controls":"move the real piece, clear hands and wait for camera verification"):"close this panel, pinch your piece and place it on a legal glowing square")+".");
   if(chosen.HasValue){var move=chosen.Value;add((PendingAi.HasValue?"Locked AI instruction: ":"Engine suggestion (not guaranteed best): ")+MoveInstruction(position,move));var after=position.After(move);add("Suggested move gives check: "+after.InCheck(after.Side)+". It checkmates: "+(after.InCheck(after.Side)&&after.LegalMoves().Count==0)+".");}
   add("The companion cannot change moves, history, opponent or board mode. Use MENU for human/AI or physical/virtual handoff.");
   for(int square=0;square<64;square++)if(position.Board[square]!=0)add(SideName(position.Board[square])+" "+PieceName(position.Board[square])+" on "+ChessMove.Square(square)+".");
   var context=new ChessCompanion.Context{sessionId=id,revision=revision,positionKey=key,kind=kind,legalMoves=legal.Select(MoveCode).ToArray(),suggestedMove=chosen.HasValue?MoveCode(chosen.Value):"",facts=facts.ToArray()};
   companion.Ask(context,answer=>{if(!companionOpen||requestId!=companionRequest)return;if(Version!=revision||SessionId!=id||Match.Position.Key()!=key){companionText="The game changed. Ask again for the current position.";return;}
    bool valid=answer!=null&&answer.sessionId==id&&answer.revision==revision&&answer.positionKey==key&&answer.model!=null&&answer.model.StartsWith("llama")&&!string.IsNullOrWhiteSpace(answer.answer)&&answer.answer.Length<=500&&answer.factIds!=null&&answer.factIds.Length>0&&answer.factIds.All(f=>facts.Any(x=>x.id==f))&&(answer.move==""||answer.move==context.suggestedMove&&context.legalMoves.Contains(answer.move));
    companionText=valid?"META LLAMA - AI EXPLANATION\n"+answer.answer+(kind=="continue"?"\nVerified next step: "+facts[4].text:kind=="what"?"\n"+facts[2].text:"")+(answer.move!=""?"\nEngine move: "+answer.move:""):answer==null?companion.Status+" Your game is unchanged.":"Meta reply did not match the current engine facts. Retry; your game is unchanged.";
   });
  }
 }
}
