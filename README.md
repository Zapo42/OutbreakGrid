# OutbreakGrid

Unity 6 Outbreak Simulator rebuild. Current update: **Encounter prototype 2**, adding last-seen pursuit memory and a controlled corner scenario.

## Download and run

1. Click the green **Code** button above, then **Download ZIP**.
2. Extract into a new folder to preserve your existing projects.
3. In Unity Hub choose **Add project from disk** and select **OutbreakRebuild**, the folder containing Assets, Packages and ProjectSettings.
4. Open with Unity 6, then choose **Outbreak > Rebuild > Open and Run Foundation**.
5. Choose **Start Encounter**, or **Inspect Corner Pursuit Paused** and use **Step 10 ticks** to watch a zombie search after a human rounds a wall.

If buttons draw but do not respond, stop Play mode, set Active Input Handling to **Both** in Player settings, then restart Unity when prompted.

[Full launch instructions, provisional rules and validation](OutbreakRebuild/Docs/ENCOUNTER_PROTOTYPE.md)

New saves retain pursuit memory; older encounter saves keep their original rules. Automated C# checks pass, including save/resume and prior-release compatibility. Native Unity rendering and Windows builds are not verified by those checks.
