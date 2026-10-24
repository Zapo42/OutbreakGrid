using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckCityLotCases()
    {
        Check("Generated lots preserve streets and connect every enclosed building to a sidewalk",()=>{
            foreach(uint seed in new uint[]{214,999,1,325})foreach(int size in new[]{1,4,6})
            {
                GeneratedDistrict district=CityGenerator.BuildingLots(seed,size,size);WorldMap map=district.Map,streets=CityGenerator.Streets(seed,size,size);World world=new World(map,seed,encounter:true,hunger:true);
                Require(district.Buildings.Length==size*size && world.ShelterRoomCount==size*size && !world.ShelterScanLimited,"Generated room discovery lost a building.");
                var rooms=new HashSet<int>();foreach(GeneratedBuilding b in district.Buildings)
                {
                    int room=world.HouseAt(b.Left+1,b.Top+1);Require(room!=0 && rooms.Add(room),"Generated lots overlapped or failed room enclosure.");
                    Require(map.Get(b.DoorX,b.DoorY)==Terrain.DoorClosed && map.Get(b.DoorX,b.DoorY-1)==Terrain.Floor,"Generated door did not connect inside to outside.");
                    for(int y=b.DoorY+1;y<=b.PathEndY;y++)Require(map.Get(b.DoorX,y)==Terrain.Sidewalk,"Entrance path had a gap.");
                    Require(streets.Get(b.DoorX,b.PathEndY)==Terrain.Sidewalk,"Entrance path missed street sidewalk.");
                    ActorState actor=new ActorState{species=Species.Human,x=b.DoorX,y=b.DoorY-1,assignedHouseId=room};int distance;
                    Require(CityRoute(world,actor,b.DoorX,b.PathEndY,out distance)>=0 && distance==b.PathEndY-actor.y,"Generated doorway/path was not directly traversable.");
                }
                for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)if(streets.Get(x,y)!=Terrain.Grass)Require(map.Get(x,y)==streets.Get(x,y),"Lot construction damaged street, crossing, sidewalk or border.");
                Require(State(world)==State(new World(CityGenerator.BuildingLots(seed,size,size).Map,seed,encounter:true,hunger:true)),"Generated building seed failed to reproduce.");
            }
            Reject(()=>CityGenerator.BuildingLots(0));Reject(()=>CityGenerator.BuildingLots(1,0));
        });
        Check("Generated building preview binds homes and jobs with explicit finite supplies",()=>{
            foreach(uint seed in new uint[]{214,999})
            {
                World world=EncounterScenarios.GeneratedBuildingTrial(seed);Require(world.Count==5 && world.ShelterRoomCount==8 && world.StoreSupplies().Length==2 && world.DistributionCenters().Length==1 && world.FoodSupplies().RemainingMeals==63,"Generated preview had wrong roles, population or finite food.");
                var homes=new HashSet<int>();int stores=0,centers=0,residences=0;foreach(var pair in world.BuildingUses()){if(pair.Value==BuildingUse.Store)stores++;else if(pair.Value==BuildingUse.DistributionCenter)centers++;else if(pair.Value==BuildingUse.Home)residences++;}
                Require(stores==2 && centers==1 && residences==5,"Building designation colors did not match generated roles.");
                for(int i=0;i<world.Count;i++)
                {
                    ActorState actor=world.ActorAt(i);Require(homes.Add(actor.assignedHouseId) && world.BuildingUseAt((actor.assignedHouseId-1)%world.Width,(actor.assignedHouseId-1)/world.Width)==BuildingUse.Home,"Resident lacked a separate generated home.");
                    if(i<3)Require(world.HouseAt(actor.x,actor.y)!=actor.assignedHouseId && world.Occupation(i)!=HumanOccupation.Consumer,"Worker spawned at home or lost its job.");
                }
                Require(State(world)==State(EncounterScenarios.GeneratedBuildingTrial(seed)) && State(world)==State(Documents.ParseSave(State(world))),"Preview assignments/inventory did not repeat or round-trip exactly.");
                Run(world,50);Require(world.PendingSupplyOrders().Length==2,"Generated clerks failed phone requests.");World copy=Documents.ParseSave(State(world));Run(world,60);Run(copy,60);Require(State(world)==State(copy),"Generated jobs diverged across a saved continuation.");
            }
        });
    }
}
