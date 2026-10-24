using System; // Keep deterministic fixtures and error messages straightforward.
using System.Globalization; // Parse setup numbers consistently across machines.
using System.IO; // Remember the last folder used for native browsing.
using Outbreak.Contracts; // Inspect portable actor values without changing them.
using Outbreak.Core; // Drive the same production world and scheduler used by headless checks.
using Outbreak.Persistence; // Load candidates transactionally and save complete documents.
using UnityEngine; // Limit engine dependencies to this presentation adapter.
namespace Outbreak.UnityAdapter // Keep screen and platform behavior out of authoritative simulation code.
{ // Begin the temporary foundation inspection shell.
    public sealed partial class FoundationApp : MonoBehaviour // Render a minimal movement test before rebuilding the full product interface.
    { // Begin application presentation state.
        public const string BuildLabel = "Foundation 113 / configurable generated cities"; // Identify the actual source revision in native screenshots and diagnostics.
        private World world; private TickClock clock; private ActorState[] previous; private Texture2D terrain; // Own presentation caches independently from the simulation.
        private WeaponPresentation[] weaponMotion;
        private double[] hitFlashUntil;
        private BiteAction[] biteFeedback;
        private ShovePresentation[] grabMotion,struggleMotion;
        private int setupInfectionScale=1;
        private ShovePresentation[] shoveMotion;private double[] beforeRenderX,beforeRenderY;
        private MovementPresentation[] motion; // Blend ordinary moves over their full cadence without changing collision cells.
        private string status = "Foundation 1: movement, clock and persistence only.", folder = ""; // Make implemented scope visible in the application itself.
        private float centerX, centerY, span = 48; private int selected = -1; private bool confirmReset, showStart = true; private Species? placing; // Keep camera, start flow and manual placement outside running saves.
        private bool showSetup, setupPaused = true; private ScenarioMap setupMap = ScenarioMap.ShelterArena; private string setupHumans = "24", setupZombies = "1", setupSeed = "214", setupBats = ScenarioBuilder.DefaultBats.ToString(CultureInfo.InvariantCulture); private Vector2 menuScroll; // Retain editable setup independently of any active run.
        private bool showTargetMarkers=true;
        private bool phoneBrush;private int selectedPhone;
        private bool showCenters;private int centerCourier=-1;
        private bool showStores;
        private int scheduleActor=-1,scheduleStart=-1,scheduleHours=-1;private string shiftStartInput="7",shiftHoursInput="12";
        private bool showHouseholds;private int householdHouse;
        private int focusedSupplyRoom;private bool showAllShelterSupplies;
        private bool showInfectionMarkers=true;
        private bool watchNextTurn;
        private Vector2 inspectorScroll; // Let inventory and detailed diagnostics remain reachable in smaller Game views.
        private int focusedBatCell=-1, selectedBatCell=-1;
        private bool restBrush,eraseRestSpot;
        private bool boxBrush;private int selectedStorageBox;private int courierEditor=-1;
        private string storageReserveInput="6";private int reserveInputBox=-1,reserveInputValue=-1;
        private int shelfBrush = -1, selectedShelf; // Keep furniture authoring and selection out of saved simulation state.
        private const int HistoryCapacity=120;
        private readonly int[] historyHumans=new int[HistoryCapacity],historyZombies=new int[HistoryCapacity];
        private readonly long[] historyTicks=new long[HistoryCapacity];
        private int historyCount,historyNext;private long historyStartTick,historyBucket=-1;
        private SaveDocument scenarioStart; private bool scenarioAdvanced; private int newConversions;
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
            inspectorIdentity="";showCenters=false;centerCourier=-1;showStores=false;scheduleActor=-1;clerkScheduleActor=-1;showHouseholds=false;householdHouse=0;courierEditor=-1;focusedSupplyRoom=0;reserveInputBox=-1;reserveInputValue=-1;watchNextTurn=false;scenarioStart=next.Capture();scenarioAdvanced=false;newConversions=0;canResume = false; showSetup = false; showCitySetup = false; menuScroll = new Vector2(); world = next; clock = new TickClock();shoveMotion=new ShovePresentation[world.Capacity];beforeRenderX=new double[world.Capacity];beforeRenderY=new double[world.Capacity]; previous = new ActorState[world.Capacity]; motion = new MovementPresentation[world.Capacity]; weaponMotion=new WeaponPresentation[world.Capacity]; hitFlashUntil=new double[world.Capacity]; biteFeedback=new BiteAction[world.Capacity];grabMotion=new ShovePresentation[world.Capacity];struggleMotion=new ShovePresentation[world.Capacity]; centerX = world.Width / 2f; centerY = world.Height / 2f; span = Math.Min(128, Math.Max(16, world.Width)); selected = -1; textureX = -1; placing = null; shelfBrush = -1;restBrush=false;boxBrush=false;phoneBrush=false; selectedShelf = 0;selectedStorageBox=0;selectedPhone=0; focusedBatCell=-1; selectedBatCell=-1; showStart = false; // Begin loaded simulations paused, clear stale actions and retain their authoritative tick.
            ResetPopulationHistory();
            for (int i = 0; i < world.Count; i++) { previous[i] = world.ActorAt(i); motion[i].Snap(previous[i].x, previous[i].y, world.Tick);weaponMotion[i].Reset(); } // Prevent a visual jump from stale interpolation history.
        } // Finish one transactional screen swap.
        private void StepWorld() // Advance authoritative ticks and capture safe presentation targets.
        { // Begin one complete simulation step.
            if(!scenarioAdvanced){scenarioStart=world.Capture();ResetPopulationHistory();scenarioAdvanced=true;} // Include authored starting bodies and supplies before the first simulation tick.
            long revision = world.TerrainRevision, condition = world.DoorConditionRevision; for (int i = 0; i < world.Count; i++){previous[i] = world.ActorAt(i);beforeRenderX[i]=motion[i].X(world.Tick+1);beforeRenderY[i]=motion[i].Y(world.Tick+1);} world.Step(); if (world.TerrainRevision != revision || world.DoorConditionRevision != condition) textureX = -1; // Keep the previous canonical cells for corner validation.
            RecordPopulationHistory();
            int weaponEvent=0;
            for (int i = 0; i < world.Count; i++) { ActorState actor = world.ActorAt(i), before = previous[i];if(before.species==Species.Human && actor.species==Species.Zombie)newConversions++;weaponMotion[i].Move(actor.x-before.x,actor.y-before.y);if(weaponEvent<world.WeaponActionCount && world.WeaponActionAt(weaponEvent).actorId==actor.id)weaponMotion[i].Observe(world.WeaponActionAt(weaponEvent++),actor.x,actor.y); bool diagonal = actor.x != before.x && actor.y != before.y; bool safe = !diagonal || (TerrainRules.Walkable(world.TerrainAt(actor.x, before.y)) && TerrainRules.Walkable(world.TerrainAt(before.x, actor.y))); if (clock.Paused || (!world.MaulingEnabled && world.InfectedEscape && before.turnAtTick == 0 && actor.turnAtTick > 0)) motion[i].Snap(actor.x, actor.y, world.Tick); else motion[i].Observe(actor.x, actor.y, world.Tick, safe,world.TerrainAt(before.x,before.y)==Terrain.DoorOpen?EncounterRules.MoveInterval:world.MovementBlendTicks(i),world.SprintEnabled); } // Do not smooth a compound move across a solid corner.
            if(!clock.Paused)for(int e=0;e<world.ShoveActionCount;e++)
            {
                ShoveAction action=world.ShoveActionAt(e);int human=-1,zombie=-1;for(int i=0;i<world.Count;i++){int id=world.ActorAt(i).id;if(id==action.actorId)human=i;if(id==action.targetId)zombie=i;}if(human<0 || zombie<0)continue;
                int dx=Math.Sign(action.fromX-action.humanX),dy=Math.Sign(action.fromY-action.humanY);shoveMotion[human].Observe(dx,dy,action.tick);
                double touchX=beforeRenderX[human]+MovementPresentation.OffsetX(action.actorId)+0.84*dx-MovementPresentation.OffsetX(action.targetId),touchY=beforeRenderY[human]+MovementPresentation.OffsetY(action.actorId)+0.84*dy-MovementPresentation.OffsetY(action.targetId);
                // Blocked pushes keep the target on its verified cell; the human's lean and hands still show the shove.
                if((action.fromX!=action.toX || action.fromY!=action.toY) && previous[zombie].x==action.fromX && previous[zombie].y==action.fromY)motion[zombie].Push(beforeRenderX[zombie],beforeRenderY[zombie],touchX,touchY,action.toX,action.toY,action.tick);
            }
            for(int e=0;e<world.WeaponActionCount;e++){WeaponAction action=world.WeaponActionAt(e);if(!action.hit || action.kind!=WeaponActionKind.Swing)continue;for(int i=0;i<world.Count;i++)if(world.ActorAt(i).id==action.targetId){hitFlashUntil[i]=action.tick+WeaponPresentation.HitFlashTicks;break;}} // Flash the struck body, including a fatal hit, at its rendered position.
            for(int e=0;e<world.BiteActionCount;e++){BiteAction action=world.BiteActionAt(e);int victim=-1,attacker=-1;for(int i=0;i<world.Count;i++){int id=world.ActorAt(i).id;if(id==action.targetId){victim=i;biteFeedback[i]=action;}if(id==action.actorId)attacker=i;}if(world.MaulingEnabled && !clock.Paused && !action.evaded && victim>=0 && attacker>=0){int dx=Math.Sign(previous[victim].x-previous[attacker].x),dy=Math.Sign(previous[victim].y-previous[attacker].y);grabMotion[attacker].Observe(dx,dy,action.tick);struggleMotion[victim].Observe(-dx,-dy,action.tick);}}
            CheckTurnWatch();
        } // Finish canonical and presentation updates.
        private void ResetPopulationHistory()
        {
            historyCount=0;historyNext=0;historyBucket=-1;historyStartTick=world.Tick;RecordPopulationHistory(true);
        }
        private void RecordPopulationHistory(bool initial=false)
        {
            int humans=0,zombies=0;for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.health<=0)continue;if(actor.species==Species.Human)humans++;else if(actor.species==Species.Zombie)zombies++;}
            long bucket=(world.Tick-historyStartTick)/Rules.TicksPerSecond;
            bool replace=!initial && historyBucket==bucket && historyCount>0;
            int slot=replace?(historyNext+HistoryCapacity-1)%HistoryCapacity:historyNext;
            historyHumans[slot]=humans;historyZombies[slot]=zombies;historyTicks[slot]=world.Tick;
            if(!replace){historyNext=(historyNext+1)%HistoryCapacity;historyCount=Math.Min(HistoryCapacity,historyCount+1);}if(!initial)historyBucket=bucket;
        }
        private void DrawPopulationHistory()
        {
            if(!world.Encounter || historyCount==0)return;
            GUILayout.Label("OUTBREAK HISTORY");GUILayout.Label("Blue: humans | olive: active zombies");
            int first=(historyNext-historyCount+HistoryCapacity)%HistoryCapacity,peak=1;
            for(int n=0;n<historyCount;n++){int slot=(first+n)%HistoryCapacity;peak=Math.Max(peak,historyHumans[slot]+historyZombies[slot]);}
            Rect graph=GUILayoutUtility.GetRect(220,64);GUI.color=new Color(0.08f,0.09f,0.11f);GUI.DrawTexture(graph,Texture2D.whiteTexture);
            float width=graph.width/historyCount;
            for(int n=0;n<historyCount;n++){int slot=(first+n)%HistoryCapacity;float humanHeight=graph.height*historyHumans[slot]/peak,zombieHeight=graph.height*historyZombies[slot]/peak,x=graph.x+n*width;if(humanHeight>0){GUI.color=new Color(0.45f,0.65f,0.9f);GUI.DrawTexture(new Rect(x,graph.y+graph.height-humanHeight,width,humanHeight),Texture2D.whiteTexture);}if(zombieHeight>0){GUI.color=new Color(0.48f,0.55f,0.24f);GUI.DrawTexture(new Rect(x,graph.y+graph.height-humanHeight-zombieHeight,width,zombieHeight),Texture2D.whiteTexture);}}
            GUI.color=Color.white;GUILayout.Label("Ticks "+historyTicks[first]+"–"+historyTicks[(historyNext+HistoryCapacity-1)%HistoryCapacity]+" | Scale: 0–"+peak+" actors");
        }
        private static void ConvertLegacyEnergy(SaveDocument saved)
        {
            for(int i=0;i<saved.actors.Length;i++){saved.actors[i].energySpent=saved.actors[i].shoveFatigue*EnergyRules.ShoveCost;saved.actors[i].shoveFatigue=0;}
        }
        private static void SeedLegacyCorpseTimers(SaveDocument saved)
        {
            int scale=saved.infectionTimeScale==0?1:saved.infectionTimeScale;
            for(int i=0;i<saved.actors.Length;i++){ActorState actor=saved.actors[i];if(actor.species==Species.Human && actor.health==0 && actor.turnAtTick==0){actor.turnAtTick=saved.tick+InfectionRules.Delay(actor,scale);saved.actors[i]=actor;}}
        }
        private void RestartScenario() // Restore the starting checkpoint while retaining enabled rules upgrades.
        {
            SaveDocument initial=scenarioAdvanced?World.Restore(scenarioStart).Capture():world.Capture();if(world.EnergyEnabled && initial.rulesVersion!=EncounterRules.Version && initial.rulesVersion!=EncounterRules.EnergyVersion && initial.rulesVersion!=EncounterRules.BedVersion && initial.rulesVersion!=EncounterRules.FoodRestVersion && initial.rulesVersion!=EncounterRules.EnergyRiskVersion && initial.rulesVersion!=EncounterRules.EnergyReturnVersion && initial.rulesVersion!=EncounterRules.SprintVersion && initial.rulesVersion!=EncounterRules.SprintNoiseVersion && initial.rulesVersion!=EncounterRules.CorneredShoveVersion && initial.rulesVersion!=EncounterRules.MaulingVersion && initial.rulesVersion!=EncounterRules.ReanimationVersion && initial.rulesVersion!=EncounterRules.RiseVersion && initial.rulesVersion!=EncounterRules.RiseAwarenessVersion && initial.rulesVersion!=EncounterRules.StorageVersion && initial.rulesVersion!=EncounterRules.RestockingVersion && initial.rulesVersion!=EncounterRules.ReserveTargetsVersion && initial.rulesVersion!=EncounterRules.BoxMemoryVersion && initial.rulesVersion!=EncounterRules.BoxReportsVersion && initial.rulesVersion!=EncounterRules.SupplyDeliveriesVersion && initial.rulesVersion!=EncounterRules.DeliveryControlsVersion && initial.rulesVersion!=EncounterRules.CourierAssignmentsVersion && initial.rulesVersion!=EncounterRules.HouseholdFoodTargetsVersion && initial.rulesVersion!=EncounterRules.CourierSchedulesVersion && initial.rulesVersion!=EncounterRules.StoresVersion && initial.rulesVersion!=EncounterRules.DistributionCentersVersion && initial.rulesVersion!=EncounterRules.SupplyRequestsVersion && initial.rulesVersion!=EncounterRules.OrderFulfillmentVersion && initial.rulesVersion!=EncounterRules.SurvivalDamageVersion && initial.rulesVersion!=EncounterRules.ShortagePrioritiesVersion && initial.rulesVersion!=EncounterRules.HomeSuppliesVersion && initial.rulesVersion!=EncounterRules.StoreShoppingVersion && initial.rulesVersion!=EncounterRules.StoreDiscoveryVersion && initial.rulesVersion!=EncounterRules.WorkerShiftsVersion && initial.rulesVersion!=EncounterRules.LongRoutesVersion)ConvertLegacyEnergy(initial);if(world.ReanimationEnabled && initial.rulesVersion!=EncounterRules.Version)SeedLegacyCorpseTimers(initial);initial.rulesVersion=world.Capture().rulesVersion;World candidate=World.Restore(initial);UseWorld(candidate);status="Scenario restarted at tick "+world.Tick+", paused. Run or step to begin again.";
        }
        private void SetPaused(bool value) // Keep visual history coherent across an explicit pause boundary.
        { // Begin pause presentation synchronization.
#if UNITY_EDITOR // A deliberate Run action must also release a host pause that would prevent all future ticks.
            if (!value && UnityEditor.EditorApplication.isPaused) UnityEditor.EditorApplication.isPaused = false; // Resume the host only when the person explicitly starts playback.
#endif // Finish the editor-only playback action.
            if(value)watchNextTurn=false;
            clock.SetPaused(value); for (int i = 0; i < world.Count; i++) { previous[i] = world.ActorAt(i); motion[i].Snap(previous[i].x, previous[i].y, world.Tick);shoveMotion[i].Reset();grabMotion[i].Reset();struggleMotion[i].Reset(); } // Prevent stale interpolation from jumping backward when playback resumes.
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
            if (canResume) { SetPaused(true); resetAction = action; confirmReset = true; showSetup = false; showCitySetup = false; showStart = false; } else action(); // Draw confirmation over the retained world before starting another scenario.
        } // Finish scenario selection.
        private void DrawSetup() // Make population, map and repeatability choices available without source edits.
        { // Begin a separate scrollable setup card.
            GUI.color = new Color(0.07f, 0.08f, 0.1f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; float width = Math.Min(560, Screen.width - 24), height = Math.Min(620, Screen.height - 24); GUILayout.BeginArea(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), GUI.skin.box); menuScroll = GUILayout.BeginScrollView(menuScroll); GUILayout.Label("SET UP ENCOUNTER"); GUILayout.Label(BuildLabel); // Keep the form reachable in smaller Game views.
            GUILayout.Label("Selected map: " + ScenarioBuilder.MapName(setupMap)); GUILayout.BeginHorizontal(); if (GUILayout.Button("Shelter arena (20 x 16)")) Queue(() => { setupMap = ScenarioMap.ShelterArena; }); if (GUILayout.Button("Shelter district (100 x 100)")) Queue(() => { setupMap = ScenarioMap.ShelterDistrict; }); GUILayout.EndHorizontal(); // Keep map changes out of the active GUI tree traversal.
            GUILayout.Label("Humans at start"); setupHumans = GUILayout.TextField(setupHumans, 32); GUILayout.Label("Zombies at start"); setupZombies = GUILayout.TextField(setupZombies, 32); GUILayout.Label("Repeatable seed (1 to 4294967295)"); setupSeed = GUILayout.TextField(setupSeed, 32); GUILayout.Label("Bats at start (0 to " + ScenarioBuilder.BatLimit(setupMap) + ")"); setupBats=GUILayout.TextField(setupBats,32); // Leave invalid input visible so the player can correct it.
            if(GUILayout.Button("Infection pace: "+(setupInfectionScale==1?"Prototype":setupInfectionScale==InfectionRules.OneDayScale?"~1 game day":"~3 game days")))Queue(()=>{setupInfectionScale=setupInfectionScale==1?InfectionRules.OneDayScale:setupInfectionScale==InfectionRules.OneDayScale?InfectionRules.ThreeDayScale:1;});GUILayout.Label("Strength, endurance and luck extend infection time. Existing infections keep their deadline.");
            if (GUILayout.Button("Start paused: " + (setupPaused ? "Yes" : "No"))) Queue(() => { setupPaused = !setupPaused; }); GUILayout.Label("Maximum 1000 bodies total. This map's spawn limits: " + ScenarioBuilder.HumanLimit(setupMap) + " humans / " + ScenarioBuilder.ZombieLimit(setupMap) + " zombies."); GUILayout.Label("Species start in separate outdoor areas. Bats are scattered in the human starting area; collecting one costs an action. Humans use mixed brave, average and fearful test traits."); // Explain the actual prototype placement policy.
            int humans = 0, zombies = 0, bats=0; uint seed = 0; bool parsed = int.TryParse(setupHumans, NumberStyles.None, CultureInfo.InvariantCulture, out humans) && int.TryParse(setupZombies, NumberStyles.None, CultureInfo.InvariantCulture, out zombies) && uint.TryParse(setupSeed, NumberStyles.None, CultureInfo.InvariantCulture, out seed) && int.TryParse(setupBats,NumberStyles.None,CultureInfo.InvariantCulture,out bats); string error = parsed ? ScenarioBuilder.ValidationError(setupMap, humans, zombies, seed,bats) : "Enter whole numbers for humans, zombies and bats, and a seed from 1 to 4294967295."; GUILayout.Label(error ?? ("Ready: " + humans + " humans + " + zombies + " zombies + " + bats + " ground bats. " + (setupPaused ? "Use Run or tick buttons after starting." : "Simulation starts running."))); // Validate before exposing a start action.
            GUI.enabled = error == null; if (GUILayout.Button("Start configured encounter")) { ScenarioMap choice = setupMap; bool paused = setupPaused; int infectionScale=setupInfectionScale; Queue(() => StartScenario(() => { World candidate = ScenarioBuilder.Create(choice, humans, zombies, seed,bats);candidate.SetInfectionTimeScale(infectionScale); UseWorld(candidate); SetPaused(paused); status = ScenarioBuilder.MapName(choice) + ": " + humans + " humans, " + zombies + " zombies, " + bats + " ground bats; seed " + seed + ". " + (paused ? "Paused: Run or step to begin." : "Running."); })); } GUI.enabled = true; // Construct the complete candidate before replacing the retained run.
            if (GUILayout.Button("Back to Main Menu")) Queue(() => { showSetup = false; showCitySetup = false; menuScroll = new Vector2(); }); GUILayout.Label(status); GUILayout.EndScrollView(); GUILayout.EndArea(); // Back preserves both settings and the menu-paused world.
        } // Finish encounter setup.
        private void DrawStart() // Explain the available experiment before presenting an inert-looking map.
        { // Begin the start screen.
            if(showCitySetup){DrawCitySetup();return;}
            if (showSetup) { DrawSetup(); return; } // Keep setup separate from the diagnostic scenario list.
            GUI.color = new Color(0.07f, 0.08f, 0.1f); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; // Keep the start screen legible against a stable background.
            float width = Math.Min(560, Screen.width - 24), height = Math.Min(690, Screen.height - 24); GUILayout.BeginArea(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), GUI.skin.box); // Keep the choices centered in an ordinary Game view.
            GUILayout.Label("OUTBREAK SIMULATOR"); GUILayout.Label(BuildLabel); menuScroll = GUILayout.BeginScrollView(menuScroll); GUILayout.Space(12); // Identify the actual source revision without implying finished outbreak behavior.
            GUILayout.Label("Set up your own encounter, or use the quick start with 24 humans and one zombie. Scroll for the smaller behavior tests."); GUILayout.Label("Choose Encounter for stress, bravery, group support, fight/freeze/flight, pursuit and infection. Movement Test preserves the original diagnostic."); GUILayout.Space(12); // State what the user will see before running it.
            if(GUILayout.Button("Set Up Generated City"))Queue(()=>{showCitySetup=true;showSetup=false;menuScroll=new Vector2();});
            if (GUILayout.Button("Set Up Encounter")) Queue(() => { showSetup = true; menuScroll = new Vector2(); }); // Open custom counts without discarding the active run.
            if (canResume && GUILayout.Button("Resume current run")) Queue(() => { canResume = false; showStart = false; SetPaused(false); }); // Resume the exact retained world rather than resetting it.
            GUILayout.BeginHorizontal(); // Keep the three controlled reaction choices together.
            foreach (HumanReaction response in new[] { HumanReaction.Fight, HumanReaction.Freeze, HumanReaction.Flight }) if (GUILayout.Button("Inspect " + response + " Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HumanResponse(response)); status = "Select human #1, then step ticks to inspect " + response + ". Prototype defensive fighting uses shoves."; })); // Use the same authored fixtures as the acceptance suite.
            GUILayout.EndHorizontal(); // Finish controlled reaction choices.
            if(GUILayout.Button("Inspect Supply Delivery Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.SupplyDeliveryTest());selected=0;status="The left shelter needs food. This human remembers surplus in the right shelter. Run or step to watch it collect real food, preserve the donor's six-meal reserve, and carry supplies home.";}));
            if(GUILayout.Button("Inspect Shelter Shortages Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.ShelterShortageTest());status="The first room has hungry occupants and no food; a restocker is collecting supplies. The second is supplied and the third is unoccupied. Use Next shelter shortage or Show all shelters.";}));
            if(GUILayout.Button("Inspect Box Sharing Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.BoxReportTest());selected=1;status="Step one tick: human #1 tells hungry human #2 about a remembered box behind the wall. Run or step to watch #2 find food using that report.";}));
            if(GUILayout.Button("Inspect Box Memory Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.BoxMemoryTest());selected=0;status="This hungry human previously saw the box on the other side of the wall. Run or step to watch it walk around the corner, check current stock and collect food.";}));
            if(GUILayout.Button("Inspect Restocking Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.StorageRestockTest());selected=0;status="Run or step: the fed human checks the door, collects from its remembered outdoor shelf, returns and stocks the box toward a six-meal reserve.";}));
            if(GUILayout.Button("Inspect Idle Motion Paused"))Queue(()=>StartScenario(()=>{SaveDocument saved=EncounterScenarios.HungerDamageTest().Capture();saved.actors[0].health=100;saved.actors[0].hunger=10;saved.actors[0].energySpent=0;saved.map.name="Idle motion test";UseWorld(World.Restore(saved));selected=0;status="Run: this enclosed human shifts naturally while idle. Its grid position and energy stay unchanged; hunger still advances normally.";}));
            if(GUILayout.Button("Inspect Generated Buildings Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.GeneratedBuildingTrial());selected=0;showCenters=true;showStores=true;status="Seed 214: eight generated buildings, door paths to sidewalks, five assigned homes, two stores and one distribution center. Five humans share 63 finite meals.";}));
            if(GUILayout.Button("Inspect Generated Buildings Alternate Seed Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.GeneratedBuildingTrial(999));selected=0;showCenters=true;showStores=true;status="Seed 999: alternate generated building sizes and positions, each with a connected doorway path. Jobs and residences remain separate.";}));
            if(GUILayout.Button("Inspect Pedestrian Crossing Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.PedestrianCrossingTrial());selected=0;status="Off-duty courier walks home: follow the sidewalks to the lower crosswalk. Street use is a preference; an unavailable crossing can lead to an ordinary road crossing.";}));
            if(GUILayout.Button("Inspect Generated City Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.GeneratedCityTrial());selected=0;showCenters=true;showStores=true;status="Seed 214 street prototype: connected streets, sidewalks and crosswalks. Follow the courier delivering across more than 64 cells. Streets remain pedestrian terrain; lots and city population come next.";}));
            if(GUILayout.Button("Inspect Generated City Alternate Seed Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.GeneratedCityTrial(999));selected=0;showCenters=true;showStores=true;status="Seed 999: alternate connected street spacing, sidewalks and crosswalks. Same finite courier test, a different generated layout.";}));
            if(GUILayout.Button("Inspect Larger District Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.LargerDistrictTrial());selected=3;showStores=true;showCenters=true;status="64 x 36 district: three assigned homes, three familiar shops, two couriers and 109 finite meals. Follow residents shopping and near/far store deliveries. Roads are walking terrain.";}));
            if(GUILayout.Button("Inspect Worker Shift End Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.WorkerShiftTest());selected=1;showStores=true;showCenters=true;status="07:54: shifts end at 08:00. Workers finish loaded work and return to their separate assigned home. Brown = courier; yellow/blue = clerk; gray = off duty; white = consumer.";}));
            if(GUILayout.Button("Inspect Worker Shift Start Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.WorkerShiftTest(true));selected=1;showStores=true;showCenters=true;status="07:54: workers begin at home off duty. At 08:00 they commute to their workplaces and begin work.";}));
            if(GUILayout.Button("Inspect Store Discovery Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.StoreDiscoveryTest());selected=0;showStores=true;status="No store or shelf memories: the resident surveys outside, checks the nearer empty shop, discovers a second shop and brings food home. Run or step to observe personal discoveries.";}));
            if(GUILayout.Button("Inspect Store Shopping Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.StoreShoppingTest());selected=0;showStores=true;status="The selected resident knows a store shelf from an earlier visit. Its home chest is empty with a four-meal target. Run or step: shop, carry food home, and watch the clerk refill the shelf.";}));
            if(GUILayout.Button("Inspect Home Supplies Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.HomeSupplyInspectionTest());selected=0;status="This human has an assigned home but has not checked its chest. Run or step to inspect it. The distribution center is not a routine consumer food destination.";}));
            if(GUILayout.Button("Inspect Supply Chain Visuals Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.SupplyChainIdentityTest());showStores=true;showCenters=true;status="Visual preview: tan home at lower left, blue stores, green distribution center. Eight humans have a separate assigned home with six meals. Home shopping is the next increment.";}));
            if(GUILayout.Button("Inspect Multi-Store Supply Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.MultiStoreSupplyTrial());showCenters=true;showStores=true;status="Three stores, two center couriers and eight humans share 34 total meals. The center starts with 26 meals. Run or step game hours to observe competing phone orders, deliveries and shortages.";}));
            if(GUILayout.Button("Inspect Hunger Damage Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.HungerDamageTest());selected=0;status="Empty hunger and energy gauges: Step 50 ticks to see health loss. Food or recovered energy stops the deterioration.";}));
            if(GUILayout.Button("Inspect Courier Fulfillment Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.CourierFulfillmentTest());selected=0;showCenters=true;showStores=true;status="The clerk calls for four meals. A center courier with no manual destinations accepts the order, makes two deliveries and records completion.";}));
            if(GUILayout.Button("Inspect Phone Supply Order Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.PhoneSupplyOrderTest());selected=0;showStores=true;status="The store crate is empty. Its clerk walks to the cyan phone, makes a call and creates one pending replenishment order.";}));
            if(GUILayout.Button("Inspect Distribution Center Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.DistributionCenterTest());selected=0;showCenters=true;showStores=true;status="Center #183 supplies store #192 through an explicit courier route. The store clerk then carries delivered food onto its shelf.";}));
            if(GUILayout.Button("Inspect Store Clerk Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.StoreClerkTest());selected=0;showStores=true;status="Step 10 ticks: consumer empties the shelf while the clerk collects back stock. Run or step to watch the clerk carry food from the crate to the shelf.";}));
            if(GUILayout.Button("Inspect Courier Shift End Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.CourierScheduleTest());selected=0;status="18:54: courier works until 19:00. Run or step: finish the current delivery, then return home without starting the next destination.";}));
            if(GUILayout.Button("Inspect Household Food Targets Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.HouseholdFoodTargetsTest());showHouseholds=true;householdHouse=183;status="Two assigned residents need six meals, shared across two automatic boxes. Away couriers still count; visitors do not.";}));
            if(GUILayout.Button("Inspect Households Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.HouseholdManagementTest());showHouseholds=true;householdHouse=183;status="House #183 has one resident at home and one courier visiting house #192. Select a resident to change their assigned house.";}));
            if(GUILayout.Button("Inspect Assigned Courier Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.AssignedCourierTest());selected=0;status="Assigned courier: source box 4, 3; near destination 13, 3 and far destination 42, 3. Run or step to deliver.";}));
            if(GUILayout.Button("Inspect Storage Deposit Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.StorageSupplyTest());selectedStorageBox=3*world.Width+4+1;status="Step one tick: the human deposits two surplus meals and keeps one. Select the blue-grey box to inspect its inventory.";}));
            if(GUILayout.Button("Inspect Storage Meal Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.StorageSupplyTest(true));selectedStorageBox=3*world.Width+4+1;status="Step one tick: the hungry human takes three meals from the box. Step five more ticks: it eats one.";}));
            if(GUILayout.Button("Inspect Reanimation Escape Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.ReanimationEscapeTest());selected=1;status="Step two ticks: the nearby unarmed human backs away while the corpse turns green. Run to continue the encounter.";}));
            if(GUILayout.Button("Inspect Rising Zombie Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.RisingZombieTest());selected=0;status="Step one tick: the corpse reanimates at 50 health and fades from grey to green over five ticks. It cannot act until the transition ends. Step five more ticks: it can attack the nearby frozen human.";}));
            if(GUILayout.Button("Inspect Reanimation Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.ReanimationTest());selected=0;status="Run or step: this infected human dies at tick 2, stays a corpse until tick 20, then rises with 50 zombie health. Death keeps the original infection deadline.";}));
            if(GUILayout.Button("Inspect Mauling Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.MaulingTest());selected=0;status="This infected human is trapped at 45 health. Run: the zombie keeps grabbing and mauling until the human dies. Further attacks never reset its infection deadline; its corpse rises when that timer expires.";}));
            if(GUILayout.Button("Inspect Grapple Escape Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.GrappleEscapeTest());selected=1;status="Run: the zombie grabs and bites; this strong human struggles free and keeps fleeing while infected. Strength, endurance, luck and energy affect the escape.";}));
            if(GUILayout.Button("Inspect Turned Health Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.ConvertedHealthTest());selected=0;status="Step one tick: a human at 20 health turns into a zombie at the 50-health minimum, with its own health. Toggle its health bar in Vitals & needs.";}));
            if(GUILayout.Button("Inspect Cornered Shove Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.CorneredShoveTest());selected=0;status="Run: this frightened human has no retreat step, so it shoves the adjacent zombie without switching to Fight. Each push costs 30 energy and has a cooldown. It buys time, not guaranteed escape.";}));
            if(GUILayout.Button("Inspect Sprint Footsteps Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.SprintNoiseTest());selected=2;status="Step three ticks: running footsteps attract the hidden zombie toward the closed door. It hears the sprint cell, not the human's new position. Run to see the pursuit and sound-only investigation.";}));
            if(GUILayout.Button("Inspect Escape Sprint Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.EscapeSprintTest());selected=0;centerX=20;span=16;status="Run: the fleeing human takes extra escape steps while a zombie is nearby and energy is at least 30. Each extra step costs double walking energy. Door crossings retain their normal pace.";}));
            if(GUILayout.Button("Inspect Energy Return Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.EnergyReturnTest());selected=0;status="The forager has 18 energy and no food yet. Run: it turns back to shelter to recover before exhaustion. Urgent hunger or supplies within reach take priority.";}));
            if(GUILayout.Button("Inspect Tired Forager Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.TiredForagerTest());selected=0;status="The hungry human has 20 energy. Stress risk 35 plus low-energy penalty 20 exceeds tolerance 40. Run to see it wait for stress to settle before leaving.";}));
            if(GUILayout.Button("Inspect Food Trip Rest Paused"))Queue(()=>StartScenario(()=>{UseWorld(EncounterScenarios.FoodRestTest());selected=0;status="Hungry and tired: watch the human use a bed to regain at least 60 energy, then inspect the exit to search for food. Urgent hunger cuts rest short.";}));
            if (GUILayout.Button("Inspect Bed Rest Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.RestSpotTest());selected=0;status="The tired human walks to the teal bed inside its shelter. Run or step: it recovers 20 energy per resting action for one hunger point, then frees the spot when full."; }));
            if (GUILayout.Button("Inspect Energy Rest Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.EnergyRestTest());selected=0;status="This healthy human has 20/100 energy. Step 10 ticks: resting in shelter restores 20 energy and costs two hunger points. Shoves cost 30 energy; bat swings cost 10. Hunger and danger interrupt rest."; }));
            if (GUILayout.Button("Inspect Shove Fatigue Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShoveFatigueTest());selected=0;status="This human is already at 2/3 shove fatigue. Step 1 tick: its final shove reaches 3/3. Step 10 ticks: it prioritizes retreat. Fatigue recovers only after cooldown in perceived safety, not while a zombie remains visible."; }));
            if (GUILayout.Button("Inspect Shove Contact Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.ShoveContactTest());selected=0;status="Click Run to watch the shove: the human leans in with both hands, contact happens first, then the zombie recoils quickly. Pause and manual steps show exact simulation cells. Shoves remain nonlethal."; }));
            if (GUILayout.Button("Inspect Unarmed Spacing Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.UnarmedSpacingTest());selected=0;status="This alarmed, supported human is willing to fight but has no bat. Step 1 tick: it backs away instead of charging. Contact still allows a defensive shove; while that shove recovers, it tries to create space."; }));
            if (GUILayout.Button("Inspect Combat Noise Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.CombatNoiseTest());selected=2;status="Step 1 tick: a bat impact attracts the zombie behind the closed door. Step 10 ticks: it approaches the sound without knowing the human's position or breaking the door. Missed swings are quiet."; }));
            if (GUILayout.Button("Inspect Recovery Rations Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.RecoveryRationsTest());selected=0;status="This badly injured human keeps two meals rather than storing one in the empty shelf. Step 50 ticks: it rests, eats one meal when hungry and continues healing. At 50 health, the usual one-meal reserve resumes."; }));
            if (GUILayout.Button("Inspect Shelter Healing Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.RestHealingTest());selected=0;status="This injured, uninfected human rests in a safe room. Step 50 ticks: health rises from 60 to 80 and hunger increases. Hunger and danger interrupt healing; rest cannot cure infection."; }));
            if (GUILayout.Button("Inspect Health Bars Paused")) Queue(() => StartScenario(() => { UseWorld(EncounterScenarios.HealthBarsTest());selected=0;status="Select a living human or zombie, then toggle its health bar in Vitals & needs. Optional H/F/E bars appear only on the selected NPC; human hunger counts down from 100 fed to 0 starving."; }));
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
        private int NextFightIndex()
        {
            if(world==null || !world.Encounter || world.Count==0)return -1;int start=selected>=0 && selected<world.Count?selected+1:0;
            for(int offset=0;offset<world.Count;offset++){int index=(start+offset)%world.Count;if(world.CloseVisibleZombie(index)>=0)return index;}return -1;
        }
        private void FocusNextFight()
        {
            int index=NextFightIndex();if(index<0){status="No close confrontations remain.";return;}
            ActorState human=world.ActorAt(index),zombie=world.ActorAt(world.CloseVisibleZombie(index));RevealObservedEvent();SetPaused(true);selected=index;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;centerX=(human.x+zombie.x)/2f+0.5f;centerY=(human.y+zombie.y)/2f+0.5f;textureX=-1;
            status="Human #"+human.id+" and zombie #"+zombie.id+" are within two cells and can see one another. Run or step to watch their response.";
        }
        private void FocusNextHumanInNeed(bool infected)
        {
            if(world==null || world.Count==0)return;int start=selected>=0 && selected<world.Count?selected+1:0;
            for(int offset=0;offset<world.Count;offset++)
            {
                int index=(start+offset)%world.Count;ActorState person=world.ActorAt(index);
                if(person.species!=Species.Human || person.health<=0 || (infected?person.turnAtTick==0:person.health>=100))continue;
                RevealObservedEvent();SetPaused(true);selected=index;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;centerX=person.x+0.5f;centerY=person.y+0.5f;textureX=-1;
                status="Selected "+(infected?"infected":"injured")+" human #"+person.id+" ("+person.health+"/100 health).";return;
            }
            status="No "+(infected?"infected":"injured")+" humans remain.";
        }
        private void DrawInfectionMarker(Rect body,ActorState actor,float cell)
        {
            if(!showInfectionMarkers || actor.species!=Species.Human || actor.turnAtTick<=world.Tick || (actor.health==0 && !world.ReanimationEnabled))return;
            bool corpse=actor.health==0;GUI.color=corpse?new Color(0.85f,0.3f,0.65f):new Color(1f,0.55f,0.1f);
            float size=cell*0.14f,x=body.x+body.width-size-cell*0.04f,y=body.y+cell*0.04f;
            GUI.DrawTexture(new Rect(x,y,size,size),Texture2D.whiteTexture);
            if(corpse)GUI.DrawTexture(new Rect(x-size-cell*0.04f,y,size,size),Texture2D.whiteTexture);
        }
        private int SoonestTurnIndex()
        {
            if(world==null || !world.Encounter)return -1;int best=-1;long deadline=long.MaxValue;
            for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.species!=Species.Human || actor.turnAtTick<=world.Tick || (actor.health==0 && !world.ReanimationEnabled))continue;if(best<0 || actor.turnAtTick<deadline){best=i;deadline=actor.turnAtTick;}}
            return best; // Stable actor order breaks equal-deadline ties without touching simulation state.
        }
        private void FocusSoonestTurn()
        {
            int index=SoonestTurnIndex();if(index<0){status="No pending turns remain.";return;}ActorState actor=world.ActorAt(index);
            RevealObservedEvent();SetPaused(true);selected=index;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;centerX=actor.x+0.5f;centerY=actor.y+0.5f;textureX=-1;
            status="Soonest turn: "+(actor.health==0?"corpse":"infected human")+" #"+actor.id+" in "+((actor.turnAtTick-world.Tick)/(double)Rules.TicksPerSecond).ToString("0.0")+"s. Run or step to observe.";
        }
        private void StartTurnWatch()
        {
            int index=SoonestTurnIndex();if(index<0){status="No pending turns remain.";return;}
            FocusSoonestTurn();if(world.ActorAt(index).turnAtTick-world.Tick<=1){status="Next turn is one tick away. Step 1 tick to observe it.";return;}
            watchNextTurn=true;SetPaused(false);status="Watching the next pending turn. Playback will pause one tick before it.";
        }
        private void CheckTurnWatch()
        {
            if(!watchNextTurn || clock.Paused)return;int index=SoonestTurnIndex();
            if(index<0){watchNextTurn=false;status="Turn watch ended: no pending turns remain.";return;}
            if(world.ActorAt(index).turnAtTick-world.Tick<=1){FocusSoonestTurn();status="Turn watch paused one tick before the next turn. Step 1 tick to observe it.";}
        }
        private void SetStorageReserve(int boxKey,int target)
        {
            SetPaused(true);status=world.TrySetStorageFoodReserve((boxKey-1)%world.Width,(boxKey-1)/world.Width,target)?(target==0?"Deliberate restocking disabled for this box. Food transfers remain available.":"Storage reserve set to "+target+" meals."):"Choose a reserve from zero to the box capacity.";
        }
        private void DrawStorageReserveControls(StorageBoxDocument box)
        {
            int key=box.y*world.Width+box.x+1,current=world.EffectiveFoodReserve(box);
            if(world.DeliveryControlsEnabled)
            {
                GUILayout.Label("Outgoing donations: "+(box.blockDonations?"blocked":"allowed"));
                if(GUILayout.Button(box.blockDonations?"Allow donations":"Block donations"))
                {
                    World editedWorld=world;bool allowed=box.blockDonations;
                    Queue(()=>{if(world!=editedWorld)return;SetPaused(true);world.TrySetStorageDonations((key-1)%world.Width,(key-1)/world.Width,allowed);status=allowed?"Outgoing donations allowed.":"New donor pickups blocked. Collected food stays with couriers.";});
                }
                GUILayout.Label("Local meals, deposits and incoming deliveries remain available.");
            }
            if(world.HouseholdMealsPerResident(world.HouseAt(box.x,box.y))>0)
            {
                GUILayout.Label(box.foodReserve.HasValue?"Manual box reserve override":"Automatic share of household food target");
                if(box.foodReserve.HasValue && GUILayout.Button("Use household target"))Queue(()=>{SetPaused(true);world.TryUseHouseholdFoodTarget(box.x,box.y);});
            }
            if(reserveInputBox!=key || reserveInputValue!=current){reserveInputBox=key;reserveInputValue=current;storageReserveInput=current.ToString(CultureInfo.InvariantCulture);}
            GUILayout.Label("Food reserve target (0–"+box.capacity+")");storageReserveInput=GUILayout.TextField(storageReserveInput,4);
            int target;bool valid=int.TryParse(storageReserveInput,NumberStyles.None,CultureInfo.InvariantCulture,out target) && target>=0 && target<=box.capacity;
            bool enabled=GUI.enabled;GUI.enabled=enabled && valid;
            if(GUILayout.Button("Apply reserve")){int value=target;World editedWorld=world;Queue(()=>{if(world==editedWorld)SetStorageReserve(key,value);});}GUI.enabled=enabled;
            if(!valid)GUILayout.Label("Enter a whole number from 0 to "+box.capacity+".");
            if(GUILayout.Button("Restocking off"))Queue(()=>SetStorageReserve(key,0));
            if(GUILayout.Button("Default reserve"))Queue(()=>SetStorageReserve(key,Math.Min(RestockRules.ReserveMeals,box.capacity)));
            if(GUILayout.Button("Fill to capacity"))Queue(()=>SetStorageReserve(key,box.capacity));
            GUILayout.Label(current==0?"Restocking off; surplus deposits and meals still allowed.":"Restockers aim for this reserve. Capacity remains "+box.capacity+" items.");
        }
        private void DrawCourierAssignmentControls()
        {
            if(!world.CourierAssignmentsEnabled)return;
            if(selected>=0 && selected<world.Count)
            {
                ActorState person=world.ActorAt(selected);if(person.species!=Species.Human || person.health<=0)return;
                GUILayout.Label("Assigned house: "+(person.assignedHouseId==0?"none":"#"+person.assignedHouseId));
                if(GUILayout.Button("Assign current house")){int index=selected;Queue(()=>{SetPaused(true);ActorState actor=world.ActorAt(index);status=world.TryAssignHouse(index,actor.x,actor.y)?"Home house assigned.":"Stand inside a house and finish the current delivery before assigning home.";});}
                if(GUILayout.Button("Edit courier assignment")){int index=selected;Queue(()=>{SetPaused(true);courierEditor=index;status="Select a storage box to assign its house, set the courier source, or add a destination.";});}
                if(person.courierSourceBox!=0)GUILayout.Label("Assigned source: "+((person.courierSourceBox-1)%world.Width)+", "+((person.courierSourceBox-1)/world.Width));
                if(person.courierBoxes!=null)for(int n=0;n<person.courierBoxes.Length;n++){int key=person.courierBoxes[n]-1;GUILayout.Label("Destination "+(n+1)+": "+(key%world.Width)+", "+(key/world.Width));}
                DrawCourierScheduleControls(selected,person);
                if(person.courierReturningHome)GUILayout.Label("Courier returning to assigned house.");
                if(person.courierSourceBox!=0 && GUILayout.Button("Clear courier route")){int index=selected;Queue(()=>{SetPaused(true);status=world.TryClearCourierRoute(index)?"Courier route cleared; home house retained.":"Finish the current delivery before editing the route.";});}
            }
            if(courierEditor<0 || courierEditor>=world.Count)return;
            ActorState editor=world.ActorAt(courierEditor);if(editor.species!=Species.Human || editor.health<=0){courierEditor=-1;return;}
            GUILayout.Label("Editing courier #"+editor.id+": select a box.");
            if(selectedStorageBox>0)
            {
                int index=courierEditor,x=(selectedStorageBox-1)%world.Width,y=(selectedStorageBox-1)/world.Width;
                if(GUILayout.Button("Assign this box's house"))Queue(()=>{SetPaused(true);status=world.TryAssignHouse(index,x,y)?"Assigned home house for courier.":"Choose an indoor box and finish the current delivery first.";});
                if(GUILayout.Button("Use as courier source"))Queue(()=>{SetPaused(true);status=world.TrySetCourierSource(index,x,y)?"Courier source assigned.":"Assign a house first and finish active work. Source must be indoors and separate from destinations. Unassign distribution-center membership before choosing a different source.";});
                int key=y*world.Width+x+1;bool assigned=editor.courierBoxes!=null && Array.IndexOf(editor.courierBoxes,key)>=0;
                if(GUILayout.Button(assigned?"Remove courier destination":"Add courier destination"))Queue(()=>{SetPaused(true);status=world.TryToggleCourierDestination(index,x,y)?"Courier destinations updated.":"Set a home and source first. Choose another indoor box (maximum eight); finish the current delivery first.";});
            }
            if(GUILayout.Button("Finish courier editing"))Queue(()=>{selected=courierEditor;selectedStorageBox=0;selectedPhone=0;courierEditor=-1;});
        }
        private void DrawDistributionControls()
        {
            if(!world.DistributionCentersEnabled)return;
            if(GUILayout.Button(showCenters?"Hide distribution centers":"Distribution centers"))Queue(()=>{SetPaused(true);showCenters=!showCenters;});
            if(showCenters)
            {
                GUILayout.Label("DISTRIBUTION CENTERS");DistributionCenterOverview[] centers=world.DistributionCenters();if(centers.Length==0)GUILayout.Label("Select an indoor storage box to designate a center's stock.");
                foreach(DistributionCenterOverview center in centers)
                {
                    GUILayout.Label("Center #"+center.CenterId+" | Couriers: "+center.Couriers+" | Active trips: "+center.ActiveTrips+"\nStock: "+center.StockMeals+"/"+center.Capacity+" meals | Available for delivery: "+center.AvailableMeals);
                    int centerBox=center.SupplyBox,centerId=center.CenterId;if(GUILayout.Button("Observe center #"+centerId))Queue(()=>FocusSupplyBox(centerBox));
                    if(center.StockMeals==0)GUILayout.Label("Center #"+center.CenterId+": stock empty.");
                }
            }
        }
        private void DrawSelectedDistributionActions()
        {
            if(!world.DistributionCentersEnabled)return;
            if(selected>=0 && selected<world.Count)
            {
                ActorState actor=world.ActorAt(selected);if(actor.species==Species.Human && actor.health>0)
                {
                    GUILayout.Label(actor.distributionCenterId==0?"Distribution center: none":"Distribution center: #"+actor.distributionCenterId);
                    if(GUILayout.Button("Assign courier to distribution center")){int index=selected;Queue(()=>{SetPaused(true);centerCourier=index;status="Select a center's stock box, then assign this courier. Their home and delivery destinations stay assigned.";});}
                    if(actor.distributionCenterId!=0 && GUILayout.Button("Unassign distribution center")){int index=selected;Queue(()=>{SetPaused(true);status=world.TryUnassignDistributionCourier(index)?"Center membership removed; independent courier route retained.":"Finish the active supply task before changing center membership.";});}
                }
            }
            if(selectedStorageBox>0)
            {
                int key=selectedStorageBox-1,x=key%world.Width,y=key/world.Width;
                if(GUILayout.Button("Use as distribution stock box"))Queue(()=>{SetPaused(true);status=world.TryConfigureDistributionCenter(x,y)?"Distribution center configured with this finite stock box.":"Choose an indoor box separate from store back stock. Unassign the center's couriers before replacing its stock box.";});
                if(centerCourier>=0 && centerCourier<world.Count)
                {
                    int index=centerCourier;GUILayout.Label("Assigning courier #"+world.ActorAt(index).id);
                    if(GUILayout.Button("Assign courier to this center"))Queue(()=>{SetPaused(true);status=world.TryAssignDistributionCourier(index,world.HouseAt(x,y))?"Courier assigned to the center's stock source; destinations and shift retained.":"Configure a center, assign this human a home, clear any clerk role, and finish active work first. Center stock cannot also be a destination.";});
                }
            }
        }
        private void DrawStoreControls()
        {
            if(!world.StoresEnabled)return;
            if(GUILayout.Button(showStores?"Hide store management":"Store management"))Queue(()=>{SetPaused(true);showStores=!showStores;});
            if(showStores)
            {
                GUILayout.Label("STORES");StoreOverview[] stores=world.StoreSupplies();if(stores.Length==0)GUILayout.Label("Select an indoor storage box and use it as a store supply crate.");
                SupplyOrderDocument[] pending=world.PendingSupplyOrders();Array.Sort(pending,world.CompareSupplyOrderPriority);GUILayout.Label(world.ShortagePrioritiesEnabled ? "Dispatch: least supplied share first; oldest request breaks ties. Active trips continue." : "Dispatch: oldest unclaimed request first.");GUILayout.Label("Pending orders: "+pending.Length+" | Completed history: "+world.CompletedSupplyOrders().Length);
                if(world.SupplyRequestsEnabled)foreach(SupplyOrderDocument order in pending)DrawSupplyOrderRow(order);
                foreach(SupplyOrderDocument order in world.CompletedSupplyOrders())GUILayout.Label("Completed order #"+order.id+" | Store #"+order.storeId+" | Delivered: "+order.deliveredMeals+" food");
                foreach(StoreOverview store in stores)
                {
                    GUILayout.Label("Store #"+store.StoreId+" | Clerks: "+store.Clerks+"\nShelf meals: "+store.ShelfMeals+" | Empty shelves: "+store.EmptyShelves+"/"+store.Shelves+"\nBack-stock meals: "+store.CrateMeals+" | Shelf target: "+store.ShelfTarget);
                    int storeBox=store.SupplyBox,storeId=store.StoreId;if(GUILayout.Button("Observe store #"+storeId))Queue(()=>FocusSupplyBox(storeBox));
                    if(store.CrateMeals==0)GUILayout.Label("Store #"+store.StoreId+": crate empty — needs supplies.");
                }
            }
        }
        private void FocusSupplyBox(int key)
        {
            SetPaused(true);selected=-1;selectedShelf=0;selectedPhone=0;selectedBatCell=-1;selectedStorageBox=key;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;
            centerX=(key-1)%world.Width+0.5f;centerY=(key-1)/world.Width+0.5f;span=Math.Min(span,16);textureX=-1;sections["Selected item"]=true;inspectorScroll=new Vector2();
        }
        private void DrawSupplyOrderRow(SupplyOrderDocument order)
        {
            int carrier=world.SupplyOrderCourier(order.id);
            GUILayout.Label("Order #"+order.id+" | Store #"+order.storeId+"\nDelivered: "+order.deliveredMeals+"/"+order.requestedMeals+" food | Remaining: "+(order.requestedMeals-order.deliveredMeals)+"\n"+(carrier==0?"Awaiting courier":"Assigned courier #"+carrier)+" | Caller #"+order.requestedBy);
            if(carrier!=0 && GUILayout.Button("Observe courier #"+carrier))Queue(()=>{for(int i=0;i<world.Count;i++)if(world.ActorAt(i).id==carrier){SelectHouseholdResident(i);break;}});
            int phone=order.phone,id=order.id;
            if(GUILayout.Button("Observe order #"+id))Queue(()=>{SetPaused(true);selected=-1;selectedShelf=0;selectedStorageBox=0;selectedBatCell=-1;selectedPhone=phone;centerX=(phone-1)%world.Width+0.5f;centerY=(phone-1)/world.Width+0.5f;textureX=-1;sections["Selected item"]=true;inspectorScroll=new Vector2();});
        }
        private void DrawSelectedStoreActions()
        {
            if(!world.StoresEnabled)return;
            if(selectedStorageBox>0)
            {
                int key=selectedStorageBox-1,x=key%world.Width,y=key/world.Width;
                if(GUILayout.Button("Use as store supply crate"))Queue(()=>{SetPaused(true);status=world.TryConfigureStore(x,y)?"Store configured. Its room's shelves are served from this back-stock crate. Select a human in the store to assign a clerk.":"Store supply crates must be inside a discovered room.";});
            }
            if(selected>=0 && selected<world.Count)
            {
                ActorState actor=world.ActorAt(selected);if(actor.species!=Species.Human || actor.health<=0)return;int index=selected;
                GUILayout.Label(actor.clerkStoreId==0?"Clerk store: none":"Clerk store: #"+actor.clerkStoreId+(actor.clerkShelf==0?" | idle":" | restocking shelf "+((actor.clerkShelf-1)%world.Width)+", "+((actor.clerkShelf-1)/world.Width)));
                if(actor.clerkNeedsSupplies)GUILayout.Label(actor.clerkPhone==0?"Supply call: needs a reachable phone":"Supply call: "+(actor.clerkCallProgress>0?"calling":"walking to phone"));
                if(GUILayout.Button("Assign clerk to current store"))Queue(()=>{SetPaused(true);ActorState person=world.ActorAt(index);status=world.TryAssignClerk(index,world.HouseAt(person.x,person.y))?"Clerk assigned. Hungry or threatened clerks pause work.":"Configure the store crate, stand inside its room, clear courier assignments and finish active supply work first.";});
                if(actor.clerkStoreId!=0 && GUILayout.Button("Unassign clerk"))Queue(()=>{SetPaused(true);status=world.TryUnassignClerk(index)?"Clerk assignment removed; carried food retained.":"Clerk cannot be unassigned right now.";});
            }
        }
        private void SetCourierSchedule(int index,int start,int hours)
        {
            SetPaused(true);status=world.TrySetCourierSchedule(index,start,hours)?"Courier schedule saved. Active deliveries finish before rest; hunger and danger take priority.":"Choose a living uninfected courier, start hour 0–23, and 1–23 working hours.";
        }
        private void DrawCourierScheduleControls(int index,ActorState person)
        {
            if(!world.CourierSchedulesEnabled || (person.courierBoxes==null && !(world.OrderFulfillmentEnabled && person.distributionCenterId!=0)))return;
            GUILayout.Label(person.courierShiftHours==0?"Courier schedule: always available":"Courier shift: "+person.courierShiftStartHour.ToString("00")+":00–"+((person.courierShiftStartHour+person.courierShiftHours)%24).ToString("00")+":00 ("+person.courierShiftHours+" hours)");
            GUILayout.Label("Courier duty: "+world.CourierScheduleStatus(index));
            if(scheduleActor!=person.id || scheduleStart!=person.courierShiftStartHour || scheduleHours!=person.courierShiftHours)
            {
                scheduleActor=person.id;scheduleStart=person.courierShiftStartHour;scheduleHours=person.courierShiftHours;
                shiftStartInput=(person.courierShiftHours==0?7:person.courierShiftStartHour).ToString(CultureInfo.InvariantCulture);shiftHoursInput=(person.courierShiftHours==0?12:person.courierShiftHours).ToString(CultureInfo.InvariantCulture);
            }
            GUILayout.Label("Shift start hour (0–23)");shiftStartInput=GUILayout.TextField(shiftStartInput,2);
            GUILayout.Label("Working hours (1–23)");shiftHoursInput=GUILayout.TextField(shiftHoursInput,2);
            int start,hours;bool valid=int.TryParse(shiftStartInput,NumberStyles.None,CultureInfo.InvariantCulture,out start) && start>=0 && start<=23;
            valid=int.TryParse(shiftHoursInput,NumberStyles.None,CultureInfo.InvariantCulture,out hours) && hours>=1 && hours<=23 && valid;
            bool enabled=GUI.enabled;GUI.enabled=enabled && valid;if(GUILayout.Button("Apply courier schedule")){int first=start,length=hours;Queue(()=>SetCourierSchedule(index,first,length));}GUI.enabled=enabled;
            if(!valid)GUILayout.Label("Enter a start hour from 0 to 23 and working hours from 1 to 23.");
            if(GUILayout.Button("Day shift 07:00–19:00"))Queue(()=>SetCourierSchedule(index,7,12));
            if(GUILayout.Button("Night shift 19:00–07:00"))Queue(()=>SetCourierSchedule(index,19,12));
            if(GUILayout.Button("Courier always available"))Queue(()=>SetCourierSchedule(index,0,0));
        }
        private void DrawHouseholdFoodControls(int houseId)
        {
            if(!world.HouseholdFoodTargetsEnabled)return;HouseholdFoodSupply food=world.HouseholdFoodSupplies(houseId);
            GUILayout.Label("HOUSEHOLD FOOD TARGET");
            GUILayout.Label(food.MealsPerResident==0?"Manual box targets":"Meals per assigned resident: "+food.MealsPerResident);
            if(food.MealsPerResident>0)
            {
                GUILayout.Label("Assigned residents: "+food.Residents+" | Requested meals: "+food.RequestedMeals+"\nBox reserve total: "+food.BoxTarget+" | Stored meals: "+food.BoxMeals+"\nReserve deficit: "+food.ReserveDeficit+" | Unallocated meals: "+food.UnallocatedMeals);
                if(food.UnallocatedMeals>0)GUILayout.Label("Add storage capacity or release manual box overrides to cover the household target.");
                if(GUILayout.Button("More meals per resident"))Queue(()=>{SetPaused(true);world.TrySetHouseholdMealsPerResident(houseId,Math.Min(HouseholdFoodRules.MaxMealsPerResident,food.MealsPerResident+1));});
                if(GUILayout.Button("Fewer meals per resident"))Queue(()=>{SetPaused(true);world.TrySetHouseholdMealsPerResident(houseId,Math.Max(1,food.MealsPerResident-1));});
                if(GUILayout.Button("Use manual box targets"))Queue(()=>{SetPaused(true);world.TrySetHouseholdMealsPerResident(houseId,0);});
            }
            else if(GUILayout.Button("Use 3 meals per resident"))Queue(()=>{SetPaused(true);world.TrySetHouseholdMealsPerResident(houseId,HouseholdFoodRules.DefaultMealsPerResident);});
            GUILayout.Label("Away residents count; visitors do not. Custom box reserves override their automatic share.");
        }
        private void SelectHouseholdResident(int index)
        {
            if(index<0 || index>=world.Count)return;ActorState person=world.ActorAt(index);
            if(person.species!=Species.Human || person.health<=0)return;
            RevealObservedEvent();SetPaused(true);selected=index;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;
            centerX=person.x+0.5f;centerY=person.y+0.5f;textureX=-1;status="Selected resident #"+person.id+". Choose a house below to change their home.";
        }
        private void NextUnassignedResident()
        {
            int start=selected>=0?selected+1:0;
            for(int n=0;n<world.Count;n++){int index=(start+n)%world.Count;ActorState actor=world.ActorAt(index);if(actor.species==Species.Human && actor.health>0 && actor.assignedHouseId==0){SelectHouseholdResident(index);return;}}
            status="No unassigned living humans.";
        }
        private void DrawHouseholds()
        {
            if(!world.CourierAssignmentsEnabled)return;
            if(GUILayout.Button(showHouseholds?"Hide household management":"Household management"))Queue(()=>{SetPaused(true);showHouseholds=!showHouseholds;});
            if(!showHouseholds)return;
            HouseholdOverview[] houses=world.Households();int unassigned=0;
            for(int i=0;i<world.Count;i++){ActorState actor=world.ActorAt(i);if(actor.species==Species.Human && actor.health>0 && actor.assignedHouseId==0)unassigned++;}
            GUILayout.Label("HOUSEHOLDS");GUILayout.Label("Unassigned residents: "+unassigned);
            if(GUILayout.Button("Next unassigned resident"))Queue(NextUnassignedResident);
            if(houses.Length==0){GUILayout.Label("No discovered houses.");return;}
            if(householdHouse==0)householdHouse=houses[0].HouseId;
            bool hasSelected=selected>=0 && selected<world.Count && world.ActorAt(selected).species==Species.Human && world.ActorAt(selected).health>0;
            if(hasSelected)GUILayout.Label("Selected resident: #"+world.ActorAt(selected).id+". Home changes keep courier routes.");
            else GUILayout.Label("Select a resident or use Next unassigned resident to assign a home.");
            foreach(HouseholdOverview house in houses)
            {
                GUILayout.Label("House #"+house.HouseId+": assigned "+house.Residents.Length+" | home "+house.AtHome+" | away "+house.Away);
                GUILayout.Label("Visitors: "+house.Visitors+" | Boxes: "+house.Boxes+" | Shelter capacity: "+house.Capacity);
                int houseId=house.HouseId,x=house.FocusX,y=house.FocusY;
                if(GUILayout.Button("Manage house #"+houseId))Queue(()=>{SetPaused(true);householdHouse=houseId;centerX=x+0.5f;centerY=y+0.5f;textureX=-1;});
                if(hasSelected)
                {
                    int index=selected;
                    if(GUILayout.Button("Assign selected resident to house #"+houseId))Queue(()=>{SetPaused(true);status=world.TryAssignHouse(index,x,y)?"Resident assigned to house #"+houseId+". They travel normally when safe; no inventory moved.":"Finish the current delivery or restocking task first. Infected residents cannot be reassigned.";});
                }
            }
            if(hasSelected && GUILayout.Button("Unassign selected resident"))
            {
                int index=selected;Queue(()=>{SetPaused(true);status=world.TryUnassignHouse(index)?"Home assignment removed.":"Clear the courier route and finish any active task first. Infected residents cannot be reassigned.";});
            }
            foreach(HouseholdOverview house in houses)if(house.HouseId==householdHouse)
            {
                DrawHouseholdFoodControls(house.HouseId);
                GUILayout.Label("RESIDENTS OF HOUSE #"+house.HouseId);
                if(house.Residents.Length==0)GUILayout.Label("No assigned residents.");
                foreach(HouseholdResident resident in house.Residents)
                {
                    GUILayout.Label("Resident #"+resident.Id+": "+(resident.AtHome?"at home":resident.CurrentHouseId==0?"away outdoors":"visiting house #"+resident.CurrentHouseId)+(resident.Courier?" | courier":"")+(resident.Busy?" | active supply task":"")+(resident.Infected?" | infected":"")+(resident.Courier && world.CourierSchedulesEnabled?" | "+resident.CourierStatus:""));
                    int index=resident.ActorIndex;if(GUILayout.Button("Select resident #"+resident.Id))Queue(()=>SelectHouseholdResident(index));
                }
            }
            GUILayout.Label("Assigned residents and visitors are separate. Shelter capacity is admission guidance, not an assignment limit.");
        }
        private void FocusShelterSupply(int roomId)
        {
            foreach(ShelterSupplyOverview supply in world.ShelterSupplies())if(supply.RoomId==roomId)
            {
                SetPaused(true);focusedSupplyRoom=roomId;selected=-1;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;centerX=supply.FocusX+0.5f;centerY=supply.FocusY+0.5f;span=Math.Min(span,16);textureX=-1;status="Supply view: room #"+roomId+" | "+supply.Status+". "+supply.Meals+" meals for "+supply.Occupants+" occupants; "+supply.RestockingTrips+" restocking trips.";return;
            }
        }
        private void FocusNextShelterShortage()
        {
            ShelterSupplyOverview[] supplies=world.ShelterSupplies();int first=0;
            foreach(ShelterSupplyOverview supply in supplies)if(supply.NeedsAttention){if(first==0)first=supply.RoomId;if(supply.RoomId>focusedSupplyRoom){FocusShelterSupply(supply.RoomId);return;}}
            if(first!=0)FocusShelterSupply(first);else status="No occupied shelter shortages.";
        }
        private void DrawShelterSupplies()
        {
            ShelterSupplyOverview[] supplies=world.ShelterSupplies();int shortages=0;foreach(ShelterSupplyOverview supply in supplies)if(supply.NeedsAttention)shortages++;
            GUILayout.Label("SHELTER SUPPLIES");GUILayout.Label("Shelters needing supplies: "+shortages+"/"+supplies.Length);
            bool enabled=GUI.enabled;GUI.enabled=enabled && shortages>0;if(GUILayout.Button("Next shelter shortage"))Queue(FocusNextShelterShortage);GUI.enabled=enabled;
            if(supplies.Length==0){GUILayout.Label("No shelter rooms available.");return;}
            if(GUILayout.Button(showAllShelterSupplies?"Show shortages only":"Show all shelters"))Queue(()=>showAllShelterSupplies=!showAllShelterSupplies);
            if(shortages==0)GUILayout.Label("No occupied shelter shortages.");
            GUILayout.Label("Player totals; humans use their own observations.");
            foreach(ShelterSupplyOverview supply in supplies)
            {
                if(!showAllShelterSupplies && !supply.NeedsAttention && supply.RoomId!=focusedSupplyRoom)continue;
                GUILayout.Label("Room #"+supply.RoomId+": "+supply.Status+(supply.RoomId==focusedSupplyRoom?" (focused)":""));
                GUILayout.Label("Occupants: "+supply.Occupants+" | Hungry: "+supply.Hungry+"\nVery hungry: "+supply.VeryHungry+"\nFood: "+supply.Meals+" meals\nCarried: "+supply.CarriedMeals+" | Boxes: "+supply.BoxMeals+"\nShelves: "+supply.ShelfMeals+"\nReserve deficit: "+supply.ReserveDeficit+" meals\nRestocking trips: "+supply.RestockingTrips+"\nDeliveries in: "+supply.IncomingDeliveries+" | out: "+supply.OutgoingDeliveries);
                if(supply.UnallocatedMeals>0)GUILayout.Label("Unallocated household meals: "+supply.UnallocatedMeals);
                int roomId=supply.RoomId;if(GUILayout.Button("Focus shelter #"+roomId))Queue(()=>FocusShelterSupply(roomId));
            }
            if(world.ShelterScanLimited)GUILayout.Label("Some room geometry exceeds shelter scan limits.");
        }
        private void FocusNextStorageBox()
        {
            StorageBoxDocument[] boxes=world.StorageBoxSites();if(boxes.Length==0)return;StorageBoxDocument next=boxes[0];foreach(StorageBoxDocument box in boxes)if(box.y*world.Width+box.x+1>selectedStorageBox){next=box;break;}
            SetPaused(true);selected=-1;selectedShelf=0;selectedBatCell=-1;selectedStorageBox=next.y*world.Width+next.x+1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;centerX=next.x+0.5f;centerY=next.y+0.5f;textureX=-1;status="Selected storage box at "+next.x+", "+next.y+". Humans transfer actual items when safe.";
        }
        private void FocusNextHungryHuman()
        {
            if(world==null || !world.HungerEnabled || world.Count==0)return;
            int start=selected>=0 && selected<world.Count?selected+1:0;
            for(int offset=0;offset<world.Count;offset++)
            {
                int index=(start+offset)%world.Count;ActorState person=world.ActorAt(index);
                if(person.species!=Species.Human || person.hunger<HungerRules.SeekAt)continue;
                RevealObservedEvent();SetPaused(true);selected=index;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;selectedBatCell=-1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;centerX=person.x+0.5f;centerY=person.y+0.5f;textureX=-1;
                status="Selected hungry human #"+person.id+" ("+(100-person.hunger)+"/100 food remaining). Click Next hungry human to cycle.";return;
            }
            status="No hungry humans remain.";
        }
        private void FocusNextGroundBat()
        {
            WeaponPickupDocument[] sites=world.WeaponSites();if(sites.Length==0)return;WeaponPickupDocument chosen=sites[0];foreach(WeaponPickupDocument site in sites)if(site.y*world.Width+site.x>focusedBatCell){chosen=site;break;}
            SetPaused(true);selected=-1;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;focusedBatCell=chosen.y*world.Width+chosen.x;selectedBatCell=focusedBatCell;centerX=chosen.x+0.5f;centerY=chosen.y+0.5f;textureX=-1;status="Ground bat at "+chosen.x+", "+chosen.y+" | Bats in pile: "+chosen.Quantity+(world.WeaponDurability?" | Best condition: "+(MeleeRules.BatHits-(chosen.wear==null?0:chosen.wear[0]))+"/"+MeleeRules.BatHits:"")+". Humans still need to see it and collect it when safe.";
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
            SetPaused(true);selected=-1;selectedBatCell=-1;selectedStorageBox=0;selectedPhone=0;selectedShelf=next.y*world.Width+next.x+1;placing=null;shelfBrush=-1;restBrush=false;boxBrush=false;phoneBrush=false;
            centerX=next.x+0.5f;centerY=next.y+0.5f;textureX=-1;
            status="Selected shelf at "+next.x+", "+next.y+" ("+next.food+" meals). Click Next stocked shelf to cycle.";
        }
        private bool SelectedTarget(out int x,out int y,out Color color,out string label)
        {
            x=0;y=0;color=Color.white;label="";if(world==null || selected<0 || selected>=world.Count)return false;
            ActorState actor=world.ActorAt(selected);
            if(actor.species==Species.Human)
            {
                if(actor.workerCommuteId!=0 && actor.turnAtTick==0){x=actor.shelterId!=0?actor.shelterX:actor.x;y=actor.shelterId!=0?actor.shelterY:actor.y;if(actor.shelterId==0)foreach(HouseholdOverview house in world.Households())if(house.HouseId==actor.workerCommuteId){x=house.FocusX;y=house.FocusY;break;}color=new Color(0.65f,0.75f,0.95f);label=actor.workerCommuteId==actor.assignedHouseId?"Return home":"Commute to work";}
                else if(actor.shoppingMeals>0 && actor.turnAtTick==0){int key=actor.restockPhase==RestockPhase.Returning?actor.restockBox:actor.shoppingShelf;if(key>0){x=(key-1)%world.Width;y=(key-1)/world.Width;}else{x=actor.shelterId!=0?actor.shelterX:actor.x;y=actor.shelterId!=0?actor.shelterY:actor.y;}color=new Color(0.18f,0.82f,0.75f);label=actor.restockPhase==RestockPhase.Returning?"Home food deposit":actor.shoppingShelf!=0?"Shopping shelf":actor.shoppingStoreId!=0?"Store investigation":"Looking for stores";}
                else if(world.SupplyRequestsEnabled && actor.turnAtTick==0 && actor.clerkPhone>0){x=(actor.clerkPhone-1)%world.Width;y=(actor.clerkPhone-1)/world.Width;color=new Color(0.18f,0.7f,0.75f);label="Supply phone";}
                else if(world.HungerEnabled && actor.turnAtTick==0 && actor.foodUntilTick>world.Tick){x=actor.foodX;y=actor.foodY;color=new Color(0.18f,0.82f,0.75f);label="Food target";}
                else if(world.FoodExploration && actor.turnAtTick==0 && actor.foodExploreRoomId!=0){x=actor.shelterX;y=actor.shelterY;color=new Color(0.7f,0.45f,1f);label="Room investigation";}
                else if(world.SheltersEnabled && actor.shelterState==ShelterState.Seeking){x=actor.shelterX;y=actor.shelterY;color=new Color(0.25f,0.55f,1f);label="Shelter target";}
                else return false;
            }
            else if(world.PursuitMemory && actor.targetId!=0){x=actor.lastSeenX;y=actor.lastSeenY;color=new Color(1f,0.3f,0.35f);label="Observed human";}
            else
            {
                color=new Color(1f,0.72f,0.18f);
                if(world.SearchNoise && actor.heardSearchUntilTick>world.Tick && actor.heardSearchUntilTick>=actor.heardWarningUntilTick && actor.heardSearchUntilTick>=actor.heardDoorUntilTick){x=actor.heardSearchX;y=actor.heardSearchY;label=actor.heardSearchKind==3?"Heard running footsteps":actor.heardSearchKind==2?"Heard combat":actor.heardSearchKind==1?"Heard door movement":"Heard shelf search";}
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
            GUILayout.Label("OUTBREAK SIMULATOR / " + BuildLabel + (HostPaused ? " / UNITY EDITOR PAUSED" : "") + " | Tick " + world.Tick + " | " + TimeSpan.FromSeconds(CourierScheduleRules.StartHour * 3600 + world.GameSeconds).ToString() + (clock.DebtSeconds>=1 ? " | Catching up" : "")); // Distinguish the current source build and an editor error pause from simulation pause.
            GUILayout.BeginHorizontal(); // Group the genuinely parallel clock controls.
            bool wantsRun = clock.Paused || HostPaused; GUI.enabled = !confirmReset && world.Count > 0; if (GUILayout.Button(new GUIContent(wantsRun ? "Run" : "Pause", world.Count == 0 ? "Place a human or zombie before running the movement test." : "Run resumes an editor pause too. Pause stops the simulation and its interpolation."))) Queue(() => SetPaused(!wantsRun)); // Offer a genuine resume action when an editor error pause has stopped Update.
            GUI.enabled = clock.Paused && !confirmReset && world.Count > 0; foreach (int ticks in new[] { 1, 10, 50, 100 }) if (GUILayout.Button(new GUIContent("Step " + ticks + (ticks == 1 ? " tick" : " ticks"), "Advance exactly " + ticks + " ticks and remain paused."))) Queue(() => clock.StepTicks(ticks, StepWorld)); GUI.enabled = !confirmReset; // Advance all systems through the ordinary tick entry point after dispatch.
            foreach (double speed in new[] { 0.5, 1.0, 2.0, 3.0, 5.0, 10.0, 20.0, 50.0 }) if (GUILayout.Button(new GUIContent(speed + "x" + (clock.Speed == speed ? " *" : ""), "Change real-time playback speed; simulation ticks stay at 0.20 seconds."))) Queue(() => clock.SetSpeed(speed)); // Expose the agreed speeds without mutating state during GUI traversal.
            GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); // Separate file controls from clock controls.
            if (GUILayout.Button(new GUIContent("Load Map", "Open the system file browser for a foundation .obmap2 map."))) Queue(() => Browse(false, false)); // Keep native map selection outside the GUI event loop.
            if (GUILayout.Button(new GUIContent("Save Map", "Save terrain and preserved authoring data as .obmap2."))) Queue(() => Browse(true, false)); // Keep starting maps separate from running saves.
            if (GUILayout.Button(new GUIContent("Load Run", "Open a saved simulation and restore its exact random state."))) Queue(() => Browse(false, true)); // Restore the candidate outside the active control traversal.
            if (GUILayout.Button(new GUIContent("Save Run", "Save this tick, population, terrain and random stream as .obsave2."))) Queue(() => Browse(true, true)); // Persist the exact first-slice simulation without nesting a native dialog in OnGUI.
            if (world.HumanReactions && !world.PedestrianRoutesEnabled && GUILayout.Button(world.LongRoutesEnabled ? "Enable pedestrian street routes" : world.WorkerShiftsEnabled ? "Enable longer routes" : world.StoreDiscoveryEnabled ? "Enable worker shifts and commutes" : world.StoreShoppingEnabled ? "Enable store discovery" : world.HomeSuppliesEnabled ? "Enable store shopping" : world.ShortagePrioritiesEnabled ? "Enable home supply checks" : world.SurvivalDamageEnabled ? "Enable shortage priorities" : world.OrderFulfillmentEnabled ? "Enable hunger and exhaustion damage" : world.SupplyRequestsEnabled ? "Enable courier fulfillment" : world.DistributionCentersEnabled ? "Enable phone supply requests" : world.StoresEnabled ? "Enable distribution centers" : world.CourierSchedulesEnabled ? "Enable store clerks" : world.HouseholdFoodTargetsEnabled ? "Enable courier schedules" : world.CourierAssignmentsEnabled ? "Enable household food targets" : world.DeliveryControlsEnabled ? "Enable courier assignments" : world.SupplyDeliveriesEnabled ? "Enable delivery controls" : world.BoxReportsEnabled ? "Enable supply deliveries" : world.BoxMemoryEnabled ? "Enable box reports" : world.ReserveTargetsEnabled ? "Enable box memory" : world.RestockingEnabled ? "Enable reserve targets" : world.StorageEnabled ? "Enable restocking trips" : world.RiseAwareness ? "Enable storage boxes" : world.RiseEnabled ? "Enable reanimation awareness" : world.ReanimationEnabled ? "Enable reanimation transition" : world.MaulingEnabled ? "Enable corpse reanimation" : world.CorneredShoveEnabled ? "Enable grapples and mauling" : world.SprintNoiseEnabled ? "Enable cornered shoves" : world.SprintEnabled ? "Enable sprint footsteps" : world.EnergyReturnEnabled ? "Enable escape sprints" : world.EnergyRiskEnabled ? "Enable low-energy returns" : world.FoodRestEnabled ? "Enable energy-aware food risk" : world.RestSpotsEnabled ? "Enable food trip rest" : world.EnergyEnabled ? "Enable beds" : world.ShoveFatigueEnabled ? "Enable energy and rest" : world.UnarmedSpacing ? "Enable shove fatigue" : world.CombatNoise ? "Enable unarmed spacing" : world.RecoveryRations ? "Enable combat noise" : world.RestHealing ? "Enable recovery food reserves" : world.FirstWeaponPriority ? "Enable shelter healing" : world.UnarmedCourtesy ? "Enable first weapon priority" : world.SupplyCourtesy ? "Enable first bat courtesy" : world.ReplacementTrips ? "Enable supply courtesy" : world.WeaponReplacement ? "Enable replacement trips" : world.CombatFootwork ? "Enable weapon replacement" : world.WeaponDurability ? "Enable combat footwork" : world.BiteEvasion ? "Enable bat durability" : world.ShelterSpacing ? "Enable bite evasion" : world.CombatExperience ? "Enable drops and shelter spacing" : world.WeaponSeeking ? "Enable combat experience" : world.MeleeCombat ? "Enable weapon seeking" : world.FoodExploration ? "Enable melee combat" : world.FoodYieldChoice ? "Enable food exploration" : world.FoodRetreatFallback ? "Enable food yield choices" : world.FoodTripRetreat ? "Enable alternate shelter returns" : world.SafeFoodTargets ? "Enable food trip retreat" : world.FairFoodPickup ? "Enable guarded food routes" : world.FoodRouteChoice ? "Enable considerate pickup" : world.DoorMovementNoise ? "Enable food route choices" : world.QuickStockPickup ? "Enable door movement noise" : world.FoodRouteMemory ? "Enable quick food pickup" : world.SearchCoordination ? "Enable food route memory" : world.CarefulSearch ? "Enable search coordination" : world.SearchNoise ? "Enable cautious searching" : world.FoodDelivery ? "Enable search noise" : world.PantryTrips ? "Enable food delivery" : world.PantryStorage ? "Enable pantry trips" : world.ShelfReports ? "Enable pantry storage" : world.ShelfMemoryEnabled ? "Enable shelf reports" : world.FoodSharing ? "Enable shelf memory" : world.HungerEnabled ? "Enable food sharing" : world.DoorRepair ? "Enable hunger and inventory" : world.WarningSheltering ? "Enable door repair" : world.DoorBracing ? "Enable warning sheltering" : world.ZombieWarningInvestigation ? "Enable door bracing" : world.HumanWarnings ? "Enable warning attraction" : world.ZombieSoundInvestigation ? "Enable human warnings" : world.DoorHearing ? "Enable zombie sound" : world.DoorBreaking ? "Enable door hearing" : world.ZombieWandering ? "Enable door breaking" : world.ShelterRecovery ? "Enable zombie wandering" : world.DoorPursuit ? "Enable shelter recovery" : world.SheltersEnabled ? "Update doorway pursuit" : world.InfectedEscape ? "Enable shelter behavior" : world.CautiousMovement ? "Update escape behavior" : "Use cautious behavior")) Queue(() => { SetPaused(true); SaveDocument saved = world.Capture(); if(!world.EnergyEnabled)ConvertLegacyEnergy(saved); saved.rulesVersion = EncounterRules.Version;if(!world.ReanimationEnabled)SeedLegacyCorpseTimers(saved); World candidate = World.Restore(saved); world = candidate; status = "Current encounter behavior and longer routes enabled; run remains paused. Original save file is unchanged; save to a new name to preserve it."; }); // Explicitly upgrade an older reaction run without losing actors, clocks or random state.
            if(GUILayout.Button(new GUIContent("Restart scenario","Restore this scenario's starting setup, or the loaded checkpoint, and pause. Current progress is replaced.")))Queue(RestartScenario);
            if (GUILayout.Button("Main Menu")) Queue(() => { SetPaused(true); canResume = true; showSetup = false; showCitySetup = false; menuScroll = new Vector2(); showStart = true; }); // Pause and retain world, camera, selection and speed while showing the menu.
            if (GUILayout.Button("Zoom +")) Queue(() => { span = Math.Max(16, span / 1.25f); textureX = -1; }); if (GUILayout.Button("Zoom -")) Queue(() => { span = Math.Min(256, span * 1.25f); textureX = -1; }); // Keep rendering changes outside the current event traversal.
            GUILayout.EndHorizontal(); GUILayout.Label(status); GUILayout.EndArea(); // Keep implementation scope and errors readable in the shell.
            DrawWorld(new Rect(12, 138, Math.Max(64, Screen.width - 300), Math.Max(64, Screen.height - 184))); // Cull the map through a bounded viewport.
            GUILayout.BeginArea(new Rect(Screen.width - 276, 138, 264, Screen.height - 184), GUI.skin.box); inspectorScroll = GUILayout.BeginScrollView(inspectorScroll); // Keep selection diagnostics outside the map viewport.
            DrawEncounterSidebar();
            GUILayout.EndScrollView(); GUILayout.EndArea(); GUI.Label(new Rect(12, Screen.height - 36, Screen.width - 24, 30), GUI.tooltip); // Display a plain-language explanation for hovered controls.
            GUI.enabled = true; if (confirmReset) // Re-enable only the modal controls before offering a concrete reset confirmation.
            { // Begin the reset confirmation.
                GUILayout.BeginArea(new Rect(Screen.width / 2f - 190, Screen.height / 2f - 65, 380, 130), GUI.skin.box); GUILayout.Label("Replace the current run with the fixed movement test?"); // Identify exactly what the reset changes.
                if (GUILayout.Button("Replace run")) Queue(() => { confirmReset = false; Action next = resetAction ?? ResetTest; resetAction = null; canResume = false; next(); }); if (GUILayout.Button("Keep current run")) Queue(() => { confirmReset = false; resetAction = null; canResume = false; }); GUILayout.EndArea(); // Apply reset only after the deliberate second action and outside control traversal.
            } // Finish the reset confirmation.
        } // Finish the screen pass.
        private void DrawWorld(Rect bounds) // Draw a bounded terrain texture and only bodies inside its camera range.
        { // Begin viewport rendering.
            RefreshBuildingPalette();
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
            foreach(RestSpotDocument spot in world.RestSpotSites()){if(spot.x<left || spot.y<top || spot.x>=left+width || spot.y>=top+height)continue;float bedX=bounds.x+(spot.x-left)*cell,bedY=bounds.y+(spot.y-top)*cell;GUI.color=new Color(0.38f,0.30f,0.23f);GUI.DrawTexture(new Rect(bedX+cell*0.1f,bedY+cell*0.08f,cell*0.8f,cell*0.84f),Texture2D.whiteTexture);GUI.color=new Color(0.28f,0.58f,0.62f);GUI.DrawTexture(new Rect(bedX+cell*0.16f,bedY+cell*0.16f,cell*0.68f,cell*0.68f),Texture2D.whiteTexture);GUI.color=new Color(0.86f,0.87f,0.82f);GUI.DrawTexture(new Rect(bedX+cell*0.22f,bedY+cell*0.19f,cell*0.56f,cell*0.2f),Texture2D.whiteTexture);}
            foreach(StorageBoxDocument box in world.StorageBoxSites())
            {
                if(box.x<left || box.y<top || box.x>=left+width || box.y>=top+height)continue;
                Rect furniture=new Rect(bounds.x+(box.x-left+0.12f)*cell,bounds.y+(box.y-top+0.12f)*cell,cell*0.76f,cell*0.76f);DrawStorageIcon(furniture,world.BuildingUseAt(box.x,box.y),StorageRules.Used(box)>0,selectedStorageBox==box.y*world.Width+box.x+1);
                if(!confirmReset && !placing.HasValue && !restBrush && !boxBrush && !phoneBrush && shelfBrush<0 && input.type==EventType.MouseDown && input.button==0 && furniture.Contains(input.mousePosition)){int key=box.y*world.Width+box.x+1;Queue(()=>{SetPaused(true);selected=-1;selectedShelf=0;selectedBatCell=-1;selectedStorageBox=key;});input.Use();}
            }
            foreach(PhoneBlockDocument phone in world.PhoneBlocks())
            {
                if(phone.x<left || phone.y<top || phone.x>=left+width || phone.y>=top+height)continue;
                Rect block=new Rect(bounds.x+(phone.x-left+0.15f)*cell,bounds.y+(phone.y-top+0.15f)*cell,cell*0.7f,cell*0.7f);DrawPhoneIcon(block,selectedPhone==phone.y*world.Width+phone.x+1);
                if(!confirmReset && !placing.HasValue && !restBrush && !boxBrush && !phoneBrush && shelfBrush<0 && input.type==EventType.MouseDown && input.button==0 && block.Contains(input.mousePosition)){int key=phone.y*world.Width+phone.x+1;Queue(()=>{SetPaused(true);selected=-1;selectedShelf=0;selectedStorageBox=0;selectedBatCell=-1;selectedPhone=key;});input.Use();}
            }
            foreach (ShelfDocument shelf in world.ShelfSites()) { if (shelf.x < left || shelf.y < top || shelf.x >= left + width || shelf.y >= top + height) continue; Rect furniture = new Rect(bounds.x + (shelf.x - left + 0.12f) * cell, bounds.y + (shelf.y - top + 0.12f) * cell, cell * 0.76f, cell * 0.76f); GUI.color = shelf.food == 0 ? new Color(0.4f, 0.4f, 0.36f) : new Color(0.68f, 0.57f, 0.25f); GUI.DrawTexture(furniture, Texture2D.whiteTexture); if (!confirmReset && !placing.HasValue && !restBrush && !boxBrush && !phoneBrush && shelfBrush < 0 && input.type == EventType.MouseDown && input.button == 0 && furniture.Contains(input.mousePosition)) { int key = shelf.y * world.Width + shelf.x + 1; Queue(() => { selectedShelf = key; selectedStorageBox=0;selectedPhone=0;selected = -1; selectedBatCell=-1; }); input.Use(); } } // Gold means stocked and grey means empty for the player; human knowledge remains independent.
            foreach(WeaponPickupDocument pickup in world.WeaponSites())
            {
                if(pickup.x<left || pickup.y<top || pickup.x>=left+width || pickup.y>=top+height)continue;
                Rect bat=new Rect(bounds.x+(pickup.x-left+0.82f)*cell,bounds.y+(pickup.y-top+0.17f)*cell,cell*0.14f,cell*0.66f);
                GUI.color=GroundBatColor(world.WeaponDurability && pickup.wear!=null?pickup.wear[0]:0);GUI.DrawTexture(bat,Texture2D.whiteTexture);
                if(selectedBatCell==pickup.y*world.Width+pickup.x){GUI.color=Color.white;GUI.DrawTexture(new Rect(bat.x-cell*0.05f,bat.y+bat.height+cell*0.03f,cell*0.24f,cell*0.04f),Texture2D.whiteTexture);}
                Rect target=new Rect(bat.x-cell*0.06f,bat.y-cell*0.05f,cell*0.26f,cell*0.76f);
                if(!confirmReset && !placing.HasValue && !restBrush && !boxBrush && !phoneBrush && shelfBrush<0 && input.type==EventType.MouseDown && input.button==0 && target.Contains(input.mousePosition)){int key=pickup.y*world.Width+pickup.x;Queue(()=>{SetPaused(true);selectedBatCell=key;focusedBatCell=key;selected=-1;selectedShelf=0;selectedStorageBox=0;selectedPhone=0;});input.Use();}
            }
            DrawSelectedTarget(bounds,left,top,width,height,cell);
            for (int i = 0; i < world.Count; i++) // Inspect the bounded active population instead of instantiating scene objects.
            { // Begin one visible body.
                ActorState actor = world.ActorAt(i); double time = world.Tick + (clock.Paused?0:clock.Alpha); float x = (float)((clock.Paused ? actor.x : motion[i].X(time)) + MovementPresentation.OffsetX(actor.id)), y = (float)((clock.Paused ? actor.y : motion[i].Y(time)) + MovementPresentation.OffsetY(actor.id)); // Interpolate only between committed simulation positions.
                if(!clock.Paused && actor.species==Species.Human && actor.health>0 && actor.turnAtTick==0 && actor.stress==0 && actor.grappledById==0 && actor.freezeUntilTick<=world.Tick && actor.nextMeleeTick<=world.Tick && actor.nextShoveTick<=world.Tick && actor.clerkShelf==0 && actor.clerkPhone==0 && actor.restockPhase==RestockPhase.None && !world.RestSpotAt(actor.x,actor.y) && world.TerrainAt(actor.x,actor.y)!=Terrain.DoorOpen && world.TerrainAt(actor.x,actor.y)!=Terrain.DoorClosed){x+=(float)motion[i].IdleX(time,actor.id);y+=(float)motion[i].IdleY(time,actor.id);}
                if (x < left - 0.18f || y < top - 0.18f || x >= left + width - 0.18f || y >= top + height - 0.18f) continue; // Cull bodies outside the camera.
                if(!clock.Paused && actor.health>0){if(actor.species==Species.Human){x+=(float)(shoveMotion[i].X(time)+0.5*struggleMotion[i].X(time));y+=(float)(shoveMotion[i].Y(time)+0.5*struggleMotion[i].Y(time));}else {x+=(float)grabMotion[i].X(time);y+=(float)grabMotion[i].Y(time);}}
                Rect body = new Rect(bounds.x + (x - left + 0.18f) * cell, bounds.y + (y - top + 0.18f) * cell, cell * 0.64f, cell * 0.64f); GUI.color = selected == i ? Color.yellow : actor.health==0 ? new Color(0.4f,0.4f,0.4f) : actor.species == Species.Zombie ? new Color(0.23f, 0.29f, 0.14f) : new Color(0.75f, 0.8f, 0.92f); GUI.DrawTexture(body, Texture2D.whiteTexture); // Use visibly distinct temporary debug bodies until artwork bindings return.
                if(world.RiseEnabled && (actor.health==0 || actor.riseUntilTick>world.Tick)){float progress=actor.health==0?0:(float)RiseRules.Progress(actor,time);GUI.color=new Color(Mathf.Lerp(0.4f,0.23f,progress),Mathf.Lerp(0.4f,0.29f,progress),Mathf.Lerp(0.4f,0.14f,progress));float inset=selected==i?cell*0.035f:0;GUI.DrawTexture(new Rect(body.x+inset,body.y+inset,body.width-inset*2,body.height-inset*2),Texture2D.whiteTexture);}
                DrawOccupationOutline(i,actor,body,cell);
                DrawNpcBars(i,body,actor,visible,cell);
                if(time<hitFlashUntil[i]){GUI.color=new Color(1f,0.75f,0.65f);GUI.DrawTexture(new Rect(body.x+cell*0.06f,body.y+cell*0.06f,body.width-cell*0.12f,body.height-cell*0.12f),Texture2D.whiteTexture);}
                BiteAction bite= biteFeedback[i];if(actor.species==Species.Human && bite.targetId==actor.id && time>=bite.tick && time<bite.tick+WeaponPresentation.HitFlashTicks){GUI.color=bite.evaded?new Color(0.45f,0.8f,1f):new Color(1f,0.5f,0.5f);GUI.DrawTexture(new Rect(body.x+cell*0.06f,body.y+cell*0.06f,body.width-cell*0.12f,body.height-cell*0.12f),Texture2D.whiteTexture);}
                if(!clock.Paused && actor.species==Species.Human && shoveMotion[i].Strength(time)>0)DrawShoveHands(shoveMotion[i],x,y,time,bounds,left,top,cell);
                if(!clock.Paused && actor.species==Species.Zombie && actor.health>0 && grabMotion[i].Strength(time)>0)DrawGrappleHands(grabMotion[i],x,y,time,bounds,left,top,cell);
                if(actor.species==Species.Human && (actor.meleeWeapon==MeleeWeapon.Bat || weaponMotion[i].Breaking(time)))DrawBat(weaponMotion[i],x,y,time,bounds,left,top,cell,actor.health>0 && actor.species==Species.Human);
                DrawInfectionMarker(body,actor,cell); // Overlay after contact flashes so infection remains readable.
                if (!confirmReset && !placing.HasValue && !restBrush && !boxBrush && !phoneBrush && shelfBrush < 0 && input.type == EventType.MouseDown && input.button == 0 && body.Contains(input.mousePosition)) { int chosen = i; Queue(() => { selected = chosen; selectedShelf = 0;selectedStorageBox=0;selectedPhone=0; selectedBatCell=-1; }); input.Use(); } // Defer selection so the inspector cannot add layout entries midway through a mouse event.
            } // Finish one visible body.
            GUI.color = Color.white; if (confirmReset || !visible.Contains(input.mousePosition)) return; // Keep modal confirmation and toolbar events out of map input.
            if(phoneBrush && input.type==EventType.MouseDown && input.button==0){int x=left+(int)((input.mousePosition.x-bounds.x)/cell),y=top+(int)((input.mousePosition.y-bounds.y)/cell);Queue(()=>{if(world.TryPlacePhone(x,y)){phoneBrush=false;textureX=-1;status="Phone placed. Clerks in this room can call for back stock.";}else status="Phone needs a free indoor floor cell away from bodies and furniture.";});input.Use();}
            if(boxBrush && input.type==EventType.MouseDown && input.button==0){int x=left+(int)((input.mousePosition.x-bounds.x)/cell),y=top+(int)((input.mousePosition.y-bounds.y)/cell);Queue(()=>{if(world.TryPlaceStorageBox(x,y)){boxBrush=false;phoneBrush=false;textureX=-1;selected=-1;selectedShelf=0;selectedBatCell=-1;selectedStorageBox=y*world.Width+x+1;status="Placed empty storage box: 0/30 items.";}else status="Storage box needs a free non-door cell, away from bodies, furniture and reserved destinations.";});input.Use();}
            if(restBrush && input.type==EventType.MouseDown && input.button==0){int x=left+(int)((input.mousePosition.x-bounds.x)/cell),y=top+(int)((input.mousePosition.y-bounds.y)/cell);bool erase=eraseRestSpot;Queue(()=>{bool changed=erase?world.TryRemoveRestSpot(x,y):world.TryPlaceRestSpot(x,y);if(changed){restBrush=false;boxBrush=false;phoneBrush=false;textureX=-1;status=erase?"Bed removed.":"Bed placed. Tired humans can use it when their shelter is safe.";}else status="Choose a free floor cell inside a shelter, or an existing bed to remove.";});input.Use();}
            if (shelfBrush >= 0 && input.type == EventType.MouseDown && input.button == 0) { int x = left + (int)((input.mousePosition.x - bounds.x) / cell), y = top + (int)((input.mousePosition.y - bounds.y) / cell), meals = shelfBrush; Queue(() => { if (world.TryPlaceShelf(x, y, meals)) { shelfBrush = -1;restBrush=false;boxBrush=false;phoneBrush=false; textureX = -1; selectedShelf = y * world.Width + x + 1;selectedStorageBox=0;selectedPhone=0; selected = -1; selectedBatCell=-1; status = (meals == 0 ? "Placed EMPTY shelf: 0 meals. " : "Placed STOCKED shelf: " + meals + " meals. ") + "Gold = stocked; grey = empty. Humans still need to search."; } else status = "Shelf needs a free non-door cell, away from bodies and reserved destinations."; }); input.Use(); }
            if (placing.HasValue && input.type == EventType.MouseDown && input.button == 0) // Handle manual placement only after an explicit tool choice and a map click.
            { // Begin one placement command.
                int x = left + (int)((input.mousePosition.x - bounds.x) / cell), y = top + (int)((input.mousePosition.y - bounds.y) / cell); Species species = placing.Value; // Capture the chosen cell and species before the GUI event ends.
                Queue(() => { int id; if (world.TrySpawn(species, x, y, out id)) { selected = world.Count - 1; selectedShelf=0;selectedStorageBox=0;selectedPhone=0; selectedBatCell=-1; previous[selected] = world.ActorAt(selected); motion[selected].Snap(x, y, world.Tick); status = "Placed " + species + " #" + id + ". Click Run to start movement."; placing = null; } else status = "Cannot place here: choose a walkable cell with space, within the population cap."; }); input.Use(); // Synchronize new body history outside OnGUI and retain invalid placement for another attempt.
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
        private static void DrawShoveHands(ShovePresentation pose,float x,float y,double tick,Rect bounds,int left,int top,float cell)
        {
            float dx=pose.DX,dy=pose.DY,reach=(float)(0.26+0.16*pose.Strength(tick));GUI.color=new Color(0.88f,0.76f,0.62f);
            for(int side=-1;side<=1;side+=2)for(int part=0;part<4;part++){float along=0.16f+(reach-0.16f)*part/3f,px=x+0.5f+dx*along-dy*side*0.14f,py=y+0.5f+dy*along+dx*side*0.14f;GUI.DrawTexture(new Rect(bounds.x+(px-left-0.06f)*cell,bounds.y+(py-top-0.06f)*cell,cell*0.12f,cell*0.12f),Texture2D.whiteTexture);}
        }
        private static void DrawGrappleHands(ShovePresentation pose,float x,float y,double tick,Rect bounds,int left,int top,float cell)
        {
            float dx=pose.DX,dy=pose.DY,reach=(float)(0.28+0.2*pose.Strength(tick));GUI.color=new Color(0.42f,0.48f,0.26f);
            for(int side=-1;side<=1;side+=2)for(int part=0;part<4;part++){float along=0.14f+(reach-0.14f)*part/3f,px=x+0.5f+dx*along-dy*side*0.16f,py=y+0.5f+dy*along+dx*side*0.16f;GUI.DrawTexture(new Rect(bounds.x+(px-left-0.06f)*cell,bounds.y+(py-top-0.06f)*cell,cell*0.12f,cell*0.12f),Texture2D.whiteTexture);}
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
            if (health > 0) { bool open = forceOpen || world.TerrainAt(x, y) == Terrain.DoorOpen; float damage = 1 - health / (float)DoorRules.MaxHealth; return new Color(Mathf.Lerp(open ? 0.39f : 0.70f, open ? 0.47f : 0.72f, damage), Mathf.Lerp(open ? 0.33f : 0.62f, open ? 0.20f : 0.32f, damage), Mathf.Lerp(open ? 0.28f : 0.53f, open ? 0.18f : 0.30f, damage)); } if(world.TerrainAt(x,y)==Terrain.Floor)return BuildingFloorColor(world.BuildingUseAt(x,y));return ColorFor(world.TerrainAt(x, y)); // Muted closed/open browns acquire a continuous red tint as damage accumulates.
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
