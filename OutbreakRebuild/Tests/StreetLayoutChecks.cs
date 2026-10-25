using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckSurfaceNetwork(WorldMap map,bool roadway)
    {
        var seen=new HashSet<int>();var queue=new Queue<int>();int total=0;
        for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)
        {
            Terrain tile=map.Get(x,y);bool matches=roadway?TerrainRules.RoadSurface(tile):tile==Terrain.Sidewalk || tile==Terrain.Crosswalk;
            if(!matches)continue;total++;if(seen.Count==0){int key=y*map.Width+x;seen.Add(key);queue.Enqueue(key);}
        }
        while(queue.Count>0)
        {
            int cell=queue.Dequeue(),x=cell%map.Width,y=cell/map.Width;
            foreach(var offset in new[]{(1,0),(-1,0),(0,1),(0,-1)})
            {
                int nx=x+offset.Item1,ny=y+offset.Item2;if(!map.Contains(nx,ny))continue;Terrain tile=map.Get(nx,ny);bool matches=roadway?TerrainRules.RoadSurface(tile):tile==Terrain.Sidewalk || tile==Terrain.Crosswalk;int key=ny*map.Width+nx;
                if(matches && seen.Add(key))queue.Enqueue(key);
            }
        }
        Require(total>0 && total==seen.Count,roadway?"Neighborhood isolated a roadway segment.":"Neighborhood disconnected its pedestrian network.");
    }
    private static bool IsStreetCap(WorldMap map,int x,int y,int dx,int dy)
    {
        int px=dy==0?0:1,py=dy==0?1:0;
        foreach(var point in new[]{(x,y),(x+px,y+py),(x-dx,y-dy),(x-dx+px,y-dy+py)})if(!map.Contains(point.Item1,point.Item2) || !TerrainRules.RoadSurface(map.Get(point.Item1,point.Item2)))return false;
        foreach(var point in new[]{(x+dx,y+dy),(x+dx+px,y+dy+py),(x-px,y-py),(x+2*px,y+2*py)})if(!map.Contains(point.Item1,point.Item2) || map.Get(point.Item1,point.Item2)!=Terrain.Sidewalk)return false;
        return true;
    }
    private static void CheckStreetLayoutCases()
    {
        Check("Neighborhood streets repeat across seeds with connected roads pedestrian links and fewer grid crossings",()=>{
            foreach(uint seed in new[]{214u,999u,1u,uint.MaxValue})foreach(var size in new[]{(2,2),(4,2),(6,5)})
            {
                WorldMap map=CityGenerator.Streets(seed,size.Item1,size.Item2,CityStreetLayout.Neighborhood),grid=CityGenerator.Streets(seed,size.Item1,size.Item2);int replaced=0,crossings=0,oldCrossings=0,tJunctions=0,caps=0;
                CheckSurfaceNetwork(map,true);CheckSurfaceNetwork(map,false);
                for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)
                {
                    Terrain tile=map.Get(x,y),original=grid.Get(x,y);
                    if(IsStreetCap(map,x,y,1,0) || IsStreetCap(map,x,y,-1,0) || IsStreetCap(map,x,y,0,1) || IsStreetCap(map,x,y,0,-1))caps++;if(TerrainRules.RoadSurface(original) && tile==Terrain.Sidewalk)replaced++;
                    if(tile==Terrain.Crosswalk)crossings++;if(original==Terrain.Crosswalk)oldCrossings++;
                    if(x==0 || y==0 || x==map.Width-1 || y==map.Height-1)Require(tile==Terrain.Wall,"Neighborhood damaged map border.");
                    if(x>1 && y>1 && x<map.Width-3 && y<map.Height-3 && grid.Get(x-1,y)==Terrain.Crosswalk && grid.Get(x,y-1)==Terrain.Crosswalk && grid.Get(x+2,y)==Terrain.Crosswalk && grid.Get(x,y+2)==Terrain.Crosswalk)
                    {
                        int arms=(TerrainRules.RoadSurface(map.Get(x-1,y))?1:0)+(TerrainRules.RoadSurface(map.Get(x,y-1))?1:0)+(TerrainRules.RoadSurface(map.Get(x+2,y))?1:0)+(TerrainRules.RoadSurface(map.Get(x,y+2))?1:0);if(arms==3)tJunctions++;
                    }
                }
                Require(replaced>0 && crossings>0 && crossings<oldCrossings && tJunctions>0 && caps>0,"Neighborhood did not break the complete grid into genuine T-junctions/links.");
                World world=new World(map,seed);Require(State(world)==State(new World(CityGenerator.Streets(seed,size.Item1,size.Item2,CityStreetLayout.Neighborhood),seed)) && State(world)==State(Documents.ParseSave(State(world))),"Street variation was not repeatable or save-stable.");
            }
        });
        Check("Neighborhood layouts preserve furnished entrances and support both residential densities",()=>{
            foreach(int density in new[]{1,2})foreach(uint seed in new[]{214u,999u})
            {
                GeneratedDistrict district=CityGenerator.BuildingLots(seed,4,2,density,CityStreetLayout.Neighborhood);World world=CitySetup.Create(seed,4,2,density,CityStreetLayout.Neighborhood);WorldMap streets=CityGenerator.Streets(seed,4,2,CityStreetLayout.Neighborhood);
                CitySetupSummary summary=CitySetup.Describe(seed,4,2,density,CityStreetLayout.Neighborhood);Require(world.Count==summary.Humans && world.ShelterRoomCount==summary.Buildings && world.FoodSupplies().RemainingMeals==summary.Meals,"Street style changed stock or housing counts.");
                foreach(GeneratedBuilding building in district.Buildings)
                {
                    ActorState actor=world.ActorAt(0);actor.x=building.InsideX;actor.y=building.InsideY;actor.assignedHouseId=world.HouseAt(actor.x,actor.y);int length;
                    Require(CityRoute(world,actor,building.PathEndX,building.PathEndY,out length)>=0,"Neighborhood cut off furnished building access.");
                }
                for(int y=0;y<world.Height;y++)for(int x=0;x<world.Width;x++)if(streets.Get(x,y)!=Terrain.Grass)Require(district.Map.Get(x,y)==streets.Get(x,y),"Building construction damaged varied streets/crosswalks.");
                Run(world,50);Require(world.PendingSupplyOrders().Length==2,"Varied streets lost store phone requests.");World copy=Documents.ParseSave(State(world));Run(world,80);Run(copy,80);Require(State(world)==State(copy),"Neighborhood job continuation diverged.");
            }
        });
        Check("Crosswalks remain shared road surfaces and street layout validation is bounded",()=>{
            Require(TerrainRules.RoadSurface(Terrain.Road) && TerrainRules.RoadSurface(Terrain.Crosswalk) && TerrainRules.Walkable(Terrain.Crosswalk),"Crosswalk lost pedestrian/shared roadway classification.");
            foreach(Terrain tile in new[]{Terrain.Sidewalk,Terrain.Grass,Terrain.Floor,Terrain.Wall,Terrain.Water})Require(!TerrainRules.RoadSurface(tile),"Non-road ground became a future vehicle surface.");
            Reject(()=>CityGenerator.Streets(214,4,2,(CityStreetLayout)99));Reject(()=>CitySetup.Create(214,4,2,1,(CityStreetLayout)99));
        });
    }
}
