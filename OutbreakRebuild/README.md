Latest build: [Foundation 29 / blocked food routes](Docs/BLOCKED_FOOD_ROUTES.md).

# Current update: Shelters and doors

Humans can choose enclosed floor-based rooms, reserve space, open doors, enter, close behind themselves and reassess observed internal danger. The main encounter now has two rooms. [Launch steps, exact limits, old-run upgrade and validation](Docs/SHELTERS_AND_DOORS.md). Door breaking is still pending.

## Previous milestone

# Current update: Bitten escape

Infected humans now prioritize escape over courage, freezing and wandering. They seek local obstacle cover or an observed human screen without approaching a visible zombie. [Rules, controlled scenario, existing-run update and validation](Docs/INFECTED_ESCAPE.md).

## Previous milestone

# Current update: Cautious movement

Aware/Cautious humans keep their distance instead of automatically approaching on a positive courage margin. Full-interval visual motion and direct route preferences reduce stop/start movement and gratuitous zigzags. [Behavior, remaining grid limitations, old-run upgrade and validation](Docs/MOVEMENT_UPDATE.md).

## Previous milestone

# Current update: Human reactions

Stress, visible group support and bravery now drive **Fight / Freeze / Flight**. Fight performs defensive shoves; Freeze resolves after a saved variable delay. The simulator also has **1 / 10 / 50 / 100 tick** buttons and **Main Menu / Resume current run**. [Launch steps, exact prototype rules and validation](Docs/HUMAN_REACTIONS.md). Older saves retain their original behavior.

## Previous update

# Current update: Encounter prototype 2

Choose **Start Encounter** for zombies pursuing visible humans, last-seen search after losing sight, prototype fleeing, contact infection and delayed turning. Choose **Inspect Corner Pursuit Paused** for a controlled obstacle scenario. Prior encounter saves retain their original behavior. The original Movement Test remains available. Read [launch instructions, provisional rules and validation](Docs/ENCOUNTER_PROTOTYPE.md).

The sections below describe the original Foundation 1 baseline; their statements that encounter behavior is absent apply to that baseline.

# Outbreak Simulator — clean rebuild, Foundation 1

This is a separate Unity 6 source project. The original simulator and DragonGlassPixelWorkshop archives remain unchanged. This package starts a new implementation of the established Outbreak scope.

**The production C# core and persistence have been executed and checked. The Unity shell has not been imported or run in Unity here. No Windows game EXE is included.**

## Startup and button repair 2

This source identifies itself as **Foundation 1 / startup repair 2**. Extract the current archive into a new empty folder and add that project to Unity Hub. Do not merge it into an older simulator project; fresh extraction isolates stale editor caches and old bootstrap scripts without deleting the old work.

The manifest restores the documented Audio, Image Conversion, IMGUI and JSONSerialize built-in modules. The previous minimal manifest disabled Audio, which explains the reported AudioListener deletion. Restoring the module does not reconstruct a component already removed from a previously saved scene. The AudioListener error is separate from the observed SearchDatabase indexing failure; restoring these modules is not proof that the Search error is repaired.

There are no automatic editor-startup scene writes now. **Open and Run Foundation** creates the entry scene when explicitly chosen. UI commands use an editor callback as well as the player Update pump, so a Unity Editor pause cannot leave an accepted command waiting indefinitely for a simulation Update. A deliberate Run action resumes host pause; Step 10 can execute while host pause stays on. The UI marks **UNITY EDITOR PAUSED** when applicable and leaves Console Error Pause preferences unchanged.

If controls still fail, attempt one click and choose **Outbreak → Rebuild → Show Input Diagnostics** from Unity's menu. Paste the report copied to your clipboard. It records the exact build/version, whether the editor is paused, live app count, received mouse events, queued/executed commands and the latest command exception. This produces evidence even when the simulator's own controls do not respond.

If the simulator draws its buttons but none respond: stop Play mode, open **Edit → Project Settings → Player → Other Settings → Configuration → Active Input Handling**, select **Both**, and restart Unity when prompted. Some Unity versions display Active Input Handling directly under Other Settings. Runtime `OnGUI` needs the legacy input backend; New-only mode can draw the UI without sending it clicks. This is a documented Unity behavior, not a confirmed diagnosis of the separate SearchDatabase exception.

The project checks input compatibility before its editor launch/build commands and displays recovery instructions in New-only mode. Legacy input is a Player setting; there is no `com.unity.modules.inputlegacy` dependency to install. If you used an earlier correction, use the current complete `Packages/manifest.json`; do not reduce it to IMGUI alone. The project preserves existing Player settings and avoids reflection into Unity 6 build-profile settings. Native dialogs and changing panels run after the GUI event completes.

## What this stage does

- Runs one authoritative fixed clock: 5 ticks per second, 600 ticks per game hour, pause, step ten, and half/normal/double/triple playback.
- Stores terrain in sparse 32×32 chunks. Dimensions are configurable from 1×1 to 10,000×10,000; large empty maps do not allocate a full grid. The current file-size limit is 32 MiB, so this is not a promise that every densely painted 10,000×10,000 map fits this format.
- Moves baseline fixture bodies deterministically, including diagonals, solid collision, corner clearance, four-human/eight-body cell limits, and a 1,000-body population cap.
- Saves the actual clock, actor identities and values, terrain, capacity, next identity and random-generator state. A resumed test follows the same future as an uninterrupted test.
- Provides a small Unity inspection shell: Run/Pause, stepping, speeds, camera pan/zoom, actor selection, and explicit native **Load Map**, Save Map, Load Run and Save Run actions for Unity Editor and Windows.

Humans and the zombie currently perform the same **movement fixture behavior**. Pursuit, perception, stress, infection, combat, economy, research, survivors, full art bindings and the rebuilt authoring tools are future stages. The window says this explicitly.

## Open the source

1. Extract this folder into a new empty location and add it as a project in Unity Hub using Unity 6. The version marker matches the recovered project's Unity 6.0 baseline; allow your installed Unity 6 editor to perform its normal import. The user's exact installed patch remains unknown.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**. This explicitly creates and opens the entry scene and enters Play mode after checking input compatibility. After scene creation you can open `Assets/Scenes/Foundation.unity` and press Play directly. The Game tab shows the runtime interface.
3. The start screen offers **Start Movement Test**, which begins a fixed 24-human/one-zombie fixture, and **Inspect Movement Test Paused** for deliberate inspection. Middle-drag pans, the wheel zooms, and clicking a body opens its values.
4. **Outbreak → Rebuild → Build Windows player** builds this foundation when the Windows build module is available. Its actual Unity compilation, dialogs, rendering and EXE behavior still require native verification.

This shell is an inspection tool, not the finished obsidian-glass UI or rebuilt Workshop.

## Files and compatibility

The new map envelope is `outbreak.map`, schema version 1, saved as `.obmap2`. The running envelope is `outbreak.save`, schema version 1, saved as `.obsave2`. These new extensions avoid overwriting old files with incompatible data.

**Legacy `.obmap`, `.dgproject`, `.obworld`, sprites and Workshop projects are not imported by this stage.** Their originals are preserved; their import and round-trip migration is the next implementation gate. Neither existing application has been replaced. The shared map contract is ready for the rebuilt adapters, but the old Workshop does not yet use it.

Load Map deliberately loads only terrain and its opaque authoring payload, with no generated population. Its empty-population message and disabled Run control explain this state. Choose **Place human** or **Place zombie**, click a walkable cell, then Run. These are baseline test placements; full stat configuration and population generation remain future work. Use **New movement test** for the fixed 24+1 fixture, or load `Examples/FoundationRun.obsave2` for a populated saved run. The example map is an open 100×100 terrain fixture; it is separate from the small room arena created by the shell.

Invalid loads construct and validate a candidate before replacing the current world. Cancellation leaves the current world paused. Saves flush an adjacent temporary file and atomically publish it; failed publication leaves the old file intact.

## Executed checks

See `Examples/FOUNDATION_CHECKS.txt` for exact results and measured movement timings. The acceptance run includes 15,000 ticks at 1,000 bodies and save/resume continuation, plus a 100/250/500/1,000 map-size ladder.

With Mono installed:

```bash
python3 Tools/run_checks.py
```

Or with a .NET 8 SDK:

```bash
dotnet run --project Tests/FoundationChecks.csproj -- Examples
```

These commands execute the actual delivered simulation and persistence source. Their success does not establish Unity compilation or full-game performance. The recorded ladder measures baseline movement on open terrain, with fixtures initially placed in a 50×20 patch; navigation, perception, combat and rendering are absent from that benchmark.

The Mono runner checks the documented module baseline, compiles the actual Windows dialog bindings and checks native record layout, then runs the production UI handlers against a narrow API double in Legacy and New-only configurations. The regression includes a paused host with no simulation Update, exactly-once dispatch, explicit Run resume, paused manual stepping, copied native diagnostics and callback cleanup. These are command/configuration checks, not Unity's real package resolver, layout, mouse backend or operating-system dialogs. The .NET project runs the core/persistence suite only.

Read `Docs/REBUILD_PLAN.md`, `Docs/LOCKED_SCOPE.md` and `Docs/ARCHITECTURE.md` for the work order, retained requirements and boundaries. `Docs/LEGACY_INVENTORY.json` records the hashes of all 637 files in the two recovered archives.
