# Foundation 79: Grapples, lethal mauling and infection pacing

Zombies no longer stop damaging a human after the first infection. The old bite handler returned immediately for infected victims and clamped human health above zero. Current rules remove both protections: living infected humans remain valid pursuit targets and can be mauled to death when trapped.

## Contact and escape

A successful initial bite does 10 damage and starts infection. Subsequent contact attacks do 15 damage without resetting the infection deadline. These attacks use the zombie's ordinary five-tick opportunity. The first bite can still be dodged under the existing evasion rules.

Successful contact briefly grabs the human for three ticks. On a normal decision while held, the human can spend five energy to struggle free; success lets it continue its escape decision immediately. The provisional chance is `15 + 4×strength + 2×endurance + luck − 2×zombie strength − stress/10`, bounded to 5–90%. Insufficient energy prevents a struggle attempt. Timer expiry, separation, loss of contact, attacker death or attacker stagger releases a grab. An ally's shove can therefore free a victim. Sprint bonus steps cannot bypass a live hold.

Infected humans facing a zombie within two cells prioritize their existing escape behavior before supply or shelter routines. A human with a viable route can keep moving after being bitten; a truly blocked victim can still be killed. Corpses no longer provide social support, compete for bats or receive shelf reports. Equipped bats drop with their remaining condition. Human deaths appear in the encounter summary.

Mauling deaths remain dead human bodies in this build; their infection deadline and active behavior are cleared, so they do not later reanimate. Living humans still convert when their deadline is reached.

## Converted health

At conversion, zombie health becomes `max(50, remaining human health)` on the existing 100-point scale. Thus a survivor at 80 health becomes an 80-health zombie; a survivor at 20 becomes a 50-health zombie. Zombies now show their own health bars even at full health. Deaths cannot receive this conversion health floor.

## Infection time

The prototype delay is `50 + 3×strength + 5×endurance + 2×luck` ticks, or 10–30 simulation seconds across supported stat ranges. Higher strength, endurance and luck each extend the incubation time. Repeated attacks never restart it.

**Set Up Encounter** now offers an **Infection pace** button:

- **Prototype:** short delays for testing.
- **~1 game day:** multiplies the delay by 144.
- **~3 game days:** multiplies it by 432.

With each relevant stat at five, these last exactly one or three game days; other stats give shorter or longer times. These use the existing simulation game clock, not real-world days. The setting, deadline and random stream persist in saves and scenario restarts. Existing infections retain their recorded deadlines when an old run is upgraded.

## Try it

- **Inspect Grapple Escape Paused → Run:** a strong human is grabbed, breaks free and keeps fleeing while infected. Zombie reaching hands, forward lean and victim recoil use the same presentation style as shoves.
- **Inspect Mauling Paused → Run:** a trapped infected human starts with 45 health, drops to 30 and 15 on successive attacks, then dies at tick 12. The zombie does not ignore the infection.
- **Inspect Turned Health Paused → Step 1 tick:** a human at 20 health becomes a zombie at the 50-health floor with its own health bar.
- In a configured encounter, choose **Infection pace: ~1 game day** or **~3 game days** before starting to use longer incubation periods.

New runs use encounter rules 64. Foundation 78 saves keep rules 63 until **Enable grapples and mauling** is clicked. Earlier upgrade buttons enable current behavior too. Old saved runs retain their original single-bite and conversion behavior until upgraded. New optional fields record grapple ownership/deadline and infection pace; malformed, stale and incompatible state is rejected before replacement.

Automated checks cover repeated infected damage and lethal cleanup, continued pursuit and escape, successful/failed struggles, ally rescue, grapple save continuation, bat drops, inherited conversion health, stat-based bite deadlines, day pacing, invalid saves and prior-rule compatibility. UI checks cover grab hands, corpse/hold/health displays, visible zombie health bars, new setup pacing, upgrades and restarts using narrow Unity API doubles. Native Unity rendering, input and pacing still need editor testing.
