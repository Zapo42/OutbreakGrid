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

Maintain this table for every subsequent increment. Keep the checkpoint unchanged until the user explicitly identifies a newer tested build. Automated UI checks use API doubles and do not verify Unity layout, native rendering, or a Windows executable.
