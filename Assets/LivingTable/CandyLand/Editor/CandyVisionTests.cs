#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LivingTable.CandyLand;
namespace LivingTable.Editor
{
 public static class CandyVisionTests
 {
  const string Folder="Assets/LivingTable/CandyLand/Resources/Vision/";
  static readonly string[] Names={"plumpy","mr-mint","jolly","gramma-nutt","princess-lolly","queen-frostine"};
  public static void Bake()
  {
   var edition=new CandyPhysicalVision.Edition{edition="Candy Land 1984",board=Reference("Assets/LivingTable/CandyLand/Resources/candy-board-reference.png",768,1800),pictures=new CandyVisionFeatures.Reference[6]};
   for(int i=0;i<6;i++)edition.pictures[i]=Reference("archives/living-table/references/candy-land/candyland-original/special-"+Names[i]+".jpg",400,700);
   Directory.CreateDirectory(Folder);File.WriteAllText(Folder+"candy-1984-features.json",JsonUtility.ToJson(edition));AssetDatabase.Refresh();
  }
  static Color32[] Load(string path,int width,out int height){var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);texture.LoadImage(File.ReadAllBytes(path));height=Mathf.RoundToInt(texture.height*(float)width/texture.width);var data=CandyPhysicalVision.Reduce(texture.GetPixels32(),texture.width,texture.height,width,height);UnityEngine.Object.DestroyImmediate(texture);return data;}
  static CandyVisionFeatures.Reference Reference(string path,int width,int count){var pixels=Load(path,width,out int height);return CandyVisionFeatures.Extract(pixels,width,height,count);}
  static void Check(bool ok,string message){if(!ok)throw new Exception("VISION TEST: "+message);}
  public static void Run()
  {
   Bake();var edition=JsonUtility.FromJson<CandyPhysicalVision.Edition>(File.ReadAllText(Folder+"candy-1984-features.json"));
   var stability=new CandyVisionStability();Check(!stability.Observe("Red",1),"First frame cannot commit");Check(!stability.Observe("Red",1),"Duplicate frame rejected");Check(!stability.Observe("Red",1.4),"Two frames cannot commit");Check(stability.Observe("Red",1.9),"Stable three-frame card");Check(!stability.Observe("Blue",2.2),"Changed card resets evidence");Check(!stability.Observe(null,2.4),"Missing observation resets evidence");Check(!stability.Observe("Blue",2.8),"Missing interval cannot bridge confirmation");Check(!stability.Observe("Blue",5),"Stale gap resets evidence");
   var colors=new[]{Color.red,new Color(.6f,0,.8f),Color.yellow,Color.blue,new Color(1,.4f,0),Color.green};
   for(int c=0;c<6;c++)for(int count=1;count<=2;count++){var image=Enumerable.Repeat(new Color32(70,70,70,255),240*160).ToArray();for(int y=35;y<120;y++)for(int x=25;x<(count==1?125:170);x++)image[y*240+x]=new Color32(230,230,220,255);for(int block=0;block<count;block++)for(int y=55;y<100;y++)for(int x=45+block*57;x<90+block*57;x++)image[y*240+x]=colors[c];var found=CandyVisionDetector.ReadColorCards(image,240,160,null);Check(found.Count==1&&found[0].Color==(CandyColor)c&&found[0].Count==count,"Color card "+c+" x"+count);var board=new[]{Vector2.zero,new Vector2(239,0),new Vector2(239,159),new Vector2(0,159)};Check(CandyVisionDetector.ReadColorCards(image,240,160,board).Count==0,"Board squares are not cards");}
   var blank=Enumerable.Repeat(new Color32(100,100,100,255),320*240).ToArray();Check(CandyVisionDetector.ReadColorCards(blank,320,240,null).Count==0,"Blank frame rejected");
   for(int id=0;id<6;id++){var original=Load("archives/living-table/references/candy-land/candyland-original/special-"+Names[id]+".jpg",320,out int h);var scene=Enumerable.Repeat(new Color32(60,55,50,255),640*480).ToArray();for(int y=0;y<h;y++)for(int x=0;x<320;x++){var p=original[y*320+x];scene[(y+70)*640+x+120]=new Color32((byte)(p.r*.82f),(byte)(p.g*.82f),(byte)(p.b*.82f),255);}var features=CandyVisionFeatures.Extract(scene,640,480,1200);var match=CandyVisionFeatures.Locate(edition.pictures[id],features,9);Check(match!=null,"Picture card matches at different scale/brightness: "+Names[id]);Check((match.Project(Vector2.zero)-new Vector2(120,70)).magnitude<12,"Picture card geometry: "+Names[id]);}
   var pixels=Load("Assets/LivingTable/CandyLand/Resources/candy-board-reference.png",640,out int bh);var boardFeatures=CandyVisionFeatures.Extract(pixels,640,bh,1200);Check(CandyVisionFeatures.Locate(edition.board,boardFeatures,14)!=null,"Board reference recognized at smaller scale");
   var timer=System.Diagnostics.Stopwatch.StartNew();var engine=new CandyVisionDetector(edition.board,edition.pictures);var boardResult=engine.Detect(pixels,640,bh,null,0);timer.Stop();Debug.Log("[CandyVisionTest] "+boardResult.diagnostic);Check(boardResult.board!=null&&!boardResult.card.HasValue,"Printed board never becomes a playing card");Debug.Log("[CandyVisionTest] Full detector desktop milliseconds: "+timer.ElapsedMilliseconds);
   for(int color=0;color<4;color++)Check(!CandyVisionDetector.FindPiece(pixels,640,bh,boardResult,boardResult.rectified,color).HasValue,"Unchanged detailed board cannot become a pawn: "+color);
   var baseImage=Enumerable.Repeat(new Color32(175,175,175,255),320*240).ToArray();var quad=new[]{Vector2.zero,new Vector2(319,0),new Vector2(319,239),new Vector2(0,239)};var background=CandyVisionDetector.Rectify(baseImage,320,240,quad);var pieceFrame=(Color32[])baseImage.Clone();for(int y=90;y<110;y++)for(int x=140;x<152;x++)pieceFrame[y*320+x]=new Color32(10,45,210,255);var finding=new CandyVisionDetector.Finding{inverse=CandyVisionFeatures.Fit(quad,new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up},new[]{0,1,2,3})};
   Check(CandyVisionDetector.FindPiece(pieceFrame,320,240,finding,background,1).HasValue,"Blue pawn foreground detected");Check(!CandyVisionDetector.FindPiece(pieceFrame,320,240,finding,background,0).HasValue,"Wrong pawn color rejected");Check(!CandyVisionDetector.FindPiece(baseImage,320,240,finding,background,1).HasValue,"Unchanged board cannot become a pawn");for(int y=90;y<110;y++)for(int x=200;x<212;x++)pieceFrame[y*320+x]=new Color32(10,45,210,255);Check(!CandyVisionDetector.FindPiece(pieceFrame,320,240,finding,background,1).HasValue,"Two same-color objects are ambiguous");
   // Independent geometric distortions, not exact replays of the reference pixels.
   for(int id=0;id<6;id++){var original=Load("archives/living-table/references/candy-land/candyland-original/special-"+Names[id]+".jpg",400,out int ph);var destination=new[]{new Vector2(135,75),new Vector2(495,120),new Vector2(460,120+ph*.8f),new Vector2(115,75+ph*.8f)};var canonical=new[]{Vector2.zero,new Vector2(399,0),new Vector2(399,ph-1),new Vector2(0,ph-1)};var warp=CandyVisionFeatures.Fit(destination,canonical,new[]{0,1,2,3});var scene=Enumerable.Repeat(new Color32(45,45,45,255),640*480).ToArray();for(int y=0;y<480;y++)for(int x=0;x<640;x++){var q=CandyVisionFeatures.ProjectPoint(warp,new Vector2(x,y));if(q.x>=0&&q.y>=0&&q.x<400&&q.y<ph)scene[y*640+x]=original[(int)q.y*400+(int)q.x];}Check(CandyVisionFeatures.Locate(edition.pictures[id],CandyVisionFeatures.Extract(scene,640,480,1200),9)!=null,"Tilted picture card: "+Names[id]);}Check(CandyVisionFeatures.Locate(edition.board,CandyVisionFeatures.Extract(blank,320,240),14)==null,"No invented board in blank image");
   var data=JsonUtility.FromJson<CandyBoardData>(File.ReadAllText("Assets/LivingTable/CandyLand/Resources/candy-board-1984.json"));var s=data.spaces[10];Check(CandyPhysicalVision.NearestSpace(data,new Vector2(s.x,1-s.y))==10,"Piece maps to printed square");Check(CandyPhysicalVision.NearestSpace(data,new Vector2(-1,-1))==-1,"Off-board piece rejected");
   Debug.Log("CANDY_VISION_TESTS_PASSED");
  }
  public static void RunBatch(){try{Run();EditorApplication.Exit(0);}catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
#endif
