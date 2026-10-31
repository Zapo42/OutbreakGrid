# Reference-corrected NPC symbols

The published literal three-row experiment was rejected by the user. This correction follows the taller figures in the approved pixel-scale sheet: **three columns by five rows**, with a shared human/zombie silhouette. The historical “3x3” preview button remains for discoverability; it no longer describes the corrected raster height.

```text
Human/Zombie   Corpse   Rising
    .#.         ...      ...
    ...         ...      .#.
    ###         ...      ...
    .#.         ###      ###
    #.#         ...      #.#
```

The detached head, shoulders, narrow torso and separated feet replace the squat shape and invented asymmetric zombie. Human/zombie identity remains in existing blue/green ink; a pending infection does not change the living human silhouette. Corpse and rising poses use the same five-row grid. Reanimation timing, state, movement, hitboxes and decisions remain in Core.

The upright glyph fits within the unchanged logical body using square integer screen pixels. When a full five-row figure cannot fit, a one-pixel distant mark is used. Origins snap to pixels. Existing selection/occupation outlines, need bars, carried objects and flash cues remain; flash colors only occupied cells. No glow, global palette, sprite textures, scene/settings or GUID changes.

## Preview

Current Tests > **Inspect 3x3 Symbols Paused**. Left to right: human, zombie, corpse, infected human. The name is retained from the earlier trial. Zoom in to inspect the corrected proportions; restart restores the authored paused data.

[Actual-draw sheet, enlarged and at submitted pixel size](VisualReferences/npc-reference-corrected-preview.png). This is rasterized hosted drawing output, not a Unity screenshot. The approved direction remains [the pixel-scale study](VisualReferences/approved-pixel-scale-study.png).

## Verification

Independent row expectations verify standing geometry, head gap, feet, corpse/rise stages, square integer pitch and distant fallback. Actual DrawWorld selection/toggle/save-state/restart tests remain. Seated-driver assertions account for glyph-or-distant-dot scale while still excluding the seated driver; shove tests retain contact/recoil checks. See BUILD_VALIDATION.md and harness/build-log.md for final evidence. Native Unity rendering/input/DPI remain unrun under the testing pause.

Palette mapping and object-symbol rollout remain separate work. This correction stays local until publication is requested.
