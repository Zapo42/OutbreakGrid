using System; // Report invalid files without changing a running world.
using System.IO; // Bound reads and replace complete files atomically.
using System.Runtime.Serialization.Json; // Use a platform serializer without adding an engine dependency.
using System.Text; // Keep all saved text in explicit UTF-8.
using Outbreak.Contracts; // Serialize the shared versioned envelopes.
using Outbreak.Core; // Validate reconstructed state before exposing it to callers.
namespace Outbreak.Persistence // Isolate file storage from simulation and screen code.
{ // Begin versioned persistence.
    public static class Documents // Expose transactional map and running-save operations.
    { // Begin portable JSON and local file operations.
        public const int MaximumBytes = 32 * 1024 * 1024; // Bound this foundation's file input before allocating parser state.
        public static string Encode<T>(T value) // Serialize a complete detached document with the shared contract.
        { // Begin JSON encoding.
            using (MemoryStream stream = new MemoryStream()) { new DataContractJsonSerializer(typeof(T)).WriteObject(stream, value); return Encoding.UTF8.GetString(stream.ToArray()); } // Keep serializer state local to this operation.
        } // Finish JSON encoding.
        private static T Decode<T>(string json) // Parse only a bounded UTF-8 document of the requested kind.
        { // Begin JSON decoding.
            if (json == null || json.Length > MaximumBytes || Encoding.UTF8.GetByteCount(json) > MaximumBytes) throw new ArgumentException("File exceeds the foundation's 32 MiB input limit."); // Reject oversized text before parser allocation.
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) { return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream); } // Require the explicit data members declared in the portable contract.
        } // Finish JSON decoding.
        public static WorldMap ParseMap(string json) { return WorldMap.Restore(Decode<MapDocument>(json)); } // Validate all map records before returning a replacement.
        public static World ParseSave(string json) { return World.Restore(Decode<SaveDocument>(json)); } // Validate clocks, identities, occupancy, terrain, and random state together.
        public static string Read(string path) // Read a file only after checking its declared size.
        { // Begin a bounded file read.
            using (FileStream stream = File.OpenRead(path)) // Keep one handle open across the size check and read.
            { // Begin the checked read scope.
                if (stream.Length > MaximumBytes) throw new IOException("File exceeds the foundation's 32 MiB input limit."); // Refuse oversized files before creating the string.
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true)) // Respect a UTF-8 byte-order marker when present.
                { // Begin incremental reading.
                    StringBuilder result = new StringBuilder(); char[] buffer = new char[4096]; int count; // Keep temporary memory bounded even if a file grows during reading.
                    while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) { if (result.Length + count > MaximumBytes) throw new IOException("File grew beyond the input limit."); result.Append(buffer, 0, count); } // Recheck length during the read instead of trusting the original metadata.
                    return result.ToString(); // Return the complete candidate for transactional validation.
                } // Finish incremental reading.
            } // Finish the checked read scope.
        } // Finish a bounded file read.
        public static void Write(string path, string json) // Replace an entire validated output without exposing a partially written destination.
        { // Begin atomic file replacement.
            if (string.IsNullOrWhiteSpace(path) || json == null) throw new ArgumentException("Choose an output file."); // Require a concrete destination before creating temporary bytes.
            byte[] bytes = Encoding.UTF8.GetBytes(json); if (bytes.Length > MaximumBytes) throw new IOException("Output exceeds this foundation's 32 MiB file limit."); // Keep generated files inside the corresponding reader limit.
            string destination = Path.GetFullPath(path), folder = Path.GetDirectoryName(destination); Directory.CreateDirectory(folder); // Resolve the final parent before creating the adjacent temporary file.
            string temporary = Path.Combine(folder, "." + Path.GetFileName(destination) + "." + Guid.NewGuid().ToString("N") + ".tmp"); // Keep the temporary file on the same volume for atomic replacement.
            try // Preserve the old destination if any preparation step fails.
            { // Begin complete temporary output.
                using (FileStream stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); } // Finish and flush the entire document before publication.
                if (File.Exists(destination)) File.Replace(temporary, destination, null); else File.Move(temporary, destination); // Use the filesystem's whole-file commit operation for both save cases.
            } // Finish complete temporary output.
            finally { if (File.Exists(temporary)) File.Delete(temporary); } // Remove an abandoned temporary file without deleting the previous destination.
        } // Finish atomic file replacement.
    } // Finish portable JSON and local file operations.
} // Finish versioned persistence.
