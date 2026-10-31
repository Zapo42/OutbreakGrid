# Development Plan — Existing Roadmap Only

Status: GRAPHICS-3X3-01 implemented and hosted-validated locally.3x3 actor masks/preview ready; no publication/native test. Earlier consolidation records and survival issues remain; single-agent ownership stays active.

## GRAPHICS-3X3-01 — user-selected NPC symbol increment

- User chose3x3 for the first graphics increment. One agent, existing local branch; baseline8a4a6fe. No publication/native testing or unrelated feature work.
- Files: new Unity/NpcGlyphPresentation.cs and stable.meta; FoundationApp.cs actor/feedback drawing and Current Tests preview; UiHarness.cs actual mask/frame/zoom/state/selection checks and migration of old solid-body assertion; graphics/validation/plan/build records. No Core, Contracts, saves, packages, scenes, existing GUIDs or world/vehicle/terrain artwork changes.
- Contract:3x3 logical human/zombie/flat-corpse masks with integer-sized screen pixels and snapped glyph origin. Existing identity colors and rise timing retained; rising states use existing Core progress. Under3 physical pixels, use one-pixel distant fallback. Retain logical body anchors/culling/occupation outlines/HFE/feedback; UI clicking also accepts the snapped glyph bounds so a distant dot remains selectable. Simulation hitboxes remain unchanged. Yellow selection frame separate from glyph. Glow off; no global16-color or320x200 pipeline rollout.
- Acceptance: exact mask occupancy/grid; human/zombie/death/rise distinctions; no full solid actor rectangles or solid flash replacement; actual DrawWorld/selection/toggle/paused/restart and saved-state invariants; distant and zoom bounds; existing UI/NPC/World checks retained. Preview is paused authored data only, all decisions stay in Core. Run sequential hosted validation, not native rendering. Supply factual local report/next steps.
- Complete at source1101b5e: full711/743692, ABI, UI4992+3; upgrade75 and World UI20 PASS. Expected tiny-map old-call control still fails correctly. No Core/contracts/pins or existing assets/GUIDs changed; technical draw sheet and docs/evidence saved. Native appearance/performance not validated; no publication.

## CONSOLIDATE-20261030 — completed graphics-first compilation audit

- User request: combine the uploaded work, compile it, report completed/remaining work for each area starting with graphics, then proceed incrementally. Attached instructions are handoff context, not independent feature/publication authorization. Single agent owns all integration; no team delegation.
- Base: clean21338cf, UI1826dc6 already integrated exactly once. NPC bundle verified; local merge brings only R1b6c2b9a and F14365b59c9. World finald499d03 imported as a narrow12-path diff from20380be (generator/store/DC/fixture tools/tests/docs only); its older rules97 game tree is not merged.
- Files: NPC nine-file combined diff; World twelve-path diff; representative store/DC maps; approved graphics reference; current single-agent instructions/status/validation records. Existing local/remote main and permanent tags untouched. Original dirty drafts retained separately in verified packages, not applied as current source.
- Risks: source candidates differ; updated UI coverage must survive. R1 fixes generated deliveries but older cartographer survival worsens; F143 enables doors but two store-delivery deaths and next-day work failures remain. Do not tune food/staff/capacity or claim survival from replay passes.
- Required checks: actual combined unified runner sequentially, focused NPC-upgrade/World UI and house/store/DC projects, Python generator/fixture tests, supplied store observer against combined107 with finite ledger/replay and known failure distinctions. Native Unity/player paused; no EXE. No new sprites/palette/renderer implementation in this task.
- Complete: combined source1628f24 passes711 cases/743692 assertions, ABI/UI4899+3; Python48+2,39 recipe replays; upgrade75, World UI20, house1264/store857/DC93; standalone authoring build0 warnings/errors. Supplied four store trials reproduce F143 final states exactly,28 replay groups/840 exact steps. Delivery still2 deaths with158food; generated courier9 credits/1death/health100. Full evidence and graphics-first report saved locally; no push/publication, native testing still paused. No pixel-art rollout implemented.

Before implementation, identify the existing roadmap and latest accepted task from project documentation, Git history, issue tracker and available Codex handoffs. Present evidence and unresolved ambiguities. Do not invent, reorder or expand the next task. The user must approve changes to task order or scope.

## ROLE-SPLIT-02 — approved non-NPC role allocation

- Authorization: user assigned the acting Project Lead to coordinate coders and split development responsibilities, explicitly retaining the already active dedicated NPC agent.
- Baseline: clean checkout f1b170bab2d6565097b292d0d83883f3c352afd6 (published Foundation 131); one local worktree. User reports Foundation 132 happiness partially integrated at 72a80e6, specialist checks passed but full QA unfinished. That object is absent here; do not overwrite/recreate it or call it complete. Obtain its actual handoff before continuing that integration.
- Lead ownership for this documentation task: AGENTS.md, PLANS.md, START_HERE.md, agents/lead.md, agents/simulation.md coordination note, agents/workshop.md, new agents/world.md and agents/engine.md, agents/qa.md and agents/release.md pause reminders, Docs/BUILD_VALIDATION.md and harness/build-log.md. No application, tests, assets, settings, dependency, version or save changes.
- Existing NPC coder: retains NPC behavior and behavioral tests; no replacement agent is assigned. NPC movement/driving/cargo interfaces remain coordinated with that coder.
- Additional coding role 1: Procedural World & Infrastructure — city generation, building placement, streets, sidewalks, crosswalk geometry, connectivity and infrastructure tests (agents/world.md).
- Additional coding role 2: Graphics & UI — graphics/sprites/palettes, animation presentation, cameras, inspectors, menus, editor tools, vehicle/building appearance and presentation tests (agents/workshop.md).
- Additional coding role 3: Engine & Development Tools — engine infrastructure, diagnostics, test-runner maintenance and build tooling (agents/engine.md).
- Lead: planning, task allocation, shared interfaces, final integration, documentation and verification records; coordinate QA & Integration and Build & Release. These role files do not create permanent background agents or prove contact with an external NPC coder.
- Coordination: assign exact files and acceptance criteria for each approved increment before work. Shared Contracts, persistence, tick integration, scenes/settings and test manifests need an agreed interface and one writer. Concurrent writers need isolated branches/worktrees; in cloud tasks use the existing checkout unless worktrees are explicitly requested, otherwise sequence writers. No gameplay assignments or concurrent coding were started by this instruction change.
- Risks: broad role overlap is controlled by per-task file ownership; missing Foundation 132 state prevents integration claims; hosted evidence must not be presented as native Unity/Windows validation. Native Unity testing remains paused; pinned versions and permanent checkpoints remain unchanged.
- Acceptance: three additional non-NPC roles documented, existing NPC coder retained, presentation/checkpoint requirements preserved, onboarding role-split status updated, local instruction links and diff checks pass, independent read-only review completed. Results recorded in harness/build-log.md.
- Next authorized work: coordination instructions only. No new roadmap or gameplay task is selected by this role split.

## Historical team-setup checkpoint
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

## UI-PRES-01/01A-INTEGRATE — three-file merge onto current candidate

- User supplied transfer/ui-pres-01-01a at1826dc6fbb8b9b122816dcca0f93177e6622be0b, based on Foundation131. Current candidate clean atc67eeca. Merged original history without file replacements into source e95d5b338f904aec6e983cf321783c76f7c8d26b; only NeedBarsPresentation.cs, authorized FoundationApp caller/comment and UiHarness.cs changed.
- Lead owns merge/evidence/docs; Graphics implementation ownership fulfilled on handoff. Existing newer happiness assertions, NPC upgrade/restart/occupation fixes and World recovery-area rendering retained. Core/contracts/World assets, dependencies, settings and current runner unchanged. NPC-F141-R1 remains a separate seven-file assignment.
- Read-only QA found no code blocker; meaningful geometry/selection/toggle/state cases and actual7×5/span256 caller coverage verified by inspection. Lead negative control on an isolated git-archive copy reverts only bounds→visible and exits1 at the intended tiny-map alignment assertion. No worktree or active candidate mutation used.
- Validation complete on exact source e95d5b3: full unified PASS701 cases/743092 assertions, Windows ABI, Legacy UI4899/NewOnly3; separate NPC upgrade UI75 and World UI20 PASS. Old-call negative control exits1 at intended tiny-map alignment assertion. Newer happiness coverage retained (14 assertions beyond the transfer baseline). Evidence at harness/validation/ui-pres-01a. Native Unity paused; publish candidate only, main remains1b05a73 pending independent NPC correction.

## UI-PRES-01A — Lead decision on tiny-map overlay bounds

- Authorize full allocated map-panel `bounds` for selected need bars, rather than terrain `visible`. Keep readable33px H/F/E stack; no font shrink, hidden rows, zoom restriction or terrain resizing. Actual panel minimum64×64 supplies room even when7×5 terrain at span256 is about27×19px.
- Exact exclusive Graphics files: NeedBarsPresentation.cs, UiHarness.cs, and FoundationApp.cs limited to the DrawWorld→DrawNpcBars argument/comment. Full paths, baseline80fcaa6, reconciliation rules and acceptance are in harness/UI_PRES_01A_ASSIGNMENT.md. Lead hands over that narrow shared-file change; NPC-F141-R1 ownership is nonoverlapping. No concurrent FoundationApp writer.
- Test real DrawWorld caller, tiny-map maximum zoom-out, panel containment including blank padding, no sidebar/toolbar overlap, unchanged culling/input/terrain and selected-only optional semantics. Preserve current NPC/World UI assertions and state invariants; run Legacy/NewOnly, config/diff and candidate focused UI checks. Native testing stays paused.
- Completion: combined three-file transfer1826dc6 received and merged as e95d5b3; Lead validation/QA evidence recorded under UI-PRES-01/01A-INTEGRATE. Original specialist counts remain historical; current integrated Legacy4899/NewOnly3 pass. Main still held for the separate NPC-F141 regression.

## NPC-F141-R1 — user-selected courier regression correction

- Authorization: user chose option1, dedicated NPC correction before main promotion. Baseline2aeac34 on lead/npc-f141-integration-20261028; production source eb630f1. Main remains1b05a73.
- Existing NPC coder exclusively owns the seven exact implementation/test/diagnostic paths in harness/NPC_F141_R1_ASSIGNMENT.md; only change necessary files. Lead owns shared version/persistence/UI/manifests and final records/publication. No new NPC agent or simultaneous shared-checkout writer started.
- Scope: meal/reserve/departure interaction and empty-return claim lifecycle behind the reproduced7→4 delivery loss. Preserve safe eating, paid movement/rest, finite inventory, danger priorities, off-duty home return, exact old106 behavior and World/UI integration. No food/staffing/capacity/speed tuning or accounting workaround.
- Acceptance/evidence: focused failing-before/passing-after cases; exact saves/legacy digest; paired unchanged-input trials resolving loss and claim waste; current controls, core/daily/courier diagnostics; separate supplied cartographer observations. No unapproved roadmap expansion or native testing. Full handoff requirements are in the assignment file.
- Status: assignment published for user relay to the existing NPC coder; no direct cross-chat contact or implementation start claimed. Release remains blocked. Lead will integrate/revalidate the returned correction before main publication.

## NPC-F141-INTEGRATE — source candidate, release blocked by delivery regression

- Authorization: user requested integration of transfer/npc-f141-needs-priority at b37a589ab6df1f220d6786c35581ad08514dad65, including prerequisites and diagnostic review before release. Current main inspected at1b05a73 (Foundation131+World), clean. New branch lead/npc-f141-integration-20261028; no worktrees created, no tag moves/resets.
- Source: verified source c6902e059e8ac6a6fe2bfa8a8bbad2b238589830 with all NPC132–137 prerequisites and F138–141 diagnostics/fix. Package checksum,44 payload hashes and complete bundle PASS. Merge preserves history without duplicating specialist commits or replacing current World schema14. Lead is sole integration writer; NPC coder retains future behavioral implementation/test ownership.
- Exact integrated source/test file inventory: harness/validation/npc-f141/INTEGRATED_PATHS.txt. Lead-owned additional UI work: FoundationApp.cs upgrade/restart/preview/label integration, FoundationPanels.cs assigned occupation identity, WorkerPresentation.cs persistent outline identity, new NpcRuleUpgradeUiChecks.cs/.csproj. Supplied NPC behavior/tests are integrated unchanged. Shared Contracts/Documents.cs retains both optional NPC state and World map fields; World/Encounter gates preserve rules100–106 until explicit107 upgrade. Current runner and all World files/GUIDs remain retained.
- Lead owns PLANS.md, BUILD_VALIDATION.md, build log, integration documentation and evidence. QA source/regression agents read-only in this checkout; paired diagnostic writer owns only /workspace/review/npc141-regression, unmodified production source archive. No concurrent builds within a checkout. UI-PRES-01 need-bar task remains separate; its files are not otherwise redesigned.
- Acceptance: full sequential hosted runner on actual merge; explicit upgrade and restart tests across100–106 with schema14 houses; World focused checks and unchanged generator suite; current daily/courier audits with replay/accounting distinguished from city health. Native Unity remains paused. Do not change capacities, refill stock, weaken tests or alter NPC logic to conceal the regression.
- Regression review: paired same-seed initial state differing only in rules version reproduces7→4 credited meals,4→2 deaths,20→25 remaining source stock and courier health100→69. Stage traces attribute the three missing credits to consumption of carried delivery stock and a later hungry empty-return/claim-expiry cycle. No NPC fix made by Lead. Main publication remains held pending dedicated NPC correction or explicit user acceptance of the tradeoff; candidate branch may be published for review with this blocker prominent.
- Candidate validation at eb630f125339d458d005dcbc62d529c3d7486052: full runner PASS701 cases/743092 assertions, ABI PASS, UI2477+3; explicit upgrade/restart/schema14 integration75 assertions PASS; World infrastructure1264 and UI20 PASS; Python40 PASS; daily/courier replay-accounting checks PASS. Generated credits4, controls12/20; integrated107 final byte-identical to transfer. Release remains BLOCKED by the paired7→4 regression; source checks are not city-health acceptance. See harness/validation/npc-f141/RESULTS.json.
- Actual cartographer save follow-up: original rules106 exact roundtrip;14400ticks under106 and explicit107, repeated save replays PASS. Separate64x36fixture deaths8/11→7/11, remaining food24→25, retained-order credits40→38. Compatibility only; not survival or full accounting acceptance. No geometry, stock or production behavior changes. See cartographer report in the candidate evidence.

## WORLD-INTEGRATE-20380BE — Lead assembly

- User authorized Lead final assembly, verification and GitHub publication; transfer branch world/lead-transfer-20261028 at28dcbab supplies implementation20380be and prerequisite generators/reference profiles.
- Lead owns selective five-file shared C# patch integration, new room-group assets/.meta, focused test inclusion, generator/reference snapshots and final documentation/evidence. Excluded older maps game ancestry and older test runner; did not import NPC137 routine source into production. Source commit f31a56e133069c6baefa958e0b784d7398fa8bca on lead/world-integration-20261028.
- Independent QA read-only, no concurrent writers/builds. Full hosted runner PASS627 cases/737155 assertions, ABI PASS, UI2463+3; Python40 PASS;20 legacy recipes canonical-identical;4 house maps fresh-replay byte-identical; focused infrastructure1264 assertions PASS; new Lead-owned BuildingRoomUiChecks.cs/.csproj PASS20 assertions through production rendering with API doubles. UiHarness.cs itself remains unchanged and Graphics-owned. Standalone NPC137 static fixture reproduced separately with11 actors/21 routes and byte-identical map/save/manifest, no behavioral ticks.
- World source handoff includes user-directed follow-ons beyond initial four-file residential scope: parks, prototype layouts and live two-room-house metadata. Integrated only explicit delivered implementation; do not infer new restaurant/church/order-computer mechanics or alter roadmap order.
- Status: selective source integration and hosted validation complete; Lead owns normal GitHub publication and verification. Native Unity/player testing remains paused. Schema14 maps require the matching reader; rules106 routine save belongs to separate NPC137 source, not this rules100 release. NPC137 reported Legacy UI failure remains unresolved outside this increment.
- UI-PRES-01 and all NPC behavior/tests remain with their assigned coders. Future NPC integration must preserve these World changes and obtain full integrated QA rather than merge old branches wholesale. Source/evidence/limits recorded in BUILD_VALIDATION.md and harness/build-log.md.

## UI-PRES-01 — selected need-bar edge placement and regression checks

- Assigned to: Graphics & UI developer. Scope: small hardening increment for existing selected-only optional H/F/E bar placement near viewport edges; preserve existing colors, meanings and toggles. This is presentation maintenance, not the deferred pixel-art rollout or happiness implementation.
- Baseline: developer reports clean f1b170b (Foundation 131), onboarding only, no tests rerun. Verify actual HEAD/status before editing and report divergence; Lead currently holds assignment documentation on docs/world-res-01-assignment. Do not reset other work or copy an older UI over Foundation 132.
- Exclusive implementation ownership: OutbreakRebuild/Assets/Outbreak/Unity/NeedBarsPresentation.cs and OutbreakRebuild/Tests/UiHarness.cs only. No other coder may write those files during this assignment. No test manifest/API-double files outside UiHarness.cs, FoundationPanels.cs, FoundationApp.cs, Core, Contracts, persistence, scenes, assets, settings or recipes are assigned. Lead owns plan/build-validation/build-log integration records.
- Shared test-file gate: NPC-F141 candidate eb630f1 now retains the original Foundation132 UI assertions in UiHarness.cs; UI-PRES-01 must merge its focused changes with these assertions rather than overwrite the file from Foundation131. Lead handled only this shared integration; further feature writes remain with Graphics & UI. UiHarness.cs was used by earlier Foundation 132 UI specialist work. The Graphics coder must report any local/newer edits or current owner before writing; if that file is still actively owned, sequence the writers through the Lead. Existing Foundation 132 assertions must survive integration. No simultaneous writers in a checkout; use existing isolated developer checkout or sequence work, not a newly created worktree without explicit request.
- Acceptance: at supported viewport/zoom sizes, enabled selected-actor bars and H/F/E labels stay inside the viewport at all four edges/corners without changing order, colors or descending food semantics; keep labels/fills readable and preserve actor selection/occupation outlines. Define supported viewport dimensions from current app code, not arbitrary tiny inputs. Do not change the camera, actor positions, global rendering or bar toggle semantics.
- Regression checks: actual production drawing through UiHarness; selected versus nonselected actor, toggles off/on, valid human/zombie/dead states and feature-disabled food/energy, zero/full/intermediate values, viewport edges and zoom extremes supported by current app. Assert geometry/labels and fill fractions, plus unchanged Documents.Encode(world.Capture()) and tick across drawing/toggle-only operations. Reuse existing fixtures and add focused cases, not duplicate every suite. If baseline already meets all geometry requirements, report that and strengthen meaningful missing coverage rather than inventing a visual change.
- NPC interfaces: read ActorState health/hunger/species, World.HungerEnabled, World.EnergyEnabled, EnergyRules.Remaining(actor) and selection only; existing presentation-local toggle commands. No new NPC state, commands, formulas, ticks or saves. Coordinate only if this contract must change; such changes are not authorized here. No dependency on Foundation 132 happiness state.
- World dependency: viewport/body rectangles and existing cell scale from current renderer, no generated lot/building/sidewalk data. WORLD-RES-01 owns the Python generators/district README separately; no geometry coordination/change needed for this task.
- Validation: existing UI checks in Legacy and NewOnly modes, project config check, git diff --check, exact output counts/exit status. Builds sequential per checkout. Lead runs final unified runner on integrated source and updates validation records. UI API doubles establish dispatch/recorded drawing geometry, not native text metrics, clipping, rendering/input or Windows execution. Native Unity testing stays paused.
- Handoff: task/baseline, exact two changed files, before/after evidence or coverage-only finding, commands/results, current report paths, limitations and integration instructions. No independent merge/publication. Lead integrates with unpublished Foundation 132 carefully, obtains QA and publishes after validation. Assignment only; implementation/tests NOT RUN by Lead for this handoff.

## WORLD-RES-01 — residential-block generation (user-selected)

- Scope: seeded house placement, lot spacing, setbacks and sidewalk-connected entrances; no NPC behavior changes or city-wide redesign.
- Baseline: Lead checkout work at f1b170bab2d6565097b292d0d83883f3c352afd6, one worktree. Existing uncommitted role/documentation changes preserved. Workspace discovery and tracked HEAD inspection found none of the proposed Python sources or the district README.
- Exclusive World ownership: OutbreakRebuild/Tools/generate_reference_city.py, OutbreakRebuild/Tools/generate_district_city.py, OutbreakRebuild/Tools/test_district_city.py and OutbreakRebuild/CityReferences/districts/README.md. Source baseline: 8e92e361206507219a725d698fca55dd26de5ff9 on maps/city-reference-pack-20261026; Lead fetched and verified all four paths. Work from that source branch, preserving existing changes and reporting divergence. Do not reset to the older Lead checkout, create replacement scripts or substitute the C# generator.
- Lead owns assignment/integration records in agents/world.md, PLANS.md, BUILD_VALIDATION.md and harness/build-log.md. QA independently reviews/runs checks; World remains the only writer of assigned generator tests. Existing NPC coder retains NPC behavior/tests. Other coders must not concurrently edit World-reserved files.
- Dependencies: actual recipe schema/units, CLI/options, output consumers, lot boundaries and sidewalk connectivity. Recipe files/generated assets remain read-only unless separately assigned. Additional files/shared contracts need coordinated ownership. Concurrent writers require isolated checkouts only when explicitly requested; otherwise sequence writers.
- Acceptance: deterministic same-seed/recipe/options output; usable interior-to-sidewalk entrances; valid recipe-defined spacing/setbacks/boundaries; existing recipe/default/input-output compatibility. Derive numeric limits and impossible-lot policy from actual sources. Detailed evidence requirements are in agents/world.md.
- Validation: capture baseline, run existing Python district tests and representative generation across relevant seeds/recipes, orientations and tight/edge lots; verify geometry and compatibility directly. Then run sequential unified hosted checks after integration. Native Unity testing stays paused. C# checks do not substitute for generator tests.
- Risks: unknown recipe contracts, blocked doors, connection paths crossing neighbors, random-state coupling and compatibility regressions hidden by rewritten fixtures.
- Status: assignment authorized and exact paths/source verified; source-location limitation resolved. Implementation and QA remain with the World developer and reviewers; no generator changes or completed validation are claimed by this documentation handoff.

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
