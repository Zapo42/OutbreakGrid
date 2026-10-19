Latest build: [Foundation 22 / sharing shelf reports](SHELF_REPORTS.md).

> Current milestone: prototype 6 adds small enclosed room discovery, persistent destinations/capacity, human door use, unsafe-room reassessment and evacuation. It does not complete global navigation, knowledge, building metadata or door durability/breaching. See SHELTERS_AND_DOORS.md.

> Latest correction: prototype 5 makes bitten humans seek safe local cover instead of confronting zombies. Recommended next milestone is shelter destination selection and door use; that remains pending. See INFECTED_ESCAPE.md.

> Current correction: prototype 4 adds awareness-limited approach, more direct cell steering and full-cadence presentation. It is still grid-based, not continuous navigation. See MOVEMENT_UPDATE.md for exact scope and checks.

> Latest user-selected milestone: Human reactions (prototype 3), including stress, cached local support, defensive shove, exact tick batches and a run-preserving Main Menu. See HUMAN_REACTIONS.md for provisional rules, checks and remaining work. Full Stage 2/3/4 requirements remain open.

> Current user-selected work: Encounter prototype 2 (including last-seen pursuit and a controlled corner scenario) is implemented ahead of full Stage 2 migration. See ENCOUNTER_PROTOTYPE.md for exact scope and test evidence. Stage 2 and the full Stage 3/4 requirements remain pending. Native Unity acceptance remains open.

# Rebuild work order

The goal remains the full observational Outbreak Simulator and compatible DragonGlass authoring workflow. Foundation 1 is the first completed core slice, not a reduced replacement scope.

| Stage | Concrete result | Gate before the next stage | Status |
| --- | --- | --- | --- |
| 0. Recovery | Preserve both latest source archives, asset identities and a hash inventory; record scope | Originals remain unchanged; no asset or user project is overwritten | Done |
| 1. Foundation | Isolated simulation, fixed clock, sparse grid, stable bodies, collision, deterministic continuation and minimal Unity shell | Executed C# invariants and 15,000-tick run; native Unity/Windows check remains open | Core checks passed; shell source prepared |
| 2. Content and movement | Legacy map/project migration; shared terrain/asset catalog; real pathfinding; doors/windows; spawn regions; authored room/building IDs | Import representative user maps, preserve pixels and metadata, save/reload, walk the same cells in Workshop and simulator; verify file browser in EXE | Next |
| 3. Encounter | Real human/zombie movement, local senses, occlusion, last-known locations, witness knowledge, stress, flight and hiding | Small controlled maps show each behavior; seeded runs reproduce; pause stops every system | Planned |
| 4. Combat and infection | Shove, stagger, grab/pile-on, weapons/projectiles, delayed turning, SPECIAL inheritance, emitters and four-part queues | Repeatable multi-body edge cases, exact weapon/strain data, correct sprite state, stable selection through turning | Planned |
| 5. Civilian world and laboratories | Population profiles, homes/jobs, inventory/needs, stores/couriers/DCs, LEO and full automatic research | Whole-day small-town scenario and exact save/resume of tasks, stock, experiments and ownership | Planned |
| 6. Survivors | Classes, base claiming, household planning, shelter/occupancy, patrols, resources, vehicles and expansion | Class-specific scenarios obey risk, storage, patrol, alert and base limits | Planned |
| 7. Product and scale | Finished start flow, shared authoring components, docking/accessibility, sprite animation, minimap/follow, profiling and Windows build | Native end-to-end author→save→browse→load→simulate→save→resume, plus agreed population/map benchmark and long-run memory checks | Planned |

Native Unity import and the Windows movement shell should be checked before substantial Stage 2 integration. The next deliverable is map/asset compatibility and a trustworthy navigation test, not another pile of partially connected features.

## Rules for each stage

1. Add a small vertical scenario that can be observed and reproduced. Use the authored test maps, not an uncontrolled generated town as the only evidence.
2. Make the underlying data and save/resume contract work before polishing its panel. Keep UI and simulation ownership separate.
3. Migrate one feature and its real artwork deliberately. Keep original files and unknown authoring information intact until the new exporter proves a round trip.
4. Run meaningful production-code checks for the affected behavior. Distinguish executed C# logic from actual Unity, native dialog and Windows player checks.
5. Keep the 100/250/500/1,000-cell and 1,000-body ladder. Report the systems enabled in each benchmark; movement-only timings are not full-game timings.
6. Track every new retained collection and allocation path. Keep diagnostic history bounded, but never discard authoritative tasks, infections or experiments merely to reduce memory.
7. Keep plain-English comments on new code lines. Avoid ambiguous names and the old large simulation/UI partial-class dependency web.

## Migration evidence

Recovered simulator: `OutbreakSimulator_Unity_Emitter_Animations_Weapons.zip`, revision 14, modified October 14, 2026 UTC. Recovered Workshop: `DragonGlassPixelWorkshop.zip`, revision 17, modified October 13, 2026 UTC.

The simulator uses a fixed 100×100 map and contains a roughly 154 KB main simulation source file. Its map loader and content code already contain useful regression scenarios; use their behavior as evidence rather than copying the old architecture wholesale. For example, legacy map code contains 80-HP doors and 35-HP windows, while the retained scope specifies 100 and 10. Resolve such differences against the accepted requirement when migrating each system and record the result.

Sprite sheets, `.dgproject` projects, modular laboratory overlays, building templates, terrain artwork, palettes and saved layouts remain in their original archives. Foundation 1 has not converted or replaced them. The inventory records exact paths and SHA-256 hashes so the next migration can prove preservation.
