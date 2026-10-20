# Foundation 30: exposed food pickup

Humans collect food from already searched shelves in one action. Human-stocked pantry shelves also count as exposed supplies. Unsearched shelves still require three actions and reveal no stock before completion. The ordinary hunger, bravery, sight and immediate-danger checks still run first; quick pickup does not override safety.

Food collection, eating and sharing remain separate actions. Carry limits and finite stock apply to every pickup. Collecting exposed food makes one ordinary search noise. If someone exposes a shelf while another human is partway through searching it, that human can collect on its next eligible action and clears its partial progress.

Choose **Inspect Exposed Food Paused**, then **Step 1 tick**. Human #1 collects three meals and leaves three on the shelf, without eating yet. Compare **Inspect Careful Search Paused** for the three-action unopened-shelf behavior.

New encounters use encounter-prototype-29. Older saves retain their prior timing until **Enable quick food pickup** (or the earlier-feature upgrade button) is clicked. No new actor state or map schema is needed; the existing searched flag determines whether the food is exposed.

Native Unity rendering and input require local testing. Automated checks execute production simulation and UI handlers with narrow Unity API doubles.

Validation: 208 simulation cases / 35,088 assertions; 518 legacy-input UI assertions and 3 NewOnly assertions. Project configuration and whitespace checks passed.
