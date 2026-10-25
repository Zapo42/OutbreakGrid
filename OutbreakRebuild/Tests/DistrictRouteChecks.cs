using System;
using System.Collections.Generic;
using System.Reflection;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckDistrictRouteCases()
    {
        Check("Expanded city recognizes 247 buildings with distinct homes finite supplies and exact saves",()=>{
            World world=CitySetup.Create(214,8,8,4,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters);var info=CitySetup.Describe(214,8,8,4,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters);
            Require(world.DistrictRoutesEnabled && world.Count==244 && world.ShelterRoomCount==247 && !world.ShelterScanLimited && world.FoodSupplies().RemainingMeals==780 && info.Buildings==247,"Expanded city truncated rooms, residents or stock.");var houses=new HashSet<int>();for(int i=0;i<world.Count;i++)Require(houses.Add(world.ActorAt(i).assignedHouseId),"Expanded residents share an unintended house assignment.");Require(State(world)==State(Documents.ParseSave(State(world))),"Expanded save did not restore exactly.");Reject(()=>CitySetup.Create(214,10,10,4));Require(CitySetup.Describe(214,10,10,2).Buildings==197,"Supported ten-block city was rejected.");
        });
        Check("District routing crosses old radius on connected sidewalks and compresses the network",()=>{
            World world=new World(CityGenerator.Streets(214,8,8,CityStreetLayout.Staggered),214,1000,true,hunger:true);ActorState actor=new ActorState{species=Species.Human,x=3,y=4,assignedHouseId=1};int goalX=world.Width-5,goalY=world.Height-8,length;
            Require(Math.Max(goalX-actor.x,goalY-actor.y)>CityRouteRules.Radius && CityRoute(world,actor,goalX,goalY,out length)>=0 && length>256,"District planner did not bridge the former radius limit.");
            var cells=(HashSet<int>)typeof(World).GetField("districtSurface",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(world);var portals=(HashSet<int>)typeof(World).GetField("districtPortals",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(world);Require(portals.Count<cells.Count/2,"Walkway corridors were not compressed.");
            int[] sx={0,1,1,1,0,-1,-1,-1,0},sy={1,1,0,-1,-1,-1,0,1,0};int moves=0;for(;moves<1600 && (actor.x!=goalX || actor.y!=goalY);moves++){int d=CityRoute(world,actor,goalX,goalY,out length);Require(d>=0 && d<8,"District trip stalled or bounced before arriving.");actor.x+=sx[d];actor.y+=sy[d];Require(TerrainRules.Walkable(world.TerrainAt(actor.x,actor.y)),"District route crossed solid terrain.");}Require(actor.x==goalX && actor.y==goalY,"District trip failed to reach its real destination.");
            Require(CityRoute(world,actor,actor.x+DistrictRouteRules.Radius+1,actor.y,out length)<0,"District route escaped its new query bound.");
        });
        Check("District local movement rechecks live furniture obstacles",()=>{
            World world=new World(CityGenerator.Streets(214,8,8,CityStreetLayout.Staggered),214,1000,true,hunger:true);ActorState actor=new ActorState{species=Species.Human,x=3,y=4,assignedHouseId=1};int length,d=CityRoute(world,actor,world.Width-5,world.Height-8,out length);Require(d>=0,"Initial district query failed.");int[] sx={0,1,1,1,0,-1,-1,-1,0},sy={1,1,0,-1,-1,-1,0,1,0};int x=actor.x+sx[d],y=actor.y+sy[d];Require(world.TryPlaceStorageBox(x,y),"Could not obstruct first local step.");int detour=CityRoute(world,actor,world.Width-5,world.Height-8,out length);Require(detour<0 || detour!=d,"Cached district road plan ignored live furniture.");Require(State(world)==State(Documents.ParseSave(State(world))),"Derived route graph leaked into saved state.");
        });
        Check("Disconnected district walkways do not produce imaginary long-distance links",()=>{
            WorldMap map=new WorldMap(420,12);for(int x=1;x<200;x++)map.Set(x,4,Terrain.Sidewalk);for(int x=220;x<419;x++)map.Set(x,4,Terrain.Sidewalk);for(int y=0;y<12;y++)map.Set(210,y,Terrain.Wall);World world=new World(map,214,1000,true,hunger:true);ActorState actor=new ActorState{species=Species.Human,x=3,y=4,assignedHouseId=1};int length;Require(CityRoute(world,actor,390,4,out length)<0,"District planner connected separate walkway components through a wall.");
            World roads=new World(CityGenerator.Streets(214,8,8,CityStreetLayout.Staggered),214,1000,true,hunger:true);int id;Require(roads.TryPlaceVehicle(3,4,out id) && roads.TrySetVehicleDestination(0,roads.Width-5,roads.Height-8),"Expanded road destination retained the old vehicle radius cap.");
        });
        Check("Old rules retain their routing radius and room catalog until explicit upgrade",()=>{
            World world=new World(CityGenerator.Streets(214,8,8,CityStreetLayout.Staggered),214,1000,true,hunger:true);var saved=world.Capture();saved.rulesVersion=EncounterRules.JunctionTrafficVersion;World old=World.Restore(saved);ActorState actor=new ActorState{species=Species.Human,x=3,y=4,assignedHouseId=1};int length;Require(!old.DistrictRoutesEnabled && CityRoute(old,actor,old.Width-5,old.Height-8,out length)<0,"Old run silently acquired expanded routes.");
            World expanded=CitySetup.Create(214,8,8,4,CityStreetLayout.Staggered);saved=expanded.Capture();saved.rulesVersion=EncounterRules.JunctionTrafficVersion;Reject(()=>World.Restore(saved));
        });
        Check("Expanded district couriers physically credit distant store deliveries",()=>{
            World full=CitySetup.Create(214,8,8,4,CityStreetLayout.Staggered,CityZoning.Fixed);var saved=full.Capture();saved.actors=new[]{saved.actors[0],saved.actors[1],saved.actors[2]};World world=World.Restore(saved);int startX=world.ActorAt(0).x,startY=world.ActorAt(0).y;bool claimed=false,credited=false,traveled=false;
            for(int t=0;t<3300;t++){world.Step();claimed|=world.ActorAt(0).courierOrderId!=0;traveled|=Math.Abs(world.ActorAt(0).x-startX)>64 || Math.Abs(world.ActorAt(0).y-startY)>64;if(t%100==0)foreach(var order in world.PendingSupplyOrders())credited|=order.deliveredMeals>0;}
            Require(claimed && credited && traveled,"District courier failed to carry and credit an actual distant delivery.");Require(State(world)==State(Documents.ParseSave(State(world))),"Distant delivery state failed exact saving.");
        });
        Check("Derived district plans rebuild identically after a save and do not consume RNG",()=>{
            World world=CitySetup.Create(214,8,8,4,CityStreetLayout.Staggered);ActorState actor=world.ActorAt(0);actor.x=3;actor.y=4;int length,first=CityRoute(world,actor,world.Width-5,world.Height-8,out length);string state=State(world);World copy=Documents.ParseSave(state);int copyLength;Require(CityRoute(copy,actor,copy.Width-5,copy.Height-8,out copyLength)==first && copyLength==length && State(world)==state && State(copy)==state,"District plans changed decisions or authoritative state across save/load.");
            Run(world,50);copy=Documents.ParseSave(State(world));Run(world,80);Run(copy,80);Require(State(world)==State(copy),"Expanded live city diverged after rebuilding derived routes.");
        });
    }
}
