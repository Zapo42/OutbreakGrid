# OutbreakGrid — complete World transfer for one-agent consolidation

The user is consolidating development back to one agent. This packet preserves the
World developer's completed work, source history, fixtures, evidence and unfinished
original checkout. Historical specialist ownership in older handoffs describes
how the work was produced; follow the user's current consolidated-agent direction.
This packaging task does not implement pending NPC/graphics features or merge branches.

**Read CURRENT_STATE.md, then RESUME_INSTRUCTIONS.md.**

## What is included

- `OutbreakGrid-history.bundle`: **complete, self-contained Git history**, no
  prerequisite fetch required. Contains final World source d499d03, Lead main
  1b05a73, NPC137 baseline0aa8ec34 and original checkout c563076. Intermediate
  house/store/fixture commits are reachable, including19e18e7 and efa3634.
- `packages/OutbreakGrid-lead-transfer-20380be.zip`: residential modules, six park
  variants, original building-layout prototypes, live two-room houses, original
  daily-routine save/map, complete generator snapshot and earlier evidence.
- `packages/OutbreakGrid-store-interiors-19e18e7.zip`: functional store geometry,
  sales shelves/stockrooms, narrow patch, examples, inspection saves and evidence.
- `packages/OutbreakGrid-store-validation-efa3634.zip`: four paused store trials,
  NPC checklist/results template, assignments/inventories/routes and exact source.
- `packages/OutbreakGrid-distribution-centers-d499d03.zip`: dispatch/warehouse
  interiors, reserved computer footprint, narrow patch, examples and **separate
  Lead100/NPC106 inspection saves**, site manifests and exact validation sources.
- `handoffs/`: key current handoffs extracted for immediate reading.
- `preserved-original-work/`: original dirty checkout status, tracked-change patch
  and all195 untracked non-ignored files in a separate ZIP. Do not extract over a
  current integration checkout. Ignored caches, credentials and installed tools
  are deliberately excluded.
- `BUNDLE-VERIFICATION.txt`: fresh offline clone, reference recovery and Git fsck.
- `TRANSFER-MANIFEST.json` and `SHA256SUMS.txt`: payload sizes and integrity hashes.

There is no single fully integrated source tree claimed here. The main, World and
NPC histories contain different work. In particular, never merge the older maps
branch's entire game ancestry over the Lead's newer shared sources. The bundle
preserves all inputs so the receiving agent can integrate deliberately.

## Essential state

Current World code: `d499d033d437b72a6de96be21e60acd932a301d2`.
Lead main verified remotely: `1b05a73c0994720230524c519dac84fc58d0bbd3`.
NPC baseline verified remotely: `0aa8ec34aa168e4797bfcd672053755dcdfaa034`.

**No behavioral results for the four requested store trials have been received or
verified here.** Static fixture checks and existing core regression tests are not
substitutes for those trials. The original reported four-death save is unavailable.
Order-computer interaction is still unimplemented; its solid wall placeholder
marks geometry only. Restaurant/bar/church layouts remain prototype geometry.

Unity6000.6.3f1; preserve dependencies, asset GUIDs and .meta files. Native Unity
and Windows player testing remain paused. No executable release is supplied.

## Recovery in a fresh environment

From this extracted packet directory:

```sh
git clone --no-checkout -b world/distribution-center-interiors OutbreakGrid-history.bundle OutbreakGrid-review
git -C OutbreakGrid-review fetch ../OutbreakGrid-history.bundle refs/remotes/origin/main:refs/remotes/lead/main refs/transfer/npc137-baseline:refs/remotes/npc/f137
git -C OutbreakGrid-review switch world/distribution-center-interiors
```

That gives the final World tree for review. For integration, create your working
branch from recovered `refs/remotes/lead/main`, preserving the original World and
NPC references. See RESUME_INSTRUCTIONS.md before combining changes.

This consolidation packet is prepared locally. Previously published branch links
are recorded in CURRENT_STATE.md. No new consolidation publication or merge is
claimed. The local transfer branch/commit reported in the final chat contains the
ZIP; the bundle inside it intentionally records the completed code throughd499d03.
