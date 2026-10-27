# Build and Verification Log

Append one section per completed or blocked task.

## Template
- Task ID:
- Date:
- Baseline commit:
- Branch/worktree:
- Agent:
- Scope and changed files:
- Automated checks and actual results:
- Unity editor compilation: PASS / FAIL / NOT RUN / BLOCKED
- Runtime test: PASS / FAIL / NOT RUN / BLOCKED
- Windows EXE build: PASS / FAIL / NOT RUN / BLOCKED
- Known issues:
- Lead Developer review:
- Next authorized task:

## TEAM-SETUP-01 — coordinated roles installed
- Date: 2026-10-25 (America/Chicago).
- Baseline commit: 49fc25b05567c4ec6b22acb03a9df98da3b39c76; pre-team restore tag RESTORE42, verified locally and on GitHub.
- Branch/worktree: work, /workspace/OutbreakGrid; specialists read-only in shared checkout.
- Agent: Lead primary agent; Simulation & AI, Workshop & UI, QA & Integration, and Build & Release specialists initialized through collaboration tools and all four handoffs reviewed.
- Scope and changed files: installed AGENTS.md, PLANS.md, START_HERE.md, agents/lead.md, agents/simulation.md, agents/workshop.md, agents/qa.md, agents/release.md, harness/build-log.md. PLANS.md records the actual baseline, scope, ownership and future isolated-worktree policy; START_HERE.md points returning sessions to the completed audit. No gameplay/project files changed.
- Acceptance: all nine files present; AGENTS.md and five role files byte-identical to the supplied ZIP; baseline and four specialist reviews recorded; no gameplay changes; RESTORE42 unchanged. PASS.
- Automated checks and actual results: Python file-presence and ZIP-content assertions PASS; git diff --exit-code -- OutbreakRebuild PASS; git diff --check PASS; restore-tag resolution PASS. Core/UI suites NOT RERUN for Markdown-only setup. Existing Foundation 130 logs record 614 cases / 735724 assertions, UI 2321 and NewOnly 3.
- Unity editor compilation: NOT RUN (no Unity executable found on PATH).
- Runtime test: NOT RUN (native testing remains paused).
- Windows EXE build: NOT RUN. Interactive Build Windows player entrypoint exists, but no unattended release pipeline or verified player artifact established.
- Known issues: stale milestone labels in root/historical docs; legacy Mono runner omits newer sources; no tracked Unity .meta assets. These audit findings remain outside setup scope.
- Lead Developer review: four read-only specialist handoffs accepted; QA verified scope and checkpoint and requested this completed log. No overlapping writes, gameplay modifications, dependency changes or asset migrations.
- Integration instructions: publish these nine instruction/log files through the existing incremental GitHub workflow; keep RESTORE42 fixed. Roles are reusable repository instructions; live specialist processes are session-scoped.
- Next authorized task: team setup only, now complete. Next documented gameplay task is purposeful free-time walking, awaiting user selection; happiness follows separately.

## MAINT-01 — current documentation and complete automated runner
- Date: 2026-10-25 (America/Chicago).
- Baseline commit: c0494eb8b787a965b47e95e8d7315d882f7a3493; RESTORE42 remains 49fc25b05567c4ec6b22acb03a9df98da3b39c76.
- Branch/worktree: Lead on work (/workspace/OutbreakGrid); Build & Release on maintenance-check-runner (/tmp/outbreak-check-runner). Exclusive file ownership; no simultaneous shared-checkout edits.
- Agent: Lead documentation/integration; Build & Release runner implementation/verification; QA & Integration independent read-only review.
- Scope and changed files: Tools/run_checks.py; new Tests/WindowsDialogChecks.csproj; root/project READMEs; Docs/ARCHITECTURE.md, REBUILD_PLAN.md, DEVELOPMENT_PREFERENCES.md, BUILD_VALIDATION.md, new AUTOMATED_CHECKS.md; PLANS.md and this log. No Assets, Packages, ProjectSettings or Examples changes.
- Acceptance: current labels identify Foundation 130 while Foundation 82 stays the native Unity checkpoint; historical notes marked as historical; runner sources follow current project manifests/globs, supports explicit .NET executable and Mono compatibility options, runs sequentially, fails on errors and defaults to retained temporary reports outside tracked Examples.
- Automated command: DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home python3 OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/toolchains/dotnet/dotnet --output /tmp/maint01-results (executed in isolated implementation worktree; log /tmp/maint01-runner.log).
- Actual full-run results: PASS, exit 0. Configuration PASS; core 614 cases / 735724 assertions; Windows branch strict compilation and 64-bit OPENFILENAMEW ABI PASS; UI Legacy 2321 assertions; UI NewOnly 3 assertions. Combined report /tmp/maint01-results/FOUNDATION_CHECKS.txt. No full-suite repeat after identical two-file cherry-pick; later changes are documentation only.
- Runner behavior checks: missing explicit executable, non-SDK executable, conflicting backend flags, automatic SDK selection, canonical suite order, manifest/glob/current-vehicle coverage, input symbols, missing-source rejection, unrelated cwd, spaced executable/output paths, configuration failure propagation, child failure stopping later suites, and default output isolation PASS. Some branch/failure checks use controlled fake tools/subprocesses; these do not substitute for the actual full run. Log /tmp/maint01-behavior.log.
- Documentation/source checks: local Markdown links in updated entrypoints/guides PASS; git diff --check PASS; no gameplay/settings/assets/example diff from RESTORE42 PASS.
- Mono execution: NOT RUN; compiler/runtime absent. Manifest/source/symbol resolution checked, but Mono compilation/runtime remains unverified.
- Unity editor compilation: NOT RUN.
- Runtime test: Native Unity/Windows dialogs NOT RUN; hosted core/UI checks above PASS.
- Windows EXE build: NOT RUN. ABI host check is not a player build.
- Known issues: no tracked Unity .meta assets remains an existing audit finding outside maintenance scope. Native rendering, package resolution, desktop input and player release readiness remain unverified.
- Lead Developer review: scope and full-run evidence accepted; gameplay remains Foundation 130, rules unchanged. RESTORE42 preserved.
- Integration instructions: Build & Release commit 76503070b9cc58cd9515552dde365a1b9f8b3e7d cherry-picked as 430c6d7; publish with Lead documentation after final QA review.
- Next authorized task: this maintenance task only. Next documented gameplay task is purposeful free-time walking, awaiting user selection.
- Final QA review: PASS. Independently inspected integrated runner/project/docs and actual full-run/behavior logs; verified fail-fast sequential coverage, report isolation, unchanged gameplay/assets/settings/examples, clean diff checks and unchanged RESTORE42. No further full-suite run required for documentation-only integration.

## F131 — purposeful consumer free-time walks
- Date: 2026-10-25 (America/Chicago).
- Baseline commit: c9d5cd2bca0f425ca19312f0e911e4f62ebd0ad9; RESTORE42 remains 49fc25b05567c4ec6b22acb03a9df98da3b39c76.
- Branch/worktree: Lead work (/workspace/OutbreakGrid); Simulation foundation131-simulation (/tmp/outbreak131-simulation); UI foundation131-ui (/tmp/outbreak131-ui); QA foundation131-qa (/tmp/outbreak131-qa); Release foundation131-validation (/tmp/outbreak131-validation). Exclusive ownership and separate build intermediates.
- Agents: Lead shared rules/fixture/menu/integration/docs; Simulation behavior/contracts/cleanup; UI selected activity and actual-control checks; QA independent behavior/compatibility tests and final review; Build & Release complete runner execution.
- Scope/changed files: Contracts/Documents.cs optional leisure enum/fields; Core/HumanLeisure.cs new bounded planner; HumanReactions.cs decision hook; ShelterRecovery.cs physical door handling; Hunger.cs scalar cancellation/cleanup; StorageRestocking.cs narrow routine-return exception; CourierAssignments.cs and Households.cs atomic home cleanup; World.cs and Encounter.cs rules100/legacy99; EncounterScenarios.cs neighborhood preview; Unity/FoundationApp.cs build/menu/upgrade/restart; FoundationPanels.cs activity; Tests/HumanLeisureChecks.cs, Program.cs, FoundationChecks.csproj, UiHarness.cs; root/project READMEs, Docs/HUMAN_LEISURE.md, ARCHITECTURE.md, AUTOMATED_CHECKS.md, REBUILD_PLAN.md, CITY_LIFE_AND_TRANSPORT_PLAN.md, DEVELOPMENT_PREFERENCES.md, BUILD_VALIDATION.md; PLANS.md and this log. No settings, package, art, scene/prefab or example changes.
- Acceptance: safe consumer home→outdoor→pause→home cycle, normal movement cost, occasional cooldown, actual doors, no foreign-room/ordinary-road transit, urgent needs/jobs/safety first, exact saves and explicit old-rules upgrade. PASS automated/source review.
- Automated final command: DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home python3 OutbreakRebuild/Tools/run_checks.py --dotnet /workspace/toolchains/dotnet/dotnet --output /tmp/outbreak131-final-results (independent Release checkout at exact integrated source 6af1645b9d3893b2570772fc9a02cdfb7060d108; log /tmp/outbreak131-final.log).
- Executed results: core 627 cases / 737155 assertions PASS; Windows real branch strict compilation and 64-bit OPENFILENAMEW ABI PASS; Legacy UI 2463 assertions PASS; NewOnly 3 assertions PASS; Unity target/module config PASS. Combined report /tmp/outbreak131-final-results/FOUNDATION_CHECKS.txt.
- Specialist checks: Simulation 1000 ticks with exact save/load comparison each tick PASS. QA focused 13 cases / 1446 assertions PASS (/tmp/qa131-focus.log); independent pre-feature rules99 HomeSupplyInspectionTest after600ticks matched SHA256 0B8A502E56A55C18298230B5B9C35D85C1908B8E6AFC7390CDAB3552D98916F6. UI actual preview/tick/restart/upgrade handlers PASS2463 + NewOnly3 (/tmp/ui131-specialist.log, /tmp/ui131-specialist-new.log). Updated local Markdown links and git diff --check PASS.
- Regressions found/fixed before release: old unconditional home return caused doorstep U-turn; narrow active-leisure exception preserves real tasks. Home reassignment/unassignment left invalid saved walk geometry; cleanup now atomic. Normal cooldown originally fell through to automatic shelter exit; eligible consumers now wait at home. Bitten/grappled walkers now cancel even between movement decisions. Covered by tests.
- Unity editor compilation: NOT RUN; user-paused native testing, no compatible editor available.
- Runtime test: hosted core and UI-double checks PASS; native Unity and native Windows dialogs NOT RUN.
- Windows EXE build: NOT RUN. Actual Mono execution NOT RUN (unavailable).
- Known limits: first slice only pure consumers, no worker leisure, happiness, parks or bars. Provisional healthy/fed/rested thresholds and timings documented; local paths are cardinal and bounded, not continuous navigation. Native rendering/player behavior remains unverified.
- Integration instructions: Lead rules/fixture 4575689+7c4a8fd; Simulation cc01a33→ab96835; UI8c46129→ea2be89; QA b24a78d→b455e54 and4266f11→6af1645. Source unchanged after independent final validation; later integration edits are docs only. Publish completed build through authorized incremental GitHub workflow.
- Lead Developer review: source/contract integration and four specialist handoffs accepted; no unresolved concrete defects from independent QA. No gameplay scope expansion beyond the approved walk increment.
- Next authorized task: F131 complete; next documented task is happiness foundation (saved −50…+50 with personality preferences), awaiting user selection.
- Final independent QA acceptance: PASS after verifying full-run logs, README131 labels, walking guide and validation/log counts. No remaining concerns or repeated suite required. Native Unity/Windows results remain NOT RUN.

## NPC-F132 — happiness behavior and compatibility handoff
- Date: 2026-10-27 (America/Chicago).
- Baseline: existing local F132 integration 72a80e6; last published completed gameplay build f1b170b (Foundation 131). User approved dedicated NPC scope; general project/UI/release ownership transferred to the other agent.
- Branch/worktree: npc/f132-happiness, /tmp/outbreak132-npc; primary checkout and its pending PLANS.md preserved.
- Scope: finish NPC happiness acceptance and existing NPC compatibility fixtures; no new UI, city generation, general tooling, settings, artwork or release work.
- NPC implementation under review: existing Happiness.cs, optional actor mood/cadence fields and ClearHunger cleanup; version/tick wiring already provided by the prior Lead integration. No production-source changes after baseline.
- Changed files: Tests/HappinessChecks.cs, FoundationChecks.csproj, Program.cs; Tests/StorageChecks.cs, HomeSupplyChecks.cs, MultiStoreChecks.cs and MeleeChecks.cs; NPC handoff and this verification record.
- Acceptance: neutral signed/clamped mood, charisma preferences, visible living pedestrian company, rider/danger handling, deterministic cadence/saves, strict invalid/legacy-state rejection, cleanup and unchanged NPC movement/needs/inventory/RNG.
- Focused results: ten happiness cases / 798 assertions PASS; storage/home-stock/happiness 52 cases / 2042 assertions PASS; legacy dispatch 1 case / 9 assertions PASS; complete combat 104 cases / 2481 assertions PASS.
- Compatibility fixes: authored observation time jumps reschedule mood sampling; true older-rule records omit both new fields; negative storage/combat checks retain their original malformed data so happiness cannot mask unrelated validation regressions. Production validation unchanged.
- Full NPC core command: DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak132-npc-results; executed at dbd3d23. Final result: PASS, exit 0; 637 cases / 737953 assertions.
- Evidence: /tmp/outbreak132-npc-core.log; /tmp/outbreak132-npc-results/FOUNDATION_CHECKS.txt; /tmp/outbreak132-npc-focus.log; /tmp/outbreak132-melee-focus.log; /tmp/outbreak132-multistore-focus.log.
- Independent source/fixture QA: PASS; no remaining concrete NPC defects. Old100 independent600-tick continuation digest matches pre-feature source. Snapshot.Change is the raw contribution; actual stored delta clamps at limits, noted for UI wording.
- Native Unity compilation/runtime/dialogs, Windows player and actual Mono: NOT RUN. General UI/release acceptance remains with the project lead; no completed Foundation 132 release claimed.
- Integration: NPC commits after72a80e6 are6c07c67,9ec16a9,6ab3492,a537cb5,dbd3d23. Details and ownership boundary: harness/NPC_F132_HANDOFF.md. Preserve RESTORE42 and foundation-82-unity-checkpoint.
- Next authorized task: complete this NPC handoff; subsequent NPC gameplay work is not yet selected.

## NPC-F133 — temporary off-duty consumer roles
- Date: 2026-10-27 (America/Chicago).
- Baseline: 9c8c41440e4aa2aef176b5038eabba596352b4d6, NPC happiness handoff. RESTORE42 unchanged.
- Branch/worktree: npc/f133-off-duty, /tmp/outbreak133-npc. Dedicated NPC developer completed implementation/testing directly after specialist work was interrupted; no overlapping edits occurred.
- Scope: clerks and couriers act as consumers off shift while preserving permanent jobs, schedules, homes and finite work loads. Normal movement energy and urgent-needs priority remain intact. No UI/world generation/tooling/release changes.
- Changed production files: WorkerShifts.cs, HumanLeisure.cs, CourierVehicles.cs, StorageRestocking.cs, HomeSupplies.cs, World.cs and Encounter.cs. AssignedOccupation remains permanent; EffectiveOccupation and current Occupation report the temporary consumer role under rules102. Work loads finish safely before consumer leisure. Empty work vans no longer block personal routines; active returning shipments retain center access. Old rules101 keep prior behavior.
- Changed NPC tests: OffDutyConsumerChecks.cs, FoundationChecks.csproj, Program.cs and MultiStoreChecks.cs. The deliberate pre-leisure dispatch fixture now excludes leisure fields that can be created by newly off-duty workers; validation was not weakened.
- Focused results: ten new cases / 1064 assertions PASS; combined existing worker shifts + new off-duty cases 18 cases / 2718 assertions PASS, log /tmp/npc133-focus.log. Includes actual physical home returns, both jobs, full leisure cycles, paid steps, shift-start interruption, grocery priority, saved continuation, overnight/always-on schedules, shelf loads, idle vans and loaded vehicle delivery completion.
- Independent pre-feature legacy101 digest: WorkerShiftTest after200ticks matches B573D8C04701EA54A7C6B80253FC3B4EF1322923CB4FA57196CD4B18F81CF8C4 (pinned before source changes).
- Full core command: DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak133-npc-results, at fa560ed. Final result PASS, exit 0: 647 cases / 739017 assertions. Log /tmp/npc133-core.log; report /tmp/outbreak133-npc-results/FOUNDATION_CHECKS.txt.
- Regressions caught/fixed: after-hours courier could not cross the distribution-center threshold to finish returning its shipment; legitimate active-work access restored. Switching directly to generic return-home routing delayed a loaded clerk's home arrival; original work-to-home commute preserved before free-time takeover. Both fixes have behavioral coverage.
- Shared integration: sole NPC writer updated World/Encounter with OffDutyConsumersEnabled, currentrules102 and HappinessVersion101 alias. Project lead must review and add corresponding UI upgrade/restart handling and use AssignedOccupation for permanent identity/gray off-duty outlines.
- Native Unity/player/dialogs and actual Mono NOT RUN; independent integration review and general UI/release validation remain the project lead's responsibilities. No main merge or completed player release claimed.
- Integration commits: 6bceb9e and fa560ed atop npc/f132-happiness. Details in harness/NPC_F133_HANDOFF.md.
- Next task: finish this NPC handoff; further NPC work awaits user selection.

## NPC-F134 — decisions for known park visits
- Date: 2026-10-27 (America/Chicago).
- Baseline: 34a36a32cdf9b8417c5b74c5fd6a184d7e28e449, off-duty NPC handoff. Isolated branch npc/f134-park-visits, /tmp/outbreak134-npc; sole writer dedicated NPC developer.
- Scope/files: Core/ParkVisits.cs new explicit familiarity/forget APIs; HumanLeisure.cs park selection/stay/status; ShelfMemory.cs detached arrays; Hunger.cs cleanup; Contracts/Documents.cs optional NPC fields; minimal World.cs/Encounter.cs rule103 gate; ParkVisitChecks.cs and core project/entrypoint; NPC handoff and this log. No UI, city generation, artwork, settings, packages, tooling or release changes.
- Acceptance: known reachable park visit, bounded paid travel and stationary stay, physical return, no hidden venue/crowd/stock knowledge, urgent priorities, exact saves, detached bounded knowledge, strict validation and old102 continuation. No flat happiness/energy bonus.
- Build PASS zero warnings/errors. Focused park suite PASS ten cases / 967 assertions, /tmp/npc134-focus.log. Independent old102300-tick digest matches 2DA13FF9F743969B08F933389A6356325EB414733403F50320F8922218D09F20.
- Full core command: DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak134-npc-results at9fd55d1. Final result PASS, exit 0: 657 cases / 739984 assertions. Log /tmp/npc134-core.log; report /tmp/outbreak134-npc-results/FOUNDATION_CHECKS.txt.
- Limits: park familiarity must come from the world layer's authored knowledge or actual observation; no generated parks or park presentation claimed. Current local outing budget remains12-cell radius/20initialsteps. NPCs use ordinary outdoor/company happiness influences, not a new facility bonus or mood-maximization decision.
- Native Unity, UI/release checks, Windows player and Mono NOT RUN; independent integration review remains with project lead. Preserve RESTORE42 and Foundation82 native checkpoint.
- Integration: NPC commit9fd55d1 plus this handoff/log, atop npc/f133-off-duty. World/UI owner supplies park locations and upgrades/previews; see harness/NPC_F134_HANDOFF.md.
- Next authorized task: complete park NPC handoff; further NPC behavior awaits selection.


## NPC-F135 — safe free-time social greetings
- Date: 2026-10-27 (America/Chicago).
- Baseline: 11af94498007b095acd019b380d96b42df2ea7d1, NPC park-visit handoff. Isolated npc/f135-social-interactions in /tmp/outbreak135-npc; dedicated NPC developer sole writer. Shared contract/tick changes remain subject to lead integration review.
- Approved scope: nearby willing NPC greetings at home or during an outing pause, charisma-based happiness rewards, saved pair cooldown and read-only recent interaction API. No new relationship, movement, energy bonus, UI, art, generation, package/settings, tooling or release work.
- Changed production files: new Core/SocialInteractions.cs; Happiness.cs human cleanup; Contracts/Documents.cs three optional fields; minimal World.cs post-movement tick, current104 gate and validator; Encounter.cs ParkVisitsVersion103 alias. Both effective consumers must be free of needs/work/danger; charisma0–4 declines,5 gains1,6–10 gains2, clamped at50. Both cooldowns update atomically. Actual movement remains paid and unaffected.
- Changed tests: SocialInteractionChecks.cs new14 cases; FoundationChecks.csproj and Program.cs registration; HappinessChecks.cs fixture explicitly isolates passive happiness by disabling new greetings. Assertions are retained.
- Focused result PASS14 cases /875 assertions, /tmp/npc135-focus.log. Covers willingness, pair cooldown, visibility/range/real danger, injuries/needs/freeze/infection, pedestrians/riders, park pause, both off-duty jobs and on-duty exclusions, clamping, exact saves/read-only API, death/turn cleanup, corrupt history and legacy continuation. A 300-tick normalized comparison proves otherwise unchanged movement, needs, inventory and RNG.
- Independent pre-feature rules103 two-consumer300-tick SHA256: DC6445178D8D4396DBC7AC42D10FD19E796FBB05495E035FB92068BFBE5CD73C; matches new code's unupgraded continuation. Prior source probe /tmp/npc135-golden; log /tmp/npc135-golden.log.
- Full core command: DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- /tmp/outbreak135-npc-results. Tested implementation3bf486a391b67463b7364e96b6f5cf0bf13aa79d. Result PASS, exit0,671 cases /740859 assertions. Log /tmp/npc135-core.log; report /tmp/outbreak135-npc-results/FOUNDATION_CHECKS.txt.
- Native Unity, Windows player, UI/release checks and Mono NOT RUN. Unity testing remains paused. No independent specialist QA, native NPC performance or completed release claimed. Existing generic movement benchmark is not evidence of greeting-enabled city performance.
- Integration: implementation3bf486a plus this handoff/log, atop npc/f134-park-visits. Review gate/contract/tick integration; world/UI agent consumes SocialInteractionKnownBy, adds upgrade/inspection/preview/presentation and runs general release checks. Main remains untouched; RESTORE42 still peels to49fc25b; Unity6000.6.3f1 and Foundation82 native checkpoint preserved. Full details in harness/NPC_F135_HANDOFF.md.
- Next authorized task: complete this handoff; subsequent NPC increment awaits user selection.
