# Occasional zombie wandering

Current build: **Foundation 10 / zombie wandering**, rules `encounter-prototype-9`.

Zombies no longer remain permanently motionless after forgetting a human. With no visible human and no active remembered search, each zombie has a **25% chance to take one nearby step on its normal one-second movement opportunity**. Other opportunities are pauses. This timing is provisional tuning, separate from active pursuit speed.

## Try it

1. Download the latest project using GitHub **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Click **Inspect Zombie Wandering Paused**. Select zombie **#2**.
4. Click **Step 10 ticks**: it is still investigating the remembered doorway.
5. Click **Step 50 ticks**, then **Step 100 ticks**. The sighting expires; the zombie takes occasional short steps with pauses. The inspector reports **Idle: occasional wandering** when there is no remembered target.

For an existing Foundation 8 or 9 run: **Load Run → Enable zombie wandering → Run**. The update pauses the simulation and retains actors, terrain, clock and random state. It does not overwrite the source save. New quick and configured encounters enable wandering automatically.

## Behavior and limits

- Seeing a human takes priority over wandering. Existing pursuit, bites, sight radius and collision rules remain active.
- A zombie with active memory continues to investigate the last observed cell, including approaching a closed door. It does not wander away just because the path is blocked. Once memory expires, idle wandering becomes eligible.
- An idle step chooses uniformly among currently legal neighboring cells. Walls, closed doors, diagonal corner restrictions, map bounds and the eight-body cell limit still apply. A trapped zombie rests.
- Wandering reads no hidden human position and does not refresh old sightings. A human encountered by sight is acquired through the normal perception code on a later decision.
- The chance and direction draws use the existing saved simulation random stream. Pause consumes no draws; saving and loading reproduces the same future wandering.
- Fresh idle zombies and zombies produced by turning can wander too. Occasional movement is chance based; several consecutive pauses are expected.

The **Guarded Exit** diagnostic deliberately retains the preceding stationary-idle rules so its waiting guard reliably tests human inspection and retreat. Its **Enable zombie wandering** button can opt it into normal wandering. The new wandering diagnostic and ordinary encounters use the latest rules.

This is local idle movement. It does not implement persistent roaming destinations, sound attraction, hordes, door damage or door breaking.

## Validation

Run sequentially from **OutbreakRebuild**:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-wandering
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

The core suite checks occasional movement and pauses, movement cadence, solid corners, closed doors, bounds, full cells, visible-target priority, exact memory expiry, movement after forgetting, unchanged behavior when a hidden human moves elsewhere, pause and exact saved continuation. Independent prototype-eight fixtures were produced by executing commit `051d500`; old runs retain their earlier continuation until explicitly upgraded.

All 77 simulation acceptance cases / 32,785 assertions passed, along with 313 Legacy UI/input assertions and three New-only input assertions. Project configuration checks passed. The UI harness executes the real diagnostic, tick and upgrade callbacks using narrow API doubles. See **WANDERING_CHECKS.txt** for recorded results. Native Unity import, layout, graphics, input and a Windows player build remain unverified in the cloud environment. The maintained Mono runner was not executed here.
