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
