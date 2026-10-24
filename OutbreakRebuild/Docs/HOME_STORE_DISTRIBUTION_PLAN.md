# Home, store and distribution chain

## Refined development brief

Target Unity **6000.6.3f1** for this and every future incremental build. Preserve the Foundation 82 native-testing checkpoint until the user explicitly resumes testing. Publish each completed increment with a GitHub ZIP link and next-step options.

Give every participating human an assigned residence with food storage, separate from work. Humans inspect their home supply and use intelligence to influence planning and when they decide to shop. They obtain finite food from consumer-facing store shelves, then carry it home and deposit it. Preserve hunger, energy, bravery, danger, inventory limits and personal knowledge; the player’s global stock overview must not become NPC knowledge. Distribution centers supply stores through couriers, rather than serving as consumer shopping destinations.

Make the system readable: home, store and distribution floors use different colors; their storage has distinct shapes; phones look like phones. Courier outlines are brown. Clerk outlines have yellow upper and blue lower halves, divided horizontally. Pure consumers have white outlines. Workers off duty have gray outlines. Shifts eventually let clerks, couriers and future occupations return to their assigned homes to eat/rest and behave as consumers off duty.

Establish the complete local supply chain before expanding test maps/travel distances. Then introduce specialized stores and shopping priorities based on the kinds of supplies missing at home. Continue to build one small increment at a time. Storage remains food-only until explicitly revisited; no weapons in boxes.

## Increment order and acceptance goals

1. **Foundation 104 — visual groundwork and editor target (implemented).** Distinct floors, storage icons, recognizable phones and a paused preview with an actual separate assigned home. No decision-rule changes.
2. **Foundation 105 — home supply inspection and distribution boundaries (implemented).** Use assigned residence rather than the nearest workplace as a normal home. Consumers inspect their own storage and retain observations. Show known/unknown/stale home supplies. Routine consumer food searches should use home storage/store shelves rather than distribution back stock. Preserve legitimate courier access and deliberate emergency shelter behavior. Implemented first-hand stock checks, freshness/report distinctions, own-home routine pantry deposits and routine distribution entry/food-search restrictions. Existing safe assigned-home returns remain active. Full deliberate store shopping and off-duty commutes are still upcoming. See [Foundation 105 guide](HOME_SUPPLY_CHECKS.md).
3. **Foundation 106 — store shopping and return deposits (implemented for known shelves).** Make a saved, interruptible home → store shelf → home box trip with finite pickup/deposit and clear inspector status. Test blocked/empty stores, occupied boxes, danger, infection and reloading mid-trip. Intelligence should influence early planning/recheck timing, rather than provide hidden stock knowledge or make low-intelligence humans incapable of feeding themselves. Implemented an intelligence-dependent trigger, one finite load per trip, household box claims, saved collecting/returning phases, shelf search and home deposits with personal rations preserved. Foundation 107 adds local visible-entrance discovery, store investigation and alternative shops during the same trip. City-wide search patrols and larger travel ranges remain later work. See [Foundation 106 guide](STORE_SHOPPING.md).
4. **Foundation 108 — occupation state, shifts, commutes and outlines (implemented).** Add the requested colors when actual work/off-duty state is available. Preserve existing courier schedules; extend shifts to clerks, distinct commutes, return-home rest and off-duty shopping in small steps. Active deliveries finish safely at shift end.
5. **Chain integration.** Observe home depletion → shopping → store shelf depletion → clerk refill → phone request → distribution courier → store crate. Conserve food and preserve all in-progress tasks across save/load.
6. **Scale and specialization.** Larger maps, longer-distance travel and route testing, followed by different store types and prioritized shopping lists. Roads/vehicles remain a separate later system.

These are requested milestones, not claims that later behavior already exists. Keep future next-step options aligned with this chain.

## Foundation 104 observation guide

Open **Inspect Supply Chain Visuals Paused**. The lower-left tan room is a separate assigned home for all eight humans and contains a chest with six meals. Three blue stores retain their clerks/shelves/phones. The green distribution center retains two couriers and finite stock. Total food starts at 40 meals. This is a compact visual fixture, not the final residential district or a completed shopping simulation.

- Tan floors and a brown lidded chest: home/shelter-capable room.
- Blue floors and a blue double-door cabinet: store.
- Muted green floors and a braced green crate: distribution center.
- Cyan desk phone with dark handset, display and nine keypad buttons.
- A small pale inventory mark means stocked; a dark mark means empty. Yellow border marks selected storage/phones.

Click storage for its building type, inventory and existing controls. **Map markers & navigation** contains the legend. Building configuration changes update the floor palette without requiring a camera move. Existing saves acquire these presentation changes without a rules upgrade; the rules remain encounter-prototype-83.

Rooms without a store/center designation are shown as home/shelter-capable; that visual classification does not assert private ownership, assign residents or enforce entry permissions. Existing fixtures may still assign workers to workplaces; only the new preview deliberately separates them.

## Compatibility and validation limits

ProjectSettings/ProjectVersion.txt now targets **6000.6.3f1**. The project-configuration check rejects accidental target drift. The adapter continues using IMGUI and the existing editor-bundled modules; no new packages or input dependencies are needed. Open the extracted OutbreakRebuild folder with that exact editor in Unity Hub.

Headless core checks and UI API doubles can verify classification, finite initial supplies, roles/assignments, drawing commands, selection, refresh and save/restart behavior. They do not establish actual import, shader/rendering, screen layout or native compilation in Unity 6000.6.3f1. Native testing remains paused after Foundation 82.

Foundation 107: [store discovery and alternatives](STORE_DISCOVERY.md) now completes the local consumer discovery leg without preseeded shelf memories. Foundation 108 adds worker shifts, commutes and occupation outlines. Larger-map/distance testing remains the next requested milestone.

Foundation 108: [worker shifts and natural idle motion](WORKER_SHIFTS.md) adds clerk schedules, work/home commutes, off-duty shopping, occupation colors and energy-free within-cell weight shifts. Next extend the tested district and travel distances before store specialization.

Foundation 109: [larger district trial](LARGER_DISTRICT.md) exercises three separate households, three stores and near/far courier trips on a 64 × 36 map. This tests the integrated finite-food chain within existing routing bounds. Arbitrary city-distance routing and vehicles remain future work; store specialization is the next planned gameplay expansion.

Foundation 110: [city streets and longer routes](CITY_STREETS_AND_ROUTES.md) adds bounded extended pathfinding and the first seeded road/sidewalk/crosswalk generator. Buildings remain controlled fixtures; procedural building lots and pedestrian street use are next city steps. The user specifies future item categories as **supplies**, **medical** and **ammo**, rather than food subtypes. Defer that inventory expansion until its own increment; existing food and no-weapons-in-boxes rules remain.
