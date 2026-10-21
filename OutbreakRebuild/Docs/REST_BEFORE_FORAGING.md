# Foundation 73: Rest before foraging

Hungry humans now weigh energy against the need to leave shelter for food. Inside a safe shelter with no known supplies, a human below **60 energy** can recover before heading out. An available bed provides the existing **20 energy per rest action**; ordinary shelter rest provides **10**. Each action still costs one hunger point, so recovery has a real tradeoff.

Carried meals and personally known room supplies take priority. Rest stops at the departure reserve or when hunger becomes urgent. The urgency threshold is 70 hunger for neutral bravery, reduced by three points per positive bravery level and raised by three per negative level (bounded to 55–85). Braver humans therefore leave sooner. Actual recovery also requires calm, no observed zombie, no recent warning or door noise, and no infection. Existing danger checks still govern the food trip after preparation.

Once exit inspection has begun, this preparation rule does not repeatedly pull the human back to bed. A hungry human already outside continues foraging. Ordinary fed rest continues to restore energy fully as before.

## Try it

1. Choose **Inspect Food Trip Rest Paused** from the main menu.
2. Select **Run**, or step 10 ticks twice followed by one tick.
3. The selected human starts at 50 hunger and 20 energy, walks to the bed, and reaches 78 energy at tick 21. Its preparation label disappears.
4. Continue running to see room investigation and the existing cautious exit sequence resume.
5. **Restart scenario** restores the original needs and bed.

The selected-human panel shows **Preparing food trip: rest to 60 energy (hunger cost)** only while this decision applies. Beds can still be placed and removed with the existing controls.

New runs use encounter rules 58. Foundation 72 saves keep rules 57 until **Enable food trip rest** is clicked. Bed positions and existing energy survive the upgrade and restart. Earlier upgrade controls enable current behavior too; the map format remains unchanged.

Automated simulation checks cover preparation and resumption, energy/hunger accounting, occupied-bed fallback, carried and local food priorities, bravery-based urgency, stress interruption, exact save continuation and prior bed-rule compatibility. UI checks exercise the demo, status, upgrade and restart through narrow API doubles. Native Unity layout, animation and input still require editor testing.
