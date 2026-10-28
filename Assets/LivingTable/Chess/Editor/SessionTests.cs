#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using LivingTable.Chess;
using LivingTable.CandyLand;
namespace LivingTable.Editor
{
 public static class SessionTests
 {
  static void Check(bool ok,string message){if(!ok)throw new Exception("SESSION TEST: "+message);}
  static ChessMove Move(ChessMatch m,int from,int to)=>m.Position.LegalMoves().Single(x=>x.From==from&&x.To==to);
  public static void Run()
  {
   GameSessionStore.TestRoot=Path.GetFullPath("Logs/session-tests-"+Guid.NewGuid().ToString("N"));
   var match=new ChessMatch();match.Play(Move(match,12,28));match.Play(Move(match,52,36));match.Play(Move(match,6,21));
   var restored=ChessMatch.Restore(JsonUtility.FromJson<ChessMatch.State>(JsonUtility.ToJson(match.Capture())));
   Check(restored.Position.Key()==match.Position.Key()&&restored.Position.Ply==3,"Chess full position and turn restore");
   var repeat=new ChessMatch();for(int i=0;i<2;i++){repeat.Play(Move(repeat,6,21));repeat.Play(Move(repeat,62,45));repeat.Play(Move(repeat,21,6));repeat.Play(Move(repeat,45,62));}Check(ChessMatch.Restore(repeat.Capture()).CanClaim,"Repetition history retained");
   var data=JsonUtility.FromJson<CandyBoardData>(File.ReadAllText("Assets/LivingTable/CandyLand/Resources/candy-board-1984.json"));
   var candy=new CandyGame(data,4,829);for(int i=0;i<7;i++){candy.Draw();candy.Commit(candy.Preview().To);}var pending=candy.Draw();var clone=CandyGame.Restore(data,JsonUtility.FromJson<CandyGame.State>(JsonUtility.ToJson(candy.Capture())));
   Check(clone.PendingCard.Value.Label==pending.Label&&clone.Turn==7&&clone.CurrentPlayer==3,"Pending card and exact turn restored");
   candy.Commit(candy.Preview().To);clone.Commit(clone.Preview().To);clone=CandyGame.Restore(data,JsonUtility.FromJson<CandyGame.State>(JsonUtility.ToJson(clone.Capture())));Check(!clone.HasPending,"Empty pending survives Unity JSON serialization");for(int i=0;i<300&&candy.Winner<0;i++){Check(candy.Draw().Label==clone.Draw().Label,"Same remaining deck and future shuffle");candy.Commit(candy.Preview().To);clone.Commit(clone.Preview().To);}Check(candy.Winner==clone.Winner,"Same game outcome after resume");
   var physical=new CandyGame(data,2,4);physical.ObserveCard(new CandyCard(CandyColor.Picture,1,"Plumpy"));physical.Commit(physical.Preview().To);bool rejected=false;try{physical.ObserveCard(new CandyCard(CandyColor.Picture,1,"Plumpy"));}catch(InvalidOperationException){rejected=true;}Check(rejected&&physical.DrawCount+physical.DiscardCount==64,"Physical card cannot be reused by switching to virtual");
   string id=GameSessionStore.NewId();GameSessionStore.Save(id,"chess","start","one");GameSessionStore.Save(id,"chess","handoff","two");Check(GameSessionStore.Load(id).revision==2&&GameSessionStore.Load(id).payload=="two","Append-only checkpoint");
   string path=Path.Combine(GameSessionStore.Root,id,"00000001.json");File.WriteAllText(path,File.ReadAllText(path).Replace("one","changed"));rejected=false;try{GameSessionStore.Load(id);}catch(InvalidDataException){rejected=true;}Check(rejected,"Corrupt history refused without reset");
   var appearance=Enumerable.Range(0,64).Select(_=>new float[12]).ToArray();var changed=appearance.Select(a=>(float[])a.Clone()).ToArray();Check(ChessPhysicalVision.Matches(appearance,changed),"Matching board appearance");changed[25][0]=1;Check(!ChessPhysicalVision.Matches(appearance,changed),"Changed board blocks reconciliation");
   File.WriteAllText("Logs/Session-rules-tests.txt","PASS: chess complete replay and repetition, Candy pending draw/deck/RNG continuation, physical deck conservation, append-only revisions, corrupt journal refusal, board appearance mismatch.\n");Debug.Log("SESSION_RULES_TESTS_PASSED");
   SessionState.SetString("SessionTestRoot",GameSessionStore.TestRoot);EditorSceneManager.OpenScene("Assets/LivingTable/CandyLand/CandyLandDesktop.unity");SessionState.SetBool("SessionSmoke",true);EditorApplication.delayCall+=()=>EditorApplication.EnterPlaymode();
  }
  static int stage;static double deadline;static ChessTable chess;static CandyTable candy;static string idSaved;
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("SessionSmoke",false))return;GameSessionStore.TestRoot=SessionState.GetString("SessionTestRoot","");stage=0;deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;}
  static void Tick(){try{if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Session scene timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;var experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();if(experience==null)return;
   if(stage==0){candy=UnityEngine.Object.FindFirstObjectByType<CandyTable>();if(candy==null||candy.Data==null)return;experience.SelectChess();chess=UnityEngine.Object.FindFirstObjectByType<ChessTable>();chess.Configure(false,true,1);chess.StartGame();idSaved=chess.SessionId;chess.Play(Move(chess.Match,12,28));stage=1;}
   else if(stage==1){if(chess.Busy)return;int ply=chess.Match.Position.Ply;chess.SwitchOpponent();Check(!chess.TwoPlayers&&chess.Match.Position.Ply==ply&&chess.SessionId==idSaved,"AI takeover preserves identity/position");chess.SwitchOpponent();chess.SwitchSurface();Check(chess.Recovering&&chess.Physical&&chess.Match.Position.Ply==1,"Physical transition locks until reconciliation");Check(!chess.Play(Move(chess.Match,52,36)),"Cannot bypass physical reconciliation");chess.NewGame();chess.ResumeSession(idSaved);Check(chess.Match!=null,"Resume succeeded: "+chess.Message);Check(chess.Match.Position.Ply==1&&chess.Recovering&&!chess.Aligned,"Relaunch retains board and requires alignment");chess.SwitchSurface();Check(!chess.Physical&&chess.Match.Position.Ply==1,"Virtual continuation preserves position");chess.NewGame();var pendingMatch=new ChessMatch();pendingMatch.Play(Move(pendingMatch,12,28));var pendingState=new ChessTable.SessionState{match=pendingMatch.Capture(),physical=true,twoPlayers=false,humanSide=1,promotion=5,hasPending=true,pending=new ChessMatch.SavedMove(Move(pendingMatch,52,36))};string aiId=GameSessionStore.NewId();GameSessionStore.Save(aiId,"chess","AI pending",JsonUtility.ToJson(pendingState));chess.ResumeSession(aiId);Check(chess.PendingAi.HasValue&&chess.Match.Position.Ply==1,"AI proposal persisted without committing physical move");chess.SwitchOpponent();Check(chess.PendingAi.HasValue,"Returning human cannot erase AI proposal");chess.TogglePause();chess.SwitchSurface();Check(chess.PendingAi.HasValue&&chess.Match.Position.Ply==1,"Paused virtual transfer retains unexecuted AI proposal");chess.TogglePause();var wrong=Move(chess.Match,51,35);Check(!chess.Play(wrong)&&chess.Match.Position.Ply==1,"Different move cannot replace locked AI proposal");chess.NewGame();chess.Configure(false,true,1);var p=ChessPosition.FromFen("4k3/8/8/8/8/8/4P3/4K3 w - - 0 1");Check(chess.ImportPosition(p)&&chess.Unverified&&chess.Match.Position.Board[12]==1,"External game import explicitly unverified");chess.NewGame();experience.SelectCandy();candy.Configure(1,false,false,false);candy.StartGame(93);idSaved=candy.SessionId;candy.DrawVirtual();Check(GameSessionStore.Load(idSaved).payload.Contains("pending"),"Draw saved before animation");stage=2;}
   else if(stage==2){if(candy.Phase!=CandyPhase.Ready)return;int turn=candy.Game.Turn;var deck=JsonUtility.ToJson(candy.Game.Capture());candy.SwitchOpponent();Check(candy.TwoHumans&&JsonUtility.ToJson(candy.Game.Capture())==deck,"Candy human handoff retains all state");candy.SwitchSurface();Check(candy.SessionRecovering,"Candy physical requires reconciliation");candy.ReturnToSetup();candy.ResumeSession(idSaved);Check(candy.Game.Turn==turn&&candy.SessionRecovering,"Candy persistent resume");candy.SwitchSurface();Check(!candy.PhysicalOverlay&&candy.Game.Turn==turn&&candy.SessionId==idSaved,"Candy virtual continuation");candy.ReturnToSetup();Finish(0,"SESSION_SCENE_TESTS_PASSED");}
  }catch(Exception e){Finish(1,e.ToString());}}
  static void Finish(int code,string text){SessionState.SetBool("SessionSmoke",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(text);else Debug.LogError(text);EditorApplication.Exit(code);}
 }
}
#endif
