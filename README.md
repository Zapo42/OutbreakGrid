# OutbreakGrid

Current build: **Foundation 131 / purposeful neighborhood walks**. Targets **Unity 6000.6.3f1**. Healthy, fed consumers can take short outdoor walks from assigned homes, pause, and return. Actual movement costs energy; urgent needs and safety take priority. [Walking guide and controlled preview](OutbreakRebuild/Docs/HUMAN_LEISURE.md) · [Vehicle shopping](OutbreakRebuild/Docs/CONSUMER_VEHICLE_SHOPPING.md) · [Development workflow](OutbreakRebuild/Docs/CITY_LIFE_AND_TRANSPORT_PLAN.md).

World integration adds opt-in seeded two-room house maps, living-area recovery markers and separate household storage. [Generation, loadable examples, compatibility and validation limits](OutbreakRebuild/Docs/WORLD_HOUSES_INTEGRATION.md). NPC Foundations 132–137 and their daily-routine behavior are not included in this Foundation131 integration.

**Unity testing checkpoint:** Foundation 82 is the last build the user reported working in Unity. Builds starting with Foundation 83 are untested in Unity until the user explicitly resumes testing. [Checkpoint, rollback download and build validation record](OutbreakRebuild/Docs/BUILD_VALIDATION.md).

[Current development preferences](OutbreakRebuild/Docs/DEVELOPMENT_PREFERENCES.md): incremental builds, next-step options, and food-only storage.

Unity 6 Outbreak Simulator rebuild. The configurable encounter includes the small two-room arena and a 100×100 district with twelve rooms. Pursuit, infection, stress, fight/freeze/flight and shelter recovery remain active; idle zombies can wander, pursuing zombies can breach doors, and nearby humans react to heard impacts, and idle zombies investigate them.

## Download and run

1. Click the green **Code** button above, then **Download ZIP**.
2. Extract into a new folder to preserve your existing projects.
3. In Unity Hub choose **Add project from disk** and select **OutbreakRebuild**, the folder containing Assets, Packages and ProjectSettings.
4. Open with Unity **6000.6.3f1**, then choose **Outbreak > Rebuild > Open and Run Foundation**.
5. Choose **Set Up Encounter**, select **Shelter district (100 x 100)**, enter your human/zombie counts and **Bats at start** (six by default, zero for unarmed) and click **Start configured encounter**. It starts paused by default; click **Run** or use tick buttons. **Start Encounter** remains the quick 24-human/one-zombie start.
6. During simulation, use **Main Menu** to pause and return; **Resume current run** preserves it. Use **Restart scenario** beside Main Menu to return to the same starting setup, paused. Step **1, 10, 50 or 100** ticks while paused. Food-enabled runs also offer **Step 1 game hour** in the food overview.

If buttons draw but do not respond, stop Play mode, set Active Input Handling to **Both** in Player settings, then restart Unity when prompted.

[Full launch instructions, setup limits and validation](OutbreakRebuild/Docs/ENCOUNTER_SETUP.md)

New saves retain room goals, recovery/exit progress, door states, reactions, infection and pursuit memory. Older saves keep their rules; Foundation 90–91 runs offer **Enable supply deliveries**; Foundation 89 runs offer **Enable box reports**; Foundation 88 runs offer **Enable box memory**; Foundation 87 runs offer **Enable reserve targets**; Foundation 86 runs offer **Enable restocking trips**; Foundation 82–85 runs offer **Enable storage boxes**; Foundation 81 runs offer **Enable reanimation awareness**; Foundation 80 runs offer **Enable reanimation transition**; Foundation 79 runs offer **Enable corpse reanimation**; Foundation 78 runs offer **Enable grapples and mauling**; Foundation 77 runs offer **Enable cornered shoves**; Foundation 76 runs offer **Enable sprint footsteps**; Foundation 75 runs offer **Enable escape sprints**; Foundation 74 runs offer **Enable low-energy returns**; Foundation 73 runs offer **Enable energy-aware food risk**; Foundation 72 runs offer **Enable food trip rest**; Foundation 71 runs offer **Enable beds**; Foundation 70 runs offer **Enable energy and rest**; Foundation 68/69 runs offer **Enable shove fatigue**; Foundation 67 runs offer **Enable unarmed spacing**; Foundation 66 runs offer **Enable combat noise**; Foundation 65 runs offer **Enable recovery food reserves**; Foundation 57–64 runs offer **Enable shelter healing**; Foundation 56 runs offer **Enable first weapon priority**; Foundation 55 runs offer **Enable first bat courtesy**; Foundation 54 runs offer **Enable supply courtesy**; Foundation 53 runs offer **Enable replacement trips**; Foundation 52 runs offer **Enable weapon replacement**; Foundation 51 runs offer **Enable combat footwork**; Foundation 50 runs offer **Enable bat durability**; Foundation 49 runs offer **Enable bite evasion**; Foundation 47/48 runs can opt in with **Enable drops and shelter spacing**; Foundation 46 runs offer **Enable combat experience**; Foundation 44/45 runs offer **Enable weapon seeking**; Foundation 42/43 runs offer **Enable melee combat**. Existing open maps need authored room geometry; the new encounter includes it. Collision remains grid-based. Automated C# checks pass, including save/resume and prior-release compatibility. Native Unity rendering and Windows builds are not verified by those checks.

## Development and automated checks

[Team roles and audited baseline](PLANS.md) · [Automated check commands and limits](OutbreakRebuild/Docs/AUTOMATED_CHECKS.md). `RESTORE42` preserves Foundation 130 before team setup. Automated C# checks are separate from native Unity and Windows player verification.
