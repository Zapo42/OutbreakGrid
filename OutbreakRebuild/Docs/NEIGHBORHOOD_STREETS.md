# Foundation 116 — neighborhood streets and distinct crosswalks

In **Set Up Generated City**, click **Street layout: Grid** to choose **Neighborhood**, then start a new city. **Inspect Neighborhood Streets Paused** opens a standard two-house neighborhood directly. The original Grid option stays available and retains its previous layout. Seed, size, residential density, staffing and finite starting supplies remain independent controls/policies.

## Street variety

Neighborhood generation keeps a connected perimeter and seeded road backbone, then changes optional connecting streets into short dead-end side streets or pedestrian links. Supported playable city sizes include genuine T-junctions, a shortened side street and a pedestrian link. Crosswalks are placed only on junction arms that still carry a road, rather than leaving painted crossings across removed arms.

Both networks remain connected: roads plus crosswalks form one road-surface network, and sidewalks plus crosswalks form one pedestrian network. Shortened road ends meet sidewalk paving; the former connecting space becomes a walkable link. Building entrance paths retain their sidewalk connections, including both houses in dense residential blocks. Some frontages can face a pedestrian link instead of a through road.

The block anchors and building-lot geometry are still rectangular. This increment breaks the complete street grid into varied connections; it does not yet create curved streets, arbitrary lot shapes, parking, turning circles or traffic signals. Dead-end streets have simple straight caps, not vehicle turning bulbs.

## Crosswalks and future vehicles

Crosswalks now use **dark road paving with pale zebra stripes**. The stripes orient to the crossing direction. Sidewalks keep **solid light paving**. The map legend explains both. This presentation change also applies to existing saved crosswalk tiles without a rules upgrade.

Crosswalks keep their separate terrain identity and pedestrian preference. `TerrainRules.RoadSurface` explicitly includes **Road and Crosswalk**, while excluding Sidewalk. This is shared-surface groundwork for future vehicles: painting a crossing does not turn road ground into pedestrian-only sidewalk or create a barrier. There are no vehicles, driving, lanes, right of way or collision rules in this increment. Future vehicle navigation still needs those systems.

Markings are drawn over terrain and do not alter movement, collision, energy, tick state or random streams. The visible crossing list refreshes with the terrain viewport, avoiding a full-map scan each frame. Loading a map without crosswalks clears the previous markings.

## Persistence and validation

New neighborhood street maps use city-streets-2; furnished neighborhood maps use city-neighborhood-lots-1 or city-neighborhood-lots-2 for single/two-house density. Existing grid IDs and defaults are preserved. Saves and restart retain actual generated terrain; they do not rerun the generator. Simulation rules remain encounter-prototype-90, and no save upgrade is required for this increment.

Checks cover deterministic generation, T-junctions, dead-end caps, reduced crossing counts, connected road and pedestrian networks across seeds/sizes, preserved furnished entrances, both densities, finite setup counts, clerk orders and saved job continuation. UI API doubles verify the style choice, preview/restart, striped marking orientations, distinct paving colors, state-neutral drawing and cache clearing on map replacement. They do not verify native rendering, layout, input or frame rate in Unity 6000.6.3f1; the Foundation 82 checkpoint remains unchanged.

Future inventory categories remain supplies, medical and ammo. Existing food-only storage is unchanged.
