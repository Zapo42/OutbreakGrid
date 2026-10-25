Current update: [Foundation 119 adds lane directions and following distance](VEHICLE_LANES.md). The text below records the Foundation 118 prototype.

# Foundation 118 — vehicle road-routing prototype

Choose **Inspect Vehicle Routing Paused**, then Run or step 100 ticks. The scene has one blue vehicle and no NPCs. It drives along a small road loop, crosses the painted zebra crossings, turns and stops at its chosen destination. Vehicle-only runs can use the normal Run, Pause, tick and speed controls.

Click the vehicle, or use **Vehicles → Observe vehicle #…**. The selected-item panel shows its position, destination and status:

- **Choose vehicle destination:** pause and click a reachable road/crosswalk cell.
- **Stop vehicle:** clear its command and park in its current cell.
- **Vehicles → Place vehicle:** click an unoccupied road/crosswalk to add a parked car, then choose its destination.
- **Cancel vehicle action:** cancel an unfinished placement/destination click.

A selected vehicle is yellow; otherwise its body is blue, with a window and four dark wheels. Its body orientation changes with direction. Motion interpolates between ticks; pausing, stepping, loading and restarting synchronize to actual positions. This is a simple top-down marker, not a finished car sprite or steering animation.

## Routing and safety

Vehicles travel only on **Road** and **Crosswalk**, using the shared road-surface classification established in Foundation 116. Sidewalks, pedestrian links, grass and interiors are excluded. Routes use deterministic cardinal steps, including across crosswalk paint. The same API works on generated Grid and Neighborhood roads. A vehicle takes one cell every three ticks, with normal simulation speed controls affecting playback.

A vehicle waits if its next cell contains another vehicle or an actor. Before entering a crossing, it also checks the connected crosswalk tiles, so a pedestrian occupying the other half prevents entry. Bodies and zombies block vehicles too; there is no ramming or collision damage. Walking, pursuit and shove/knockback movement cannot put actors into vehicle cells. Vehicles also act as solid cell obstacles under existing sight/corner rules. Furniture cannot be placed under a vehicle.

After a crossing or road obstruction clears, the vehicle retries automatically. A blocked destination/route reports **Route unavailable** and waits; it does not drive through furniture or leave the road. Traffic with opposing parked cars can remain blocked until a vehicle is retargeted. There are no lanes, traffic lights, priority rules, overtaking or deadlock recovery yet.

The prototype uses a **one-cell logical footprint**, at most **16 vehicles**, a 256-cell query radius and a 16,384-road-cell search budget. Routes are recomputed from current terrain without random draws or persistent path caches. Custom connected crosswalk regions over 64 cells conservatively block entry. There is no vehicle physics, multi-cell chassis, fuel, drivers, boarding, passengers, cargo or courier integration in this step. Existing NPC commutes and deliveries continue on foot.

## Saves and compatibility

Vehicles, IDs, destinations, headings, wait states and movement deadlines are saved in **running saves**. Save Map retains terrain only. Restart restores the initial run's vehicles/routes and removes later additions. Loading does not replay the route or generate vehicles.

New runs use **encounter-prototype-91**. Rules-90 runs retain their previous behavior until **Enable vehicle road routing** is clicked; earlier saves expose their applicable earlier upgrade. Generated cities still begin without vehicles, so use the placement control after enabling the feature. Old software that does not understand rules 91 cannot load the new running saves. No inventory types or weapon storage were added; future supplies/medical/ammo categories remain planned.

## Validation

Checks cover road/crosswalk-only arrival and turns, generated-neighborhood routes, pedestrian crossing yield/resume, actor/vehicle exclusion, parked traffic, rejected commands, dynamic blocked destinations, exact save continuation, detached state, corruption rejection, legacy rules and entity bounds. UI API doubles cover the zero-NPC preview, normal tick controls, placement/retarget/stop, invalid sidewalk clicks, restart, smooth motion, pause/resume and explicit old-run upgrades. Actual Unity 6000.6.3f1 rendering, input, file dialogs and performance remain unverified; the Foundation 82 checkpoint is unchanged.
