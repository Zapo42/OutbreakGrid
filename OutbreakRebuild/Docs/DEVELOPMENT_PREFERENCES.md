# Current development preferences

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
