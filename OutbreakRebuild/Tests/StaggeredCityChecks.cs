using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckStaggeredCityCases()
    {
        Check("Staggered streets offset side roads while roads sidewalks and directed lanes remain connected",()=>{
            foreach(uint seed in new[]{1u,214u,999u,uint.MaxValue})foreach(var size in new[]{(2,2),(4,2),(6,5)}){
                WorldMap map=CityGenerator.Streets(seed,size.Item1,size.Item2,CityStreetLayout.Staggered);CheckSurfaceNetwork(map,true);CheckSurfaceNetwork(map,false);
                int start=-1,total=0,through=0;for(int x=5;x<map.Width-7;x++){bool continuous=true;for(int y=3;y<map.Height-4;y++)continuous&=TerrainRules.RoadSurface(map.Get(x,y));if(continuous)through++;}Require(through==0,"Interior vertical streets still form an uninterrupted grid.");
                for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)if(TerrainRules.RoadSurface(map.Get(x,y))){Require(map.RoadLaneMaskAt(x,y)>0,"Staggered roadway lacks traffic directions.");start=y*map.Width+x;total++;}
                for(int reverse=0;reverse<2;reverse++){
                    var seen=new HashSet<int>();var queue=new Queue<int>();queue.Enqueue(start);seen.Add(start);int[] dx={1,0,-1,0},dy={0,1,0,-1};
                    while(queue.Count>0){int key=queue.Dequeue(),x=key%map.Width,y=key/map.Width;for(int d=0;d<4;d++){int nx=x+dx[d],ny=y+dy[d];if(!map.Contains(nx,ny) || !TerrainRules.RoadSurface(map.Get(nx,ny)))continue;int mask=reverse==0?map.RoadLaneMaskAt(x,y):map.RoadLaneMaskAt(nx,ny);if((mask&(1<<(reverse==0?d:(d+2)%4)))!=0 && seen.Add(ny*map.Width+nx))queue.Enqueue(ny*map.Width+nx);}}Require(seen.Count==total,"Staggered lanes have an unreachable road segment.");
                }
                World world=new World(map,seed);Require(State(world)==State(new World(CityGenerator.Streets(seed,size.Item1,size.Item2,CityStreetLayout.Staggered),seed)) && State(world)==State(Documents.ParseSave(State(world))),"Staggered generation or saving was not deterministic.");
            }
        });
        Check("Reclaimed roads do not become oversized solid sidewalk slabs",()=>{
            foreach(uint seed in new[]{1u,214u,999u})foreach(CityStreetLayout layout in new[]{CityStreetLayout.Neighborhood,CityStreetLayout.Staggered}){
                WorldMap map=CityGenerator.Streets(seed,4,2,layout);for(int y=0;y<map.Height-3;y++)for(int x=0;x<map.Width-3;x++){bool solid=true;for(int cy=y;cy<y+4;cy++)for(int cx=x;cx<x+4;cx++)solid&=map.Get(cx,cy)==Terrain.Sidewalk;Require(!solid,"Street generation left a four-cell-thick sidewalk slab.");}
            }
        });
        Check("Vehicles clear staggered T-junctions and save their turns exactly",()=>{
            WorldMap map=CityGenerator.Streets(214,4,2,CityStreetLayout.Staggered);World world=new World(map,214,1000,true,hunger:true);int id;Require(world.TryPlaceVehicle(3,8,out id) && world.TrySetVehicleDestination(0,map.Width-5,map.Height-8),"Staggered vehicle trip could not be assigned.");World copy=null;bool arrived=false;
            for(int t=0;t<1600;t++){world.Step();if(t==20)copy=Documents.ParseSave(State(world));else if(copy!=null){copy.Step();Require(State(world)==State(copy),"Staggered driving diverged after saving.");}if(world.VehicleStatus(0)=="Arrived"){arrived=true;break;}}Require(arrived,"Staggered T-junction held a car indefinitely.");
        });
        Check("Staggered lots preserve streets food housing and reachable entrances",()=>{
            foreach(int density in new[]{1,2})foreach(uint seed in new[]{214u,999u}){
                GeneratedDistrict district=CityGenerator.BuildingLots(seed,4,2,density,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters);World world=CitySetup.Create(seed,4,2,density,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters);WorldMap streets=CityGenerator.Streets(seed,4,2,CityStreetLayout.Staggered);var summary=CitySetup.Describe(seed,4,2,density,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters);
                Require(world.Count==summary.Humans && world.ShelterRoomCount==summary.Buildings && world.FoodSupplies().RemainingMeals==summary.Meals,"Staggering changed housing or stock.");
                foreach(var b in district.Buildings){ActorState actor=world.ActorAt(0);actor.x=b.InsideX;actor.y=b.InsideY;actor.assignedHouseId=world.HouseAt(actor.x,actor.y);int distance;Require(CityRoute(world,actor,b.PathEndX,b.PathEndY,out distance)>=0,"An offset street made a doorway inaccessible.");}
                for(int y=0;y<world.Height;y++)for(int x=0;x<world.Width;x++)if(streets.Get(x,y)!=Terrain.Grass)Require(district.Map.Get(x,y)==streets.Get(x,y),"A lot overwrote staggered street terrain.");
                Run(world,50);Require(world.PendingSupplyOrders().Length==2,"Staggered clerks failed to request stock.");World copy=Documents.ParseSave(State(world));Run(world,50);Run(copy,50);Require(State(world)==State(copy),"Staggered city continuation diverged.");
            }
        });
    }
}
