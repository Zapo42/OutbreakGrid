# NPC-F133 — off-duty consumer routines

Status: NPC implementation and full core validation complete. Ready for project-lead integration.

Baseline: `9c8c41440e4aa2aef176b5038eabba596352b4d6` on `npc/f132-happiness`. RESTORE42 remains fixed. This increment builds on the pending Foundation 132 source; it does not claim the other agent completed a release.

## Scope and acceptance

Employed NPCs temporarily act as consumers outside their shifts while retaining permanent employment, home and schedules. Off-duty shopping and leisure must work for clerks and couriers. New shifts interrupt optional leisure and restore work routines. Finite existing loads must complete/return safely before optional activities; do not erase jobs or discard stock. Preserve observed knowledge, normal movement energy, urgent-needs priority, deterministic saves and old rules. No UI, artwork, city generation, tooling or release work.

## File ownership

- Dedicated NPC developer (completed the implementation directly): NPC production changes in WorkerShifts.cs, HumanLeisure.cs, CourierSchedules.cs, vehicle/store/shopping/restocking routines only as needed; sole writer for minimal World.cs/Encounter.cs version-gate integration. Isolated worktree `/tmp/outbreak133-npc`, branch `npc/f133-off-duty`.
- Dedicated NPC developer: new OffDutyConsumerChecks.cs, FoundationChecks.csproj and Program.cs. Specialist work was interrupted before any implementation; no overlapping edits occurred.
- Dedicated NPC developer: review, NPC-test integration/compatibility corrections, core validation, this handoff and NPC build-log entry. No overlapping writes with specialists.
- Other project lead: shared-file review, UI/occupation presentation, general-runner validation and release publication.

## Risks and validation plan

Keep assigned job identity separate from effective consumer behavior. Do not change intrinsic assignment validation to depend on time of day. Avoid forced home loops during leisure and shopping; ensure idle courier vehicle assignments do not suppress off-duty movement. Carrying a finite work load is an obligation that prevents optional walks, not permission to delete supplies. Test both occupations, schedule boundaries/overnight/always-on schedules, interrupted walks, grocery needs, physical home return, exact saves and independent legacy continuation. Native Unity testing remains paused.

## Implemented behavior and API

`AssignedOccupation(index)` retains permanent employment; `EffectiveOccupation(index)` exposes current behavior. In generation 102, `Occupation(index)` returns the effective role; generation 101 and earlier retain their previous result. Current occupations are clerk and courier. Off-clock workers become consumers once existing work loads are safely finished. They shop, rest and take normal energy-costing walks. Shift start cancels optional leisure and resumes work travel; essential shopping already in progress completes safely under existing priority rules. Job/home/schedule identities are never erased.

A loaded shelf task or active vehicle shipment remains a work obligation until completion/return. That worker has not yet finished work and does not start optional leisure. Empty idle work vehicles no longer consume off-duty actions or block walking grocery trips. Work vehicles are not repurposed as personal shopping cars. Returning a shipment retains legitimate access to the assigned distribution center after closing; idle off-duty workers do not gain unrestricted center access.

No additional actor state is required: effective roles derive from saved employment, schedule, clock and existing task/cargo state. New `OffDutyConsumersEnabled` gate is rules102; `HappinessVersion` aliases101. Shared World/Encounter integration has one writer in this branch and needs project-lead review.

## Evidence

Focused ten-case suite: PASS, 1064 assertions. Combined existing worker-shift and new off-duty suite: PASS, 18 cases / 2718 assertions (`/tmp/npc133-focus.log`). Tests cover both occupations, complete physical outings/paid moves, shift resumption, saves, overnight/always-on schedules, clerk loads, courier vehicle shipments, idle work vans and essential grocery priority. Independent pre-feature101 WorkerShiftTest after200ticks matches SHA256 `B573D8C04701EA54A7C6B80253FC3B4EF1322923CB4FA57196CD4B18F81CF8C4`.

Full core regression: PASS, exit 0, **647 cases / 739017 assertions** at `fa560ed`. No source changes after validation; subsequent changes are this handoff/log only. Command: `DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak133-npc-results`. Log `/tmp/npc133-core.log`; report `/tmp/outbreak133-npc-results/FOUNDATION_CHECKS.txt`.

## Project-lead integration checklist

- Review/integrate NPC commits `6bceb9e` and `fa560ed` atop `npc/f132-happiness`, including the existing pending happiness integration.
- Update the UI build label/preview as appropriate; no UI or general project files were changed here.
- Extend explicit-upgrade controls with `OffDutyConsumersEnabled` and an off-duty consumer option for older happiness-enabled runs. In RestartScenario, include `EncounterRules.HappinessVersion` in the exclusions for legacy energy conversion.
- Use AssignedOccupation for permanent job identity and occupation coloring; use EffectiveOccupation/Occupation for current behavior. Preserve the established gray off-duty worker outlines if retaining that presentation convention.
- Run general UI/input/ABI/config validation and independent integration QA, then publish through the existing release workflow. This NPC handoff is not a completed player build/release.
- Preserve RESTORE42 and the Foundation82 native Unity checkpoint. Native Unity, Windows player and Mono testing remain NOT RUN here.

The original physical commute home from work is retained. Consumer leisure takes over at home or continues an already active walk, preserving doorway progress and existing arrival guarantees. The compatibility dispatch test omits leisure fields when authoring a genuinely older save.
