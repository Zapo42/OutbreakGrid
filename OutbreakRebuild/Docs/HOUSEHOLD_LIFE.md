# Household life — first local prototype

Supply-chain work and shifts are shelved. This is the first furniture/needs/media increment from the user's new household interaction request. Relationship quality, liked-person compounding and invitations are **pending**, followed by multiple houses and greater distances. Existing greeting recognition is preserved; it is not yet a liking/disliking system.

## Try the household

Use **Inspect Household Life Paused** in Current Tests. The small house has two residents, a kitchen table with four chairs, a comfy living-room chair, a two-seat couch, a bed, a TV and a radio. Select a resident and open **Household activities**. The first resident begins hungry at the table; the second begins tired near the TV. Run at1× or use the existing paused step controls. All movement is committed by Core and pays the normal movement cost.

The panel lists that resident's home furniture and lets you toggle TV/radio on or off. Toggles change appliance state, not the simulation clock. Mood changes happen on the existing staggered120tick/12game-minute pulses, so switching a device while paused does not instantly award happiness. Restart reproduces the starting setup. Save Run/Load Run retains objects, device states and current activity. Object placement tools and rotated furniture are not part of this preview. The couch now spans two adjacent floor cells; each cushion is a separate usable seat.

## Eating

The user's explicit choice is that **a snack and a table meal each consume one whole food item**. Hunger0means fed;100means starving, so relief reduces the meter.

- Snacks relieve1–5 hunger according to endurance:0→1,5→3,10→5. An actor outside or without a usable table chair can snack.
- A meal at a chair beside a kitchen table relieves50 hunger, using the existing full-meal amount as an initial value.
- Safe residents with food seek a visible, reachable, free table chair in their current room. They walk there before consuming the item. A table or route does not provide food by itself.

These are initial tuning values for review. The deliberately small snack relief means eating away from tables is expensive under the selected whole-item cost; food is never silently split into portions.

## Resting and idle

A dining chair/comfy chair/couch/bed recovers3/6/6/12energy per normal actor opportunity. Each seat holds one body; a two-cell couch holds two people on separate cushions. Either cushion can be reserved, approached and used independently, so an occupied first cushion does not hide the second. The planner compares approximate recovery time including travel, so a small chair break can stay local while exhaustion can justify a faster nearby bed. TV preference can favor a suitable rest location when mood is low. Active jobs, vehicles, errands and danger retain priority; the initial prototype focuses on residents at home.

Standing idle neither restores nor spends energy. Normal time-based hunger continues; personality and nearby media can still change mood. Actual walking still costs energy. Recovering energy or the existing2health points pays the existing1hunger recovery cost. Hungry residents do not heal. A hungry empty-handed resident can recover energy at furniture if no usable food is personally known; visible food retains priority. Full-energy, healthy TV lounging does not add a recovery hunger cost.

Beds give **zero intrinsic happiness**. Radio, TV, personality/environment or company effects can still apply near a bed. Furniture does not add a separate comfort happiness bonus. NPC seated/lying animation poses are not implemented in this increment; the normal tiny human symbols remain, with presentation-only idle sway suppressed during actual furniture rest.

## Happiness

- A working radio within4cells adds2per pulse. This is proximity/hearing and does not require TV-style line of sight.
- A working TV within6cells adds2only during actual chair/comfy-chair/couch/bed rest with a clear view of its surface. Walls and corners obstruct it. Merely standing near a TV gives no TV gain.
- Device bonuses do not stack per extra identical device. On/off state is explicit; an electricity grid, audio playback and video content are not implemented.
- Existing charisma-based preferences are reused provisionally: high(>=6) recovers outdoors, with an additional bonus near a remembered park; low(<=4) recovers indoors and/or alone. Neutral5has neither preference bonus. Existing purposeful outing/park behavior remains.
- Known people, actual relationship quality and liked-company multipliers are different concepts. The latter two, plus home/outings invitations and consent/return handling, remain the next local stage.

## Compatibility and limits

Household maps opt into rules114. Original one-cell maps use schema15; new two-cell couches and comfy chairs require schema16. Old one-cell couches are not silently expanded, and their independently recorded rules114 continuation remains exact. **Existing city maps and saves are not automatically furnished or switched to snack/furniture-only recovery.** Old maps keep their prior needs policy; old saved generations remain exact until explicitly upgraded. Furniture mode remains enabled even if all its objects are removed. Older rule readers reject new household maps rather than discarding their objects. The map and actor data remain defensive, bounded and canonically saved.

Furniture uses one-cell symbolic geometry except for new two-cell couches. Wide couches validate both cells atomically, are captured once at their left-hand anchor, and are removed in full if terrain under either seat is replaced. The table+four chairs composite is atomic. Table/TV/radio bodies block movement; chairs/comfy chairs/couches/beds are usable floor cells with exclusive occupancy. Existing shelves, storage, rest markers, phones and weapons cannot overlap typed furniture. Existing doors, storage artwork, sprite tiers, controls, selected-only bars and collapsible organization are preserved.

Furniture art now uses solid seven-row silhouettes, a two-cushion couch and a compact radio speaker/tuner with an aerial. Integer shared drawing edges and contiguous color runs remove the fractional ink rectangles that could cause visible seams. Wide objects are clipped to the allocated map viewport, including when their anchor is offscreen. The renderer-call sheet is a hosted illustration, not a native Unity screenshot; actual native visual confirmation remains paused.

Hosted Core, API-double UI and save/replay evidence are in BUILD_VALIDATION.md and harness/validation/household16 and household17. Native Unity compilation/rendering/input/dialogs/player testing remains paused; the project targets Unity6000.6.3f1 and no Windows EXE is claimed.
