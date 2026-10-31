# OutbreakGrid consolidation — graphics-first status

This snapshot precedes the user-selected [3x3 NPC-symbol increment](NPC_3X3_SYMBOLS.md), now implemented locally. The original tree below records the consolidation audit; it does not mean the later glyph work is still unstarted.

The three uploads are overlapping handoffs for the same project, not three separate applications. They have been reconciled locally on `local/single-agent-consolidation-20261030`. Compiled source: `1628f24715337fd8f72d75431236f80fc7470bfa`. No GitHub push, release, native Unity run or Windows EXE was produced.

“Implemented” below means present in the combined source with the stated hosted evidence. It does not mean native rendering/input or whole-day survival has passed.

## Graphics and UI tree

```text
Graphics & UI
├─ Visual direction                         APPROVED CONCEPT
│  ├─ Tiny symbols / early color LCD         approved reference
│  ├─ Restrained 16-color direction          approved look, not enforced
│  └─ 3×3 default                           proposed, not finalized
├─ Existing inspection UI                   IMPLEMENTED / HOSTED-CHECKED
│  ├─ Selected-item and collapsible panels   retained
│  ├─ Current / Old Test Scenarios           retained
│  ├─ Run, speed, Main Menu, Restart         retained
│  ├─ Optional selected-only H/F/E bars      edge/tiny-map fixes integrated
│  └─ Happiness, jobs, legacy upgrades       retained and checked
├─ NPC symbol renderer                     NOT STARTED
│  ├─ Human / infected / corpse masks        pending
│  ├─ Exact grids and zoom transitions       pending
│  └─ Occupation / selection readability     existing outlines to preserve
├─ Terrain and object pixel treatment      CURRENT PLACEHOLDERS
│  ├─ Houses / stores / storage / doors      identities exist; new art pending
│  ├─ Vehicles and paint colors             basic presentation exists
│  └─ DC order computer                     wall reservation, not an object
├─ Pixel pipeline / instrument-panel style NOT STARTED
│  ├─ Logical canvas / integer scaling      undecided, not implemented
│  ├─ Palette mapping and enforcement       pending
│  └─ Bitmap-style UI typography            pending
└─ LCD glow / post-treatment                DEFERRED / KEEP OFF
```

### What is finished here

The approved [tiny-symbol reference](VisualReferences/approved-pixel-scale-study.png) is preserved as a design reference, not copied into production sprites. Detailed modern character art was rejected. Neither exact3×3 masks nor a320×200 production canvas has been finalized; do not infer measured glyph dimensions or an exact pixel palette from a generated reference image.

UI-PRES-01/01A was already integrated before these uploads. It was not applied twice. Need bars clamp the entire stack and use allocated map-panel padding on tiny maps, retaining labels, colors, descending food and selected-only toggles. All newer happiness assertions, occupation identity, upgrade/restart handling and World recovery-area drawing survived consolidation. Existing motion/vehicle turns/door states and debug graphics remain presentation of authoritative simulation, not new decisions.

### What remains for graphics

The visible game has not yet been converted to the approved tiny-symbol art style. Production still uses the existing debug/body/terrain/object presentation. A restrained palette, pixel-stable scaling, native glyph masks, object art, bitmap-style UI and optional LCD treatment are future increments. Keep glow off until symbol readability is established. Native glyph metrics, density, overlap, actual layout and input remain unverified while Unity testing is paused.

### Suggested first graphics increment

1. Use one existing representative scenario to compare1–2px,3×3 and3×5 human/infected/corpse marks. Treat3×3 as a trial, not a locked contract.
2. Agree a minimal mask/palette/selection/occupation contract, then implement only that small NPC-symbol set. Preserve need bars and simulation state.
3. Extend terrain, storage/door/vehicle symbols in later small increments; typography and LCD treatment follow later. No art implementation was started by this compilation/status task.

## Other project areas

| Area | Completed in source | Still remaining / limitations |
| --- | --- | --- |
| Core NPC simulation | Human/zombie behavior, combat/infection/death, hunger/energy, finite food, homes/jobs/shifts, physical transfers, saved happiness, off-duty routines, known parks and social/familiarity state. | Personal-shopping empty returns, recovery/home priorities and next-day work still need investigation. Do not claim city supply or survival solved. Park metadata does not automatically provide NPC knowledge or facility bonuses. |
| R1 courier correction | Conservative pickup readiness, paid route recovery and unusable-claim release. Fresh generated trial now credits9 meals versus the earlier4, with courier health100 and one death. Controls12/20. | Fewer wasted claims is not global supply acceptance. Packaged R1 evidence worsens the older64×36 cartographer fixture from7 to9 deaths; that separate scenario was not freshly rerun in this consolidation. |
| F143 door execution | Clerk and household furniture tasks open closed internal doors as an ordinary action, then pay for later movement. Old106 gate/continuation remains covered. | Food still fails to reach some actors; opening doors alone does not solve task lifecycle or daily attendance. |
| World generation | Seeded streets, lots, setbacks, sidewalk entrances, six park templates, grouped multi-room houses, live6/8-shelf stores/stockrooms/phones, DC dispatch/warehouse and authoritative supply box. | New layouts currently use opt-in Python generation/import, not a rewrite of default C# city-generation menus. DC computer is a solid reserved footprint with no computer object or interaction. |
| Buildings/facilities | Houses, stores and DC infrastructure are materialized with current schema14 reader and focused tests. | Restaurant/bar and church remain prototype layouts/hints. No cooking/waiter/counter service, bar hours or church mechanics. Extra racks remain hints beyond the one authoritative supply box. |
| Vehicles | Existing driving, parking, courier/consumer cargo transfers, shopping and saved paint colors retained in tested source. | Broader vehicle types, bicycles, fuel/refueling and tiny-symbol vehicle art remain future work. No new tuning in this consolidation. |
| Tooling/persistence | One local source tree; verified bundles/patches; current runner; optional NPC save state, map14 room groups; UI explicit upgrades and restart checks; Python recipes and focused authoring tools. | Native Unity import/compilation/rendering/input/dialogs and Windows player execution remain paused/unrun. No EXE or fresh-environment restoration claim. |
| Workshop | Original data/history preserved. | DragonGlassPixelWorkshop application is not present/integrated; no import or migration authorized. |

## Fresh behavior trials on combined source

These four supplied trials start with203 finite meals each and run15600ticks (26 game hours). No geometry, stock, staffing or capacity was changed to improve outcomes.

| Trial | Credited meals | Deaths | Remaining food | Interpretation |
| --- | ---: | ---: | ---: | --- |
| Restocking | 9 | 0 | 124 | Physical stocking/food handling observed. |
| Shopping | 17 | 0 | 125 | Real shelf purchases and home resupply observed. |
| Delivery | 15 | 2 | 158 | Still unhealthy:156 meals remain at the DC. |
| Long shopping | 15 | 0 | 144 | Longer trips occur, but health/needs and next-day attendance remain incomplete. |

All four finals are byte-identical to the supplied authoritative F143 finals. Food ledgers show zero unexplained creation/loss. All28 replay checkpoint groups/840 exact continuation steps pass. This proves reproducible accounting/continuation, not healthy daily routines. The delivery deaths and missed next-day work are still concrete unfinished work. The known personal-shopping empty-return state is an investigation target, not a universal cause established for every failure.

## Fresh compile and validation

- Unified hosted runner:711 cases /743692 assertions; Windows ABI PASS; Legacy UI4899 and NewOnly3 PASS.
- Python:48 city-generator tests plus2 fixture tests PASS.39 existing recipe/map pairs reproduce canonically.
- NPC upgrade/restart UI75, World area/bed UI20, house infrastructure1264, store infrastructure857 and DC infrastructure93 assertions PASS.
- Standalone fixture-authoring project builds with0 warnings/errors.
- Current runtime Core/Contracts/Persistence matches NPC143 exactly; the12 narrow World paths match finald499d03. UI transfer1826dc6 remains integrated once. Unity6000.6.3f1, dependency versions, GUIDs/.meta and both permanent checkpoint targets are unchanged.

[Machine-readable results](../../harness/validation/consolidation-20261030/RESULTS.json) and original logs identify exact source and outcomes. Single-agent review was performed here; no replacement team or independent-review claim is made.

## Preservation and scope

All three outer ZIPs passed path/CRC checks and supplied integrity verification: NPC2795, graphics23 and World22 payload entries, plus nested store41/DC220 entries. Original bundles, patches and195 untracked legacy drafts remain separately preserved; they were not blindly overlaid onto modern code. Historical F142 diagnostic variants are not production changes.

The current request authorized compilation, reconciliation and a rundown. Old attached instructions are context, not authority for publication, new mechanics, renderer rollout or native testing. Current single-agent/local-only instructions supersede older team ownership and automatic publication procedures. No push/publication was made.
