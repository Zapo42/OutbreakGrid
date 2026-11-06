using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private delegate int LongRouteProbe(ActorState actor,int x,int y,bool walkOnly,out int distance,int radius);
    private static LongRouteProbe LongRouteFor(World world){return (LongRouteProbe)Delegate.CreateDelegate(typeof(LongRouteProbe),world,typeof(World).GetMethod("LongRoute",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic));}
    private static int RouteVisited(World world){return ((Dictionary<int,int>)typeof(World).GetField("cityBest",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(world)).Count;}
    private static void CheckBlockedLongRouteCases()
    {
        Check("Full or zombie route targets reject without a frontier search while the current cell remains valid",()=>{
            World w=new World(new WorldMap(20,20),214,encounter:true,hunger:true);int id,length;for(int i=0;i<4;i++)Require(w.TrySpawn(Species.Human,10,5,out id),"Could not fill target.");var a=new ActorState{species=Species.Human,id=999,x=1,y=5};var query=LongRouteFor(w);Require(query(a,10,5,false,out length,256)<0 && RouteVisited(w)==0,"A full goal still searched the map.");var here=w.ActorAt(0);Require(query(here,10,5,false,out length,256)==8 && length==0,"A full current cell prevented standing still.");
            var saved=w.Capture();saved.actors[0].x=2;saved.actors[0].y=2;w=World.Restore(saved);Require(LongRouteFor(w)(a,10,5,false,out length,256)>=0 && length==9,"Freeing one place did not restore the real route.");
            w=new World(new WorldMap(20,20),214,encounter:true,hunger:true);Require(w.TrySpawn(Species.Zombie,10,5,out id),"Zombie target failed.");Require(LongRouteFor(w)(a,10,5,false,out length,256)<0 && RouteVisited(w)==0,"Zombie goal still searched the map.");
        });
        Check("A sealed room entrance rejects remote floor searches and reopening one entry restores routing",()=>{
            WorldMap map=new WorldMap(20,20);for(int y=3;y<=12;y++)for(int x=5;x<=14;x++)map.Set(x,y,x==5 || x==14 || y==3 || y==12?Terrain.Wall:Terrain.Floor);map.Set(5,7,Terrain.DoorClosed);World w=new World(map,214,encounter:true,hunger:true);int id,length;for(int i=0;i<4;i++)Require(w.TrySpawn(Species.Human,6,7,out id),"Could not block inside entry.");var outside=new ActorState{species=Species.Human,id=999,x=1,y=7};Require(LongRouteFor(w)(outside,10,8,false,out length,256)<0 && RouteVisited(w)==0,"Blocked entrance caused a map-wide search for an empty interior goal.");var saved=w.Capture();saved.actors[0].x=8;saved.actors[0].y=5;w=World.Restore(saved);Require(LongRouteFor(w)(outside,10,8,false,out length,256)>=0,"Reopened entry remained cached as blocked.");
            map.Set(14,8,Terrain.DoorClosed);w=new World(map,214,encounter:true,hunger:true);for(int i=0;i<4;i++)w.TrySpawn(Species.Human,6,7,out id);Require(LongRouteFor(w)(outside,10,8,false,out length,256)>=0,"One blocked entrance hid another usable door.");
        });
        Check("An actor already on a full doorway can enter a free room and route probes preserve saved state",()=>{
            WorldMap map=new WorldMap(20,20);for(int y=3;y<=12;y++)for(int x=5;x<=14;x++)map.Set(x,y,x==5 || x==14 || y==3 || y==12?Terrain.Wall:Terrain.Floor);map.Set(5,7,Terrain.DoorOpen);World w=new World(map,214,encounter:true,hunger:true);int id,length;for(int i=0;i<4;i++)Require(w.TrySpawn(Species.Human,5,7,out id),"Could not fill doorway.");string before=State(w);Require(LongRouteFor(w)(w.ActorAt(0),10,8,false,out length,256)>=0,"Current doorway occupancy prevented leaving it.");Require(State(w)==before,"Route scratch altered saved state.");
        });
    }
    private static void CheckCityScaleCases()
    {
        CheckBlockedLongRouteCases();
        Check("Compact three and four-home blocks have separate rooms and clear outward entrances",()=>{
            foreach(uint seed in new[]{1u,214u,999u,uint.MaxValue})foreach(int density in new[]{3,4})foreach(CityStreetLayout layout in new[]{CityStreetLayout.Grid,CityStreetLayout.Staggered}){
                var district=CityGenerator.BuildingLots(seed,4,2,density,layout);World world=CitySetup.Create(seed,4,2,density,layout);var summary=CitySetup.Describe(seed,4,2,density,layout);
                Require(world.Count==summary.Homes && world.ShelterRoomCount==summary.Buildings && !world.ShelterScanLimited && world.FoodSupplies().RemainingMeals==summary.Meals,"Compact city counts or room discovery failed.");
                var occupied=new HashSet<int>();foreach(var b in district.Buildings){for(int y=b.Top;y<b.Top+b.Height;y++)for(int x=b.Left;x<b.Left+b.Width;x++)Require(occupied.Add(y*world.Width+x),"Compact buildings overlap.");ActorState actor=world.ActorAt(0);actor.x=b.InsideX;actor.y=b.InsideY;actor.assignedHouseId=world.HouseAt(actor.x,actor.y);int length;Require(actor.assignedHouseId!=0 && CityRoute(world,actor,b.PathEndX,b.PathEndY,out length)>=0,"Compact home lacks a reachable doorway.");}
                Require(State(world)==State(Documents.ParseSave(State(world))),"Compact city failed exact saving.");
            }
            var large=CitySetup.Describe(214,6,3,4,CityStreetLayout.Staggered);Require(large.Homes==60 && large.Buildings==63 && CitySetup.Create(214,6,3,4,CityStreetLayout.Staggered).ShelterRoomCount==63,"Largest compact preset lost housing.");Reject(()=>CitySetup.Create(214,10,10,4));Reject(()=>CitySetup.Create(214,4,2,5));
        });
        Check("Varied staggered block dimensions match setup measurements and preserve seeded generation",()=>{
            foreach(uint seed in new[]{1u,214u,999u,uint.MaxValue}){
                WorldMap map=CityGenerator.Streets(seed,4,2,CityStreetLayout.Staggered);var summary=CitySetup.Describe(seed,4,2,4,CityStreetLayout.Staggered);Require(map.Width==summary.Width && map.Height==summary.Height,"Varied blocks disagree with setup size.");
                var gaps=new HashSet<int>();int last=-1;for(int y=0;y<map.Height;y++)if(TerrainRules.RoadSurface(map.Get(3,y)) && (y==0 || !TerrainRules.RoadSurface(map.Get(3,y-1)))){if(last>=0)gaps.Add(y-last);last=y;}
                // Use the horizontal road lanes at the outside curb to identify the seeded axes.
                var axes=new List<int>();for(int y=1;y<map.Height-1;y++)if((map.RoadLaneMaskAt(8,y)&4)!=0)axes.Add(y);
                for(int i=1;i<axes.Count;i++)gaps.Add(axes[i]-axes[i-1]);Require(gaps.Count>1,"Staggered blocks lack dimension variation.");
                Require(State(new World(map,seed))==State(new World(CityGenerator.Streets(seed,4,2,CityStreetLayout.Staggered),seed)),"Varied blocks are not repeatable.");
            }
        });
        Check("Routing optimizations preserve pre-optimization simulation hashes",()=>{
            string[] expected={"36DD62EE1E9E5AA6DD5F9863FE58671E82B2B535E44D1A13819F67B4F2B1F718","00BBF233E437C42A6739060B78C4E82B66FC4B875E52611E721824E3742CA8BD","553AE11BEA242BFF68FAD684C99E40063E8EF1A9B22D3FC47BC5E30B20170095"};
            // This recorded routing oracle predates varied starting needs; preserve its original input, not a new digest.
            World[] worlds={EncounterScenarios.GeneratedBuildingTrial(214,4,2,2,variedNeeds:false),EncounterScenarios.PedestrianCrossingTrial(),EncounterScenarios.VehicleIntersectionTest()};
            for(int i=0;i<worlds.Length;i++){var legacy=worlds[i].Capture();legacy.rulesVersion=EncounterRules.JunctionTrafficVersion;for(int a=0;a<legacy.actors.Length;a++){legacy.actors[a].familiarStoreId=0;legacy.actors[a].additionalFamiliarStores=null;}if(legacy.vehicles!=null)for(int c=0;c<legacy.vehicles.Length;c++){legacy.vehicles[c].paint=Outbreak.Contracts.VehiclePaint.Blue;if(legacy.vehicles[c].nextMoveTick>legacy.tick)legacy.vehicles[c].nextMoveTick=legacy.tick+VehicleRules.MoveTicks;}worlds[i]=World.Restore(legacy);Run(worlds[i],500);string actual=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(State(worlds[i]))));Require(actual==expected[i],"Route optimization changed deterministic decisions.");}
        });
        Check("Warm long-route queries reuse bounded scratch without per-node allocations",()=>{
            World world=new World(CityGenerator.Streets(214),214,1000,true,hunger:true);ActorState actor=new ActorState{species=Species.Human,x=3,y=4,assignedHouseId=1};int length;var query=(LongRouteProbe)Delegate.CreateDelegate(typeof(LongRouteProbe),world,typeof(World).GetMethod("LongRoute",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic));for(int i=0;i<5;i++)Require(query(actor,world.Width-5,world.Height-8,false,out length,CityRouteRules.Radius)>=0,"Warmup route unavailable.");string before=State(world);long allocated=GC.GetAllocatedBytesForCurrentThread();for(int i=0;i<50;i++)Require(query(actor,world.Width-5,world.Height-8,false,out length,CityRouteRules.Radius)>=0,"Repeated route unavailable.");long bytes=GC.GetAllocatedBytesForCurrentThread()-allocated;Require(bytes<262144,"Warm routing still allocates search nodes per query: "+bytes);Require(State(world)==before,"Route scratch escaped into authoritative state.");
        });
    }
}
