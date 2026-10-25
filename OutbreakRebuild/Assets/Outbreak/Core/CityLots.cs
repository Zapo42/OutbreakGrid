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
            if(homesPerBlock<1 || homesPerBlock>4)throw new ArgumentException("Choose one to four homes per residential block.");
            int blocks=columns*rows,shops=blocks>6?2:blocks>2?1:0;
            if((blocks-shops-1)*homesPerBlock+shops+1>ShelterRules.MaxRooms)throw new ArgumentException("This density exceeds the 64-building city limit.");
            BuildingUse[] zones=PlanZones(seed,columns,rows,zoning);
            uint axesState=seed;
            int[] baseXs=Axes(columns,ref axesState,layout==CityStreetLayout.Staggered),ys=Axes(rows,ref axesState,layout==CityStreetLayout.Staggered);
            uint state=seed^0xC17B10C5u;if(state==0)state=1;
            var buildings=new System.Collections.Generic.List<GeneratedBuilding>();
            for(int row=0;row<rows;row++)for(int column=0;column<columns;column++)
            {
                int[] xs=layout==CityStreetLayout.Staggered?RowStreetAxes(baseXs,seed,row):baseXs;
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
                    if(count>=3)
                    {
                        int blockWidth=xs[column+1]-xs[column],blockHeight=ys[row+1]-ys[row];
                        building.Width=Math.Min(building.Width,Math.Max(7,(blockWidth-8)/2));building.Height=Math.Min(building.Height,Math.Max(7,(blockHeight-8)/2));
                        bool east=lot%2==1,south=lot>=2;
                        building.Left=east?xs[column+1]-3-building.Width:xs[column]+4;building.Top=south?ys[row+1]-3-building.Height:ys[row]+4;
                        building.Facing=LotRandom(ref state)%2==0?(south?BuildingFacing.South:BuildingFacing.North):(east?BuildingFacing.East:BuildingFacing.West);
                    }
                    if(zoning==CityZoning.NeighborhoodCenters && use!=BuildingUse.Home)building.Facing=ZoneFacing(zones,index,columns,building.Facing);
                    PaintLot(map,ref building,xs[column]+2,xs[column+1]-1,ys[row]+2,ys[row+1]-1);
                    buildings.Add(building);
                }
            }
            MapDocument document=map.Capture();document.name="Generated building district (seed "+seed+")";document.id=homesPerBlock>=3?"city-compact-lots-"+homesPerBlock:layout==CityStreetLayout.Staggered?(homesPerBlock==2?"city-staggered-lots-2":"city-staggered-lots-1"):zoning==CityZoning.NeighborhoodCenters?"city-zoned-lots-1":layout==CityStreetLayout.Neighborhood?(homesPerBlock==2?"city-neighborhood-lots-2":"city-neighborhood-lots-1"):(homesPerBlock==2?"city-lots-3":"city-lots-2");
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
