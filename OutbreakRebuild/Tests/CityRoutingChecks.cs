using System;
using System.Collections.Generic;
using System.Reflection;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static int CityRoute(World world,ActorState actor,int x,int y,out int distance)
    {
        object[] args={actor,x,y,0,0,null,false,false};
        int direction=(int)typeof(World).GetMethod("ShelterRoute",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(world,args);distance=(int)args[4];return direction;
    }
    private static void CheckCityRoutingCases()
    {
        Check("Seeded city streets repeat and connect every sidewalk road and crosswalk",()=>{
            foreach(uint seed in new uint[]{214,1,999})
            {
                WorldMap map=CityGenerator.Streets(seed);World world=new World(map,seed);
                Require(State(world)==State(new World(CityGenerator.Streets(seed),seed)),"City seed did not reproduce geometry.");
                var seen=new HashSet<int>();var queue=new Queue<int>();int total=0,crossings=0,sides=0;
                for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)
                {
                    Terrain tile=map.Get(x,y);if(tile==Terrain.Road || tile==Terrain.Sidewalk || tile==Terrain.Crosswalk){total++;if(queue.Count==0 && seen.Count==0){int key=y*map.Width+x;seen.Add(key);queue.Enqueue(key);}}
                    if(tile==Terrain.Crosswalk)crossings++;if(tile==Terrain.Sidewalk)sides++;
                    if(x==0 || y==0 || x==map.Width-1 || y==map.Height-1)Require(tile==Terrain.Wall,"Generated street escaped border.");
                }
                while(queue.Count>0){int key=queue.Dequeue(),x=key%map.Width,y=key/map.Width;foreach(var offset in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int nx=x+offset.Item1,ny=y+offset.Item2;if(!map.Contains(nx,ny))continue;Terrain tile=map.Get(nx,ny);int next=ny*map.Width+nx;if((tile==Terrain.Road || tile==Terrain.Sidewalk || tile==Terrain.Crosswalk) && seen.Add(next))queue.Enqueue(next);}}
                Require(total==seen.Count && crossings==120 && sides>0,"Generated street network disconnected or omitted crossing/sidewalks.");
                Require(State(world)==State(Documents.ParseSave(State(world))),"Generated geometry failed exact save round trip.");
            }
            Require(State(new World(CityGenerator.Streets(214)))!=State(new World(CityGenerator.Streets(999))),"Different seeds failed to vary blocks.");
            Reject(()=>CityGenerator.Streets(0));Reject(()=>CityGenerator.Streets(1,7));
        });
        Check("Long routes reach beyond the old square and preserve old saved rules",()=>{
            World world=EncounterScenarios.GeneratedCityTrial();ActorState actor=world.ActorAt(0);int goal=world.ActorAt(1).x,distance;
            Require(world.LongRoutesEnabled && goal-actor.x>64 && CityRoute(world,actor,goal,10,out distance)>=0 && distance>64,"Assigned worker could not plan beyond local range.");
            SaveDocument old=world.Capture();old.rulesVersion=EncounterRules.WorkerShiftsVersion;World legacy=World.Restore(old);
            Require(!legacy.LongRoutesEnabled && legacy.WorkerShiftsEnabled && CityRoute(legacy,legacy.ActorAt(0),goal,10,out distance)<0,"Legacy save silently changed its route search.");
            Require(CityRoute(world,actor,actor.x+CityRouteRules.Radius+1,actor.y,out distance)<0,"Over-budget target escaped route bound.");
            Require(State(world)==State(Documents.ParseSave(State(world))),"Route queries changed saved state.");
        });
        Check("Long route detours respect walls and fully blocked destinations",()=>{
            World initial=EncounterScenarios.GeneratedCityTrial();SaveDocument saved=initial.Capture();WorldMap map=WorldMap.Restore(saved.map);int barrier=map.Width/2;
            for(int y=1;y<map.Height-8;y++)map.Set(barrier,y,Terrain.Wall);
            saved.map=map.Capture();World world=World.Restore(saved);int distance;Require(CityRoute(world,world.ActorAt(0),world.ActorAt(1).x,10,out distance)>=0 && distance>64,"Long search missed detour outside old route square.");
            for(int y=map.Height-8;y<map.Height-1;y++)map.Set(barrier,y,Terrain.Wall);
            saved.map=map.Capture();world=World.Restore(saved);Require(CityRoute(world,world.ActorAt(0),world.ActorAt(1).x,10,out distance)<0,"Long route crossed sealed wall.");
        });
        Check("Generated city courier completes physical long deliveries and resumes deterministically",()=>{
            SaveDocument fixture=EncounterScenarios.GeneratedCityTrial().Capture();fixture.rulesVersion=EncounterRules.LongRoutesVersion;World world=World.Restore(fixture);ActorState[] previous=world.Capture().actors;int eaten=0;bool travelled=false,delivered=false;World copy=null;
            for(int tick=1;tick<=4800;tick++)
            {
                world.Step();if(copy!=null){copy.Step();Require(State(world)==State(copy),"Long courier save continuation diverged.");}
                for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);Require(actor.health>0,"Controlled long courier trial killed a worker.");if(actor.hunger<previous[i].hunger)eaten++;previous[i]=actor;}
                Require(world.FoodSupplies().RemainingMeals+eaten==32,"Generated-city delivery lost or created food.");
                travelled|=world.ActorAt(0).x>70;
                foreach(SupplyOrderDocument order in world.PendingSupplyOrders())delivered|=order.deliveredMeals>0;
                delivered|=world.CompletedSupplyOrders().Length>0;
                if(tick==300)world=Documents.ParseSave(State(world));if(tick==4760)copy=Documents.ParseSave(State(world));
            }
            Require(travelled && delivered && world.CompletedSupplyOrders().Length>0,"Physical courier failed distant store delivery.");
        });
    }
}
