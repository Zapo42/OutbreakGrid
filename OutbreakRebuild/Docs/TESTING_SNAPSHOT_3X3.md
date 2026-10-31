# 3x3 consolidated testing snapshot

Testing snapshot prepared October 30, 2026.

Source baseline: e1768b3 (graphics source 1101b5e). This snapshot combines the reconciled NPC R1/F143, World infrastructure, happiness and selected-need-bar work with the first 3x3 glyph increment. It is experimental: city survival and next-day attendance are unfinished.

## Download and start

Extract the project into a fresh folder. In Unity Hub add **OutbreakRebuild** and use **Unity 6000.6.3f1** without upgrading dependencies. Choose **Outbreak > Rebuild > Open and Run Foundation**. The tests below are supplied for the user's next native session; cloud native testing remains paused and none are recorded as passed.

For a Windows player, install the matching Windows Build Support module, then use **Outbreak > Rebuild > Build Windows player** and choose an empty output folder. Distribute the entire output folder, including the EXE, Data folder and supporting libraries. No Windows player was produced or verified in this cloud environment.

## Manual tests

1. **Symbol readability (5 minutes).** Current Tests > Inspect 3x3 Symbols Paused. Left to right: human, zombie, corpse, infected human. Zoom in and out, pan, select each actor. Expect distinct shapes, existing state colors, a separate yellow selection frame and a selectable single-pixel mark when far away. The infected human should retain a human shape before turning. Capture a screenshot at normal zoom and maximum zoom-out; report screen resolution and UI scaling.
2. **Need bars and layout (5 minutes).** In that preview select the living human. Enable the optional H/F/E bars, move the selected actor near the map-panel edges by panning, and zoom far out. Expect the complete three-row stack inside the map panel, possibly over its blank padding, without toolbar/inspector overlap. Disable bars and select another item; expect selected-only behavior. Food decreases visually in the established direction. Report the actor, zoom and a screenshot for clipping.
3. **Pause, restart and save continuity (5 minutes).** Step 1, 10 and 100 ticks; try Run/Pause and speed controls. Record the tick, save, advance, reload and compare the restored tick/state. Main Menu should retain a resumable run. Restart scenario should restore its authored initial state paused. Record any input failure or state mismatch; avoid overwriting valued older saves.
4. **Needs and deliveries (10 minutes).** Current Tests > Inspect Needs Priority Paused, then Inspect Vehicle Deliveries Paused (separate runs). Observe carried food being eaten before routine work when safe; inspect physical pickup, vehicle cargo and unloading. Record scenario, seed, tick, food/cargo and any stalled actor. This is observational regression testing: do not expect all generated-city hunger/death or next-day work issues to be solved.
5. **World entrances and identity (5 minutes).** Use Load Map with a bundled map in CityReferences/districts/houses. Inspect sidewalk-to-door access, internal doors, distinct living/storage spaces and readable storage/door states. Terrain-only imports do not automatically create population/jobs. For movement testing place a human via the existing tools; report blocked routes with the exact map filename and screenshot. This does not validate long-term survival.

Please return: test number, PASS/FAIL, project commit, Unity version, OS/display scaling, scenario/map, seed/tick, screenshot and reproducible steps. A Console compile error blocks native testing: include its full first error rather than changing editor/package versions.

## Optional automated check

With Python 3 and .NET 8 SDK installed, run from OutbreakRebuild:

```text
python Tools/run_checks.py --output check-results
```

Run only one instance at a time. This compiles/runs the hosted simulation and UI API-double harnesses, not a Unity player. Include the resulting FOUNDATION_CHECKS.txt if reporting a failure.

## Known limits

Hosted API doubles do not validate Unity rendering, native input/dialogs, DPI or player execution. Tiny symbols retain existing colors; a global 16-color palette and object-symbol rollout are not implemented. City delivery deaths and missed next-day work remain documented in CONSOLIDATION_STATUS_2026-10-30.md. This is a source testing package, not a verified Windows executable.
