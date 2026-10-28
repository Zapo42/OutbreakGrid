# NPC-F141 lead transfer — SOURCE REVIEW, NOT A PLAYER RELEASE

Repository: Zapo42/OutbreakGrid. Branch: `transfer/npc-f141-needs-priority`.
Tested implementation/source commit: `c6902e059e8ac6a6fe2bfa8a8bbad2b238589830`.
Last previously published NPC prerequisite: `0aa8ec34aa168e4797bfcd672053755dcdfaa034` (npc/f137-familiar-greetings).
Unity target stays6000.6.3f1; current core rules107; existing106and earlier runs retain their behavior until explicit upgrade.

## Material limitation — read before integration

Targeted safe-eating and hungry off-duty home-return cases pass. On the unchanged generated city, deaths fall4→2 and both clerks survive/return home, BUT deliveries regress7→4 and courier final health100→69. Attribution of the throughput regression is unresolved. Short control12→12; larger district19→20. Remaining deaths/hunger and insufficient initial food budget persist. Do not treat this as a global food-system improvement, performance certification or release-ready city. Paired delivery-stage/inventory trials are recommended.

## What is here

- `source.bundle`: complete reachable source history for the tested branch, usable offline.
- `changes.patch`: four cumulative commits since the published NPC-F137 prerequisite (F138/F139/F140 diagnostics plusF141 repair).
- `files/`: changed repository files since that prerequisite, provided for review; NOT a standalone Unity project and NOT safe to blindly overlay onto main or divergent shared files.
- `MANIFEST.json`: source/prerequisite IDs, payload SHA256 and byte counts, plus each overlay file's expected old/new hash.
- `VERIFY.py`: read-only payload integrity verification.
- `evidence/`: full hosted core report/log, focused log, current/historical daily and courier reports, replay snapshots and timelines.

The final GitHub transfer commit adds this archive and transfer instructions after the source commit above. No production change is introduced by packaging.

## Recommended online review

From the lead's existing repository:

    git fetch origin transfer/npc-f141-needs-priority
    git worktree add --detach ../npc-f141-review FETCH_HEAD

Review in that isolated checkout. Do not merge main or overwrite the cartographer's shared files automatically. If the lead has already integrated NPC-F137, review/apply the four commits after `0aa8ec34aa168e4797bfcd672053755dcdfaa034`. If starting from published main(Foundation131at inspection), earlier NPC132–137 and pending happiness/UI/shared wiring must be reviewed too. The full source history is supplied to avoid missing prerequisites; original specialist commits must not be duplicated.

## Offline review

Extract the archive, run `python VERIFY.py`, then:

    git bundle verify source.bundle
    git clone --branch transfer/npc-f141-needs-priority source.bundle npc-f141-review

If `git bundle verify` is run outside a Git repository, clone first and verify with `git -C npc-f141-review bundle verify ../source.bundle`.

## Checks

Hosted .NET8: full core PASS701cases/743092assertions; focused final PASS9cases/738assertions; exact old106worker continuation; daily observer and current courier accounting/replays PASS; historical106problem reproduction PASS/exact baseline. These diagnostic passes DO NOT mean city health passed. Read-only independent source review accepted, no blocking defect; reviewer did not run builds/runtime.

In the source checkout, use installed .NET8:

    dotnet run --project OutbreakRebuild/Tests/FoundationChecks.csproj -- <core-results>
    dotnet run --project OutbreakRebuild/Tests/NpcDailyRoutineChecks.csproj -- <daily-results>
    dotnet run --project OutbreakRebuild/Tests/NpcCourierThroughputAudit.csproj -- <courier-results>

Run projects sequentially(they share build intermediates). Historical `NpcHungerInvestigation.csproj` deliberately selects106 to reproduce old bugs; it is not a current-repair acceptance test. Its optional second argument is the historical final save to compare, NOT the supplied current107snapshot.

## Shared integration ownership

Review World.cs/Encounter.cs version gates, HumanReactions.cs/Hunger.cs action ordering and WorkerShifts.cs route ownership with one writer per shared file. No actor/map save fields were added. UI owner must add explicit NeedsPriorityEnabled upgrade handling and SocialFamiliarityVersion106 plus pending101–105aliases in RestartScenario legacy-energy handling; update preview/build labels and run UI/native checks. This NPC task does not ship those UI changes.

Native Unity/player verification remains paused; last user-tested checkpoint is Foundation82. Preserve RESTORE42 at49fc25b05567c4ec6b22acb03a9df98da3b39c76. No Windows player binary is included. Cartographer-package testing remains already authorized and queued for arrival.

User instruction2026-10-28 specifically authorized this transfer-branch push. General future NPC work remains local by default; the lead owns main integration/publication.

## Archive identity

File: `NPC_F141_transfer.zip`

SHA256: `0ab8525ad2e0436e44306053a86105c0c41e5981d3058638b8bb484c7f69316d`

Bytes: 1470093

Package verification completed: ZIP CRC/path checks,44payload hashes, offline full-bundle clone, four patches applied to the prerequisite, and exact reconstructed source-tree/28file hash equality all PASS. See NPC_F141_PACKAGE_CHECKS.txt beside this archive.
