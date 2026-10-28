using System;
using System.Collections.Generic;
using UnityEngine;
namespace LivingTable.CandyLand
{
    // Small, deterministic CPU feature matcher. References ship with the edition; no user training or network.
    public static class CandyVisionFeatures
    {
        [Serializable] public sealed class Point { public Vector2 p; public uint[] bits; public int strength; }
        [Serializable] public sealed class Reference { public int width,height; public Point[] points; }
        public sealed class Match { public double[] h; public int inliers; public float error; public Vector2 Project(Vector2 p)=>ProjectPoint(h,p); }
        static readonly int[] cx={0,1,2,3,3,3,2,1,0,-1,-2,-3,-3,-3,-2,-1},cy={3,3,2,1,0,-1,-2,-3,-3,-3,-2,-1,0,1,2,3};
        static readonly Vector2Int[] pairs=Pairs();
        static Vector2Int[] Pairs(){var r=new System.Random(1984);var p=new Vector2Int[512];for(int i=0;i<p.Length;i++)p[i]=new Vector2Int(r.Next(-10,11),r.Next(-10,11));return p;}
        public static Reference Extract(Color32[] pixels,int width,int height,int limit=900)
        {
            var result=new List<Point>();int w=width,h=height;var gray=new byte[w*h];for(int i=0;i<gray.Length;i++)gray[i]=(byte)((pixels[i].r*77+pixels[i].g*150+pixels[i].b*29)>>8);
            for(int level=0;level<4&&w>=80&&h>=60;level++)
            {
                var candidates=new List<Point>();
                for(int y=15;y<h-15;y++)for(int x=15;x<w-15;x++)
                {
                    int v=gray[y*w+x],bright=0,dark=0,score=0;
                    for(int j=0;j<24;j++){int d=gray[(y+cy[j%16])*w+x+cx[j%16]]-v;bright=d>16?bright+1:0;dark=d< -16?dark+1:0;score+=j<16?Math.Abs(d):0;if(bright>=9||dark>=9){candidates.Add(new Point{p=new Vector2(x,y),strength=score});break;}}
                }
                candidates.Sort((a,b)=>b.strength.CompareTo(a.strength));var occupied=new bool[w*h];int added=0;
                foreach(var p in candidates){int x=(int)p.p.x,y=(int)p.p.y;if(occupied[y*w+x])continue;
                    float mx=0,my=0;for(int dy=-8;dy<=8;dy++)for(int dx=-8;dx<=8;dx++){float value=gray[(y+dy)*w+x+dx];mx+=dx*value;my+=dy*value;}
                    float angle=Mathf.Atan2(my,mx),c=Mathf.Cos(angle),s=Mathf.Sin(angle);p.bits=new uint[8];
                    for(int b=0;b<256;b++){var a=pairs[b*2];var z=pairs[b*2+1];int ax=x+Mathf.RoundToInt(c*a.x-s*a.y),ay=y+Mathf.RoundToInt(s*a.x+c*a.y),bx=x+Mathf.RoundToInt(c*z.x-s*z.y),by=y+Mathf.RoundToInt(s*z.x+c*z.y);if(gray[ay*w+ax]<gray[by*w+bx])p.bits[b/32]|=1u<<(b%32);}
                    for(int dy=-5;dy<=5;dy++)for(int dx=-5;dx<=5;dx++)occupied[(y+dy)*w+x+dx]=true;
                    p.p=new Vector2((x+.5f)*width/w-.5f,(y+.5f)*height/h-.5f);result.Add(p);if(++added>=limit/4)break;
                }
                int nw=w*3/4,nh=h*3/4;var small=new byte[nw*nh];for(int y=0;y<nh;y++)for(int x=0;x<nw;x++){int sx=x*w/nw,sy=y*h/nh;small[y*nw+x]=(byte)((gray[sy*w+sx]+gray[sy*w+Math.Min(w-1,sx+1)]+gray[Math.Min(h-1,sy+1)*w+sx]+gray[Math.Min(h-1,sy+1)*w+Math.Min(w-1,sx+1)])/4);}gray=small;w=nw;h=nh;
            }
            return new Reference{width=width,height=height,points=result.ToArray()};
        }
        static int Pop(uint n){n-=((n>>1)&0x55555555);n=(n&0x33333333)+((n>>2)&0x33333333);return (int)((((n+(n>>4))&0x0F0F0F0F)*0x01010101)>>24);}
        static int Distance(uint[] a,uint[] b){int d=0;for(int i=0;i<8;i++)d+=Pop(a[i]^b[i]);return d;}
        public static Match Locate(Reference reference,Reference frame,int minimum=10)
        {
            var from=new List<Vector2>();var to=new List<Vector2>();var used=new HashSet<int>();
            foreach(var a in reference.points){int best=257,second=257,index=-1;for(int i=0;i<frame.points.Length;i++){int d=Distance(a.bits,frame.points[i].bits);if(d<best){second=best;best=d;index=i;}else if(d<second)second=d;}
                if(best<65&&best<second*.76f&&used.Add(index)){from.Add(a.p);to.Add(frame.points[index].p);}}
            if(from.Count<minimum)return null;var random=new System.Random(4403);var selected=new int[4];Match winner=null;var winning=new List<int>();
            for(int attempt=0;attempt<180;attempt++){
                for(int j=0;j<4;j++){bool duplicate;do{selected[j]=random.Next(from.Count);duplicate=false;for(int k=0;k<j;k++)if(selected[k]==selected[j])duplicate=true;}while(duplicate);}
                var matrix=Fit(from,to,selected);if(matrix==null)continue;var inside=new List<int>();float error=0;
                for(int i=0;i<from.Count;i++){float e=(ProjectPoint(matrix,from[i])-to[i]).sqrMagnitude;if(e<16){inside.Add(i);error+=e;}}
                if(inside.Count>=minimum&&(winner==null||inside.Count>winner.inliers)){winner=new Match{h=matrix,inliers=inside.Count,error=error/inside.Count};winning=inside;}
            }
            if(winner==null)return null;
            var min=new Vector2(float.MaxValue,float.MaxValue);var max=Vector2.zero;foreach(int i in winning){min=Vector2.Min(min,from[i]);max=Vector2.Max(max,from[i]);}
            if((max.x-min.x)/reference.width<.3f||(max.y-min.y)/reference.height<.3f)return null;
            winner.h=Fit(from,to,winning.ToArray())??winner.h;return winner;
        }
        public static Vector2 ProjectPoint(double[] h,Vector2 p){double z=h[6]*p.x+h[7]*p.y+1;if(Math.Abs(z)<1e-8)return new Vector2(1e6f,1e6f);return new Vector2((float)((h[0]*p.x+h[1]*p.y+h[2])/z),(float)((h[3]*p.x+h[4]*p.y+h[5])/z));}
        public static double[] Fit(IList<Vector2> from,IList<Vector2> to,int[] indices)
        {
            var normal=new double[8,9];foreach(int i in indices){double x=from[i].x,y=from[i].y,u=to[i].x,v=to[i].y;var a=new[]{x,y,1,0,0,0,-u*x,-u*y};var b=new[]{0,0,0,x,y,1,-v*x,-v*y};for(int r=0;r<8;r++){for(int c=0;c<8;c++)normal[r,c]+=a[r]*a[c]+b[r]*b[c];normal[r,8]+=a[r]*u+b[r]*v;}}
            for(int col=0;col<8;col++){int pivot=col;for(int row=col+1;row<8;row++)if(Math.Abs(normal[row,col])>Math.Abs(normal[pivot,col]))pivot=row;if(Math.Abs(normal[pivot,col])<1e-9)return null;for(int j=col;j<=8;j++){double temp=normal[col,j];normal[col,j]=normal[pivot,j];normal[pivot,j]=temp;}double div=normal[col,col];for(int j=col;j<=8;j++)normal[col,j]/=div;for(int row=0;row<8;row++)if(row!=col){double m=normal[row,col];for(int j=col;j<=8;j++)normal[row,j]-=m*normal[col,j];}}
            var h=new double[8];for(int i=0;i<8;i++){h[i]=normal[i,8];if(double.IsNaN(h[i])||double.IsInfinity(h[i]))return null;}return h;
        }
        public static bool Inside(Vector2 p,Vector2[] q){float sign=0;for(int i=0;i<4;i++){var a=q[(i+1)%4]-q[i];var b=p-q[i];float cross=a.x*b.y-a.y*b.x;if(i==0)sign=cross;else if(sign*cross<0)return false;}return true;}
        public static Vector2[] Corners(Reference r,Match m)=>new[]{m.Project(Vector2.zero),m.Project(new Vector2(r.width-1,0)),m.Project(new Vector2(r.width-1,r.height-1)),m.Project(new Vector2(0,r.height-1))};
        public static bool ValidQuad(Vector2[] q,int width,int height,float minimumArea)
        {
            float area=0,sign=0;for(int i=0;i<4;i++){if(q[i].x< -3||q[i].x>=width+3||q[i].y< -3||q[i].y>=height+3)return false;var a=q[(i+1)%4]-q[i];var b=q[(i+2)%4]-q[(i+1)%4];float cross=a.x*b.y-a.y*b.x;if(i==0)sign=cross;else if(sign*cross<=0)return false;area+=q[i].x*q[(i+1)%4].y-q[i].y*q[(i+1)%4].x;}return Mathf.Abs(area)*.5f>=minimumArea;
        }
    }
}
