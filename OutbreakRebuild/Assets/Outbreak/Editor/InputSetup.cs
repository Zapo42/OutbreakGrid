using UnityEditor; // Expose an editor-side check for the runtime UI input backend.
using UnityEngine; // Report the actual current-process input capability without querying Unity Search.
namespace Outbreak.EditorAdapter // Keep project configuration outside the running simulation.
{ // Begin the runtime UI input setup.
    public static class InputSetup // Protect IMGUI controls from new-input-only project defaults.
    { // Begin input validation.
        private const string Guidance = "Outbreak's runtime buttons need legacy input. Set Edit > Project Settings > Player > Other Settings > Active Input Handling to Both, then restart Unity."; // Give a concrete recovery path even when controls receive no events.
        [MenuItem("Outbreak/Rebuild/Check UI Input Setup")] public static void Check() { EnsureReady(); } // Offer a repeatable editor action outside the broken runtime controls.
        public static bool EnsureReady() // Validate the currently compiled backend rather than editing an ambiguous Unity 6 build-profile settings object.
        { // Begin the current-process check.
            if (Outbreak.UnityAdapter.FoundationApp.RuntimeButtonsAvailable) return true; // Preserve compatible Legacy or Both configurations without changing project preferences.
            Debug.LogError(Guidance); EditorUtility.DisplayDialog("Outbreak input setup", Guidance, "OK"); return false; // Explain a blocked launch immediately instead of appearing to ignore the selected editor command.
        } // Finish the current-process check.
    } // Finish input validation.
} // Finish the runtime UI input setup.
