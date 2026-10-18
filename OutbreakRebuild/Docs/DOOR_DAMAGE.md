# Door damage and breaches

Current build: **Foundation 11 / door damage and breaches**, rules `encounter-prototype-10`.

Closed doors now delay a pursuing zombie instead of holding it indefinitely. Damage persists, and a broken door becomes a permanently open passage that humans cannot close behind themselves.

## Try it

1. Download GitHub **Code → Download ZIP**, extract into a fresh folder, and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Door Breach Paused**, then click zombie **#2**, standing just outside the door.
4. Click **Step 10 ticks** repeatedly. The inspector shows nearby door health: **68 HP at tick 10**, **36 at tick 20**, **4 at tick 30**. The door breaks on **tick 32**.
5. At tick 40, the doorway is red and passable. Further steps let the zombie bite the trapped human. This tiny dead-end room deliberately isolates breach and contact timing; ordinary encounters still use their available escape routes.

New quick and configured encounters enable this behavior automatically. For a Foundation 10 save, choose **Load Run → Enable door breaking → Run**. The update preserves the exact actors, terrain, clock and random state, starts paused and leaves the source file unchanged.

## Rules in this prototype

- Every intact door has the retained **100 HP**.
- A zombie can attack a cardinally adjacent closed door that blocks its route to a still-remembered human sighting. It does not attack random doors while wandering or inspect a hidden human's current coordinates.
- An attack consumes the zombie's normal one-second movement opportunity. Damage is provisionally **10 + 2 × Strength**: a Strength-three zombie removes 16 HP and needs seven hits. Multiple attackers contribute to the same door in stable actor order. Stagger suppresses attacks using the existing clock.
- A hit does not move the attacker. After the final hit opens the door, movement or biting happens on a later eligible opportunity. Ordinary vision and target reacquisition apply through the opening.
- Memory expiry stops attacks; existing damage remains. A zombie may then wander, or resume attacking after a new legitimate sighting gives it a remembered route.
- Humans can open and close damaged doors while HP is positive. A zero-HP door stays open. A human who sees a breach rejects that shelter and reassesses its available escape options; hidden structural damage is not revealed through walls.

Closed doors are brown, damaged closed doors turn orange, ordinary open doors are tan, and broken doorways are red. Selecting a nearby body exposes the door's actual HP or **BROKEN** state. Positive damage refreshes the terrain cache without snapping movement; a breach also updates collision and visibility.

The earlier **Shelter Entry** diagnostic deliberately retains its preceding indestructible-door rules to isolate entry and last-seen pursuit. The **Guarded Exit** diagnostic retains a stationary guard. Their upgrade buttons can enable current behavior; use **Inspect Door Breach Paused** for the full new lifecycle.

Door damage does not yet add locks, repair controls, window breaking, banging sounds, sound attraction or barricades. These require their own behavior and validation rather than granting humans knowledge through walls.

## Persistence and compatibility

Running saves retain door HP, broken state, infection, actor decisions and both random streams. Exported maps also keep door condition, so loading a broken doorway as a new map does not heal or close it.

Unchanged maps still use schema version one and omit the new condition field. Maps with damaged doors use **schema version two**, with sparse condition records for HP below 100; broken records require open-door terrain. Older builds reject this newer map version instead of silently losing damage. Loading validates coordinates, terrain, HP, unique records and version compatibility. Source authoring metadata is preserved. Sparse condition collections are bounded at 100,000 records and never silently cropped.

Previous running saves retain their original rules until explicitly upgraded. Independent prototype-nine reference fixtures were produced by executing commit `afca45e`, not by regenerating expectations from the breach implementation.

## Validation

Run sequentially from **OutbreakRebuild**:

```
dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak-door-damage
dotnet run --project Tests/UiChecks.csproj
dotnet run --project Tests/UiChecks.csproj -p:InputMode=NewOnly
python3 Tools/check_project_config.py
```

Core coverage includes exact hit/break/entry/contact timing, pause, damaged and broken saves, remembered-route eligibility, memory expiry, idle restraint, window solidity, strength, stagger, shared attacks, map export, ownership, malformed records, visible versus occluded breaches and prior-release continuation. UI checks execute the actual scenario, stepping, inspector selection/health display, cache invalidation, upgrade and real local Save/Load Run and Map callbacks using narrow native API doubles.

All 84 simulation acceptance cases / 32,841 assertions passed, along with 340 Legacy UI/input assertions and three New-only input assertions. Project module configuration checks passed. See **DOOR_DAMAGE_CHECKS.txt** for executed results. Native Unity import, layout, graphics, keyboard/mouse input and a Windows player build remain unverified in the cloud environment. The maintained Mono runner was not executed here.
