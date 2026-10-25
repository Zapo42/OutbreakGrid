using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public enum BuildingFacing { North, East, South, West }
    public struct GeneratedBuilding
    {
        public int Left,Top,Width,Height,DoorX,DoorY,PathEndX,PathEndY;
        public BuildingFacing Facing;
        public BuildingUse Use;
        public int InsideX { get { return DoorX-Math.Sign(PathEndX-DoorX); } }
        public int InsideY { get { return DoorY-Math.Sign(PathEndY-DoorY); } }
        public int BoxX { get { return Left+Width-2; } }
        public int BoxY { get { return Top+1; } }
    }
    public sealed class GeneratedDistrict
    {
        public WorldMap Map;
        public GeneratedBuilding[] Buildings;
    }
    public static partial class CityGenerator
    {
        // Geometry only. Scenario setup assigns work, residents and finite inventories separately.
        public static GeneratedDistrict BuildingLots(uint seed,int columns=4,int rows=2)
        {
            WorldMap map=Streets(seed,columns,rows);uint axesState=seed;
            int[] xs=Axes(columns,ref axesState),ys=Axes(rows,ref axesState);
            uint state=seed^0xC17B10C5u;if(state==0)state=1;
            var buildings=new GeneratedBuilding[columns*rows];
            for(int row=0;row<rows;row++)for(int column=0;column<columns;column++)
            {
                int index=row*columns+column;
                BuildingUse use=index==0?BuildingUse.DistributionCenter:index==2 || index==6?BuildingUse.Store:BuildingUse.Home;
                int minimum=use==BuildingUse.DistributionCenter?12:use==BuildingUse.Store?10:7;
                int variation=use==BuildingUse.Home?4:3;
                GeneratedBuilding building=new GeneratedBuilding{Left=xs[column]+5+(int)(LotRandom(ref state)%3),Top=ys[row]+5+(int)(LotRandom(ref state)%3),Width=minimum+(int)(LotRandom(ref state)%variation),Height=minimum+(int)(LotRandom(ref state)%variation),Use=use,Facing=(BuildingFacing)(LotRandom(ref state)%4)};
                building.DoorX=building.Left+building.Width/2;building.DoorY=building.Top+building.Height/2;
                if(building.Facing==BuildingFacing.North){building.DoorY=building.Top;building.PathEndX=building.DoorX;building.PathEndY=ys[row]+2;}
                else if(building.Facing==BuildingFacing.South){building.DoorY=building.Top+building.Height-1;building.PathEndX=building.DoorX;building.PathEndY=ys[row+1]-1;}
                else if(building.Facing==BuildingFacing.West){building.DoorX=building.Left;building.PathEndX=xs[column]+2;building.PathEndY=building.DoorY;}
                else{building.DoorX=building.Left+building.Width-1;building.PathEndX=xs[column+1]-1;building.PathEndY=building.DoorY;}
                for(int y=building.Top;y<building.Top+building.Height;y++)for(int x=building.Left;x<building.Left+building.Width;x++)
                    map.Set(x,y,x==building.Left || x==building.Left+building.Width-1 || y==building.Top || y==building.Top+building.Height-1?Terrain.Wall:Terrain.Floor);
                map.Set(building.DoorX,building.DoorY,Terrain.DoorClosed);
                int pathX=building.DoorX,pathY=building.DoorY,stepX=Math.Sign(building.PathEndX-pathX),stepY=Math.Sign(building.PathEndY-pathY);
                while(pathX!=building.PathEndX || pathY!=building.PathEndY){pathX+=stepX;pathY+=stepY;map.Set(pathX,pathY,Terrain.Sidewalk);}
                buildings[index]=building;
            }
            MapDocument document=map.Capture();document.name="Generated building district (seed "+seed+")";document.id="city-lots-2";
            return new GeneratedDistrict{Map=WorldMap.Restore(document),Buildings=buildings};
        }
        private static uint LotRandom(ref uint state){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
    }
}
