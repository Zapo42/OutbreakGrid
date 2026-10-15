using System.IO; // Check for the foundation scene before creating it.
using UnityEditor; // Configure the project and expose a Windows build action.
using UnityEditor.SceneManagement; // Create a clean scene without carrying over old runtime objects.
using UnityEngine; // Name build destinations and display build errors.
using UnityEngine.SceneManagement; // Close only the temporary setup scene.
namespace Outbreak.EditorAdapter // Keep all editor dependencies in their own editor-only assembly.
{ // Begin project setup.
    public static class ProjectSetup // Create scenes only after an explicit menu action, outside editor startup indexing.
    { // Begin editor integration.
        private const string ScenePath = "Assets/Scenes/Foundation.unity"; // Use one explicit entry scene for Windows builds.
        [MenuItem("Outbreak/Rebuild/Create foundation scene")] public static void EnsureScene() // Make setup repeatable without overwriting a saved scene.
        { // Begin safe scene setup.
            if (File.Exists(ScenePath) || EditorApplication.isPlayingOrWillChangePlaymode) return; Directory.CreateDirectory("Assets/Scenes"); // Preserve existing scenes and avoid editing during play mode.
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive); EditorSceneManager.SaveScene(scene, ScenePath); EditorSceneManager.CloseScene(scene, true); // Generate the entry scene without replacing the user's open scene.
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }; // Include the actual entry scene in subsequent player builds.
        } // Finish safe scene setup.
        [MenuItem("Outbreak/Rebuild/Open and Run Foundation")] public static void OpenAndRun() // Give the source project one obvious action that enters the running application.
        { // Begin the explicit editor launch.
            if (EditorApplication.isPlayingOrWillChangePlaymode || !InputSetup.EnsureReady() || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return; EnsureScene(); // Require functional runtime input and protect unsaved scene edits before launch.
            EditorSceneManager.OpenScene(ScenePath); EditorApplication.isPlaying = true; // Open the actual entry scene and show its start screen in Play mode.
        } // Finish the explicit editor launch.
        [MenuItem("Outbreak/Rebuild/Build Windows player")] public static void BuildWindows() // Provide a concrete reproducible Windows build action.
        { // Begin Windows build setup.
            if (!InputSetup.EnsureReady()) return; EnsureScene(); string folder = EditorUtility.OpenFolderPanel("Choose Windows build folder", "", ""); if (string.IsNullOrEmpty(folder)) return; // Reject a known noninteractive build configuration before creating player files.
            BuildPlayerOptions options = new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = Path.Combine(folder, "OutbreakSimulator.exe"), target = BuildTarget.StandaloneWindows64, options = BuildOptions.None }; // Build the foundation scene for the established target.
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options); Debug.Log("Outbreak foundation build: " + report.summary.result); // Report the real build result rather than assuming success.
        } // Finish Windows build setup.
    } // Finish editor integration.
} // Finish project setup.
