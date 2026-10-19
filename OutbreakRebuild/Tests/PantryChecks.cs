using Outbreak.Contracts;
using Outbreak.Core;
public static partial class Program
{
    private static void CheckPantryCases()
    {
        Check("Pantry trips route inside the room, conserve food and resume exactly", () => {
            World w=EncounterScenarios.PantryTripTest();w.Step();Require(w.ActorAt(0).y<7 && w.ActorAt(0).carriedFood==3,"Human did not start walking before depositing.");World copy=World.Restore(w.Capture());for(int i=0;i<49;i++){w.Step();copy.Step();ActorState a=w.ActorAt(0);Require(a.x>=2 && a.x<=4 && a.y>=3 && a.y<=7,"Pantry route left the room.");}Require(w.ActorAt(0).carriedFood==1 && w.ShelfSites()[0].food==2,"Pantry trip failed to deposit conserved spare meals.");Require(State(w)==State(copy),"Mid-trip restore diverged.");
        });
        Check("Pantry trips stop for hunger and old rules keep adjacent-only storage", () => {
            World w=EncounterScenarios.PantryTripTest();w.Step();SaveDocument s=w.Capture();s.actors[0].hunger=80;w=World.Restore(s);for(int i=0;i<5;i++)w.Step();Require(w.ActorAt(0).carriedFood==2 && w.ShelfSites()[0].food==0,"Hungry traveler did not eat before continuing storage.");s=EncounterScenarios.PantryTripTest().Capture();s.rulesVersion=EncounterRules.PantryVersion;w=World.Restore(s);for(int i=0;i<20;i++)w.Step();Require(!w.PantryTrips && w.PantryStorage && w.ActorAt(0).carriedFood==3 && w.ShelfSites()[0].food==0,"Old save silently gained pantry routing.");
        });
        Check("Recent warnings interrupt pantry work and disconnected shelves cannot pull humans outside", () => {
            SaveDocument s=EncounterScenarios.PantryTripTest().Capture();s.actors[0].heardWarningX=2;s.actors[0].heardWarningY=7;s.actors[0].heardWarningUntilTick=20;World w=World.Restore(s);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ShelfSites()[0].food==0 && w.ActorAt(0).y==7,"Recent danger did not interrupt pantry routing.");s=EncounterScenarios.PantryTripTest().Capture();WorldMap map=WorldMap.Restore(s.map);for(int x=2;x<=4;x++)map.SetShelf(x,5,0);s.map=map.Capture();w=World.Restore(s);for(int i=0;i<30;i++)w.Step();Require(w.ActorAt(0).y>=6,"Pantry route crossed solid furniture or left the shelter.");
        });
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
