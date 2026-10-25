# Foundation 114 — varied street-facing buildings

Choose **Set Up Generated City** and keep using the seed, block counts and size presets. Both **Inspect Generated Buildings** previews also use this generation update. New cities now have entrances facing any of the four streets around their block instead of every entrance facing south.

Each building retains one connected entrance path. North-facing paths join the sidewalk along the north street; east, south and west entrances join the matching street sidewalk. Paths stay in their block and preserve existing roads, sidewalks, crosswalks and map borders. All buildings remain simple enclosed rectangular rooms.

Footprints now vary by use:

- Homes: 7–10 cells wide and high.
- Stores: 10–12 cells wide and high.
- Distribution center: 12–14 cells wide and high.

Placement and facing are deterministic for the seed and map size. The center, stores, homes, furniture, separate worker housing, staffing, shifts and finite starting food retain their existing setup policies. The standard city still has eight buildings, five humans and 63 meals. The largest supported setup still has 36 buildings and 33 humans, within current room-discovery limits.

The generator now identifies new terrain as `city-lots-2`. Reusing an old seed to create a **new** city gives this updated layout, rather than the Foundation 112/113 layout. Existing saved cities retain their actual terrain and inventory: loading or restarting a loaded checkpoint does not rerun generation or rotate its doors. No simulation-rule migration is required; current rules remain encounter-prototype-89.

Automated checks cover all four door orientations, role-specific footprint bounds, preserved street tiles, enclosure and direct doorway reachability across seeds and map sizes, furniture clearance and connected usable interiors. Separate simulations verify off-duty workers physically leave their workplaces and reach their assigned homes through generated entrances, with exact mid-trip saved continuation. Existing setup, phone-order, household and supply-chain checks remain active.

This is still one building per block with a fixed role pattern and one resident per home. Multiple lots per block, multi-room floor plans, flexible zoning and road-layout variety remain later city-generation steps. Native Unity 6000.6.3f1 presentation, input and performance remain unverified; the Foundation 82 checkpoint stays unchanged. Future item categories remain supplies, medical and ammo; current storage stays food-only.
