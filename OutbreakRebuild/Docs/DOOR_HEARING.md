# Hearing nearby door impacts

Current build: **Foundation 12 / door-impact hearing**, rules `encounter-prototype-11`.

Humans now react to nearby zombie strikes on doors without needing to see the attacker. A heard impact raises stress, cancels calm exit clearance and temporarily postpones recovery and departure. This is simulated hearing behavior; this build does not play a speaker audio clip.

## Try it

1. Download GitHub **Code → Download ZIP**, extract into a fresh folder, and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Door Hearing Paused** and click human **#1** inside the room.
4. Click **Step 10 ticks**. One hidden door strike occurred at tick 2. Human stress is **8**, the inspector shows **Heard door impact at 3, 2**, and the human postpones departure.
5. Click **Step 50 ticks**. The zombie forgot its sighting before another attack. Quiet cleared the heard memory at tick 27; stress then recovered to zero, beginning a fresh calm interval before another exit inspection.

Try **Inspect Door Breach Paused** for repeated impacts and eventual entry. New quick and configured encounters enable hearing automatically. For a Foundation 11 save: **Load Run → Enable door hearing → Run**. The update retains actors, damaged doors, clock and random state, pauses the run and leaves the source file unchanged.

## Prototype behavior

- Each eligible zombie door strike emits one impact from the actual door cell, including the strike that breaks it. Merely forgetting, wandering beside a door or being unable to attack emits nothing.
- The impact travels along cardinal paths for at most **eight cells**. Solid wall tiles block paths. Doors, windows, fences, gates and other terrain transmit this first acoustic model. A sound can travel around a short wall if the remaining travel budget permits it.
- A reachable human gains **eight stress units**, capped at 100. Repeated hits accumulate stress and refresh a **25-tick / five-second** recent-hearing memory. These range, stress and timing values are provisional tuning.
- The listener remembers the impacted door location. Hearing does not reveal a zombie ID or its current position, does not refresh visual avoidance, and does not grant global target knowledge. Zombies do not respond to these sounds yet.
- Recent impacts interrupt calm clearance and an exit inspection already underway. Sheltered humans return toward their existing interior destination and close an available intact door using normal movement and door rules. Visible threats and infection continue to take priority.
- Without a visible threat, ordinary stress recovery waits until recent hearing expires. An outdoor listener with no other shelter or escape action pauses rather than taking random wandering steps while listening.
- After five seconds without another heard impact, memory clears on its exact tick. Stress recovery resumes on normal actor opportunities. Shelter departure requires stress to reach zero and a new calm interval; old clearance is not reused.

The inspector shows the latest heard door location and remaining recent-hearing time. Hearing, stress, recovery, damage and movement all obey simulation pause and exact tick stepping.

## Persistence and scope

Running saves retain the heard source and deadline, alongside existing door damage, infection and actor decisions. Saving and loading reproduces subsequent impacts and expiry exactly. Turning clears human-only hearing. Older rules reject active hearing fields; prior saves keep their original behavior until explicitly upgraded. Independent prototype-ten fixtures were produced by executing commit `e605a6d`.

Propagation uses a bounded 17×17 scratch frontier per impact and retains no event backlog. No additional random stream or random draws are used. This is a local hearing model, not a full acoustic simulation: wall material, attenuation, individual Perception thresholds, shouts, weapon sounds, zombie attraction and speaker audio remain future work. Hearing identifies a sound location without guaranteeing the listener can see or safely reach it.

## Validation

Run sequentially from **OutbreakRebuild**:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-door-hearing
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

Core cases exercise unseen impacts, nearby/distant/enclosed listeners, transmission through a door/window, repeated hits, stress bounds, interrupting open exit clearance, exact quiet expiry, recovery, pause, complete saved continuation, infection priorities, turning cleanup, malformed fields and prior-release compatibility. UI checks run the actual new scenario, body selection, tick commands, heard-location inspector and upgrade callbacks against narrow native API doubles.

All 92 simulation acceptance cases / 32,994 assertions passed, along with 350 Legacy UI/input assertions and three New-only input assertions. Project configuration checks passed. See **DOOR_HEARING_CHECKS.txt** for executed results. Native Unity import, layout, graphics, keyboard/mouse input and a Windows player build remain unverified in the cloud environment. The maintained Mono runner was not executed here.
