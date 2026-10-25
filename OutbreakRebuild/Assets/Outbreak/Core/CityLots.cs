using System;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public enum BuildingFacing { North, East, South, West }
    public struct GeneratedBuilding
    {
        public int Left,Top,Width,Height,DoorX,DoorY,PathEndX,PathEndY,BlockColumn,BlockRow;
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
        public static GeneratedDistrict BuildingLots(uint seed,int columns=4,int rows=2,int homesPerBlock=1,CityStreetLayout layout=CityStreetLayout.Grid,CityZoning zoning=CityZoning.Fixed)
        {
            WorldMap map=Streets(seed,columns,rows,layout);
            if(homesPerBlock<1 || homesPerBlock>2)throw new ArgumentException("Choose one or two homes per residential block.");
            int blocks=columns*rows,shops=blocks>6?2:blocks>2?1:0;
            if((blocks-shops-1)*homesPerBlock+shops+1>ShelterRules.MaxRooms)throw new ArgumentException("This density exceeds the 64-building city limit.");
            BuildingUse[] zones=PlanZones(seed,columns,rows,zoning);
            uint axesState=seed;
            int[] xs=Axes(columns,ref axesState),ys=Axes(rows,ref axesState);
            uint state=seed^0xC17B10C5u;if(state==0)state=1;
            var buildings=new System.Collections.Generic.List<GeneratedBuilding>();
            for(int row=0;row<rows;row++)for(int column=0;column<columns;column++)
            {
                int index=row*columns+column;
                BuildingUse use=zones[index];
                int count=use==BuildingUse.Home?homesPerBlock:1;
                for(int lot=0;lot<count;lot++)
                {
                    int minimum=use==BuildingUse.DistributionCenter?12:use==BuildingUse.Store?10:7;
                    int variation=use==BuildingUse.Home?(count==2?3:4):3;
                    GeneratedBuilding building=new GeneratedBuilding{BlockColumn=column,BlockRow=row,Left=xs[column]+5+(int)(LotRandom(ref state)%3),Top=ys[row]+5+(int)(LotRandom(ref state)%3),Width=minimum+(int)(LotRandom(ref state)%variation),Height=minimum+(int)(LotRandom(ref state)%variation),Use=use,Facing=(BuildingFacing)(LotRandom(ref state)%4)};
                    if(count==2)
                    {
                        building.Left=lot==0?xs[column]+4+(int)(LotRandom(ref state)%2):xs[column+1]-11+(int)(LotRandom(ref state)%2);
                        building.Top=lot==0?ys[row]+4+(int)(LotRandom(ref state)%2):ys[row+1]-11+(int)(LotRandom(ref state)%2);
                        building.Facing=lot==0?(LotRandom(ref state)%2==0?BuildingFacing.North:BuildingFacing.West):(LotRandom(ref state)%2==0?BuildingFacing.South:BuildingFacing.East);
                    }
                    if(zoning==CityZoning.NeighborhoodCenters && use!=BuildingUse.Home)building.Facing=ZoneFacing(zones,index,columns,building.Facing);
                    PaintLot(map,ref building,xs[column]+2,xs[column+1]-1,ys[row]+2,ys[row+1]-1);
                    buildings.Add(building);
                }
            }
            MapDocument document=map.Capture();document.name="Generated building district (seed "+seed+")";document.id=zoning==CityZoning.NeighborhoodCenters?"city-zoned-lots-1":layout==CityStreetLayout.Neighborhood?(homesPerBlock==2?"city-neighborhood-lots-2":"city-neighborhood-lots-1"):(homesPerBlock==2?"city-lots-3":"city-lots-2");
            return new GeneratedDistrict{Map=WorldMap.Restore(document),Buildings=buildings.ToArray()};
        }
        private static void PaintLot(WorldMap map,ref GeneratedBuilding building,int west,int east,int north,int south)
        {
            building.DoorX=building.Left+building.Width/2;building.DoorY=building.Top+building.Height/2;
            if(building.Facing==BuildingFacing.North){building.DoorY=building.Top;building.PathEndX=building.DoorX;building.PathEndY=north;}
            else if(building.Facing==BuildingFacing.South){building.DoorY=building.Top+building.Height-1;building.PathEndX=building.DoorX;building.PathEndY=south;}
            else if(building.Facing==BuildingFacing.West){building.DoorX=building.Left;building.PathEndX=west;building.PathEndY=building.DoorY;}
            else{building.DoorX=building.Left+building.Width-1;building.PathEndX=east;building.PathEndY=building.DoorY;}
            for(int y=building.Top;y<building.Top+building.Height;y++)for(int x=building.Left;x<building.Left+building.Width;x++)
                map.Set(x,y,x==building.Left || x==building.Left+building.Width-1 || y==building.Top || y==building.Top+building.Height-1?Terrain.Wall:Terrain.Floor);
            map.Set(building.DoorX,building.DoorY,Terrain.DoorClosed);
            int pathX=building.DoorX,pathY=building.DoorY,stepX=Math.Sign(building.PathEndX-pathX),stepY=Math.Sign(building.PathEndY-pathY);
            while(pathX!=building.PathEndX || pathY!=building.PathEndY){pathX+=stepX;pathY+=stepY;map.Set(pathX,pathY,Terrain.Sidewalk);}
        }
        private static uint LotRandom(ref uint state){state^=state<<13;state^=state>>17;state^=state<<5;return state;}
    }
}
