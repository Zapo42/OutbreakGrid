using System;
using System.Collections.Generic;
namespace Outbreak.Core
{
    public enum CityStreetLayout { Grid, Neighborhood, Staggered }
    public static partial class CityGenerator
    {
        private sealed class StreetEdge
        {
            public int A,B,X,Y,Length;
            public bool Vertical,Perimeter,Keep;
        }
        private static int StreetRoot(int[] parents,int node)
        {
            while(parents[node]!=node){parents[node]=parents[parents[node]];node=parents[node];}return node;
        }
        private static bool JoinStreet(int[] parents,int a,int b)
        {
            int first=StreetRoot(parents,a),second=StreetRoot(parents,b);if(first==second)return false;
            parents[Math.Max(first,second)]=Math.Min(first,second);return true;
        }
        private static uint StreetRandom(ref uint state){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
        private static void VaryStreets(WorldMap map,int[] xs,int[] ys,uint seed)
        {
            int columns=xs.Length-1,rows=ys.Length-1,stride=columns+1;
            var edges=new List<StreetEdge>();
            for(int row=0;row<=rows;row++)for(int col=0;col<columns;col++)edges.Add(new StreetEdge{A=row*stride+col,B=row*stride+col+1,X=xs[col],Y=ys[row],Length=xs[col+1]-xs[col],Perimeter=row==0 || row==rows});
            for(int col=0;col<=columns;col++)for(int row=0;row<rows;row++)edges.Add(new StreetEdge{A=row*stride+col,B=(row+1)*stride+col,X=xs[col],Y=ys[row],Length=ys[row+1]-ys[row],Vertical=true,Perimeter=col==0 || col==columns});
            int[] parents=new int[xs.Length*ys.Length];for(int i=0;i<parents.Length;i++)parents[i]=i;
            foreach(StreetEdge edge in edges)if(edge.Perimeter){edge.Keep=true;JoinStreet(parents,edge.A,edge.B);}
            uint state=seed^0x7A9D238Bu;if(state==0)state=1;
            for(int i=edges.Count-1;i>0;i--){int other=(int)(StreetRandom(ref state)%(uint)(i+1));StreetEdge swap=edges[i];edges[i]=edges[other];edges[other]=swap;}
            // Keep a seeded connecting backbone before shortening any optional streets.
            foreach(StreetEdge edge in edges)if(!edge.Keep && JoinStreet(parents,edge.A,edge.B))edge.Keep=true;
            StreetEdge link=null,deadEnd=null;
            foreach(StreetEdge edge in edges)if(!edge.Keep)
            {
                int ax=edge.A%stride,ay=edge.A/stride,bx=edge.B%stride,by=edge.B/stride;
                if(ax==0 || ax==columns || ay==0 || ay==rows || bx==0 || bx==columns || by==0 || by==rows){link=edge;break;}
            }
            foreach(StreetEdge edge in edges)if(!edge.Keep && edge!=link){deadEnd=edge;break;}
            foreach(StreetEdge edge in edges)
            {
                if(edge.Keep)continue;
                int mode=edge==link?2:edge==deadEnd?1:(int)(StreetRandom(ref state)%3);
                if(mode==0)continue;
                int length=edge.Length-2,stub=mode==1?length/3+(int)(StreetRandom(ref state)%(uint)(length/3)):0;
                bool reverse=(StreetRandom(ref state)&1)!=0;
                for(int i=0;i<length;i++)
                {
                    bool road=reverse?i>=length-stub:i<stub;
                    int x=edge.X+(edge.Vertical?0:i+2),y=edge.Y+(edge.Vertical?i+2:0);
                    bool cap=i==0 || i==length-1 || (stub>0 && (reverse?i==length-stub-1:i==stub));
                    Terrain tile=road?Terrain.Road:cap?Terrain.Sidewalk:Terrain.Grass;
                    map.Set(x,y,tile);
                    map.Set(x+(edge.Vertical?1:0),y+(edge.Vertical?0:1),tile);
                }
            }
        }
        private static void PaintStreetCrosswalks(WorldMap map,int[] xs,int[] ys)
        {
            foreach(int x in xs)foreach(int y in ys)for(int lane=0;lane<2;lane++)
            {
                if(map.Get(x,y-2)==Terrain.Road && map.Get(x+1,y-2)==Terrain.Road)map.Set(x+lane,y-1,Terrain.Crosswalk);
                if(map.Get(x,y+3)==Terrain.Road && map.Get(x+1,y+3)==Terrain.Road)map.Set(x+lane,y+2,Terrain.Crosswalk);
                if(map.Get(x-2,y)==Terrain.Road && map.Get(x-2,y+1)==Terrain.Road)map.Set(x-1,y+lane,Terrain.Crosswalk);
                if(map.Get(x+3,y)==Terrain.Road && map.Get(x+3,y+1)==Terrain.Road)map.Set(x+2,y+lane,Terrain.Crosswalk);
            }
        }
    }
}
