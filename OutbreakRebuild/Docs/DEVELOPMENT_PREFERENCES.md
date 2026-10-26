# Current development preferences

- Follow the updated [city life and transport workflow](CITY_LIFE_AND_TRANSPORT_PLAN.md). The user selected parking/traffic recovery first, with the large-city failed-shopping report next. Happiness, leisure, transport variety/fuel, pixel presentation and old-test menu organization are queued incremental requirements.

- Target **Unity 6000.6.3f1** for every future build. Preserve this exact ProjectVersion target and run the configuration check; never claim native compatibility without actual Unity validation.
- Follow [the home/store/distribution plan](HOME_STORE_DISTRIBUTION_PLAN.md): home supplies and consumer shopping first, occupation shifts/outlines next, then scale and store specialization. Build incrementally.
- Continue incremental builds and publish each completed build to GitHub with its ZIP download link.
- Include a few options for the next task after completing a build.
- Unity testing is paused. Foundation 82 remains the last user-reported working Unity checkpoint until the user explicitly resumes native testing and identifies a newer tested build. Routine acknowledgements do not change this status. Maintain [the validation record](BUILD_VALIDATION.md) and run appropriate automated checks.
- Keep weapons out of storage at this time. Storage supports food. Do not implement or keep suggesting weapon storage unless the user explicitly revisits that decision.
- Box-location sharing and a shortage overview are complete. The user then selected food deliveries between shelters. Supply priorities remain a separate possible milestone and have not been selected; player overview totals must not be disclosed to human AI.

- Foundation 93: user selected per-box donation controls. Food-only storage remains in effect; supply priorities are still a possible next step.

- Foundation 94: user requested couriers with assigned houses and source/destination boxes, multiple destinations and different travel distances. This build serves one destination per load in rotation; multi-stop loads remain a future extension.

- Foundation 95: user selected household management to inspect and change assigned residents. Keep permanent household membership distinct from physical visitors and courier delivery destinations.

- Foundation 96: user selected household food targets based on assigned residents. Away residents count toward their own household; visitors do not. Policies are opt-in and manual box overrides remain supported.

- Foundation 97: user selected courier schedules. Shifts are opt-in, follow the game clock and preserve active collection/delivery tasks at shift end. Existing hunger, energy, shelter and danger rules remain active.

- Foundation 98: user requested incremental store clerks → consumer shelves → secondary supply crate → requests to distribution-center couriers. First increment implements finite crate-to-shelf replenishment and consumer access; empty-crate requests and distribution-center dispatch are the next requested stages. Keep next-step options aligned with this supply chain.

- Foundation 99: user selected distribution centers before supply requests. Implemented stock-box designation and courier membership with a manual-route center → store → shelf demo. Store requests and automatic fulfillment remain outstanding requested supply-chain stages.

- Foundation 100: user requested empty-crate replenishment orders made by clerks at a phone block, plus collapsible encounter options and clearer inspection of clicked items/events. Keep clicked details first and observation tools easy to find. Courier request acceptance/fulfillment remains outstanding.

- Foundation 101: user selected courier fulfillment and reported missing health loss at maximum hunger/zero energy. Added automatic center dispatch, partial deposits/completion, claim release and combined-deprivation health loss/death. Preserve the collapsible selected-item-first UI.

- Foundation 102: user selected a multi-store scenario with several stores/couriers sharing finite stock. Added three stores, two couriers and eight humans, and corrected the order-list status/remaining quantity display. Existing survival damage and collapsible inspection remain active.

- Foundation 103: user selected shortage priorities. Dispatch compares the fulfilled fraction of reported requests, then age/ID; active trips and legacy rules remain unchanged. This does not use hidden live stock or guarantee equal lifetime allocations.

- Foundation 104: user requested a refined prompt before execution, Unity 6000.6.3f1, distinct building/storage/phone visuals, assigned-home food checks and intelligence-led shopping/return deposits, occupation outlines and off-duty home routines. First increment pins the editor target and adds visual distinctions/separate-home preview. Routine distribution-center wandering, shopping and new shifts are still upcoming; do not present them as fixed.

- Foundation 105: user chose home-supply inspection and consumer boundaries, then added 5x/10x/20x/50x speeds and optional selected-only Health/Hunger/Energy bars (H/F/E) with hunger displayed as 100 fed → 0 starving. All bars start off; toggles sit beside their stats. The unfinished “gunger should” fragment was explicitly withdrawn. Preserve these testing controls and countdown presentation in future builds. Deliberate store shopping, intelligence planning, occupation outlines/shifts, large-map scaling and store specialization remain pending.

- Foundation 106: user selected store shopping/home deposits with intelligence planning. Implemented consumer trips from checked home-box shortages to personally known store shelves, finite pickup/return/deposit, personal rations, saved tasks and box claims. Intelligence changes the remaining-food trigger; empty stock remains actionable for every intelligence level. Unknown-store discovery and in-trip fallback are not yet implemented. Keep off-duty worker shopping with the future shifts/commute milestone.

- Foundation 107: user selected unfamiliar-store discovery and alternatives after empty shelves. Added personal visible-entrance memory, bounded store investigation, same-trip alternative stores and unsuccessful-visit/retry delays. Preserve old rules and the ability to upgrade active grocery trips. Discovery remains local to visible or remembered entrances; no city-wide patrol/range expansion. Worker shifts, home commutes, occupation outlines, larger-map/distance trials and store specialization remain pending.

- Foundation 108: user selected worker shifts/home commutes/occupation outlines, then requested more natural idle movement without energy use. Added clerk schedules, courier/clerk work-home travel, post-shift work completion, off-duty shopping with retained job assignments, brown courier / horizontally yellow-over-blue clerk / gray off-duty / white consumer outlines. Idle motion is bounded visual weight shifting; canonical travel retains energy costs. Preserve the Foundation 82 Unity testing checkpoint, 6000.6.3f1 target, testing speeds and selected-only need bars. Larger districts and longer routes are the next requested stage, before store specialization.

- Foundation 109: user selected the larger district milestone. Added a 64 × 36 controlled district with three separately assigned homes, three familiar stores and two center couriers; longer near/far deliveries and resident groceries remain physical and finite. Keep city-wide routing and vehicles separate from this bounded scenario. Next planned gameplay stage is specialized stores and household shopping priorities.

- Foundation 110: user selected longer-distance routing and requested the start of procedural city generation (streets, sidewalks, crosswalks). Added bounded extended A* for safe assigned-human trips and seeded connected street geometry, retaining old saved rules. Future item/store categories are explicitly **supplies**, **medical**, **ammo**; do not substitute food subtypes. Street geometry comes before generated building lots/population and traffic.

- Foundation 111: user selected pedestrian sidewalk/crosswalk preference. Safe route planning favors those tiles while allowing fallback road crossings; threat-aware routes preserve escape choices and actual travel still costs energy. Existing saves retain old rules until explicitly upgraded. Continue toward generated building lots/entrances; future item categories remain supplies/medical/ammo.

- During Foundation 111 development the user reported a sideways deviation before a Generated City doorway. Reproduced the old local-search arc and retained A* scoring through the final approach; cover both that doorway trace and a clear straight approach in regression checks.

- Foundation 112: user selected generated building lots and sidewalk entrance connections. Geometry generation varies building footprints/placement by seed, preserves streets/crosswalks and supports 1–6 blocks per axis. The playable preview uses five separate homes, two stores, one center and five assigned humans; no generalized population or zoning model is claimed. Preserve the corrected approach routing, future supplies/medical/ammo categories and existing food-only storage.

- Foundation 113: user selected city setup controls for seed and map size. Add a separate generated-city form, 2–6 blocks per axis, small/standard/large presets, exact summary counts, paused/running start and existing retained-run protection. Keep population simple and explain the minimum city has workers but no pure consumers. Future supplies/medical/ammo categories and no weapon storage remain in effect.

- Foundation 114: user requested further city generation. Extend geometry incrementally with four street-facing entrance orientations, connected paths and role-specific footprint sizes; retain city setup, finite stock and separate home/job assignments. New generated maps use city-lots-2; saved materialized maps do not regenerate. Keep supplies/medical/ammo planned for later and food-only storage now.

- Foundation 115: user reported an intelligent NPC leaving an empty home but failing to reach a nearby stocked shop, and selected denser residential blocks. Reproduce hidden-entrance discovery failure; recognize visible outside shop frontage without hidden stock knowledge. Preserve old rule behavior until explicit upgrade. Add optional two houses per residential block with independent home inventories/residents and a 6×5 dense Large preset under the 64-building limit. Keep one resident per house separate from future household-size controls.

- Foundation 116: user selected varied street layouts, then requested visibly distinct crosswalks that future vehicles can cross. Add a Grid/Neighborhood choice, deterministic connected road backbone, T-junctions, short dead-end streets and pedestrian links. Crosswalks stay separate terrain, with zebra stripes on dark road paving, pedestrian use and RoadSurface classification for future vehicles; do not claim driving exists.

- Foundation 117: user selected neighborhood zoning and better store placement. Add an optional Neighborhood centers policy beside Fixed layout; optimize nearest-shop distances in blocks, seed ties, place distribution on a corner and orient commercial entrances toward served homes. Preserve both street layouts, housing densities, individual home inventories, job ordering and finite food; do not grant hidden stock knowledge.

- Foundation 118: user selected basic vehicle routes on roads/crosswalks. Add a small independent vehicle entity/route prototype with manual placement, destinations, stop/arrival, crossing yield, collision exclusion, persistence and a vehicle-only paused preview. Do not imply NPC drivers, boarding, cargo, lanes, fuel or traffic signals exist. New rule generation 91 preserves old-run opt-in.

- Foundation 119: user selected lane directions and following distance. Generated roads use deterministic right-hand traffic, legal junction turns and dead-end returns. Keep a one-cell gap on straight sections; preserve crossing yield and collision exclusion. Old rules-91 runs explicitly opt in; old unmarked maps keep free routing.

- Foundation 120: user selected intersection right-of-way. Marked connected junctions admit one vehicle at a time after checking its exit; oldest eligible approach wins, stable ID breaks ties. Save waiting ages, retain pedestrian priority and explicit rules-92 opt-in. Keep NPC drivers and traffic signals separate.

- Foundation 120 follow-up: user requested turning instead of instant 90-degree heading changes. Rotate the complete vehicle smoothly during playback while retaining cardinal simulation routes, exact paused/manual-step poses and rotated hit testing.

- Foundation 121: user reported grid-like generation and broken-looking road ends with thick sidewalks. New setup and primary generated previews default to Staggered: row-offset side streets and connected T-junctions. Derive building lots from the same row axes, narrow curbs around actual roads, and replace removed Neighborhood roadway with grass/caps. Preserve old saved terrain; do not call this curved or fully organic generation.

- Foundation 122: user selected varied block sizes/denser placement and reported poor maximum-setting performance, with much larger cities intended. Add one-to-four homes per block with safe compact presets, seeded wider Staggered spacing, and profile actual city workloads. Optimize allocation-heavy A* and repeated room/collision scans without skipping ticks or changing deterministic outcomes. Keep measured core improvements separate from unverified native Unity FPS; document remaining city/query limits.

- Foundation 123: user selected larger-city routing and higher building limits. Add a shared compressed sidewalk/crosswalk graph with live local movement checks, deterministic reconstruction and furniture invalidation. Raise new rules to 256 rooms / 32,768 floor cells and 2–10 blocks per axis; 8×8 four-home preview has 244 residents / 247 buildings. Keep rules-93 opt-in and legacy discovery caps. Preserve finite supplies and document that one center/two stores, walking carry limits and needs do not yet constitute balanced large-city logistics.

- Foundation 123 rendering follow-up: batch visible terrain pixels through a reused buffer instead of one native Unity call per pixel. Keep unchanged viewport uploads suppressed; headless API checks do not establish native FPS.

- Foundation 124: user selected NPC driving and vehicle deliveries. First increment is center couriers using assigned vehicles for phone orders: physical loading/boarding/driving/unloading/return, separate 24-meal shipment space, saved identity/phase/cargo, energy/needs and bounded shared road fields. Keep manual home-box routes/consumer passengers separate. Preserve finite food, source reserves, destination capacity, cancelled/full returns and incapacitated-driver shipment recovery. Do not imply vehicle combat/fuel or native Unity validation.

- Foundation 127: generated residents have authored familiar shop locations without hidden stock knowledge. Shopping rest breaks retain destinations and spend hunger; all actual movement still costs energy. Preserve older rules until explicit upgrade. See [city shopping continuity](CITY_SHOPPING_CONTINUITY.md).

- Consumer vehicle shopping should mitigate the food costs of long walks through faster travel, lower exertion and larger shopping loads. This is the next transport increment; preserve finite food and needs rather than disabling hunger while driving.

- Consumer vehicles need larger cargo storage and a separate finite shopping-cart/haul inventory, allowing multiple loading trips from store shelves to a parked vehicle. Keep personal rations distinct and physically unload vehicle cargo into home storage. See the cart requirements in the city-life workflow.

- Foundation 128: keep recent milestone previews directly accessible under Current Tests. Put historical fixtures under Show Old Test Scenarios; preserve their original launch behavior. Keep archived scenarios available for regression checks.
