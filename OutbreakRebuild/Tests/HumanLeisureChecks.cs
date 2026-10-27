using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    // Observe public saved phases and real movement rather than depend on the planner's private route.
    private static World LeisureAtPhase(HumanLeisurePhase phase)
    {
        World world=EncounterScenarios.HumanLeisureTest();
        for(int t=0;t<900 && world.ActorAt(0).leisurePhase!=phase;t++)world.Step();
        Require(world.ActorAt(0).leisurePhase==phase,"Controlled human did not reach leisure phase "+phase+".");return world;
    }
    private static void CheckHumanLeisureCases()
    {
        Check("A free-time walk physically leaves home pauses outdoors and returns with paid movement",()=>{
            World world=EncounterScenarios.HumanLeisureTest();Require(world.LeisureEnabled,"Current rules lack leisure.");int home=world.ActorAt(0).assignedHouseId,moves=0;bool outbound=false,paused=false,returning=false,left=false,finished=false;
            for(int t=0;t<900;t++){
                var before=world.ActorAt(0);world.Step();var actor=world.ActorAt(0);outbound|=actor.leisurePhase==HumanLeisurePhase.Outbound;paused|=actor.leisurePhase==HumanLeisurePhase.Pausing;returning|=actor.leisurePhase==HumanLeisurePhase.Returning;
                int house=world.HouseAt(actor.x,actor.y);Require(house==0 || house==home,"Leisure entered somebody else's building.");left|=house==0;Require(world.TerrainAt(actor.x,actor.y)!=Terrain.Road,"Leisure wandered into a vehicle road.");
                Require(Math.Abs(actor.x-before.x)<=1 && Math.Abs(actor.y-before.y)<=1,"Leisure teleported through its route.");
                if(actor.x!=before.x || actor.y!=before.y){moves++;Require(actor.energySpent>before.energySpent,"Actual leisure movement was free.");}
                if(paused && returning && actor.leisurePhase==HumanLeisurePhase.None && house==home){finished=true;break;}
            }
            Require(outbound && paused && returning && left && finished && moves>=4,"Walk did not complete all observable phases.");
            long next=world.ActorAt(0).nextLeisureTick;Require(next>world.Tick,"Walk did not schedule an occasional cooldown.");
            for(int t=0;t<50 && world.Tick+1<next;t++){world.Step();Require(world.ActorAt(0).leisurePhase==HumanLeisurePhase.None,"Finished walk immediately restarted.");}
        });
        Check("A normal unscheduled resident waits at home before occasionally taking a walk",()=>{
            // A neutral preference retains the original baseline wait; indoor/outdoor waits have separate coverage.
            var saved=EncounterScenarios.HumanLeisureTest().Capture();saved.actors[0].nextLeisureTick=0;saved.actors[0].charisma=5;World world=World.Restore(saved);int home=world.ActorAt(0).assignedHouseId;
            for(int t=0;t<300;t++){world.Step();var actor=world.ActorAt(0);Require(actor.leisurePhase==HumanLeisurePhase.None && world.HouseAt(actor.x,actor.y)==home,"Initial free time fell through to automatic shelter departure.");}
            bool walked=false;for(int t=0;t<120;t++){world.Step();walked|=world.ActorAt(0).leisurePhase==HumanLeisurePhase.Outbound;}Require(walked,"Initial bounded waiting never produced an optional walk.");
        });
        Check("Changing or clearing an assigned home immediately clears saved leisure geometry",()=>{
            foreach(bool active in new[]{false,true})foreach(bool unassign in new[]{false,true}){
                World world=active?LeisureAtPhase(HumanLeisurePhase.Pausing):EncounterScenarios.HumanLeisureTest();
                Require(unassign?world.TryUnassignHouse(0):world.TryAssignHouse(0,18,9),"House command was unexpectedly rejected.");
                var actor=world.ActorAt(0);Require(actor.leisurePhase==HumanLeisurePhase.None && actor.nextLeisureTick==0 && actor.leisureUntilTick==0 && actor.leisureHomeCell==0 && actor.leisureDoor==0,"House change retained an old walk or cooldown.");
                Require(State(world)==State(Documents.ParseSave(State(world))),"House command produced an unloadable save before the next tick.");
            }
        });
        Check("Leisure save continuation is exact during outbound pause return and cooldown",()=>{
            foreach(var phase in new[]{HumanLeisurePhase.Outbound,HumanLeisurePhase.Pausing,HumanLeisurePhase.Returning}){
                World world=LeisureAtPhase(phase),copy=Documents.ParseSave(State(world));string paused=State(world);TickClock clock=new TickClock();clock.Advance(100,world.Step);Require(State(world)==paused,"Paused real time advanced a walk.");
                for(int t=0;t<120;t++){world.Step();copy.Step();Require(State(world)==State(copy),"Leisure save diverged in "+phase+".");}
            }
            World idle=LeisureAtPhase(HumanLeisurePhase.Returning);for(int t=0;t<400 && idle.ActorAt(0).leisurePhase!=HumanLeisurePhase.None;t++)idle.Step();Require(idle.ActorAt(0).leisurePhase==HumanLeisurePhase.None,"Return never completed.");World restored=Documents.ParseSave(State(idle));Run(idle,80);Run(restored,80);Require(State(idle)==State(restored),"Cooldown save changed continuation.");
        });
        Check("Hunger low energy injury and stress prevent optional departures",()=>{
            foreach(int condition in new[]{0,1,2,3}){
                var saved=EncounterScenarios.HumanLeisureTest().Capture();var actor=saved.actors[0];
                if(condition==0){actor.hunger=40;actor.carriedFood=0;}if(condition==1)actor.energySpent=50;if(condition==2)actor.health=60;if(condition==3){actor.stress=40;actor.reaction=HumanReaction.Aware;}
                saved.actors[0]=actor;World world=World.Restore(saved);
                // Observe the first human action before healing, eating or recovery can remove the need.
                for(int t=0;t<5;t++){world.Step();Require(world.ActorAt(0).leisurePhase==HumanLeisurePhase.None,"Optional departure outranked condition "+condition+".");}
            }
        });
        Check("Urgent needs and danger interrupt an existing optional outdoor pause",()=>{
            foreach(int condition in new[]{0,1,2,3}){
                var saved=LeisureAtPhase(HumanLeisurePhase.Pausing).Capture();var actor=saved.actors[0];
                if(condition==0){actor.hunger=50;actor.carriedFood=0;}if(condition==1)actor.energySpent=75;if(condition==2)actor.health=50;if(condition==3){actor.stress=40;actor.reaction=HumanReaction.Aware;}
                saved.actors[0]=actor;World world=World.Restore(saved);Run(world,5);var after=world.ActorAt(0);Require(after.leisurePhase!=HumanLeisurePhase.Outbound && after.leisurePhase!=HumanLeisurePhase.Pausing,"Optional trip ignored changed condition "+condition+".");
            }
        });
        Check("A bitten walker abandons leisure between movement decisions and clears it on turning",()=>{
            var saved=LeisureAtPhase(HumanLeisurePhase.Pausing).Capture();saved.actors[0].health=90;saved.actors[0].turnAtTick=saved.tick+10;World world=World.Restore(saved);world.Step();Require(world.ActorAt(0).leisurePhase==HumanLeisurePhase.None,"Bitten walker retained leisure until its next movement action.");Run(world,10);var actor=world.ActorAt(0);Require(actor.species==Species.Zombie && actor.leisurePhase==HumanLeisurePhase.None && actor.nextLeisureTick==0,"Turning retained human leisure state.");Require(State(world)==State(Documents.ParseSave(State(world))),"Turned former walker cannot save.");
        });
        Check("A visible zombie interrupts optional leisure promptly",()=>{
            World world=LeisureAtPhase(HumanLeisurePhase.Pausing);var person=world.ActorAt(0);int zombie;bool placed=false;
            for(int y=person.y-2;y<=person.y+2 && !placed;y++)for(int x=person.x-2;x<=person.x+2 && !placed;x++)if((x!=person.x || y!=person.y) && world.HouseAt(x,y)==0)placed=world.TrySpawn(Species.Zombie,x,y,out zombie);
            Require(placed,"Threat fixture could not place a visible zombie.");Run(world,5);var after=world.ActorAt(0);Require(after.leisurePhase!=HumanLeisurePhase.Outbound && after.leisurePhase!=HumanLeisurePhase.Pausing,"Human continued optional leisure beside a zombie.");Require(after.stress>0 || after.turnAtTick>0 || after.health<100,"Threat was never perceived by the fixture.");
        });
        Check("Assigned work and home shopping retain priority over optional walking",()=>{
            World work=EncounterScenarios.StoreShoppingTest();Run(work,100);Require(work.ActorAt(1).leisurePhase==HumanLeisurePhase.None,"Clerk took a leisure trip during work.");
            World shop=EncounterScenarios.LargeCityShoppingTest();bool shopping=false;for(int t=0;t<150;t++){shop.Step();var actor=shop.ActorAt(0);shopping|=actor.shoppingStoreId!=0;Require(actor.leisurePhase==HumanLeisurePhase.None,"Empty-home shopping was replaced by leisure.");}Require(shopping,"Priority fixture did not begin actual shopping.");
        });
        Check("Newly observed empty home supplies interrupt an outbound walk for real shopping",()=>{
            World initial=LeisureAtPhase(HumanLeisurePhase.Outbound);var saved=initial.Capture();var actor=saved.actors[0];actor.intelligence=10;actor.familiarStoreId=initial.HouseAt(18,9);actor.storageMemory[0].food=0;actor.storageMemory[0].space=12;actor.storageMemory[0].observedTick=saved.tick;actor.shelfMemory=new[]{new ShelfMemoryRecord{x=19,y=10,food=6,observedTick=saved.tick}};saved.actors[0]=actor;
            foreach(var box in saved.map.storageBoxes)if(box.x==3 && box.y==7)box.items=new ItemStackDocument[0];
            World world=World.Restore(saved);Run(world,10);var after=world.ActorAt(0);Require(after.leisurePhase==HumanLeisurePhase.None && after.restockPhase!=RestockPhase.None,"Optional walk displaced newly necessary home shopping.");
        });
        Check("An enclosed home with no safe outdoor destination does not create a leisure trip",()=>{
            World original=EncounterScenarios.HumanLeisureTest();var saved=original.Capture();WorldMap map=WorldMap.Restore(saved.map);
            for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)if(original.HouseAt(x,y)==0 && (map.Get(x,y)==Terrain.Grass || map.Get(x,y)==Terrain.Sidewalk || map.Get(x,y)==Terrain.Crosswalk))map.Set(x,y,Terrain.Wall);
            saved.map=map.Capture();World world=World.Restore(saved);for(int t=0;t<150;t++){world.Step();Require(world.ActorAt(0).leisurePhase==HumanLeisurePhase.None,"Blocked exterior fabricated a safe destination.");}Require(State(world)==State(Documents.ParseSave(State(world))),"Blocked fallback cannot save.");
        });
        Check("Leisure save validation rejects malformed state and incompatible older rules",()=>{
            Action<Action<SaveDocument>> bad=change=>{var saved=LeisureAtPhase(HumanLeisurePhase.Outbound).Capture();change(saved);Reject(()=>World.Restore(saved));};
            bad(s=>s.actors[0].leisurePhase=(HumanLeisurePhase)99);bad(s=>s.actors[0].leisureX=-1);bad(s=>s.actors[0].leisureY=s.map.height);bad(s=>s.actors[0].leisureDoor=0);bad(s=>s.actors[0].leisureHomeCell=0);bad(s=>s.actors[0].leisureUntilTick=-1);bad(s=>s.actors[0].leisureUntilTick=s.tick+10000);bad(s=>s.rulesVersion="encounter-prototype-99");
            var idle=EncounterScenarios.HumanLeisureTest().Capture();idle.actors[0].leisurePhase=HumanLeisurePhase.None;idle.actors[0].leisureDoor=1;Reject(()=>World.Restore(idle));
        });
        Check("Legacy rules99 retains independently recorded pre-leisure continuation",()=>{
            var saved=EncounterScenarios.HomeSupplyInspectionTest().Capture();saved.rulesVersion="encounter-prototype-99";World legacy=World.Restore(saved);Require(!legacy.LeisureEnabled,"Older run silently enabled free-time walking.");Run(legacy,600);
            // Digest was generated from unmodified c9d5cd2 before any leisure source was copied into QA.
            string digest;using(var sha=SHA256.Create())digest=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(State(legacy)))).Replace("-","");Require(digest=="0B8A502E56A55C18298230B5B9C35D85C1908B8E6AFC7390CDAB3552D98916F6","Rules99 continuation changed from the independent prior release.");
        });
    }
}
