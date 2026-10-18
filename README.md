# OutbreakGrid

Doorway pursuit fix (Foundation 7): zombies follow their last sighting up to closed doors. Load older shelter saves and press **Update doorway pursuit**, or start a new encounter. See [details](OutbreakRebuild/Docs/SHELTERS_AND_DOORS.md).

Unity 6 Outbreak Simulator rebuild. Current update: **Shelters and doors**, adding reachable room goals, capacity reservations, human door opening/closing and unsafe-room reassessment. Bitten escape remains active. Stress, fight/freeze/flight, tick buttons and Main Menu remain available.

## Download and run

1. Click the green **Code** button above, then **Download ZIP**.
2. Extract into a new folder to preserve your existing projects.
3. In Unity Hub choose **Add project from disk** and select **OutbreakRebuild**, the folder containing Assets, Packages and ProjectSettings.
4. Open with Unity 6, then choose **Outbreak > Rebuild > Open and Run Foundation**.
5. Choose **Start Encounter** for the new two-room arena, or **Inspect Shelter Entry Paused** and use **Step 10 ticks** twice to see entry and closing behind.
6. During simulation, use **Main Menu** to pause and return; **Resume current run** preserves it. Step **1, 10, 50 or 100** ticks while paused.

If buttons draw but do not respond, stop Play mode, set Active Input Handling to **Both** in Player settings, then restart Unity when prompted.

[Full launch instructions, provisional rules and validation](OutbreakRebuild/Docs/SHELTERS_AND_DOORS.md)

New saves retain room goals, door states, reactions, infection and pursuit memory. Older saves keep their rules; prototype-five runs can opt in with **Enable shelter behavior**. Existing open maps need authored room geometry; the new encounter includes it. Collision remains grid-based. Automated C# checks pass, including save/resume and prior-release compatibility. Native Unity rendering and Windows builds are not verified by those checks.
