# OutbreakGrid

Current build: **Foundation 8 / shelter recovery**. Humans recover to calm, inspect a real exit and return outdoors when clear; visible danger sends them back inside. See [test scenarios and update instructions](OutbreakRebuild/Docs/SHELTER_RECOVERY.md).

Unity 6 Outbreak Simulator rebuild. Current update: **Shelter recovery and safe exits**, extending room goals, capacity reservations, human door actions and unsafe-room reassessment. Bitten escape remains active. Stress, fight/freeze/flight, tick buttons and Main Menu remain available.

## Download and run

1. Click the green **Code** button above, then **Download ZIP**.
2. Extract into a new folder to preserve your existing projects.
3. In Unity Hub choose **Add project from disk** and select **OutbreakRebuild**, the folder containing Assets, Packages and ProjectSettings.
4. Open with Unity 6, then choose **Outbreak > Rebuild > Open and Run Foundation**.
5. Choose **Inspect Shelter Recovery Paused**, then **Step 50 ticks** and **Step 10 ticks** twice to watch inspection and departure. Try **Inspect Guarded Exit Paused** to see the human refuse a guarded exit, or **Start Encounter** for the mixed arena.
6. During simulation, use **Main Menu** to pause and return; **Resume current run** preserves it. Step **1, 10, 50 or 100** ticks while paused.

If buttons draw but do not respond, stop Play mode, set Active Input Handling to **Both** in Player settings, then restart Unity when prompted.

[Full launch instructions, provisional rules and validation](OutbreakRebuild/Docs/SHELTER_RECOVERY.md)

New saves retain room goals, recovery/exit progress, door states, reactions, infection and pursuit memory. Older saves keep their rules; Foundation 7 runs can opt in with **Enable shelter recovery**. Existing open maps need authored room geometry; the new encounter includes it. Collision remains grid-based. Automated C# checks pass, including save/resume and prior-release compatibility. Native Unity rendering and Windows builds are not verified by those checks.
