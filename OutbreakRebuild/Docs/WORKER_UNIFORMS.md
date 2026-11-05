# Existing-worker uniforms and color scope

The user selected two existing job uniforms:

- **Couriers:** brown clothing. The close sprite chooses reference wood #79583D or light tan #BA975F against its current terrain, retaining brown-family identity without sacrificing contrast.
- **Clerks:** adaptive blue shirt/sleeves/legs, with a pale-yellow #E0C87A apron. The apron occupies the center bib pixel and three torso pixels.

The same shared5x5 human silhouette, face, walk/facing frames and sixteen occupied cells remain. These are uniform inks, not new geometry or markers. Uniforms read existing permanent AssignedOccupation, so employment remains visible off duty; existing gray/duty occupation bands continue to show duty state. Ordinary residents remain unchanged. At3x3/1x1 all humans keep species-blue ink and existing worker bands, rather than squeezing clothing details into a tiny dot. Death/zombie states override living uniforms, and hit/bite flashes replace every occupied cell.

Current Tests > **Inspect Worker Uniforms Paused** reuses the existing finite worker-shift fixture: courier, clerk and resident. It starts at07:54 paused; Run or step uses existing shift behavior, not new uniform-driven AI. Restart restores the same authored data. [Actual-draw sheet](VisualReferences/worker-uniforms.png) · [SVG](VisualReferences/worker-uniforms.svg), enlarged/unit pitch on grass and home floor. Technical plots, not Unity screenshots.

## Enough color coding

After this increment, no additional color layers or cosmetic passes are planned by default. Existing cues cover species/life state, jobs, selection, needs and building/storage states. There is no need for a unique color for every behavior, stat or activity. Leave approved doors/storage alone.

Future conventions are **notes only**: LEOs may wear dark blue; medics may carry a green-cross symbol. Neither role, its behavior, marker nor data enum is implemented by this increment. Symbols can supplement color when those actual gameplay roles are authorized.

The next recommended work is the already documented shopping/survival problem, starting with a reproducible empty-handed personal-shopping return. Preserve finite inventory, paid movement/needs and legacy save rules; do not conceal failures by adding food. That is a recommendation for the next selected task, not a change implemented here.

Validation uses the sequential hosted runner and exact uniform cells/colors, smaller-tier species identity, walking/facing/flash/death/zombie handling, real job-to-render wiring and unchanged state/restart checks. Existing off-duty/on-duty role cases remain. Native Unity testing stays paused; no Windows player claim. See BUILD_VALIDATION.md and harness/build-log.md.
