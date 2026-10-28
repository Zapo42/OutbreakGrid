# UI-PRES-01A — allocated map-panel bounds for selected need bars

Lead decision: **use the full allocated map-panel bounds for need-bar placement**, not the terrain texture's visible rectangle. Keep the existing readable three-row stack; do not shrink fonts, remove rows, limit zoom or resize terrain to make the bars fit.

## Basis and baseline

`OnGUI` allocates the map panel with width/height at least64px. `DrawWorld(Rect bounds)` derives a separate `visible` rectangle from actual map cells. A7×5 map at span256 can therefore render terrain around27×19px while the map panel still has ample room. The H/F/E overlay belongs inside that allocated panel, including its blank padding, rather than being constrained to a33px-high stack inside19px of terrain.

The Lead candidate is `lead/npc-f141-integration-20261028`, baseline `80fcaa61b914da44542ff21f2bcf9e73e0f9be77` (production source eb630f1). Preserve your existing two-file fix as a commit/patch before reconciling with that candidate. Do not overwrite current FoundationApp or UiHarness wholesale from your older Foundation131 checkout: the candidate contains World recovery rendering and NPC upgrade/restart, preview and regression assertions that must remain intact.

## Exclusive Graphics & UI ownership

1. `OutbreakRebuild/Assets/Outbreak/Unity/NeedBarsPresentation.cs` — existing bottom-edge fix and any strictly necessary placement adjustment.
2. `OutbreakRebuild/Tests/UiHarness.cs` — original focused cases plus end-to-end tiny-map coverage, preserving the candidate's existing assertions.
3. `OutbreakRebuild/Assets/Outbreak/Unity/FoundationApp.cs` — **only** the `DrawWorld` call to `DrawNpcBars`: pass `bounds` rather than `visible`, with a short explanatory comment. No other edits in this shared file are assigned.

Lead temporarily hands this narrow FoundationApp change to Graphics; no concurrent writer may edit the file until handoff/integration. NPC-F141-R1's seven-file ownership excludes these three files. Use the existing isolated developer checkout or sequence writers; no new worktree without explicit request. Additional changes require coordinated ownership.

## Acceptance

- The actual production `DrawWorld` path passes the allocated panel to the bar helper. Do not merely test a helper with synthetic generous bounds while leaving the real call unchanged.
- At7×5 and span256, all enabled H/F/E labels/fills remain readable and within the allocated panel even when they extend into its blank padding beyond the terrain texture.
- Cover panel edges/corners, all three rows, each toggle, selected-only behavior and existing supported zoom sizes. Use the actual panel allocation (minimum64×64); do not claim arbitrary smaller rectangles can hold a33px stack.
- Never intrude into toolbar, inspector or bottom status area. Keep the bar stack near its selected actor as available space permits. Offscreen actors remain culled and do not gain a detached overlay.
- Preserve terrain scale/drawing, camera, culling, hit testing/placement, actor coordinates, bar colors/order, descending food semantics and occupation outlines. Keep World/NPC decisions, inventory, ticks and RNG unchanged by drawing/toggle-only operations.
- Preserve the original bottom-edge regression cases and the candidate's NPC happiness/worker/upgrade assertions. Run Legacy and NewOnly UI checks, project configuration and diff checks sequentially. Also run the candidate's `NpcRuleUpgradeUiChecks.csproj` and `BuildingRoomUiChecks.csproj` to confirm shared-file reconciliation. Native Unity remains paused; API-double checks do not establish native text metrics/rendering/input.

## Return to Lead

Return a combined three-file patch/local commit covering UI-PRES-01 and this follow-up, with actual base/source IDs, exact changed files, current commands/counts/exit status, before/after tiny-map evidence and limitations. Do not merge or publish independently. Lead will review the real artifact, run final integrated checks and own publication.

Graphics reported Legacy4816/NewOnly3 plus configuration/diff checks for the original two-file work. Those are specialist-reported results, not Lead-rerun or artifact acceptance. The patch itself has not arrived in this chat. This assignment authorizes the follow-up; it does not claim implementation or native verification.
