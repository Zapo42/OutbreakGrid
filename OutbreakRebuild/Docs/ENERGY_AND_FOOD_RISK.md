# Foundation 74: Energy and food-trip risk

Low energy now makes humans more cautious about food trips involving visible zombies, lingering stress, recent warnings or heard door noise. Below 60 energy, each two missing energy points add one risk point (rounded up), up to 30 at exhaustion. The existing hunger-and-bravery tolerance still decides whether that risk is acceptable.

This penalty applies only when the human already perceives risk. Clear, calm routes receive no penalty. Immediate zombie contact remains risk 1000 and cannot be overridden by hunger or energy. Hidden zombies do not affect this decision.

The same risk calculation feeds food departure, searching and the existing food-trip retreat decisions. A tired human may wait for stress to subside or abandon a trip sooner when danger appears. Increasing hunger and bravery can still make a trip acceptable. Safe shelter preparation and beds continue to work as in Foundation 73.

## Try it

Choose **Inspect Tired Forager Paused**, then **Run**. The selected human starts with 20 energy, 80 hunger, neutral bravery and stress 35. Its food-trip risk is **55**, including a **20-point low-energy penalty**, above its tolerance of **40**. It waits for stress to settle before leaving. In the same starting situation, a fully rested human perceives risk 35 and begins exit preparation sooner.

The selected-human panel shows **Low-energy food risk: +20**, alongside the total risk and tolerance. Restart restores the initial energy and risk.

New runs use encounter rules 59. Foundation 73 saves retain rules 58 until **Enable energy-aware food risk** is clicked. Upgrade and restart preserve existing energy, needs, beds and supply state. Map and actor formats are unchanged.

Automated checks compare actual rested/tired departures and later reconsideration, visible-danger acceptance, urgent hunger, clear routes, immediate contact, exact save continuation, earlier food-rest behavior and the inspector/upgrade/restart controls. UI checks use narrow Unity API doubles. Native Unity rendering and input require editor testing.
