> These notes describe the prototype-three baseline. See [MOVEMENT_UPDATE.md](MOVEMENT_UPDATE.md) for current cautious behavior, presentation timing and explicit upgrade instructions. The current Fight scenario starts Alarmed; Group Support does not make a cautious human charge.

# Human reactions and simulation navigation

Current build label: **Foundation 3 / human reactions**. New encounter saves use `encounter-prototype-3`. Original foundation and prototype-one/two saves retain their original behavior; start a new encounter to use human reactions.

## Try the update

1. Download the latest GitHub ZIP and extract into a fresh folder, preserving earlier projects and saves.
2. Add the extracted **OutbreakRebuild** folder in Unity Hub and open using Unity 6.
3. Choose **Outbreak > Rebuild > Open and Run Foundation**.
4. Choose **Inspect Fight Paused**, **Inspect Freeze Paused** or **Inspect Flight Paused**. Select human #1, then **Step 1 tick** to observe the controlled initial response. The different scenarios use explicit natural SPECIAL values with bravery derived by the established formula.
5. Try **Inspect Group Support Paused** to see an otherwise fearful human supported by two brave allies. **Start Encounter** uses 24 humans with explicitly mixed bravery, plus one zombie; this fixture is not the future weighted population generator.
6. Use **Main Menu** during any simulation. It pauses and retains the run. **Resume current run** resumes the same world with camera, selection and speed retained. Choosing another scenario asks before replacing the run; **Keep current run** leaves it paused in the simulator.

The simulation offers **Step 1 tick**, **Step 10 ticks**, **Step 50 ticks** and **Step 100 ticks**, enabled only while paused with bodies present. Each advances every system by exactly that number and stays paused. At normal speed these represent 0.2, 2, 10 and 20 simulation seconds; the existing game-time ratio remains six game seconds per tick. Large batches execute synchronously, so a crowded run may briefly take time to process. Existing Run/Pause and 0.5x/1x/2x/3x playback remain available.

If buttons do not receive clicks, stop Play mode, set Active Input Handling to **Both** in Player settings and restart Unity when prompted. Native import/rendering remains a separate verification step from the headless checks below.

## Decisions and provisional tuning

The retained formula is **Courage Margin = bravery + observed group support - stress penalty - threat pressure**. Margin at least +1 produces Fight; zero or minus one initially produces Freeze; minus two or lower produces Flight.

- Stress is bounded to 0–100. The selected human displays Normal at zero, Aware at 1–20, Cautious at 21–40, Alarmed at 41–60, Afraid at 61–80, and Panicked at 81–100. Reaction and stress label are separate: high bravery/support can produce Fight despite elevated stress. Stress penalty is integer stress divided by 20.
- Humans observe on their existing five-tick opportunities. A visible threat adds two stress, or four within two cells. No visible threat recovers two stress per opportunity. This is preliminary out-of-sight recovery, not a completed shelter/hiding system.
- A threat more than six cells away with stress below 20 initially produces **Aware**: the human watches before automatically fleeing or charging. The original prototype sight radius and wall/corner occlusion remain.
- Visible nearby humans within four cells contribute their bravery, with each contribution bounded to -1 through +3. The combined prototype support is capped from -3 through +6. Support is observed every ten ticks (two simulation seconds), so departures do not affect it instantly. Walls block observation. This uses bravery rather than implementing the later witness/knowledge model.
- Threat pressure is the number of additional visible zombies plus one when the nearest zombie is within two cells, capped at three. Zombies outside sight contribute nothing.
- **Freeze** stops movement for a seeded 5–15 ticks (one to three simulation seconds). The duration is chosen once using a separately saved random stream. At its deadline it resolves to Fight when the cached margin is at least one, otherwise Flight. It does not keep renewing while stress remains. Losing sight permits recovery; reaching zero stress resets the episode.
- **Flight** chooses a valid neighboring step farther from the nearest visible zombie. Full shelter selection, stress-aware routes and hiding remain future work.
- **Fight** approaches a visible zombie and performs a nonlethal defensive shove on contact. A shove staggers for three ticks and can displace the zombie one cell away if terrain and crowd capacity permit. It does not damage zombie health. Each human has a ten-tick shove cooldown, preventing that human from continuously renewing stagger. A staggered zombie cannot move/bite; humans can pass its cell within normal crowd limits. Full grabbing, evasion, weapons and offensive combat remain pending.

All numbers beyond the retained courage thresholds are provisional and centralized in HumanRules/EncounterRules. Selection shows stress, reaction, support, pressure, margin, freeze countdown and zombie stagger state. No automatic infected-debug tint has been enabled.

## Persistence and validation

Stress, current reaction, cached support, deadlines, freeze episode state and the independent reaction random stream are saved. Conversion clears human-only reaction state while retaining the existing stable identity and physical-stat inheritance. Malformed modes, enums, values, deadlines and species-incompatible state reject transactionally before replacing a live world. Old releases reject prototype-three saves; original saves load under their original rules without silent upgrades.

Executed with .NET 8.0.419 on Linux:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-checks
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

Core: 32 acceptance scenarios / 21,433 assertions. Coverage includes real reaction actions, stress recovery, support occlusion and delayed refresh, variable freeze with pause/save continuation, blocked/open shoves, stagger/cooldown deadlines, conversion cleanup, invalid saves, all four tick batches and independent continuation from prototype-one and prototype-two releases. Both movement and encounter suites remain intact. A 1,000-body reaction fixture ran 500 ticks on a 1,000-square map, then compared another 100 ticks after reload; exact host measurements are in HUMAN_REACTION_CHECKS.txt. Measurements exclude rendering and are not complete-game performance claims.

The UI harness compiles actual runtime handlers against narrow Unity API doubles. It checks the scenario buttons, tick batches, disabled running steps, paused-host menu dispatch, exact retained world/camera/speed on resume, and both replacement outcomes. It does not verify native Unity layout, mouse input, editor import, operating-system dialogs or a Windows executable. The module-baseline Python check passed; native Unity package resolution remains unverified here.
