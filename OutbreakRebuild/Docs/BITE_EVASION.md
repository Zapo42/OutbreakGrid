# Foundation 50: evading contact bites

Humans now have a chance to evade a zombie's contact bite. Higher human agility improves the chance; zombie agility and human stress lower it. The provisional chance is `10 + 5 × human agility − 2 × zombie agility − stress / 4`, using integer division and clamped to 0–60%. Frozen humans have no chance to evade until their freeze expires. Bats do not grant an automatic dodge bonus.

A dodge consumes the zombie's normal attack opportunity without damage, infection or movement. There is no teleport or extra escape step. A bite that connects still deals ten internal health points, starts the existing endurance-based infection countdown and triggers infected escape. Subsequent contact with an already infected human does not deal another initial bite, reroll evasion or reset the deadline. Range, walls, solid corners, defeated attackers and the one-second movement/attack cadence keep their existing rules.

Each eligible attempt with a nonzero chance uses one draw from the saved reaction random stream. Frozen or zero-chance targets are bitten without a dodge draw. Rendering does not draw randomness. Saving immediately before or after a dodge preserves the exact future simulation.

## Feedback and controlled tests

A dodge briefly flashes the human blue; a connected bite flashes red. These flashes last 0.3 seconds at normal speed and follow the rendered body. The selected human's inspector shows its agility and stress and, for two simulation seconds, the last attack result, attacker identity and the actual chance used for that attempt. Pausing freezes feedback. Loading clears transient feedback rather than replaying a past attack; health, infection, positions and random continuation remain saved.

1. Open the latest **OutbreakRebuild** project and choose **Outbreak > Rebuild > Open and Run Foundation**.
2. Confirm **Foundation 50 / bite evasion** appears on the menu.
3. Choose **Inspect Dodged Bite Paused**, then **Step 1 tick**. The agile human has a 54% chance and the seeded attempt is dodged. Its health stays at 100 internally and it remains uninfected. The inspector says **DODGED**.
4. Choose **Inspect Successful Bite Paused**, then **Step 1 tick**. The same stats can fail: this seeded attempt connects, leaves 90 health internally and starts infection. The inspector says **BITTEN**.
5. Run an ordinary configured encounter to see the same evasion rules alongside weapons, learned skill, equipment drops, hunger and shelter behavior.

These are deterministic demonstrations of both outcomes, not guaranteed protection for agile humans in general. Large paused tick batches skip intermediate flashes.

New runs use encounter-prototype-43. Existing Foundation 49 saves retain encounter-prototype-42 and guaranteed initial contact bites until **Enable bite evasion** is clicked. Upgrading preserves the map, actors, ticks and random states and leaves the run paused. Save under a new name to keep the earlier file. Older upgrade controls still enable the current complete rules.

Validation: 283 simulation/presentation cases / 320,758 assertions; 762 legacy-input UI assertions and 3 NewOnly assertions. Coverage includes actual hits and dodges, agility/stress bounds, frozen targets, unchanged infection deadlines on repeated contact, walls and defeated attackers, exact save continuation, older rules, per-tick event clearing, inspector outcomes, body flashes and their expiration, load reset and explicit upgrade. Project configuration and whitespace checks passed.

Native Unity visuals and input require local testing; the UI harness uses narrow Unity API doubles. No Windows executable was built here.
