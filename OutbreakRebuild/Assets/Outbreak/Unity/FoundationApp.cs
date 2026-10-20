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
        public const string BuildLabel = "Foundation 59 / visible combat health"; // Identify the actual source revision in native screenshots and diagnostics.
        private World world; private TickClock clock; private ActorState[] previous; private Texture2D terrain; // Own presentation caches independently from the simulation.
        private WeaponPresentation[] weaponMotion;
        private double[] hitFlashUntil;
        private BiteAction[] biteFeedback;
        private MovementPresentation[] motion; // Blend ordinary moves over their full cadence without changing collision cells.
        private string status = "Foundation 1: movement, clock and persistence only.", folder = ""; // Make implemented scope visible in the application itself.
        private float centerX, centerY, span = 48; private int selected = -1; private bool confirmReset, showStart = true; private Species? placing; // Keep camera, start flow and manual placement outside running saves.
        private bool showSetup, setupPaused = true; private ScenarioMap setupMap = ScenarioMap.ShelterArena; private string setupHumans = "24", setupZombies = "1", setupSeed = "214", setupBats = ScenarioBuilder.DefaultBats.ToString(CultureInfo.InvariantCulture); private Vector2 menuScroll; // Retain editable setup independently of any active run.
        private bool showTargetMarkers=true;
        private Vector2 inspectorScroll; // Let inventory and detailed diagnostics remain reachable in smaller Game views.
        private int focusedBatCell=-1, selectedBatCell=-1;
        private int shelfBrush = -1, selectedShelf; // Keep furniture authoring and selection out of saved simulation state.
        private SaveDocument scenarioStart; private bool scenarioAdvanced;
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
            UseWorld(ScenarioBuilder.Create(ScenarioMap.ShelterArena)); status = "Mixed-trait encounter with six ground bats. Humans collect them when safe; inspect weapon supplies and learned skill. Use Set Up Encounter in the menu to choose starting counts and map."; // Keep the quick start identical to the default configurable scenario.
        } // Finish encounter scenario setup.
        private void UseWorld(World next) // Swap in a validated world and capture its restart baseline.
        { // Begin one transactional screen swap.
            scenarioStart=next.Capture();scenarioAdvanced=false;canResume = false; showSetup = false; menuScroll = new Vector2(); world = next; clock = new TickClock(); previous = new ActorState[world.Capacity]; motion = new MovementPresentation[world.Capacity]; weaponMotion=new WeaponPresentation[world.Capacity]; hitFlashUntil=new double[world.Capacity]; biteFeedback=new BiteAction[world.Capacity]; centerX = world.Width / 2f; centerY = world.Height / 2f; span = Math.Min(128, Math.Max(16, world.Width)); selected = -1; textureX = -1; placing = null; shelfBrush = -1; selectedShelf = 0; focusedBatCell=-1; selectedBatCell=-1; showStart = false; // Begin loaded simulations paused, clear stale actions and retain their authoritative tick.
            for (int i = 0; i < world.Count; i++) { previous[i] = world.ActorAt(i); motion[i].Snap(previous[i].x, previous[i].y, world.Tick);weaponMotion[i].Reset(); } // Prevent a visual jump from stale interpolation history.
        } // Finish one transactional screen swap.
        private void StepWorld() // Advance authoritative ticks and capture safe presentation targets.
        { // Begin one complete simulation step.
            if(!scenarioAdvanced){scenarioStart=world.Capture();scenarioAdvanced=true;} // Include authored starting bodies and supplies before the first simulation tick.
            long revision = world.TerrainRevision, condition = world.DoorConditionRevision; for (int i = 0; i < world.Count; i++) previous[i] = world.ActorAt(i); world.Step(); if (world.TerrainRevision != revision || world.DoorConditionRevision != condition) textureX = -1; // Keep the previous canonical cells for corner validation.
            int weaponEvent=0;
            for (int i = 0; i < world.Count; i++) { ActorState actor = world.ActorAt(i), before = previous[i];weaponMotion[i].Move(actor.x-before.x,actor.y-before.y);if(weaponEvent<world.WeaponActionCount && world.WeaponActionAt(weaponEvent).actorId==actor.id)weaponMotion[i].Observe(world.WeaponActionAt(weaponEvent++),actor.x,actor.y); bool diagonal = actor.x != before.x && actor.y != before.y; bool safe = !diagonal || (TerrainRules.Walkable(world.TerrainAt(actor.x, before.y)) && TerrainRules.Walkable(world.TerrainAt(before.x, actor.y))); if (clock.Paused || (world.InfectedEscape && before.turnAtTick == 0 && actor.turnAtTick > 0)) motion[i].Snap(actor.x, actor.y, world.Tick); else motion[i].Observe(actor.x, actor.y, world.Tick, safe); } // Do not smooth a compound move across a solid corner.
            for(int e=0;e<world.WeaponActionCount;e++){WeaponAction action=world.WeaponActionAt(e);if(!action.hit || action.kind!=WeaponActionKind.Swing)continue;for(int i=0;i<world.Count;i++)if(world.ActorAt(i).id==action.targetId){hitFlashUntil[i]=action.tick+WeaponPresentation.HitFlashTicks;break;}} // Flash the struck body, including a fatal hit, at its rendered position.
            for(int e=0;e<world.BiteActionCount;e++){BiteAction action=world.BiteActionAt(e);for(int i=0;i<world.Count;i++)if(world.ActorAt(i).id==action.targetId){biteFeedback[i]=action;break;}}
        } // Finish canonical and presentation updates.
        private void RestartScenario() // Restore the starting checkpoint while retaining enabled rules upgrades.
        {
            SaveDocument initial=scenarioAdvanced?World.Restore(scenarioStart).Capture():world.Capture();initial.rulesVersion=world.Capture().rulesVersion;World candidate=World.Restore(initial);UseWorld(candidate);status="Scenario restarted at tick "+world.Tick+", paused. Run or step to begin again.";
        }
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
                else { string json = Documents.Read(path); World candidate = running ? Documents.ParseSave(json) : new World(Documents.ParseMap(json), encounter: true, hunger: true); UseWorld(candidate); status = running ? "Run loaded paused. Click Run to continue." : "Terrain loaded: 0 bodies. Use Place human / Place zombie, click the map, then Run."; } // Commit a valid candidate and explain exactly how to start activity.
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
            GUILayout.Label("Humans at start"); setupHumans = GUILayout.TextField(setupHumans, 32); GUILayout.Label("Zombies at start"); setupZombies = GUILayout.TextField(setupZombies, 32); GUILayout.Label("Repeatable seed (1 to 4294967295)"); setupSeed = GUILayout.TextField(setupSeed, 32); GUILayout.Label("Bats at start (0 to " + ScenarioBuilder.BatLimit(setupMap) + ")"); setupBats=GUILayout.TextField(setupBats,32); // Leave invalid input visible so the player can correct it.
            if (GUILayout.Button("Start paused: " + (setupPaused ? "Yes" : "No"))) Queue(() => { setupPaused = !setupPaused; }); GUILayout.Label("Maximum 1000 bodies total. This map's spawn limits: " + ScenarioBuilder.HumanLimit(setupMap) + " humans / " + ScenarioBuilder.ZombieLimit(setupMap) + " zombies."); GUILayout.Label("Species start in separate outdoor areas. Bats are scattered in the human starting area; collecting one costs an action. Humans use mixed brave, average and fearful test traits."); // Explain the actual prototype placement policy.
            int humans = 0, zombies = 0, bats=0; uint seed = 0; bool parsed = int.TryParse(setupHumans, NumberStyles.None, CultureInfo.InvariantCulture, out humans) && int.TryParse(setupZombies, NumberStyles.None, CultureInfo.InvariantCulture, out zombies) && uint.TryParse(setupSeed, NumberStyles.None, CultureInfo.InvariantCulture, out seed) && int.TryParse(setupBats,NumberStyles.None,CultureInfo.InvariantCulture,out bats); string error = parsed ? ScenarioBuilder.ValidationError(setupMap, humans, zombies, seed,bats) : "Enter whole numbers for humans, zombies and bats, and a seed from 1 to 4294967295."; GUILayout.Label(error ?? ("Ready: " + humans + " humans + " + zombies + " zombies + " + bats + " ground bats. " + (setupPaused ? "Use Run or tick buttons after starting." : "Simulation starts running."))); // Validate before exposing a start action.
            GUI.enabled = error == null; if (GUILayout.Button("Start configured encounter")) { ScenarioMap choice = setupMap; bool paused = setupPaused; Queue(() => StartScenario(() => { World candidate = ScenarioBuilder.Create(choice, humans, zombies, seed,bats); UseWorld(candidate); SetPaused(paused); status = ScenarioBuilder.MapName(choice) + ": " + humans + " humans, " + zombies + " zombies, " + bats + " ground bats; seed " + seed + ". " + (paused ? "Paused: Run or step to begin." : "Running."); })); } GUI.enabled = true; // Construct the complete candidate before replacing the retained run.
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
            if (GUILayout.Button("Inspect Health Bars Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HealthBarsTest());selected=0;status="Injured humans and zombies show a small health bar above their body. Full-health actors and defeated zombies have no bar. Select a body for exact health. Inspect Blunt Combat Paused shows bars changing during real combat."; }));
            if (GUILayout.Button("Inspect Bat Conditions Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.BatConditionsTest());selectedBatCell=4*world.Width+2;status="Fresh bats are brown; worn bats fade toward grey. Click a bat or Next ground bat to inspect every condition in its pile. This fixture has fresh, half-worn, nearly broken and mixed supplies."; }));
            if (GUILayout.Button("Inspect First Weapon Priority Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FirstWeaponPriorityTest());selected=0;status="An armed and an unarmed human are equally close to one bat. Run or Step 50 ticks: the armed human favors the other supply, leaving the first weapon for the unarmed human. Restart scenario repeats the setup."; }));
            if (GUILayout.Button("Inspect First Bat Courtesy Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.UnarmedCourtesyTest());selected=0;status="Two unarmed humans can see two bats. Run or Step 50 ticks: they favor separate supplies and collect one bat each. Restart scenario repeats the setup."; }));
            if (GUILayout.Button("Inspect Supply Courtesy Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.SupplyCourtesyTest());selected=0;status="Two humans need better bats. Run or Step 50 ticks: they favor separate supplies over competing for one bat. Restart scenario repeats the setup."; }));
            if (GUILayout.Button("Inspect Replacement Trip Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ReplacementTripTest());selected=0;status="This human has two hits left on its bat and can see a fresh one down the corridor. Run or Step 50 ticks: it walks to the supply, swaps in a separate action and leaves the used bat behind. Restart scenario repeats the same trip."; }));
            if (GUILayout.Button("Inspect Weapon Replacement Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.WeaponReplacementTest());selected=0;status="This calm human has two hits left on its bat and a fresh bat at its feet. Step 1 tick: it swaps, leaving its used bat on the ground. Restart scenario reruns the same starting setup."; }));
            if (GUILayout.Button("Inspect Combat Footwork Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.CombatFootworkTest());selected=0;status="The zombie has a wall behind it, so the first bat hit cannot push it away. Run or Step 10 ticks: the human steps back while its swing recovers, then attacks normally when ready."; }));
            if (GUILayout.Button("Inspect Bat Durability Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.BatDurabilityTest());selected=0;status="This bat has one hit remaining. Run or Step 1 tick: the full-damage strike breaks it. Inspect condition and swing cooldown; skill is retained, and another bat can be collected when safe."; }));
            if (GUILayout.Button("Inspect Dodged Bite Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.BiteEvasionTest());selected=1;status="An agile human faces a contact attack with a 54% dodge chance. Step 1 tick to see a seeded dodge: blue flash, no damage and no infection."; }));
            if (GUILayout.Button("Inspect Successful Bite Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.BiteEvasionTest(false));selected=1;status="The same agility and stress can still fail a dodge. Step 1 tick: red flash, bite damage and the existing infection countdown."; }));
            if (GUILayout.Button("Inspect Weapon Drop Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.WeaponDropTest());selected=0;status="This infected human carries a bat. Step 1 tick: it turns and leaves the bat on the ground. Survivors can recover it when the area is safe."; }));
            if (GUILayout.Button("Inspect Shelter Spacing Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShelterSpacingTest());status="Four stressed humans share one corner and destination. Run or Step 50 ticks: they choose separate positions inside the room."; }));
            if (GUILayout.Button("Inspect Combat Experience Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.CombatExperienceTest()); selected=0; status="This human has blunt skill 0 and 2/3 practice hits. Step 1 tick to collect the bat, then Run: its next successful hit raises skill to 1. Select the human to inspect saved practice and bat damage."; }));
            if (GUILayout.Button("Inspect Weapon Seeking Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.WeaponSeekingTest()); status="A calm human can see a nearby bat. Run to watch it approach and collect the single bat. Food, shelter and visible danger take priority over seeking equipment."; }));
            if (GUILayout.Button("Inspect Blunt Combat Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.BluntCombatTest()); status = "A brave, skilled human has a bat at its feet. Step 1 tick to pick it up, then Step 50 ticks to watch timed strikes, knockback and a defeated zombie. Select either body to inspect equipment and health."; }));
            if (GUILayout.Button("Inspect Food Scarcity Trial Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FoodScarcityTrial()); status = "Twelve humans, four rooms and 24 meals, with no zombies. Use Step 1 game hour in the food overview. Three hours consumes the starting food; keep stepping to watch the shortage develop."; }));
            if (GUILayout.Button("Inspect Food Exploration Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FoodExplorationTest()); status = "The known outdoor shelf is empty. Step 1 tick, then Next hungry human: purple marks a room investigation. Step 100 ticks, then Step 50 ticks to watch the human check the empty left room and discover food in the right room."; }));
            if (GUILayout.Button("Inspect Food Yield Choices Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FoodYieldChoiceTest()); status = "The closer shelf has one exposed meal; the other has six, one extra walking step away. Step 1 tick, then Next hungry human to see the choice. Step 50 ticks to see pickup and eating."; }));
            if (GUILayout.Button("Inspect Alternate Food Shelter Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.AlternateFoodShelterTest()); status = "The original shelter on the left is full. Step 1 tick, then Next hungry human: the stressed forager heads to the room on the right. Step 50 ticks to see it inside with the door closed and shelf untouched."; }));
            if (GUILayout.Button("Inspect Food Trip Retreat Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FoodTripRetreatTest()); status = "Stress has pushed this forager above its risk tolerance. Step 1 tick, then Next hungry human: it abandons the shelf and heads home. Step 50 ticks to watch it return and close the door."; }));
            if (GUILayout.Button("Inspect Guarded Food Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.SafeFoodTargetTest()); status = "The near shelf is guarded by a visible zombie. Step 1 tick, then Next hungry human: the teal target is the safer shelf at 3,8. Continue stepping to watch survival take priority as the zombie approaches."; }));
            if (GUILayout.Button("Inspect Considerate Pickup Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FairFoodPickupTest()); status = "Three hungry humans, three meals. Step 1 tick: the first takes one and leaves two. Step 10 ticks: all three have collected and eaten a meal."; }));
            if (GUILayout.Button("Inspect Food Route Choices Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FoodRouteChoiceTest()); status = "Human #1 knows both shelves. The nearer one needs a long detour around the wall. Step 1 tick to choose the shorter walking route; step 50 ticks to collect and eat."; }));
            if (GUILayout.Button("Inspect Door Noise Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.DoorMovementNoiseTest()); status = "Step 1 tick: human #1 closes the nearby door. Select zombie #2, then step 10 ticks: it investigates the door without seeing the human or damaging it."; }));
            if (GUILayout.Button("Inspect Exposed Food Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.QuickStockPickupTest()); status = "The shelf is already searched. Step 1 tick: human #1 collects three meals immediately. Compare Inspect Careful Search for an unopened shelf."; }));
            if (GUILayout.Button("Inspect Blocked Food Route Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.BlockedFoodRouteTest()); status = "Human #1 remembers food behind the wall. Step 1 tick to note the blocked route; step 50 ticks to use the reachable shelf instead."; }));
            if (GUILayout.Button("Inspect Coordinated Search Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.SearchCoordinationTest()); status = "Step 10 ticks: human #1 searches the near shelf while human #2 chooses the other. Step 20 more to collect food from both shelves."; }));
            if (GUILayout.Button("Inspect Careful Search Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.CarefulSearchTest()); status = "Select human #1. Step 1 tick: search progress 1/3, no food taken. Step 10 ticks: the third action collects food. Danger above its risk budget cancels progress."; }));
            if (GUILayout.Button("Inspect Search Noise Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.SearchNoiseTest()); status = "Step 1 tick: searching wakes the zombie beyond the closed door. Select zombie #2, then step 10 ticks to watch it investigate without seeing the human or attacking the door."; }));
            if (GUILayout.Button("Inspect Food Delivery Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FoodDeliveryTest()); status = "Human #1 carries three meals. Human #2 is hungry across the room. Step 50 ticks to see delivery, eating and pantry storage."; }));
            if (GUILayout.Button("Inspect Pantry Trip Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.PantryTripTest()); status = "Select human #1, then step 50 ticks. It walks to the empty shelter shelf, deposits two meals and keeps one."; }));
            if (GUILayout.Button("Inspect Pantry Storage Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.PantryStorageTest()); status = "Select human #1: three carried meals. Step 1 tick: two meals go onto the adjacent shelter shelf, with one kept in reserve."; }));
            if (GUILayout.Button("Inspect Shelf Reports Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShelfReportTest()); status = "Human #1 knows food behind the closed partition. Select hungry human #2, then step 1 tick to hear the report; step 50 ticks to follow its supply trip."; })); // Demonstrate information transfer without global stock knowledge.
            if (GUILayout.Button("Inspect Shelf Memory Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShelfMemoryTest()); status = "Select human #1. It remembers an empty shelf at 2,3 and three meals at 7,4 behind the closed door. Step 10 ticks to see it return toward remembered supplies; 40 more to search and eat."; })); // Start after an actual earlier search, with supplies now out of sight.
            if (GUILayout.Button("Inspect Food Sharing Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.FoodSharingTest()); status = "Human #1 has three meals; hungry human #2 has none. Step 1 tick to transfer a meal, then 1 more tick for #2 to eat. Both can stay sheltered."; })); // Inspect transfer and eating as separate actual actions.
            if (GUILayout.Button("Inspect Hungry Forager Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HungerForaging(3)); status = "Select human #1. Step 50 ticks to see it leave, search the outdoor shelf and carry three meals. Step 50 more to see it eat and return home."; })); // Follow a complete supply trip.
            if (GUILayout.Button("Inspect Timid Forager Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HungerForaging(-3)); status = "Same hunger and stress, lower bravery: at tick 50 this human still waits inside. Continue stepping to compare its later supply trip."; })); // Compare risk tolerance without changing need or perceived stress.
            if (GUILayout.Button("Inspect Stocked Shelter Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HungerForaging(3, true)); status = "The room has a searchable shelf. Step 10 ticks: the human takes food into inventory and eats without leaving."; })); // Show why a supplied room is worth staying in.
            if (GUILayout.Button("Inspect Door Repair Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.DoorRepairTest()); status = "Select human #1. The quiet door starts at 40 HP. Step 10 ticks to reach 50 HP; five more batches of 10 reach full repair, then normal calm departure resumes."; })); // Inspect real health restoration without changing collision.
            if (GUILayout.Button("Inspect Warning Shelter Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.WarningShelter()); status = "Select human #3 below the room. Step 10 ticks to follow its response to a heard warning, then 10 more to see it enter and close the door."; })); // Demonstrate shelter seeking without a personal zombie sighting.
            if (GUILayout.Button("Inspect Door Bracing Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.DoorBrace()); status = "Select human #1. Step 10 ticks: it braces the attacked door. At tick 30 the door still stands; continued attacks eventually break it."; })); // Inspect a physical defense that buys time without healing damage.
            if (GUILayout.Button("Inspect Warning Attraction Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.WarningAttraction()); status = "Select zombie #4 below the room. Step 10 ticks: it approaches a heard human warning, then waits at the closed partition door without a visible target."; })); // Demonstrate the consequence of a witness warning others.
            if (GUILayout.Button("Inspect Human Warning Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HumanWarning()); status = "Select human #1 inside the room. Step 10 ticks to see a nearby warning, then 40 more to watch stress recover in quiet."; })); // Inspect hearing without granting sight through the closed door.
            if (GUILayout.Button("Inspect Zombie Sound Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ZombieSoundInvestigation()); status = "Select zombie #3. Step 10 ticks: it follows the heard door impacts. Closed doors still block it; only sight reveals the human after the breach."; })); // Demonstrate sound knowledge without a hidden human target.
            if (GUILayout.Button("Inspect Door Hearing Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.DoorHearing()); status = "Select human #1. Step 10 ticks: it heard an unseen door strike and postpones leaving. Step 50 more: quiet lets the hearing memory expire and stress recover."; })); // Isolate hearing from visual zombie knowledge and sustained attacks.
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
        private void FocusNextHungryHuman()
        {
            if(world==null || !world.HungerEnabled || world.Count==0)return;
            int start=selected>=0 && selected<world.Count?selected+1:0;
            for(int offset=0;offset<world.Count;offset++)
            {
                int index=(start+offset)%world.Count;ActorState person=world.ActorAt(index);
                if(person.species!=Species.Human || person.hunger<HungerRules.SeekAt)continue;
                SetPaused(true);selected=index;selectedShelf=0;selectedBatCell=-1;placing=null;shelfBrush=-1;centerX=person.x+0.5f;centerY=person.y+0.5f;textureX=-1;
                status="Selected hungry human #"+person.id+" ("+person.hunger+"/100). Click Next hungry human to cycle.";return;
            }
            status="No hungry humans remain.";
        }
        private void FocusNextGroundBat()
        {
            WeaponPickupDocument[] sites=world.WeaponSites();if(sites.Length==0)return;WeaponPickupDocument chosen=sites[0];foreach(WeaponPickupDocument site in sites)if(site.y*world.Width+site.x>focusedBatCell){chosen=site;break;}
            SetPaused(true);selected=-1;selectedShelf=0;placing=null;shelfBrush=-1;focusedBatCell=chosen.y*world.Width+chosen.x;selectedBatCell=focusedBatCell;centerX=chosen.x+0.5f;centerY=chosen.y+0.5f;textureX=-1;status="Ground bat at "+chosen.x+", "+chosen.y+" | Bats in pile: "+chosen.Quantity+(world.WeaponDurability?" | Best condition: "+(MeleeRules.BatHits-(chosen.wear==null?0:chosen.wear[0]))+"/"+MeleeRules.BatHits:"")+". Humans still need to see it and collect it when safe.";
        }
        private void FocusNextStockedShelf()
        {
            if(world==null || !world.HungerEnabled)return;
            ShelfDocument first=null,next=null;
            foreach(ShelfDocument shelf in world.ShelfSites())
            {
                if(shelf.food<=0)continue;if(first==null)first=shelf;
                if(shelf.y*world.Width+shelf.x+1>selectedShelf){next=shelf;break;}
            }
            if(next==null)next=first;if(next==null){status="No stocked shelves remain.";return;}
            SetPaused(true);selected=-1;selectedBatCell=-1;selectedShelf=next.y*world.Width+next.x+1;placing=null;shelfBrush=-1;
            centerX=next.x+0.5f;centerY=next.y+0.5f;textureX=-1;
            status="Selected shelf at "+next.x+", "+next.y+" ("+next.food+" meals). Click Next stocked shelf to cycle.";
        }
        private bool SelectedTarget(out int x,out int y,out Color color,out string label)
        {
            x=0;y=0;color=Color.white;label="";if(world==null || selected<0 || selected>=world.Count)return false;
            ActorState actor=world.ActorAt(selected);
            if(actor.species==Species.Human)
            {
                if(world.HungerEnabled && actor.turnAtTick==0 && actor.foodUntilTick>world.Tick){x=actor.foodX;y=actor.foodY;color=new Color(0.18f,0.82f,0.75f);label="Food target";}
                else if(world.FoodExploration && actor.turnAtTick==0 && actor.foodExploreRoomId!=0){x=actor.shelterX;y=actor.shelterY;color=new Color(0.7f,0.45f,1f);label="Room investigation";}
                else if(world.SheltersEnabled && actor.shelterState==ShelterState.Seeking){x=actor.shelterX;y=actor.shelterY;color=new Color(0.25f,0.55f,1f);label="Shelter target";}
                else return false;
            }
            else if(world.PursuitMemory && actor.targetId!=0){x=actor.lastSeenX;y=actor.lastSeenY;color=new Color(1f,0.3f,0.35f);label="Observed human";}
            else
            {
                color=new Color(1f,0.72f,0.18f);
                if(world.SearchNoise && actor.heardSearchUntilTick>world.Tick && actor.heardSearchUntilTick>=actor.heardWarningUntilTick && actor.heardSearchUntilTick>=actor.heardDoorUntilTick){x=actor.heardSearchX;y=actor.heardSearchY;label=actor.heardSearchKind==1?"Heard door movement":"Heard shelf search";}
                else if(world.ZombieWarningInvestigation && actor.heardWarningUntilTick>world.Tick && actor.heardWarningUntilTick>=actor.heardDoorUntilTick){x=actor.heardWarningX;y=actor.heardWarningY;label="Heard warning";}
                else if(world.ZombieSoundInvestigation && actor.heardDoorUntilTick>world.Tick){x=actor.heardDoorX;y=actor.heardDoorY;label="Heard door impact";}
                else return false;
            }
            label+=": "+x+", "+y;return true;
        }
        private void DrawSelectedTarget(Rect bounds,int left,int top,int width,int height,float cell)
        {
            int x,y;Color color;string label;if(!showTargetMarkers || !SelectedTarget(out x,out y,out color,out label) || x<left || y<top || x>=left+width || y>=top+height)return;
            float px=bounds.x+(x-left+0.07f)*cell,py=bounds.y+(y-top+0.07f)*cell,size=cell*0.86f,edge=Math.Max(1f,Math.Min(3f,cell*0.08f));
            GUI.color=color;GUI.DrawTexture(new Rect(px,py,size,edge),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(px,py+size-edge,size,edge),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(px,py,edge,size),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(px+size-edge,py,edge,size),Texture2D.whiteTexture);GUI.color=Color.white;
        }
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
            if (world.HumanReactions && !world.FirstWeaponPriority && GUILayout.Button(world.UnarmedCourtesy ? "Enable first weapon priority" : world.SupplyCourtesy ? "Enable first bat courtesy" : world.ReplacementTrips ? "Enable supply courtesy" : world.WeaponReplacement ? "Enable replacement trips" : world.CombatFootwork ? "Enable weapon replacement" : world.WeaponDurability ? "Enable combat footwork" : world.BiteEvasion ? "Enable bat durability" : world.ShelterSpacing ? "Enable bite evasion" : world.CombatExperience ? "Enable drops and shelter spacing" : world.WeaponSeeking ? "Enable combat experience" : world.MeleeCombat ? "Enable weapon seeking" : world.FoodExploration ? "Enable melee combat" : world.FoodYieldChoice ? "Enable food exploration" : world.FoodRetreatFallback ? "Enable food yield choices" : world.FoodTripRetreat ? "Enable alternate shelter returns" : world.SafeFoodTargets ? "Enable food trip retreat" : world.FairFoodPickup ? "Enable guarded food routes" : world.FoodRouteChoice ? "Enable considerate pickup" : world.DoorMovementNoise ? "Enable food route choices" : world.QuickStockPickup ? "Enable door movement noise" : world.FoodRouteMemory ? "Enable quick food pickup" : world.SearchCoordination ? "Enable food route memory" : world.CarefulSearch ? "Enable search coordination" : world.SearchNoise ? "Enable cautious searching" : world.FoodDelivery ? "Enable search noise" : world.PantryTrips ? "Enable food delivery" : world.PantryStorage ? "Enable pantry trips" : world.ShelfReports ? "Enable pantry storage" : world.ShelfMemoryEnabled ? "Enable shelf reports" : world.FoodSharing ? "Enable shelf memory" : world.HungerEnabled ? "Enable food sharing" : world.DoorRepair ? "Enable hunger and inventory" : world.WarningSheltering ? "Enable door repair" : world.DoorBracing ? "Enable warning sheltering" : world.ZombieWarningInvestigation ? "Enable door bracing" : world.HumanWarnings ? "Enable warning attraction" : world.ZombieSoundInvestigation ? "Enable human warnings" : world.DoorHearing ? "Enable zombie sound" : world.DoorBreaking ? "Enable door hearing" : world.ZombieWandering ? "Enable door breaking" : world.ShelterRecovery ? "Enable zombie wandering" : world.DoorPursuit ? "Enable shelter recovery" : world.SheltersEnabled ? "Update doorway pursuit" : world.InfectedEscape ? "Enable shelter behavior" : world.CautiousMovement ? "Update escape behavior" : "Use cautious behavior")) Queue(() => { SetPaused(true); SaveDocument saved = world.Capture(); saved.rulesVersion = EncounterRules.Version; World candidate = World.Restore(saved); world = candidate; status = "First weapon priority, first bat courtesy, supply courtesy and replacement trips enabled; run remains paused. Original save file is unchanged; save to a new name to preserve it."; }); // Explicitly upgrade an older reaction run without losing actors, clocks or random state.
            if(GUILayout.Button(new GUIContent("Restart scenario","Restore this scenario's starting setup, or the loaded checkpoint, and pause. Current progress is replaced.")))Queue(RestartScenario);
            if (GUILayout.Button("Main Menu")) Queue(() => { SetPaused(true); canResume = true; showSetup = false; menuScroll = new Vector2(); showStart = true; }); // Pause and retain world, camera, selection and speed while showing the menu.
            if (GUILayout.Button("Zoom +")) Queue(() => { span = Math.Max(16, span / 1.25f); textureX = -1; }); if (GUILayout.Button("Zoom -")) Queue(() => { span = Math.Min(256, span * 1.25f); textureX = -1; }); // Keep rendering changes outside the current event traversal.
            GUILayout.EndHorizontal(); GUILayout.Label(status); GUILayout.EndArea(); // Keep implementation scope and errors readable in the shell.
            DrawWorld(new Rect(12, 138, Math.Max(64, Screen.width - 300), Math.Max(64, Screen.height - 184))); // Cull the map through a bounded viewport.
            GUILayout.BeginArea(new Rect(Screen.width - 276, 138, 264, Screen.height - 184), GUI.skin.box); inspectorScroll = GUILayout.BeginScrollView(inspectorScroll); // Keep selection diagnostics outside the map viewport.
            if(world.HungerEnabled)
            {
                FoodOverview food=world.FoodSupplies();GUILayout.Label("FOOD OVERVIEW");
                GUILayout.Label("Humans: " + food.Humans + " | Infected: " + food.Infected);
                GUILayout.Label("Hungry (40+): " + food.Hungry + "\nVery hungry (80+): " + food.VeryHungry);
                GUILayout.Label("Food trips: " + food.SeekingFood + " | Searching: " + food.Searching);
                GUILayout.Label("Investigating rooms: " + food.InvestigatingRooms);
                GUILayout.Label("Meals: carried " + food.CarriedMeals + " | shelves " + food.ShelfMeals);
                GUILayout.Label(food.RemainingMeals==0?"Food supply exhausted":"Food remaining: "+food.RemainingMeals+" meals");
                GUILayout.Label("Shelves: stocked " + food.StockedShelves + " | empty " + food.EmptyShelves + "\nUnsearched shelves: " + food.UnsearchedShelves);
                bool overviewEnabled=GUI.enabled;GUI.enabled=overviewEnabled && clock.Paused && world.Count>0;
                if(GUILayout.Button(new GUIContent("Step 1 game hour","Advance 600 ticks using ordinary simulation actions and remain paused.")))Queue(()=>clock.StepTicks(TickClock.MaxManualTicks,StepWorld));
                GUI.enabled=overviewEnabled && food.Hungry>0;
                if(GUILayout.Button(new GUIContent("Next hungry human","Pause and focus the next human at hunger 40+, in stable identity order.")))Queue(FocusNextHungryHuman);
                GUI.enabled=overviewEnabled && food.StockedShelves>0;
                if(GUILayout.Button(new GUIContent("Next stocked shelf","Pause and focus the next shelf with food, including unsearched stock in the map overview.")))Queue(FocusNextStockedShelf);
                GUI.enabled=overviewEnabled;
            }
            if(GUILayout.Button(showTargetMarkers?"Hide target markers":"Show target markers"))Queue(()=>showTargetMarkers=!showTargetMarkers);
            int targetX,targetY;Color targetColor;string targetLabel;
            if(SelectedTarget(out targetX,out targetY,out targetColor,out targetLabel))GUILayout.Label("Selected target: "+targetLabel);
            if(showTargetMarkers)GUILayout.Label("Targets: teal = food | blue = shelter\nRed = sight memory | amber = sound");
            GUILayout.Label("MOVEMENT INSPECTOR"); GUILayout.Label("Middle-drag: pan\nWheel: zoom\nClick: inspect a body"); // Explain the few controls that exist in this slice.
            if (world.SheltersEnabled) GUILayout.Label("Doors: light brown = closed | dark = open\nRed tint = damage | grey = broken"); // Make actual door actions visible while the temporary debug tiles remain.
            if (world.Count == 0) GUILayout.Label("TERRAIN ONLY: no bodies loaded. Place test bodies below, then click Run."); // Make the empty-population state prominent beside the map.
            if (GUILayout.Button("New movement test")) Queue(() => { SetPaused(true); confirmReset = true; }); // Open the confirmation on the next stable GUI tree.
            if (GUILayout.Button(new GUIContent("Place human", "Pause and place one baseline human on your next valid map click."))) Queue(() => { SetPaused(true); placing = Species.Human; shelfBrush = -1; status = "Click a walkable cell to place one human."; }); // Arm placement after the current control tree finishes.
            if (GUILayout.Button(new GUIContent("Place zombie", "Pause and place one baseline zombie on your next valid map click."))) Queue(() => { SetPaused(true); placing = Species.Zombie; shelfBrush = -1; status = "Click a walkable cell to place one zombie. Encounter rules apply only in Encounter runs."; }); // Keep species identity explicit without inserting controls mid-event.
            if (world.HungerEnabled) { if (GUILayout.Button("Place stocked shelf")) Queue(() => { SetPaused(true); placing = null; shelfBrush = 6; status = "Click a free cell for a shelf containing six meals."; }); if (GUILayout.Button("Place empty shelf")) Queue(() => { SetPaused(true); placing = null; shelfBrush = 0; status = "Click a free cell for an empty searchable shelf."; }); }
            if(world.MeleeCombat)
            {
                int groundBats=0,armedHumans=0,retainedBats=0;foreach(WeaponPickupDocument pile in world.WeaponSites())groundBats+=pile.Quantity;for(int i=0;i<world.Count;i++){ActorState carrier=world.ActorAt(i);if(carrier.meleeWeapon==MeleeWeapon.Bat){if(carrier.species==Species.Human)armedHumans++;else retainedBats++;}}
                GUILayout.Label("WEAPON OVERVIEW");GUILayout.Label("Bats on ground: "+groundBats+"\nArmed humans: "+armedHumans+"\nBats retained by zombies: "+retainedBats);
                bool supplyEnabled=GUI.enabled;GUI.enabled=supplyEnabled && groundBats>0;if(GUILayout.Button("Next ground bat"))Queue(FocusNextGroundBat);GUI.enabled=supplyEnabled;
                bool equipmentEnabled=GUI.enabled;GUI.enabled=equipmentEnabled && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human && world.ActorAt(selected).turnAtTick==0 && world.ActorAt(selected).meleeWeapon==MeleeWeapon.None;
                if(GUILayout.Button(new GUIContent("Place bat at feet","Pause and place one ground bat at the selected human's cell. Picking it up costs an action."))){int recipientIndex=selected;World recipientWorld=world;Queue(()=>{if(world!=recipientWorld || recipientIndex<0 || recipientIndex>=world.Count)return;SetPaused(true);ActorState recipient=world.ActorAt(recipientIndex);if(recipient.species!=Species.Human || recipient.turnAtTick!=0 || recipient.meleeWeapon!=MeleeWeapon.None)return;status=world.TryPlaceBat(recipient.x,recipient.y)?"Bat placed. Step ticks so the human can pick it up when safe.":"Bat needs a walkable non-door cell without another pickup.";});}
                GUI.enabled=equipmentEnabled;
                if(selected>=0 && selected<world.Count){ActorState fighter=world.ActorAt(selected);GUILayout.Label("Melee weapon: "+fighter.meleeWeapon+"\nBlunt skill: "+fighter.bluntSkill+"/10"+(fighter.health==0?"\nDEFEATED":fighter.species==Species.Zombie?"\nEquipment inactive after turning":fighter.nextMeleeTick>world.Tick?"\nSwing cooldown: "+(fighter.nextMeleeTick-world.Tick)+" ticks":""));}
            }
            if(selected>=0 && selected<world.Count)GUILayout.Label("Health: "+world.ActorAt(selected).health+"/100");
            DrawGroundBatInspector();
            if(world.WeaponDurability && selected>=0 && selected<world.Count){ActorState owner=world.ActorAt(selected);if(owner.meleeWeapon==MeleeWeapon.Bat)GUILayout.Label("Bat condition: "+(MeleeRules.BatHits-owner.batWear)+"/"+MeleeRules.BatHits+" hits remaining");if(weaponMotion[selected].RecentlyBroken(world.Tick))GUILayout.Label("Bat broke");if(weaponMotion[selected].RecentlySwapped(world.Tick))GUILayout.Label("Bat replaced");}
            if(world.BiteEvasion && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human){ActorState defender=world.ActorAt(selected);GUILayout.Label("Bite evasion: agility "+defender.agility+" | stress "+defender.stress);BiteAction outcome=biteFeedback[selected];if(outcome.targetId==defender.id && world.Tick>=outcome.tick && world.Tick-outcome.tick<BiteRules.FeedbackTicks)GUILayout.Label("Last bite attempt: "+(outcome.evaded?"DODGED":"BITTEN")+"\nZombie #"+outcome.actorId+" | evasion chance "+outcome.dodgeChance+"%");}
            if(world.CombatExperience && selected>=0 && selected<world.Count){ActorState learner=world.ActorAt(selected);if(learner.species==Species.Human)GUILayout.Label((learner.bluntSkill==10?"Blunt practice: maximum skill":"Blunt practice: "+learner.bluntPractice+"/"+MeleeRules.HitsPerLevel(learner.bluntSkill)+" hits")+(learner.meleeWeapon==MeleeWeapon.Bat?"\nBat damage: "+MeleeRules.Damage(learner):""));}
            if ((placing.HasValue || shelfBrush >= 0) && GUILayout.Button("Cancel placement")) Queue(() => { placing = null; shelfBrush = -1; status = "Placement canceled."; });
            if(world.FoodExploration && selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human){ActorState explorer=world.ActorAt(selected);GUILayout.Label("Recently investigated rooms: "+(explorer.foodRoomVisits==null?0:explorer.foodRoomVisits.Length)+(explorer.foodExploreRoomId!=0?"\nInvestigating room "+explorer.foodExploreRoomId:""));}
            if (world.HungerEnabled && selected >= 0 && selected < world.Count && world.ActorAt(selected).species == Species.Human) { ActorState person = world.ActorAt(selected); GUILayout.Label("Hunger " + person.hunger + "/100\nFood inventory " + person.carriedFood + "/" + HungerRules.CarryLimit + " meals\nFood-trip risk " + world.FoodTripRisk(selected) + " / tolerance " + HungerRules.RiskBudget(person) + (person.foodUntilTick > world.Tick ? "\nKnown shelf at " + person.foodX + ", " + person.foodY : "") + (person.homeShelterId != 0 ? "\nSupply trip: return to room " + person.homeShelterId : "")); }
            if (world.ShelfMemoryEnabled && selected >= 0 && selected < world.Count) { ShelfMemoryRecord[] notes = world.ActorAt(selected).shelfMemory; if (notes != null) { GUILayout.Label("Remembered shelves: " + notes.Length + "/" + ShelfMemoryRules.Limit); foreach (ShelfMemoryRecord note in notes) GUILayout.Label("Shelf " + note.x + ", " + note.y + ": " + (note.food < 0 ? "unsearched" : note.food == 0 ? "remembered empty" : "last seen " + note.food + " meals") + (note.reportedBy != 0 ? "\nTold by human #" + note.reportedBy : "\nPersonally observed") + "\nSource observation " + ((world.Tick - note.observedTick) / 5.0).ToString("0.0") + " seconds ago"); } } // Distinguish remembered contents from current hidden shelf state.
            if (selectedShelf > 0) { ShelfDocument shelf = world.ShelfAt((selectedShelf - 1) % world.Width, (selectedShelf - 1) / world.Width); if (shelf != null) GUILayout.Label("Shelf at " + shelf.x + ", " + shelf.y + "\nStock: " + shelf.food + " meals\n" + (shelf.searched ? "Searched: " + shelf.food + " meals remain" : "Unsearched: contents unknown to humans")); }
            GUILayout.Label("Sparse chunks: " + world.AllocatedChunks + "\nTick debt: " + clock.DebtSeconds.ToString("0.000") + " s\nMap: " + world.Width + " x " + world.Height); // Show storage and scheduling evidence directly.
            if (world.DoorBracing && selected >= 0 && selected < world.Count) { ActorState defender = world.ActorAt(selected); if (defender.bracedDoor != 0) GUILayout.Label("Bracing door at " + ((defender.bracedDoor - 1) % world.Width) + ", " + ((defender.bracedDoor - 1) / world.Width)); } // Display the actual saved defensive action.
            if (world.HumanWarnings && selected >= 0 && selected < world.Count) { ActorState listener = world.ActorAt(selected); if (listener.heardWarningUntilTick > world.Tick) GUILayout.Label((listener.species == Species.Zombie ? "Heard warning / investigation at " : "Heard human warning at ") + listener.heardWarningX + ", " + listener.heardWarningY + "\nRecent for " + ((listener.heardWarningUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds"); } // Show the speaker location rather than a concealed zombie.
            if (world.DoorHearing && selected >= 0 && selected < world.Count) { ActorState listener = world.ActorAt(selected); if (listener.heardDoorUntilTick > world.Tick) GUILayout.Label((listener.species == Species.Zombie ? "Heard impact / investigation at " : "Heard door impact at ") + listener.heardDoorX + ", " + listener.heardDoorY + "\nRecent for " + ((listener.heardDoorUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds"); } // Display a heard location without inventing a visible attacker.
            if (world.DoorBreaking && selected >= 0 && selected < world.Count) { ActorState actor = world.ActorAt(selected); for (int y = actor.y - 1; y <= actor.y + 1; y++) for (int x = actor.x - 1; x <= actor.x + 1; x++) { int health = world.DoorHealthAt(x, y); if (health >= 0) GUILayout.Label("Nearby door " + x + ", " + y + ": " + (health == 0 ? "BROKEN" : health + "/" + DoorRules.MaxHealth + " HP" + (world.DoorIsBraced(x, y) ? " / BRACED" : ""))); } } // Read actual nearby durability without recomputing or mutating pursuit routes.
            if (selected >= 0 && selected < world.Count) { ActorState actor = world.ActorAt(selected); if(world.FoodRouteMemory && actor.blockedFoodUntilTick>world.Tick)GUILayout.Label("Blocked food route: " + actor.blockedFoodX + ", " + actor.blockedFoodY + " | retry in " + ((actor.blockedFoodUntilTick-world.Tick)/5.0).ToString("0.0") + "s"); if(world.CarefulSearch && actor.species==Species.Human && actor.shelfSearchProgress>0)GUILayout.Label("Shelf search: " + actor.shelfSearchProgress + "/3 actions at " + actor.foodX + ", " + actor.foodY); GUILayout.Label("#" + actor.id + " " + actor.species + "\nCell " + actor.x + ", " + actor.y + "\nHealth " + (actor.health / 10f).ToString("0.0") + "\nS/P/E/C/I/A/L: " + actor.strength + "/" + actor.perception + "/" + actor.endurance + "/" + actor.charisma + "/" + actor.intelligence + "/" + actor.agility + "/" + actor.luck + "\nBravery " + actor.bravery + (actor.shelterId != 0 ? "\nShelter " + actor.shelterId + ": " + actor.shelterState + "\nDestination " + actor.shelterX + ", " + actor.shelterY : "") + (world.ShelterRecovery && actor.shelterId != 0 ? "\nExit: " + actor.shelterExitPhase + (actor.shelterCalmSinceTick > 0 && actor.shelterExitPhase == ShelterExitPhase.None ? " | calm wait " + (Math.Max(0, ShelterRecoveryRules.CalmTicks - (world.Tick - actor.shelterCalmSinceTick)) / 5.0).ToString("0.0") + " seconds" : actor.stress > 0 ? " | recovering" : "") : "") + (actor.rejectedShelterId != 0 ? "\nRejected room " + actor.rejectedShelterId : "") + (world.HumanReactions && actor.species == Species.Human ? "\nStress " + HumanRules.Level(actor.stress) + " (" + actor.stress + ")\nResponse " + actor.reaction + "\nSupport " + actor.groupSupport + " | Pressure " + actor.threatPressure + "\nCourage margin " + HumanRules.Margin(actor) + (actor.freezeUntilTick > 0 ? "\nFreeze: " + ((actor.freezeUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds" : "") : "") + (actor.staggerUntilTick > 0 ? "\nStaggered" : "") + (actor.species == Species.Zombie && actor.targetId == 0 ? "\n" + (world.SearchNoise && actor.heardSearchUntilTick>world.Tick && actor.heardSearchUntilTick>=actor.heardWarningUntilTick && actor.heardSearchUntilTick>=actor.heardDoorUntilTick ? (actor.heardSearchKind==1 ? "Activity: investigating door movement at " : "Activity: investigating shelf search at ") + actor.heardSearchX + ", " + actor.heardSearchY + " (" + ((actor.heardSearchUntilTick-world.Tick)/5.0).ToString("0.0") + "s)" : world.ZombieWarningInvestigation && actor.heardWarningUntilTick > world.Tick && actor.heardWarningUntilTick >= actor.heardDoorUntilTick ? "Activity: investigating human warning" : world.ZombieSoundInvestigation && actor.heardDoorUntilTick > world.Tick ? "Activity: investigating heard impact" : "Idle: " + (world.ZombieWandering ? "occasional wandering" : "stationary (older rules)")) : "") + (actor.targetId != 0 ? "\nObserved human #" + actor.targetId + " at " + actor.lastSeenX + ", " + actor.lastSeenY + "\nMemory: " + ((actor.searchUntilTick - world.Tick) / 5.0).ToString("0.0") + " seconds" : "") + (actor.turnAtTick > 0 ? "\nInfected: turns in " + ((actor.turnAtTick - world.Tick) / 5.0).ToString("0.0") + " simulation seconds" : "")); } // Inspect actual saved statistics using a detached value record.
            GUILayout.EndScrollView(); GUILayout.EndArea(); GUI.Label(new Rect(12, Screen.height - 36, Screen.width - 24, 30), GUI.tooltip); // Display a plain-language explanation for hovered controls.
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
            DrawDoorCrossings(bounds,left,top,width,height,cell);
            foreach (ShelfDocument shelf in world.ShelfSites()) { if (shelf.x < left || shelf.y < top || shelf.x >= left + width || shelf.y >= top + height) continue; Rect furniture = new Rect(bounds.x + (shelf.x - left + 0.12f) * cell, bounds.y + (shelf.y - top + 0.12f) * cell, cell * 0.76f, cell * 0.76f); GUI.color = shelf.food == 0 ? new Color(0.4f, 0.4f, 0.36f) : new Color(0.68f, 0.57f, 0.25f); GUI.DrawTexture(furniture, Texture2D.whiteTexture); if (!confirmReset && !placing.HasValue && shelfBrush < 0 && input.type == EventType.MouseDown && input.button == 0 && furniture.Contains(input.mousePosition)) { int key = shelf.y * world.Width + shelf.x + 1; Queue(() => { selectedShelf = key; selected = -1; selectedBatCell=-1; }); input.Use(); } } // Gold means stocked and grey means empty for the player; human knowledge remains independent.
            foreach(WeaponPickupDocument pickup in world.WeaponSites())
            {
                if(pickup.x<left || pickup.y<top || pickup.x>=left+width || pickup.y>=top+height)continue;
                Rect bat=new Rect(bounds.x+(pickup.x-left+0.82f)*cell,bounds.y+(pickup.y-top+0.17f)*cell,cell*0.14f,cell*0.66f);
                GUI.color=GroundBatColor(world.WeaponDurability && pickup.wear!=null?pickup.wear[0]:0);GUI.DrawTexture(bat,Texture2D.whiteTexture);
                if(selectedBatCell==pickup.y*world.Width+pickup.x){GUI.color=Color.white;GUI.DrawTexture(new Rect(bat.x-cell*0.05f,bat.y+bat.height+cell*0.03f,cell*0.24f,cell*0.04f),Texture2D.whiteTexture);}
                Rect target=new Rect(bat.x-cell*0.06f,bat.y-cell*0.05f,cell*0.26f,cell*0.76f);
                if(!confirmReset && !placing.HasValue && shelfBrush<0 && input.type==EventType.MouseDown && input.button==0 && target.Contains(input.mousePosition)){int key=pickup.y*world.Width+pickup.x;Queue(()=>{SetPaused(true);selectedBatCell=key;focusedBatCell=key;selected=-1;selectedShelf=0;});input.Use();}
            }
            DrawSelectedTarget(bounds,left,top,width,height,cell);
            for (int i = 0; i < world.Count; i++) // Inspect the bounded active population instead of instantiating scene objects.
            { // Begin one visible body.
                ActorState actor = world.ActorAt(i); double time = world.Tick + (clock.Paused?0:clock.Alpha); float x = (float)((clock.Paused ? actor.x : motion[i].X(time)) + MovementPresentation.OffsetX(actor.id)), y = (float)((clock.Paused ? actor.y : motion[i].Y(time)) + MovementPresentation.OffsetY(actor.id)); // Interpolate only between committed simulation positions.
                if (x < left - 0.18f || y < top - 0.18f || x >= left + width - 0.18f || y >= top + height - 0.18f) continue; // Cull bodies outside the camera.
                Rect body = new Rect(bounds.x + (x - left + 0.18f) * cell, bounds.y + (y - top + 0.18f) * cell, cell * 0.64f, cell * 0.64f); GUI.color = selected == i ? Color.yellow : actor.health==0 ? new Color(0.4f,0.4f,0.4f) : actor.species == Species.Zombie ? new Color(0.23f, 0.29f, 0.14f) : new Color(0.75f, 0.8f, 0.92f); GUI.DrawTexture(body, Texture2D.whiteTexture); // Use visibly distinct temporary debug bodies until artwork bindings return.
                DrawHealthBar(body,actor.health,visible,cell);
                if(time<hitFlashUntil[i]){GUI.color=new Color(1f,0.75f,0.65f);GUI.DrawTexture(new Rect(body.x+cell*0.06f,body.y+cell*0.06f,body.width-cell*0.12f,body.height-cell*0.12f),Texture2D.whiteTexture);}
                BiteAction bite= biteFeedback[i];if(actor.species==Species.Human && bite.targetId==actor.id && time>=bite.tick && time<bite.tick+WeaponPresentation.HitFlashTicks){GUI.color=bite.evaded?new Color(0.45f,0.8f,1f):new Color(1f,0.5f,0.5f);GUI.DrawTexture(new Rect(body.x+cell*0.06f,body.y+cell*0.06f,body.width-cell*0.12f,body.height-cell*0.12f),Texture2D.whiteTexture);}
                if(actor.species==Species.Human && (actor.meleeWeapon==MeleeWeapon.Bat || weaponMotion[i].Breaking(time)))DrawBat(weaponMotion[i],x,y,time,bounds,left,top,cell,actor.health>0 && actor.species==Species.Human);
                if (!confirmReset && !placing.HasValue && shelfBrush < 0 && input.type == EventType.MouseDown && input.button == 0 && body.Contains(input.mousePosition)) { int chosen = i; Queue(() => { selected = chosen; selectedShelf = 0; selectedBatCell=-1; }); input.Use(); } // Defer selection so the inspector cannot add layout entries midway through a mouse event.
            } // Finish one visible body.
            GUI.color = Color.white; if (confirmReset || !visible.Contains(input.mousePosition)) return; // Keep modal confirmation and toolbar events out of map input.
            if (shelfBrush >= 0 && input.type == EventType.MouseDown && input.button == 0) { int x = left + (int)((input.mousePosition.x - bounds.x) / cell), y = top + (int)((input.mousePosition.y - bounds.y) / cell), meals = shelfBrush; Queue(() => { if (world.TryPlaceShelf(x, y, meals)) { shelfBrush = -1; textureX = -1; selectedShelf = y * world.Width + x + 1; selected = -1; selectedBatCell=-1; status = (meals == 0 ? "Placed EMPTY shelf: 0 meals. " : "Placed STOCKED shelf: " + meals + " meals. ") + "Gold = stocked; grey = empty. Humans still need to search."; } else status = "Shelf needs a free non-door cell, away from bodies and reserved destinations."; }); input.Use(); }
            if (placing.HasValue && input.type == EventType.MouseDown && input.button == 0) // Handle manual placement only after an explicit tool choice and a map click.
            { // Begin one placement command.
                int x = left + (int)((input.mousePosition.x - bounds.x) / cell), y = top + (int)((input.mousePosition.y - bounds.y) / cell); Species species = placing.Value; // Capture the chosen cell and species before the GUI event ends.
                Queue(() => { int id; if (world.TrySpawn(species, x, y, out id)) { selected = world.Count - 1; selectedShelf=0; selectedBatCell=-1; previous[selected] = world.ActorAt(selected); motion[selected].Snap(x, y, world.Tick); status = "Placed " + species + " #" + id + ". Click Run to start movement."; placing = null; } else status = "Cannot place here: choose a walkable cell with space, within the population cap."; }); input.Use(); // Synchronize new body history outside OnGUI and retain invalid placement for another attempt.
            } // Finish one placement command.
            if (input.type == EventType.MouseDrag && input.button == 2) { centerX -= input.delta.x / cell; centerY -= input.delta.y / cell; input.Use(); } // Pan the camera without moving logical bodies.
            if (input.type == EventType.ScrollWheel) { span = Mathf.Clamp(span * (input.delta.y > 0 ? 1.1f : 0.9f), 16, 256); input.Use(); } // Zoom the bounded viewport around its current camera center.
        } // Finish viewport rendering.
        private void DrawDoorCrossings(Rect bounds,int left,int top,int width,int height,float cell)
        {
            if(clock.Paused)return;double tick=world.Tick+clock.Alpha;
            for(int i=0;i<world.Count;i++)
            {
                ActorState actor=world.ActorAt(i);double x=motion[i].X(tick)+MovementPresentation.OffsetX(actor.id)+0.18,y=motion[i].Y(tick)+MovementPresentation.OffsetY(actor.id)+0.18;
                for(int cy=(int)Math.Floor(y);cy<=(int)Math.Floor(y+0.64);cy++)for(int cx=(int)Math.Floor(x);cx<=(int)Math.Floor(x+0.64);cx++)
                {
                    if(cx<left || cy<top || cx>=left+width || cy>=top+height || !motion[i].OverlapsCell(cx,cy,tick,actor.id) || world.TerrainAt(cx,cy)!=Terrain.DoorClosed)continue;
                    GUI.color=ColorForCell(cx,cy,true);GUI.DrawTexture(new Rect(bounds.x+(cx-left)*cell,bounds.y+(cy-top)*cell,cell,cell),Texture2D.whiteTexture);
                }
            }
            GUI.color=Color.white; // Keep a logically closed door visually open only while the rendered body still crosses it.
        }
        private static void DrawHealthBar(Rect body,int health,Rect viewport,float cell)
        {
            if(health<=0 || health>=100)return;
            float height=Mathf.Clamp(cell*0.08f,2,4),fraction=health/100f;
            Rect bar=new Rect(body.x,Math.Max(viewport.y,body.y-height-2),body.width,height);
            GUI.color=new Color(0.1f,0.1f,0.12f);GUI.DrawTexture(bar,Texture2D.whiteTexture);
            GUI.color=new Color(0.85f-0.35f*fraction,0.25f+0.4f*fraction,0.2f);GUI.DrawTexture(new Rect(bar.x,bar.y,bar.width*fraction,bar.height),Texture2D.whiteTexture);
        }
        private static Color GroundBatColor(int wear)
        {
            float worn=Mathf.Clamp(wear/(float)(MeleeRules.BatHits-1),0,1);
            return new Color(Mathf.Lerp(0.55f,0.49f,worn),Mathf.Lerp(0.34f,0.48f,worn),Mathf.Lerp(0.16f,0.45f,worn));
        }
        private void DrawGroundBatInspector()
        {
            if(selectedBatCell<0)return;WeaponPickupDocument pile=null;
            foreach(WeaponPickupDocument site in world.WeaponSites())if(site.y*world.Width+site.x==selectedBatCell){pile=site;break;}
            if(pile==null){GUILayout.Label("Selected bat pile is gone.");return;}
            GUILayout.Label("GROUND BATS at "+pile.x+", "+pile.y);GUILayout.Label("Bats in pile: "+pile.Quantity);
            if(!world.WeaponDurability){GUILayout.Label("Condition: not tracked for this run");return;}
            if(pile.wear==null){GUILayout.Label("Condition: "+MeleeRules.BatHits+"/"+MeleeRules.BatHits+" hits x "+pile.Quantity);return;}
            int start=0;while(start<pile.wear.Length){int end=start+1;while(end<pile.wear.Length && pile.wear[end]==pile.wear[start])end++;GUILayout.Label("Condition: "+(MeleeRules.BatHits-pile.wear[start])+"/"+MeleeRules.BatHits+" hits x "+(end-start));start=end;}
        }
        private void DrawBat(WeaponPresentation weapon,float x,float y,double tick,Rect bounds,int left,int top,float cell,bool active)
        {
            WeaponPose pose=weapon.Pose(x,y,tick,active);
            GUI.color=new Color(0.55f,0.34f,0.16f);
            for(int part=0;part<9;part++){double fraction=part/8.0;float size=cell*(part>=6?0.13f:0.085f);GUI.DrawTexture(new Rect(bounds.x+(float)(pose.handX+(pose.tipX-pose.handX)*fraction-left)*cell-size/2,bounds.y+(float)(pose.handY+(pose.tipY-pose.handY)*fraction-top)*cell-size/2,size,size),Texture2D.whiteTexture);}
        }
        private Color ColorForCell(int x, int y,bool forceOpen=false) // Distinguish real door damage from ordinary open and closed terrain.
        { // Begin condition-aware debug coloring.
            int health = world.DoorHealthAt(x, y); if (health == 0) return new Color(0.46f, 0.47f, 0.48f); // Broken doors become grey, preserving their passable state.
            if (health > 0) { bool open = forceOpen || world.TerrainAt(x, y) == Terrain.DoorOpen; float damage = 1 - health / (float)DoorRules.MaxHealth; return new Color(Mathf.Lerp(open ? 0.39f : 0.70f, open ? 0.47f : 0.72f, damage), Mathf.Lerp(open ? 0.33f : 0.62f, open ? 0.20f : 0.32f, damage), Mathf.Lerp(open ? 0.28f : 0.53f, open ? 0.18f : 0.30f, damage)); } return ColorFor(world.TerrainAt(x, y)); // Muted closed/open browns acquire a continuous red tint as damage accumulates.
        } // Finish terrain feedback.
        private static Color ColorFor(Terrain value) // Keep debug colors in presentation rather than in movement rules.
        { // Begin terrain presentation.
            if (value == Terrain.Wall || value == Terrain.Fence || value == Terrain.GateClosed) return new Color(0.18f, 0.18f, 0.2f); // Show solid structural cells clearly.
            if (value == Terrain.Road) return new Color(0.27f, 0.28f, 0.31f); if (value == Terrain.Sidewalk || value == Terrain.Crosswalk) return new Color(0.6f, 0.6f, 0.56f); // Distinguish travel surfaces in the fixture.
            if (value == Terrain.Floor) return new Color(0.55f, 0.45f, 0.32f); if (value == Terrain.Window) return new Color(0.3f, 0.65f, 0.72f); if (value == Terrain.Water) return new Color(0.15f, 0.36f, 0.6f); // Keep room floors, glass, and water visually separate.
            if (value == Terrain.DoorClosed) return new Color(0.70f, 0.62f, 0.53f); if (value == Terrain.DoorOpen) return new Color(0.39f, 0.33f, 0.28f); if (value == Terrain.GateOpen) return new Color(0.65f, 0.45f, 0.2f); return new Color(0.23f, 0.36f - (int)value * 0.005f, 0.22f); // Supply a stable grass fallback for remaining known terrain.
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
