# NPC-F138 — daily-routine diagnostic setup

Baseline:0aa8ec34aa168e4797bfcd672053755dcdfaa034. Sole NPC writer, isolated local npc/f138-daily-routine worktree. User explicitly requested local-only delivery and coordination with the lead cartographer through a relay brief; no connected cartographer agent exists. See NPC_F138_CARTOGRAPHER_BRIEF.md.

Scope: standalone hosted C# observation runner using existing controlled scenarios and a generated-city full-day baseline, plus optional cartographer-supplied save input. No production behavior, map, UI, schema, editor version or release changes. Owned files: Tests/NpcDailyRoutineChecks.cs and .csproj; this handoff, local workflow, cartographer brief and build log. No shared production writers.

Acceptance: controlled work-start/home-return, physical shopping/home deposit, physical leisure/home-return; deterministic short replay checkpoints; inspectable per-actor sampled timeline, long active-task inactivity and rapid room transitions marked for review; day-scale generated-city observation; explicit distinction between healthy waiting and suspected stalls. Supply shortage/death is reported, not silently repaired or called navigation failure. Supplied-map integration remains pending the cartographer's reply.

## Use

From the project root:

`DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/NpcDailyRoutineChecks.csproj -- /tmp/outbreak138-routine-results`

On another machine, use its installed .NET8 SDK with `dotnet run --project OutbreakRebuild/Tests/NpcDailyRoutineChecks.csproj -- <output-directory>`. Supply a cartographer save as the second argument after the output directory to observe it for14400ticks without upgrading its rules. This accepts an existing .obsave2, not a bare map lacking NPC assignments. Keep map editing/roles with the cartographer and behavior interpretation with NPC ownership.

Outputs: initial/final saves, sampled CSV per scenario and DAILY_ROUTINE_REPORT.txt. At each scenario midpoint a20-tick replay window must match a restored copy exactly, then observation resumes from the pre-replay save. No simulated progress is counted twice. CSV samples every30ticks plus room transitions/deaths; counters update every tick. Reported hunger is the internal0-fed/100-starved burden, not the inverse display bar. Dead actors have needs reset by existing cleanup.

## Baseline findings

- Controlled workers: work attendance and home returns PASS for courier/clerk,1000ticks starting at540.
- Controlled generated-city shopper: actual shopping/home deposit PASS over1800ticks.
- Controlled leisure: physical departure/return PASS over500ticks.
- Generated city: seed214,4x2blocks,2homes/block, staggered streets, neighborhood-center zoning,10NPCs,14400ticks(one simulated day). Observation/replay completed. Daily routine health is NOT accepted: four deaths(ids2,3,6,7), hungry survivors and24meals remaining of78initial meals.54meal events observed. This alone cannot establish starvation cause, inaccessible stock or navigation failure; no gameplay fix is bundled.
- Repeated room crossings were flagged for actors5/7 and the functioning controlled courier. Therefore a review flag is intentionally not a bug assertion. No long stationary healthy travel task exceeded300ticks in this baseline. Shifting/looping movement can still require CSV/map inspection.
- The generated courier and both clerks have explicit07:00–19:00 shifts. Both clerks recorded work attendance but no home return before death. This is a concrete integrated work/needs review target, not evidence that they were authored as always-on workers. Scheduled work/home coverage passes in the small controlled case.

## Evidence and limits

Hosted diagnostic command above: PASS exit0 for observer invariants, controlled goals and deterministic replay windows. Report /tmp/outbreak138-routine-results/DAILY_ROUTINE_REPORT.txt; log /tmp/npc138-routine.log; initial/final saves and CSVs alongside report. A local report copy is tracked at harness/NPC_F138_BASELINE_REPORT.txt.

This is test-only: production Assets diff against baseline is empty. The existing692-case core suite was not rerun because no production code or its test entrypoint was changed; prior NPC-F137 evidence remains distinct. Native Unity/player, performance certification and cartographer-supplied-map acceptance NOT RUN/pending. No claim that the city is healthy, that every NPC must take leisure, or that finite supplies are sufficient for a day.

## Integration and coordination

User has been given the copy-paste cartographer brief; no direct contact or acknowledgement from that agent is claimed. Await their map/save, source revision, seed, assignments, intended schedules/routes and known obstructions. Do not edit their map or general UI files without an ownership handoff. Run their supplied save and compare to this recorded baseline before diagnosing behavior versus geometry.

All work remains local. Updated agents/simulation.md and NPC_LOCAL_WORKFLOW.md record the user's delivery instruction. Do not push this branch. Produce a source/evidence transfer package only on request, for the user to relay to the lead designer agent. RESTORE42, Unity6000.6.3f1 and Foundation82 native checkpoint stay unchanged.

Recommended next step: investigate hunger with food remaining, starting with the cartographer's intended shifts and stock accessibility. This is a finding to investigate, not an authorized automatic redesign.
