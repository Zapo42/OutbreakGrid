using System; // Mark portable records for ordinary C# tools.
using System.Runtime.Serialization; // Define the same explicit JSON contract for every adapter.
namespace Outbreak.Contracts // Keep file contracts independent of Unity and simulation behavior.
{ // Begin portable records.
    [DataContract] public sealed class ChunkDocument // Store only terrain chunks that differ from ordinary grass.
    { // Begin one sparse chunk.
        [DataMember(IsRequired = true)] public int x, y; // Locate the chunk in chunk coordinates.
        [DataMember(IsRequired = true)] public ushort[] cells; // Store row-major terrain identities inside this chunk.
    } // Finish one sparse chunk.
    [DataContract] public sealed class DoorConditionDocument // Store only damaged or broken doors in a map.
    { // Begin sparse door condition.
        [DataMember(IsRequired = true)] public int x, y, health; // Zero health means a permanently open broken door.
    } // Finish door condition.
    [DataContract] public sealed class MapDocument // Separate starting terrain from an ongoing simulation save.
    { // Begin the shared map format.
        [DataMember(IsRequired = true)] public string kind = "outbreak.map"; // Refuse to interpret unrelated documents as maps.
        [DataMember(IsRequired = true)] public int schemaVersion = 1; // Reject unknown versions before changing a live world.
        [DataMember(IsRequired = true)] public string id = "", name = "Untitled"; // Keep stable identity separate from a display name.
        [DataMember(IsRequired = true)] public int width, height, chunkSize = 32; // Support variable dimensions without dense world allocations.
        [DataMember(IsRequired = true)] public ChunkDocument[] chunks = new ChunkDocument[0]; // Serialize chunks in canonical coordinate order.
        [DataMember(EmitDefaultValue = false)] public DoorConditionDocument[] doors; // Version-two maps retain damaged-door health; unchanged version-one maps omit this field.
        [DataMember(IsRequired = true)] public string authoringJson = ""; // Retain opaque future authoring data without interpreting or discarding it.
    } // Finish the shared map format.
    public enum Species { Human, Zombie } // Keep species identity independent of animation and sprite color.
    public enum HumanReaction { Calm, Aware, Fight, Freeze, Flight } // Store the actual decision separately from stress intensity.
    public enum StressLevel { Normal, Aware, Cautious, Alarmed, Afraid, Panicked } // Give the inspector the six retained stress labels.
    public enum ShelterState { None, Seeking, Inside } // Separate a chosen destination from confirmed entry into its floor region.
    public enum ShelterExitPhase { None, Approaching, Checking, Leaving } // Retain observed exit progress across saving and pausing.
    [DataContract] public struct ActorState // Return value copies so a renderer cannot mutate simulation state.
    { // Begin a stable actor record.
        [DataMember(IsRequired = true)] public int id, x, y; // Keep identity stable through movement and future conversion.
        [DataMember(IsRequired = true)] public Species species; // Store the actual biological state rather than a visual tint.
        [DataMember(IsRequired = true)] public int strength, perception, endurance, charisma, intelligence, agility, luck; // Reserve all seven agreed SPECIAL statistics.
        [DataMember(IsRequired = true)] public int morality, bravery, health; // Keep internal health on the agreed zero-to-one-hundred scale.
        [DataMember] public int heardWarningX, heardWarningY; // Remember the speaker cell rather than a hidden zombie.
        [DataMember] public long heardWarningUntilTick, nextWarningTick; // Bound listening and repeated warnings on the simulation clock.
        [DataMember] public int heardDoorX, heardDoorY; // Remember the impact cell rather than the hidden attacker.
        [DataMember] public long heardDoorUntilTick; // Keep recent hearing on the authoritative clock.
        [DataMember] public long turnAtTick; // Zero means uninfected; otherwise retain the authoritative turning deadline.
        [DataMember] public int targetId, lastSeenX, lastSeenY; // Store only the identity and cell actually observed by a pursuing zombie.
        [DataMember] public long searchUntilTick; // Bound pursuit memory using simulation time rather than wall time.
        [DataMember] public int stress, groupSupport, threatPressure; // Persist the bounded inputs to courage decisions and cached local support.
        [DataMember] public HumanReaction reaction; // Resume the exact current human response.
        [DataMember] public bool freezeResolved; // Avoid repeatedly freezing during the same stressful encounter.
        [DataMember] public long freezeUntilTick, supportUntilTick, nextShoveTick, staggerUntilTick; // Drive reaction, support observation and contact timing from the authoritative clock.
        [DataMember] public int avoidX, avoidY; // Remember only the zombie cell actually seen during infected escape.
        [DataMember] public long avoidUntilTick; // Retain avoidance briefly after losing sight without reading a hidden zombie position.
        [DataMember] public int shelterId, shelterX, shelterY, rejectedShelterId; // Persist a stable room destination and one temporarily rejected room.
        [DataMember] public ShelterState shelterState; // Retain shelter progress without claiming the room is permanently safe.
        [DataMember] public long shelterCalmSinceTick; // Require a calm interval before inspecting an exit.
        [DataMember] public ShelterExitPhase shelterExitPhase; // Separate approaching, checking and physically leaving.
        [DataMember] public int shelterExitDoor, shelterExitX, shelterExitY; // Save one door key plus one and its adjacent outdoor destination.
        [DataMember] public long rejectedUntilTick; // Bound unsafe-room avoidance on the same simulation clock.
    } // Finish a stable actor record.
    [DataContract] public sealed class SaveDocument // Include every authoritative value needed to continue this foundation.
    { // Begin a running-save envelope.
        [DataMember(IsRequired = true)] public string kind = "outbreak.save"; // Keep a running save distinct from a reusable map.
        [DataMember(IsRequired = true)] public int schemaVersion = 1; // Make later behavior migrations explicit.
        [DataMember(IsRequired = true)] public string rulesVersion = "foundation-1"; // Reject saves produced by incompatible simulation rules.
        [DataMember(IsRequired = true)] public MapDocument map; // Own the exact terrain and preserved authoring payload.
        [DataMember(IsRequired = true)] public long tick; // Restore the logical clock without replaying elapsed time.
        [DataMember] public uint reactionRandomState; // Keep variable freeze durations independent of wandering draws.
        [DataMember(IsRequired = true)] public uint randomState; // Resume random draws directly rather than reseeding and replaying them.
        [DataMember(IsRequired = true)] public int nextId, capacity; // Preserve future identity allocation and the population limit.
        [DataMember(IsRequired = true)] public ActorState[] actors = new ActorState[0]; // Preserve canonical ascending actor order.
    } // Finish a running-save envelope.
} // Finish portable records.
