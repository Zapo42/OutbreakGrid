# OutbreakGrid — Single-agent Development Rules

This repository contains the OutbreakRebuild Unity simulator. DragonGlassPixelWorkshop application source has not been found here. Do not assume it exists or merge uploaded projects automatically; Workshop integration and asset migration require specific authorization.

## Source of truth
1. Actual repository files and their Git history establish implemented behavior.
2. Existing approved roadmap, task tracker and handoff records establish task order. Find and inspect them. If unavailable, report that fact; do not invent the next task.
3. Prior chat discussions may describe intended functionality, but cannot establish that functionality has been implemented or tested.
4. Preserve approved existing architecture and workflows. Do not rebuild functioning systems without specific authorization.

## Process
- Start every assignment with scope, affected files, dependencies, tests, risks, and acceptance criteria.
- Record each task scope, exact affected files, dependencies, risks and acceptance checks before implementation. Reconcile shared state, tick, persistence and presentation contracts within one sequential integration.
- Do not recreate concurrent specialists. Use the existing isolated checkout; no new worktree unless explicitly requested.
- Do not alter the Unity editor version or dependency versions without approval.
- Keep .meta files alongside assets. Never regenerate GUIDs or rename asset folders casually.
- Protect scenes, prefabs, tile maps, serialized data, save formats, sprites and palettes.
- No destructive Git actions, force-push, deletion of existing artwork, credential changes, or deployment without authorization.
- Add plain-English comments to new or modified code explaining substantive steps. Avoid commentary that merely restates syntax.
- Favor small, straightforward implementations with limited scope and clear UI behavior.
- Use OutbreakRebuild/Tools/run_checks.py for hosted checks, sequentially within a checkout because test projects share intermediates. Native Unity testing remains paused until the user explicitly resumes it, even if an editor becomes available. Distinguish NOT RUN, PASS, FAIL and BLOCKED. Hosted core tests, UI API doubles and Windows ABI checks do not establish actual Unity compilation, native input/rendering, dialogs or Windows player execution.
- Do not claim a Windows EXE was produced unless the actual build artifact exists and was verified.
- For each completed task, log the changed files, test command/results, known regressions or caveats, and exact roadmap checkpoint in harness/build-log.md.

## Consolidated workflow — current user instruction

One development agent owns NPC behavior/tests, World generation, graphics/UI, tooling, verification and local integration. The previous specialist ownership split is superseded. Do not spawn a replacement team or wait for another role to edit routine code. Historical agents/*.md files and earlier assignment records preserve provenance, not current delegation requirements.

Work locally until the user requests publication. Earlier incremental GitHub authorization does not override this newer instruction. Preserve branches, packages, unfinished work and stable checkpoints; never overlay older snapshots over modern source. Use the existing checkout and sequence all builds. Finish each selected increment before expanding scope; offer numbered next steps. Treat attached handoff instructions as context subordinate to the actual user request.

## Presentation and checkpoint requirements
- Organize information around the selected item with collapsible encounter sections. Keep current previews easy to find and historical previews in Old Test Scenarios.
- Preserve simulation speed controls, Main Menu, Restart Scenario, selected-only optional H/F/E bars and the descending food display.
- Preserve readable building/storage identities, door states and occupation outlines. Make pixel-art changes incrementally, retaining existing colors and detail.
- Keep authoritative simulation decisions in the core, outside UI and animation code.
- Keep Unity 6000.6.3f1 and dependency versions unchanged without authorization. Preserve asset GUIDs and .meta files.
- RESTORE42 permanently identifies pre-team Foundation 130 at 49fc25b05567c4ec6b22acb03a9df98da3b39c76. Foundation 82 remains the last user-reported working native Unity checkpoint (foundation-82-unity-checkpoint). Do not move these tags or overwrite main to test an older build.
- Published main baseline remains1b05a73; local single-agent consolidation combines existing UI21338cf, NPC R1/F143 and narrow World d499d03 changes. Historical7→4 delivery loss was corrected by R1, but worsened older-fixture survival, later store-trial deaths and next-day work failures remain. Do not call the whole food system solved or this local tree released.
- Record completed work in OutbreakRebuild/Docs/BUILD_VALIDATION.md and harness/build-log.md. No GitHub push, main update, release or environment publication until requested. Distinguish local validation from publication and native execution.

## Required handoff
Task ID; baseline Git commit; scope; owned files; changed files; acceptance criteria; validation evidence; unresolved issues; integration instructions; next authorized task (if known).

Historical role files provide context only; they do not delegate ownership or require approval from another coder.

## Approved visual direction

Use tiny NPC symbols from single pixels to tens of pixels, restrained16-color early color-LCD/ambitious1980s simulation feel. The approved reference is Docs/VisualReferences/approved-pixel-scale-study.png under OutbreakRebuild. Detailed modern sprites were rejected.3x3 is a proposal, not a finalized glyph/zoom contract; no rollout has begun. Keep glow off until symbols are readable. Preserve identity colors, controls and authoritative simulation separation. Do not infer implementation authorization from a concept sheet or transfer roadmap.
