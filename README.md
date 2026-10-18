# OutbreakGrid

Current build: **Foundation 14 / human warnings**. Humans who see zombies warn nearby people, delaying shelter exits without revealing hidden zombie locations. See [test and update instructions](OutbreakRebuild/Docs/HUMAN_WARNINGS.md).

Unity 6 Outbreak Simulator rebuild. The configurable encounter includes the small two-room arena and a 100×100 district with twelve rooms. Pursuit, infection, stress, fight/freeze/flight and shelter recovery remain active; idle zombies can wander, pursuing zombies can breach doors, and nearby humans react to heard impacts, and idle zombies investigate them.

## Download and run

1. Click the green **Code** button above, then **Download ZIP**.
2. Extract into a new folder to preserve your existing projects.
3. In Unity Hub choose **Add project from disk** and select **OutbreakRebuild**, the folder containing Assets, Packages and ProjectSettings.
4. Open with Unity 6, then choose **Outbreak > Rebuild > Open and Run Foundation**.
5. Choose **Set Up Encounter**, select **Shelter district (100 x 100)**, enter your human/zombie counts and click **Start configured encounter**. It starts paused by default; click **Run** or use tick buttons. **Start Encounter** remains the quick 24-human/one-zombie start.
6. During simulation, use **Main Menu** to pause and return; **Resume current run** preserves it. Step **1, 10, 50 or 100** ticks while paused.

If buttons draw but do not respond, stop Play mode, set Active Input Handling to **Both** in Player settings, then restart Unity when prompted.

[Full launch instructions, setup limits and validation](OutbreakRebuild/Docs/ENCOUNTER_SETUP.md)

New saves retain room goals, recovery/exit progress, door states, reactions, infection and pursuit memory. Older saves keep their rules; Foundation 12 runs can opt in with **Enable zombie sound**. Existing open maps need authored room geometry; the new encounter includes it. Collision remains grid-based. Automated C# checks pass, including save/resume and prior-release compatibility. Native Unity rendering and Windows builds are not verified by those checks.
