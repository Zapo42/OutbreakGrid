# NPC zoom sprites: 5x5 / 3x3 / 1x1

The user selected three raster sizes for zoom. This supersedes the single 3-column/5-row reference correction; the close figure retains that upright silhouette within a 5x5 canvas. Human and zombie use identical geometry at each tier, with their existing state colors. No global palette rollout or glow.

| Available body size in screen pixels (shorter side) | Raster | Scaling |
| --- | --- | --- |
| Below 3 | 1x1 distant presence | One screen pixel |
| 3 to below 5 | 3x3 compact person | One screen pixel per cell |
| 5 and above | 5x5 upright person | Whole-number pixel pitch, floor(available / 5) |

```text
Close 5x5   Medium 3x3   Distant
  ..#..        .#.         #
  .....        .#.
  .###.        #.#
  ..#..
  .#.#.
```

The close sprite retains a detached head, shoulders, narrow torso and feet. The compact sprite reduces this to a center stem and separated feet. Flat corpse and crouched rise states have dedicated masks in both larger tiers. At one pixel, state is shown through existing ink; a corpse cannot have a separate silhouette at that size. Pending infection keeps the human shape until Core changes species. Rise timing and all decisions remain in Core.

The raster centers inside the unchanged logical body. Pixel origins snap to whole pixels; masks never stretch to fractional pitch. Tier choice is derived from current available screen space, so zooming back restores the richer mask. Logical collision, movement, body/occupation/selection anchors and optional selected H/F/E bars are unchanged. Hit/bite flashes tint occupied cells only. No new textures, asset GUIDs, dependencies or editor settings.

## Inspect

Current Tests > **Inspect Zoom Sprites Paused**. Left to right: human, zombie, corpse, infected human. Zoom in/out and select each. Run/step/restart use the existing controls. This replaces the earlier “Inspect 3x3 Symbols Paused” button.

[Actual-draw technical sheet](VisualReferences/npc-zoom-preview.png) shows all three tiers enlarged and at unit pitch; [SVG](VisualReferences/npc-zoom-preview.svg). These are hosted drawing records, not a Unity screenshot. The [approved pixel-scale study](VisualReferences/approved-pixel-scale-study.png) remains the design reference. Historical reference-correction previews are retained as records of earlier work.

## Verification and limits

Checks independently assert authored rows for every tier/state, exact switch boundaries, square pixels, GUI ink restoration, real DrawWorld transitions in both directions without saved-state/tick changes, selection, need bars, restart and combat contact. UI API doubles cannot establish actual Unity rendering, DPI/input/dialogs or Windows player execution. Native testing remains paused; this increment is published under the user’s standing authorization. See BUILD_VALIDATION.md and harness/build-log.md for results.
