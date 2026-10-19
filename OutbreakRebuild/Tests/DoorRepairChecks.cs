using System; // Construct independent saved eligibility controls.
using System.IO; // Read prior-release traces generated before this implementation.
using Outbreak.Contracts; // Inspect portable shelter and map condition state.
using Outbreak.Core; // Execute real repair actions and normal recovery.
using Outbreak.Persistence; // Compare complete deterministic continuation.
public static partial class Program
{
    private static void CheckDoorRepairCases()
    {
        Check("A calm sheltered human repairs five HP per normal opportunity without moving", () => {
            World w=EncounterScenarios.DoorRepairTest();Require(w.DoorRepair && w.DoorHealthAt(3,2)==40,"Repair fixture did not begin damaged.");w.Step();Require(w.DoorHealthAt(3,2)==45 && w.ActorAt(0).x==2 && w.ActorAt(0).y==2 && w.ActorAt(0).shelterCalmSinceTick==0 && w.ActorAt(0).shelterExitPhase==ShelterExitPhase.None,"Repair failed, moved its worker or started departure.");Run(w,9);Require(w.DoorHealthAt(3,2)==50 && w.TerrainRevision==0 && w.DoorConditionRevision==2 && w.ActorAt(0).heardDoorUntilTick==0,"Repair changed collision, used extra actions or emitted an attack sound.");Inspect(w);
        });
        Check("Full repair clamps health, canonicalizes map conditions and permits quiet departure", () => {
            World w=EncounterScenarios.DoorRepairTest();Run(w,60);Require(w.DoorHealthAt(3,2)==100 && w.Capture().map.schemaVersion==1 && w.Capture().map.doors==null && w.DoorConditionRevision==12,"Full repair overshot or retained stale damage records.");Run(w,40);Require(w.ActorAt(0).shelterId==0 && w.DoorHealthAt(3,2)==100,"Completed work blocked normal calm departure.");Inspect(w);
        });
        Check("Broken doors remain broken and open doors must close before repair", () => {
            SaveDocument s=EncounterScenarios.DoorRepairTest().Capture();WorldMap m=WorldMap.Restore(s.map);m.SetDoorHealth(3,2,0);s.map=m.Capture();World w=World.Restore(s);Run(w,50);Require(w.DoorHealthAt(3,2)==0 && w.TerrainAt(3,2)==Terrain.DoorOpen,"Repair rebuilt or closed a broken door.");
            s=EncounterScenarios.DoorRepairTest().Capture();m=WorldMap.Restore(s.map);m.Set(3,2,Terrain.DoorOpen);s.map=m.Capture();w=World.Restore(s);w.Step();Require(w.DoorHealthAt(3,2)==40 && w.TerrainAt(3,2)==Terrain.DoorClosed,"Worker repaired an open door in the same closing opportunity.");Run(w,5);Require(w.DoorHealthAt(3,2)==45,"Safely closed door did not become repairable.");
        });
        Check("Stress, infection, visible zombies and distance prevent repair", () => {
            SaveDocument s=EncounterScenarios.DoorRepairTest().Capture();s.actors[0].stress=10;World w=World.Restore(s);w.Step();Require(w.DoorHealthAt(3,2)==40,"Stressed human repaired a door.");s=EncounterScenarios.DoorRepairTest().Capture();s.actors[0].turnAtTick=65;w=World.Restore(s);w.Step();Require(w.DoorHealthAt(3,2)==40,"Infected human repaired a door.");
            w=EncounterScenarios.DoorRepairTest();int id;w.TrySpawn(Species.Zombie,2,2,out id);w.Step();Require(w.DoorHealthAt(3,2)==40,"A visible zombie failed to interrupt work.");
            s=EncounterScenarios.DoorRepairTest().Capture();WorldMap m=WorldMap.Restore(s.map);m.Set(2,3,Terrain.Floor);m.Set(2,4,Terrain.Wall);s.map=m.Capture();s.actors[0].y=3;s.actors[0].shelterY=3;w=World.Restore(s);w.Step();Require(w.DoorHealthAt(3,2)==40 && w.ActorAt(0).y==3,"Worker repaired from a diagonal or distant cell.");
        });
        Check("Recent warnings and impacts postpone repair until their exact quiet deadline", () => {
            SaveDocument s=EncounterScenarios.DoorRepairTest().Capture();s.actors[0].heardWarningX=4;s.actors[0].heardWarningY=2;s.actors[0].heardWarningUntilTick=25;World w=World.Restore(s);Run(w,24);Require(w.DoorHealthAt(3,2)==40,"Recent warning allowed quiet maintenance.");Run(w,2);Require(w.DoorHealthAt(3,2)==45 && w.ActorAt(0).heardWarningUntilTick==0,"Quiet did not enable repair after warning expiry.");
            s=EncounterScenarios.DoorRepairTest().Capture();s.actors[0].heardDoorX=3;s.actors[0].heardDoorY=2;s.actors[0].heardDoorUntilTick=25;w=World.Restore(s);Run(w,24);Require(w.DoorHealthAt(3,2)==40 && w.DoorIsBraced(3,2),"Human repaired instead of defending a recently attacked door.");Run(w,2);Require(w.DoorHealthAt(3,2)==45 && !w.DoorIsBraced(3,2),"Quiet did not switch bracing to maintenance.");
        });
        Check("Repairs pause and continue exactly after save through full restoration and exit", () => {
            World w=EncounterScenarios.DoorRepairTest();Run(w,10);string before=State(w);TickClock c=new TickClock();c.Advance(100,w.Step);Require(State(w)==before,"Pause advanced repairs.");World resumed=Documents.ParseSave(before);for(int i=0;i<100;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Repair continuation diverged after save/load.");}Inspect(w);
        });
        Check("Prototype-sixteen saves retain independent pre-repair warning shelter behavior", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v16-input.json")));Require(w.WarningSheltering && !w.DoorRepair,"Old run silently enabled repairs.");Run(w,80);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v16-expected.json")))),"Prior warning shelter continuation changed.");
        });
    }
}
