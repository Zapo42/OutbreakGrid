using System;
using System.Collections.Generic;
using Outbreak.Contracts;
namespace Outbreak.Core
{
    public enum CityPackTheme { Mixed, DenseUrban, UrbanApartments, TerraceResidential, GardenSuburb, CuldesacSuburb, SuburbanGrid, BusinessCore, BusinessCampus, RetailCorridor }
    public static class CityPackSetup
    {
        public static string SourceHash { get { return CityPackData.SourceHash; } }
        public static string ThemeName(CityPackTheme theme){return theme==CityPackTheme.Mixed?"Mixed city":theme>=CityPackTheme.DenseUrban && theme<=CityPackTheme.RetailCorridor?CityPackData.Profiles[(int)theme-1].Name:"Unknown city pack";}
        public static string ValidationError(uint seed,int columns,int rows,int homesPerBlock=2,CityPackTheme theme=CityPackTheme.Mixed)
        {
            if(theme<CityPackTheme.Mixed || theme>CityPackTheme.RetailCorridor)return "Choose a listed city-pack theme.";
            if(seed==0)return "Seed must be between 1 and 4294967295.";
            if(columns<2 || columns>10 || rows<2 || rows>10)return "Choose 2 to 10 blocks across and down.";
            if(homesPerBlock<1 || homesPerBlock>4)return "Choose one to four homes per residential block.";
            int blocks=columns*rows,stores=blocks>6?2:1,buildings=(blocks-stores-1)*homesPerBlock+stores+1;
            if(buildings*2>ShelterRules.MaxRooms)return "Two-room pack buildings exceed 256 physical rooms. Reduce block counts or residential density.";
            return null;
        }
        public static CitySetupSummary Describe(uint seed,int columns,int rows,int homesPerBlock=2,CityPackTheme theme=CityPackTheme.Mixed)
        {
            string error=ValidationError(seed,columns,rows,homesPerBlock,theme);if(error!=null)throw new ArgumentException(error);
            return Describe(CityPackGenerator.Plan(seed,columns,rows,homesPerBlock,theme));
        }
        private static CitySetupSummary Describe(PackPlan p)
        {
            int homes=0,stores=0;foreach(var b in p.Parcels){if(b.Blueprint.Use==BuildingUse.Home)homes++;if(b.Blueprint.Use==BuildingUse.Store)stores++;}
            return new CitySetupSummary{Width=p.Width,Height=p.Height,Buildings=p.Parcels.Count,Homes=homes,Stores=stores,Humans=homes,Consumers=homes-stores-1,Meals=40+4*stores+3*homes};
        }
        public static WorldMap GenerateMap(uint seed,int columns,int rows,int homesPerBlock=2,CityPackTheme theme=CityPackTheme.Mixed)
        {
            string error=ValidationError(seed,columns,rows,homesPerBlock,theme);if(error!=null)throw new ArgumentException(error);return CityPackGenerator.Build(CityPackGenerator.Plan(seed,columns,rows,homesPerBlock,theme));
        }
        public static World Create(uint seed,int columns,int rows,int homesPerBlock=2,CityPackTheme theme=CityPackTheme.Mixed)
        {
            string error=ValidationError(seed,columns,rows,homesPerBlock,theme);if(error!=null)throw new ArgumentException(error);
            PackPlan plan=CityPackGenerator.Plan(seed,columns,rows,homesPerBlock,theme);WorldMap map=CityPackGenerator.Build(plan);var homes=new List<PackParcel>();var jobs=new List<PackParcel>();
            foreach(var b in plan.Parcels)if(b.Blueprint.Use==BuildingUse.Home)homes.Add(b);else jobs.Add(b);
            jobs.Sort((a,b)=>{int order=((int)b.Blueprint.Use).CompareTo((int)a.Blueprint.Use);if(order==0)order=a.Row.CompareTo(b.Row);return order==0?a.Column.CompareTo(b.Column):order;});World world=new World(map,seed,1000,true,hunger:true);int id;
            for(int i=0;i<jobs.Count;i++){
                var b=jobs[i];bool center=b.Blueprint.Use==BuildingUse.DistributionCenter;
                if(!(center?world.TryConfigureDistributionCenter(b.BoxX,b.BoxY):world.TryConfigureStore(b.BoxX,b.BoxY)))throw new InvalidOperationException("Pack workplace configuration failed.");
                if(!world.TrySpawn(Species.Human,b.SpawnX,b.SpawnY,out id) || !world.TryAssignHouse(i,homes[i].SpawnX,homes[i].SpawnY))throw new InvalidOperationException("Pack worker home/spawn failed.");
                int workplace=world.HouseAt(b.BoxX,b.BoxY);
                if(center){if(!world.TryAssignDistributionCourier(i,workplace) || !world.TrySetCourierSchedule(i,7,12))throw new InvalidOperationException("Pack courier configuration failed.");}
                else if(!world.TryAssignClerk(i,workplace) || !world.TrySetClerkSchedule(i,7,12))throw new InvalidOperationException("Pack clerk configuration failed.");
            }
            for(int i=jobs.Count;i<homes.Count;i++){var h=homes[i];if(!world.TrySpawn(Species.Human,h.SpawnX,h.SpawnY,out id) || !world.TryAssignHouse(i,h.SpawnX,h.SpawnY))throw new InvalidOperationException("Pack resident home/spawn failed.");}
            // Use the existing pair-balanced needs policy and three-meal personal budget; templates do not manufacture supplies.
            SaveDocument saved=world.Capture();uint needs=seed^0x9E3779B9u;int pairHunger=10,pairFood=1;
            for(int i=0;i<saved.actors.Length;i++){
                var actor=saved.actors[i];actor.hunger=10;actor.carriedFood=1;
                if(i%2==1 || i+1<saved.actors.Length){if(i%2==0){needs=unchecked(needs*1664525u+1013904223u);pairHunger=(int)(needs%21);pairFood=(int)((needs>>16)%3);}actor.hunger=i%2==0?pairHunger:20-pairHunger;actor.carriedFood=i%2==0?pairFood:2-pairFood;}
                foreach(var box in saved.map.storageBoxes)if(world.HouseAt(box.x,box.y)==actor.assignedHouseId)box.items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=3-actor.carriedFood}};
                actor.intelligence=8;actor.shelterId=world.HouseAt(actor.x,actor.y);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;saved.actors[i]=actor;
            }
            World prepared=World.Restore(saved);prepared.TryFamiliarizeResidentsWithShops();return prepared;
        }
    }
}
