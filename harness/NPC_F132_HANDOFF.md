# NPC handoff — Foundation 132 happiness

## Ownership and baseline

The user approved the dedicated NPC role on 2026-10-27. This work covers NPC mood state, observations, compatibility and behavior tests. General project coordination, UI, world generation, tooling and releases belong to the project lead/other agent.

- Last published completed build: Foundation 131, `f1b170bab2d6565097b292d0d83883f3c352afd6`.
- Existing local integration baseline: `72a80e6` (contains earlier lead wiring, NPC happiness core and UI changes).
- NPC work branch: `npc/f132-happiness`; isolated worktree `/tmp/outbreak132-npc`.
- Preserve `RESTORE42` at `49fc25b05567c4ec6b22acb03a9df98da3b39c76` and the separate Foundation 82 native Unity checkpoint.
- The primary checkout's existing uncommitted `PLANS.md` was preserved; this NPC task does not overwrite it.

## NPC implementation already present

- `Core/Happiness.cs`: saved neutral-start mood from −50 to +50, 120-tick cadence, read-only observation API.
- `Contracts/Documents.cs`: optional `happiness` and `nextHappinessTick` actor fields.
- `Core/Hunger.cs`: canonical mood cleanup on death/turning.
- Lead wiring already present in `World.cs`/`Encounter.cs`: rules 101 and legacy alias `LeisureVersion` for rules 100; mood advances after authoritative movement. Review/integrate these shared changes under one designated owner.

Provisional preferences: charisma 1–4 prefers solitude/indoors, 5 is neutral, 6–10 prefers company/outdoors. Each matching contributor is +1 and each mismatch −1. Only visible living pedestrians within three cells count, capped at four. Riders are excluded; seated observers retain mood. Stress, infection, grapples, warnings or visible living zombies suppress positive net gains. Updates use no random draws and do not change hunger, energy, inventory, movement or task decisions. Happiness remains observational; parks, bars, venue-seeking and energy bonuses are not implemented by this increment.

`HappinessKnownBy(index)` returns a detached `HappinessSnapshot` with value, availability, paused/danger flags, current contributors and preference descriptions. `Change` is the raw contribution after danger handling; actual stored changes clamp at ±50. The UI owner may label this as potential change/balance rather than promise a gain at a limit. Snapshot calls never advance timers or mutate state.

## NPC commits after the local integration baseline

- `6c07c67`: imported the previously prepared QA contribution (`53d0e48`): ten happiness cases, core project inclusion and test entry-point hook.
- `9ec16a9`: repaired NPC storage/home-stock observation fixtures that author time jumps; mood sampling is rescheduled for the new time origin. Legacy records omit mood fields.
- `6ab3492`: a stepped pre-happiness dispatch fixture now omits mood fields alongside its existing newer store-memory omissions.
- `a537cb5`: legacy storage negative checks omit unrelated mood fields too, preventing happiness rejection from masking a storage-validation regression.

- `dbd3d23`: legacy combat fixtures omit unavailable mood state; full combat coverage retains original assertions.

Production mood validation was not relaxed. These corrections change test authoring, not NPC behavior.

## Acceptance and validation

Acceptance: signed/clamped neutral mood; contrasting social/environment preferences; LOS/dead/rider filtering; saved cadence and exact continuation; read-only queries; safe cleanup; invalid/old-rule rejection; unchanged other NPC state and random streams.

- Original focused happiness suite: PASS, 10 cases / 798 assertions.
- Latest focused storage + home-stock + happiness suite: PASS, 52 cases / 2042 assertions (`/tmp/outbreak132-npc-focus.log`).
- Focused legacy shortage dispatch: PASS, 1 case / 9 assertions (`/tmp/outbreak132-multistore-focus.log`).
- Complete combat suite: PASS, 104 cases / 2481 assertions (`/tmp/outbreak132-melee-focus.log`).
- Full core regression: PASS, exit 0 at `dbd3d23`, **637 cases / 737953 assertions**. Source and tests remained unchanged after that run; later changes are this handoff and its log.
- Full core command: `DOTNET_CLI_HOME=/workspace/toolchains/dotnet-home /workspace/toolchains/dotnet/dotnet run --project Tests/FoundationChecks.csproj -- /tmp/outbreak132-npc-results`, executed from the isolated `OutbreakRebuild` folder.
- Full core log: `/tmp/outbreak132-npc-core.log`; generated report: `/tmp/outbreak132-npc-results/FOUNDATION_CHECKS.txt`.
- Independent old-rules-100 continuation: HomeSupplyInspectionTest after 600 ticks matches pre-feature SHA256 `B6454D3C9612FC00540E4AEFF1556C95741653153B37BACE1916FF2E10DBA7EB`.
- Native Unity/editor/player, native dialogs, Windows EXE and actual Mono: NOT RUN.

## Integration instructions for the project lead

If your checkout already contains `72a80e6`, integrate the NPC commits above in order or fast-forward to this branch after preserving local work. If starting from published Foundation 131, this branch also contains the pre-existing lead wiring and UI commits; review those under their respective owners. Do not apply both original specialist commits and their cherry-picked equivalents.

The UI owner previously reported 2477 hosted UI assertions and 3 NewOnly assertions for the earlier integration. Those are inherited handoff evidence, not new NPC validation. The project lead owns final UI/general-runner validation, current-build documentation, publication and the download link. Do not describe this handoff as a completed published Foundation 132 release.

Next NPC task is not yet selected. The documented broader queue proceeds toward parks; coordinate destination interfaces with the world/UI owner before implementing NPC park visits. Off-duty worker leisure is another possible NPC increment. Keep happiness energy tuning separate.
