Latest build: [Encounter setup](ENCOUNTER_SETUP.md).

# Shelter recovery and safe exits

Current build: **Foundation 8 / shelter recovery**, rules `encounter-prototype-8`.

Healthy sheltered humans now recover, inspect an exit and return outdoors when their observations allow it. The doorway pursuit fix remains active.

## Try the build

1. On GitHub, choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Select **Inspect Shelter Recovery Paused**. Click **Step 50 ticks**, then **Step 10 ticks**: the human is inside beside the open inspection door. Another **Step 10 ticks** completes departure and closes the door behind it.
4. Try **Inspect Guarded Exit Paused** with the same buttons. The human opens the door, sees the waiting zombie, closes it and returns toward its interior destination. When danger is visible immediately, opening and closing occur in the same logical tick; a peek need not leave an open door on screen.

For an existing Foundation 7 saved run, **Load Run → Enable shelter recovery → Run**. The upgrade keeps the run paused and preserves actors, tick, terrain and random state. The source file stays unchanged; save to another filename if keeping the earlier run. Earlier generations retain their existing upgrade labels and upgrade to all current behavior when explicitly selected.

## Behavior

- Human stress must reach zero, followed by 25 calm ticks (five simulation seconds). Stress or a visible zombie resets this interval.
- The human chooses a reachable observation cell just inside a real door leading outdoors. It keeps its room reservation while approaching and inspecting.
- Opening costs a movement opportunity. The newly open sightline is checked immediately; visible danger cancels departure and closes an unoccupied door. If clear, the human stays inside for another observation opportunity before entering the doorway.
- Each subsequent exit step rechecks visible threats. A new sighting cancels departure and uses the existing route back into the shelter. Occupied outdoor destinations delay departure.
- Reaching the adjacent outdoor cell releases the room reservation and resumes ordinary movement. The human closes the door behind itself if no other body occupies it.
- Infected humans keep their previous shelter/escape behavior and do not attempt this return to ordinary routine. Turning clears recovery state.

The inspector shows exit progress and the calm countdown. Decisions, door changes and countdowns obey pause and exact tick controls. Save/load retains the calm interval, chosen door, outdoor destination and current phase.

“Safe” means the human's current visible area and departure corridor are clear. This does not reveal hidden zombies, inspect the whole town or guarantee survival after leaving. A blocked exit keeps the human sheltered; a guarded selected door can be checked again after recovery rather than granting knowledge of other exits. Door breaking, sound propagation, patrols and survivor daily schedules remain future work.

## Validation

Run these sequentially from **OutbreakRebuild**:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-recovery
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

The core checks cover complete recovery/inspection/departure, hidden and newly visible threats, crowding, occupied-door safety, infected cancellation, turning, pause, saved phase validation and exact continuation. Independent prototype-seven traces were produced by executing commit `189f34d`; prior saves keep that behavior until explicitly upgraded. The UI harness runs actual scenario, tick and upgrade callbacks against narrow API doubles.

All 65 core acceptance cases / 22,909 assertions passed, along with 157 Legacy UI/input assertions and three New-only input assertions. Project configuration checks passed. See **RECOVERY_CHECKS.txt** for executed results. Native Unity import, layout, mouse input, graphics and a Windows player build remain unverified in the cloud environment.
