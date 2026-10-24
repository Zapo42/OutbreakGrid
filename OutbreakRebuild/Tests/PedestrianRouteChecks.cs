using System;
using System.Collections.Generic;
using System.Reflection;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static HashSet<int> WalkPedestrianPlan(World world,ActorState actor,int goalX,int goalY)
    {
        int[] px={0,1,1,1,0,-1,-1,-1,0},py={1,1,0,-1,-1,-1,0,1,0};var visited=new HashSet<int>();string before=State(world);
        for(int i=0;i<200 && (actor.x!=goalX || actor.y!=goalY);i++)
        {
            int length,step=CityRoute(world,actor,goalX,goalY,out length);Require(step>=0 && step<8,"Pedestrian plan became stuck.");actor.x+=px[step];actor.y+=py[step];visited.Add(actor.y*world.Width+actor.x);
        }
        Require(actor.x==goalX && actor.y==goalY && State(world)==before,"Pedestrian route looped or mutated world during planning.");return visited;
    }
    private static void CheckPedestrianRouteCases()
    {
        Check("Safe assigned walkers keep a straight final approach instead of switching to local zigzags",()=>{
            SaveDocument saved=EncounterScenarios.PedestrianCrossingTrial().Capture();WorldMap map=WorldMap.Restore(saved.map);
            for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)if(map.Get(x,y)==Terrain.Road || map.Get(x,y)==Terrain.Sidewalk || map.Get(x,y)==Terrain.Crosswalk)map.Set(x,y,Terrain.Grass);
            saved.map=map.Capture();World world=World.Restore(saved);ActorState actor=world.ActorAt(0);actor.x=12;actor.y=12;
            var path=WalkPedestrianPlan(world,actor,22,12);Require(path.Count==10,"Final approach took unnecessary steps.");foreach(int key in path)Require(key/world.Width==12,"Walker deviated sideways on a clear straight final approach.");
            World city=EncounterScenarios.GeneratedCityTrial();actor=city.ActorAt(0);actor.x=60;actor.y=14;path=WalkPedestrianPlan(city,actor,74,10);
            Require(path.Contains(10*city.Width+73),"Generated city approach missed its doorway.");foreach(int key in path)Require(key/city.Width>=10 && key/city.Width<=14 && key%city.Width<=74,"Generated city approach arced away from its doorway.");
        });
        Check("Safe pedestrian routes use a nearby crosswalk instead of a shorter road crossing",()=>{
            World world=EncounterScenarios.PedestrianCrossingTrial();ActorState actor=world.ActorAt(0);actor.x=12;actor.y=6;
            var path=WalkPedestrianPlan(world,actor,18,6);Require(path.Contains(10*world.Width+14) && path.Contains(10*world.Width+15),"Safe walker skipped the nearby crosswalk.");
            foreach(int cell in path)if(cell%world.Width==14 || cell%world.Width==15)Require(cell/world.Width==10,"Pedestrian cut across road away from the available crossing.");
            actor.assignedHouseId=0;path=WalkPedestrianPlan(world,actor,18,6);Require(path.Contains(10*world.Width+14),"Local unassigned pedestrian ignored street preference.");
            SaveDocument old=world.Capture();old.rulesVersion=EncounterRules.LongRoutesVersion;World legacy=World.Restore(old);actor=legacy.ActorAt(0);actor.x=12;actor.y=6;
            path=WalkPedestrianPlan(legacy,actor,18,6);Require(!legacy.PedestrianRoutesEnabled && !path.Contains(10*world.Width+14),"Legacy routing silently acquired crossing preference.");
        });
        Check("Missing and blocked crosswalks allow ordinary road crossing without trapping walkers",()=>{
            foreach(bool blocked in new[]{false,true})
            {
                SaveDocument saved=EncounterScenarios.PedestrianCrossingTrial().Capture();WorldMap map=WorldMap.Restore(saved.map);map.Set(14,10,blocked?Terrain.Wall:Terrain.Road);map.Set(15,10,blocked?Terrain.Wall:Terrain.Road);saved.map=map.Capture();World world=World.Restore(saved);ActorState actor=world.ActorAt(0);actor.x=12;actor.y=6;
                var path=WalkPedestrianPlan(world,actor,18,6);bool crossed=false;foreach(int cell in path)if(cell%world.Width==14){crossed=true;Require(!blocked || cell/world.Width!=10,"Walker crossed blocked crossing.");}Require(crossed,"Fallback route never crossed the street.");
            }
        });
        Check("Threat-aware routing bypasses pedestrian preference and retains old local choices",()=>{
            World world=EncounterScenarios.PedestrianCrossingTrial();SaveDocument saved=world.Capture();saved.rulesVersion=EncounterRules.LongRoutesVersion;World old=World.Restore(saved);ActorState actor=world.ActorAt(0);actor.x=12;actor.y=6;
            object[] currentArgs={actor,18,6,1,0,null,false,false},oldArgs={actor,18,6,1,0,null,false,false};MethodInfo route=typeof(World).GetMethod("ShelterRoute",BindingFlags.Instance|BindingFlags.NonPublic);
            Require((int)route.Invoke(world,currentArgs)==(int)route.Invoke(old,oldArgs) && (int)currentArgs[4]==(int)oldArgs[4],"Threat-aware route was diverted by street preference.");
        });
        Check("Off-duty worker physically follows the crossing home with finite energy and exact saves",()=>{
            World world=EncounterScenarios.PedestrianCrossingTrial(),copy=null;bool crossed=false,arrived=false,spent=false;int previousX=world.ActorAt(0).x,previousY=world.ActorAt(0).y;
            for(int tick=1;tick<=400;tick++)
            {
                world.Step();if(copy!=null){copy.Step();Require(State(world)==State(copy),"Pedestrian commute diverged after save/load.");}
                ActorState actor=world.ActorAt(0);spent|=EnergyRules.Remaining(actor)<100;Require(Math.Abs(actor.x-previousX)<=1 && Math.Abs(actor.y-previousY)<=1,"Pedestrian commute teleported.");previousX=actor.x;previousY=actor.y;
                if(actor.x==14 || actor.x==15){crossed=true;Require(actor.y==10,"Actual commuter ignored nearby crossing.");}
                arrived|=world.HouseAt(actor.x,actor.y)==actor.assignedHouseId && actor.workerCommuteId==0;
                if(tick==50)copy=Documents.ParseSave(State(world));
            }
            Require(crossed && arrived && world.FoodSupplies().RemainingMeals==17 && spent,"Commute failed arrival, food conservation or travel energy cost.");
        });
    }
}
