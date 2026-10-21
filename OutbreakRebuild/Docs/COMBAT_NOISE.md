# Foundation 67 / combat noise

Successful bat hits now produce a local impact that nearby zombies can investigate. Missed swings, pickups and swaps remain quiet. The source is the actual impact cell before knockback, including fatal hits and a bat's final hit before breaking.

Noise uses the existing local hearing model: up to four cardinal travel steps, with walls blocking propagation and doors transmitting it. A zombie already seeing a human retains its visual awareness. Other listeners learn the impact location, not a hidden human's position. The hearing expires after 25 ticks and cannot by itself authorize door breaking or extend visual pursuit memory.

Choose **Inspect Combat Noise Paused**, then **Step 1 tick**. A fatal bat impact attracts the zombie behind the closed door. The inspector and amber target marker identify **Heard combat**. Step ten more ticks: the listener approaches the door without damaging it or acquiring the hidden human. Restart restores the original encounter.

Existing saves keep their original combat behavior until **Enable combat noise** is selected. Saved combat hearing resumes exactly under the new rules; earlier rules reject that unsupported hearing type.

Automated checks cover real fatal impacts, hidden listeners, walls, missed swings, visual priority, bounded expiry, non-destructive investigation, exact save continuation, older rules, inspector labels, upgrade and restart. Native Unity appearance and desktop input require local verification.

Validation: 320 core cases / 320,909 assertions; 1,107 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
