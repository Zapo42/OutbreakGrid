# Foundation 60 / survivor health overview

The simulator sidebar now shows injured and infected human counts. **Next injured human** pauses and focuses on each living human below full health. **Next infected human** does the same for humans awaiting conversion. Both cycle in stable identity order and wrap around, with unavailable buttons disabled.

Selecting a human shows exact health and infection status, including time until conversion in seconds. These inspection tools clear placement tools and other selections. They do not change health, infection, inventory or simulation state. Zombies and defeated bodies are excluded from survivor counts.

Choose **Inspect Health Bars Paused**, then **Next injured human**, to focus on the injured survivor. **Inspect Weapon Drop Paused** provides an infected human to inspect before stepping through conversion. Counts and button availability update as actors turn. Restarting restores the original scenario and its survivor counts.

This presentation update requires no save upgrade. Automated UI checks cover counts, camera focus, stable wraparound, pausing a running simulation, exact infection status, unchanged simulation state during inspection, conversion, restart and empty-group controls. Native Unity appearance and desktop input require local verification.

Validation: 901 UI harness assertions plus 3 with New Input only. Project configuration and whitespace checks passed.
