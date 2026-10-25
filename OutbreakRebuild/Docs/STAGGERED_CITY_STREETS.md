# Staggered connected city streets — Foundation 121

Start a **new city** through **Set Up Generated City**, which now defaults to **Street layout: Staggered**, or open **Inspect Staggered City Paused** for a dense, zoned example. The regular Generated City and Generated Buildings previews also use staggered streets. Existing running saves and Restart scenario retain their saved terrain; restarting an old city does not regenerate it.

Side streets shift between district rows and meet the main roads at offset T-junctions, instead of continuing every vertical road through a complete grid. The road network is connected, with usable right-hand lanes, crossing stripes and narrow sidewalks following actual road edges. Building lots and entrance paths use the same row-specific street positions, so houses, stores and distribution centers stay accessible. Starting populations, building counts and finite stock remain as configured.

The generator still uses rectangular lots and cardinal road tiles. This is staggered block geometry, not curved roads or fully organic land subdivision. Grid remains an explicit layout choice; Neighborhood remains available for its connected backbone and dead ends. Neighborhood no longer replaces removed two-lane streets with solid four-cell-wide paving: their interiors become grass strips, with narrow sidewalk caps at the ends. Existing sidewalk links remain connected.

T-junction traffic protection includes the complete crossing footprint, even where one quadrant has only one outgoing direction. Cars check exits and take turns under Foundation 120's rules, and their bodies rotate smoothly during playback. There are still no NPC drivers, boarding or cargo.

No save migration or new rule generation is needed for the new geometry. Lane directions and actual terrain remain in map schema 12; running traffic uses encounter-prototype-93. Loading preserves a saved map exactly and never regenerates it from its seed.

Checks cover road and pedestrian connectivity, directed lane reachability, offset axes, absence of oversized solid sidewalk slabs, deterministic generation/saving, entrance access, housing and food counts, clerk orders, vehicle traversal and UI presets/restart. Actual Unity 6000.6.3f1 rendering and play testing remain unverified under the standing instruction; the Foundation 82 checkpoint is unchanged.
