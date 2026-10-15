using System; // Mark portable records for ordinary C# tools.
using System.Runtime.Serialization; // Define the same explicit JSON contract for every adapter.
namespace Outbreak.Contracts // Keep file contracts independent of Unity and simulation behavior.
{ // Begin portable records.
    [DataContract] public sealed class ChunkDocument // Store only terrain chunks that differ from ordinary grass.
    { // Begin one sparse chunk.
        [DataMember(IsRequired = true)] public int x, y; // Locate the chunk in chunk coordinates.
        [DataMember(IsRequired = true)] public ushort[] cells; // Store row-major terrain identities inside this chunk.
    } // Finish one sparse chunk.
    [DataContract] public sealed class MapDocument // Separate starting terrain from an ongoing simulation save.
    { // Begin the shared map format.
        [DataMember(IsRequired = true)] public string kind = "outbreak.map"; // Refuse to interpret unrelated documents as maps.
        [DataMember(IsRequired = true)] public int schemaVersion = 1; // Reject unknown versions before changing a live world.
        [DataMember(IsRequired = true)] public string id = "", name = "Untitled"; // Keep stable identity separate from a display name.
        [DataMember(IsRequired = true)] public int width, height, chunkSize = 32; // Support variable dimensions without dense world allocations.
        [DataMember(IsRequired = true)] public ChunkDocument[] chunks = new ChunkDocument[0]; // Serialize chunks in canonical coordinate order.
        [DataMember(IsRequired = true)] public string authoringJson = ""; // Retain opaque future authoring data without interpreting or discarding it.
    } // Finish the shared map format.
    public enum Species { Human, Zombie } // Keep species identity independent of animation and sprite color.
    [DataContract] public struct ActorState // Return value copies so a renderer cannot mutate simulation state.
    { // Begin a stable actor record.
        [DataMember(IsRequired = true)] public int id, x, y; // Keep identity stable through movement and future conversion.
        [DataMember(IsRequired = true)] public Species species; // Store the actual biological state rather than a visual tint.
        [DataMember(IsRequired = true)] public int strength, perception, endurance, charisma, intelligence, agility, luck; // Reserve all seven agreed SPECIAL statistics.
        [DataMember(IsRequired = true)] public int morality, bravery, health; // Keep internal health on the agreed zero-to-one-hundred scale.
        [DataMember] public long turnAtTick; // Zero means uninfected; otherwise retain the authoritative turning deadline.
    } // Finish a stable actor record.
    [DataContract] public sealed class SaveDocument // Include every authoritative value needed to continue this foundation.
    { // Begin a running-save envelope.
        [DataMember(IsRequired = true)] public string kind = "outbreak.save"; // Keep a running save distinct from a reusable map.
        [DataMember(IsRequired = true)] public int schemaVersion = 1; // Make later behavior migrations explicit.
        [DataMember(IsRequired = true)] public string rulesVersion = "foundation-1"; // Reject saves produced by incompatible simulation rules.
        [DataMember(IsRequired = true)] public MapDocument map; // Own the exact terrain and preserved authoring payload.
        [DataMember(IsRequired = true)] public long tick; // Restore the logical clock without replaying elapsed time.
        [DataMember(IsRequired = true)] public uint randomState; // Resume random draws directly rather than reseeding and replaying them.
        [DataMember(IsRequired = true)] public int nextId, capacity; // Preserve future identity allocation and the population limit.
        [DataMember(IsRequired = true)] public ActorState[] actors = new ActorState[0]; // Preserve canonical ascending actor order.
    } // Finish a running-save envelope.
} // Finish portable records.
