# Complete single-agent consolidation transfer

Download and extract OutbreakGrid-complete-single-agent-transfer-d499d03.zip. Start with START_HERE.md. The ZIP contains a standalone full-history Git bundle, all World transfer packages, maps/saves, validation evidence, original unfinished checkout backup, integration notes and a receiving-agent prompt. It requires no missing prerequisite commits.

The code baseline in the bundle is d499d03; this later commit only packages the transfer. The World, Lead and NPC code trees are separate inputs, not a claimed completed integration. Pending NPC store-trial results, computer mechanics and prototype building limitations are explicitly documented. Native Unity testing remains paused.

The sibling SHA256 file verifies the outer ZIP; the manifest and SHA256SUMS inside verify every payload. Offline clone/reference recovery, Git fsck, all nested ZIP integrity checks and payload checks passed. No code tests were repeated for this packaging-only change.
