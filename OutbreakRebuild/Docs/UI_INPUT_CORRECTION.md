# Simulator controls: input and GUI event correction

Reported symptoms: the map appears but simulator buttons do not respond, an AudioListener is deleted, and the full editor exception runs through SearchDatabase.EnumerateAll → GetDefaultSearchDatabase → SearchInit.IndexationOnStartup → EditorApplication.Internal_CallDelayFunctions. No Outbreak frame appears in that stack.

| Observation | What is established | Current correction or next evidence |
| --- | --- | --- |
| AudioListener deleted because a built-in package is disabled | The supplied manifest omitted the real Audio module | Restore Audio in the complete manifest; a component already removed from a saved scene is not reconstructed automatically |
| SearchDatabase throws during editor startup indexing | The trace identifies Unity's startup-indexing path | No confirmed exact cause or engine repair; this source no longer creates/saves scenes automatically during editor startup |
| All runtime controls appear inactive | The user reports native failure; earlier host tests did not establish native input | Correct the confirmed paused-host queue defect and collect native input counters with Show Input Diagnostics |

Use the current **Foundation 1 / startup repair 2** archive in a newly extracted empty folder to isolate old scripts and generated caches without deleting previous work. A fresh folder is an isolation step, not proof that the SearchDatabase bug is fixed. The user's exact Unity version and actual current input/pause state remain unobserved until a native report is supplied.

## Input requirement

This temporary shell uses runtime `OnGUI`/IMGUI. Unity documents that runtime OnGUI does not receive input events when Active Input Handling is set to Input System Package (New). The screen can therefore draw while all its controls appear dead. This is a plausible explanation of the reported all-buttons symptom; the user's actual Player setting has not been observed.

Stop Play mode, choose **Edit → Project Settings → Player → Other Settings → Configuration → Active Input Handling → Both**, and restart Unity. Some versions place the setting directly under Other Settings. Then use **Outbreak → Rebuild → Open and Run Foundation** and choose **Start Movement Test** in the Game tab.

Official reference: <https://docs.unity3d.com/Packages/com.unity.inputsystem@1.19/manual/UISupport.html#immediate-mode-gui>.

## Source changes

- Restore the documented Audio, Image Conversion, IMGUI and JSONSerialize baseline. The earlier IMGUI-only manifest disabled normal audio support; the still earlier `com.unity.modules.inputlegacy` name was invalid. Input backend selection belongs in Player settings.
- Check current compiled input capability before the supplied editor launch and Windows build actions.
- In a New-only process, show the exact recovery steps instead of drawing inert controls.
- Preserve Player preferences and avoid reflection into Unity 6 build-profile settings or Unity Search.
- Process accepted GUI commands through the editor loop as well as player Update, after the active GUI event finishes. Unity Editor pause must not strand a click. The callback does not use the startup delayCall chain shown in the reported Search exception. Explicit Run resumes host pause; Step 10 can run while host pause stays on. Console Error Pause preferences are not disabled or hidden.
- Cancel scheduled callbacks when the component is destroyed, and ensure editor/player pumps cannot execute a command twice.
- Remove automatic scene writes during editor startup; create the entry scene only after an explicit menu action.
- Add **Outbreak → Rebuild → Show Input Diagnostics**. Attempt a click, then select that editor menu to copy the actual Unity/build version, live owner count, pause state, received mouse-event counts, queued/executed action counts and any action exception. This menu does not call Unity Search.
- Provide an explicit start screen. Start Movement Test starts the 24+1 fixture immediately; paused inspection is a separate choice.
- Explain empty terrain-only loads, disable meaningless empty-population Run/Step actions, and provide baseline human/zombie placement.

## Validation limits

The production C# core and file persistence tests pass. The runtime component, input guard and native-menu report compile against a narrow UI API double in Legacy and New-only configurations. The harness dispatches real start, pause, step, speed, load, save, placement, cancellation and reset callbacks, checks post-GUI dialog dispatch, and reproduces a paused host with no simulation Update. It checks exactly-once editor dispatch, explicit Run resume, paused stepping, report copying and cleanup. A separate configuration check rejects missing or unreviewed module declarations against the documented baseline.

The harness does not execute Unity, render actual layout, inject native mouse events, run a Windows player or reproduce the SearchDatabase exception. These remain native verification tasks; code-level checks are not a claim that the reported machine is repaired.
