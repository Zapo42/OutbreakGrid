using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckNeighborhoodCases()
    {
        Check("Empty-home intelligent shopper finds nearby stocked shop despite its hidden doorway",()=>{
            World world=EncounterScenarios.HiddenStoreEntranceTest();SaveDocument before=world.Capture();before.rulesVersion=EncounterRules.PedestrianRoutesVersion;World legacy=World.Restore(before);Run(legacy,600);
            Require(!legacy.StorefrontDiscoveryEnabled && legacy.ActorAt(0).storeMemory==null && legacy.HouseholdFoodSupplies(legacy.ActorAt(0).assignedHouseId).BoxMeals==0,"Legacy hidden-door shopping failure was not reproduced.");
            ActorState[] previous=world.Capture().actors;int eaten=0;bool stocked=false;World copy=null;
            for(int tick=1;tick<=600;tick++)
            {
                world.Step();if(copy!=null){copy.Step();Require(State(world)==State(copy),"Hidden-door grocery trip diverged after saving.");if(tick==150)copy=null;}
                for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.hunger<previous[i].hunger)eaten++;previous[i]=actor;}
                Require(world.FoodSupplies().RemainingMeals+eaten==20,"Storefront discovery created or lost food.");
                stocked|=world.HouseholdFoodSupplies(world.ActorAt(0).assignedHouseId).BoxMeals>=2;
                if(tick==100)copy=Documents.ParseSave(State(world));
            }
            Require(stocked && world.ActorAt(0).intelligence==10 && world.ActorAt(0).storeMemory!=null,"Intelligent resident failed to discover, shop and resupply home.");
        });
        Check("Visible storefront recognition reveals no stock and respects intervening walls",()=>{
            SaveDocument saved=EncounterScenarios.HiddenStoreEntranceTest().Capture();saved.actors[0].x=6;saved.actors[0].y=5;saved.actors[0].shelterId=0;saved.actors[0].shelterX=0;saved.actors[0].shelterY=0;saved.actors[0].shelterState=ShelterState.None;
            World full=World.Restore(saved);WorldMap emptyMap=WorldMap.Restore(saved.map);emptyMap.SetShelf(13,7,0,true);saved.map=emptyMap.Capture();World empty=World.Restore(saved);full.Step();empty.Step();
            Require(full.ActorAt(0).storeMemory!=null && full.ActorAt(0).shelfMemory==null && Documents.Encode(full.ActorAt(0))==Documents.Encode(empty.ActorAt(0)),"Facade observation leaked hidden shelf stock.");
            WorldMap blocked=WorldMap.Restore(saved.map);for(int y=0;y<blocked.Height;y++)blocked.Set(9,y,Terrain.Wall);saved.map=blocked.Capture();World unseen=World.Restore(saved);unseen.Step();Require(unseen.ActorAt(0).storeMemory==null,"Store was identified through an intervening wall.");
        });
        Check("Dense residential blocks contain separate reachable houses and preserve the street network",()=>{
            foreach(uint seed in new[]{214u,999u,uint.MaxValue})foreach(var size in new[]{(2,2),(4,2),(6,5)})
            {
                GeneratedDistrict district=CityGenerator.BuildingLots(seed,size.Item1,size.Item2,2);WorldMap streets=CityGenerator.Streets(seed,size.Item1,size.Item2);World world=CitySetup.Create(seed,size.Item1,size.Item2,2);CitySetupSummary summary=CitySetup.Describe(seed,size.Item1,size.Item2,2);var lots=new Dictionary<int,int>();var occupied=new HashSet<int>();
                Require(world.ShelterRoomCount==summary.Buildings && world.Count==summary.Homes && world.Count==summary.Humans && world.FoodSupplies().RemainingMeals==summary.Meals && !world.ShelterScanLimited,"Dense setup counts/room discovery were wrong.");
                foreach(GeneratedBuilding b in district.Buildings)
                {
                    for(int y=b.Top;y<b.Top+b.Height;y++)for(int x=b.Left;x<b.Left+b.Width;x++)Require(occupied.Add(y*world.Width+x),"Dense building footprints overlap.");
                    int block=b.BlockRow*size.Item1+b.BlockColumn;if(b.Use==BuildingUse.Home){int count;lots.TryGetValue(block,out count);lots[block]=count+1;}
                    int xPath=b.DoorX,yPath=b.DoorY,sx=Math.Sign(b.PathEndX-xPath),sy=Math.Sign(b.PathEndY-yPath);while(xPath!=b.PathEndX || yPath!=b.PathEndY){xPath+=sx;yPath+=sy;Require(district.Map.Get(xPath,yPath)==Terrain.Sidewalk,"A neighboring house cut off its entrance path.");}
                    ActorState actor=world.ActorAt(0);actor.x=b.InsideX;actor.y=b.InsideY;actor.assignedHouseId=world.HouseAt(actor.x,actor.y);int distance;Require(actor.assignedHouseId!=0 && CityRoute(world,actor,b.PathEndX,b.PathEndY,out distance)>=0,"Dense house doorway was unreachable.");
                }
                foreach(int count in lots.Values)Require(count==2,"Residential block did not have two homes.");
                for(int y=0;y<world.Height;y++)for(int x=0;x<world.Width;x++)if(streets.Get(x,y)!=Terrain.Grass)Require(district.Map.Get(x,y)==streets.Get(x,y),"Dense houses damaged the original streets.");
                var homes=new HashSet<int>();for(int i=0;i<world.Count;i++)Require(homes.Add(world.ActorAt(i).assignedHouseId),"Dense city lost separate household assignments.");
                Require(State(world)==State(Documents.ParseSave(State(world))) && State(world)==State(CitySetup.Create(seed,size.Item1,size.Item2,2)),"Dense generation or save/load was not deterministic.");
            }
        });
        Check("Density limits reject oversized catalogs and dense clerk jobs continue after saves",()=>{
            Require(CitySetup.ValidationError(214,6,6,2)!=null,"Dense city exceeded supported room count.");Reject(()=>CitySetup.Create(214,6,6,2));Reject(()=>CityGenerator.BuildingLots(214,6,6,2));Reject(()=>CitySetup.Create(214,4,2,3));
            World world=CitySetup.Create(214,4,2,2);Require(world.Count==10 && world.ShelterRoomCount==13 && world.FoodSupplies().RemainingMeals==78,"Dense standard preview did not match its advertised setup.");Run(world,50);Require(world.PendingSupplyOrders().Length==2,"Dense city clerk requests failed.");World copy=Documents.ParseSave(State(world));Run(world,80);Run(copy,80);Require(State(world)==State(copy),"Dense job continuation diverged.");
        });
    }
}
