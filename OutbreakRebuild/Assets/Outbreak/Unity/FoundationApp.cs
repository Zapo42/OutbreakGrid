using System; // Keep deterministic fixtures and error messages straightforward.
using System.Globalization; // Parse setup numbers consistently across machines.
using System.IO; // Remember the last folder used for native browsing.
using Outbreak.Contracts; // Inspect portable actor values without changing them.
using Outbreak.Core; // Drive the same production world and scheduler used by headless checks.
using Outbreak.Persistence; // Load candidates transactionally and save complete documents.
using UnityEngine; // Limit engine dependencies to this presentation adapter.
namespace Outbreak.UnityAdapter // Keep screen and platform behavior out of authoritative simulation code.
{ // Begin the temporary foundation inspection shell.
    public sealed class FoundationApp : MonoBehaviour // Render a minimal movement test before rebuilding the full product interface.
    { // Begin application presentation state.
        public const string BuildLabel = "Foundation 11 / door damage and breaches"; // Identify the actual source revision in native screenshots and diagnostics.
        private World world; private TickClock clock; private ActorState[] previous; private Texture2D terrain; // Own presentation caches independently from the simulation.
        private MovementPresentation[] motion; // Blend ordinary moves over their full cadence without changing collision cells.
        private string status = "Foundation 1: movement, clock and persistence only.", folder = ""; // Make implemented scope visible in the application itself.
        private float centerX, centerY, span = 48; private int selected = -1; private bool confirmReset, showStart = true; private Species? placing; // Keep camera, start flow and manual placement outside running saves.
        private bool showSetup, setupPaused = true; private ScenarioMap setupMap = ScenarioMap.ShelterArena; private string setupHumans = "24", setupZombies = "1", setupSeed = "214"; private Vector2 menuScroll; // Retain editable setup independently of any active run.
        private bool canResume; private Action resetAction; // Preserve a menu-paused run and confirm replacements before discarding it.
        private int textureX = -1, textureY = -1, textureWidth, textureHeight; // Rebuild terrain pixels only when the viewed cell range changes.
        private Action pendingAction; // Apply interface commands after the current GUI event finishes.
        private long guiPasses, mousePresses, mouseReleases, queuedActions, executedActions, updatePasses; private string actionError = ""; // Keep bounded counters that distinguish input delivery, queued work and a paused editor.
        private static bool HostPaused // Distinguish Unity Editor pause from the simulation's own pause control.
        { // Begin host pause inspection.
            get // Query the actual host instead of assuming Update always runs.
            { // Begin platform-specific pause inspection.
#if UNITY_EDITOR // Editor pause stops Update even though the UI may still receive events.
                return UnityEditor.EditorApplication.isPaused; // Read host pause without changing the Console's error policy.
#else // A standalone player has no Unity Editor pause state.
                return false; // Keep player scheduling independent of editor-only APIs.
#endif // Finish host-specific pause inspection.
            } // Finish the host pause getter.
        } // Finish host pause inspection.
        public string InputDiagnostics // Expose observed native input and dispatch state through the editor menu.
        { // Begin the diagnostic snapshot.
            get { return "Build: " + BuildLabel + "\nUnity: " + Application.unityVersion + "\nInput permits runtime IMGUI: " + RuntimeButtonsAvailable + "\nEditor paused: " + HostPaused + "\nGUI passes: " + guiPasses + "\nMouse down/up events received: " + mousePresses + "/" + mouseReleases + "\nActions queued/executed: " + queuedActions + "/" + executedActions + "\nPending action: " + (pendingAction != null) + "\nUpdate passes: " + updatePasses + "\nWorld bodies/tick: " + (world == null ? "none" : world.Count + "/" + world.Tick) + "\nSimulation paused: " + (clock == null || clock.Paused) + "\nLast action error: " + actionError; } // Report the actual build and observations rather than a speculative diagnosis.
        } // Finish the diagnostic snapshot.
        public static bool RuntimeButtonsAvailable // Expose the current compiled input capability for diagnostics and platform checks.
        { // Begin the compatibility query.
            get // Keep the normal control code compiled even when its input backend is unavailable.
            { // Begin the backend choice.
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER // Unity does not deliver runtime OnGUI input in new-input-only mode.
                return false; // Show recovery instructions instead of inert controls.
#else // The legacy input path is available for the temporary IMGUI shell.
                return true; // Allow the ordinary runtime controls to receive their events.
#endif // Finish the compile-time backend choice.
            } // Finish the compatibility getter.
        } // Finish the compatibility query.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] private static void Boot() // Start the shell from the supplied empty scene.
        { // Begin runtime setup.
            if (FindFirstObjectByType<FoundationApp>() == null) new GameObject("Outbreak Foundation").AddComponent<FoundationApp>(); // Avoid duplicate application owners after a scene reload.
        } // Finish runtime setup.
        private void Awake() { Application.targetFrameRate = 60; if (Camera.main == null) { Camera camera = new GameObject("Foundation camera").AddComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.07f, 0.08f, 0.1f); } folder = Application.persistentDataPath; ResetTest(); showStart = true; } // Prepare the test but show explicit start choices before displaying a paused map.
        private static WorldMap TestMap() // Build one small, explicitly defined acceptance arena.
        { // Begin the authored fixture.
            WorldMap map = new WorldMap(48, 32, "Foundation movement arena", "foundation-arena"); // Keep a small reviewable map distinct from a randomly generated town.
            for (int y = 0; y < 32; y++) for (int x = 0; x < 48; x++) if (x == 0 || y == 0 || x == 47 || y == 31) map.Set(x, y, Terrain.Wall); // Surround the arena with visible collision boundaries.
            for (int y = 9; y <= 22; y++) for (int x = 18; x <= 30; x++) map.Set(x, y, x == 18 || x == 30 || y == 9 || y == 22 ? Terrain.Wall : Terrain.Floor); // Provide a room with explicit walls for corner and doorway observation.
            map.Set(24, 9, Terrain.DoorOpen); map.Set(18, 15, Terrain.Window); // Include one passable opening and one solid window.
            for (int x = 1; x < 47; x++) { map.Set(x, 6, Terrain.Sidewalk); map.Set(x, 7, Terrain.Road); } // Make movement easy to inspect against two terrain bands.
            return map; // Return authoring data before constructing the isolated running world.
        } // Finish the authored fixture.
        private void ResetTest() // Replace the current test only after the screen's explicit reset confirmation.
        { // Begin fixture reset.
            World next = new World(TestMap(), 214, 1000); int id; // Use a fixed seed so repeated tests can be compared.
            for (int i = 0; i < 24; i++) next.TrySpawn(Species.Human, 3 + i % 12, 12 + i / 12 * 3, out id); next.TrySpawn(Species.Zombie, 38, 17, out id); // Place twenty-four baseline humans and the agreed single zombie fixture.
            UseWorld(next); status = "Paused movement test. Zombie pursuit, combat and infection are not implemented in this foundation."; // Avoid presenting placeholder movement as finished outbreak AI.
        } // Finish fixture reset.
        private void ResetEncounter() // Create a compact observable scenario without modifying legacy fixtures.
        { // Begin encounter scenario setup.
            UseWorld(ScenarioBuilder.Create(ScenarioMap.ShelterArena)); status = "Mixed-trait encounter: stress, support, fight/freeze/flight, pursuit, infection and shelter recovery. Use Set Up Encounter in the menu to choose starting counts and map."; // Keep the quick start identical to the default configurable scenario.
        } // Finish encounter scenario setup.
        private void UseWorld(World next) // Swap in a fully validated world and reset only presentation state.
        { // Begin one transactional screen swap.
            canResume = false; showSetup = false; menuScroll = new Vector2(); world = next; clock = new TickClock(); previous = new ActorState[world.Capacity]; motion = new MovementPresentation[world.Capacity]; centerX = world.Width / 2f; centerY = world.Height / 2f; span = Math.Min(128, Math.Max(16, world.Width)); selected = -1; textureX = -1; placing = null; showStart = false; // Begin loaded simulations paused, clear stale actions and retain their authoritative tick.
            for (int i = 0; i < world.Count; i++) { previous[i] = world.ActorAt(i); motion[i].Snap(previous[i].x, previous[i].y, world.Tick); } // Prevent a visual jump from stale interpolation history.
        } // Finish one transactional screen swap.
        private void StepWorld() // Advance authoritative ticks and capture safe presentation targets.
        { // Begin one complete simulation step.
            long revision = world.TerrainRevision, condition = world.DoorConditionRevision; for (int i = 0; i < world.Count; i++) previous[i] = world.ActorAt(i); world.Step(); if (world.TerrainRevision != revision || world.DoorConditionRevision != condition) textureX = -1; // Keep the previous canonical cells for corner validation.
            for (int i = 0; i < world.Count; i++) { ActorState actor = world.ActorAt(i), before = previous[i]; bool diagonal = actor.x != before.x && actor.y != before.y; bool safe = !diagonal || (TerrainRules.Walkable(world.TerrainAt(actor.x, before.y)) && TerrainRules.Walkable(world.TerrainAt(before.x, actor.y))); if (clock.Paused || world.TerrainRevision != revision || (world.InfectedEscape && before.turnAtTick == 0 && actor.turnAtTick > 0)) motion[i].Snap(actor.x, actor.y, world.Tick); else motion[i].Observe(actor.x, actor.y, world.Tick, safe); } // Do not smooth a compound move across a solid corner.
        } // Finish canonical and presentation updates.
        private void SetPaused(bool value) // Keep visual history coherent across an explicit pause boundary.
        { // Begin pause presentation synchronization.
#if UNITY_EDITOR // A deliberate Run action must also release a host pause that would prevent all future ticks.
            if (!value && UnityEditor.EditorApplication.isPaused) UnityEditor.EditorApplication.isPaused = false; // Resume the host only when the person explicitly starts playback.
#endif // Finish the editor-only playback action.
            clock.SetPaused(value); for (int i = 0; i < world.Count; i++) { previous[i] = world.ActorAt(i); motion[i].Snap(previous[i].x, previous[i].y, world.Tick); } // Prevent stale interpolation from jumping backward when playback resumes.
        } // Finish pause presentation synchronization.
        private void Queue(Action action) // Keep GUI mutations outside OnGUI while allowing controls to work during host pause.
        { // Begin one deferred interface command.
            if (pendingAction != null) return; pendingAction = action; queuedActions++; // Admit one command without replacing an already accepted click.
#if UNITY_EDITOR // Editor callbacks continue while the simulation Update loop is paused.
            UnityEditor.EditorApplication.update -= ProcessPendingAction; UnityEditor.EditorApplication.update += ProcessPendingAction; // Schedule one post-GUI callback without depending on a simulation frame.
#endif // Finish the editor-specific command wake-up.
        } // Finish one deferred interface command.
        private void ProcessPendingAction() // Use the same exactly-once command pump in editor callbacks and player updates.
        { // Begin command execution.
#if UNITY_EDITOR // Remove the scheduled copy if Update reached this command first.
            UnityEditor.EditorApplication.update -= ProcessPendingAction; // Avoid duplicate callbacks and callbacks retained after component destruction.
#endif // Finish deferred callback cleanup.
            Action action = pendingAction; pendingAction = null; if (action == null) return; // Consume the accepted action exactly once.
            try { action(); executedActions++; actionError = ""; } catch (Exception error) { actionError = error.ToString(); status = "Control failed: " + error.Message; Debug.LogException(error); } // Keep failures visible both on screen and in the native diagnostic report.
        } // Finish command execution.
        private void Update() { updatePasses++; ProcessPendingAction(); if (world != null) clock.Advance(Time.unscaledDeltaTime, StepWorld); } // Schedule simulation ticks after processing any pending player command.
        private void Browse(bool save, bool running) // Keep native file selection separate from validation and world mutation.
        { // Begin one requested file operation.
            SetPaused(true); // Pause every simulation system and synchronize its visual history before opening a blocking dialog.
            try // Convert platform or file errors into an inspectable status message.
            { // Begin file selection and candidate handling.
                string path = FileDialogs.Choose(save, running, folder); if (string.IsNullOrEmpty(path)) { status = "File selection canceled."; return; } // Preserve the current world after cancellation.
                if (save) { Documents.Write(path, running ? Documents.Encode(world.Capture()) : Documents.Encode(world.Capture().map)); status = "Saved " + Path.GetFileName(path); } // Serialize the concrete running or map contract rather than an untyped object.
                else { string json = Documents.Read(path); World candidate = running ? Documents.ParseSave(json) : new World(Documents.ParseMap(json), encounter: true); UseWorld(candidate); status = running ? "Run loaded paused. Click Run to continue." : "Terrain loaded: 0 bodies. Use Place human / Place zombie, click the map, then Run."; } // Commit a valid candidate and explain exactly how to start activity.
                folder = Path.GetDirectoryName(path); // Reuse the selected folder for the next ordinary browse operation.
            } // Finish file selection and candidate handling.
            catch (Exception error) { status = "File operation failed: " + error.Message; } // Retain the previous world after parsing, validation, or native failures.
        } // Finish one requested file operation.
        private void StartScenario(Action action) // Keep returning to the menu from silently replacing a live run.
        { // Begin explicit replacement handling.
            if (canResume) { SetPaused(true); resetAction = action; confirmReset = true; showSetup = false; showStart = false; } else action(); // Draw confirmation over the retained world before starting another scenario.
        } // Finish scenario selection.
        private void DrawSetup() // Make population, map and repeatability choices available without source edits.
        { // Begin a separate scrollable setup card.
            GUI.color = new Color(0.07f, 0.08f, 0.1f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; float width = Math.Min(560, Screen.width - 24), height = Math.Min(620, Screen.height - 24); GUILayout.BeginArea(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), GUI.skin.box); menuScroll = GUILayout.BeginScrollView(menuScroll); GUILayout.Label("SET UP ENCOUNTER"); GUILayout.Label(BuildLabel); // Keep the form reachable in smaller Game views.
            GUILayout.Label("Selected map: " + ScenarioBuilder.MapName(setupMap)); GUILayout.BeginHorizontal(); if (GUILayout.Button("Shelter arena (20 x 16)")) Queue(() => { setupMap = ScenarioMap.ShelterArena; }); if (GUILayout.Button("Shelter district (100 x 100)")) Queue(() => { setupMap = ScenarioMap.ShelterDistrict; }); GUILayout.EndHorizontal(); // Keep map changes out of the active GUI tree traversal.
            GUILayout.Label("Humans at start"); setupHumans = GUILayout.TextField(setupHumans, 32); GUILayout.Label("Zombies at start"); setupZombies = GUILayout.TextField(setupZombies, 32); GUILayout.Label("Repeatable seed (1 to 4294967295)"); setupSeed = GUILayout.TextField(setupSeed, 32); // Leave invalid input visible so the player can correct it.
            if (GUILayout.Button("Start paused: " + (setupPaused ? "Yes" : "No"))) Queue(() => { setupPaused = !setupPaused; }); GUILayout.Label("Maximum 1000 bodies total. This map's spawn limits: " + ScenarioBuilder.HumanLimit(setupMap) + " humans / " + ScenarioBuilder.ZombieLimit(setupMap) + " zombies."); GUILayout.Label("Species start in separate outdoor areas. Humans use the current mixed brave, average and fearful test traits."); // Explain the actual prototype placement policy.
            int humans = 0, zombies = 0; uint seed = 0; bool parsed = int.TryParse(setupHumans, NumberStyles.None, CultureInfo.InvariantCulture, out humans) && int.TryParse(setupZombies, NumberStyles.None, CultureInfo.InvariantCulture, out zombies) && uint.TryParse(setupSeed, NumberStyles.None, CultureInfo.InvariantCulture, out seed); string error = parsed ? ScenarioBuilder.ValidationError(setupMap, humans, zombies, seed) : "Enter whole numbers for both counts and a seed from 1 to 4294967295."; GUILayout.Label(error ?? ("Ready: " + humans + " humans + " + zombies + " zombies. " + (setupPaused ? "Use Run or tick buttons after starting." : "Simulation starts running."))); // Validate before exposing a start action.
            GUI.enabled = error == null; if (GUILayout.Button("Start configured encounter")) { ScenarioMap choice = setupMap; bool paused = setupPaused; Queue(() => StartScenario(() => { World candidate = ScenarioBuilder.Create(choice, humans, zombies, seed); UseWorld(candidate); SetPaused(paused); status = ScenarioBuilder.MapName(choice) + ": " + humans + " humans, " + zombies + " zombies; seed " + seed + ". " + (paused ? "Paused: Run or step to begin." : "Running."); })); } GUI.enabled = true; // Construct the complete candidate before replacing the retained run.
            if (GUILayout.Button("Back to Main Menu")) Queue(() => { showSetup = false; menuScroll = new Vector2(); }); GUILayout.Label(status); GUILayout.EndScrollView(); GUILayout.EndArea(); // Back preserves both settings and the menu-paused world.
        } // Finish encounter setup.
        private void DrawStart() // Explain the available experiment before presenting an inert-looking map.
        { // Begin the start screen.
            if (showSetup) { DrawSetup(); return; } // Keep setup separate from the diagnostic scenario list.
            GUI.color = new Color(0.07f, 0.08f, 0.1f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; // Keep the start screen legible against a stable background.
            float width = Math.Min(560, Screen.width - 24), height = Math.Min(690, Screen.height - 24); GUILayout.BeginArea(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), GUI.skin.box); // Keep the choices centered in an ordinary Game view.
            GUILayout.Label("OUTBREAK SIMULATOR"); GUILayout.Label(BuildLabel); menuScroll = GUILayout.BeginScrollView(menuScroll); GUILayout.Space(12); // Identify the actual source revision without implying finished outbreak behavior.
            GUILayout.Label("Set up your own encounter, or use the quick start with 24 humans and one zombie. Scroll for the smaller behavior tests."); GUILayout.Label("Choose Encounter for stress, bravery, group support, fight/freeze/flight, pursuit and infection. Movement Test preserves the original diagnostic."); GUILayout.Space(12); // State what the user will see before running it.
            if (GUILayout.Button("Set Up Encounter")) Queue(() => { showSetup = true; menuScroll = new Vector2(); }); // Open custom counts without discarding the active run.
            if (canResume && GUILayout.Button("Resume current run")) Queue(() => { canResume = false; showStart = false; SetPaused(false); }); // Resume the exact retained world rather than resetting it.
            GUILayout.BeginHorizontal(); // Keep the three controlled reaction choices together.
            foreach (HumanReaction response in new[] { HumanReaction.Fight, HumanReaction.Freeze, HumanReaction.Flight }) if (GUILayout.Button("Inspect " + response + " Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HumanResponse(response)); status = "Select human #1, then step ticks to inspect " + response + ". Prototype defensive fighting uses shoves."; })); // Use the same authored fixtures as the acceptance suite.
            GUILayout.EndHorizontal(); // Finish controlled reaction choices.
            if (GUILayout.Button("Inspect Door Breach Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.DoorBreach()); status = "Select zombie #2. Step 10 ticks to see damage; by tick 32 the door breaks. Later steps let the zombie enter and reach the trapped human."; })); // Observe a complete breach without depending on a random encounter.
            if (GUILayout.Button("Inspect Zombie Wandering Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ZombieWandering()); status = "Zombie #2 remembers the doorway for three simulation seconds, then occasionally wanders. Step 50 or 100 ticks to see pauses and short moves."; })); // Demonstrate forgetting followed by movement without hidden-target knowledge.
            if (GUILayout.Button("Inspect Shelter Entry Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShelterEntry()); status = "Shelter entry: select human #1, then step. It chooses the enclosed room, opens its door, enters and closes behind itself."; })); // Show the actual room and door lifecycle in a reproducible scenario.
            if (GUILayout.Button("Inspect Shelter Recovery Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShelterRecovery()); status = "Recovery: stress falls to zero, then the human waits calmly, checks the exit and returns outside. Use Tick 50, then Tick 1 or Tick 10."; })); // Show a safe departure with exact stepping.
            if (GUILayout.Button("Inspect Guarded Exit Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShelterRecovery(true)); status = "Guarded exit: a zombie waits behind the closed door. The recovering human opens to inspect, sees danger and retreats instead of leaving."; })); // Demonstrate an observation-driven cancellation.
            if (GUILayout.Button("Inspect Unsafe Shelter Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.UnsafeShelter()); status = "Unsafe shelter: a visible zombie is already inside the selected room. Step to observe rejection and reassessment."; })); // Demonstrate reassessment rather than declaring every room safe.
            if (GUILayout.Button("Inspect Bitten Escape Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.BittenEscape()); status = "Bitten escape: select human #1, then step. Infection overrides fighting and freezing; the human seeks a route behind the nearby wall."; })); // Demonstrate the reported infected behavior in a controlled cover fixture.
            if (GUILayout.Button("Inspect Group Support Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HumanResponse(HumanReaction.Flight, true)); status = "Select human #1: brave nearby allies support an otherwise fearful person. Support is observed every two simulation seconds."; })); // Demonstrate local support independently of individual bravery.
            if (GUILayout.Button("Inspect Corner Pursuit Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.CornerSearch()); status = "Corner pursuit: select zombie #1 and Step 10 ticks. It searches the last observed cell, then reacquires only after sight returns."; })); // Open a controlled pause-first obstacle scenario.
            if (GUILayout.Button("Start Encounter")) Queue(() => StartScenario(() => { ResetEncounter(); SetPaused(false); })); // Start the new prototype through the existing pause-safe command pump.
            if (GUILayout.Button(new GUIContent("Start Movement Test", "Open the fixed arena and start the 24-human, one-zombie movement fixture."), GUILayout.Height(42))) Queue(() => StartScenario(() => { ResetTest(); SetPaused(false); })); // Start visible movement after the GUI event finishes.
            if (GUILayout.Button("Inspect Movement Test Paused")) Queue(() => StartScenario(ResetTest)); // Keep deliberate paused inspection available without replacing the active GUI tree mid-event.
            if (GUILayout.Button(new GUIContent("Load Map", "Load terrain, then place human or zombie test bodies before clicking Run."))) Queue(() => Browse(false, false)); // Open the blocking native dialog outside the active GUI layout pass.
            if (GUILayout.Button(new GUIContent("Load Run", "Load terrain and saved bodies, then click Run to continue."))) Queue(() => Browse(false, true)); // Distinguish a populated save while preserving GUI event lifetime.
            GUILayout.Space(8); GUILayout.Label("During a test: Run / Pause, step 1/10/50/100 ticks, Main Menu, speed controls, save/load, middle-drag to pan and wheel to zoom."); GUILayout.Label(status); GUILayout.EndScrollView(); GUILayout.EndArea(); // Keep cancellation and file errors visible on the start screen too.
        } // Finish the start screen.
        private void OnGUI() // Keep the first shell deliberately small and focused on inspecting the core.
        { // Begin the screen pass.
            guiPasses++; GUI.enabled = true; if (Event.current.type == EventType.MouseDown) mousePresses++; if (Event.current.type == EventType.MouseUp) mouseReleases++; // Record whether the actual native event stream reaches this application.
            if (!RuntimeButtonsAvailable) { GUI.Label(new Rect(20, 20, Screen.width - 40, Screen.height - 40), "SIMULATOR INPUT IS DISABLED\nStop Play mode. Set Edit > Project Settings > Player > Other Settings > Active Input Handling to Both. Restart Unity, then run the simulator again."); return; } // Detect unsupported input mode and replace inert controls with recovery instructions.
            if (showStart) { DrawStart(); return; } // Do not hide the start choices behind a paused terrain display.
            if (world == null) return; GUI.enabled = !confirmReset; GUI.color = new Color(0.07f, 0.08f, 0.1f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; // Draw a stable dark background and disable underlying controls during confirmation.
            GUILayout.BeginArea(new Rect(12, 10, Screen.width - 24, 118), GUI.skin.box); // Reserve a predictable inspection bar above the map.
            GUILayout.Label("OUTBREAK SIMULATOR / " + BuildLabel + (HostPaused ? " / UNITY EDITOR PAUSED" : "")); // Distinguish the current source build and an editor error pause from simulation pause.
            GUILayout.BeginHorizontal(); // Group the genuinely parallel clock controls.
            bool wantsRun = clock.Paused || HostPaused; GUI.enabled = !confirmReset && world.Count > 0; if (GUILayout.Button(new GUIContent(wantsRun ? "Run" : "Pause", world.Count == 0 ? "Place a human or zombie before running the movement test." : "Run resumes an editor pause too. Pause stops the simulation and its interpolation."))) Queue(() => SetPaused(!wantsRun)); // Offer a genuine resume action when an editor error pause has stopped Update.
            GUI.enabled = clock.Paused && !confirmReset && world.Count > 0; foreach (int ticks in new[] { 1, 10, 50, 100 }) if (GUILayout.Button(new GUIContent("Step " + ticks + (ticks == 1 ? " tick" : " ticks"), "Advance exactly " + ticks + " ticks and remain paused."))) Queue(() => clock.StepTicks(ticks, StepWorld)); GUI.enabled = !confirmReset; // Advance all systems through the ordinary tick entry point after dispatch.
            foreach (double speed in new[] { 0.5, 1.0, 2.0, 3.0 }) if (GUILayout.Button(new GUIContent(speed + "x" + (clock.Speed == speed ? " *" : ""), "Change real-time playback speed; simulation ticks stay at 0.20 seconds."))) Queue(() => clock.SetSpeed(speed)); // Expose the agreed speeds without mutating state during GUI traversal.
            GUILayout.Label("Tick " + world.Tick + " | " + TimeSpan.FromSeconds(7 * 3600 + world.GameSeconds).ToString() + " | bodies " + world.Count + " | " + (world.Encounter ? "Encounter" : "Movement test")); // Show the exact tick and a seven-o'clock fixture clock.
            GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); // Separate file controls from clock controls.
            if (GUILayout.Button(new GUIContent("Load Map", "Open the system file browser for a foundation .obmap2 map."))) Queue(() => Browse(false, false)); // Keep native map selection outside the GUI event loop.
            if (GUILayout.Button(new GUIContent("Save Map", "Save terrain and preserved authoring data as .obmap2."))) Queue(() => Browse(true, false)); // Keep starting maps separate from running saves.
            if (GUILayout.Button(new GUIContent("Load Run", "Open a saved simulation and restore its exact random state."))) Queue(() => Browse(false, true)); // Restore the candidate outside the active control traversal.
            if (GUILayout.Button(new GUIContent("Save Run", "Save this tick, population, terrain and random stream as .obsave2."))) Queue(() => Browse(true, true)); // Persist the exact first-slice simulation without nesting a native dialog in OnGUI.
            if (world.HumanReactions && !world.DoorBreaking && GUILayout.Button(world.ZombieWandering ? "Enable door breaking" : world.ShelterRecovery ? "Enable zombie wandering" : world.DoorPursuit ? "Enable shelter recovery" : world.SheltersEnabled ? "Update doorway pursuit" : world.InfectedEscape ? "Enable shelter behavior" : world.CautiousMovement ? "Update escape behavior" : "Use cautious behavior")) Queue(() => { SetPaused(true); SaveDocument saved = world.Capture(); saved.rulesVersion = EncounterRules.Version; World candidate = World.Restore(saved); world = candidate; status = "Updated door breaking, zombie wandering and shelter behavior enabled; run remains paused. Original save file is unchanged; save to a new name to preserve it."; }); // Explicitly upgrade an older reaction run without losing actors, clocks or random state.
            if (GUILayout.Button("Main Menu")) Queue(() => { SetPaused(true); canResume = true; showSetup = false; menuScroll = new Vector2(); showStart = true; }); // Pause and retain world, camera, selection and speed while showing the menu.
            if (GUILayout.Button("New movement test")) Queue(() => { SetPaused(true); confirmReset = true; }); // Open the confirmation on the next stable GUI tree.
            if (GUILayout.Button("Zoom +")) Queue(() => { span = Math.Max(16, span / 1.25f); textureX = -1; }); if (GUILayout.Button("Zoom -")) Queue(() => { span = Math.Min(256, span * 1.25f); textureX = -1; }); // Keep rendering changes outside the current event traversal.
            GUILayout.EndHorizontal(); GUILayout.Label(status); GUILayout.EndArea(); // Keep implementation scope and errors readable in the shell.
            DrawWorld(new Rect(12, 138, Math.Max(64, Screen.width - 300), Math.Max(64, Screen.height - 184))); // Cull the map through a bounded viewport.
            GUILayout.BeginArea(new Rect(Screen.width - 276, 138, 264, Screen.height - 184), GUI.skin.box); // Keep selection diagnostics outside the map viewport.
            GUILayout.Label("MOVEMENT INSPECTOR"); GUILayout.Label("Middle-drag: pan\nWheel: zoom\nClick: inspect a body"); // Explain the few controls that exist in this slice.
            if (world.SheltersEnabled) GUILayout.Label("Doors: brown = closed | tan = open\nOrange = damaged | red = broken"); // Make actual door actions visible while the temporary debug tiles remain.
            if (world.Count == 0) GUILayout.Label("TERRAIN ONLY: no bodies loaded. Place test bodies below, then click Run."); // Make the empty-population state prominent beside the map.
            if (GUILayout.Button(new GUIContent("Place human", "Pause and place one baseline human on your next valid map click."))) Queue(() => { SetPaused(true); placing = Species.Human; status = "Click a walkable cell to place one human."; }); // Arm placement after the current control tree finishes.
            if (GUILayout.Button(new GUIContent("Place zombie", "Pause and place one baseline zombie on your next valid map click."))) Queue(() => { SetPaused(true); placing = Species.Zombie; status = "Click a walkable cell to place one zombie. Encounter rules apply only in Encounter runs."; }); // Keep species identity explicit without inserting controls mid-event.
            if (placing.HasValue && GUILayout.Button("Cancel placement")) Queue(() => { placing = null; status = "Placement canceled."; }); // Remove placement controls on the next GUI tree.
            GUILayout.Label("Sparse chunks: " + world.AllocatedChunks + "\nTick debt: " + clock.DebtSeconds.ToString("0.000") + " s\nMap: " + world.Width + " x " + world.Height); // Show storage and scheduling evidence directly.
            if (world.DoorBreaking && selected >= 0 && selected < world.Count) { ActorState actor = world.ActorAt(selected); for (int y = actor.y - 1; y <= actor.y + 1; y++) for (int x = actor.x - 1; x <= actor.x + 1; x++) { int health = world.DoorHealthAt(x, y); if (health >= 0) GUILayout.Label("Nearby door " + x + ", " + y + ": " + (health == 0 ? "BROKEN" : health + "/" + DoorRules.MaxHealth + " HP")); } } // Read actual nearby durability without recomputing or mutating pursuit routes.
            if (selected >= 0 && selected < world.Count) { ActorState actor = world.ActorAt(selected); GUILayout.Label("#" + actor.id + " " + actor.species + "\nCell " + actor.x + ", " + actor.y + "\nHealth " + (actor.health / 10f).ToString("0.0") + "\nS/P/E/C/I/A/L: " + actor.strength + "/" + actor.perception + "/" + actor.endurance + "/" + actor.charisma + "/" + actor.intelligence + "/" + actor.agility + "/" + actor.luck + "\nBravery " + actor.bravery + (actor.shelterId != 0 ? "\nShelter " + actor.shelterId + ": " + actor.shelterState + "\nDestination " + actor.shelterX + ", " + actor.shelterY : "") + (world.ShelterRecovery && actor.shelterId != 0 ? "\nExit: " + actor.shelterExitPhase + (actor.shelterCalmSinceTick > 0 && actor.shelterExitPhase == ShelterExitPhase.None ? " | calm wait " + (Math.Max(0, ShelterRecoveryRules.CalmTicks - (world.Tick - actor.shelterCalmSinceTick)) / 5.0).ToString("0.0") + " seconds" : actor.stress > 0 ? " | recovering" : "") : "") + (actor.rejectedShelterId != 0 ? "\nRejected room " + actor.rejectedShelterId : "") + (world.HumanReactions && actor.species == Species.Human ? "\nStress " + HumanRules.Level(actor.stress) + " (" + actor.stress + ")\nResponse " + actor.reaction + "\nSupport " + actor.groupSupport + " | Pressure " + actor.threatPressure + "\nCourage margin " + HumanRules.Margin(actor) + (actor.freezeUntilTick > 0 ? "\nFreeze: " + ((actor.freezeUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds" : "") : "") + (actor.staggerUntilTick > 0 ? "\nStaggered" : "") + (actor.species == Species.Zombie && actor.targetId == 0 ? "\nIdle: " + (world.ZombieWandering ? "occasional wandering" : "stationary (older rules)") : "") + (actor.targetId != 0 ? "\nObserved human #" + actor.targetId + " at " + actor.lastSeenX + ", " + actor.lastSeenY + "\nMemory: " + ((actor.searchUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds" : "") + (actor.turnAtTick > 0 ? "\nInfected: turns in " + ((actor.turnAtTick - world.Tick) / 5.0).ToString("0.0") + " simulation seconds" : "")); } // Inspect actual saved statistics using a detached value record.
            GUILayout.EndArea(); GUI.Label(new Rect(12, Screen.height - 36, Screen.width - 24, 30), GUI.tooltip); // Display a plain-language explanation for hovered controls.
            GUI.enabled = true; if (confirmReset) // Re-enable only the modal controls before offering a concrete reset confirmation.
            { // Begin the reset confirmation.
                GUILayout.BeginArea(new Rect(Screen.width / 2f - 190, Screen.height / 2f - 65, 380, 130), GUI.skin.box); GUILayout.Label("Replace the current run with the fixed movement test?"); // Identify exactly what the reset changes.
                if (GUILayout.Button("Replace run")) Queue(() => { confirmReset = false; Action next = resetAction ?? ResetTest; resetAction = null; canResume = false; next(); }); if (GUILayout.Button("Keep current run")) Queue(() => { confirmReset = false; resetAction = null; canResume = false; }); GUILayout.EndArea(); // Apply reset only after the deliberate second action and outside control traversal.
            } // Finish the reset confirmation.
        } // Finish the screen pass.
        private void DrawWorld(Rect bounds) // Draw a bounded terrain texture and only bodies inside its camera range.
        { // Begin viewport rendering.
            float cell = bounds.width / span; int width = Math.Min(world.Width, Math.Max(1, (int)Math.Ceiling(span))), height = Math.Min(world.Height, Math.Max(1, Math.Min(256, (int)Math.Ceiling(bounds.height / cell)))); // Bound texture memory independently from world dimensions.
            int left = Math.Max(0, Math.Min(world.Width - width, (int)(centerX - width / 2f))), top = Math.Max(0, Math.Min(world.Height - height, (int)(centerY - height / 2f))); // Clamp the camera to real logical cells.
            if (terrain == null || textureWidth != width || textureHeight != height) { if (terrain != null) Destroy(terrain); terrain = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point }; textureWidth = width; textureHeight = height; textureX = -1; } // Reuse one small GPU texture until its dimensions change.
            if (textureX != left || textureY != top) // Refresh terrain only after a changed viewed range or loaded world.
            { // Begin a bounded pixel refresh.
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) terrain.SetPixel(x, height - 1 - y, ColorForCell(left + x, top + y)); terrain.Apply(false); textureX = left; textureY = top; // Upload only visible terrain cells, with logical north at the top.
            } // Finish a bounded pixel refresh.
            Rect visible = new Rect(bounds.x, bounds.y, width * cell, height * cell); GUI.DrawTexture(visible, terrain, ScaleMode.StretchToFill); // Render terrain in one texture operation.
            Event input = Event.current; // Use UI events so the shell does not depend on an extra input package.
            for (int i = 0; i < world.Count; i++) // Inspect the bounded active population instead of instantiating scene objects.
            { // Begin one visible body.
                ActorState actor = world.ActorAt(i); double time = world.Tick + clock.Alpha; float x = (float)((clock.Paused ? actor.x : motion[i].X(time)) + MovementPresentation.OffsetX(actor.id)), y = (float)((clock.Paused ? actor.y : motion[i].Y(time)) + MovementPresentation.OffsetY(actor.id)); // Interpolate only between committed simulation positions.
                if (x < left - 0.18f || y < top - 0.18f || x >= left + width - 0.18f || y >= top + height - 0.18f) continue; // Cull bodies outside the camera.
                Rect body = new Rect(bounds.x + (x - left + 0.18f) * cell, bounds.y + (y - top + 0.18f) * cell, cell * 0.64f, cell * 0.64f); GUI.color = selected == i ? Color.yellow : actor.species == Species.Zombie ? new Color(0.23f, 0.29f, 0.14f) : new Color(0.75f, 0.8f, 0.92f); GUI.DrawTexture(body, Texture2D.whiteTexture); // Use visibly distinct temporary debug bodies until artwork bindings return.
                if (!confirmReset && !placing.HasValue && input.type == EventType.MouseDown && input.button == 0 && body.Contains(input.mousePosition)) { int chosen = i; Queue(() => selected = chosen); input.Use(); } // Defer selection so the inspector cannot add layout entries midway through a mouse event.
            } // Finish one visible body.
            GUI.color = Color.white; if (confirmReset || !visible.Contains(input.mousePosition)) return; // Keep modal confirmation and toolbar events out of map input.
            if (placing.HasValue && input.type == EventType.MouseDown && input.button == 0) // Handle manual placement only after an explicit tool choice and a map click.
            { // Begin one placement command.
                int x = left + (int)((input.mousePosition.x - bounds.x) / cell), y = top + (int)((input.mousePosition.y - bounds.y) / cell); Species species = placing.Value; // Capture the chosen cell and species before the GUI event ends.
                Queue(() => { int id; if (world.TrySpawn(species, x, y, out id)) { selected = world.Count - 1; previous[selected] = world.ActorAt(selected); motion[selected].Snap(x, y, world.Tick); status = "Placed " + species + " #" + id + ". Click Run to start movement."; placing = null; } else status = "Cannot place here: choose a walkable cell with space, within the population cap."; }); input.Use(); // Synchronize new body history outside OnGUI and retain invalid placement for another attempt.
            } // Finish one placement command.
            if (input.type == EventType.MouseDrag && input.button == 2) { centerX -= input.delta.x / cell; centerY -= input.delta.y / cell; input.Use(); } // Pan the camera without moving logical bodies.
            if (input.type == EventType.ScrollWheel) { span = Mathf.Clamp(span * (input.delta.y > 0 ? 1.1f : 0.9f), 16, 256); input.Use(); } // Zoom the bounded viewport around its current camera center.
        } // Finish viewport rendering.
        private Color ColorForCell(int x, int y) // Distinguish real door damage from ordinary open and closed terrain.
        { // Begin condition-aware debug coloring.
            int health = world.DoorHealthAt(x, y); if (health == 0) return new Color(0.72f, 0.22f, 0.16f); if (health > 0 && health < DoorRules.MaxHealth && world.TerrainAt(x, y) == Terrain.DoorClosed) return new Color(0.65f + 0.2f * (1 - health / 100f), 0.3f, 0.08f); return ColorFor(world.TerrainAt(x, y)); // Broken doors are passable red cells; damaged closed doors turn orange.
        } // Finish terrain feedback.
        private static Color ColorFor(Terrain value) // Keep debug colors in presentation rather than in movement rules.
        { // Begin terrain presentation.
            if (value == Terrain.Wall || value == Terrain.Fence || value == Terrain.GateClosed) return new Color(0.18f, 0.18f, 0.2f); // Show solid structural cells clearly.
            if (value == Terrain.Road) return new Color(0.27f, 0.28f, 0.31f); if (value == Terrain.Sidewalk || value == Terrain.Crosswalk) return new Color(0.6f, 0.6f, 0.56f); // Distinguish travel surfaces in the fixture.
            if (value == Terrain.Floor) return new Color(0.55f, 0.45f, 0.32f); if (value == Terrain.Window) return new Color(0.3f, 0.65f, 0.72f); if (value == Terrain.Water) return new Color(0.15f, 0.36f, 0.6f); // Keep room floors, glass, and water visually separate.
            if (value == Terrain.DoorClosed) return new Color(0.42f, 0.23f, 0.09f); if (value == Terrain.DoorOpen) return new Color(0.82f, 0.70f, 0.48f); if (value == Terrain.GateOpen) return new Color(0.65f, 0.45f, 0.2f); return new Color(0.23f, 0.36f - (int)value * 0.005f, 0.22f); // Supply a stable grass fallback for remaining known terrain.
        } // Finish terrain presentation.
        private void OnDestroy() // Release presentation resources and any scheduled callback owned by this component.
        { // Begin component cleanup.
#if UNITY_EDITOR // A scheduled editor delegate can otherwise outlive its destroyed component.
            UnityEditor.EditorApplication.update -= ProcessPendingAction; // Release the editor's reference to this component.
#endif // Finish editor callback cleanup.
            pendingAction = null; resetAction = null; if (terrain != null) Destroy(terrain); // Drop pending work and release the owned texture.
        } // Finish component cleanup.
    } // Finish application presentation state.
} // Finish the temporary foundation inspection shell.
