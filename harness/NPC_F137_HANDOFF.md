# NPC-F137 — recognize familiar greeting partners

Baseline: 8c4f3504adfd65f254517c5987b8a9ada3cca38c (NPC park preferences). Dedicated NPC developer is sole writer in isolated npc/f137-familiar-greetings. RESTORE42 remains fixed.

Approved scope: familiarity learned from actual successful greetings; prefer familiar visible willing partners in the existing local greeting range. Save a bounded history of stable person identities/last greeting ticks; expose detached/read-only personal knowledge. Preserve current rewards, cooldowns, willingness, needs/work/danger priority and movement. No relationships, friendship scores, remote contact/location tracking, new movement or UI/art/release work.

Owned files: new Core/SocialFamiliarity.cs; narrow SocialInteractions.cs ranking/record hooks; Happiness.cs human cleanup; ShelfMemory.cs detached arrays; Contracts/Documents.cs optional bounded records; World.cs feature gate/capture/restore validation; Encounter.cs version alias; NPC tests/registration and this handoff/log. Shared contracts/version/gates await project-lead integration review. One writer per file; no main/shared checkout edits.

Dependencies: existing social greetings, stable identities, occupation/leisure eligibility, visibility and deterministic persistence. Risks: invented acquaintances, hidden actor knowledge, stale aliasing, unstable eviction, cooldown farming or legacy changes. Acceptance: actual mutual greetings establish knowledge; memories survive cooldown and saves; familiar visible willing nearby partners rank first, then distance/identity; hidden/far/dead/turned/unwilling/busy people cannot be greeted; deterministic bounded memory and validation; death/turn clears the owner without erasing a survivor's history; current rewards/paid movement/priorities unchanged; old105 independently recorded continuation matches. Run focused/regression and full core suites. Native Unity testing remains paused.

## NPC behavior and knowledge

- A successful existing greeting records both participants' stable identities and the actual Tick. Merely seeing, living near, working with or remembering a person's location grants no acquaintance. Existing older social history is not backfilled; after an explicit upgrade, subsequent real greetings establish the new records.
- Each living human retains at most eight FamiliarPersonRecord values, sorted by actorId. Repeat greetings update lastGreetedTick without duplicating or allocating another array. A new contact at capacity replaces the least recently greeted contact; equal timestamps evict the largest stable identity. New-contact growth/replacement allocates only a bounded array.
- Familiarity persists independently of the240-tick greeting cooldown and120-tick recent-event snapshot. No decay timer or new per-tick scan was added. Memory is history, not a live status feed.
- Existing partner eligibility and range remain authoritative: visible, willing, on foot, within two cells, safe free time, with needs/work/shopping/infection/injury and danger checks intact. Among eligible visible candidates, familiar beats unfamiliar; equal familiarity uses distance then stable identity. Familiarity never makes an unwilling NPC consent or sends the initiator to a remembered location.
- The selected partner's own danger veto remains in place, as in the prior increment. If they perceive danger, that opportunity is skipped; this change does not search through hidden danger information to find alternative partners.
- Rewards are unchanged(+1 neutral/+2 social, clamped at50); familiarity is not friendship, affinity, trust or a larger mood bonus. No movement, greeting cooldown, hunger, energy or inventory changes were introduced.
- On death/turning, the affected owner loses human familiarity. Other people's genuine historical memories remain, including a remembered person who later dies, turns or leaves view. Greeting eligibility still excludes dead/turned partners; retaining identity history is not a remote health report.

## Read-only API and persistence

- `RecognizesPerson(actorIndex, personId)` asks only that actor's stored acquaintance history. It takes a stable person ID, not an actor array index. It neither queries nor reveals that person's current location/health/job. Invalid/unknown person IDs return false; invalid observer indices throw.
- `FamiliarPeopleKnownBy(actorIndex)` returns a detached array of actorId/lastGreetedTick records. Older rules or unavailable owners return an empty array. ActorAt, Capture and Restore boundaries also detach the optional array. These snapshots contain no current partner data.
- New optional ActorState.familiarPeople uses FamiliarPersonRecord value records in Contracts/Documents.cs. Null is canonical empty knowledge. Validation checks the feature gate, living human owner,1–8 entries, ascending unique nonself identities that exist in the save, and positive past/current encounter times. A remembered person's present species/health/proximity is not a historical validity requirement.
- Rules106 gates state/ranking; ParkPreferencesVersion aliases105. Older runs keep old nearest/identity greeting selection and do not learn this memory until explicitly upgraded. Existing saved targets and clocks remain in place.

## Validation evidence

- Focused/regression PASS78 cases /7198 assertions, /tmp/npc137-focus.log. Ten new familiarity cases plus existing leisure/happiness/park/off-duty/social behavior.
- New cases cover actual mutual learning versus unwilling company; retention after cooldown expiry and repeat-contact refresh; familiar-before-distance choice and distance/identity ties; no bypass of willingness/range/walls/injury/hunger; capacity/oldest eviction and deterministic ties; detached ActorAt/API/Capture/Restore; exact saves across500ticks of future choice/refresh; unchanged pair rewards, needs, inventory, energy, movement and RNG after stripping only familiarity; death/turn owner cleanup/survivor history; corrupt/legacy state and independent old105 continuation.
- Prior social normalization helper strips this new social-only record when comparing current social behavior against pre-social rules103. Its original assertions are retained.
- Independent pre-feature baseline: source8c4f350, two home consumers charisma8/5,500ticks, SHA2568193FC4D32CDD150E71205712FC426B78E293D7F49535536C8C83AEE4E7775EE. Probe /tmp/npc137-golden references only prior source; log /tmp/npc137-golden.log. New code matches exact rules105 continuation.
- Full core suite PASS, exit0, **692 cases /742922 assertions**, tested implementation0c249f5487f5c24bdd31f1e0c14a2388e7633891. Report /tmp/outbreak137-npc-results/FOUNDATION_CHECKS.txt. Command: `DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak137-npc-results`. Log /tmp/npc137-core.log.
- Native Unity, player, general UI/release checks, Mono and independent specialist review NOT RUN. Native Unity testing remains paused; no published build or native performance result claimed.

## Integration

Integrate implementation0c249f5 atop npc/f136-park-preferences, plus the final handoff/log commit. Review shared Contracts fields, array ownership and World/Encounter rules106 capture/restore/validator. The branch includes earlier pending NPC increments; avoid duplicate integration. Preserve RESTORE42, Foundation82 native checkpoint and Unity6000.6.3f1.

The world/UI owner can display FamiliarPeopleKnownBy with the existing recent greeting snapshot; show last actual encounter time rather than invented friendship or current partner status. A controlled preview can establish an actual two-person greeting, wait for cooldown, and add a closer unfamiliar willing NPC: the observer prefers the original acquaintance if still visible and eligible. This task adds no presentation, menus, scenes or published build.

Add explicit SocialFamiliarityEnabled upgrade controls and ParkPreferencesVersion105 to RestartScenario legacy-energy exclusions along with pending101–104 aliases. Update labels and run general UI/release integration checks before publication. Main is untouched.

Possible next NPC increments, requiring user selection: approach a visible familiar person during free time; remember recently observed park crowds.
