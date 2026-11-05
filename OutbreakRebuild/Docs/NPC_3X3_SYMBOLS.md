# NPC zoom sprites: 5x5 / 3x3 / 1x1

Current colors use [the NPC reference palette and contrast guide](NPC_PALETTE.md). The zoom/facing/walking technical sheets are refreshed with current default inks on a dark canvas; the current contrast matrix and world scenes show the surface-aware blue/green/gray choices.

The user selected three raster sizes for zoom. The current close tier adds modest detail within the same 5x5 canvas; medium and distant tiers retain their existing simpler masks. Human and zombie use identical geometry at each tier, with their existing state colors. No global palette rollout or glow.

| Available body size in screen pixels (shorter side) | Raster | Scaling |
| --- | --- | --- |
| Below 3 | 1x1 distant presence | One screen pixel |
| 3 to below 5 | 3x3 compact person | One screen pixel per cell |
| 5 and above | 5x5 upright person | Whole-number pixel pitch, floor(available / 5) |

```text
Close 5x5   Medium 3x3   Distant
  .###.        .#.         #
  .###.        .#.
  #####        #.#
  .###.
  .#.#.
```

The close sprite has a fuller head, shoulders, torso and separated feet. Its head uses cream for humans or olive for zombies, with two dark eye pixels from the approved reference palette; existing blue/green body inks remain. Human/zombie anatomy stays identical. Face accents only apply to the standing close mask, blend through the existing Core rise transition and are completely overridden by hit/bite flashes. The compact sprite reduces this to a center stem and separated feet. Flat corpse and crouched rise states have dedicated masks in both larger tiers. At one pixel, state is shown through existing ink; a corpse cannot have a separate silhouette at that size. Pending infection keeps the human shape until Core changes species. Rise timing and all decisions remain in Core.

The raster centers inside the unchanged logical body. Pixel origins snap to whole pixels; masks never stretch to fractional pitch. Tier choice is derived from current available screen space, so zooming back restores the richer mask. Logical collision, movement, body/occupation/selection anchors and optional selected H/F/E bars are unchanged. Hit/bite flashes tint occupied cells only. No new textures, asset GUIDs, dependencies or editor settings. No global palette conversion or glow.

## Inspect

Current Tests > **Inspect Zoom Sprites Paused**. Left to right: human, zombie, corpse, infected human. Zoom in/out and select each. Run/step/restart use the existing controls. This replaces the earlier “Inspect 3x3 Symbols Paused” button.

[Actual-draw technical sheet](VisualReferences/npc-zoom-preview.png) shows all three tiers enlarged and at unit pitch; [SVG](VisualReferences/npc-zoom-preview.svg). These are hosted drawing records, not a Unity screenshot. The [approved pixel-scale study](VisualReferences/approved-pixel-scale-study.png) remains the design reference. Historical reference-correction previews are retained as records of earlier work.

## Camera controls

Click the map to leave any inspector text field, then hold **WASD or arrow keys** to pan. Both sets are equivalent; diagonals have the same speed as straight movement. Panning uses real time, works while the simulation is paused and clamps at map edges. It is suppressed while a field owns keyboard focus, on menus/confirmation dialogs, or when the app is unfocused. Middle-drag remains available. If the whole map already fits, zoom in first; the camera cannot pan beyond map edges. Use an expanded-city preview to check longer pans.

Use the wheel anywhere over the **allocated map panel**, including blank padding. The wheel magnitude is respected and zoom-out range adapts to panel resolution, so 5x5, 3x3 and 1x1 can all be reached on larger screens. Sidebar/toolbar scrolling does not zoom the map. The close tier may persist across several wheel steps before its physical size reaches the next threshold; this is intentional.

## Two-frame close walking

The5x5 standing sprite now has two walking foot poses. Head, eyes, body inks, logical dimensions and selection/occupation anchors stay identical. Both species use the same frames. The3x3/1x1 tiers remain static.

```text
Standing feet   Step A   Step B
    .#.#.        .##..    ..##.
```

Feet alternate over the existing verified movement segment, first half then second half. There is no extra timer: ordinary/fast movement uses the segment's existing duration. Idle bob, unsafe/snap/teleport transitions and pushes do not drive walking. Death, active reanimation, vehicle seating and active grapple/shove presentation take precedence. Finishing a segment returns the stationary stance.

**Pause holds the current foot pose.** The existing camera/body snap-on-pause behavior remains; a bounded presentation-only cache holds just the feet. Repeated redraws and zoom out/back keep the same frozen phase. Manual ticks reset to an inspected standing pose; load/restart clears stale gait; resuming uses subsequent committed movement. No animation state is serialized and no movement/food/AI decisions are made by presentation.

Current Tests > **Inspect Walking Sprites Paused** opens a small seeded, non-encounter movement fixture with a human and zombie. Click Run to see both walking; click Pause to freeze them; restart restores the original paused scene. This uses existing baseline wandering, not new NPC behavior or scripted movement forced by animation. Zoom and camera controls remain available.

[Animated technical preview](VisualReferences/npc-walk-preview.gif) · [standing/A/B sheet](VisualReferences/npc-walk-preview.png) · [actual-draw SVG](VisualReferences/npc-walk-preview.svg). These plot hosted drawing records, not Unity capture. The GIF uses a fixed illustrative half-second per frame; the game uses each real movement segment's timing.

## Left/right facing and selection

Close5x5 living sprites face the last verified horizontal movement. Their heads shift one logical pixel toward that side and show a single outward eye. Body/feet, occupied-cell count, species colors and logical anchors stay unchanged. Vertical movement and idle/camera offsets do not change the last side; both species share the same anatomy. Medium3x3/distant1x1 and corpse/reanimation states remain unchanged.

Pause/resume position snapping preserves facing. Initial spawn/load/restart and unsafe/manual inspection snaps use the front pose. Direction is presentation-only and is not saved or used to choose movement, attacks or targets.

[Facing sheet](VisualReferences/npc-facing-preview.png) · [actual-draw SVG](VisualReferences/npc-facing-preview.svg). Use **Inspect Walking Sprites Paused** and Run to see head turns together with the existing foot cycle; Pause holds them for inspection. These are technical previews, not native Unity screenshots.

The default **white consumer box has been removed** from ordinary humans, selected or unselected. Yellow remains the selected-only frame. Actual courier/clerk role outlines remain, including gray off-duty identity and the existing duty colors. Ordinary unselected humans are unframed; removing their default rectangle does not remove worker identity.

## Verification and limits

Checks independently assert authored rows for every tier/state, exact switch boundaries, both walking frames, pause/zoom/restart and segment/idle/push boundaries, square pixels, GUI ink restoration, real DrawWorld transitions in both directions without saved-state/tick changes, selection, need bars, restart and combat contact. UI API doubles cannot establish actual Unity rendering, DPI/input/dialogs or Windows player execution. Native testing remains paused; this increment is published under the user’s standing authorization. See BUILD_VALIDATION.md and harness/build-log.md for results.
