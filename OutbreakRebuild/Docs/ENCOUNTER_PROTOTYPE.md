# Encounter prototype 1

This user-selected milestone adds a small playable encounter ahead of the broader content-migration stage. It is a prototype, not the complete encounter/combat scope.

## Open and play on Windows

1. Extract the delivered ZIP into a new folder; preserve your existing projects.
2. In Unity Hub, choose Add > Add project from disk and select `OutbreakRebuild` (the folder containing Assets, Packages and ProjectSettings).
3. Open using Unity 6. The project records 6000.0.0f1; native import with your installed patch still needs verification.
4. Choose Outbreak > Rebuild > Open and Run Foundation.
5. In the Game view choose **Start Encounter**. The compact enclosed arena begins with 24 humans and one zombie.
6. Use Pause, Step 10 ticks, playback speeds and Save Run / Load Run. Click a human to inspect its infection countdown; selection keeps the same identity after turning.

If controls draw but ignore clicks, stop Play mode, set Edit > Project Settings > Player > Other Settings > Active Input Handling to Both, and restart Unity when prompted.

## Implemented rules

- Zombies acquire the nearest visible human within a circular 12-cell radius. Stable IDs break ties. Solid collision cells, including closed doors and windows, block prototype sight and bites.
- Pursuit uses an eight-neighbor shortest-path search bounded to 25 by 25 cells with reusable buffers. Solid corners and live crowd limits are respected. There is no last-known-position memory; unseen targets are not pursued. Zombies wait when they have no visible target.
- Humans choose a valid neighboring cell farther from their nearest visible zombie, otherwise wait. With no visible threat they retain random wandering. Humans cannot enter zombie-occupied cells.
- Actors receive one movement/contact opportunity every five ticks, staggered by ID. An unobstructed adjacent or same-cell zombie contact infects a human deterministically and removes 10 internal health points, with a floor of one. Later contact does not reset the deadline or apply more bite damage.
- Turning is due 50 + END * 5 ticks after infection: ten to twenty simulation seconds at five TPS. Pause freezes it; speed changes playback only. These provisional numbers are centralized in EncounterRules, and are not final strain/combat calibration.
- Conversion preserves ID, position, health, END, AGI, Luck and other physical stats; STR/PER have a floor of one, CHA/INT become zero. Human occupancy is released. No body is removed or replaced.
- Encounter saves use rulesVersion `encounter-prototype-1`, retain infection deadlines, and resume deterministically. Original `foundation-1` saves and Movement Test keep their original behavior. Loading a terrain-only map creates an Encounter world; place bodies before running it.
- Infection is shown in the selected actor's inspection panel, without automatically tinting infected humans. Existing debug body rendering remains temporary.

Still pending: full vision cones, sound, knowledge/stress, last-known targets, shelter planning, grabs/escape, bite chances, strain inheritance/decay, infection slowdowns, weapons, sprites, and legacy map migration. This prototype does not claim those systems.

## Executed validation

.NET SDK 8.0.419, Linux:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-checks
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

17 core scenarios / 20,071 assertions; UI handler harness 49 assertions with Legacy input and 3 with New-only input (correctly rejected). The handler harness compiles production runtime UI against narrow Unity API doubles; it does not establish native Unity compilation or mouse delivery.

Core checks cover pursuit, retreat, wall/corner occlusion, contact infection, exact turning deadlines, pause, conversion identity/stats/occupancy, malformed saves, legacy saves and deterministic resume. Existing 15,000-tick movement soak and 100/250/500/1,000 map ladder pass. A 1,000-body encounter on a 1,000-square map ran 500 ticks in approximately 3.0 seconds (about 6 ms/tick), excluding rendering, with another 100 ticks compared after save/reload. This is one host measurement, not a full-game performance promise. See ENCOUNTER_CHECKS.txt for the core run report.

Unity import, real rendering/dialogs, editor menus and Windows EXE compilation have not been executed here.
