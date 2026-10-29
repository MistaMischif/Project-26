using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LivingTable.CandyLand
{
    public enum CandyPhase { Setup, Ready, CheckingCard, AwaitingMove, Animating, Finished, Revealing }
    public sealed partial class CandyTable : MonoBehaviour
    {
        public CandyGame Game { get; private set; }
        public CandyPhase Phase { get; private set; } = CandyPhase.Setup;
        public string Message { get; private set; } = "Bring your bored games to life.";
        public string HostLine { get; private set; } = "Welcome to Candy Land!";
        public bool Manual { get; private set; }
        public bool PhysicalOverlay { get; private set; }
        public int Opponents { get; private set; } = 1;
        public bool TwoHumans { get; private set; }
        public CandyPhysicalVision Vision {get;private set;}
        public bool CameraEnabled {get;private set;}=true;
        public bool VisionFallback {get;private set;}
        public bool AutomaticPhysical=>PhysicalOverlay&&CameraEnabled;
        public bool NeedsPhysicalCard=>Game!=null&&(Manual||AutomaticPhysical||Game.CurrentPlayer==0);
        public bool RecognitionAllowed=>calibrationStage==0&&!relocating&&!placingPanel;
        public Transform BoardFrame=>content;
        public double LastBoardHandSeen {get;private set;}
        readonly Dictionary<int,Vector3> observedHands=new Dictionary<int,Vector3>();
        readonly Dictionary<int,float> observedHandTimes=new Dictionary<int,float>();
        public bool HandNearBoard=>observedHands.Any(h=>{if(content==null||!observedHandTimes.TryGetValue(h.Key,out var t)||Time.unscaledTime-t>=.6f)return false;var p=content.InverseTransformPoint(h.Value);return Mathf.Abs(p.x)<Width*.6f&&Mathf.Abs(p.z)<Width*Aspect*.6f&&p.y>-.03f&&p.y<.22f;});
        public void ObservePhysicalHand(int id,Vector3 world){if(content==null)return;observedHands[id]=world;observedHandTimes[id]=Time.unscaledTime;var p=content.InverseTransformPoint(world);if(Mathf.Abs(p.x)<Width*.6f&&Mathf.Abs(p.z)<Width*Aspect*.6f&&p.y>-.03f&&p.y<.22f)LastBoardHandSeen=Time.realtimeSinceStartupAsDouble;}
        public bool AcceptCameraCard(CandyCard card,CandyGame observedGame,int turn)
        {
            if(StorageBlocked||SessionRecovering||!AutomaticPhysical||VisionFallback||Paused||!RecognitionAllowed||Game==null||Game!=observedGame||Game.Turn!=turn||Phase!=CandyPhase.Ready||!NeedsPhysicalCard)return false;
            try{Game.Resolve(-1,card);}catch{return false;}
            CardRevision++;candidate=card;Phase=CandyPhase.CheckingCard;ConfirmCard();return true;
        }
        public bool AcceptCameraMove(int space,CandyGame observedGame,int turn)
        {
            if(StorageBlocked||SessionRecovering||!AutomaticPhysical||VisionFallback||Paused||!RecognitionAllowed||Game==null||Game!=observedGame||Game.Turn!=turn||HandNearBoard)return false;
            return ConfirmMove(space);
        }
        public bool AlignRecognizedBoard(Vector3[] corners)
        {
            if((Phase!=CandyPhase.Setup&&!SessionRecovering)||corners==null||corners.Length!=4)return false;
            var edge=corners[1]-corners[0];float width=edge.magnitude,height=Vector3.Distance(corners[0],corners[3]);
            if(width<.2f||width>1.5f||Mathf.Abs(height/width-Aspect)>.12f)return false;
            var up=content.up;var forward=Vector3.Cross(edge.normalized,up);if(Vector3.Dot(forward,corners[3]-corners[0])<=0)return false;
            Width=width;content.SetPositionAndRotation((corners[0]+corners[1]+corners[2]+corners[3])*.25f,Quaternion.LookRotation(forward,up));boardFace.transform.localScale=Vector3.one*Width;boardBody.transform.localScale=new Vector3(Width,.012f,Width*Aspect);BuildHosts();BuildPawns(TwoHumans?2:Opponents+1);if(SessionRecovering)RefreshSessionView();BuildUI();return true;
        }
        public void ToggleVisionFallback(){if(StorageBlocked||SessionRecovering)return;SessionUnverified=true;Vision?.Invalidate(false);VisionFallback=!VisionFallback;CardRevision++;candidate=null;cardPicker=false;if(Phase==CandyPhase.CheckingCard)Phase=CandyPhase.Ready;SaveSession("manual fallback changed");BuildUI();}
        public void ToggleCamera(){CameraEnabled=!CameraEnabled;VisionFallback=false;Vision?.RetryPermission();BuildUI();}
        public float ConfirmationSeconds { get; private set; } = 8;
        public event Action ScanRequested;
        public event Action<string> HostSpoke;
        public event Action<string> CharacterChanged;
        public string NearbyCharacter { get; private set; } = "Plumpy";
        public int ScanRequests { get; private set; }
        public int CardRevision { get; private set; }
        public bool Desktop { get; private set; }
        public bool Paused { get; private set; }
        public CandyBoardData Data { get; private set; }
        public float Width { get; private set; } = .64f;
        public int VisiblePawnCount => pawns.FindAll(p=>p!=null&&p.gameObject.activeInHierarchy&&p.GetComponentsInChildren<Renderer>().Length>0).Count;
        public Vector3 PawnWorldPosition(int player)=>pawns[player].position;
        const float Aspect = 913f / 1280f;
        readonly List<Material> materials = new List<Material>();
        readonly Dictionary<Color,Material> palette = new Dictionary<Color,Material>();
        readonly Dictionary<int,Material> characterCards=new Dictionary<int,Material>();
        readonly Dictionary<Color,Material> plastics = new Dictionary<Color,Material>();
        Vector2 desktopScroll;
        readonly List<Button> buttons = new List<Button>();
        readonly List<Transform> pawns = new List<Transform>();
        readonly List<Transform> hosts = new List<Transform>();
        readonly List<TextMesh> scores = new List<TextMesh>();
        public bool EffectsTestMode {get;private set;}
        int testCharacter;
        readonly string[] pictureNames = { "Plumpy", "Mr Mint", "Jolly", "Gramma Nutt", "Princess Lolly", "Queen Frostine" };
        readonly Color[] colors = { new Color(.94f,.16f,.25f), new Color(.63f,.27f,.83f), new Color(1,.8f,.12f), new Color(.08f,.65f,.95f), new Color(1,.46f,.16f), new Color(.12f,.76f,.48f), new Color(1,.54f,.76f) };
        public TableCardPresentation Cards { get; private set; }
        public CandyMoveMagic MoveMagic {get;private set;}
        public CandyCardCelebration Celebration {get;private set;}
        public int LastHopCount {get;private set;}
        public const float MoveSeconds=1.2f;
        public CandyTreatBucket TreatBucket {get;private set;}
        public int PlayerColor {get;private set;}
        public void CyclePlayerColor(){if(Phase!=CandyPhase.Setup)return;PlayerColor=(PlayerColor+1)%4;BuildPawns(TwoHumans?2:Opponents+1);BuildUI();}
        bool buttonBusy,placingPanel,cardPicker;int pickerPage;
        bool relocating,pausedBeforeRelocation;readonly List<Renderer> relocationRenderers=new List<Renderer>();
        Material depthText;GameObject boardBody;
        TableMusic music;
        public bool CharacterVoicesEnabled {get;private set;}=true;
        string localCharacterLine="";float localLineUntil;
        public void ShowCharacterLine(string line){localCharacterLine=line;localLineUntil=Time.unscaledTime+7;}
        public void ToggleCharacterVoices()
        {
            CharacterVoicesEnabled=!CharacterVoicesEnabled;
            if(!CharacterVoicesEnabled){foreach(var play in GetComponentsInChildren<CandyCharacterPlay>())play.MuteVoice();if(meta!=null&&meta.Listening)meta.StopVoice();}
            else if(meta!=null&&meta.Configured&&!meta.Listening)meta.ToggleVoice();BuildUI();
        }
        public TableFacingPanel StatusPanel {get;private set;}
        Transform panelText,turnRoot;
        TextMesh turnText,scoreText;
        float turnRemaining;
        public string TurnAnnouncement=>turnText!=null&&turnRoot.gameObject.activeSelf?turnText.text:"";
        public bool MusicEnabled=>music!=null&&music.Enabled;
        Material template;
        CandyMetaConnection meta;
        Transform content, ui, pawnRoot, scenery;
        GameObject boardFace;
        LineRenderer destinationRing;
        TextMesh worldStatus, hostCaption, cardCaption, metaCaption;
        float countdown, aiAt, lastTouch = -1;
        int multiplier = 1, lastPlayer;
        bool scannedAgain;
        Vector3 calibrationStart;
        int calibrationStage;
        CandyCard? candidate, lastDrawn;
        Rect desktopPanel = new Rect(12, 12, 300, 690);
        public float Countdown => Mathf.Max(0, countdown);
        public string CardText => candidate.HasValue ? candidate.Value.Label : Game != null && Game.PendingCard.HasValue ? Game.PendingCard.Value.Label : lastDrawn.HasValue ? lastDrawn.Value.Label : "Draw a card";
        sealed class Button { public Rect rect; public Action action; public string text; public TableButtonVisual visual; public bool circular; public bool Contains(Vector2 p)=>circular?Vector2.Distance(p,rect.center)<=rect.width*.5f:rect.Contains(p); }

        public void SetGameVisible(bool visible)
        {
            if(visible)FindFirstObjectByType<LivingTableExperience>()?.SetVirtualRoom(VirtualRoom&&!PhysicalOverlay);
            if(!visible){SaveSession("hidden");ReleaseCharacterStretches();ReleasePieceGrabs(true);meta?.StopVoice();}
            if(content!=null)content.gameObject.SetActive(visible);if(Vision!=null)Vision.enabled=visible;if(meta!=null)meta.enabled=visible;if(music!=null)music.enabled=visible;enabled=visible;
        }
        public void Initialize(bool desktop, Material material)
        {
            Desktop = desktop; template = material;
            Data = JsonUtility.FromJson<CandyBoardData>(Resources.Load<TextAsset>("candy-board-1984").text); Data.Validate();
            content = new GameObject("Candy Land experience").transform; content.SetParent(transform, false); content.localPosition=Vector3.up*.012f;
            Cards=new GameObject("Shared card presentation").AddComponent<TableCardPresentation>(); Cards.transform.SetParent(content,false);
            StatusPanel=new GameObject("Viewer facing status panel").AddComponent<TableFacingPanel>();StatusPanel.transform.SetParent(content,false);StatusPanel.transform.localPosition=new Vector3(0,.20f,Width*Aspect*.5f+.01f);
            turnRoot=new GameObject("New turn announcement").transform;turnRoot.SetParent(content,false);turnRoot.localPosition=new Vector3(0,Width*.25f,0);turnRoot.gameObject.AddComponent<TableFacingPanel>();
            turnRoot.gameObject.AddComponent<TableCabinetVisual>().Panel(.39f,.105f,true);
            turnText=Label(turnRoot,"Turn name","",Vector3.zero,.005f,new Color(1,.9f,.4f));turnText.color=new Color(.22f,.045f,.33f);turnText.transform.localRotation=Quaternion.identity;turnRoot.gameObject.SetActive(false);
            music=gameObject.AddComponent<TableMusic>();music.Initialize();
            RefreshSaved();BuildBoard(); BuildHosts(); BuildPawns(2); BuildUI();
            meta=gameObject.AddComponent<CandyMetaConnection>();
            Vision=gameObject.AddComponent<CandyPhysicalVision>();Vision.Initialize(this);
        }
        Material Mat(Color c)
        {
            if(palette.TryGetValue(c,out var existing)) return existing;
            var m = template != null ? new Material(template) : new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            m.color = c; materials.Add(m); palette[c]=m; return m;
        }
        GameObject Shape(Transform parent, string name, PrimitiveType type, Vector3 p, Vector3 scale, Color c)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = p; go.transform.localScale = scale; Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = Mat(c); return go;
        }
        TextMesh Label(Transform parent, string name, string text, Vector3 p, float size, Color color)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = p;
            go.transform.localRotation = Quaternion.Euler(90,0,0);
            var t = go.AddComponent<TextMesh>(); t.text = text; t.fontSize = 60; t.characterSize = size; t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.color = color;
            if(depthText==null){depthText=new Material(t.GetComponent<Renderer>().sharedMaterial);depthText.shader=Resources.Load<Shader>("DepthText");materials.Add(depthText);}
            t.GetComponent<Renderer>().sharedMaterial=depthText;return t;
        }
        void BuildBoard()
        {
            boardFace = new GameObject("Supplied board artwork - virtual mode"); boardFace.transform.SetParent(content, false);
            var mesh = new Mesh(); mesh.name = "Candy board aspect-preserving surface";
            float h = Aspect * .5f;
            mesh.vertices = new[] { new Vector3(-.5f,0,-h),new Vector3(-.5f,0,h),new Vector3(.5f,0,h),new Vector3(.5f,0,-h) };
            mesh.uv = new[] { new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0) };
            mesh.triangles = new[] {0,1,2,0,2,3}; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            boardFace.AddComponent<MeshFilter>().sharedMesh = mesh;
            var m = new Material(Mat(Color.white)); materials.Add(m); m.mainTexture = Resources.Load<Texture2D>("candy-board-reference"); boardFace.AddComponent<MeshRenderer>().sharedMaterial = m;
            boardFace.transform.localScale = Vector3.one * Width;
            boardBody=Shape(content,"Solid board edge",PrimitiveType.Cube,new Vector3(0,-.006f,0),new Vector3(Width,.012f,Width*Aspect),new Color(.24f,.075f,.14f));boardBody.AddComponent<TableBoardCabinet>();
            pawnRoot = new GameObject("Playing pieces").transform; pawnRoot.SetParent(content,false);
            var ring = new GameObject("Legal destination outline"); ring.transform.SetParent(content,false); destinationRing=ring.AddComponent<LineRenderer>();
            destinationRing.useWorldSpace=false; destinationRing.loop=true; destinationRing.positionCount=32; destinationRing.widthMultiplier=.0015f; destinationRing.sharedMaterial=Mat(new Color(1,.88f,.25f));
            ring.SetActive(false);
        }
        public Vector3 SpacePosition(int index, int player = -1)
        {
            Vector3 p;
            if (index < 0) return new Vector3(-Width*.27f+(Mathf.Max(0,player)%2-.5f)*.057f,.004f,-Width*Aspect*.39f+(Mathf.Max(0,player)/2-.5f)*.032f);
            else { var s=Data.spaces[index]; p=new Vector3((s.x-.5f)*Width,.008f,(.5f-s.y)*Width*Aspect); }
            if(player<0||Game==null)return p;
            int count=1,slot=0;for(int i=0;i<Game.Players;i++)if(i!=player&&Game.Position(i)==index){count++;if(i<player)slot++;}
            if(count>1)p+=Vector3.forward*((slot-(count-1)*.5f)*.026f);
            return p;
        }
        void ArrangePawnsOnSpaces()
        {
            if(Game==null)return;
            for(int i=0;i<pawns.Count;i++)
            {
                int index=Game.Position(i);
                pawns[i].localScale=Vector3.one;
                if(!pieceGrabs.Values.Any(g=>g.index==i))pawns[i].localPosition=SpacePosition(index,i);
                pawns[i].GetComponent<CandyPlayerPiece>().SetSludged(index>=0&&Data.spaces[index].releaseColor==(int)CandyColor.Red);
            }
        }
        public Vector3 SeparatePawn(Vector3 desired,int player)
        {
            // Actual footprints include arms and sludge. Test adjacent cells too.
            for(int ring=0;ring<=12;ring++)for(int slot=0;slot<(ring==0?1:32);slot++)
            {
                float angle=slot*Mathf.PI/16;
                var p=desired+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(ring*.008f);
                bool clear=true;
                for(int i=0;i<pawns.Count;i++)
                {
                    if(i==player||!pawns[i].gameObject.activeSelf)continue;
                    var d=p-pawns[i].localPosition;
                    if(Mathf.Abs(d.y)<.058f&&Mathf.Abs(d.x)<.054f&&Mathf.Abs(d.z)<.026f){clear=false;break;}
                }
                if(clear)return p;
            }
            return desired+Vector3.up*.07f;
        }
        // Special characters now appear only as card illustrations.
        public void CelebrateCharacter(int index,bool animate=true) { }
        void BuildHosts()
        {
            ReleaseCharacterStretches();
            if(scenery!=null){scenery.gameObject.SetActive(false);Destroy(scenery.gameObject);}
            hosts.Clear();
        }
        public void Configure(int aiCount, bool manual, bool twoHumans, bool physical)
        {
            if(Phase!=CandyPhase.Setup) return;
            Vision?.Invalidate();VisionFallback=false;CameraEnabled=!manual&&!twoHumans;
            Opponents=Mathf.Clamp(aiCount,1,3); TwoHumans=twoHumans; Manual=manual||twoHumans; PhysicalOverlay=physical||Manual;
            boardFace.SetActive(!PhysicalOverlay);boardBody.SetActive(!PhysicalOverlay); BuildPawns(TwoHumans?2:Opponents+1); BuildUI();
        }
        public void StartVirtualGame(int seed){if(Phase!=CandyPhase.Setup)return;Configure(Opponents,false,false,false);StartGame(seed);}
        public void SetTimer(float seconds) { ConfirmationSeconds=Mathf.Clamp(seconds,3,30); if(Phase==CandyPhase.Setup) BuildUI(); }
        public void StartGame(int seed)
        {
            if(Phase!=CandyPhase.Setup) return;
            if(StorageBlocked)return;
            if(AutomaticPhysical&&!Desktop&&(Vision==null||!Vision.Ready)){Message="Look at the full physical board first, or turn CAMERA OFF.";BuildUI();return;}
            Cards.Clear(); if(MoveMagic!=null)MoveMagic.Clear(); Game=new CandyGame(Data,TwoHumans?2:Opponents+1,seed);SessionId=GameSessionStore.NewId();SessionRecovering=SessionUnverified=StorageBlocked=pendingShortcutPassed=false;Paused=false;if(!SaveSession("started"))return; candidate=null;lastDrawn=null; ScanRequests=0; scannedAgain=false;
            BuildPawns(Game.Players);music.SetGameplay(true);
            Debug.Log($"[Candy] Started manual={Manual} physical={PhysicalOverlay} players={Game.Players} visiblePieces={VisiblePawnCount}");
            Phase=CandyPhase.Ready; ShowTurn(); Say("Your turn! Let's find the Candy Castle."); Message=AutomaticPhysical?"Show one physical card beside the board.":Manual?"Draw one physical card, then show or select it.":"Draw your first card."; BuildUI();
        }
        void BuildPawns(int count)
        {
            ReleasePieceGrabs(true);
            foreach(Transform p in pawns){p.GetComponent<CandyPlayerPiece>()?.SetSludged(false);p.gameObject.SetActive(false);Destroy(p.gameObject);}pawns.Clear();
            for(int i=0;i<count;i++)
            {
                var pawn=new GameObject(i==0?"Your avatar":"Opponent "+i); pawn.transform.SetParent(pawnRoot,false); pawn.transform.localPosition=SpacePosition(-1,i); pawns.Add(pawn.transform);
                Color color=new[]{new Color(.90f,.04f,.08f),new Color(.04f,.32f,.90f),new Color(.03f,.55f,.15f),new Color(1,.8f,.02f)}[(i+PlayerColor)%4];
                if(!plastics.TryGetValue(color,out var plastic)){plastic=new Material(Resources.Load<Shader>("CandyPiecePlastic"));plastic.color=color;plastics[color]=plastic;materials.Add(plastic);}
                pawn.AddComponent<CandyPlayerPiece>().Build(plastic,Mat(new Color(1,.97f,.88f)));
                pawn.SetActive(!Manual && (!PhysicalOverlay || i>0));
            }
        }
        public void ReturnToSetup()
        {
            if(!SaveSession("saved to menu"))return;SessionId=null;EffectsTestMode=false;SessionRecovering=false;sessionMenu=false;RefreshSaved();
            Celebration?.Clear();Vision?.Invalidate();VisionFallback=false;
            if(TreatBucket!=null){TreatBucket.ResetTreats();TreatBucket.gameObject.SetActive(false);}
            ReleaseCharacterStretches();ReleasePieceGrabs(true);foreach(var play in GetComponentsInChildren<CandyCharacterPlay>())play.StopInteraction();music.SetGameplay(false);StopAllCoroutines();foreach(var bloom in GetComponentsInChildren<CandyShortcutBloom>())Destroy(bloom); turnRemaining=0;turnRoot.gameObject.SetActive(false);buttonBusy=false; Cards.Clear(); if(MoveMagic!=null)MoveMagic.Clear(); Game=null; candidate=null;lastDrawn=null; Phase=CandyPhase.Setup; Paused=false; calibrationStage=0;
            BuildPawns(TwoHumans?2:Opponents+1); destinationRing.gameObject.SetActive(false); HostLine="Choose how you want to play."; Message="A new game starts when you press PLAY."; BuildUI();
        }
        public void DrawVirtual()
        {
            if(StorageBlocked||SessionRecovering||Paused||Phase!=CandyPhase.Ready||Manual||AutomaticPhysical) return;
            ReleasePieceGrabs(true);var card=Game.Draw();pendingShortcutPassed=false;if(!SaveSession("card drawn; pending move"))return; AnnounceCard(card); StartCoroutine(PresentAndMove(card));
            Debug.Log($"[Candy] Draw player={lastPlayer} card={card.Label} phase={Phase} visiblePieces={VisiblePawnCount}");
        }
        public void BeginScan()
        {
            if(StorageBlocked||SessionRecovering||Paused||(!Manual&&!(PhysicalOverlay&&VisionFallback))||(Phase!=CandyPhase.Ready&&Phase!=CandyPhase.CheckingCard)) return;
            Phase=CandyPhase.CheckingCard; candidate=null; countdown=ConfirmationSeconds; scannedAgain=false; RequestScan(); BuildUI();
        }
        void RequestScan()
        {
            ScanRequests++;CardRevision++;
            Message=ScanRequested==null?"Camera reader not connected. Select the card below.":"Show the card. Let's take a look!";
            ScanRequested?.Invoke();
        }
        public void OfferScan(CandyCard card, float confidence)
        {
            if(Phase!=CandyPhase.CheckingCard||Paused) return;
            if(float.IsNaN(confidence)||confidence<.9f) { Message="A little closer? Choose the card or let me look again."; return; }
            // A reader proposes; only explicit player confirmation commits a recognized card.
            candidate=card; countdown=ConfirmationSeconds; Message="Is this your card?"; BuildUI();
        }
        public void SelectPhysicalCard(CandyCard card)
        {
            if(StorageBlocked||SessionRecovering||Paused||(!Manual&&!(PhysicalOverlay&&VisionFallback))||(Phase!=CandyPhase.Ready&&Phase!=CandyPhase.CheckingCard)) return;
            cardPicker=false;CardRevision++;candidate=card; Phase=CandyPhase.CheckingCard; countdown=ConfirmationSeconds; scannedAgain=false; Message="Confirm: "+card.Label+"?"; BuildUI();
        }
        public void ConfirmCard()
        {
            if(StorageBlocked||SessionRecovering||Paused||Phase!=CandyPhase.CheckingCard||!candidate.HasValue) return;
            ReleasePieceGrabs(true);var card=candidate.Value; try{Game.ObserveCard(card);}catch(Exception e){Message=e.Message;return;}pendingShortcutPassed=false;if(!AutomaticPhysical||VisionFallback)SessionUnverified=true;if(!SaveSession("physical card confirmed; pending move"))return; candidate=null; AnnounceCard(card); StartCoroutine(PresentAndMove(card));
        }
        IEnumerator PresentAndMove(CandyCard card)
        {
            turnRemaining=0;turnRoot.gameObject.SetActive(false);Phase=CandyPhase.Revealing; BuildUI();
            yield return Cards.Reveal(CreateCard(card),Width,()=>Paused,card.Color==CandyColor.Picture?Width*.05f:0);
            var magic=Cards.CurrentCard.GetComponent<CandyCardMagic>();
            while(magic!=null&&magic.enabled&&!magic.TransferComplete)yield return null;
            BeginMove();
        }
        Transform CreateCard(CandyCard card,bool restored=false)
        {
            var root=new GameObject("Turn card: "+card.Label).transform;root.SetParent(content,false);
            Shape(root,"Card body",PrimitiveType.Cube,Vector3.zero,new Vector3(.09f,.00065f,.13f),new Color(1,.97f,.88f));
            if(card.Color!=CandyColor.Picture)
                for(int i=0;i<card.Count;i++)Shape(root,"Card color",PrimitiveType.Cube,new Vector3(0,.0007f,(i-(card.Count-1)*.5f)*.039f),new Vector3(.032f,.0002f,.032f),colors[(int)card.Color]);
            else
            {
                int index=Array.IndexOf(pictureNames,card.Picture);
                // Frostine's Characters entry is a card-reference photograph; use her existing cutout illustration.
                var texture=Resources.Load<Texture2D>((index==5?"Celebrations/":"Characters/")+CandyCharacterModel.Ids[index]);

                if(texture!=null)
                {
                    var art=GameObject.CreatePrimitive(PrimitiveType.Quad);art.name="Character illustration";Destroy(art.GetComponent<Collider>());art.transform.SetParent(root,false);art.transform.localPosition=Vector3.up*.003f;art.transform.localRotation=Quaternion.Euler(90,0,0);
                    float ratio=(float)texture.width/texture.height;float height=Mathf.Min(.09f,.073f/ratio);art.transform.localScale=new Vector3(height*ratio,height,1);
                    if(!characterCards.TryGetValue(index,out var m)){m=new Material(Resources.Load<Shader>("CharacterCard"));m.SetTexture("_MainTex",texture);materials.Add(m);characterCards[index]=m;}art.GetComponent<Renderer>().sharedMaterial=m;
                    root.gameObject.AddComponent<CandyCardMagic>().Initialize(art.GetComponent<Renderer>(),index,()=>Paused,()=>Cards.IsPresenting,restored,content,SpacePosition(!restored&&Game.HasPending?Game.Preview().To:0),Width);
                }
                Label(root,"Picture card name",card.Picture,new Vector3(0,.001f,-.053f),.0013f,new Color(.5f,.1f,.3f));
            }
            Label(root,"Card heading",PlayerName(Game.CurrentPlayer).ToUpper(),new Vector3(0,.001f,.052f),.0015f,new Color(.35f,.12f,.22f));
            return root;
        }
        void AnnounceCard(CandyCard card)
        {
            if(MoveMagic==null){var root=new GameObject("Candy movement magic");root.transform.SetParent(content,false);MoveMagic=root.AddComponent<CandyMoveMagic>();MoveMagic.IsPaused=()=>Paused;}
            var move=Game.Preview();int index=card.Color==CandyColor.Picture?Array.IndexOf(pictureNames,card.Picture):-1;
            if(!move.Stuck&&move.To!=move.From)
                MoveMagic.Play(content.InverseTransformPoint(pawns[Game.CurrentPlayer].position),SpacePosition(move.To),-1,Width,colors[(int)Data.spaces[move.To].color],DestinationTangent(move.To),DestinationSize(move.To),null,card.Color!=CandyColor.Picture);
            else MoveMagic.Clear();
            lastDrawn=card;Say(PlayerName(Game.CurrentPlayer)+" drew "+card.Label+".");
        }
        Vector3 DestinationTangent(int index)
        {
            return SpacePosition(Mathf.Min(Data.spaces.Length-1,index+1))-SpacePosition(Mathf.Max(0,index-1));
        }
        Vector2 DestinationSize(int index)
        {
            float spacing=Width*.033f;
            if(index>0)spacing=Mathf.Min(spacing,Vector3.Distance(SpacePosition(index),SpacePosition(index-1)));
            if(index<Data.spaces.Length-1)spacing=Mathf.Min(spacing,Vector3.Distance(SpacePosition(index),SpacePosition(index+1)));
            return new Vector2(Width*.021f,spacing*.78f);
        }
        public string PlayerName(int i) => i==0?"You":TwoHumans?"Player "+(i+1):new[]{"Silly Sally","Wobbly Walter","Giggles Greta"}[Mathf.Clamp(i-1,0,2)];
        bool shortcutEntrance;
        public int ExpectedManualSpace=>shortcutEntrance?Game.Preview().Via:Game.Preview().To;
        void BeginMove()
        {
            var move=Game.Preview(); lastPlayer=Game.CurrentPlayer;shortcutEntrance=!pendingShortcutPassed&&move.Via==33&&move.To==46;
            if(move.To!=move.From)pawns[lastPlayer].GetComponent<CandyPlayerPiece>().SetSludged(false);
            if(Manual || (PhysicalOverlay && Game.CurrentPlayer==0))
            {
                Phase=CandyPhase.AwaitingMove;
                Message=AutomaticPhysical&&!VisionFallback?(move.Stuck?"Stay here this turn. Let the camera see your piece.":"Move your piece to the glowing space, then lift your hand away."):move.Stuck?"Stay here this turn. Tap DONE.":"Move the physical piece to the glowing space, then tap DONE.";
                ShowDestination(ExpectedManualSpace); BuildUI();
            }
            else { Phase=CandyPhase.Animating; BuildUI(); StartCoroutine(AnimateMove(move,lastPlayer)); }
        }
        public bool ConfirmMove(int space)
        {
            if(StorageBlocked||SessionRecovering||Paused||Phase!=CandyPhase.AwaitingMove) return false;
            if(space!=ExpectedManualSpace) { Message="This card takes you to the glowing space."; return false; }
            if(shortcutEntrance){shortcutEntrance=false;pendingShortcutPassed=true;SaveSession("shortcut entrance confirmed");Phase=CandyPhase.Animating;BuildUI();StartCoroutine(ManualShortcut());return true;}
            CompleteMove(); return true;
        }
        IEnumerator ManualShortcut(){yield return BloomGumdropPass();Phase=CandyPhase.AwaitingMove;ShowDestination(Game.Preview().To);Message=AutomaticPhysical&&!VisionFallback?"Follow Gumdrop Pass, then lift your hand away.":"Follow Gumdrop Pass, then tap DONE.";BuildUI();}
        IEnumerator BloomGumdropPass(){var bloom=content.gameObject.AddComponent<CandyShortcutBloom>();yield return bloom.Show(SpacePosition(33),SpacePosition(46),()=>Paused);}
        IEnumerator AnimateMove(CandyMove move,int player)
        {
            int direction=move.Via>=move.From?1:-1;
            int first=move.From;
            LastHopCount=0;int steps=Mathf.Abs(move.Via-move.From)+(move.To!=move.Via?1:0);
            float hopSeconds=Mathf.Max(.045f,MoveSeconds/Mathf.Max(1,steps));
            // Animate each path square; shortcut then travels directly across its printed bridge.
            while(first!=move.Via)
            {
                first+=direction;LastHopCount++; yield return Hop(pawns[player],SpacePosition(first,player),hopSeconds);
            }
            if(move.Via==33&&move.To==46)yield return BloomGumdropPass();
            if(move.To!=move.Via){LastHopCount++;yield return Hop(pawns[player],SpacePosition(move.To,player),hopSeconds);}
            if(move.From==move.To) yield return Hop(pawns[player],SpacePosition(move.To,player),.25f);
            CompleteMove();
        }
        IEnumerator Hop(Transform pawn,Vector3 end,float duration)
        {
            Vector3 start=pawn.localPosition; float elapsed=0;
            while(elapsed<duration)
            {
                if(!Paused) elapsed+=Time.deltaTime;
                float t=Mathf.Clamp01(elapsed/duration); pawn.localPosition=Vector3.Lerp(start,end,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.025f; yield return null;
            }
            pawn.localPosition=end;
        }
        void CompleteMove()
        {
            var move=Game.Commit(Game.Preview().To);pendingShortcutPassed=false;if(PhysicalOverlay&&(!AutomaticPhysical||VisionFallback))SessionUnverified=true;if(!SaveSession("turn completed"))return;MoveMagic?.Landed(); ArrangePawnsOnSpaces(); destinationRing.gameObject.SetActive(false);
            Debug.Log($"[Candy] Move player={lastPlayer} from={move.From} to={move.To} turn={Game.Turn} visiblePieces={VisiblePawnCount}");
            pawns[lastPlayer].GetComponent<CandyPlayerPiece>().SetSludged(Data.spaces[move.To].releaseColor==(int)CandyColor.Red);
            UpdateNearby();
            if(move.Won) { Phase=CandyPhase.Finished; Say(PlayerName(Game.Winner)+" reached the Candy Castle! You made it!"); Message="A sweet adventure. Play again?"; }
            else
            {
                Phase=CandyPhase.Ready; aiAt=Time.unscaledTime+2.2f;ShowTurn();
                if(move.Stuck) Say(PlayerName(lastPlayer)+" stays here. Waiting for "+((CandyColor)Data.spaces[move.To].releaseColor)+".");
                else Say(PlayerName(Game.CurrentPlayer)+", your turn!");
                Message=Manual||AutomaticPhysical?"Draw one physical card for "+PlayerName(Game.CurrentPlayer)+".":Game.CurrentPlayer==0?"Your turn. Draw a card!":"Your friend is drawing...";
            }
            if(EffectsTestMode){turnRemaining=0;turnRoot.gameObject.SetActive(false);Message="Effects test - REPLAY SPECIAL or NEXT SPECIAL. Not saved.";}
            BuildUI();
        }
        void UpdateNearby()
        {
            int pos=Game.Position(0),best=0; float distance=float.MaxValue;
            for(int i=0;i<pictureNames.Length;i++)
            {
                int location=Array.FindIndex(Data.spaces,s=>s.picture==pictureNames[i]); float d=Mathf.Abs(pos-location);
                if(d<distance) {distance=d;best=i;}
            }
            string next=pictureNames[best]; if(next!=NearbyCharacter) { NearbyCharacter=next; CharacterChanged?.Invoke(next); }
        }
        void Say(string line) { HostLine=line; HostSpoke?.Invoke(line); }
        void ShowDestination(int index)
        {
            destinationRing.gameObject.SetActive(true); var p=SpacePosition(index); float radius=Width*.011f;
            for(int i=0;i<32;i++) {float a=i*Mathf.PI*2/32; destinationRing.SetPosition(i,p+new Vector3(Mathf.Cos(a)*radius,.001f,Mathf.Sin(a)*radius));}
        }
        public void TickConfirmation(float delta)
        {
            if(Paused||Phase!=CandyPhase.CheckingCard||scannedAgain||!candidate.HasValue) return;
            countdown-=delta;
            if(countdown<=0) {candidate=null;scannedAgain=true;RequestScan();BuildUI();}
        }
        void Update()
        {
            if(content==null) return;
            TickConfirmation(Time.unscaledDeltaTime);
            bool celebrating=Cards.IsPresenting||(Cards.CurrentCard!=null&&Cards.CurrentCard.TryGetComponent<CandyCardMagic>(out var cardMagic)&&cardMagic.ShowingIllustration);
            StatusPanel.gameObject.SetActive(!celebrating);
            if(turnRemaining>0){if(!Paused&&!celebrating)turnRemaining-=Time.unscaledDeltaTime;turnRoot.gameObject.SetActive(turnRemaining>0&&!celebrating);}
            music.SetPaused(Paused,(meta!=null&&meta.IsSpeaking)||CandyCharacterPlay.AnySpeaking);
            if(Paused){ReleaseCharacterStretches();ReleasePieceGrabs(true);}
            foreach(var id in pieceGrabs.Where(p=>Time.unscaledTime-p.Value.seen>.3f).Select(p=>p.Key).ToArray())EndPieceGrab(id,true);
            foreach(var id in stretchSeen.Where(p=>Time.unscaledTime-p.Value>.3f).Select(p=>p.Key).ToArray())UpdateCharacterStretch(id,Vector3.zero,true);
            if(Game!=null&&!EffectsTestMode&&!StorageBlocked&&!SessionRecovering&&!Paused&&!TwoHumans&&!Manual&&!AutomaticPhysical&&Phase==CandyPhase.Ready&&Game.CurrentPlayer>0&&Time.unscaledTime>=aiAt) DrawVirtual();
            
            if(worldStatus!=null) worldStatus.text=Wrap(StorageBlocked?Message:SessionRecovering?"Restore the saved layout, then MANUAL CHECK.":(Paused?"PAUSED":Message)+(Game!=null?" � "+(SessionUnverified?"MANUAL SESSION":EffectsTestMode?"EFFECTS TEST - NOT SAVED":"LOCAL SESSION"):""),40);
            if(hostCaption!=null) hostCaption.text=Wrap(HostLine,40);
            if(cardCaption!=null) cardCaption.text=CardText+(Phase==CandyPhase.CheckingCard&&candidate.HasValue&&!scannedAgain?"  ·  "+Mathf.CeilToInt(Countdown)+"s":"");
            if(metaCaption!=null&&meta!=null)metaCaption.text=Wrap(Time.unscaledTime<localLineUntil?localCharacterLine:AutomaticPhysical&&!VisionFallback&&Vision!=null?Vision.Status:meta.CharacterLine,48);
            if(Desktop) DesktopTouch();
        }
        void DesktopTouch()
        {
#if ENABLE_INPUT_SYSTEM
            if(Mouse.current==null||Camera.main==null) return;
            var pos=Mouse.current.position.ReadValue();
            if(desktopPanel.Contains(new Vector2(pos.x,Screen.height-pos.y))) return;
            var ray=Camera.main.ScreenPointToRay(pos); var plane=new Plane(transform.up,transform.position);
            if(plane.Raycast(ray,out float d)) { HoverWorld(ray.GetPoint(d)); if(Mouse.current.leftButton.wasPressedThisFrame)TouchWorld(ray.GetPoint(d)); }
#endif
        }
        public void TouchWorld(Vector3 point)
        {
            if(buttonBusy||Time.unscaledTime-lastTouch<.25f) return; lastTouch=Time.unscaledTime;
            if(placingPanel){var local=content.InverseTransformPoint(point);bool cancel=local.y>-.04f&&local.y<.18f&&buttons.Any(b=>b.text=="CANCEL PLACEMENT"&&b.Contains(new Vector2(local.x,local.z)));if(!cancel)StatusPanel.Pin(point);placingPanel=false;Message=cancel?"Panel placement cancelled.":"Panel pinned. It will keep facing you.";BuildUI();return;}
            if(calibrationStage>0) {Calibrate(point);return;}
            var p=content.InverseTransformPoint(point); if(p.y<-.04f||p.y>.18f) return;
            foreach(var b in buttons) if(b.Contains(new Vector2(p.x,p.z))) { PressButton(b); return; }
            if(TryNudgeCharacter(point))return;
            if(Phase==CandyPhase.AwaitingMove&&(!AutomaticPhysical||VisionFallback))
            {
                int closest=-1; float dist=.025f;
                for(int i=0;i<Data.spaces.Length;i++) {var v=SpacePosition(i);float d=Vector2.Distance(new Vector2(p.x,p.z),new Vector2(v.x,v.z));if(d<dist){dist=d;closest=i;}}
                if(closest>=0) ConfirmMove(closest);
            }
        }
        void Calibrate(Vector3 point)
        {
            if(calibrationStage==1) {calibrationStart=point;calibrationStage=2;Message="Pinch the bottom RIGHT corner of the printed board.";return;}
            var edge=point-calibrationStart;edge.y=0;
            if(edge.magnitude<.2f||edge.magnitude>1.5f) {Message="Corners should be 20-150 cm apart. Try the right corner again.";return;}
            Width=edge.magnitude;var right=edge.normalized;var forward=Vector3.Cross(right,Vector3.up);
            content.SetPositionAndRotation(calibrationStart+edge*.5f+forward*(Width*Aspect*.5f),Quaternion.LookRotation(forward,Vector3.up));
            boardFace.transform.localScale=Vector3.one*Width;boardBody.transform.localScale=new Vector3(Width,.012f,Width*Aspect);calibrationStage=0;Message="Board aligned. Check the path before play.";BuildHosts();BuildUI();
        }
        public void HoverWorld(Vector3 point)
        {
            if(content==null||calibrationStage>0)return;
            var p=content.InverseTransformPoint(point);if(p.y<-.04f||p.y>.18f)return;
            foreach(var b in buttons)if(b.Contains(new Vector2(p.x,p.z))){b.visual.Hover();return;}
        }
        void PressButton(Button b)
        {
            if(buttonBusy)return;buttonBusy=true;b.visual.Press();StartCoroutine(CommitButton(b.action));
        }
        IEnumerator CommitButton(Action action)
        {
            yield return new WaitForSecondsRealtime(.16f);buttonBusy=false;action();
        }
        readonly Dictionary<int,bool> pressArmed=new Dictionary<int,bool>();
        // Direct press requires an approach from above; a resting or newly tracked hand cannot repeat draw.
        public void DirectPressWorld(int handId,Vector3 point)
        {
            ObservePhysicalHand(handId,point);
            if(!Paused&&!relocating&&calibrationStage==0&&!placingPanel)
            {
                CandyCharacterActor contact=null;foreach(var host in hosts){var actor=host.GetComponent<CandyCharacterActor>();if(actor.Contains(point,.006f)){contact=actor;break;}}
                characterContacts.TryGetValue(handId,out var previous);
                if(contact!=null){if(previous!=contact)contact.Nudge(point);characterContacts[handId]=contact;return;}
                characterContacts.Remove(handId);
            }
            if(stretchedCharacters.ContainsKey(handId)||pieceGrabs.ContainsKey(handId)||(TreatBucket!=null&&TreatBucket.IsHolding(handId)))return;
            var p=content.InverseTransformPoint(point);
            if(p.y>.045f)pressArmed[handId]=true;
            if(buttonBusy||p.y<-.025f||p.y>.018f)return;
            if(!pressArmed.TryGetValue(handId,out bool armed)||!armed)return;
            foreach(var b in buttons)if(b.Contains(new Vector2(p.x,p.z)))
            {pressArmed[handId]=false;lastTouch=Time.unscaledTime;PressButton(b);return;}
        }
        sealed class PieceGrab {public int index;public Vector3 offset,home;public float seen;}
        readonly Dictionary<int,PieceGrab> pieceGrabs=new Dictionary<int,PieceGrab>();
        public int GrabbedPieceCount=>pieceGrabs.Count;
        public bool BeginPieceGrab(int handId,Vector3 worldPoint)
        {
            if(Paused||relocating||placingPanel||calibrationStage>0||Phase==CandyPhase.Animating||Phase==CandyPhase.Revealing||pieceGrabs.ContainsKey(handId)||stretchedCharacters.ContainsKey(handId))return false;
            int selected=-1;float nearest=float.MaxValue;
            for(int i=0;i<pawns.Count;i++)
            {
                var pawn=pawns[i];if(!pawn.gameObject.activeInHierarchy||pieceGrabs.Values.Any(g=>g.index==i))continue;var p=pawn.InverseTransformPoint(worldPoint);
                if(Mathf.Abs(p.x)>.028f||Mathf.Abs(p.z)>.018f||p.y<-.004f||p.y>.058f)continue;
                float distance=(p-new Vector3(0,.025f,0)).sqrMagnitude;if(distance<nearest){nearest=distance;selected=i;}
            }
            if(selected<0)return false;var chosen=pawns[selected];pieceGrabs[handId]=new PieceGrab{index=selected,home=chosen.localPosition,offset=chosen.localPosition-chosen.parent.InverseTransformPoint(worldPoint),seen=Time.unscaledTime};return true;
        }
        public bool UpdatePieceGrab(int handId,Vector3 point,bool released)
        {
            if(!pieceGrabs.TryGetValue(handId,out var grab))return false;
            if(released||Paused||relocating||Phase==CandyPhase.Animating||Phase==CandyPhase.Revealing){EndPieceGrab(handId,Paused||relocating||Phase==CandyPhase.Animating||Phase==CandyPhase.Revealing);return true;}
            var pawn=pawns[grab.index];var p=pawn.parent.InverseTransformPoint(point)+grab.offset;p.x=Mathf.Clamp(p.x,-Width*.65f,Width*.65f);p.z=Mathf.Clamp(p.z,-Width*Aspect*.7f,Width*Aspect*.7f);p.y=Mathf.Clamp(p.y,.004f,Width*.65f);pawn.localPosition=SeparatePawn(p,grab.index);grab.seen=Time.unscaledTime;return true;
        }
        void EndPieceGrab(int handId,bool cancelled)
        {
            if(!pieceGrabs.TryGetValue(handId,out var grab))return;
            if(grab.index<pawns.Count&&pawns[grab.index]!=null){if(Game!=null){pawns[grab.index].localPosition=SpacePosition(Game.Position(grab.index),grab.index);}else if(cancelled)pawns[grab.index].localPosition=SeparatePawn(grab.home,grab.index);else{var resting=pawns[grab.index].localPosition;resting.y=.004f;pawns[grab.index].localPosition=SeparatePawn(resting,grab.index);}}
            pieceGrabs.Remove(handId);
        }
        void ReleasePieceGrabs(bool cancelled){foreach(var hand in pieceGrabs.Keys.ToArray())EndPieceGrab(hand,cancelled);}
        readonly Dictionary<int,CandyCharacterActor> stretchedCharacters=new Dictionary<int,CandyCharacterActor>();
        readonly Dictionary<int,float> stretchSeen=new Dictionary<int,float>();
        public bool BeginCharacterStretch(int handId,Vector3 point)
        {
            if(Paused||relocating||placingPanel||calibrationStage>0||pieceGrabs.ContainsKey(handId)||stretchedCharacters.ContainsKey(handId))return false;
            foreach(var host in hosts){var actor=host.GetComponent<CandyCharacterActor>();if(actor.Contains(point,.008f)&&actor.BeginStretch(point)){stretchedCharacters[handId]=actor;characterContacts[handId]=actor;stretchSeen[handId]=Time.unscaledTime;return true;}}return false;
        }
        public bool UpdateCharacterStretch(int handId,Vector3 point,bool released)
        {
            if(!stretchedCharacters.TryGetValue(handId,out var actor))return false;
            if(actor==null){stretchedCharacters.Remove(handId);stretchSeen.Remove(handId);return true;}
            if(released||Paused||relocating){actor.EndStretch(Paused||relocating);stretchedCharacters.Remove(handId);stretchSeen.Remove(handId);}
            else{actor.DragStretch(point);stretchSeen[handId]=Time.unscaledTime;}return true;
        }
        void ReleaseCharacterStretches(){foreach(var actor in stretchedCharacters.Values)if(actor!=null)actor.EndStretch(true);stretchedCharacters.Clear();stretchSeen.Clear();}
        readonly Dictionary<int,CandyCharacterActor> characterContacts=new Dictionary<int,CandyCharacterActor>();
        bool TryNudgeCharacter(Vector3 point){if(Paused||relocating)return false;foreach(var host in hosts){var actor=host.GetComponent<CandyCharacterActor>();if(actor.Contains(point)){actor.Nudge(point);return true;}}return false;}
        public void ForgetDirectHand(int handId){pressArmed.Remove(handId);characterContacts.Remove(handId);if(stretchedCharacters.TryGetValue(handId,out var actor)){actor.EndStretch(true);stretchedCharacters.Remove(handId);stretchSeen.Remove(handId);}EndPieceGrab(handId,true);TreatBucket?.Release(handId,true);}
        public void BeginRelocation()
        {
            SaveSession("board moving");
            if(relocating)return;Vision?.Invalidate(false);ReleaseCharacterStretches();ReleasePieceGrabs(true);relocating=true;pausedBeforeRelocation=Paused;Paused=true;
            relocationRenderers.Clear();foreach(var r in GetComponentsInChildren<Renderer>())if(r.enabled){relocationRenderers.Add(r);r.enabled=false;}
        }
        public void EndRelocation(){if(!relocating)return;relocating=false;Paused=pausedBeforeRelocation;foreach(var r in relocationRenderers)if(r!=null)r.enabled=true;relocationRenderers.Clear();}
        void AddRound(string text,int slot,Action action,Color color,bool active=false,bool primary=false)
        {
            float diameter=primary?.15f:.075f;
            float x=slot==0?0:Mathf.Sign(slot)*(.137f+(Mathf.Abs(slot)-1)*.085f),z=-Width*Aspect*.5f-.10f;
            var root=new GameObject(text+" button");root.transform.SetParent(ui,false);root.transform.localPosition=new Vector3(x,.010f,z);
            var label=Label(ui,text,text.Replace(" ","\n"),new Vector3(x,.016f,z),primary?.0035f:.00165f,Color.white);
            var visual=root.AddComponent<TableButtonVisual>();visual.Build(label.transform,color,active,true,diameter,primary);visual.UseGoldTrim();
            float hit=primary?.18f:.081f;buttons.Add(new Button{rect=new Rect(x-hit*.5f,z-hit*.5f,hit,hit),action=action,text=text,visual=visual,circular=true});
        }
        void AddDrawButton()=>AddRound("DRAW CARD",0,DrawVirtual,new Color(.01f,.85f,.03f),true,true);
        void AddButton(string text,int col,int row,Action action,Color? color=null,bool active=false)
        {
            float w=.145f,h=.038f; float x=(col-1.5f)*.153f,z=-Width*Aspect*.5f-.045f-row*.046f;
            var root=new GameObject(text+" button");root.transform.SetParent(ui,false);root.transform.localPosition=new Vector3(x,.010f,z);
            var label=Label(ui,text,text,new Vector3(x,.016f,z),.0018f,Color.white);
            var visual=root.AddComponent<TableButtonVisual>();visual.Build(label.transform,new Color(.43f,.055f,.73f),active);visual.UseGoldTrim();
            buttons.Add(new Button{rect=new Rect(x-w*.5f,z-h*.5f,w,h),action=action,text=text,visual=visual});
        }
        void BuildUI()
        {
            if(content==null) return;
            if(ui!=null) {ui.gameObject.SetActive(false);Destroy(ui.gameObject);} buttons.Clear();scores.Clear();
            ui=new GameObject("Hands-only controls").transform;ui.SetParent(content,false);
            if(panelText!=null){panelText.gameObject.SetActive(false);Destroy(panelText.gameObject);}
            panelText=new GameObject("Status text").transform;panelText.SetParent(StatusPanel.transform,false);
            panelText.gameObject.AddComponent<TableCabinetVisual>().Panel(.55f,.27f,true);
            PanelLabel("Brand","LIVING TABLE / CANDY LAND",.105f,.0032f,new Color(1,.85f,.5f));
            hostCaption=PanelLabel("Host caption",Wrap(HostLine,40),.060f,.0025f,Color.white);
            worldStatus=PanelLabel("Status",Wrap(Message,40),.010f,.0025f,Color.white);
            cardCaption=PanelLabel("Card",CardText,-.036f,.0024f,new Color(1,.85f,.5f));
            metaCaption=PanelLabel("Conversation","",-.115f,.0018f,Color.white);
            string standings=Game==null?"Choose your game":string.Join("   ",System.Linq.Enumerable.Range(0,Game.Players).Select(i=>PlayerName(i)+" "+(Game.Position(i)+1)));
            scoreText=PanelLabel("Positions",Wrap(standings,40),-.077f,.0022f,Color.white);
            if(sessionMenu&&Game!=null){SessionUI();return;}
            if(Phase==CandyPhase.Setup)
            {
                AddRound("PLAY NOW",0,()=>StartGame(Environment.TickCount),new Color(.55f,.08f,.85f),true,true);
                AddButton("VIRTUAL + AI",0,4,()=>Configure(Opponents,false,false,false),active:!PhysicalOverlay);
                AddButton("PHYSICAL + AI",1,4,()=>Configure(Opponents,false,false,true),active:PhysicalOverlay&&!Manual);
                AddButton("MANUAL PHYSICAL",2,4,()=>Configure(Opponents,true,false,true),active:Manual&&!TwoHumans);
                AddButton("TWO HUMANS",3,4,()=>Configure(1,true,true,true),active:TwoHumans);
                AddButton("ALIGN BOARD",0,5,()=>{Vision?.Invalidate();calibrationStage=1;Message="Pinch the bottom LEFT corner of the board.";});
                AddButton("TIMER: "+ConfirmationSeconds+"s",1,5,()=>SetTimer(ConfirmationSeconds>=30?3:ConfirmationSeconds+1));
                AddButton("MOVE BOARD",2,5,()=>FindFirstObjectByType<LivingTableExperience>()?.MoveCandyTable());
                AddButton("AI: "+Opponents,3,5,()=>Configure(Opponents%3+1,Manual,TwoHumans,PhysicalOverlay));
                AddButton(StatusPanel.Floating?"PIN PANEL":"FLOAT PANEL",0,6,()=>{StatusPanel.SetFloating(!StatusPanel.Floating);BuildUI();});
                AddButton(placingPanel?"CANCEL PLACEMENT":"MOVE PANEL",1,6,TogglePanelPlacement);
                AddButton(music.Enabled?"MUSIC ON":"MUSIC OFF",2,6,()=>{music.Toggle();BuildUI();});
                AddButton(CharacterVoicesEnabled?"HOST OFF":"HOST ON",3,6,ToggleCharacterVoices);
                AddButton(CameraEnabled?"CAMERA ON":"CAMERA OFF",2,7,ToggleCamera,active:CameraEnabled);
                AddButton("CHESS",3,7,()=>FindFirstObjectByType<LivingTableExperience>()?.SelectChess());
                AddButton("TEST SPECIAL CARDS",0,7,StartEffectsTest);
                AddButton("YOUR COLOR: "+new[]{"RED","BLUE","GREEN","YELLOW"}[PlayerColor],1,7,CyclePlayerColor);
                Message=TwoHumans?"Two humans · use your real pieces":Manual?"Manual · move all real pieces yourself":PhysicalOverlay?"Physical board · your real piece + virtual AI":"Virtual game · "+(Opponents+1)+" colored pieces at START";
            }
            else if(cardPicker&&(Manual||VisionFallback)&&!Paused&&(Phase==CandyPhase.Ready||Phase==CandyPhase.CheckingCard))
            {
                AddRound("BACK",-4,()=>{cardPicker=false;BuildUI();},new Color(.3f,.2f,.5f));
                AddRound("NEXT CARDS",4,()=>{pickerPage=(pickerPage+1)%3;BuildUI();},new Color(.05f,.4f,.8f));
                int[] slots={-3,-2,-1,1,2,3};
                for(int i=0;i<6;i++){int c=i;if(pickerPage<2)AddRound(((CandyColor)i).ToString()+" x"+(pickerPage+1),slots[i],()=>SelectPhysicalCard(new CandyCard((CandyColor)c,pickerPage+1)),colors[i]);else AddRound(pictureNames[i],slots[i],()=>SelectPhysicalCard(new CandyCard(CandyColor.Picture,1,pictureNames[c])),colors[i]);}
            }
            else
            {
                AddRound(Paused?"RESUME":"PAUSE",-1,()=>{if(StorageBlocked)return;Vision?.Invalidate(false);Paused=!Paused;SaveSession("pause changed");BuildUI();},new Color(.95f,.47f,.025f),Paused);
                AddRound("MOVE BOARD",-2,()=>FindFirstObjectByType<LivingTableExperience>()?.MoveCandyTable(),new Color(.025f,.48f,.92f));
                AddRound(music.Enabled?"MUSIC ON":"MUSIC OFF",-3,()=>{music.Toggle();BuildUI();},new Color(.65f,.08f,.85f),music.Enabled);
                AddRound("NEW GAME",-4,ReturnToSetup,new Color(.85f,.06f,.13f));
                AddRound(CharacterVoicesEnabled?"HOST OFF":"HOST ON",1,ToggleCharacterVoices,new Color(.02f,.67f,.64f),CharacterVoicesEnabled);
                AddRound(StatusPanel.Floating?"PIN PANEL":"FLOAT PANEL",2,()=>{StatusPanel.SetFloating(!StatusPanel.Floating);BuildUI();},new Color(.85f,.12f,.46f),StatusPanel.Floating);
                AddRound(placingPanel?"CANCEL PLACEMENT":"MOVE PANEL",3,TogglePanelPlacement,new Color(.38f,.45f,.82f));
                if(!Paused&&!StorageBlocked&&!SessionRecovering)
                {
                    if(PhysicalOverlay&&CameraEnabled)AddRound(VisionFallback?"AUTO":"MANUAL",-5,ToggleVisionFallback,new Color(.35f,.2f,.7f));
                    if(EffectsTestMode&&Phase==CandyPhase.Ready){AddRound("REPLAY SPECIAL",0,()=>DrawTestSpecial(testCharacter),new Color(.7f,.1f,.9f),true,true);AddRound("NEXT SPECIAL",4,()=>DrawTestSpecial((testCharacter+1)%6),new Color(1,.4f,.1f));}
                    else if(Phase==CandyPhase.Ready&&!Manual&&!AutomaticPhysical&&(Game.CurrentPlayer==0||TwoHumans))AddDrawButton();
                    if((Manual||VisionFallback)&&NeedsPhysicalCard&&(Phase==CandyPhase.Ready||Phase==CandyPhase.CheckingCard))
                    {
                        AddRound("CHOOSE CARD",4,()=>{cardPicker=true;BuildUI();},new Color(.68f,.47f,.06f));
                        if(candidate.HasValue)AddRound("CONFIRM CARD",0,ConfirmCard,new Color(.03f,.8f,.2f),true,true);
                        else if(!AutomaticPhysical)AddRound("READ CARD",0,BeginScan,new Color(.03f,.8f,.2f),true,true);
                    }
                    if(Phase==CandyPhase.AwaitingMove&&(!AutomaticPhysical||VisionFallback))AddRound("DONE",0,()=>ConfirmMove(ExpectedManualSpace),new Color(.03f,.8f,.2f),true,true);
                }
            }
            SessionUI();
        }
        void TogglePanelPlacement(){placingPanel=!placingPanel;Message=placingPanel?"Pinch where you want the panel.":"Panel placement cancelled.";BuildUI();}
        TextMesh PanelLabel(string name,string text,float y,float size,Color color)
        {var label=Label(panelText,name,text,new Vector3(0,y,0),size,new Color(.22f,.055f,.30f));label.transform.localRotation=Quaternion.identity;return label;}
        public void ShowTurn()
        {
            string name=PlayerName(Game.CurrentPlayer);
            turnText.text=Game.CurrentPlayer==0?"You're up!":Game.Turn%2==0?"You're up,\n"+name+"!":name+"'s turn!";
            turnRoot.localPosition=new Vector3(0,Width*.25f,0);turnRemaining=1.8f;turnRoot.gameObject.SetActive(true);turnRoot.GetComponent<TableFacingPanel>().FaceViewer();
        }
        void OnGUI()
        {
            if(!Desktop||content==null) return;
            desktopPanel.height=Mathf.Min(Screen.height-24,690);GUILayout.BeginArea(desktopPanel,GUI.skin.box);
            desktopScroll=GUILayout.BeginScrollView(desktopScroll);
            GUILayout.Label("LIVING TABLE\nCandy Land · 1984 rules");GUILayout.Space(8);
            GUILayout.Label("GAME HOST: "+HostLine);GUILayout.Space(8);GUILayout.Label(Paused?"Paused":Message);GUILayout.Label("CARD: "+CardText);
            if(Phase==CandyPhase.CheckingCard&&candidate.HasValue&&!scannedAgain) GUILayout.Label("Confirm in "+Mathf.CeilToInt(Countdown)+"s · then rescan");
            GUILayout.Space(8);
            var snapshot=buttons.ToArray();foreach(var b in snapshot) if(GUILayout.Button(b.text,GUILayout.Height(25))) {PressButton(b);break;}
            GUILayout.Space(8);GUILayout.Label("Nearby: "+NearbyCharacter);
            if(meta!=null){GUILayout.Label(meta.Status);GUILayout.Label(meta.CharacterLine);}
            GUILayout.Label("Character card illustrations.");
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
        void OnApplicationPause(bool pause) {if(pause){ReleaseCharacterStretches();ReleasePieceGrabs(true);}if(pause&&Game!=null){Paused=true;SaveSession("suspended");BuildUI();}}
        static string Wrap(string value,int width)
        {
            if(string.IsNullOrEmpty(value))return "";
            var words=value.Split(' ');string result="";int line=0;
            foreach(var word in words){if(line+word.Length>width){result+="\n";line=0;}result+=word+" ";line+=word.Length+1;}return result.TrimEnd();
        }
        void OnDestroy() {StopAllCoroutines();foreach(var m in materials) if(m!=null)Destroy(m); if(boardFace!=null)Destroy(boardFace.GetComponent<MeshFilter>().sharedMesh);}
    }
}
