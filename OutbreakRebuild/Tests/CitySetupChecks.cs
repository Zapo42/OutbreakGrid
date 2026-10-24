using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckCitySetupCases()
    {
        Check("City setup rejects unsupported dimensions and seeds before constructing a city",()=>{
            foreach(int columns in new[]{-1,0,1,7,int.MaxValue}){Require(CitySetup.ValidationError(214,columns,2)!=null,"Invalid width accepted.");Reject(()=>CitySetup.Create(214,columns,2));}
            foreach(int rows in new[]{-1,0,1,7,int.MaxValue}){Require(CitySetup.ValidationError(214,2,rows)!=null,"Invalid height accepted.");Reject(()=>CitySetup.Describe(214,2,rows));}
            Reject(()=>CitySetup.Create(0,4,2));Reject(()=>EncounterScenarios.GeneratedBuildingTrial(214,1,1));
            for(int columns=2;columns<=6;columns++)for(int rows=2;rows<=6;rows++)Require(CitySetup.ValidationError(uint.MaxValue,columns,rows)==null,"Supported rectangle rejected.");
            Require(State(CitySetup.Create(214,4,2))==State(EncounterScenarios.GeneratedBuildingTrial()),"Default configured city changed the existing preview.");
        });
        Check("City setup summaries match generated geometry population homes jobs and finite food",()=>{
            foreach(uint seed in new[]{1u,214u,uint.MaxValue})foreach(var size in new[]{(2,2),(2,6),(6,2),(6,6),(4,2),(3,5)})
            {
                CitySetupSummary summary=CitySetup.Describe(seed,size.Item1,size.Item2);World world=CitySetup.Create(seed,size.Item1,size.Item2);
                Require(world.Width==summary.Width && world.Height==summary.Height && world.ShelterRoomCount==summary.Buildings && !world.ShelterScanLimited,"Setup geometry did not match preview.");
                Require(world.Count==summary.Humans && summary.Humans==summary.Homes && world.StoreSupplies().Length==summary.Stores && world.DistributionCenters().Length==1 && world.FoodSupplies().RemainingMeals==summary.Meals,"Population/stock did not match setup summary.");
                var homes=new HashSet<int>();int consumers=0,clerks=0,couriers=0;
                for(int i=0;i<world.Count;i++)
                {
                    ActorState actor=world.ActorAt(i);Require(actor.assignedHouseId!=0 && homes.Add(actor.assignedHouseId),"Configured city lost unique assigned housing.");
                    HumanOccupation occupation=world.Occupation(i);if(occupation==HumanOccupation.Consumer)consumers++;else{Require(world.HouseAt(actor.x,actor.y)!=actor.assignedHouseId,"Configured worker lives at its workplace.");if(occupation==HumanOccupation.Clerk)clerks++;else couriers++;}
                }
                Require(consumers==summary.Consumers && clerks==summary.Stores && couriers==1,"Setup staff counts were wrong.");
                Require(State(world)==State(Documents.ParseSave(State(world))),"Configured city failed exact save/load.");
                if(seed==214)Require(State(world)==State(CitySetup.Create(seed,size.Item1,size.Item2)),"Configured city failed deterministic regeneration.");
            }
        });
        Check("Custom rectangular city resumes active clerk requests and courier work exactly",()=>{
            World world=CitySetup.Create(uint.MaxValue,3,2);Run(world,50);Require(world.PendingSupplyOrders().Length==1,"Small custom city did not start its store order.");World copy=Documents.ParseSave(State(world));Run(world,80);Run(copy,80);Require(State(world)==State(copy),"Configured rectangular city diverged after saving.");
        });
    }
}
