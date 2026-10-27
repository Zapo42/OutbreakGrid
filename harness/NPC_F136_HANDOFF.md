# NPC-F136 — personality-based park choices

Baseline: 93a8b5deb1c87b4b79e79c1d03446b896438321b (NPC social greetings). Dedicated NPC developer, sole writer in isolated npc/f136-park-preferences. RESTORE42 stays fixed.

Approved scope: use existing charisma-based solitude/company and indoor/outdoor preferences for known-park selection and outing frequency. Only actual current observations may describe crowds. Preserve saved targets, path/energy limits and urgent priorities. No persistent crowd reports, relationship system, new park generation, UI/art, energy bonuses or releases.

Ownership: new Core/ParkPreferences.cs; narrow HumanLeisure.cs target scoring/wait/validation integration; World.cs feature gate/restore/capture and Encounter.cs version alias; NPC tests/registration and this handoff/log. No actor contract fields are planned. Shared version/gate changes are delivered for project-lead integration review; no overlapping writers.

Dependencies: existing park familiarity, safe-leisure eligibility, authoritative visibility, happiness preferences and off-duty effective occupations. Risks: hidden crowd knowledge, population-dependent search cost, route oscillation, stale saved targets/clocks and old-rule behavior. Acceptance: company/solitude produce different choices with genuinely visible people; walls/range/dead/riding/unknown locations do not grant knowledge; unknown occupancy stays unknown; indoors/outdoors change bounded wait times; busy parks can defer solitude outings; destinations stay fixed once chosen; physical travel remains paid and needs/work/danger win; exact saves and independently generated rules104 continuation pass. Run focused cases, relevant leisure/park/social regressions and full core suite. Native Unity testing remains paused.

## Behavior

- Existing charisma preferences are unchanged:0–4 solitude/indoors,5 neutral,6–10 company/outdoors. This task changes decisions rather than adding new happiness or energy rewards.
- At the start of an outing, compare reachable known parks within the existing12-cell entrance radius and20-step initial route budget. Prefer an observed favorable company state, then unknown company state, then unfavorable state; route distance breaks equal preference scores. Neutral NPCs retain nearest-park selection. Known parks stay ahead of generic walk targets.
- Only living on-foot humans within the NPC's actual12-cell sight radius and line of sight can count. A person must also be within the existing3-cell happiness company radius of the park and visible from that site. The park itself must be currently visible and passable before company can affect its score. Counts cap at4. Walls, vehicle riders, dead people and far/unknown sites grant no crowd knowledge.
- An unseen park has SocialContribution0 and Visible=false, not a claim of emptiness. A visible park with no visible company favors solitude; visible company favors social NPCs. There is no persistent crowd memory in this increment. A closed home door can therefore leave park company unknown, preserving a nearest-known fallback.
- If the best reachable known park is visibly busy and the resident prefers solitude/indoors, defer the outing and wait at home. Existing home/need/work decisions continue normally.
- Environment preference adjusts the existing occasional outing timers: indoor preference initial600ticks/cooldown1200; neutral300/600; outdoor150/300. Add the existing stable identity modulo120 stagger. These timers apply to ordinary free-time walks too; all existing safety, health, food, energy, shopping and occupation eligibility still apply. No RNG draws are introduced.
- Crowd changes do not retarget an active outing. Existing outbound/pause/return state and actual paid movement remain authoritative.

## Inspection and save state

`ParkPreferenceKnownBy(actorIndex,x,y)` returns a detached present-observation snapshot: Available, Known, Visible, VisiblePeople, SocialContribution, InitialDelayTicks and CooldownTicks. The two delay values exclude the identity stagger. Available describes feature/observer availability, not permission to start leisure. The snapshot never starts an outing, modifies a target or records knowledge. Present observations must not be displayed as a historical reason for a previously selected target.

No actor or map fields were added. Existing saved target, phase and nextLeisureTick preserve decisions and waiting. Rules105 gates new behavior; SocialInteractionsVersion aliases104. Old rules retain old target scoring/timers and the original clock bound until explicitly upgraded. Rules105 allows the longer indoor cooldown; validation retains all prior identity/geometry checks. Upgrading leaves an already saved destination/wait in place; future newly scheduled choices use preferences.

Runtime: one reused eight-entry score buffer per world; at most eight local observation scans when actually choosing a new destination, bounded visibility rays and four-person count caps. Destination BFS reads the cached scores, avoiding actor scans per path cell. Read-only inspection computes only the requested known site's present observation. No ongoing per-tick population pass or persistent crowd cache was added. No native performance claim is made.

## Validation evidence

- Focused/regression suite PASS68 cases /6050 assertions, /tmp/npc136-focus.log. Includes11 new preference cases plus existing leisure, happiness, park visits, off-duty and social interactions.
- New cases cover opposite and neutral selections, unknown hidden crowds, dead/riding people, sight/familiarity/routing limits, initial and completed-trip clocks, crowded-park deferral, stable destinations and paid steps, needs/new shifts, read-only inspection, exact route/long-cooldown saves, incompatible clock rejection and legacy104 continuation.
- Existing baseline-wait test explicitly uses neutral charisma; the passive happiness comparison explicitly runs happiness-generation101 versus100 to isolate its original increment. Assertions are retained.
- Independent baseline: previous source93a8b5d, HumanLeisureTest with familiar parks8,9 and13,9, unscheduled initial wait,800ticks. SHA2565FFBC2348BBD7591FBA3CFBF614AFB4CB4BC25A924C89761479912918E699ED6 matches new code under rules104. Probe /tmp/npc136-golden; log /tmp/npc136-golden.log.
- Full core suite PASS, exit0, **682 cases /741774 assertions**, tested implementation77b8a31214622a42f6e6631d4d2c8efdbdb78b2e. Report /tmp/outbreak136-npc-results/FOUNDATION_CHECKS.txt. Command: `DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak136-npc-results`. Log /tmp/npc136-core.log.
- Native Unity, Windows player, general UI/release checks and Mono NOT RUN. Native testing remains paused; no independent specialist review is claimed.

## Lead integration

Integrate implementation77b8a31 plus the final handoff/log commit atop npc/f135-social-interactions and review shared World/Encounter gate/version wiring. The branch contains earlier pending happiness/off-duty/park/social work; avoid duplicate cherry-picks. Preserve RESTORE42, the Foundation82 native checkpoint and Unity6000.6.3f1.

World/UI agent still supplies actual park venues and TryRememberPark from real observation/authored familiarity. The new read-only snapshot can support selected-NPC inspection and a controlled two-park preview with an open sight line; this task creates no scene, menu, park artwork, animation or published build.

Add explicit ParkPreferencesEnabled upgrade controls, include SocialInteractionsVersion104 along with previous pending101–103 aliases in RestartScenario's legacy-energy exclusions, update labels, and run general UI/integration checks before publication. Do not silently upgrade older runs.

Next NPC options, awaiting user selection: remember recent observed park crowds; recognize familiar NPCs for greetings.
