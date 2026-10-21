# Foundation 75: Return before exhaustion

Food-trip humans can now turn back toward their home shelter when energy runs low, even if no zombie has appeared. The check uses the existing guarded route home: its reserve is twice the route's step count plus a ten-point buffer, bounded to **20–40 energy**. This allows earlier returns on longer routes while leaving prepared humans enough energy to start another trip.

Urgently hungry humans with no food keep foraging; urgency uses the same hunger-and-bravery threshold as food-trip preparation. Hungry humans eat carried meals first and can finish searching a personally observed shelf already within reach. Danger can still force the existing retreat independently of energy.

Returns use real movement, door opening and crossing. When home is full or unusable, the existing guarded alternate-shelter choice applies. Once a return is committed, its saved shelter goal maintains the trip rather than switching direction at every threshold. Inside, normal food, rest and bed behavior resumes. The reserve is a planning estimate, not a guarantee against running out of energy after circumstances change.

## Try it

Choose **Inspect Energy Return Paused**, then **Run** or step:

1. At tick 1, the human with 18 energy commits to returning and opens its shelter door.
2. At tick 6, it enters the doorway.
3. At tick 11, it reaches shelter with 16 energy and closes the door.
4. At tick 16, safe ordinary rest brings energy to 26 for one hunger point.
5. Continue running: it prepares its energy reserve, then resumes foraging. Restart restores the starting outdoor position and needs.

The selected-human panel shows **Returning to shelter with low energy** while a tired human follows a return goal outside the room. This describes its current state, including returns also prompted by danger.

New runs use encounter rules 60. Foundation 74 saves retain rules 59 until **Enable low-energy returns** is clicked. Earlier upgrade controls enable current behavior. Save and map fields are unchanged; existing goals preserve return progress across saves and restarts.

Automated checks cover physical return, energy and hunger accounting, doorway save continuation, recovery, distance-based decisions, urgent hunger, carried meals, adjacent shelves, full-home fallback, old-rule compatibility, and UI demo/upgrade/restart controls. UI checks use narrow Unity API doubles; native Unity rendering and input require editor testing.
