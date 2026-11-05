# Terrain-first reference palette

This records the terrain-first increment. The subsequent [NPC palette pass](NPC_PALETTE.md) updates actor colors while leaving these material mappings and approved doors/storage unchanged. Scene previews are refreshed to the current NPC palette.

This increment maps terrain surfaces to swatches from the approved16-color pixel-scale sheet. It is not a global recolor: NPC sprites, doors/gates, storage/phones/furniture, occupation/selection outlines, need bars and interface colors retain their current presentation. The user specifically approved existing doors and storage.

| Surface | Reference swatch |
| --- | --- |
| Grass / Grass4 | #556B3C olive green |
| Grass2 / Grass5 | #56715E sage |
| Grass3 | #87944D light olive |
| Road / crosswalk base | #101820 deep ink |
| Sidewalk | #80877A gray-green |
| Wall | #26333B dark slate |
| Fence | #79583D wood |
| Window | #78B7B0 aqua |
| Water | #7097AE muted blue |
| Home / unassigned floor | #BA975F warm sand |
| Store floor | #454F6B slate blue |
| Distribution-center floor | #56715E sage |
| Crosswalk paint | #C6C7AE cream |
| Lane paint | #E0C87A gold |

The five authoritative grass IDs are preserved and quantized into three green swatches. No terrain type, passability, generation recipe, building role, geometry, saved rule or inventory changes. Roads and crossings remain the same road surface; only their paint ink changes, with the exact existing stripe/arrow placement. Walls remain distinct from roads; floor roles keep warm/blue/green identities.

Door open/closed/broken/damage colors and gate colors are exact legacy values. ColorForCell's door logic and all storage/phone icon functions remain byte-identical to the prior source. The full frame can therefore contain more than16 colors during this incremental pass. No shader, glow, texture asset, dependency/editor version or GUID changes.

## Inspect the pass

Current Tests > **Inspect Supply Chain Visuals Paused** shows home/store/DC floors alongside unchanged door/storage identities. **Inspect Neighborhood Streets Paused** shows road, sidewalk, crosswalk and lane contrast. Existing current sprite previews and WASD/arrow/wheel controls remain available.

[Building scene](VisualReferences/terrain-palette-buildings.png) · [Street scene](VisualReferences/terrain-palette-streets.png). These reconstruct the production terrain-color buffer supplied to the texture upload and submitted overlay rectangles from the hosted renderer. They are technical previews, not Unity screenshots, native rendering/input validation or a promise about display gamma/DPI. The original [approved sheet](VisualReferences/approved-pixel-scale-study.png) remains the color reference.

Checks cover fixed approved surface/floor swatches, actual buffer role colors, road/crossing identity, preserved door/gate goldens and existing damage/storage/phone/worker/sprite cases, plus unchanged saved state/tick. Crosswalk regression checks now distinguish cream rectangular stripes from the cream square face pixels, preserving both crossing-orientation and stale-paint coverage. See BUILD_VALIDATION.md and harness/build-log.md for final results. Native Unity testing remains paused; city survival/next-day work issues remain separate.
