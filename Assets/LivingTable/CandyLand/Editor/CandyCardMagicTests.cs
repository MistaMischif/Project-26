using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LivingTable.CandyLand;
namespace LivingTable.Editor
{
 public static class CandyCardMagicTests
 {
  static CandyTable table;static Transform card,art;static CandyCardMagic magic;static float began;static int stage,theme,savedCount;static double deadline,pauseAt;static Vector3 pausedPosition,pausedPopup,startingScale;static bool flightChecked,orbitChecked,bloomChecked;static float observedDegrees,lastAngle;static Vector3 pausedItem;
  static readonly string[] Names={"Plumpy","Mr Mint","Jolly","Gramma Nutt","Princess Lolly","Queen Frostine"};
  public static void Run(){CandyBuilder.RulesTests();CandyVisionTests.Run();CheckBoard();EditorSceneManager.OpenScene("Assets/LivingTable/CandyLand/CandyLandDesktop.unity");SessionState.SetBool("CardMagicTest",true);EditorApplication.EnterPlaymode();}
  static void CheckBoard()
  {
   var data=JsonUtility.FromJson<CandyBoardData>(File.ReadAllText("Assets/LivingTable/CandyLand/Resources/candy-board-1984.json"));
   var texture=new Texture2D(2,2);texture.LoadImage(File.ReadAllBytes("Assets/LivingTable/CandyLand/Resources/candy-board-reference.png"));
   float[] hues={.01f,.84f,.14f,.54f,.075f,.37f};
   for(int i=0;i<data.spaces.Length;i++)
   {
    var s=data.spaces[i];if(s.color==CandyColor.Picture)continue;
    var color=texture.GetPixel(Mathf.RoundToInt(s.x*texture.width),Mathf.RoundToInt((1-s.y)*texture.height));
    Color.RGBToHSV(color,out float h,out float saturation,out _);
    if(saturation<.35f){color=texture.GetPixel(Mathf.RoundToInt(s.x*texture.width)+7,Mathf.RoundToInt((1-s.y)*texture.height));Color.RGBToHSV(color,out h,out saturation,out _);}
    float delta=Mathf.Abs(h-hues[(int)s.color]);Check(Mathf.Min(delta,1-delta)<.08f&&saturation>.35f,"Printed square color agrees at index "+i);
   }
   UnityEngine.Object.DestroyImmediate(texture);
   Check(data.spaces[17].picture=="Mr Mint"&&data.spaces[18].color==CandyColor.Orange,"Mr Mint then orange printed order");
   var game=new CandyGame(data,2,42);Check(game.Resolve(16,new CandyCard(CandyColor.Orange)).To==18,"Orange skips Mr Mint");Check(game.Resolve(-1,new CandyCard(CandyColor.Picture,1,"Mr Mint")).To==17,"Mr Mint targets picture space");
   var state=new CandyTable.SessionState{game=game.Capture()};state.game.positions=new[]{17,18};CandyTable.MigrateBoardState(state);Check(state.game.positions.SequenceEqual(new[]{18,17}),"Old saves preserve occupied space identities");CandyTable.MigrateBoardState(state);Check(state.game.positions.SequenceEqual(new[]{18,17}),"Migration is idempotent");CandyGame.Restore(data,state.game);
   Debug.Log("CANDY_BOARD_ALIGNMENT_TESTS_PASSED");
  }
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("CardMagicTest",false))return;deadline=EditorApplication.timeSinceStartup+180;stage=theme=0;EditorApplication.update+=Tick;}
  static void Check(bool value,string message){if(!value)throw new Exception(message);}
  static void SetPause(bool value)=>typeof(CandyTable).GetProperty("Paused").SetValue(table,value);
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Card test timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
   if(stage>0&&magic!=null&&magic.enabled)CheckOrbit();
   if(stage==0){table=UnityEngine.Object.FindFirstObjectByType<CandyTable>();if(table==null||table.Cards==null||table.Vision==null)return;savedCount=GameSessionStore.List("candy-land-1984").Length;table.StartEffectsTest();Check(table.EffectsTestMode&&table.SessionId==null,"Separate unsaved test mode");Check(table.GetComponentsInChildren<CandyCharacterActor>(true).Length==0&&table.TreatBucket==null,"No legacy characters or bowl");BeginCheck();
   }else if(stage==1){Check(magic.Dissolve==0,"No dissolve during reveal");if(magic.Elapsed<.55f)return;
    Check(magic.Popup!=null&&!art.IsChildOf(card),"Illustration has lifted off the card");Check(Mathf.Abs(magic.Popup.localPosition.x)<.0001f&&Mathf.Abs(magic.Popup.localPosition.z)<.0001f,"Lifted illustration centered on board");
    Check(!table.StatusPanel.gameObject.activeSelf,"Status panel cannot cover the card");
    var viewer=Camera.main.transform.position-card.position;Check(Vector3.Dot(card.up,viewer.normalized)>.1f,"Card face points toward viewer");Capture(art,"Logs/Card-popup-"+theme+".png");
    var glow=table.MoveMagic.SquareGlow.GetComponent<Renderer>().sharedMaterial;Color.RGBToHSV(glow.color,out _,out float saturation,out _);Check(saturation>.99f&&glow.GetFloat("_DstBlend")==10,"Saturated glow replaces washed-out additive blend");
    SetPause(true);pausedPosition=card.position;pausedPopup=magic.Popup.position;pausedItem=magic.ItemPosition(0);pauseAt=EditorApplication.timeSinceStartup;stage=2;
   }else if(stage==2){if(EditorApplication.timeSinceStartup-pauseAt<.35)return;Check(card.position==pausedPosition&&magic.Popup.position==pausedPopup&&magic.ItemPosition(0)==pausedItem&&magic.Dissolve==0,"Pause freezes card and illustration");SetPause(false);stage=3;
   }else if(stage==3){Check(table.GetComponentsInChildren<CandyPlayerPiece>().First(p=>p.name=="Your avatar").transform.localScale==startingScale,"Pawn keeps starting scale while moving");if(table.Cards.IsPresenting){Check(magic.Dissolve==0,"Central illustration remains through flight");if(magic.Elapsed>1.3f){Check(card.localPosition.x>.05f,"Card is flying toward discard");Check(magic.Popup!=null&&Mathf.Abs(magic.Popup.localPosition.x)<.0001f&&Mathf.Abs(magic.Popup.localPosition.z)<.0001f,"Illustration stays centered while card flies away");if(!flightChecked)CaptureOverview("Logs/Themed-orbit-"+theme+".png");flightChecked=true;}return;}Check(Mathf.Abs(table.Cards.LastDuration-2)<.001f,"Two-second reveal preserved");if(table.Phase!=CandyPhase.Ready||magic.enabled)return;
    int target=Array.FindIndex(table.Data.spaces,s=>s.picture==Names[theme]);Check(table.Game.Position(0)==target,"Special card lands on matching illustration space");
    var pawn=table.GetComponentsInChildren<CandyPlayerPiece>().First(p=>p.name=="Your avatar");Check(Vector3.Distance(pawn.transform.localPosition,table.SpacePosition(target))<.0001f,"Pawn stays centered on its square");
    Check(flightChecked&&magic.ArrivedParticles==CandyCardMagic.ItemCount&&orbitChecked&&bloomChecked&&magic.SproutingObserved&&Vector3.Distance(magic.Target,table.SpacePosition(target))<.0001f,"Every burst particle reaches the legal destination");Check(magic.Dissolve==1&&magic.ParticleCount==0&&magic.Popup==null&&table.Cards.SettledCount==1,"Detached illustration and particle cleanup keeps discard");Check(GameSessionStore.List("candy-land-1984").Length==savedCount,"Tests do not create saved games");
    if(++theme==6){table.ReturnToSetup();Check(!table.EffectsTestMode,"Exit restores ordinary setup");Finish(0,"CARD_MAGIC_TESTS_PASSED");return;}
    Camera.main.transform.position=table.transform.position+new Vector3(0,.8f,theme%2==0?-.8f:.8f);table.DrawTestSpecial(theme);BeginCheck();
   }
  }catch(Exception e){Finish(1,e.ToString());}}
  static void BeginCheck(){card=table.Cards.CurrentCard;art=card.Find("Character illustration");magic=card.GetComponent<CandyCardMagic>();Check(art!=null&&magic!=null&&art.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex")!=null,"Illustration texture loaded for "+Names[theme]);Check(table.Game.PendingCard.Value.Picture==Names[theme],"Test control drew requested character");startingScale=table.GetComponentsInChildren<CandyPlayerPiece>().First(p=>p.name=="Your avatar").transform.localScale;flightChecked=orbitChecked=bloomChecked=false;observedDegrees=lastAngle=0;began=Time.time;stage=1;}
  static void CheckOrbit()
  {
   if(magic.Elapsed<CandyCardMagic.OrbitStart)return;
   if(magic.Elapsed<CandyCardMagic.OrbitStart+CandyCardMagic.OrbitDuration)
   {
    var local=Quaternion.Inverse(magic.OrbitFrame)*(magic.ItemPosition(0)-magic.OrbitCenter);float angle=Mathf.Atan2(local.y,local.x)*Mathf.Rad2Deg;observedDegrees+=Mathf.DeltaAngle(lastAngle,angle);lastAngle=angle;
    Check(magic.ArrivedParticles==0&&!magic.TransferComplete,"Items orbit before traveling to target");
    var mesh=magic.Item(0).GetComponent<MeshFilter>().sharedMesh;Check(mesh.vertexCount>100&&mesh.colors.Length==mesh.vertexCount,"Themed colored mesh assets replace plain sparkles");
   }
   else if(!orbitChecked){observedDegrees+=Mathf.DeltaAngle(lastAngle,0);Check(Mathf.Abs(observedDegrees-720)<1,"Observed two complete revolutions: "+observedDegrees);orbitChecked=true;}
   if(magic.Elapsed>CandyCardMagic.ArrivalTime+.9f&&!bloomChecked)
   {
    Check(magic.SproutedCount==CandyCardMagic.EnvironmentCount,"All destination scenery sprouts after arrival");Check(magic.Sprout(0).localScale.x>0,"Sprout visibly grows");
    Check(Vector3.Distance(new Vector3(magic.Sprout(0).localPosition.x,magic.Target.y,magic.Sprout(0).localPosition.z),magic.Target)<table.Width*.06f,"Environment belongs to destination square");
    CaptureOverview("Logs/Themed-sprout-"+theme+".png");bloomChecked=true;
   }
  }
  public static void Capture(Transform art,string path)
  {
   var go=new GameObject("Card verification camera");var camera=go.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.gray;
   camera.transform.position=art.position-art.forward*.4f;camera.transform.rotation=Quaternion.LookRotation(art.forward,art.up);camera.orthographic=true;camera.orthographicSize=.15f;camera.nearClipPlane=.001f;
   var rt=new RenderTexture(600,800,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(600,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,600,800),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
   var visible=image.GetPixels32();var renderer=art.GetComponent<Renderer>();renderer.enabled=false;camera.Render();image.ReadPixels(new Rect(0,0,600,800),0,0);image.Apply();renderer.enabled=true;
   var hidden=image.GetPixels32();int changed=0;for(int i=0;i<visible.Length;i++)if(Mathf.Abs(visible[i].r-hidden[i].r)+Mathf.Abs(visible[i].g-hidden[i].g)+Mathf.Abs(visible[i].b-hidden[i].b)>35)changed++;
   Check(changed>4000,"Rendered illustration contributes visible pixels (not occluded): "+path+" pixels="+changed);

   RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(go);
  }
  static void CaptureOverview(string path)
  {
   var go=new GameObject("Celebration overview camera");var camera=go.AddComponent<Camera>();camera.transform.position=table.transform.TransformPoint(new Vector3(0,.65f,-.8f));camera.transform.LookAt(table.transform.TransformPoint(new Vector3(0,.10f,0)));camera.fieldOfView=55;camera.nearClipPlane=.01f;
   var rt=new RenderTexture(1200,900,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1200,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1200,900),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(go);
  }
  static void Finish(int code,string message){SessionState.SetBool("CardMagicTest",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(message);else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
