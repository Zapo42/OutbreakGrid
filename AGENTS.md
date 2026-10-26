# Unity Development Team — Repository Rules

This repository contains an existing Unity outbreak simulator and/or DragonGlassPixelWorkshop. Do not assume both are in this repository. Inspect the filesystem and actual project metadata first.

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
- Run available static checks, Unity batch compilation/tests and runtime validation when a compatible Unity editor is available. Distinguish NOT RUN, PASS, FAIL and BLOCKED. Never claim compilation or runtime verification without evidence.
- Do not claim a Windows EXE was produced unless the actual build artifact exists and was verified.
- For each completed task, log the changed files, test command/results, known regressions or caveats, and exact roadmap checkpoint in harness/build-log.md.

## Delegation
- Lead Developer (agents/lead.md): task order, architecture, file ownership, approval, final integration.
- Simulation & AI (agents/simulation.md): NPC behavior, zombie AI, combat, infection, physics, population and simulation performance.
- Workshop & UI (agents/workshop.md): DragonGlassPixelWorkshop, editors, tools, sprite metadata, palettes, UI, animation and editor file formats.
- QA & Integration (agents/qa.md): review, validation, bug reproduction, compatibility and regression testing; no opportunistic redesign.
- Build & Release (agents/release.md): repeatable builds, Windows packages, versioned releases and build evidence.

## Required handoff
Task ID; baseline Git commit; scope; owned files; changed files; acceptance criteria; validation evidence; unresolved issues; integration instructions; next authorized task (if known).

Read the applicable role file before performing specialist work.
