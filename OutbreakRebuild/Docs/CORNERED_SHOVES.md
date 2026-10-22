# Foundation 78: Cornered defensive shoves

A healthy human whose normal decision is Flight now attempts a defensive shove when an adjacent visible zombie leaves it no legal retreat step. The human remains in **Flight**. A legal retreat takes priority over shoving, including one that becomes available after a previous push.

This reuses the existing physical shove: **30 energy**, a **10-tick cooldown**, a **3-tick stagger**, and a one-cell push directly away when terrain and zombie occupancy allow it. It does no damage. A wall can stop displacement while the brief stagger still applies. The shove consumes the human's action; it does not also move the human.

Low energy, an active shove cooldown, an already staggered target, unresolved freezing or an active melee cooldown can prevent the shove. Infection continues to use the existing infected escape behavior. A fleeing human holding a bat can shove defensively without swinging the bat or switching to Fight. Other food, shelter and survival priorities retain their existing order; this fallback applies when the ordinary Flight decision reaches local retreat.

A shove buys time rather than guaranteeing escape. A permanently blocked human can still be bitten as the cooldown and stagger expire or its energy runs low. The existing animation displays body contact and the push. The selected-human panel shows the defensive shove cooldown.

## Try it

Choose **Inspect Cornered Shove Paused**, then **Run** or step:

- Tick 1: the frightened human cannot retreat, so it pushes the zombie from x2 to x3 and spends 30 energy while remaining in Flight.
- Tick 6: it cannot repeat the shove during cooldown.
- Tick 11: it can try again if still cornered and sufficiently energized.

Continue to see why repeated pushes do not provide permanent safety. Restart restores the original positions, full energy and ready cooldown.

New runs use encounter rules 63. Foundation 77 saves keep rules 62 until **Enable cornered shoves** is clicked. Earlier upgrade buttons enable current behavior too. Actor and map formats are unchanged; existing energy, stagger, cooldown and reaction fields preserve exact save continuation.

Automated checks cover nonlethal pushes during Flight, energy/cooldown accounting, exact saved retries, available and newly opened retreats, infection/freezing/resource/cooldown limits, armed escape defense, blocked displacement, later bites, earlier footstep-rule compatibility, and UI demo/upgrade/restart controls. UI checks use narrow Unity API doubles; native Unity appearance and input require editor testing.
