# Current development preferences

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
