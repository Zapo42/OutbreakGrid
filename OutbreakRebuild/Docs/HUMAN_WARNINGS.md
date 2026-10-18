# Nearby human warnings

Current build: **Foundation 14 / human warnings**, rules `encounter-prototype-13`.

A human who personally sees a zombie warns nearby humans on a normal decision opportunity, at most once every ten simulation seconds. Listeners become aware and gain eight stress points. Hearing cancels shelter departure and delays recovery for five seconds, then quiet recovery resumes.

## Try it

1. On GitHub choose **Code → Download ZIP**, extract to a fresh folder and open **OutbreakRebuild** in Unity 6.
2. Choose **Outbreak → Rebuild → Open and Run Foundation**.
3. Choose **Inspect Human Warning Paused**, then click human **#1** inside the small room.
4. Click **Step 10 ticks**. The inspector shows a warning from **6, 2**, outside the closed door. The sheltered human has stress despite never seeing the zombie.
5. Click **Step 10 ticks** four more times. The warning expires, stress recovers and the human begins its normal calm shelter wait.

New encounters enable warnings automatically. For a Foundation 13 save, choose **Load Run → Enable human warnings**. The run stays paused, preserving actors, doors, random state and clock. Save under a new name to retain the original file.

## Rules and limits

Warnings share the door-impact acoustic model: at most eight cardinal steps, solid walls block paths, and doors/windows transmit. Only current personal sight can produce a warning; hearing alone cannot relay it. Each listener remembers the speaker's cell rather than the zombie's position or identity. Warnings preserve existing flight, freeze, fight, infection and personal avoidance knowledge. Visible threats still take priority. Zombies currently respond to door impacts, not human warning cues; no voice audio is included.

All warning memory and cooldowns use simulation ticks, pause with the world and survive save/load. Turning clears human communication state. Older saves retain their prior rules until explicitly upgraded. Controlled human reaction diagnostics disable warnings to keep their isolated courage examples stable.

## Verification

The portable production-core suite covers actual sight-triggered delivery through a closed door, acoustic range and enclosing walls, absence of hidden target knowledge or relaying, exact expiry, quiet recovery, pause, deterministic save continuation, conversion cleanup and invalid saved memory. Independent prototype-12 fixtures were generated from commit `a4bd4f8339eca6db6a70224559983c62f8d538f0` before adding warnings. The UI harness exercises the menu, selection, inspector, tick buttons and explicit prior-save upgrade with source-file preservation.

Validation passed: **107 core acceptance cases / 33,301 assertions**, **376 UI assertions** with legacy input and **3** with New-only input, plus the project configuration check.

These checks compile production C# against a narrow Unity API double. Native Unity rendering, input and a Windows player still require testing in Unity.
