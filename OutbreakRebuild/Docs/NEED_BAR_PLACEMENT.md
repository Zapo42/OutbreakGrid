# Selected H/F/E bar placement — UI-PRES-01 and UI-PRES-01A

The selected actor's optional health/food/energy stack now clamps its complete label height at the bottom edge. `DrawWorld` passes the allocated map-panel bounds to the overlay, rather than the smaller terrain texture rectangle.

A7×5 map at maximum zoom-out (span256) can occupy about27×19px of terrain, while the allocated panel remains at least64×64px. The existing33px three-row stack may use blank panel padding. It does not require smaller fonts, missing rows, changed terrain scale or a zoom restriction, and must remain outside toolbar/inspector/status regions.

This change preserves selected-only/defaults-off controls, H/F/E order and colors, descending food values, feature/species/death gating, occupation outlines, actor culling, input handling, camera and simulation state. Only `NeedBarsPresentation.cs`, the one `DrawNpcBars` caller in `FoundationApp.cs`, and `Tests/UiHarness.cs` changed. Current NPC happiness assertions and World recovery-area rendering are retained.

## Verification and limits

Run `Tools/run_checks.py` sequentially, followed by the separate `NpcRuleUpgradeUiChecks.csproj` and `BuildingRoomUiChecks.csproj` projects. The regression suite records submitted label rectangles/fills, exercises edge/animation-overflow cases and actual tiny-map `DrawWorld`, and checks captured state/ticks across drawing/toggles. Existing happiness/upgrade/worker checks remain present after merging the Foundation131-based transfer.

Lead's isolated negative control changed only the caller back to `visible` on the integrated source and ran Legacy UI. It exited1 at the intended assertion: `Small-map bars were clamped to terrain bounds instead of the allocated panel.` This expected failure is evidence for the regression check, not a candidate-suite failure. Current results are recorded in BUILD_VALIDATION.md and harness/build-log.md.

These are API-double geometry/dispatch checks. Native glyph metrics, visual layout/input, Unity compilation and Windows execution remain unverified while native testing is paused. UI integration does not resolve the separate NPC-F141 supply regression or authorize main release.
