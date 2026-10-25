using System;
using System.Collections.Generic;
using Outbreak.Core;
using Outbreak.Contracts;
using Outbreak.Persistence;
public static partial class Program
{
    private static int ZoningCoverage(BuildingUse[] zones,int columns,out int total)
    {
        int worst=0;total=0;
        for(int home=0;home<zones.Length;home++)
        {
            if(zones[home]!=BuildingUse.Home)continue;int best=int.MaxValue;
            for(int store=0;store<zones.Length;store++)if(zones[store]==BuildingUse.Store)best=Math.Min(best,Math.Abs(home%columns-store%columns)+Math.Abs(home/columns-store/columns));
            worst=Math.Max(worst,best);total+=best;
        }
        return worst;
    }
    private static void CheckCityZoningCases()
    {
        Check("Neighborhood zoning keeps roles staffed and improves worst home-to-shop block distance",()=>{
            bool improved=false;var patterns=new HashSet<string>();
            foreach(uint seed in new[]{1u,214u,999u,uint.MaxValue})for(int columns=2;columns<=6;columns++)for(int rows=2;rows<=6;rows++)
            {
                BuildingUse[] zones=CityGenerator.PlanZones(seed,columns,rows),fixedZones=CityGenerator.PlanZones(seed,columns,rows,CityZoning.Fixed);int homes=0,stores=0,centers=0;
                for(int i=0;i<zones.Length;i++){if(zones[i]==BuildingUse.Home)homes++;else if(zones[i]==BuildingUse.Store)stores++;else if(zones[i]==BuildingUse.DistributionCenter){centers++;Require((i%columns==0 || i%columns==columns-1) && (i/columns==0 || i/columns==rows-1),"Distribution center was not on the town edge.");}}
                Require(stores==(columns*rows>6?2:1) && centers==1 && homes==columns*rows-stores-1,"Zoning changed site counts.");
                int total,oldTotal,worst=ZoningCoverage(zones,columns,out total),oldWorst=ZoningCoverage(fixedZones,columns,out oldTotal);
                Require(worst<=oldWorst && (worst<oldWorst || total<=oldTotal),"Coverage zoning was worse than the fixed layout.");improved|=worst<oldWorst;
                Require(string.Join(",",zones)==string.Join(",",CityGenerator.PlanZones(seed,columns,rows)),"Zoning seed did not reproduce.");
                if(columns==4 && rows==2)patterns.Add(string.Join(",",zones));
            }
            Require(improved && patterns.Count>1,"Zoning did not improve coverage or vary its seeded arrangement.");
            int sum;Require(ZoningCoverage(CityGenerator.PlanZones(214,4,2),4,out sum)==1,"Standard neighborhood did not place every residential block beside a store block.");
            Reject(()=>CityGenerator.PlanZones(0,4,2));Reject(()=>CitySetup.Create(214,4,2,1,CityStreetLayout.Grid,(CityZoning)99));
        });
        Check("Zoned generated cities preserve entrances independent homes food and repeatable jobs",()=>{
            foreach(uint seed in new[]{214u,999u})foreach(int density in new[]{1,2})foreach(CityStreetLayout layout in new[]{CityStreetLayout.Grid,CityStreetLayout.Neighborhood})
            {
                World world=CitySetup.Create(seed,4,2,density,layout,CityZoning.NeighborhoodCenters);GeneratedDistrict district=CityGenerator.BuildingLots(seed,4,2,density,layout,CityZoning.NeighborhoodCenters);CitySetupSummary summary=CitySetup.Describe(seed,4,2,density,layout,CityZoning.NeighborhoodCenters);var homes=new HashSet<int>();
                Require(world.Count==summary.Humans && world.ShelterRoomCount==summary.Buildings && world.FoodSupplies().RemainingMeals==summary.Meals && world.Occupation(0)==HumanOccupation.Courier,"Zoning lost population, finite food or courier ordering.");
                foreach(GeneratedBuilding b in district.Buildings)
                {
                    Require(world.BuildingUseAt(b.BoxX,b.BoxY)==b.Use,"Zoned role did not match configured building.");
                    if(b.Use==BuildingUse.Store)
                    {
                        BuildingUse[] zones=CityGenerator.PlanZones(seed,4,2);int block=b.BlockRow*4+b.BlockColumn,vx=0,vy=0;
                        for(int home=0;home<zones.Length;home++)if(zones[home]==BuildingUse.Home)
                        {
                            int nearest=-1,best=int.MaxValue;for(int shop=0;shop<zones.Length;shop++)if(zones[shop]==BuildingUse.Store){int d=Math.Abs(home%4-shop%4)+Math.Abs(home/4-shop/4);if(d<best){best=d;nearest=shop;}}
                            if(nearest==block){vx+=home%4-b.BlockColumn;vy+=home/4-b.BlockRow;}
                        }
                        int fx=b.Facing==BuildingFacing.East?1:b.Facing==BuildingFacing.West?-1:0,fy=b.Facing==BuildingFacing.South?1:b.Facing==BuildingFacing.North?-1:0;
                        if(vx!=0 || vy!=0)Require(vx*fx+vy*fy>0,"Store entrance faced away from its served neighborhood.");
                    }
ActorState actor=world.ActorAt(0);actor.x=b.InsideX;actor.y=b.InsideY;actor.assignedHouseId=world.HouseAt(actor.x,actor.y);int distance;Require(CityRoute(world,actor,b.PathEndX,b.PathEndY,out distance)>=0,"Zoned entrance was blocked.");
                }
                for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);Require(homes.Add(actor.assignedHouseId),"Zoning assigned two households to the same home.");if(world.Occupation(i)!=HumanOccupation.Consumer)Require(actor.assignedHouseId!=world.HouseAt(actor.x,actor.y),"Zoned worker was assigned its workplace as home.");}
                Require(State(world)==State(Documents.ParseSave(State(world))) && State(world)==State(CitySetup.Create(seed,4,2,density,layout,CityZoning.NeighborhoodCenters)),"Zoned generation or save round trip diverged.");
                Run(world,50);Require(world.PendingSupplyOrders().Length==2,"Relocated clerks did not call for supplies.");World copy=Documents.ParseSave(State(world));Run(world,60);Run(copy,60);Require(State(world)==State(copy),"Zoned clerk/courier tasks diverged after loading.");
            }
        });
        Check("Large and small zoning retain catalog limits and existing fixed generation",()=>{
            foreach(var size in new[]{(2,2,2),(6,6,1),(6,5,2)})
            {
                World world=CitySetup.Create(214,size.Item1,size.Item2,size.Item3,CityStreetLayout.Neighborhood,CityZoning.NeighborhoodCenters);CitySetupSummary summary=CitySetup.Describe(214,size.Item1,size.Item2,size.Item3,CityStreetLayout.Neighborhood,CityZoning.NeighborhoodCenters);Require(!world.ShelterScanLimited && world.ShelterRoomCount==summary.Buildings && world.Count==summary.Homes && world.FoodSupplies().RemainingMeals==summary.Meals,"Zoned size limit or summary was wrong.");
            }
            Require(State(CitySetup.Create(214,4,2))==State(CitySetup.Create(214,4,2,1,CityStreetLayout.Grid,CityZoning.Fixed)),"Fixed-layout default changed.");Reject(()=>CitySetup.Create(214,6,6,2,CityStreetLayout.Neighborhood,CityZoning.NeighborhoodCenters));
        });
    }
}
