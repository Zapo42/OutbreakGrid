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
        public static World ReanimationEscapeTest()
        {
            WorldMap map=new WorldMap(12,9,"Reanimation escape test","rise-escape-1");for(int y=0;y<9;y++)for(int x=0;x<12;x++)map.Set(x,y,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,5,4,out id);world.TrySpawn(Species.Human,6,4,out id);SaveDocument saved=world.Capture();saved.actors[0].health=0;saved.actors[0].turnAtTick=1;return World.Restore(saved);
        }
        public static World RisingZombieTest()
        {
            WorldMap map=new WorldMap(8,5,"Rising zombie test","rise-1");for(int y=0;y<5;y++)for(int x=0;x<8;x++)map.Set(x,y,Terrain.Wall);map.Set(2,2,Terrain.Grass);map.Set(3,2,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,2,out id);world.TrySpawn(Species.Human,3,2,out id);SaveDocument saved=world.Capture();saved.actors[0].health=0;saved.actors[0].turnAtTick=1;saved.actors[1].reaction=HumanReaction.Freeze;saved.actors[1].freezeUntilTick=10;saved.actors[1].stress=30;return World.Restore(saved);
        }
        public static World ReanimationTest()
        {
            SaveDocument saved=MaulingTest().Capture();saved.actors[0].health=10;saved.actors[0].turnAtTick=20;saved.map.name="Corpse reanimation test";saved.map.id="reanimation-1";return World.Restore(saved);
        }
        public static World MaulingTest()
        {
            SaveDocument saved=CorneredShoveTest().Capture();saved.actors[0].turnAtTick=100;saved.actors[0].health=45;saved.map.name="Trapped infected mauling test";saved.map.id="mauling-1";return World.Restore(saved);
        }
        public static World GrappleEscapeTest()
        {
            WorldMap map=new WorldMap(18,7,"Grapple escape test","grapple-escape-1");for(int y=0;y<7;y++)for(int x=0;x<18;x++)map.Set(x,y,Terrain.Wall);for(int x=1;x<17;x++)map.Set(x,3,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Zombie,7,3,out id);world.TrySpawn(Species.Human,8,3,out id);SaveDocument saved=world.Capture();saved.actors[1].strength=8;saved.actors[1].endurance=8;saved.actors[1].luck=8;saved.actors[1].agility=0;saved.actors[1].bravery=-3;saved.actors[1].stress=60;saved.reactionRandomState=2;return World.Restore(saved);
        }
        public static World ConvertedHealthTest(int health=20)
        {
            SaveDocument saved=EnergyRestTest().Capture();saved.actors[0].health=health;saved.actors[0].turnAtTick=1;saved.map.name="Converted zombie health test";return World.Restore(saved);
        }
        public static World CorneredShoveTest()
        {
            WorldMap map=new WorldMap(10,7,"Cornered defensive shove test","cornered-shove-1");for(int y=0;y<7;y++)for(int x=0;x<10;x++)map.Set(x,y,Terrain.Wall);for(int x=1;x<=8;x++)map.Set(x,3,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,1,3,out id);world.TrySpawn(Species.Zombie,2,3,out id);SaveDocument saved=world.Capture();saved.actors[0].bravery=-3;saved.actors[0].stress=80;return World.Restore(saved);
        }
        public static World SprintNoiseTest()
        {
            WorldMap map=new WorldMap(16,12,"Sprint footsteps test","sprint-noise-1");for(int y=0;y<12;y++)for(int x=0;x<16;x++)map.Set(x,y,Terrain.Wall);for(int x=1;x<15;x++)map.Set(x,4,Terrain.Grass);map.Set(6,5,Terrain.DoorClosed);map.Set(6,6,Terrain.Grass);map.Set(6,7,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,8,4,out id);world.TrySpawn(Species.Zombie,12,4,out id);world.TrySpawn(Species.Zombie,6,7,out id);SaveDocument saved=world.Capture();saved.actors[0].bravery=-3;saved.actors[0].stress=80;return World.Restore(saved);
        }
        public static World EscapeSprintTest()
        {
            WorldMap map=new WorldMap(32,7,"Escape sprint test","escape-sprint-1");for(int y=0;y<7;y++)for(int x=0;x<32;x++)map.Set(x,y,Terrain.Wall);for(int x=1;x<31;x++)map.Set(x,3,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,20,3,out id);world.TrySpawn(Species.Zombie,24,3,out id);SaveDocument saved=world.Capture();saved.actors[0].bravery=-3;saved.actors[0].stress=80;return World.Restore(saved);
        }
        public static World EnergyReturnTest()
        {
            SaveDocument saved=HungerForaging(0).Capture();saved.rulesVersion=EncounterRules.Version;saved.actors[0].x=4;saved.actors[0].homeShelterId=saved.actors[0].shelterId;saved.actors[0].shelterId=0;saved.actors[0].shelterX=0;saved.actors[0].shelterY=0;saved.actors[0].shelterState=ShelterState.None;saved.actors[0].stress=0;saved.actors[0].hunger=50;saved.actors[0].energySpent=82;saved.map.name="Low-energy food return test";saved.map.id="energy-return-1";return World.Restore(saved);
        }
        public static World TiredForagerTest()
        {
            SaveDocument saved=HungerForaging(0).Capture();saved.rulesVersion=EncounterRules.Version;saved.actors[0].energySpent=80;saved.map.name="Tired forager risk test";saved.map.id="energy-risk-1";return World.Restore(saved);
        }
        public static World FoodRestTest()
        {
            SaveDocument saved=RestSpotTest().Capture();saved.map.name="Rest before foraging test";saved.map.id="food-rest-1";saved.actors[0].hunger=50;saved.actors[0].bravery=0;return World.Restore(saved);
        }
        public static World RestSpotTest()
        {
            SaveDocument saved=EnergyRestTest().Capture();WorldMap map=WorldMap.Restore(saved.map);map.SetRestSpot(2,5);saved.map=map.Capture();saved.map.name="Bed rest test";saved.map.id="rest-spot-1";return World.Restore(saved);
        }
        public static World EnergyRestTest()
        {
            SaveDocument saved=RestHealingTest().Capture();saved.map.name="Energy and shelter rest test";saved.map.id="energy-rest-1";saved.actors[0].health=100;saved.actors[0].energySpent=80;return World.Restore(saved);
        }
        public static World ShoveFatigueTest()
        {
            SaveDocument saved=ShoveContactTest().Capture();saved.map.name="Shove fatigue test";saved.map.id="shove-fatigue-1";saved.actors[0].shoveFatigue=2;saved.rulesVersion=EncounterRules.ShoveFatigueVersion;return World.Restore(saved);
        }
        public static World ShoveContactTest()
        {
            SaveDocument saved=UnarmedSpacingTest().Capture();saved.map.name="Shove contact test";saved.map.id="shove-contact-1";saved.actors[3].x=5;return World.Restore(saved);
        }
        public static World UnarmedSpacingTest()
        {
            WorldMap map=new WorldMap(12,7,"Unarmed spacing test","unarmed-spacing-1");for(int y=0;y<7;y++)for(int x=0;x<12;x++)map.Set(x,y,Terrain.Wall);for(int x=1;x<=9;x++)map.Set(x,3,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,4,3,out id);world.TrySpawn(Species.Human,2,3,out id);world.TrySpawn(Species.Human,2,3,out id);world.TrySpawn(Species.Zombie,6,3,out id);SaveDocument saved=world.Capture();for(int i=0;i<3;i++)saved.actors[i].bravery=3;saved.actors[0].stress=45;return World.Restore(saved);
        }
        public static World CombatNoiseTest()
        {
            WorldMap map=new WorldMap(12,9,"Combat noise test","combat-noise-1");for(int y=0;y<9;y++)map.Set(5,y,Terrain.Wall);map.Set(5,4,Terrain.DoorClosed);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,4,out id);world.TrySpawn(Species.Zombie,4,4,out id);world.TrySpawn(Species.Zombie,7,4,out id);SaveDocument saved=world.Capture();saved.actors[0].meleeWeapon=MeleeWeapon.Bat;saved.actors[0].strength=10;saved.actors[0].perception=8;saved.actors[0].agility=8;saved.actors[0].bravery=3;saved.actors[0].bluntSkill=5;saved.actors[1].health=10;saved.reactionRandomState=1;return World.Restore(saved);
        }
        public static World RecoveryRationsTest()
        {
            SaveDocument saved=RestHealingTest().Capture();saved.map.name="Recovery rations test";saved.map.id="recovery-rations-1";WorldMap map=WorldMap.Restore(saved.map);map.SetShelf(3,3,0,true);saved.map=map.Capture();saved.actors[0].health=30;saved.actors[0].hunger=35;saved.actors[0].carriedFood=2;return World.Restore(saved);
        }
        public static World RestHealingTest()
        {
            WorldMap map=new WorldMap(16,12,"Shelter rest healing test","rest-healing-1");AddShelterRoom(map,1,2,false);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,3,out id);SaveDocument saved=world.Capture();saved.actors[0].health=60;saved.actors[0].hunger=10;saved.actors[0].shelterId=51;saved.actors[0].shelterX=2;saved.actors[0].shelterY=3;saved.actors[0].shelterState=ShelterState.Inside;return World.Restore(saved);
        }
        public static World HealthBarsTest()
        {
            World world=new World(new WorldMap(12,10,"Health bars test","health-bars-1"),214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,3,out id);world.TrySpawn(Species.Zombie,6,3,out id);world.TrySpawn(Species.Human,2,6,out id);world.TrySpawn(Species.Zombie,6,6,out id);SaveDocument saved=world.Capture();saved.actors[0].health=40;saved.actors[1].health=70;saved.actors[3].health=0;return World.Restore(saved);
        }
        public static World BatConditionsTest()
        {
            WorldMap map=new WorldMap(12,8,"Ground bat conditions test","ground-bat-conditions-1");map.SetWeapon(2,4,MeleeWeapon.Bat);map.SetWeapon(5,4,MeleeWeapon.Bat);map.SetWeapon(8,4,MeleeWeapon.Bat);map.SetWeapon(5,6,MeleeWeapon.Bat);SaveDocument saved=new World(map,214,encounter:true,hunger:true).Capture();saved.map.schemaVersion=6;saved.map.weapons[1].wear=new[]{6};saved.map.weapons[2].wear=new[]{11};saved.map.weapons[3].extraBats=2;saved.map.weapons[3].wear=new[]{0,6,11};return World.Restore(saved);
        }
        public static World FirstWeaponPriorityTest()
        {
            WorldMap map=new WorldMap(12,11,"First weapon priority test","first-weapon-priority-1");map.SetWeapon(6,4,MeleeWeapon.Bat);map.SetWeapon(6,8,MeleeWeapon.Bat);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,5,out id);world.TrySpawn(Species.Human,3,3,out id);SaveDocument saved=world.Capture();saved.actors[0].meleeWeapon=MeleeWeapon.Bat;saved.actors[0].batWear=10;return World.Restore(saved);
        }
        public static World UnarmedCourtesyTest()
        {
            SaveDocument saved=SupplyCourtesyTest().Capture();saved.map.name="First bat courtesy test";saved.map.id="first-bat-courtesy-1";for(int i=0;i<saved.actors.Length;i++){saved.actors[i].meleeWeapon=MeleeWeapon.None;saved.actors[i].batWear=0;}return World.Restore(saved);
        }
        public static World SupplyCourtesyTest()
        {
            WorldMap map=new WorldMap(12,9,"Supply courtesy test","supply-courtesy-1");map.SetWeapon(6,4,MeleeWeapon.Bat);map.SetWeapon(2,7,MeleeWeapon.Bat);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,4,out id);world.TrySpawn(Species.Human,5,4,out id);SaveDocument saved=world.Capture();for(int i=0;i<2;i++){saved.actors[i].meleeWeapon=MeleeWeapon.Bat;saved.actors[i].batWear=10;}return World.Restore(saved);
        }
        public static World ReplacementTripTest()
        {
            WorldMap map=new WorldMap(12,7,"Replacement trip test","replacement-trip-1");for(int y=0;y<7;y++)for(int x=0;x<12;x++)map.Set(x,y,Terrain.Wall);for(int x=2;x<=9;x++)map.Set(x,3,Terrain.Grass);map.SetWeapon(8,3,MeleeWeapon.Bat);
            World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,3,out id);SaveDocument saved=world.Capture();saved.actors[0].meleeWeapon=MeleeWeapon.Bat;saved.actors[0].batWear=10;saved.actors[0].bluntSkill=2;saved.actors[0].bluntPractice=1;return World.Restore(saved);
        }
        public static World WeaponReplacementTest()
        {
            WorldMap map=new WorldMap(12,8,"Weapon replacement test","weapon-replacement-1");map.SetWeapon(3,4,MeleeWeapon.Bat);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,4,out id);SaveDocument saved=world.Capture();saved.actors[0].meleeWeapon=MeleeWeapon.Bat;saved.actors[0].batWear=10;saved.actors[0].bluntSkill=2;saved.actors[0].bluntPractice=1;return World.Restore(saved);
        }
        public static World CombatFootworkTest()
        {
            WorldMap map=new WorldMap(7,5,"Combat footwork test","combat-footwork-1");for(int y=0;y<5;y++)for(int x=0;x<7;x++)map.Set(x,y,Terrain.Wall);for(int x=2;x<=4;x++)map.Set(x,2,Terrain.Grass);
            World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,2,out id);world.TrySpawn(Species.Zombie,4,2,out id);SaveDocument saved=world.Capture();saved.actors[0].meleeWeapon=MeleeWeapon.Bat;saved.actors[0].strength=10;saved.actors[0].perception=8;saved.actors[0].agility=8;saved.actors[0].bravery=3;saved.actors[0].bluntSkill=5;saved.reactionRandomState=1;return World.Restore(saved);
        }
        public static World BatDurabilityTest()
        {
            World world=BluntCombatTest();world.Step();SaveDocument saved=world.Capture();saved.tick=5;saved.actors[0].batWear=MeleeRules.BatHits-1;saved.actors[1].x=3;saved.reactionRandomState=1;return World.Restore(saved);
        }
        public static World BiteEvasionTest(bool evade=true)
        {
            WorldMap map=new WorldMap(7,5,"Bite evasion test","bite-evasion-1");for(int y=0;y<5;y++)for(int x=0;x<7;x++)map.Set(x,y,Terrain.Wall);for(int x=2;x<=4;x++)map.Set(x,2,Terrain.Grass);
            World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Zombie,2,2,out id);world.TrySpawn(Species.Human,3,2,out id);SaveDocument saved=world.Capture();saved.actors[1].agility=10;saved.reactionRandomState=evade?2u:1u;return World.Restore(saved);
        }
        public static World WeaponDropTest()
        {
            World world=new World(new WorldMap(12,8,"Weapon drop test","weapon-drop-1"),214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,4,out id);SaveDocument saved=world.Capture();saved.actors[0].meleeWeapon=MeleeWeapon.Bat;saved.actors[0].turnAtTick=1;return World.Restore(saved);
        }
        public static World ShelterSpacingTest()
        {
            WorldMap map=new WorldMap(12,10,"Shelter spacing test","shelter-spacing-1");AddShelterRoom(map,1,1,false);World world=new World(map,214,encounter:true,hunger:true);int id;for(int i=0;i<4;i++)world.TrySpawn(Species.Human,2,2,out id);SaveDocument saved=world.Capture();for(int i=0;i<4;i++){saved.actors[i].shelterId=27;saved.actors[i].shelterX=2;saved.actors[i].shelterY=2;saved.actors[i].shelterState=ShelterState.Inside;saved.actors[i].stress=100;}return World.Restore(saved);
        }
        public static World CombatExperienceTest()
        {
            SaveDocument saved=BluntCombatTest().Capture();saved.actors[0].bluntSkill=0;saved.actors[0].bluntPractice=2;return World.Restore(saved);
        }
        public static World WeaponSeekingTest()
        {
            WorldMap map=new WorldMap(12,8);map.SetWeapon(6,4,MeleeWeapon.Bat);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,4,out id);return world;
        }
        public static World BluntCombatTest()
        {
            WorldMap map=new WorldMap(8,5,"Blunt combat test","blunt-combat-1");for(int y=0;y<5;y++)for(int x=0;x<8;x++)map.Set(x,y,Terrain.Wall);for(int x=2;x<=6;x++)map.Set(x,2,Terrain.Floor);map.SetWeapon(2,2,MeleeWeapon.Bat);
            World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,2,out id);world.TrySpawn(Species.Zombie,6,2,out id);SaveDocument saved=world.Capture();saved.actors[0].strength=10;saved.actors[0].perception=8;saved.actors[0].agility=8;saved.actors[0].bravery=3;saved.actors[0].bluntSkill=5;return World.Restore(saved);
        }
        public static World FoodScarcityTrial(uint seed=214)
        {
            WorldMap map=new WorldMap(22,23,"Food scarcity trial","food-scarcity-trial-1");
            for(int y=0;y<23;y++)for(int x=0;x<22;x++)if(x==0 || y==0 || x==21 || y==22)map.Set(x,y,Terrain.Wall);
            foreach(int top in new[]{2,13}){AddShelterRoom(map,1,top,false);AddShelterRoom(map,11,top,false);map.Set(15,top+3,Terrain.Wall);map.Set(11,top+3,Terrain.DoorClosed);}
            map.SetShelf(4,3,6);map.SetShelf(13,3,6);map.SetShelf(4,14,0);map.SetShelf(13,14,6);map.SetShelf(8,7,6,true);map.SetShelf(9,9,0,true);
            World world=new World(map,seed,encounter:true,hunger:true);int id;for(int i=0;i<12;i++)if(!world.TrySpawn(Species.Human,6+2*(i%3),9+2*(i/3),out id))throw new System.InvalidOperationException("Food scarcity trial spawn was blocked.");
            SaveDocument saved=world.Capture();for(int i=0;i<saved.actors.Length;i++){saved.actors[i].hunger=60+10*(i%3);saved.actors[i].bravery=3*(i%3-1);}return World.Restore(saved);
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
        public static World SupplyDeliveryTest()
        {
            WorldMap map=new WorldMap(20,12,"Shelter supply delivery test","supply-delivery-1");AddShelterRoom(map,1,2,false);AddShelterRoom(map,10,2,false);map.Set(14,5,Terrain.Wall);map.Set(10,5,Terrain.DoorClosed);
            map.SetStorageBox(new StorageBoxDocument{x=4,y=3,capacity=30,items=new ItemStackDocument[0]});map.SetStorageBox(new StorageBoxDocument{x=12,y=3,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=12}}});World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,5,out id);SaveDocument saved=world.Capture();saved.actors[0].shelterId=63;saved.actors[0].shelterX=3;saved.actors[0].shelterY=5;saved.actors[0].shelterState=ShelterState.Inside;saved.actors[0].hunger=10;saved.actors[0].carriedFood=1;saved.actors[0].storageMemory=new[]{new StorageMemoryRecord{x=4,y=3,food=0,space=30,observedTick=0},new StorageMemoryRecord{x=12,y=3,food=12,space=18,observedTick=0}};return World.Restore(saved);
        }
        public static World ShelterShortageTest()
        {
            WorldMap map=new WorldMap(26,12,"Shelter shortages test","shelter-shortages-1");foreach(int left in new[]{1,10,19})AddShelterRoom(map,left,2,false);
            foreach(int x in new[]{4,13,22})map.SetStorageBox(new StorageBoxDocument{x=x,y=3,capacity=30,items=x==13?new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=6}}:new ItemStackDocument[0]});
            map.SetShelf(12,5,4);map.SetShelf(8,9,12,true);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,2,4,out id);world.TrySpawn(Species.Human,3,5,out id);world.TrySpawn(Species.Human,11,4,out id);world.TrySpawn(Species.Human,7,5,out id);
            SaveDocument saved=world.Capture();for(int i=0;i<3;i++){saved.actors[i].shelterId=i<2?81:90;saved.actors[i].shelterX=saved.actors[i].x;saved.actors[i].shelterY=saved.actors[i].y;saved.actors[i].shelterState=ShelterState.Inside;}saved.actors[0].hunger=80;saved.actors[1].hunger=50;saved.actors[2].hunger=10;saved.actors[2].carriedFood=1;
            saved.actors[3].hunger=10;saved.actors[3].carriedFood=1;saved.actors[3].homeShelterId=81;saved.actors[3].restockHomeId=81;saved.actors[3].restockBox=83;saved.actors[3].restockPhase=RestockPhase.Collecting;saved.actors[3].restockUntilTick=600;saved.actors[3].shelfMemory=new[]{new ShelfMemoryRecord{x=8,y=9,food=12,observedTick=0}};return World.Restore(saved);
        }
        public static World ConsumerVehicleShoppingTest()
        {
            World original=LargeCityShoppingTest();SaveDocument saved=original.Capture();int home=saved.actors[0].assignedHouseId,shop=saved.actors[0].familiarStoreId;StorageBoxDocument homeBox=null,shopBox=null;
            foreach(var box in saved.map.storageBoxes){int room=original.HouseAt(box.x,box.y);if(room==home){box.foodReserve=12;homeBox=box;}if(room==shop)shopBox=box;}
            foreach(var shelf in saved.map.shelves)if(original.HouseAt(shelf.x,shelf.y)==shop)shelf.food=18;
            World world=World.Restore(saved);if(homeBox==null || shopBox==null || !world.TryPlaceParkingNearBox(homeBox.x,homeBox.y) || !world.TryPlaceParkingNearBox(shopBox.x,shopBox.y))throw new System.InvalidOperationException("Vehicle shopping preview needs home and store parking.");
            ParkingBayDocument chosen=null;int nearest=int.MaxValue;foreach(var bay in world.ParkingBays()){int distance=System.Math.Abs(bay.x-homeBox.x)+System.Math.Abs(bay.y-homeBox.y);if(distance<nearest){nearest=distance;chosen=bay;}}
            int id;if(chosen==null || !world.TryPlaceVehicle(chosen.x,chosen.y,out id) || !world.TryAssignVehicleShopper(0,0))throw new System.InvalidOperationException("Vehicle shopping preview could not assign its consumer.");return world;
        }
        public static World LargeCityShoppingTest()
        {
            World world=CitySetup.Create(214,8,8,4,CityStreetLayout.Staggered,CityZoning.NeighborhoodCenters);SaveDocument saved=world.Capture();int chosen=3,best=int.MaxValue;
            for(int i=3;i<saved.actors.Length;i++){ActorState actor=saved.actors[i];foreach(var shop in world.StoreSupplies())if(shop.StoreId==actor.familiarStoreId){int key=shop.SupplyBox-1,distance=System.Math.Abs(actor.x-key%world.Width)+System.Math.Abs(actor.y-key/world.Width),score=System.Math.Abs(distance-45);if(score<best){best=score;chosen=i;}}}
            ActorState resident=saved.actors[chosen];saved.actors=new[]{resident};foreach(var box in saved.map.storageBoxes)if(world.HouseAt(box.x,box.y)==resident.assignedHouseId)box.items=new ItemStackDocument[0];return World.Restore(saved);
        }
        public static World VehicleParkingTest()
        {
            World world=VehicleHomeDeliveryTest();foreach(var box in world.StorageBoxSites())if(!world.TryPlaceParkingNearBox(box.x,box.y))throw new System.InvalidOperationException("Parking preview lacks a usable bay.");return world;
        }
        public static World VehicleTrafficRecoveryTest()
        {
            WorldMap map=WorldMap.Restore(VehicleRoutingTest().Capture().map);World world=new World(map,214,encounter:true,hunger:true);int id;
            world.TryPlaceVehicle(7,4,out id);world.TryPlaceVehicle(10,4,out id);world.TryPlaceVehicle(10,5,out id);world.TrySetVehicleDestination(0,20,4);return world;
        }
        public static World VehicleHomeDeliveryTest()
        {
            World world=CitySetup.Create(214,4,2,1,CityStreetLayout.Staggered,CityZoning.Fixed);
            int assigned=0;foreach(var box in world.StorageBoxSites())if(world.BuildingUseAt(box.x,box.y)==BuildingUse.Home && world.TryToggleCourierDestination(0,box.x,box.y)){world.TrySetStorageFoodReserve(box.x,box.y,6);if(++assigned==2)break;}
            int id;if(assigned!=2 || !world.TryPlaceVehicle(3,8,out id) || !world.TryAssignVehicleCourier(0,0))throw new System.InvalidOperationException("Home delivery preview could not assign its courier.");return world;
        }
        public static World VehicleDeliveryTest()
        {
            World world=CitySetup.Create(214,4,2,1,CityStreetLayout.Staggered,CityZoning.Fixed);int id;if(!world.TryPlaceVehicle(3,8,out id) || !world.TryAssignVehicleCourier(0,0))throw new System.InvalidOperationException("Vehicle delivery preview could not assign its courier.");return world;
        }
        public static World VehicleIntersectionTest()
        {
            WorldMap map=CityGenerator.Streets(214,1,1);World world=new World(map,214,1000,true,hunger:true);int id;
            world.TryPlaceVehicle(2,4,out id);world.TrySetVehicleDestination(0,9,4);
            world.TryPlaceVehicle(4,5,out id);world.TrySetVehicleDestination(1,4,1);return world;
        }
        public static World VehicleLaneTest()
        {
            WorldMap map=WorldMap.Restore(VehicleRoutingTest().Capture().map);CityGenerator.PaintRoadLanes(map,new[]{3,23},new[]{4,12});
            World world=new World(map,214,1000,true,hunger:true);int id;world.TryPlaceVehicle(11,5,out id);world.TrySetVehicleDestination(0,23,10);world.TryPlaceVehicle(9,5,out id);world.TrySetVehicleDestination(1,23,8);return world;
        }
        public static World VehicleRoutingTest()
        {
            WorldMap map=new WorldMap(28,18,"Vehicle road routing trial","vehicle-routing-1");
            for(int y=0;y<18;y++)for(int x=0;x<28;x++)
            {
                if(x==0 || y==0 || x==27 || y==17)map.Set(x,y,Terrain.Wall);
                else if(x>=3 && x<=24 && y>=4 && y<=13 && (x<=4 || x>=23 || y<=5 || y>=12))map.Set(x,y,Terrain.Road);
            }
            for(int y=1;y<17;y++)for(int x=1;x<27;x++)if(map.Get(x,y)==Terrain.Grass)
            {
                bool beside=false;foreach(var offset in new[]{(1,0),(-1,0),(0,1),(0,-1)})beside|=TerrainRules.RoadSurface(map.Get(x+offset.Item1,y+offset.Item2));if(beside)map.Set(x,y,Terrain.Sidewalk);
            }
            foreach(int x in new[]{3,4,23,24})map.Set(x,8,Terrain.Crosswalk);foreach(int y in new[]{4,5,12,13})map.Set(14,y,Terrain.Crosswalk);
            World world=new World(map,214,1000,true,hunger:true);int id;world.TryPlaceVehicle(7,4,out id);world.TrySetVehicleDestination(0,23,11);return world;
        }
        public static World HiddenStoreEntranceTest()
        {
            SaveDocument saved=StoreShoppingTest().Capture();WorldMap map=WorldMap.Restore(saved.map);map.Set(11,5,Terrain.Wall);map.Set(15,5,Terrain.DoorClosed);saved.map=map.Capture();saved.map.name="Nearby store with a hidden entrance";saved.actors[0].shelfMemory=null;saved.actors[0].intelligence=10;return World.Restore(saved);
        }
        public static World GeneratedBuildingTrial(uint seed=214,int columns=4,int rows=2,int homesPerBlock=1,CityStreetLayout layout=CityStreetLayout.Grid,CityZoning zoning=CityZoning.Fixed)
        {
            string error=CitySetup.ValidationError(seed,columns,rows,homesPerBlock,layout,zoning);if(error!=null)throw new System.ArgumentException(error);
            GeneratedDistrict district=CityGenerator.BuildingLots(seed,columns,rows,homesPerBlock,layout,zoning);WorldMap map=district.Map;
            var homes=new System.Collections.Generic.List<GeneratedBuilding>();var jobs=new System.Collections.Generic.List<GeneratedBuilding>();
            foreach(GeneratedBuilding building in district.Buildings)
            {
                int food=building.Use==BuildingUse.DistributionCenter?40:building.Use==BuildingUse.Home?2:0;
                map.SetStorageBox(new StorageBoxDocument{x=building.BoxX,y=building.BoxY,capacity=building.Use==BuildingUse.DistributionCenter?60:12,foodReserve=building.Use==BuildingUse.Home?2:4,items=food==0?new ItemStackDocument[0]:new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=food}}});
                map.SetRestSpot(building.Left+1,building.Top+building.Height-2);
                if(building.Use==BuildingUse.Store){map.SetShelf(building.Left+2,building.Top+2,4,true);map.SetPhone(building.Left+3,building.Top+2);}
                if(building.Use==BuildingUse.Home)homes.Add(building);else jobs.Add(building);
            }
            jobs.Sort((a,b)=>{int order=(a.Use==BuildingUse.DistributionCenter?0:1).CompareTo(b.Use==BuildingUse.DistributionCenter?0:1);if(order==0)order=a.BlockRow.CompareTo(b.BlockRow);return order==0?a.BlockColumn.CompareTo(b.BlockColumn):order;});
            World world=new World(map,seed,1000,true,hunger:true);int id;
            foreach(GeneratedBuilding building in jobs)
            {
                if(building.Use==BuildingUse.DistributionCenter)world.TryConfigureDistributionCenter(building.BoxX,building.BoxY);else world.TryConfigureStore(building.BoxX,building.BoxY);
                int index=world.Count;world.TrySpawn(Species.Human,building.Left+1,building.Top+2,out id);GeneratedBuilding home=homes[index];world.TryAssignHouse(index,home.Left+1,home.Top+1);
                if(building.Use==BuildingUse.DistributionCenter){world.TryAssignDistributionCourier(index,world.HouseAt(building.BoxX,building.BoxY));world.TrySetCourierSchedule(index,7,12);}else{world.TryAssignClerk(index,world.HouseAt(building.BoxX,building.BoxY));world.TrySetClerkSchedule(index,7,12);}
            }
            for(int i=jobs.Count;i<homes.Count;i++){GeneratedBuilding home=homes[i];int index=world.Count;world.TrySpawn(Species.Human,home.Left+1,home.Top+2,out id);world.TryAssignHouse(index,home.Left+1,home.Top+1);}
            SaveDocument saved=world.Capture();for(int i=0;i<saved.actors.Length;i++){ActorState actor=saved.actors[i];actor.hunger=10;actor.carriedFood=1;actor.intelligence=8;actor.shelterId=world.HouseAt(actor.x,actor.y);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;saved.actors[i]=actor;}World prepared=World.Restore(saved);prepared.TryFamiliarizeResidentsWithShops();return prepared;
        }
        public static World PedestrianCrossingTrial()
        {
            WorldMap map=new WorldMap(34,20,"Pedestrian crossing trial","pedestrian-crossing-1");
            for(int y=0;y<20;y++)for(int x=0;x<34;x++)
                if(x==0 || y==0 || x==33 || y==19)map.Set(x,y,Terrain.Wall);
            for(int y=1;y<19;y++){map.Set(14,y,Terrain.Road);map.Set(15,y,Terrain.Road);map.Set(13,y,Terrain.Sidewalk);map.Set(16,y,Terrain.Sidewalk);}
            for(int x=5;x<=26;x++)map.Set(x,10,Terrain.Sidewalk);
            map.Set(14,10,Terrain.Crosswalk);map.Set(15,10,Terrain.Crosswalk);
            AddShelterRoom(map,1,3,false);map.SetStorageBox(new StorageBoxDocument{x=4,y=4,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=12}}});
            AddShelterRoom(map,26,3,false);map.Set(30,6,Terrain.Wall);map.Set(26,6,Terrain.DoorClosed);map.SetStorageBox(new StorageBoxDocument{x=29,y=4,capacity=12,foodReserve=4,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=4}}});map.SetRestSpot(27,8);
            World world=new World(map,214,1000,true,hunger:true);int id;world.TrySpawn(Species.Human,3,6,out id);world.TryAssignHouse(0,28,6);world.TryConfigureDistributionCenter(4,4);world.TryAssignDistributionCourier(0,world.HouseAt(4,4));world.TrySetCourierSchedule(0,19,12);
            SaveDocument saved=world.Capture();saved.actors[0].hunger=10;saved.actors[0].carriedFood=1;saved.actors[0].shelterId=world.HouseAt(3,6);saved.actors[0].shelterX=3;saved.actors[0].shelterY=6;saved.actors[0].shelterState=ShelterState.Inside;return World.Restore(saved);
        }
        public static World GeneratedCityTrial(uint seed=214)
        {
            WorldMap map=CityGenerator.Streets(seed);int far=73;
            AddShelterRoom(map,7,7,false);map.SetStorageBox(new StorageBoxDocument{x=10,y=8,capacity=40,foodReserve=6,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=30}}});map.SetRestSpot(8,12);
            AddShelterRoom(map,far,7,false);map.Set(far+4,10,Terrain.Wall);map.Set(far,10,Terrain.DoorClosed);map.SetStorageBox(new StorageBoxDocument{x=far+3,y=8,capacity=4,items=new ItemStackDocument[0]});map.SetShelf(far+2,12,0,true);map.SetPhone(far+3,11);map.SetRestSpot(far+1,11);
            AddShelterRoom(map,far,42,false);map.SetStorageBox(new StorageBoxDocument{x=far+3,y=43,capacity=12,foodReserve=4,items=new ItemStackDocument[0]});map.SetRestSpot(far+1,47);
            World world=new World(map,seed,1000,true,hunger:true);int id;world.TrySpawn(Species.Human,9,10,out id);world.TrySpawn(Species.Human,far+1,10,out id);
            for(int i=0;i<2;i++)world.TryAssignHouse(i,far+2,45);
            world.TryConfigureDistributionCenter(10,8);world.TryAssignDistributionCourier(0,world.HouseAt(10,8));world.TryConfigureStore(far+3,8);world.TryAssignClerk(1,world.HouseAt(far+1,10));
            SaveDocument saved=world.Capture();for(int i=0;i<2;i++){ActorState actor=saved.actors[i];actor.hunger=10;actor.carriedFood=1;actor.shelterId=world.HouseAt(actor.x,actor.y);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;saved.actors[i]=actor;}return World.Restore(saved);
        }
        public static World LargerDistrictTrial(uint seed=214)
        {
            WorldMap map=new WorldMap(64,36,"Larger home supply district","larger-district-1");
            for(int y=0;y<36;y++)for(int x=0;x<64;x++)
                if(x==0 || y==0 || x==63 || y==35)map.Set(x,y,Terrain.Wall);
            for(int x=1;x<63;x++){map.Set(x,20,Terrain.Road);map.Set(x,21,Terrain.Sidewalk);}
            int[] storeX={14,34,54},storeY={3,9,3},homeX={14,34,54};
            AddShelterRoom(map,2,14,false);map.SetStorageBox(new StorageBoxDocument{x=5,y=15,capacity=100,foodReserve=6,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=80}}});map.SetRestSpot(3,19);
            for(int i=0;i<3;i++)
            {
                int x=storeX[i],y=storeY[i];AddShelterRoom(map,x,y,false);map.Set(x+4,y+3,Terrain.Wall);map.Set(x,y+3,Terrain.DoorClosed);
                map.SetStorageBox(new StorageBoxDocument{x=x+3,y=y+1,capacity=6,foodReserve=4,items=new ItemStackDocument[0]});map.SetShelf(x+2,y+5,6,true);map.SetPhone(x+3,y+4);map.SetRestSpot(x+1,y+4);
                x=homeX[i];AddShelterRoom(map,x,25,false);map.SetStorageBox(new StorageBoxDocument{x=x+3,y=26,capacity=12,foodReserve=4,items=new ItemStackDocument[0]});map.SetRestSpot(x+1,30);map.SetRestSpot(x+2,30);
            }
            World world=new World(map,seed,1000,true,hunger:true);int id;
            world.TrySpawn(Species.Human,4,17,out id);world.TrySpawn(Species.Human,4,18,out id);
            world.TryConfigureDistributionCenter(5,15);
            for(int i=0;i<2;i++){world.TryAssignHouse(i,homeX[i]+2,28);world.TryAssignDistributionCourier(i,world.HouseAt(5,15));world.TrySetCourierSchedule(i,7,12);}
            for(int i=0;i<3;i++)
            {
                int clerk=world.Count;world.TrySpawn(Species.Human,storeX[i]+1,storeY[i]+3,out id);world.TryConfigureStore(storeX[i]+3,storeY[i]+1);world.TryAssignClerk(clerk,world.HouseAt(storeX[i]+1,storeY[i]+3));world.TryAssignHouse(clerk,homeX[i]+2,28);world.TrySetClerkSchedule(clerk,7,12);
                for(int n=0;n<2;n++){int consumer=world.Count;world.TrySpawn(Species.Human,homeX[i]+1+n,28,out id);world.TryAssignHouse(consumer,homeX[i]+2,28);}
            }
            SaveDocument saved=world.Capture();
            for(int i=0;i<saved.actors.Length;i++)
            {
                ActorState actor=saved.actors[i];actor.hunger=10;actor.carriedFood=1;actor.intelligence=8;actor.shelterId=world.HouseAt(actor.x,actor.y);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;
                // Residents already know their neighborhood shop; discovery has its own separate preview.
                if(i>=2 && (i-2)%3!=0){int shop=(i-2)/3;actor.shelfMemory=new[]{new ShelfMemoryRecord{x=storeX[shop]+2,y=storeY[shop]+5,food=6,observedTick=0}};}
                saved.actors[i]=actor;
            }
            return World.Restore(saved);
        }
        public static World WorkerShiftTest(bool starting=false)
        {
            WorldMap map=new WorldMap(32,20,starting?"Worker shift start":"Worker shift end","worker-shift-1");AddShelterRoom(map,1,6,false);AddShelterRoom(map,11,2,false);map.Set(15,5,Terrain.Wall);map.Set(11,5,Terrain.DoorClosed);AddShelterRoom(map,21,6,false);map.Set(25,9,Terrain.Wall);map.Set(21,9,Terrain.DoorClosed);
            map.SetStorageBox(new StorageBoxDocument{x=4,y=7,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=6}}});map.SetRestSpot(2,11);map.SetRestSpot(3,11);
            map.SetStorageBox(new StorageBoxDocument{x=14,y=3,capacity=6,foodReserve=4,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=4}}});map.SetShelf(13,7,0,true);map.SetPhone(14,6);
            map.SetStorageBox(new StorageBoxDocument{x=24,y=7,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=20}}});map.SetRestSpot(22,11);
            World world=new World(map,214,1000,true,hunger:true);int id;world.TrySpawn(Species.Human,23,9,out id);world.TrySpawn(Species.Human,12,5,out id);world.TrySpawn(Species.Human,4,9,out id);for(int i=0;i<3;i++)world.TryAssignHouse(i,3,9);world.TryConfigureStore(14,3);world.TryAssignClerk(1,world.HouseAt(12,5));world.TryConfigureDistributionCenter(24,7);world.TryAssignDistributionCourier(0,world.HouseAt(23,9));world.TryToggleCourierDestination(0,14,3);world.TrySetCourierSchedule(0,starting?8:7,1);world.TrySetClerkSchedule(1,starting?8:7,1);
            SaveDocument saved=world.Capture();saved.tick=540;for(int i=0;i<3;i++){ActorState actor=saved.actors[i];if(starting && i<2){actor.x=2+i;actor.y=9;}actor.hunger=10;actor.carriedFood=1;actor.shelterId=world.HouseAt(actor.x,actor.y);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;saved.actors[i]=actor;}return World.Restore(saved);
        }
        public static World StoreDiscoveryTest()
        {
            WorldMap map=new WorldMap(26,20,"Unfamiliar and empty stores","store-discovery-1");AddShelterRoom(map,1,6,false);AddShelterRoom(map,9,2,false);map.Set(13,5,Terrain.Wall);map.Set(9,5,Terrain.DoorClosed);AddShelterRoom(map,16,9,false);map.Set(20,12,Terrain.Wall);map.Set(16,12,Terrain.DoorClosed);
            map.SetStorageBox(new StorageBoxDocument{x=4,y=7,capacity=6,foodReserve=2,items=new ItemStackDocument[0]});map.SetRestSpot(2,11);
            map.SetStorageBox(new StorageBoxDocument{x=12,y=3,capacity=30,items=new ItemStackDocument[0]});map.SetShelf(11,7,0,false);
            map.SetStorageBox(new StorageBoxDocument{x=19,y=10,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=8}}});map.SetShelf(18,14,6,true);
            World world=new World(map,214,1000,true,hunger:true);int id;world.TrySpawn(Species.Human,3,9,out id);world.TryAssignHouse(0,3,9);world.TryConfigureStore(12,3);world.TryConfigureStore(19,10);
            SaveDocument saved=world.Capture();saved.actors[0].hunger=10;saved.actors[0].carriedFood=1;saved.actors[0].intelligence=8;saved.actors[0].shelterId=world.HouseAt(3,9);saved.actors[0].shelterX=3;saved.actors[0].shelterY=9;saved.actors[0].shelterState=ShelterState.Inside;return World.Restore(saved);
        }
        public static World StoreShoppingTest()
        {
            WorldMap map=new WorldMap(22,12,"Home store shopping","store-shopping-1");AddShelterRoom(map,1,2,false);AddShelterRoom(map,11,2,false);map.Set(15,5,Terrain.Wall);map.Set(11,5,Terrain.DoorClosed);
            map.SetStorageBox(new StorageBoxDocument{x=4,y=3,capacity=6,foodReserve=4,items=new ItemStackDocument[0]});map.SetRestSpot(2,7);
            map.SetStorageBox(new StorageBoxDocument{x=14,y=3,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=12}}});map.SetShelf(13,7,6,true);map.SetPhone(14,6);
            World world=new World(map,214,1000,true,hunger:true);int id;world.TrySpawn(Species.Human,3,5,out id);world.TrySpawn(Species.Human,12,5,out id);world.TryAssignHouse(0,3,5);world.TryAssignHouse(1,3,5);world.TryConfigureStore(14,3);world.TryAssignClerk(1,world.HouseAt(12,5));
            SaveDocument saved=world.Capture();for(int i=0;i<2;i++){ActorState actor=saved.actors[i];actor.hunger=10;actor.carriedFood=1;actor.shelterId=world.HouseAt(actor.x,actor.y);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;saved.actors[i]=actor;}
            saved.actors[0].intelligence=8;saved.actors[0].shelfMemory=new[]{new ShelfMemoryRecord{x=13,y=7,food=6,observedTick=0}};return World.Restore(saved);
        }
        public static World HomeSupplyInspectionTest()
        {
            WorldMap map=new WorldMap(32,18,"Assigned home supply inspection","home-supply-inspection-1");
            for(int y=1;y<=14;y++)for(int x=1;x<=14;x++)map.Set(x,y,x==1 || x==14 || y==1 || y==14?Terrain.Wall:Terrain.Floor);
            map.Set(14,7,Terrain.DoorClosed);map.SetStorageBox(new StorageBoxDocument{x=12,y=12,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=6}}});
            AddShelterRoom(map,21,2,false);map.SetStorageBox(new StorageBoxDocument{x=24,y=3,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=20}}});map.SetShelf(23,7,5,true);
            World world=new World(map,214,1000,true,hunger:true);int id;world.TrySpawn(Species.Human,2,2,out id);world.TryAssignHouse(0,2,2);world.TryConfigureDistributionCenter(24,3);world.TrySetHouseholdMealsPerResident(world.HouseAt(2,2),3);
            SaveDocument saved=world.Capture();saved.actors[0].hunger=10;saved.actors[0].carriedFood=1;saved.actors[0].shelterId=world.HouseAt(2,2);saved.actors[0].shelterX=2;saved.actors[0].shelterY=2;saved.actors[0].shelterState=ShelterState.Inside;return World.Restore(saved);
        }
        public static World SupplyChainIdentityTest()
        {
            SaveDocument saved=MultiStoreSupplyTrial().Capture();WorldMap map=WorldMap.Restore(saved.map);
            AddShelterRoom(map,2,16,false);map.SetStorageBox(new StorageBoxDocument{x=5,y=17,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=6}}});map.SetRestSpot(3,21);
            saved.map=map.Capture();saved.map.name="Home store distribution visual preview";World world=World.Restore(saved);
            for(int i=0;i<world.Count;i++)world.TryAssignHouse(i,4,19);
            world.TrySetHouseholdMealsPerResident(world.HouseAt(4,19),HouseholdFoodRules.DefaultMealsPerResident);return world;
        }
        public static World MultiStoreSupplyTrial(uint seed=214)
        {
            WorldMap map=new WorldMap(35,24,"Multi-store supply district","multi-store-supply-1");
            for(int y=0;y<24;y++)for(int x=0;x<35;x++)if(x==0 || y==0 || x==34 || y==23)map.Set(x,y,Terrain.Wall);
            for(int x=7;x<25;x++)map.Set(x,11,Terrain.Road);for(int y=5;y<=18;y++)map.Set(12,y,Terrain.Sidewalk);for(int x=12;x<14;x++){map.Set(x,5,Terrain.Sidewalk);map.Set(x,18,Terrain.Sidewalk);}
            AddShelterRoom(map,2,8,false);map.SetStorageBox(new StorageBoxDocument{x=5,y=9,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=26}}});map.SetRestSpot(3,13);
            int[] lefts={14,25,14},tops={2,8,15};
            for(int i=0;i<3;i++)
            {
                int left=lefts[i],top=tops[i];AddShelterRoom(map,left,top,false);map.Set(left+4,top+3,Terrain.Wall);map.Set(left,top+3,Terrain.DoorClosed);
                map.SetStorageBox(new StorageBoxDocument{x=left+3,y=top+1,capacity=4,items=new ItemStackDocument[0]});map.SetShelf(left+2,top+5,1,true);map.SetPhone(left+3,top+4);map.SetRestSpot(left+1,top+4);
            }
            World world=new World(map,seed,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,4,11,out id);world.TrySpawn(Species.Human,4,12,out id);
            for(int i=0;i<3;i++){world.TrySpawn(Species.Human,lefts[i]+1,tops[i]+3,out id);world.TrySpawn(Species.Human,lefts[i]+1,tops[i]+5,out id);}
            SaveDocument saved=world.Capture();for(int i=0;i<saved.actors.Length;i++)
            {
                ActorState actor=saved.actors[i];actor.hunger=i<2 || i%2==0?10:80;actor.carriedFood=i<2 || i%2==0?1:0;actor.shelterId=world.HouseAt(actor.x,actor.y);actor.shelterX=actor.x;actor.shelterY=actor.y;actor.shelterState=ShelterState.Inside;saved.actors[i]=actor;
            }
            world=World.Restore(saved);world.TryConfigureDistributionCenter(5,9);int center=world.HouseAt(5,9);
            for(int i=0;i<2;i++){ActorState actor=world.ActorAt(i);world.TryAssignHouse(i,actor.x,actor.y);world.TryAssignDistributionCourier(i,center);}
            for(int i=0;i<3;i++){world.TryConfigureStore(lefts[i]+3,tops[i]+1);int clerk=2+i*2;ActorState actor=world.ActorAt(clerk);world.TryAssignHouse(clerk,actor.x,actor.y);world.TryAssignClerk(clerk,world.HouseAt(actor.x,actor.y));actor=world.ActorAt(clerk+1);world.TryAssignHouse(clerk+1,actor.x,actor.y);}
            return world;
        }
        public static World HungerDamageTest()
        {
            WorldMap map=new WorldMap(3,3,"Hunger and exhaustion damage","deprivation-1");for(int y=0;y<3;y++)for(int x=0;x<3;x++)map.Set(x,y,Terrain.Wall);map.Set(1,1,Terrain.Grass);World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,1,1,out id);SaveDocument saved=world.Capture();saved.actors[0].health=10;saved.actors[0].hunger=100;saved.actors[0].energySpent=100;return World.Restore(saved);
        }
        public static World CourierFulfillmentTest()
        {
            World world=DistributionCenterTest();world.TryToggleCourierDestination(0,13,3);SaveDocument saved=world.Capture();StorageBoxDocument crate=System.Array.Find(saved.map.storageBoxes,box=>box.x==13 && box.y==3);crate.capacity=4;saved.map.name="Phone order fulfillment test";world=World.Restore(saved);world.TryPlacePhone(13,6);return world;
        }
        public static World PhoneSupplyOrderTest()
        {
            SaveDocument saved=StoreClerkTest().Capture();saved.map.storageBoxes[0].items=new ItemStackDocument[0];saved.actors[1].hunger=10;World world=World.Restore(saved);world.TryPlacePhone(2,5);return world;
        }
        public static World DistributionCenterTest()
        {
            World world=AssignedCourierTest();world.TryToggleCourierDestination(0,42,3);SaveDocument saved=world.Capture();WorldMap map=WorldMap.Restore(saved.map);map.SetShelf(12,7,0,true);saved.map=map.Capture();saved.map.name="Distribution center to store test";world=World.Restore(saved);int id;world.TrySpawn(Species.Human,11,5,out id);saved=world.Capture();saved.actors[1].hunger=10;saved.actors[1].carriedFood=1;world=World.Restore(saved);
            world.TryConfigureDistributionCenter(4,3);world.TryAssignDistributionCourier(0,183);world.TryConfigureStore(13,3);world.TryAssignClerk(1,192);return world;
        }
        public static World StoreClerkTest()
        {
            SaveDocument saved=StorageSupplyTest().Capture();WorldMap map=WorldMap.Restore(saved.map);map.SetShelf(4,5,1,true);StorageBoxDocument crate=map.StorageBoxAt(4,3);crate.items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=12}};map.SetStorageBox(crate);saved.map=map.Capture();saved.map.name="Store clerk and consumer test";saved.actors[0].carriedFood=1;
            World world=World.Restore(saved);int id;world.TrySpawn(Species.Human,3,5,out id);saved=world.Capture();saved.actors[1].hunger=80;saved.actors[1].shelterId=27;saved.actors[1].shelterX=3;saved.actors[1].shelterY=5;saved.actors[1].shelterState=ShelterState.Inside;world=World.Restore(saved);world.TryConfigureStore(4,3);world.TryAssignClerk(0,27);return world;
        }
        public static World CourierScheduleTest()
        {
            SaveDocument saved=AssignedCourierTest().Capture();saved.tick=11*Rules.TicksPerGameHour+540;saved.map.name="Courier shift-end test";World world=World.Restore(saved);world.TrySetCourierSchedule(0,7,12);return world;
        }
        public static World HouseholdFoodTargetsTest()
        {
            SaveDocument saved=HouseholdManagementTest().Capture();WorldMap map=WorldMap.Restore(saved.map);map.SetStorageBox(new StorageBoxDocument{x=2,y=7,capacity=30,items=new ItemStackDocument[0]});saved.map=map.Capture();saved.map.name="Household food targets test";World world=World.Restore(saved);
            world.TrySetHouseholdMealsPerResident(183,3);world.TrySetHouseholdMealsPerResident(192,3);world.TryUseHouseholdFoodTarget(13,3);return world;
        }
        public static World HouseholdManagementTest()
        {
            World world=AssignedCourierTest();int id;world.TrySpawn(Species.Human,3,6,out id);world.TrySpawn(Species.Human,12,6,out id);world.TrySpawn(Species.Human,7,8,out id);
            SaveDocument saved=world.Capture();saved.map.name="Household management test";
            saved.actors[0].x=11;saved.actors[0].y=5;saved.actors[0].shelterId=192;saved.actors[0].shelterX=11;saved.actors[0].shelterY=5;
            for(int i=1;i<3;i++){saved.actors[i].assignedHouseId=i==1?183:192;saved.actors[i].shelterId=saved.actors[i].assignedHouseId;saved.actors[i].shelterX=saved.actors[i].x;saved.actors[i].shelterY=saved.actors[i].y;saved.actors[i].shelterState=ShelterState.Inside;}
            return World.Restore(saved);
        }
        public static World AssignedCourierTest()
        {
            WorldMap map=new WorldMap(60,12,"Assigned courier distances","assigned-courier-1");
            foreach(int left in new[]{1,10,40})
            {
                AddShelterRoom(map,left,2,false);
                if(left!=1){map.Set(left+4,5,Terrain.Wall);map.Set(left,5,Terrain.DoorClosed);}
            }
            foreach(int x in new[]{4,13,42})map.SetStorageBox(new StorageBoxDocument{x=x,y=3,capacity=30,foodReserve=x==4?(int?)null:2,items=x==4?new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=30}}:new ItemStackDocument[0]});
            World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,5,out id);SaveDocument saved=world.Capture();saved.actors[0].shelterId=world.HouseAt(3,5);saved.actors[0].shelterX=3;saved.actors[0].shelterY=5;saved.actors[0].shelterState=ShelterState.Inside;saved.actors[0].hunger=10;saved.actors[0].carriedFood=1;world=World.Restore(saved);
            world.TryAssignHouse(0,3,5);world.TrySetCourierSource(0,4,3);world.TryToggleCourierDestination(0,13,3);world.TryToggleCourierDestination(0,42,3);return world;
        }
        public static World BoxReportTest()
        {
            World world=BoxMemoryTest();int id;world.TrySpawn(Species.Human,6,4,out id);SaveDocument saved=world.Capture();saved.tick=5;saved.actors[0].hunger=10;saved.actors[1].hunger=80;saved.map.name="Shared box location test";return World.Restore(saved);
        }
        public static World BoxMemoryTest()
        {
            WorldMap map=new WorldMap(10,9,"Box memory test","box-memory-1");for(int y=0;y<=5;y++)map.Set(4,y,Terrain.Wall);map.SetStorageBox(new StorageBoxDocument{x=2,y=3,capacity=30,items=new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=6}}});World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,3,out id);world.Step();SaveDocument saved=world.Capture();saved.actors[0].x=6;saved.actors[0].y=3;saved.actors[0].hunger=80;saved.actors[0].energySpent=0;return World.Restore(saved); // Begin after a real observation, with the observer now around the solid corner.
        }
        public static World StorageRestockTest()
        {
            SaveDocument saved=StorageSupplyTest().Capture();WorldMap map=WorldMap.Restore(saved.map);map.SetShelf(9,3,12,true);saved.map=map.Capture();saved.map.name="Storage restocking test";saved.actors[0].carriedFood=1;saved.actors[0].shelfMemory=new[]{new ShelfMemoryRecord{x=9,y=3,food=12,observedTick=0}};return World.Restore(saved);
        }
        public static World StorageSupplyTest(bool hungry=false)
        {
            WorldMap map=new WorldMap(12,9,"Storage box supplies test","storage-supplies-1");for(int y=1;y<=6;y++)for(int x=1;x<=6;x++)map.Set(x,y,(x==1 || x==6 || y==1 || y==6)?Terrain.Wall:Terrain.Floor);map.Set(6,3,Terrain.DoorClosed);
            map.SetStorageBox(new StorageBoxDocument{x=4,y=3,capacity=StorageRules.DefaultCapacity,items=hungry?new[]{new ItemStackDocument{item=InventoryItem.Food,quantity=6}}:new ItemStackDocument[0]});
            World world=new World(map,214,encounter:true,hunger:true);int id;world.TrySpawn(Species.Human,3,3,out id);SaveDocument saved=world.Capture();saved.actors[0].shelterId=27;saved.actors[0].shelterX=3;saved.actors[0].shelterY=3;saved.actors[0].shelterState=ShelterState.Inside;saved.actors[0].hunger=hungry?80:10;saved.actors[0].carriedFood=hungry?0:3;return World.Restore(saved);
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
