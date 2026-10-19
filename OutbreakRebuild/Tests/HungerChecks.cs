using System; // Compare personal observations and reject malformed inventory.
using System.IO; // Read independently recorded pre-hunger continuations.
using Outbreak.Contracts; // Inspect finite shelves, inventory and saved needs.
using Outbreak.Core; // Execute actual searching, eating, travel and risk decisions.
using Outbreak.Persistence; // Verify complete saves, maps and continuation.
public static partial class Program
{
    private static void CheckHungerCases()
    {
        Check("Hunger advances on exact simulation ticks, pauses and caps at one hundred", () => {
            SaveDocument s=EncounterScenarios.HungerForaging().Capture();s.actors[0].hunger=0;s.actors[0].stress=0;World w=World.Restore(s);Run(w,29);Require(w.ActorAt(0).hunger==0 && w.ActorAt(0).shelterState==ShelterState.Inside,"Fed occupant left or hunger advanced early.");string before=State(w);new TickClock().Advance(100,w.Step);Require(State(w)==before,"Paused hunger advanced.");w.Step();Require(w.ActorAt(0).hunger==1,"Hunger missed its exact thirtieth tick.");s=w.Capture();s.actors[0].hunger=100;w=World.Restore(s);Run(w,30);Require(w.ActorAt(0).hunger==100,"Hunger exceeded its cap.");
        });
        Check("A stocked room supplies inventory through a search action followed by a separate meal", () => {
            World w=EncounterScenarios.HungerForaging(3,true);w.Step();Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(0).hunger==80 && w.ShelfAt(2,3).food==3 && w.ShelfAt(2,3).searched,"Searching failed finite transfer or ate in the same action.");Run(w,5);Require(w.ActorAt(0).carriedFood==2 && w.ActorAt(0).hunger==30 && w.ActorAt(0).shelterState==ShelterState.Inside,"Eating did not consume exactly one carried meal without leaving.");Run(w,100);Require(w.ActorAt(0).shelterState==ShelterState.Inside && w.ActorAt(0).carriedFood==2,"Fed and supplied occupant left without a need.");Inspect(w);
        });
        Check("An empty-room forager leaves, searches, eats and returns with spare meals", () => {
            World w=EncounterScenarios.HungerForaging(3);Run(w,50);Require(w.ActorAt(0).shelterId==0 && w.ActorAt(0).homeShelterId==27 && w.ActorAt(0).carriedFood==3 && w.ShelfAt(6,2).food==3 && w.ActorAt(0).targetId==0,"Hungry supply trip did not collect portable food without invented opponents.");Run(w,50);Require(w.ActorAt(0).shelterState==ShelterState.Inside && w.ActorAt(0).carriedFood==2 && w.ActorAt(0).hunger<40 && w.ActorAt(0).homeShelterId==0 && w.TerrainAt(3,2)==Terrain.DoorClosed,"Forager failed to eat, return home or close behind itself.");Inspect(w);
        });
        Check("Bravery and increasing hunger independently raise accepted food-trip risk", () => {
            World brave=EncounterScenarios.HungerForaging(3),timid=EncounterScenarios.HungerForaging(-3);Run(brave,40);Run(timid,40);Require(brave.ActorAt(0).shelterId==0 && timid.ActorAt(0).shelterState==ShelterState.Inside,"Bravery did not change willingness to leave under equal stress and hunger.");Run(timid,40);Require(timid.ActorAt(0).shelterId==0,"A timid human never reconsidered after stress fell and hunger grew.");
            SaveDocument low=EncounterScenarios.HungerForaging(0).Capture();low.actors[0].hunger=40;World a=World.Restore(low);low.actors[0].hunger=90;World b=World.Restore(low);Run(a,40);Run(b,40);Require(a.ActorAt(0).shelterState==ShelterState.Inside && b.ActorAt(0).shelterId==0,"Increasing hunger did not increase accepted stress at fixed bravery.");
        });
        Check("Close visible zombies override hunger and carried meals", () => {
            SaveDocument s=EncounterScenarios.HungerForaging(3).Capture();s.actors[0].hunger=100;s.actors[0].carriedFood=1;s.actors[0].stress=0;WorldMap m=WorldMap.Restore(s.map);m.Set(3,2,Terrain.DoorOpen);s.map=m.Capture();World w=World.Restore(s);int id;w.TrySpawn(Species.Zombie,4,2,out id);Require(w.FoodTripRisk(0)==1000,"Visible close contact was not classified as immediate danger.");w.Step();Require(w.ActorAt(0).carriedFood==1 && w.ActorAt(0).hunger==100 && w.TerrainAt(3,2)==Terrain.DoorClosed,"Desperate human ate or stepped into immediate danger instead of defending shelter.");
        });
        Check("Hunger does not abandon a brace when a food trip is still too dangerous", () => {
            SaveDocument s=EncounterScenarios.DoorBrace().Capture();s.rulesVersion=EncounterRules.Version;s.actors[0].hunger=80;World w=World.Restore(s);Run(w,7);Require(w.ActorAt(0).bracedDoor==28 && w.DoorHealthAt(3,2)==76 && w.ActorAt(0).shelterState==ShelterState.Inside,"Unacceptable trip risk made a hungry occupant abandon physical defense.");
        });
        Check("Unopened shelf contents do not affect decisions until the actual search", () => {
            World full=EncounterScenarios.HungerForaging(3);SaveDocument s=full.Capture();s.map.shelves[0].food=0;World empty=World.Restore(s);for(int i=0;i<45;i++){full.Step();empty.Step();Require(Documents.Encode(full.ActorAt(0))==Documents.Encode(empty.ActorAt(0)) && full.Capture().randomState==empty.Capture().randomState,"Human read unopened shelf contents from hidden world state.");}full.Step();empty.Step();Require(full.ActorAt(0).carriedFood==3 && empty.ActorAt(0).carriedFood==0 && empty.ShelfAt(6,2).searched && empty.ActorAt(0).foodUntilTick==0,"Searching an empty shelf did not cost the action and reveal emptiness.");
        });
        Check("Competing searchers cannot duplicate stock or exceed inventory capacity", () => {
            WorldMap m=new WorldMap(10,5);m.SetShelf(5,2,4);World w=new World(m,214,encounter:true,hunger:true);int id;w.TrySpawn(Species.Human,4,2,out id);w.TrySpawn(Species.Human,4,2,out id);SaveDocument s=w.Capture();s.actors[0].hunger=80;s.actors[0].bravery=3;s.actors[1].hunger=80;s.actors[1].bravery=3;w=World.Restore(s);Run(w,2);Require(w.ActorAt(0).carriedFood==3 && w.ActorAt(1).carriedFood==1 && w.ShelfAt(5,2).food==0,"Shelf search duplicated food or exceeded three meals.");Run(w,5);Require(w.ActorAt(0).carriedFood==2 && w.ActorAt(1).carriedFood==0 && w.ShelfAt(5,2).food==0,"Meals replenished a shelf or consumed the wrong stock.");
        });
        Check("Solid shelves block placement and hidden shelves do not reveal targets", () => {
            WorldMap m=new WorldMap(12,8);m.SetShelf(8,2,6);for(int y=0;y<8;y++)m.Set(5,y,Terrain.Wall);World w=new World(m,214,encounter:true,hunger:true);int id;Require(!w.TrySpawn(Species.Human,8,2,out id),"Human spawned inside solid furniture.");w.TrySpawn(Species.Human,2,2,out id);SaveDocument s=w.Capture();s.actors[0].hunger=80;w=World.Restore(s);w.Step();Require(w.ActorAt(0).foodUntilTick==0 && !w.ShelfAt(8,2).searched,"An unseen shelf leaked its location through a wall.");Require(!w.TryPlaceShelf(w.ActorAt(0).x,w.ActorAt(0).y,6),"Shelf overwrote a body.");
        });
        Check("Shelf maps preserve search state, finite stock, metadata and defensive ownership", () => {
            WorldMap m=new WorldMap(10,5,"Food map","food-1","retained payload");m.SetShelf(5,2,6,true);MapDocument s=m.Capture();Require(s.schemaVersion==3 && s.shelves.Length==1 && s.doors==null,"Shelf map did not use the new explicit format.");s.shelves[0].food=99;Require(m.ShelfAt(5,2).food==6,"Captured shelf mutated live storage.");WorldMap copy=Documents.ParseMap(Documents.Encode(m.Capture()));Require(copy.ShelfAt(5,2).food==6 && copy.ShelfAt(5,2).searched && copy.AuthoringJson=="retained payload" && !copy.CanWalk(5,2),"Shelf map round-trip lost stock, search, metadata or collision.");s=m.Capture();s.schemaVersion=2;Reject(()=>WorldMap.Restore(s));s=m.Capture();s.shelves[0].food=-1;Reject(()=>WorldMap.Restore(s));s=m.Capture();s.shelves[0].food=1001;Reject(()=>WorldMap.Restore(s));s=m.Capture();s.shelves=new[]{s.shelves[0],s.shelves[0]};Reject(()=>WorldMap.Restore(s));s=m.Capture();s.shelves[0].x=10;Reject(()=>WorldMap.Restore(s));
        });
        Check("Need, inventory and shelf memory validate and conversion clears human possessions", () => {
            World w=EncounterScenarios.HungerForaging(3,true);w.Step();SaveDocument s=w.Capture();s.actors[0].hunger=101;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].carriedFood=4;Reject(()=>World.Restore(s));s=w.Capture();s.rulesVersion=EncounterRules.RepairVersion;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].foodX=6;s.actors[0].foodY=2;s.actors[0].foodUntilTick=s.tick+151;Reject(()=>World.Restore(s));s=w.Capture();s.actors[0].homeShelterId=999;Reject(()=>World.Restore(s));
            s=w.Capture();s.actors[0].turnAtTick=s.tick+1;w=World.Restore(s);w.Step();Require(w.ActorAt(0).species==Species.Zombie && w.ActorAt(0).hunger==0 && w.ActorAt(0).carriedFood==0 && w.ActorAt(0).foodUntilTick==0 && w.ActorAt(0).homeShelterId==0,"Turning retained human needs, inventory or remembered supply trip.");Inspect(w);
        });
        Check("Shelf memory expires exactly and food trips pause and continue through saves", () => {
            World w=EncounterScenarios.HungerForaging();Run(w,41);string before=State(w);new TickClock().Advance(100,w.Step);Require(State(w)==before,"Paused food trip consumed hunger or searched supplies.");World resumed=Documents.ParseSave(before);for(int i=0;i<150;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Food, stock or return trip diverged after loading.");}Inspect(w);
            w=EncounterScenarios.HungerForaging();Run(w,41);SaveDocument s=w.Capture();s.actors[0].foodUntilTick=43;w=World.Restore(s);w.Step();Require(w.ActorAt(0).foodUntilTick==43,"Shelf memory expired early.");w.Step();Require(w.ActorAt(0).foodUntilTick==0 && w.ActorAt(0).foodX==0 && w.ActorAt(0).foodY==0,"Shelf memory did not clear between human opportunities.");
        });
        Check("Configured encounters supply shelves and hunger remains deterministic across a mixed run", () => {
            World w=ScenarioBuilder.Create(ScenarioMap.ShelterDistrict,100,10);Require(w.HungerEnabled && w.ShelfSites().Length==7,"Configured map omitted needs or authored shelf opportunities.");Run(w,1250);World resumed=Documents.ParseSave(State(w));for(int i=0;i<50;i++){w.Step();resumed.Step();Require(State(w)==State(resumed),"Mixed hunger encounter diverged after continuation.");}Inspect(w);
        });
        Check("Prototype-seventeen saves retain independently recorded pre-hunger repair behavior", () => {
            string folder=Path.Combine(AppContext.BaseDirectory,"Fixtures");World w=Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v17-input.json")));Require(w.DoorRepair && !w.HungerEnabled,"Previous save silently gained needs or inventory behavior.");Run(w,100);Require(State(w)==State(Documents.ParseSave(File.ReadAllText(Path.Combine(folder,"encounter-v17-expected.json")))),"Previous quiet repair continuation changed.");
        });
    }
}
