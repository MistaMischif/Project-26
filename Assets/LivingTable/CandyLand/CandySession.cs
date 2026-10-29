using System;
using UnityEngine;

namespace LivingTable.CandyLand
{
 public sealed partial class CandyTable
 {
  [Serializable] public sealed class SessionState
  {
   public CandyGame.State game;public bool physical,manual,twoHumans,camera,fallback,unverified,room,shortcutPassed;
   public int opponents,color,boardRevision;public float timer;
  }
  public string SessionId {get;private set;}
  public bool SessionRecovering {get;private set;}
  public bool SessionUnverified {get;private set;}
  public bool StorageBlocked {get;private set;}
  public bool VirtualRoom {get;private set;}
  bool pendingShortcutPassed,sessionMenu,sessionMenuWasPaused;int savedIndex;GameSessionStore.Entry[] saved=Array.Empty<GameSessionStore.Entry>();
  public bool SaveSession(string reason)
  {
   if(EffectsTestMode||Game==null||SessionId==null)return true;
   var state=new SessionState{boardRevision=2,game=Game.Capture(),physical=PhysicalOverlay,manual=Manual,twoHumans=TwoHumans,camera=CameraEnabled,fallback=VisionFallback,unverified=SessionUnverified,room=VirtualRoom,opponents=Opponents,color=PlayerColor,timer=ConfirmationSeconds,shortcutPassed=pendingShortcutPassed};
   try{GameSessionStore.Save(SessionId,"candy-land-1984",reason,JsonUtility.ToJson(state));StorageBlocked=false;return true;}
   catch(Exception e){StorageBlocked=true;Paused=true;Message="Session save failed. Play locked: "+e.Message;Debug.LogError("[Session] "+e.Message);if(ui!=null)BuildUI();return false;}
  }
  public static void MigrateBoardState(SessionState state)
  {
   if(state==null||state.game==null)throw new ArgumentException("Missing saved state");
   if(state.boardRevision>=2)return;
   // Preserve the identity of the space occupied under the former reversed pair.
   if(state.game.positions!=null)for(int i=0;i<state.game.positions.Length;i++)
    if(state.game.positions[i]==17)state.game.positions[i]=18;else if(state.game.positions[i]==18)state.game.positions[i]=17;
   state.boardRevision=2;
  }
  void RefreshSaved(){saved=GameSessionStore.List("candy-land-1984");savedIndex=0;}
  public void ResumeSession(string id)
  {
   try{var record=GameSessionStore.Load(id);if(record.game!="candy-land-1984")throw new ArgumentException("Wrong game");var state=JsonUtility.FromJson<SessionState>(record.payload);MigrateBoardState(state);var restored=CandyGame.Restore(Data,state.game);
    if(state.opponents<1||state.opponents>3||state.color<0||state.color>3)throw new ArgumentException("Invalid saved players");if(Game!=null&&!SaveSession("switch saved session"))return;StopAllCoroutines();buttonBusy=false;ReleasePieceGrabs(true);EffectsTestMode=false;Game=restored;SessionId=id;PhysicalOverlay=state.physical;Manual=state.manual;TwoHumans=state.twoHumans;CameraEnabled=state.camera;VisionFallback=state.fallback;SessionUnverified=state.unverified;VirtualRoom=state.room;Opponents=state.opponents;PlayerColor=state.color;ConfirmationSeconds=state.timer;pendingShortcutPassed=state.shortcutPassed;
    StorageBlocked=false;sessionMenu=false;Paused=false;candidate=null;lastDrawn=Game.PendingCard;SessionRecovering=PhysicalOverlay;Phase=Game.Winner>=0?CandyPhase.Finished:CandyPhase.Ready;Vision.Invalidate();Cards.Clear();MoveMagic?.Clear();music.SetGameplay(true);RefreshSessionView();
    foreach(var card in state.game.discard)Cards.RestoreSettled(CreateCard(card.Card,true),Width);
    if(!SessionRecovering)ContinuePending();Message=SessionRecovering?"Recreate the saved piece positions. MANUAL CHECK records your confirmation.":"Saved game restored, including the deck and pending card.";
   }catch(Exception e){Message="Cannot resume: "+e.Message;}
  }
  void RefreshSessionView()
  {
   boardFace.SetActive(!PhysicalOverlay);boardBody.SetActive(!PhysicalOverlay);BuildPawns(Game.Players);
   for(int i=0;i<Game.Players;i++){pawns[i].localPosition=SpacePosition(Game.Position(i),i);if(SessionRecovering)pawns[i].gameObject.SetActive(true);if(Game.Position(i)>=0)pawns[i].GetComponent<CandyPlayerPiece>().SetSludged(Data.spaces[Game.Position(i)].releaseColor==(int)CandyColor.Red);}
   ArrangePawnsOnSpaces();FindFirstObjectByType<LivingTableExperience>()?.SetVirtualRoom(VirtualRoom&&!PhysicalOverlay);BuildUI();
  }
  void ContinuePending()
  {
   if(Game.HasPending){lastPlayer=Game.CurrentPlayer;Phase=CandyPhase.AwaitingMove;BeginMove();}
   else{Phase=Game.Winner>=0?CandyPhase.Finished:CandyPhase.Ready;aiAt=Time.unscaledTime+2.2f;}
   BuildUI();
  }
  public void SwitchOpponent()
  {
   if(Game==null||StorageBlocked)return;TwoHumans=!TwoHumans;if(TwoHumans&&PhysicalOverlay&&!Manual){Manual=true;SessionRecovering=true;RefreshSessionView();}SaveSession("opponent control changed; same players and pending card");BuildUI();
  }
  public void SwitchSurface()
  {
   if(Game==null||StorageBlocked)return;if(Phase==CandyPhase.Animating||Phase==CandyPhase.Revealing){Message="Finish the current animation, then switch. Its card is already saved.";return;}
   StopAllCoroutines();buttonBusy=false;candidate=null;CardRevision++;ReleasePieceGrabs(true);PhysicalOverlay=!PhysicalOverlay;Manual=PhysicalOverlay;CameraEnabled=PhysicalOverlay;VisionFallback=false;SessionRecovering=PhysicalOverlay;Vision.Invalidate();
   if(PhysicalOverlay){VirtualRoom=false;Message="Arrange every real piece at its saved position. Confirm with MANUAL CHECK.";}
   SaveSession("surface changed; game and deck retained");RefreshSessionView();if(!SessionRecovering)ContinuePending();
  }
  public void AttestSessionBoard()
  {
   if(!SessionRecovering||StorageBlocked)return;SessionUnverified=true;SessionRecovering=false;
   if(CameraEnabled&&!Vision.Ready)VisionFallback=true;
   SaveSession("manual board reconciliation; not camera verified");RefreshSessionView();ContinuePending();Message="Manual board check recorded. Continue from the saved turn.";
  }
  void ToggleRoom(){if(PhysicalOverlay||StorageBlocked)return;VirtualRoom=!VirtualRoom;FindFirstObjectByType<LivingTableExperience>()?.SetVirtualRoom(VirtualRoom);SaveSession("view changed");BuildUI();}
  void AddSessionButton(string text,int col,int row,Action action){AddRound(text,col-1,action,new Color(.4f,.15f,.8f));}
  void SessionUI()
  {
   if(EffectsTestMode)return;
   if(Phase==CandyPhase.Setup){if(saved.Length>0){AddButton("RESUME SAVED",0,8,()=>ResumeSession(saved[savedIndex].id));AddButton("SAVE "+(savedIndex+1)+" / "+saved.Length,1,8,()=>{savedIndex=(savedIndex+1)%saved.Length;Message="Saved "+saved[savedIndex].utc;BuildUI();});}return;}
   if(!sessionMenu){AddRound("SESSION",5,()=>{sessionMenuWasPaused=Paused;Paused=true;sessionMenu=true;BuildUI();},new Color(.4f,.15f,.8f));return;}
   AddRound("BACK",-2,()=>{sessionMenu=false;if(!StorageBlocked)Paused=sessionMenuWasPaused;BuildUI();},new Color(.25f,.4f,.65f));
   AddSessionButton(TwoHumans?"AI TAKE OVER":"HUMANS RETURN",0,8,SwitchOpponent);AddSessionButton(PhysicalOverlay?"GO VIRTUAL":"USE REAL BOARD",1,8,SwitchSurface);
   if(!PhysicalOverlay)AddSessionButton(VirtualRoom?"SEE ROOM":"VIRTUAL ROOM",2,8,ToggleRoom);
   if(SessionRecovering){AddSessionButton("MANUAL CHECK",2,8,AttestSessionBoard);AddSessionButton("ALIGN BOARD",3,8,()=>{calibrationStage=1;Message="Pinch bottom-left, then bottom-right board corner.";});}
   if(StorageBlocked)AddSessionButton("RETRY SAVE",4,8,()=>{if(SaveSession("save retry")){RefreshSessionView();if(!SessionRecovering)ContinuePending();Message="Saved. Resume to continue.";}});
  }
 }
}
