# Instructions for the receiving single agent

The user is consolidating agents, not asking you to infer untested success. Start
from real Git state and preserve all unfinished work. This packet is the complete
World-side transfer; other chats may have additional newer work that must also be
collected before you call the whole project consolidated.

1. Verify hashes and recover the full bundle using START_HERE.md. It has no missing
   prerequisite commits. The code baseline is Worldd499d03; the later packaging
   commit contains no new game implementation.
2. Inspect recovered Lead main1b05a73, Worldd499d03 and NPC0aa8ec34. The authoritative
   integration starting point is the Lead tree, not a wholesale checkout/merge of
   older World game ancestry. Preserve the user's original dirty checkout archive
   separately; do not overwrite the completed pipeline with those older drafts.
3. Lead main already contains assembled World house/grouping changes, potentially
   with Lead-specific compatibility fixes. Do not reapply the full old house Core
   patch or replace shared C# files wholesale. Preserve asset .meta/GUIDs.
4. Integrate missing World increments selectively:19e18e7 live stores, d499d03 DC
   geometry. Their narrow patches, exact files and evidence are inside their ZIPs.
   Resolve Python/docs conflicts using the final source as reference while retaining
   legitimate Lead changes. Old transfer-only commits28dcbab/b78fccc are packaging,
   not gameplay requirements. Do not cherry-pick every ancestor indiscriminately.
5. The NPC137 source contains newer behavior than Lead main. Review and integrate
   it deliberately with current World/shared code if requested; do not assume a
   no-conflict merge or replace current saves/contracts blindly. Prior full NPC137
   UI failure is recorded. Keep shared contract/persistence/tick edits sequential.
6. efa3634 supplies new standalone fixture tools/checks and handoffs. The four paused
   NPC trial saves require NPC106 rules plus the World schema14 reader. The fixture
   tool references newer NPC park/familiarity APIs; it is not a Foundation131-only
   project. Use matching source or complete actual NPC integration first.
7. Read NPC_HANDOFF.md and keep DIAGNOSTIC_RESULTS.md pending until you run the real
   behavior trials. Use save/seed/hash/source, stable actor IDs, ticks, coordinates,
   inventory/needs values and pre-failure saves. World static checks do not establish
   that clerks restock, couriers deliver or all NPCs survive. Obtain the original
   four-death save before claiming its reproduction or cause.
8. Retain Unity6000.6.3f1, dependencies and stable asset identities. Native Unity
   testing remains paused. Validate hosted code through the documented unified
   runner, sequentially in each checkout, plus focused map/infrastructure projects
   and Python generator checks. Do not claim a Windows release was tested.
9. Once actual integration and trials are complete, select further implementation
   with the user. Pending options include real order-computer pickup/claim behavior,
   restaurant/bar interiors/mechanics, church materialization or generation variety.
   The solid computer footprint is not a live terminal. Earlier separate-role rules
   are historical context under the user's new consolidated-agent workflow.

## Commands worth keeping

Python3 with Shapely2.1.2/pyproj3.7.2 (requirements-atlas.txt in bundle). .NET8 SDK
8.0.427 was used. Existing environment paths are conveniences, not portable promises:

```sh
python -m unittest discover -s OutbreakRebuild/Tools -p 'test_*city*.py'
python -m unittest discover -s OutbreakRebuild/Tools -p test_store_validation.py
python3 OutbreakRebuild/Tools/run_checks.py --dotnet /path/to/dotnet --output /tmp/outbreak-checks
```

Run focused projects after the runner completes, using separate intermediates:

```sh
dotnet build OutbreakRebuild/Tests/DistributionLayoutChecks.csproj -c Release -p:BaseIntermediateOutputPath=/tmp/dc/obj/ -p:OutputPath=/tmp/dc/bin/
dotnet /tmp/dc/bin/DistributionLayoutChecks.dll /path/to/three-dc-maps
```

Other focused projects: BuildingRoomChecks, StoreLayoutChecks, StoreRoutineFixture.
Follow their packet handoffs for required map inputs, exact commands and assertions;
these are standalone tools, not silently registered replacements for NPC tests.

The previous host needed DOTNET_CLI_HOME, XDG_RUNTIME_DIR, XDG_DATA_HOME and
NUGET_PACKAGES under /workspace/.outbreak-tools, plus telemetry opt-out. Configure
appropriate directories in the new environment; installed SDKs and credentials are
not packaged. Credentials and ignored caches were never exported.

Existing source geographic attribution is retained in the packages. The reference
atlas uses named OSM survey/profile provenance with fictional seeded geometry;
no claim of Google Maps scraping or machine-learning training is made.
