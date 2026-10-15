using Outbreak.UnityAdapter; // Inspect the actual running application through an editor menu that does not depend on its buttons.
using UnityEditor; // Display and copy diagnostic evidence while the simulator is paused or unresponsive.
using UnityEngine; // Read the editor version and currently loaded runtime components.
namespace Outbreak.EditorAdapter // Keep troubleshooting UI separate from simulation behavior.
{ // Begin native input diagnostics.
    public static class InputDiagnostics // Gather evidence before selecting another input repair.
    { // Begin the diagnostic menu.
        [MenuItem("Outbreak/Rebuild/Show Input Diagnostics")] public static void Show() // Work from Unity's menu even if no simulator button receives clicks.
        { // Begin one diagnostic capture.
            FoundationApp[] apps = Object.FindObjectsByType<FoundationApp>(FindObjectsSortMode.None); string report = "Build: " + FoundationApp.BuildLabel + "\nUnity: " + Application.unityVersion + "\nPlaying: " + EditorApplication.isPlaying + "\nEditor paused: " + EditorApplication.isPaused + "\nActive foundation components: " + apps.Length + "\nRuntime IMGUI backend available: " + FoundationApp.RuntimeButtonsAvailable; // Identify the exact build and count live owners without Unity Search.
            for (int i = 0; i < apps.Length; i++) report += "\n\nComponent " + (i + 1) + "\n" + apps[i].InputDiagnostics; // Include actual received-event and executed-command counters for each owner.
            EditorGUIUtility.systemCopyBuffer = report; Debug.Log(report); EditorUtility.DisplayDialog("Outbreak input diagnostics", report + "\n\nThis report has been copied to the clipboard.", "OK"); // Make the exact version and input evidence easy to return without a screenshot.
        } // Finish one diagnostic capture.
    } // Finish the diagnostic menu.
} // Finish native input diagnostics.
