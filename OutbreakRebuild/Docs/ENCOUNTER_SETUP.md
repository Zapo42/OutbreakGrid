Latest build: [Foundation 20 / sharing food](FOOD_SHARING.md).

# Encounter setup

Current build: **Foundation 9 / encounter setup**. Running simulation rules remain `encounter-prototype-8`; this update adds startup choices without changing existing saved-run behavior.

## Try it

1. On GitHub, choose **Code → Download ZIP**, extract into a fresh folder, and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Click **Set Up Encounter**.
4. Select **Shelter district (100 x 100)**, enter **75** humans and **5** zombies. Leave the seed at **214** and **Start paused: Yes**.
5. Click **Start configured encounter**, then use **Run** or the tick buttons. Use the mouse wheel or Zoom buttons to look closer.
6. Use **Main Menu → Set Up Encounter** to try another setup. Starting a replacement offers the existing **Replace run / Keep current run** choice. **Back to Main Menu → Resume current run** keeps the current simulation.

The main menu and setup form scroll in smaller Game views. Existing behavior test buttons, quick **Start Encounter**, movement tests and file loading remain available.

## Available maps and counts

| Map | Size | Enclosed rooms | Human spawn limit | Zombie spawn limit |
| --- | --- | --- | --- | --- |
| Shelter arena | 20 × 16 | 2 | 96 | 128 |
| Shelter district | 100 × 100 | 12 | 1000 | 1000 |

The combined population must be **1–1000**. Either species may have zero actors. These are actual authored spawn-area limits, not the room comfort capacity. Invalid requests show an explanation and disable starting; actors are never silently omitted.

Both species begin in separate outdoor regions. Placement spreads one body per candidate cell before using additional occupancy layers, with at most four humans or eight total bodies per cell. Solid terrain, doors and indoor floor cells are excluded from initial placement. Default district groups are close enough to begin an observed encounter.

The default quick encounter retains its original map, 24 humans, one zombie and starting positions. The district is a fixed authored arena with rooms and a central road; changing the seed changes random simulation decisions rather than generating a new town. Humans retain the current repeating brave/average/fearful test traits; weighted natural population generation remains future work.

## Repeating and saving a run

Use the same map, counts and seed for the same initial world and deterministic continuation. Seeds range from **1 to 4294967295**. **Start paused: Yes** is the default; toggle it to **No** for automatic playback.

Setup fields are retained while navigating within this application session. **Save Run** preserves the actual map, bodies, random state, clock, infection, reactions and shelter recovery progress. Loading that save resumes its world; it does not reconstruct the form's original starting counts or seed. Existing running saves need no new behavior upgrade for this menu update.

## Executed validation

Run sequentially from **OutbreakRebuild**:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-setup
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

Core acceptance exercises exact species counts, maximum population, map-specific placement limits, invalid requests, zero-species setups, seed repeatability, saved continuation, unchanged default placement, template ownership and an immediately observable district encounter. The UI harness executes production text fields, validation, queued starts, paused/running preferences, map selection, cancellation, replacement and menu resume using narrow API doubles.

All 71 simulation acceptance cases / 30,313 assertions passed, along with 301 Legacy UI/input assertions and three New-only input assertions. Project module configuration checks passed. See **SETUP_CHECKS.txt** for recorded results. Native Unity layout, scrolling, mouse/keyboard input, import and a Windows player build remain unverified in the cloud environment. A valid 1000-body start is a capacity check, not a native rendering or performance claim. The maintained Mono runner was not executed here.
