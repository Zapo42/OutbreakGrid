using System; // Report invalid dimensions and corrupt chunk records.
using System.Collections.Generic; // Allocate terrain only where authored cells differ from default grass.
using Outbreak.Contracts; // Share exactly one document type with persistence and future editor tools.
namespace Outbreak.Core // Keep the grid independent of textures, cameras, and scene objects.
{ // Begin sparse map storage.
    public sealed partial class WorldMap // Bound logical dimensions without allocating every logical cell.
    { // Begin the authoritative terrain owner.
        private readonly Dictionary<int, ushort[]> chunks = new Dictionary<int, ushort[]>(); // Retain only changed thirty-two-cell chunks.
        private readonly int columns; // Encode chunk coordinates without collisions.
        public int Width { get; private set; } public int Height { get; private set; } // Keep dimensions immutable after validation.
        public string Id { get; private set; } public string Name { get; private set; } public string AuthoringJson { get; private set; } // Carry authoring identity without changing its payload.
        public int AllocatedChunks { get { return chunks.Count; } } // Expose a useful memory metric without exposing mutable storage.
        public WorldMap(int width, int height, string name = "Untitled", string id = "", string authoringJson = "") // Create a clean variable-sized world.
        { // Begin map construction.
            if (width < 1 || height < 1 || width > Rules.MaxDimension || height > Rules.MaxDimension) throw new ArgumentOutOfRangeException("width", "Map dimensions must be between 1 and 10000."); // Reject invalid bounds before multiplication or allocation.
            Width = width; Height = height; columns = (width + Rules.ChunkSize - 1) / Rules.ChunkSize; Name = name ?? "Untitled"; Id = id ?? ""; AuthoringJson = authoringJson ?? ""; // Keep metadata usable even for newly created documents.
        } // Finish map construction.
        public bool Contains(int x, int y) { return x >= 0 && y >= 0 && x < Width && y < Height; } // Use the same boundary test everywhere.
        public Terrain Get(int x, int y) // Read absent chunks as ordinary grass without allocating them.
        { // Begin a terrain read.
            if (!Contains(x, y)) throw new ArgumentOutOfRangeException("x"); // Never silently wrap an invalid grid coordinate.
            ushort[] cells; return chunks.TryGetValue((y / Rules.ChunkSize) * columns + x / Rules.ChunkSize, out cells) ? (Terrain)cells[(y % Rules.ChunkSize) * Rules.ChunkSize + x % Rules.ChunkSize] : Terrain.Grass; // Resolve one constant-time chunk lookup.
        } // Finish a terrain read.
        public bool CanWalk(int x, int y) { return Contains(x, y) && TerrainRules.Walkable(Get(x, y)) && !shelves.ContainsKey(y * Width + x); } // Treat the world edge as a solid boundary.
        public void Set(int x, int y, Terrain terrain) // Apply a terrain edit only to this independent map instance.
        { // Begin a terrain write.
            if (!Contains(x, y) || !TerrainRules.Known((ushort)terrain)) throw new ArgumentOutOfRangeException("terrain"); // Validate both address and meaning before mutation.
            if (terrain == Terrain.DoorClosed && DoorHealthAt(x, y) == 0) throw new ArgumentException("A broken door cannot be closed."); if (terrain != Terrain.DoorClosed && terrain != Terrain.DoorOpen) doorHealth.Remove(y * Width + x); // Replacing a door removes its condition; closing a breach requires explicit repair.
            if (!TerrainRules.Walkable(terrain) || terrain == Terrain.DoorOpen) shelves.Remove(y * Width + x); if(!TerrainRules.Walkable(terrain) && terrain!=Terrain.DoorClosed) weapons.Remove(y*Width+x); int key = (y / Rules.ChunkSize) * columns + x / Rules.ChunkSize; ushort[] cells; // Locate the owning chunk once.
            if (!chunks.TryGetValue(key, out cells)) { if (terrain == Terrain.Grass) return; cells = new ushort[Rules.ChunkSize * Rules.ChunkSize]; chunks.Add(key, cells); } // Avoid allocating an unchanged default cell.
            cells[(y % Rules.ChunkSize) * Rules.ChunkSize + x % Rules.ChunkSize] = (ushort)terrain; // Replace only the requested logical cell.
            if (terrain == Terrain.Grass && Array.TrueForAll(cells, value => value == 0)) chunks.Remove(key); // Reclaim a chunk when its last authored tile is erased.
        } // Finish a terrain write.
        internal int[] ShelterFloorCells(int limit) // Discover authored floors without scanning every empty cell of a sparse map.
        { // Begin bounded floor collection.
            List<int> result = new List<int>(); foreach (KeyValuePair<int, ushort[]> chunk in chunks) for (int i = 0; i < chunk.Value.Length; i++) if (chunk.Value[i] == (ushort)Terrain.Floor) { if (result.Count == limit) return null; int x = chunk.Key % columns * 32 + i % 32, y = chunk.Key / columns * 32 + i / 32; result.Add(y * Width + x); } result.Sort(); return result.ToArray(); // Canonical floor order gives stable room identities after save/load.
        } // Finish floor discovery.
        public MapDocument Capture() // Produce a defensive and canonically ordered map document.
        { // Begin map capture.
            List<int> keys = new List<int>(chunks.Keys); keys.Sort(); ChunkDocument[] output = new ChunkDocument[keys.Count]; // Make dictionary insertion order irrelevant to saved bytes.
            for (int i = 0; i < keys.Count; i++) output[i] = new ChunkDocument { x = keys[i] % columns, y = keys[i] / columns, cells = (ushort[])chunks[keys[i]].Clone() }; // Keep all mutable arrays detached from the live map.
            return new MapDocument { schemaVersion = HasDroppedWeapons ? 5 : HasWeapons ? 4 : HasShelves ? 3 : HasDoorDamage ? 2 : 1, weapons = CaptureWeapons(), shelves = CaptureShelves(), doors = CaptureDoorConditions(), width = Width, height = Height, id = Id, name = Name, authoringJson = AuthoringJson, chunks = output }; // Retain the unmodified authoring payload beside terrain.
        } // Finish map capture.
        public static WorldMap Restore(MapDocument source) // Validate the whole replacement before returning a new map.
        { // Begin transactional map loading.
            if (source == null || source.kind != "outbreak.map" || (source.schemaVersion != 1 && source.schemaVersion != 2 && source.schemaVersion != 3 && source.schemaVersion != 4 && source.schemaVersion != 5) || source.chunkSize != Rules.ChunkSize || source.chunks == null || source.chunks.Length > 100000) throw new ArgumentException("Unsupported or invalid foundation map."); // Fail explicitly for old and future formats rather than guessing.
            WorldMap map = new WorldMap(source.width, source.height, source.name, source.id, source.authoringJson); // Validate dimensions before accepting any chunk.
            foreach (ChunkDocument chunk in source.chunks) // Validate every terrain record in the candidate document.
            { // Begin one chunk validation.
                if (chunk == null || chunk.x < 0 || chunk.y < 0 || chunk.x >= map.columns || chunk.y >= (map.Height + 31) / 32 || chunk.cells == null || chunk.cells.Length != 1024) throw new ArgumentException("Invalid terrain chunk."); // Bound coordinates and fixed chunk size before indexing.
                int key = chunk.y * map.columns + chunk.x; if (map.chunks.ContainsKey(key)) throw new ArgumentException("Duplicate terrain chunk."); // Refuse ambiguous repeated records.
                for (int i = 0; i < chunk.cells.Length; i++) if (!TerrainRules.Known(chunk.cells[i]) || ((chunk.x * 32 + i % 32 >= map.Width || chunk.y * 32 + i / 32 >= map.Height) && chunk.cells[i] != 0)) throw new ArgumentException("Unknown terrain or nonempty cells outside map bounds."); // Reject hidden data beyond partial edge chunks.
                map.chunks.Add(key, (ushort[])chunk.cells.Clone()); // Own the validated data without retaining caller aliases.
            } // Finish one chunk validation.
            map.RestoreDoorConditions(source); map.RestoreShelves(source); map.RestoreWeapons(source); return map; // Publish the candidate only after terrain and door conditions have passed.
        } // Finish transactional map loading.
    } // Finish the authoritative terrain owner.
} // Finish sparse map storage.
