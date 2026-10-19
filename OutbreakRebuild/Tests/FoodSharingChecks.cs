using System; // Build detached eligibility and deterministic tie controls.
using System.IO; // Read independently executed pre-sharing traces.
using Outbreak.Contracts; // Inspect actual inventories and separate eating actions.
using Outbreak.Core; // Execute real human opportunities and line-of-sight checks.
using Outbreak.Persistence; // Compare complete saved continuation.
public static partial class Program
{
    private static void CheckFoodSharingCases()
    {
        Check("A safe supplied human transfers one real meal and the recipient eats separately", () => {
            World w=EncounterScenarios.FoodSharingTest();uint random=w.Capture().randomState;w.Step();Require(w.FoodSharing && w.ActorAt(0).carriedFood==2 && w.ActorAt(1).carriedFood==1 && w.ActorAt(1).hunger==80,"Transfer created food, fed instantly or used the wrong action.");Require(w.ActorAt(0).x==2 && w.ActorAt(0).y==2 && w.Capture().randomState==random,"Sharing moved its donor or consumed random draws.");w.Step();Require(w.ActorAt(1).carriedFood==0 && w.ActorAt(1).hunger==30 && w.ActorAt(0).carriedFood==2,"Recipient failed to spend its own opportunity eating one shared meal.");Run(w,98);Require(w.ActorAt(0).shelterState==ShelterState.Inside && w.ActorAt(1).shelterState==ShelterState.Inside,"Helped occupant still made an unnecessary supply trip.");Inspect(w);
        });
        Check("Donors keep a reserve meal and satisfy their own hunger first", () => {
            SaveDocument s=EncounterScenarios.FoodSharingTest().Capture();s.actors[0].carriedFood=1;World w=World.Restore(s);w.Step();Require(w.ActorAt(0).carriedFood==1 && w.ActorAt(1).carriedFood==0,"Donor gave away its last meal.");s=EncounterScenarios.FoodSharingTest().Capture();s.actors[0].hunger=80;w=World.Restore(s);w.Step();Require(w.ActorAt(0).hunger==30 && w.ActorAt(0).carriedFood==2 && w.ActorAt(1).carriedFood==0,"Hungry donor shared before eating.");
        });
        Check("Sharing chooses the hungriest adjacent human and stable identity breaks ties", () => {
            World w=EncounterScenarios.FoodSharingTest();int id;w.TrySpawn(Species.Human,3,2,out id);SaveDocument s=w.Capture();s.actors[1].hunger=60;s.actors[2].hunger=90;w=World.Restore(s);w.Step();Require(w.ActorAt(1).carriedFood==0 && w.ActorAt(2).carriedFood==1 && w.ActorAt(0).carriedFood==2,"Sharing did not prioritize actual greater need.");s.actors[1].hunger=80;s.actors[2].hunger=80;w=World.Restore(s);w.Step();Require(w.ActorAt(1).carriedFood==1 && w.ActorAt(2).carriedFood==0,"Equal-need selection ignored stable actor identity.");
        });
        Check("People who already carry food or are not hungry do not receive spare meals", () => {
            SaveDocument s=EncounterScenarios.FoodSharingTest().Capture();s.actors[1].carriedFood=1;World w=World.Restore(s);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(1).carriedFood==1,"Recipient hoarded more food before eating its own meal.");s=EncounterScenarios.FoodSharingTest().Capture();s.actors[1].hunger=39;w=World.Restore(s);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(1).carriedFood==0,"Fed neighbor triggered an unnecessary gift.");
        });
        Check("Infection, visible zombies and recent danger cues prevent sharing", () => {
            for(int target=0;target<2;target++){SaveDocument s=EncounterScenarios.FoodSharingTest().Capture();s.actors[target].turnAtTick=65;World w=World.Restore(s);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(1).carriedFood==0,"Infection did not preserve survival priority.");s=EncounterScenarios.FoodSharingTest().Capture();s.actors[target].heardWarningX=6;s.actors[target].heardWarningY=2;s.actors[target].heardWarningUntilTick=25;w=World.Restore(s);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(1).carriedFood==0,"Recent heard danger failed to postpone sharing.");}
            World danger=EncounterScenarios.FoodSharingTest();int id;danger.TrySpawn(Species.Zombie,3,3,out id);danger.Step();Require(danger.ActorAt(0).carriedFood==3 && danger.ActorAt(1).carriedFood==0,"A visible zombie failed to interrupt help.");
        });
        Check("Remote people and danger visible only to the receiver do not cause unsafe transfers", () => {
            SaveDocument s=EncounterScenarios.FoodSharingTest().Capture();s.actors[0].x=3;s.actors[1].x=5;s.actors[1].y=2;s.actors[1].shelterId=0;s.actors[1].shelterX=0;s.actors[1].shelterY=0;s.actors[1].shelterState=ShelterState.None;World w=World.Restore(s);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(1).carriedFood==0,"Food passed through distance or a closed door.");
            s=EncounterScenarios.FoodSharingTest().Capture();WorldMap m=WorldMap.Restore(s.map);m.Set(4,3,Terrain.DoorOpen);s.map=m.Capture();w=World.Restore(s);int id;w.TrySpawn(Species.Zombie,5,3,out id);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(1).carriedFood==0,"Donor ignored a threat visible from the receiver's position.");
        });
        Check("Receiving a meal cancels an impending food trip before the recipient opens its exit", () => {
            SaveDocument s=EncounterScenarios.FoodSharingTest().Capture();s.rulesVersion=EncounterRules.HungerVersion;World w=World.Restore(s);Run(w,25);Require(w.ActorAt(1).shelterCalmSinceTick==2 && w.ActorAt(1).carriedFood==0,"Control recipient was not preparing a hungry exit.");s=w.Capture();s.rulesVersion=EncounterRules.Version;w=World.Restore(s);w.Step();Require(w.ActorAt(1).carriedFood==1 && w.ActorAt(1).shelterCalmSinceTick==0 && w.ActorAt(1).shelterExitPhase==ShelterExitPhase.None,"Receiving food retained a stale departure.");w.Step();Require(w.ActorAt(1).hunger==30 && w.TerrainAt(4,2)==Terrain.DoorClosed,"Recipient opened its exit instead of eating the shared meal.");
        });
        Check("Food sharing pauses and saves exactly without losing inventory conservation", () => {
            World w=EncounterScenarios.FoodSharingTest();w.Step();string before=State(w);new TickClock().Advance(100,w.Step);Require(State(w)==before,"Paused transfer advanced recipient eating or hunger.");World resumed=Documents.ParseSave(before);for(int i=0;i<150;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Sharing or later needs diverged after save/load.");}Require(w.ActorAt(0).carriedFood+w.ActorAt(1).carriedFood==2,"Continuation created or lost uneaten meals.");Inspect(w);
        });
        Check("Prototype-eighteen saves retain independent hunger behavior without sharing", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v18-input.json")));Require(w.HungerEnabled && !w.FoodSharing,"Earlier run silently enabled food sharing.");Run(w,100);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v18-expected.json")))),"Previous hunger and separate inventories changed.");
        });
    }
}
