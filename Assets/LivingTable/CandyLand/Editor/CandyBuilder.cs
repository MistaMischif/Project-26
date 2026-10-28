#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using LivingTable.CandyLand;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LivingTable.Editor
{
    public static class CandyBuilder
    {
        const string Folder="Assets/LivingTable/CandyLand";
        public static void PrepareAndTest()
        {
            CandyCardMagicTests.Run();
        }
        static void CreateCopy(string source,string destination,bool desktop)
        {
            // Re-running tests never regenerates or overwrites an existing authored scene.
            if(File.Exists(destination)) return;
            var scene=EditorSceneManager.OpenScene(source);
            EditorSceneManager.SaveScene(scene,destination,true);
            scene=EditorSceneManager.OpenScene(destination);
            var experience=UnityEngine.Object.FindFirstObjectByType<LivingTableExperience>();
            if(experience==null) throw new Exception("Astra experience not found in source scene.");
            experience.candyLandMode=true;experience.desktopPreview=desktop;
            if(desktop)
            {
                var camera=Camera.main; camera.transform.position=new Vector3(-.15f,1.85f,-.65f);camera.transform.LookAt(new Vector3(-.15f,.75f,-.05f));
                camera.fieldOfView=55;
            }
            EditorSceneManager.SaveScene(scene);
        }
        static void Check(bool condition,string message) {if(!condition)throw new Exception("CANDY TEST: "+message);}
        public static void RulesTests()
        {
            var data=JsonUtility.FromJson<CandyBoardData>(File.ReadAllText(Folder+"/Resources/candy-board-1984.json"));data.Validate();
            var game=new CandyGame(data,4,42);
            Check(data.spaces.Length==134,"Path should contain 134 traced spaces.");
            Check(game.DrawCount==64,"Deck count.");
            Check(game.Resolve(-1,new CandyCard(CandyColor.Red)).To==0,"First red.");
            Check(game.Resolve(-1,new CandyCard(CandyColor.Red,2)).To==6,"Double red.");
            Check(game.Resolve(-1,new CandyCard(CandyColor.Orange)).To==58,"Rainbow shortcut.");
            Check(game.Resolve(32,new CandyCard(CandyColor.Purple)).To==46,"Gumdrop shortcut.");
            Check(game.Resolve(120,new CandyCard(CandyColor.Blue)).Stuck,"Swamp requires red.");
            Check(game.Resolve(120,new CandyCard(CandyColor.Red)).To==126,"Swamp red release.");
            Check(game.Resolve(120,new CandyCard(CandyColor.Red,2)).To==132,"Swamp double red release.");
            Check(game.Resolve(85,new CandyCard(CandyColor.Blue)).To==91,"Lollipop blue release.");
            Check(game.Resolve(47,new CandyCard(CandyColor.Yellow)).To==53,"Gumdrops yellow release.");
            Check(game.Resolve(120,new CandyCard(CandyColor.Picture,1,"Plumpy")).Stuck,"Picture card cannot bypass color penalty.");
            Check(game.Resolve(80,new CandyCard(CandyColor.Picture,1,"Plumpy")).To==8,"Backward picture move.");
            Check(game.Resolve(132,new CandyCard(CandyColor.Purple)).Won,"Exact purple finish.");
            Check(game.Resolve(132,new CandyCard(CandyColor.Blue)).Won,"Beyond-end finish.");
            game.ObserveCard(new CandyCard(CandyColor.Red));
            bool rejected=false;try{game.Commit(5);}catch(InvalidOperationException){rejected=true;}
            Check(rejected&&game.CurrentPlayer==0&&game.HasPending,"Reject wrong physical destination without changing turn.");
            game.Commit(0);game.ObserveCard(new CandyCard(CandyColor.Red));game.Commit(0);
            Check(game.Position(0)==game.Position(1),"Shared spaces allowed.");
            rejected=false;try{game.ObserveCard(new CandyCard(CandyColor.Picture,1,"unknown"));}catch(ArgumentException){rejected=true;}
            Check(rejected&&!game.HasPending,"Unknown recognition cannot commit.");
            int reshuffles=0,maxTurns=0;
            for(int seed=0;seed<250;seed++)
            {
                var g=new CandyGame(data,2+seed%3,seed);
                while(g.Winner<0&&g.Turn<5000)
                {
                    if(g.DrawCount==0)reshuffles++;
                    g.Draw();var result=g.Preview();g.Commit(result.To);
                    Check(g.DrawCount+g.DiscardCount==64,"Deck conservation.");
                    for(int player=0;player<g.Players;player++)Check(g.Position(player)>=-1&&g.Position(player)<134,"Valid player position.");
                }
                Check(g.Winner>=0,"Seeded game must terminate.");maxTurns=Math.Max(maxTurns,g.Turn);
            }
            Check(reshuffles>0,"Exercise reshuffling.");
            File.WriteAllText("Logs/CandyLand-rules-tests.txt",$"PASS: original movement, both shortcuts, all penalties, picture travel, finish, shared cells, invalid recognition, wrong-position refusal.\n250 complete seeded games, 2–4 players. Reshuffles: {reshuffles}; max turns: {maxTurns}. Deck conserved at 64 cards.\nPhysical route and camera accuracy still require on-device validation.\n");
            Debug.Log("CANDY_RULES_TESTS_PASSED");
        }
        static double deadline,magicWait,moveGameStarted;
        static bool poofCaptured;
        static Vector3 treatPoint,treatReleased;
        static CandyCharacterPlay testedPlay;static float playPausedAt;
        static Vector3 stretchPoint,stretchOrigin;static CandyTestHandFeed stretchFeed;
        static float pausedMagic;static Vector3 pausedActor;static CandyCharacterActor testedActor;
        static int stage;
        static Vector3 playerStart,opponentStart;
        [InitializeOnLoadMethod] static void Resume()
        {
            if(!SessionState.GetBool("CandySmoke",false))return;
            deadline=EditorApplication.timeSinceStartup+240;stage=0;EditorApplication.update+=Tick;
        }
        static void Tick()
        {
            try
            {
                if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Candy scene test timed out.");
                if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
                var table=UnityEngine.Object.FindFirstObjectByType<CandyTable>();if(table==null||table.Data==null)return;
                if(stage==0)
                {
                    var button=table.GetComponentsInChildren<TableButtonVisual>().First(b=>b.name=="VIRTUAL + AI button");
                    table.HoverWorld(button.transform.position);Check(button.Hovered,"Tracked hover highlights target.");
                    float height=button.transform.Find("Moving button cap").localPosition.y;
                    table.TouchWorld(button.transform.position);Check(button.Pressed&&button.transform.Find("Moving button cap").localPosition.y<height,"Press darkens and depresses before action.");
                    SaveView("Logs/CandyLand-buttons-pressed.png");stage=10;return;
                }
                if(stage==10)
                {
                    if(table.GetComponentsInChildren<TableButtonVisual>().Any(b=>b.Pressed))return;
                    Check(table.VisiblePawnCount==2,"Two visible preview pieces before pressing Play.");
                    var audio=table.GetComponent<TableMusic>();audio.Toggle();Check(!audio.Enabled&&!audio.Source.isPlaying,"Music off stops playback.");audio.Toggle();Check(audio.Enabled&&audio.Source.isPlaying,"Music on explicitly restarts playback.");Check(Mathf.Abs(audio.Source.volume-.11f)<.001f,"Music volume halved.");
                    Check(table.GetComponentsInChildren<TableButtonVisual>().Any(b=>b.name=="PLAY NOW button"),"Large PLAY NOW replaces old play controls.");
                    Check(table.GetComponent<TableMusic>().Source.clip!=null&&table.GetComponent<TableMusic>().Source.loop,"Looping background composition loaded.");
                    Check(Mathf.Abs(table.GetComponent<TableMusic>().Source.clip.length-48)<.1f,"Full 48-second waltz loop.");
                    var savedCamera=Camera.main.transform.position;
                    var pin=table.StatusPanel.transform.position;table.StatusPanel.Pin(pin);Camera.main.transform.position+=Vector3.right*.2f;table.StatusPanel.FaceViewer();
                    Check(Vector3.Distance(table.StatusPanel.transform.position,pin)<.001f,"Pinned panel retains location.");
                    Check(Vector3.Dot(table.StatusPanel.transform.forward,(pin-Camera.main.transform.position).normalized)>.99f,"Panel faces viewer after head movement.");
                    table.StatusPanel.SetFloating(true);Check(table.StatusPanel.Floating,"Panel can float.");table.StatusPanel.SetFloating(false);Camera.main.transform.position=savedCamera;table.StatusPanel.FaceViewer();
                    var plumpy=Resources.Load<GameObject>("Characters/plumpy");Check(plumpy.transform.Find("Sculpted Plumpy")!=null,"Full Plumpy sculpture replaces old asset.");
                    Check(table.GetComponentsInChildren<CandyCharacterActor>().Length==9,"Nine volumetric character figures instantiated.");
                    foreach(string id in CandyCharacterModel.Ids)Check(Resources.Load<Texture2D>("Characters/"+id)!=null,"Character artwork: "+id);
                    Check(table.PlayerName(1)=="Silly Sally"&&table.PlayerName(2)=="Wobbly Walter"&&table.PlayerName(3)=="Giggles Greta","Distinct silly AI names.");
                    table.Configure(3,true,false,true);table.StartGame(42);
                    Check(table.VisiblePawnCount==0,"Physical manual mode does not duplicate real pieces.");Check(table.TurnAnnouncement=="You're up!","First turn announces player.");
                    table.BeginScan();table.TickConfirmation(31);Check(table.ScanRequests==1&&table.Game.Turn==0,"Confirmation timer starts only after a card proposal.");
                    table.SelectPhysicalCard(new CandyCard(CandyColor.Red));int scans=table.ScanRequests;
                    table.TickConfirmation(31);Check(table.ScanRequests==scans+1&&!table.Game.HasPending&&table.Game.Turn==0,"Timer must rescan without guessing.");
                    table.TickConfirmation(31);Check(table.ScanRequests==scans+1,"No repeated automatic rescan loop.");
                    table.SelectPhysicalCard(new CandyCard(CandyColor.Red));table.ConfirmCard();
                    Check(table.Phase==CandyPhase.Revealing&&table.Cards.SettledCount==0,"Confirmed manual card reveals before moving.");
                    Check(table.MoveMagic.LandingColor==new Color(.94f,.16f,.25f),"Ordinary red-card destination uses red sparkles and glow.");
                    stage=11;return;
                }
                else if(stage==11)
                {
                    if(table.Phase==CandyPhase.Revealing)return;
                    Check(table.Phase==CandyPhase.AwaitingMove&&table.Cards.SettledCount==1,"Manual card lands once before physical move.");
                    Check(!table.ConfirmMove(1)&&table.Game.Turn==0,"Wrong physical space rejected.");
                    Check(table.ConfirmMove(0)&&table.Game.Turn==1,"Confirmed physical move advances.");
                    table.ReturnToSetup();table.Configure(1,true,false,true);table.StartVirtualGame(42);
                    Check(!table.Manual&&!table.PhysicalOverlay&&table.VisiblePawnCount==2,"Direct virtual start always creates two visible pieces from physical mode.");
                    var toolbar=table.GetComponentsInChildren<TableButtonVisual>();Check(toolbar.All(b=>Mathf.Abs(b.transform.localPosition.z-toolbar[0].transform.localPosition.z)<.001f),"Gameplay buttons share one horizontal row.");
                    Check(toolbar.All(b=>b.transform.Find("Physical silver bevel")!=null),"Every gameplay button has real bevel geometry.");Check(toolbar.Any(b=>b.name=="MOVE BOARD button"),"Move board available during game.");
                    playerStart=table.PawnWorldPosition(0);opponentStart=table.PawnWorldPosition(1);
                    Check(Vector3.Distance(playerStart,opponentStart)>.045f,"Starting pieces must be separated.");
                    var draw=table.GetComponentsInChildren<TableButtonVisual>().First(b=>b.name=="DRAW CARD button");
                    Check(Mathf.Abs(draw.transform.localPosition.x)<.001f,"Draw button centered.");
                    var drawFace=draw.transform.Find("Moving button cap/Gloss face").GetComponent<MeshRenderer>();Check(drawFace.transform.localScale.x>=.15f,"Oversized circular draw face.");
                    var pressPoint=draw.transform.position+table.transform.right*.08f;
                    table.DirectPressWorld(44,pressPoint);Check(!draw.Pressed,"Unarmed hand cannot accidentally draw.");
                    table.DirectPressWorld(44,pressPoint+table.transform.up*.08f);table.DirectPressWorld(44,pressPoint);
                    Check(draw.Pressed,"Direct press works inside generous outer hit area.");
                    SaveView("Logs/CandyLand-draw-button.png");stage=12;return;
                }
                else if(stage==12)
                {
                    if(table.Phase!=CandyPhase.Revealing)return;Check(table.TurnAnnouncement=="","Turn text clears before card reveal.");
                    var before=table.Cards.CurrentCard;table.BeginRelocation();Check(table.Paused&&table.gameObject.activeInHierarchy,"Board movement pauses without disabling gameplay coroutines.");table.EndRelocation();Check(!table.Paused&&table.Cards.CurrentCard==before,"Board movement preserves current card and pause state.");
                    Check(table.Cards.CurrentCard!=null&&Mathf.Abs(table.Cards.CurrentCard.localPosition.y-table.Width*.25f)<.001f,"Reveal at quarter board-length height.");
                    Check(Mathf.Abs(Vector3.Dot(table.Cards.CurrentCard.up,Vector3.up))<.01f,"Revealed card is perpendicular to board.");
                    SaveView("Logs/CandyLand-card-reveal.png");stage=1;
                }
                else if(stage==1&&table.Phase==CandyPhase.Ready)
                {
                    Check(table.Game.Turn==1,"Virtual animated move committed.");Check(table.TurnAnnouncement.Contains("Silly Sally"),"Next turn announces AI name.");SaveView("Logs/CandyLand-turn-announcement.png");
                    Check(table.Cards.SettledCount==1&&Mathf.Abs(table.Cards.LastDuration-2)<.001f,"Exactly two presentation seconds and one discard.");
                    Check(Vector3.Distance(table.PawnWorldPosition(0),playerStart)>.01f&&table.VisiblePawnCount==2,"Player render geometry actually moved and stayed visible.");stage=3;
                }
                else if(stage==3&&table.Game.Turn==2&&table.Phase==CandyPhase.Ready)
                {
                    Check(table.Game.CurrentPlayer==0&&Vector3.Distance(table.PawnWorldPosition(1),opponentStart)>.01f,"AI piece moved and control returned to the player.");
                    Check(table.Cards.CurrentCard.GetComponentsInChildren<TextMesh>().All(t=>t.GetComponent<Renderer>().sharedMaterial.shader.name=="LivingTable/DepthText"),"Discard labels use depth-tested text.");
                    Check(table.Cards.SettledCount==2&&table.Cards.CurrentCard.localPosition.y>.005f,"Human and AI cards both increase stack height.");
                    Check(Quaternion.Angle(table.Cards.CurrentCard.localRotation,Quaternion.identity)>1&&Quaternion.Angle(table.Cards.CurrentCard.localRotation,Quaternion.identity)<10,"Discard has bounded natural skew.");
                    SaveView("Logs/CandyLand-card-stack.png");
                    table.ReturnToSetup();Check(table.Cards.SettledCount==0&&!table.Cards.IsPresenting,"New game clears presentation.");table.Configure(3,false,false,false);table.StartGame(42);Check(table.VisiblePawnCount==4,"All four colored original-style pieces visible.");stage=2;
                }
                else if(stage==2)
                {
                    var camera=Camera.main;var rt=new RenderTexture(1600,1100,24);camera.targetTexture=rt;camera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,1100,TextureFormat.RGB24,false);
                    image.ReadPixels(new Rect(0,0,1600,1100),0,0);image.Apply();File.WriteAllBytes("Logs/CandyLand-preview.png",image.EncodeToPNG());
                    camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);
                    RenderPiecePreview(table);RenderCharacterPreview();RenderCardPreview(table);CheckDiscardOcclusion(table);
                    File.WriteAllText("Logs/CandyLand-scene-tests.txt","PASS: real Unity Play Mode setup, four players, manual card/position confirmation, no guessing on countdown expiry, exactly one rescan, virtual animated move. Screenshot saved.\nHeadset not tested.\n");
                    CheckCharacterInteractions(table);
                    table.BeginRelocation();pausedMagic=table.MoveMagic.Elapsed;pausedActor=testedActor.transform.localPosition;magicWait=EditorApplication.timeSinceStartup+.3;stage=20;
                }
                else if(stage==20)
                {
                    if(EditorApplication.timeSinceStartup<magicWait)return;
                    Check(table.MoveMagic.Elapsed==pausedMagic&&testedActor.transform.localPosition==pausedActor,"Character reactions and trails freeze while repositioning.");table.EndRelocation();magicWait=EditorApplication.timeSinceStartup+.6;stage=21;
                }
                else if(stage==21)
                {
                    if(EditorApplication.timeSinceStartup<magicWait)return;
                    Check(table.MoveMagic.Elapsed>pausedMagic,"Magic resumes after relocation.");var art=table.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Supplied board artwork - virtual mode");bool wasActive=art.gameObject.activeSelf;art.gameObject.SetActive(true);SaveView("Logs/CandyLand-magic-trail.png");SaveDetail("Logs/CandyLand-magic-detail.png",table.MoveMagic.transform.TransformPoint(table.MoveMagic.To)+Vector3.up*.025f);table.MoveMagic.SquareGlow.gameObject.SetActive(false);SaveDetail("Logs/CandyLand-magic-without-glow.png",table.MoveMagic.transform.TransformPoint(table.MoveMagic.To)+Vector3.up*.025f);table.MoveMagic.SquareGlow.gameObject.SetActive(true);art.gameObject.SetActive(wasActive);magicWait=EditorApplication.timeSinceStartup+2;stage=22;
                }
                else if(stage==22)
                {
                    if(EditorApplication.timeSinceStartup<magicWait)return;
                    Check(table.GetComponentsInChildren<CandyCharacterActor>().First(a=>a.CharacterId==1).ReactionCount>0,"Mr Mint celebrates trail arrival.");
                    table.ReturnToSetup();Check(table.MoveMagic.LiveCount==0,"New game clears all magic.");
                    table.Configure(1,false,false,false);table.StartGame(42);
                    CheckSludge(table);
                    testedActor=table.GetComponentsInChildren<CandyCharacterActor>().First(a=>a.CharacterId==7);stretchOrigin=testedActor.transform.localPosition;stretchPoint=testedActor.transform.position+table.transform.up*.04f;
                    Check(table.BeginCharacterStretch(93,stretchPoint),"Pinch captures character.");Check(!table.BeginCharacterStretch(94,stretchPoint),"A second hand cannot steal a captured character.");
                    stretchFeed=new GameObject("Synthetic character pinch").AddComponent<CandyTestHandFeed>();stretchFeed.table=table;stretchFeed.hand=93;stretchFeed.point=stretchPoint+table.transform.right*.3f+table.transform.up*.3f;stage=23;
                }
                else if(stage==23)
                {
                    if(stretchFeed.elapsed<.4f)return;
                    Check(testedActor.StretchRatio.y>2&&testedActor.StretchRatio.y<=3.01f&&Mathf.Abs(testedActor.StretchRatio.x-1)<.001f&&Mathf.Abs(testedActor.StretchRatio.z-1)<.001f,"Upward pulls stretch height only, never outward. Ratio="+testedActor.StretchRatio+", held="+testedActor.Stretching);
                    Check(Vector3.Distance(testedActor.transform.localPosition,stretchOrigin)<.001f,"Stretch keeps feet anchored.");SaveView("Logs/CandyLand-character-stretch.png");
                    stretchFeed.enabled=false;UnityEngine.Object.Destroy(stretchFeed.gameObject);table.UpdateCharacterStretch(93,stretchPoint,true);magicWait=EditorApplication.timeSinceStartup+.8;stage=24;
                }
                else if(stage==24)
                {
                    if(EditorApplication.timeSinceStartup<magicWait)return;
                    Check(!testedActor.Stretching&&Vector3.Distance(testedActor.StretchRatio,Vector3.one)<.02f,"Released stretch returns smoothly to original shape.");
                    Check(table.BeginCharacterStretch(93,stretchPoint),"Character can be pinched again.");table.ForgetDirectHand(93);Check(!testedActor.Stretching,"Tracking loss releases character.");
                    Check(table.BeginCharacterStretch(93,stretchPoint),"Pinch after tracking restoration.");table.BeginRelocation();Check(!testedActor.Stretching,"Board relocation cancels stretch.");table.EndRelocation();
                    CheckPiecePlay(table);stage=25;
                }
                else if(stage==25)
                {
                    if(table.Phase!=CandyPhase.Ready)return;
                    Check(table.GetComponent<TableMusic>().Source.clip.name=="candy-soft-shuffle","Gameplay switches to warm background music.");
                    foreach(var id in CandyCharacterModel.Ids)for(int i=0;i<2;i++)Check(Resources.Load<AudioClip>("Audio/Voices/"+id+"-"+i)!=null,"Bundled character voice "+id);
                    poofCaptured=false;testedPlay=TriggerPlay(table,0);Check(testedPlay.EffectCount==4&&testedPlay.HasPlayedVoice,"Plumpy drops four berries and speaks offline.");stage=26;
                }
                else if(stage==26)
                {
                    if(testedPlay.EffectAge<.7f)return;
                    Check(testedPlay.EffectRoot.GetComponentsInChildren<Transform>().Any(t=>t.name=="Rolling purple berry"&&t.localPosition.y<.02f),"Berries fall to the board and roll.");SaveDetail("Logs/CandyLand-plumpy-berries.png",testedPlay.transform.position+Vector3.up*.015f);
                    table.BeginRelocation();playPausedAt=testedPlay.EffectAge;magicWait=EditorApplication.timeSinceStartup+.3;stage=27;
                }
                else if(stage==27)
                {
                    if(EditorApplication.timeSinceStartup<magicWait)return;Check(testedPlay.EffectAge==playPausedAt,"Character props pause during board movement.");table.EndRelocation();stage=28;
                }
                else if(stage==28)
                {
                    if(!poofCaptured&&testedPlay.EffectRoot!=null&&testedPlay.EffectAge>1.8f)
                    {
                        var puff=testedPlay.EffectRoot.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Tiny berry poof"&&t.localScale.x>.0001f);
                        if(puff!=null){Check(puff.localScale.x<.015f,"Berry poof remains very small.");SaveDetail("Logs/CandyLand-berry-poof.png",puff.position);poofCaptured=true;}
                    }
                    if(testedPlay.EffectCount>0)return;Check(poofCaptured,"Berries end with visible tiny poof and sparkles.");testedPlay=TriggerPlay(table,7);stage=29;
                }
                else if(stage==29)
                {
                    if(testedPlay.EffectAge<.5f)return;
                    Check(testedPlay.EffectCount==4&&testedPlay.EffectRoot.GetComponentsInChildren<AudioSource>().All(a=>a.isPlaying),"Each flying bat has its own flap audio.");
                    Check(testedPlay.EffectRoot.GetComponentsInChildren<Transform>().Any(t=>t.name=="Flying bat"&&t.localPosition.y>.02f),"Bats rise off the board.");SaveDetail("Logs/CandyLand-licorice-bats.png",testedPlay.transform.position+Vector3.up*.06f);stage=30;
                }
                else if(stage==30)
                {
                    if(testedPlay.EffectCount>0)return;testedPlay=TriggerPlay(table,1);stage=31;
                }
                else if(stage==31)
                {
                    if(testedPlay.EffectAge<.7f)return;
                    Check(testedPlay.EffectCount==3&&testedPlay.EffectRoot.GetComponentsInChildren<MeshFilter>().Count()==3,"Three modeled candy canes sprout.");SaveDetail("Logs/CandyLand-mint-canes.png",testedPlay.transform.position+Vector3.up*.025f);
                    table.ToggleCharacterVoices();Check(!table.CharacterVoicesEnabled&&!CandyCharacterPlay.AnySpeaking,"Voice control mutes bundled lines.");table.ToggleCharacterVoices();stage=32;
                }
                else if(stage==32)
                {
                    if(testedPlay.EffectCount>0)return;table.ReturnToSetup();Check(table.GetComponent<TableMusic>().Source.clip.name=="candy-garden-waltz","Setup music returns after leaving gameplay.");
                    CheckPartySpacing(table);table.StartGame(42);table.Game.ObserveCard(new CandyCard(CandyColor.Blue));
                    typeof(CandyTable).GetMethod("BeginMove",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(table,null);moveGameStarted=Time.timeAsDouble;stage=33;
                }
                else if(stage==33)
                {
                    CheckNoOverlap(table);if(table.Phase!=CandyPhase.Ready)return;
                    Check(table.LastHopCount==4&&table.Game.Position(0)==3,"Short move hops through four consecutive path squares.");
                    double moveElapsed=Time.timeAsDouble-moveGameStarted;Check(moveElapsed>=CandyTable.MoveSeconds-Time.maximumDeltaTime&&moveElapsed<CandyTable.MoveSeconds+(table.LastHopCount+1)*Time.maximumDeltaTime+.1f,"Short move uses shared game time within per-hop frame quantization. Elapsed="+moveElapsed);
                    table.CelebrateCharacter(0);stage=34;
                }
                else if(stage==34)
                {
                    if(table.Celebration.Age<.7f)return;Check(table.Celebration.LiveCount==74,"Bounded fireworks and twenty themed treats.");SaveView("Logs/CandyLand-special-celebration.png");
                    table.BeginRelocation();pausedMagic=table.Celebration.Age;magicWait=EditorApplication.timeSinceStartup+.25;stage=35;
                }
                else if(stage==35)
                {
                    if(EditorApplication.timeSinceStartup<magicWait)return;Check(table.Celebration.Age==pausedMagic,"Special celebration pauses with board.");table.EndRelocation();stage=36;
                }
                else if(stage==36)
                {
                    if(table.Celebration.LiveCount>0)return;table.CelebrateCharacter(7);Check(table.Celebration.Theme==7,"Licorice wrapped candy theme.");table.ReturnToSetup();Check(table.Celebration.LiveCount==0,"Reset clears celebration.");
                    int before=table.PlayerColor;table.CyclePlayerColor();Check(table.PlayerColor==(before+1)%4,"Setup color cycles.");table.Configure(3,true,false,false);table.StartGame(42);table.CyclePlayerColor();Check(table.PlayerColor==(before+1)%4,"Color cannot change during play.");
                    table.SelectPhysicalCard(new CandyCard(CandyColor.Picture,1,"Plumpy"));table.ConfirmCard();Check(table.TreatBucket.CountFor(0)==1,"First player's special card fills shared bucket.");
                    var owner=table.GetComponentsInChildren<CandyCharacterActor>().First(a=>a.CharacterId==0);Check(Vector3.Distance(table.Celebration.Origin,owner.transform.localPosition)<.001f,"Celebration originates at the character, not pawn.");stage=37;
                }
                else if(stage==37)
                {
                    if(table.Phase!=CandyPhase.AwaitingMove)return;table.ConfirmMove(table.Game.Preview().To);Check(table.Game.CurrentPlayer==1,"Second player turn.");
                    table.SelectPhysicalCard(new CandyCard(CandyColor.Picture,1,"Mr Mint"));table.ConfirmCard();Check(table.TreatBucket.CountFor(1)==1&&table.TreatBucket.Count==2,"Another player's special card also fills the bucket.");stage=38;
                }
                else if(stage==38)
                {
                    if(table.Phase!=CandyPhase.AwaitingMove)return;
                    for(int theme=0;theme<6;theme++){for(int refill=0;refill<10;refill++)table.TreatBucket.Fill(theme);Check(table.TreatBucket.CountFor(theme)==5,"Five reusable treats per character.");}
                    Check(table.TreatBucket.Count==30,"Stock stays bounded across all six card characters.");SaveDetail("Logs/CandyLand-treat-bucket.png",table.TreatBucket.BucketWorldPosition+Vector3.up*.03f);
                    treatPoint=table.TreatBucket.TreatWorldPosition(29);Check(table.TreatBucket.BeginGrab(501,treatPoint),"Treat can be pinched.");Check(!table.TreatBucket.BeginGrab(501,treatPoint),"One treat per hand.");stage=39;
                }
                else if(stage==39)
                {
                    table.TreatBucket.UpdateGrab(501,treatPoint+Vector3.up*.08f,false);stage=40;
                }
                else if(stage==40)
                {
                    table.TreatBucket.UpdateGrab(501,treatPoint+Vector3.up*.10f+table.transform.right*.035f,false);table.TreatBucket.Release(501);Check(!table.TreatBucket.IsHolding(501),"Release throws the toy.");treatReleased=table.TreatBucket.TreatWorldPosition(29);magicWait=EditorApplication.timeSinceStartup+.15;stage=41;
                }
                else if(stage==41)
                {
                    if(EditorApplication.timeSinceStartup<magicWait)return;Check(Vector3.Distance(treatReleased,table.TreatBucket.TreatWorldPosition(29))>.005f,"Released treat continues in flight.");
                    var point=table.TreatBucket.TreatWorldPosition(28);Check(table.TreatBucket.BeginGrab(502,point),"Second treat pickup.");table.ForgetDirectHand(502);Check(!table.TreatBucket.IsHolding(502),"Tracking loss releases safely.");
                    var bowl=table.TreatBucket.BucketWorldPosition;var rim=bowl+table.transform.right*.055f+table.transform.up*.018f;
                    Check(table.TreatBucket.BeginGrab(503,rim)&&table.TreatBucket.BowlHeld,"Bowl rim selects and grabs.");table.TreatBucket.UpdateGrab(503,rim+table.transform.right*.1f+table.transform.up*.1f,false);Check(Vector3.Distance(bowl,table.TreatBucket.BucketWorldPosition)>.1f,"Bowl follows hand.");table.TreatBucket.Release(503);Check(!table.TreatBucket.BowlHeld,"Bowl deselects on release.");
                    var actor=table.GetComponentsInChildren<CandyCharacterActor>().First();actor.Flick(Vector3.right*.4f);float gentle=actor.ReactionIntensity;actor.Flick(Vector3.right*2);Check(actor.ReactionIntensity>gentle,"Fast flick has stronger animation.");
                    table.ReturnToSetup();Check(table.TreatBucket.Count==0&&!table.TreatBucket.gameObject.activeSelf,"New game resets bucket.");table.Configure(1,true,false,false);table.StartGame(42);
                    var positions=(int[])typeof(CandyGame).GetField("positions",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(table.Game);positions[0]=32;
                    table.Game.ObserveCard(new CandyCard(CandyColor.Purple));typeof(CandyTable).GetMethod("BeginMove",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(table,null);
                    Check(table.ExpectedManualSpace==33,"Gumdrop Pass first targets entrance.");Check(!table.ConfirmMove(46),"Cannot skip entrance bloom.");Check(table.ConfirmMove(33)&&table.Phase==CandyPhase.Animating,"Entrance starts bloom.");magicWait=EditorApplication.timeSinceStartup+.6;stage=42;
                }
                else if(stage==42){if(EditorApplication.timeSinceStartup<magicWait)return;Check(table.Game.Position(0)==32&&table.Game.Turn==0,"Bloom never commits move early.");Check(table.GetComponentsInChildren<CandyShortcutBloom>().Length==1,"Bounded bloom effect active.");SaveView("Logs/CandyLand-gumdrop-bloom.png");stage=43;}
                else if(stage==43){if(table.Phase!=CandyPhase.AwaitingMove)return;Check(table.ExpectedManualSpace==46,"Bloom completes before crossing prompt.");Check(table.ConfirmMove(46)&&table.Game.Position(0)==46&&table.Game.Turn==1,"Shortcut commits once at exit.");table.ReturnToSetup();table.Configure(1,false,false,true);table.StartGame(42);var game=table.Game;
                    Check(!table.AcceptCameraCard(new CandyCard(CandyColor.Red),game,1),"Stale camera turn rejected");Check(table.AcceptCameraCard(new CandyCard(CandyColor.Red),game,0),"Camera card starts physical turn without buttons");Check(!table.AcceptCameraCard(new CandyCard(CandyColor.Red),game,0),"Duplicate card cannot start second reveal");stage=44;}
                else if(stage==44){if(table.Phase!=CandyPhase.AwaitingMove)return;Check(!table.AcceptCameraMove(1,table.Game,0),"Camera wrong square cannot commit");Check(!table.AcceptCameraMove(0,table.Game,1),"Stale move cannot commit");table.ObservePhysicalHand(999,table.BoardFrame.TransformPoint(table.SpacePosition(0)+Vector3.up*.03f));Check(!table.AcceptCameraMove(0,table.Game,0),"Hand occlusion blocks move");magicWait=EditorApplication.timeSinceStartup+.8;stage=45;}
                else if(stage==45){if(EditorApplication.timeSinceStartup<magicWait)return;Check(table.AcceptCameraMove(0,table.Game,0)&&table.Game.Turn==1,"Settled camera move advances once without DONE");Check(!table.AcceptCameraMove(0,table.Game,0),"Duplicate move rejected");Check(table.AcceptCameraCard(new CandyCard(CandyColor.Red),table.Game,1)&&table.Phase==CandyPhase.Revealing,"AI also uses the shared physical deck");var old=table.Game;table.ReturnToSetup();table.Configure(1,false,false,true);table.StartGame(42);Check(!table.AcceptCameraCard(new CandyCard(CandyColor.Red),old,0),"Old game frame rejected");table.ToggleVisionFallback();Check(!table.AcceptCameraCard(new CandyCard(CandyColor.Red),table.Game,0),"Fallback suspends automatic commits");table.SelectPhysicalCard(new CandyCard(CandyColor.Blue));Check(table.Phase==CandyPhase.CheckingCard,"Fallback card selection available");table.ConfirmCard();Check(table.Phase==CandyPhase.Revealing,"Fallback still playable");table.ReturnToSetup();Finish(0,"CANDY_SCENE_TESTS_PASSED");}
            }catch(Exception e){Finish(1,e.ToString());}
        }
        static CandyCharacterPlay TriggerPlay(CandyTable table,int character)
        {
            var actor=table.GetComponentsInChildren<CandyCharacterActor>().First(a=>a.CharacterId==character);var point=actor.transform.position+table.transform.up*.04f;
            Check(table.BeginCharacterStretch(101,point),"Character interaction begins "+character);table.UpdateCharacterStretch(101,point,true);return actor.GetComponent<CandyCharacterPlay>();
        }
        static void CheckNoOverlap(CandyTable table)
        {
            var pieces=table.GetComponentsInChildren<CandyPlayerPiece>().Where(p=>p.gameObject.activeSelf).ToArray();
            for(int i=0;i<pieces.Length;i++)for(int j=i+1;j<pieces.Length;j++)
            {var d=pieces[i].transform.localPosition-pieces[j].transform.localPosition;Check(Mathf.Abs(d.y)>=.0579f||Mathf.Abs(d.x)>=.0539f||Mathf.Abs(d.z)>=.0259f,"Player footprints never overlap.");}
        }
        static void CheckPartySpacing(CandyTable table)
        {
            table.Configure(3,false,false,false);CheckNoOverlap(table);
            Check(Mathf.Abs(table.PawnWorldPosition(0).z-table.PawnWorldPosition(2).z)>.02f,"Starting group has two rows.");SaveView("Logs/CandyLand-start-group.png");
            var pieces=table.GetComponentsInChildren<CandyPlayerPiece>().Where(p=>p.gameObject.activeSelf).ToArray();
            for(int i=0;i<4;i++)pieces[i].transform.localPosition=table.SpacePosition(20,i);CheckNoOverlap(table);
            for(int i=0;i<4;i++)pieces[i].transform.localPosition=table.SpacePosition(20+i,i);CheckNoOverlap(table);
            for(int i=0;i<6;i++)Check(Resources.Load<Texture2D>("Celebrations/"+CandyCharacterModel.Ids[i])!=null,"Excited illustration exists "+i);
        }
        static void CheckPiecePlay(CandyTable table)
        {
            table.ReturnToSetup();table.Configure(1,false,false,false);var start=table.PawnWorldPosition(0);var hand=start+table.transform.up*.025f;
            Check(table.BeginPieceGrab(102,hand),"Setup piece can be picked up.");table.UpdatePieceGrab(102,hand+table.transform.right*.08f+table.transform.up*.05f,false);table.UpdatePieceGrab(102,hand,true);Check(Vector3.Distance(table.PawnWorldPosition(0),start)>.04f,"Setup piece keeps playful placement.");
            table.StartGame(42);start=table.PawnWorldPosition(0);hand=start+table.transform.up*.025f;Check(table.BeginPieceGrab(102,hand),"In-game piece can be lifted.");table.UpdatePieceGrab(102,hand+table.transform.right*.12f+table.transform.up*.08f,false);Check(Vector3.Distance(table.PawnWorldPosition(0),start)>.08f,"Held piece follows hand.");table.UpdatePieceGrab(102,hand,true);
            Check(Vector3.Distance(table.PawnWorldPosition(0),start)<.001f&&table.Game.Position(0)==-1&&table.Game.Turn==0,"Release restores legal position without changing rules.");
            Check(table.BeginPieceGrab(102,hand),"Piece can be grabbed again.");table.UpdatePieceGrab(102,hand+table.transform.right*.1f,false);table.ForgetDirectHand(102);Check(Vector3.Distance(table.PawnWorldPosition(0),start)<.001f,"Tracking loss restores piece.");
            Check(table.BeginPieceGrab(102,hand),"Grab before draw.");table.UpdatePieceGrab(102,hand+table.transform.right*.1f,false);table.DrawVirtual();Check(table.GrabbedPieceCount==0&&Vector3.Distance(table.PawnWorldPosition(0),start)<.001f,"Drawing resets decorative grabs before animation.");
        }
        static void CheckSludge(CandyTable table)
        {
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var positions=(int[])typeof(CandyGame).GetField("positions",flags).GetValue(table.Game);positions[0]=119;
            void Commit(CandyColor color){table.Game.ObserveCard(new CandyCard(color));typeof(CandyTable).GetField("lastPlayer",flags).SetValue(table,table.Game.CurrentPlayer);typeof(CandyTable).GetMethod("CompleteMove",flags).Invoke(table,null);}
            var pieces=table.GetComponentsInChildren<CandyPlayerPiece>(true).Where(p=>p.gameObject.activeSelf).ToArray();var player=pieces.First(p=>p.gameObject.name=="Your avatar");var opponent=pieces.First(p=>p.gameObject.name=="Opponent 1");
            Commit(CandyColor.Red);Check(table.Game.Position(0)==120&&player.Sludged&&!opponent.Sludged,"Landing in Molasses Swamp coats only that player's piece.");SaveDetail("Logs/CandyLand-sludge.png",player.transform.position+Vector3.up*.025f);
            Commit(CandyColor.Red);Commit(CandyColor.Blue);Check(player.Sludged&&table.Game.Position(0)==120,"Sludge persists while trapped.");
            Commit(CandyColor.Red);Commit(CandyColor.Red);Check(!player.Sludged&&table.Game.Position(0)==126,"Legal red release clears sludge.");
            table.ReturnToSetup();table.Configure(1,false,false,false);table.StartGame(42);
        }
        static void CheckCharacterInteractions(CandyTable table)
        {
            foreach(int id in new[]{1,7})
            {
                var prefab=Resources.Load<GameObject>("Characters/"+CandyCharacterModel.Ids[id]);
                Check(prefab.transform.Find("Sculpted "+CandyCharacterModel.Names[id])!=null,"Detailed replacement model "+id);
                Check(prefab.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.vertexCount)>10000,"Replacement has complete modeled geometry "+id);
            }
            var actors=table.GetComponentsInChildren<CandyCharacterActor>();Check(actors.Select(a=>a.CharacterId).Distinct().Count()==9,"Nine distinct reaction styles.");
            testedActor=actors.First(a=>a.CharacterId==7);var point=testedActor.transform.position+table.transform.up*.035f;int count=testedActor.ReactionCount,turn=table.Game.Turn;
            table.DirectPressWorld(91,point);table.DirectPressWorld(91,point);
            Check(testedActor.ReactionCount==count+1&&table.Game.Turn==turn,"Nudge reacts once for a held hand without advancing gameplay.");
            table.BeginRelocation();Check(!testedActor.Nudge(point),"Paused character ignores nudges.");table.EndRelocation();table.ForgetDirectHand(91);
            string[] pictures={"Plumpy","Mr Mint","Jolly","Gramma Nutt","Princess Lolly","Queen Frostine"};
            for(int i=0;i<6;i++)
            {
                table.ReturnToSetup();table.Configure(1,true,false,false);table.StartGame(42);
                var from=table.PawnWorldPosition(0);table.SelectPhysicalCard(new CandyCard(CandyColor.Picture,1,pictures[i]));table.ConfirmCard();
                Check(table.MoveMagic.GetComponentsInChildren<Renderer>().All(r=>r.bounds.size.sqrMagnitude<.000001f),"New effects start invisible before their first animation frame.");
                Check(table.MoveMagic.SquareGlow!=null&&table.MoveMagic.SquareGlow.GetComponent<Renderer>().sharedMaterial.shader.name=="LivingTable/CandyMagicGlow","Procedural neon interior shader loaded.");
                Check(table.MoveMagic.LandingColor==new Color(1,.54f,.76f),"Picture square uses destination pink, independently of trail theme.");
                Check(table.MoveMagic.Theme==i&&table.MoveMagic.LiveCount==40,"Special card creates correct bounded trail theme "+i);
                Check(Vector3.Distance(table.MoveMagic.transform.TransformPoint(table.MoveMagic.From),from)<.001f,"Trail begins at drawing player's actual pawn.");
                var space=table.Data.spaces[table.Game.Preview().To];Check(Mathf.Abs(table.MoveMagic.To.x-(space.x-.5f)*table.Width)<.001f,"Trail ends at legal picture square.");
            }
            // Exercise an actual backward card from a later space, then a trapped picture card.
            table.ReturnToSetup();table.Configure(1,true,false,false);table.StartGame(42);
            var field=typeof(CandyGame).GetField("positions",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            ((int[])field.GetValue(table.Game))[0]=80;
            table.SelectPhysicalCard(new CandyCard(CandyColor.Picture,1,"Plumpy"));table.ConfirmCard();Check(table.Game.Preview().To==8&&table.MoveMagic.Theme==0,"Backward picture trail respects original rules.");
            table.ReturnToSetup();table.Configure(1,true,false,false);table.StartGame(42);((int[])field.GetValue(table.Game))[0]=120;
            table.SelectPhysicalCard(new CandyCard(CandyColor.Picture,1,"Plumpy"));table.ConfirmCard();Check(table.Game.Preview().Stuck&&table.MoveMagic.LiveCount==0,"Trapped picture card never suggests an illegal destination.");
            table.ReturnToSetup();table.Configure(1,true,false,false);table.StartGame(42);table.SelectPhysicalCard(new CandyCard(CandyColor.Picture,1,"Mr Mint"));table.ConfirmCard();
        }
        static void SaveDetail(string path,Vector3 target)
        {
            var camera=Camera.main;var position=camera.transform.position;var rotation=camera.transform.rotation;float near=camera.nearClipPlane;
            camera.nearClipPlane=.005f;camera.transform.position=target+new Vector3(0,.045f,-.12f);camera.transform.LookAt(target);SaveView(path);camera.transform.SetPositionAndRotation(position,rotation);camera.nearClipPlane=near;
        }
        static void SaveView(string path)
        {
            var camera=Camera.main;var oldTarget=camera.targetTexture;var previous=RenderTexture.active;
            var rt=new RenderTexture(1600,1100,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1600,1100,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1100),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            camera.targetTexture=oldTarget;RenderTexture.active=previous;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);
        }
        static void Finish(int code,string message){SessionState.SetBool("CandySmoke",false);EditorApplication.update-=Tick;Debug.Log(message);EditorApplication.Exit(code);}
        static void RenderPiecePreview(CandyTable table)
        {
            var root=new GameObject("Piece review");root.transform.position=new Vector3(100,0,0);
            var palette=new[]{Color.red,new Color(.05f,.35f,1),new Color(.03f,.55f,.15f),new Color(1,.8f,.02f)};
            var materials=new System.Collections.Generic.List<Material>();
            var white=new Material(Shader.Find("Universal Render Pipeline/Unlit"));white.color=Color.white;materials.Add(white);
            for(int i=0;i<4;i++)
            {
                var mat=new Material(Resources.Load<Shader>("CandyPiecePlastic"));mat.color=palette[i];materials.Add(mat);
                var piece=new GameObject("Color "+i);piece.transform.SetParent(root.transform,false);piece.transform.localPosition=new Vector3(i*.065f,0,0);
                piece.AddComponent<CandyPlayerPiece>().Build(mat,white);
            }
            var camObject=new GameObject("Piece review camera");var cam=camObject.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=.062f;cam.nearClipPlane=.01f;cam.farClipPlane=5;
            cam.transform.position=new Vector3(100.0975f,.075f,-.26f);cam.transform.LookAt(new Vector3(100.0975f,.026f,0));cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.14f,.17f,.2f);
            var target=new RenderTexture(1600,700,24);cam.targetTexture=target;cam.Render();var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(1600,700,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,700),0,0);image.Apply();File.WriteAllBytes("Logs/CandyLand-player-pieces.png",image.EncodeToPNG());
            Check(image.GetPixels32().Count(p=>p.r>100||p.g>100||p.b>100)>5000,"Close-up must contain visible rendered pieces, not an empty background.");
            RenderTexture.active=previous;cam.targetTexture=null;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(camObject);
            foreach(var mat in materials)UnityEngine.Object.Destroy(mat);
        }
        static void CheckDiscardOcclusion(CandyTable table)
        {
            var create=typeof(CandyTable).GetMethod("CreateCard",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            var lower=(Transform)create.Invoke(table,new object[]{new CandyCard(CandyColor.Red)});
            var upper=(Transform)create.Invoke(table,new object[]{new CandyCard(CandyColor.Blue)});
            lower.position=new Vector3(300,0,0);lower.rotation=Quaternion.identity;upper.position=new Vector3(300,.003f,0);upper.rotation=Quaternion.identity;
            var label=lower.GetComponentInChildren<TextMesh>();label.text="HIDDEN";label.characterSize=.002f;label.color=Color.magenta;label.transform.localPosition=Vector3.up*.001f;
            var go=new GameObject("Occlusion camera");var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=.08f;camera.nearClipPlane=.01f;camera.farClipPlane=2;camera.transform.position=new Vector3(300,.5f,0);camera.transform.rotation=Quaternion.LookRotation(Vector3.down,Vector3.forward);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            var rt=new RenderTexture(256,256,24);camera.targetTexture=rt;var previous=RenderTexture.active;
            Color32[] Capture(){camera.Render();RenderTexture.active=rt;var im=new Texture2D(256,256,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,256,256),0,0);im.Apply();var pixels=im.GetPixels32();UnityEngine.Object.Destroy(im);return pixels;}
            var withHidden=Capture();label.gameObject.SetActive(false);var withoutHidden=Capture();Check(withHidden.SequenceEqual(withoutHidden),"Covered lower-card text cannot affect rendered top card pixels.");
            RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(go);UnityEngine.Object.Destroy(lower.gameObject);UnityEngine.Object.Destroy(upper.gameObject);
        }
        static void RenderCardPreview(CandyTable table)
        {
            var root=new GameObject("Card review");root.transform.position=new Vector3(200,0,0);
            var create=typeof(CandyTable).GetMethod("CreateCard",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            string[] names={"Plumpy","Mr Mint","Jolly","Gramma Nutt","Princess Lolly","Queen Frostine"};
            for(int i=0;i<6;i++){var card=(Transform)create.Invoke(table,new object[]{new CandyCard(CandyColor.Picture,1,names[i])});Check(card.Find("Character illustration")!=null,"Picture card has matching rendered art: "+names[i]);card.SetParent(root.transform,false);card.localPosition=new Vector3((i%3)*.12f,(1-i/3)*.17f,0);card.localRotation=Quaternion.Euler(-90,0,0);}
            var go=new GameObject("Card review camera");var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=.19f;camera.nearClipPlane=.01f;camera.farClipPlane=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.15f,.19f);camera.transform.position=new Vector3(200.12f,.085f,-1);camera.transform.LookAt(new Vector3(200.12f,.085f,0));
            var rt=new RenderTexture(1400,1400,24);camera.targetTexture=rt;camera.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1400,1400,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1400),0,0);image.Apply();File.WriteAllBytes("Logs/CandyLand-character-cards.png",image.EncodeToPNG());RenderTexture.active=prev;camera.targetTexture=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(go);
        }
        static void RenderCharacterPreview()
        {
            var root=new GameObject("Character review");root.transform.position=new Vector3(100,0,0);
            for(int i=0;i<9;i++){var obj=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Characters/"+CandyCharacterModel.Ids[i]),root.transform);UnityEngine.Object.DestroyImmediate(obj.GetComponent<CandyCharacterActor>());obj.transform.localPosition=new Vector3((i%3)*1.1f,(2-i/3)*1.3f,0);}
            var go=new GameObject("Character camera");var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=2.1f;camera.nearClipPlane=.01f;camera.farClipPlane=20;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.15f,.19f);camera.transform.position=new Vector3(101.1f,1.9f,-7);camera.transform.LookAt(new Vector3(101.1f,1.9f,0));
            var rt=new RenderTexture(1400,1400,24);camera.targetTexture=rt;camera.Render();var prev=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1400,1400,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1400,1400),0,0);image.Apply();File.WriteAllBytes("Logs/CandyLand-character-models.png",image.EncodeToPNG());RenderTexture.active=prev;camera.targetTexture=null;UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(go);
        }
        public static void BuildQuest()
        {
            Directory.CreateDirectory("Builds");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Folder+"/CandyLandQuest.unity"},locationPathName="Builds/LivingTable-CandyLand-Quest.apk",target=BuildTarget.Android,options=BuildOptions.None});
            var s=report.summary;File.WriteAllText("Builds/CandyLand-build-report.txt",$"Result: {s.result}\nErrors: {s.totalErrors}\nWarnings: {s.totalWarnings}\nDuration: {s.totalTime}\n");
            if(s.result!=BuildResult.Succeeded)throw new Exception("Candy Land Quest build failed.");
            Debug.Log("CANDY_QUEST_BUILD_SUCCEEDED");
        }
    }
}
#endif
