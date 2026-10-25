using System;
namespace Outbreak.Core
{
    public static partial class CityGenerator
    {
        // Offset side streets between districts instead of continuing every vertical axis through town.
        internal static int[] RowStreetAxes(int[] axes,uint seed,int row)
        {
            int[] result=(int[])axes.Clone();uint state=seed^0x5B13A29Du;if(state==0)state=1;
            int phase=(int)(StreetRandom(ref state)%3),shift=new[]{-3,0,3}[(row+phase)%3];
            for(int i=1;i<result.Length-1;i++)result[i]+=shift;return result;
        }
        private static WorldMap StaggeredStreets(uint seed,int[] xs,int[] ys)
        {
            int width=xs[xs.Length-1]+5,height=ys[ys.Length-1]+5;
            WorldMap map=new WorldMap(width,height,"Generated staggered streets (seed "+seed+")","city-staggered-streets-1");
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)if(x==0 || y==0 || x==width-1 || y==height-1)map.Set(x,y,Terrain.Wall);
            foreach(int y in ys)for(int x=xs[0];x<=xs[xs.Length-1]+1;x++){PaintDirectedRoad(map,x,y,4);PaintDirectedRoad(map,x,y+1,1);}
            for(int row=0;row<ys.Length-1;row++)foreach(int x in RowStreetAxes(xs,seed,row))for(int y=ys[row];y<=ys[row+1]+1;y++){PaintDirectedRoad(map,x,y,2);PaintDirectedRoad(map,x+1,y,8);}
            int[] dx={1,0,-1,0},dy={0,1,0,-1};
            for(int y=1;y<height-1;y++)for(int x=1;x<width-1;x++)if(TerrainRules.RoadSurface(map.Get(x,y)))
            {
                int mask=map.RoadLaneMaskAt(x,y),valid=0;for(int d=0;d<4;d++)if((mask&(1<<d))!=0 && TerrainRules.RoadSurface(map.Get(x+dx[d],y+dy[d])))valid|=1<<d;
                if(valid==0)throw new ArgumentException("Staggered lane has no exit.");map.SetRoadLaneMask(x,y,valid);
                for(int cy=y-1;cy<=y+1;cy++)for(int cx=x-1;cx<=x+1;cx++)if(map.Get(cx,cy)==Terrain.Grass)map.Set(cx,cy,Terrain.Sidewalk);
            }
            for(int row=0;row<ys.Length-1;row++)foreach(int x in RowStreetAxes(xs,seed,row))
            {
                PaintCrossingPair(map,x,ys[row]+2,true);PaintCrossingPair(map,x,ys[row+1]-1,true);
                foreach(int y in new[]{ys[row],ys[row+1]}){PaintCrossingPair(map,x-1,y,false);PaintCrossingPair(map,x+2,y,false);}
            }
            return map;
        }
        private static void PaintDirectedRoad(WorldMap map,int x,int y,int direction)
        {
            int mask=map.RoadLaneMaskAt(x,y);map.Set(x,y,Terrain.Road);map.SetRoadLaneMask(x,y,mask|direction);
        }
        private static void PaintCrossingPair(WorldMap map,int x,int y,bool acrossVertical)
        {
            int nx=x+(acrossVertical?1:0),ny=y+(acrossVertical?0:1);
            if(!map.Contains(nx,ny))return;
            int first=map.RoadLaneMaskAt(x,y),second=map.RoadLaneMaskAt(nx,ny);
            if(first==0 || second==0 || (first&(first-1))!=0 || (second&(second-1))!=0)return;
            map.Set(x,y,Terrain.Crosswalk);map.Set(nx,ny,Terrain.Crosswalk);
        }
    }
}
