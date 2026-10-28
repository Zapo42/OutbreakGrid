# Unity Development Team — Repository Rules

This repository contains the OutbreakRebuild Unity simulator. DragonGlassPixelWorkshop application source has not been found here. Do not assume it exists or merge uploaded projects automatically; Workshop integration and asset migration require specific authorization.

## Source of truth
1. Actual repository files and their Git history establish implemented behavior.
2. Existing approved roadmap, task tracker and handoff records establish task order. Find and inspect them. If unavailable, report that fact; do not invent the next task.
3. Prior chat discussions may describe intended functionality, but cannot establish that functionality has been implemented or tested.
4. Preserve approved existing architecture and workflows. Do not rebuild functioning systems without specific authorization.

## Process
- Start every assignment with scope, affected files, dependencies, tests, risks, and acceptance criteria.
- Lead Developer allocates mutually exclusive file ownership for concurrent edits. Shared code, assets, package manifests, Unity project settings, scenes and prefabs require coordination.
- Specialists work on isolated Git branches or worktrees if concurrent writes are needed. Never let two agents edit one working tree concurrently.
- Do not alter the Unity editor version or dependency versions without approval.
- Keep .meta files alongside assets. Never regenerate GUIDs or rename asset folders casually.
- Protect scenes, prefabs, tile maps, serialized data, save formats, sprites and palettes.
- No destructive Git actions, force-push, deletion of existing artwork, credential changes, or deployment without authorization.
- Add plain-English comments to new or modified code explaining substantive steps. Avoid commentary that merely restates syntax.
- Favor small, straightforward implementations with limited scope and clear UI behavior.
- Use OutbreakRebuild/Tools/run_checks.py for hosted checks, sequentially within a checkout because test projects share intermediates. Native Unity testing remains paused until the user explicitly resumes it, even if an editor becomes available. Distinguish NOT RUN, PASS, FAIL and BLOCKED. Hosted core tests, UI API doubles and Windows ABI checks do not establish actual Unity compilation, native input/rendering, dialogs or Windows player execution.
- Do not claim a Windows EXE was produced unless the actual build artifact exists and was verified.
- For each completed task, log the changed files, test command/results, known regressions or caveats, and exact roadmap checkpoint in harness/build-log.md.

## Delegation
- Acting Project Lead (agents/lead.md): coordinate the existing dedicated NPC coder and three additional development roles below. Own planning, task allocation, interfaces, integration, documentation and verification records; coordinate QA & Integration and Build & Release.
- Existing dedicated NPC coder (agents/simulation.md): retains NPC behavior and its behavioral tests. Do not create a replacement NPC agent or take over its work. Coordinate movement/driving/cargo interfaces with this coder.
- Procedural World & Infrastructure (agents/world.md): procedural city generation, building placement, streets, sidewalks, crosswalk geometry, connectivity and world infrastructure tests.
- Graphics & UI (agents/workshop.md): graphics, sprites, palettes, animation presentation, cameras, inspectors, menus, editor tools, vehicle/building appearance and presentation tests. The existing role-file path is retained for continuity; Workshop migration remains separately authorized future work.
- Engine & Development Tools (agents/engine.md): general engine infrastructure, diagnostics, test-runner maintenance and build tooling. Coordinate persistence/tick contracts with the Lead and NPC coder; no NPC behavior ownership.
- QA & Integration (agents/qa.md): review, validation, bug reproduction, compatibility and regression testing; no opportunistic redesign.
- Build & Release (agents/release.md): repeatable builds, Windows packages, versioned releases and build evidence.

Role scopes guide assignments; they do not grant blanket ownership of folders. Before parallel work, the Lead records the task, baseline, exact owned files, branch/worktree, dependencies, acceptance checks and integration order in PLANS.md. Shared Contracts, persistence, tick integration, scenes, prefabs, settings and test manifests require an agreed interface and one writer. The dedicated NPC coder owns behavioral tests; QA independently reviews/runs them and requests changes through that coder. These instructions do not reorder the approved roadmap.

## Presentation and checkpoint requirements
- Organize information around the selected item with collapsible encounter sections. Keep current previews easy to find and historical previews in Old Test Scenarios.
- Preserve simulation speed controls, Main Menu, Restart Scenario, selected-only optional H/F/E bars and the descending food display.
- Preserve readable building/storage identities, door states and occupation outlines. Make pixel-art changes incrementally, retaining existing colors and detail.
- Keep authoritative simulation decisions in the core, outside UI and animation code.
- Keep Unity 6000.6.3f1 and dependency versions unchanged without authorization. Preserve asset GUIDs and .meta files.
- RESTORE42 permanently identifies pre-team Foundation 130 at 49fc25b05567c4ec6b22acb03a9df98da3b39c76. Foundation 82 remains the last user-reported working native Unity checkpoint (foundation-82-unity-checkpoint). Do not move these tags or overwrite main to test an older build.
- Last published completed build: Foundation 131, f1b170bab2d6565097b292d0d83883f3c352afd6. User reports unpublished Foundation 132 happiness partially integrated at 72a80e6 with specialist checks passed and full QA unfinished. That commit is absent here; preserve unfinished work if found and do not call Foundation 132 completed or release-validated.
- Record completed work in OutbreakRebuild/Docs/BUILD_VALIDATION.md and harness/build-log.md. Continue the authorized incremental GitHub publication workflow after validation and QA. Include https://github.com/Zapo42/OutbreakGrid/archive/refs/heads/main.zip with completed published builds; never claim publication or a Windows EXE without evidence.

## Required handoff
Task ID; baseline Git commit; scope; owned files; changed files; acceptance criteria; validation evidence; unresolved issues; integration instructions; next authorized task (if known).

Read the applicable role file before performing specialist work.
