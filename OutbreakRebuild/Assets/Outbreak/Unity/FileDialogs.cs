using System; // Report platform failures without changing a running world.
using System.IO; // Start browsing at a usable folder.
using System.Runtime.InteropServices; // Open the real Windows file browser after building the player.
namespace Outbreak.UnityAdapter // Keep native operating-system bindings outside the simulation.
{ // Begin the file dialog adapter.
    public static class FileDialogs // Reuse one dialog boundary for maps and running saves.
    { // Begin platform selection.
        public static string Choose(bool save, bool running, string folder) // Return an empty string when a person cancels.
        { // Begin one dialog request.
            string extension = running ? "obsave2" : "obmap2", title = (save ? "Save " : "Load ") + (running ? "Run" : "Map"); // Keep map and save choices clearly named.
#if UNITY_EDITOR // Use Unity's native desktop dialogs during editor testing.
            return save ? UnityEditor.EditorUtility.SaveFilePanel(title, folder, running ? "OutbreakRun" : "OutbreakMap", extension) : UnityEditor.EditorUtility.OpenFilePanel(title, folder, extension); // Provide actual navigation rather than a preset filename.
#elif UNITY_STANDALONE_WIN // Keep native browsing available in the Windows EXE.
            IntPtr buffer = Marshal.AllocHGlobal(65536), filter = Marshal.StringToHGlobalUni(title + "\0*." + extension + "\0\0"); // Allocate a Unicode path and a double-null-terminated filter.
            try // Always release native memory, including after cancellation.
            { // Begin the Windows common dialog call.
                Marshal.WriteInt16(buffer, 0); OpenFileName data = new OpenFileName { size = Marshal.SizeOf(typeof(OpenFileName)), owner = GetActiveWindow(), filter = filter, file = buffer, maximumFile = 32768, initialFolder = Directory.Exists(folder) ? folder : null, title = title, defaultExtension = extension, filterIndex = 1, flags = 0x00080000 | 0x00000008 | 0x00000800 | (save ? 0x00000002 : 0x00001000) }; // Preserve the working directory and confirm overwrites for save dialogs.
                bool accepted = save ? GetSaveFileName(ref data) : GetOpenFileName(ref data); if (accepted) return Marshal.PtrToStringUni(buffer); // Return the complete selected path after a successful dialog.
                int error = CommDlgExtendedError(); if (error != 0) throw new IOException("Windows file browser error " + error + "."); return ""; // Distinguish cancellation from native failure.
            } // Finish the Windows common dialog call.
            finally { Marshal.FreeHGlobal(buffer); Marshal.FreeHGlobal(filter); } // Release both buffers exactly once.
#else // Make unsupported platforms explicit during this Windows-first foundation.
            throw new PlatformNotSupportedException("This foundation has native file dialogs in Unity Editor and Windows players."); // Avoid claiming a browser opened when no supported adapter exists.
#endif // Finish platform-specific selection.
        } // Finish one dialog request.
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR // Compile Windows signatures only where they can be called.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)] private struct OpenFileName // Match the pointer and WORD fields of OPENFILENAMEW.
        { // Begin the native record.
            public int size; public IntPtr owner, instance, filter, customFilter; public int maximumCustomFilter, filterIndex; public IntPtr file; public int maximumFile; public IntPtr fileTitle; public int maximumFileTitle; public string initialFolder, title; public int flags; public short fileOffset, extensionOffset; public string defaultExtension; public IntPtr customData, hook; public string template; public IntPtr reserved; public int reservedValue, extraFlags; // Preserve the system ABI on both 32-bit and 64-bit Windows.
        } // Finish the native record.
        [DllImport("comdlg32.dll", EntryPoint = "GetOpenFileNameW", CharSet = CharSet.Unicode, ExactSpelling = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetOpenFileName(ref OpenFileName data); // Invoke the ordinary Windows Open dialog.
        [DllImport("comdlg32.dll", EntryPoint = "GetSaveFileNameW", CharSet = CharSet.Unicode, ExactSpelling = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetSaveFileName(ref OpenFileName data); // Invoke the ordinary Windows Save dialog.
        [DllImport("comdlg32.dll")] private static extern int CommDlgExtendedError(); // Keep user cancellation separate from dialog errors.
        [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow(); // Parent the dialog to the active simulator window.
#endif // Finish Windows-only signatures.
    } // Finish platform selection.
} // Finish the file dialog adapter.
