# Human warnings attract zombies

Current build: **Foundation 15 / warnings attract zombies**, rules `encounter-prototype-14`.

Human warnings now reach nearby zombies as well as humans. An idle zombie investigates the cell where the warning originated, creating a cost to warning others. It cannot track the speaker after they move out of sight.

## Try it

1. On GitHub choose **Code → Download ZIP**, extract into a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Warning Attraction Paused** and select zombie **#4**, below the small room.
4. Click **Step 10 ticks**. It moves toward the warning heard at **6, 2**, stopping outside the closed partition door at **3, 4**. The inspector shows its heard source and no observed human target.
5. Continue stepping: hearing alone cannot damage or pass the door. An unrefreshed warning expires after five simulation seconds, allowing ordinary wandering again.

New encounters enable attraction automatically. Existing Foundation 14 saves keep human-only warning behavior until you choose **Load Run → Enable warning attraction**. The upgrade preserves actor state, terrain, random streams and clock, stays paused and leaves the source file intact.

## Behavior

- The existing eight-step acoustic path budget applies. Solid walls block sound; doors and windows transmit it.
- Zombies retain the speaker's last heard cell for 25 ticks (five simulation seconds). No speaker identity, hidden current position or human stress is granted.
- Priority is **visible human → personally remembered human → most recent sound → occasional wandering**. If a door impact and warning share a deadline, the warning wins the tie. Both memories retain their own expiry.
- Reaching the heard warning cell clears that investigation. A later real warning can refresh it. Hearing alone does not permit door breaking; ordinary pursuit resumes only after a personal sighting.
- Warning cooldowns and human listener behavior remain unchanged. The older **Inspect Human Warning Paused** diagnostic retains its original rules to isolate human recovery; the new attraction scenario uses current rules.
- No voice audio clips are included. This is simulation hearing, shown in the inspector.

## Verification

Production-core checks cover closed-door delivery and approach, blocked-door safety, acoustic walls/range, sight and pursuit priority, newest-sound selection, exact expiry, arrival cleanup, pause and deterministic save continuation, invalid saved records and independent prototype-13 continuation. The UI harness covers the new scenario, body selection, heard-source inspector and explicit older-save upgrade with source-file preservation.

Validation passed: **114 core acceptance cases / 33,436 assertions**, **384 UI assertions** with legacy input, and the project configuration check.

Native Unity rendering, physical input and a Windows player still need local Unity testing.
