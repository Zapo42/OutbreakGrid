using System;
using Outbreak.Contracts;
using Outbreak.Core;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckFoodScarcityCases()
    {
        Check("Twelve-hour crowded food trials conserve every meal across three seeds", () => {
            foreach(uint seed in new uint[]{214,123,999})
            {
                World world=EncounterScenarios.FoodScarcityTrial(seed);FoodOverview start=world.FoodSupplies();Require(world.Count==12 && world.ShelterRoomCount==4 && start.RemainingMeals==24 && start.StockedShelves==4 && start.EmptyShelves==2,"Scarcity fixture did not contain twelve humans, four rooms and 24 finite meals.");
                ActorState[] prior=world.Capture().actors;bool[] fed=new bool[12],movedAfterDepletion=new bool[12];int eaten=0,exhaustedAt=0,allFedAt=0;bool exploredAfterDepletion=false;
                for(int tick=1;tick<=7200;tick++)
                {
                    world.Step();for(int i=0;i<world.Count;i++)
                    {
                        ActorState actor=world.ActorAt(i);if(actor.hunger<prior[i].hunger){eaten++;fed[i]=true;}if(exhaustedAt>0 && (actor.x!=prior[i].x || actor.y!=prior[i].y))movedAfterDepletion[i]=true;
                        Require(actor.species==Species.Human && actor.health==100 && actor.hunger>=0 && actor.hunger<=100 && actor.carriedFood>=0 && actor.carriedFood<=HungerRules.CarryLimit && (actor.foodRoomVisits==null || actor.foodRoomVisits.Length<=FoodExplorationRules.HistoryLimit) && (actor.shelfMemory==null || actor.shelfMemory.Length<=ShelfMemoryRules.Limit),"Long scarcity run broke human needs, inventory or bounded memory.");prior[i]=actor;
                    }
                    FoodOverview food=world.FoodSupplies();Require(food.RemainingMeals+eaten==24 && world.Count==12,"Food was created, lost or duplicated during collection, sharing, storage or eating.");
                    if(allFedAt==0 && Array.TrueForAll(fed,value=>value))allFedAt=tick;if(exhaustedAt==0 && food.RemainingMeals==0)exhaustedAt=tick;
                    if(exhaustedAt>0 && food.InvestigatingRooms>0)exploredAfterDepletion=true;
                    if(tick%600==0){Inspect(world);World.Restore(world.Capture());}
                }
                Require(allFedAt>0 && allFedAt<=600 && exhaustedAt>0 && exhaustedAt<=2400 && eaten==24,"Crowded trial failed first-hour feeding or four-hour supply depletion.");
                Require(world.FoodSupplies().RemainingMeals==0 && world.FoodSupplies().VeryHungry==12 && Array.TrueForAll(prior,actor=>actor.hunger==100) && exploredAfterDepletion && Array.TrueForAll(movedAfterDepletion,value=>value),"Humans froze after exhaustion, invented food or failed to retain bounded hunger.");
                Console.WriteLine("SCARCITY seed "+seed+": all 12 fed by tick "+allFedAt+", all 24 meals eaten by tick "+exhaustedAt+", 12-hour checkpoint tick "+world.Tick+", remaining meals 0; all humans moved after depletion.");
            }
        });
        Check("Crowded scarcity saves resume exactly across feeding and exhausted-food phases", () => {
            World world=EncounterScenarios.FoodScarcityTrial();Run(world,600);World copy=Documents.ParseSave(Documents.Encode(world.Capture()));Require(world.FoodSupplies().RemainingMeals>0,"Save-resume test missed the carried-food phase.");for(int hour=0;hour<5;hour++){Run(world,600);Run(copy,600);Require(State(world)==State(copy),"Scarcity continuation lost clocks, stock, door state, notebooks or random streams.");}
            Require(world.Tick==3600 && world.FoodSupplies().RemainingMeals==0,"Save-resume test failed to cross into prolonged shortage.");copy=Documents.ParseSave(Documents.Encode(world.Capture()));Run(world,600);Run(copy,600);Require(State(world)==State(copy),"Exhausted-world save resumed differently.");Inspect(world);
        });
        Check("New visible food restarts eating after prolonged total depletion", () => {
            World world=EncounterScenarios.FoodScarcityTrial();Run(world,3600);Require(world.FoodSupplies().RemainingMeals==0,"Recovery test did not reach total depletion.");bool placed=false;for(int i=0;i<world.Count && !placed;i++){ActorState actor=world.ActorAt(i);foreach(int[] direction in new[]{new[]{1,0},new[]{0,1},new[]{-1,0},new[]{0,-1}})if(world.TryPlaceShelf(actor.x+direction[0],actor.y+direction[1],6)){placed=true;break;}}
            Require(placed && world.FoodSupplies().RemainingMeals==6,"Could not add one ordinary six-meal shelf beside a hungry human.");int eaten=0;ActorState[] prior=world.Capture().actors;for(int tick=0;tick<200;tick++){world.Step();for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.hunger<prior[i].hunger)eaten++;prior[i]=actor;}Require(world.FoodSupplies().RemainingMeals+eaten==6,"New supplies were lost or duplicated after a shortage.");}
            Require(eaten>0 && world.FoodSupplies().RemainingMeals<6,"Hungry humans remained stuck in empty-room routines after discovering food.");World.Restore(world.Capture());
        });
    }
}
