using System; // Inspect the actual platform binding without invoking a Windows DLL on another host.
using System.IO; // Append the exact platform-check scope to the acceptance evidence.
using System.Reflection; // Read the private native record used by production code.
using System.Runtime.InteropServices; // Verify pointer-sized layout and documented Windows field offsets.
using Outbreak.UnityAdapter; // Compile the actual delivered Windows dialog implementation.
public static class WindowsDialogChecks // Keep ABI checks distinct from native dialog execution.
{ // Begin platform signature acceptance.
    public static int Main(string[] args) // Return a failure if the actual native record has an invalid layout.
    { // Begin the platform check.
        Type record = typeof(FileDialogs).GetNestedType("OpenFileName", BindingFlags.NonPublic); int expectedSize = IntPtr.Size == 8 ? 152 : 88, expectedFile = IntPtr.Size == 8 ? 48 : 28; // Use OPENFILENAMEW's documented pointer-dependent layout.
        if (record == null || Marshal.SizeOf(record) != expectedSize || Marshal.OffsetOf(record, "file").ToInt32() != expectedFile || Marshal.OffsetOf(record, "maximumFile").ToInt32() != expectedFile + IntPtr.Size) { Console.Error.WriteLine("Windows dialog ABI mismatch."); return 1; } // Check the filename pointer and capacity used by the real file browser.
        string message = "PASS Windows Open/Save branch strict compilation and " + (IntPtr.Size * 8) + "-bit OPENFILENAMEW layout. Native Windows dialogs were not executed."; Console.WriteLine(message); if (args.Length == 1) File.AppendAllText(args[0], message + Environment.NewLine); return 0; // Preserve a narrow truthful result beside the core evidence.
    } // Finish the platform check.
} // Finish platform signature acceptance.
