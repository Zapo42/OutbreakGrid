using System;
using System.Collections.Generic;
namespace Outbreak.Core
{
    // First generation stage: deterministic connected street blocks; no population or hidden supplies.
    public static class CityGenerator
    {
        public static WorldMap Streets(uint seed,int columns=4,int rows=2)
        {
            if(seed==0 || columns<1 || columns>6 || rows<1 || rows>6)throw new ArgumentException("City needs a nonzero seed and 1–6 blocks per axis.");
            uint state=seed;int[] xs=Axes(columns,ref state),ys=Axes(rows,ref state);
            int width=xs[xs.Length-1]+5,height=ys[ys.Length-1]+5;
            WorldMap map=new WorldMap(width,height,"Generated city streets (seed "+seed+")","city-streets-1");
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                if(x==0 || y==0 || x==width-1 || y==height-1){map.Set(x,y,Terrain.Wall);continue;}
                bool road=false,sidewalk=false;
                foreach(int axis in xs){road|=x>=axis && x<=axis+1;sidewalk|=x==axis-1 || x==axis+2;}
                foreach(int axis in ys){road|=y>=axis && y<=axis+1;sidewalk|=y==axis-1 || y==axis+2;}
                if(road)map.Set(x,y,Terrain.Road);else if(sidewalk)map.Set(x,y,Terrain.Sidewalk);
            }
            // Crosswalks extend the perimeter sidewalks through both lanes at each junction.
            foreach(int x in xs)foreach(int y in ys)for(int lane=0;lane<2;lane++)
            {
                map.Set(x+lane,y-1,Terrain.Crosswalk);map.Set(x+lane,y+2,Terrain.Crosswalk);
                map.Set(x-1,y+lane,Terrain.Crosswalk);map.Set(x+2,y+lane,Terrain.Crosswalk);
            }
            return map;
        }
        private static int[] Axes(int blocks,ref uint state)
        {
            var result=new List<int>{3};for(int i=0;i<blocks;i++)
            {
                state^=state<<13;state^=state>>17;state^=state<<5;
                result.Add(result[result.Count-1]+26+(int)(state%5));
            }
            return result.ToArray();
        }
    }
}
