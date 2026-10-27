# NPC-F135 — free-time social greetings

Baseline: 11af94498007b095acd019b380d96b42df2ea7d1 (NPC park visits). RESTORE42 remains fixed. Dedicated NPC developer is the single writer in isolated npc/f135-social-interactions. No concurrent shared-file writers.

Approved scope: brief local exchanges during safe consumer free time, charisma-based willingness/reward, saved per-person cooldown and an inspectable recent result. No relationship system, conversation animation, remote social planning, facility generation or energy bonus.

Owned files: new Core/SocialInteractions.cs; minimal Happiness.cs death/turn cleanup; Contracts/Documents.cs optional NPC fields; World.cs tick/gate/restore validation; Encounter.cs generation alias; new SocialInteractionChecks.cs and test registration; this handoff and build log. Shared contract/tick changes require lead review at integration. UI/art/world/release remain with the other agent.

Dependencies: existing happiness, off-duty effective occupations, leisure safety and line-of-sight. Risks: duplicate pair rewards, urgent task eligibility, hidden actor knowledge, invalid saved history and legacy behavior. Acceptance: only safe nearby visible willing on-foot consumers greet; no compulsory movement or delay; no cooldown farming; actual mood bounds; work/needs/danger win; saved continuation is exact; invalid/legacy state rejected; inspect APIs do not mutate state; older rules103 match an independently generated baseline. Run focused cases and full core checks. Native Unity testing stays paused.

## Behavior and read-only API

- At most once per existing 120-tick identity-staggered opportunity cadence, a free NPC checks for a willing visible on-foot partner within Euclidean radius two. Selection prefers distance, then stable identity. No RNG draw or path change is added.
- Both must be full-health, uninfected, calm, sufficiently fed/energized effective consumers, free of shopping, delivery, commute, searching and shelter-departure obligations. They must be at their assigned home in free time or pausing during an existing outing. Actual observed danger vetoes either person's participation. The closest candidate's own safety is checked before consenting; if unsafe, the initiating opportunity is skipped rather than searching through hidden observations.
- Charisma 0–4 declines optional greetings, 5 accepts with +1 happiness, 6–10 accepts with +2. Each actual gain is clamped at +50. Solitude NPCs retain existing ambient solitude/company preferences. These small, provisional greeting rewards supplement existing mood pulses; no energy reduction is introduced.
- The exchange is instantaneous. It neither holds movement nor lengthens a leisure pause. Every actual step retains normal energy costs. Off-duty clerks/couriers can participate without losing their permanent occupation; on-duty workers and unfinished cargo remain ineligible.
- Both participants atomically receive a 240-tick cooldown (24 game minutes), preventing a second participant's staggered pulse or another nearby NPC from awarding another greeting. No cooldown or happiness changes occur while paused.
- `SocialInteractionKnownBy(actorIndex)` returns a detached value with Available, Recent, PartnerId, HappinessGain and CooldownRemaining. It never mutates simulation state. Partner/gain are presented for 120 ticks after the exchange, then hidden from the recent snapshot while the cooldown continues. The reward is a separate historical event, not included in `HappinessKnownBy().Change`, which still describes passive per-pulse contributors.

## Save state and compatibility

New optional fields: nextSocialTick, lastSocialPartnerId, lastSocialGain. The next deadline also dates the last exchange; no redundant clock is stored. Expiry clears the three fields together. Death/turning clears the affected person's state. A survivor's memory of a real greeting remains until expiry even if their partner dies, turns or walks away; do not require current reciprocal proximity or consent when validating historical state.

Rules104 gates the behavior and state; ParkVisitsVersion aliases103. Rules103 and older are not automatically upgraded. Validation rejects incompatible owners/rules, invalid actual gains, unknown/self partners, and expired/out-of-bound clocks before publishing a restored world. No registry, map, relationships or item fields were added.

Runtime work: one allocation-free cooldown pass over actors per tick; at most one partner scan per eligible identity-staggered actor every120 ticks, with distance rejection before eligibility and line-of-sight checks. Worst-case partner searching is quadratic at that bounded cadence. No new NPC performance threshold or native performance result is claimed.

Independent baseline: pre-feature source 11af944, two co-located home consumers (charisma8/5), 300 ticks, SHA256 DC6445178D8D4396DBC7AC42D10FD19E796FBB05495E035FB92068BFBE5CD73C. Probe /tmp/npc135-golden references only the prior source; output /tmp/npc135-golden.log. New code matches that exact rules103 continuation.

## Validation

- Focused suite: PASS, 14 cases / 875 assertions; /tmp/npc135-focus.log. Covers pair rewards/cooldown, all personality combinations, needs/injury/infection/fear/unresolved freeze, range/occlusion, visible versus hidden zombie danger, walkers/riders, park pause and energy, off-duty/on-duty workers, stable ties/clamping, read-only recent snapshots, exact saves across expiry/new exchanges, death/turn history, invalid state and independent legacy continuation.
- Additional comparison strips only mood/social history to prove otherwise exact movement/needs/inventory/RNG continuation for 300 ticks.
- Existing HappinessBox explicitly disables the new greeting feature to continue isolating passive happiness formulas. SocialPair explicitly enables rules104. Existing assertions are retained.
- Full core suite: PASS, exit0, **671 cases / 740859 assertions**. Tested implementation commit3bf486a391b67463b7364e96b6f5cf0bf13aa79d; report /tmp/outbreak135-npc-results/FOUNDATION_CHECKS.txt. Command: `DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak135-npc-results`. Log /tmp/npc135-core.log.
- Native Unity, Windows player, general UI/release checks and Mono: NOT RUN. Unity testing remains paused; no independent specialist review is claimed.

## Integration instructions

Integrate implementation3bf486a plus the final handoff/log commit atop npc/f134-park-visits. This branch includes the pending earlier happiness/off-duty/park integrations; avoid duplicate application. Review shared optional actor fields, generation104 gate, world post-movement tick hook and restore validator. The NPC source keeps Unity6000.6.3f1 and all packages/assets unchanged.

The world/UI owner can display SocialInteractionKnownBy beside the existing happiness contributors, and later add greeting presentation independently. A deterministic integration preview is two willing consumers at their own shared home during free time; park meetings require real registered park sites from the prior handoff. No new scene/menu, animations or published build is provided by this NPC task.

Add explicit social feature upgrade controls with SocialInteractionsEnabled. Include ParkVisitsVersion103 plus the pending HappinessVersion101 and OffDutyConsumersVersion102 in RestartScenario's legacy-energy exclusions; update build/menu labels and run general integration checks before a release. Older saved runs stay on their own rules until an explicit upgrade. RESTORE42 remains at 49fc25b; native-tested checkpoint remains Foundation82.

Potential next increments (user selection required): personality-based park choices; brief social activity presentation (world/UI agent); later relationship familiarity as its own NPC increment.
