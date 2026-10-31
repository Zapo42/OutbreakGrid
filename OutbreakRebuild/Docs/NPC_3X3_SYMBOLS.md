# 3x3 NPC symbol increment

The user selected3x3. This local increment replaces solid actor rectangles with small logical masks; it does not implement a new simulation, global pixel canvas,16-color palette conversion or LCD effect.

## Masks

```text
Human    Zombie   Corpse   Rising
 .#.      .#.      ...      .#.
 ###      ##.      ###      ###
 #.#      .##      ...      ...
```

Each occupied cell is one square of ink. The mask scales at integer screen-pixel pitch and its origin snaps to whole pixels. When fewer than3 screen pixels fit the logical body, a one-pixel distant mark is used rather than an unreadable fractional grid. Simulation dimensions, collision and body anchors remain unchanged. Clicking also accepts the snapped glyph bounds, keeping that distant dot selectable.

Existing human, zombie and corpse colors are retained. A living infected human remains a human silhouette with the existing optional infection marker; it does not turn into a zombie visually ahead of Core. Reanimation uses Core's existing rise progress and grey-to-green timing: flat, crouched, then upright. Hit/bite flashes tint occupied glyph cells rather than covering the symbol with a solid square. No glow or new shader/texture allocation.

The yellow selection frame sits outside the unchanged occupation band, leaving the state-colored glyph visible. Existing movement/interpolation, idle/contact cues, carried-object/weapon presentation and role outlines stay on their logical anchors. The glyph snaps to pixels; this is not yet whole-frame pixel scaling or bitmap UI typography. Very dense/far views, actual contrast and native appearance still need native review when testing resumes.

## Find the preview

Choose **Inspect 3x3 Symbols Paused** under **Current Tests**. Left to right: human, zombie, corpse, infected human with the existing marker. It opens paused with the human selected. Run/step begins the existing simulation; Restart restores the authored initial data. Nothing about this preview changes normal NPC decisions or saved rules.

The [enlarged technical sheet](VisualReferences/npc-3x3-draw-preview.svg) is generated from actual hosted draw records. It is not a Unity screenshot or proof of native rendering/input. The [approved direction sheet](VisualReferences/approved-pixel-scale-study.png) remains the visual reference; an exact palette/terrain/object rollout is separate.

## Validation scope

UI checks assert independent mask rows, exact occupied pixel rectangles, integer pitch/origin, distant fallback, current colors, GUI color restoration, rise states, glyph-only flashes, actual DrawWorld output and preview selection/bars/restart/state invariants. Existing seated-driver checks now count six human mask cells per visible walker, still requiring no pedestrian ink for a seated driver. Contact tests retain exact logical contact/lean/recoil/hands checks while inspecting the snapped zombie pixels and the separate human selection frame.

No Core/Contracts/persistence, old assets/GUIDs, package/editor pins, scene settings, terrain/vehicle art or food/needs behavior changed. A new stable.meta belongs to the new helper script. Current counts and results are in BUILD_VALIDATION.md and harness/build-log.md. Native Unity/editor/input/text metrics and Windows player remain paused/unrun; no EXE or publication.

## Next small options

1. Map the approved restrained palette to the symbols and check state contrast while preserving identity.
2. Extend the same tiny-symbol language to doors/storage, keeping their existing functional states clear.

Global canvas/zoom scaling, typography and optional LCD treatment remain later work. Do not add glow before symbols are readable.
