# Foundation 36: considerate food pickup

When physically collecting food, a human leaves one possible meal for each visible hungry uninfected human with no carried food within two cells of the shelf (circular distance). This reduces pickup only when stock is scarce. The forager still takes at least one available meal, and the ordinary three-meal carry limit applies. With ample supplies, pickup remains unchanged.

The food stays on the shelf; it is not reserved or transferred remotely. Others must reach and collect it themselves, and then eat on a later action. Hidden, distant, fed, infected or already-supplied humans do not affect the calculation. Hunger/bravery and immediate-danger checks still run before collection. Shelf selection and unfinished searches do not inspect unopened quantities; this calculation happens only when the actual search or exposed-stock pickup completes.

Choose **Inspect Considerate Pickup Paused**. Three hungry humans stand by a searched shelf holding three meals. **Step 1 tick**: human #1 takes one and leaves two. **Step 10 ticks**: all three have collected and eaten, with no extra food created.

New encounters use encounter-prototype-32. Earlier saves retain their prior pickup amounts until **Enable considerate pickup** (or an earlier-feature upgrade button) is clicked. There is no new actor state, permanent reservation or food schema.

Native Unity rendering and input require local testing. Automated checks execute production simulation and UI handlers with narrow Unity API doubles.

Validation: 220 simulation cases / 35,143 assertions; 564 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
