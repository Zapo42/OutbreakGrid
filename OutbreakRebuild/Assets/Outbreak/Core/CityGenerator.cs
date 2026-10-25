using System;
using System.Collections.Generic;
namespace Outbreak.Core
{
    // First generation stage: deterministic connected street blocks; no population or hidden supplies.
    public static partial class CityGenerator
    {
        public static WorldMap Streets(uint seed,int columns=4,int rows=2,CityStreetLayout layout=CityStreetLayout.Grid)
        {
            if(seed==0 || columns<1 || columns>6 || rows<1 || rows>6)throw new ArgumentException("City needs a nonzero seed and 1–6 blocks per axis.");
            if(layout!=CityStreetLayout.Grid && layout!=CityStreetLayout.Neighborhood && layout!=CityStreetLayout.Staggered)throw new ArgumentException("Unknown street layout.");
            uint state=seed;int[] xs=Axes(columns,ref state),ys=Axes(rows,ref state);
            if(layout==CityStreetLayout.Staggered)return StaggeredStreets(seed,xs,ys);
            int width=xs[xs.Length-1]+5,height=ys[ys.Length-1]+5;
            WorldMap map=new WorldMap(width,height,(layout==CityStreetLayout.Neighborhood?"Generated neighborhood streets (seed ":"Generated city streets (seed ")+seed+")",layout==CityStreetLayout.Neighborhood?"city-streets-2":"city-streets-1");
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            {
                if(x==0 || y==0 || x==width-1 || y==height-1){map.Set(x,y,Terrain.Wall);continue;}
                bool road=false,sidewalk=false;
                foreach(int axis in xs){road|=x>=axis && x<=axis+1;sidewalk|=x==axis-1 || x==axis+2;}
                foreach(int axis in ys){road|=y>=axis && y<=axis+1;sidewalk|=y==axis-1 || y==axis+2;}
                if(road)map.Set(x,y,Terrain.Road);else if(sidewalk)map.Set(x,y,Terrain.Sidewalk);
            }
            if(layout==CityStreetLayout.Neighborhood)VaryStreets(map,xs,ys,seed);
            PaintStreetCrosswalks(map,xs,ys);
            PaintRoadLanes(map,xs,ys);
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
