# Foundation 77: Sprint footsteps

Each successfully completed extra sprint step now produces a local footstep cue at its destination cell. Zombies within **four cardinal sound-path steps** can hear it. Walls block propagation; doors transmit it. Ordinary walking and blocked sprint attempts do not produce this cue.

A listener remembers the heard cell for **25 ticks** (five simulation seconds). Later audible sprint steps update that location. Hearing does not identify the runner, follow its current hidden position, or authorize door attacks. Existing direct sight and active pursuit retain their higher priority. Zombies already seeing a human do not replace that visual target with the new footstep cue.

The selected zombie's target and activity labels identify **running footsteps**, separately from combat, doors and shelf searches. This update adds the simulation stimulus and investigation behavior; it does not add an audible footstep sound effect.

## Try it

Choose **Inspect Sprint Footsteps Paused**, then step one tick three times:

1. The human retreats normally from x8 to x7 on row 4.
2. The pursuing zombie approaches.
3. The human sprints to **6, 4**. The hidden zombie at **6, 7** hears that step and moves toward the closed door, reaching **6, 6**.

The selected listener shows **Heard running footsteps: 6, 4** and an investigation activity. Continue running: a later audible step can replace its remembered location, but it cannot break the door merely because it hears movement. Once no new steps are audible, footstep memory expires. Other independently heard warnings can still influence its behavior.

Restart restores the starting chase and clears hearing. New runs use encounter rules 62. Foundation 76 saves keep their quiet sprint rules (61) until **Enable sprint footsteps** is clicked. Earlier upgrade buttons enable current behavior too. Existing energy and scenario baselines survive upgrade and restart. The existing hearing fields now support kind 3; older kinds retain their meaning and prior saves retain their rules.

Automated checks cover actual sprint-cell emission, investigation through a closed door, no hidden pursuit or sound-only door damage, subsequent heard locations, quiet walking/blocked attempts, acoustic walls and range, direct sight priority, exact expiry, save continuation, malformed hearing and earlier sprint-rule compatibility. UI checks cover the demo, target/activity labels, restart and upgrade using narrow Unity API doubles. Native Unity visuals and input require editor testing.
