# Development Plan — Existing Roadmap Only

Status: Baseline audited; team setup authorized. Gameplay implementation still requires a selected task.

Before implementation, identify the existing roadmap and latest accepted task from project documentation, Git history, issue tracker and available Codex handoffs. Present evidence and unresolved ambiguities. Do not invent, reorder or expand the next task. The user must approve changes to task order or scope.

## Current checkpoint
- Repository: Zapo42/OutbreakGrid; Unity project: OutbreakRebuild/
- Baseline commit: 49fc25b05567c4ec6b22acb03a9df98da3b39c76; permanent pre-team tag RESTORE42
- Unity version: 6000.6.3f1
- Existing roadmap location: OutbreakRebuild/Docs/CITY_LIFE_AND_TRANSPORT_PLAN.md; historical context in REBUILD_PLAN.md
- Last verified working build: Native Unity: Foundation 82, tag foundation-82-unity-checkpoint. Automated: Foundation 130 (614 core cases / 735724 assertions; UI 2321; NewOnly 3). These are distinct checkpoints.
- Last completed documented task: Foundation 130, faster cars and saved paint colors.
- Next documented task: Purposeful free-time walking; not authorized for implementation by team setup alone.
- Blockers: Native Unity editor/runtime and Windows player validation unavailable here; Unity testing remains paused by user instruction. Root README and historical architecture notes have stale milestone labels. Legacy Mono runner lacks newer sources. No tracked Unity .meta assets; preserve existing local GUIDs.

## Approved assignments
Record each task ID, owner, branch/worktree, file ownership, acceptance criteria, verification and merge status.

## TEAM-SETUP-01 — approved team configuration

- Baseline: RESTORE42 / 49fc25b; scope: repository instructions, roles, audited baseline and setup log only.
- Lead (primary agent): AGENTS.md, PLANS.md, START_HERE.md, agents/* and final integration.
- Simulation & AI: read agents/simulation.md; review assigned Core behavior. No writes until an approved gameplay task assigns exact files.
- Workshop & UI: read agents/workshop.md; review Unity/Editor presentation. DragonGlass application is absent; do not invent or import it.
- QA & Integration: read agents/qa.md; independently review setup and production test capabilities; report only.
- Build & Release: read agents/release.md; review build capability and release evidence; report only.
- All specialists during setup: read-only in shared checkout; no commits, pushes, edits or builds that generate project files.
- Future concurrent writes: isolated branches/worktrees with mutually exclusive file ownership. Lead integrates shared Contracts, persistence, scenes, settings and assets sequentially.
- Acceptance: all nine starter files installed, verified baseline recorded, four specialist roles initialized and reviewed, no gameplay changes, RESTORE42 unchanged.
- Publication: team setup follows the existing authorized incremental GitHub workflow. Gameplay, dependency upgrades and Windows releases need their own scope.

## Runtime coordination

The primary agent acts as Lead Developer and starts specialists through the available collaboration tools. These Markdown role files are reusable instructions, not permanent background processes or custom platform agent registrations. New sessions must read AGENTS.md and applicable roles. Spawn specialists only when useful for the approved task, within available concurrency slots.

## TEAM-SETUP-01 completion

All four specialist reviews completed and accepted. QA verified the checkpoint, nine-file installation and absence of gameplay changes; the completed evidence is in harness/build-log.md. Build & Release confirms an interactive Windows build action exists, while native compilation, runtime and Windows release readiness remain unverified. Team setup is complete; no gameplay assignment has been started.

## MAINT-01 — documentation and test-runner maintenance (authorized)

- Baseline: c0494eb8b787a965b47e95e8d7315d882f7a3493; RESTORE42 remains fixed.
- Scope: correct stale current-build labels and document/repair the existing automated runner; no gameplay, Unity target, dependency, asset or save-format changes.
- Lead ownership: README.md, OutbreakRebuild/README.md, Docs/ARCHITECTURE.md, REBUILD_PLAN.md, DEVELOPMENT_PREFERENCES.md, BUILD_VALIDATION.md, new Docs/AUTOMATED_CHECKS.md, PLANS.md and harness/build-log.md.
- Build & Release ownership: Tools/run_checks.py and optional Tests/WindowsDialogChecks.csproj, only in /tmp/outbreak-check-runner on maintenance-check-runner. No overlapping writes; Lead integrates reviewed commit.
- QA & Integration: read-only final code/documentation review and failure/coverage checks; no concurrent compiles in the implementation worktree.
- Dependencies/risks: pinned .NET 8 test projects and available toolchain; old manually duplicated source lists can omit coverage; tests share build intermediates and must run sequentially. Native Unity/Windows runtime evidence remains unavailable.
- Acceptance: accurate Foundation 130/current vs Foundation 82/native checkpoint labels; one documented runner executes configuration, all current core sources, Windows ABI checks and both UI modes; failed prerequisites/commands report failure; no tracked generated outputs or gameplay changes.
- Next documented gameplay task remains purposeful free-time walking, unstarted.

## MAINT-01 completion

Current-build labels and historical notes corrected; automated runner repaired using canonical project manifests. Full .NET run passed: 614 cases / 735724 assertions, Windows dialog ABI, UI Legacy 2321 and NewOnly 3. Tool-discovery/error/path/coverage checks passed. Actual Mono and native Unity/Windows player execution remain NOT RUN. Detailed commands/evidence and ownership are in harness/build-log.md. Earlier baseline audit findings about stale labels and the incomplete runner are resolved by this maintenance task; missing tracked .meta assets remains out of scope. Gameplay stays Foundation 130; RESTORE42 unchanged. Purposeful free-time walking remains the next documented gameplay task, unstarted.

## F131 — purposeful free-time walks (authorized)

- Baseline: c9d5cd2bca0f425ca19312f0e911e4f62ebd0ad9; pre-team RESTORE42 remains fixed.
- Scope: occasional short home → reachable outdoor destination → pause → home walks; normal movement energy; urgent needs, jobs and safety retain priority; saved phase/target; selected-NPC activity and controlled preview. No happiness/parks/bars/vehicle changes.
- Lead ownership: Core/World.cs, Core/Encounter.cs rules integration, Core/EncounterScenarios.cs fixture, Unity/FoundationApp.cs label/menu/upgrade, documentation/PLANS/build log. Lead integrates and reviews shared save contracts.
- Simulation ownership (isolated /tmp/outbreak131-simulation, foundation131-simulation): Core/HumanLeisure.cs, HumanReactions.cs, ShelterRecovery.cs if needed, Hunger.cs cleanup, Contracts/Documents.cs new optional records.
- UI ownership (isolated /tmp/outbreak131-ui, foundation131-ui): Unity/FoundationPanels.cs and Tests/UiHarness.cs.
- QA ownership (isolated /tmp/outbreak131-qa, foundation131-qa): Tests/HumanLeisureChecks.cs, FoundationChecks.csproj, Program.cs; independent final review.
- Build & Release: final unified-runner execution after integration in a separate checkout; no overlapping writes/build intermediates.
- Dependencies/risks: shelter departure and work/home priorities can consume free-time actions; deterministic current save generation100, legacy99 unchanged; bounded outdoor routing must not trespass through unrelated buildings; all actual steps retain existing energy accounting.
- Acceptance: observed departure/pause/return and meaningful cooldown; selected activity; no urgent task interruption; no unrelated building transit; valid exact saved continuation and invalid-state rejection; full core/config/WindowsABI/both UI modes pass; native Unity/player remain NOT RUN per paused testing.
- F131 ownership clarification: Simulation also owns narrow Core/StorageRestocking.cs routine-home-return integration and Core/CourierAssignments.cs cleanup on home reassignment; these preserve leisure progress without preempting genuine supply tasks and keep immediate saves coherent.
- F131 cleanup clarification: Simulation owns minimal Core/Households.cs unassignment cleanup too. Walk/cooldown state must clear atomically when assigned home is removed, and injury/infection/grapple cancellation must run even when normal movement decisions are skipped.

## F131 completion

Purposeful free-time walking implemented for pure consumers with saved phase/target/deadline/cooldown, paid movement, bounded outdoor routing and needs/work/safety priority. A controlled neighborhood preview and selected-human activity expose the full outing. Atomic home changes and scalar injury/infection/grapple cancellation keep saves coherent. Older rules-99 behavior remains independently verified until explicit upgrade. Source and documentation QA accepted; detailed validation and integration evidence is in harness/build-log.md. The next documented gameplay task is the happiness foundation; it has not been started. RESTORE42 and the Foundation 82 native Unity checkpoint remain unchanged.
