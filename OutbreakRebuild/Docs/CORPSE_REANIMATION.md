# Foundation 80: Infected corpse reanimation

Infected humans killed by mauling now retain their original infection deadline. They remain motionless corpses until that deadline, then become active zombies at **50/100 health**. Death no longer cancels incubation. A lethal first bite also starts a stat-based infection deadline so that victim can later rise.

Reanimation does not restore the corpse as a living human. Human reactions, inventory, energy and grapple state stay cleared. A dropped bat remains on the ground and is not duplicated or returned to the reanimated zombie. The existing conversion path updates occupancy, population history and health bars.

The inspector shows **Reanimation in: ...s** on a dead infected human. The encounter summary now labels currently dead human bodies as **Human corpses**; that count falls when one rises. The existing game-hour infection countdown also remains visible. Day-paced infections continue on the same long timer after death.

## Try it

Choose **Inspect Reanimation Paused**:

1. Step 10 ticks: the human is dead, with two simulation seconds left on its timer.
2. Step another 10 ticks: at tick 20 it becomes a zombie with 50 health.
3. Restart restores the original human, health and deadline.

The ordinary **Inspect Mauling Paused** demo also now reanimates its dead victim at its original tick-100 deadline.

## Existing saves

New runs use encounter rules 65. Foundation 79 saves offer **Enable corpse reanimation**. Infected living humans keep their recorded deadlines when upgraded. Foundation 79 already discarded the timers on dead humans, so the upgrade assigns those corpses a fresh deadline using their strength, endurance, luck and the saved infection pace. That repaired starting state also survives restart. Earlier upgrade buttons enable current behavior too.

Saves using older rules retain their recorded behavior until explicitly upgraded. No new map or actor fields are required: a dead human can now retain the existing future `turnAtTick` field, with the same deadline validation as a living infected human. Other active corpse state remains invalid.

Automated checks cover exact post-death timing, pause, save/resume, lethal first bites, long game-day countdowns, conversion health, bat conservation, earlier mauling behavior, old-corpse upgrade/repair and restart. UI checks cover the countdown, corpse count, converted health and upgrade through narrow Unity API doubles. Native Unity appearance and input still need editor testing.
