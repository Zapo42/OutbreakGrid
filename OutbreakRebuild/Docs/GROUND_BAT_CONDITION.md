# Foundation 58 / ground bat condition

Ground bats now fade from brown toward muted grey as their remaining hits decrease. A mixed pile uses its best bat's color, matching the bat that would be collected first.

Click a ground bat or use **Next ground bat** to pause and inspect the pile. A small white underline marks the selection. The inspector shows the quantity of bats at each condition, including mixed piles. If someone collects the last bat while stepping or running, the inspector reports that the selected pile is gone rather than showing stale stock. Selecting a human or shelf replaces the ground selection; restarting or loading clears it.

Choose **Inspect Bat Conditions Paused** to compare fresh, half-worn, nearly broken and mixed supplies. Cycle **Next ground bat** or click each bat to inspect its remaining hits. Ground inspection is a player view and does not grant humans knowledge or change their behavior.

Existing saves use their existing rules; this presentation update requires no rules upgrade. Runs predating durability retain the original brown color and explicitly show that condition is not tracked.

Automated checks cover color values sent to rendering, actual click dispatch, mixed condition breakdowns, unchanged simulation state while inspecting, finite pickup refresh, restart and older saves. Native Unity appearance and desktop input require local verification.

Validation: 310 core cases / 320,868 assertions; 873 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
