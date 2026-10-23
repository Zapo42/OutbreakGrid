# Foundation 83: pending turns overview

The encounter summary now distinguishes **Pending corpse reanimations** from **Zombies getting up**. A run with pending corpse reanimations no longer reports that infection is cleared, even when no living humans or active zombies remain.

**Soonest turn** pauses and centers the camera on the infected human or corpse with the earliest future conversion deadline. Equal deadlines use stable actor identity order. It clears placement tools and other object selections without changing simulation state. The button is disabled when no eligible pending turns remain. Existing **Next infected human** continues to cycle living infected humans.

Use **Inspect Rising Zombie Paused**, then **Soonest turn**, and step to see pending corpse counts become get-up counts. Restart restores the original pending state. Old runs without corpse reanimation rules do not count dead humans as future zombies.

This is a presentation update: no rules-version, infection timing or save format changes. UI checks cover earliest deadline, equal-deadline ties, camera and pause, state preservation, corpse-only outbreaks, dead bodies without infection, conversion refresh and restart.

**Unity validation: untested.** Foundation 82 remains the [last user-tested checkpoint](BUILD_VALIDATION.md). Automated checks do not establish native Unity behavior.
