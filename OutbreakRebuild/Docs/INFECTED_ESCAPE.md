> The recommended shelter milestone is now implemented in bounded prototype form. See [SHELTERS_AND_DOORS.md](SHELTERS_AND_DOORS.md) for the current build, room recognition, door actions, limits and native verification boundary.

# Bitten-human escape update

Current build: **Foundation 5 / bitten escape**, rules `encounter-prototype-5`.

## What changed

Infection previously left the normal courage logic active, so a stressed, brave or supported bitten human could still choose Fight. In the new rules, a real bite immediately sets escape priority, cancels a freeze and records the attacker's observed cell. Subsequent human opportunities override Fight, Freeze and ordinary wandering until conversion. The original infection deadline, stable identity and health rules are unchanged; bitten humans do not begin transmitting before turning.

The escape planner examines reachable cells in a bounded nine-by-nine local region. It respects solid corners, crowd limits and zombie blocking. Candidate paths cannot enter cells closer than the start to any currently observed zombie; this permits lateral detours around cover without advancing into a threat. On each later decision the distance boundary is recalculated, so each committed first step remains non-approaching relative to the information available then. If no safe improvement exists the human holds rather than charges.

Tactical scores favor actual sight occlusion behind terrain, then an observed human between the destination and the primary threat, then more clearance and shorter routes. Human screens are a **soft positioning preference**, not solid walls or guaranteed protection. Only nearby humans visible to the actor contribute; the planner cannot discover unseen people through walls. It also avoids choosing a route safe from one visible zombie that approaches another.

After losing sight, the human avoids the last observed zombie cell for 25 ticks (five simulation seconds). It does not track the hidden zombie's actual position. Expired memory clears on the exact simulation tick; without a new known threat the bitten person stays put instead of randomly wandering back into danger. Pause freezes memory and infection alike. Turning clears all human-only escape state.

A newly bitten actor's visual segment synchronizes to its committed location to stop displaying a stale pre-bite approach. Normal movement retains the full-cadence presentation from the preceding update. The simulation is still grid-based; this is local tactical cover selection, not global shelter navigation or a complete hiding/knowledge system.

## Try it

Download the latest GitHub ZIP, open **OutbreakRebuild** in Unity 6 and choose **Outbreak > Rebuild > Open and Run Foundation**.

- Start a new encounter to use the correction immediately.
- **Inspect Bitten Escape Paused** provides a controlled brave, already-bitten human near a wall. Select human #1 and Step 1 tick; it moves behind the wall instead of advancing toward the zombie.
- For an existing prototype-four run, click **Update escape behavior** in the simulation toolbar. Older prototype-three reaction runs retain the **Use cautious behavior** upgrade button. Both explicitly upgrade the run while preserving identities, clock, random streams and infection deadlines, and leave it paused. The source save file is untouched; save to a new name to retain both versions.
- Original saves continue under their original rules until explicitly upgraded. Prototype-one/two runs still require a new encounter for the current human reaction features.

The prototype-four compatibility fixtures were produced by executing commit 45ce8e8b0b8724923d62d6616d2d4ec692c790bb. Fixture: 11x9 open map, seed 214, human (5,4), zombie (8,4), human bravery 3, stress 50 and turnAtTick 80. Capture after one step and after another 100. These expectations were not generated from prototype-five source.

## Validation and next milestone

The executed suite passed **45 acceptance cases / 22,572 assertions**, plus **125 Legacy UI assertions and 3 New-only input assertions** (unsupported input correctly rejected). The module-baseline check passed. The production C# suite covers infection overriding courage/freeze, a real bite applying escape immediately, reachable wall cover, an intervening human, multiple observed zombies, sight-loss memory, pause, exact expiry, conversion cleanup, corrupted-save rejection and prior-release continuation. UI checks exercise the new scenario, exact stepping, and explicit update of a preceding-version run without modifying the original file. See INFECTED_ESCAPE_CHECKS.txt for the current executed report and benchmark scope.

The UI harness uses narrow API doubles. Native Unity layout, visual motion, mouse input, operating-system dialogs and a Windows executable remain unverified here.

The recommended next milestone is **shelter destinations and door use**: evaluate reachable rooms/buildings, keep a chosen safe destination instead of only local cover, manage occupancy, and use doors to break pursuit. That builds on this local escape correction; it has not been implemented in this update. Full grabbing, escape contests and weapon combat also remain later work.
