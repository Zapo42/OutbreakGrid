using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static void CheckCityLotCases()
    {
        Check("Generated building roles have distinct sizes and all four street-facing entrances",()=>{
            var facings=new HashSet<BuildingFacing>();
            foreach(uint seed in new[]{214u,999u})foreach(GeneratedBuilding b in CityGenerator.BuildingLots(seed,6,6).Buildings)
            {
                facings.Add(b.Facing);int min=b.Use==BuildingUse.DistributionCenter?12:b.Use==BuildingUse.Store?10:7,max=b.Use==BuildingUse.DistributionCenter?14:b.Use==BuildingUse.Store?12:10;
                Require(b.Width>=min && b.Width<=max && b.Height>=min && b.Height<=max,"Generated footprint did not match building use.");
                Require(b.Facing==BuildingFacing.North?b.DoorY==b.Top && b.PathEndY<b.DoorY:b.Facing==BuildingFacing.South?b.DoorY==b.Top+b.Height-1 && b.PathEndY>b.DoorY:b.Facing==BuildingFacing.West?b.DoorX==b.Left && b.PathEndX<b.DoorX:b.DoorX==b.Left+b.Width-1 && b.PathEndX>b.DoorX,"Door did not face its connected street.");
            }
            Require(facings.Count==4,"Generated neighborhoods omitted an entrance orientation.");
        });
        Check("Generated lots preserve streets and connect every enclosed building to a sidewalk",()=>{
            foreach(uint seed in new uint[]{214,999,1,325})foreach(int size in new[]{1,4,6})
            {
                GeneratedDistrict district=CityGenerator.BuildingLots(seed,size,size);WorldMap map=district.Map,streets=CityGenerator.Streets(seed,size,size);World world=new World(map,seed,encounter:true,hunger:true);
                Require(district.Buildings.Length==size*size && world.ShelterRoomCount==size*size && !world.ShelterScanLimited,"Generated room discovery lost a building.");
                var rooms=new HashSet<int>();foreach(GeneratedBuilding b in district.Buildings)
                {
                    int room=world.HouseAt(b.Left+1,b.Top+1);Require(room!=0 && rooms.Add(room),"Generated lots overlapped or failed room enclosure.");
                    Require(map.Get(b.DoorX,b.DoorY)==Terrain.DoorClosed && map.Get(b.InsideX,b.InsideY)==Terrain.Floor,"Generated door did not connect inside to outside.");
                    int pathX=b.DoorX,pathY=b.DoorY,stepX=Math.Sign(b.PathEndX-pathX),stepY=Math.Sign(b.PathEndY-pathY);
                    Require((stepX==0)!=(stepY==0),"Entrance path was not aligned to its facing street.");
                    while(pathX!=b.PathEndX || pathY!=b.PathEndY){pathX+=stepX;pathY+=stepY;Require(map.Get(pathX,pathY)==Terrain.Sidewalk,"Entrance path had a gap.");}
                    Require(streets.Get(b.PathEndX,b.PathEndY)==Terrain.Sidewalk,"Entrance path missed street sidewalk.");
                    ActorState actor=new ActorState{species=Species.Human,x=b.InsideX,y=b.InsideY,assignedHouseId=room};int distance;
                    Require(CityRoute(world,actor,b.PathEndX,b.PathEndY,out distance)>=0 && distance==Math.Abs(b.PathEndX-actor.x)+Math.Abs(b.PathEndY-actor.y),"Generated doorway/path was not directly traversable.");
                }
                for(int y=0;y<map.Height;y++)for(int x=0;x<map.Width;x++)if(streets.Get(x,y)!=Terrain.Grass)Require(map.Get(x,y)==streets.Get(x,y),"Lot construction damaged street, crossing, sidewalk or border.");
                Require(State(world)==State(new World(CityGenerator.BuildingLots(seed,size,size).Map,seed,encounter:true,hunger:true)),"Generated building seed failed to reproduce.");
            }
            Reject(()=>CityGenerator.BuildingLots(0));Reject(()=>CityGenerator.BuildingLots(1,0));
        });
        Check("Furnished generated buildings retain usable paths through every street-facing door",()=>{
            foreach(uint seed in new[]{214u,999u})
            {
                World world=CitySetup.Create(seed,6,6);WorldMap map=WorldMap.Restore(world.Capture().map);
                foreach(GeneratedBuilding b in CityGenerator.BuildingLots(seed,6,6).Buildings)
                {
                    ActorState actor=world.ActorAt(0);actor.x=b.InsideX;actor.y=b.InsideY;actor.assignedHouseId=world.HouseAt(actor.x,actor.y);int length;
                    Require(map.CanWalk(actor.x,actor.y) && CityRoute(world,actor,b.PathEndX,b.PathEndY,out length)>=0,"Furniture blocked a generated doorway approach.");
                    var seen=new HashSet<int>();var queue=new Queue<int>();int start=actor.y*world.Width+actor.x;seen.Add(start);queue.Enqueue(start);
                    while(queue.Count>0){int key=queue.Dequeue(),x=key%world.Width,y=key/world.Width;foreach(var offset in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int nx=x+offset.Item1,ny=y+offset.Item2;if(nx<=b.Left || nx>=b.Left+b.Width-1 || ny<=b.Top || ny>=b.Top+b.Height-1 || !map.CanWalk(nx,ny))continue;int next=ny*world.Width+nx;if(seen.Add(next))queue.Enqueue(next);}}
                    for(int y=b.Top+1;y<b.Top+b.Height-1;y++)for(int x=b.Left+1;x<b.Left+b.Width-1;x++)if(map.CanWalk(x,y))Require(seen.Contains(y*world.Width+x),"Furnishing isolated usable interior space.");
                }
            }
        });
        Check("Workers physically leave varied entrances and reach their separate generated homes",()=>{
            foreach(uint seed in new[]{214u,999u})
            {
                World world=CitySetup.Create(seed,4,2);world.TrySetCourierSchedule(0,19,12);world.TrySetClerkSchedule(1,19,12);world.TrySetClerkSchedule(2,19,12);var arrived=new HashSet<int>();ActorState[] previous=world.Capture().actors;World copy=null;
                for(int tick=1;tick<=1600 && arrived.Count<3;tick++)
                {
                    world.Step();if(copy!=null){copy.Step();Require(State(world)==State(copy),"Varied-entrance commute diverged after loading.");if(tick==60)copy=null;}
                    for(int i=0;i<3;i++){ActorState actor=world.ActorAt(i);Require(actor.health>0 && Math.Abs(actor.x-previous[i].x)<=1 && Math.Abs(actor.y-previous[i].y)<=1,"Generated commute teleported or killed its worker.");if(world.HouseAt(actor.x,actor.y)==actor.assignedHouseId && actor.workerCommuteId==0)arrived.Add(i);previous[i]=actor;}
                    if(tick==50)copy=Documents.ParseSave(State(world));
                }
                Require(arrived.Count==3,"Worker failed to leave work or enter home through its generated doorway.");
            }
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
