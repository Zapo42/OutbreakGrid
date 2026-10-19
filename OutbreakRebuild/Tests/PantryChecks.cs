using Outbreak.Contracts;
using Outbreak.Core;
public static partial class Program
{
    private static void CheckPantryCases()
    {
        Check("Pantry deposits conserve meals and retain a personal reserve", () => {
            World w=EncounterScenarios.PantryStorageTest();w.Step();Require(w.ActorAt(0).carriedFood==1,"Reserve was not retained.");ShelfDocument shelf=w.ShelfSites()[0];Require(shelf.food==2 && shelf.searched,"Shelf did not receive actual spare meals.");Require(w.ActorAt(0).shelfMemory[0].food==2 && w.ActorAt(0).shelfMemory[0].reportedBy==0,"Deposit was not remembered directly.");World copy=World.Restore(w.Capture());for(int i=0;i<100;i++){w.Step();copy.Step();}Require(State(w)==State(copy),"Pantry continuation diverged after restore.");
        });
        Check("Hungry or infected occupants keep their food; older saves preserve behavior", () => {
            SaveDocument s=EncounterScenarios.PantryStorageTest().Capture();s.actors[0].hunger=80;World w=World.Restore(s);w.Step();Require(w.ShelfSites()[0].food==0,"Hungry person deposited instead of eating.");s=EncounterScenarios.PantryStorageTest().Capture();s.actors[0].turnAtTick=100;w=World.Restore(s);w.Step();Require(w.ShelfSites()[0].food==0,"Infected person stocked a pantry.");s=EncounterScenarios.PantryStorageTest().Capture();s.rulesVersion=EncounterRules.ReportVersion;w=World.Restore(s);w.Step();Require(!w.PantryStorage && w.ShelfReports && w.ActorAt(0).carriedFood==3 && w.ShelfSites()[0].food==0,"Prior generation silently gained storage.");
        });
        Check("Pantry capacity limits conserve excess carried food", () => {
            SaveDocument s=EncounterScenarios.PantryStorageTest().Capture();WorldMap map=WorldMap.Restore(s.map);map.SetShelf(3,2,999);s.map=map.Capture();World w=World.Restore(s);w.Step();Require(w.ShelfSites()[0].food==1000 && w.ActorAt(0).carriedFood==2,"Capacity overflow destroyed or invented food.");
        });
    }
}
