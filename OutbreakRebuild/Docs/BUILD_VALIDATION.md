# Build validation and rollback checkpoint

The user reported Foundation 82 working, then instructed that later incremental builds will go untested in Unity until they say otherwise. Acknowledging or continuing development does not establish Unity validation. Continue automated checks for each change and record native validation separately.

## Last user-tested checkpoint

- Foundation 82 / reanimation awareness
- Commit: `0fc9522513593d7585de88d9fc2a356dcd52dfe7`
- Permanent Git tag: `foundation-82-unity-checkpoint`
- [Download the checkpoint ZIP](https://github.com/Zapo42/OutbreakGrid/archive/refs/tags/foundation-82-unity-checkpoint.zip)
- [Inspect the checkpoint source](https://github.com/Zapo42/OutbreakGrid/tree/foundation-82-unity-checkpoint)

Extract that archive into a separate folder and open its OutbreakRebuild project to return to the known checkpoint. Preserve current projects and save files. Newer rules saves may not load in the older checkpoint; use its original scenarios or compatible checkpoint saves. Do not reset or overwrite GitHub main merely to try an earlier build.

## Builds after the checkpoint

| Build | Change | Automated validation | Unity validation |
| --- | --- | --- | --- |
| 83 | Pending-turn focus, corpse and get-up counts, outbreak summary correction | UI harness: 1306 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 84 | Visible infection and pending corpse markers, display toggle and legend | UI harness: 1317 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 85 | Pending-turn countdown, imminent warning and one-shot pause before conversion | UI harness: 1335 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 86 | Independent storage-box inventories, placement, shelter food deposits and withdrawals | Core: 387 cases / 321153 assertions; UI: 1359 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 87 | Saved shelter restocking tasks, six-meal reserves, collection and return trips | Core: 395 cases / 321544 assertions; UI: 1373 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 88 | Per-box food reserve settings, zero/off, presets, persisted targets and trip interruption | Core: 401 cases / 321565 assertions; UI: 1397 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 89 | Personal box-location and stock memory, remembered food/deposit routing, bounded saved observations | Core: 407 cases / 321651 assertions; UI: 1409 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 90 | Shared firsthand box observations, report provenance and source-age preservation | Core: 414 cases / 321677 assertions; UI: 1421 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 91 | Read-only shelter food census, shortage filters, camera focus and assigned-trip counts | Core: 420 cases / 321702 assertions; UI: 1446 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 92 | Box-to-box shelter deliveries, protected donor reserves, saved courier source and delivery census | Core: 428 cases / 322201 assertions; UI: 1456 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 93 | Per-box outgoing donation controls, saved defaults, pickup cancellation and loaded-cargo preservation | Core: 431 cases / 322216 assertions; UI: 1466 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 94 | Assigned courier homes, source box and rotating multi-box destinations; longer bounded routes and home return | Core: 439 cases / 323749 assertions; UI: 1488 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 95 | Household rosters, home/away/visitor counts, resident selection, reassignment and safe home removal | Core: 443 cases / 323798 assertions; UI: 1514 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |
| 96 | Opt-in meals per assigned resident, balanced box targets, manual overrides, protected donor reserves and capacity-gap warnings | Core: 449 cases / 324496 assertions; UI: 1533 assertions; NewOnly input: 3 assertions; project configuration and diff checks | Untested, per user instruction |

Maintain this table for every subsequent increment. Keep the checkpoint unchanged until the user explicitly identifies a newer tested build. Automated UI checks use API doubles and do not verify Unity layout, native rendering, or a Windows executable.
