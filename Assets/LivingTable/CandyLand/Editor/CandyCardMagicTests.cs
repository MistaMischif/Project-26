#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LivingTable.CandyLand;
namespace LivingTable.Editor
{
 public static class CandyCardMagicTests
 {
  static CandyTable table;static Transform card,art;static CandyCardMagic magic;static float began;static int stage;static double deadline;
  public static void Run(){CandyBuilder.RulesTests();CandyVisionTests.Run();EditorSceneManager.OpenScene("Assets/LivingTable/CandyLand/CandyLandDesktop.unity");SessionState.SetBool("CardMagicTest",true);EditorApplication.EnterPlaymode();}
  [InitializeOnLoadMethod]static void Resume(){if(!SessionState.GetBool("CardMagicTest",false))return;deadline=EditorApplication.timeSinceStartup+90;stage=0;EditorApplication.update+=Tick;}
  static void Check(bool value,string message){if(!value)throw new Exception(message);}
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Card test timeout");if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
   if(stage==0){table=UnityEngine.Object.FindFirstObjectByType<CandyTable>();if(table==null||table.Cards==null)return;table.StartVirtualGame(42);Check(table.GetComponentsInChildren<CandyCharacterActor>(true).Length==0,"No 3D characters");Check(table.TreatBucket==null,"No treat bucket");Check(table.VisiblePawnCount==2,"Player pieces preserved");var create=typeof(CandyTable).GetMethod("CreateCard",BindingFlags.NonPublic|BindingFlags.Instance);card=(Transform)create.Invoke(table,new object[]{new CandyCard(CandyColor.Picture,1,"Plumpy"),false});art=card.Find("Character illustration");magic=card.GetComponent<CandyCardMagic>();Check(magic!=null&&art!=null,"Card illustration and magic");Check(art.GetComponent<CandyCardPose>()==null,"No portrait pose animation");table.StartCoroutine(table.Cards.Reveal(card,table.Width,()=>false));began=Time.time;stage=1;
   }else if(stage==1){Check(art.localPosition==Vector3.up*.001f,"Portrait rigidly attached");if(table.Cards.IsPresenting){Check(magic.Dissolve==0,"No dissolve during flight");return;}Check(Time.time-began>=1.9f,"Two second reveal preserved");stage=2;
   }else if(stage==2){if(magic.enabled)return;Check(magic.Dissolve==1&&magic.ParticleCount==0,"Dissolve completes and particles cleaned");Check(card!=null&&table.Cards.SettledCount==1,"Discard remains");Finish(0,"CARD_MAGIC_TESTS_PASSED");}
  }catch(Exception e){Finish(1,e.ToString());}}
  static void Finish(int code,string message){SessionState.SetBool("CardMagicTest",false);EditorApplication.update-=Tick;if(code==0)Debug.Log(message);else Debug.LogError(message);EditorApplication.Exit(code);}
 }
}
#endif
