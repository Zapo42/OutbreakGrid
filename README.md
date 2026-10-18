# OutbreakGrid

Unity 6 Outbreak Simulator rebuild. Current update: **Cautious movement**, keeping wary humans at a safe distance and reducing movement pauses and unnecessary zigzags. Stress, fight/freeze/flight, tick buttons and Main Menu remain available.

## Download and run

1. Click the green **Code** button above, then **Download ZIP**.
2. Extract into a new folder to preserve your existing projects.
3. In Unity Hub choose **Add project from disk** and select **OutbreakRebuild**, the folder containing Assets, Packages and ProjectSettings.
4. Open with Unity 6, then choose **Outbreak > Rebuild > Open and Run Foundation**.
5. Choose **Start Encounter**, or inspect Fight/Freeze/Flight paused and use **Step 1 tick** to observe the selected response.
6. During simulation, use **Main Menu** to pause and return; **Resume current run** preserves it. Step **1, 10, 50 or 100** ticks while paused.

If buttons draw but do not respond, stop Play mode, set Active Input Handling to **Both** in Player settings, then restart Unity when prompted.

[Full launch instructions, provisional rules and validation](OutbreakRebuild/Docs/MOVEMENT_UPDATE.md)

New saves retain reactions, stress, support, freeze timing and pursuit memory. Older saves keep their rules; prototype-three reaction saves can explicitly opt into the correction using **Use cautious behavior**. Collision remains grid-based. Automated C# checks pass, including save/resume and prior-release compatibility. Native Unity rendering and Windows builds are not verified by those checks.
