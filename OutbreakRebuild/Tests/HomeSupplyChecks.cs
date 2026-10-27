using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static World CenterVisitor(bool legacy=false,bool panic=false,bool courier=false)
    {
        World initial=EncounterScenarios.HomeSupplyInspectionTest();SaveDocument saved=initial.Capture();WorldMap map=WorldMap.Restore(saved.map);map.Set(25,5,Terrain.DoorOpen);saved.map=map.Capture();ActorState actor=saved.actors[0];actor.assignedHouseId=courier?actor.assignedHouseId:0;actor.x=26;actor.y=5;actor.shelterX=0;actor.shelterY=0;actor.shelterId=0;actor.shelterState=ShelterState.None;actor.hunger=panic?10:80;actor.carriedFood=panic?1:0;
        if(panic){actor.stress=50;actor.reaction=HumanReaction.Flight;actor.shelterId=initial.HouseAt(23,5);actor.shelterX=23;actor.shelterY=5;actor.shelterState=ShelterState.Seeking;}
        saved.actors[0]=actor;if(legacy)saved.rulesVersion=EncounterRules.ShortagePrioritiesVersion;World world=World.Restore(saved);if(courier)Require(world.TryAssignDistributionCourier(0,world.HouseAt(23,5)),"Could not bind authorized courier.");return world;
    }
    private static void CheckHomeSupplyCases()
    {
        Check("Assigned residents inspect unknown home storage with exact saved continuation",()=>{
            World world=EncounterScenarios.HomeSupplyInspectionTest(),copy=Documents.ParseSave(State(world));HomeSupplyKnowledge knowledge=world.HomeSuppliesKnownBy(0);Require(knowledge.Boxes==1 && knowledge.UnknownBoxes==1 && knowledge.KnownMeals==0,"Unseen home contents leaked into knowledge.");int food=world.FoodSupplies().RemainingMeals;bool moved=false;
            for(int i=0;i<100 && world.HomeSuppliesKnownBy(0).KnownBoxes==0;i++){world.Step();copy.Step();Require(State(world)==State(copy),"Home inspection diverged after reload.");moved|=world.ActorAt(0).x!=2 || world.ActorAt(0).y!=2;}
            knowledge=world.HomeSuppliesKnownBy(0);Require(moved && knowledge.KnownBoxes==1 && knowledge.KnownMeals==6 && knowledge.UnknownBoxes==0,"Resident did not inspect its home chest.");Require(world.FoodSupplies().RemainingMeals==food,"Inspection moved or created food.");Require(State(world)==State(Documents.ParseSave(State(world))),"Observed home knowledge could not reload.");
        });
        Check("A safe consumer returns to its assigned residence and checks that home instead of nearby distribution stock",()=>{
            World world=CenterVisitor();SaveDocument saved=world.Capture();saved.actors[0].assignedHouseId=world.HouseAt(2,2);saved.actors[0].hunger=10;saved.actors[0].carriedFood=1;world=World.Restore(saved);bool arrived=false;
            for(int i=0;i<400;i++){world.Step();ActorState actor=world.ActorAt(0);Require(world.BuildingUseAt(actor.x,actor.y)!=BuildingUse.DistributionCenter,"Home return took a routine shortcut through distribution storage.");arrived|=world.HouseAt(actor.x,actor.y)==actor.assignedHouseId;}
            Require(arrived && world.HomeSuppliesKnownBy(0).KnownBoxes==1,"Consumer failed to return to and inspect its assigned home.");
        });
        Check("Away residents retain observed quantities and distinguish stale reported and missing knowledge",()=>{
            World world=EncounterScenarios.HomeSupplyInspectionTest();Run(world,20);SaveDocument saved=world.Capture();ActorState actor=saved.actors[0];actor.x=18;actor.y=15;actor.shelterId=0;actor.shelterX=0;actor.shelterY=0;actor.shelterState=ShelterState.None;saved.actors[0]=actor;
            Array.Find(saved.map.storageBoxes,b=>b.x==12).items=new ItemStackDocument[0];world=World.Restore(saved);string before=State(world);Require(world.HomeSuppliesKnownBy(0).KnownMeals==6 && State(world)==before,"Away inspection read hidden updated stock or mutated state.");
            // Age the authored stock observation without carrying a mood deadline from the earlier time origin.
            saved.tick=HomeSupplyRules.RecheckTicks+20;for(int i=0;i<saved.actors.Length;i++)saved.actors[i].nextHappinessTick=0;world=World.Restore(saved);Require(world.HomeSuppliesKnownBy(0).StaleBoxes==1 && world.HomeSuppliesKnownBy(0).KnownMeals==0,"Old observations appeared freshly checked.");
            saved.actors[0].storageMemory=null;world=World.Restore(saved);Require(world.HomeSuppliesKnownBy(0).UnknownBoxes==1,"Missing observation fabricated food knowledge.");
            World reported=EncounterScenarios.HomeSupplyInspectionTest();int id;Require(reported.TrySpawn(Species.Human,17,16,out id),"Could not create reporter identity.");saved=reported.Capture();saved.actors[0].storageMemory=new[]{new StorageMemoryRecord{x=12,y=12,food=6,space=24,observedTick=0,reportedBy=id}};world=World.Restore(saved);Require(world.HomeSuppliesKnownBy(0).ReportedBoxes==1 && world.HomeSuppliesKnownBy(0).KnownBoxes==0 && world.HomeSuppliesKnownBy(0).KnownMeals==0,"Report counted as a personal home check.");Run(world,100);Require(world.HomeSuppliesKnownBy(0).KnownBoxes==1,"Reported supplies were never checked first-hand.");
        });
        Check("Hunger energy infection and visible danger interrupt deliberate home inspection",()=>{
            foreach(int mode in new[]{0,1,2})
            {
                SaveDocument saved=EncounterScenarios.HomeSupplyInspectionTest().Capture();if(mode==0){saved.actors[0].hunger=80;saved.actors[0].carriedFood=1;}if(mode==1)saved.actors[0].energySpent=100;if(mode==2)saved.actors[0].turnAtTick=InfectionRules.Delay(saved.actors[0],1);World world=World.Restore(saved);world.Step();Require(world.HomeSuppliesKnownBy(0).UnknownBoxes==1,"Unsafe inspection revealed distant stock.");if(mode==0)Require(world.ActorAt(0).hunger<80,"Home check delayed eating carried food.");
            }
            World threatened=EncounterScenarios.HomeSupplyInspectionTest();int id;Require(threatened.TrySpawn(Species.Zombie,4,2,out id),"Could not place inspection threat.");threatened.Step();Require(threatened.HomeSuppliesKnownBy(0).KnownBoxes==0,"Visible danger failed to stop stock inspection.");
        });
        Check("Consumers avoid distribution food exploration and entry while legacy visitors can still shop there",()=>{
            World world=CenterVisitor(),legacy=CenterVisitor(true);bool oldEntered=false;
            for(int tick=0;tick<250;tick++){world.Step();legacy.Step();ActorState actor=world.ActorAt(0),old=legacy.ActorAt(0);Require(world.BuildingUseAt(actor.x,actor.y)!=BuildingUse.DistributionCenter,"Routine consumer entered the distribution center.");oldEntered|=legacy.BuildingUseAt(old.x,old.y)==BuildingUse.DistributionCenter;if(tick%50==0)world=Documents.ParseSave(State(world));}
            Require(oldEntered && !legacy.HomeSuppliesEnabled && legacy.Capture().rulesVersion==EncounterRules.ShortagePrioritiesVersion,"Old rules silently gained consumer boundaries.");Require(world.ShelfAt(23,7).food==5 && StorageRules.Count(world.StorageBoxAt(24,3),InventoryItem.Food)==20,"Consumer withdrew distribution food.");
        });
        Check("Center staff and emergency shelter retain physical access without granting ordinary consumers supplies",()=>{
            World panic=CenterVisitor(panic:true),courier=CenterVisitor(courier:true);bool shelter=false,worker=false;
            for(int i=0;i<100;i++){panic.Step();courier.Step();ActorState a=panic.ActorAt(0),b=courier.ActorAt(0);shelter|=panic.BuildingUseAt(a.x,a.y)==BuildingUse.DistributionCenter;worker|=courier.BuildingUseAt(b.x,b.y)==BuildingUse.DistributionCenter;}
            Require(shelter && worker,"Consumer boundaries blocked emergency refuge or authorized center staff.");Require(panic.ShelfAt(23,7).food==5,"Emergency refuge granted consumer shopping access.");
        });
    }
}
