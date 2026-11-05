# NPC reference palette and surface contrast

Stable NPC ink now uses the approved reference palette, keeping human/zombie/corpse color families:

| State | Light / dark body swatches |
| --- | --- |
| Human | #7097AE / #454F6B blue |
| Zombie | #87944D / #556B3C green |
| Corpse | #80877A / #454C50 gray |

The renderer chooses the stronger relative-luminance contrast within each family against the terrain material under the interpolated body center. It includes the existing forced-open door crossing appearance. Choosing a shade reads presentation/world data only: no movement, targets, danger, NPC knowledge, health or saved state is changed. Reference luminance values are cached and the body choice is made once per draw, not per pixel.

Human faces remain #C6C7AE cream; zombie faces use the selected green, keeping all green silhouette pixels readable rather than hiding the head against matching grass. Eyes remain #26333B dark slate. Masks, facing/walking frames, occupied-cell counts and hit/selection anchors are unchanged. No rectangular boxes, outlines, glow or new textures are introduced. Ordinary consumers remain unframed; existing worker/selection cues are unchanged.

The same world position/material gives the same shade at all zoom levels and selections. Shades can change when crossing surfaces. Pause retains the existing positional-snap and frozen-foot/facing behavior; the ink then reflects the surface at that inspected position. No palette state or camera data is serialized. Gray-to-green reanimation retains Core timing and continuous blending; flash colors retain their prior overrides. Those transitions and protected doors/storage/UI remain deliberate exceptions to whole-frame16-color claims.

## Inspect

[Contrast matrix](VisualReferences/npc-palette-contrast.png) · [SVG](VisualReferences/npc-palette-contrast.svg) shows actual submitted draws at5x5/3x3/1x1 on grass, light grass, sage grass, roads, sidewalks and home/store/DC floors, enlarged and at unit pitch. Stable non-eye pixels meet a1.5 relative-luminance contrast floor on those tested surfaces. This is a modest non-text glyph regression threshold, not an accessibility certification, universal-background guarantee or native DPI/display-gamma result.

The [building](VisualReferences/terrain-palette-buildings.png) and [street](VisualReferences/terrain-palette-streets.png) technical scenes are refreshed from production terrain buffers and overlays with the new NPC colors. All are hosted render-record plots, not Unity screenshots. The current zoom/facing/walking sheets and illustrative walking GIF are refreshed with the new default inks on their dark canvas; this matrix shows how shades change on other materials.

Use **Inspect Walking Sprites Paused**, **Inspect Zoom Sprites Paused**, **Inspect Supply Chain Visuals Paused** and **Inspect Neighborhood Streets Paused** under Current Tests. Check all zoom levels, walk across materials and pause. Masks/facing/walking, WASD/arrows, selected optional H/F/E and descending food, collapsible inspectors, restart/menu and legacy runs remain intact. Doors/storage/furniture/vehicles/role indicators/UI inks are not recolored in this increment.

Validation includes independent per-material shade goldens and luminance calculations, exact occupied-cell counts/no new outline, real DrawWorld material wiring, preserved state/tick and the retained full UI/legacy suite. An isolated archive control omitting backdrop wiring fails the real-renderer check despite direct helper correctness. See BUILD_VALIDATION.md and harness/build-log.md for results. Native Unity testing remains paused; survival/next-day work issues are separate and unresolved.
