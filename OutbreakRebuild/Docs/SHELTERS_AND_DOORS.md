# Shelters and doors

## Doorway pursuit fix — Foundation 7

Fixed zombies stopping short when a fleeing human closes a door. They now approach the closed door along the route to their last sighting, stop outside it, and resume if it opens before their existing search deadline. A hidden human does not refresh their memory. Walls and diagonal collision remain solid; zombies still cannot break doors.

Start a new encounter to use the fix. For a saved Foundation 6 run, load it and press **Update doorway pursuit**, then Resume. Older saves retain their original rules until upgraded; the update leaves the source save file unchanged.

Use **Inspect Shelter Entry Paused** and advance with Tick buttons to see the zombie reach the closed doorway instead of stopping several cells away.

Current build: **Foundation 7 / doorway pursuit fix**, rules `encounter-prototype-7`.

## Try this build

Download the latest GitHub ZIP, extract into a fresh folder and open **OutbreakRebuild** in Unity 6. Choose **Outbreak > Rebuild > Open and Run Foundation**.

- **Inspect Shelter Entry Paused**: select human #1. Step 10 ticks: the human has selected the enclosed room and opened its door. Step another 10: it is inside and has closed the door behind itself. Continue stepping to reach the retained interior destination. The pursuing zombie stays outside the closed door.
- **Inspect Unsafe Shelter Paused**: a visible zombie is already inside the human's selected room. Step 1 tick to see the original room rejected and an alternate destination chosen. Subsequent escape depends on reachable space and the moving threat; this is not guaranteed rescue.
- **Start Encounter** now includes two enclosed rooms in the mixed-trait arena. The 24 humans and one zombie retain their existing spawn fixture, with finite shelter space rather than enough room for everyone.
- Load a previous prototype-five run and click **Enable shelter behavior** to upgrade it explicitly. It remains paused, with identities, clock, random streams and infection timing retained. The original save file is untouched. Earlier reaction versions retain their prior upgrade button labels and opt into the current rules. Existing open-arena maps do not acquire new buildings: start the new encounter or load a map with suitable floor/wall/door geometry.

Main Menu / Resume current run and the 1/10/50/100-tick buttons remain available. Selecting a human shows its room destination, entry state and temporary room rejection. Closed doors are dark brown and open doors light tan in the debug view; the inspector includes a legend. The inspector reports discovered room count and whether a discovery limit was reached.

## Room choice and retained goals

Rooms are recognized from connected **Floor** tiles bounded cardinally by Walls, Windows and actual DoorOpen/DoorClosed cells. Floor spilling into ordinary outdoor terrain is not treated as an enclosed building. Door tiles separate floor components, so adjoining rooms can have different identities. Discovery uses authored sparse terrain, not a dense scan of the world.

Bitten humans, humans already in Flight, and humans seeing a threat with a sufficiently negative courage margin seek an available reachable room. Once chosen, the destination is retained while valid rather than switching on small score differences. Humans move deeper away from openings and remain there as stress recovers instead of immediately wandering back outside. Infection continues while sheltered and still turns the same actor on the original deadline.

Provisional planning capacity is one human per two floor cells, with a minimum of one. Actual human occupants and inbound destinations count together without double-counting an individual. Individual floor-cell reservations also respect the ordinary four-human limit. This is admission/comfort planning, not a new global restriction on manual spawns. Narrow doorway congestion may delay entry even when the room has space. A blocked route can choose another interior cell, wait when not under immediate threat, or abandon the route when danger is close.

Selection favors fewer openings, room space and shorter reachable routes. Door-aware local paths use a bounded 33x33 region centered on the actor. Bitten humans retain the previous non-approaching rule relative to observed threats. Healthy fleeing humans may route around a threat but avoid coming within two cells where possible; if already closer, a planned step cannot worsen that clearance. Closed doors can be opened by humans; diagonal doorway/corner shortcuts remain disallowed. These are provisional safety rules, not global hierarchical pathfinding or a complete shelter risk model.

## Door actions and unsafe rooms

Opening a closed door consumes one human opportunity before moving through it. A human closes an adjacent open door after actually reaching the interior, provided nobody occupies the door cell. A human or zombie in the doorway prevents closure. Distant entrances are not closed automatically.

A zombie actually observed inside the destination makes the human release it, reject that room for 50 ticks (ten simulation seconds), and reassess reachable alternatives. Hidden zombies are not revealed by the room-danger check. If no alternative room exists, a human can still open a reachable exit, evacuate the unsafe room and close behind itself. Impossible or blocked escapes are not bypassed with teleportation. Turning releases human room reservations and clears human shelter state immediately; other occupants must notice the resulting zombie through existing sight rules.

Door changes update the world's cloned terrain, not the original source map object. Actual door state is included in map/run saves. Presentation invalidates its terrain cache and synchronizes body positions when doors change, preventing a stale visual segment from appearing inside a newly closed door. Pause and manual stepping govern all door actions.

**Zombies do not yet break doors, operate handles, or damage windows.** Locks, door durability/breaching, complex building metadata, room exploration/knowledge, long-distance routes and finished hiding behavior remain future work. Closed doors can currently hold a pursuing zombie indefinitely. Windows retain the existing solid prototype collision/sight behavior.

## Bounds and persistence

Discovery supports at most 8,192 authored floor cells per world scan, 64 recognized rooms, 256 floor cells per room and four door cells per room. Exceeding the floor-scan limit disables the room catalog for that world; oversized rooms or extra rooms are omitted with the public scan-limit flag set. The terrain itself is never cropped or rewritten. Local escape remains available where no usable room is found. These explicit bounds prepare a representative workflow rather than claiming every large authored town is supported.

Room identity is derived deterministically from its first row-major floor cell. Current destination, interior/approach state and the temporary rejected-room deadline are saved. Loads validate IDs, coordinates, species, room membership and deadline bounds before replacing the live world. Formerly observed doorway cells may legitimately close after an actor leaves; pursuit/avoidance saves now allow that state. Source map metadata strings remain untouched.

Earlier saves keep their original rules until explicitly upgraded. The prototype-five compatibility fixtures were executed from commit 0700d611172f5436f6e153909477804f0e6f484c: BittenEscape(), capture after seven ticks, then after another hundred ticks. Their expected behavior was not generated by the shelter implementation.

## Executed validation

Foundation 7 additionally checks the closing-door pursuit regression, reopening, memory expiry, exact resume, the explicit upgrade button and independent Foundation 6 continuation. Current evidence: [DOOR_PURSUIT_CHECKS.txt](DOOR_PURSUIT_CHECKS.txt).

.NET 8.0.419, Linux:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-door-pursuit
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

57 core acceptance cases / 22,751 assertions passed, plus 142 Legacy UI assertions and three New-only assertions (unsupported input correctly rejected). Coverage includes enclosure/scan limits, real open-enter-close behavior, occupied-door safety, room/cell reservations, unsafe-room reassessment and evacuation, malformed loads, turning cleanup and a healthy occupant witnessing another turn, pause, open-door save continuation and independently recorded prior-release behavior. A 25-body/two-room scenario ran admission, doors and reactions through save/resume; its exact timing and scope are in SHELTER_CHECKS.txt. Existing 1,000-body and movement benchmarks remain separate from this shelter-enabled measurement.

UI checks exercise both authored menu scenarios, door-triggered texture-cache invalidation, exact stepping and explicit previous-save upgrade without rewriting its source file. They compile production handlers against narrow API doubles. Native Unity import, layout, rendering/motion, mouse input, operating-system dialogs and Windows EXE behavior still need native verification.
