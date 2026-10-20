using Outbreak.Contracts; // Define reproducible scenarios with real portable actor records.
namespace Outbreak.Core // Share the exact obstacle fixture between the shell and acceptance checks.
{ // Begin encounter scenario construction.
    public static class EncounterScenarios // Keep fixture creation separate from simulation decisions.
    { // Begin controlled scenarios.
        public static World CornerSearch() // Demonstrate losing sight at a right-angle corridor and searching the last seen cell.
        { // Begin authored corner fixture.
            WorldMap map = new WorldMap(7, 8, "Corner pursuit test", "corner-search-1"); // Keep the entire scenario visible at the minimum camera zoom.
            for (int y = 0; y < map.Height; y++) for (int x = 0; x < map.Width; x++) map.Set(x, y, Terrain.Wall); // Begin with a solid boundary rather than invisible escape routes.
            for (int x = 1; x <= 5; x++) map.Set(x, 3, Terrain.Floor); for (int y = 3; y <= 6; y++) map.Set(5, y, Terrain.Floor); // Carve the two legs of the right-angle corridor.
            World world = new World(map, 214, 1000, true, humanReactions: false); int id; world.TrySpawn(Species.Zombie, 2, 3, out id); world.TrySpawn(Species.Human, 4, 3, out id); return world; // Stable IDs stagger pursuit and fleeing so the human breaks sight at tick seven.
        } // Finish reproducible scenario setup.
        public static World HumanResponse(HumanReaction reaction, bool group = false) // Isolate the three courage outcomes with reviewable natural SPECIAL values.
        { // Begin a controlled human scenario.
            if (reaction != HumanReaction.Fight && reaction != HumanReaction.Freeze && reaction != HumanReaction.Flight) throw new System.ArgumentException("Choose a courage response fixture."); // Reject labels that do not define a controlled threshold case.
            WorldMap map = new WorldMap(14, 10, group ? "Group support test" : reaction + " reaction test", "human-response-1"); // Keep the scenario compact and named in saved maps.
            for (int y = 0; y < map.Height; y++) for (int x = 0; x < map.Width; x++) if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1) map.Set(x, y, Terrain.Wall); // Bound movement with visible terrain.
            World world = new World(map, 214, 1000, true, humanWarnings: false); int id; world.TrySpawn(Species.Human, 5, 5, out id); if (group) { world.TrySpawn(Species.Human, 4, 4, out id); world.TrySpawn(Species.Human, 4, 6, out id); } world.TrySpawn(Species.Zombie, 9, 5, out id); // Observe a single threat from four cells away before contact.
            SaveDocument saved = world.Capture(); if (reaction == HumanReaction.Fight && !group) saved.actors[0].stress = 41; for (int i = 0; i < saved.actors.Length - 1; i++) { int stat = i > 0 || reaction == HumanReaction.Fight ? 5 : reaction == HumanReaction.Flight ? 1 : 3; saved.actors[i].strength = stat; saved.actors[i].perception = stat; saved.actors[i].charisma = stat; saved.actors[i].bravery = Rules.Bravery(stat, stat, stat); } return World.Restore(saved); // Derive bravery from explicit stats instead of hardcoding a displayed trait.
        } // Finish reaction scenario creation.
        public static World BittenEscape() // Demonstrate infection overriding a brave human's approach decision.
        { // Begin local cover scenario.
            WorldMap map = new WorldMap(11, 9, "Bitten escape test", "bitten-escape-1"); for (int y = 0; y < map.Height; y++) for (int x = 0; x < map.Width; x++) if (x == 0 || y == 0 || x == 10 || y == 8) map.Set(x, y, Terrain.Wall); map.Set(6, 2, Terrain.Wall); map.Set(6, 3, Terrain.Wall); // Place a short wall that can be reached without approaching the zombie.
            World world = new World(map, 214, 1000, true); int id; world.TrySpawn(Species.Human, 5, 4, out id); world.TrySpawn(Species.Zombie, 8, 4, out id); SaveDocument saved = world.Capture(); saved.actors[0].bravery = 3; saved.actors[0].stress = 50; saved.actors[0].turnAtTick = 80; return World.Restore(saved); // A brave, already-bitten human should escape instead of confronting the threat.
        } // Finish reproducible infected scenario.
        private static void AddShelterRoom(WorldMap map, int left, int top, bool northDoor) // Author visible floor and structural boundaries rather than invisible room metadata.
        { // Begin a small rectangular shelter.
            for (int y = top; y <= top + 6; y++) for (int x = left; x <= left + 4; x++) map.Set(x, y, x == left || x == left + 4 || y == top || y == top + 6 ? Terrain.Wall : Terrain.Floor); map.Set(northDoor ? left + 2 : left + 4, northDoor ? top : top + 3, Terrain.DoorClosed); // Give the room a real traversable door once a human opens it.
        } // Finish room authoring.
        public static WorldMap MixedShelterMap() // Add actual room choices to the ordinary mixed-population encounter.
        { // Begin the revised default arena.
            WorldMap map = new WorldMap(20, 16, "Mixed shelter arena", "mixed-shelter-1"); for (int y = 0; y < 16; y++) for (int x = 0; x < 20; x++) if (x == 0 || y == 0 || x == 19 || y == 15) map.Set(x, y, Terrain.Wall); AddShelterRoom(map, 1, 9, false); AddShelterRoom(map, 13, 1, false); map.Set(17, 4, Terrain.Wall); map.Set(13, 4, Terrain.DoorClosed); return map; // Place entrances toward the outdoor arena without overlapping the existing spawn fixture.
        } // Finish mixed room authoring.
        public static WorldMap ShelterDistrictMap() // Provide room choices and outdoor spawn space for larger controlled populations.
        { // Begin an authored hundred-cell district, not a procedural town generator.
            WorldMap map = new WorldMap(100, 100, "Shelter district", "shelter-district-1"); for (int y = 0; y < 100; y++) for (int x = 0; x < 100; x++) { if (x == 0 || y == 0 || x == 99 || y == 99) map.Set(x, y, Terrain.Wall); else if (x == 49 || x == 50) map.Set(x, y, Terrain.Road); } foreach (int top in new[] { 24, 36, 48, 66 }) { AddShelterRoom(map, 28, top, false); AddShelterRoom(map, 70, top, false); } foreach (int left in new[] { 42, 56 }) { AddShelterRoom(map, left, 24, false); AddShelterRoom(map, left, 66, false); } return map; // Keep spawn strips outside room boundaries and close enough for an initial encounter.
        } // Finish district authoring.
        public static World ShelterEntry() // Observe a frightened human choosing and entering a nearby enclosed room.
        { // Begin entry scenario setup.
            WorldMap map = new WorldMap(16, 12, "Shelter entry test", "shelter-entry-1"); AddShelterRoom(map, 1, 2, false); World world = new World(map, 214, 1000, true, doorBreaking: false); int id; world.TrySpawn(Species.Human, 7, 5, out id); world.TrySpawn(Species.Zombie, 11, 5, out id); SaveDocument saved = world.Capture(); saved.actors[0].bravery = -3; saved.actors[0].stress = 50; saved.actors[0].reaction = HumanReaction.Flight; return World.Restore(saved); // Keep this entry/pursuit diagnostic on prior door durability rules; the breach diagnostic exercises damage.
        } // Finish entry fixture.
        public static World ShelterRecovery(bool guarded = false) // Demonstrate calm recovery followed by a real look through the doorway.
        { // Begin a repeatable clear or guarded exit.
            WorldMap map = new WorldMap(16, 12, guarded ? "Guarded shelter exit" : "Shelter recovery", guarded ? "shelter-guarded-1" : "shelter-recovery-1"); AddShelterRoom(map, 1, 2, false); World world = new World(map, 214, 1000, true, zombieWandering: !guarded); int id; world.TrySpawn(Species.Human, 2, 3, out id); if (guarded) world.TrySpawn(Species.Zombie, 6, 5, out id); SaveDocument saved = world.Capture(); saved.actors[0].bravery = -3; saved.actors[0].stress = 10; saved.actors[0].reaction = HumanReaction.Aware; saved.actors[0].shelterId = 3 * map.Width + 3; saved.actors[0].shelterX = 2; saved.actors[0].shelterY = 3; saved.actors[0].shelterState = ShelterState.Inside; return World.Restore(saved); // The closed door conceals the optional guard; that controlled guard retains stationary idle rules to isolate exit inspection.
        } // Finish recovery fixture.
        public static World ZombieWandering() // Observe a remembered doorway followed by chance-based idle movement.
        { // Begin the fading-sighting fixture.
            SaveDocument saved = ShelterRecovery(true).Capture(); saved.rulesVersion = EncounterRules.RepairVersion; saved.actors[0].stress = 100; saved.actors[1].targetId = saved.actors[0].id; saved.actors[1].lastSeenX = 5; saved.actors[1].lastSeenY = 5; saved.actors[1].searchUntilTick = 15; return World.Restore(saved); // Keep the hidden human sheltered while the zombie forgets the doorway after three simulation seconds.
        } // Finish idle wandering fixture.
        public static World ShelfReportTest() // A fed witness points a hungry neighbor toward supplies now hidden by a door.
        {
            SaveDocument saved=ShelfMemoryTest().Capture(); saved.rulesVersion=EncounterRules.NoiseVersion; saved.actors[0].hunger=10; saved.actors[0].carriedFood=1; World world=World.Restore(saved); int id; world.TrySpawn(Species.Human,2,5,out id); saved=world.Capture(); saved.actors[1].hunger=80; saved.actors[1].bravery=3; saved.tick=30; return World.Restore(saved); // The speaker keeps its last meal and shares dated knowledge instead.
        }
        public static World ShelfMemoryTest() // Revisit actual earlier discoveries after the supply door has closed.
        {
            WorldMap map = new WorldMap(16,10,"Shelf memory test","shelf-memory-1"); for (int y=0;y<10;y++) map.Set(5,y,Terrain.Wall); map.Set(5,4,Terrain.DoorOpen); map.SetShelf(2,3,0); map.SetShelf(7,4,6); World world = new World(map,214,encounter:true,hunger:true,carefulSearch:false); int id; world.TrySpawn(Species.Human,2,4,out id); SaveDocument saved=world.Capture(); saved.actors[0].hunger=80; saved.actors[0].bravery=3; world=World.Restore(saved); for(int tick=0;tick<26;tick++)world.Step(); saved=world.Capture(); saved.actors[0].x=2; saved.actors[0].y=4; saved.actors[0].carriedFood=0; saved.actors[0].hunger=80; map=WorldMap.Restore(saved.map); map.Set(5,4,Terrain.DoorClosed); saved.map=map.Capture(); return World.Restore(saved); // The prior search left three meals at 7,4; this paused continuation returns after using its carried supplies.
        }
        public static World AlternateFoodShelterTest()
        {
            WorldMap map=new WorldMap(20,12,"Alternate food shelter test","alternate-food-shelter-1");AddShelterRoom(map,1,2,false);AddShelterRoom(map,10,2,false);map.Set(14,5,Terrain.Wall);map.Set(10,5,Terrain.DoorClosed);map.SetShelf(8,7,6);
            World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,7,5,out id);
            for(int i=0;i<7;i++)world.TrySpawn(Species.Human,2+i%3,3+i/3,out id);
            SaveDocument saved=world.Capture();saved.actors[0].hunger=80;saved.actors[0].stress=80;saved.actors[0].bravery=3;saved.actors[0].homeShelterId=63;saved.actors[0].foodX=8;saved.actors[0].foodY=7;saved.actors[0].foodUntilTick=150;return World.Restore(saved);
        }
        public static World FoodTripRetreatTest()
        {
            SaveDocument saved=HungerForaging(3).Capture();saved.rulesVersion=EncounterRules.Version;ActorState actor=saved.actors[0];actor.x=5;actor.y=2;actor.homeShelterId=actor.shelterId;actor.shelterId=0;actor.shelterX=0;actor.shelterY=0;actor.shelterState=ShelterState.None;actor.stress=80;actor.foodX=6;actor.foodY=2;actor.foodUntilTick=150;actor.shelfSearchProgress=1;saved.actors[0]=actor;saved.map.name="Food trip retreat test";return World.Restore(saved);
        }
        public static World SafeFoodTargetTest()
        {
            WorldMap map=new WorldMap(14,12,"Guarded food target test","safe-food-target-1");map.SetShelf(6,3,6,true);map.SetShelf(3,8,6,true);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,3,out id);world.TrySpawn(Species.Zombie,6,4,out id);SaveDocument saved=world.Capture();saved.actors[0].hunger=100;saved.actors[0].bravery=3;saved.actors[0].shelfMemory=new[]{new ShelfMemoryRecord{x=6,y=3,food=6,observedTick=0},new ShelfMemoryRecord{x=3,y=8,food=6,observedTick=0}};return World.Restore(saved);
        }
        public static World FairFoodPickupTest()
        {
            WorldMap map=new WorldMap(12,9,"Considerate food pickup test","fair-food-pickup-1");map.SetShelf(4,3,3,true);World world=new World(map,214,encounter:true,hunger:true);int id;for(int i=0;i<3;i++)world.TrySpawn(Species.Human,3,3,out id);SaveDocument saved=world.Capture();for(int i=0;i<3;i++){saved.actors[i].hunger=80;saved.actors[i].bravery=3;}return World.Restore(saved);
        }
        public static World FoodExplorationTest()
        {
            WorldMap map=new WorldMap(20,12,"Food exploration test","food-exploration-1");AddShelterRoom(map,1,2,false);AddShelterRoom(map,10,2,false);map.Set(14,5,Terrain.Wall);map.Set(10,5,Terrain.DoorClosed);map.SetShelf(7,7,0,true);map.SetShelf(12,3,6);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,7,5,out id);SaveDocument saved=world.Capture();saved.actors[0].hunger=80;saved.actors[0].bravery=3;return World.Restore(saved);
        }
        public static World FoodYieldChoiceTest()
        {
            WorldMap map=new WorldMap(14,12,"Food yield choices test","food-yield-choice-1");map.SetShelf(5,4,1,true);map.SetShelf(2,8,6,true);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,4,out id);SaveDocument saved=world.Capture();saved.actors[0].hunger=80;saved.actors[0].bravery=3;return World.Restore(saved);
        }
        public static World FoodRouteChoiceTest()
        {
            WorldMap map=new WorldMap(16,12,"Food route choices test","food-route-choice-1");for(int y=0;y<9;y++)map.Set(7,y,Terrain.Wall);map.SetShelf(8,3,6,true);map.SetShelf(3,7,6,true);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,5,3,out id);SaveDocument saved=world.Capture();saved.actors[0].hunger=80;saved.actors[0].bravery=3;saved.actors[0].shelfMemory=new[]{new ShelfMemoryRecord{x=8,y=3,food=6,observedTick=0},new ShelfMemoryRecord{x=3,y=7,food=6,observedTick=0}};return World.Restore(saved);
        }
        public static World DoorMovementNoiseTest(bool opening=false)
        {
            WorldMap map=new WorldMap(14,12,"Door movement noise test","door-movement-noise-1");AddShelterRoom(map,1,2,false);if(!opening)map.Set(5,5,Terrain.DoorOpen);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,opening?6:4,5,out id);world.TrySpawn(Species.Zombie,opening?9:7,opening?5:6,out id);SaveDocument saved=world.Capture();saved.actors[0].shelterId=45;saved.actors[0].shelterX=4;saved.actors[0].shelterY=5;saved.actors[0].shelterState=opening?ShelterState.Seeking:ShelterState.Inside;if(opening){saved.actors[0].reaction=HumanReaction.Flight;saved.actors[0].stress=50;saved.actors[0].bravery=-3;}return World.Restore(saved);
        }
        public static World QuickStockPickupTest()
        {
            SaveDocument saved=CarefulSearchTest().Capture();WorldMap map=WorldMap.Restore(saved.map);map.SetShelf(2,3,6,true);saved.map=map.Capture();saved.map.name="Exposed food pickup test";return World.Restore(saved);
        }
        public static World BlockedFoodRouteTest()
        {
            WorldMap map=new WorldMap(14,10,"Blocked food route test","blocked-food-route-1");for(int y=0;y<10;y++)map.Set(6,y,Terrain.Wall);map.SetShelf(8,3,6,true);map.SetShelf(3,6,6);World world=new World(map,214,encounter:true,hunger:true,foodRouteChoice:false);int id;world.TrySpawn(Species.Human,3,3,out id);SaveDocument saved=world.Capture();saved.actors[0].hunger=80;saved.actors[0].bravery=3;saved.actors[0].shelfMemory=new[]{new ShelfMemoryRecord{x=8,y=3,food=6,observedTick=0}};return World.Restore(saved);
        }
        public static World SearchCoordinationTest()
        {
            WorldMap map=new WorldMap(12,9,"Coordinated shelf search test","search-coordination-1");map.SetShelf(4,3,6);map.SetShelf(4,5,6);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,3,out id);world.TrySpawn(Species.Human,3,3,out id);SaveDocument saved=world.Capture();for(int i=0;i<2;i++){saved.actors[i].hunger=80;saved.actors[i].bravery=3;}return World.Restore(saved);
        }
        public static World CarefulSearchTest()
        {
            SaveDocument saved=HungerForaging(3,true).Capture();saved.rulesVersion=EncounterRules.Version;saved.map.name="Careful shelf search test";return World.Restore(saved);
        }
        public static World SearchNoiseTest()
        {
            WorldMap map=new WorldMap(12,9,"Shelf search noise test","search-noise-1");for(int y=0;y<9;y++)map.Set(5,y,Terrain.Wall);map.Set(5,4,Terrain.DoorClosed);map.SetShelf(3,3,6);World world=new World(map,214,encounter:true,hunger:true,carefulSearch:false);int id;world.TrySpawn(Species.Human,3,4,out id);world.TrySpawn(Species.Zombie,6,5,out id);SaveDocument saved=world.Capture();saved.actors[0].hunger=80;return World.Restore(saved);
        }
        public static World FoodDeliveryTest()
        {
            SaveDocument saved=PantryTripTest().Capture();World world=World.Restore(saved);int id;world.TrySpawn(Species.Human,2,3,out id);saved=world.Capture();saved.actors[1].hunger=80;saved.actors[1].shelterId=45;saved.actors[1].shelterX=2;saved.actors[1].shelterY=3;saved.actors[1].shelterState=ShelterState.Inside;saved.actors[1].stress=20;saved.map.name="Food delivery test";return World.Restore(saved);
        }
        public static World PantryTripTest()
        {
            WorldMap map=new WorldMap(14,12,"Pantry trip test","pantry-trip-1");AddShelterRoom(map,1,2,false);map.SetShelf(4,3,0);World world=new World(map,214,encounter:true,hunger:true,carefulSearch:false);int id;world.TrySpawn(Species.Human,2,7,out id);SaveDocument saved=world.Capture();saved.actors[0].shelterId=45;saved.actors[0].shelterX=2;saved.actors[0].shelterY=7;saved.actors[0].shelterState=ShelterState.Inside;saved.actors[0].carriedFood=3;saved.actors[0].hunger=10;return World.Restore(saved);
        }
        public static World PantryStorageTest()
        {
            SaveDocument saved = FoodSharingTest().Capture(); saved.actors = new[] { saved.actors[0] }; WorldMap map = WorldMap.Restore(saved.map); map.SetShelf(3,2,0); saved.map=map.Capture(); saved.map.name="Pantry storage test"; return World.Restore(saved);
        }
        public static World FoodSharingTest() // A supplied occupant helps a hungry neighbor without a supply trip.
        {
            WorldMap map = new WorldMap(12,8,"Food sharing test","food-sharing-1"); for(int y=1;y<=4;y++)for(int x=1;x<=4;x++)map.Set(x,y,(x==1||x==4||y==1||y==4)?Terrain.Wall:Terrain.Floor);map.Set(4,2,Terrain.DoorClosed);World world=new World(map,214,encounter:true,hunger:true,carefulSearch:false);int id;world.TrySpawn(Species.Human,2,2,out id);world.TrySpawn(Species.Human,2,3,out id);SaveDocument saved=world.Capture();for(int i=0;i<2;i++){saved.actors[i].shelterId=27;saved.actors[i].shelterX=2;saved.actors[i].shelterY=2+i;saved.actors[i].shelterState=ShelterState.Inside;}saved.actors[0].hunger=10;saved.actors[0].carriedFood=3;saved.actors[1].hunger=80;return World.Restore(saved);
        }
        public static World HungerForaging(int bravery = 3, bool stockedRoom = false) // Compare risk tolerance without granting any hidden zombie knowledge.
        { // Begin a portable supply run from an empty room.
            SaveDocument saved = DoorBreach().Capture(); saved.rulesVersion = EncounterRules.ReportVersion; saved.actors = new[] { saved.actors[0] }; saved.actors[0].hunger = 80; saved.actors[0].stress = 35; saved.actors[0].bravery = bravery; saved.actors[0].reaction = HumanReaction.Aware; WorldMap map = WorldMap.Restore(saved.map); map.SetShelf(6, 2, 6); if (stockedRoom) { map.Set(2, 3, Terrain.Floor); map.Set(2, 4, Terrain.Wall); map.SetShelf(2, 3, 6); saved.actors[0].stress = 0; } saved.map = map.Capture(); saved.map.name = "Hunger and shelves test"; saved.map.id = "hunger-shelves-1"; return World.Restore(saved); // Shelf contents remain unsearched until a human spends an action beside them.
        } // Finish hunger diagnostic.
        public static World DoorRepairTest() // A calm occupant restores a damaged, closed door without rebuilding a breach.
        { // Begin a quiet repair fixture.
            SaveDocument saved = DoorBreach().Capture(); saved.rulesVersion = EncounterRules.RepairVersion; saved.actors = new[] { saved.actors[0] }; saved.actors[0].stress = 0; saved.actors[0].reaction = HumanReaction.Calm; WorldMap map = WorldMap.Restore(saved.map); map.SetDoorHealth(3, 2, 40); saved.map = map.Capture(); saved.map.name = "Quiet door repair test"; saved.map.id = "door-repair-1"; return World.Restore(saved); // The human spends its normal opportunities restoring five HP, then resumes calm shelter recovery.
        } // Finish repair fixture.
        public static World WarningShelter() // An outdoor listener seeks cover without personally seeing the zombie.
        { // Begin an actual warning-triggered shelter route.
            WorldMap map = WorldMap.Restore(WarningAttraction().Capture().map); map.Set(3, 2, Terrain.Wall); map.Set(2, 3, Terrain.DoorClosed); World world = new World(map, 214, encounter: true); int id; world.TrySpawn(Species.Human, 6, 2, out id); world.TrySpawn(Species.Zombie, 9, 2, out id); world.TrySpawn(Species.Human, 2, 4, out id); SaveDocument saved = world.Capture(); saved.map.name = "Warning shelter test"; saved.map.id = "warning-shelter-1"; saved.actors[0].bravery = 3; return World.Restore(saved); // Witness #1 warns at tick one; listener #3 hears through the partition and chooses the empty room.
        } // Finish warning shelter fixture.
        public static World DoorBrace() // Compare active physical defense against the unchanged breach diagnostic.
        { // Begin the braced-door scenario.
            SaveDocument saved = DoorBreach().Capture(); saved.rulesVersion = EncounterRules.BraceVersion; saved.map.name = "Door bracing test"; saved.map.id = "door-bracing-1"; return World.Restore(saved); // The human hears the first strike and begins bracing on its next opportunity.
        } // Finish bracing fixture.
        public static World DoorBreach() // Isolate damage, permanent opening and renewed contact in a small dead-end room.
        { // Begin a reproducible recent doorway sighting.
            WorldMap map = new WorldMap(12, 8, "Door breach test", "door-breach-1"); for (int y = 0; y < 8; y++) for (int x = 0; x < 12; x++) if (x == 0 || y == 0 || x == 11 || y == 7 || (x >= 1 && x <= 3 && y >= 1 && y <= 3)) map.Set(x, y, Terrain.Wall); map.Set(2, 2, Terrain.Floor); map.Set(3, 2, Terrain.DoorClosed); World world = new World(map, 214, 1000, true, doorBracing: false); int id; world.TrySpawn(Species.Human, 2, 2, out id); world.TrySpawn(Species.Zombie, 4, 2, out id); SaveDocument saved = world.Capture(); saved.actors[0].bravery = -3; saved.actors[0].stress = 100; saved.actors[0].shelterId = 27; saved.actors[0].shelterX = 2; saved.actors[0].shelterY = 2; saved.actors[0].shelterState = ShelterState.Inside; saved.actors[1].targetId = 1; saved.actors[1].lastSeenX = 3; saved.actors[1].lastSeenY = 2; saved.actors[1].searchUntilTick = EncounterRules.SearchTicks; return World.Restore(saved); // A remembered entry is sufficient to investigate; hidden human coordinates never select the door.
        } // Finish the breach fixture.
        public static World DoorHearing() // Observe a calm listener react to one hidden impact, then recover after quiet.
        { // Begin a reproducible short-lived doorway sighting.
            SaveDocument saved = DoorBreach().Capture(); saved.map.name = "Door hearing test"; saved.map.id = "door-hearing-1"; saved.actors[0].stress = 0; saved.actors[0].reaction = HumanReaction.Calm; saved.actors[1].searchUntilTick = 7; return World.Restore(saved); // One strike at tick two is followed by memory expiry, five seconds of caution and eventual recovery.
        } // Finish impact hearing fixture.
        public static World ZombieSoundInvestigation() // Show an idle zombie approaching another zombie's actual impacts.
        { // Begin a controlled audible breach.
            World world = DoorBreach(); int id; world.TrySpawn(Species.Zombie, 7, 2, out id); return world; // Zombie #3 hears #2's strikes, approaches the door and acquires the human only after sight opens.
        } // Finish sound investigation fixture.
        public static World WarningAttraction() // An idle zombie hears a witness through a blocking doorway.
        { // Begin a controlled sound-only approach.
            SaveDocument saved = HumanWarning().Capture(); saved.rulesVersion = EncounterRules.RepairVersion; WorldMap map = WorldMap.Restore(saved.map); for (int y = 1; y <= 6; y++) map.Set(4, y, Terrain.Wall); map.Set(4, 4, Terrain.DoorClosed); saved.map = map.Capture(); World world = World.Restore(saved); int id; world.TrySpawn(Species.Zombie, 2, 4, out id); return world; // Zombie #4 can hear the witness at 6,2 but cannot see through the partition or attack from hearing alone.
        } // Finish warning-attraction fixture.
        public static World HumanWarning() // Warn a sheltered listener about a zombie seen only by a nearby witness.
        { // Begin a closed-door communication fixture.
            MapDocument terrain = DoorBreach().Capture().map; terrain.name = "Human warning test"; terrain.id = "human-warning-1"; World world = new World(WorldMap.Restore(terrain), 214, 1000, true, zombieWarningInvestigation: false); int id; world.TrySpawn(Species.Human, 2, 2, out id); world.TrySpawn(Species.Human, 6, 2, out id); world.TrySpawn(Species.Zombie, 9, 2, out id); SaveDocument saved = world.Capture(); saved.actors[0].shelterId = 27; saved.actors[0].shelterX = 2; saved.actors[0].shelterY = 2; saved.actors[0].shelterState = ShelterState.Inside; saved.actors[1].bravery = -3; saved.actors[1].stress = 50; saved.actors[1].reaction = HumanReaction.Flight; return World.Restore(saved); // The listener cannot see through the closed door, but can hear the witness before it retreats.
        } // Finish warning fixture.
        public static World UnsafeShelter() // Make a previously selected room visibly unsafe while providing another authored destination.
        { // Begin reassessment scenario setup.
            WorldMap map = new WorldMap(16, 18, "Unsafe shelter test", "unsafe-shelter-1"); AddShelterRoom(map, 1, 2, false); AddShelterRoom(map, 7, 10, true); World world = new World(map, 214, 1000, true); int id; world.TrySpawn(Species.Human, 4, 7, out id); world.TrySpawn(Species.Zombie, 2, 3, out id); SaveDocument saved = world.Capture(); saved.actors[0].bravery = -3; saved.actors[0].reaction = HumanReaction.Flight; saved.actors[0].stress = 50; saved.actors[0].shelterId = 3 * map.Width + 3; saved.actors[0].shelterX = 2; saved.actors[0].shelterY = 3; saved.actors[0].shelterState = ShelterState.Inside; return World.Restore(saved); // The room identity comes from its first row-major floor cell plus one.
        } // Finish unsafe-room fixture.
    } // Finish scenario construction.
} // Finish engine-independent fixtures.
