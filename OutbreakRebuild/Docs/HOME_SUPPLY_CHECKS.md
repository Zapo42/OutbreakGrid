# Foundation 105: home knowledge, consumer boundaries and testing controls

Target editor: **Unity 6000.6.3f1**. Native Unity testing remains paused after Foundation 82.

## Home checks

Open **Inspect Home Supplies Paused**. The selected human is inside an assigned residence, but its chest is initially outside sight. The inspector starts with one unknown box and zero known meals. Run or step: the human approaches its own storage, observes six meals, and remembers the actual stock. Checking does not transfer food. Current target markers continue to show the shelter/inspection goal.

Home-supply knowledge uses the existing saved personal box observations. It never replaces an away resident’s remembered quantity with the current hidden inventory. The inspector separates recently checked, unknown, stale, and reported-but-not-personally-checked boxes. Only recent first-hand observations contribute to the shown known-food total; zero known food with unknown/stale boxes does not mean the home is certainly empty. Check the counts beside it. Notes become stale after 300 ticks (30 game minutes); normal six-game-hour memory expiry still applies.

An idle resident safely inside its home approaches reachable unknown/stale/reported storage. Sight can establish stock before adjacency. Hunger with carried food, low energy, infection, warnings, stress, visible threats and active deliveries take precedence. Inaccessible furniture does not grant stock knowledge. Checks reuse normal movement and consume normal movement energy; they do not teleport or create food. The existing bounded eight-box memory remains in effect.

Existing assigned-home return behavior brings safe consumers home. Routine rest destinations prefer their assigned residence; unassigned visitors do not acquire a home automatically. Ordinary surplus pantry deposits by assigned residents now stay in their own home, rather than restocking whichever workplace they happened to enter. Explicit delivery destinations remain supported. Clerk and courier shifts/commutes are still separate upcoming work; home inspection does not interrupt active delivery tasks.

## Distribution boundaries

Ordinary consumers no longer choose distribution interiors/shelves/boxes for routine food exploration, food pickup, calm rest or wandering shortcuts. Routing and the final movement check both respect this. Store shelves remain consumer-facing. Existing store/center stock-box protection remains active.

Authorized center staff and actors with a real delivery source/destination retain access. Existing saves that explicitly assign a residence inside a center retain that residence. Emergency refuge during flight, infection, visible danger, warning or alarm remains possible; it does not grant ordinary consumers permission to take distribution food. An actor already inside may leave safely.

This is an NPC planning/access rule, not a physical locked-door system. It does not make other residential storage private, create stores, assign new homes or implement a new shopping transaction. The next increment is a deliberate home-shortage → store shelf → home deposit trip with intelligence affecting planning.

## Speed buttons

The toolbar now offers **5x, 10x, 20x and 50x**, alongside 0.5x/1x/2x/3x. The active setting has an asterisk. Selecting a speed while paused stays paused. Running uses the same exact 0.20-second logical ticks and advances every system normally.

Work remains bounded to ten ticks per rendered frame; unprocessed time is retained for catch-up. The header says **Catching up** when substantial debt remains. Actual playback can fall behind the requested rate on slow frames or demanding scenarios; ticks are not skipped to fake speed. Pause deliberately clears playback debt as before. Manual stepping remains exact and independent of the speed setting.

## Selected-NPC bars

Expand **Vitals & needs**. Use **Show health bar**, **Show hunger bar**, and **Show energy bar** beside the respective stats. Each toggles independently and is off initially. Enabled bars appear only on the selected living NPC, with **H** (health), **F** (hunger/food) and **E** (energy) beside them. Health is the first row, hunger second, energy third; disabled rows collapse. Humans show the enabled needs supported by the run. Zombies show health only. Corpses do not show need bars. Deselecting or selecting furniture hides the NPC bars.

The displayed hunger gauge now starts at **100 when fed** and falls toward **0 when starving**; eating fills it again. Numeric hunger, the F bar and hungry-NPC focus feedback use the same direction. Energy likewise shows remaining energy. The underlying deprivation, eating and save mechanics are preserved; this presentation uses `100 - actor.hunger` for compatibility with existing saved hunger debt. The deterioration warning now describes empty hunger and energy.

Bar choices are session view preferences, shared when switching the selection, and do not change NPC/save state. Drawing a bar or opening an inspector does not observe hidden food or change simulation state.

## Saved runs and validation

New scenarios use **encounter-prototype-84**. Earlier rules retain their previous home/consumer behavior until explicitly upgraded with **Enable home supply checks**. Save under another name to preserve an old run. Speed controls and selected bars are presentation controls and are available without upgrading behavioral rules.

Automated checks cover actual homeward travel/observation, hidden-stock independence, saved continuation, stale/reported knowledge, eating/safety interruptions, legacy consumer entry versus current boundaries, emergency/staff access, UI upgrades/inspection, all speed settings and exact 50x catch-up, and selected-only bar proportions/toggles. Native import, rendering, label layout and input in Unity 6000.6.3f1 remain unverified while Unity testing is paused.
