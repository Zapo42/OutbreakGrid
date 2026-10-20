using System; // Validate capacity, stable identities, and saved actor state.
using System.Collections.Generic; // Index occupied cells instead of scanning the whole map.
using Outbreak.Contracts; // Reuse portable species and actor records.
namespace Outbreak.Core // Own simulation state independently of the Unity object lifecycle.
{ // Begin the first executable simulation slice.
    internal sealed class Occupancy // Track local body counts with storage proportional to active occupied cells.
    { // Begin spatial counts.
        private struct Counts { public int bodies, humans; } // Track the two agreed cell limits separately.
        private readonly Dictionary<int, Counts> cells = new Dictionary<int, Counts>(); // Avoid allocating a dense index for ten-thousand-cell worlds.
        public int Count { get { return cells.Count; } } // Expose a structural bound for long-run diagnostics.
        public bool Allows(int key, Species species) // Check a proposed arrival against current occupancy.
        { // Begin arrival validation.
            Counts count; cells.TryGetValue(key, out count); return count.bodies < Rules.BodiesPerCell && (species != Species.Human || count.humans < Rules.HumansPerCell); // Permit eight bodies with at most four humans per cell.
        } // Finish arrival validation.
        public void Change(int key, Species species, int delta) // Update the index at the same moment as authoritative movement.
        { // Begin one local count change.
            Counts count; cells.TryGetValue(key, out count); count.bodies += delta; if (species == Species.Human) count.humans += delta; // Keep human and total counts in lockstep.
            if (count.bodies == 0) cells.Remove(key); else cells[key] = count; // Drop empty cells so long runs cannot accumulate a trail of buckets.
        } // Finish one local count change.
    } // Finish spatial counts.
    public sealed partial class World // Provide deterministic movement and persistence before higher-level behavior is added.
    { // Begin the authoritative world.
        private readonly WorldMap map; private readonly ActorState[] actors; private readonly Occupancy occupancy = new Occupancy(); private readonly RandomStream random; // Keep all mutable simulation storage in one owner.
        private int count, nextId = 1; // Allocate stable identities monotonically.
        private static readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1, 0 }, dy = { 1, 1, 0, -1, -1, -1, 0, 1, 0 }; // Support diagonal movement and a deliberate idle choice.
        public int Count { get { return count; } } public int Capacity { get { return actors.Length; } } // Expose population metrics without exposing mutable arrays.
        public int Width { get { return map.Width; } } public int Height { get { return map.Height; } } // Allow adapters to frame the world without owning its terrain.
        public string Name { get { return map.Name; } } public int AllocatedChunks { get { return map.AllocatedChunks; } } public int OccupiedCells { get { return occupancy.Count; } } // Expose bounded storage diagnostics without exposing mutable indexes.
        public bool BiteEvasion { get; private set; }
        public bool WeaponDrops { get; private set; }
        public bool ShelterSpacing { get; private set; }
        public bool CombatExperience { get; private set; }
        public bool WeaponSeeking { get; private set; }
        public bool MeleeCombat { get; private set; }
        public bool FoodExploration { get; private set; }
        public bool FoodYieldChoice { get; private set; }
        public bool FoodRetreatFallback { get; private set; }
        public bool FoodTripRetreat { get; private set; }
        public bool SafeFoodTargets { get; private set; }
        public bool FairFoodPickup { get; private set; }
        public bool FoodRouteChoice { get; private set; }
        public bool DoorMovementNoise { get; private set; }
        public bool QuickStockPickup { get; private set; }
        public bool FoodRouteMemory { get; private set; }
        public bool SearchCoordination { get; private set; }
        public bool CarefulSearch { get; private set; }
        public bool SearchNoise { get; private set; }
        public bool FoodDelivery { get; private set; }
        public bool PantryTrips { get; private set; }
        public bool PantryStorage { get; private set; }
        public bool ShelfReports { get; private set; } // Older notebooks remain personal until explicitly enabling conversations.
        public bool ShelfMemoryEnabled { get; private set; } // Preserve earlier single-target food behavior until explicitly updated.
        public bool FoodSharing { get; private set; } // Preserve earlier food behavior until explicit opt-in.
        public bool HungerEnabled { get; private set; } // Opt controlled fixtures and earlier saves into needs explicitly.
        public bool DoorRepair { get; private set; } // Keep older runs on their original non-repairing rules.
        public bool WarningSheltering { get; private set; } // Preserve older runs until explicitly enabling shelter seeking after heard danger.
        public bool DoorBracing { get; private set; } // Older runs explicitly opt into physical door defense.
        public bool ZombieWarningInvestigation { get; private set; } // Explicitly opt older saves into attraction to human warnings.
        public bool HumanWarnings { get; private set; } // Preserve prior saves until explicitly enabling local human warnings.
        public bool ZombieSoundInvestigation { get; private set; } // Preserve earlier saves until explicitly enabling sound investigation.
        public bool DoorHearing { get; private set; } // Preserve prior saves until explicitly enabling nearby impact hearing.
        public bool DoorBreaking { get; private set; } // Preserve earlier saves until the user explicitly enables physical door attacks.
        public bool ZombieWandering { get; private set; } // Preserve older saved idle behavior until an explicit upgrade.
        public bool ShelterRecovery { get; private set; } // Keep older saved runs on their original shelter decisions.
        public bool DoorPursuit { get; private set; } // Preserve the closed-door pursuit fix as an explicit saved rules generation.
        public long Tick { get; private set; } // Advance the only simulation clock through Step.
        public long GameSeconds { get { return Tick * 6; } } // Derive game time from exact integer ticks.
        public World(WorldMap source, uint seed = 1, int capacity = Rules.DefaultCapacity, bool encounter = false, bool pursuitMemory = true, bool humanReactions = true, bool cautiousMovement = true, bool infectedEscape = true, bool shelters = true, bool doorPursuit = true, bool shelterRecovery = true, bool zombieWandering = true, bool doorBreaking = true, bool doorHearing = true, bool zombieSoundInvestigation = true, bool humanWarnings = true, bool zombieWarningInvestigation = true, bool doorBracing = true, bool warningSheltering = true, bool doorRepair = true, bool hunger = false, bool foodSharing = true, bool shelfMemory = true, bool shelfReports = true, bool pantryStorage = true, bool pantryTrips = true, bool foodDelivery = true, bool searchNoise = true, bool carefulSearch = true, bool searchCoordination = true, bool foodRouteMemory = true, bool quickStockPickup = true, bool doorMovementNoise = true, bool foodRouteChoice = true, bool fairFoodPickup = true, bool safeFoodTargets = true, bool foodTripRetreat = true, bool foodRetreatFallback = true, bool foodYieldChoice = true, bool foodExploration = true, bool meleeCombat = true, bool weaponSeeking = true, bool combatExperience = true, bool weaponDrops = true, bool shelterSpacing = true, bool biteEvasion = true) // Construct an isolated scenario instance.
        { // Begin world construction.
            if (source == null || capacity < 1 || capacity > Rules.DefaultCapacity) throw new ArgumentException("A valid map and population cap of 1–1000 are required."); // Keep the agreed default cap explicit for this first slice.
            Encounter = encounter; PursuitMemory = encounter && pursuitMemory; HumanReactions = PursuitMemory && humanReactions; CautiousMovement = HumanReactions && cautiousMovement; InfectedEscape = CautiousMovement && infectedEscape; SheltersEnabled = InfectedEscape && shelters; DoorPursuit = SheltersEnabled && doorPursuit; ShelterRecovery = DoorPursuit && shelterRecovery; ZombieWandering = ShelterRecovery && zombieWandering; DoorBreaking = ZombieWandering && doorBreaking; DoorHearing = DoorBreaking && doorHearing; ZombieSoundInvestigation = DoorHearing && zombieSoundInvestigation; HumanWarnings = ZombieSoundInvestigation && humanWarnings; ZombieWarningInvestigation = HumanWarnings && zombieWarningInvestigation; DoorBracing = ZombieWarningInvestigation && doorBracing; WarningSheltering = DoorBracing && warningSheltering; DoorRepair = WarningSheltering && doorRepair; HungerEnabled = DoorRepair && hunger; FoodSharing = HungerEnabled && foodSharing; ShelfMemoryEnabled = FoodSharing && shelfMemory; ShelfReports = ShelfMemoryEnabled && shelfReports; PantryStorage = ShelfReports && pantryStorage; PantryTrips = PantryStorage && pantryTrips; FoodDelivery = PantryTrips && foodDelivery; SearchNoise = FoodDelivery && searchNoise; CarefulSearch = SearchNoise && carefulSearch; SearchCoordination = CarefulSearch && searchCoordination; FoodRouteMemory = SearchCoordination && foodRouteMemory; QuickStockPickup = FoodRouteMemory && quickStockPickup; DoorMovementNoise = QuickStockPickup && doorMovementNoise; FoodRouteChoice = DoorMovementNoise && foodRouteChoice; FairFoodPickup = FoodRouteChoice && fairFoodPickup; SafeFoodTargets = FairFoodPickup && safeFoodTargets; FoodTripRetreat = SafeFoodTargets && foodTripRetreat; FoodRetreatFallback = FoodTripRetreat && foodRetreatFallback; FoodYieldChoice = FoodRetreatFallback && foodYieldChoice; FoodExploration = FoodYieldChoice && foodExploration; MeleeCombat = FoodExploration && meleeCombat; WeaponSeeking = MeleeCombat && weaponSeeking; CombatExperience = WeaponSeeking && combatExperience; WeaponDrops = CombatExperience && weaponDrops; ShelterSpacing = WeaponDrops && shelterSpacing; BiteEvasion = ShelterSpacing && biteEvasion; if(source.HasDroppedWeapons && !WeaponDrops)throw new ArgumentException("Dropped weapon piles require current encounter rules."); if(source.HasWeapons && !MeleeCombat)throw new ArgumentException("Ground weapons require melee encounter rules."); if (source.HasDoorDamage && !DoorBreaking) throw new ArgumentException("Door conditions require current encounter rules."); reactionRandom = new RandomStream(seed ^ 0xA341316Cu); map = WorldMap.Restore(source.Capture()); actors = new ActorState[capacity]; random = new RandomStream(seed); InitializeShelters(); // Detach source terrain so authoring cannot mutate a running world.
        } // Finish world construction.
        public Terrain TerrainAt(int x, int y) { return map.Get(x, y); } // Offer a read-only terrain query to rendering.
        public ActorState ActorAt(int index) { if (index < 0 || index >= count) throw new ArgumentOutOfRangeException("index"); return DetachActor(actors[index]); } // Return a struct copy to protect authoritative state.
        private int Key(int x, int y) { return y * map.Width + x; } // Flatten valid cells safely within the supported dimension bound.
        public bool TrySpawn(Species species, int x, int y, out int id) // Admit a body only when all placement constraints pass.
        { // Begin one explicit spawn command.
            id = 0; if ((species != Species.Human && species != Species.Zombie) || count == actors.Length || !map.CanWalk(x, y) || !occupancy.Allows(Key(x, y), species)) return false; // Leave state and randomness untouched after a rejected placement.
            ActorState actor = new ActorState { id = nextId++, x = x, y = y, species = species, strength = 3, perception = 3, endurance = 3, charisma = 3, intelligence = 3, agility = 3, luck = 3, morality = 0, bravery = 0, health = 100 }; // Use explicit baseline fixtures until weighted population generation is rebuilt.
            actors[count++] = actor; occupancy.Change(Key(x, y), species, 1); id = actor.id; return true; // Commit identity, storage, and occupancy together.
        } // Finish one explicit spawn command.
        public void Step() // Execute one logical tick with no dependency on frame timing or rendering.
        { // Begin a deterministic tick.
            if (Tick >= long.MaxValue / 6 - 1) throw new InvalidOperationException("Simulation clock limit reached."); // Keep the derived game clock inside its integer range.
            weaponActionCount=0; biteActionCount=0;
            Tick++; // Advance time once, even when there are no active bodies.
            if (Encounter) { StepEncounter(); return; } // Keep legacy fixture draws and movement unchanged.
            for (int i = 0; i < count; i++) // Visit actors in stable identity order.
            { // Begin baseline movement for one body.
                ActorState actor = actors[i]; if (Tick % 5 != actor.id % 5) continue; int direction = random.Below(9), nx = actor.x + dx[direction], ny = actor.y + dy[direction]; // Move each fixture at one cell per second, staggered across ticks.
                if ((nx == actor.x && ny == actor.y) || !map.CanWalk(nx, ny) || !occupancy.Allows(Key(nx, ny), actor.species)) continue; // Respect boundaries, solid terrain, and current local capacity.
                if (nx != actor.x && ny != actor.y && (!map.CanWalk(nx, actor.y) || !map.CanWalk(actor.x, ny))) continue; // Prevent diagonal movement through the corner of two barriers.
                occupancy.Change(Key(actor.x, actor.y), actor.species, -1); actor.x = nx; actor.y = ny; actors[i] = actor; occupancy.Change(Key(nx, ny), actor.species, 1); // Commit the move and spatial counts as one deterministic operation.
            } // Finish baseline movement for one body.
        } // Finish a deterministic tick.
        public SaveDocument Capture() // Snapshot every authoritative value and no presentation state.
        { // Begin world capture.
            ActorState[] output = new ActorState[count]; for (int i = 0; i < count; i++) output[i] = DetachActor(actors[i]); // Give persistence an isolated actor array.
            return new SaveDocument { rulesVersion = SavedRulesVersion(), map = map.Capture(), actors = output, tick = Tick, randomState = random.State, reactionRandomState = HumanReactions ? reactionRandom.State : 0, nextId = nextId, capacity = actors.Length }; // Preserve direct random continuation and exact future identities.
        } // Finish world capture.
        private string SavedRulesVersion() // Preserve each saved behavior generation explicitly.
        { // Begin version selection.
            return BiteEvasion ? EncounterRules.Version : ShelterSpacing ? EncounterRules.SpacingVersion : WeaponDrops ? EncounterRules.DropVersion : CombatExperience ? EncounterRules.ExperienceVersion : WeaponSeeking ? EncounterRules.WeaponSeekingVersion : MeleeCombat ? EncounterRules.MeleeVersion : FoodExploration ? EncounterRules.ExplorationVersion : FoodYieldChoice ? EncounterRules.YieldVersion : FoodRetreatFallback ? EncounterRules.AlternateShelterVersion : FoodTripRetreat ? EncounterRules.RetreatVersion : SafeFoodTargets ? EncounterRules.SafeFoodVersion : FairFoodPickup ? EncounterRules.FairPickupVersion : FoodRouteChoice ? EncounterRules.RouteChoiceVersion : DoorMovementNoise ? EncounterRules.DoorNoiseVersion : QuickStockPickup ? EncounterRules.PickupVersion : FoodRouteMemory ? EncounterRules.RouteMemoryVersion : SearchCoordination ? EncounterRules.CoordinationVersion : CarefulSearch ? EncounterRules.CarefulVersion : SearchNoise ? EncounterRules.NoiseVersion : FoodDelivery ? EncounterRules.DeliveryVersion : PantryTrips ? EncounterRules.PantryTripVersion : PantryStorage ? EncounterRules.PantryVersion : ShelfReports ? EncounterRules.ReportVersion : ShelfMemoryEnabled ? EncounterRules.ShelfMemoryVersion : FoodSharing ? EncounterRules.SharingVersion : HungerEnabled ? EncounterRules.HungerVersion : DoorRepair ? EncounterRules.RepairVersion : WarningSheltering ? EncounterRules.WarningShelterVersion : DoorBracing ? EncounterRules.BraceVersion : ZombieWarningInvestigation ? EncounterRules.AttractionVersion : HumanWarnings ? EncounterRules.WarningVersion : ZombieSoundInvestigation ? EncounterRules.InvestigationVersion : DoorHearing ? EncounterRules.HearingVersion : DoorBreaking ? EncounterRules.BreachVersion : ZombieWandering ? EncounterRules.WanderVersion : ShelterRecovery ? EncounterRules.RecoveryVersion : DoorPursuit ? EncounterRules.DoorVersion : SheltersEnabled ? EncounterRules.ShelterVersion : InfectedEscape ? EncounterRules.EscapeVersion : CautiousMovement ? EncounterRules.CautiousVersion : HumanReactions ? EncounterRules.ReactionVersion : PursuitMemory ? EncounterRules.MemoryVersion : Encounter ? EncounterRules.LegacyVersion : "foundation-1"; // Do not silently upgrade earlier runs.
        } // Finish version selection.
        private static int RulesGeneration(string version) // Centralize cumulative feature activation for previous release compatibility.
        { // Begin supported rule dispatch.
            switch (version) { case "foundation-1": return 0; case EncounterRules.LegacyVersion: return 1; case EncounterRules.MemoryVersion: return 2; case EncounterRules.ReactionVersion: return 3; case EncounterRules.CautiousVersion: return 4; case EncounterRules.EscapeVersion: return 5; case EncounterRules.ShelterVersion: return 6; case EncounterRules.DoorVersion: return 7; case EncounterRules.RecoveryVersion: return 8; case EncounterRules.WanderVersion: return 9; case EncounterRules.BreachVersion: return 10; case EncounterRules.HearingVersion: return 11; case EncounterRules.InvestigationVersion: return 12; case EncounterRules.WarningVersion: return 13; case EncounterRules.AttractionVersion: return 14; case EncounterRules.BraceVersion: return 15; case EncounterRules.WarningShelterVersion: return 16; case EncounterRules.RepairVersion: return 17; case EncounterRules.HungerVersion: return 18; case EncounterRules.SharingVersion: return 19; case EncounterRules.ShelfMemoryVersion: return 20; case EncounterRules.ReportVersion: return 21; case EncounterRules.PantryVersion: return 22; case EncounterRules.PantryTripVersion: return 23; case EncounterRules.DeliveryVersion: return 24; case EncounterRules.NoiseVersion: return 25; case EncounterRules.CarefulVersion: return 26; case EncounterRules.CoordinationVersion: return 27; case EncounterRules.RouteMemoryVersion: return 28; case EncounterRules.PickupVersion: return 29; case EncounterRules.DoorNoiseVersion: return 30; case EncounterRules.RouteChoiceVersion: return 31; case EncounterRules.FairPickupVersion: return 32; case EncounterRules.SafeFoodVersion: return 33; case EncounterRules.RetreatVersion: return 34; case EncounterRules.AlternateShelterVersion: return 35; case EncounterRules.YieldVersion: return 36; case EncounterRules.ExplorationVersion: return 37; case EncounterRules.MeleeVersion: return 38; case EncounterRules.WeaponSeekingVersion: return 39; case EncounterRules.ExperienceVersion: return 40; case EncounterRules.DropVersion: return 41; case EncounterRules.SpacingVersion: return 42; case EncounterRules.Version: return 43; default: return -1; } // Unknown future generations reject before world construction.
        } // Finish rule dispatch.
        public static World Restore(SaveDocument source) // Build and validate a new world before any caller swaps it into the UI.
        { // Begin transactional world restoration.
            int generation = source == null ? -1 : RulesGeneration(source.rulesVersion); // Determine all feature flags from the saved version, never guessed fields.
            if (source == null || source.kind != "outbreak.save" || source.schemaVersion != 1 || generation < 0 || source.tick < 0 || source.tick >= long.MaxValue / 6 || source.randomState == 0 || source.actors == null || source.nextId < 1) throw new ArgumentException("Unsupported or invalid foundation save."); // Refuse corrupt clocks and unsupported rules before accepting state.
            World world = new World(WorldMap.Restore(source.map), source.randomState, source.capacity, generation >= 1, generation >= 2, generation >= 3, generation >= 4, generation >= 5, generation >= 6, generation >= 7, generation >= 8, generation >= 9, generation >= 10, generation >= 11, generation >= 12, generation >= 13, generation >= 14, generation >= 15, generation >= 16, generation >= 17, generation >= 18, generation >= 19, generation >= 20, generation >= 21, generation >= 22, generation >= 23, generation >= 24, generation >= 25, generation >= 26, generation >= 27, generation >= 28, generation >= 29, generation >= 30, generation >= 31, generation >= 32, generation >= 33, generation >= 34, generation >= 35, generation >= 36, generation >= 37, generation >= 38, generation >= 39, generation >= 40, generation >= 41, generation >= 42, generation >= 43); // Activate exactly the features used by the saved run.
            if ((world.HumanReactions && source.reactionRandomState == 0) || (!world.HumanReactions && source.reactionRandomState != 0)) throw new ArgumentException("Invalid reaction random state."); // Refuse corrupt or incompatible named-stream state.
            if (world.HumanReactions) world.reactionRandom = new RandomStream(source.reactionRandomState); // Resume freeze choices directly without reseeding.
            if (source.actors.Length > source.capacity || source.nextId > int.MaxValue - source.capacity) throw new ArgumentException("Saved population or identity allocation exceeds capacity."); // Bound actor validation and leave enough integer space for future spawns.
            int previousId = 0; // Require a canonical identity order so later ticks remain reproducible.
            foreach (ActorState actor in source.actors) // Validate every actor without touching any existing live world.
            { // Begin one actor validation.
                if (actor.id <= previousId || actor.id >= source.nextId || (actor.species != Species.Human && actor.species != Species.Zombie) || !world.map.CanWalk(actor.x, actor.y) || !world.occupancy.Allows(world.Key(actor.x, actor.y), actor.species)) throw new ArgumentException("Invalid identity, species, placement, or crowding in save."); // Refuse duplicate IDs and inconsistent spatial state.
                if (actor.turnAtTick < 0 || (actor.turnAtTick != 0 && (!world.Encounter || actor.species != Species.Human || actor.turnAtTick <= source.tick || actor.turnAtTick - source.tick > EncounterRules.MaxTurnDelay))) throw new ArgumentException("Invalid infection deadline."); // Refuse incompatible or already overdue infection state.
                int[] stats = { actor.strength, actor.perception, actor.endurance, actor.charisma, actor.intelligence, actor.agility, actor.luck }; // Validate every effective SPECIAL statistic explicitly.
                if (Array.Exists(stats, value => value < 0 || value > 10) || actor.morality < -5 || actor.morality > 5 || actor.bravery < -3 || actor.bravery > 3 || (actor.health < 1 && !(world.MeleeCombat && actor.species==Species.Zombie && actor.health==0)) || actor.health > 100) throw new ArgumentException("Invalid actor statistics."); // Retain the source's signed morality range while applying the agreed signed bravery range.
                world.actors[world.count++] = DetachActor(actor); world.occupancy.Change(world.Key(actor.x, actor.y), actor.species, 1); previousId = actor.id; // Restore a coherent occupancy index from authoritative actor state.
            } // Finish one actor validation.
            world.Tick = source.tick; world.nextId = source.nextId; world.ValidatePursuit(); world.ValidateHumanReactions(); world.ValidateAvoidance(); world.ValidateShelters(); world.ValidateShelterRecovery(); world.ValidateDoorHearing(); world.ValidateHumanWarnings(); world.ValidateDoorBracing(); world.ValidateFoodRouteMemory(); world.ValidateHunger(); world.ValidateShelfMemory(); world.ValidateSearchHearing(); world.ValidateFoodExploration(); world.ValidateMelee(); return world; // Publish the replacement only after all validation succeeds.
        } // Finish transactional world restoration.
    } // Finish the authoritative world.
} // Finish the first executable simulation slice.
