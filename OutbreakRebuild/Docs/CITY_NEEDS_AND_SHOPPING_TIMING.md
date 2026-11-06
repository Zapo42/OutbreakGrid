# City needs and routine shopping timing

Fresh generated cities use seeded hunger0–20 and carried meals0–2 instead of giving everyone hunger10 and one meal. Neighbor pairs balance their totals; an odd final resident keeps10/1. Each owned pantry plus personal food still totals three meals. The seed stream is separate from movement/reaction RNG. No food is created, and initial population means remain hunger10/one carried meal. Existing recipes, roles, work schedules and food/movement rates remain.

Rules112 spreads healthy, nonhungry ordinary consumers' routine shopping reviews over300ticks (30game minutes), aligned with each resident's existing five-tick action opportunities. This is maintenance planning only. Actual hunger, injury and a personally known empty home bypass the review window. Workers keep their previous food preparation around shifts; schedules are unchanged. Urgent eating, danger, shelf access, physical pickup/deposit, movement cost and store stock remain authoritative Core behavior.

Old saves retain their original rules and recorded needs on load. While paused, rules111 offers **Enable staggered routine shopping**. Upgrade enables planning policy but never rerolls residents, food or RNG. Start a new city to get varied initial needs. Restart repeats the same seeded starting setup. The controlled Large City Shopping preview retains its authored original needs for diagnostic/legacy continuity; it is not the fresh-population diversity preview.

The preceding rules111 fix permits hungry empty-handed household pedestrians to seek their own meals despite another household pedestrian shopper. Vehicle/delivery/order claims stay exclusive. Four unchanged finite-food trials improved to zero deaths, but some survivors still reached hunger100: this is not a complete food-supply solution.

The storage traversal optimization reduces hosted Core allocation without changing decisions. Its frozen244resident synchronized-load workload fell from166016288 to75726528allocated bytes per250ticks (about54%); mean tick time was roughly14% lower on this host. Native frame rate, rendering/input and GC stalls remain untested under the native-testing pause. High simulation speeds can still accumulate work faster than the host can execute it.

Longer profiling identified a blocked store-entry pileup: a courier tried many interior cells after the only interior entrance cell filled, spending about4.3seconds in one decision. Long routes now reject a full/zombie goal or a physically sealed recognized room before exhausting the search frontier. Existing actors may still leave a full doorway. Another usable door, newly freed entry or actor already inside retains the original route search. No failed-route cooldown, path budget, food grant or decision priority was added.

In the same244resident six-hour run, the complete final save is byte-identical before/after this optimization. Mean Core tick wall time18.93→9.21ms; worst4.509seconds→123.97ms. Total process CPU (including identical observations/encoding)70.56→34.42seconds. Host/JIT timings vary; these numbers do not establish native FPS or eliminate every stall. Frozen54/244resident rules111 workloads also retain exact final hashes.

Fresh-city shopping starts in the busiest25tick window dropped111→12 when comparing homogeneous111 against diverse/staggered112. This is a separate behavior comparison: geometry/food budget remain equal, arrival timing intentionally changes. Do not confuse it with the byte-identical route optimization comparison.

Manual checks for the published source (native execution remains paused for the agent):

1. Choose **Inspect Expanded City Paused**, or set up a NEW generated city with seed214,8×8blocks,four homes/block,staggered streets/neighborhood centers. Pause immediately and inspect several residents: hunger should vary0–20 and carried meals0–2; pantry plus personal meals remains3 per household. Restart should reproduce the same residents/needs. Loading an old save does not randomize its existing needs.
2. Watch the first several game hours at1×, then the same city at a higher speed. Compare the timing of food errands and pauses/lag; note selected speed, population, zoom and game hour. Core allocation reduction does not establish native frame-rate improvement. Existing WASD/arrows, zoom tiers, menu/restart and selected H/F/E remain.
3. Load a known rules111 save while paused. Confirm the explicit “Enable staggered routine shopping” upgrade is available. Upgrading changes routine policy, not stock or initial needs. Hungry/empty-home residents should seek food without waiting for a routine review slot. Follow a resident all the way through shelf pickup, eating and home return; report any critical hunger with food visibly inaccessible.

Next authorized work: finish remaining shelf/consumer food-access bottlenecks, then stagger meaningful work/eat/rest shifts. Do not add pointless movement or hide starvation by spawning food.
