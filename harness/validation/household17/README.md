# HOUSEHOLD-17 — furniture correction and living-room seats

Baseline: d53d4ca780f8b6cb7ec1e4cd7cf25b88d57aae37. Single sequential integration; supply/shifts and social/invitation work are outside this increment.

Reproduce with the documented runner, one build at a time:

```
python3 OutbreakRebuild/Tools/run_checks.py --dotnet /path/to/dotnet --output /tmp/household17
dotnet run --project harness/diagnostics/household16/HouseholdTrial.csproj -c Release -- /tmp/household17-trial
dotnet run --project OutbreakRebuild/Tests/NpcRuleUpgradeUiChecks.csproj --no-launch-profile
```

Core covers both simultaneous couch users, a paid approach to the second cushion when the first is occupied/reserved, exact mid-use continuation, comfy-chair recovery, whole-footprint rejection/defensive capture/terrain removal, and schema compatibility. The original single-seat rules114 fixture is an unmodified tick100 snapshot from the completed HOUSEHOLD-16 observation. Continuing1100ticks must reproduce that prior run's tick1200 SHA256: 839d946afddf53319b82ba305358803b50ad9b4ddbbf1c5dec2b2545679d93d1. It is not regenerated from the changed code.

Fixture SHA256: e966366ab5c40ea612257b1cc849b861df833c69bea6765c7df31b742da00ace.

UI API doubles exercise pixel-aligned bounds at four fractional zooms, exactly-once ink coverage across a solid tabletop row, distant fallback, both camera edges of a two-cell couch (including offscreen anchor), GUI tint restoration and unchanged simulation state. The generated HOUSEHOLD_FURNITURE.svg records actual renderer calls, was rasterized and visually inspected, and is not a native Unity screenshot. The final sheet is also in Docs/VisualReferences/household-furniture.svg.

New couches use width2, two actual adjacent seats, one person per seat. Old omitted-width couches remain single-seat. New comfy chairs and wide couches require map16; original maps remain schema15 and rules114. Old-city policies and version controls stay unchanged. Unity6000.6.3f1, existing metas and dependencies are preserved. Native Unity compilation/rendering/input/dialogs and Windows player execution remain NOT RUN under pause; no EXE.
