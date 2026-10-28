using System;
using System.Collections.Generic;
using UnityEngine;
namespace LivingTable.CandyLand
{
    public sealed class CandyVisionDetector
    {
        public sealed class Finding {public CandyCard? card;public Vector2? cardCenter;public Vector2[] board;public double[] inverse;public int boardInliers;public Vector2? piece;public bool boardClear;public Color32[] rectified;public string diagnostic;}
        public readonly CandyVisionFeatures.Reference boardReference;
        readonly CandyVisionFeatures.Reference[] pictures;
        public const int MapWidth=256,MapHeight=183;
        static readonly string[] Names={"Plumpy","Mr Mint","Jolly","Gramma Nutt","Princess Lolly","Queen Frostine"};
        public CandyVisionDetector(CandyVisionFeatures.Reference board,CandyVisionFeatures.Reference[] cards){boardReference=board;pictures=cards;}
        public Finding Detect(Color32[] frame,int w,int h,Color32[] background,int playerColor,Vector2[] projectedBoard=null)
        {
            var result=new Finding();var features=CandyVisionFeatures.Extract(frame,w,h,1200);var match=CandyVisionFeatures.Locate(boardReference,features,14);
            if(match!=null){var corners=CandyVisionFeatures.Corners(boardReference,match);if(CandyVisionFeatures.ValidQuad(corners,w,h,w*h*.12f)){
                var canonical=new[]{Vector2.zero,Vector2.right,new Vector2(1,1),Vector2.up};var inverse=CandyVisionFeatures.Fit(corners,canonical,new[]{0,1,2,3});
                if(inverse!=null){result.board=corners;result.inverse=inverse;result.boardInliers=match.inliers;result.rectified=Rectify(frame,w,h,corners);result.boardClear=true;if(background!=null)result.piece=FindPiece(frame,w,h,result,background,playerColor);}
            }}
            var exclusion=result.board??projectedBoard;var cards=new List<CandyCard>();var centers=new List<Vector2>();
            for(int i=0;i<pictures.Length;i++){var found=CandyVisionFeatures.Locate(pictures[i],features,9);if(found==null)continue;var q=CandyVisionFeatures.Corners(pictures[i],found);if(!CandyVisionFeatures.ValidQuad(q,w,h,2200))continue;var center=(q[0]+q[2])*.5f;if(exclusion!=null&&CandyVisionFeatures.Inside(center,exclusion))continue;cards.Add(new CandyCard(CandyColor.Picture,1,Names[i]));centers.Add(center);}
            // Color cards require isolated uniform color squares surrounded by pale card stock, outside the board.
            if(cards.Count==0)cards=ReadColorCards(frame,w,h,exclusion,out centers);
            if(cards.Count==1){result.card=cards[0];result.cardCenter=centers[0];}result.diagnostic=$"landmarks={result.boardInliers}, cards={cards.Count}, piece={result.piece.HasValue}";return result;
        }
        public static Color32[] Rectify(Color32[] source,int w,int h,Vector2[] q)
        {
            var matrix=CandyVisionFeatures.Fit(new[]{Vector2.zero,Vector2.right,new Vector2(1,1),Vector2.up},q,new[]{0,1,2,3});var pixels=new Color32[MapWidth*MapHeight];
            for(int y=0;y<MapHeight;y++)for(int x=0;x<MapWidth;x++){var p=CandyVisionFeatures.ProjectPoint(matrix,new Vector2((x+.5f)/MapWidth,(y+.5f)/MapHeight));pixels[y*MapWidth+x]=Sample(source,w,h,p);}
            return pixels;
        }
        static Color32 Sample(Color32[] source,int w,int h,Vector2 p)=>source[Mathf.Clamp(Mathf.RoundToInt(p.y),0,h-1)*w+Mathf.Clamp(Mathf.RoundToInt(p.x),0,w-1)];
        public static int ColorClass(Color32 p)
        {
            Color.RGBToHSV(p,out float hue,out float saturation,out float value);if(saturation<.43f||value<.19f)return -1;
            if(hue<.035f||hue>.96f)return 0;if(hue<.105f)return 4;if(hue<.19f)return 2;if(hue<.46f)return 5;if(hue<.69f)return 3;if(hue<.90f)return 1;return -1;
        }
        static bool White(Color32 p){Color.RGBToHSV(p,out _,out float s,out float v);return s<.28f&&v>.43f;}
        sealed class Blob {public int color;public List<int> pixels=new List<int>();public int minX=int.MaxValue,minY=int.MaxValue,maxX,maxY;public Vector2 center;public float size,angle;}
        static List<Blob> Blobs(int[] mask,int w,int h,int minimum)
        {
            var seen=new bool[mask.Length];var queue=new int[mask.Length];var blobs=new List<Blob>();
            for(int start=0;start<mask.Length;start++){if(mask[start]<0||seen[start])continue;var b=new Blob{color=mask[start]};int head=0,tail=0;queue[tail++]=start;seen[start]=true;
                while(head<tail){int i=queue[head++],x=i%w,y=i/w;b.pixels.Add(i);b.minX=Math.Min(x,b.minX);b.maxX=Math.Max(x,b.maxX);b.minY=Math.Min(y,b.minY);b.maxY=Math.Max(y,b.maxY);b.center+=new Vector2(x,y);
                    for(int d=0;d<4;d++){int nx=x+(d==0?-1:d==1?1:0),ny=y+(d==2?-1:d==3?1:0);if(nx<0||ny<0||nx>=w||ny>=h)continue;int n=ny*w+nx;if(!seen[n]&&mask[n]==b.color){seen[n]=true;queue[tail++]=n;}}
                }if(b.pixels.Count>=minimum){b.center/=b.pixels.Count;blobs.Add(b);}}
            return blobs;
        }
        public static List<CandyCard> ReadColorCards(Color32[] frame,int w,int h,Vector2[] board)=>ReadColorCards(frame,w,h,board,out _);
        static List<CandyCard> ReadColorCards(Color32[] frame,int w,int h,Vector2[] board,out List<Vector2> centers)
        {
            centers=new List<Vector2>();
            var mask=new int[frame.Length];for(int i=0;i<mask.Length;i++){Color.RGBToHSV(frame[i],out _,out float saturation,out float value);mask[i]=saturation>=.6f&&value>=.25f?ColorClass(frame[i]):-1;}var all=Blobs(mask,w,h,Math.Max(50,w*h/5000));var squares=new List<Blob>();
            foreach(var b in all){if(b.pixels.Count>w*h*.12f||(board!=null&&CandyVisionFeatures.Inside(b.center,board)))continue;
                float bestArea=float.MaxValue,bestAngle=0,bestX=0,bestY=0;Vector2 bestMin=Vector2.zero;
                for(int deg=0;deg<90;deg+=5){float a=deg*Mathf.Deg2Rad,c=Mathf.Cos(a),s=Mathf.Sin(a);var min=new Vector2(float.MaxValue,float.MaxValue);var max=new Vector2(float.MinValue,float.MinValue);
                    foreach(int i in b.pixels){float x=i%w,y=i/w;var p=new Vector2(c*x+s*y,-s*x+c*y);min=Vector2.Min(min,p);max=Vector2.Max(max,p);}float dx=max.x-min.x+1,dy=max.y-min.y+1;if(dx*dy<bestArea){bestArea=dx*dy;bestAngle=a;bestX=dx;bestY=dy;bestMin=min;}}
                if(bestX<10||bestY<10||bestX/bestY<.65f||bestX/bestY>1.55f||b.pixels.Count/bestArea<.82f)continue;
                float cos=Mathf.Cos(bestAngle),sin=Mathf.Sin(bestAngle);int whites=0,total=0;
                for(int side=0;side<4;side++)for(int step=0;step<7;step++){float t=(step+1)/8f;float x=bestMin.x+(side<2?(side==0?-.13f:1.13f):t)*bestX;float y=bestMin.y+(side>=2?(side==2?-.13f:1.13f):t)*bestY;var p=new Vector2(cos*x-sin*y,sin*x+cos*y);total++;if(p.x>=0&&p.y>=0&&p.x<w&&p.y<h&&White(Sample(frame,w,h,p)))whites++;}
                if(whites<total*.88f)continue;b.size=Mathf.Sqrt(bestArea);b.angle=bestAngle;squares.Add(b);
            }
            var result=new List<CandyCard>();var used=new HashSet<Blob>();foreach(var a in squares){if(used.Contains(a))continue;Blob pair=null;foreach(var b in squares){if(b==a||used.Contains(b)||b.color!=a.color||Mathf.Abs(a.size-b.size)>a.size*.25f)continue;float d=Vector2.Distance(a.center,b.center)/a.size;if(d>1.02f&&d<1.65f&&White(Sample(frame,w,h,(a.center+b.center)*.5f))){if(pair!=null){pair=null;break;}pair=b;}}
                used.Add(a);if(pair!=null)used.Add(pair);if(PaperBoundary(frame,w,h,a,pair)){result.Add(new CandyCard((CandyColor)a.color,pair==null?1:2));centers.Add(pair==null?a.center:(a.center+pair.center)*.5f);}}
            return result;
        }
        static bool PaperBoundary(Color32[] frame,int w,int h,Blob a,Blob b)
        {
            var center=b==null?a.center:(a.center+b.center)*.5f;var right=new Vector2(Mathf.Cos(a.angle),Mathf.Sin(a.angle));var up=new Vector2(-right.y,right.x);var extents=new float[4];
            for(int side=0;side<4;side++){var direction=side<2?right*(side==0?1:-1):up*(side==2?1:-1);float whiteStart=-1,brightness=0;
                float start=Vector2.Dot(a.center-center,direction)+a.size*.55f;if(b!=null)start=Mathf.Max(start,Vector2.Dot(b.center-center,direction)+b.size*.55f);
                for(float d=start;d<a.size*3;d++){var p=center+direction*d;if(p.x<1||p.y<1||p.x>=w-1||p.y>=h-1)break;var sample=Sample(frame,w,h,p);float value=(sample.r+sample.g+sample.b)/765f;
                    if(whiteStart<0){if(White(sample)){whiteStart=d;brightness=value;}}else if(!White(sample)||Mathf.Abs(value-brightness)>.13f){if(d-whiteStart>=a.size*.10f){extents[side]=d;break;}whiteStart=-1;}
                }
                if(extents[side]==0)return false;
            }
            float width=extents[0]+extents[1],height=extents[2]+extents[3],ratio=Mathf.Max(width,height)/Mathf.Min(width,height);
            return ratio>1.15f&&ratio<2.1f&&Mathf.Min(width,height)>a.size*1.2f&&Mathf.Max(width,height)<a.size*3.4f;
        }
        public static Vector2? FindPiece(Color32[] frame,int w,int h,Finding finding,Color32[] background,int playerColor)
        {
            if(background.Length!=MapWidth*MapHeight)return null;int wanted=new[]{0,3,5,2}[playerColor%4];var mask=new int[w*h];Array.Fill(mask,-1);
            // Compare against the board's automatically captured appearance, registered independently each frame.
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){int i=y*w+x;if(ColorClass(frame[i])!=wanted)continue;var uv=CandyVisionFeatures.ProjectPoint(finding.inverse,new Vector2(x,y));if(uv.x<0||uv.y<0||uv.x>=1||uv.y>=1)continue;var old=background[Mathf.Clamp((int)(uv.y*MapHeight),0,MapHeight-1)*MapWidth+Mathf.Clamp((int)(uv.x*MapWidth),0,MapWidth-1)];Color a=frame[i],b=old;float difference=Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b);if(difference>.42f)mask[i]=wanted;}
            // A one-pixel registration edge is not a raised plastic piece. Remove thin residuals.
            var solid=new int[mask.Length];Array.Fill(solid,-1);for(int y=1;y<h-1;y++)for(int x=1;x<w-1;x++){int i=y*w+x;if(mask[i]<0)continue;bool full=true;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if(mask[i+dy*w+dx]<0)full=false;if(full)solid[i]=wanted;}
            var candidates=Blobs(solid,w,h,12);Vector2? result=null;
            foreach(var b in candidates){int dx=b.maxX-b.minX+1,dy=b.maxY-b.minY+1;if(b.pixels.Count>w*h*.012f||dx<4||dy<5||dx/(float)dy<.18f||dx/(float)dy>2.5f)continue;
                // The lowest visible part is the base, not the raised head (which has parallax).
                float px=0,count=0;foreach(int i in b.pixels)if(i/w<=b.minY+Math.Max(1,dy/6)){px+=i%w;count++;}var basePoint=new Vector2(px/count,b.minY+1);var uv=CandyVisionFeatures.ProjectPoint(finding.inverse,basePoint);
                if(result.HasValue)return null;result=uv;
            }return result;
        }
    }
    // Time continuity is separate from image classification and tested without a headset.
    public sealed class CandyVisionStability
    {
        string key;double began,last;int frames;public void Reset(){key=null;frames=0;began=last=0;}
        public bool Observe(string value,double timestamp,float requiredSeconds=.7f)
        {
            if(string.IsNullOrEmpty(value)||double.IsNaN(timestamp)||double.IsInfinity(timestamp)){Reset();return false;}
            if(timestamp<=last)return false;
            if(value!=key||timestamp-last>1.25){key=value;frames=1;began=last=timestamp;return false;}
            last=timestamp;frames++;return frames>=3&&timestamp-began>=requiredSeconds;
        }
    }
}
