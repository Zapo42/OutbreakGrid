using System; // Validate public inputs with ordinary C# exceptions.
namespace Outbreak.Core // Keep authoritative rules outside the engine adapter.
{ // Begin foundational rules.
    public static class Rules // Centralize locked values instead of scattering literals through screens.
    { // Begin shared constants.
        public const int TicksPerSecond = 5, TicksPerGameHour = 600, DefaultCapacity = 1000; // Preserve five TPS and one game hour per two real minutes.
        public const int MaxDimension = 10000, ChunkSize = 32, HumansPerCell = 4, BodiesPerCell = 8; // Separate authoring bounds from the one-thousand-cell benchmark target.
        public static int Bravery(int strength, int perception, int charisma) // Apply the approved formula with explicit midpoint rounding.
        { // Begin stat derivation.
            int baseline = (int)Math.Round(((strength + perception) / 2.0 - 3.0) * 1.5, MidpointRounding.AwayFromZero); // Make half-point behavior stable across hosts.
            return Math.Max(-3, Math.Min(3, baseline + charisma - 3)); // Clamp the resulting courage modifier to its agreed range.
        } // Finish stat derivation.
    } // Finish shared constants.
    public enum Terrain : ushort { Grass, Grass2, Grass3, Grass4, Grass5, Sidewalk, Road, Floor, Wall, DoorClosed, DoorOpen, Window, Water, Fence, GateClosed, GateOpen, Crosswalk } // Give stock terrain stable identities independent of artwork.
    public static class TerrainRules // Supply the same collision rule to maps and simulation.
    { // Begin foundational collision.
        public static bool RoadSurface(Terrain value) { return value==Terrain.Road || value==Terrain.Crosswalk; } // Shared roadway ground for future vehicles; sidewalks remain separate.
        public static bool Known(ushort value) { return value <= (ushort)Terrain.Crosswalk; } // Reject unknown terrain instead of treating it as grass.
        public static bool Walkable(Terrain value) { return value != Terrain.Wall && value != Terrain.DoorClosed && value != Terrain.Window && value != Terrain.Fence && value != Terrain.GateClosed; } // Keep closed barriers solid in the movement slice.
    } // Finish foundational collision.
    public sealed class RandomStream // Own a portable random stream with one directly serializable state.
    { // Begin deterministic randomness.
        public uint State { get; private set; } // Expose state for snapshots without allowing rendering to change it.
        public RandomStream(uint seed) { State = seed == 0 ? 0x9E3779B9u : seed; } // Avoid the absorbing zero state when creating a scenario.
        public uint Next() // Advance exactly once for each requested simulation draw.
        { // Begin the fixed xorshift algorithm.
            uint value = State; value ^= value << 13; value ^= value >> 17; value ^= value << 5; State = value; return value; // Specify unsigned arithmetic so host random libraries cannot alter replays.
        } // Finish one random draw.
        public int Below(int limit) // Choose an integer without modulo bias.
        { // Begin bounded selection.
            if (limit <= 0) throw new ArgumentOutOfRangeException("limit"); // Refuse impossible selections before consuming the stream.
            uint bound = (uint)limit, threshold = unchecked(0u - bound) % bound, value; // Compute the short rejected prefix of the unsigned range.
            do { value = Next(); } while (value < threshold); return (int)(value % bound); // Accept only a range divisible by the requested bound.
        } // Finish bounded selection.
    } // Finish deterministic randomness.
} // Finish foundational rules.
