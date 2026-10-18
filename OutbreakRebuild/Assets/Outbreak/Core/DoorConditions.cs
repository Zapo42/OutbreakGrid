using System; // Validate sparse saved condition records before publishing a map.
using System.Collections.Generic; // Store condition only for doors that have actually taken damage.
using Outbreak.Contracts; // Keep door health in both running saves and exported starting maps.
namespace Outbreak.Core // Own condition and attacks independently of presentation.
{ // Begin persistent door condition.
    public static class DoorRules // Publish retained durability and provisional attack tuning.
    { // Begin versioned prototype tuning.
        public const int MaxHealth = 100, BaseDamage = 10, StrengthDamage = 2, MaxConditionRecords = 100000; // A baseline STR-three zombie removes sixteen HP per one-second attack opportunity.
    } // Finish door tuning.
    public sealed partial class WorldMap // Extend sparse terrain without allocating health for every map cell.
    { // Begin sparse condition ownership.
        private readonly Dictionary<int, int> doorHealth = new Dictionary<int, int>(); // Missing records mean intact one-hundred-HP doors.
        public bool HasDoorDamage { get { return doorHealth.Count != 0; } } // Keep unchanged maps in the original version-one format.
        public int DoorHealthAt(int x, int y) // Read door health without allocating state or changing the world.
        { // Begin a read-only condition query.
            if (!Contains(x, y) || (Get(x, y) != Terrain.DoorClosed && Get(x, y) != Terrain.DoorOpen)) return -1; int health; return doorHealth.TryGetValue(y * Width + x, out health) ? health : DoorRules.MaxHealth; // Minus one distinguishes non-door terrain.
        } // Finish condition query.
        public void SetDoorHealth(int x, int y, int health) // Support explicit condition authoring while keeping broken terrain physically passable.
        { // Begin one validated condition edit.
            if (health < 0 || health > DoorRules.MaxHealth || DoorHealthAt(x, y) < 0) throw new ArgumentException("Door health requires a door cell and a value from zero to one hundred."); int key = y * Width + x; if (health == DoorRules.MaxHealth) doorHealth.Remove(key); else { if (!doorHealth.ContainsKey(key) && doorHealth.Count == DoorRules.MaxConditionRecords) throw new InvalidOperationException("Door condition limit reached."); doorHealth[key] = health; } if (health == 0) Set(x, y, Terrain.DoorOpen); // A break persists as an open doorway rather than an invisible solid barrier.
        } // Finish condition edit.
        private DoorConditionDocument[] CaptureDoorConditions() // Return detached, canonically ordered sparse health records.
        { // Begin condition capture.
            if (doorHealth.Count == 0) return null; List<int> keys = new List<int>(doorHealth.Keys); keys.Sort(); DoorConditionDocument[] result = new DoorConditionDocument[keys.Count]; for (int i = 0; i < keys.Count; i++) result[i] = new DoorConditionDocument { x = keys[i] % Width, y = keys[i] / Width, health = doorHealth[keys[i]] }; return result; // Caller mutation cannot change the map's live state.
        } // Finish condition capture.
        private void RestoreDoorConditions(MapDocument source) // Refuse malformed or unsupported door-bearing map versions.
        { // Begin condition loading after terrain validation.
            if (source.schemaVersion == 1) { if (source.doors != null) throw new ArgumentException("Version-one maps cannot contain door condition records."); return; } if (source.doors == null || source.doors.Length == 0 || source.doors.Length > DoorRules.MaxConditionRecords) throw new ArgumentException("Invalid door condition collection."); // New condition-bearing maps require version two so older builds reject them rather than silently heal doors.
            foreach (DoorConditionDocument door in source.doors) { if (door == null || !Contains(door.x, door.y) || DoorHealthAt(door.x, door.y) < 0 || door.health < 0 || door.health >= DoorRules.MaxHealth || (door.health == 0 && Get(door.x, door.y) != Terrain.DoorOpen)) throw new ArgumentException("Invalid door condition."); int key = door.y * Width + door.x; if (doorHealth.ContainsKey(key)) throw new ArgumentException("Duplicate door condition."); doorHealth.Add(key, door.health); } // Validate health, geometry and unique ownership before publishing the map.
        } // Finish condition restoration.
    } // Finish sparse map condition.
    public sealed partial class World // Apply door attacks through the existing authoritative actor clock.
    { // Begin attack execution and read-only presentation queries.
        public long DoorConditionRevision { get; private set; } // Refresh damaged-door visuals without snapping movement when collision has not changed.
        public int DoorHealthAt(int x, int y) { return map.DoorHealthAt(x, y); } // Inspect actual authoritative durability.
        private bool TryAttackDoor(ActorState actor, int direction) // Attack only a closed door that blocks the active remembered route.
        { // Begin a complete attack opportunity.
            if (!DoorBreaking || actor.species != Species.Zombie || actor.targetId == 0 || actor.searchUntilTick <= Tick || direction < 0 || direction >= 8 || (dx[direction] != 0 && dy[direction] != 0)) return false; int x = actor.x + dx[direction], y = actor.y + dy[direction]; if (!map.Contains(x, y) || map.Get(x, y) != Terrain.DoorClosed) return false; int damage = DoorRules.BaseDamage + DoorRules.StrengthDamage * actor.strength; if (DoorBracing && DoorIsBraced(x, y)) damage = Math.Max(1, damage / 2); int health = Math.Max(0, map.DoorHealthAt(x, y) - damage); map.SetDoorHealth(x, y, health); DoorConditionRevision++; if (health == 0) { TerrainRevision++; ReleaseDoorBraces(Key(x, y) + 1); } if (DoorHearing) EmitDoorImpact(x, y, actor.id); return true; // Attacking consumes this opportunity; entering the breached doorway waits until a later decision.
        } // Finish door attack.
    } // Finish authoritative door execution.
} // Finish persistent door condition.
