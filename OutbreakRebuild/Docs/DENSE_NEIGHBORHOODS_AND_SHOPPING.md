# Foundation 115 — denser neighborhoods and storefront discovery

## Nearby-shop failure

The reported empty-cabinet/high-intelligence behavior was reproduced with a nearby stocked shop whose doorway faces away from the resident. Previously, store discovery required seeing the entrance itself. The resident could leave home, see the shop's near wall, fail to identify the shop, return home and repeat after the retry delay. Intelligence changed the pantry threshold but could not supply missing store knowledge.

Current rules also recognize a shop when an outdoor human can see its exterior frontage within normal sight range. The human remembers that store, routes to its actual door, enters, observes shelves, collects finite food and brings it home. Seeing the facade gives no shelf quantity or stock knowledge. Shops fully obscured by intervening walls remain unknown; danger, energy, infection, full inventory and other existing trip conditions still apply. This fixes the reproduced hidden-entrance case, rather than giving NPCs a global list of stocked stores.

Choose **Inspect Hidden Store Entrance Paused**, then Run or step one game hour. Intelligence is 10, the home cabinet starts empty, and the nearby store has stock but its door is on the far side. The regression checks reproduce the prior failure and verify current home resupply, food conservation, no stock leakage and exact saved continuation.

New games use encounter-prototype-90. For an existing rules-89 run click **Enable visible storefront discovery** before continuing. Earlier saves expose the appropriate earlier feature upgrade. Old saved runs retain their recorded behavior until upgraded. No new actor fields were added, and the immutable frontage cache is rebuilt from geometry after loading.

## Two houses per residential block

In **Set Up Generated City**, click **Residential density: One house per block** to switch to **Two houses per block**. The standard 4×2 layout then contains ten homes, two stores and one center: 13 buildings, ten humans and **78 finite meals**. Each home has its own food cabinet, bed and assigned resident; workers retain separate residences and jobs. **Inspect Dense Neighborhood Paused** opens this setup directly.

Paired homes are compact 7–9-cell rectangles in opposite corners of a residential block. Their doors face outward toward different streets, with separate paths to the surrounding sidewalks. Footprints do not overlap, and paths preserve the original street/crosswalk network. Stores and the distribution center retain a single larger building in their blocks. Single-house generation stays available with its previous geometry and defaults.

Generated density is validated against the existing 64-room catalog. Two-house 6×6 cities would exceed that limit and are rejected before starting. In dense mode the Large preset is **6×5 blocks**, containing 54 homes, two stores and one center: 57 buildings, 54 humans, with 210 finite meals. Default-density Large remains 6×6. Independent block counts still work within these limits.

Dense maps identify their generated terrain as city-lots-3. Existing saved cities retain their actual buildings, doors, inventory and population. Restart restores the recorded initial world; changing a setup preference does not rebuild a running city. Current food-only storage remains in place. Future categories remain supplies, medical and ammo; weapons are not added to boxes.

## Validation and next steps

Checks cover paired-house separation, both entrance paths, preserved streets, all room discovery limits, setup summary counts, individual housing, finite food, deterministic generation and saved clerk/courier continuation across multiple seeds and sizes. UI checks cover the density switch, supported large preset, invalid oversized density, both new previews, restart and the rules-89 discovery upgrade. Native Unity 6000.6.3f1 rendering/input/performance remain unverified; the Foundation 82 checkpoint stays unchanged.

Further city work can add neighborhood zoning/store placement and less uniform street layouts. Multiple residents per home and household-size controls remain separate from multiple houses per block.
